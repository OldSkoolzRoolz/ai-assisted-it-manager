// Project Name: CorePolicyEngine
// File Name: LogIngestionRepositories.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


using System.Data;

using Microsoft.Data.SqlClient;


namespace KC.ITCompanion.CorePolicyEngine.Storage.Sql;


public sealed class LogSourceRepository : ILogSourceRepository
{
    private readonly ISqlConnectionFactory _factory;





    public LogSourceRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }





    public async Task<IReadOnlyList<LogSourceDto>> GetEnabledAsync(CancellationToken token)
    {
        const string sql =
            "SELECT LogSourceId,Application,FilePath,Enabled FROM dbo.LogSource WHERE Enabled = 1 ORDER BY Application";
        using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        List<LogSourceDto> list = new();
        using SqlDataReader? r = await ((SqlCommand)cmd).ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await r.ReadAsync(token).ConfigureAwait(false))
            list.Add(new LogSourceDto(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetBoolean(3)));
        return list;
    }





    public async Task UpsertAsync(string application, string filePath, bool enabled, CancellationToken token)
    {
        const string sql = @"MERGE dbo.LogSource AS tgt
USING (SELECT @Application AS Application, @FilePath AS FilePath) AS src
ON tgt.FilePath = src.FilePath
WHEN MATCHED THEN UPDATE SET Application = src.Application, Enabled=@Enabled, UpdatedUtc=SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Application, FilePath, Enabled, CreatedUtc, UpdatedUtc) VALUES (src.Application, src.FilePath, @Enabled, SYSUTCDATETIME(), SYSUTCDATETIME());";
        using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        IDbDataParameter p1 = cmd.CreateParameter();
        p1.ParameterName = "@Application";
        p1.Value = application;
        cmd.Parameters.Add(p1);
        IDbDataParameter p2 = cmd.CreateParameter();
        p2.ParameterName = "@FilePath";
        p2.Value = filePath;
        cmd.Parameters.Add(p2);
        IDbDataParameter p3 = cmd.CreateParameter();
        p3.ParameterName = "@Enabled";
        p3.Value = enabled;
        cmd.Parameters.Add(p3);
        await ((SqlCommand)cmd).ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }
}



public sealed class LogIngestionCursorRepository : ILogIngestionCursorRepository
{
    private readonly ISqlConnectionFactory _factory;





    public LogIngestionCursorRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }





    public async Task<LogIngestionCursorDto?> GetAsync(int logSourceId, CancellationToken token)
    {
        const string sql =
            "SELECT LogSourceId,LastFile,LastPosition,LastFileSize,LastHash,UpdatedUtc FROM dbo.LogIngestionCursor WHERE LogSourceId=@Id";
        using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using IDbCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = "@Id";
        p.Value = logSourceId;
        cmd.Parameters.Add(p);
        using SqlDataReader? r = await ((SqlCommand)cmd).ExecuteReaderAsync(token).ConfigureAwait(false);
        if (await r.ReadAsync(token).ConfigureAwait(false))
            return new LogIngestionCursorDto(r.GetInt32(0), r.IsDBNull(1) ? null : r.GetString(1), r.GetInt64(2),
                r.GetInt64(3), r.IsDBNull(4) ? null : (byte[])r[4], r.GetDateTime(5));
        return null;
    }





    public async Task UpsertAsync(LogIngestionCursorDto cursor, CancellationToken token)
    {
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
        cmd.Parameters.Add(Param(cmd, "@LastHash", (object?)cursor.LastHash ?? DBNull.Value));
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



public sealed class LogEventRepository : ILogEventRepository
{
    private readonly ISqlConnectionFactory _factory;





    public LogEventRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }





    public async Task BulkInsertAsync(IEnumerable<LogEventDto> eventsBatch, CancellationToken token)
    {
        var table = new DataTable();
        table.Columns.Add("LogSourceId", typeof(int));
        table.Columns.Add("Ts", typeof(DateTime));
        table.Columns.Add("Level", typeof(byte));
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
            table.Rows.Add(e.LogSourceId, e.Ts, e.Level, (object?)e.EventId ?? DBNull.Value, e.Category, e.Message,
                e.Session, e.Host, e.UserName, e.AppVersion, e.ModuleVersion, null);
        using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
        using var bulk = new SqlBulkCopy((SqlConnection)conn)
            { DestinationTableName = "dbo.LogEvent", BatchSize = table.Rows.Count };
        foreach (DataColumn c in table.Columns) bulk.ColumnMappings.Add(c.ColumnName, c.ColumnName);
        await bulk.WriteToServerAsync(table, token).ConfigureAwait(false);
    }
}