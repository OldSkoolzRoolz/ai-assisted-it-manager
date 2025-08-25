using KC.ITCompanion.ClientShared;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;
using Xunit;

namespace ClientShared.Tests;

public class PolicySettingViewModelTests
{
    [Fact]
    public void EnumItems_Populated_ForEnumElement()
    {
        var enumEl = new EnumElement(new ElementId("E1"), null, new[]{new EnumItem("Item1", null, System.Array.Empty<RegistryAction>())});
        var policy = new AdminPolicy(new PolicyKey(new System.Uri("urn:test"), "P1"), PolicyClass.Machine, new LocalizedRef(new ResourceId("d")), null, new CategoryRef(new CategoryId("Cat")), null, null, new[]{enumEl}, new PolicyStateBehavior(PolicyDefaultState.NotConfigured, System.Array.Empty<RegistryAction>(), System.Array.Empty<RegistryAction>(), System.Array.Empty<RegistryAction>()), System.Array.Empty<Tags>(), new PolicyVersion(1,0), new DocumentLineage(new System.Uri("file:///c:/p.admx"), "", System.DateTimeOffset.UtcNow, null, null));
        var vm = new PolicySettingViewModel(policy, enumEl);
        Assert.Single(vm.EnumItems);
    }
}
