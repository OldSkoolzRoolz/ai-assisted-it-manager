| 🗂️ **Field**           | **Value**                                                           |
|-------------------------|---------------------------------------------------------------------|
| **Date**                | 2025-08-25                                                          |
| **Modified By**         | @copilot                                                            |
| **Last Modified**       | 2025-08-25                                                          |
| **Title**               | *Enterprise Policy Manager & AI-Assisted IT Companion Architecture*|
| **Author**              | Architecture Team                                                   |
| **Document ID**         | ARCH-REF-001                                                        |
| **Document Authority**  | @KyleC69                                                            |
| **Version**             | 2025-08-25.v5                                                       |

---

<img width="1536" height="1024" alt="developer schematic" src="https://github.com/user-attachments/assets/93264a27-42bc-4822-a6ba-c8a71edc7d2e" />

---

## **Enterprise Policy Manager + AI‑Assisted IT Companion — Architecture Reference**

### Layer 1 — Client Layer (UI / Interaction)
Frameworks: WinUI 3 (current), legacy WPF retired, XAML, C# (.NET 8 runtime head; shared libs multi-target .NET 8/9)

Logical Components (current / planned):
- PolicyEditor (PoliciesPage + PolicyDetailDialog) — consumes shared PolicyEditorViewModel from ClientShared layer.
- DeploymentControl (planned) — triggers push via future DeploymentService API.
- RollbackView (planned) — surfaces historical states (VersionControlService snapshots).
- NLQueryInput (planned) — dispatches structured JSON to AI Layer endpoint (gRPC/REST TBD).

Cross-Cutting UI Support:
- ClientShared project supplies: PolicyEditorViewModel, PolicySettingViewModel, navigation models, dispatcher/prompt abstraction (IUiDispatcher, IMessagePromptService).
- WinUI adapters: WinUiDispatcher, WinUiPromptService implement abstractions.

### Layer 2 — Core Policy Engine
Runtime: .NET (multi-target net8.0-windows10.0.19041.0 + net9.0-windows)
Primary Responsibilities:
- Parsing: AdmxAdmlParser loads local ADMX/ADML into AdminTemplateCatalog.
- Catalog Indexing & Summaries: Materializes PolicySummary entries for fast UI filtering.
- Storage / Audit: AuditWriter + AuditStore (file/DB persistence – evolving toward SQL).
- Grouping & Policy Definitions: PolicyGroupRepository (SQL), definition DTOs.
- (Planned) DeploymentService for applying policy deltas.
- Validation & Evaluation: Registry action evaluation scaffolding.

### Layer 3 — AI Layer (Planned / Scaffold)
Runtime: Hybrid (C# services + optional Python / ONNX Runtime sidecar)
Planned Modules:
- DataCollector — WMI + Event Log + system snapshots -> normalized telemetry.
- AnomalyDetection.onnx — model to flag drift / misconfiguration.
- RecommendationEngine — hybrid rule + ML suggestions.
- SelfHealingExecutor (PowerShell) — executes remediation sequences and reports back.
Current Status: Interfaces & hooks pending; no active inference path yet.

### Layer 4 — Enterprise Dashboard (Phase 3)
Framework: Blazor Server (+ SignalR for push)
Planned Components:
- UserManager — RBAC (Admin, Auditor, HelpDesk roles).
- NotificationHub — Teams / Slack / Webhook fan‑out.
- ReportService — Compliance export (ISO / NIST / CIS) PDF & CSV.
Status: Project scaffolding present; feature implementation deferred.

### Layer 5 — Security Layer
Concerns (applies transitively across all layers):
- Transport: TLS 1.3, optional mutual cert auth for service surfaces.
- Integrity: SHA‑256 signatures on *.policy packages (planned signer service).
- Audit: Append‑only audit log (current basic writer; future tamper‑evident storage).
- Hardening Roadmap: Code signing, secure secret storage, runtime attestation.

### Layer 6 — Data & Control Flow Summary
1. Client (WinUI) requests catalog load -> Core Policy Engine (local process call; future service boundary gRPC/REST).
2. Core parses ADMX/ADML, emits AdminTemplateCatalog + PolicySummary list.
3. User selects policy -> shared VM resolves elements -> audit event logged.
4. User edits settings (sandbox) -> push triggers audit trail (deployment pipeline TBD).
5. AI Layer (future) ingests telemetry & policy state -> anomaly / recommendation feedback loop.
6. Dashboard (future) visualizes fleet compliance & receives push notifications.
7. Security layer envelopes all network, artifacts, and audit records.

### Shared Layer — ClientShared (New Consolidation)
Purpose: Eliminate duplication between legacy WPF and WinUI heads; provide framework‑agnostic view models & abstractions.
Contains:
- PolicyEditorViewModel & related navigation/row models.
- PolicySettingViewModel (element abstraction) & enum option records.
- Dispatcher & prompt service interfaces consumed by platform-specific adapters.
Migration Status: WinUI head now fully consumes ClientShared; WPF artifacts removed.

### Persistence & Future Data Stores
Current: Basic audit writer (file/placeholder), SQL repositories for policy groups (ADO / SqlClient).
Planned: Consolidated persistence via EF Core for policy definitions, history (VersionControlService), telemetry ingestion, and RBAC.

### Non-Functional Considerations
- Performance: Catalog load limited by max file count parameter (currently 50) to keep UI responsive; future lazy paging.
- Threading: UI interactions marshaled via IUiDispatcher; background parsing on thread pool.
- Extensibility: All policy actions routed through interfaces (IAdminTemplateLoader, IAuditWriter) to allow swapping storage / parsing strategies.
- Testing (roadmap): Introduce test projects (e.g., CorePolicyEngine.Tests) covering parsing, evaluation, and view model filtering logic.

### Async & Concurrency Strategy
Goal: Keep UI responsive, avoid deadlocks, simplify cancellation.

Principles:
- End-to-end async: All I/O (file, registry abstraction, DB, future network) implemented with async/await; no .Result / .Wait().
- UI thread affinity: Only ObservableCollection / XAML-bound mutations occur via IUiDispatcher.Post. All heavy work (parsing, diff computation, validation) executes off the UI thread before dispatching the result.
- Cancellation: Every public async API in core layers accepts a CancellationToken; tokens are passed through without being optional swallowed. Cooperative cancellation used (no Thread.Abort or Task.Run fire-and-forget).
- Error flow: Async pipelines surface exceptions to caller; UI layer decides presentation (InfoBar, dialog). Background fire-and-forget permitted only for audit/telemetry—exceptions are caught and logged.
- ConfigureAwait: Used (false) in pure library layers (CorePolicyEngine, Storage) to reduce context hops; omitted in UI or ClientShared view-model code to preserve context where necessary.
- Async void: Limited strictly to event handlers (WinUI events, PropertyChanged triggers). All other methods return Task/Task<T>.
- Parallelism: Bounded via SemaphoreSlim when batching ADMX file parsing; no unbounded Parallel.ForEach. Future large catalog parsing may adopt channel-based pipeline if profiling shows contention.
- Streaming: Large file reads (future: exported policy sets, deployment diffs) will use streaming APIs (FileStream with async read) rather than full-buffer loads.
- CPU-bound work: Only performance-hot CPU tasks (e.g., rule evaluation over thousands of policies) may use ValueTask or dedicated Task.Run partitioning after measurement proves need.
- Testing hooks: CancellationToken exposure allows deterministic cancellation tests; no hidden Task.Delay loops without token checks.

Enforcement:
- Analyzer configuration treats sync-over-async anti-patterns as warnings (escalated to errors per build flags).
- Code reviews: PR checklist includes “async contract maintained and no blocking calls added.”

Planned Enhancements:
- Introduce an AsyncPolicy (retry/circuit) wrapper for transient DB/network once those surfaces exist.
- Add structured concurrency (Task scopes) when .NET introduces first-class APIs (future consideration).

### Open Items / Roadmap Checklist
- [ ] Implement DeploymentService + sandbox publish pipeline.
- [ ] Introduce VersionControlService & rollback UI.
- [ ] Add AI anomaly detection service stub + no‑op fallback.
- [ ] Replace synchronous dialog blocking with async pattern (WinUI prompt).
- [ ] Harden audit log (tamper‑evident append store + rotation).
- [ ] Add unit/integration test projects & CI workflows.
- [ ] Implement RBAC & secure secret management for Dashboard.
- [ ] Provide migration path for remote policy catalogs (network fetch + caching).

### Localization Strategy
Globalization goals: no code changes required when adding a language.

Key Points:
- Resource template keys use *_Template suffix with indexed placeholders.
- Satellite .resx per culture (e.g., fr-FR, qps-PLOC for pseudo) placed alongside neutral resources.
- LocalizationService publishes culture changes; UI binds through LocalizedStringProvider.
- Logging uses localized full-sentence templates; stable EventIds preserved.
- Pseudo-loc culture (qps-PLOC) included to detect truncation/RTL issues early.
- Future: validation/error messages and remaining modules will migrate to the same pattern.

Culture Switch Flow:
1. User selects culture (UI command TBD).
2. LocalizationService.ChangeCulture updates CurrentUICulture and raises CultureChanged.
3. LocalizedStringProvider raises PropertyChanged (string.Empty) causing rebind.
4. Views update text without restart.

Testing:
- Placeholder index validation tests ensure consistent formatting.
- Satellite presence tests planned for all modules.

---

This document reflects current post‑migration state (ClientShared consolidation, WinUI head) and outlines staged expansion paths. Keep synchronized with DOCUMENTATION_VERSION_MANIFEST.md when updated.

<!-- End Document -->
