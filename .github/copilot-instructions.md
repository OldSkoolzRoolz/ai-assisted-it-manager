# Copilot Coding Agent – Repository Onboarding Guide

READ THIS FIRST  
These instructions are the canonical source of truth for automations working in this repository. Always follow them in the order given. ONLY perform exploratory searches (grep/code search) if the needed information is absent here or an executed command contradicts what is documented.

---

## 1. Repository Summary

This repository (ai-assisted-it-manager) is the codebase for a multi‑phase "AI-Assisted Policy Manager & IT Companion" platform. Core goals:
- Parse, validate, version, and deploy Windows Group Policy templates (ADMX/ADML).
- Provide a desktop UI (initial WPF; future WinUI 3) plus future Blazor Server dashboard.
- Implement a self-healing & anomaly detection layer (policy drift, predictive alerts).
- Centralize configuration and policy state in a SQL Server–backed data layer.
- Evolve toward enterprise RBAC, auditing, and monetization phases.

Current emphasis (Phase 1) = Core Policy Manager & foundational modules.

---

## 2. High-Level Tech / Tooling

- Primary language: C# (.NET 9 target as per README – expect preview SDK until GA).
- Solution file: ITCompanion.sln at repo root (multi-project structure).
- Platform focus: Windows 10/11 (WPF UI; registry / WMI interactions).
- Database: SQL Server (local developer instance likely required for DB-related features).
- AI Layer (future / scaffolding): ONNX runtime integration (not necessarily active yet).
- Scripts & Docs:
  - docs/ (architecture drafts).
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
- SelfHealingPolicyEngine/ (module directory – details not enumerated here)
- docs/
- onboarding/
- src/ (contains "several projects" – main code modules)
- tests/ (solution test projects)

Module references (from onboarding/README.md):
- CorePolicyEngine (core ADMX/ADML parsing & validation)
- ClientApp (desktop UI – WPF/WinUI)
- EnterpriseDashboard (future Blazor Server)
- ITCompanionDB (database schema / migrations)
- Security (defender integration, enforcement logic)
- SelfHealingPolicyEngine (present at root – may integrate or evolve)

Expect each implementation project to reside under src/<ProjectName>/ with a corresponding *.csproj and be included in ITCompanion.sln. The SelfHealingPolicyEngine currently sits at root (historic or experimental placement); when modifying it, ensure it remains properly referenced in the solution.

---

## 4. Bootstrap / Environment Setup

ALWAYS perform these steps in a clean clone before attempting builds:

1. Clone:
   git clone https://github.com/OldSkoolzRoolz/ai-assisted-it-manager.git
   cd ai-assisted-it-manager

2. Ensure required .NET SDK (target: .NET 9).  
   Check:
   dotnet --version  
   If mismatch (older major), install latest .NET 9 (Preview if before GA).

3. Windows PowerShell / Terminal environment:
   - Use PowerShell 7+ (pwsh).
   - Set execution policy if scripts blocked:
     Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

4. Run workspace setup script:
   pwsh onboarding/workspace-presets.ps1
   ALWAYS run this after first clone and after adding new projects requiring config.
   (This script provisions workspace presets, applies Defender exclusions, sets up symbolic links; do NOT skip.)

5. Restore solution dependencies (even if VS would auto-restore):
   dotnet restore ITCompanion.sln

6. (If database features needed) Confirm local SQL Server instance reachable. Provide (or create) a local connection string in user-secrets or appsettings.Development.json if project demands it. If unsure and build fails on missing connection string, create placeholder:
   Server=(localdb)\\MSSQLLocalDB;Database=ITCompanion;Trusted_Connection=True;Encrypt=True;

---

## 5. Build Instructions

Canonical build sequence (ALWAYS in this order after bootstrap):

1. dotnet restore ITCompanion.sln
2. dotnet build ITCompanion.sln -c Debug
3. (Optional release) dotnet build ITCompanion.sln -c Release

If build fails due to preview SDK mismatch, update Visual Studio 2022 (latest preview) and/or install matching .NET 9 preview.

If adding a new project:
- Create directory under src/
- dotnet new <template> -n <ProjectName> -o src/<ProjectName>
- dotnet sln ITCompanion.sln add src/<ProjectName>/<ProjectName>.csproj
- Then repeat restore + build sequence.

---

## 6. Running Applications

(Names may map to actual csproj names; follow these patterns.)

Desktop UI (ClientApp):
- From repo root:
  dotnet run -c Debug --project src/ClientApp/ClientApp.csproj
If a WinUI 3 migration exists, run from its project instead (e.g., ClientApp.WinUI). Only one UI host should be active; prefer WPF if both exist unless migration notes specify otherwise.

Self-Healing / Policy Engine (headless service):
  dotnet run --project src/CorePolicyEngine/CorePolicyEngine.csproj

Enterprise Dashboard (future / when implemented):
  dotnet run --project src/EnterpriseDashboard/EnterpriseDashboard.csproj
Expect it to host a Kestrel server; check console output for URL.

ALWAYS ensure required environment variables (e.g., connection strings, feature flags) are set before running. If not defined in project docs, use localdb placeholder.

---

## 7. Testing

Standard test invocation (from README):
  dotnet test tests/

Preferred verbose form (surface all results, skip build duplicates):
  dotnet test ITCompanion.sln --no-build --configuration Debug

ALWAYS run dotnet build first for deterministic behavior; dotnet test will otherwise restore & build implicitly, which can mask incremental issues.

If integration tests (e.g., DB or WMI) exist and are slow/flaky:
- Look for traits/categories (e.g., [Category("Integration")]) and optionally filter:
  dotnet test --filter TestCategory!=Integration

Add new tests in corresponding test project mirroring source project naming:
- src/CorePolicyEngine/ -> tests/CorePolicyEngine.Tests/
Ensure new test project is referenced in solution and uses the standard test framework (likely xUnit or MSTest; inspect existing test project for conventions before adding).

---

## 8. Linting / Formatting / Quality (Assumed Defaults)

If a dotnet format or analyzers configuration (Directory.Build.props / .editorconfig) exists, enforce locally:

Recommended pre-commit routine:
1. dotnet build
2. dotnet test
3. dotnet format (if tool is installed)
4. (Optional) dotnet pack (only for library packaging scenarios)

ALWAYS fix analyzer warnings introduced by new code—treat warnings as future risk even if not failing CI yet.

If no automated linting config present and you add one, keep it minimal and incremental.

---

## 9. Database / Migrations (If ITCompanionDB Active)

Look for a project (e.g., src/ITCompanionDB or database project). Typical patterns:
- EF Core migrations: dotnet ef migrations add <Name> --project src/ITCompanionDB
- Apply locally via application startup or:
  dotnet ef database update --project src/ITCompanionDB

ALWAYS update the solution + build before generating migrations to avoid stale model issues.

If migrations not yet implemented, do NOT unilaterally introduce EF—verify architectural intent in docs/ first.

---

## 10. Configuration & Secrets

Common config file hierarchy (expected):
- appsettings.json
- appsettings.Development.json
- user-secrets (for local secure credentials)
Initialize .NET user-secrets if sensitive info needed:
  dotnet user-secrets init --project src/<ProjectNeedingSecrets>

NEVER commit credentials. Use placeholders in committed config.

---

## 11. Adding New Code / Features

ALWAYS:
1. Identify correct module (e.g., parsing logic → CorePolicyEngine; UI interaction → ClientApp).
2. Add new classes with cohesive namespaces mirroring folder structure.
3. Update DI registration (if a central Startup/Program or composition root exists—search for Program.cs under each host project).
4. Add unit tests in corresponding test project before or alongside implementation.
5. Run full validation sequence (Section 14).

When modifying ADMX/ADML logic:
- Central parser utilities likely reside in CorePolicyEngine (look for Parser, Model, or Admx* classes).
- Keep performance in mind—avoid large synchronous UI-blocking operations; offload to background tasks if necessary.

---

## 12. Self-Healing / Policy Engine Notes

The SelfHealingPolicyEngine directory at root may represent a service or library. Before refactoring its placement into src/, ensure:
- Project reference integrity (update solution).
- Any scripts or docs pointing to old path are adjusted.

---

## 13. Docs & Onboarding Resources

- docs/ (architecture drafts) – ALWAYS inspect docs/ for design decisions when making changes.
- onboarding/README.md – lists module definitions; onboarding/setup-guide.md (consult for detailed environment prerequisites).
If you update architecture, reflect changes consistently across:
1. docs/ architecture diagrams/text
2. onboarding/ module overview
3. This instructions file (only if foundational process changes—avoid churn for minor refactors)

---

## 14. Canonical Validation Sequence (Run Before Opening PR)

ALWAYS execute in this exact order from a clean working tree (no uncommitted changes):

1. git fetch --all --prune
2. git switch -c feature/<short-descriptor> (or rebase from latest master before final push)
3. pwsh onboarding/workspace-presets.ps1            (ensures local prerequisites are refreshed)
4. dotnet restore ITCompanion.sln
5. dotnet build ITCompanion.sln -c Debug
6. dotnet test ITCompanion.sln --no-build
7. (If format tooling present) dotnet format --verify-no-changes
   - If changes needed: dotnet format (if formatting changes affect test files, re-run tests)
8. (If DB changes) apply migrations locally; verify startup of affected host project
9. Run primary executable (e.g., ClientApp) to smoke test
10. (If adding new API/service endpoints) exercise minimal functional path
11. git add .
12. git commit -m "feat: <concise summary>"
    - For complex changes, add a commit message body describing the rationale and impact:
      git commit -m "feat: <concise summary>" -m "<detailed explanation>"
    - If the change is breaking, include a footer in the body: BREAKING CHANGE: <description of breaking change>
13. git push -u origin feature/<short-descriptor>

NEVER skip steps 4–6. ALWAYS re-run steps 5–7 after resolving merge conflicts.

---

## 15. CI / Workflows (General Expectations)

Even though specific workflow YAML files are not enumerated here, assume CI will:
- Restore + build solution
- Run unit tests
- Possibly enforce formatting/analyzers
Design changes so they pass non-interactively (no UI prompts). If you add steps requiring secrets or services, gate them behind conditionals (e.g., only if env var present).

---

## 16. Common Pitfalls & Mitigations

Pitfall: Using wrong SDK version → Build failure referencing target frameworks.
Mitigation: Install matching .NET 9 SDK; run dotnet --info to verify.

Pitfall: Missing local SQL instance → Runtime exception on startup.
Mitigation: Provide fallback localdb connection string in development config.

Pitfall: Adding new project but forgetting solution inclusion → Tests/build skip code.
Mitigation: dotnet sln ITCompanion.sln list (verify presence) before pushing.

Pitfall: Long restore times after minor edits.
Mitigation: Avoid unnecessary global package version changes; keep restore deterministic.

Pitfall: UI project fails due to WinUI preview mismatch.
Mitigation: If WinUI not stabilized, keep WPF as default run target; do not upgrade without verifying docs.

---

## 17. Naming & Branching Conventions (Recommended)

- feature/<topic>, fix/<issue-number>, chore/<maintenance>
- Commit prefixes: feat:, fix:, refactor:, test:, docs:, chore:, perf:, build:
- Keep PRs focused (one feature/fix). Include test additions in same PR.

---

## 18. Extensibility Guidance

When introducing AI / ONNX components:
- Encapsulate model loading (e.g., IAnomalyDetectionService) behind interface
- Ensure fallback (no-op) implementation when model assets absent—prevents runtime crashes in minimal developer setups.

When adding policy parsing features:
- Maintain separation: Parsing (pure), Validation (rules), Deployment (side effects)

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

## 20. When to Search Outside This File

Only search the codebase if:
- A project path or command referenced here produces an error indicating the target does not exist.
- You need the exact name of a project inside src/ for a run/build command.
- You are adding code and must inspect existing patterns (DI setup, config binding, test conventions).

Otherwise TRUST THESE INSTRUCTIONS.

---

## 21. Minimal Quick Start (Copy/Paste)

```
git clone https://github.com/OldSkoolzRoolz/ai-assisted-it-manager.git
cd ai-assisted-it-manager
pwsh onboarding/workspace-presets.ps1
dotnet restore ITCompanion.sln
dotnet build ITCompanion.sln -c Debug
dotnet test ITCompanion.sln --no-build
dotnet run --project src/ClientApp/ClientApp.csproj
```

If any path differs, list src/ to identify correct project and adjust only that line.

---

## 22. Adding a New Feature (Example Workflow)

1. Create branch: git switch -c feature/policy-diff
2. Implement parser enhancement in src/CorePolicyEngine/ (new class + tests).
3. Add/Update tests in tests/CorePolicyEngine.Tests/
4. Run validation sequence (Section 14).
5. Push & open PR with concise description + affected modules.

---

## 23. Quality Bar

A change is "ready" ONLY if:
- Builds cleanly (no new warnings if avoidable).
- All tests pass (and new tests cover new logic).
- No hard-coded environment-only paths or credentials.
- UI or service still starts successfully after change.

---

By following this guide strictly you minimize failed CI runs and reduce unnecessary repository scanning. Trust these steps first; investigate only when concrete discrepancies arise.