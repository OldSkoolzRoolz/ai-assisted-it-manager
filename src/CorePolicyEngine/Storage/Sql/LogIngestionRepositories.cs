// Project Name: CorePolicyEngine
// File Name: LogIngestionRepositories.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System.Data;
using Microsoft.Data.SqlClient;

namespace KC.ITCompanion.CorePolicyEngine.Storage.Sql;

/// <summary>
/// Repository for log source definitions.
/// </summary>
public sealed class LogSourceRepository : ILogSourceRepository
{
    private readonly ISqlConnectionFactory _factory;

    /// <summary>Create repository.</summary>
    public LogSourceRepository(ISqlConnectionFactory factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    /// <summary>Returns enabled log sources.</summary>
    public async Task<IReadOnlyList<LogSourceDto>> GetEnabledAsync(CancellationToken token)
    {
        const string sql =
            "SELECT LogSourceId,Application,FilePath,Enabled FROM dbo.LogSource WHERE Enabled = 1 ORDER BY Application";
        using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        List<LogSourceDto> list = new();
        using SqlDataReader r = await ((SqlCommand)cmd).ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await r.ReadAsync(token).ConfigureAwait(false))
            list.Add(new LogSourceDto(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetBoolean(3)));
        return list;
    }

    /// <summary>Insert or update a log source by file path.</summary>
    public async Task UpsertAsync(string application, string filePath, bool enabled, CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(application);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        const string sql = @"MERGE dbo.LogSource AS tgt
USING (SELECT @Application AS Application, @FilePath AS FilePath) AS src
ON tgt.FilePath = src.FilePath
WHEN MATCHED THEN UPDATE SET Application = src.Application, Enabled=@Enabled, UpdatedUtc=SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Application, FilePath, Enabled, CreatedUtc, UpdatedUtc) VALUES (src.Application, src.FilePath, @Enabled, SYSUTCDATETIME(), SYSUTCDATETIME());";
        using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(Param(cmd, "@Application", application));
        cmd.Parameters.Add(Param(cmd, "@FilePath", filePath));
        cmd.Parameters.Add(Param(cmd, "@Enabled", enabled));
        await ((SqlCommand)cmd).ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }

    private static SqlParameter Param(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        return (SqlParameter)p;
    }
}

/// <summary>
/// Repository for log ingestion cursor persistence.
/// </summary>
public sealed class LogIngestionCursorRepository : ILogIngestionCursorRepository
{
    private readonly ISqlConnectionFactory _factory;

    /// <summary>Create repository.</summary>
    public LogIngestionCursorRepository(ISqlConnectionFactory factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    /// <summary>Gets a cursor for a log source if present.</summary>
    public async Task<LogIngestionCursorDto?> GetAsync(int logSourceId, CancellationToken token)
    {
        const string sql =
            "SELECT LogSourceId,LastFile,LastPosition,LastFileSize,LastHash,UpdatedUtc FROM dbo.LogIngestionCursor WHERE LogSourceId=@Id";
        using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(Param(cmd, "@Id", logSourceId));
        using SqlDataReader r = await ((SqlCommand)cmd).ExecuteReaderAsync(token).ConfigureAwait(false);
        if (await r.ReadAsync(token).ConfigureAwait(false))
        {
            string? lastFile = await r.IsDBNullAsync(1, token).ConfigureAwait(false) ? null : r.GetString(1);
            ReadOnlyMemory<byte>? hash = await r.IsDBNullAsync(4, token).ConfigureAwait(false) ? null : new ReadOnlyMemory<byte>((byte[])r[4]);
            return new LogIngestionCursorDto(r.GetInt32(0), lastFile, r.GetInt64(2), r.GetInt64(3), hash, r.GetDateTime(5));
        }
        return null;
    }

    /// <summary>Upserts a cursor row.</summary>
    public async Task UpsertAsync(LogIngestionCursorDto cursor, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        const string sql = @"MERGE dbo.LogIngestionCursor AS tgt
USING (SELECT @LogSourceId AS LogSourceId) AS src
ON tgt.LogSourceId = src.LogSourceId
WHEN MATCHED THEN UPDATE SET LastFile=@LastFile, LastPosition=@LastPosition, LastFileSize=@LastFileSize, LastHash=@LastHash, UpdatedUtc=SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (LogSourceId, LastFile, LastPosition, LastFileSize, LastHash, UpdatedUtc) VALUES (@LogSourceId, @LastFile, @LastPosition, @LastFileSize, @LastHash, SYSUTCDATETIME());";
        using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(Param(cmd, "@LogSourceId", cursor.LogSourceId));
        cmd.Parameters.Add(Param(cmd, "@LastFile", (object?)cursor.LastFile ?? DBNull.Value));
        cmd.Parameters.Add(Param(cmd, "@LastPosition", cursor.LastPosition));
        cmd.Parameters.Add(Param(cmd, "@LastFileSize", cursor.LastFileSize));
        cmd.Parameters.Add(Param(cmd, "@LastHash", cursor.LastHash.HasValue ? cursor.LastHash.Value.ToArray() : (object)DBNull.Value));
        await ((SqlCommand)cmd).ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }

    private static SqlParameter Param(IDbCommand cmd, string name, object value)
    {
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        return (SqlParameter)p;
    }
}

/// <summary>
/// Repository for bulk inserting log events.
/// </summary>
public sealed class LogEventRepository : ILogEventRepository
{
    private readonly ISqlConnectionFactory _factory;

    /// <summary>Create repository.</summary>
    public LogEventRepository(ISqlConnectionFactory factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

    /// <summary>Bulk inserts a batch of log events via <see cref="SqlBulkCopy"/>.</summary>
    public async Task BulkInsertAsync(IEnumerable<LogEventDto> eventsBatch, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(eventsBatch);
        using var table = new DataTable();
        table.Columns.Add("LogSourceId", typeof(int));
        table.Columns.Add("Ts", typeof(DateTime));
        table.Columns.Add("Level", typeof(int));
        table.Columns.Add("EventId", typeof(int));
        table.Columns.Add("Category", typeof(string));
        table.Columns.Add("Message", typeof(string));
        table.Columns.Add("Session", typeof(string));
        table.Columns.Add("Host", typeof(string));
        table.Columns.Add("UserName", typeof(string));
        table.Columns.Add("AppVersion", typeof(string));
        table.Columns.Add("ModuleVersion", typeof(string));
        table.Columns.Add("RawJson", typeof(string));
        foreach (LogEventDto e in eventsBatch)
        {
            object eventIdVal = e.EventId is null ? DBNull.Value : e.EventId.Value;
            table.Rows.Add(e.LogSourceId, e.Ts, e.Level, eventIdVal, e.Category, e.Message,
                e.Session, e.Host, e.UserName, e.AppVersion, e.ModuleVersion, null);
        }
        using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using var bulk = new SqlBulkCopy((SqlConnection)conn)
        { DestinationTableName = "dbo.LogEvent", BatchSize = table.Rows.Count };
        foreach (DataColumn c in table.Columns) bulk.ColumnMappings.Add(c.ColumnName, c.ColumnName);
        await bulk.WriteToServerAsync(table, token).ConfigureAwait(false);
    }
}