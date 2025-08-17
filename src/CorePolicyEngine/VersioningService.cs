using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shared;


namespace CorePolicyEngine;


public sealed class VersioningService : IVersioningService
{
    private readonly IPolicyRepository _repo;
    public VersioningService(IPolicyRepository repo) => this._repo = repo;

    public Task<Result<int>> CommitAsync(PolicySet policySet, string? message, CancellationToken cancellationToken)
    {
        // TODO: persist snapshot + increment version
        return Task.FromResult(Result<int>.Ok(1));
    }

    public Task<Result<IReadOnlyList<PolicySet>>> GetHistoryAsync(string policySetId, CancellationToken cancellationToken)
    {
        // TODO: query history storage
        return Task.FromResult(Result<IReadOnlyList<PolicySet>>.Ok(new List<PolicySet>()));
    }

    public Task<Result> RollbackAsync(string policySetId, int version, CancellationToken cancellationToken)
    {
        // TODO: fetch snapshot and redeploy
        return Task.FromResult(Result.Ok());
    }
}