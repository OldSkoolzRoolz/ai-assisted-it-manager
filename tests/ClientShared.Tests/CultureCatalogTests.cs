// Project Name: ClientShared.Tests
// File Name: CultureCatalogTests.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System.Linq;
using System.Resources;
using KC.ITCompanion.ClientShared.Localization;
using Xunit;

namespace ClientShared.Tests;

public sealed class CultureCatalogTests
{
    [Fact]
    public void Discover_FindsExpectedCultures()
    {
        var rm = new ResourceManager("KC.ITCompanion.ClientShared.Resources.Shell", typeof(CultureCatalogTests).Assembly);
        var cultures = CultureCatalog.Discover(rm, "Nav_Status");
        Assert.Contains("en-US", cultures);
        Assert.Contains("fr-FR", cultures);
    }
}
