// Project Name: ClientApp
// File Name: PolicyEditorViewModelLogs.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.Extensions.Logging;
using KC.ITCompanion.ClientApp.Resources;
using System.Globalization;

namespace KC.ITCompanion.ClientApp.Logging;

// Source generator based logging (Microsoft.Extensions.Logging built-in) with resource indirection.
internal static partial class PolicyEditorViewModelLogs
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "{Message}")]
    public static partial void InitializedCore(this ILogger logger, string message);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "{Message}")]
    public static partial void CatalogLoadedCore(this ILogger logger, string message);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning, Message = "{Message}")]
    public static partial void CatalogLoadFailedCore(this ILogger logger, string message);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Debug, Message = "{Message}")]
    public static partial void SearchFilterAppliedCore(this ILogger logger, string message);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Information, Message = "{Message}")]
    public static partial void SearchExecutedCore(this ILogger logger, string message);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Debug, Message = "{Message}")]
    public static partial void CategoryExpandedCore(this ILogger logger, string message);

    [LoggerMessage(EventId = 1006, Level = LogLevel.Information, Message = "{Message}")]
    public static partial void PolicySelectedCore(this ILogger logger, string message);

    private static string I(string value) => value; // identity helper to reduce repeated analyzer false-positives

    public static void Initialized(this ILogger logger) => InitializedCore(logger, I(Strings.PolicyEditor_Initialized));

    public static void CatalogLoaded(this ILogger logger, string languageTag, int policyCount, long elapsedMs)
        => CatalogLoadedCore(logger, I(Strings.PolicyEditor_CatalogLoaded)
            .Replace("{LanguageTag}", languageTag, StringComparison.Ordinal)
            .Replace("{PolicyCount}", policyCount.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{ElapsedMs}", elapsedMs.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));

    public static void CatalogLoadFailed(this ILogger logger, string languageTag)
        => CatalogLoadFailedCore(logger, I(Strings.PolicyEditor_CatalogLoadFailed)
            .Replace("{LanguageTag}", languageTag, StringComparison.Ordinal));

    public static void SearchFilterApplied(this ILogger logger, string query)
        => SearchFilterAppliedCore(logger, I(Strings.PolicyEditor_SearchFilterApplied)
            .Replace("{Query}", query, StringComparison.Ordinal));

    public static void SearchExecuted(this ILogger logger, string query, int resultCount)
        => SearchExecutedCore(logger, I(Strings.PolicyEditor_SearchExecuted)
            .Replace("{Query}", query, StringComparison.Ordinal)
            .Replace("{ResultCount}", resultCount.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));

    public static void CategoryExpanded(this ILogger logger, string categoryId, int childCategoryCount, int policyCount)
        => CategoryExpandedCore(logger, I(Strings.PolicyEditor_CategoryExpanded)
            .Replace("{CategoryId}", categoryId, StringComparison.Ordinal)
            .Replace("{ChildCategoryCount}", childCategoryCount.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{PolicyCount}", policyCount.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));

    public static void PolicySelected(this ILogger logger, string policyKey, int settingCount)
        => PolicySelectedCore(logger, I(Strings.PolicyEditor_PolicySelected)
            .Replace("{PolicyKey}", policyKey, StringComparison.Ordinal)
            .Replace("{SettingCount}", settingCount.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
}
