// Project Name: CorePolicyEngine
// File Name: LogQueryRepositories.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System.Data;
using Microsoft.Data.SqlClient;

namespace KC.ITCompanion.CorePolicyEngine.Storage.Sql;

/// <summary>
/// Query repository for reading log events (separate from ingestion path).
/// </summary>
public interface ILogEventQueryRepository
{
    /// <summary>Gets most recent events (newest first).</summary>
    /// <param name="maxRows">Maximum rows to return (1-10000).</param>
    /// <param name="minLevel">Optional minimum level inclusive (0-5).</param>
    /// <param name="contains">Optional substring match applied to Message or Category.</param>
    /// <param name="token">Cancellation token.</param>
    Task<IReadOnlyList<LogEventDto>> GetRecentAsync(int maxRows, byte? minLevel, string? contains, CancellationToken token);
	/// <summary>Gets events since a given UTC timestamp (newest first) limited by <paramref name="maxRows"/>.</summary>
	/// <param name="sinceUtc">Lower bound exclusive UTC timestamp.</param>
	/// <param name="maxRows">Row limit.</param>
	/// <param name="token">Cancellation token.</param>
	Task<IReadOnlyList<LogEventDto>> GetSinceAsync(DateTime sinceUtc, int maxRows, CancellationToken token);
}

/// <summary>
/// SQL Server implementation of <see cref="ILogEventQueryRepository"/>.
/// </summary>
public sealed class LogEventQueryRepository : ILogEventQueryRepository
{
	private readonly ISqlConnectionFactory _factory;
	/// <summary>Create repository.</summary>
	public LogEventQueryRepository(ISqlConnectionFactory factory) => _factory = factory;

	/// <inheritdoc />
	public async Task<IReadOnlyList<LogEventDto>> GetRecentAsync(int maxRows, byte? minLevel, string? contains, CancellationToken token)
	{
		int take = Math.Clamp(maxRows, 1, 10_000);
		var sql = "SELECT TOP (@Top) LogEventId,LogSourceId,Ts,Level,EventId,Category,Message,Session,Host,UserName,AppVersion,ModuleVersion FROM dbo.LogEvent";
		List<string> where = new();
		if (minLevel.HasValue) where.Add("Level >= @MinLevel");
		if (!string.IsNullOrWhiteSpace(contains)) where.Add("(Message LIKE @Contains OR Category LIKE @Contains)");
		if (where.Count > 0) sql += " WHERE " + string.Join(" AND ", where);
		sql += " ORDER BY Ts DESC";
		using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
		using IDbCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		cmd.Parameters.Add(Param(cmd, "@Top", take));
		if (minLevel.HasValue) cmd.Parameters.Add(Param(cmd, "@MinLevel", minLevel.Value));
		if (!string.IsNullOrWhiteSpace(contains)) cmd.Parameters.Add(Param(cmd, "@Contains", "%" + contains + "%"));
		List<LogEventDto> list = new(take);
		using SqlDataReader r = await ((SqlCommand)cmd).ExecuteReaderAsync(token).ConfigureAwait(false);
		while (await r.ReadAsync(token).ConfigureAwait(false))
			list.Add(Map(r));
		return list;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<LogEventDto>> GetSinceAsync(DateTime sinceUtc, int maxRows, CancellationToken token)
	{
		int take = Math.Clamp(maxRows, 1, 10_000);
		const string sql = @"SELECT TOP (@Top) LogEventId,LogSourceId,Ts,Level,EventId,Category,Message,Session,Host,UserName,AppVersion,ModuleVersion 
FROM dbo.LogEvent WHERE Ts > @Since ORDER BY Ts DESC";
		using IDbConnection conn = await _factory.OpenAsync(token).ConfigureAwait(false);
		using IDbCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;
		cmd.Parameters.Add(Param(cmd, "@Top", take));
		cmd.Parameters.Add(Param(cmd, "@Since", sinceUtc));
		List<LogEventDto> list = new(take);
		using SqlDataReader r = await ((SqlCommand)cmd).ExecuteReaderAsync(token).ConfigureAwait(false);
		while (await r.ReadAsync(token).ConfigureAwait(false))
			list.Add(Map(r));
		return list;
	}

	private static LogEventDto Map(SqlDataReader r) => new(
		LogEventId: r.GetInt64(0),
		LogSourceId: r.GetInt32(1),
		Ts: r.GetDateTime(2),
		Level: r.GetByte(3),
		EventId: r.IsDBNull(4) ? null : r.GetInt32(4),
		Category: r.IsDBNull(5) ? null : r.GetString(5),
		Message: r.IsDBNull(6) ? null : r.GetString(6),
		Session: r.IsDBNull(7) ? null : r.GetString(7),
		Host: r.IsDBNull(8) ? null : r.GetString(8),
		UserName: r.IsDBNull(9) ? null : r.GetString(9),
		AppVersion: r.IsDBNull(10) ? null : r.GetString(10),
		ModuleVersion: r.IsDBNull(11) ? null : r.GetString(11));

	private static SqlParameter Param(IDbCommand cmd, string name, object value)
	{
		IDbDataParameter p = cmd.CreateParameter();
		p.ParameterName = name;
		p.Value = value;
		return (SqlParameter)p;
	}
}
