// Project Name: ClientApp
// File Name: PolicyEditorView.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace ClientApp.Views;

public partial class PolicyEditorView : UserControl
{
    public PolicyEditorView()
    {
        InitializeComponent();
        ViewModel =
            ((App)Application.Current).Services.GetService(typeof(PolicyEditorViewModel)) as PolicyEditorViewModel ??
            throw new InvalidOperationException();
        Loaded += OnLoaded;
        DataContext = ViewModel;
    }

    public PolicyEditorViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.Policies.Any())
        {
            await ViewModel.SearchLocalPoliciesAsync(null, CancellationToken.None).ConfigureAwait(false);
        }
    }

    private void CategoryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is CategoryTreeItem item)
        {
            if (item.IsPolicy && item.Policy is not null)
            {
                ViewModel.SelectedCategoryNode = null;
                ViewModel.SelectedPolicy = item.Policy;
            }
            else if (!item.IsPolicy)
            {
                ViewModel.SelectedCategoryNode = item;
            }
        }
    }

    private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is TreeViewItem tvi && tvi.DataContext is CategoryTreeItem node)
        {
            ViewModel.EnsureCategoryChildren(node);
            // Force refresh of Items to realize newly loaded children
            tvi.Items.Refresh();
        }
    }

    private void OnOpenDevDiag(object sender, RoutedEventArgs e)
    {
        var devVm = ((App)Application.Current).Services.GetService(typeof(DevDiagnosticsViewModel)) as DevDiagnosticsViewModel;
        if (devVm != null)
        {
            var win = new DevDiagnosticsWindow { DataContext = devVm };
            win.Show();
        }
    }

    private void OnOpenLogs(object sender, RoutedEventArgs e)
    {
        var logVm = ((App)Application.Current).Services.GetService(typeof(LogViewerViewModel)) as LogViewerViewModel;
        if (logVm != null)
        {
            // Simple on-demand window container
            var win = new Window
            {
                Title = "Logs",
                Width = 1000,
                Height = 600,
                Content = new Views.LogViewerView { DataContext = logVm }
            };
            win.Show();
        }
    }
}