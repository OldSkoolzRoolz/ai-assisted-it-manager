// Project Name: ClientShared
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

namespace KC.ITCompanion.ClientShared;

/// <summary>
/// View model representing a single configurable element within a policy.
/// </summary>
public sealed class PolicySettingViewModel : INotifyPropertyChanged
{
    private readonly PolicyElement _element;
    private bool _enabled;
    private string? _value;

    /// <summary>Create a setting view model for a policy element.</summary>
    /// <param name="policy">Parent policy definition.</param>
    /// <param name="element">Specific element.</param>
    /// <exception cref="ArgumentNullException">policy or element null.</exception>
    public PolicySettingViewModel(AdminPolicy policy, PolicyElement element)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(element);
        PolicyKey = policy.Key;
        _element = element;
        ElementId = element.Id;
        ElementKind = element.GetType().Name;
        if (element is EnumElement ee)
            EnumItems = ee.Items.Select(i => new EnumOption(i.Label?.Id.Value ?? i.Name, i.Name)).ToList();
    }

    /// <summary>Policy key owning this element.</summary>
    public PolicyKey PolicyKey { get; }
    /// <summary>Element identifier.</summary>
    public ElementId ElementId { get; }
    /// <summary>Element CLR type name.</summary>
    public string ElementKind { get; }
    /// <summary>ADMX part id (element id value).</summary>
    public string PartId => ElementId.Value;
    /// <summary>Logical value type for UI editors.</summary>
    public string ValueType => ElementKind switch
    { nameof(BooleanElement) => "Boolean", nameof(DecimalElement) => "Numeric", nameof(TextElement) => "Text", nameof(MultiTextElement) => "Text", nameof(EnumElement) => "Enum", _ => "Text" };
    /// <summary>Enumeration options (if Enum element).</summary>
    public IReadOnlyList<EnumOption> EnumItems { get; private set; } = Array.Empty<EnumOption>();
    /// <summary>Whether element is currently enabled (for optional parts).</summary>
    public bool Enabled { get => _enabled; set { if (_enabled != value) { _enabled = value; OnPropertyChanged(); } } }
    /// <summary>Current textual value (raw, prior to deployment conversion).</summary>
    public string? Value { get => _value; set { if (_value != value) { _value = value; OnPropertyChanged(); } } }
    /// <summary>Validation errors for current value.</summary>
    public ObservableCollection<string> Errors { get; } = new();
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>Simple name/value pair for enum element choices.</summary>
public sealed record EnumOption(string Name, string Value);
