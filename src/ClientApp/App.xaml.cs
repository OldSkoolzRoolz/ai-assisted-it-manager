// Project Name: ClientApp
// File Name: App.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers


using CorePolicyEngine.Parsing;

using Microsoft.Extensions.DependencyInjection;


namespace ClientApp;


public partial class App : Application
{
    public ServiceProvider Services { get; private set; } = null!;





    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Services = ConfigureServices();
        MainWindow window = new()
        {
            DataContext = Services.GetService(typeof(PolicyEditorViewModel))
        };
        window.Show();
    }





    private ServiceProvider ConfigureServices()
    {
        ServiceCollection sc = new();
        sc.AddSingleton<IAdmxCatalogLoader, AdmxAdmlParser>();
        sc.AddSingleton<PolicyEditorViewModel>();
        return sc.BuildServiceProvider();
    }
}