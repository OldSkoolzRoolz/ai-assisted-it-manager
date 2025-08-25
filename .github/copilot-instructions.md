| 🗂️ **Field**           | **Value**                                   |
|-------------------------|---------------------------------------------|
| **Date**                | 2025-08-25                                  |
| **Modified By**         | @KyleC69                                    |
| **Last Modified**       | 2025-08-25                                  |
| **Title**               | *Copilot Coding Agent Operational Guide*   |
| **Author**              | Repository CODEOWNER                        |
| **Document ID**         | COPILOT-INSTR-001                           |
| **Document Authority**  | @KyleC69                                    |
| **Version**             | 2025-08-25.v4                               |

---

# Copilot Coding Agent 

**Repository CODEOWNER**: @KyleC69

---

## 0. Operational Directives (Enforced)
These directives override any prior ambiguity. The assistant MUST:
- Execute requested engineering tasks end-to-end without asking for confirmation once the user has stated or re-stated intent ("proceed" / explicit instruction).
- Avoid speculative prompts or meta-questions; only ask if a critical blocker (missing file, ambiguous target) prevents safe implementation.
- Prefer direct implementation (edit, create, remove) over advisory prose when changes are required.
- Never reintroduce removed technologies (SQLite, deprecated repositories) unless explicitly re-approved by CODEOWNER.
- Treat build warnings (analyzers, CS1591) as failures: fix or apply narrowly-scoped suppression with justification comment.
- Keep answers concise, in the prescribed tone (peer, practical, no fluff). No marketing language.
- When user mandates completion of enumerated tasks (e.g., Policy Viewer, Log Viewer), do not defer—produce concrete code changes until functional baseline exists (UI wired + data binding active, empty states handled, error surfacing, interaction commands).
- For UI components: ensure DataContext set, bindings resolved, and feature toggles (grouping, filtering, search) connected to underlying view models.
- For test coverage directives: add or extend tests without prompting until coverage for targeted public surface is present or limitations (e.g., platform UI constraints) are documented inline in test class comments.
- Use reflection-based smoke tests only as supplementary; prefer behavior/assertion tests for core logic.
- Do not downgrade framework targets or platform-specific properties without explicit instruction.
- When removing code (e.g., legacy stores), also purge related docs, stray references, and comments mentioning the removed component.
- Maintain Logging Event ID schema stability; adding IDs requires updating the schema table.
- Ensure each edit batch ends with a verification build (run_build) unless only documentation changed.
- APPLY ASYNC GUIDELINES: Prefer async/await for all I/O, DB, file, network, and long-running operations; never block with .Result, .Wait(), Task.Run() on async call chains; propagate CancellationToken; avoid async void (except event handlers). Refactor synchronous wrappers to delegate to async core methods.

Escalation Guidelines:
- If a required dependency or file is truly absent, perform a targeted search (code/text) first; only then notify with exact missing artifact name.
- If a request conflicts with security/integrity (e.g., logging secrets), refuse and cite policy succinctly.

---

## 1. Repository Summary

This repository (ai-assisted-it-manager) is the codebase for a multi‑phase "AI-Assisted Policy Manager & IT Companion" platform. Core goals:
- Parse, validate, version, and deploy Windows Group Policy templates (ADMX/ADML).
- Provide a desktop UI (initial WPF; future WinUI 3) plus future Blazor Server dashboard.
- Implement a self-healing & anomaly detection layer (policy drift, predictive alerts).
- Centralize configuration and policy state in a SQL Server–backed data layer.
- Evolve toward enterprise RBAC, auditing, and monetization phases.

Current emphasis (Phase 1) = Core Policy Manager & foundational modules, minimal conversational AI.

---

## 2. High-Level Tech / Tooling

- Primary language: C# (.NET 9 target as per README – expect preview SDK until GA).
- Solution file: ITCompanion.sln at repo root (multi-project structure).
- Platform focus: Windows 10/11 (WinUI; registry / WMI interactions).
- Database: SQL Server (local developer instance likely required for DB-related features).
- AI Layer (ure / scaffolding): ONNX runtime integration (not necessarily active yet).
- Scripts & Docs:
  - docs/ (architecture drafts, versioned documentation with audit trails via DOCUMENTATION_VERSION_MANIFEST.md, Internal setting standards `ConfigSettings.md`).
  - onboarding/ (module descriptions, setup guide, workspace-presets.ps1).

---

## 3. Repository Layout (Observed to Date)

Root files & directories:
- .gitattributes
- .gitignore
- ITCompanion.sln
- README.md
- SelfHealingPolicyEngine/ 
- docs/
- onboarding/
- src/ (contains all project modules)
- tests/ (solution test projects) To be implemented later

Module references (from onboarding/README.md):
- CorePolicyEngine (core ADMX/ADML parsing & validation)
- ClientApp (desktop UI – WPF/WinUI)
- EnterpriseDashboard (future Blazor Server)
- ITCompanionDB (database schema / migrations)
- Security (defender integration, enforcement logic)
- SelfHealingPolicyEngine (tamper protection, policy enforcement)

Expect each implementation project to reside under src/<ProjectName>/ with a corresponding *.csproj and be included in ITCompanion.sln. The SelfHealingPolicyEngine currently sits at root (historic or experimental placement); when modifying it, ensure it remains properly referenced in the solution.

---

## 4. Module Logging

### 4.0 Localization Enforcement (Global)
ALL user-facing or diagnostically meaningful log message text MUST originate from resource templates to enable full localization and word-order flexibility. Performance-sensitive, high-frequency debug/trace messages MAY retain invariant structured templates via `LoggerMessage` only if they do NOT need localization (internal-only) — otherwise switch to resource-based formatting as per PolicyEditor refactor.

### 4.1 Resource Template Rules
- Store full sentence templates in `.resx` with key suffix `_Template` (e.g., `CatalogLoaded_Template`).
- Use indexed placeholders `{0}`, `{1}`, ... (no named placeholders) to simplify translator workflow and maintain ordering freedom per culture.
- Provide satellite `.resx` files for each supported culture (e.g., `fr-FR`, `qps-PLOC` pseudo). Pseudo locale required for early layout issues detection.
- Never concatenate localized fragments at call sites; compose entire sentence within resource template.
- Add tests that:
  - Ensure every neutral `*_Template` key appears in each satellite.
  - Validate sequential placeholder indexes (0..n-1) and balanced braces.

### 4.2 Logging API Patterns
Preferred two patterns:
1. Resource Template Logger (localized):
```csharp
var tmpl = Res.GetString("CatalogLoaded_Template", culture);
logger.LogInformation(EventIds.CatalogLoaded, string.Format(culture, tmpl, lang, count, elapsedMs));
```
2. High-Frequency Invariant (non-localized internal only):
```csharp
[LoggerMessage(EventId = 3101, Level = LogLevel.Debug, Message = "ADMX parse fragment {File} bytes={Bytes}")]
internal static partial void AdmxParseFragment(this ILogger logger, string File, long Bytes);
```
IF at any point an invariant event becomes user-visible, migrate it to pattern 1.

### 4.3 Structured Context + Localization
- When using resource templates, include structured context via scopes when needed:
```csharp
using (logger.BeginScope(new { PolicyKey = key, Count = count }))
    logger.PolicySelected(key, count); // localized message already created
```
- Do NOT duplicate large payloads in both message text and properties.

### 4.4 EventId Allocation
(Schema retained) — add new ranges to table below when needed; preserve stability.

| Range | Subsystem / Purpose | Notes |
|-------|---------------------|-------|
| 1000–1099 | Security (authZ / access evaluation) | Implemented |
| 1100–1199 | Security (future integrity / tamper) | Reserved |
| 2000–2099 | Client UI Policy Editor | Localized resource templates implemented |
| 2100–2199 | Client UI Log Viewer & Diagnostics | To be added (use *_Template) |
| 3000–3099 | ADMX/ADML Parsing / Catalog Loader | Add localized wrappers |
| 3100–3199 | Validation Engine | Planned localized templates |
| 4000–4099 | Deployment / Registry application | Planned |
| 5000–5099 | Versioning / Snapshot / Diff | Planned |
| 6000–6099 | Storage / Repository operations | Low-noise localized/invariant hybrid |
| 7000–7099 | Self-Healing / Drift detection | Future |
| 9000–9099 | Critical / Fallback / Fatal | Always localized + invariant scope props |

### 4.5 Implementation Checklist (Log Entry Additions)
1. Reserve/EventId in correct range.
2. Add resource key `<Name>_Template` to neutral `.resx` + all satellites.
3. Add or update tests validating key presence & placeholders.
4. Implement extension or helper that formats using `CultureInfo.CurrentUICulture`.
5. If high-frequency & internal-only: justify invariant LoggerMessage usage in code comment (`// Perf: invariant, not user visible`).
6. Update documentation (this file and architecture if new subsystem introduced).

### 4.6 Forbidden Practices
- NO hard-coded English sentences in `logger.Log*` calls.
- NO string interpolation in localized logger wrappers (use `string.Format(culture, template, ...)`).
- NO partial localization (previous `{Msg}` prefix pattern is deprecated).
- NO mixing of localized text with raw user input without sanitization (log injection risk).

### 4.7 Migration Notes
Existing legacy LoggerMessage usages MUST be migrated when: (a) surfaced in UI; (b) exported to user-consumable logs; (c) needed for localization QA. PolicyEditor already migrated — use it as reference baseline.

### 4.8 Testing Requirements
- Unit tests fail if any new `*_Template` key missing in any satellite culture committed to repo.
- Add pseudo-locale pass test ensuring bracket markers (`[!!` if used) appear for every template (signals correct satellite load).
- Optional: analyzer/test scanning for disallowed hard-coded color hex codes and hard-coded log English phrases using regex heuristics.

### 4.9 High Contrast Considerations
Logging itself does not render directly; UI log viewers must use theme resources for foreground/background brushes (no inline colors). Ensure viewer binds to theme brushes only.

---

## 4.10 Example (Localized + Structured)
```csharp
// Resource key: PolicySelected_Template = "Policy selected: {0} settings={1}" (neutral)
public static void PolicySelected(this ILogger logger, string policyKey, int settingCount)
{
    var tmpl = Res.GetString("PolicySelected_Template", CultureInfo.CurrentUICulture) ?? "Policy selected: {0} settings={1}";
    logger.LogInformation(EventIds.PolicySelected, string.Format(CultureInfo.CurrentUICulture, tmpl, policyKey, settingCount));
}
```

---

## 5. Build Instructions

Canonical build sequence (ALWAYS in this order after bootstrap):

1. dotnet restore ITCompanion.sln
2. dotnet build ITCompanion.sln -c Debug -warnaserror /p:TreatWarningsAsErrors=true /p:AnalysisLevel=latest /p:EnforceCodeStyleInBuild=true
3. (Optional release) dotnet build ITCompanion.sln -c Release -warnaserror /p:TreatWarningsAsErrors=true /p:AnalysisLevel=latest /p:EnforceCodeStyleInBuild=true
4. (Analyzer formatting) dotnet format analyzers --verify-no-changes || dotnet format analyzers

ALWAYS ensure zero warnings. If an upstream package triggers unavoidable warnings, add a targeted suppression with justification (never blanket disable ruleset globally without CODEOWNER approval).

---
## 5a. Warning / Analyzer Enforcement Policy

Automation MUST:
- Fail fast if any compiler/analyzer warning appears (treat as error).
- Prefer code fixes over suppressions; suppress only with linked issue + rationale comment.
- Keep .editorconfig authoritative for code style; do not inline style suppressions unless rule conflicts with generated code.

Manual Workflow Quick Command (local):
```
dotnet clean ITCompanion.sln
DOTNET_NOLOGO=1 dotnet build ITCompanion.sln -c Debug -warnaserror /p:TreatWarningsAsErrors=true /p:AnalysisLevel=latest /p:EnforceCodeStyleInBuild=true
```

---
## 6. Coding Standards
- Follow .NET naming conventions (PascalCase for types/methods, camelCase for parameters/locals).
- Use async/await for any potentially blocking operation (disk, network, IPC, DB, process, heavy CPU offload). Never introduce new sync-over-async patterns.
- Always provide a CancellationToken on public async APIs; pass through to lower layers.
- Avoid async void (except UI/event handlers). Library/internal logic uses Task / Task<T>.
- Do not wrap synchronous work in Task.Run just to appear async; only offload CPU-bound work that would otherwise block UI thread.
- Avoid fire-and-forget; if unavoidable (telemetry), capture/log exceptions safely.
- Propagate ConfigureAwait(false) ONLY in pure library code not touching UI; omit in UI-layer code to keep context.
- Prefer ValueTask/ValueTask<T> only when profiling proves allocation benefit and method frequently returns synchronously.
- Use SemaphoreSlim (async) not lock for awaiting asynchronous critical sections.
- Stream large payloads instead of buffering whole content in memory.
- Validate arguments early; throw ArgumentException derivatives with nameof().


