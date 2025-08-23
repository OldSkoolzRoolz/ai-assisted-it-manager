# WinUI Migration – Temporary Workarounds & Compatibility Notes

Status: Active (in?progress port from WPF ClientApp)
Last Updated: 2025-08-23
Maintainer: (Repository CODEOWNER) @KyleC69

> PURPOSE: Central ledger of every *temporary* workaround or deviation introduced while bringing up the WinUI 3 (Windows App SDK) client. This file MUST stay current so each item can later be re?evaluated and replaced with a durable solution. Do **not** let ad?hoc comments remain only in code.

---
## 1. Target Framework Alignment
- WinUI head project `ITCompanionClient` is currently targeting `net8.0-windows10.0.19041.0`.
- Core library `CorePolicyEngine` originally targeted only `net9.0-windows`; multi?targeting added: `net9.0-windows;net8.0-windows10.0.19041.0`.
- Rationale: Windows App SDK 1.7 officially supports .NET 8 at time of port; .NET 9 head caused NuGet incompatibility (NU1201) during restore.
- TODO: Monitor Windows App SDK release notes for formal .NET 9 support; when stable, remove net8 TFM (or invert if both required by downstream hosts).

## 2. UI Control Substitutions
| WPF Control | WinUI Status | Temporary Replacement | Notes |
|-------------|--------------|-----------------------|-------|
| `GroupBox`  | Not present in WinUI 3 | `Expander` + heading TextBlock | Visual style differs; future: custom reusable `GroupSection` control. |
| `DataGrid` (WPF) | No built?in equivalent | Planned: CommunityToolkit WinUI DataGrid OR `ItemsRepeater` + custom headers | Decide after performance spike measurement. |
| `ContextMenu` | Use `MenuFlyout` | Refactor on port of Policies view | Ensure keyboard accessibility parity. |
| `MessageBox` | Use `ContentDialog` | Replace during detail/push dialogs | Audit all existing direct calls in WPF layer. |

## 3. Theming & Resource Dictionaries
- Imported base brushes into `ITCompanionClient/Themes/*.xaml`.
- Currently merged: `BaseTheme.xaml`, `Theme.Light.xaml`, `Theme.Dark.xaml` but **no runtime theme switching hook yet**.
- WPF dynamic resources (e.g., `DynamicResource`) -> WinUI expects `ThemeResource` for theme?sensitive values.
- TODO: Introduce a `ThemeServiceWinUI` analog that toggles `Application.RequestedTheme` and ensures high contrast mode integration.

## 4. Analyzer Relaxation (TEMPORARY)
- Disabled analyzers (`TreatWarningsAsErrors=false`, analyzers off) *only* for WinUI project during bootstrap.
- Risk: Divergence in code quality; MUST re?enable before declaring WinUI preview ready.
- Action Item: Create tracking issue to re?enable with staged cleanup.

## 5. XAML Build / Item Inclusion
- Initial attempt manually declared `<Page Include=...>` which conflicted with implicit items ? removed manual entries.
- Lesson: Rely on implicit XAML includes unless build requires customization (e.g., source generators, exclude patterns).

## 6. Reflection / Access Patterns
- WPF `MainWindow` previously used reflection to reach private `PoliciesControl` for search dialog invocation; **DO NOT replicate** in WinUI.
- TODO: Introduce routed command or mediator/event aggregator for cross?component actions.

## 7. Policy Editing Sandbox (Forward Port Plan)
- Existing WPF sandbox + push audit flow will be re?implemented using `ContentDialog` and a dedicated `PolicyDetailPage` or inline panel.
- Ensure audit hooks (PolicyEdited, PolicyPush) survive translation; unify code via shared view models (extract if needed to a neutral project).

## 8. Multi?Target Impact Checklist
| Area | Risk | Mitigation |
|------|------|-----------|
| NuGet dependency drift | Different transitive graphs (net8 vs net9) | Run `dotnet list package --include-transitive` per TFM periodically. |
| Conditional compilation | Behavior forks | Avoid `#if NET8_0` unless absolutely necessary. |
| Performance | JIT differences | Benchmark after parity achieved. |

## 9. Known Gaps / Open Tasks
- [ ] Implement Policies page (navigation item stub currently placeholder text).
- [ ] Choose grid technology for policy list (Toolkit DataGrid vs custom repeater).
- [ ] Add search dialog using `ContentDialog`.
- [ ] Wire theme switching & high contrast detection.
- [ ] Replace any future `MessageBox` usages with `ContentDialog`.
- [ ] Re-enable analyzers & address warnings (create phased plan).
- [ ] Add automated UI smoke test (WinAppDriver / Playwright for Win32 window introspection) – optional stretch.
- [ ] Investigate why WinUI XAML compiler stopped generating entry point after project relocation (temporary Program.cs workaround in place).

## 10. Decision Log (Add Entries Chronologically)
| Date (UTC) | Decision | Reason | Revisit By |
|------------|----------|--------|-----------|
| 2025-08-23 | Multi-target CorePolicyEngine to net8/net9 | WinUI head needs net8 now | After WinAppSDK adds .NET 9 GA support |
| 2025-08-23 | Disable analyzers in WinUI project | Accelerate bootstrap | Before first public preview |
| 2025-08-23 | Replace GroupBox with Expander | Control not available | When custom section style added |
| 2025-08-23 | Add manual Program.cs entry point (WinUI) | XAML build not producing generated Main | After resolving XAML generation issue |

## 11. Maintenance Directive
> Every workaround above MUST be either: (A) removed, or (B) justified in a permanent design note. When resolving an item, update this file and append an entry to the Decision Log with the outcome.

Maintain this file alongside code changes affecting the WinUI client. **No workaround may linger undocumented.**

---
### How To Propose a Fix
1. Open an issue referencing the section number here.
2. Provide: root cause, proposed change, migration impact, rollback plan.
3. On merge: update this document (close checkbox, add decision log row).

---
*End of document.*
