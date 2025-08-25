// Project Name: CorePolicyEngine
// File Name: EnumRule.cs
// Author: Automation
// License: MIT
// Do not remove file headers

using System;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace KC.ITCompanion.CorePolicyEngine.Validation.Rules;

/// <summary>Validates membership in allowed string set (case-insensitive).</summary>
public sealed class EnumRule : IValidationRule
{
    private readonly string[] _allowed;
    private readonly string _ruleId;
    private static readonly ResourceManager Rm = new("KC.ITCompanion.CorePolicyEngine.Resources.ValidationMessages", typeof(EnumRule).Assembly);

    /// <inheritdoc />
    public string RuleId => _ruleId;

    /// <summary>Create rule.</summary>
    public EnumRule(string ruleId, params string[] allowed)
    { _ruleId = ruleId; _allowed = allowed; }

    /// <inheritdoc />
    public ValidationResult Validate(string? input, CultureInfo culture)
    {
        if (input is null) return Fail(input, culture);
        if (_allowed.Any(a => string.Equals(a, input, StringComparison.OrdinalIgnoreCase)))
            return ValidationResult.Ok(input);
        return Fail(input, culture);
    }

    private ValidationResult Fail(string? input, CultureInfo culture)
    {
        var msg = Rm.GetString(_ruleId, culture) ?? _ruleId;
        return ValidationResult.Fail(null, string.Format(culture, msg, input ?? string.Empty, _allowed.FirstOrDefault() ?? string.Empty));
    }
}
