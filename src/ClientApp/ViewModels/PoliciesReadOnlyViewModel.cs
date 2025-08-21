// Project Name: ClientApp
// File Name: PoliciesReadOnlyViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates; // for PolicySummary

namespace KC.ITCompanion.ClientApp.ViewModels;

public sealed class PoliciesReadOnlyViewModel : INotifyPropertyChanged
{
    private readonly PolicyEditorViewModel _editorVm;
    private readonly ILogger<PoliciesReadOnlyViewModel> _logger;

    public ObservableCollection<PolicySummary> Policies { get; } = [];

    public ICommand RefreshCommand { get; }

    private string? _searchText;
    public string? SearchText { get => _searchText; set { if (_searchText != value) { _searchText = value; OnPropertyChanged(); ApplyFilter(); } } }

    public PoliciesReadOnlyViewModel(PolicyEditorViewModel editorVm, ILogger<PoliciesReadOnlyViewModel> logger)
    {
        _editorVm = editorVm;
        _logger = logger;
        RefreshCommand = new RelayCommand(_ => Refresh(), _ => true);
        Refresh();
    }

    private void Refresh()
    {
        if (_editorVm.Policies.Count == 0 && _editorVm.Catalog == null)
        {
            // trigger load
            Task.Run(async () => await _editorVm.SearchLocalPoliciesAsync(null, CancellationToken.None));
        }
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Policies.Clear();
        IEnumerable<PolicySummary> source = _editorVm.Policies;
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var q = SearchText.ToLowerInvariant();
            source = source.Where(p => (p.DisplayName?.ToLowerInvariant().Contains(q) ?? false) || p.Key.Name.ToLowerInvariant().Contains(q));
        }
        foreach (var p in source.Take(500)) Policies.Add(p); // cap for UI responsiveness
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
