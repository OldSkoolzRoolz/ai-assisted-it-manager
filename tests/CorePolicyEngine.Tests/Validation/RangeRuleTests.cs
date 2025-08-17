using System.Collections.Generic;
using CorePolicyEngine.Validation.Rules;
using Shared;
using Xunit;

namespace CorePolicyEngine.Tests.Validation;

public class RangeRuleTests
{
    [Fact]
    public void EmitsErrorWhenBelowMin()
    {
        var part = new PolicyPartDefinition("numPart", PolicyValueType.Numeric, Min: 10, Max: 20);
        var policy = new AdmxPolicy("P1","Number Policy","Cat", true,true, new List<PolicyPartDefinition>{part}, null);
        var catalog = new AdmxCatalog(new List<AdmxCategory>(), new List<AdmxPolicy>{policy}, new List<PolicyEnum>(), "en-US");
        var setting = new PolicySetting("P1","numPart", true, "5", PolicyValueType.Numeric);
        var set = new PolicySet("set1","Test","LocalMachine", new List<PolicySetting>{setting});
        var rule = new RangeRule();
        var messages = rule.Evaluate(set, catalog, new ValidationContext("Windows-Default"));
        Assert.Contains(messages, m => m.Message.Contains("below minimum"));
    }
}
