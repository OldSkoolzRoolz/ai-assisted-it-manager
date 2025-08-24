// Project Name: ClientShared
// File Name: PolicyEditorViewModel.Logging.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.Extensions.Logging;

namespace KC.ITCompanion.ClientShared;

/// <summary>
/// Source-generated logging definitions for <see cref="PolicyEditorViewModel"/> to satisfy CA1848.
/// </summary>
internal static partial class PolicyEditorLog
{
    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "PolicyEditor initialized")]
    internal static partial void PolicyEditorInitialized(this ILogger logger);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Debug, Message = "Search filter applied '{Query}'")]
    internal static partial void SearchFilterApplied(this ILogger logger, string Query);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning, Message = "Catalog load failed for language {LanguageTag}")]
    internal static partial void CatalogLoadFailed(this ILogger logger, string LanguageTag);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "Catalog loaded language {LanguageTag} policies={PolicyCount} in {ElapsedMs}ms")]
    internal static partial void CatalogLoaded(this ILogger logger, string LanguageTag, int PolicyCount, long ElapsedMs);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Warning, Message = "Selected policy key {PolicyKey} not found in catalog documents")]
    internal static partial void PolicyKeyNotFound(this ILogger logger, string PolicyKey);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Information, Message = "Policy selected {PolicyKey} settings={SettingCount}")]
    internal static partial void PolicySelected(this ILogger logger, string PolicyKey, int SettingCount);

    [LoggerMessage(EventId = 2006, Level = LogLevel.Error, Message = "Failed loading policy groups")]
    internal static partial void PolicyGroupsLoadFailed(this ILogger logger, Exception exception);
}
