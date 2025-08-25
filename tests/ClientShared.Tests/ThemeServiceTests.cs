using System;
using KC.ITCompanion.ClientShared;
using Xunit;

namespace ClientShared.Tests;

public class ThemeServiceTests
{
    private sealed class DummyThemeService : IThemeService
    {
        public AppTheme Current { get; private set; } = AppTheme.Light;
        public event EventHandler<AppTheme>? ThemeChanged;
        public void Initialize() { }
        public void Apply(AppTheme theme, bool force = false) { Current = theme == AppTheme.Auto ? AppTheme.Light : theme; ThemeChanged?.Invoke(this, Current); }
    }

    [Fact]
    public void Apply_ChangesTheme()
    {
        var svc = new DummyThemeService();
        AppTheme changed = AppTheme.Light;
        svc.ThemeChanged += (_, t) => changed = t;
        svc.Apply(AppTheme.Dark);
        Assert.Equal(AppTheme.Dark, svc.Current);
        Assert.Equal(AppTheme.Dark, changed);
    }
}
