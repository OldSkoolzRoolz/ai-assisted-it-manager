| ??? **Field**           | **Value**                                         |
|-------------------------|---------------------------------------------------|
| **Date**                | 2025-08-25                                        |
| **Modified By**         | @copilot                                          |
| **Last Modified**       | 2025-08-25                                        |
| **Title**               | *Behavior Settings Registry & ADMX Mapping*       |
| **Author**              | Configuration Team                                |
| **Document ID**         | BEHAV-SET-REF-001                                 |
| **Document Authority**  | @KyleC69                                          |
| **Version**             | 2025-08-25.v3                                     |

---

# Behavior Settings Registry & ADMX Mapping

## 1. Terminology Clarification
| Term | Definition |
|------|------------|
| Policy | An **ADMX/ADML defined Administrative Template policy** (i.e., a configuration item exposed via Group Policy infrastructure, represented in an ADMX XML file, localized by ADML). Each ADMX policy maps to one or more registry value operations in the standard Windows Policies hives. |
| Behavior Setting | An **internal runtime configuration knob** used by the IT Companion client (e.g., logging queue depth). Some of these will be surfaced later *as* ADMX policies by generating an Administrative Template that writes to the same registry value names documented here. |
| Effective Policy | Merged view after layer precedence (LocalDefault < OrgBaseline < SiteOverride < MachineOverride < UserOverride) producing the active BehaviorPolicy record the runtime consumes. |

## 2. Registry Root Conventions
To align with Windows Administrative Template semantics we standardize on the Windows Policies locations (NOT arbitrary custom roots):

- Machine scope: `HKLM\Software\Policies\KCITCompanion\Client`
- User scope:    `HKCU\Software\Policies\KCITCompanion\Client`

(Existing earlier paths such as `HKLM\Software\AIManager\Client\Settings` are superseded by this convention. A backward?compatibility shim can be added later if needed.)

The future ADMX file will define each exposed behavior setting using these same value names so that Group Policy processing and manual registry configuration behave identically.

## 3. Layering & Resolution
1. LocalDefault (DB seeded / first run defaults)
2. OrgBaseline (centrally distributed baseline)
3. SiteOverride
4. MachineOverride (HKLM Policies values ? when surfaced via ADMX or manually configured)
5. UserOverride (HKCU Policies values ? when surfaced via ADMX or manually configured)

Missing values at a higher layer inherit from lower layers. The merged result becomes `BehaviorPolicySnapshot.Effective`.

## 4. Behavior Setting Catalog & Registry Mapping
| Setting (BehaviorPolicy property) | Registry Value Name | Hive Scope(s) | Type | Default | ADMX Exposure | Notes |
|----------------------------------|---------------------|--------------|------|---------|---------------|-------|
| LogRetentionDays | `LogRetentionDays` | HKLM/HKCU | DWORD | 7 | Planned | UI filter horizon (days); does not delete files (view-only until purge implemented). |
| MaxLogFileSizeMB | `MaxLogFileSizeMB` | HKLM/HKCU | DWORD | 5 | Planned | Per-file rotation threshold; applies per module. |
| MinLogLevel | `MinLogLevel` | HKLM/HKCU | STRING | Information | Planned | Accepted: Trace, Debug, Information, Warning, Error, Critical. |
| UiLanguage | `UiLanguage` | HKLM/HKCU | STRING | en-US | Planned | IETF language tag; fallback to en-US. |
| EnableTelemetry | `EnableTelemetry` | HKLM/HKCU | DWORD (0/1) | 0 | Planned | Future remote diagnostics; disabled by default. |
| PolicyVersion | `PolicyVersion` | HKLM/HKCU | STRING | 0.0.0 | Informational | Set by distribution pipeline; not user-edited. |
| EffectiveUtc | `EffectiveUtc` | HKLM/HKCU | STRING (ISO 8601) | (generated) | Not exposed | Timestamp for audit only (written by client). |
| AllowedGroupsCsv | `AllowedGroupsCsv` | HKLM/HKCU | STRING | BUILTIN\Administrators | Planned | Semicolon-delimited list; supports `*` (allow all) for testing. |
| LogViewPollSeconds | `LogViewPollSeconds` | HKLM/HKCU | DWORD | 15 | Planned | UI refresh cadence for log viewer (5–300 clamp). |
| LogQueueMaxDepthPerModule | `LogQueueMaxDepthPerModule` | HKLM/HKCU | DWORD | 5000 | Planned | Bounded per-module queue; overflow triggers failover logging. |
| LogCircuitErrorThreshold | `LogCircuitErrorThreshold` | HKLM/HKCU | DWORD | 25 | Planned | Errors within window to open circuit. |
| LogCircuitErrorWindowSeconds | `LogCircuitErrorWindowSeconds` | HKLM/HKCU | DWORD | 60 | Planned | Sliding window length for circuit error counting. |
| LogFailoverEnabled | `LogFailoverEnabled` | HKLM/HKCU | DWORD (0/1) | 1 | Planned | If disabled, failover file writing suppressed (NOT recommended). |

### Type Notes
- DWORD values stored as unsigned 32-bit integers (range validation applied in runtime before use).
- STRING = REG_SZ.
- All string comparisons are case-insensitive where applicable.

## 5. ADMX Policy Planning
Each *exposed* behavior setting will map 1:1 to an ADMX policy under a proposed category path: `IT Companion Client Configuration`.

### Example ADMX Snippet (Conceptual)
```xml
<policy name="LogRetentionDays" class="Machine" displayName="$(string.LogRetentionDays)" explainText="$(string.LogRetentionDays_Explain)" key="Software\Policies\KCITCompanion\Client" valueName="LogRetentionDays">
  <decimal minValue="1" maxValue="365" />
</policy>
```
User versions (where user-scope makes sense) will duplicate with `class="User"` referencing the same valueName.

### Exposure Strategy
| Setting | Machine | User | Rationale |
|---------|---------|------|-----------|
| LogRetentionDays | Yes | Yes | UI preference may vary per user. |
| MaxLogFileSizeMB | Yes | (Optional) | Usually a machine operational constraint. |
| MinLogLevel | Yes | Yes | User can temporarily elevate logging for session. |
| UiLanguage | Yes | Yes | Per-user localization expected. |
| EnableTelemetry | Yes | Yes | User opt-in + admin control. |
| AllowedGroupsCsv | Yes | No | Access enforced at process start; machine-level security. |
| LogViewPollSeconds | Yes | Yes | UI preference. |
| LogQueueMaxDepthPerModule | Yes | No | Resource protection (machine). |
| LogCircuitErrorThreshold | Yes | No | Operational reliability. |
| LogCircuitErrorWindowSeconds | Yes | No | Operational reliability. |
| LogFailoverEnabled | Yes | No | Operational reliability.

## 6. Validation Rules (Runtime)
| Setting | Validation |
|---------|-----------|
| LogRetentionDays | Clamp 1–365 |
| MaxLogFileSizeMB | Clamp 1–512 (enforced) |
| MinLogLevel | Must parse to Microsoft.Extensions.Logging.LogLevel |
| UiLanguage | Valid CultureInfo; fallback to en-US |
| LogViewPollSeconds | Clamp 5–300 |
| LogQueueMaxDepthPerModule | Clamp 500–50000 |
| LogCircuitErrorThreshold | Clamp 5–500 |
| LogCircuitErrorWindowSeconds | Clamp 10–3600 |
| AllowedGroupsCsv | Split on ';' trim; allow '*' only entry or mixed with named groups |

## 7. Read / Merge Order Implementation Notes
1. Database layers (persisted) combine first.
2. Registry machine HKLM values override missing DB values (intended future step once ADMX deployed).
3. Registry user HKCU values override machine for user-scope allowed settings.
4. Session-only overrides (e.g., developer hot toggles) not persisted.

## 8. Backward Compatibility / Migration
- On upgrade introduce detection of legacy `HKLM\Software\AIManager\Client\Settings` values and migrate once into LocalDefault layer; do not continue reading legacy path after successful migration.
- Write a migration audit event capturing old vs new path mapping.

## 9. Security Considerations
- AllowedGroupsCsv must not silently grant broader access if parse fails—fallback to deny (except development * wildcard explicit).
- Telemetry disabled unless explicitly set to 1 in any resolving layer.
- Failover logging should *never* be disabled in production; ADMX UI description must warn administrators.

## 10. Open Items
| Area | Question | Action |
|------|----------|--------|
| PolicyVersion / EffectiveUtc exposure | Expose read-only in ADMX? | Probably no (informational only). |
| Separate Audit Retention | Distinct from LogRetentionDays? | Add future `AuditRetentionDays`. |
| Per-module MinLogLevel | Needed? | Evaluate after telemetry phase. |

## 11. Example Registry Export
```reg
Windows Registry Editor Version 5.00

[HKEY_LOCAL_MACHINE\Software\Policies\KCITCompanion\Client]
"LogRetentionDays"=dword:00000007
"MaxLogFileSizeMB"=dword:00000005
"MinLogLevel"="Information"
"UiLanguage"="en-US"
"EnableTelemetry"=dword:00000000
"AllowedGroupsCsv"="BUILTIN\\Administrators"
"LogViewPollSeconds"=dword:0000000f
"LogQueueMaxDepthPerModule"=dword:00001388 ; 5000
"LogCircuitErrorThreshold"=dword:00000019   ; 25
"LogCircuitErrorWindowSeconds"=dword:0000003c ; 60
"LogFailoverEnabled"=dword:00000001
```

## 12. Summary
"Policy" in this project = **ADMX administrative policy** (future distributable template). The entries above are **behavior settings** that will be surfaced *as* policies by generating a companion ADMX file targeting the standardized Policies registry roots. This document is the authoritative mapping reference for all collaborators and future ADMX generation tooling.

<!-- End Document -->
