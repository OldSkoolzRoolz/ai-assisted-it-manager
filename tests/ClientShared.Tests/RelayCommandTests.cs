using System.Threading.Tasks;
using KC.ITCompanion.ClientShared;
using Xunit;

namespace ClientShared.Tests;

public class RelayCommandTests
{
    [Fact]
    public void SyncExecute_Works()
    {
        bool ran = false;
        var cmd = new RelayCommand(_ => ran = true);
        Assert.True(cmd.CanExecute(null));
        cmd.Execute(null);
        Assert.True(ran);
    }

    [Fact]
    public async Task AsyncExecute_Works()
    {
        bool ran = false;
        var tcs = new TaskCompletionSource();
        var cmd = new RelayCommand(async _ => { ran = true; await Task.Delay(1); });
        cmd.Execute(null);
        await Task.Delay(10);
        Assert.True(ran);
    }
}
