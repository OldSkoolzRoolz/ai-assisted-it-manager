| 🗂️ **Field**           | **Value**                                         |
|-------------------------|---------------------------------------------------|
| **Date**                | 2025-08-25                                        |
| **Modified By**         | @KyleC69                                          |
| **Last Modified**       | 2025-08-25                                        |
| **Title**               | *AI copilot instructions*  |
| **Author**              | @KyleC69                          |
| **Document ID**         | AI-CODING-STD-001                                 |
| **Document Authority**  | @KyleC69                                          |
| **Version**             | 2025-08-25.v3.7                                     |



## **(ENFORCED) – 1. Build Instructions**
Canonical build sequence (ALWAYS in this order after bootstrap):

1. `dotnet restore ITCompanion.sln`  
2. `dotnet build ITCompanion.sln -c Debug -warnaserror /p:TreatWarningsAsErrors=true /p:AnalysisLevel=latest /p:EnforceCodeStyleInBuild=true`  
3. *(Optional release)* `dotnet build ITCompanion.sln -c Release -warnaserror /p:TreatWarningsAsErrors=true /p:AnalysisLevel=latest /p:EnforceCodeStyleInBuild=true`  
4. *(Analyzer formatting)* `dotnet format analyzers --verify-no-changes || dotnet format analyzers`

- Always ensure **zero warnings**.  
- If an upstream package triggers unavoidable warnings, add a **targeted** suppression with justification (never blanket disable without CODEOWNER approval).

---

## **(ENFORCED) – 2. Warning / Analyzer Enforcement Policy**
- Fail fast on any compiler/analyzer warning (treat as error).  
- Prefer code fixes over suppressions; suppress only with linked issue + rationale comment.  
- `.editorconfig` is authoritative for style; inline suppressions only if rule conflicts with generated code.

**Manual quick build (local):**
```powershell
dotnet clean ITCompanion.sln
DOTNET_NOLOGO=1 dotnet build ITCompanion.sln -c Debug -warnaserror /p:TreatWarningsAsErrors=true /p:AnalysisLevel=latest /p:EnforceCodeStyleInBuild=true
```

---

## **(ENFORCED) – 3. Coding Standards**
- Follow .NET naming conventions.  
- Use `async`/`await` for any potentially blocking op; no sync‑over‑async.  
- Public async APIs must take and pass through a `CancellationToken`.  
- Avoid `async void` except for UI/event handlers.  
- Only use `Task.Run` for genuine CPU‑bound offload.  
- Avoid fire‑and‑forget; if unavoidable, safely log exceptions.  
- Use `ConfigureAwait(false)` only in pure libraries.  
- Use `ValueTask` only when profiling shows benefit.  
- Prefer `SemaphoreSlim` for async critical sections.  
- Stream large payloads.  
- Validate arguments early with `nameof()`.  
- Prefer DI and interfaces; avoid concrete types in public APIs.  
- Use LINQ and immutable collections where feasible.  
- Follow SOLID; keep methods under ~30 lines.  
- Organize `using` statements: System → third‑party → project.  
- XML docs required for all members — see Adding New Code for tags.  
- Profile before optimizing.  
- Unit tests required for new logic — see Testing.

---

## **(ENFORCED) – 4. Testing**
- Build first with `-warnaserror` before running tests.  
- Preferred: `dotnet test ITCompanion.sln --no-build --configuration Debug`  
- Filter slow/flaky integration tests with traits if needed.  
- Mirror test project names/structure with source — see Adding New Code.

---

## **(FLEXIBLE) – 5. Database / Migrations**
- Check for `src/ITCompanionDB` or equivalent.  
- Create migration: `dotnet ef migrations add <Name> --project src/ITCompanionDB`  
- Apply: via app startup or `dotnet ef database update --project src/ITCompanionDB`  
- Build before migration to avoid stale models.  
- Test on fresh DB.  
- Document in `docs/migrations.md`; migrations should be idempotent/reversible.

---

## **(ENFORCED) – 6. Adding New Code / Features**
1. Identify correct module.  
2. Match namespace to folder structure.  
3. Update DI registration.  
4. Add tests per Testing section.  
5. Run full validation sequence.  
6. Avoid blocking UI thread.  
7. Extend rather than modify shared logic.  
8. **XML docs mandatory** — `<summary>`, `<param>`, `<returns>`, `<exception>`; update docs with signature/behavior changes; missing docs block PR.

---

## **(FLEXIBLE) – 7. Docs & Onboarding Resources**
- `docs/` has solution‑wide docs: architecture, design decisions, coding standards, build/test instructions.
- `onboarding/README.md` + `onboarding/setup-guide.md` cover module definitions and prerequisites.  
- Reflect architecture changes consistently across all documentation.
- `workarounds.md` for known issues and fixes. Document workarounds with context and links to issues/PRs. Include purpose and impact.
- Keep docs updated with code changes; outdated docs can mislead.
- Use diagrams for complex concepts.
- Encourage team contributions to docs.
- Review docs in PRs for accuracy and clarity.
- Use markdown linting tools to maintain quality.
- Consider a docs review cycle every few months.

---

## **(ENFORCED) – 8. File & Directory Quick Reference**
```
Root:
- ITCompanion.sln
- README.md
- docs/
- onboarding/
- src/
- tests/
- .gitignore / .gitattributes
```

---

## **(ENFORCED) – 9. Module Logging**
- Use Microsoft.Extensions.Logging for all logging.
- Configure logging in `Program.cs` or `Startup.cs` of each module.
- Use static LoggerMessages for high‑frequency logs to improve performance.
- Select log levels based on event severity.
- Avoid logging sensitive data; log strings should be resource‑based for localization.
- Prefer structured properties over string concatenation.
- Use source‑generated partial static logger classes named `<Area>Log` or `Logger` in the subsystem namespace.

### **9.1 Logging Event ID Schema (ENFORCED)**
Each subsystem owns a numeric EventId range; no overlaps. Update the schema when adding a subsystem.

| Range      | Subsystem / Purpose                                  | Notes |
|------------|------------------------------------------------------|-------|
| 1000–1099  | Security (authZ / access evaluation)                  | Already implemented in Security.Logging.Logger |
| 1100–1199  | Security (future integrity / tamper)                  | Reserved |
| 2000–2099  | Client UI Policy Editor (search, selection, catalog)  | Implemented in PolicyEditorLog |
| 2100–2199  | Client UI Log Viewer & Diagnostics                    | To be added |
| 3000–3099  | ADMX/ADML Parsing / Catalog Loader                     | Add source‑generated logger before extending |
| 3100–3199  | Validation Engine (rules execution)                   | Pending |
| 4000–4099  | Deployment / Registry application                     | DeploymentService future |
| 5000–5099  | Versioning / Snapshot / Diff                          | Future phase |
| 6000–6099  | Storage / Repository operations (SQL / persistence)   | Optional, keep low‑noise |
| 7000–7099  | Self‑Healing / Drift detection                        | Future phase |
| 9000–9099  | Critical / Fallback / Fatal recoverable states        | Use sparingly, page operators |

Guidelines:
- EventId ranges stable; do not renumber post‑release.
- One LoggerMessage method per semantic event.
- For CA1848: avoid direct `LogInformation($"...")` in new code unless justified.
- Static LoggerMessages should be `internal static partial void` in `Logger` class in `Logging` folder.
- Each module will control its own logger class and generate a separate log file.
- A debug logger will be added conditionally in debug builds for verbose tracing.

---

## **(FLEXIBLE) – 10. Adding a New LoggerMessage**
1. Pick next unused EventId in range.
2. Create/extend partial static class with `[LoggerMessage]`.
3. Descriptive parameter names.
4. Update schema if new range/category.
5. Include XML docs; mention EventId in summary when relevant.

---

## **(FLEXIBLE) – 11. Copilot Language Guidelines**
### clarity:
  - Avoid academic jargon; prefer plain English and analogies.
  - Short, direct sentences.
  - Explain acronyms/terms only when needed.

### tone for self taught:
  - Address a hands‑on, self‑taught developer.
  - Celebrate resourcefulness.
  - Avoid condescension.

### examples and explanation:
  - Use relatable, real‑world examples.
  - Break complex ideas into steps.
  - Lead with why before how.

### formatting:
  - Bullets, headings, and code blocks over dense text.
  - Highlight key takeaways.

### personality alignment:
  - Sound like a helpful peer.
  - Add light humor/empathy when appropriate.
  - Respect time, experience, and autonomy.


## 12. **Change Validation Checklist (ENFORCED)**

A change is "ready" ONLY if:
[ ] Builds cleanly (no warnings: enforced by -warnaserror / TreatWarningsAsErrors=true).
[ ] Resolve all intellisense issues using best practices as guide.
[ ] All tests pass (and new tests cover new logic).
[ ] No hard-coded environment-only paths or credentials.
[ ] UI or service still starts successfully after change.
[ ] Documentation changes include manifest updates and maintain technical accuracy.
[ ] Analyzer + code style passes: dotnet format analyzers/style --verify-no-changes.
[ ] XML documentation headers exist for all new or modified members (see Section 11).
[ ] Logging changes follow EventId schema (Section 9) and use static LoggerMessage.
[ ] No sync-over-async patterns introduced; async/await used properly with CancellationToken.
[ ] DI registrations updated if new services added.
[ ] No sensitive data logged or exposed.
[ ] Code adheres to coding standards (Section 3).
[ ] Relevant docs (architecture, onboarding) updated if design changes.
[ ] PR description includes summary of changes and references any related issues.
[ ] All steps outlined and requested by user completed fully (no deferrals). No unnecessary prompts.
[ ] If new dependencies added, ensure they are actively maintained and secure.
[ ] If DB changes, migrations tested on fresh instance and documented.
[ ] If existing public APIs modified, XML docs updated accordingly.
[ ] If performance impact likely, profiling or benchmarks included.
[ ] If security impact likely, threat model or risk assessment included.
[ ] If config changes, defaults documented and validated.
[ ] If UI changes, DataContext set and bindings verified.
[ ] If DB changes, migrations tested on fresh instance and documented.

---

## 13. **Localization & Globalization**

### **(ENFORCED) – 13.1 Localization & Globalization Standards**
	- All user-facing strings must be resource-based for localization.
	- Use `IStringLocalizer<T>` for dependency-injected localization.
	- Avoid hard-coded strings in UI, logs, exceptions, or messages.
	- Follow .NET globalization best practices for date, time, number formatting.
	- Test UI in different cultures to ensure layout and text fit.

---

### **Theming & UI Consistency** (ENFORCED)
	- Follow established theming guidelines for colors, fonts, and styles.
	- Use shared styles and resources to ensure consistency across modules.
	- Test UI changes in both light and dark modes if applicable.
	- Ensure accessibility standards are met (e.g., contrast ratios, keyboard navigation).
	- Refer to `docs/winuitheming.md` for detailed theming and accessibility standards.
	---

## **Redundancy & Conflict Notes**
- XML doc rule stated once in Coding Standards; referenced in Adding New Code.
- Test placement rule stated in Testing; referenced in Adding New Code.
- Build flags only in Build Instructions; Testing references them.
- No conflicting intent detected — all sections aligned.
- 