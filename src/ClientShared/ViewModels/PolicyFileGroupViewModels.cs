// Project Name: ClientShared
// File Name: PolicyFileGroupViewModels.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;

namespace KC.ITCompanion.ClientShared;

/// <summary>Wrapper for individual policy enabling drift flag etc.</summary>
public sealed class PolicyItemViewModel : INotifyPropertyChanged
{
    private bool _isDrifted;
    public PolicyItemViewModel(PolicySummary summary) { Summary = summary; }
    public PolicySummary Summary { get; }
    public bool IsDrifted { get => _isDrifted; set { if (_isDrifted != value) { _isDrifted = value; OnPropertyChanged(); } } }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>Group of policies sourced from one ADMX file.</summary>
public sealed class PolicyFileGroup : INotifyPropertyChanged
{
    private bool _isExpanded; private bool _hasDrift; private int _driftCount;
    public PolicyFileGroup(string fileName, string fullPath) { FileName = fileName; FullPath = fullPath; }
    public string FileName { get; } public string FullPath { get; }
    public ObservableCollection<PolicyItemViewModel> Policies { get; } = new();
    public bool IsExpanded { get => _isExpanded; set { if (_isExpanded != value) { _isExpanded = value; OnPropertyChanged(); } } }
    public bool HasDrift { get => _hasDrift; set { if (_hasDrift != value) { _hasDrift = value; OnPropertyChanged(); } } }
    public int DriftCount { get => _driftCount; set { if (_driftCount != value) { _driftCount = value; OnPropertyChanged(); } } }
    public override string ToString() => FileName;
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
