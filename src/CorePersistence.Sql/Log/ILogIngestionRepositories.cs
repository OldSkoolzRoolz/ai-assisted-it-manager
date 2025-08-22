// Project Name: CorePersistence.Sql
// File Name: ILogIngestionRepositories.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.Data.SqlClient;

namespace KC.ITCompanion.CorePersistence.Sql;

public interface ILogSourceRepository
{
    Task<IReadOnlyList<LogSourceDto>> GetEnabledAsync(CancellationToken token);
    Task UpsertAsync(string application, string filePath, bool enabled, CancellationToken token);
}

public interface ILogIngestionCursorRepository
{
    Task<LogIngestionCursorDto?> GetAsync(int logSourceId, CancellationToken token);
    Task UpsertAsync(LogIngestionCursorDto cursor, CancellationToken token);
}

public interface ILogEventRepository
{
    Task BulkInsertAsync(IEnumerable<LogEventDto> eventsBatch, CancellationToken token);
}
