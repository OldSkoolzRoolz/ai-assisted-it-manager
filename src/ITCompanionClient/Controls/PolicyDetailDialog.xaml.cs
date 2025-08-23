// Project Name: ITCompanionClient
// File Name: PolicyDetailDialog.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using KC.ITCompanion.CorePolicyEngine.Storage;
using KC.ITCompanion.ClientShared; // shared VM types

namespace ITCompanionClient.Controls;

/// <summary>
/// XAML backed dialog displaying details of the currently selected policy using shared PolicyEditorViewModel data.
/// </summary>
public sealed partial class PolicyDetailDialog : ContentDialog, INotifyPropertyChanged
{
    private readonly IAuditWriter? _audit;
    private readonly PolicyEditorViewModel _editor;
    private bool _isEditMode;
    private readonly Dictionary<string, string?> _originalValues = new();
    private readonly string? _description;

    public PolicyDetailDialog(PolicyEditorViewModel editor, string? description)
    {
        _editor = editor;
        _audit = App.Services.GetService(typeof(IAuditWriter)) as IAuditWriter;
        _description = description;
        foreach (var vm in _editor.SelectedPolicySettings)
            _originalValues[vm.PartId] = vm.Value;
        InitializeComponent();
        DataContext = this;
        PrimaryButtonClick += OnPushClick;
    }

    public string PolicyDisplayName => _editor.SelectedPolicy?.DisplayName ?? _editor.SelectedPolicy?.Key.Name ?? "(none)";
    public string PolicyKey => _editor.SelectedPolicy?.Key.Name ?? string.Empty;
    public string CategoryPath => _editor.SelectedPolicy?.CategoryPath ?? string.Empty;
    public string? Description => _description;
    public ObservableCollection<PolicySettingViewModel> Settings => _editor.SelectedPolicySettings;

    public bool IsEditMode
    {
        get => _isEditMode;
        set { if (_isEditMode != value) { _isEditMode = value; OnPropertyChanged(); } }
    }

    private async void OnPushClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (_audit == null || _editor.SelectedPolicy == null) return;
        var changes = Settings.Where(s => _originalValues.TryGetValue(s.PartId, out var ov) ? ov != s.Value : true).ToList();
        foreach (var c in changes)
        {
            try { await _audit.PolicyEditedAsync(_editor.SelectedPolicy.Key.Name, c.PartId, c.Value); } catch { }
        }
        try { await _audit.PolicyEditPushedAsync(changes.Count); } catch { }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

/// <summary>Proxy summary retained for potential future binding scenarios (not used in XAML version).</summary>
public sealed class PolicySummaryProxy
{
    public string? DisplayName { get; set; }
    public string? CategoryPath { get; set; }
    public KC.ITCompanion.CorePolicyEngine.AdminTemplates.PolicyKey Key { get; set; }
}
