# Retro Easter Eggs

Optional retro / nostalgic UI behaviors toggled manually. OFF by default and **no impact** on core functionality.

## 1. Retro Splash Screen
ANSI-green, pseudo "tape drive" style startup sequence with staged status messages.

### Enable at Startup (Developer Only)
Uncomment the `ShowSplash();` line in `App.xaml.cs` (marked "Retro splash (developer manual toggle)"). Commit should keep it commented.

### Manual Launch (Runtime)
Press `Ctrl + Alt + Shift + F12` in the main window to open the splash screen non-modally at any time. (Obscure combo to avoid collisions.)

### Disable
Re-comment the `ShowSplash();` line and avoid pressing the key combination.

### Notes
ANSI-green, pseudo "tape drive" style startup sequence with staged status messages.

### Enable at Startup (Developer Only)
Uncomment the `ShowSplash();` line in `App.xaml.cs` (marked "Retro splash (developer manual toggle)"). Commit should keep it commented.

### Manual Launch (Runtime)
Press `Ctrl + Alt + Shift + F12` in the main window to open the splash screen non-modally at any time. (Obscure combo to avoid collisions.)

### Disable
Re-comment the `ShowSplash();` line and avoid pressing the key combination.

### Notes
- Startup splash is modal (blocks until script completes). Manual splash is non-modal.
- Logging initializes before splash so early events are captured.
- No environment variables, registry keys, or external integration involved.
## 2. Future Ideas (Not Implemented Yet)
| Idea | Description |
|------|-------------|
| CRT Raster Theme | Scanline overlay + phosphor palette |
| ASCII Policy Diff | ASCII art style diff preview in diagnostics |
| Chiptune Alert | Short chiptune on successful deployment |

When implementing new eggs:
1. Keep strictly opt-in with clear code comments.
2. Update this document.
3. Avoid any external system dependencies.

---
Document Version: 2025-08-22.v3
Maintainer: @KyleC69
