using System.Collections.Generic;

using Shared;


namespace CorePolicyEngine;


public sealed class RequiredEnabledValueRule : IValidationRule
{
    public string Id => "RequiredValueWhenEnabled";
    public IEnumerable<ValidationMessage> Evaluate(PolicySet policySet, AdmxCatalog catalog, ValidationContext context)
    {
        foreach (var s in policySet.Settings)
        {
            if (s.Enabled && s.ValueType != PolicyValueType.Boolean && string.IsNullOrWhiteSpace(s.Value))
            {
                yield return new ValidationMessage(s.PolicyId, s.PartId, ValidationSeverity.Error, "Enabled setting requires a value");
            }
        }
    }
}