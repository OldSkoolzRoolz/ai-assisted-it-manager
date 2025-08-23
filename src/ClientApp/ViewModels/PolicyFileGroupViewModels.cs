// Project Name: ClientApp
// File Name: PolicyFileGroupViewModels.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using KC.ITCompanion.CorePolicyEngine.AdminTemplates;


namespace KC.ITCompanion.ClientApp.ViewModels;


// Wrapper for individual policy (allows drift flag etc.)
public sealed class PolicyItemViewModel : INotifyPropertyChanged
{
    private bool _isDrifted;





    public PolicyItemViewModel(PolicySummary summary)
    {
        this.Summary = summary;
    }





    public PolicySummary Summary { get; }

    public bool IsDrifted
    {
        get => this._isDrifted;
        set
        {
            if (this._isDrifted != value)
            {
                this._isDrifted = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;





    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}



// Group of policies sourced from one ADMX file
public sealed class PolicyFileGroup : INotifyPropertyChanged
{
    private int _driftCount;

    private bool _hasDrift;

    private bool _isExpanded;





    public PolicyFileGroup(string fileName, string fullPath)
    {
        this.FileName = fileName;
        this.FullPath = fullPath;
    }





    public string FileName { get; }
    public string FullPath { get; }
    public ObservableCollection<PolicyItemViewModel> Policies { get; } = [];

    public bool IsExpanded
    {
        get => this._isExpanded;
        set
        {
            if (this._isExpanded != value)
            {
                this._isExpanded = value;
                OnPropertyChanged();
            }
        }
    }

    public bool HasDrift
    {
        get => this._hasDrift;
        set
        {
            if (this._hasDrift != value)
            {
                this._hasDrift = value;
                OnPropertyChanged();
            }
        }
    }

    public int DriftCount
    {
        get => this._driftCount;
        set
        {
            if (this._driftCount != value)
            {
                this._driftCount = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;





    public override string ToString()
    {
        return this.FileName;
    }





    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}