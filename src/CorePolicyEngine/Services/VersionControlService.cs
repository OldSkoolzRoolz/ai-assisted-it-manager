using System.Collections.Concurrent;

using Microsoft.Data.Sqlite;

namespace CorePolicyEngine.Services;

public readonly record struct PolicyVersion(int Version, string Content);

public interface IVersionControlService
{
    int AddVersion(string policyName, string content);
    PolicyVersion? GetLatest(string policyName);
}

public sealed class VersionControlService : IVersionControlService
{
    private readonly ConcurrentDictionary<string, List<PolicyVersion>> _store = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _connectionString;

    public VersionControlService()
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "policy.db");
        _connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
        Initialize();
    }

    private void Initialize()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        SqliteCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS PolicyHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PolicyName TEXT NOT NULL,
                Version INTEGER NOT NULL,
                Content TEXT NOT NULL,
                CreatedUtc TEXT NOT NULL
            );";
        cmd.ExecuteNonQuery();
    }

    public int AddVersion(string policyName, string content)
    {
        List<PolicyVersion> list = _store.GetOrAdd(policyName, _ => []);
        var next = list.LastOrDefault().Version + 1;
        var pv = new PolicyVersion(next, content);
        list.Add(pv);

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using SqliteTransaction tx = conn.BeginTransaction();

        SqliteCommand insert = conn.CreateCommand();
        insert.CommandText = @"INSERT INTO PolicyHistory (PolicyName, Version, Content, CreatedUtc)
                               VALUES ($name, $ver, $content, $utc)";
        insert.Parameters.AddWithValue("$name", policyName);
        insert.Parameters.AddWithValue("$ver", next);
        insert.Parameters.AddWithValue("$content", content);
        insert.Parameters.AddWithValue("$utc", DateTime.UtcNow.ToString("o"));
        insert.ExecuteNonQuery();

        tx.Commit();
        return next;
    }

    public PolicyVersion? GetLatest(string policyName)
    {
        return _store.TryGetValue(policyName, out List<PolicyVersion>? list) && list.Count > 0 ? list[^1] : null;
    }
}
