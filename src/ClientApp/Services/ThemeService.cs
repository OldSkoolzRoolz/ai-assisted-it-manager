// Project Name: ClientApp
// File Name: ThemeService.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Windows;

namespace KC.ITCompanion.ClientApp.Services;

public enum AppTheme
{
    Light,
    Dark,
    HighContrast
}

public interface IThemeService
{
    AppTheme Current { get; }
    void Apply(AppTheme theme);
}

public sealed class ThemeService : IThemeService
{
    private readonly Dictionary<AppTheme, string> _colorDictionaryMap = new()
    {
        { AppTheme.Light, "Themes/Colors.xaml" },
        { AppTheme.Dark, "Themes/Colors.Dark.xaml" },
        { AppTheme.HighContrast, "Themes/Colors.HighContrast.xaml" }
    };

    public AppTheme Current { get; private set; } = AppTheme.Light;

    public void Apply(AppTheme theme)
    {
        if (theme == Current) return;
        if (Application.Current.Resources.MergedDictionaries.Count == 0) return;
        // Assume first merged dictionary is active color palette per App.xaml
        var merged = Application.Current.Resources.MergedDictionaries;
        var colorDictIndex = merged.ToList().FindIndex(rd => rd.Source != null && rd.Source.ToString().Contains("Colors"));
        if (colorDictIndex >= 0)
        {
            merged[colorDictIndex] = new ResourceDictionary { Source = new Uri(_colorDictionaryMap[theme], UriKind.Relative) };
            Current = theme;
        }
    }
}
