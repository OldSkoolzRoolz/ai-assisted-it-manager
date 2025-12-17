// Project Name: ITCompanionClient
// File Name: PolicyDetailDialog.xaml.cs
// Author: Kyle Crowder
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
using KC.ITCompanion.ClientShared;
using KC.ITCompanion.ClientShared.ViewModels; // shared VM types

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

    /// <summary>Create dialog for selected policy.</summary>
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

    /// <summary>Display name of selected policy.</summary>
    public string PolicyDisplayName => _editor.SelectedPolicy?.DisplayName ?? _editor.SelectedPolicy?.Key.Name ?? "(none)";
    /// <summary>Selected policy key.</summary>
    public string PolicyKey => _editor.SelectedPolicy?.Key.Name ?? string.Empty;
    /// <summary>Category path for selected policy.</summary>
    public string CategoryPath => _editor.SelectedPolicy?.CategoryPath ?? string.Empty;
    /// <summary>Explain text / description.</summary>
    public string? Description => _description;
    /// <summary>Current setting view models.</summary>
    public ObservableCollection<PolicySettingViewModel> Settings => _editor.SelectedPolicySettings;

    /// <summary>Indicates edit mode state.</summary>
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
            try { await _audit.PolicyEditedAsync(_editor.SelectedPolicy.Key.Name, c.PartId, c.Value).ConfigureAwait(false); }
            catch (InvalidOperationException) { }
            catch (System.IO.IOException) { }
        }
        try { await _audit.PolicyEditPushedAsync(changes.Count).ConfigureAwait(false); }
        catch (InvalidOperationException) { }
        catch (System.IO.IOException) { }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

/// <summary>Proxy summary retained for potential future binding scenarios (not used in XAML version).</summary>
public sealed class PolicySummaryProxy
{
    /// <summary>Display name.</summary>
    public string? DisplayName { get; set; }
    /// <summary>Category path.</summary>
    public string? CategoryPath { get; set; }
    /// <summary>Policy key struct.</summary>
    public KC.ITCompanion.CorePolicyEngine.AdminTemplates.PolicyKey Key { get; set; }
}
