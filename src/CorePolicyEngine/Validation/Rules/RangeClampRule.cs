// Project Name: CorePolicyEngine
// File Name: RangeClampRule.cs
// Author: Automation
// License: MIT
// Do not remove file headers

using System;
using System.Globalization;
using System.Resources;

namespace KC.ITCompanion.CorePolicyEngine.Validation.Rules;

/// <summary>Clamps integer value within inclusive bounds.</summary>
public sealed class RangeClampRule : IValidationRule
{
    private readonly int _min;
    private readonly int _max;
    private readonly string _ruleId;
    private static readonly ResourceManager Rm = new("KC.ITCompanion.CorePolicyEngine.Resources.ValidationMessages", typeof(RangeClampRule).Assembly);

    /// <inheritdoc />
    public string RuleId => _ruleId;

    /// <summary>Create rule.</summary>
    public RangeClampRule(string ruleId, int min, int max)
    { _ruleId = ruleId; _min = min; _max = max; }

    /// <inheritdoc />
    public ValidationResult Validate(string? input, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(input) || !int.TryParse(input, out var v))
        {
            var msg = Rm.GetString(_ruleId, culture) ?? _ruleId;
            return ValidationResult.Fail(null, string.Format(culture, msg, input ?? string.Empty, _min, _max));
        }
        if (v < _min || v > _max)
        {
            var clamped = Math.Clamp(v, _min, _max);
            var msg = Rm.GetString(_ruleId, culture) ?? _ruleId;
            return ValidationResult.Fail(clamped.ToString(culture), string.Format(culture, msg, v, _min, _max));
        }
        return ValidationResult.Ok(v.ToString(culture));
    }
}
