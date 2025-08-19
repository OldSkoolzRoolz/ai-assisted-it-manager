// Project Name: ClientApp
// File Name: App.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers


using CorePolicyEngine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console; // explicit for AddConsole
using Microsoft.Extensions.Logging.Debug;   // explicit for AddDebug
using ClientApp.Logging;
using ClientApp.ViewModels;
using Security;
using CorePolicyEngine.Storage;


namespace ClientApp;


public partial class App : Application
{
    public ServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        this.Services = ConfigureServices();

        var startupLogger = this.Services.GetRequiredService<ILogger<App>>();
        startupLogger.LogInformation("ClientApp starting up at {StartTimeUtc}", DateTime.UtcNow);

        // Initialize audit
        var auditStore = Services.GetService<IAuditStore>();
        if (auditStore != null)
            await auditStore.InitializeAsync(CancellationToken.None);

        // Load behavior policy snapshot to get dynamic allowed groups
        var store = Services.GetService<IBehaviorPolicyStore>();
        string[] dynamicGroups = ["Administrators"];
        if (store != null)
        {
            await store.InitializeAsync(CancellationToken.None);
            var snap = await store.GetSnapshotAsync(CancellationToken.None);
            if (!string.IsNullOrWhiteSpace(snap.Effective.AllowedGroupsCsv))
            {
                dynamicGroups = snap.Effective.AllowedGroupsCsv.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
        }

        // Replace initial policy by creating a new evaluator instance locally
        var accessPolicy = new GroupMembershipAccessPolicy(dynamicGroups);
        var evaluator = new ClientAccessEvaluator(accessPolicy);

        if (!evaluator.CheckAccess(out var denial))
        {
            startupLogger.LogWarning("Access denied starting client: {Reason}", denial);
            var writer = Services.GetService<IAuditWriter>();
            if (writer != null)
            {
                try { await writer.AccessDeniedAsync(denial ?? "unknown", CancellationToken.None); } catch { }
            }
            MessageBox.Show(denial ?? "Access denied", "IT Companion", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
            return;
        }

        MainWindow window = new()
        {
            DataContext = this.Services.GetService(typeof(PolicyEditorViewModel))
        };
        window.Show();
    }

    private ServiceProvider ConfigureServices()
    {
        ServiceCollection sc = new();

        sc.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddFileLogger(); // central file sink
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        sc.AddSingleton<IAdminTemplateLoader, AdmxAdmlParser>();
        sc.AddSingleton<PolicyEditorViewModel>();
        sc.AddSingleton<LogViewerViewModel>();
        sc.AddSingleton<DevDiagnosticsViewModel>();
        sc.AddSingleton<IBehaviorPolicyStore, BehaviorPolicyStore>();
        sc.AddSingleton<IAuditStore, AuditStore>();
        sc.AddSingleton<IAuditWriter, AuditWriter>();

        // Placeholder access policy (will be superseded at runtime evaluation)
        sc.AddSingleton<IClientAccessPolicy>(_ => new GroupMembershipAccessPolicy(new []{"Administrators"}));
        sc.AddSingleton<IClientAccessEvaluator, ClientAccessEvaluator>();

        return sc.BuildServiceProvider();
    }
}