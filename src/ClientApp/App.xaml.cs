// Project Name: ClientApp
// File Name: App.xaml.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using KC.ITCompanion.ClientApp.Services;
using KC.ITCompanion.ClientApp.Views;
using KC.ITCompanion.CorePolicyEngine.Models;
using KC.ITCompanion.CorePolicyEngine.Parsing;
using KC.ITCompanion.CorePolicyEngine.Storage;
using KC.ITCompanion.CorePolicyEngine.Storage.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Security;

namespace KC.ITCompanion.ClientApp;

public partial class App : Application
{
    private static readonly string[] DefaultAccessGroups = ["Administrators"]; // CA1861 fixed
    public ServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        this.Services = ConfigureServices();

        ILogger<App> startupLogger = this.Services.GetRequiredService<ILogger<App>>();
        Logger.LogClientappStartingUpAtStarttimeutc(startupLogger, DateTime.UtcNow);

        await InitializeAuditStoreAsync(startupLogger).ConfigureAwait(false);
        var dynamicGroups = await ResolveDynamicGroupsAsync(startupLogger).ConfigureAwait(false);
        if (!await VerifyAccessAsync(dynamicGroups, startupLogger).ConfigureAwait(false))
        {
            Shutdown(-1);
            return;
        }

        var themeSvc = this.Services.GetRequiredService<IThemeService>();
        themeSvc.Initialize(); // auto apply system theme

        // ShowSplash(); // Uncomment to show splash screen
        CreateAndShowMainWindow();
    }

    private static ServiceProvider ConfigureServices()
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
        sc.AddSingleton<IClientAccessPolicy>(sp =>
            new GroupMembershipAccessPolicy(DefaultAccessGroups,
                sp.GetService<ILogger<GroupMembershipAccessPolicy>>()));
        sc.AddSingleton<IClientAccessEvaluator, ClientAccessEvaluator>();
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
            Logger.LogAuditStoreInitializationFailed(logger, ex);
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
            Logger.LogBehaviorPolicyStoreInitializationFailedUsingDefaultAccessGroups(logger, ex);
        }

        return dynamicGroups;
    }

    private async Task<bool> VerifyAccessAsync(string[] dynamicGroups, ILogger logger)
    {
        var accessPolicy = new GroupMembershipAccessPolicy(dynamicGroups,
            this.Services.GetService<ILogger<GroupMembershipAccessPolicy>>());
        var evaluator = new ClientAccessEvaluator(accessPolicy);
        if (evaluator.CheckAccess(out var denialReason)) return true;

        Logger.LogAccessDeniedStartingClientReason(logger, denialReason ?? "unknown"); // CS8604 fixed
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
        catch { }
    }

    private static void ShowSplash()
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