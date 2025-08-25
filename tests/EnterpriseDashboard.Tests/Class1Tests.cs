using EnterpriseDashboard;
using Xunit;

namespace EnterpriseDashboard.Tests;

public sealed class Class1Tests
{
    [Fact]
    public void Construct()
    {
        var c = new Class1();
        Assert.NotNull(c);
    }
}
