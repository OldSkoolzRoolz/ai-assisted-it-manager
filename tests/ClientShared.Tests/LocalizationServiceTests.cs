// Project Name: ClientShared.Tests
// File Name: LocalizationServiceTests.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.Globalization;
using KC.ITCompanion.ClientShared.Localization;
using Xunit;

namespace ClientShared.Tests;

/// <summary>
/// Tests for LocalizationService ensuring culture switching semantics.
/// </summary>
public sealed class LocalizationServiceTests
{
    [Fact]
    public void ChangeCulture_RaisesEventAndUpdatesCulture()
    {
        var svc = new LocalizationService(CultureInfo.GetCultureInfo("en-US"));
        CultureInfo? raised = null;
        svc.CultureChanged += (_, e) => raised = e.Culture;
        svc.ChangeCulture("fr-FR");
        Assert.Equal("fr-FR", svc.CurrentUICulture.Name);
        Assert.NotNull(raised);
        Assert.Equal("fr-FR", raised!.Name);
    }

    [Fact]
    public void ChangeCulture_SameCulture_NoEvent()
    {
        var svc = new LocalizationService(CultureInfo.GetCultureInfo("en-US"));
        bool raised = false;
        svc.CultureChanged += (_, _) => raised = true;
        svc.ChangeCulture("en-US");
        Assert.False(raised);
    }

    [Fact]
    public void ChangeCulture_Invalid_Throws()
    {
        var svc = new LocalizationService();
        Assert.Throws<ArgumentException>(() => svc.ChangeCulture(""));
        Assert.Throws<CultureNotFoundException>(() => svc.ChangeCulture("xx-INVALID"));
    }
}
