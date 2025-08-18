// Project Name: ClientApp
// File Name: PolicySettingViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers


using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace ClientApp.ViewModels;


public class PolicySettingViewModel : INotifyPropertyChanged
{
    public string PolicyId { get; }
    public string? PartId { get; }
    public PolicyValueType ValueType { get; }
    public string? EnumId { get; }
    public ObservableCollection<PolicyEnumItem>? EnumItems { get; }

    private bool _enabled;
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled != value)
            {
                _enabled = value;
                OnPropertyChanged();
                Validate();
            }
        }
    }

    private string? _value;
    public string? Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged();
                Validate();
            }
        }
    }

    public ObservableCollection<string> Errors { get; } = [];


    // Existing constructor (fallback / boolean simple cases)
    public PolicySettingViewModel(PolicySetting setting)
    {
        PolicyId = setting.PolicyId;
        PartId = setting.PartId;
        ValueType = setting.ValueType;
        _enabled = setting.Enabled;
        _value = setting.Value;
    }

    // New constructor from part definition + catalog (for enum population)
    public PolicySettingViewModel(string policyId, PolicyPartDefinition part, AdmxCatalog catalog)
    {
        PolicyId = policyId;
        PartId = part.Id;
        ValueType = part.ValueType;
        EnumId = part.EnumId;
        _enabled = false;
        _value = null;
        if (part.ValueType == PolicyValueType.Enum && part.EnumId is not null)
        {
            PolicyEnum? match = catalog.Enums.FirstOrDefault(e => e.Id == part.EnumId);
            if (match != null)
            {
                EnumItems = new ObservableCollection<PolicyEnumItem>(match.Items);
            }
        }
    }

    public PolicySetting ToModel()
    {
        return new(PolicyId, PartId, Enabled, Value, ValueType);
    }

    private void Validate()
    {
        Errors.Clear();
        if (Enabled && ValueType != PolicyValueType.Boolean && string.IsNullOrWhiteSpace(Value))
        {
            Errors.Add("Value required when enabled");
        }

        if (ValueType == PolicyValueType.Numeric && Value is not null && !decimal.TryParse(Value, out _))
        {
            Errors.Add("Invalid number");
        }

        if (ValueType == PolicyValueType.Enum && Enabled && string.IsNullOrWhiteSpace(Value))
        {
            Errors.Add("Select an option");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}