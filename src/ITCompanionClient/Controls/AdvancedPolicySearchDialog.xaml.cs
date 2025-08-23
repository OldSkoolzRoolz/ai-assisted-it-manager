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

public sealed partial class AdvancedPolicySearchDialog : ContentDialog, INotifyPropertyChanged
{
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
    private string _queryText = string.Empty; public string QueryText { get => _queryText; set { if (_queryText != value) { _queryText = value; OnPropertyChanged(); } } }
    private bool _includeKeys = true; public bool IncludeKeys { get => _includeKeys; set { if (_includeKeys != value) { _includeKeys = value; OnPropertyChanged(); } } }
    private bool _includeDescriptions; public bool IncludeDescriptions { get => _includeDescriptions; set { if (_includeDescriptions != value) { _includeDescriptions = value; OnPropertyChanged(); } } }
    private bool _wholeWord; public bool WholeWord { get => _wholeWord; set { if (_wholeWord != value) { _wholeWord = value; OnPropertyChanged(); } } }
    public event PropertyChangedEventHandler? PropertyChanged; private void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}