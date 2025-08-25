// Project Name: ITCompanionClient
// File Name: LogsPage.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KC.ITCompanion.CorePolicyEngine.Storage.Sql;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace ITCompanionClient.Controls;
/// <summary>
/// Live log viewer page pulling recent events from the database on an interval.
/// </summary>
public sealed partial class LogsPage : UserControl, INotifyPropertyChanged, IDisposable
{
    private readonly ILogEventQueryRepository _repo;
    private readonly DispatcherQueue _dispatcher;
    private CancellationTokenSource? _pollCts;
    private int _pollSeconds = 5;
    private byte? _minLevel;
    private string? _search;
    private bool _autoScroll = true;
    private DateTime _lastRefreshUtc = DateTime.MinValue;
    private bool _disposed;

    /// <summary>Collection of log event view models currently displayed.</summary>
    public ObservableCollection<LogItemViewModel> Events { get; } = new();

    /// <summary>Polling interval seconds (1-600).</summary>
    public int PollSeconds { get => _pollSeconds; set { if (value != _pollSeconds) { _pollSeconds = Math.Clamp(value, 1, 600); OnPropertyChanged(); RestartPolling(); } } }
    /// <summary>Minimum log level filter inclusive.</summary>
    public byte? MinLevel { get => _minLevel; set { if (_minLevel != value) { _minLevel = value; OnPropertyChanged(); _ = RefreshAsync(); } } }
    /// <summary>Search text applied to message/category.</summary>
    public string? Search { get => _search; set { if (_search != value) { _search = value; OnPropertyChanged(); _ = RefreshAsync(); } } }
    /// <summary>Auto-scrolls to newest entry when enabled.</summary>
    public bool AutoScroll { get => _autoScroll; set { if (_autoScroll != value) { _autoScroll = value; OnPropertyChanged(); } } }

    /// <summary>Create log viewer page.</summary>
    public LogsPage()
    {
        InitializeComponent();
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        _repo = (ILogEventQueryRepository)App.Services.GetService(typeof(ILogEventQueryRepository))!;
        DataContext = this;
        Loaded += OnLoaded; Unloaded += OnUnloaded;
    }

    /// <summary>Loaded event handler — starts polling loop.</summary>
    private void OnLoaded(object sender, RoutedEventArgs e) => RestartPolling();
    /// <summary>Unloaded event handler — cancels active polling.</summary>
    private void OnUnloaded(object sender, RoutedEventArgs e) => CancelPolling();

    /// <summary>Restarts the polling loop with current interval.</summary>
    private void RestartPolling()
    {
        CancelPolling();
        _pollCts = new CancellationTokenSource();
        _ = PollLoopAsync(_pollCts.Token);
    }

    /// <summary>Cancels active polling (if any).</summary>
    private void CancelPolling()
    {
        if (_pollCts != null)
        {
            try { _pollCts.Cancel(); } catch (ObjectDisposedException) { }
            _pollCts.Dispose();
            _pollCts = null;
        }
    }

    /// <summary>Background loop fetching log events until cancelled.</summary>
    private async Task PollLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try { await RefreshAsync().ConfigureAwait(false); }
            catch (OperationCanceledException) { }
            catch (Exception) { /* ignore transient fetch errors for UI */ }
            try { await Task.Delay(TimeSpan.FromSeconds(PollSeconds), token).ConfigureAwait(false); } catch (TaskCanceledException) { }
        }
    }

    /// <summary>Fetches latest log events and updates UI collection.</summary>
    private async Task RefreshAsync()
    {
        var list = await _repo.GetRecentAsync(500, MinLevel, Search, CancellationToken.None).ConfigureAwait(false);
        _lastRefreshUtc = DateTime.UtcNow;
        _dispatcher.TryEnqueue(() =>
        {
            Events.Clear();
            foreach (var e in list.OrderBy(x => x.Ts))
                Events.Add(new LogItemViewModel(e));
            if (AutoScroll)
                LogList.ScrollIntoView(Events.LastOrDefault());
        });
    }

    /// <summary>Clears all displayed log events.</summary>
    private void OnClear(object sender, RoutedEventArgs e) => Events.Clear();
    /// <summary>Pauses polling.</summary>
    private void OnPause(object sender, RoutedEventArgs e) => CancelPolling();
    /// <summary>Resumes polling.</summary>
    private void OnResume(object sender, RoutedEventArgs e) => RestartPolling();
    /// <summary>Updates minimum level based on selection change.</summary>
    private void OnLevelChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LevelCombo.SelectedItem is ComboBoxItem cbi)
            MinLevel = cbi.Tag is string s && byte.TryParse(s, out var b) ? b : null;
    }
    /// <summary>Search box text change handler.</summary>
    private void OnSearchChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => Search = sender.Text;

    /// <summary>Disposes polling resources.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        CancelPolling();
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        GC.SuppressFinalize(this);
    }

    /// <summary>Property changed notification event.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>Raises <see cref="PropertyChanged"/>.</summary>
    private void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

/// <summary>View model for a single log event row.</summary>
public sealed class LogItemViewModel
{
    /// <summary>Create view model from DTO.</summary>
    /// <param name="dto">Source log event DTO.</param>
    public LogItemViewModel(LogEventDto dto)
    { Dto = dto; Level = (byte)dto.Level; Ts = dto.Ts; Message = dto.Message ?? string.Empty; Category = dto.Category ?? string.Empty; SourceId = dto.LogSourceId; }
    /// <summary>Underlying DTO.</summary>
    public LogEventDto Dto { get; }
    /// <summary>Event id.</summary>
    public long Id => Dto.LogEventId;
    /// <summary>Source id.</summary>
    public int SourceId { get; }
    /// <summary>Numeric level.</summary>
    public byte Level { get; }
    /// <summary>Timestamp.</summary>
    public DateTime Ts { get; }
    /// <summary>Message text.</summary>
    public string Message { get; }
    /// <summary>Event category.</summary>
    public string Category { get; }
}

/// <summary>Level to badge brush converter (inline simple mapping).</summary>
public sealed class LogLevelToBrushConverter : IValueConverter
{
    /// <summary>Converts numeric level to a color hex value for UI styling.</summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is byte b)
        {
            return b switch
            {
                >= 5 => "#FFD13438", // critical
                4 => "#FFE81123", // error
                3 => "#FFFFB900", // warn
                2 => "#FF107C10", // info
                1 => "#FF0050EF", // debug
                _ => "#FF838383" // trace
            };
        }
        return "#FF838383";
    }
    /// <summary>Back conversion not supported.</summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
}