// Project Name: CorePolicyEngine
// File Name: ValidationResult.cs
// Author: Automation
// License: MIT
// Do not remove file headers

namespace KC.ITCompanion.CorePolicyEngine.Validation;

/// <summary>Represents the outcome of a validation rule evaluation.</summary>
public sealed record ValidationResult(bool Success, string? NormalizedValue, string? Message)
{
    /// <summary>Create success result.</summary>
    public static ValidationResult Ok(string? normalized) => new(true, normalized, null);
    /// <summary>Create failure result with message (normalized may contain clamped value).</summary>
    public static ValidationResult Fail(string? normalized, string message) => new(false, normalized, message);
}
