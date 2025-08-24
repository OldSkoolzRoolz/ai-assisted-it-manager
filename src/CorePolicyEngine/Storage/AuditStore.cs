// Project Name: CorePolicyEngine
// File Name: AuditStore.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KC.ITCompanion.CorePolicyEngine.Storage;

/// <summary>
/// Abstraction for persisting audit records (append-only jsonl file currently).
/// </summary>
public interface IAuditStore
{
    /// <summary>Ensures backing storage exists / is initialized.</summary>
    Task InitializeAsync(CancellationToken token);
    /// <summary>Writes an audit record.</summary>
    Task WriteAsync(AuditRecord record, CancellationToken token);
}

/// <summary>
/// Immutable audit record for user / policy actions.
/// </summary>
/// <param name="AuditId">Unique audit identifier (GUID N format).</param>
/// <param name="EventType">Event type discriminator (e.g. PolicyViewed).</param>
/// <param name="Actor">Actor identifier (username).</param>
/// <param name="ActorType">Actor classification (e.g. User, System).</param>
/// <param name="PolicyKey">Associated policy key if applicable.</param>
/// <param name="DetailsJson">Optional JSON payload with extra details.</param>
/// <param name="CreatedUtc">Creation timestamp (UTC).</param>
public sealed record AuditRecord(
    string AuditId,
    string EventType,
    string Actor,
    string ActorType,
    string? PolicyKey,
    string? DetailsJson,
    DateTime CreatedUtc
);

/// <summary>
/// File-based implementation of <see cref="IAuditStore"/> writing JSONL lines.
/// </summary>
public sealed class AuditStore : IAuditStore, IDisposable
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    /// <summary>Creates a new audit store writing to the provided path or default common app data location.</summary>
    public AuditStore(string? filePath = null)
    {
        _filePath = filePath ??
                     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                         "AIManager", "client", "audit-events.jsonl");
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken token)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        if (!File.Exists(_filePath))
        {
            await File.WriteAllTextAsync(_filePath, string.Empty, token).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task WriteAsync(AuditRecord record, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(record);
        var line = System.Text.Json.JsonSerializer.Serialize(record);
        await _gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine, token).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Dispose pattern.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _gate.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// High level writer that constructs audit records for common policy related events.
/// </summary>
public interface IAuditWriter
{
    /// <summary>Records an event when a policy is viewed.</summary>
    Task PolicyViewedAsync(string policyKey, CancellationToken token = default);
    /// <summary>Records an event when a policy is selected.</summary>
    Task PolicySelectedAsync(string policyKey, CancellationToken token = default);
    /// <summary>Records an edit to a policy element.</summary>
    Task PolicyEditedAsync(string policyKey, string elementId, string? newValue, CancellationToken token = default);
    /// <summary>Records a push / apply operation of pending policy edits.</summary>
    Task PolicyEditPushedAsync(int changeCount, CancellationToken token = default);
    /// <summary>Records an access denied event with reason.</summary>
    Task AccessDeniedAsync(string reason, CancellationToken token = default);
}

/// <summary>
/// Default implementation of <see cref="IAuditWriter"/> delegating persistence to an <see cref="IAuditStore"/>.
/// </summary>
public sealed class AuditWriter : IAuditWriter
{
    private readonly IAuditStore _store;

    /// <summary>Create a new writer.</summary>
    public AuditWriter(IAuditStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    /// <inheritdoc/>
    public Task PolicyViewedAsync(string policyKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyKey);
        return WriteAsync("PolicyViewed", policyKey, null, token);
    }

    /// <inheritdoc/>
    public Task PolicySelectedAsync(string policyKey, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyKey);
        return WriteAsync("PolicySelected", policyKey, null, token);
    }

    /// <inheritdoc/>
    public Task PolicyEditedAsync(string policyKey, string elementId, string? newValue, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(elementId);
        return WriteAsync("PolicyEdited", policyKey, $"{BuildJsonKV("elementId", elementId)},{BuildJsonKV("value", newValue, allowNull:true)}", token);
    }

    /// <inheritdoc/>
    public Task PolicyEditPushedAsync(int changeCount, CancellationToken token = default)
    {
        return WriteAsync("PolicyPush", null, $"{BuildJsonKV("changeCount", changeCount.ToString(System.Globalization.CultureInfo.InvariantCulture))}", token);
    }

    /// <inheritdoc/>
    public Task AccessDeniedAsync(string reason, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        var safeReason = reason.Replace("\"", "'", StringComparison.Ordinal);
        return WriteAsync("AccessDenied", null, $"{BuildJsonKV("reason", safeReason)}", token);
    }

    private static string Actor() => Environment.UserName;

    private Task WriteAsync(string eventType, string? policyKey, string? detailsInnerJson, CancellationToken token)
    {
        string? details = detailsInnerJson is null ? null : "{" + detailsInnerJson + "}";
        return _store.WriteAsync(
            new AuditRecord(Guid.NewGuid().ToString("N"), eventType, Actor(), "User", policyKey, details,
                DateTime.UtcNow), token);
    }

    private static string BuildJsonKV(string key, string? value, bool allowNull = false)
    {
        if (value is null)
        {
            if (!allowNull) value = string.Empty; else return $"\"{key}\":null";
        }
        var escaped = value.Replace("\"", "'", StringComparison.Ordinal);
        return $"\"{key}\":\"{escaped}\"";
    }
}