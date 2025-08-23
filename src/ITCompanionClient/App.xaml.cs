using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using KC.ITCompanion.CorePolicyEngine.Parsing;
using KC.ITCompanion.CorePolicyEngine.Storage;
using KC.ITCompanion.CorePolicyEngine.Storage.Sql;

namespace ITCompanionClient;
/// <summary>
/// WinUI application bootstrapper configuring DI and showing the main window.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    public static ServiceProvider Services { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Add logging hookup
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Services = ConfigureServices();
        _window = new MainWindow();
        _window.Activate();
    }

    private static ServiceProvider ConfigureServices()
    {
        var sc = new ServiceCollection();
        sc.AddLogging(b =>
        {
            b.AddDebug();
            b.SetMinimumLevel(LogLevel.Information);
        });
        sc.AddSingleton<IAdminTemplateLoader, AdmxAdmlParser>();
        sc.AddSingleton<IAuditStore, AuditStore>();
        sc.AddSingleton<IAuditWriter, AuditWriter>();
        sc.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        return sc.BuildServiceProvider();
    }
}
