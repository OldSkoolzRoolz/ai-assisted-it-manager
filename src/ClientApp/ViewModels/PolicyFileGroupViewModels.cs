// Project Name: ClientApp
// File Name: PolicyFileGroupViewModels.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;

namespace KC.ITCompanion.ClientApp.ViewModels;

// Wrapper for individual policy (allows drift flag etc.)
public sealed class PolicyItemViewModel : INotifyPropertyChanged
{
    public PolicySummary Summary { get; }
    private bool _isDrifted;
    public bool IsDrifted { get => _isDrifted; set { if (_isDrifted != value) { _isDrifted = value; OnPropertyChanged(); } } }

    public PolicyItemViewModel(PolicySummary summary) => Summary = summary;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// Group of policies sourced from one ADMX file
public sealed class PolicyFileGroup : INotifyPropertyChanged
{
    public string FileName { get; }
    public string FullPath { get; }
    public ObservableCollection<PolicyItemViewModel> Policies { get; } = [];

    private bool _isExpanded;
    public bool IsExpanded { get => _isExpanded; set { if (_isExpanded != value) { _isExpanded = value; OnPropertyChanged(); } } }

    private bool _hasDrift;
    public bool HasDrift { get => _hasDrift; set { if (_hasDrift != value) { _hasDrift = value; OnPropertyChanged(); } } }

    private int _driftCount;
    public int DriftCount { get => _driftCount; set { if (_driftCount != value) { _driftCount = value; OnPropertyChanged(); } } }

    public PolicyFileGroup(string fileName, string fullPath)
    {
        FileName = fileName;
        FullPath = fullPath;
    }

    public override string ToString() => FileName;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
