using System;
using KC.ITCompanion.CorePolicyEngine;
using Xunit;

namespace CorePolicyEngine.Tests;

public class ResultAdvancedTests
{
    [Fact]
    public void Combine_AllOk_Succeeds()
    {
        var c = Result.Combine(Result.Ok(), Result.Ok());
        Assert.True(c.Success);
        Assert.Empty(c.Errors);
    }

    [Fact]
    public void Combine_WithFailures_AggregatesErrors()
    {
        var c = Result.Combine(Result.Fail("a"), Result.Ok(), Result.Fail("b"));
        Assert.False(c.Success);
        Assert.Equal(2, c.Errors.Count);
    }

    [Fact]
    public void Map_PropagatesValue()
    {
        var r = ResultFactory.Ok(10).Map(x => x * 2);
        Assert.True(r.Success);
        Assert.Equal(20, r.Value);
    }

    [Fact]
    public void Map_OnFailure_PropagatesError()
    {
        var fail = ResultFactory.Fail<int>("err").Map(x => x * 2);
        Assert.False(fail.Success);
        Assert.NotEmpty(fail.Errors);
    }

    [Fact]
    public void Bind_ChainsResults()
    {
        var r = ResultFactory.Ok(5).Bind(x => ResultFactory.Ok(x + 5));
        Assert.True(r.Success);
        Assert.Equal(10, r.Value);
    }
}