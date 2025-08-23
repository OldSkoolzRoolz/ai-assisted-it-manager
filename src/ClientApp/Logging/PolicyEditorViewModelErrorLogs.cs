// Project Name: ClientApp
// File Name: PolicyEditorViewModelErrorLogs.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.Extensions.Logging;

namespace KC.ITCompanion.ClientApp.Logging;

internal static partial class PolicyEditorViewModelErrorLogs
{
    [LoggerMessage(EventId = 3000, Level = LogLevel.Error, Message = "Unexpected exception loading catalog language {languageTag}")]
    public static partial void CatalogUnexpectedError(ILogger logger, Exception ex, string languageTag);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Error, Message = "Failed loading policy groups")]
    public static partial void LoadPolicyGroupsFailed(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Warning, Message = "Selected policy key {policyKey} not found in catalog documents")]
    public static partial void SelectedPolicyMissing(ILogger logger, string policyKey);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Warning, Message = "Category expanded {categoryId} childCats={childCategoryCount} policies={policyCount}")]
    public static partial void CategoryExpandedWarning(ILogger logger, string categoryId, int childCategoryCount, int policyCount);
}
