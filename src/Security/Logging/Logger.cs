// Project Name: Security
// File Name: Logger.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


using Microsoft.Extensions.Logging;


namespace Security.Logging;


/// <summary>
///     Central strongly-typed logging helpers for the Security project.
///     Uses source-generated LoggerMessage for high-performance structured logging.
/// </summary>
internal static partial class Logger
{
    // Initialization
    [LoggerMessage(EventId = 1000, Level = LogLevel.Debug,
        Message = "Access policy initialized allowed='{Allowed}' allowAny={AllowAny}")]
    internal static partial void AccessPolicyInitialized(this ILogger logger, string allowed, bool allowAny);





    // Bypass / wildcard
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Bypass env var for '{User}'")]
    internal static partial void AccessBypass(this ILogger logger, string user);





    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Wildcard grant '{User}'")]
    internal static partial void WildcardGrant(this ILogger logger, string user);





    // Generic grant with reason category
    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Grant ({Reason}) '{User}'")]
    internal static partial void Grant(this ILogger logger, string reason, string user);





    // Deny (warning)
    [LoggerMessage(EventId = 1010, Level = LogLevel.Warning,
        Message = "User not in allowed groups User='{User}' Allowed='{Allowed}' tokenGroups={TokenGroups}")]
    internal static partial void AccessDenied(this ILogger logger, string user, string allowed, string tokenGroups);





    // Evaluation error (fail-open)
    [LoggerMessage(EventId = 1015, Level = LogLevel.Error, Message = "Access evaluation error '{User}' fail-open")]
    internal static partial void AccessEvaluationError(this ILogger logger, Exception exception, string user);
}