using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Shared;

namespace ClientApp.ViewModels;

public class PolicySettingViewModel : INotifyPropertyChanged
{
    public string PolicyId { get; }
    public string? PartId { get; }
    public PolicyValueType ValueType { get; }

    private bool _enabled;
    public bool Enabled { get => _enabled; set { if (_enabled!=value){ _enabled=value; OnPropertyChanged(); Validate(); } } }

    private string? _value;
    public string? Value { get => _value; set { if (_value!=value){ _value=value; OnPropertyChanged(); Validate(); } } }

    public ObservableCollection<string> Errors { get; } = new();

    public PolicySettingViewModel(PolicySetting setting)
    {
        PolicyId = setting.PolicyId;
        PartId = setting.PartId;
        ValueType = setting.ValueType;
        _enabled = setting.Enabled;
        _value = setting.Value;
    }

    public PolicySetting ToModel() => new(PolicyId, PartId, Enabled, Value, ValueType);

    private void Validate()
    {
        Errors.Clear();
        if (Enabled && ValueType != PolicyValueType.Boolean && string.IsNullOrWhiteSpace(Value))
            Errors.Add("Value required when enabled");
        if (ValueType == PolicyValueType.Numeric && Value is not null && !decimal.TryParse(Value, out _))
            Errors.Add("Invalid number");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name=null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
