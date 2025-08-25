// Project Name: CorePolicyEngine
// File Name: RuleRegistry.cs
// Author: Automation
// License: MIT
// Do not remove file headers

using System.Collections.Generic;
using KC.ITCompanion.CorePolicyEngine.Validation.Rules;

namespace KC.ITCompanion.CorePolicyEngine.Validation;

/// <summary>Central registry mapping known validation rule ids to rule instances.</summary>
public static class RuleRegistry
{
    private static readonly Dictionary<string, IValidationRule> Rules = new()
    {
        ["VR_LOG_RETENTION_RANGE"] = new RangeClampRule("VR_LOG_RETENTION_RANGE", 1, 365),
        ["VR_MAX_LOG_FILE_SIZE"] = new RangeClampRule("VR_MAX_LOG_FILE_SIZE", 1, 512),
        ["VR_POLL_INTERVAL_RANGE"] = new RangeClampRule("VR_POLL_INTERVAL_RANGE", 10, 3600),
        ["VR_CULTURE_TAG"] = new CultureRule("VR_CULTURE_TAG", "en-US"),
        ["VR_LOGLEVEL_ENUM"] = new EnumRule("VR_LOGLEVEL_ENUM", "Trace","Debug","Information","Warning","Error","Critical"),
    };

    /// <summary>Gets a rule instance for id or null.</summary>
    public static IValidationRule? Get(string ruleId) => Rules.TryGetValue(ruleId, out var r) ? r : null;
}
