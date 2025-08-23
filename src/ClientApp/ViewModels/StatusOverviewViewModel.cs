// Project Name: ClientApp
// File Name: StatusOverviewViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
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

    public System.Windows.Media.Brush SeverityBrush =>
        this.Severity switch
        {
            "ERROR" => System.Windows.Media.Brushes.Red,
            "WARN" => System.Windows.Media.Brushes.Orange,
            _ => System.Windows.Media.Brushes.LightGray
        };
}



public sealed class StatusOverviewViewModel : INotifyPropertyChanged
{
    private readonly ILogger<StatusOverviewViewModel> _logger;
    private int _driftCount;

    private string _overallHealth = "Unknown";





    public StatusOverviewViewModel(ILogger<StatusOverviewViewModel> logger)
    {
        this._logger = logger;
        SeedSample();
    }





    public ObservableCollection<HighImportanceEvent> HighImportanceEvents { get; } = [];
    public ObservableCollection<ActiveAlert> ActiveAlerts { get; } = [];

    public string OverallHealth
    {
        get => this._overallHealth;
        set
        {
            if (this._overallHealth != value)
            {
                this._overallHealth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(this.OverallHealthBrush));
            }
        }
    }

    public System.Windows.Media.Brush OverallHealthBrush =>
        this.OverallHealth switch
        {
            "Good" => System.Windows.Media.Brushes.LightGreen,
            "Degraded" => System.Windows.Media.Brushes.Orange,
            "Critical" => System.Windows.Media.Brushes.Red,
            _ => System.Windows.Media.Brushes.Gray
        };

    public int ActiveAlertCount => this.ActiveAlerts.Count;

    public int DriftCount
    {
        get => this._driftCount;
        private set
        {
            if (this._driftCount != value)
            {
                this._driftCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(this.DriftBrush));
            }
        }
    }

    public System.Windows.Media.Brush DriftBrush => this.DriftCount == 0 ? System.Windows.Media.Brushes.LightGreen :
        this.DriftCount < 5 ? System.Windows.Media.Brushes.Orange : System.Windows.Media.Brushes.Red;

    public event PropertyChangedEventHandler? PropertyChanged;





    private void SeedSample()
    {
        this.OverallHealth = "Good";
        this.HighImportanceEvents.Clear();
        this.HighImportanceEvents.Add(new HighImportanceEvent
            { Timestamp = DateTime.UtcNow.AddMinutes(-2), Message = "Policy catalog loaded" });
        this.HighImportanceEvents.Add(new HighImportanceEvent
            { Timestamp = DateTime.UtcNow.AddMinutes(-1), Message = "No drift detected in last scan" });
        this.ActiveAlerts.Clear();
        // placeholder: in future populate from monitoring services
        this.DriftCount = 0;
    }





    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}