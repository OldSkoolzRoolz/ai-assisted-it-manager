// Project Name: CorePolicyEngine
// File Name: BehaviorPolicyStore.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


using System.Security.Cryptography;
using System.Text;

using KC.ITCompanion.CorePolicyEngine.Models;


namespace KC.ITCompanion.CorePolicyEngine.Storage;

/// <summary>
/// Abstraction for storing layered behavior policies (default, org baseline, overrides, etc.) and producing snapshots.
/// </summary>
public interface IBehaviorPolicyStore
{
	/// <summary>Initializes the store (ensures default layer present).</summary>
	Task InitializeAsync(CancellationToken token);
	/// <summary>Returns an effective snapshot (merged layers) plus per-layer hashes.</summary>
	Task<BehaviorPolicySnapshot> GetSnapshotAsync(CancellationToken token);
	/// <summary>Upserts a policy layer definition.</summary>
	Task UpsertLayerAsync(BehaviorPolicyLayer layer, BehaviorPolicy policy, CancellationToken token);
}

/// <summary>
/// In?memory implementation of <see cref="IBehaviorPolicyStore"/>; suitable for client/runtime local composition.
/// </summary>
public sealed class BehaviorPolicyStore : IBehaviorPolicyStore
{
	private readonly Dictionary<BehaviorPolicyLayer, BehaviorPolicy> _layers = new();

	/// <inheritdoc/>
	public Task InitializeAsync(CancellationToken token)
	{
		if (!_layers.ContainsKey(BehaviorPolicyLayer.LocalDefault))
			_layers[BehaviorPolicyLayer.LocalDefault] = BehaviorPolicy.Default;
		return Task.CompletedTask;
	}

	/// <inheritdoc/>
	public Task UpsertLayerAsync(BehaviorPolicyLayer layer, BehaviorPolicy policy, CancellationToken token)
	{
		ArgumentNullException.ThrowIfNull(policy);
		_layers[layer] = policy with { EffectiveUtc = DateTime.UtcNow };
		return Task.CompletedTask;
	}

	/// <inheritdoc/>
	public Task<BehaviorPolicySnapshot> GetSnapshotAsync(CancellationToken token)
	{
		BehaviorPolicy effective = BehaviorPolicy.Default with { };
		foreach (BehaviorPolicyLayer layer in Enum.GetValues<BehaviorPolicyLayer>().OrderBy(l => l))
			if (_layers.TryGetValue(layer, out BehaviorPolicy? p))
				effective = effective with
				{
					LogRetentionDays = p.LogRetentionDays,
					MaxLogFileSizeMb = p.MaxLogFileSizeMb,
					MinLogLevel = p.MinLogLevel,
					UiLanguage = p.UiLanguage,
					EnableTelemetry = p.EnableTelemetry,
					PolicyVersion = p.PolicyVersion,
					EffectiveUtc = p.EffectiveUtc,
					AllowedGroupsCsv = p.AllowedGroupsCsv,
					LogViewPollSeconds = p.LogViewPollSeconds,
					LogQueueMaxDepthPerModule = p.LogQueueMaxDepthPerModule,
					LogCircuitErrorThreshold = p.LogCircuitErrorThreshold,
					LogCircuitErrorWindowSeconds = p.LogCircuitErrorWindowSeconds,
					LogFailoverEnabled = p.LogFailoverEnabled
				};

		string Hash(BehaviorPolicyLayer l) => _layers.TryGetValue(l, out BehaviorPolicy? pol) ? ComputeHash(pol) : string.Empty;

		var snap = new BehaviorPolicySnapshot(effective,
			Hash(BehaviorPolicyLayer.LocalDefault),
			Hash(BehaviorPolicyLayer.OrgBaseline),
			Hash(BehaviorPolicyLayer.SiteOverride),
			Hash(BehaviorPolicyLayer.MachineOverride),
			Hash(BehaviorPolicyLayer.UserOverride),
			DateTime.UtcNow);
		return Task.FromResult(snap);
	}

	private static string ComputeHash(BehaviorPolicy p)
	{
		var raw =
			$"{p.LogRetentionDays}|{p.MaxLogFileSizeMb}|{p.MinLogLevel}|{p.UiLanguage}|{p.EnableTelemetry}|{p.PolicyVersion}|{p.EffectiveUtc:O}|{p.AllowedGroupsCsv}|{p.LogViewPollSeconds}|{p.LogQueueMaxDepthPerModule}|{p.LogCircuitErrorThreshold}|{p.LogCircuitErrorWindowSeconds}|{p.LogFailoverEnabled}";
		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
	}
}