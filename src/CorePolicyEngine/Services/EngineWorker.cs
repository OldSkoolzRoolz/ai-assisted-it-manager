using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Security;


namespace CorePolicyEngine.Services;

public sealed class EngineWorker(
    ILogger<EngineWorker> logger,
    IAuditLogService audit,
    IDeploymentService _deployment, // <-- Added missing dependency
    IPolicyParser _parser,          // <-- Added missing dependency
    IVersionControlService _vc,     // <-- Added missing dependency
    IComplianceService _compliance  // <-- Added missing dependency
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        audit.Info("CorePolicyEngine started.");
        // Phase 1: simple demo loop every 30s (could be triggered by API in future)
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Engine tick at {Time}", DateTimeOffset.Now);
            var sampleAdmx = "<policy name=\"Sample\">value</policy>"; // placeholder
            PolicyDocument doc = _parser.Parse(sampleAdmx);
            var version = _vc.AddVersion("SamplePolicy", doc.RawAdmx);

            await _deployment.DeployAsync("SamplePolicy", doc.RawAdmx, stoppingToken);
            PolicyVersion? latest = _vc.GetLatest("SamplePolicy");
            if (latest.HasValue)
            {
                ComplianceResult compliance = _compliance.Compare(doc.RawAdmx, latest.Value.Content);
                Console.WriteLine($"Compliance: {compliance.IsCompliant} differences={string.Join(';', compliance.Differences)}");
            }
            Console.WriteLine($"Processed SamplePolicy version {version} at {DateTime.UtcNow:o}");
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
        audit.Info("CorePolicyEngine stopping.");
    }
}