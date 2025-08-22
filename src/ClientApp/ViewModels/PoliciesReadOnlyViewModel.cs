// Project Name: ClientApp
// File Name: PoliciesReadOnlyViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT License. See LICENSE file in the project root for full license information.
// Do not remove file headers


using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using KC.ITCompanion.CorePolicyEngine.AdminTemplates;

using Microsoft.Extensions.Logging;


// for PolicySummary


namespace KC.ITCompanion.ClientApp.ViewModels;


public sealed class PoliciesReadOnlyViewModel : INotifyPropertyChanged
{
    private readonly PolicyEditorViewModel _editorVm;
    private readonly ILogger<PoliciesReadOnlyViewModel> _logger;

    private string? _searchText;





    public PoliciesReadOnlyViewModel(PolicyEditorViewModel editorVm, ILogger<PoliciesReadOnlyViewModel> logger)
    {
        this._editorVm = editorVm;
        this._logger = logger;
        this.RefreshCommand = new RelayCommand(_ => Refresh(), _ => true);
        Refresh();
    }





    public ObservableCollection<PolicySummary> Policies { get; } = [];

    public ICommand RefreshCommand { get; }

    public string? SearchText
    {
        get => this._searchText;
        set
        {
            if (this._searchText != value)
            {
                this._searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;





    private async Task Refresh()
    {
        try
        {
            await this._editorVm.EnsureCatalogLoadedAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Policies read-only refresh failed");
        }
    }





    private void ApplyFilter()
    {
        this.Policies.Clear();
        IEnumerable<PolicySummary> source = this._editorVm.Policies;
        if (!string.IsNullOrWhiteSpace(this.SearchText))
        {
            var q = this.SearchText.ToLowerInvariant();
            source = source.Where(p =>
                (p.DisplayName?.ToLowerInvariant().Contains(q) ?? false) || p.Key.Name.ToLowerInvariant().Contains(q));
        }

        foreach (PolicySummary p in source.Take(500)) this.Policies.Add(p); // cap for UI responsiveness
    }





    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}