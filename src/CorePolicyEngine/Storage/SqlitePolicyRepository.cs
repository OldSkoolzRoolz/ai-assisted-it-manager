using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Shared;

namespace CorePolicyEngine.Storage;

public sealed class SqlitePolicyRepository : IPolicyRepository
{
    private readonly string _dbPath;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public SqlitePolicyRepository(string dbPath)
    {
        if (string.IsNullOrWhiteSpace(dbPath)) throw new ArgumentException("dbPath required", nameof(dbPath));
        _dbPath = dbPath;
    }

    private SqliteConnection Open()
    {
        var first = !File.Exists(_dbPath);
        var conn = new SqliteConnection($"Data Source={_dbPath};Cache=Shared");
        conn.Open();
        if (first) InitializeSchema(conn);
        return conn;
    }

    private void InitializeSchema(SqliteConnection conn)
    {
        const string ddl = @"CREATE TABLE IF NOT EXISTS PolicySet (Id TEXT PRIMARY KEY, Name TEXT NOT NULL, TargetScope TEXT NOT NULL, Json TEXT NOT NULL);";
        using var cmd = conn.CreateCommand();
        cmd.CommandText = ddl;
        cmd.ExecuteNonQuery();
    }

    public async Task<Result<PolicySet>> GetAsync(string id, CancellationToken cancellationToken)
    {
        await using var conn = Open();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Json FROM PolicySet WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        var scalar = await cmd.ExecuteScalarAsync(cancellationToken);
        if (scalar is null) return Result<PolicySet>.Fail("Not found");
        var json = (string)scalar;
        var model = JsonSerializer.Deserialize<PolicySet>(json, _jsonOptions);
        if (model == null) return Result<PolicySet>.Fail("Corrupt record");
        return Result<PolicySet>.Ok(model);
    }

    public async Task<Result> SaveAsync(PolicySet set, CancellationToken cancellationToken)
    {
        await using var conn = Open();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO PolicySet(Id,Name,TargetScope,Json) VALUES(@id,@n,@t,@j) ON CONFLICT(Id) DO UPDATE SET Name=excluded.Name, TargetScope=excluded.TargetScope, Json=excluded.Json;";
        cmd.Parameters.AddWithValue("@id", set.Id);
        cmd.Parameters.AddWithValue("@n", set.Name);
        cmd.Parameters.AddWithValue("@t", set.TargetScope);
        var json = JsonSerializer.Serialize(set, _jsonOptions);
        cmd.Parameters.AddWithValue("@j", json);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return Result.Ok();
    }
}
