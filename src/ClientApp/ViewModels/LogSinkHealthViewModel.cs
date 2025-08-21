// Project Name: ClientApp
// File Name: LogSinkHealthViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KC.ITCompanion.ClientApp.Logging;

namespace KC.ITCompanion.ClientApp.ViewModels;

/// <summary>
/// ViewModel for displaying logging system health metrics without expensive Count() operations.
/// </summary>
public class LogSinkHealthViewModel : INotifyPropertyChanged
{
    private readonly ILogHealthMonitor _healthMonitor;
    private System.Threading.Timer? _refreshTimer;

    public string Module { get; init; } = "FileLogger";
    
    public long Enqueued => _healthMonitor.MessagesEnqueued;
    public long Written => _healthMonitor.MessagesWritten;
    public long Dropped => _healthMonitor.MessagesDropped;
    public long WriteErrors => _healthMonitor.WriteErrors;
    public DateTime? LastErrorUtc => _healthMonitor.LastErrorUtc;
    public bool CircuitOpen => !_healthMonitor.IsHealthy;

    public string HealthStatus => _healthMonitor.IsHealthy ? "Healthy" : "Unhealthy";
    public string HealthStatusColor => _healthMonitor.IsHealthy ? "Green" : "Red";

    public ICommand RefreshCommand { get; }

    public LogSinkHealthViewModel(ILogHealthMonitor healthMonitor)
    {
        _healthMonitor = healthMonitor;
        RefreshCommand = new RelayCommand(_ => RefreshMetrics(), _ => true);
        
        // Auto-refresh every 5 seconds
        _refreshTimer = new System.Threading.Timer(_ => RefreshMetrics(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }

    private void RefreshMetrics()
    {
        // Refresh all properties - these are O(1) operations using Interlocked.Read
        OnPropertyChanged(nameof(Enqueued));
        OnPropertyChanged(nameof(Written));
        OnPropertyChanged(nameof(Dropped));
        OnPropertyChanged(nameof(WriteErrors));
        OnPropertyChanged(nameof(LastErrorUtc));
        OnPropertyChanged(nameof(CircuitOpen));
        OnPropertyChanged(nameof(HealthStatus));
        OnPropertyChanged(nameof(HealthStatusColor));
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged([CallerMemberName] string? name = null) 
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}