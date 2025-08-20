// Project Name: ClientApp
// File Name: App.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using KC.ITCompanion.CorePolicyEngine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using KC.ITCompanion.ClientApp.Logging;
using KC.ITCompanion.ClientApp.ViewModels;
using KC.ITCompanion.Security;
using KC.ITCompanion.CorePolicyEngine.Storage;
using KC.ITCompanion.CorePersistence.Sql;

namespace KC.ITCompanion.ClientApp;

public partial class App : Application
{
    public ServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Services = ConfigureServices();

        var startupLogger = Services.GetRequiredService<ILogger<App>>();
        startupLogger.LogInformation("ClientApp starting up at {StartTimeUtc}", DateTime.UtcNow);

        var auditStore = Services.GetService<IAuditStore>();
        if (auditStore != null)
            await auditStore.InitializeAsync(CancellationToken.None);

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

        var window = new MainWindow
        {
            DataContext = Services.GetService(typeof(PolicyEditorViewModel))
        };
        window.Show();
    }

    private ServiceProvider ConfigureServices()
    {
        var sc = new ServiceCollection();

        sc.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddFileLogger();
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
        sc.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        sc.AddSingleton<IPolicyDefinitionRepository, PolicyDefinitionRepository>();
        sc.AddSingleton<IPolicyGroupRepository, PolicyGroupRepository>();
        sc.AddSingleton<IClientAccessPolicy>(_ => new GroupMembershipAccessPolicy(new []{"Administrators"}));
        sc.AddSingleton(typeof(IClientAccessEvaluator), typeof(ClientAccessEvaluator));

        return sc.BuildServiceProvider();
    }
}