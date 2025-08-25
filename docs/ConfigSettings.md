| ??? **Field**           | **Value**                                   |
|-------------------------|---------------------------------------------|
| **Date**                | 2025-08-25                                  |
| **Modified By**         | @copilot                                    |
| **Last Modified**       | 2025-08-25                                  |
| **Title**               | *Client Application Configurable Settings*  |
| **Author**              | Configuration Team                          |
| **Document ID**         | CFG-SET-REF-001                             |
| **Document Authority**  | @KyleC69                                    |
| **Version**             | 2025-08-25.v3                               |

---

# Client Application Configurable Settings

Scope convention:
- Machine-wide defaults: `HKLM\Software\AIManager\Client\Settings`
- User overrides: `HKCU\Software\AIManager\Client\Settings`
- If both exist, user scope overrides machine scope. Behavior policy layering (DB) then applies in order: LocalDefault < OrgBaseline < SiteOverride < MachineOverride < UserOverride.

Unless otherwise noted, values are read at startup and when the BehaviorPolicyProvider detects a change.

## Quick View
| Setting Name | Default Value | Registry Value Name | Type | Description | Notes |
|--------------|---------------|---------------------|------|-------------|-------|
| LogRetentionDays | 7 | `LogRetentionDays` | DWORD | Number of days to retain log files before deletion. | Policy layer may override; minimum enforced = 1. |
| MaxLogFileSizeMB | 5 | `MaxLogFileSizeMB` | DWORD | Maximum size (MB) for a single rolling log file before a new file is created. | Hard cap in code; must be >0. |
| MinLogLevel | Information | `MinLogLevel` | STRING | Minimum log level emitted (Trace, Debug, Information, Warning, Error, Critical). | Affects all sinks; dynamic updates applied. |
| UiLanguage | en-US | `UiLanguage` | STRING | Preferred UI culture (IETF language tag). | Falling back to en-US if resources unavailable. |
| EnableTelemetry | 0 (false) | `EnableTelemetry` | DWORD | Enables anonymous diagnostic telemetry when set to 1. | Future enterprise aggregation. |
| PolicyVersion | 0.0.0 | `PolicyVersion` | STRING | Version string of applied behavior policy layer. | Set by policy distribution; not user-edited normally. |
| EffectiveUtc | (current UTC at creation) | `EffectiveUtc` | STRING (ISO 8601) | Timestamp the current behavior policy became effective. | For audit; informational. |
| PolicyPollIntervalSeconds | 30 | `PolicyPollIntervalSeconds` | DWORD | Interval (seconds) BehaviorPolicyProvider polls for changes. | Values < 10 forced to 10. |
| DatabasePath | %ProgramData%\AIManager\client\behavior.db | `DatabasePath` | STRING | Location of the behavior policy SQLite database. | Machine scope only; user override ignored. |
| LogsDirectory | <AppBase>\logs | `LogsDirectory` | STRING | Root directory for JSON log files. | Changing requires restart. |
| SessionCorrelationEnabled | 1 (true) | `SessionCorrelationEnabled` | DWORD | Enables inclusion of session, host, process and module version metadata in logs. | Disable only for privacy-constrained environments. |
| AutoPurgeOnStartup | 1 (true) | `AutoPurgeOnStartup` | DWORD | Run log retention purge immediately at startup. | If disabled, purge runs on first scheduled interval. |
| TelemetryEndpoint | (empty) | `TelemetryEndpoint` | STRING | Custom endpoint for telemetry submission. | Empty disables remote send even if telemetry enabled. |
| EtwWatcherEnabled | 0 (false) | `EtwWatcherEnabled` | DWORD | Enables ETW watcher subsystem (future feature). | Will activate ETW watcher service when implemented. |
| EtwWatcherConfigPath | (empty) | `EtwWatcherConfigPath` | STRING | Path to watcher definition bundle (JSON/ signed). | Future feature placeholder. |

## Detailed Matrix
| Setting | ModelProperty | RegistryName | Scope | Type | Default | Allowed | HotReload | Restart | ValidationRuleId | FutureAdmxPolicyName | IntroducedVersion | Owner | SecurityNote |
|---------|---------------|--------------|-------|------|---------|---------|-----------|---------|------------------|----------------------|-------------------|-------|-------------|
| LogRetentionDays | LogRetentionDays | LogRetentionDays | Machine/User | Int | 7 | 1-365 | Yes | None | VR_LOG_RETENTION_RANGE | AIManager_LogRetentionDays | 0.1.0 | Core | Prevent extremely low values that remove audit trail |
| MaxLogFileSizeMB | MaxLogFileSizeMB | MaxLogFileSizeMB | Machine/User | Int | 5 | 1-512 | Yes | App | VR_MAX_LOG_FILE_SIZE | AIManager_MaxLogFileSize | 0.1.0 | Core | Large values increase disk usage |
| MinLogLevel | MinLogLevel | MinLogLevel | Machine/User | Enum(LogLevel) | Information | Trace|Debug|Information|Warning|Error|Critical | Yes | None | VR_LOGLEVEL_ENUM | AIManager_MinLogLevel | 0.1.0 | Core | Lowering may reduce diagnostic visibility |
| UiLanguage | UiLanguage | UiLanguage | Machine/User | Culture | en-US | Valid culture tags | Partial | App | VR_CULTURE_TAG | AIManager_UiLanguage | 0.1.0 | UI | Invalid culture falls back to en-US |
| EnableTelemetry | EnableTelemetry | EnableTelemetry | Machine/User | Bool | 0 | 0|1 | Yes | None | VR_BOOL | AIManager_EnableTelemetry | 0.1.0 | Telemetry | Ensure disclosure & consent |
| PolicyPollIntervalSeconds | PolicyPollIntervalSeconds | PolicyPollIntervalSeconds | Machine/User | Int | 30 | 10-3600 | Yes | None | VR_POLL_INTERVAL_RANGE | AIManager_PolicyPollInterval | 0.1.0 | Core | Too low increases load |
| DatabasePath | (N/A) | DatabasePath | Machine | Path | %ProgramData%/AIManager/client/behavior.db | Absolute path | No | App | VR_PATH_ABSOLUTE | AIManager_DatabasePath | 0.1.0 | Core | Controlled by installer/admin |
| LogsDirectory | (N/A) | LogsDirectory | Machine/User | Path | <AppBase>/logs | Absolute path | No | App | VR_PATH_ABSOLUTE | AIManager_LogsDirectory | 0.1.0 | Core | Ensure ACLs protect contents |
| SessionCorrelationEnabled | (N/A) | SessionCorrelationEnabled | Machine/User | Bool | 1 | 0|1 | Yes | None | VR_BOOL | AIManager_SessionCorrelation | 0.1.0 | Core | Disabling reduces forensic context |
| AutoPurgeOnStartup | (N/A) | AutoPurgeOnStartup | Machine/User | Bool | 1 | 0|1 | Yes | None | VR_BOOL | AIManager_AutoPurge | 0.1.0 | Core | If disabled, log growth until scheduled purge |
| EtwWatcherEnabled | (N/A) | EtwWatcherEnabled | Machine/User | Bool | 0 | 0|1 | Planned | Service | VR_BOOL | AIManager_EtwWatcherEnabled | 0.1.0 | Diagnostics | May surface sensitive event data |
| EtwWatcherConfigPath | (N/A) | EtwWatcherConfigPath | Machine/User | Path | (empty) | Absolute path | Planned | Service | VR_PATH_ABSOLUTE | AIManager_EtwWatcherConfigPath | 0.1.0 | Diagnostics | Validate source authenticity |
| TelemetryEndpoint | (N/A) | TelemetryEndpoint | Machine/User | Url | (empty) | Valid URI/empty | Yes | None | VR_URI_OPTIONAL | AIManager_TelemetryEndpoint | 0.1.0 | Telemetry | Endpoint must be trusted |

Legend: HotReload: Yes=applies immediately; Partial=some UI elements may require refresh; Planned=future feature.

## Behavior Policy Layer Storage
Behavior policy authoritative values are persisted in the SQLite database (table `BehaviorPolicyLayer`) and *may* be hydrated from registry on first run to create the LocalDefault layer. Subsequent changes should be made through the policy management UI or distribution mechanism, which updates the DB and (optionally) writes user/machine override hints to registry.

## Resolution Order
1. Database layers merged (LocalDefault ? UserOverride)
2. Registry machine scope overrides missing database values (initial seeding only)
3. Registry user scope overrides missing database values (initial seeding only)
4. In-memory overrides (temporary session adjustments) ? not persisted

## Registry Seeding Strategy
- On first launch, if DB empty: read any present registry values to build LocalDefault layer then persist.
- After DB initialized, registry writes are optional hints only; DB is source of truth.

## Validation Rules
| Setting | Validation |
|---------|------------|
| LogRetentionDays | Clamp to [1, 365] |
| MaxLogFileSizeMB | Clamp to [1, 512] |
| MinLogLevel | Must parse to Microsoft.Extensions.Logging.LogLevel (case-insensitive) |
| UiLanguage | Must be valid culture; fallback to en-US |
| PolicyPollIntervalSeconds | Clamp to [10, 3600] |
| DatabasePath | Must be absolute, writable, directory must exist or be creatable |
| LogsDirectory | Must be absolute & writable |

## Future Extensions (Planned)
- Add cryptographic signature fields: `PolicySignature`, `PolicySigner` for distributed policies.
- Introduce `AnomalyDetectionEnabled` and thresholds feeding AI Companion.
- ETW Watcher definitions persisted (table `BehaviorEtwWatcher`) referencing dynamic action catalog.

## Example Machine Scope Registry Export (.reg)
```
Windows Registry Editor Version 5.00

[HKEY_LOCAL_MACHINE\Software\AIManager\Client\Settings]
"LogRetentionDays"=dword:00000007
"MaxLogFileSizeMB"=dword:00000005
"MinLogLevel"="Information"
"UiLanguage"="en-US"
"EnableTelemetry"=dword:00000000
"PolicyPollIntervalSeconds"=dword:0000001e
"DatabasePath"="C:\\ProgramData\\AIManager\\client\\behavior.db"
"LogsDirectory"="C:\\ProgramData\\AIManager\\client\\logs"
"SessionCorrelationEnabled"=dword:00000001
"AutoPurgeOnStartup"=dword:00000001
"EtwWatcherEnabled"=dword:00000000
```

## Notes
- Settings not present fall back to defaults defined in `BehaviorPolicy.Default` or hard-coded operational defaults.
- Changing `UiLanguage` at runtime triggers UI resource reload on next refresh cycle (planned hook). Restart may be required for certain views.
- High-frequency polling can increase I/O; keep interval >= 30s unless actively testing distribution.

<!-- End Document -->
