// Project Name: CorePolicyEngine
// File Name: SqlConnectionFactory.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: All Rights Reserved. No use without consent.
// Do not remove file headers


using System.Data;

using Microsoft.Data.SqlClient;


namespace KC.ITCompanion.CorePolicyEngine.Storage.Sql;


public interface ISqlConnectionFactory
{
    Task<IDbConnection> OpenAsync(CancellationToken token);
}



public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _cs;





    public SqlConnectionFactory()
    {
        _cs = Environment.GetEnvironmentVariable("ITC_DB_CONN") ??
                   "Server=(localdb)\\MSSQLLocalDB;Database=ITCompanion;Integrated Security=True;TrustServerCertificate=True;";
    }





    public async Task<IDbConnection> OpenAsync(CancellationToken token)
    {
        var conn = new SqlConnection(_cs);
        await conn.OpenAsync(token).ConfigureAwait(false);
        return conn;
    }
}