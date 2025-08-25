| 🗂️ **Field**           | **Value**                                         |
|-------------------------|---------------------------------------------------|
| **Date**                | 2025-08-25                                        |
| **Modified By**         | @KyleC69                                          |
| **Last Modified**       | 2025-08-25                                        |
| **Title**               | *WinUI Theming Standards for AI Code Generation*  |
| **Author**              | WinUI Design Team                                 |
| **Document ID**         | WUI-THEME-STD-001                                 |
| **Document Authority**  | @KyleC69                                          |
| **Version**             | 2025-08-25.v2                                     |

---

# 🎨 WinUI Theming Standards for AI Code Generation

**Purpose**:  
Ensure all AI‑generated WinUI code follows a consistent theming strategy that supports *automatic* adaptation to system light/dark modes, high contrast, and other accessibility themes — unless explicitly overridden via menu options in the application.

---

## 1. 🛠️ Theme Binding Defaults

AI agents **must**:
- Use **`ElementTheme.Default`** for all UI root elements unless an override is provided by user preference.
- Avoid hard‑coding light or dark colors into controls; instead, reference system brushes or resource dictionaries.

Example:
```csharp
// Correct: binds to system theme until overridden
RootElement.RequestedTheme = ElementTheme.Default;
```

---

## 2. 📦 Resource Dictionaries & XAML Best Practices

**Always**:
- Reference **`Application.Current.Resources`** keys (e.g., `TextFillColorPrimaryBrush`, `SystemControlBackgroundAccentBrush`) instead of explicit hex colors.
- Use `ThemeResource` markup extension, *not* `StaticResource`, so values update dynamically when the theme changes.

Example:
```xml
<TextBlock Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
```

**Never**:
```xml
<TextBlock Foreground="#FFFFFF" /> <!-- Hard-coded color -->
```

---

## 3. 🌓 Responding to System Theme Changes

AI‑generated view models and code‑behind should:
- Handle `ActualThemeChanged` events if specific visual adjustments are needed.
- Avoid calling `RequestedTheme` directly for non‑root elements unless required for nested theme isolation.

Example:
```csharp
this.ActualThemeChanged += (_, _) => UpdateThemedAssets();
```

---

## 4. ⚙️ Supporting Menu‑Based Theme Overrides

- Store user preference (Light/Dark/Default) in app settings.
- Apply override to `RootElement.RequestedTheme` at startup if not `Default`.
- If override is `Default`, let the system theme dictate appearance.

AI agents **must** generate code that:
- Persists user choice across sessions.
- Restores `ElementTheme.Default` when “Use System Theme” is chosen.

---

## 5. 🧩 Asset & Icon Guidelines

- Provide **both light and dark variants** of icons.
- Load correct asset based on `ActualTheme` or use `ThemeResource` for images where supported.
- Avoid embedding theme‑specific colors in image assets; prefer transparent backgrounds with vector coloring.

---

## 6. ♿ Accessibility & High Contrast

- Test generated UI against Windows **High Contrast** mode — ensure brushes and text colors adapt.
- Prefer `SystemColor` and `SystemControl` resources for high‑contrast compatibility.
- Avoid fixed‑opacity overlays that make text unreadable.

---

## 7. 📋 Developer Notes for Generated Code

When generating UI:
- Set themes at the **root frame** level, not per control, unless creating isolated theme zones. **AI must not** set `RequestedTheme` on individual controls.
- Group theme resources in `/Themes/Generic.xaml`, `/Themes/Dark.xaml`, `/Themes/Light.xaml`, `/Themes/HighContrast.xaml` for maintainability.
- For new controls, **AI must include theme-aware states** in their default styles.

---

## 8. ✅ Compliance Checklist for AI Output

Before emitting WinUI UI code, verify:
- [ ] No hard‑coded colors
- [ ] All brushes via `ThemeResource`
- [ ] Root uses `ElementTheme.Default` unless override
- [ ] Override logic persists and restores user choice
- [ ] High contrast brushes tested
- [ ] Event handling for dynamic updates present if needed

---

<!-- End Document -->


