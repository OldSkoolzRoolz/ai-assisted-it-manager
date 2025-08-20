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

public class LogViewerViewModel : INotifyPropertyChanged
{
    private readonly ILogFileAccessor _logAccessor;
    private readonly ILogger<LogViewerViewModel> _logger;

    public ObservableCollection<LogViewerEntry> Entries { get; } = [];
    public ObservableCollection<string> Levels { get; } = new(["TRACE","DEBUG","INFORMATION","WARNING","ERROR","CRITICAL"]);

    private string? _filterText;
    public string? FilterText { get => _filterText; set { if (_filterText != value) { _filterText = value; OnPropertyChanged(); Load(); } } }

    private string? _selectedLevel;
    public string? SelectedLevel { get => _selectedLevel; set { if (_selectedLevel != value) { _selectedLevel = value; OnPropertyChanged(); Load(); } } }

    public ICommand RefreshCommand { get; }

    public LogViewerViewModel(ILogFileAccessor accessor, ILogger<LogViewerViewModel> logger)
    {
        _logAccessor = accessor;
        _logger = logger;
        RefreshCommand = new RelayCommand(_ => Load(), _ => true);
        Load();
    }

    private void Load()
    {
        Entries.Clear();
        foreach (var file in _logAccessor.GetRecentLogFiles(2))
        {
            try
            {
                foreach (var line in File.ReadLines(file))
                {
                    LogViewerEntry? entry = null;
                    try
                    {
                        using var doc = JsonDocument.Parse(line);
                        var root = doc.RootElement;
                        var lvl = root.GetProperty("level").GetString() ?? "";
                        if (!string.IsNullOrWhiteSpace(SelectedLevel) && !lvl.Equals(SelectedLevel, StringComparison.OrdinalIgnoreCase))
                            continue;
                        var msg = root.GetProperty("message").GetString() ?? "";
                        if (!string.IsNullOrWhiteSpace(FilterText) && msg.Contains(FilterText, StringComparison.OrdinalIgnoreCase) == false)
                            continue;
                        entry = new LogViewerEntry
                        {
                            Timestamp = root.GetProperty("ts").GetDateTime(),
                            Level = lvl,
                            Category = root.GetProperty("cat").GetString() ?? "",
                            EventId = root.GetProperty("id").GetInt32(),
                            Message = msg,
                            Session = root.TryGetProperty("session", out var s) ? s.GetString() ?? "" : ""
                        };
                    }
                    catch { /* skip malformed line */ }
                    if (entry != null) Entries.Add(entry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed reading log file {File}", file);
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
