| 🗂️ **Field**           | **Value**                               |
|-------------------------|-----------------------------------------|
| **Date**                | 2025-08-25                              |
| **Modified By**         | @copilot                                |
| **Last Modified**       | 2025-08-25                              |
| **Title**               | *Phase 1 Scope and Implementation Plan* |
| **Author**              | Project Planning                        |
| **Document ID**         | PHASE1-PLAN-001                         |
| **Document Authority**  | @KyleC69                                |
| **Version**             | 2025-08-25.v3                           |

---

**Planning reference and versioning**

To add a reference to GPMgmtLib (COM):  
- **Security Requirement:** Needs elevated privileges & domain admin rights.  
- **Versioning:** Persist policies, policy settings, history, and deployment log in a relational store (SQL Server baseline). Define schema objects for PolicyDefinition, PolicyGroup(+Items), Assignments, EffectivePolicy, DriftEvent, AuditEvent.  
- **Rollback:** Restore previous snapshot (captured as versioned policy set) then apply changes and log audit + deployment outcome.  
- **Validation:** Validate against the ADMX schema, check supported OS and `supportedOn` attributes.  
- **Windows Packaging:** Link to packaging.md (avoid inline duplication).  
For logging, use structured JSON rolling files (and summary rows in AuditEvent). Concise but complete entries with correlation ids.

# Phase 1 scope overview

Phase 1 delivers a local-first, admin-grade policy manager: parse and edit ADMX/ADML, validate policies with live feedback, deploy to local machine and (optionally) OU, and provide rollback/version history via the relational policy store. Keep the surface area tight to ship quickly, with clean seams for Phase 2/3 expansion.

---

# Detailed feature list

- **ADMX/ADML parsing and model:**
  - Parse ADMX (definitions) and ADML (localized strings) into a strongly-typed object model.
  - Support categories, policies (boolean, numeric, enum, text), parts/presentations, and supportedOn.
  - Merge ADMX with corresponding ADML culture at runtime.

- **Visual policy editor:**
  - Tree view for categories/policies, detail pane for editing values.
  - Property editors per data type (checkbox, dropdown, numeric, text, enum).
  - XML/source viewer with syntax highlighting and validation badges.

- **Real-time validation:**
  - Schema validation (well-formed XML, required attributes).
  - Semantics (type/range checks, required parts, enum membership).
  - OS support checks using supportedOn and target OS profile.

- **Deployment engine (local + OU preview):**
  - Apply settings to local machine/user policy registry hives.
  - Export/import policy sets to portable artifacts (`.policy.json` + optional `.reg`).
  - Optional domain GPO push via GPMC COM when RSAT/GPMC present.

- **Versioning and rollback:**
  - Relational-backed version history (PolicySetVersion + diff materialization service).
  - One-click rollback to any prior snapshot.
  - Deployment log with outcome, duration, and changed keys summary.

- **Packaging and elevation:**
  - Packaged app (MSIX – future), broker / elevated helper for privileged ops.
  - Admin check and UAC prompt for deployment operations.

- **Diagnostics and logging:**
  - Structured JSON rolling files.
  - Policy validation errors/warnings surfaced inline and in a Problems pane.

---

# Implementation plan by feature

## ADMX/ADML parsing and model
1. **Domain model scaffolding:**
   - Classes: AdmxCatalog, AdmxCategory, AdmxPolicy, AdmxPresentation, AdmxPart, AdmxSupportedOn, AdmlStrings.
2. **File discovery:**
   - Search standard & custom PolicyDefinitions paths.
3. **XML loaders:** Map definitions + strings.
4. **Merge layer:** Bind ADMX references to ADML resources; graceful fallback.
5. **Validation pass:** Collect errors/warnings.
6. **Caching:** In-memory cache keyed by file set + culture.

Acceptance: Load base Windows ADMX set successfully; tree populated; zero false positives on known-good files.

## Visual policy editor
1. Tree + Details + Source + Validation tabs.
2. Data templates: Boolean, Enum, Numeric (range), Text.
3. Two-way binding with inline errors.
4. Source viewer (read-only) with highlighting.
5. Search / filter with fuzzy substring.
6. Dirty tracking & Apply/Revert.

Acceptance: Edit multiple policy types; validation immediate; revert and apply behave correctly.

## Real-time validation
1. Rule interfaces (IValidationRule). 
2. Rules: Type, Range, Enum, Required, SupportedOn.
3. OS profile model feeding SupportedOnRule.
4. Execute on change; aggregate list.

Acceptance: Invalid values flagged; unsupported policies surfaced; navigation from Problems pane.

## Deployment engine (local + OU preview)
1. Registry writers (HKLM/HKCU Software\Policies vendor path).
2. Dry-run diff (current vs desired) with structured output.
3. Transaction model + rollback token.
4. GPO abstraction (preview). Capability detection before GPMC operations.
5. Import/export pipeline.

Acceptance: Dry-run accurate; apply modifies registry; preview OU operations gated by capability.

## Versioning and rollback
1. Relational schema: PolicySet, PolicySetVersion, PolicySetItem, DeploymentLog.
2. Snapshot capture + diff computation (added/changed/removed).
3. Rollback uses prior snapshot -> deployment pipeline.
4. Optional labels / notes per version.

Acceptance: Each apply creates version; rollback restores previous state; diffs human-readable.

## Packaging and elevation
1. Broker process / service boundary design.
2. Elevation strategy documented; enforcement for privileged writes.
3. Status bar for environment & capability flags (admin, domain, RSAT, broker).

## Diagnostics and logging
1. Rolling JSON logs + retention purge.
2. Correlation id across UI action → deploy pipeline.
3. Log viewer (filter by level / text).
4. Diagnostic bundle exporter (logs + recent deployment transcript).

---

# Data model and formats

## Core relational schema (conceptual)
```sql
-- Policy sets & versions
CREATE TABLE PolicySet (
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  Name NVARCHAR(256) NOT NULL,
  TargetScope NVARCHAR(64) NOT NULL, -- LocalMachine|CurrentUser|Gpo:{Guid}
  CreatedUtc DATETIME2 NOT NULL,
  CreatedBy NVARCHAR(128) NULL,
  Notes NVARCHAR(1024) NULL
);

CREATE TABLE PolicySetVersion (
  PolicySetVersionId UNIQUEIDENTIFIER PRIMARY KEY,
  PolicySetId UNIQUEIDENTIFIER NOT NULL,
  VersionNumber INT NOT NULL,
  CreatedUtc DATETIME2 NOT NULL,
  CreatedBy NVARCHAR(128) NULL,
  SnapshotJson NVARCHAR(MAX) NOT NULL,
  Message NVARCHAR(512) NULL,
  FOREIGN KEY (PolicySetId) REFERENCES PolicySet(Id)
);

CREATE TABLE PolicySetItem (
  PolicySetItemId UNIQUEIDENTIFIER PRIMARY KEY,
  PolicySetVersionId UNIQUEIDENTIFIER NOT NULL,
  PolicyKey NVARCHAR(512) NOT NULL,
  PartKey NVARCHAR(256) NULL,
  Value NVARCHAR(MAX) NULL,
  ValueType NVARCHAR(32) NOT NULL,
  Enabled BIT NOT NULL,
  FOREIGN KEY (PolicySetVersionId) REFERENCES PolicySetVersion(PolicySetVersionId)
);

CREATE TABLE DeploymentLog (
  DeploymentLogId UNIQUEIDENTIFIER PRIMARY KEY,
  PolicySetId UNIQUEIDENTIFIER NOT NULL,
  VersionNumber INT NOT NULL,
  Target NVARCHAR(128) NOT NULL,
  StartedUtc DATETIME2 NOT NULL,
  EndedUtc DATETIME2 NOT NULL,
  Status NVARCHAR(16) NOT NULL, -- Success|Failed|Partial
  DiffJson NVARCHAR(MAX) NOT NULL,
  Error NVARCHAR(MAX) NULL,
  FOREIGN KEY (PolicySetId) REFERENCES PolicySet(Id)
);
```

## Internal policy JSON (export)
```json
{
  "name": "Baseline - Workstations",
  "targetScope": "LocalMachine",
  "osProfile": "Windows 11 23H2",
  "settings": [
    { "policyId": "Computer\\Microsoft Defender Antivirus\\Turn off Microsoft Defender Antivirus", "enabled": false, "valueType": "Boolean", "value": "false" },
    { "policyId": "Computer\\Removable Storage Access\\All Removable Storage classes: Deny all access", "enabled": true, "valueType": "Boolean", "value": "true" }
  ]
}
```

---

# Repository and solution structure (current intent)
```
/src
  /ClientApp                 # WPF UI
  /CorePolicyEngine          # Parsing, validation, policy evaluation & services
  /Security                  # AuthZ/AuthN & integrity (future)
  /ITCompanionDB             # Database project (T-SQL schema & sprocs)
  /EnterpriseDashboard       # Blazor (future phase)
  /Package                   # MSIX packaging (future)
/docs
```

- **Boundaries:** UI isolated from privileged operations; future broker enforces elevation boundary.
- **Contracts:** Interfaces concentrate in CorePolicyEngine; concrete SQL / infra hidden behind repositories/services.

---

# Milestones, acceptance criteria, and risks

(Milestones retained; wording updated for provider-agnostic persistence.)

## Milestone 1: Parsers + basic editor (2 weeks)
Deliverables: Catalog load/merge, base tree, editors, localized strings.  
Acceptance: 20+ policies view/edit; validation baseline.

## Milestone 2: Validation + search + source view (1 week)
Deliverables: Rules engine, OS profile, Problems pane, XML highlight, global search.  
Acceptance: Unsupported flagged; navigation works.

## Milestone 3: Local deployment + versioning (2 weeks)
Deliverables: Registry writer, dry-run diff, version snapshot tables, rollback path.  
Acceptance: Accurate diff; rollback restores prior state.

## Milestone 4: Packaging + diagnostics (1 week)
Deliverables: Packaging scaffolding, logging viewer, diagnostic export.  
Acceptance: Install & run; logs captured for success & failure.

## Stretch: OU/GPO writer (1 week)
Deliverables: GPO abstraction & capability detection.  
Acceptance: Test GPO created when environment supports it.

## Key risks / mitigations
- **COM GPMC availability:** Capability check & fall back to local deployment only.
- **Elevation in packaged app:** Broker/service; no direct UI elevation.
- **Schema drift:** Database project (DACPAC) + documented migration practice.
- **Large ADMX sets:** Caching & lazy materialization.

---

# Change log (Phase 1 doc updates)
- Replaced SQLite-specific design with provider-agnostic relational model (SQL Server baseline).
- Added conceptual relational schema for versioning artifacts.
- Streamlined milestones to reflect current persistence direction.

<!-- End Document -->
