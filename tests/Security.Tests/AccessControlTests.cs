using System.Security.Principal;
using Microsoft.Extensions.Logging.Abstractions;
using Security;
using Xunit;

namespace Security.Tests;

public class AccessControlTests
{
    [Fact]
    public void PolicyAllowAny_AllowsUser()
    {
        var policy = new GroupMembershipAccessPolicy(new[]{"*"}, NullLogger<GroupMembershipAccessPolicy>.Instance);
        using var id = WindowsIdentity.GetCurrent();
        var allowed = policy.IsAccessAllowed(id, out var reason);
        Assert.True(allowed);
        Assert.Null(reason);
    }

    [Fact]
    public void EvaluatorWrapsPolicy()
    {
        var policy = new GroupMembershipAccessPolicy(new[]{"*"});
        var evaluator = new ClientAccessEvaluator(policy);
        var ok = evaluator.CheckAccess(out var reason);
        Assert.True(ok);
        Assert.Null(reason);
    }
}
