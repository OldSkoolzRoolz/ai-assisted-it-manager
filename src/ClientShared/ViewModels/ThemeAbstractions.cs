// Project Name: ClientShared
// File Name: ThemeAbstractions.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;

namespace KC.ITCompanion.ClientShared;

/// <summary>Supported application theme modes.</summary>
public enum AppTheme
{
    /// <summary>Light theme.</summary>
    Light,
    /// <summary>Dark theme.</summary>
    Dark,
    /// <summary>High contrast / accessibility theme.</summary>
    HighContrast,
    /// <summary>Automatically follow system light/dark (unless high contrast active).</summary>
    Auto // Follow system light/dark (unless high contrast active)
}

/// <summary>Abstraction for theme management across UI frameworks.</summary>
public interface IThemeService
{
    /// <summary>Current effective theme after resolution.</summary>
    AppTheme Current { get; }

    /// <summary>Raised when the effective theme changes.</summary>
    event EventHandler<AppTheme>? ThemeChanged;

    /// <summary>Initialize subscription to system/theme signals and apply initial theme (usually Auto).</summary>
    void Initialize();

    /// <summary>Apply the requested theme (Auto resolves to system preference).</summary>
    /// <param name="theme">Requested theme.</param>
    /// <param name="force">Force apply even if unchanged.</param>
    void Apply(AppTheme theme, bool force = false);
}
