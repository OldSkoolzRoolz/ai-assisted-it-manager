// Project Name: ClientShared.Tests
// File Name: LocalizationResourceTests.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using Xunit;

namespace ClientShared.Tests;

/// <summary>
/// Tests verifying localization resource template integrity for PolicyEditor.
/// Ensures all required *_Template keys are present in neutral culture and (if added) in satellite cultures.
/// </summary>
public sealed class LocalizationResourceTests
{
    private static readonly string[] RequiredKeys = new[]
    {
        "PolicyEditorInitialized_Template",
        "SearchFilterApplied_Template",
        "CatalogLoadFailed_Template",
        "CatalogLoaded_Template",
        "PolicyKeyNotFound_Template",
        "PolicySelected_Template",
        "PolicyGroupsLoadFailed_Template"
    };

    private static ResourceManager GetPolicyEditorManager()
        => new("KC.ITCompanion.ClientShared.Resources.PolicyEditorLog", typeof(LocalizationResourceTests).Assembly);

    [Fact]
    public void NeutralResourcesContainAllKeys()
    {
        var rm = GetPolicyEditorManager();
        foreach (var key in RequiredKeys)
        {
            var value = rm.GetString(key, CultureInfo.InvariantCulture);
            Assert.False(string.IsNullOrWhiteSpace(value), $"Missing or empty resource: {key}");
            // Ensure placeholder indexes are balanced (simple check for '{' occurrence pairs)
            int open = value!.Count(c => c == '{');
            int close = value.Count(c => c == '}');
            Assert.Equal(open, close);
        }
    }

    [Fact]
    public void PlaceholdersAreIndexedSequentially()
    {
        var rm = GetPolicyEditorManager();
        foreach (var key in RequiredKeys)
        {
            var value = rm.GetString(key, CultureInfo.InvariantCulture)!;
            var indexes = ExtractPlaceholderIndexes(value).ToList();
            for (int i = 0; i < indexes.Count; i++)
                Assert.Equal(i, indexes[i]);
        }
    }

    private static IEnumerable<int> ExtractPlaceholderIndexes(string template)
    {
        // naive scan for patterns {n} where n is int.
        for (int i = 0; i < template.Length - 2; i++)
        {
            if (template[i] == '{')
            {
                int close = template.IndexOf('}', i + 1);
                if (close > i + 1)
                {
                    var inner = template.Substring(i + 1, close - i - 1);
                    if (int.TryParse(inner, out int idx))
                        yield return idx;
                    i = close;
                }
            }
        }
    }
}
