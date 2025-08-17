# GitHub Copilot Usage Guide (Project Quick Reference)


Purpose: Provide concise instructions so prompts to Copilot align with this solution's architecture and standards.

## 1. Project Context (Summarize in Prompts)
Layers: UI (WinUI3) -> Core Engine (parsing, validation, deployment, versioning) -> AI Layer (optional ML + data collection) -> (Future) Dashboard (Blazor) -> Security (TLS, signing, auditing).
Primary Languages: C# (.NET 9) for UI/Core; optional Python for ML prep; PowerShell for remediation scripts.
Persistence: SQLite (policy + history + deployment log).

## 2. Prompt Patterns
Use these templates when asking Copilot for help:
- Implement interface: "Implement IDeploymentService in Core.Engine handling local registry writes with rollback token and diff generation. Return Result object with status + errors list."
- Add validation rule: "Create SupportedOnRule : IValidationRule checking policy supportedOn metadata against selected OS profile enum TargetOsProfile."
- Generate test: "Produce xUnit tests for AdmxLoader merging ADMX + ADML (culture fallback en-US). Include malformed XML case."
- Refactor request: "Refactor RegistryWriter to inject IRegistryAccessor for testability; add abstraction and adjust constructor DI." 
- SQL migration help: "Draft initial CREATE TABLE statements for PolicySet, PolicySetting, PolicyHistory, DeploymentLog matching existing model classes."

Always include: desired layer, existing types, success criteria.

## 3. Coding Conventions
- C#: PascalCase for public members, _camelCase for private fields. Use file-scoped namespaces.
- Prefer async suffix for async methods; cancellation tokens on all IO boundaries.
- Use dependency injection; no static singletons (except pure helpers).
- Avoid premature optimization; measure before introducing caching beyond planned ADMX cache.
- Logging: use ILogger<T> injected; no Console.WriteLine.

## 4. Architecture Constraints
- UI must NOT access registry or COM directly; route via services / broker.
- Core parsing is pure (no side effects) except explicit I/O loads.
- Versioning service responsible for diff + snapshot compression.
- Future AI layer optional; keep clean seam via IDataCollector + IRecommendationEngine abstraction.
- All projects must compile with .NET 9.0 SDK; no legacy .NET Framework code.
- Use SQLite for all data persistence; no direct file I/O outside of SQLite context.
- All imports must be explicitly defined in the project file; no implicit imports.
- UI will use WPF until the SDK is fixed and WinUI is more stable. UI may be converted in later phases.

## 5. Data & Error Handling
- Return Result<T> (Success flag + Errors collection) instead of throwing for validation failures.
- Throw only for exceptional/unrecoverable conditions (e.g., corrupted DB schema).
- Wrap external calls (registry, COM, WMI) with narrow adapters to facilitate mocking.

## 6. Testing Guidance
- Unit test: parsers, validators, diff engine (deterministic logic).
- Integration test: registry write (use in-memory abstraction / temp hive), SQLite migrations.
- Snapshot tests for ADMX parse trees (serialize core object model to JSON, compare baseline).

## 7. Security & Compliance Notes
- Never embed credentials, cert private keys, or domain specifics in code or prompts.
- Ensure paths and file operations validate input (no directory traversal).
- Plan for TLS + signing but mock in early phases.

## 8. Good vs Weak Prompt Examples
Good: "Generate C# WinUI3 view model PolicySettingViewModel with INotifyPropertyChanged, properties: PolicyId, Enabled (bool), Value (string), Errors (ObservableCollection<string>); raise errors when Value empty while Enabled true." 
Weak: "Make a view model for policies."

## 9. Incremental Workflow
1. Describe target change + acceptance criteria.
2. Ask Copilot for skeleton / interface.
3. Fill domain specifics manually (enums, mapping tables).
4. Request unit tests.
5. Refine performance / logging after correctness.

## 10. Avoid
- Generating large monolithic classes (>300 lines) — ask for slices (e.g., "only diff logic").
- Mixing layers (no UI types inside Core.Engine).
- Direct registry manipulation from tests without abstraction.

## 11. Quick Reference Interfaces (Planned)
- IAdmxCatalog: LoadAsync(pathSet, culture)
- IValidationService: Validate(PolicySet, TargetOsProfile)
- IDeploymentService: DryRunAsync(policySet), ApplyAsync(policySet)
- IVersioningService: CommitAsync(policySet, message), GetHistoryAsync(id)

## 12. When Unsure
Prompt: "List existing services and suggest integration point for <feature>." Then verify manually against architecture doc.

---
Keep this file concise; expand only when a repeated clarification emerges. Update alongside architecture changes.
