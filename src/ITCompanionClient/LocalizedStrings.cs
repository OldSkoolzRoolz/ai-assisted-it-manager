// Project Name: ITCompanionClient
// File Name: LocalizedStrings.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System.Resources;
using KC.ITCompanion.ClientShared.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace ITCompanionClient;

/// <summary>
/// XAML helper exposing a LocalizedStringProvider instance for shell resources.
/// </summary>
public sealed class LocalizedStrings
{
    /// <summary>Provider with indexer for bindings.</summary>
    public LocalizedStringProvider Provider { get; }

    /// <summary>Indexer forwarding to provider (optional convenience).</summary>
    public string this[string key] => Provider[key];

    /// <summary>Default ctor resolves dependencies via App.Services.</summary>
    public LocalizedStrings()
    {
        var loc = (ILocalizationService)App.Services.GetRequiredService(typeof(ILocalizationService));
        Provider = new LocalizedStringProvider(loc, new ResourceManager("KC.ITCompanion.ClientShared.Resources.Shell", typeof(LocalizedStrings).Assembly));
    }
}
