// Project Name: CorePolicyEngine
// File Name: IValidationRule.cs
// Author: Automation
// License: MIT
// Do not remove file headers

using System.Globalization;

namespace KC.ITCompanion.CorePolicyEngine.Validation;

/// <summary>
/// Contract for a configuration or policy validation rule.
/// </summary>
public interface IValidationRule
{
    /// <summary>Stable rule identifier (maps to resource key for localized message).</summary>
    string RuleId { get; }
    /// <summary>Attempts to validate and optionally normalize the supplied value.</summary>
    /// <param name="input">Raw input value (string form).</param>
    /// <param name="culture">Culture for localization.</param>
    /// <returns>Validation result containing success flag, normalized value, and optional message.</returns>
    ValidationResult Validate(string? input, CultureInfo culture);
}
