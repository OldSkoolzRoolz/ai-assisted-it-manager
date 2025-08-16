using CorePolicyEngine.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Security;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Core services
builder.Services.AddSingleton<IAuditLogService, AuditLogService>();
builder.Services.AddSingleton<IPolicyParser, PolicyParser>();
builder.Services.AddSingleton<IDeploymentService, DeploymentService>();
builder.Services.AddSingleton<IVersionControlService, VersionControlService>();
builder.Services.AddSingleton<IComplianceService, ComplianceService>();

// Worker
builder.Services.AddHostedService<EngineWorker>();

await builder.Build().RunAsync();