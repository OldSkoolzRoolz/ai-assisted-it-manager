// Project Name: ClientApp
// File Name: App.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT License. See LICENSE file in the project root for full license text.
// Do not remove file headers


using KC.ITCompanion.ClientApp.Services;
using KC.ITCompanion.ClientApp.Views;
using KC.ITCompanion.CorePersistence.Sql;
using KC.ITCompanion.CorePolicyEngine.Models;
using KC.ITCompanion.CorePolicyEngine.Parsing;
using KC.ITCompanion.CorePolicyEngine.Storage;
using KC.ITCompanion.Security;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


// log ingestion repos live in CorePolicyEngine.Storage.Sql


namespace KC.ITCompanion.ClientApp;


public partial class App : Application
{
    public ServiceProvider Services { get; private set; } = null!;





    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        this.Services = ConfigureServices();

        ILogger<App> startupLogger = this.Services.GetRequiredService<ILogger<App>>();
        startupLogger.LogInformation("ClientApp starting up at {StartTimeUtc}", DateTime.UtcNow);

        await InitializeAuditStoreAsync(startupLogger).ConfigureAwait(false);
        var dynamicGroups = await ResolveDynamicGroupsAsync(startupLogger).ConfigureAwait(false);
        if (!await VerifyAccessAsync(dynamicGroups, startupLogger).ConfigureAwait(false))
        {
            Shutdown(-1);
            return;
        }

        this.Services.GetRequiredService<IThemeService>().Apply(AppTheme.Light);

        // ShowSplash(); // Uncomment to show splash screen
        CreateAndShowMainWindow();
    }





    /// <summary>
    ///     Configures and registers application services into a dependency injection container.
    /// </summary>
    /// <remarks>
    ///     This method sets up various services required by the application, including logging,
    ///     view models, repositories, and other core services. It ensures that all dependencies
    ///     are properly registered and available for use throughout the application.
    /// </remarks>
    /// <returns>
    ///     A <see cref="Microsoft.Extensions.DependencyInjection.ServiceProvider" /> instance
    ///     containing the configured services.
    /// </returns>
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
        sc.AddSingleton<StatusOverviewViewModel>();
        sc.AddSingleton<PoliciesReadOnlyViewModel>();
        sc.AddSingleton<IBehaviorPolicyStore, BehaviorPolicyStore>();
        sc.AddSingleton<IAuditStore, AuditStore>();
        sc.AddSingleton<IAuditWriter, AuditWriter>();
        sc.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        sc.AddSingleton<IPolicyDefinitionRepository, PolicyDefinitionRepository>();
        sc.AddSingleton<IPolicyGroupRepository, PolicyGroupRepository>();
        sc.AddSingleton<IClientAccessPolicy>(sp => new GroupMembershipAccessPolicy(new[] { "Administrators" }, sp.GetService<ILogger<GroupMembershipAccessPolicy>>()));
        sc.AddSingleton(typeof(IClientAccessEvaluator), typeof(ClientAccessEvaluator));
        sc.AddSingleton<IThemeService, ThemeService>();
        sc.AddSingleton<ILogSourceRepository, LogSourceRepository>();
        sc.AddSingleton<ILogIngestionCursorRepository, LogIngestionCursorRepository>();
        sc.AddSingleton<ILogEventRepository, LogEventRepository>();

        return sc.BuildServiceProvider();
    }





    #region Startup Steps

    private async Task InitializeAuditStoreAsync(ILogger logger)
    {
        IAuditStore? auditStore = this.Services.GetService<IAuditStore>();
        if (auditStore == null) return;
        try
        {
            await auditStore.InitializeAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Audit store initialization failed");
        }
    }





    private async Task<string[]> ResolveDynamicGroupsAsync(ILogger logger)
    {
        string[] dynamicGroups = ["BUILTIN\\Administrators"]; // default
        IBehaviorPolicyStore? store = this.Services.GetService<IBehaviorPolicyStore>();
        if (store == null) return dynamicGroups;
        try
        {
            await store.InitializeAsync(CancellationToken.None).ConfigureAwait(false);
            BehaviorPolicySnapshot snap = await store.GetSnapshotAsync(CancellationToken.None).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(snap.Effective.AllowedGroupsCsv))
                dynamicGroups = snap.Effective.AllowedGroupsCsv
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Behavior policy store initialization failed; using default access groups");
        }

        return dynamicGroups;
    }





    private async Task<bool> VerifyAccessAsync(string[] dynamicGroups, ILogger logger)
    {
        var accessPolicy = new GroupMembershipAccessPolicy(dynamicGroups, Services.GetService<ILogger<GroupMembershipAccessPolicy>>());
        var evaluator = new ClientAccessEvaluator(accessPolicy);
        if (evaluator.CheckAccess(out var denialReason)) return true;

        logger.LogWarning("Access denied starting client: {Reason}", denialReason);
        await AuditAccessDeniedAsync(denialReason);
        MessageBox.Show(denialReason ?? "Access denied", "IT Companion", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
    }





    private async Task AuditAccessDeniedAsync(string? denialReason)
    {
        IAuditWriter? writer = this.Services.GetService<IAuditWriter>();
        if (writer == null) return;
        try
        {
            await writer.AccessDeniedAsync(denialReason ?? "unknown", CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
        }
    }





    private void ShowSplash()
    {
        var splash = new Splash();
        splash.ShowDialog();
    }





    private void CreateAndShowMainWindow()
    {
        var window = new MainWindow
        {
            DataContext = this.Services.GetService(typeof(PolicyEditorViewModel))
        };
        window.Show();
    }

    #endregion
}