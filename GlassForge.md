---
type: project
date: 2026-05-23
project: GlassForge
tags:
  - glassforge
  - windows-customization
  - dwm
  - shell
  - theming
  - aero-glass
status: planning
---

# GlassForge

> Bring Nexus's Crystal Glass engine to the Windows 11 shell — taskbar, title bars, Start menu, and every window on your desktop.

## Vision

GlassForge is a standalone Windows 11 shell customization tool that extracts the proven theming engine from [[NexusSystemMonitor/NexusSystemMonitor|Nexus System Monitor]] and applies it to the OS itself. Where Nexus styled its own window, GlassForge reaches into the Windows shell — giving every app on the desktop wallpaper-adaptive transparency, curated theme presets, per-surface accent colors, and specular shimmer effects without requiring the user to modify system files or disable security features.

The goal: a coherent, designer-quality Windows 11 theming experience that feels like it should have shipped with the OS.

## Design Principles

> [!important] These three principles override convenience at every decision point.

### 1. Future-Proof by Design

Every Windows build can change DWM internals, rename window classes, or deprecate undocumented APIs. GlassForge treats this as a certainty, not a risk.

- **API abstraction layer** — All Win32/DWM calls go through a `IShellBackend` interface. Each Windows build range (22H2, 23H2, 24H2, 25H2+) gets its own implementation. When a new Windows version ships, add a new backend — existing ones stay untouched.
- **Runtime Windows build detection** — `RtlGetVersion` (not `Environment.OSVersion`, which lies) checks the exact build number at startup. GlassForge selects the correct `IShellBackend` automatically and disables features that don't exist on the running build rather than crashing.
- **Documented vs undocumented API separation** — Documented APIs (`DwmSetWindowAttribute` with official `DWMWA_*` values) live in a stable module. Undocumented APIs (`SetWindowCompositionAttribute`, internal DWM structs) live in a separate, versioned module with clear "this may break" boundaries. If an undocumented call fails, GlassForge falls back to the documented path gracefully.
- **Feature capability probing** — Before applying any effect, probe whether it actually works on the current system (call it, check the result, verify visually via `DwmGetWindowAttribute` round-trip). Cache the capability map at startup. Don't assume — verify.
- **No DLL injection, no hooking explorer.exe** — These techniques break on every major Windows update. Stick to public window messages, `SetWinEventHook`, and `DwmSetWindowAttribute`. Fewer capabilities but dramatically higher survivability.

### 2. Auto-Adapt to Windows Updates

GlassForge should detect Windows changes and respond — not wait for a manual app update.

- **Windows Update detection** — Monitor `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\UBR` (Update Build Revision) via registry change notification. When the UBR changes after a Windows update, re-run capability probing and switch `IShellBackend` if the build range changed.
- **Self-healing on feature breakage** — If an applied effect stops working (e.g., `DwmSetWindowAttribute` returns `E_INVALIDARG` after an update), automatically disable that specific feature, notify the user via tray balloon, and log the failure with the new build number for diagnostics.
- **Remote compatibility manifest** — Ship a `compat.json` embedded in the app, but also check a hosted version on GitHub (raw file in the repo, no server needed). This manifest maps Windows build numbers → known broken/working APIs → recommended backend. The app can auto-adapt to a new Windows release before a full GlassForge update ships.
- **Lightweight auto-updater** — Check GitHub Releases API on startup (once per 24h, cached). If a newer version exists, show a tray notification with one-click update. Download the new installer to `%TEMP%`, verify hash, launch, and exit. No background update service — just a single HTTP check.
- **Telemetry-free diagnostics** — On capability probe failure, write a local `diagnostics.json` with build number, failed APIs, and fallback actions taken. Users can optionally share this file when reporting issues. No phone-home.

### 3. Lightweight and Universal

GlassForge runs in the background 24/7. Every byte of RAM and every CPU cycle matters.

- **Target footprint: <15 MB RAM, <0.1% CPU idle** — The background service should be nearly invisible. For reference, TranslucentTB uses ~8 MB.
- **No background UI rendering** — The WPF settings window is created on demand (tray icon → Settings click) and fully disposed on close. The background process is a headless tray app with zero WPF overhead when the settings window isn't open.
- **Event-driven, not polling** — Use `SetWinEventHook` (OS callback) for new windows, `RegNotifyChangeKeyValue` for registry changes, `SHChangeNotifyRegister` for wallpaper changes. The only timer is the optional auto-update check (once per 24h). Zero polling loops.
- **Single-file publish** — Ship as a single `.exe` via .NET 8 `PublishSingleFile` + `PublishTrimmed`. No runtime install required. Trim removes unused framework code, keeping the binary small.
- **Lazy SkiaSharp loading** — SkiaSharp is the heaviest dependency (wallpaper luminance analysis). Load it only when wallpaper-adaptive mode is enabled, and release it after analysis completes. Don't keep the SkiaSharp native library resident if smart tint is off.
- **Efficient window enumeration** — Cache the HWND → rule mapping. Only re-evaluate when a new window appears (`EVENT_OBJECT_CREATE`) or a window is destroyed (`EVENT_OBJECT_DESTROY`). Never re-enumerate all windows on a timer.
- **No COM, no WMI** — These are heavyweight and leak-prone. Use direct P/Invoke for everything. Registry reads are cheaper than WMI queries by orders of magnitude.

## Competitive Landscape

| Tool | Taskbar Glass | Title Bar Control | Theme Presets | Wallpaper Adaptive | Specular FX | Free |
|---|---|---|---|---|---|---|
| **TranslucentTB** | Yes (taskbar only) | No | Basic | No | No | Yes |
| **MicaForEveryone** | No | Yes (Mica/Acrylic per app) | No | No | No | Yes |
| **ExplorerPatcher** | Partial | Partial | No | No | No | Yes |
| **StartAllBack** | Yes | Partial | No | No | No | No (paid) |
| **Windhawk** | Via mods | Via mods | No | No | No | Yes |
| **GlassForge** | Yes | Yes | 19 built-in + custom | Yes | Yes | Yes |

**The gap:** No existing tool combines wallpaper-adaptive luminance-aware glass, curated theme presets with per-surface-zone color control, and specular/prismatic effects into a single coherent app.

## Nexus DNA — What to Extract

These systems exist in Nexus and transfer directly to GlassForge with minimal modification:

| System | Nexus Source | Notes |
|---|---|---|
| `ThemePreset` model | `Core/Models/ThemePreset.cs` | 13 properties, zero Avalonia dependencies |
| `BuiltInThemePresets` | `Core/Themes/BuiltInThemePresets.cs` | 19 curated presets ready to port |
| `SurfaceSwatchPalettes` | `Core/Themes/SurfaceSwatchPalettes.cs` | 16 palette sets, 8 colors each |
| `ThemePresetService` | `Core/Themes/ThemePresetService.cs` | JSON persistence, custom preset CRUD |
| `WindowsWallpaperService` | `Platform.Windows/WindowsWallpaperService.cs` | Registry watch + FileSystemWatcher |
| `WallpaperLuminanceAnalyzer` | `UI/Services/WallpaperLuminanceAnalyzer.cs` | SkiaSharp BT.601 luminance sampling |
| `GlassAdaptiveService` | `UI/Services/GlassAdaptiveService.cs` | Luminance-to-alpha mapping logic |
| Specular tracking | `UI/MainWindow.axaml.cs` (lines 338–426) | Pointer-following gradient highlight |
| Prismatic shimmer | `UI/MainWindow.axaml.cs` (lines 37–42) | 21-second rotating gradient animation |

All Nexus source lives at `O:\ObsidianVaults\Ideaverse\Areas\Projects\NexusSystemMonitor\`.

## Tech Stack

| Layer | Technology | Reason |
|---|---|---|
| Settings UI | WPF / .NET 8 / Windows 10.0.17763+ | Native WindowChrome, SystemBackdrop, demand-created and fully disposed on close |
| Theme engine | Shared C# class library (.NET 8) | Extracted from Nexus Core — zero Avalonia deps |
| Shell abstraction | `IShellBackend` + per-build implementations | Isolates undocumented APIs behind a versioned interface; swap backends without touching the rest |
| Shell hooks | C# P/Invoke to Win32 / DWM / UxTheme | MicaForEveryone proves this works; no C++ toolchain, no DLL injection |
| Build detection | `RtlGetVersion` P/Invoke | Returns the true build number (not the lies of `Environment.OSVersion`) |
| Capability probing | `CapabilityMap` (startup + post-update) | Disables unavailable features gracefully rather than crashing |
| Wallpaper analysis | SkiaSharp (lazy-loaded) | Only loaded when smart tint is enabled; released after analysis |
| System tray | Hardcodet.NotifyIcon.Wpf | Headless background process — zero WPF overhead when settings window is closed |
| Update check | GitHub Releases API + local `compat.json` | One HTTP call per 24h; remote manifest for API compat without requiring a full update |
| Settings storage | JSON → `%AppData%\GlassForge\` | Same pattern as Nexus `custom-themes.json` |
| Distribution | Single-file publish (`PublishSingleFile` + `PublishTrimmed`) | No runtime install required; trim removes unused framework code |
| Installer | Inno Setup | Same tooling as Nexus |

> [!note] Why WPF over Avalonia
> This app is Windows 11 only — cross-platform support adds no value. WPF has native `SystemParameters`, `WindowChrome`, and `DwmSetWindowAttribute` integration, and the extracted Nexus Core models have zero Avalonia dependencies, so they slot directly into WPF without changes.

## Architecture

```
GlassForge.sln
src/
  GlassForge.Core/           # Theme models, presets, palettes, adaptive services (no OS deps)
  GlassForge.Shell/          # IShellBackend interface + per-build implementations, P/Invoke, tray
    Backends/
      ShellBackend_22H2.cs   # Win11 22H2 (build 22621)
      ShellBackend_23H2.cs   # Win11 23H2 (build 22631)
      ShellBackend_24H2.cs   # Win11 24H2 (build 26100)
      ShellBackend_Future.cs # Forward-compatible fallback for unknown future builds
    CapabilityMap.cs         # Startup probe + post-update re-probe
    WindowTracker.cs         # SetWinEventHook-based HWND cache
    CompatManifest.cs        # Local + remote compat.json loader
  GlassForge.UI/             # WPF settings window — demand-created, fully disposed on close
tests/
  GlassForge.Tests/          # Unit tests for Core + Shell logic (no OS required)
```

**Key design rules:**
- `GlassForge.Core` is dependency-free — no WPF, no Win32, no SkiaSharp. Fully unit-testable.
- `GlassForge.Shell` never calls undocumented APIs directly — they go through the active `IShellBackend`. If a backend's API probe fails at runtime, it marks that capability as unavailable and the rest of the system routes around it.
- `GlassForge.UI` is never instantiated at startup — only when the user opens settings. The tray host process is headless.

## Key Features

### Taskbar
- Backdrop modes: Transparent, Blur, Acrylic, Mica, Opaque
- Opacity slider (0.0–1.0)
- Accent color tint from active theme preset
- Support for primary + secondary taskbars (`Shell_SecondaryTrayWnd`)
- Auto-re-apply after Explorer restart (`TaskbarCreated` message)

### Title Bars
- Per-app or global Mica / Acrylic / None backdrop
- Caption color override (`DWMWA_CAPTION_COLOR`)
- Border color override (`DWMWA_BORDER_COLOR`)
- Dark/light mode override (`DWMWA_USE_IMMERSIVE_DARK_MODE`)
- Rule editor: match by process name or window class

### Start Menu
- Backdrop mode and opacity control
- Accent tint (feasibility dependent on Windows version)

### Window Frames
- Global backdrop type via `DWMWA_SYSTEMBACKDROP_TYPE` (Mica, Mica Alt, Acrylic, Tabbed)
- Exclusion list for apps that break with forced backdrops
- Applied on window creation via `SetWinEventHook(EVENT_OBJECT_CREATE)`

### Global
- Wallpaper-adaptive smart tint (dark wallpaper → more transparent, bright → more opaque)
- 19 built-in theme presets + unlimited user-created custom presets
- System tray quick-switch submenu for instant preset changes
- Global hotkey for cycling presets (configurable)
- Startup with Windows (registry `Run` key + `StartupApproved` 12-byte pattern — already solved in Nexus)
- Import / export themes as `.json` files

## Key Win32 APIs

```csharp
// Backdrop types (Win11 22H2+)
DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ...)   // Mica, Acrylic, Tabbed

// Title bar colors
DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ...)
DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ...)
DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ...)

// Taskbar transparency (undocumented, used by TranslucentTB)
SetWindowCompositionAttribute(hwnd, ref WINCOMPATTR data)

// New window detection
SetWinEventHook(EVENT_OBJECT_CREATE, ...)

// Enumerate existing windows on startup
EnumWindows(callback, IntPtr.Zero)

// System theme / accent color
HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize
HKCU\SOFTWARE\Microsoft\Windows\DWM
```

## Technical Risks and Mitigations

> [!warning] Undocumented API Surface (`SetWindowCompositionAttribute`)
> Undocumented and has broken with Windows Insider builds before.
> **Mitigation:** Lives exclusively in `ShellBackend_*` implementations behind `IShellBackend`. Capability probe on startup + after each Windows Update (UBR watch). On failure: disable the specific feature, surface a tray notification, log build + error. Remote `compat.json` on GitHub can mark it broken for a specific build range and redirect to a documented fallback — without requiring a GlassForge update. Monitor TranslucentTB's issue tracker as a canary.

> [!warning] UWP / WinUI3 Resistance
> Some UWP and WinUI3 apps use their own compositor and silently ignore `DwmSetWindowAttribute`.
> **Mitigation:** `CapabilityMap` round-trips each `DwmSetWindowAttribute` call with a `DwmGetWindowAttribute` read-back to verify the change actually applied. Apps that don't respond are auto-added to the exclusion list rather than showing incorrect UI.

> [!warning] Windows Update Silently Breaking Effects
> An update may cause effects to stop working with no error — the API succeeds but the visual is gone.
> **Mitigation:** UBR registry watcher (`RegNotifyChangeKeyValue`) triggers a re-probe when UBR changes. Self-healing: re-apply all active effects after the probe; if the visual verify fails, fall back and notify. The `compat.json` remote manifest provides a fast-path fix before a full update ships.

> [!warning] Elevation Requirements
> Startup with Windows via `HKCU\Run` works without elevation. Any `HKLM` write needs elevation.
> **Mitigation:** Run without elevation by default — all core features use `HKCU`. Optional elevated helper process (separate small binary, launched via `ShellExecute` with `runas`) for admin-only operations. Never request elevation for the main process.

> [!warning] Resource Regression
> Background process drifting above target footprint (<15 MB RAM, <0.1% CPU) as features accumulate.
> **Mitigation:** Track `WorkingSet64` and CPU samples in `diagnostics.json` in debug builds. Enforce the event-driven constraint — no polling loops pass code review. Lazy-load SkiaSharp; release after wallpaper analysis completes.

## Phased Roadmap

### v0.1.0 — Foundation + Infrastructure
- Solution scaffold: Core, Shell, UI projects with folder structure above
- `RtlGetVersion` build detection + `IShellBackend` interface + per-build backend stubs
- `CapabilityMap` startup probe (all backends return "not available" until implemented in later phases — probe infrastructure established now)
- UBR registry watcher (`RegNotifyChangeKeyValue`) → triggers re-probe + re-apply on Windows Update
- Remote `compat.json` loader (check GitHub raw URL on first run + once per 24h; fall back to embedded copy)
- Lightweight auto-updater: GitHub Releases API check → tray notification → download to `%TEMP%` → hash verify → launch + exit
- Extract and verify Nexus theme models (ThemePreset, BuiltInThemePresets, SurfaceSwatchPalettes, ThemePresetService)
- Headless tray host process (no WPF at startup); demand-create settings window on tray click
- Settings persistence to `%AppData%\GlassForge\config.json`
- `diagnostics.json` writer (build number, capability map, failed APIs, WorkingSet64 sample)
- No visual shell modification yet — infrastructure only

### v0.2.0 — Taskbar Glass
- `SetWindowCompositionAttribute` P/Invoke
- Taskbar backdrop: Transparent, Blur, Acrylic, Opaque
- Opacity slider applied to taskbar
- Secondary taskbar support
- Re-apply on Explorer restart

### v0.3.0 — Title Bar Customization
- `DwmSetWindowAttribute` for caption color, border color, dark mode override
- Global rule + per-app rules (process name / window class)
- `SetWinEventHook` for new window detection
- `EnumWindows` on startup
- Rule editor UI

### v0.4.0 — Mica / Acrylic for All Windows
- `DWMWA_SYSTEMBACKDROP_TYPE` applied per rule
- Exclusion list editor
- Window class + process name matching

### v0.5.0 — Smart Tint + Wallpaper Adaptive
- Port `WindowsWallpaperService`, `WallpaperLuminanceAnalyzer`, `GlassAdaptiveService` from Nexus
- Real-time wallpaper change detection
- User toggle: auto-adapt vs manual opacity

### v0.6.0 — Theme Presets + Quick Switch
- Full preset system: 19 built-ins + user custom
- Tray submenu for instant preset switching
- Global hotkey for cycling presets
- Import / export themes as JSON

### v0.7.0 — Start Menu + Polish
- Start menu backdrop (feasibility dependent; `compat.json` gates this per build)
- Live preview in settings window
- Startup with Windows (`HKCU\Run` + `StartupApproved` pattern from Nexus)
- Resource footprint audit: verify <15 MB RAM / <0.1% CPU idle against real usage data

### v1.0.0 — Release
- README + screenshots + demo GIF
- Inno Setup installer
- GitHub Actions release workflow (modeled on Nexus `release.yml`)
- Community launch (modeled on [[Nexus-Community-Launch]])

## Related

- [[NexusSystemMonitor/NexusSystemMonitor|Nexus System Monitor]] — source of the Crystal Glass engine and theme presets
- [[Nexus-Community-Launch]] — launch playbook to reuse for GlassForge
