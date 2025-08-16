namespace AILayer;

public class DataCollector
{
    public SystemSnapshot Collect()
    {
        // Phase 1 stub: return static snapshot
        return new SystemSnapshot
        {
            CollectedAtUtc = DateTime.UtcNow,
            Hostname = Environment.MachineName,
            Metrics = new Dictionary<string, string>
            {
                ["OSVersion"] = Environment.OSVersion.ToString(),
                ["ProcessorCount"] = Environment.ProcessorCount.ToString()
            }
        };
    }
}

public class SystemSnapshot
{
    public DateTime CollectedAtUtc { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public IDictionary<string, string> Metrics { get; set; } = new Dictionary<string, string>();
}
