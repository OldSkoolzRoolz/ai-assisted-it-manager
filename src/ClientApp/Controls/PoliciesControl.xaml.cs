// Project Name: ClientApp
// File Name: PoliciesControl.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KC.ITCompanion.ClientApp.ViewModels;

namespace KC.ITCompanion.ClientApp.Controls;

/// <summary>
/// Policies tab control exposing hierarchical category navigation and policy grid.
/// </summary>
public partial class PoliciesControl : UserControl
{
    private PolicyEditorViewModel? _vm;

    /// <summary>
    /// Initializes the control and resolves the PolicyEditorViewModel.
    /// </summary>
    public PoliciesControl()
    {
        InitializeComponent();
        if (Application.Current is App app)
        {
            if (app.Services.GetService(typeof(PolicyEditorViewModel)) is PolicyEditorViewModel vm)
            {
                _vm = vm;
                DataContext = vm;
                vm.PolicyDetailRequested += OnPolicyDetailRequested;
                Loaded += async (_, _) =>
                {
                    if (vm.Catalog == null)
                    {
                        await vm.EnsureCatalogLoadedAsync();
                        vm.ApplySearchFilter(null);
                    }
                };
            }
        }
    }

    /// <summary>
    /// Handles a double click on the policy grid row to open details.
    /// </summary>
    private void OnPolicyGridDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGrid dg && dg.SelectedItem is PolicyGridRow row && _vm != null)
        {
            _vm.SelectedPolicy = row.Summary;
            OpenPolicyDetailWindow(row);
        }
    }

    /// <summary>
    /// Invoked when the view model raises a detail request event.
    /// </summary>
    private void OnPolicyDetailRequested(object? sender, PolicyGridRow e)
    {
        OpenPolicyDetailWindow(e);
    }

    /// <summary>
    /// Creates and shows the policy detail window.
    /// </summary>
    private void OpenPolicyDetailWindow(PolicyGridRow row)
    {
        var wnd = new PolicyDetailWindow(row, _vm!);
        wnd.Owner = Window.GetWindow(this);
        wnd.ShowDialog();
    }

    /// <summary>
    /// Opens the search dialog for advanced filtering.
    /// </summary>
    private void OpenSearchDialog()
    {
        if (_vm == null) return;
        var vm = new PolicySearchViewModel(_vm);
        var dlg = new SearchDialog(vm) { Owner = Window.GetWindow(this) };
        dlg.ShowDialog();
    }
}