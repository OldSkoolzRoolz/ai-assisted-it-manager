// Project Name: ClientShared
// File Name: LocalizationService.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.Globalization;

namespace KC.ITCompanion.ClientShared.Localization;

/// <summary>
/// Default implementation of <see cref="ILocalizationService"/>.
/// Adjusts <see cref="CultureInfo.CurrentUICulture"/> and notifies subscribers.
/// Thread-safe via simple lock for minimal contention (culture changes rare).
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private readonly object _gate = new();
    private CultureInfo _current;

    /// <inheritdoc />
    public CultureInfo CurrentUICulture => _current;

    /// <inheritdoc />
    public event EventHandler<CultureInfo>? CultureChanged;

    /// <summary>Create with initial culture (defaults to <see cref="CultureInfo.CurrentUICulture"/>).</summary>
    public LocalizationService(CultureInfo? initial = null)
    {
        _current = initial ?? CultureInfo.CurrentUICulture;
    }

    /// <inheritdoc />
    public void ChangeCulture(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName)) throw new ArgumentException("Culture name required", nameof(cultureName));
        CultureInfo newCulture = CultureInfo.GetCultureInfo(cultureName);
        lock (_gate)
        {
            if (Equals(_current, newCulture)) return;
            _current = newCulture;
            CultureInfo.CurrentUICulture = newCulture;
            CultureInfo.CurrentCulture = newCulture; // align formatting
        }
        CultureChanged?.Invoke(this, newCulture);
    }
}
