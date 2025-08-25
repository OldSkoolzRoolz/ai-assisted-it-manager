using System.Threading;
using System.Threading.Tasks;
using KC.ITCompanion.CorePolicyEngine.Storage;
using KC.ITCompanion.CorePolicyEngine.Models;
using Xunit;

namespace CorePolicyEngine.Tests;

public class BehaviorPolicyStoreTests
{
    [Fact]
    public async Task InitializeAndSnapshotRoundTrip()
    {
        var store = new BehaviorPolicyStore();
        await store.InitializeAsync(CancellationToken.None);
        var snap1 = await store.GetSnapshotAsync(CancellationToken.None);
        var modified = snap1.Effective with { LogRetentionDays = snap1.Effective.LogRetentionDays + 1 };
        await store.UpsertLayerAsync(BehaviorPolicyLayer.MachineOverride, modified, CancellationToken.None);
        var snap2 = await store.GetSnapshotAsync(CancellationToken.None);
        Assert.Equal(modified.LogRetentionDays, snap2.Effective.LogRetentionDays);
        Assert.NotEqual(snap1.MachineOverrideHash, snap2.MachineOverrideHash);
    }
}
