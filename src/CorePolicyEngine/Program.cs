using CorePolicyEngine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Security;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IAuditLogService, AuditLogService>();
builder.Services.AddHostedService<EngineWorker>();

var app = builder.Build();
await app.RunAsync();