// Project Name: CorePolicyEngine
// File Name: BehaviorPolicy.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

namespace KC.ITCompanion.CorePolicyEngine.Models;

/// <summary>
/// Represents client behavior configuration centrally managed (local or distributed).
/// Layer precedence (lowest first): LocalDefault &lt; OrgBaseline &lt; SiteOverride &lt; MachineOverride &lt; UserOverride.
/// </summary>
public sealed record BehaviorPolicy(
    int LogRetentionDays,
    int MaxLogFileSizeMb,
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
    /// <summary>Returns a baseline default policy.</summary>
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

/// <summary>
/// Policy layering levels in ascending precedence order.
/// </summary>
public enum BehaviorPolicyLayer
{
    /// <summary>Local default (hard-coded fallback).</summary>
    LocalDefault = 0,
    /// <summary>Organization baseline (central admin baseline).</summary>
    OrgBaseline = 1,
    /// <summary>Site level override.</summary>
    SiteOverride = 2,
    /// <summary>Machine specific override.</summary>
    MachineOverride = 3,
    /// <summary>User specific override.</summary>
    UserOverride = 4
}