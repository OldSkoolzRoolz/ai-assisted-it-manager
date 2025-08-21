// Project Name: ClientApp
// File Name: FileLogger.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KC.ITCompanion.ClientApp.Logging;

public interface ILogFileAccessor
{
    string CurrentLogFilePath { get; }
    IEnumerable<string> GetRecentLogFiles(int days = 7);
}

public interface ILogHealthAccessor
{
    IReadOnlyCollection<LogSinkHealth> GetHealth();
}

public sealed record LogSinkHealth(
    string Module,
    long Enqueued,
    long Written,
    long Dropped,
    long WriteErrors,
    DateTime? LastErrorUtc,
    bool CircuitOpen
);

internal sealed class FileLoggerProvider : ILoggerProvider, ILogFileAccessor, ILogHealthAccessor
{
    private readonly string _logDirectory;
    private readonly int _maxFileSizeBytes;
    private readonly int _maxQueueDepthPerModule;
    private readonly int _circuitErrorThreshold;
    private readonly TimeSpan _circuitErrorWindow;
    private readonly object _sinkGate = new();
    private readonly ConcurrentDictionary<string, ModuleSink> _sinks = new(StringComparer.OrdinalIgnoreCase);

    // Fallback failover file (shared) for sink failures
    private string FailoverFilePath => Path.Combine(_logDirectory, $"_failover-{DateTime.UtcNow:yyyyMMdd}.log");

    public string CurrentLogFilePath => string.Empty; // obsolete (multi-file design)

    private sealed record LogItem(DateTime Utc, LogLevel Level, EventId EventId, string Category, string Message, Exception? Exception);

    private sealed class ModuleSink
    {
        public string Module { get; }
        public BlockingCollection<LogItem> Queue { get; }
        public Task Worker { get; }
        public WriterState WriterState { get; set; }
        public long Enqueued;
        public long Written;
        public long Dropped;
        public long WriteErrors;
        public DateTime? LastErrorUtc;
        public bool CircuitOpen;
        public readonly ConcurrentQueue<DateTime> RecentErrors = new();
        public ModuleSink(string module, BlockingCollection<LogItem> queue, Task worker, WriterState writer)
        { Module = module; Queue = queue; Worker = worker; WriterState = writer; }
    }

    private sealed class WriterState
    {
        public DateTime Date { get; set; }
        public StreamWriter Writer { get; set; }
        public string Path { get; set; }
        public WriterState(DateTime date, StreamWriter writer, string path)
        { Date = date; Writer = writer; Path = path; }
    }

    public FileLoggerProvider(
        string? baseDirectory = null,
        int maxFileSizeBytes = 5_000_000,
        int maxQueueDepthPerModule = 5000,
        int circuitErrorThreshold = 25,
        int circuitErrorWindowSeconds = 60)
    {
        _logDirectory = Path.Combine(baseDirectory ?? AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(_logDirectory);
        _maxFileSizeBytes = maxFileSizeBytes;
        _maxQueueDepthPerModule = Math.Max(100, maxQueueDepthPerModule);
        _circuitErrorThreshold = Math.Max(5, circuitErrorThreshold);
        _circuitErrorWindow = TimeSpan.FromSeconds(Math.Max(10, circuitErrorWindowSeconds));
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

    internal void Enqueue(string category, LogLevel level, EventId eventId, string message, Exception? ex)
    {
        var module = GetModuleKey(category);
        var sink = GetOrCreateSink(module);
        if (sink.CircuitOpen)
        {
            // Drop to prevent unbounded memory; write minimal failover line
            Interlocked.Increment(ref sink.Dropped);
            WriteFailoverLine(module, "CIRCUIT_OPEN", level, eventId.Id, message, ex);
            return;
        }
        var item = new LogItem(DateTime.UtcNow, level, eventId, category, message, ex);
        if (!sink.Queue.TryAdd(item))
        {
            Interlocked.Increment(ref sink.Dropped);
            WriteFailoverLine(module, "QUEUE_FULL", level, eventId.Id, message, ex);
            return;
        }
        Interlocked.Increment(ref sink.Enqueued);
    }

    private ModuleSink GetOrCreateSink(string module)
    {
        return _sinks.GetOrAdd(module, m =>
        {
            lock (_sinkGate)
            {
                var queue = new BlockingCollection<LogItem>(new ConcurrentQueue<LogItem>(), _maxQueueDepthPerModule);
                var writer = CreateWriterState(m);
                var sink = new ModuleSink(m, queue, Task.Run(() => RunSinkAsync(m, queue, writer)), writer);
                return sink;
            }
        });
    }

    private async Task RunSinkAsync(string module, BlockingCollection<LogItem> queue, WriterState writer)
    {
        if (!_sinks.TryGetValue(module, out var sink)) return;
        try
        {
            foreach (var item in queue.GetConsumingEnumerable())
            {
                try
                {
                    RotateIfNeeded(sink);
                    WriteStructuredLine(sink, item);
                    Interlocked.Increment(ref sink.Written);
                }
                catch (Exception ex)
                {
                    RegisterSinkError(sink, ex, item);
                }
            }
        }
        catch (Exception exOuter)
        {
            RegisterSinkError(sink, exOuter, null);
        }
        finally
        {
            try { sink.WriterState.Writer.Flush(); sink.WriterState.Writer.Dispose(); } catch { }
        }
        await Task.CompletedTask;
    }

    private void RegisterSinkError(ModuleSink sink, Exception ex, LogItem? item)
    {
        Interlocked.Increment(ref sink.WriteErrors);
        sink.LastErrorUtc = DateTime.UtcNow;
        PruneErrorWindow(sink);
        sink.RecentErrors.Enqueue(sink.LastErrorUtc.Value);
        if (sink.RecentErrors.Count(e => sink.LastErrorUtc.Value - e <= _circuitErrorWindow) >= _circuitErrorThreshold)
        {
            sink.CircuitOpen = true;
            WriteFailoverLine(sink.Module, "SINK_DEGRADED", LogLevel.Error, item?.EventId.Id ?? 0, "Circuit opened due to repeated errors", ex);
        }
        else
        {
            WriteFailoverLine(sink.Module, "WRITE_ERROR", item?.Level ?? LogLevel.Error, item?.EventId.Id ?? 0, item?.Message ?? "(internal)", ex);
        }
    }

    private void PruneErrorWindow(ModuleSink sink)
    {
        while (sink.RecentErrors.TryPeek(out var ts) && sink.LastErrorUtc!.Value - ts > _circuitErrorWindow)
            sink.RecentErrors.TryDequeue(out _);
    }

    private void WriteStructuredLine(ModuleSink sink, LogItem item)
    {
        var payload = new
        {
            ts = item.Utc.ToString("O"),
            level = item.Level.ToString(),
            cat = item.Category,
            module = sink.Module,
            id = item.EventId.Id,
            name = item.EventId.Name,
            message = item.Message,
            exception = item.Exception?.ToString(),
            session = LogSession.SessionId,
            host = LogSession.Host,
            user = LogSession.User,
            pid = LogSession.ProcessId,
            ver = LogSession.AppVersion,
            moduleVer = GetModuleVersion(sink.Module)
        };
        sink.WriterState.Writer.WriteLine(JsonSerializer.Serialize(payload));
    }

    private void WriteFailoverLine(string module, string reason, LogLevel level, int eventId, string message, Exception? ex)
    {
        try
        {
            var line = JsonSerializer.Serialize(new
            {
                ts = DateTime.UtcNow.ToString("O"),
                level = level.ToString(),
                module,
                reason,
                id = eventId,
                message,
                exception = ex?.GetType().Name + ": " + ex?.Message
            });
            File.AppendAllText(FailoverFilePath, line + Environment.NewLine);
        }
        catch
        {
            // Last resort: swallow; nothing else we can do without causing app failure
        }
    }

    private string GetModuleVersion(string moduleKey)
    {
        try
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => string.Equals(a.GetName().Name, moduleKey, StringComparison.OrdinalIgnoreCase));
            if (asm != null) return asm.GetName().Version?.ToString() ?? "0.0.0";
        }
        catch { }
        return LogSession.AppVersion;
    }

    private static string GetModuleKey(string category)
    {
        if (string.IsNullOrWhiteSpace(category)) return "unknown";
        var root = category.Split('.')[0];
        return Sanitize(root);
    }

    private static string Sanitize(string moduleKey)
        => string.Concat(moduleKey.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_'));

    private WriterState CreateWriterState(string module)
    {
        var date = DateTime.UtcNow.Date;
        var path = EnsureSizeSlot(Path.Combine(_logDirectory, $"{module.ToLowerInvariant()}-{date:yyyyMMdd}.log"));
        var writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)) { AutoFlush = true };
        return new WriterState(date, writer, path);
    }

    private void RotateIfNeeded(ModuleSink sink)
    {
        var nowDate = DateTime.UtcNow.Date;
        bool needRotate = sink.WriterState.Date != nowDate || (sink.WriterState.Writer.BaseStream.Length > _maxFileSizeBytes);
        if (!needRotate) return;
        try { sink.WriterState.Writer.Flush(); sink.WriterState.Writer.Dispose(); } catch { }
        sink.WriterState.Date = nowDate;
        sink.WriterState.Path = EnsureSizeSlot(Path.Combine(_logDirectory, $"{sink.Module.ToLowerInvariant()}-{nowDate:yyyyMMdd}.log"));
        sink.WriterState.Writer = new StreamWriter(new FileStream(sink.WriterState.Path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)) { AutoFlush = true };
    }

    private string EnsureSizeSlot(string basePath)
    {
        if (!File.Exists(basePath)) return basePath;
        if (new FileInfo(basePath).Length <= _maxFileSizeBytes) return basePath;
        int i = 1;
        while (true)
        {
            var candidate = Path.Combine(Path.GetDirectoryName(basePath)!, Path.GetFileNameWithoutExtension(basePath) + $"-{i}" + Path.GetExtension(basePath));
            if (!File.Exists(candidate) || new FileInfo(candidate).Length <= _maxFileSizeBytes) return candidate;
            i++;
        }
    }

    public IEnumerable<string> GetRecentLogFiles(int days = 7)
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(-days);
        foreach (var file in Directory.EnumerateFiles(_logDirectory, "*.log", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(file);
            var idx = name.LastIndexOf('-');
            if (idx > 0 && name.Length >= idx + 1 + 8)
            {
                var slice = name.Substring(idx + 1, 8);
                if (DateTime.TryParseExact(slice, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var d) && d < cutoff)
                    continue;
            }
            yield return file;
        }
    }

    public IReadOnlyCollection<LogSinkHealth> GetHealth()
    {
        var list = new List<LogSinkHealth>(_sinks.Count);
        foreach (var s in _sinks.Values)
        {
            list.Add(new LogSinkHealth(s.Module, s.Enqueued, s.Written, s.Dropped, s.WriteErrors, s.LastErrorUtc, s.CircuitOpen));
        }
        return list;
    }

    public void Dispose()
    {
        foreach (var sink in _sinks.Values)
        {
            sink.Queue.CompleteAdding();
        }
        Task.WaitAll(_sinks.Values.Select(v => v.Worker).ToArray(), 2000);
        foreach (var sink in _sinks.Values)
        {
            try { sink.WriterState.Writer.Flush(); sink.WriterState.Writer.Dispose(); } catch { }
        }
    }
}

internal sealed class FileLogger : ILogger
{
    private readonly FileLoggerProvider _provider;
    private readonly string _category;
    public FileLogger(FileLoggerProvider provider, string category)
    { _provider = provider; _category = category; }
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        string message = string.Empty;
        try { message = formatter(state, exception); } catch { message = state?.ToString() ?? string.Empty; }
        _provider.Enqueue(_category, logLevel, eventId, message, exception);
    }
    private sealed class NullScope : IDisposable { public static readonly NullScope Instance = new(); public void Dispose() { } }
}

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string? baseDirectory = null)
    {
        var provider = new FileLoggerProvider(baseDirectory);
        builder.AddProvider(provider);
        builder.Services.AddSingleton<ILogFileAccessor>(provider);
        builder.Services.AddSingleton<ILogHealthAccessor>(provider);
        return builder;
    }
}
