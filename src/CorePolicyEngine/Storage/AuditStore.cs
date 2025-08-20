// Project Name: CorePolicyEngine
// File Name: AuditStore.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

namespace KC.ITCompanion.CorePolicyEngine.Storage;

public interface IAuditStore
{
    Task InitializeAsync(CancellationToken token);
    Task WriteAsync(AuditRecord record, CancellationToken token);
}

public sealed record AuditRecord(
    string AuditId,
    string EventType,
    string Actor,
    string ActorType,
    string? PolicyKey,
    string? DetailsJson,
    DateTime CreatedUtc
);

public sealed class AuditStore : IAuditStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1,1);

    public AuditStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AIManager","client","audit-events.jsonl");
    }

    public Task InitializeAsync(CancellationToken token)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        if (!File.Exists(_filePath)) File.WriteAllText(_filePath, string.Empty);
        return Task.CompletedTask;
    }

    public async Task WriteAsync(AuditRecord record, CancellationToken token)
    {
        var line = System.Text.Json.JsonSerializer.Serialize(record);
        await _gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine, token).ConfigureAwait(false);
        }
        finally { _gate.Release(); }
    }
}

public interface IAuditWriter
{
    Task PolicyViewedAsync(string policyKey, CancellationToken token = default);
    Task PolicySelectedAsync(string policyKey, CancellationToken token = default);
    Task AccessDeniedAsync(string reason, CancellationToken token = default);
}

public sealed class AuditWriter : IAuditWriter
{
    private readonly IAuditStore _store;
    public AuditWriter(IAuditStore store) => _store = store;
    private static string Actor() => Environment.UserName;
    private Task WriteAsync(string eventType, string? policyKey, string? details, CancellationToken token)
        => _store.WriteAsync(new AuditRecord(Guid.NewGuid().ToString("N"), eventType, Actor(), "User", policyKey, details, DateTime.UtcNow), token);
    public Task PolicyViewedAsync(string policyKey, CancellationToken token = default) => WriteAsync("PolicyViewed", policyKey, null, token);
    public Task PolicySelectedAsync(string policyKey, CancellationToken token = default) => WriteAsync("PolicySelected", policyKey, null, token);
    public Task AccessDeniedAsync(string reason, CancellationToken token = default) => WriteAsync("AccessDenied", null, "{\"reason\":\"" + reason.Replace("\"","'") + "\"}", token);
}
