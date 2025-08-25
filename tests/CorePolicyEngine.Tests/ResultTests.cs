using KC.ITCompanion.CorePolicyEngine;
using Xunit;

namespace CorePolicyEngine.Tests;

public class ResultTests
{
    [Fact]
    public void Ok_CreatesSuccess()
    {
        var r = ResultFactory.Ok(42);
        Assert.True(r.Success);
        Assert.Equal(42, r.Value);
        Assert.Empty(r.Errors);
    }

    [Fact]
    public void Fail_CreatesFailure()
    {
        var r = ResultFactory.Fail<int>("err message");
        Assert.False(r.Success);
        Assert.NotEmpty(r.Errors);
        Assert.Contains(r.Errors, e => e.Contains("err"));
    }
}
