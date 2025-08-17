using System.Collections.Generic;
using System.Linq;
using Shared;

namespace CorePolicyEngine.Validation.Rules;

public sealed class EnumMembershipRule : IValidationRule
{
    public string Id => "EnumMembership";

    public IEnumerable<ValidationMessage> Evaluate(PolicySet policySet, AdmxCatalog catalog, ValidationContext context)
    {
        var enumMap = catalog.Enums.ToDictionary(e => e.Id);
        foreach (var setting in policySet.Settings)
        {
            if (setting.ValueType == PolicyValueType.Enum && setting.Value != null)
            {
                var part = catalog.Policies.SelectMany(p => p.Parts).FirstOrDefault(pt => pt.Id == setting.PartId);
                if (part?.EnumId != null && enumMap.TryGetValue(part.EnumId, out var enumDef))
                {
                    if (!enumDef.Items.Any(i => i.Value == setting.Value))
                    {
                        yield return new ValidationMessage(setting.PolicyId, setting.PartId, ValidationSeverity.Error, $"Value '{setting.Value}' is not a member of enum '{part.EnumId}'");
                    }
                }
            }
        }
    }
}
