// Project Name: CorePolicyEngine
// File Name: BehaviorPolicySnapshot.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace KC.ITCompanion.CorePolicyEngine.Models;

/// <summary>
/// Captures the merged effective policy and underlying layer hashes for change detection.
/// </summary>
public sealed record BehaviorPolicySnapshot(
    BehaviorPolicy Effective,
    string LocalDefaultHash,
    string OrgBaselineHash,
    string SiteOverrideHash,
    string MachineOverrideHash,
    string UserOverrideHash,
    DateTime CapturedUtc
);
