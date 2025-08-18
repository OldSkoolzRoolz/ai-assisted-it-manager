// Project Name: CorePolicyEngine
// File Name: VersioningService.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers


using Shared;


namespace CorePolicyEngine;


public sealed class VersioningService
{
    public Task<Result<int>> CommitAsync(object stateSnapshot, string? message, CancellationToken cancellationToken)
        => Task.FromResult(Result<int>.Ok(1));

    public Task<Result> RollbackAsync(string id, int version, CancellationToken cancellationToken)
        => Task.FromResult(Result.Ok());
}