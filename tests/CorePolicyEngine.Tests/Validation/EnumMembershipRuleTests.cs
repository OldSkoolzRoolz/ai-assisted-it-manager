using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using CorePolicyEngine.Validation.Rules;
using Shared;
using Xunit;

namespace CorePolicyEngine.Tests.Validation;

public class EnumMembershipRuleTests
{
    [Fact]
    public void FlagsErrorWhenValueNotInEnum()
    {
        var enumDef = new PolicyEnum("Colors", new List<PolicyEnumItem>{ new("Red","R"), new("Green","G") });
        var part = new PolicyPartDefinition("colorPart", PolicyValueType.Enum, EnumId: "Colors");
        var policy = new AdmxPolicy("Policy1","Color Policy","Cat", true,true, new List<PolicyPartDefinition>{part}, null);
        var catalog = new AdmxCatalog(new List<AdmxCategory>(), new List<AdmxPolicy>{policy}, new List<PolicyEnum>{enumDef}, "en-US");
        var setting = new PolicySetting("Policy1","colorPart", true, "B", PolicyValueType.Enum);
        var set = new PolicySet("set1","Test","LocalMachine", new List<PolicySetting>{setting});

        var rule = new EnumMembershipRule();
        var messages = rule.Evaluate(set, catalog, new Shared.ValidationContext("Windows-Default"));
        Assert.Contains(messages, m => m.Severity == ValidationSeverity.Error);
    }
}
