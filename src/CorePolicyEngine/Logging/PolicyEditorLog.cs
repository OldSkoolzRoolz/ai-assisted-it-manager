// Project Name: CorePolicyEngine
// File Name: PolicyEditorLog.cs
// Author: Automation
// License: MIT
// Do not remove file headers

using Microsoft.Extensions.Logging;

namespace KC.ITCompanion.CorePolicyEngine.Logging;

/// <summary>
/// Localized logger messages for Policy Editor actions (EventId 2000-2099).
/// Message templates resolved from resources externally before invocation to keep logger source-gen simple.
/// </summary>
internal static partial class PolicyEditorLog
{
    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "{Message}")]
    internal static partial void CatalogLoaded(this ILogger logger, string message);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "{Message}")]
    internal static partial void CatalogLoadFailed(this ILogger logger, string message);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Debug, Message = "{Message}")]
    internal static partial void SearchFilterApplied(this ILogger logger, string message);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Debug, Message = "{Message}")]
    internal static partial void PolicySelected(this ILogger logger, string message);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Warning, Message = "{Message}")]
    internal static partial void PolicyKeyNotFound(this ILogger logger, string message);
}
