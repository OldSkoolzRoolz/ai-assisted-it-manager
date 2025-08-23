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

### Open Items / Roadmap Checklist
- [ ] Implement DeploymentService + sandbox publish pipeline.
- [ ] Introduce VersionControlService & rollback UI.
- [ ] Add AI anomaly detection service stub + no‑op fallback.
- [ ] Replace synchronous dialog blocking with async pattern (WinUI prompt).
- [ ] Harden audit log (tamper‑evident append store + rotation).
- [ ] Add unit/integration test projects & CI workflows.
- [ ] Implement RBAC & secure secret management for Dashboard.
- [ ] Provide migration path for remote policy catalogs (network fetch + caching).

---

This document reflects current post‑migration state (ClientShared consolidation, WinUI head) and outlines staged expansion paths. Keep synchronized with DOCUMENTATION_VERSION_MANIFEST.md when updated.
