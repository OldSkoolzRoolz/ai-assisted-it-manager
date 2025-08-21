// Project Name: CorePolicyEngine
// File Name: BehaviorPolicy.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace KC.ITCompanion.CorePolicyEngine.Models;

/// <summary>
/// Represents client behavior configuration centrally managed (local or distributed).
/// Layering order (lowest precedence first): LocalDefault < OrgBaseline < SiteOverride < MachineOverride < UserOverride.
/// </summary>
public sealed record BehaviorPolicy(
    int LogRetentionDays,
    int MaxLogFileSizeMB,
    string MinLogLevel,
    string UiLanguage,
    bool EnableTelemetry,
    string PolicyVersion,
    DateTime EffectiveUtc,
    string AllowedGroupsCsv,
    int LogViewPollSeconds,
    int LogQueueMaxDepthPerModule,
    int LogCircuitErrorThreshold,
    int LogCircuitErrorWindowSeconds,
    bool LogFailoverEnabled
)
{
    public static BehaviorPolicy Default => new(
        7,
        5,
        "Information",
        "en-US",
        false,
        "0.0.0",
        DateTime.UtcNow,
        "BUILTIN\\Administrators",
        15,
        5000,
        25,
        60,
        true
    );
}

/// <summary>
/// Snapshot returning effective merged policy and per-layer hashes for drift / change detection.
/// </summary>
public sealed record BehaviorPolicySnapshot(
    BehaviorPolicy Effective,
    string LocalDefaultHash,
    string OrgBaselineHash,
    string SiteOverrideHash,
    string MachineOverrideHash,
    string UserOverrideHash,
    DateTime GeneratedUtc
);

public enum BehaviorPolicyLayer
{
    LocalDefault = 0,
    OrgBaseline = 1,
    SiteOverride = 2,
    MachineOverride = 3,
    UserOverride = 4
}