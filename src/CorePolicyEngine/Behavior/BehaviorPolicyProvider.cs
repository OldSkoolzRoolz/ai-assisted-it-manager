// Project Name: CorePolicyEngine
// File Name: BehaviorPolicyProvider.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using KC.ITCompanion.CorePolicyEngine.Models;
using KC.ITCompanion.CorePolicyEngine.Storage;

namespace KC.ITCompanion.CorePolicyEngine.Behavior;

public interface IBehaviorPolicyProvider
{
    BehaviorPolicySnapshot Current { get; }
    event EventHandler<BehaviorPolicySnapshot>? Changed;
    Task InitializeAsync(CancellationToken token);
    Task RefreshAsync(CancellationToken token);
}

internal sealed class BehaviorPolicyProvider : IBehaviorPolicyProvider
{
    private readonly IBehaviorPolicyStore _store;
    private BehaviorPolicySnapshot _snapshot = new(BehaviorPolicy.Default, "", "", "", "", "", DateTime.UtcNow);
    private readonly TimeSpan _pollInterval;
    private CancellationTokenSource? _loopCts;

    public BehaviorPolicyProvider(IBehaviorPolicyStore store, TimeSpan? pollInterval = null)
    {
        _store = store;
        _pollInterval = pollInterval ?? TimeSpan.FromSeconds(30);
    }

    public BehaviorPolicySnapshot Current => _snapshot;
    public event EventHandler<BehaviorPolicySnapshot>? Changed;

    public async Task InitializeAsync(CancellationToken token)
    {
        await _store.InitializeAsync(token);
        await RefreshAsync(token);
        _loopCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _ = Task.Run(() => PollLoopAsync(_loopCts.Token));
    }

    public async Task RefreshAsync(CancellationToken token)
    {
        var snap = await _store.GetSnapshotAsync(token);
        if (!EqualsHashes(_snapshot, snap))
        {
            _snapshot = snap;
            Changed?.Invoke(this, _snapshot);
        }
    }

    private static bool EqualsHashes(BehaviorPolicySnapshot a, BehaviorPolicySnapshot b)
        => a.LocalDefaultHash == b.LocalDefaultHash && a.OrgBaselineHash == b.OrgBaselineHash &&
           a.SiteOverrideHash == b.SiteOverrideHash && a.MachineOverrideHash == b.MachineOverrideHash &&
           a.UserOverrideHash == b.UserOverrideHash;

    private async Task PollLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_pollInterval, token);
                await RefreshAsync(token);
            }
            catch (OperationCanceledException) { }
            catch { /* log later */ }
        }
    }
}
