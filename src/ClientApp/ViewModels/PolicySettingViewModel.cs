// Project Name: ClientApp
// File Name: PolicySettingViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;

namespace KC.ITCompanion.ClientApp.ViewModels;

// Placeholder per-policy element editing (will be expanded for element-specific controls)
public class PolicySettingViewModel : INotifyPropertyChanged
{
    private readonly PolicyElement _element; // retain original element for future expansion

    public PolicyKey PolicyKey { get; }
    public ElementId ElementId { get; }
    public string ElementKind { get; }

    // Bindable aliases expected by XAML template
    public string PartId => ElementId.Value;
    public string ValueType => ElementKind switch
    {
        nameof(BooleanElement) => "Boolean",
        nameof(DecimalElement) => "Numeric",
        nameof(TextElement) => "Text",
        nameof(MultiTextElement) => "Text",
        nameof(EnumElement) => "Enum",
        _ => "Text"
    };

    // Enum options (simple name/value pair) for EnumElement
    public IReadOnlyList<EnumOption> EnumItems { get; private set; } = Array.Empty<EnumOption>();

    private bool _enabled;
    public bool Enabled { get => _enabled; set { if (_enabled != value) { _enabled = value; OnPropertyChanged(); } } }

    private string? _value;
    public string? Value { get => _value; set { if (_value != value) { _value = value; OnPropertyChanged(); } } }

    public ObservableCollection<string> Errors { get; } = [];

    public PolicySettingViewModel(Policy policy, PolicyElement element)
    {
        PolicyKey = policy.Key;
        _element = element;
        ElementId = element.Id;
        ElementKind = element.GetType().Name;

        if (element is EnumElement ee)
        {
            EnumItems = ee.Items.Select(i => new EnumOption(i.Label?.Id.Value ?? i.Name, i.Name)).ToList();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed record EnumOption(string Name, string Value);