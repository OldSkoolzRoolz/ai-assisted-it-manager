// Project Name: ClientShared
// File Name: PolicyEditorLog.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Logging;

namespace KC.ITCompanion.ClientShared.Logging;

/// <summary>
/// Localizable logging helpers for Policy Editor (EventId 2000-2099).
/// Uses resource-driven templates (indexed placeholders) to allow per-culture
/// re-ordering without code changes. Structured property capture sacrificed for
/// full localization flexibility (message emitted as single formatted string).
/// </summary>
internal static class PolicyEditorLog
{
    private static readonly ResourceManager Res = new("KC.ITCompanion.ClientShared.Resources.PolicyEditorLog", typeof(PolicyEditorLog).Assembly);
    private static CultureInfo UICulture => CultureInfo.CurrentUICulture;
    private static string RT(string key) => Res.GetString(key, UICulture) ?? key; // resource template

    private static EventId E(int id, string name) => new(id, name);

    /// <summary>Policy editor initialized.</summary>
    internal static void PolicyEditorInitialized(this ILogger logger)
    {
        var msg = RT("PolicyEditorInitialized_Template");
        logger.LogInformation(E(2000, nameof(PolicyEditorInitialized)), msg);
    }

    /// <summary>Search filter applied.</summary>
    internal static void SearchFilterApplied(this ILogger logger, string query)
    {
        var tmpl = RT("SearchFilterApplied_Template");
        var msg = string.Format(UICulture, tmpl, query);
        logger.LogDebug(E(2001, nameof(SearchFilterApplied)), msg);
    }

    /// <summary>Catalog load failed for language.</summary>
    internal static void CatalogLoadFailed(this ILogger logger, string languageTag, Exception? ex = null)
    {
        var tmpl = RT("CatalogLoadFailed_Template");
        var msg = string.Format(UICulture, tmpl, languageTag);
        logger.LogWarning(E(2002, nameof(CatalogLoadFailed)), ex, msg);
    }

    /// <summary>Catalog loaded successfully.</summary>
    internal static void CatalogLoaded(this ILogger logger, string languageTag, int policyCount, long elapsedMs)
    {
        var tmpl = RT("CatalogLoaded_Template");
        var msg = string.Format(UICulture, tmpl, languageTag, policyCount, elapsedMs);
        logger.LogInformation(E(2003, nameof(CatalogLoaded)), msg);
    }

    /// <summary>Policy key not found in catalog documents.</summary>
    internal static void PolicyKeyNotFound(this ILogger logger, string policyKey)
    {
        var tmpl = RT("PolicyKeyNotFound_Template");
        var msg = string.Format(UICulture, tmpl, policyKey);
        logger.LogWarning(E(2004, nameof(PolicyKeyNotFound)), msg);
    }

    /// <summary>Policy selected and settings enumerated.</summary>
    internal static void PolicySelected(this ILogger logger, string policyKey, int settingCount)
    {
        var tmpl = RT("PolicySelected_Template");
        var msg = string.Format(UICulture, tmpl, policyKey, settingCount);
        logger.LogInformation(E(2005, nameof(PolicySelected)), msg);
    }

    /// <summary>Policy groups load failed.</summary>
    internal static void PolicyGroupsLoadFailed(this ILogger logger, Exception ex)
    {
        var tmpl = RT("PolicyGroupsLoadFailed_Template");
        var msg = RT("PolicyGroupsLoadFailed_Template") == tmpl ? tmpl : string.Format(UICulture, tmpl); // template has no args
        logger.LogError(E(2006, nameof(PolicyGroupsLoadFailed)), ex, msg);
    }
}
