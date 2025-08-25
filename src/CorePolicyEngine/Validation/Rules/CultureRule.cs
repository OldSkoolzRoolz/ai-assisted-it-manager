// Project Name: CorePolicyEngine
// File Name: CultureRule.cs
// Author: Automation
// License: MIT
// Do not remove file headers

using System;
using System.Globalization;
using System.Resources;

namespace KC.ITCompanion.CorePolicyEngine.Validation.Rules;

/// <summary>Validates culture tag; falls back to default on invalid.</summary>
public sealed class CultureRule : IValidationRule
{
    private readonly string _ruleId;
    private readonly string _fallback;
    private static readonly ResourceManager Rm = new("KC.ITCompanion.CorePolicyEngine.Resources.ValidationMessages", typeof(CultureRule).Assembly);

    /// <inheritdoc />
    public string RuleId => _ruleId;

    /// <summary>Create rule with fallback culture.</summary>
    public CultureRule(string ruleId, string fallback = "en-US")
    { _ruleId = ruleId; _fallback = fallback; }

    /// <inheritdoc />
    public ValidationResult Validate(string? input, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Fail(input, culture);
        try
        {
            _ = CultureInfo.GetCultureInfo(input);
            return ValidationResult.Ok(input);
        }
        // Replaced general exception handling with specific exception handling for CultureNotFoundException
        catch (CultureNotFoundException)
        {
            return Fail(input, culture);
        }
    }

    private ValidationResult Fail(string? input, CultureInfo culture)
    {
        var msg = Rm.GetString(_ruleId, culture) ?? _ruleId;
        // Used string.Empty instead of "" for better readability
        return ValidationResult.Fail(_fallback, string.Format(culture, msg, input ?? string.Empty, _fallback));
    }
}
