// Project Name: CorePolicyEngine
// File Name: AuditStore.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
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
    private readonly SemaphoreSlim _gate = new(1, 1);





    public AuditStore(string? filePath = null)
    {
        this._filePath = filePath ??
                         Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                             "AIManager", "client", "audit-events.jsonl");
    }





    public Task InitializeAsync(CancellationToken token)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(this._filePath)!);
        if (!File.Exists(this._filePath))
        {
            var filePath = this._filePath;
            if (filePath != null)
                File.WriteAllText(filePath, string.Empty);
        }

        return Task.CompletedTask;
    }





    public async Task WriteAsync(AuditRecord record, CancellationToken token)
    {
        var line = System.Text.Json.JsonSerializer.Serialize(record);
        await this._gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await File.AppendAllTextAsync(this._filePath, line + Environment.NewLine, token).ConfigureAwait(false);
        }
        finally
        {
            this._gate.Release();
        }
    }
}



public interface IAuditWriter
{
    Task PolicyViewedAsync(string policyKey, CancellationToken token = default);
    Task PolicySelectedAsync(string policyKey, CancellationToken token = default);
    Task PolicyEditedAsync(string policyKey, string elementId, string? newValue, CancellationToken token = default);
    Task PolicyEditPushedAsync(int changeCount, CancellationToken token = default);
    Task AccessDeniedAsync(string reason, CancellationToken token = default);
}



public sealed class AuditWriter : IAuditWriter
{
    private readonly IAuditStore _store;





    public AuditWriter(IAuditStore store)
    {
        this._store = store;
    }





    public Task PolicyViewedAsync(string policyKey, CancellationToken token = default)
    {
        return WriteAsync("PolicyViewed", policyKey, null, token);
    }





    public Task PolicySelectedAsync(string policyKey, CancellationToken token = default)
    {
        return WriteAsync("PolicySelected", policyKey, null, token);
    }





    public Task PolicyEditedAsync(string policyKey, string elementId, string? newValue, CancellationToken token = default)
    {
        return WriteAsync("PolicyEdited", policyKey, $"{{\"elementId\":\"{elementId}\",\"value\":{JsonEscapeNullable(newValue)} }}", token);
    }





    public Task PolicyEditPushedAsync(int changeCount, CancellationToken token = default)
    {
        return WriteAsync("PolicyPush", null, $"{{\"changeCount\":{changeCount}}}", token);
    }





    public Task AccessDeniedAsync(string reason, CancellationToken token = default)
    {
        return WriteAsync("AccessDenied", null, "{\"reason\":\"" + reason.Replace("\"", "'") + "\"}", token);
    }





    private static string Actor()
    {
        return Environment.UserName;
    }





    private Task WriteAsync(string eventType, string? policyKey, string? details, CancellationToken token)
    {
        return this._store.WriteAsync(
            new AuditRecord(Guid.NewGuid().ToString("N"), eventType, Actor(), "User", policyKey, details,
                DateTime.UtcNow), token);
    }

    private static string JsonEscapeNullable(string? value)
    {
        return value is null ? "null" : "\"" + value.Replace("\"", "'") + "\"";
    }
}