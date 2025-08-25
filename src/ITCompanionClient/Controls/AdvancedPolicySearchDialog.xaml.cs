// Project Name: ITCompanionClient
// File Name: AdvancedPolicySearchDialog.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ITCompanionClient.Controls;

/// <summary>Dialog providing advanced search options for policies.</summary>
public sealed partial class AdvancedPolicySearchDialog : ContentDialog, INotifyPropertyChanged
{
    /// <summary>Create dialog with initial query text.</summary>
    /// <param name="initial">Initial search text.</param>
    public AdvancedPolicySearchDialog(string? initial)
    {
        QueryText = initial ?? string.Empty;
        Title = "Advanced Policy Search";
        PrimaryButtonText = "Search";
        SecondaryButtonText = "Cancel";
        DefaultButton = ContentDialogButton.Primary;
        BuildContent();
        DataContext = this;
    }
    /// <summary>Builds dynamic dialog content.</summary>
    private void BuildContent()
    {
        var stack = new StackPanel { Spacing = 12, Width = 400 };
        var query = new TextBox { PlaceholderText = "Name / key / category", Text = QueryText };
        query.TextChanged += (_, __) => QueryText = query.Text;
        stack.Children.Add(query);
        var keys = new CheckBox { Content = "Include keys", IsChecked = IncludeKeys }; keys.Checked += (_, __) => IncludeKeys = true; keys.Unchecked += (_, __) => IncludeKeys = false; stack.Children.Add(keys);
        var desc = new CheckBox { Content = "Include descriptions", IsChecked = IncludeDescriptions }; desc.Checked += (_, __) => IncludeDescriptions = true; desc.Unchecked += (_, __) => IncludeDescriptions = false; stack.Children.Add(desc);
        var whole = new CheckBox { Content = "Match whole word", IsChecked = WholeWord }; whole.Checked += (_, __) => WholeWord = true; whole.Unchecked += (_, __) => WholeWord = false; stack.Children.Add(whole);
        Content = stack;
    }
    private string _queryText = string.Empty; /// <summary>Search query text.</summary>
    public string QueryText { get => _queryText; set { if (_queryText != value) { _queryText = value; OnPropertyChanged(); } } }
    private bool _includeKeys = true; /// <summary>Include policy keys in search.</summary>
    public bool IncludeKeys { get => _includeKeys; set { if (_includeKeys != value) { _includeKeys = value; OnPropertyChanged(); } } }
    private bool _includeDescriptions; /// <summary>Include description text in search.</summary>
    public bool IncludeDescriptions { get => _includeDescriptions; set { if (_includeDescriptions != value) { _includeDescriptions = value; OnPropertyChanged(); } } }
    private bool _wholeWord; /// <summary>Enable whole-word matching.</summary>
    public bool WholeWord { get => _wholeWord; set { if (_wholeWord != value) { _wholeWord = value; OnPropertyChanged(); } } }
    /// <summary>Property changed notification event.</summary>
    public event PropertyChangedEventHandler? PropertyChanged; /// <summary>Raise property changed event.</summary>
    private void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}