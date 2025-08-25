// Project Name: ClientShared.Tests
// File Name: LocalizedStringProviderTests.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System.Globalization;
using System.Resources;
using KC.ITCompanion.ClientShared.Localization;
using Xunit;

namespace ClientShared.Tests;

public sealed class LocalizedStringProviderTests
{
    [Fact]
    public void Indexer_ReturnsKeyWhenMissing()
    {
        var loc = new LocalizationService(CultureInfo.GetCultureInfo("en-US"));
        var rm = new ResourceManager("KC.ITCompanion.ClientShared.Resources.PolicyEditorLog", typeof(LocalizedStringProviderTests).Assembly);
        var provider = new LocalizedStringProvider(loc, rm);
        var value = provider["__MISSING_KEY__"];
        Assert.Equal("__MISSING_KEY__", value);
    }
}
