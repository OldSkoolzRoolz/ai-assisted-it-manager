// Project Name: ITCompanionClient
// File Name: ThemeServiceWinUi.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;
using System.IO;
using KC.ITCompanion.ClientShared;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement; // High contrast detection

namespace ITCompanionClient;

/// <summary>WinUI 3 implementation of IThemeService mapping AppTheme to ElementTheme and honoring system / high contrast.</summary>
public sealed class ThemeServiceWinUi : IThemeService
{
    private const string PrefFileName = "theme.pref"; // simple persistence
    private AppTheme _explicit = AppTheme.Auto;
    public AppTheme Current { get; private set; } = AppTheme.Auto;
    public event EventHandler<AppTheme>? ThemeChanged;

    public void Initialize()
    {
        // Load persisted preference (if any)
        var pref = LoadPersisted();
        Apply(pref, force: true);
        HookActualThemeChanged();
    }

    public void Apply(AppTheme theme, bool force = false)
    {
        _explicit = theme;
        Persist(theme);
        var resolved = Resolve(theme);
        if (!force && resolved == Current) return;
        Current = resolved;
        ApplyToRoot(theme); // pass requested to decide element theme
        ThemeChanged?.Invoke(this, Current);
    }

    private static AppTheme Resolve(AppTheme requested)
    {
        var acc = new AccessibilitySettings();
        if (acc.HighContrast) return AppTheme.HighContrast;
        if (requested == AppTheme.Auto) return requested; // stays Auto (system decides)
        return requested;
    }

    private static void ApplyToRoot(AppTheme requested)
    {
        if (App.MainWindow?.Content is FrameworkElement fe)
        {
            // ElementTheme.Default lets system + high contrast flow through.
            fe.RequestedTheme = requested switch
            {
                AppTheme.Light => ElementTheme.Light,
                AppTheme.Dark => ElementTheme.Dark,
                // HighContrast & Auto -> Default so system picks appropriate resources
                _ => ElementTheme.Default
            };
        }
    }

    private void HookActualThemeChanged()
    {
        if (App.MainWindow?.Content is FrameworkElement fe)
        {
            fe.ActualThemeChanged += (_, _) =>
            {
                if (_explicit == AppTheme.Auto)
                {
                    var prior = Current;
                    Current = Resolve(AppTheme.Auto);
                    if (prior != Current) ThemeChanged?.Invoke(this, Current);
                }
            };
        }
    }

    private static string PrefPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ITCompanion", PrefFileName);

    private static void Persist(AppTheme theme)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PrefPath)!);
            File.WriteAllText(PrefPath, theme.ToString());
        }
        catch { /* non-fatal */ }
    }

    private static AppTheme LoadPersisted()
    {
        try
        {
            if (File.Exists(PrefPath))
            {
                var txt = File.ReadAllText(PrefPath).Trim();
                if (Enum.TryParse<AppTheme>(txt, out var t)) return t;
            }
        }
        catch { }
        return AppTheme.Auto;
    }
}
