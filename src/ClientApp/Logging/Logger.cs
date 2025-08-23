// Project Name: ClientApp
// File Name: Logger.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


using System.IO;

using Microsoft.Extensions.Logging;


namespace KC.ITCompanion.ClientApp.Logging;


internal static partial class Logger // make sealed-equivalent (static) to satisfy CA1852 suggestion
{
    [LoggerMessage(LogLevel.Information, "ClientApp starting up at {startTimeUtc}")]
    public static partial void LogClientappStartingUpAtStarttimeutc(ILogger<App> logger, DateTime startTimeUtc);





    [LoggerMessage(LogLevel.Warning, "Audit store initialization failed")]
    public static partial void LogAuditStoreInitializationFailed(ILogger logger, Exception exception);





    [LoggerMessage(LogLevel.Warning, "Behavior policy store initialization failed; using default access groups")]
    public static partial void LogBehaviorPolicyStoreInitializationFailedUsingDefaultAccessGroups(ILogger logger,
        Exception exception);





    [LoggerMessage(LogLevel.Warning, "Access denied starting client: {reason}")]
    public static partial void LogAccessDeniedStartingClientReason(ILogger logger, string reason);





    [LoggerMessage(LogLevel.Error, "Log viewer initialization failed")]
    public static partial void
        LogLogViewerInitializationFailed(ILogger<LogViewerViewModel> logger, Exception exception);





    [LoggerMessage(LogLevel.Error, "Log polling loop error")]
    public static partial void LogLogPollingLoopError(ILogger<LogViewerViewModel> logger, Exception exception);





    [LoggerMessage(LogLevel.Error, "Failed loading log health snapshot")]
    public static partial void LogFailedLoadingLogHealthSnapshot(ILogger<LogViewerViewModel> logger,
        Exception exception);





    [LoggerMessage(LogLevel.Debug, "Skipping locked log file {file}")]
    public static partial void LogSkippingLockedLogFileFile(ILogger<LogViewerViewModel> logger, IOException ioex,
        string file);





    [LoggerMessage(LogLevel.Error, "Failed reading log file {file}")]
    public static partial void LogFailedReadingLogFileFile(ILogger<LogViewerViewModel> logger, Exception exFile,
        string file);





    [LoggerMessage(LogLevel.Error, "Log load failed")]
    public static partial void LogLogLoadFailed(ILogger<LogViewerViewModel> logger, Exception exception);
}