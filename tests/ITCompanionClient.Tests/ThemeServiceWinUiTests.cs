using ITCompanionClient;
using KC.ITCompanion.ClientShared;
using Xunit;

namespace ITCompanionClient.Tests;

/// <summary>
/// Tests for ThemeServiceWinUi basic behavior (headless environment assumptions: no MainWindow content).
/// </summary>
public sealed class ThemeServiceWinUiTests
{
    [Fact]
    public void Apply_Auto_DefaultsToLightWhenNoWindow()
    {
        var svc = new ThemeServiceWinUi();
        svc.Apply(AppTheme.Auto, force: true);
        Assert.Equal(AppTheme.Light, svc.Current);
    }

    [Fact]
    public void Apply_Dark_SetsDark()
    {
        var svc = new ThemeServiceWinUi();
        svc.Apply(AppTheme.Dark, force: true);
        Assert.Equal(AppTheme.Dark, svc.Current);
    }
}
