using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using KC.ITCompanion.CorePolicyEngine.Parsing;
using KC.ITCompanion.CorePolicyEngine.Storage;
using KC.ITCompanion.CorePolicyEngine.Storage.Sql;
using KC.ITCompanion.ClientShared;
using KC.ITCompanion.ClientShared.Localization;

namespace ITCompanionClient;
/// <summary>
/// WinUI application bootstrapper configuring dependency injection container and launching the main window.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    /// <summary>Main application window instance.</summary>
    public static Window MainWindow { get; private set; } = null!;
    /// <summary>Root service provider for the client process lifecycle.</summary>
    public static ServiceProvider Services { get; private set; } = null!;

    /// <summary>Constructs the application object and attaches global exception handlers.</summary>
    public App()
    {
        InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    /// <summary>Global unhandled exception hook (placeholder for logging integration).</summary>
    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Add logging hookup
    }

    /// <summary>Called by framework when application is launched.</summary>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Services = ConfigureServices();
        _window = new MainWindow();
        MainWindow = _window;
        _window.Activate();
    }

    /// <summary>Configures the DI container with core services, repositories, and view models.</summary>
    private static ServiceProvider ConfigureServices()
    {
        var sc = new ServiceCollection();
        sc.AddLogging(b =>
        {
            b.AddDebug();
            b.SetMinimumLevel(LogLevel.Information);
        });
        // Core services
        sc.AddSingleton<IAdminTemplateLoader, AdmxAdmlParser>();
        sc.AddSingleton<IAuditStore, AuditStore>();
        sc.AddSingleton<IAuditWriter, AuditWriter>();
        sc.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        sc.AddSingleton<ILogEventQueryRepository, LogEventQueryRepository>();
        sc.AddSingleton<IPolicyDefinitionRepository, PolicyDefinitionRepository>();
        sc.AddSingleton<IPolicyGroupRepository, PolicyGroupRepository>();

        // UI adapters
        sc.AddSingleton<IUiDispatcher, WinUiDispatcher>();
        sc.AddSingleton<IMessagePromptService, WinUiPromptService>();
        sc.AddSingleton<IThemeService, ThemeServiceWinUi>();
        sc.AddSingleton<ILocalizationService, LocalizationService>();

        // ViewModels
        sc.AddTransient<PolicyEditorViewModel>();
        return sc.BuildServiceProvider();
    }
}
