-- Post-deployment seed data & idempotent inserts
-- NOTE: Wrapped DML in object existence guards to satisfy build-time validation.

PRINT 'Seeding schema version';
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Meta' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    UPDATE dbo.Meta SET [Value] = '1' WHERE [Key] = 'SchemaVersion';
END;
GO

-- Seed SettingCatalog initial rows (idempotent) (template retained for future use)
-- To enable, remove comment markers.
--MERGE [dbo].[SettingCatalog] AS tgt
--USING (VALUES
-- ('LogRetentionDays','7','LogRetentionDays','Machine/User','Int','Number of days to retain log files','1-365',1,'None','LogRetentionDays','Core','0.1.0','0.1.0','AIManager_LogRetentionDays'),
-- ('MaxLogFileSizeMB','5','MaxLogFileSizeMB','Machine/User','Int','Max single log file size in MB','1-512',1,'App','MaxLogFileSizeMB','Core','0.1.0','0.1.0','AIManager_MaxLogFileSize'),
-- ('MinLogLevel','Information','MinLogLevel','Machine/User','Enum','Minimum log severity emitted','Trace|Debug|Information|Warning|Error|Critical',1,'None','MinLogLevel','Core','0.1.0','0.1.0','AIManager_MinLogLevel'),
-- ('UiLanguage','en-US','UiLanguage','Machine/User','Culture','Preferred UI culture','IETF tag',1,'App','UiLanguage','UI','0.1.0','0.1.0','AIManager_UiLanguage'),
-- ('EnableTelemetry','0','EnableTelemetry','Machine/User','Bool','Enable anonymous telemetry','0|1',1,'None','EnableTelemetry','Telemetry','0.1.0','0.1.0','AIManager_EnableTelemetry'),
-- ('PolicyPollIntervalSeconds','30','PolicyPollIntervalSeconds','Machine/User','Int','Policy polling interval seconds','10-3600',1,'None','PolicyPollIntervalSeconds','Core','0.1.0','0.1.0','AIManager_PolicyPollInterval'),
-- ('DatabasePath','%ProgramData%/AIManager/client/behavior.db','DatabasePath','Machine','Path','Behavior policy DB path','Absolute path',0,'App','(N/A)','Core','0.1.0','0.1.0','AIManager_DatabasePath'),
-- ('LogsDirectory','<AppBase>/logs','LogsDirectory','Machine/User','Path','Log root directory','Absolute path',0,'App','(N/A)','Core','0.1.0','0.1.0','AIManager_LogsDirectory'),
-- ('SessionCorrelationEnabled','1','SessionCorrelationEnabled','Machine/User','Bool','Include session metadata in logs','0|1',1,'None','(N/A)','Core','0.1.0','0.1.0','AIManager_SessionCorrelation'),
-- ('AutoPurgeOnStartup','1','AutoPurgeOnStartup','Machine/User','Bool','Run log purge at startup','0|1',1,'None','(N/A)','Core','0.1.0','0.1.0','AIManager_AutoPurge'),
-- ('EtwWatcherEnabled','0','EtwWatcherEnabled','Machine/User','Bool','Enable ETW watcher subsystem','0|1',1,'Service','(N/A)','Diagnostics','0.1.0','0.1.0','AIManager_EtwWatcherEnabled'),
-- ('EtwWatcherConfigPath','','EtwWatcherConfigPath','Machine/User','Path','Watcher definition bundle path','Absolute path',1,'Service','(N/A)','Diagnostics','0.1.0','0.1.0','AIManager_EtwWatcherConfigPath'),
-- ('AllowedGroupsCsv','Administrators','AllowedGroupsCsv','Machine/User','Csv','Semicolon separated allowed Windows groups','Free text',1,'None','AllowedGroupsCsv','Security','0.1.0','0.1.0','AIManager_AllowedGroupsCsv'),
-- ('LogViewPollSeconds','15','LogViewPollSeconds','Machine/User','Int','UI log view refresh interval seconds','5-300',1,'None','LogViewPollSeconds','Core','0.1.1','0.1.1','AIManager_LogViewPollSeconds')
--) AS src(Name,DefaultValue,RegistryName,Scope,Type,Description,Allowed,HotReload,RestartRequirement,ModelProperty,Owner,IntroducedVersion,LastChangedVersion,FutureAdmxPolicyName)
--ON tgt.Name = src.Name
--WHEN MATCHED THEN UPDATE SET DefaultValue=src.DefaultValue, RegistryName=src.RegistryName, Scope=src.Scope, Type=src.Type, Description=src.Description, Allowed=src.Allowed, HotReload=src.HotReload, RestartRequirement=src.RestartRequirement, ModelProperty=src.ModelProperty, Owner=src.Owner, IntroducedVersion=src.IntroducedVersion, LastChangedVersion=src.LastChangedVersion, FutureAdmxPolicyName=src.FutureAdmxPolicyName, UpdatedUtc=SYSUTCDATETIME()
--WHEN NOT MATCHED THEN INSERT (Name,DefaultValue,RegistryName,Scope,Type,Description,Allowed,HotReload,RestartRequirement,ModelProperty,Owner,IntroducedVersion,LastChangedVersion,FutureAdmxPolicyName,CreatedUtc,UpdatedUtc)
--VALUES (src.Name,src.DefaultValue,src.RegistryName,src.Scope,src.Type,src.Description,src.Allowed,src.HotReload,src.RestartRequirement,src.ModelProperty,src.Owner,src.IntroducedVersion,src.LastChangedVersion,src.FutureAdmxPolicyName,SYSUTCDATETIME(),SYSUTCDATETIME());
--GO

PRINT 'Seeding default behavior policy layer (LocalDefault) if empty';
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BehaviorPolicyLayer' AND schema_id = SCHEMA_ID('dbo'))
AND NOT EXISTS (SELECT 1 FROM dbo.BehaviorPolicyLayer)
BEGIN
    INSERT INTO dbo.BehaviorPolicyLayer
        (Layer, LogRetentionDays, MaxLogFileSizeMB, MinLogLevel, UiLanguage, EnableTelemetry, PolicyVersion, EffectiveUtc, AllowedGroupsCsv, LogViewPollSeconds, UpdatedUtc)
    VALUES
        (0, 7, 5, 'Information', 'en-US', 0, '0.0.0', SYSUTCDATETIME(), 'Administrators', 15, SYSUTCDATETIME());
END;
GO

PRINT 'Seeding default audit retention if empty';
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditRetention' AND schema_id = SCHEMA_ID('dbo'))
AND NOT EXISTS (SELECT 1 FROM dbo.AuditRetention)
BEGIN
    INSERT INTO dbo.AuditRetention (RetentionDays, CreatedUtc)
    VALUES (365, SYSUTCDATETIME());
END;
GO

PRINT 'Seeding sample log sources';
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'LogSource' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    MERGE dbo.LogSource AS tgt
    USING (VALUES
        ('ClientApp','C:\\ProgramData\\AIManager\\client\\logs\\app-*.log'),
        ('CorePolicyEngine','C:\\ProgramData\\AIManager\\client\\logs\\core-*.log')
    ) AS src(Application, FilePath)
    ON tgt.FilePath = src.FilePath
    WHEN MATCHED THEN UPDATE SET Application = src.Application, UpdatedUtc = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN INSERT (Application, FilePath, Enabled, CreatedUtc, UpdatedUtc)
        VALUES (src.Application, src.FilePath, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END;
GO
