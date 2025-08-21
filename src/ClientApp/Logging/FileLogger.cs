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
using System.Reflection;
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

/// <summary>
/// Provides health metrics for the logging system without expensive Count() operations.
/// </summary>
public interface ILogHealthMonitor
{
    long MessagesEnqueued { get; }
    long MessagesWritten { get; }
    long MessagesDropped { get; }
    long WriteErrors { get; }
    DateTime? LastErrorUtc { get; }
    bool IsHealthy { get; }
}

internal sealed class FileLoggerProvider : ILoggerProvider, ILogFileAccessor, ILogHealthMonitor
{
    private readonly BlockingCollection<LogMessage> _queue = new(new ConcurrentQueue<LogMessage>());
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _processingTask;
    private readonly string _logDirectory;
    private string _currentFile = string.Empty;
    private StreamWriter? _writer;
    private DateTime _fileDate = DateTime.MinValue;
    private readonly object _sync = new();
    private readonly int _maxFileSizeBytes;

    // Thread-safe counters for efficient metrics tracking without Count() operations
    private long _messagesEnqueued = 0;
    private long _messagesWritten = 0;
    private long _messagesDropped = 0;
    private long _writeErrors = 0;
    private DateTime? _lastErrorUtc = null;
    
    // Circuit breaker state
    private const int MaxConsecutiveErrors = 10;
    private int _consecutiveErrors = 0;
    private bool _circuitOpen = false;
    private DateTime _circuitOpenTime = DateTime.MinValue;

    public string CurrentLogFilePath => _currentFile;

    // ILogHealthMonitor implementation - thread-safe properties using efficient reads
    public long MessagesEnqueued => Interlocked.Read(ref _messagesEnqueued);
    public long MessagesWritten => Interlocked.Read(ref _messagesWritten);
    public long MessagesDropped => Interlocked.Read(ref _messagesDropped);
    public long WriteErrors => Interlocked.Read(ref _writeErrors);
    public DateTime? LastErrorUtc => _lastErrorUtc;
    public bool IsHealthy => !_circuitOpen && _consecutiveErrors < MaxConsecutiveErrors;

    private sealed record LogMessage(DateTime TimestampUtc, string Category, LogLevel Level, EventId EventId, string Message, Exception? Exception, IReadOnlyDictionary<string, object?> State);

    public FileLoggerProvider(string? baseDirectory = null, int maxFileSizeBytes = 5_000_000)
    {
        _logDirectory = Path.Combine(baseDirectory ?? AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(_logDirectory);
        _maxFileSizeBytes = maxFileSizeBytes;
        _processingTask = Task.Run(ProcessQueueAsync);
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

    internal void Enqueue(string category, LogLevel level, EventId eventId, string message, Exception? ex, IReadOnlyDictionary<string, object?> state)
    {
        // Check circuit breaker state first
        if (_circuitOpen)
        {
            // Try to reset circuit breaker after 60 seconds
            if (DateTime.UtcNow.Subtract(_circuitOpenTime).TotalSeconds > 60)
            {
                _circuitOpen = false;
                _consecutiveErrors = 0;
            }
            else
            {
                Interlocked.Increment(ref _messagesDropped);
                return;
            }
        }

        if (!_queue.IsAddingCompleted)
        {
            try
            {
                _queue.Add(new LogMessage(DateTime.UtcNow, category, level, eventId, message, ex, state));
                Interlocked.Increment(ref _messagesEnqueued);
            }
            catch
            {
                // Queue is full or disposed - increment dropped counter
                Interlocked.Increment(ref _messagesDropped);
            }
        }
        else
        {
            Interlocked.Increment(ref _messagesDropped);
        }
    }

    private string GetModuleVersion(string category)
    {
        try
        {
            var parts = category.Split('.');
            if (parts.Length > 0)
            {
                var asmName = parts[0];
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (asm.GetName().Name?.Equals(asmName, StringComparison.OrdinalIgnoreCase) == true)
                        return asm.GetName().Version?.ToString() ?? "0.0.0";
                }
            }
        }
        catch { }
        return LogSession.AppVersion;
    }

    private async Task ProcessQueueAsync()
    {
        try
        {
            foreach (var msg in _queue.GetConsumingEnumerable(_cts.Token))
            {
                if (WriteMessage(msg))
                {
                    Interlocked.Increment(ref _messagesWritten);
                    // Reset consecutive errors on successful write
                    if (_consecutiveErrors > 0)
                    {
                        Interlocked.Exchange(ref _consecutiveErrors, 0);
                    }
                }
                else
                {
                    // WriteMessage failed, handle error tracking
                    HandleWriteError();
                }
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            lock (_sync)
            {
                _writer?.Flush();
                _writer?.Dispose();
            }
        }
        await Task.CompletedTask;
    }

    private bool WriteMessage(LogMessage msg)
    {
        try
        {
            lock (_sync)
            {
                RotateIfNeeded();
                if (_writer == null) return false;
                var payload = new
                {
                    ts = msg.TimestampUtc.ToString("O"),
                    level = msg.Level.ToString(),
                    cat = msg.Category,
                    id = msg.EventId.Id,
                    name = msg.EventId.Name,
                    message = msg.Message,
                    exception = msg.Exception?.ToString(),
                    session = LogSession.SessionId,
                    host = LogSession.Host,
                    user = LogSession.User,
                    pid = LogSession.ProcessId,
                    ver = LogSession.AppVersion,
                    moduleVer = GetModuleVersion(msg.Category)
                };
                _writer.WriteLine(JsonSerializer.Serialize(payload));
                return true;
            }
        }
        catch 
        { 
            return false;
        }
    }

    private void HandleWriteError()
    {
        Interlocked.Increment(ref _writeErrors);
        _lastErrorUtc = DateTime.UtcNow;
        
        var errors = Interlocked.Increment(ref _consecutiveErrors);
        
        // Open circuit breaker if too many consecutive errors
        if (errors >= MaxConsecutiveErrors && !_circuitOpen)
        {
            _circuitOpen = true;
            _circuitOpenTime = DateTime.UtcNow;
        }
    }

    private void RotateIfNeeded()
    {
        var today = DateTime.UtcNow.Date;
        if (today != _fileDate || _writer is null || (_writer.BaseStream.Length > _maxFileSizeBytes))
        {
            _writer?.Flush();
            _writer?.Dispose();
            _fileDate = today;
            var fileName = $"app-{today:yyyyMMdd}-{Guid.NewGuid():N}.log";
            _currentFile = Path.Combine(_logDirectory, fileName);
            _writer = new StreamWriter(new FileStream(_currentFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) { AutoFlush = true };
        }
    }

    public IEnumerable<string> GetRecentLogFiles(int days = 7)
        => Directory.EnumerateFiles(_logDirectory, "app-*.log")
            .OrderByDescending(f => f)
            .TakeWhile(f =>
            {
                var name = Path.GetFileName(f);
                if (name.Length < 16) return true;
                var datePart = name.Substring(4, 8);
                if (DateTime.TryParseExact(datePart, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var d))
                    return d >= DateTime.UtcNow.Date.AddDays(-days);
                return true;
            });

    public void Dispose()
    {
        _queue.CompleteAdding();
        _cts.Cancel();
        try { _processingTask.Wait(2000); } catch { }
    }
}

internal sealed class FileLogger : ILogger
{
    private readonly FileLoggerProvider _provider;
    private readonly string _category;

    public FileLogger(FileLoggerProvider provider, string category)
    {
        _provider = provider;
        _category = category;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        var message = formatter(state, exception);
        IReadOnlyDictionary<string, object?> dict = state as IReadOnlyDictionary<string, object?> ?? new Dictionary<string, object?>();
        _provider.Enqueue(_category, logLevel, eventId, message, exception, dict);
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string? baseDirectory = null)
    {
        var provider = new FileLoggerProvider(baseDirectory);
        builder.AddProvider(provider);
        builder.Services.AddSingleton<ILogFileAccessor>(provider);
        builder.Services.AddSingleton<ILogHealthMonitor>(provider);
        return builder;
    }
}
