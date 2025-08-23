using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Linq;
using KC.ITCompanion.ClientShared;
using KC.ITCompanion.CorePolicyEngine.AdminTemplates;
using System.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace ITCompanionClient.Controls;
public sealed partial class PoliciesPage : UserControl
{
    public PolicyEditorViewModel ViewModel { get; }
    private InfoBar? _actionInfoBar;
    private Grid? _groupedPanel;
    private ScrollViewer? _flatScroll;

    public PoliciesPage()
    {
        InitializeComponent();
        _actionInfoBar = (InfoBar)FindName("ActionInfoBar");
        _groupedPanel = (Grid)FindName("GroupedPanel");
        _flatScroll = (ScrollViewer)FindName("FlatScroll");
        ViewModel = (PolicyEditorViewModel)App.Services.GetService(typeof(PolicyEditorViewModel))!;
        ViewModel.ColumnVisibility.PropertyChanged += OnColumnVisibilityChanged;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        LoadingRing.IsActive = true;
        await ViewModel.EnsureCatalogLoadedAsync();
        LoadingRing.IsActive = false;
        RefreshEmptyState();
        UpdateColumnVisibility();
    }

    private void OnSearchChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    { ViewModel.SearchText = sender.Text; RefreshEmptyState(); }

    private async void OnReload(object sender, RoutedEventArgs e)
    {
        LoadingRing.IsActive = true;
        ViewModel.ApplySearchFilter(null);
        await ViewModel.EnsureCatalogLoadedAsync();
        LoadingRing.IsActive = false;
        ShowInfo("Catalog reloaded.");
        RefreshEmptyState();
        UpdateColumnVisibility();
    }

    private async void OnAdvancedSearch(object sender, RoutedEventArgs e)
    {
        var dlg = new AdvancedPolicySearchDialog(ViewModel.SearchText) { XamlRoot = this.XamlRoot };
        var result = await dlg.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.SearchText = dlg.QueryText;
            RefreshEmptyState();
        }
    }
    private void OnColumns(object sender, RoutedEventArgs e) { }

    private void OnGroupingChanged(object sender, RoutedEventArgs e)
    {
        bool grouped = GroupToggle.IsChecked == true;
        if (_groupedPanel != null) _groupedPanel.Visibility = grouped ? Visibility.Visible : Visibility.Collapsed;
        if (_flatScroll != null) _flatScroll.Visibility = grouped ? Visibility.Collapsed : Visibility.Visible;
        UpdateColumnVisibility();
    }

    private async void OnItemClick(object sender, ItemClickEventArgs e)
    { if (e.ClickedItem is PolicySummary summary) await OpenPolicyAsync(summary); }

    private async void OnOpenPolicy(object sender, RoutedEventArgs e)
    { if ((sender as FrameworkElement)?.Tag is PolicySummary summary) await OpenPolicyAsync(summary); }

    private async System.Threading.Tasks.Task OpenPolicyAsync(PolicySummary summary)
    {
        ViewModel.SelectedPolicy = summary;
        if (ViewModel.Catalog == null) return;
        var policy = ViewModel.Catalog.AdmxDocuments.SelectMany(d => d.Policies).FirstOrDefault(p => p.Key.Name == summary.Key.Name);
        string? explain = null;
        if (policy?.ExplainText != null)
        {
            foreach (var adml in ViewModel.Catalog.AdmlDocuments)
                if (adml.StringTable.TryGetValue(policy.ExplainText.Id, out var text)) { explain = text; break; }
        }
        var dialog = new PolicyDetailDialog(ViewModel, explain) { XamlRoot = this.XamlRoot };
        await dialog.ShowAsync();
    }

    private void OnColumnToggle(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem m && m.Tag is string key)
        {
            var cv = ViewModel.ColumnVisibility;
            switch (key)
            { case "Name": cv.Name = !cv.Name; break; case "Category": cv.Category = !cv.Category; break; }
            UpdateColumnVisibility();
        }
    }

    private void OnColumnVisibilityChanged(object? sender, PropertyChangedEventArgs e)
        => UpdateColumnVisibility();

    private void UpdateColumnVisibility()
    {
        bool showName = ViewModel.ColumnVisibility.Name;
        bool showCategory = ViewModel.ColumnVisibility.Category;
        // Flat list items
        foreach (var child in PoliciesRepeater?.GetDescendants().OfType<FrameworkElement>() ?? Array.Empty<FrameworkElement>())
        {
            if (child.Name == "NamePanel") child.Visibility = showName ? Visibility.Visible : Visibility.Collapsed;
            else if (child.Name == "CategoryPanel") child.Visibility = showCategory ? Visibility.Visible : Visibility.Collapsed;
        }
        // Grouped items
        if (_groupedPanel?.Visibility == Visibility.Visible)
        {
            foreach (var fe in _groupedPanel.GetDescendants().OfType<FrameworkElement>())
            {
                if (fe.Name == "NamePanel") fe.Visibility = showName ? Visibility.Visible : Visibility.Collapsed;
                else if (fe.Name == "CategoryPanel") fe.Visibility = showCategory ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    private void RefreshEmptyState()
    { EmptyStatePanel.Visibility = ViewModel.FilteredPolicies.Count == 0 ? Visibility.Visible : Visibility.Collapsed; }

    private void ShowInfo(string message)
    { if (_actionInfoBar != null) { _actionInfoBar.Message = message; _actionInfoBar.IsOpen = true; } }

    private void OnCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryList.SelectedItem is CategoryListItem item)
            ViewModel.SetCategoryFilter(item.Id);
        else
            ViewModel.SetCategoryFilter(null!); // ViewModel not null; SetCategoryFilter accepts nullable
        RefreshEmptyState();
        UpdateColumnVisibility();
    }

    // Category filtering integration pending: category list currently bound to CategoryPolicyRows for display only.
}

internal static class VisualTreeHelpers
{
    public static System.Collections.Generic.IEnumerable<DependencyObject> GetChildren(this DependencyObject parent)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++) yield return VisualTreeHelper.GetChild(parent, i);
    }
    public static System.Collections.Generic.IEnumerable<DependencyObject> GetDescendants(this DependencyObject root)
    {
        foreach (var child in root.GetChildren())
        {
            yield return child;
            foreach (var d in child.GetDescendants()) yield return d;
        }
    }
}
