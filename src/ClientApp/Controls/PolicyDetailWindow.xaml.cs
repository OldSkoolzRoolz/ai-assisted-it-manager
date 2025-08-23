// Project Name: ClientApp
// File Name: PolicyDetailWindow.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using KC.ITCompanion.ClientApp.ViewModels;
using KC.ITCompanion.CorePolicyEngine.Storage;

namespace KC.ITCompanion.ClientApp.Controls;

/// <summary>
/// Window displaying detailed policy information with sandbox editing; edits require explicit push.
/// </summary>
public partial class PolicyDetailWindow : Window, INotifyPropertyChanged
{
    private bool _isReadOnlyMode = true;
    private readonly PolicyEditorViewModel _editor;
    private readonly IAuditWriter? _audit;
    private readonly Dictionary<string, string?> _originalValues = new();

    /// <summary>
    /// Initializes detail window with selected policy row and editor context.
    /// </summary>
    public PolicyDetailWindow(PolicyGridRow row, PolicyEditorViewModel editor)
    {
        InitializeComponent();
        _editor = editor;
        _audit = (Application.Current as App)?.Services.GetService(typeof(IAuditWriter)) as IAuditWriter;
        SelectedRow = row;
        Settings = new ObservableCollection<PolicySettingViewModel>(_editor.SelectedPolicySettings);
        foreach (var s in Settings) _originalValues[s.PartId] = s.Value;
        DataContext = this;
    }

    /// <summary>Gets or sets whether the form is in read-only mode (true) or edit mode (false).</summary>
    public bool IsReadOnlyMode
    {
        get => _isReadOnlyMode;
        set
        {
            if (_isReadOnlyMode != value)
            {
                if (_isReadOnlyMode && !value) // switching to edit
                {
                    var result = MessageBox.Show(
                        "You are entering edit mode. Changes are sandboxed and will NOT take effect until you push them. Continue?",
                        "Edit Mode", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result != MessageBoxResult.Yes) return;
                }
                _isReadOnlyMode = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>The row for which details are displayed.</summary>
    public PolicyGridRow SelectedRow { get; }

    /// <summary>Collection of settings (sandbox snapshot).</summary>
    public ObservableCollection<PolicySettingViewModel> Settings { get; }

    /// <summary>Pushes edits: audits each changed element then a summary event.</summary>
    private async void OnPushEdits(object sender, RoutedEventArgs e)
    {
        if (_audit == null) return;
        var changes = Settings.Where(s => _originalValues.TryGetValue(s.PartId, out var ov) ? ov != s.Value : true).ToList();
        foreach (var c in changes)
        {
            try { await _audit.PolicyEditedAsync(SelectedRow.Key, c.PartId, c.Value); } catch { }
        }
        try { await _audit.PolicyEditPushedAsync(changes.Count); } catch { }
        MessageBox.Show(changes.Count + " change(s) recorded in audit trail. Deployment engine not yet implemented.",
            "Sandbox Push", MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
