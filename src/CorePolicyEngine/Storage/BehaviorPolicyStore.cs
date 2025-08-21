// Project Name: CorePolicyEngine
// File Name: BehaviorPolicyStore.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using KC.ITCompanion.CorePolicyEngine.Models;
using System.Security.Cryptography;
using System.Text;

namespace KC.ITCompanion.CorePolicyEngine.Storage;

public interface IBehaviorPolicyStore
{
    Task InitializeAsync(CancellationToken token);
    Task<BehaviorPolicySnapshot> GetSnapshotAsync(CancellationToken token);
    Task UpsertLayerAsync(BehaviorPolicyLayer layer, BehaviorPolicy policy, CancellationToken token);
}

public sealed class BehaviorPolicyStore : IBehaviorPolicyStore
{
    private readonly Dictionary<BehaviorPolicyLayer, BehaviorPolicy> _layers = new();

    public Task InitializeAsync(CancellationToken token)
    {
        if (!_layers.ContainsKey(BehaviorPolicyLayer.LocalDefault))
            _layers[BehaviorPolicyLayer.LocalDefault] = BehaviorPolicy.Default;
        return Task.CompletedTask;
    }

    public Task UpsertLayerAsync(BehaviorPolicyLayer layer, BehaviorPolicy policy, CancellationToken token)
    {
        _layers[layer] = policy with { EffectiveUtc = DateTime.UtcNow };
        return Task.CompletedTask;
    }

    public Task<BehaviorPolicySnapshot> GetSnapshotAsync(CancellationToken token)
    {
        BehaviorPolicy effective = BehaviorPolicy.Default with { };
        foreach (var layer in Enum.GetValues<BehaviorPolicyLayer>().OrderBy(l => l))
        {
            if (_layers.TryGetValue(layer, out var p))
            {
                effective = effective with
                {
                    LogRetentionDays = p.LogRetentionDays,
                    MaxLogFileSizeMB = p.MaxLogFileSizeMB,
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
            }
        }
        string Hash(BehaviorPolicyLayer l) => _layers.TryGetValue(l, out var pol) ? ComputeHash(pol) : string.Empty;
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
        var raw = $"{p.LogRetentionDays}|{p.MaxLogFileSizeMB}|{p.MinLogLevel}|{p.UiLanguage}|{p.EnableTelemetry}|{p.PolicyVersion}|{p.EffectiveUtc:O}|{p.AllowedGroupsCsv}|{p.LogViewPollSeconds}|{p.LogQueueMaxDepthPerModule}|{p.LogCircuitErrorThreshold}|{p.LogCircuitErrorWindowSeconds}|{p.LogFailoverEnabled}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }
}
