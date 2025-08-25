# Temporary Localization Implementation Steps (To Be Merged Into Formal Docs)

Purpose: Reference checklist while refactoring modules for full localization support without code edits per new language.

## 1. Resource Template Conventions
- Use *Module*.*Area*.*SemanticName*_Template keys.
- Templates contain indexed placeholders: {0}, {1}, etc (not named) for maximum compatibility with ResX.
- Each template stands alone (no prefix+suffix composition) to allow reordering.

Example keys (Policy Editor):
- PolicyEditorInitialized_Template = "Policy editor initialized"
- SearchFilterApplied_Template = "Search filter applied: '{0}'"
- CatalogLoadFailed_Template = "Catalog load failed for language '{0}'"
- CatalogLoaded_Template = "Catalog '{0}' loaded: {1} policies in {2} ms"
- PolicyKeyNotFound_Template = "Policy key not found: {0}"
- PolicySelected_Template = "Policy selected: {0} settings={1}"
- PolicyGroupsLoadFailed_Template = "Failed loading policy groups"

## 2. Adding a New Culture
1. Copy base resx: `PolicyEditorLog.resx` -> `PolicyEditorLog.<culture>.resx` (e.g. PolicyEditorLog.fr.resx).
2. Translate only the values; preserve placeholder indexes.
3. Never add/remove placeholders or change count without updating code usage.
4. Validate with the localization test script (to be added) that all keys exist in each culture.

## 3. Code Access Pattern
- Use ResourceManager.GetString(key, CultureInfo.CurrentUICulture).
- Format with string.Format(CurrentUICulture, template, args...).
- Avoid partial prefix tokens; full sentence per template.

## 4. Logging Strategy
- For localization flexibility we accept losing structured property tokens in LoggerMessage source generator.
- EventId remains stable; message content localized.
- For metrics correlation, add structured context separately if required (future: logger.BeginScope with invariant properties).

## 5. UI Binding
- Introduce ILocalizationService (planned) to broadcast culture changes.
- XAML binding uses wrapper that re-fetches resources when culture changes.
- Culture switch triggers redraw without restart.

## 6. Validation & Errors
- Each rule gets a RuleId -> resource key mapping.
- Format dynamic portions via placeholders.

## 7. Tests (Planned)
- Reflection scan: ensure every *._Template key appears in each culture resx.
- Pseudo localization culture to detect truncation/RTL issues.

## 8. Future Enhancements
- Consider adopting ICU message formatting for pluralization and gender (library evaluation pending).
- Add tooling to export/import XLIFF for translators.

## 9. Migration Notes
- Old pattern using {Msg} + invariant suffix deprecated.
- Remove deprecated keys after confirming no runtime references.

---
Temporary file; integrate into ARCHITECTURE.md (Localization section) and remove once formalized.
