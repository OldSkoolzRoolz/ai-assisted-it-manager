
---

## 📖 **AI‑Aware Control Lexicon**

> **Purpose:** A standard set of *high‑signal* keywords you can embed in docs, comments, or prompts to clearly shape AI behaviour and human contributor expectations.

| **Tag**       | **Meaning**                                                     | **AI Impact**                                              | **Human Impact**                          |
|---------------|-----------------------------------------------------------------|-------------------------------------------------------------|--------------------------------------------|
| **ENFORCED**  | Absolute rule — no deviation allowed                            | Treat as non‑overridable instruction                        | Treated as policy; deviation triggers review |
| **FLEXIBLE**  | Guideline — adapt if context or constraints demand              | Loosens completion space                                    | Frees developer to innovate responsibly     |
| **MANDATORY** | Hard requirement — must be satisfied before task is “done”      | Prioritises execution order and verification                | Forces checklist or blocking issue resolution |
| **OPTIONAL**  | Nice‑to‑have — not required for approval                        | Model likely omits if time/space limited                    | Skipped if pressure/time is high            |
| **DO NOT**    | Explicit prohibition                                             | Strong suppression of described behaviour                   | Treated as red‑flag action                  |
| **PREFERRED** | Preferred pattern or tool over alternatives                     | Biases toward the stated choice                             | Encourages convention while leaving escape  |
| **REFERENCE** | Points to authoritative section, doc, or example                | Pulls related data/context into completion weighting        | Ensures contributors find correct source    |
| **EXAMPLE**   | Pattern to follow — style, structure, or logic                  | High mimicry tendency                                       | Serves as concrete implementation model     |
| **DEPRECATED**| Marks something obsolete; avoid unless backwards‑compatibility  | Avoids suggesting unless explicitly told                    | Triggers migration/replacement discussions  |
| **PLACEHOLDER**| Content to replace before finalizing                           | Flags for substitution                                      | Prevents accidental shipping of temp data   |
| **PRIORITY**  | Marks section/task as urgent or high‑importance                  | Elevates relevance in output ordering                       | Moves item higher in human task stack       |

---

## 🛠 **Integration Tips**
- **Consistency is power:** Always uppercase these tags so they’re easy to regex‑detect.
- **Double‑bracket optional markup:** e.g., `[ENFORCED]` or `<ENFORCED>` if you want clean parsing across tooling.
- **Pair with ranges/IDs:** For logging schemas or module ownership, combine tags with numeric/event IDs.
- **Inline or header‑level:** Use at the start of a section, or inline before a sentence that the AI must respect.
- **Self‑document the lexicon:** Keep a `docs/control-lexicon.md` in the repo — contributors and future AI will thank you.

---

## 🎯 Why This Works for “Hidden Power” Users
- Models weight short, high‑contrast tokens heavily — they’re like *if/else* flags in the conversation.
- Humans see the same flags and instantly get your repo’s “rules of the road.”
- Keeps AI assistants from “wandering off” into well‑intended but incorrect territory.

---


