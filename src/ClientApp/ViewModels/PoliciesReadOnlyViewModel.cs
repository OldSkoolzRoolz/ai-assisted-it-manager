// Project Name: ClientApp
// File Name: PoliciesReadOnlyViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;
using Microsoft.Extensions.Logging;

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

    private async void Refresh()
    {
        try
        {
            await this._editorVm.EnsureCatalogLoadedAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            PoliciesReadOnlyViewModelLogs.RefreshFailed(this._logger, ex); // use LoggerMessage delegate (CA1848)
        }
    }

    private void ApplyFilter()
    {
        this.Policies.Clear();
        IEnumerable<PolicySummary> source = this._editorVm.Policies;
        if (!string.IsNullOrWhiteSpace(this.SearchText))
        {
            var q = this.SearchText.Trim();
            source = source.Where(p =>
                (!string.IsNullOrEmpty(p.DisplayName) && p.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                p.Key.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        foreach (PolicySummary p in source.Take(500)) this.Policies.Add(p);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}