using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ITCompanionClient.Controls;

namespace ITCompanionClient;
/// <summary>
/// Main window hosting NavigationView for WinUI port.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        RootNav.SelectedItem = RootNav.MenuItems[0];
        ContentFrame.Content = new StatusView();
    }

    private void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem nvi || nvi.Tag is not string tag) return;
        switch (tag)
        {
            case "status":
                ContentFrame.Content = new StatusView();
                break;
            case "policies":
                ContentFrame.Content = new PoliciesPage();
                break;
            default:
                ContentFrame.Content = new TextBlock { Text = $"{tag} view pending port", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                break;
        }
    }
}
