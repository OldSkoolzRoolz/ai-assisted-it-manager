using ClientApp.Views;

using Microsoft.UI.Xaml;

namespace ClientApp;

// Change base class from MediaTypeNames.Application to Microsoft.UI.Xaml.Application
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    // Use Microsoft.UI.Xaml.LaunchActivatedEventArgs
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        new MainWindow().Activate();
    }
}