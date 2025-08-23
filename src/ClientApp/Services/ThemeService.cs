// Project Name: ClientApp
// File Name: ThemeService.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using Microsoft.Win32;
using System.Runtime.Versioning;


namespace KC.ITCompanion.ClientApp.Services;


public enum AppTheme
{
    Light,
    Dark,
    HighContrast,
    Auto // Follow system (Light/Dark) unless High Contrast active
}



public interface IThemeService
{
    AppTheme Current { get; }
    void Apply(AppTheme theme, bool force = false);
    void Initialize();
}



[SupportedOSPlatform("windows")]
public sealed class ThemeService : IThemeService, IDisposable
{
    private const string PersonalizeKeyPath = "Software/Microsoft/Windows/CurrentVersion/Themes/Personalize";
    private const string AppsUseLightThemeValue = "AppsUseLightTheme";

    private static readonly Dictionary<AppTheme, string> ThemeDictionaryMap = new()
    {
        { AppTheme.Light, "Themes/Theme.Light.xaml" },
        { AppTheme.Dark, "Themes/Theme.Dark.xaml" },
        { AppTheme.HighContrast, "Themes/Theme.HighContrast.xaml" }
    };

    private bool _disposed;
    private AppTheme _explicitTheme = AppTheme.Auto;

    public AppTheme Current { get; private set; } = AppTheme.Light;

    public void Initialize()
    {
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        Apply(AppTheme.Auto, force: true);
    }

    public void Apply(AppTheme theme, bool force = false)
    {
        _explicitTheme = theme;
        var resolved = ResolveTheme(theme);
        if (!force && resolved == Current) return;
        ReplaceThemeDictionary(resolved);
        Current = resolved;
    }

    private static AppTheme ResolveTheme(AppTheme requested)
    {
        if (requested == AppTheme.HighContrast)
            return AppTheme.HighContrast;
        if (SystemParameters.HighContrast)
            return AppTheme.HighContrast;
        if (requested == AppTheme.Auto)
        {
            bool isLight = ReadSystemAppsUseLightTheme();
            return isLight ? AppTheme.Light : AppTheme.Dark;
        }
        return requested;
    }

    private static bool ReadSystemAppsUseLightTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath);
            if (key?.GetValue(AppsUseLightThemeValue) is int v)
                return v > 0;
        }
        catch { }
        return true; // default to light
    }

    private void OnUserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Color)
        {
            // Re-evaluate only if auto or high contrast changed
            if (_explicitTheme == AppTheme.Auto || SystemParameters.HighContrast)
                Apply(_explicitTheme, force: true);
        }
    }

    private static void ReplaceThemeDictionary(AppTheme theme) // CA1822 static
    {
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => ReplaceThemeDictionary(theme));
            return;
        }
        var merged = Application.Current.Resources.MergedDictionaries;
        // Expect first dictionary is CurrentTheme slot
        int idx = merged.ToList().FindIndex(rd => rd.Source != null && rd.Source.ToString().Contains("Theme."));
        if (idx < 0 && merged.Count > 0)
            idx = 0;
        var rd = new ResourceDictionary { Source = new Uri(ThemeDictionaryMap[theme], UriKind.Relative) };
        if (idx >= 0 && idx < merged.Count)
            merged[idx] = rd;
        else
            merged.Add(rd);
    }

    public void Dispose()
    {
        if (_disposed) return;
        SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}