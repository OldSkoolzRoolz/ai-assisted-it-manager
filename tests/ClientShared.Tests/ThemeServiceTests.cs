using System;
using KC.ITCompanion.ClientShared;
using Xunit;

namespace ClientShared.Tests;

public class ThemeServiceTests
{
    private sealed class DummyThemeService : IThemeService
    {
        public AppTheme Current { get; private set; } = AppTheme.Light;
        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
        public void Initialize() { }
        public void Apply(AppTheme theme, bool force = false)
        {
            Current = theme == AppTheme.Auto ? AppTheme.Light : theme;
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(Current));
        }
    }

    [Fact]
    public void Apply_ChangesTheme()
    {
        var svc = new DummyThemeService();
        AppTheme changed = AppTheme.Light;
        svc.ThemeChanged += (_, e) => changed = e.Theme;
        svc.Apply(AppTheme.Dark);
        Assert.Equal(AppTheme.Dark, svc.Current);
        Assert.Equal(AppTheme.Dark, changed);
    }
}
