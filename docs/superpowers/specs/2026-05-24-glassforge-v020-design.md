# GlassForge v0.2.0 — Icon, DWM Probing & Taskbar Glass Design

**Date:** 2026-05-24
**Scope:** v0.2.0 — custom app icon, real DWM capability probing, first taskbar glass effect, basic settings controls.
**Deferred:** Multi-monitor secondary taskbars, custom tint color picker, theme preset application, SkiaSharp/refraction layer (v0.5.0+).

---

## 1. Solution Changes

No new projects added. One new tool project added outside the solution (run manually, not part of build):

```
tools/
  GlassForge.IconGenerator/
    GlassForge.IconGenerator.csproj   # net8.0-windows, console, not in GlassForge.sln
    Program.cs
assets/
  glassforge.ico                      # Generated output — committed as binary
```

All other changes are in existing projects.

---

## 2. App Icon

### Design

Classic brilliant-cut gem viewed from a front-quarter angle. Four regions:

- **Table facet** (top center): bright white-blue `#E8F4FF`, the primary highlight
- **Crown facets** (upper ring): medium blue `#4A90D9` with lighter specular glints on alternating facets
- **Girdle** (widest point): thin line `#8AB8E8`
- **Pavilion** (lower half, narrowing to point): deep navy `#1A3A7A`, slightly lighter edges

Transparent background. At 16px the gem reads as a clean blue diamond shape. At 256px facets are fully detailed with visible highlights.

### Generator (`tools/GlassForge.IconGenerator/Program.cs`)

Renders all four sizes using WPF `DrawingContext` + `PathGeometry`, captures via `RenderTargetBitmap`, then encodes frames into a single ICO binary using the standard ICO file format (ICONDIR + ICONDIRENTRY array + PNG-compressed 256px frame + BMP frames for 16/32/48).

Output path: `../../assets/glassforge.ico` relative to the tool project. Run once after checkout or whenever the design changes:

```powershell
dotnet run --project tools/GlassForge.IconGenerator/GlassForge.IconGenerator.csproj
```

### Wiring

**`GlassForge.UI.csproj`:**
```xml
<ApplicationIcon>..\..\assets\glassforge.ico</ApplicationIcon>
<ItemGroup>
  <Content Include="..\..\assets\glassforge.ico" Link="assets\glassforge.ico" CopyToOutputDirectory="PreserveNewest"/>
</ItemGroup>
```

**`Tray/TrayResources.xaml`:**
```xml
<BitmapImage x:Key="GlassForgeIcon" UriSource="pack://application:,,,/assets/glassforge.ico"/>
```
Used by WPF `Image` controls (e.g. future about/settings header). Not used for the tray icon directly.

**`TrayManager.cs`:** Load the tray icon via `System.Drawing.Icon` (Hardcodet requires this, not `BitmapImage`):
```csharp
var stream = Application.GetResourceStream(
    new Uri("pack://application:,,,/assets/glassforge.ico"))?.Stream;
_trayIcon.Icon = stream != null ? new System.Drawing.Icon(stream) : SystemIcons.Application;
```

**`TrayManager.Initialize` signature update:**
```csharp
public void Initialize(AppSettings settings, CapabilityMap capabilityMap,
    Action<AppSettings> onSettingsChanged)
```
`onSettingsChanged` is stored and forwarded to `MainWindow` when the settings window is demand-created.

---

## 3. DWM Capability Probing

### `DwmProber` (`GlassForge.Shell/DwmProber.cs`)

Static class with an injectable test seam:

```csharp
public static class DwmProber
{
    // Production: null → real P/Invoke
    // Tests: inject a Func that returns S_OK or E_FAIL per attribute
    public static ShellCapabilities Probe(
        Func<IntPtr, uint, IntPtr, uint, int>? testProbe = null)
```

**Production path:**
1. `CreateWindowEx` with `WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE`, `WS_POPUP`, zero size, `HWND_MESSAGE` parent — creates a message-only window that is never visible
2. For each of the five DWM attributes, call `DwmGetWindowAttribute` and record whether it returns `S_OK` (0)
3. `DestroyWindow` the test HWND
4. Return `ShellCapabilities` with the probed booleans

**DWM attribute constants:**
| Capability | Attribute | Value |
|---|---|---|
| `SupportsImmersiveDarkMode` | `DWMWA_USE_IMMERSIVE_DARK_MODE` | 20 |
| `SupportsBorderColor` | `DWMWA_BORDER_COLOR` | 34 |
| `SupportsCaptionColor` | `DWMWA_CAPTION_COLOR` | 35 |
| `SupportsSystemBackdropType` | `DWMWA_SYSTEMBACKDROP_TYPE` | 38 |
| `SupportsWindowCompositionAttribute` | tested via `SetWindowCompositionAttribute` with `ACCENT_DISABLED` | — |

**Test seam:** `testProbe` receives `(hwnd, attributeId, pvAttribute, cbAttribute)` and returns an HRESULT int. Tests pass a lambda; production passes null.

### Backend `ProbeCapabilities()` updates

All four backends replace their `return new ShellCapabilities()` stub with:

```csharp
public ShellCapabilities ProbeCapabilities() => DwmProber.Probe();
```

`DiagnosticsWriter` already captures `backendName = "Win11_Future"` at startup — no additional annotation needed in `ProbeCapabilities`.

---

## 4. IShellBackend Extension

```csharp
public interface IShellBackend
{
    string Name { get; }
    int MinBuild { get; }
    int MaxBuild { get; }
    ShellCapabilities ProbeCapabilities();
    void ApplyTaskbarEffect(IntPtr hwnd, AppSettings settings);
    void RemoveTaskbarEffect(IntPtr hwnd);
}
```

### P/Invoke types (shared, in `GlassForge.Shell/NativeMethods.cs` — new file)

```csharp
internal enum AccentState
{
    ACCENT_DISABLED = 0,
    ACCENT_ENABLE_BLURBEHIND = 3,
    ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
}

[StructLayout(LayoutKind.Sequential)]
internal struct ACCENT_POLICY
{
    public AccentState AccentState;
    public int AccentFlags;
    public int GradientColor;   // AABBGGRR
    public int AnimationId;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WINDOWCOMPOSITIONATTRIBDATA
{
    public int Attribute;       // WCA_ACCENT_POLICY = 19
    public IntPtr Data;
    public int SizeOfData;
}
```

### `AppSettings.TaskbarBackdropMode` → `AccentState` mapping

| Setting value | `AccentState` |
|---|---|
| `"Acrylic"` | `ACCENT_ENABLE_ACRYLICBLURBEHIND` |
| `"Blur"` | `ACCENT_ENABLE_BLURBEHIND` |
| `"None"` | `ACCENT_DISABLED` |
| (unrecognised) | `ACCENT_DISABLED` |

`TaskbarOpacity` (0.0f–1.0f) → alpha byte: `(int)(settings.TaskbarOpacity * 255) << 24`. The RGB component of `GradientColor` is `0x000000` (black tint base) — color customization is deferred.

### Per-backend implementations

**`ShellBackend_22H2` and `ShellBackend_23H2`:**
```
ApplyTaskbarEffect: SetWindowCompositionAttribute with mapped AccentState + opacity alpha
RemoveTaskbarEffect: SetWindowCompositionAttribute with ACCENT_DISABLED
```

**`ShellBackend_24H2`:**
```
ApplyTaskbarEffect:
  if mode == "Acrylic" or "Blur":
    try DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, DWMSBT_TRANSIENTWINDOW=3)
    if non-S_OK: fall back to SetWindowCompositionAttribute
  if mode == "None":
    DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, DWMSBT_NONE=1)
RemoveTaskbarEffect: SetWindowCompositionAttribute with ACCENT_DISABLED
```

**`ShellBackend_Future`:**
Same implementation as `ShellBackend_24H2`. `DiagnosticsWriter` already records `backendName = "Win11_Future"` — no additional logging needed in the effect methods.

---

## 5. TaskbarEffectService

**File:** `src/GlassForge.Shell/TaskbarEffectService.cs`

```csharp
public class TaskbarEffectService
{
    // Injectable for tests — replaces FindWindow P/Invoke
    private readonly Func<string, string?, IntPtr> _findWindow;

    public TaskbarEffectService(Func<string, string?, IntPtr>? findWindow = null)
    {
        _findWindow = findWindow ?? NativeMethods.FindWindow;
    }

    // Returns true on success, false if taskbar HWND not found
    public bool Apply(IShellBackend backend, AppSettings settings);

    // Always attempts removal; ignores invalid HWND
    public void Remove(IShellBackend backend);
}
```

`Apply`:
1. `_findWindow("Shell_TrayWnd", null)` → hwnd
2. If `!NativeMethods.IsWindow(hwnd)` → return false
3. If `!settings.TaskbarEffectEnabled` → `backend.RemoveTaskbarEffect(hwnd)`; return true
4. `backend.ApplyTaskbarEffect(hwnd, settings)`; return true

`Remove`:
1. `_findWindow("Shell_TrayWnd", null)` → hwnd
2. If valid → `backend.RemoveTaskbarEffect(hwnd)`

---

## 6. AppSettings Update

Add one property to `GlassForge.Core/Settings/AppSettings.cs`:

```csharp
public bool TaskbarEffectEnabled { get; set; } = true;
```

`SmartTintEnabled` remains reserved for the future adaptive wallpaper tinting feature and is not used as the on/off toggle.

---

## 7. App Lifecycle (`App.xaml.cs`)

Startup sequence gains step 8 after `TrayManager.Initialize`:

```
8. if settings.TaskbarEffectEnabled:
       _taskbarEffectService.Apply(backend, settings)
```

UBR-changed callback gains re-apply after re-probe:
```
onUbrChanged:
  detect build → create backend → CapabilityMap.Probe(backend)
  DiagnosticsWriter.Write(...)
  _taskbarEffectService.Apply(newBackend, currentSettings)   ← new
```

Exit:
```
OnExit:
  _taskbarEffectService.Remove(backend)
  _ubrWatcher.Dispose()
```

`_taskbarEffectService` is a field on `App`, created in `OnStartup`. `_onSettingsChanged` delegate passed to `TrayManager.Initialize` and forwarded to `MainWindow`:

```csharp
Action onSettingsChanged = (settings) =>
    _taskbarEffectService.Apply(_currentBackend, settings);
```

`_currentBackend` is a field updated on UBR change.

---

## 8. Settings UI (`MainWindow`)

Constructor gains `Action<AppSettings> onSettingsChanged` parameter.

### XAML layout

```
┌─ GlassForge ─────────────────────────────────┐
│ v0.2.0 — Win11_23H2                           │
│                                               │
│ Taskbar Glass                                 │
│  [✓] Enable effect                            │
│  Backdrop   [Acrylic          ▼]              │
│  Opacity    [━━━━━━━━━━━━━━░░] 85%            │
│                                               │
│ System Capabilities                           │
│  System Backdrop Type    ✓ Available          │
│  Caption Color           ✓ Available          │
│  Border Color            ✓ Available          │
│  Immersive Dark Mode     ✓ Available          │
│  Window Composition      ✓ Available          │
└───────────────────────────────────────────────┘
```

Backdrop `ComboBox` and Opacity `Slider` are disabled when `Enable effect` is unchecked.

### Behavior

Every control change:
1. Updates the in-memory `AppSettings` copy
2. Calls `_settingsService.Save(settings)`
3. Calls `_onSettingsChanged(settings)`

No explicit Save button — changes apply immediately. This matches the "lightweight, event-driven" principle.

---

## 9. Tests

### New test classes

**`DwmProberTests`** — 5 tests:
- All attributes return S_OK → all capabilities true
- All attributes return E_FAIL → all capabilities false
- Mixed results → correct partial capability record
- `SupportsWindowCompositionAttribute` isolated probe
- Verify test HWND is never `IntPtr.Zero` in production path (structural test via mock)

**`TaskbarEffectServiceTests`** — 6 tests:
- Valid HWND + effect enabled → `ApplyTaskbarEffect` called on backend mock
- Valid HWND + effect disabled → `RemoveTaskbarEffect` called
- Invalid HWND → returns false, backend never called
- `Remove` with valid HWND → `RemoveTaskbarEffect` called
- `Remove` with invalid HWND → no exception
- `Apply` then settings change → second `Apply` uses updated opacity

**Backend effect tests (added to existing backend test files)** — 3 tests per backend (12 total):
- `TaskbarBackdropMode = "Acrylic"` → correct `AccentState` in P/Invoke call (via injectable delegate)
- `TaskbarBackdropMode = "None"` → `ACCENT_DISABLED`
- Opacity 0.5 → alpha byte = 127 in `GradientColor`

**`SettingsServiceTests`** gains 1 test:
- `TaskbarEffectEnabled` defaults to true and round-trips correctly

---

## 10. Out of Scope for v0.2.0

- Secondary taskbars on multi-monitor setups (`Shell_SecondaryTrayWnd`)
- Custom tint color (RGB picker)
- Theme preset application to the effect
- Smart tinting from wallpaper analysis (SkiaSharp — v0.5.0)
- Liquid Glass refraction/specular layer (v0.5.0+)
- Title bar or window frame effects (later phases)
- Auto-updater / remote compat.json (v0.1.1, still deferred)
