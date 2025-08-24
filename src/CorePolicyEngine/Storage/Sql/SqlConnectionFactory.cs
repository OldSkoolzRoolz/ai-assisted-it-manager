// Project Name: CorePolicyEngine
// File Name: SqlConnectionFactory.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers

using System.Data;
using Microsoft.Data.SqlClient;

namespace KC.ITCompanion.CorePolicyEngine.Storage.Sql;

/// <summary>
/// Factory abstraction for opening SQL connections.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Opens a new open connection.
    /// </summary>
    Task<IDbConnection> OpenAsync(CancellationToken token);
}

/// <summary>
/// Default implementation using connection string in environment variable ITC_SQL or localdb fallback.
/// </summary>
public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connString;

    /// <summary>
    /// Creates factory using environment or fallback localdb.
    /// </summary>
    public SqlConnectionFactory()
    {
        _connString = Environment.GetEnvironmentVariable("ITC_SQL") ??
                      "Server=(localdb)\\MSSQLLocalDB;Database=ITCompanion;Trusted_Connection=True;Encrypt=True;";
    }

    /// <inheritdoc/>
    public async Task<IDbConnection> OpenAsync(CancellationToken token)
    {
        var conn = new SqlConnection(_connString);
        await conn.OpenAsync(token).ConfigureAwait(false);
        return conn;
    }
}