// Project Name: CorePolicyEngine
// File Name: ILogIngestionRepositories.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

namespace KC.ITCompanion.CorePolicyEngine.Storage.Sql;

/// <summary>Repository for managing log source definitions.</summary>
public interface ILogSourceRepository
{
    /// <summary>Gets enabled log sources.</summary>
    Task<IReadOnlyList<LogSourceDto>> GetEnabledAsync(CancellationToken token);
    /// <summary>Upserts a log source by file path.</summary>
    Task UpsertAsync(string application, string filePath, bool enabled, CancellationToken token);
}

/// <summary>Repository for log ingestion cursor state.</summary>
public interface ILogIngestionCursorRepository
{
    /// <summary>Gets cursor for a log source.</summary>
    Task<LogIngestionCursorDto?> GetAsync(int logSourceId, CancellationToken token);
    /// <summary>Upserts cursor for a log source.</summary>
    Task UpsertAsync(LogIngestionCursorDto cursor, CancellationToken token);
}

/// <summary>Repository for bulk inserting log events.</summary>
public interface ILogEventRepository
{
    /// <summary>Bulk inserts a batch.</summary>
    Task BulkInsertAsync(IEnumerable<LogEventDto> eventsBatch, CancellationToken token);
}