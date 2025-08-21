

---

## **Enterprise Policy Manager + AI‑Assisted IT Companion — Dev Reference**

**Layer 1 — Client Layer (UI/Interaction)**  
**Frameworks**: WinUI 3, XAML, C# .NET 9.0  
- `PolicyEditorView` — ADMX/ADML parser, syntax highlighting (Roslyn integration)  
- `DeploymentControlView` — triggers policy push commands  
- `RollbackView` — fetches historical states from SQLite  
- `NLQueryInput` — sends structured JSON requests to AI Layer API endpoint  

---

**Layer 2 — Core Policy Engine**  
**Language**: C# .NET Worker Service  
**Modules**:  
- `PolicyParser.cs` — Validates schema, parses ADMX/ADML into object model  
- `PolicyDeploymentService.cs` — Interfaces with Group Policy APIs, WMI calls  
- `VersionControlService.cs` — Stores diffs in SQLite (`PolicyHistory` table)  
- `ComplianceChecker.cs` — Compares current vs. desired state JSON configs  

---

**Layer 3 — AI Layer**  
**Runtime**: ONNX Runtime, Python microservice (optional)  
**Services**:  
- `DataCollector.cs` — WMI queries, Event Log scraping, system snapshots  
- `AnomalyDetection.onnx` — ML model to detect misconfigurations  
- `RecommendationEngine.cs` — Suggests optimal settings via rule‑based + ML hybrid  
- `SelfHealingExecutor.ps1` — Executes remediations, logs results back to Core  

---

**Layer 4 — Enterprise Dashboard (Phase 3)**  
**Framework**: Blazor Server + SignalR  
**Components**:  
- `UserManager.cs` — Role‑Based Access Control (Admin, Auditor, Help Desk)  
- `NotificationHub.cs` — Push events to Teams/Slack Webhooks  
- `ReportService.cs` — Generates PDF/CSV for ISO/NIST/CIS compliance  

---

**Layer 5 — Security Layer**  
- **Transport**: TLS 1.3 w/ mutual certificate auth  
- **Integrity**: SHA‑256 signed `.policy` packages  
- **Audit**: Append‑only logging via `AuditLogService.cs`  

---

**Layer 6 — Data Flow Summary**  
1. **Client Layer** → sends REST/gRPC calls to Core Policy Engine  
2. **Core Policy Engine** → deploys to endpoints or queries state  
3. **AI Layer** → monitors, predicts, remediates  
4. **Enterprise Dashboard** → centralizes fleet‑wide view  
5. **Security Layer** → wraps all comms & artifacts  

---

This format makes it dead‑simple to hand off to a dev team, drop into a repo as `ARCHITECTURE.md`, or use as the legend alongside your diagram so everyone can see *exactly* what runs where.  

When your visual is ready, we can merge this reference into the diagram’s legend so both code‑level and high‑level views live side‑by‑side for maximum clarity.
