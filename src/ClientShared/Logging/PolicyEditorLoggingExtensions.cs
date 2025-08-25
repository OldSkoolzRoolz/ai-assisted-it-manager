// Project Name: ClientShared
// File Name: PolicyEditorLoggingExtensions.cs
// Author: Automation
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using Microsoft.Extensions.Logging;

namespace KC.ITCompanion.ClientShared.Logging;

internal static class PolicyEditorLoggingExtensions
{
    private static readonly EventId InitializedId = new(2000, "PolicyEditorInitialized");
    private static readonly EventId SearchAppliedId = new(2001, "SearchFilterApplied");
    private static readonly EventId CatalogLoadFailedId = new(2002, "CatalogLoadFailed");
    private static readonly EventId CatalogLoadedId = new(2003, "CatalogLoaded");
    private static readonly EventId PolicyKeyNotFoundId = new(2004, "PolicyKeyNotFound");
    private static readonly EventId PolicySelectedId = new(2005, "PolicySelected");
    private static readonly EventId PolicyGroupsLoadFailedId = new(2006, "PolicyGroupsLoadFailed");

    internal static void LogPolicyEditorTemplate(this ILogger logger, string key, string message)
    {
        var (id, level) = key switch
        {
            "PolicyEditorInitialized_Template" => (InitializedId, LogLevel.Information),
            "SearchFilterApplied_Template" => (SearchAppliedId, LogLevel.Debug),
            "CatalogLoadFailed_Template" => (CatalogLoadFailedId, LogLevel.Warning),
            "CatalogLoaded_Template" => (CatalogLoadedId, LogLevel.Information),
            "PolicyKeyNotFound_Template" => (PolicyKeyNotFoundId, LogLevel.Warning),
            "PolicySelected_Template" => (PolicySelectedId, LogLevel.Debug),
            "PolicyGroupsLoadFailed_Template" => (PolicyGroupsLoadFailedId, LogLevel.Warning),
            _ => (new EventId(2999, "PolicyEditorUnknown"), LogLevel.Debug)
        };
        logger.Log(level, id, message);
    }
}
