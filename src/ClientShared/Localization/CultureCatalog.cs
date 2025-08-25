// Project Name: ClientShared
// File Name: CultureCatalog.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace KC.ITCompanion.ClientShared.Localization;

/// <summary>
/// Discovers available satellite cultures for a given resource manager by probing for a known key.
/// </summary>
public static class CultureCatalog
{
    /// <summary>
    /// Returns ordered culture names (e.g. en-US, fr-FR) that supply a value for the probe key.
    /// </summary>
    /// <exception cref="ArgumentNullException">rm or probeKey null.</exception>
    public static IReadOnlyList<string> Discover(ResourceManager rm, string probeKey)
    {
        ArgumentNullException.ThrowIfNull(rm);
        ArgumentException.ThrowIfNullOrWhiteSpace(probeKey);
        var list = new List<(string name, int sort)>();
        foreach (var c in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                var v = rm.GetString(probeKey, c);
                if (!string.IsNullOrEmpty(v))
                {
                    list.Add((c.Name, c.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase) ? 0 : 1));
                }
            }
            catch (MissingManifestResourceException)
            {
                // ignore missing resource sets
            }
        }
        return list
            .Distinct()
            .OrderBy(t => t.sort).ThenBy(t => t.name, StringComparer.OrdinalIgnoreCase)
            .Select(t => t.name)
            .ToList();
    }
}
