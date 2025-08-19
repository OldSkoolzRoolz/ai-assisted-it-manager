// Project Name: CorePolicyEngine
// File Name: BehaviorPolicy.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace CorePolicyEngine.Models;

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
    string AllowedGroupsCsv
)
{
    public static BehaviorPolicy Default => new(
        LogRetentionDays: 7,
        MaxLogFileSizeMB: 5,
        MinLogLevel: "Information",
        UiLanguage: "en-US",
        EnableTelemetry: false,
        PolicyVersion: "0.0.0",
        EffectiveUtc: DateTime.UtcNow,
        AllowedGroupsCsv: "Administrators"
    );
}

public enum BehaviorPolicyLayer
{
    LocalDefault = 0,
    OrgBaseline = 1,
    SiteOverride = 2,
    MachineOverride = 3,
    UserOverride = 4
}
