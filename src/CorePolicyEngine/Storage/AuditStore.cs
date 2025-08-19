// Project Name: CorePolicyEngine
// File Name: AuditStore.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.Data.Sqlite;

namespace CorePolicyEngine.Storage;

/// <summary>
/// Lightweight local audit store (client side). Enterprise aggregation will ingest these records.
/// </summary>
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
    private readonly string _dbPath;

    public AuditStore(string? dbPath = null)
    {
        _dbPath = dbPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AIManager","client","audit.db");
    }

    private SqliteConnection Open()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
        var c = new SqliteConnection($"Data Source={_dbPath}");
        c.Open();
        return c;
    }

    public async Task InitializeAsync(CancellationToken token)
    {
        using var c = Open();
        var cmd = c.CreateCommand();
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS AuditEvent(
AuditId TEXT PRIMARY KEY,
EventType TEXT NOT NULL,
Actor TEXT NOT NULL,
ActorType TEXT NOT NULL,
PolicyKey TEXT,
DetailsJson TEXT,
CreatedUtc TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS IX_AuditEvent_EventType ON AuditEvent(EventType);
CREATE INDEX IF NOT EXISTS IX_AuditEvent_CreatedUtc ON AuditEvent(CreatedUtc);";
        await cmd.ExecuteNonQueryAsync(token);
    }

    public async Task WriteAsync(AuditRecord record, CancellationToken token)
    {
        using var c = Open();
        var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO AuditEvent (AuditId,EventType,Actor,ActorType,PolicyKey,DetailsJson,CreatedUtc) VALUES ($id,$et,$a,$at,$pk,$d,$t)";
        cmd.Parameters.AddWithValue("$id", record.AuditId);
        cmd.Parameters.AddWithValue("$et", record.EventType);
        cmd.Parameters.AddWithValue("$a", record.Actor);
        cmd.Parameters.AddWithValue("$at", record.ActorType);
        cmd.Parameters.AddWithValue("$pk", (object?)record.PolicyKey ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$d", (object?)record.DetailsJson ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$t", record.CreatedUtc.ToString("O"));
        await cmd.ExecuteNonQueryAsync(token);
    }
}

/// <summary>
/// Simple facade providing higher level audit intents.
/// </summary>
public interface IAuditWriter
{
    Task PolicyViewedAsync(string policyKey, CancellationToken token = default);
    Task PolicySelectedAsync(string policyKey, CancellationToken token = default);
    Task AccessDeniedAsync(string reason, CancellationToken token = default);
}

public sealed class AuditWriter : IAuditWriter
{
    private readonly IAuditStore _store;

    public AuditWriter(IAuditStore store)
    {
        _store = store;
    }

    private static string Actor() => Environment.UserName;

    private Task WriteAsync(string eventType, string? policyKey, string? details, CancellationToken token)
        => _store.WriteAsync(new AuditRecord(Guid.NewGuid().ToString("N"), eventType, Actor(), "User", policyKey, details, DateTime.UtcNow), token);

    public Task PolicyViewedAsync(string policyKey, CancellationToken token = default) => WriteAsync("PolicyViewed", policyKey, null, token);
    public Task PolicySelectedAsync(string policyKey, CancellationToken token = default) => WriteAsync("PolicySelected", policyKey, null, token);
    public Task AccessDeniedAsync(string reason, CancellationToken token = default) => WriteAsync("AccessDenied", null, "{\"reason\":\"" + reason.Replace("\"","'") + "\"}", token);
}
