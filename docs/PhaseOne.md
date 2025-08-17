**Planning reference and versioning**

To add a reference to GPMgmtLib (COM):  
- **Security Requirement:** Needs elevated privileges & domain admin rights.  
- **Versioning:** Create SQLite tables for Policies, PolicySettings, PolicyHistory, and DeploymentLog. Set up schema for these.  
- **Rollback:** Restore previous snapshot, apply changes, and write logs.  
- **Validation:** Validate against the ADMX schema, check supported OS and "supportedOn" attributes.  
- **Windows Packaging:** Link to packaging.md, but without citing.  
For logging, use ETW or Serilog with a rolling file. We’ll keep things concise but detailed in the markdown format, with order and bold headings, keeping examples succinct.
# Phase 1 scope overview

Phase 1 delivers a local-first, admin-grade policy manager: parse and edit ADMX/ADML, validate policies with live feedback, deploy to local machine and (optionally) OU, and provide rollback/version history via SQLite. Keep the surface area tight to ship quickly, with clean seams for Phase 2/3 expansion.

---

# Detailed feature list

- **ADMX/ADML parsing and model:**
  - Parse ADMX (definitions) and ADML (localized strings) into a strongly-typed object model.
  - Support categories, policies (boolean, numeric, enum, text), parts/presentations, and supportedOn.
  - Merge ADMX with corresponding ADML culture at runtime.

- **Visual policy editor:**
  - Tree view for categories/policies, detail pane for editing values.
  - Property editors for each data type (checkbox, dropdown, numeric with ranges, text with masks).
  - XML/source viewer with syntax highlighting and validation badges.

- **Real-time validation:**
  - Schema validation (well-formed XML, required attributes).
  - Semantics (type/range checks, required parts, enum membership).
  - OS support checks using supportedOn and target OS profile.

- **Deployment engine (local + OU preview):**
  - Apply settings to local machine/user policy registry hives.
  - Export/import policy sets to portable artifacts (.policy.json + .reg).
  - Optional domain GPO push via GPMC COM when RSAT/GPMC present.

- **Versioning and rollback:**
  - SQLite-backed version history with diffs, labels, and timestamps.
  - One-click rollback to any prior snapshot.
  - Deployment log with outcome, duration, and changed keys.

- **Packaging and elevation:**
  - Packaged WinUI 3 app (MSIX) with requestedExecutionLevel=highestAvailable behavior via brokered actions.
  - Admin check and UAC prompt for deployment operations.

- **Diagnostics and logging:**
  - Structured logs to rolling files (and SQLite summary).
  - Policy validation errors/warnings surfaced inline and in a Problems pane.

---

# Implementation plan by feature

## ADMX/ADML parsing and model

1. **Domain model scaffolding:**
   - Define classes: AdmxCatalog, AdmxCategory, AdmxPolicy, AdmxPresentation, AdmxPart, AdmxSupportedOn, AdmlStrings.
2. **File discovery:**
   - Implement search paths for ADMX/ADML (e.g., C:\Windows\PolicyDefinitions and custom folder).
   - Culture selection (e.g., en-US fallback).
3. **XML loaders:**
   - Load ADMX → map policies, categories, supportedOn.
   - Load ADML → map string table, presentation elements.
4. **Merge layer:**
   - Bind ADMX references to ADML strings/presentations; handle missing strings gracefully.
5. **Validation pass:**
   - Schema/structure checks; collect errors/warnings into a ValidationResult model.
6. **Caching:**
   - In-memory cache keyed by file set + culture; invalidate on file change.

Acceptance criteria: Load Windows base ADMX set without crashes; policy tree populated with localized names; validation shows zero false positives for known-good files.

## Visual policy editor

1. **Views and layout:**
   - PolicyEditorView: left TreeView (categories), right TabView (Details, Source, Validation).
2. **Detail editors:**
   - Data templates per type: BooleanPart → ToggleSwitch; EnumPart → ComboBox; Decimal/Text → NumberBox/TextBox with masks.
3. **Binding and validation UI:**
   - Two-way binding to a PolicySettingViewModel; show inline errors with InfoBar/Badge.
4. **Source viewer:**
   - Read-only XML (from ADMX) with syntax highlighting (TextHighlighter rules for XML tokens).
5. **Search and filters:**
   - Search box with fuzzy match over policy name, category, and description.
6. **Dirty-state and apply:**
   - Track changed settings; enable Apply/Revert buttons.

Acceptance criteria: Edit values for multiple policy types; errors display immediately; Apply/Revert correctly update pending set.

## Real-time validation

1. **Rules engine:**
   - Implement IValidationRule: TypeRule, RangeRule, EnumRule, RequiredRule, SupportedOnRule.
2. **OS profile:**
   - User-selectable target OS (Windows 10/11 variants, Server); rules consult supportedOn map.
3. **Execution:**
   - Validate on change and on demand; aggregate to Problems panel with severity and location.
4. **Extensibility:**
   - Rule catalog to allow custom org rules (e.g., “USB must be disabled”).

Acceptance criteria: Invalid values blocked or warned; supportedOn mismatch flagged; Problems panel navigates to offending policy.

## Deployment engine (local + OU preview)

1. **Registry writers (local):**
   - Map policy parts to registry under HKLM/HKCU\Software\Policies\... and Security settings where applicable.
   - Generate .reg preview and dry-run diff (current vs desired).
2. **Transaction + rollback token:**
   - Snapshot changed keys/values before write; store in SQLite as rollback blob.
3. **OU/GPO integration (preview → enable):**
   - Introduce abstraction IPolicyTarget { LocalMachine, CurrentUser, Gpo(Id) }.
   - If GPMC COM is available: create/open GPO, write settings; otherwise, show prerequisites and keep “preview/export” only.
4. **Export/import:**
   - Export current selection to .policy.json (internal schema) + optional .reg.
   - Import merges into pending set with conflict markers.

Acceptance criteria: Local deployment applies reliably; dry-run shows correct diff; OU push succeeds when GPMC present or shows actionable prerequisites.

## Versioning and rollback

1. **SQLite schema:**
   - Tables: PolicySet, PolicySetting, PolicyHistory, DeploymentLog.
   - Store normalized settings and a compressed snapshot blob per revision.
2. **Diff engine:**
   - Compute adds/modifies/deletes between two snapshots; render readable diff.
3. **Rollback operation:**
   - Select prior revision → generate operations → apply via deployment engine with transaction.
4. **Labels and notes:**
   - Allow commit messages and tags per snapshot.

Acceptance criteria: Every deployment records a snapshot; rollback restores previous values; diffs are accurate and human-readable.

## Packaging and elevation

1. **MSIX packaging project:**
   - Identity, capabilities (e.g., registry, runFullTrust via AppExecutionAlias if needed).
2. **Elevation strategy:**
   - Broker service (Windows Service or elevated helper) for registry writes; UI communicates via IPC (named pipes/gRPC).
3. **Startup checks:**
   - Detect admin rights and domain/RSAT presence; surface status in a System Status bar.

Acceptance criteria: App installs via MSIX; privileged actions routed through broker; clear status/error UX.

## Diagnostics and logging

1. **Logging:**
   - Serilog to JSON rolling file + SQLite summary; enrich with user, machine, OS, session id.
2. **Event correlation:**
   - Correlate UI actions → engine operations → deployment outcomes with a single ActivityId.
3. **Troubleshooting view:**
   - In-app viewer for recent logs and last deployment transcript.

Acceptance criteria: Actionable logs for failures; users can export a diagnostic bundle.

---

# Data model and formats

## SQLite schema (core tables)

```sql
CREATE TABLE PolicySet (
  Id TEXT PRIMARY KEY,
  Name TEXT NOT NULL,
  CreatedUtc TEXT NOT NULL,
  CreatedBy TEXT,
  TargetScope TEXT NOT NULL, -- LocalMachine|CurrentUser|Gpo:{Guid}
  Notes TEXT
);

CREATE TABLE PolicySetting (
  Id TEXT PRIMARY KEY,
  PolicySetId TEXT NOT NULL,
  PolicyId TEXT NOT NULL,        -- e.g., "Computer/Windows Components/BitLocker/..."
  PartId TEXT,                   -- specific ADMX part
  Value TEXT NOT NULL,           -- normalized string; store original type in ValueType
  ValueType TEXT NOT NULL,       -- Boolean|Enum|Numeric|Text
  Enabled INTEGER NOT NULL,      -- 0/1
  FOREIGN KEY (PolicySetId) REFERENCES PolicySet(Id)
);

CREATE TABLE PolicyHistory (
  Id TEXT PRIMARY KEY,
  PolicySetId TEXT NOT NULL,
  Version INTEGER NOT NULL,
  CreatedUtc TEXT NOT NULL,
  CreatedBy TEXT,
  Snapshot BLOB NOT NULL,        -- compressed JSON of entire set
  Message TEXT,
  FOREIGN KEY (PolicySetId) REFERENCES PolicySet(Id)
);

CREATE TABLE DeploymentLog (
  Id TEXT PRIMARY KEY,
  PolicySetId TEXT NOT NULL,
  Version INTEGER NOT NULL,
  Target TEXT NOT NULL,
  StartedUtc TEXT NOT NULL,
  EndedUtc TEXT NOT NULL,
  Status TEXT NOT NULL,          -- Success|Failed|Partial
  DiffJson TEXT NOT NULL,
  Error TEXT
);
```

## Internal policy JSON (export)

```json
{
  "name": "Baseline - Workstations",
  "targetScope": "LocalMachine",
  "osProfile": "Windows 11 23H2",
  "settings": [
    {
      "policyId": "Computer\\Microsoft Defender Antivirus\\Turn off Microsoft Defender Antivirus",
      "enabled": false,
      "valueType": "Boolean",
      "value": "false"
    },
    {
      "policyId": "Computer\\Removable Storage Access\\All Removable Storage classes: Deny all access",
      "enabled": true,
      "valueType": "Boolean",
      "value": "true"
    }
  ]
}
```

---

# Repository and solution structure

```
/src
  /Client.WinUI            # WinUI 3 UI
    Views/
      PolicyEditorView.xaml
      DeploymentControlView.xaml
      RollbackView.xaml
    ViewModels/
    Services/Contracts/    # IPC contracts
  /Core.Engine             # Worker/Core library
    Parsing/
      AdmxLoader.cs
      AdmlLoader.cs
      AdmxModel.cs
    Validation/
      Rules/
    Deployment/
      RegistryWriter.cs
      GpoWriter.cs
    Versioning/
      SnapshotService.cs
      DiffService.cs
    Storage/
      SqliteContext.cs
  /Broker.Service          # Elevated broker (Windows Service or FullTrust broker)
    Ipc/
    Operations/
  /Shared                  # DTOs, Abstractions
  /Package                 # MSIX packaging project
/docs
  packaging.md
  ARCHITECTURE.md
  README.md
```

- **Boundaries:** UI doesn’t touch registry or COM directly; it calls Core via interfaces. All privileged actions route through Broker.
- **Contracts:** Define IPolicyRepository, IDeploymentService, IValidationService, IAdmxCatalog in Shared.

---

# Milestones, acceptance criteria, and risks

## Milestone 1: Parsers + basic editor (2 weeks)
- **Deliverables:** Load/merge ADMX/ADML, tree navigation, detail editors for boolean/enum/numeric/text.
- **Acceptance:** Can view and edit 20+ common policies with localized text; validation catches type/range.

## Milestone 2: Validation + search + source view (1 week)
- **Deliverables:** Rules engine, OS profile selector, Problems pane, XML source highlighting, global search.
- **Acceptance:** Unsupported policies flagged; Problems pane navigates; search finds by name/description.

## Milestone 3: Local deployment + dry-run + versioning (2 weeks)
- **Deliverables:** Registry writer, diff preview, snapshots in SQLite, Apply/Revert, rollback to prior version.
- **Acceptance:** Dry-run shows correct changes; apply modifies registry; rollback restores previous state.

## Milestone 4: Packaging + broker + diagnostics (1 week)
- **Deliverables:** MSIX package, elevated broker path, Serilog logs, diagnostics export.
- **Acceptance:** Installable app; privileged ops succeed via broker; logs capture a failed and a successful deployment.

## Stretch: OU/GPO writer (preview) (1 week)
- **Deliverables:** GPO abstraction with capability checks; when GPMC present, create/update a test GPO.
- **Acceptance:** Create and link a test GPO in a lab; otherwise show clear prerequisites.

## Key risks and mitigations
- **GPO COM availability:** Mitigate with capability checks and clear “preview/export” path; ship local-first.
- **Elevation in MSIX:** Use brokered helper or Windows Service; keep registry writes out of UI process.
- **ADMX edge cases:** Add robust error handling and a fallback “raw XML” inspector for unknown parts.
- **OS variance:** Model target OS profile explicitly; allow per-policy overrides for server SKUs.

If you want, I can generate the initial project scaffolding (interfaces, models, and empty services) and a baseline SQLite migration to accelerate Milestone 1.
