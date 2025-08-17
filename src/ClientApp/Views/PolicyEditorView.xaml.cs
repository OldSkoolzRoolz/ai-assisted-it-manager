using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClientApp.ViewModels;

namespace ClientApp.Views;

public partial class PolicyEditorView : UserControl
{
    public PolicyEditorViewModel ViewModel { get; }

    public PolicyEditorView()
    {
        InitializeComponent();
        ViewModel = ((App)Application.Current).Services.GetService(typeof(PolicyEditorViewModel)) as PolicyEditorViewModel ?? throw new InvalidOperationException();
        Loaded += OnLoaded;
        DataContext = ViewModel;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var policyDef = Path.Combine(winDir, "PolicyDefinitions");
        if (Directory.Exists(policyDef))
        {
            var admxFiles = Directory.GetFiles(policyDef, "*.admx").Take(5).ToList();
            await ViewModel.LoadCatalogAsync(admxFiles, "en-US", CancellationToken.None);
        }
    }

    private void CategoryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        ViewModel.SelectedPolicy = e.NewValue as Shared.AdmxPolicy;
    }
}
