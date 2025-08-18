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
            await ViewModel.SearchLocalPoliciesAsync(null, CancellationToken.None).ConfigureAwait(false); // initial load
        }
    }





    private void CategoryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is CategoryTreeItem item)
        {
            if (item.IsPolicy && item.Policy != null)
            {
                ViewModel.SelectedCategory = null; // clear category selection
                ViewModel.SelectedPolicy = null;   // force change notification even if same
                ViewModel.SelectedPolicy = item.Policy;
            }
            else if (!item.IsPolicy && item.Category != null)
            {
                ViewModel.SelectedCategory = item.Category;
            }
        }
    }
}