// Project Name: ClientApp
// File Name: LogViewerViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using KC.ITCompanion.ClientApp.Logging;
using Microsoft.Extensions.Logging;
using KC.ITCompanion.CorePolicyEngine.Storage;
using KC.ITCompanion.CorePolicyEngine.Models;

namespace KC.ITCompanion.ClientApp.ViewModels;

public class LogViewerEntry
{
    public DateTime Timestamp { get; init; }
    public string Level { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int EventId { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Session { get; init; } = string.Empty;
}

public class LogSinkHealthViewModel
{
    public string Module { get; init; } = string.Empty;
    public long Enqueued { get; init; }
    public long Written { get; init; }
    public long Dropped { get; init; }
    public long WriteErrors { get; init; }
    public DateTime? LastErrorUtc { get; init; }
    public bool CircuitOpen { get; init; }
}

public class LogViewerViewModel : INotifyPropertyChanged, IDisposable
{
    private bool _disposed = false;
    private readonly ILogFileAccessor _logAccessor;
    private readonly ILogHealthAccessor? _healthAccessor;
    private readonly ILogger<LogViewerViewModel> _logger;
    private readonly IBehaviorPolicyStore _policyStore;
    private BehaviorPolicy _policy = BehaviorPolicy.Default;
    private readonly CancellationTokenSource _cts = new();

    public ObservableCollection<LogViewerEntry> Entries { get; } = [];
    public ObservableCollection<string> Levels { get; } = new(["TRACE","DEBUG","INFORMATION","WARNING","ERROR","CRITICAL"]);
    public ObservableCollection<LogSinkHealthViewModel> LogHealth { get; } = [];

    private string? _filterText;
    public string? FilterText { get => _filterText; set { if (_filterText != value) { _filterText = value; OnPropertyChanged(); Load(); } } }

    private string? _selectedLevel;
    public string? SelectedLevel { get => _selectedLevel; set { if (_selectedLevel != value) { _selectedLevel = value; OnPropertyChanged(); Load(); } } }

    public ICommand RefreshCommand { get; }

    public LogViewerViewModel(ILogFileAccessor accessor, ILogger<LogViewerViewModel> logger, IBehaviorPolicyStore policyStore, ILogHealthAccessor? healthAccessor = null)
    {
        _logAccessor = accessor;
        _logger = logger;
        _policyStore = policyStore;
        _healthAccessor = healthAccessor;
        RefreshCommand = new RelayCommand(_ => { Load(); LoadHealth(); }, _ => true);
        Task.Run(InitializeAsync);
    }

    private async Task InitializeAsync()
    {
        try
        {
            await _policyStore.InitializeAsync(_cts.Token).ConfigureAwait(false);
            var snap = await _policyStore.GetSnapshotAsync(_cts.Token).ConfigureAwait(false);
            _policy = snap.Effective;
            _ = Task.Run(() => PollLoopAsync(_cts.Token));
            Load();
            LoadHealth();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Log viewer initialization failed");
        }
    }

    private async Task PollLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(_policy.LogViewPollSeconds, 5, 300)), token).ConfigureAwait(false);
                Load();
                LoadHealth();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log polling loop error");
            }
        }
    }

    private void LoadHealth()
    {
        if (_healthAccessor == null) return;
        try
        {
            var snapshot = _healthAccessor.GetHealth();
            InvokeOnUiThread(() =>
            {
                LogHealth.Clear();
                foreach (var h in snapshot)
                {
                    LogHealth.Add(new LogSinkHealthViewModel
                    {
                        Module = h.Module,
                        Enqueued = h.Enqueued,
                        Written = h.Written,
                        Dropped = h.Dropped,
                        WriteErrors = h.WriteErrors,
                        LastErrorUtc = h.LastErrorUtc,
                        CircuitOpen = h.CircuitOpen
                    });
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed loading log health snapshot");
        }
    }

    private void Load()
    {
        try
        {
            // Collect in a temp list off the UI thread, then batch replace to minimize dispatcher hops.
            List<LogViewerEntry> buffer = new();
            int days = Math.Clamp(_policy.LogRetentionDays, 1, 365);
            var cutoffUtc = DateTime.UtcNow.AddDays(-days);
            foreach (var file in _logAccessor.GetRecentLogFiles(days))
            {
                try
                {
                    foreach (var line in SafeReadLines(file))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        try
                        {
                            using var doc = JsonDocument.Parse(line);
                            var root = doc.RootElement;
                            var lvl = root.TryGetProperty("level", out var lvlEl) ? (lvlEl.GetString() ?? string.Empty) : string.Empty;
                            if (!string.IsNullOrWhiteSpace(SelectedLevel) && !lvl.Equals(SelectedLevel, StringComparison.OrdinalIgnoreCase))
                                continue;
                            var msg = root.TryGetProperty("message", out var msgEl) ? (msgEl.GetString() ?? string.Empty) : string.Empty;
                            if (!string.IsNullOrWhiteSpace(FilterText) && !msg.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (!root.TryGetProperty("ts", out var tsEl) || tsEl.ValueKind != JsonValueKind.String && tsEl.ValueKind != JsonValueKind.Number)
                                continue;
                            DateTime ts;
                            try { ts = tsEl.GetDateTime(); } catch { continue; }
                            if (ts < cutoffUtc) continue;
                            buffer.Add(new LogViewerEntry
                            {
                                Timestamp = ts,
                                Level = lvl,
                                Category = root.TryGetProperty("cat", out var c) ? c.GetString() ?? string.Empty : string.Empty,
                                EventId = root.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.Number ? id.GetInt32() : 0,
                                Message = msg,
                                Session = root.TryGetProperty("session", out var s) ? s.GetString() ?? string.Empty : string.Empty
                            });
                        }
                        catch
                        {
                            // ignore malformed line
                        }
                    }
                }
                catch (IOException ioex)
                {
                    // Likely file still being rotated or exclusively locked; log once per file.
                    _logger.LogDebug(ioex, "Skipping locked log file {File}", file);
                }
                catch (Exception exFile)
                {
                    _logger.LogError(exFile, "Failed reading log file {File}", file);
                }
            }

            InvokeOnUiThread(() =>
            {
                Entries.Clear();
                foreach (var e in buffer.OrderBy(e => e.Timestamp))
                    Entries.Add(e);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Log load failed");
        }
    }

    // Opens file with FileShare.ReadWrite so we can read while logger still has it open.
    private static IEnumerable<string> SafeReadLines(string path)
    {
        FileStream? fs = null;
        try
        {
            fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var sr = new StreamReader(fs);
            fs = null; // ownership passed to StreamReader
            string? line;
            while ((line = sr.ReadLine()) != null)
                yield return line;
        }
        finally
        {
            fs?.Dispose();
        }
    }

    private static void InvokeOnUiThread(Action action)
    {
        var app = System.Windows.Application.Current;
        if (app?.Dispatcher?.CheckAccess() == true)
        {
            action();
        }
        else
        {
            app?.Dispatcher?.Invoke(action);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
