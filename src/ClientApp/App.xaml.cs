using System;
using System.Windows;
using ClientApp.ViewModels;
using ClientApp.Views;
using CorePolicyEngine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace ClientApp;

public partial class App : Application
{
    public ServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Services = ConfigureServices();
        var window = new MainWindow
        {
            DataContext = Services.GetService(typeof(PolicyEditorViewModel))
        };
        window.Show();
    }

    private ServiceProvider ConfigureServices()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IAdmxCatalogLoader, AdmxAdmlParser>();
        sc.AddSingleton<PolicyEditorViewModel>();
        return sc.BuildServiceProvider();
    }
}
