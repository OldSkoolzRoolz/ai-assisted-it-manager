// Project Name: ITCompanionClient
// File Name: ThemeServiceWinUi.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;
using KC.ITCompanion.ClientShared;
using Microsoft.UI.Xaml;

namespace ITCompanionClient;

/// <summary>WinUI 3 implementation of IThemeService mapping AppTheme to ElementTheme.</summary>
public sealed class ThemeServiceWinUi : IThemeService
{
    private AppTheme _explicit = AppTheme.Auto;
    public AppTheme Current { get; private set; } = AppTheme.Light;
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public void Initialize() => Apply(AppTheme.Auto, force: true);

    public void Apply(AppTheme theme, bool force = false)
    {
        _explicit = theme;
        var resolved = Resolve(theme);
        if (!force && resolved == Current) return;
        Current = resolved;
        ApplyToRoot(resolved);
        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(Current));
    }

    private static AppTheme Resolve(AppTheme requested)
    {
        if (requested == AppTheme.Auto)
        {
            if (App.MainWindow?.Content is FrameworkElement fe)
            {
                return fe.ActualTheme switch
                {
                    ElementTheme.Dark => AppTheme.Dark,
                    _ => AppTheme.Light
                };
            }
            return AppTheme.Light;
        }
        return requested;
    }

    private static void ApplyToRoot(AppTheme theme)
    {
        if (App.MainWindow?.Content is FrameworkElement fe)
        {
            fe.RequestedTheme = theme switch
            {
                AppTheme.Dark => ElementTheme.Dark,
                _ => ElementTheme.Light
            };
        }
    }
}
