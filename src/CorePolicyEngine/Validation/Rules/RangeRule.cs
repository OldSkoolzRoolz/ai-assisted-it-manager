using System.Collections.Generic;
using System.Linq;
using Shared;

namespace CorePolicyEngine.Validation.Rules;

public sealed class RangeRule : IValidationRule
{
    public string Id => "Range";

    public IEnumerable<ValidationMessage> Evaluate(PolicySet policySet, AdmxCatalog catalog, ValidationContext context)
    {
        foreach (var setting in policySet.Settings)
        {
            if (setting.ValueType == PolicyValueType.Numeric && setting.Value != null)
            {
                if (!decimal.TryParse(setting.Value, out var num))
                {
                    continue; // Type rule handles parse error
                }
                var part = catalog.Policies.SelectMany(p => p.Parts).FirstOrDefault(pt => pt.Id == setting.PartId);
                if (part != null)
                {
                    if (part.Min.HasValue && num < part.Min.Value)
                        yield return new ValidationMessage(setting.PolicyId, setting.PartId, ValidationSeverity.Error, $"Value {num} below minimum {part.Min}");
                    if (part.Max.HasValue && num > part.Max.Value)
                        yield return new ValidationMessage(setting.PolicyId, setting.PartId, ValidationSeverity.Error, $"Value {num} above maximum {part.Max}");
                }
            }
        }
    }
}
