using System.Collections.Generic;
using Shared;


namespace CorePolicyEngine;


public sealed class TypeValidationRule : IValidationRule
{
    public string Id => "Type";
    public IEnumerable<ValidationMessage> Evaluate(PolicySet policySet, AdmxCatalog catalog, ValidationContext context)
    {
        foreach (var setting in policySet.Settings)
        {
            if (setting.ValueType == PolicyValueType.Numeric && setting.Value != null && !decimal.TryParse(setting.Value, out _))
            {
                yield return new ValidationMessage(setting.PolicyId, setting.PartId, ValidationSeverity.Error, "Value is not a valid number");
            }
        }
    }
}