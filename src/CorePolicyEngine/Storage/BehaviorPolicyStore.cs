// Project Name: CorePolicyEngine
// File Name: BehaviorPolicyStore.cs
// Author: Kyle Crowder
// Github:  OldSkoolzRoolz
// License: MIT
// Do not remove file headers

using Microsoft.Data.Sqlite;
using CorePolicyEngine.Models;
using System.Security.Cryptography;
using System.Text;

namespace CorePolicyEngine.Storage;

public interface IBehaviorPolicyStore
{
    Task InitializeAsync(CancellationToken token);
    Task<BehaviorPolicySnapshot> GetSnapshotAsync(CancellationToken token);
    Task UpsertLayerAsync(BehaviorPolicyLayer layer, BehaviorPolicy policy, CancellationToken token);
}

public sealed class BehaviorPolicyStore : IBehaviorPolicyStore
{
    private readonly string _dbPath;

    public BehaviorPolicyStore(string? dbPath = null)
    {
        _dbPath = dbPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "AIManager","client","behavior.db");
    }

    private SqliteConnection Open() { Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!); var c = new SqliteConnection($"Data Source={_dbPath}"); c.Open(); return c; }

    public async Task InitializeAsync(CancellationToken token)
    {
        using var c = Open();
        var cmd = c.CreateCommand();
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS BehaviorPolicyLayer (
Layer INTEGER PRIMARY KEY,
LogRetentionDays INTEGER NOT NULL,
MaxLogFileSizeMB INTEGER NOT NULL,
MinLogLevel TEXT NOT NULL,
UiLanguage TEXT NOT NULL,
EnableTelemetry INTEGER NOT NULL,
PolicyVersion TEXT NOT NULL,
EffectiveUtc TEXT NOT NULL,
AllowedGroupsCsv TEXT NOT NULL DEFAULT 'Administrators',
UpdatedUtc TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS SettingCatalog (
Name TEXT PRIMARY KEY,
DefaultValue TEXT NOT NULL,
RegistryName TEXT NOT NULL,
Scope TEXT NOT NULL,
Type TEXT NOT NULL,
Description TEXT NOT NULL,
Allowed TEXT,
HotReload INTEGER NOT NULL,
RestartRequirement TEXT NOT NULL,
ModelProperty TEXT NOT NULL,
Owner TEXT NOT NULL,
IntroducedVersion TEXT NOT NULL,
LastChangedVersion TEXT NOT NULL,
FutureAdmxPolicyName TEXT,
CreatedUtc TEXT NOT NULL,
UpdatedUtc TEXT NOT NULL
);";
        await cmd.ExecuteNonQueryAsync(token);
        await SeedSettingCatalogAsync(c, token);
    }

    private static async Task SeedSettingCatalogAsync(SqliteConnection c, CancellationToken token)
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var readCmd = c.CreateCommand())
        {
            readCmd.CommandText = "SELECT Name FROM SettingCatalog";
            using var r = await readCmd.ExecuteReaderAsync(token);
            while (await r.ReadAsync(token)) existing.Add(r.GetString(0));
        }
        var now = DateTime.UtcNow.ToString("O");
        var rows = new (string Name,string Default,string Reg,string Scope,string Type,string Desc,string Allowed,int HotReload,string Restart,string Model,string Owner,string Intro,string Last,string Future)[]
        {
            ("LogRetentionDays","7","LogRetentionDays","Machine/User","Int","Number of days to retain log files","1-365",1,"None","LogRetentionDays","Core","0.1.0","0.1.0","AIManager_LogRetentionDays"),
            ("MaxLogFileSizeMB","5","MaxLogFileSizeMB","Machine/User","Int","Max single log file size in MB","1-512",1,"App","MaxLogFileSizeMB","Core","0.1.0","0.1.0","AIManager_MaxLogFileSize"),
            ("MinLogLevel","Information","MinLogLevel","Machine/User","Enum","Minimum log severity emitted","Trace|Debug|Information|Warning|Error|Critical",1,"None","MinLogLevel","Core","0.1.0","0.1.0","AIManager_MinLogLevel"),
            ("UiLanguage","en-US","UiLanguage","Machine/User","Culture","Preferred UI culture","IETF tag",1,"App","UiLanguage","UI","0.1.0","0.1.0","AIManager_UiLanguage"),
            ("EnableTelemetry","0","EnableTelemetry","Machine/User","Bool","Enable anonymous telemetry","0|1",1,"None","EnableTelemetry","Telemetry","0.1.0","0.1.0","AIManager_EnableTelemetry"),
            ("PolicyPollIntervalSeconds","30","PolicyPollIntervalSeconds","Machine/User","Int","Policy polling interval seconds","10-3600",1,"None","PolicyPollIntervalSeconds","Core","0.1.0","0.1.0","AIManager_PolicyPollInterval"),
            ("DatabasePath","%ProgramData%/AIManager/client/behavior.db","DatabasePath","Machine","Path","Behavior policy DB path","Absolute path",0,"App","(N/A)","Core","0.1.0","0.1.0","AIManager_DatabasePath"),
            ("LogsDirectory","<AppBase>/logs","LogsDirectory","Machine/User","Path","Log root directory","Absolute path",0,"App","(N/A)","Core","0.1.0","0.1.0","AIManager_LogsDirectory"),
            ("SessionCorrelationEnabled","1","SessionCorrelationEnabled","Machine/User","Bool","Include session metadata in logs","0|1",1,"None","(N/A)","Core","0.1.0","0.1.0","AIManager_SessionCorrelation"),
            ("AutoPurgeOnStartup","1","AutoPurgeOnStartup","Machine/User","Bool","Run log purge at startup","0|1",1,"None","(N/A)","Core","0.1.0","0.1.0","AIManager_AutoPurge"),
            ("EtwWatcherEnabled","0","EtwWatcherEnabled","Machine/User","Bool","Enable ETW watcher subsystem","0|1",1,"Service","(N/A)","Diagnostics","0.1.0","0.1.0","AIManager_EtwWatcherEnabled"),
            ("EtwWatcherConfigPath","","EtwWatcherConfigPath","Machine/User","Path","Watcher definition bundle path","Absolute path",1,"Service","(N/A)","Diagnostics","0.1.0","0.1.0","AIManager_EtwWatcherConfigPath"),
            ("AllowedGroupsCsv","Administrators","AllowedGroupsCsv","Machine/User","Csv","Semicolon separated allowed Windows groups","Free text",1,"None","AllowedGroupsCsv","Security","0.1.0","0.1.0","AIManager_AllowedGroupsCsv")
        };
        foreach (var row in rows)
        {
            if (existing.Contains(row.Name)) continue;
            using var ins = c.CreateCommand();
            ins.CommandText = @"INSERT INTO SettingCatalog (Name,DefaultValue,RegistryName,Scope,Type,Description,Allowed,HotReload,RestartRequirement,ModelProperty,Owner,IntroducedVersion,LastChangedVersion,FutureAdmxPolicyName,CreatedUtc,UpdatedUtc)
VALUES ($n,$d,$r,$s,$t,$desc,$a,$h,$rst,$m,$o,$iv,$lv,$f,$c,$u);";
            ins.Parameters.AddWithValue("$n", row.Name);
            ins.Parameters.AddWithValue("$d", row.Default);
            ins.Parameters.AddWithValue("$r", row.Reg);
            ins.Parameters.AddWithValue("$s", row.Scope);
            ins.Parameters.AddWithValue("$t", row.Type);
            ins.Parameters.AddWithValue("$desc", row.Desc);
            ins.Parameters.AddWithValue("$a", row.Allowed);
            ins.Parameters.AddWithValue("$h", row.HotReload);
            ins.Parameters.AddWithValue("$rst", row.Restart);
            ins.Parameters.AddWithValue("$m", row.Model);
            ins.Parameters.AddWithValue("$o", row.Owner);
            ins.Parameters.AddWithValue("$iv", row.Intro);
            ins.Parameters.AddWithValue("$lv", row.Last);
            ins.Parameters.AddWithValue("$f", row.Future);
            ins.Parameters.AddWithValue("$c", now);
            ins.Parameters.AddWithValue("$u", now);
            await ins.ExecuteNonQueryAsync(token);
        }
    }

    public async Task UpsertLayerAsync(BehaviorPolicyLayer layer, BehaviorPolicy policy, CancellationToken token)
    {
        using var c = Open();
        var cmd = c.CreateCommand();
        cmd.CommandText = @"INSERT INTO BehaviorPolicyLayer (Layer,LogRetentionDays,MaxLogFileSizeMB,MinLogLevel,UiLanguage,EnableTelemetry,PolicyVersion,EffectiveUtc,AllowedGroupsCsv,UpdatedUtc)
VALUES ($l,$r,$m,$lvl,$lang,$tele,$ver,$eff,$grp,$upd)
ON CONFLICT(Layer) DO UPDATE SET LogRetentionDays=excluded.LogRetentionDays,MaxLogFileSizeMB=excluded.MaxLogFileSizeMB,MinLogLevel=excluded.MinLogLevel,UiLanguage=excluded.UiLanguage,EnableTelemetry=excluded.EnableTelemetry,PolicyVersion=excluded.PolicyVersion,EffectiveUtc=excluded.EffectiveUtc,AllowedGroupsCsv=excluded.AllowedGroupsCsv,UpdatedUtc=excluded.UpdatedUtc;";
        cmd.Parameters.AddWithValue("$l", (int)layer);
        cmd.Parameters.AddWithValue("$r", policy.LogRetentionDays);
        cmd.Parameters.AddWithValue("$m", policy.MaxLogFileSizeMB);
        cmd.Parameters.AddWithValue("$lvl", policy.MinLogLevel);
        cmd.Parameters.AddWithValue("$lang", policy.UiLanguage);
        cmd.Parameters.AddWithValue("$tele", policy.EnableTelemetry ? 1 : 0);
        cmd.Parameters.AddWithValue("$ver", policy.PolicyVersion);
        cmd.Parameters.AddWithValue("$eff", policy.EffectiveUtc.ToString("O"));
        cmd.Parameters.AddWithValue("$grp", policy.AllowedGroupsCsv);
        cmd.Parameters.AddWithValue("$upd", DateTime.UtcNow.ToString("O"));
        await cmd.ExecuteNonQueryAsync(token);
    }

    public async Task<BehaviorPolicySnapshot> GetSnapshotAsync(CancellationToken token)
    {
        using var c = Open();
        var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Layer,LogRetentionDays,MaxLogFileSizeMB,MinLogLevel,UiLanguage,EnableTelemetry,PolicyVersion,EffectiveUtc,AllowedGroupsCsv FROM BehaviorPolicyLayer";
        var dict = new Dictionary<BehaviorPolicyLayer, BehaviorPolicy>();
        using var r = await cmd.ExecuteReaderAsync(token);
        while (await r.ReadAsync(token))
        {
            var layer = (BehaviorPolicyLayer)r.GetInt32(0);
            dict[layer] = new BehaviorPolicy(
                r.GetInt32(1),
                r.GetInt32(2),
                r.GetString(3),
                r.GetString(4),
                r.GetInt32(5) == 1,
                r.GetString(6),
                DateTime.Parse(r.GetString(7)),
                r.GetString(8)
            );
        }
        // Merge with precedence
        BehaviorPolicy effective = BehaviorPolicy.Default with { };
        foreach (var layer in Enum.GetValues<BehaviorPolicyLayer>().OrderBy(l=>l))
        {
            if (dict.TryGetValue(layer, out var p))
            {
                effective = effective with
                {
                    LogRetentionDays = p.LogRetentionDays,
                    MaxLogFileSizeMB = p.MaxLogFileSizeMB,
                    MinLogLevel = p.MinLogLevel,
                    UiLanguage = p.UiLanguage,
                    EnableTelemetry = p.EnableTelemetry,
                    PolicyVersion = p.PolicyVersion,
                    EffectiveUtc = p.EffectiveUtc,
                    AllowedGroupsCsv = p.AllowedGroupsCsv
                };
            }
        }
        string Hash(BehaviorPolicyLayer l) => dict.TryGetValue(l, out var pol) ? ComputeHash(pol) : string.Empty;
        return new BehaviorPolicySnapshot(effective,
            Hash(BehaviorPolicyLayer.LocalDefault),
            Hash(BehaviorPolicyLayer.OrgBaseline),
            Hash(BehaviorPolicyLayer.SiteOverride),
            Hash(BehaviorPolicyLayer.MachineOverride),
            Hash(BehaviorPolicyLayer.UserOverride),
            DateTime.UtcNow);
    }

    private static string ComputeHash(BehaviorPolicy p)
    {
        var raw = $"{p.LogRetentionDays}|{p.MaxLogFileSizeMB}|{p.MinLogLevel}|{p.UiLanguage}|{p.EnableTelemetry}|{p.PolicyVersion}|{p.EffectiveUtc:O}|{p.AllowedGroupsCsv}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }
}
