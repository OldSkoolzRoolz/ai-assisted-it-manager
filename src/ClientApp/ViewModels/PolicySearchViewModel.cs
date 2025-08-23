// Project Name: ClientApp
// File Name: PolicySearchViewModel.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace KC.ITCompanion.ClientApp.ViewModels;

/// <summary>
/// View model backing the search dialog allowing scoped filtering.
/// </summary>
public sealed class PolicySearchViewModel : INotifyPropertyChanged
{
    private readonly PolicyEditorViewModel _editor;
    private string? _searchText;
    private bool _searchInKey = true;
    private bool _searchInName = true;
    private bool _searchInCategory = true;

    /// <summary>Creates a new instance bound to the main editor view model.</summary>
    public PolicySearchViewModel(PolicyEditorViewModel editor)
    {
        _editor = editor;
        ExecuteSearchCommand = new RelayCommand(_ => Execute(), _ => CanSearch);
    }

    public string? SearchText { get => _searchText; set { if (_searchText != value) { _searchText = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSearch)); } } }
    public bool SearchInKey { get => _searchInKey; set { if (_searchInKey != value) { _searchInKey = value; OnPropertyChanged(); } } }
    public bool SearchInName { get => _searchInName; set { if (_searchInName != value) { _searchInName = value; OnPropertyChanged(); } } }
    public bool SearchInCategory { get => _searchInCategory; set { if (_searchInCategory != value) { _searchInCategory = value; OnPropertyChanged(); } } }

    /// <summary>Command invoked by the dialog to perform filtering.</summary>
    public ICommand ExecuteSearchCommand { get; }

    /// <summary>Gets a value indicating whether searching is allowed.</summary>
    public bool CanSearch => !string.IsNullOrWhiteSpace(SearchText);

    private void Execute()
    {
        var query = SearchText?.Trim();
        if (string.IsNullOrWhiteSpace(query)) return;
        // Build filtered projection depending on flags (reuse existing ApplySearchFilter for now)
        _editor.ApplySearchFilter(query); // Future: apply flag-specific logic using the scope flags
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}