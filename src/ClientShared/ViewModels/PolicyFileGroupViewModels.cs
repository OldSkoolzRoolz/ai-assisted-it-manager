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
    /// <summary>Create wrapper for a policy summary.</summary>
    public PolicyItemViewModel(PolicySummary summary) { Summary = summary; }
    /// <summary>Underlying policy summary.</summary>
    public PolicySummary Summary { get; }
    /// <summary>Indicates the rendered state is out of sync with expected state.</summary>
    public bool IsDrifted { get => _isDrifted; set { if (_isDrifted != value) { _isDrifted = value; OnPropertyChanged(); } } }
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>Group of policies sourced from one ADMX file.</summary>
public sealed class PolicyFileGroup : INotifyPropertyChanged
{
    private bool _isExpanded; private bool _hasDrift; private int _driftCount;
    /// <summary>Constructs a group for a specific ADMX file.</summary>
    public PolicyFileGroup(string fileName, string fullPath) { FileName = fileName; FullPath = fullPath; }
    /// <summary>Short file name.</summary>
    public string FileName { get; }
    /// <summary>Full path to the ADMX file.</summary>
    public string FullPath { get; }
    /// <summary>Policies contained in this file.</summary>
    public ObservableCollection<PolicyItemViewModel> Policies { get; } = new();
    /// <summary>Whether the group is expanded in the UI.</summary>
    public bool IsExpanded { get => _isExpanded; set { if (_isExpanded != value) { _isExpanded = value; OnPropertyChanged(); } } }
    /// <summary>True when any policy inside is drifted.</summary>
    public bool HasDrift { get => _hasDrift; set { if (_hasDrift != value) { _hasDrift = value; OnPropertyChanged(); } } }
    /// <summary>Count of drifted policies.</summary>
    public int DriftCount { get => _driftCount; set { if (_driftCount != value) { _driftCount = value; OnPropertyChanged(); } } }
    /// <inheritdoc />
    public override string ToString() => FileName;
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
