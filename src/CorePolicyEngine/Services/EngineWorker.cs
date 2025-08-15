using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Security;

namespace CorePolicyEngine.Services;

public sealed class EngineWorker(ILogger<EngineWorker> logger, IAuditLogService audit) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        audit.Info("CorePolicyEngine started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Engine tick at {Time}", DateTimeOffset.Now);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        audit.Info("CorePolicyEngine stopping.");
    }
}