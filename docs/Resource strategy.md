## Resource strategy
- One neutral (en) .resx per functional area (CorePolicyEngine, ClientShared, Security, etc.) plus satellite culture .resx (e.g., PolicyEditorLog.fr.resx).
- No literals in code (UI labels, errors, log prefixes, validation messages, menu items, telemetry field captions).
- Keep resource keys stable, semantic (PolicyEditor_CatalogLoaded_Template).
- Support full templates (include placeholders) inside resources, not just prefixes.

## Logging
- Replace {Msg} pattern with full resource templates containing named placeholders: e.g. “Catalog {LanguageTag} loaded: {PolicyCount} policies in {ElapsedMs} ms”.
- Pre-resolve template: string.Format(CultureInfo.CurrentUICulture, template, …) OR use interpolated handlers if custom.
- For high-volume debug logs keep invariant template + optional localized variant flag.

## Formatting & culture data
- Always pass CultureInfo for NumberFormat / DateTime / casing (ToString(culture), string.Compare with culture + options).
- Avoid string.ToUpper() without culture; use .ToUpperInvariant() when logic, culture-aware when UI.

## Plurals / grammar
- For English-only plural logic avoid hardcoding; prepare for ICU-style patterns or adopt SmartFormat / Humanizer (wrap to isolate dependency).
- Store separate singular/plural keys if not using ICU.

## UI integration (WinUI)
- Central ILocalizationService exposing CurrentUICulture + event CultureChanged.
- XAML bindings to a ResourceWrapper (INotifyPropertyChanged) so culture change triggers UI refresh without restart.
- No hardcoded strings in XAML: use {x:Bind Loc["PolicyEditor_Title"]}.
- Dynamic reload: when culture changes set Thread.CurrentUICulture/UI, raise refresh event.

## Dynamic culture discovery
- At startup enumerate available satellite assemblies (Directory.GetFiles for *.resources.dll) to build language menu.
- Fallback chain: Requested -> Parent -> Neutral -> Invariant.

## Validation & error messages
- Each validation rule carries a resource key (RuleId -> message template).
- Inject culture when composing user-facing text; keep internal logs invariant if needed.

## Data / config
- Avoid embedding localized text in persisted configs or DB rows; store keys or codes and resolve at display time.

## Testing
- Pseudo-localization culture (e.g., qps-PLOC) run: expand strings, surround with markers to catch truncation.
- Unit tests for: missing resource keys (reflection scan), fallback correctness, culture switch live refresh.
- Snapshot tests per culture for critical user flows (e.g., catalog load banner).

## Build & packaging
- Treat resources as satellite assemblies automatically built by msbuild (no custom copy).
- Optional: separate resource-only NuGet for community language packs (assembly probing path).
- CI step: run tool that verifies every neutral key exists in each shipped culture; warn for drift.

## Contribution workflow
- Export neutral keys to .csv/.xliff for translators (use dotnet resgen / 3rd party).
- Maintain a TranslationStatus.json (key -> list of cultures available, last modified) to surface gaps.

## Performance
- Cache ResourceManager lookups (string table snapshot) per culture; reduce per-log GetString calls for hot paths.
- Warm up selected culture at app init.

## Security / injection
- Do not log raw translator-supplied content as templates (sanitize placeholders only).
- Validate translator-supplied format strings (limit { } placeholders to known set).

## Future extensibility
- Reserve resource namespace prefix per module (CorePolicy., PolicyEditor., Security.*) to avoid collisions.
- Abstract formatting pipeline so adopting ICU MessageFormat later doesn’t change call sites.

## Migration from current pattern
- Replace {Msg}+suffix events with full resource templates.
- Provide transitional wrapper that logs both old invariant and new localized (feature flag) to compare.

## Checklist before adding new language
- Add Culture-specific .resx for each base resource file.
- Run resource key diff script (no missing keys).
- Execute pseudo-localization tests.
- Validate UI layout (no clipping, right-to-left if applicable).
- Update manifest (DOCUMENTATION_VERSION_MANIFEST) with “Localization: Added <culture>”.
Following this, adding a language = drop in new .resx files, rebuild, done (no source edits). Let me know if you want the refactor of current logging file now.
