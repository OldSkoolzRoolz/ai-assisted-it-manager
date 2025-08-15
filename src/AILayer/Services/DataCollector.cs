namespace AILayer.Services;

public interface IDataCollector
{
    Task<IDictionary<string, object>> CollectAsync(CancellationToken ct = default);
}

public sealed class DataCollector : IDataCollector
{
    public Task<IDictionary<string, object>> CollectAsync(CancellationToken ct = default)
        => Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>
        {
            ["Host"] = Environment.MachineName,
            ["Utc"] = DateTime.UtcNow
        });
}