using System.Diagnostics;

using Shared.Constants;


namespace Security;

public interface IAuditLogService
{
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? ex = null);
}

public sealed class AuditLogService : IAuditLogService
{
    public void Info(string message)
    {
        Write("INFO", message);
    }

    public void Warn(string message)
    {
        Write("WARN", message);
    }

    public void Error(string message, Exception? ex = null)
    {
        Write("ERROR", $"{message}{(ex is null ? "" : $" :: {ex.Message}")}");
    }

    private static void Write(string level, string message)
    {
        Trace.WriteLine($"[{ProjectInfo.Name}] [{level}] {message}");
    }
}