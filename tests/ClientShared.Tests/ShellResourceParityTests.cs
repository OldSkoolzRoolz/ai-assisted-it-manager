// Project Name: ClientShared.Tests
// File Name: ShellResourceParityTests.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Xunit;

namespace ClientShared.Tests;

/// <summary>
/// Ensures Shell.* resource keys are present across satellite cultures.
/// </summary>
public sealed class ShellResourceParityTests
{
    private static readonly string[] Cultures = { "fr-FR", "qps-PLOC" };

    [Fact]
    public void SatelliteCulturesContainAllNeutralShellKeys()
    {
        var rm = new ResourceManager("KC.ITCompanion.ClientShared.Resources.Shell", typeof(ShellResourceParityTests).Assembly);
        var neutralSet = GetAllResourceKeys(rm, CultureInfo.InvariantCulture);
        Assert.NotEmpty(neutralSet);

        foreach (var cultureName in Cultures)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            var satelliteSet = GetAllResourceKeys(rm, culture);
            // Every neutral key must exist (value can differ or even mirror neutral for pseudo)
            var missing = neutralSet.Where(k => !satelliteSet.Contains(k)).ToList();
            Assert.True(missing.Count == 0, $"Missing keys in {cultureName}: {string.Join(",", missing)}");
        }
    }

    private static HashSet<string> GetAllResourceKeys(ResourceManager rm, CultureInfo culture)
    {
        var set = new HashSet<string>();
        var rs = rm.GetResourceSet(culture, true, true);
        if (rs != null)
        {
            foreach (System.Collections.DictionaryEntry entry in rs)
            {
                if (entry.Key is string key)
                    set.Add(key);
            }
        }
        return set;
    }
}
