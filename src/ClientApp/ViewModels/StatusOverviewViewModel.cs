// Project Name: ClientApp
// File Name: StatusOverviewViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace KC.ITCompanion.ClientApp.ViewModels;

public sealed class HighImportanceEvent
{
    public DateTime Timestamp { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class ActiveAlert
{
    public string Severity { get; init; } = string.Empty; // INFO/WARN/ERROR
    public string Description { get; init; } = string.Empty;
    public System.Windows.Media.Brush SeverityBrush => Severity switch
    {
        "ERROR" => System.Windows.Media.Brushes.Red,
        "WARN" => System.Windows.Media.Brushes.Orange,
        _ => System.Windows.Media.Brushes.LightGray
    };
}

public sealed class StatusOverviewViewModel : INotifyPropertyChanged
{
    private readonly ILogger<StatusOverviewViewModel> _logger;

    public ObservableCollection<HighImportanceEvent> HighImportanceEvents { get; } = [];
    public ObservableCollection<ActiveAlert> ActiveAlerts { get; } = [];

    private string _overallHealth = "Unknown";
    public string OverallHealth { get => _overallHealth; set { if (_overallHealth != value) { _overallHealth = value; OnPropertyChanged(); OnPropertyChanged(nameof(OverallHealthBrush)); } } }

    public System.Windows.Media.Brush OverallHealthBrush => OverallHealth switch
    {
        "Good" => System.Windows.Media.Brushes.LightGreen,
        "Degraded" => System.Windows.Media.Brushes.Orange,
        "Critical" => System.Windows.Media.Brushes.Red,
        _ => System.Windows.Media.Brushes.Gray
    };

    public int ActiveAlertCount => ActiveAlerts.Count;
    public int DriftCount { get => _driftCount; private set { if (_driftCount != value) { _driftCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(DriftBrush)); } } }
    private int _driftCount;
    public System.Windows.Media.Brush DriftBrush => DriftCount == 0 ? System.Windows.Media.Brushes.LightGreen : (DriftCount < 5 ? System.Windows.Media.Brushes.Orange : System.Windows.Media.Brushes.Red);

    public StatusOverviewViewModel(ILogger<StatusOverviewViewModel> logger)
    {
        _logger = logger;
        SeedSample();
    }

    private void SeedSample()
    {
        OverallHealth = "Good";
        HighImportanceEvents.Clear();
        HighImportanceEvents.Add(new HighImportanceEvent { Timestamp = DateTime.UtcNow.AddMinutes(-2), Message = "Policy catalog loaded" });
        HighImportanceEvents.Add(new HighImportanceEvent { Timestamp = DateTime.UtcNow.AddMinutes(-1), Message = "No drift detected in last scan" });
        ActiveAlerts.Clear();
        // placeholder: in future populate from monitoring services
        DriftCount = 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
