// Project Name: CorePolicyEngine
// File Name: DeploymentService.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers


using Shared;


namespace CorePolicyEngine;


public sealed class DeploymentService
{
    public Task<Result<string>> DryRunAsync(object desiredState, CancellationToken cancellationToken)
    {
        // TODO: map desired state (policies + element values + states) to registry actions using Evaluator.
        return Task.FromResult(Result<string>.Ok("{ }"));
    }

    public Task<Result> ApplyAsync(object desiredState, CancellationToken cancellationToken)
    {
        // TODO: evaluate and execute registry actions via abstraction.
        return Task.FromResult(Result.Ok());
    }
}