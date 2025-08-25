// Project Name: ClientShared
// File Name: PolicyEditorLog.cs
// Author: Kyle Crowder
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;
using System.Text;
using Microsoft.Extensions.Logging;

namespace KC.ITCompanion.ClientShared.Logging;

/// <summary>
/// Localized source-generated logger for Policy Editor (EventIds 2000-2008).
/// Caches parsed <see cref="CompositeFormat"/> instances to avoid repeated parsing (resolves CA1863).
/// </summary>
internal static partial class PolicyEditorLog
{
    private static readonly ResourceManager Res = new("KC.ITCompanion.ClientShared.Resources.PolicyEditorLog", typeof(PolicyEditorLog).Assembly);
    private static readonly ConcurrentDictionary<string, CompositeFormat> FormatCache = new();
    private static CultureInfo C => CultureInfo.CurrentUICulture;

    /// <summary>Retrieves localized template and returns formatted text using cached composite format.</summary>
    /// <param name="key">Resource key.</param>
    /// <param name="args">Format arguments.</param>
    private static string F(string key, params object[] args)
    {
        var raw = Res.GetString(key, C) ?? key;
        if (args.Length == 0) return raw;
        var cacheKey = C.Name + "|" + key + "|" + raw.GetHashCode();
        var cf = FormatCache.GetOrAdd(cacheKey, static (_, state) => CompositeFormat.Parse(state), raw);
        return string.Format(C, cf, args);
    }

    // Event 2000
    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "{Message}")]
    private static partial void LogInitializedCore(ILogger logger, string Message);
    /// <summary>Logs initialization of policy editor (EventId 2000).</summary>
    internal static void PolicyEditorInitialized(this ILogger logger) => LogInitializedCore(logger, F("PolicyEditorInitialized_Template"));

    // Event 2001
    [LoggerMessage(EventId = 2001, Level = LogLevel.Debug, Message = "{Message}")]
    private static partial void LogSearchAppliedCore(ILogger logger, string Message);
    /// <summary>Logs application of search filter (EventId 2001).</summary>
    internal static void SearchFilterApplied(this ILogger logger, string query) => LogSearchAppliedCore(logger, F("SearchFilterApplied_Template", query));

    // Event 2002
    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning, Message = "{Message}")]
    private static partial void LogCatalogLoadFailedCore(ILogger logger, string Message);
    /// <summary>Logs catalog load failure (EventId 2002).</summary>
    internal static void CatalogLoadFailed(this ILogger logger, string languageTag, Exception? ex = null) => LogCatalogLoadFailedCore(logger, F("CatalogLoadFailed_Template", languageTag));

    // Event 2003
    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "{Message}")]
    private static partial void LogCatalogLoadedCore(ILogger logger, string Message);
    /// <summary>Logs successful catalog load including counts (EventId 2003).</summary>
    internal static void CatalogLoaded(this ILogger logger, string languageTag, int policyCount, long elapsedMs) => LogCatalogLoadedCore(logger, F("CatalogLoaded_Template", languageTag, policyCount, elapsedMs));

    // Event 2004
    [LoggerMessage(EventId = 2004, Level = LogLevel.Warning, Message = "{Message}")]
    private static partial void LogPolicyKeyNotFoundCore(ILogger logger, string Message);
    /// <summary>Logs policy key not found (EventId 2004).</summary>
    internal static void PolicyKeyNotFound(this ILogger logger, string policyKey) => LogPolicyKeyNotFoundCore(logger, F("PolicyKeyNotFound_Template", policyKey));

    // Event 2005
    [LoggerMessage(EventId = 2005, Level = LogLevel.Information, Message = "{Message}")]
    private static partial void LogPolicySelectedCore(ILogger logger, string Message);
    /// <summary>Logs policy selection (EventId 2005).</summary>
    internal static void PolicySelected(this ILogger logger, string policyKey, int settingCount) => LogPolicySelectedCore(logger, F("PolicySelected_Template", policyKey, settingCount));

    // Event 2006
    [LoggerMessage(EventId = 2006, Level = LogLevel.Error, Message = "{Message}")]
    private static partial void LogPolicyGroupsLoadFailedCore(ILogger logger, string Message);
    /// <summary>Logs policy group load failure (EventId 2006).</summary>
    internal static void PolicyGroupsLoadFailed(this ILogger logger, Exception ex) => LogPolicyGroupsLoadFailedCore(logger, F("PolicyGroupsLoadFailed_Template"));

    // Event 2007
    [LoggerMessage(EventId = 2007, Level = LogLevel.Warning, Message = "Skipping category with empty Id (source {Source})")]
    internal static partial void SkippingEmptyCategory(this ILogger logger, Uri Source);

    // Event 2008
    [LoggerMessage(EventId = 2008, Level = LogLevel.Warning, Message = "Failed processing category (continuing). Source={Source}")]
    internal static partial void CategoryProcessFailed(this ILogger logger, Exception ex, Uri Source);
}
