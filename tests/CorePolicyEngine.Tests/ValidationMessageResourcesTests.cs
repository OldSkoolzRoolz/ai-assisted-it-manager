// Project Name: CorePolicyEngine.Tests
// File Name: ValidationMessageResourcesTests.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System.Globalization;
using System.Linq;
using System.Resources;
using Xunit;

namespace CorePolicyEngine.Tests;

public sealed class ValidationMessageResourcesTests
{
    private static readonly string[] Keys = new[]
    {
        "VR_LOG_RETENTION_RANGE","VR_MAX_LOG_FILE_SIZE","VR_LOGLEVEL_ENUM","VR_CULTURE_TAG","VR_BOOL","VR_POLL_INTERVAL_RANGE","VR_PATH_ABSOLUTE","VR_URI_OPTIONAL"
    };

    [Theory]
    [InlineData("fr-FR")]
    [InlineData("qps-PLOC")]
    public void SatelliteCulturesContainAllKeys(string cultureName)
    {
        var rm = new ResourceManager("KC.ITCompanion.CorePolicyEngine.Resources.ValidationMessages", typeof(ValidationMessageResourcesTests).Assembly);
        var neutral = Keys.Where(k => rm.GetString(k, CultureInfo.InvariantCulture) != null).ToList();
        Assert.Equal(Keys.Length, neutral.Count);
        var culture = CultureInfo.GetCultureInfo(cultureName);
        foreach (var key in Keys)
            Assert.False(string.IsNullOrWhiteSpace(rm.GetString(key, culture)), $"Missing key {key} in {cultureName}");
    }
}
