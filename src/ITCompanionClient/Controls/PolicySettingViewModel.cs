// Project Name: ITCompanionClient
// File Name: PolicySettingViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;

namespace ITCompanionClient.Controls;

public sealed class PolicySettingViewModel : INotifyPropertyChanged
{
    private readonly PolicyElement _element;
    private bool _enabled;
    private string? _value;

    public PolicySettingViewModel(AdminPolicy policy, PolicyElement element)
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

    public PolicyKey PolicyKey { get; }
    public ElementId ElementId { get; }
    public string ElementKind { get; }

    /// <summary>Identifier exposed for binding.</summary>
    public string PartId => ElementId.Value;

    /// <summary>Friendly value type descriptor.</summary>
    public string ValueType => ElementKind switch
    {
        nameof(BooleanElement) => "Boolean",
        nameof(DecimalElement) => "Numeric",
        nameof(TextElement) => "Text",
        nameof(MultiTextElement) => "Text",
        nameof(EnumElement) => "Enum",
        _ => "Text"
    };

    /// <summary>Enumeration choices when element is an enum.</summary>
    public IReadOnlyList<EnumOption> EnumItems { get; private set; } = Array.Empty<EnumOption>();

    public bool Enabled
    {
        get => _enabled;
        set { if (_enabled != value) { _enabled = value; OnPropertyChanged(); } }
    }

    public string? Value
    {
        get => _value;
        set { if (_value != value) { _value = value; OnPropertyChanged(); } }
    }

    public ObservableCollection<string> Errors { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed record EnumOption(string Name, string Value);
