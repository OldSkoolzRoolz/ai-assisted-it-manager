using SelfHealingPolicyEngine;
using Xunit;

namespace SelfHealingPolicyEngine.Tests;

public sealed class Class1Tests
{
    [Fact]
    public void Construct()
    {
        var c = new Class1();
        Assert.NotNull(c);
    }
}
