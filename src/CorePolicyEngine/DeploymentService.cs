using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shared;


namespace CorePolicyEngine;


public sealed class DeploymentService : IDeploymentService
{
    public Task<Result<string>> DryRunAsync(PolicySet policySet, CancellationToken cancellationToken)
    {
        // TODO: compute diff vs current system state
        var diffJson = "{ \"changes\": [] }";
        return Task.FromResult(Result<string>.Ok(diffJson));
    }

    public Task<Result> ApplyAsync(PolicySet policySet, CancellationToken cancellationToken)
    {
        // TODO: write to registry via abstraction + record snapshot
        return Task.FromResult(Result.Ok());
    }
}