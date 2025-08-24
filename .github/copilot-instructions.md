# Copilot Coding Agent 

**Repository CODEOWNER**: @KyleC69

Version: 3.3
Date: 2025-08-23
---

## Copilot Language Guidelines:
 ### clarity:
    - Avoid academic jargon, theoretical abstractions, or overly formal phrasing.
    - Prefer plain English and real-world analogies over textbook definitions.
    - Use short, direct sentences with clear intent.
    - Explain acronyms and technical terms only when necessary.

 ### tone for self taught:
    - Write as if explaining to a smart, curious developer who learned by doing—not by sitting through lectures.
    - Assume practical experience, not formal training.
    - Celebrate resourcefulness and hands-on problem solving.
    - Avoid condescending or overly pedantic explanations.

 ### examples and explanation:
    - Use relatable examples (e.g., “like organizing folders on your desktop”).
    - Break down complex ideas into digestible steps.
    - Prioritize actionable advice over theoretical depth.
    - When introducing a concept, explain *why it matters* before *how it works*.

  ### formatting:
    - Use bullet points, headings, and code blocks to improve readability.
    - Avoid dense paragraphs or verbose technical exposition.
    - Highlight key takeaways or “what you actually need to know.”

  ### personality alignment:
    - Sound like a helpful peer, not a professor.
    - Inject light humor or empathy when appropriate.
    - Respect the user’s time, experience, and autonomy.

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
- Platform focus: Windows 10/11 (WPF UI; registry / WMI interactions).
- Database: SQL Server (local developer instance likely required for DB-related features).
- AI Layer (future / scaffolding): ONNX runtime integration (not necessarily active yet).
- Scripts & Docs:
  - docs/ (architecture drafts, versioned documentation with audit trails via DOCUMENTATION_VERSION_MANIFEST.md).
  - onboarding/ (module descriptions, setup guide, workspace-presets.ps1).
- Tests directory: tests/ (dotnet test entry per README).
- Additional engines / future: Blazor Server (Phase 3), WinUI 3 migration path, security integrations.

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
- Use Microsoft.Extensions.Logging for all logging.
- Configure logging in the `Program.cs` or `Startup.cs` file of each module.
- Use Static LoggerMessages for high-frequency log entries to improve performance.
- Use appropriate log levels (Trace, Debug, Information, Warning, Error, Critical) based on the severity of the event.
- Avoid logging sensitive information (e.g., passwords, personal data).
- Log strings should be resource-based for localization support.

### 4.1 Logging Event ID Schema (REINSTATED)
Each subsystem owns a numeric EventId range. Do NOT overlap ranges. When adding new events, append within the allocated block; reserve gaps for expansion. If a new subsystem emerges, allocate a fresh contiguous block and document it here.

| Range | Subsystem / Purpose | Notes |
|-------|---------------------|-------|
| 1000–1099 | Security (authZ / access evaluation) | Already implemented in Security.Logging.Logger |
| 1100–1199 | Security (future integrity / tamper) | Reserved |
| 2000–2099 | Client UI Policy Editor (search, selection, catalog load) | Implemented in PolicyEditorLog |
| 2100–2199 | Client UI Log Viewer & Diagnostics | To be added |
| 3000–3099 | ADMX/ADML Parsing / Catalog Loader | Add source‑generated logger before extending |
| 3100–3199 | Validation Engine (rules execution) | Pending implementation |
| 4000–4099 | Deployment / Registry application | DeploymentService future |
| 5000–5099 | Versioning / Snapshot / Diff | Future phase |
| 6000–6099 | Storage / Repository operations (SQL / persistence) | Optional, keep low-noise |
| 7000–7099 | Self-Healing / Drift detection | Future phase |
| 9000–9099 | Critical / Fallback / Fatal recoverable states | Use sparingly, page operators |

Guidelines:
- EventId ranges stable; do not renumber after release.
- One LoggerMessage method per semantic event (avoid dynamic template changes).
- Prefer structured properties over string concatenation.
- Use source-generated partial static logger classes named `<Area>Log` or `Logger` in the subsystem namespace.
- For CA1848 compliance: avoid direct `LogInformation($"...")` in new code unless one-off / low frequency (and justify in PR).

### 4.2 Adding a New LoggerMessage
1. Pick the next unused EventId in the appropriate range.
2. Add a partial static class (or extend existing) with `[LoggerMessage]` attribute.
3. Keep parameter names descriptive (they become structured log field names).
4. Update this table if a new range or category is needed.
5. Include XML docs summarizing intent; mention EventId in summary when useful.

Example:
```csharp
[LoggerMessage(EventId = 3000, Level = LogLevel.Debug, Message = "ADMX file discovered {File} size={Bytes}")]
internal static partial void AdmxFileDiscovered(this ILogger logger, string File, long Bytes);
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
## 7. Testing

Standard test invocation (from README):
  dotnet test tests/

Preferred verbose form (surface all results, skip build duplicates):
  dotnet test ITCompanion.sln --no-build --configuration Debug

ALWAYS run dotnet build (with -warnaserror) first for deterministic behavior; dotnet test will otherwise restore & build implicitly, which can mask incremental issues.

If integration tests (e.g., DB or WMI) exist and are slow/flaky:
- Look for traits/categories (e.g., [Category("Integration")]) and optionally filter:
  dotnet test --filter TestCategory!=Integration

Add new tests in corresponding test project mirroring source project naming:
- src/CorePolicyEngine/ -> tests/CorePolicyEngine.Tests/
Ensure new test project is referenced in solution and uses the standard test framework (likely xUnit or MSTest; inspect existing test project for conventions before adding).

## 9. Database / Migrations (If ITCompanionDB Active)

Look for a project (e.g., src/ITCompanionDB or database project). Typical patterns:
- EF Core migrations: dotnet ef migrations add <Name> --project src/ITCompanionDB
- Apply locally via application startup or:
  dotnet ef database update --project src/ITCompanionDB

ALWAYS update the solution + build before generating migrations to avoid stale model issues.
Always test migration application on a fresh local database instance to verify correctness.
Always document migration procedures to outline steps for applying in production environments. migrations should be idempotent and reversible if possible. outlined in docs/migrations.md if it exists- create if it doesn't.

---

## 11. Adding New Code / Features

ALWAYS:
1. Identify correct module (e.g., parsing logic → CorePolicyEngine; UI interaction → ClientApp).
2. Add new classes with cohesive namespaces mirroring folder structure.
3. Update DI registration (if a central Startup/Program or composition root exists—search for Program.cs under each host project).
4. Add unit tests in corresponding test project before or alongside implementation.
5. Run full validation sequence (Section 14).
6. Ensure performance considerations (avoid blocking UI threads with heavy I/O).
7. Respect existing abstractions; prefer extension over modification for shared logic.

XML DOCUMENTATION MANDATORY:
- Every new class, struct, interface, enum, delegate, method (public/internal/private), property, event, and field MUST include an XML documentation header (///) summarizing purpose.
- Methods: document <summary>, each <param>, <returns> (if non-void), and <exception> tags for any thrown exceptions.
- Properties/fields: concise intent, units/ranges if applicable.
- Update or add docs when modifying a signature or behavior materially.
- No PR should introduce undocumented members. Treat missing docs as a build blocker (enforced manually until automated rule added).

---



## 13. Docs & Onboarding Resources

- docs/ (architecture drafts) – ALWAYS inspect docs/ for design decisions when making changes.
- onboarding/README.md – lists module definitions; onboarding/setup-guide.md (consult for detailed environment prerequisites).
If you update architecture, reflect changes consistently across:
1. docs/ architecture diagrams/text
2. onboarding/ module overview
3. This instructions file (only if foundational process changes—avoid churn for minor refactors)

S

---

## 19. File & Directory Quick Reference (Current Observed)

Root:
- ITCompanion.sln (multi-project solution – edit via dotnet sln commands)
- README.md (high-level roadmap)
- docs/ (architecture drafts; treat as authoritative for design)
- onboarding/ (module overview, setup guide, workspace-presets.ps1)
- src/ (primary implementation projects – enumerate before modifying)
- tests/ (test projects – ensure new tests land here)
- SelfHealingPolicyEngine/ (special-case module, root-level)
- .gitignore / .gitattributes (respect line endings & attribute normalization)

---


## 21. Minimal Quick Start (Copy/Paste)

```
git clone https://github.com/OldSkoolzRoolz/ai-assisted-it-manager.git
cd ai-assisted-it-manager
pwsh onboarding/workspace-presets.ps1
dotnet restore ITCompanion.sln
dotnet build ITCompanion.sln -c Debug -warnaserror /p:TreatWarningsAsErrors=true /p:AnalysisLevel=latest /p:EnforceCodeStyleInBuild=true
dotnet test ITCompanion.sln --no-build
dotnet run --project src/ITCompanionClient/ITCompanionClient.csproj
```

If any path differs, list src/ to identify correct project and adjust only that line.

---

## 23. Documentation Management & Versioning

**Documentation Authority**: @KyleC69 (Repository CODEOWNER) has final approval for all documentation changes.

**Version Control**: All documentation in `/docs` folder is versioned and tracked via `docs/DOCUMENTATION_VERSION_MANIFEST.md`:
- Review manifest before modifying any documentation
- Update manifest with change log entries for all documentation updates
- Follow version format: YYYY-MM-DD.vX
- Include technical accuracy reviews and framework version updates

**Automation Guidelines for Documentation**:
- Always reference the version manifest for current document versions
- Ensure technical references (e.g., .NET versions) match repository standards
- Update manifest when making any documentation changes
- Tag @KyleC69 for review approval on documentation PRs

---

## 24. Quality Bar

A change is "ready" ONLY if:
- Builds cleanly (no warnings: enforced by -warnaserror / TreatWarningsAsErrors=true).
- Resolves IntelliSense issues.
- All tests pass (and new tests cover new logic).
- No hard-coded environment-only paths or credentials.
- UI or service still starts successfully after change.
- Documentation changes include manifest updates and maintain technical accuracy.
- Analyzer + code style passes: dotnet format analyzers/style --verify-no-changes.
- XML documentation headers exist for all new or modified members (see Section 11).

---


