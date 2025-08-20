// Project Name: ClientApp
// File Name: LogSession.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace KC.ITCompanion.ClientApp.Logging;

/// <summary>
/// Provides ambient session information to enrich log records. Invariant to enable aggregation.
/// </summary>
internal static class LogSession
{
    static LogSession()
    {
        SessionId = Guid.NewGuid().ToString("N");
        Host = Environment.MachineName;
        User = Environment.UserName;
        ProcessId = Environment.ProcessId;
        AppVersion = typeof(LogSession).Assembly.GetName().Version?.ToString() ?? "0.0.0";
    }

    public static string SessionId { get; }
    public static string Host { get; }
    public static string User { get; }
    public static int ProcessId { get; }
    public static string AppVersion { get; }
}
