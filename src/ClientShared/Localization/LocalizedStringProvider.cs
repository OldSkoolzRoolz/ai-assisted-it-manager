// Project Name: ClientShared
// File Name: LocalizedStringProvider.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using KC.ITCompanion.ClientShared.Localization;

namespace KC.ITCompanion.ClientShared.Localization;

/// <summary>
/// Simple observable wrapper exposing resource strings for XAML bindings.
/// Re-raises PropertyChanged for all keys when culture changes.
/// </summary>
public sealed class LocalizedStringProvider : INotifyPropertyChanged
{
    private readonly ILocalizationService _loc;
    private readonly ResourceManager _rm;

    /// <summary>Create provider bound to a resource manager.</summary>
    public LocalizedStringProvider(ILocalizationService loc, ResourceManager rm)
    {
        _loc = loc; _rm = rm;
        _loc.CultureChanged += (_, _) => RaiseAll();
    }

    /// <summary>Indexer: {Binding Loc[Some_Key]}</summary>
    public string this[string key] => _rm.GetString(key, _loc.CurrentUICulture) ?? key;

    private void RaiseAll()
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty)); }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
}
