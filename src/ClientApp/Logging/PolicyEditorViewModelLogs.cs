// Project Name: ClientApp
// File Name: PolicyEditorViewModelLogs.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.Extensions.Logging;
using ClientApp.Resources;

namespace KC.ITCompanion.ClientApp.Logging;

// Source–generator based logging (Microsoft.Extensions.Logging built-in) with resource indirection.
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

    // Public wrappers performing resource lookup & interpolation
    public static void Initialized(this ILogger logger) => InitializedCore(logger, Strings.PolicyEditor_Initialized);
    public static void CatalogLoaded(this ILogger logger, string languageTag, int policyCount, long elapsedMs)
        => CatalogLoadedCore(logger, Strings.PolicyEditor_CatalogLoaded
            .Replace("{LanguageTag}", languageTag)
            .Replace("{PolicyCount}", policyCount.ToString())
            .Replace("{ElapsedMs}", elapsedMs.ToString()));
    public static void CatalogLoadFailed(this ILogger logger, string languageTag)
        => CatalogLoadFailedCore(logger, Strings.PolicyEditor_CatalogLoadFailed.Replace("{LanguageTag}", languageTag));
    public static void SearchFilterApplied(this ILogger logger, string query)
        => SearchFilterAppliedCore(logger, Strings.PolicyEditor_SearchFilterApplied.Replace("{Query}", query));
    public static void SearchExecuted(this ILogger logger, string query, int resultCount)
        => SearchExecutedCore(logger, Strings.PolicyEditor_SearchExecuted
            .Replace("{Query}", query)
            .Replace("{ResultCount}", resultCount.ToString()));
    public static void CategoryExpanded(this ILogger logger, string categoryId, int childCategoryCount, int policyCount)
        => CategoryExpandedCore(logger, Strings.PolicyEditor_CategoryExpanded
            .Replace("{CategoryId}", categoryId)
            .Replace("{ChildCategoryCount}", childCategoryCount.ToString())
            .Replace("{PolicyCount}", policyCount.ToString()));
    public static void PolicySelected(this ILogger logger, string policyKey, int settingCount)
        => PolicySelectedCore(logger, Strings.PolicyEditor_PolicySelected
            .Replace("{PolicyKey}", policyKey)
            .Replace("{SettingCount}", settingCount.ToString()));
}
