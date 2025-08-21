## Copilot Instructions – AI Assisted Policy & System Management

Version: 2.8  
Date: 2025-08-21

---
## 1. Purpose & Scope
Deliver a modular, local-first policy & system management companion ("IT Companion") that can expand into an enterprise multi?client governance and threat?aware configuration platform. Free edition includes a limited teaser of AI Companion capabilities (feature gating TBD). Enterprise edition extends: cross?client aggregation, centralized policy groups, drift detection, audit trails, and advanced analytics.

## 2. Solution Topology (Projects)
- CorePolicyEngine: Parse / validate / model ADMX+ADML, policy evaluation, behavior policy, policy/state persistence, validation rules.
- ClientApp: WPF UI (editing, monitoring, diagnostics, limited AI teaser interface).
- Security: Signing, integrity, authN/authZ, tamper detection enrichment.
- EnterpriseDashboard (future): Blazor aggregated control & reporting (policy groups, drift alerts, audit explorer).
- AICompanion: Recommendations, anomaly detection, cross?client correlation.

## 3. Cross-Cutting Principles
- Modularity, minimal public surface, deterministic behavior, security by design.
- Observability: structured logs + correlation metadata + audit tables for administrative and drift events.
- Configuration via layered Behavior Policy (DB + initial registry seeding). Enterprise adds group / assignment layering consistent with GPO intent (extending, not redefining LSDOU semantics).

## 4. Logging & Diagnostics
- Per?module rolling JSON logs (independent rotation). Central viewers aggregate when needed.
- Enriched fields: ts, level, category, eventId, eventName, msg, ex, sessionId, host, user, pid, appVer, moduleVer.
- Event ID allocation (1k windows):
  - CorePolicyEngine: 1000–1999 (sub?ranges per §4 prior version)
  - ClientApp (UI): 2000–2999
  - Security: 3000–3999
  - EnterpriseDashboard: 4000–4999
    - Policy Groups & Assignment: 4100–4149
    - Drift Detection & Alerts: 4150–4179
    - Audit & Compliance Export: 4180–4199
  - AICompanion: 5000–5999
- Never repurpose retired IDs. Extend sub?ranges or allocate new blocks.
- Source generator ([LoggerMessage]) + resource indirection for UI?visible messages (logs themselves remain English for analysis baseline).


## Copilot Guidance
- Copilot should explain things clearly and simply, avoiding jargon where possible. Don't over explain or use complex language. Have a friendly, helpful tone.
- Prompt for clarification if the request is ambiguous or incomplete.
- Theming Compliance: ALL new or modified UI controls must consume existing theme tokens (colors, brushes, fonts, spacing, styles) via StaticResource / DynamicResource keys defined in Themes/*.xaml. No hard-coded Color, Brush, FontFamily, FontSize, CornerRadius, or spacing literals in view XAML or code-behind. If a needed visual primitive does not exist, add a semantic token (e.g., Brush.Alert.Critical, Space.24, FontSize.Overline) to the appropriate theme dictionary (Light, Dark, HighContrast) and then reference it. Maintain parity across Colors.xaml, Colors.Dark.xaml, Colors.HighContrast.xaml when introducing new color tokens.
- Prefer style extension (BasedOn) instead of duplicating Setters; keep style keys namespaced: Control.*, Shell.*, UI.* for semantic mapping.

## 5. Configuration of Settings
- Internal system controls will use ADMX-backed registry settings (HKLM/HKCU\Software\Policies\<Vendor>\Companion) for initial configuration.
- Mirrors *intent* of Windows GPO layering without adding Enforced/BlockInheritance yet (reserved extension points). Future flags (Enforced, BlockInheritance) may be added to replicate finer LSDOU behaviors.
- SettingCatalog.md: authoritative metadata outlining all settings, their ADMX paths, types, default values, and descriptions. This serves as the source of truth for both UI and policy generation.
- Polling watcher (default 30s) with future ETW/notification optimization.

## 6. Enterprise Policy Model (Extension)
Goal: Extend existing registry?backed policy paradigm (ADMX semantics) with application?level grouping while remaining consistent with Windows’ approach (we enhance, not redefine). Elements:
- PolicyDefinition: Canonical catalog of manageable settings (maps to registry path + value or composite operation) – stable identifiers for ADMX export later.
- PolicyGroup: Admin-defined collection of PolicyDefinitions with desired Enabled + Value states.
- ClientGroup (Department / Role): Sales, Executives, IT, etc.
- PolicyAssignment: Association of PolicyGroup ? ClientGroup.
- ClientEffectivePolicy: Materialized resolved state per client (post layering + group assignments).
- Drift Detection: Compare periodic client-reported hash/value with stored desired state; log deviations.
- Audit Trail: Immutable append-only log capturing every admin change and every drift event (who/what/when/previous/next/origin).
- Registry Consistency: All applied policies should continue to land under HKLM/HKCU\Software\Policies\<Vendor>\Companion ensuring ADMX alignment.

### Enterprise Layering (Conceptual)
DesiredState = (BehaviorPolicy Effective) + (Union of PolicyGroups assigned via ClientGroup memberships) with latter overriding earlier conflicts (last assignment order or explicit priority – priority scheme TBD). Drift detection compares DesiredState vs ReportedState snapshot from client.
- Distributed policy application: Clients report effective state (hash + values) to central store; server computes drift by comparing against DesiredState.
- Centralized policy management: Admins create/modify PolicyDefinitions and PolicyGroups; assignments are made to ClientGroups.

## 7. Data & Persistence
- Central relational store (SQL Server baseline) for enterprise entities (policy definitions, groups, assignments, drift, audit, effective state). Abstractions allow future provider implementations.
- Client may optionally maintain a lightweight local cache (implementation detail) but no hard dependency on SQLite in design; persistence API must not assume engine-specific features.
- Encapsulated DB logic via stored procedures when at all possible. Parameterized inline queries for simple lookups.
- All db elements need to be scripted for possible deployment to a dedicated SQL Server instance and packaged for versioning.
- Database project being used for ease of develperment and deployment. Scripts should be versioned and maintained in a dedicated folder structure (e.g., Data\Scripts\Versioning).


## 9. UI Requirements
- ICommand + CanExecute for all interactive elements (no direct code-behind except removable dev tooling).
- Localize: labels, tooltips, titles, status messages. Dev / diagnostics windows exempt (removed pre?release).
- Resource key pattern: UI.Button.*, UI.Label.*, UI.Dialog.*; provide AutomationProperties.Name when needed.
- MVVM boundaries enforced (no service calls in code-behind besides view plumbing).

## 10. Security & Compliance
- No privileged registry / COM / GPO modification in UI process. Future broker/service boundary.
- Mandatory audit for: admin policy modifications, policy group changes, assignments, drift detections, remediation actions.
- Tamper detection (future): signed policy packets, hash verification of effective policy sets.

## 11. Localization Scope
- UI only (for now). Logs remain English for analyzers. Research: OS event log language differences & potential normalization pipeline (TBD).

## 12. Packaging & Deployment
- WPF baseline; WinUI migration exploratory only.
- MSIX Packaging for ClientApp (future: enterprise dashboard, AI companion).

## 13. Guardrails
- No speculative NuGet; verify necessity & existence.
- Do not add Microsoft.Extensions.Logging.Generators (in-box).
- Minimize public surface; prefer internal except cross-module contracts.
- Avoid >300 line monoliths; refactor into cohesive units.

## 14. Documentation & Change Management
- Increment Version/Date on change; maintain forthcoming CHANGELOG.md.
- Update SettingCatalog seed + docs when adding/removing settings.
- Provide Architecture.md (high-level), ConfigSettings.md (settings), PolicyModel.md (enterprise specifics), and DBVersioning.md (proc/interface changes & rationale).

## 15. Outstanding TBD / Research Items
| Area | Question | Target Decision Point |
|------|----------|-----------------------|
| AI Companion Teaser | Free feature subset | Pre-beta packaging |
| Log Localization | Translate for AI semantics? | Post telemetry POC |
| Policy Distribution Mechanism | Push vs pull vs hybrid | Pre enterprise preview |
| Validation Rule Catalog Format | Storage representation | >10 rule threshold |
| Broker Architecture | Service vs FullTrust process | Before privileged deploy impl |
| Event Range Extensions | Handling saturation | As expansion occurs |
| Enforcement Flags | Need Enforced / BlockInheritance? | After initial layered rollout |
| Drift Priority | Conflict resolution strategy | Before enterprise GA |

---
## Removed Redundancies (Historical)
- Consolidated overlapping modularity/localization/security directives.
- Replaced explicit stored procedure requirement with portable encapsulated DB logic (now formalized under Data & Persistence with SQL Server baseline).
- Unified logging policy & event ID guidance.
- Removed prior SQLite-specific references; design now provider-agnostic with SQL Server reference implementation.

---
(End of instructions)
