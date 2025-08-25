// Project Name: CorePolicyEngine.Tests
// File Name: ValidationRuleTests.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System.Globalization;
using KC.ITCompanion.CorePolicyEngine.Validation;
using KC.ITCompanion.CorePolicyEngine.Validation.Rules;
using Xunit;

namespace CorePolicyEngine.Tests;

public sealed class ValidationRuleTests
{
    [Theory]
    [InlineData("VR_LOG_RETENTION_RANGE", "0", false)]
    [InlineData("VR_LOG_RETENTION_RANGE", "7", true)]
    [InlineData("VR_MAX_LOG_FILE_SIZE", "999", false)]
    [InlineData("VR_LOGLEVEL_ENUM", "Debug", true)]
    [InlineData("VR_LOGLEVEL_ENUM", "Verbose", false)]
    [InlineData("VR_CULTURE_TAG", "fr-FR", true)]
    [InlineData("VR_CULTURE_TAG", "xx-INVALID", false)]
    public void Rules_Validate_AsExpected(string ruleId, string value, bool success)
    {
        var rule = RuleRegistry.Get(ruleId);
        Assert.NotNull(rule);
        var result = rule!.Validate(value, CultureInfo.GetCultureInfo("en-US"));
        Assert.Equal(success, result.Success);
    }
}
