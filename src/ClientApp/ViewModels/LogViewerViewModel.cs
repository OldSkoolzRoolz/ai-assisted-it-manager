// Project Name: ClientApp
// File Name: LogViewerViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using KC.ITCompanion.CorePolicyEngine.Models;
using KC.ITCompanion.CorePolicyEngine.Storage;
using Microsoft.Extensions.Logging;

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
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogHealthAccessor? _healthAccessor;
    private readonly ILogFileAccessor _logAccessor;
    private readonly ILogger<LogViewerViewModel> _logger;
    private readonly IBehaviorPolicyStore _policyStore;
    private bool _disposed; // CA1805 removed explicit init

    private string? _filterText;
    private BehaviorPolicy _policy = BehaviorPolicy.Default;
    private string? _selectedLevel;

    public LogViewerViewModel(ILogFileAccessor accessor, ILogger<LogViewerViewModel> logger,
        IBehaviorPolicyStore policyStore, ILogHealthAccessor? healthAccessor = null)
    {
        this._logAccessor = accessor;
        this._logger = logger;
        this._policyStore = policyStore;
        this._healthAccessor = healthAccessor;
        this.RefreshCommand = new RelayCommand(_ =>
        {
            Load();
            LoadHealth();
        }, _ => true);
        Task.Run(InitializeAsync);
    }

    public ObservableCollection<LogViewerEntry> Entries { get; } = [];
    public ObservableCollection<string> Levels { get; } =
        new(["TRACE", "DEBUG", "INFORMATION", "WARNING", "ERROR", "CRITICAL"]);
    public ObservableCollection<LogSinkHealthViewModel> LogHealth { get; } = [];

    public string? FilterText
    {
        get => this._filterText;
        set
        {
            if (this._filterText != value)
            {
                this._filterText = value;
                OnPropertyChanged();
                Load();
            }
        }
    }

    public string? SelectedLevel
    {
        get => this._selectedLevel;
        set
        {
            if (this._selectedLevel != value)
            {
                this._selectedLevel = value;
                OnPropertyChanged();
                Load();
            }
        }
    }

    public ICommand RefreshCommand { get; }

    public void Dispose()
    {
        if (_disposed) return; // idempotent & CA1816 pattern improvement
        _disposed = true;
        this._cts.Cancel();
        this._cts.Dispose();
        GC.SuppressFinalize(this);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task InitializeAsync()
    {
        try
        {
            await this._policyStore.InitializeAsync(this._cts.Token).ConfigureAwait(false);
            BehaviorPolicySnapshot snap =
                await this._policyStore.GetSnapshotAsync(this._cts.Token).ConfigureAwait(false);
            this._policy = snap.Effective;
            _ = Task.Run(() => PollLoopAsync(this._cts.Token));
            Load();
            LoadHealth();
        }
        catch (Exception ex)
        {
            Logger.LogLogViewerInitializationFailed(this._logger, ex);
        }
    }

    private async Task PollLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(this._policy.LogViewPollSeconds, 5, 300)), token)
                    .ConfigureAwait(false);
                Load();
                LoadHealth();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Logger.LogLogPollingLoopError(this._logger, ex);
            }
    }

    private void LoadHealth()
    {
        if (this._healthAccessor == null) return;
        try
        {
            IReadOnlyCollection<LogSinkHealth> snapshot = this._healthAccessor.GetHealth();
            InvokeOnUiThread(() =>
            {
                this.LogHealth.Clear();
                foreach (LogSinkHealth h in snapshot)
                    this.LogHealth.Add(new LogSinkHealthViewModel
                    {
                        Module = h.Module,
                        Enqueued = h.Enqueued,
                        Written = h.Written,
                        Dropped = h.Dropped,
                        WriteErrors = h.WriteErrors,
                        LastErrorUtc = h.LastErrorUtc,
                        CircuitOpen = h.CircuitOpen
                    });
            });
        }
        catch (Exception ex)
        {
            Logger.LogFailedLoadingLogHealthSnapshot(this._logger, ex);
        }
    }

    private void Load()
    {
        try
        {
            List<LogViewerEntry> buffer = new();
            var days = Math.Clamp(this._policy.LogRetentionDays, 1, 365);
            DateTime cutoffUtc = DateTime.UtcNow.AddDays(-days);
            foreach (var file in this._logAccessor.GetRecentLogFiles(days))
                try
                {
                    foreach (var line in SafeReadLines(file))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        try
                        {
                            using var doc = JsonDocument.Parse(line);
                            JsonElement root = doc.RootElement;
                            var lvl = root.TryGetProperty("level", out JsonElement lvlEl)
                                ? lvlEl.GetString() ?? string.Empty
                                : string.Empty;
                            if (!string.IsNullOrWhiteSpace(this.SelectedLevel) &&
                                !lvl.Equals(this.SelectedLevel, StringComparison.OrdinalIgnoreCase))
                                continue;
                            var msg = root.TryGetProperty("message", out JsonElement msgEl)
                                ? msgEl.GetString() ?? string.Empty
                                : string.Empty;
                            if (!string.IsNullOrWhiteSpace(this.FilterText) &&
                                !msg.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (!root.TryGetProperty("ts", out JsonElement tsEl) ||
                                (tsEl.ValueKind != JsonValueKind.String && tsEl.ValueKind != JsonValueKind.Number))
                                continue;
                            DateTime ts;
                            try
                            {
                                ts = tsEl.GetDateTime();
                            }
                            catch
                            {
                                continue;
                            }

                            if (ts < cutoffUtc) continue;
                            buffer.Add(new LogViewerEntry
                            {
                                Timestamp = ts,
                                Level = lvl,
                                Category = root.TryGetProperty("cat", out JsonElement c)
                                    ? c.GetString() ?? string.Empty
                                    : string.Empty,
                                EventId = root.TryGetProperty("id", out JsonElement id) &&
                                          id.ValueKind == JsonValueKind.Number
                                    ? id.GetInt32()
                                    : 0,
                                Message = msg,
                                Session = root.TryGetProperty("session", out JsonElement s)
                                    ? s.GetString() ?? string.Empty
                                    : string.Empty
                            });
                        }
                        catch
                        {
                        }
                    }
                }
                catch (IOException ioex)
                {
                    Logger.LogSkippingLockedLogFileFile(this._logger, ioex, file);
                }
                catch (Exception exFile)
                {
                    Logger.LogFailedReadingLogFileFile(this._logger, exFile, file);
                }

            InvokeOnUiThread(() =>
            {
                this.Entries.Clear();
                foreach (LogViewerEntry e in buffer.OrderBy(e => e.Timestamp)) this.Entries.Add(e);
            });
        }
        catch (Exception ex)
        {
            Logger.LogLogLoadFailed(this._logger, ex);
        }
    }

    private static IEnumerable<string> SafeReadLines(string path)
    {
        FileStream? fs = null;
        try
        {
            fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var sr = new StreamReader(fs);
            fs = null;
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
        Application? app = Application.Current;
        if (app?.Dispatcher?.CheckAccess() == true)
            action();
        else
            app?.Dispatcher?.Invoke(action);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}