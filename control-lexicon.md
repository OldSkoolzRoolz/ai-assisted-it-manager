Here’s the **share‑ready version** — written in your clean, repo‑friendly style — so other coders can drop it into their own projects like a productivity cheat code.  
It’s designed to be **human‑legible** and **AI‑parsable**, without clutter.

---

```markdown
# AI‑Aware Control Lexicon

> **Purpose:** Define a small set of high‑signal keywords that influence both human contributors and AI assistants.  
> Use consistently in documentation, code comments, and prompts to shape behaviour with zero ambiguity.

---

## 1. Tag Definitions

| **Tag**       | **Meaning**                                                     | **AI Behaviour**                                           | **Human Behaviour**                       |
|---------------|-----------------------------------------------------------------|------------------------------------------------------------|--------------------------------------------|
| ENFORCED      | Absolute rule — no deviation allowed                            | Treats as non‑overridable instruction                      | Policy; deviation triggers review          |
| FLEXIBLE      | Guideline — adapt if context or constraints demand              | Loosens interpretation space                               | Permits responsible innovation             |
| MANDATORY     | Hard requirement — must be satisfied before “done”              | Prioritises execution order and verification               | Forces checklist/blocker resolution        |
| OPTIONAL      | Nice‑to‑have — not required for approval                        | Likely omitted if time/space constrained                   | Skipped under pressure                     |
| DO NOT        | Explicit prohibition                                             | Strong suppression of prohibited action                    | Red‑flag action                            |
| PREFERRED     | Preferred pattern/tool over alternatives                        | Bias toward stated choice                                  | Encourages convention, allows exceptions   |
| REFERENCE     | Points to authoritative section/doc/example                     | Pulls related context into completions                     | Directs to correct source                  |
| EXAMPLE       | Pattern to mirror for style/logic                               | High mimicry tendency                                      | Concrete implementation model              |
| DEPRECATED    | Obsolete; avoid unless backwards‑compatibility is needed        | Avoids suggestion unless explicitly instructed             | Triggers migration/replacement discussion  |
| PLACEHOLDER   | Temporary content to be replaced                                | Flags for substitution                                     | Prevents shipping incomplete content       |
| PRIORITY      | Marks item as urgent/high‑importance                            | Elevates relevance in output ordering                      | Moves item up task stack                   |

---

## 2. Usage Guidelines

- **Always uppercase** tags for consistency and regex‑friendly parsing.
- Apply tags **at section headers** *or* inline to mark specific instructions.
- Combine with IDs/ranges for traceable policies (e.g., `ENFORCED [BUILD‑001]`).
- Keep in `docs/control-lexicon.md` so every contributor and AI assistant sees the same definitions.
- For AI‑assisted repos, wrap in brackets or pseudo‑XML for extra clarity:  
  `[ENFORCED]`, `<FLEXIBLE>`, etc.

---

## 3. Example – In a Build Guide

```markdown
## [ENFORCED] Build Sequence
1. dotnet restore ITCompanion.sln
2. dotnet build ITCompanion.sln -c Debug -warnaserror



## [FLEXIBLE] Local Debugging
You may skip analyser checks when exploring an experimental branch.

```



