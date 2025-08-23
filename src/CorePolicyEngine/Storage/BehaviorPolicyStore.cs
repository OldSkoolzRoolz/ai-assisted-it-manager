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
        if (!this._layers.ContainsKey(BehaviorPolicyLayer.LocalDefault))
            this._layers[BehaviorPolicyLayer.LocalDefault] = BehaviorPolicy.Default;
        return Task.CompletedTask;
    }





    public Task UpsertLayerAsync(BehaviorPolicyLayer layer, BehaviorPolicy policy, CancellationToken token)
    {
        this._layers[layer] = policy with { EffectiveUtc = DateTime.UtcNow };
        return Task.CompletedTask;
    }





    public Task<BehaviorPolicySnapshot> GetSnapshotAsync(CancellationToken token)
    {
        BehaviorPolicy effective = BehaviorPolicy.Default with { };
        foreach (BehaviorPolicyLayer layer in Enum.GetValues<BehaviorPolicyLayer>().OrderBy(l => l))
            if (this._layers.TryGetValue(layer, out BehaviorPolicy? p))
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

        string Hash(BehaviorPolicyLayer l)
        {
            return this._layers.TryGetValue(l, out BehaviorPolicy? pol) ? ComputeHash(pol) : string.Empty;
        }

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