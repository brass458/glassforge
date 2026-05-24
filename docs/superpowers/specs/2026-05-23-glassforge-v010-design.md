# GlassForge v0.1.0 — Foundation & Infrastructure Design

**Date:** 2026-05-23  
**Scope:** v0.1.0 only — no visual shell modification. Infrastructure that all future phases build on.  
**Deferred to v0.1.1:** Remote `compat.json` loader, lightweight auto-updater.

---

## 1. Solution Structure

```
O:\ObsidianVaults\Ideaverse\Areas\Projects\GlassForge\
  GlassForge.sln
  Directory.Build.props           # Shared version + build config
  .gitignore
  README.md
  src/
    GlassForge.Core/              # .NET 8 class library — no OS deps
      Models/
        ThemePreset.cs
      Themes/
        BuiltInThemePresets.cs
        SurfaceSwatchPalettes.cs
        ThemePresetService.cs
      Settings/
        AppSettings.cs
        SettingsService.cs
    GlassForge.Shell/             # net8.0-windows — P/Invoke, DWM, registry
      Abstractions/
        IShellBackend.cs
        ShellCapabilities.cs
      Backends/
        ShellBackend_22H2.cs      # build 22621–22630
        ShellBackend_23H2.cs      # build 22631–26099
        ShellBackend_24H2.cs      # build 26100–26199
        ShellBackend_Future.cs    # forward-compat fallback (MaxBuild = int.MaxValue)
      CapabilityMap.cs
      WindowsBuildDetector.cs
      ShellBackendFactory.cs
      UbrWatcher.cs
      DiagnosticsWriter.cs
    GlassForge.UI/                # net8.0-windows, WPF
      App.xaml
      App.xaml.cs
      MainWindow.xaml
      MainWindow.xaml.cs
      Tray/
        TrayManager.cs
        TrayResources.xaml
  tests/
    GlassForge.Tests/             # net8.0, xUnit — no OS required
```

### Project References

| Project | References | Key NuGet packages |
|---|---|---|
| `GlassForge.Core` | — | — |
| `GlassForge.Shell` | `GlassForge.Core` | — |
| `GlassForge.UI` | `GlassForge.Core`, `GlassForge.Shell` | `Hardcodet.NotifyIcon.Wpf` |
| `GlassForge.Tests` | `GlassForge.Core`, `GlassForge.Shell` | `xunit`, `xunit.runner.visualstudio`, `coverlet.collector` |

### `Directory.Build.props`

- `<Version>0.1.0</Version>`
- `<LangVersion>12</LangVersion>`
- `<Nullable>enable</Nullable>`
- `<ImplicitUsings>enable</ImplicitUsings>`
- `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`

---

## 2. Shell Abstraction

### `IShellBackend` (`GlassForge.Shell/Abstractions/IShellBackend.cs`)

```csharp
public interface IShellBackend
{
    string Name { get; }
    int MinBuild { get; }
    int MaxBuild { get; }
    ShellCapabilities ProbeCapabilities();
}
```

### `ShellCapabilities` (`GlassForge.Shell/Abstractions/ShellCapabilities.cs`)

```csharp
public record ShellCapabilities
{
    public bool SupportsSystemBackdropType { get; init; }
    public bool SupportsCaptionColor { get; init; }
    public bool SupportsBorderColor { get; init; }
    public bool SupportsImmersiveDarkMode { get; init; }
    public bool SupportsWindowCompositionAttribute { get; init; }
}
```

All backends return all-false in v0.1.0. The probe call chain is wired; actual DWM probing begins in v0.2.0.

### `WindowsBuildDetector` (`GlassForge.Shell/WindowsBuildDetector.cs`)

- Static class with `Detect(Func<OSVERSIONINFOEX>? testProbe = null)` method
- P/Invoke `RtlGetVersion` into `OSVERSIONINFOEX` struct when `testProbe` is null
- Returns `(int Build, int Ubr)` value type
- Tests pass a fake `Func<OSVERSIONINFOEX>`; production passes nothing (null → real P/Invoke)
- Never uses `Environment.OSVersion`

### `ShellBackendFactory` (`GlassForge.Shell/ShellBackendFactory.cs`)

```
build 22621–22630  →  ShellBackend_22H2   (Win11 22H2)
build 22631–26099  →  ShellBackend_23H2   (Win11 23H2)
build 26100–26199  →  ShellBackend_24H2   (Win11 24H2)
anything else      →  ShellBackend_Future  (forward-compat fallback)
```

Static `Create(int buildNumber)` method. Adding a new build range in the future means adding a new `ShellBackend_*` class and a new case — existing backends are never modified.

### `CapabilityMap` (`GlassForge.Shell/CapabilityMap.cs`)

- Exposes `ShellCapabilities Current { get; private set; }`
- `Probe(IShellBackend backend)` — calls `backend.ProbeCapabilities()`, atomically replaces `Current`. Used for both initial startup probe and re-probe after a Windows Update (same method, same code path).
- Thread-safe: `Probe` uses a lock; `Current` is read-only after set

### `UbrWatcher` (`GlassForge.Shell/UbrWatcher.cs`)

- Opens `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion` read-only
- `RegNotifyChangeKeyValue` with `REG_NOTIFY_CHANGE_LAST_SET`
- `ThreadPool.RegisterWaitForSingleObject` — not a timer or polling loop
- On change: reads new UBR; if different from cached value, fires `UbrChanged` event with new UBR
- Consumer (`App.xaml.cs`): re-detect build, select new backend if build range changed, call `CapabilityMap.Probe`
- `IDisposable` — closes registry key and wait handle on dispose

---

## 3. Tray Host & Settings Window

### `App.xaml`

```xml
<Application ShutdownMode="OnExplicitShutdown">
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="Tray/TrayResources.xaml"/>
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>
```

No `StartupUri`. No main window shown at launch.

### `App.xaml.cs` Startup Sequence

Executed in `OnStartup(StartupEventArgs)`, in order:

1. `WindowsBuildDetector.Detect()` → `(build, ubr)`
2. `ShellBackendFactory.Create(build)` → `IShellBackend`
3. `CapabilityMap.Probe(backend)` → `ShellCapabilities`
4. `DiagnosticsWriter.Write(build, ubr, backend.Name, capabilities)` → `%AppData%\GlassForge\diagnostics.json`
5. `UbrWatcher.Start(onUbrChanged)` — registry watch begins
6. `SettingsService.Load()` → `AppSettings`
7. `TrayManager.Initialize(settings)` → `TaskbarIcon` created, context menu wired

`onUbrChanged`: re-detect build, call `ShellBackendFactory.Create`, call `CapabilityMap.Reprobe`, call `DiagnosticsWriter.Write` again with updated state.

On `Application.Exit`: `UbrWatcher.Dispose()`.

### `TrayManager` (`GlassForge.UI/Tray/TrayManager.cs`)

- Creates and owns the `TaskbarIcon` (Hardcodet)
- Context menu items:
  - **Open Settings** — calls `OpenSettings()`
  - ─ separator ─
  - **Exit** — `UbrWatcher.Dispose()`, `Application.Current.Shutdown()`
- Tray left-click also calls `OpenSettings()`

### Settings Window Lifecycle

```
OpenSettings():
  if (_window == null)
    _window = new MainWindow(_settings);
    _window.Closed += (_, _) => _window = null;
    _window.Show();
  else
    _window.Activate();
```

- Fully demand-created: no WPF window exists until first tray interaction
- `Closed` sets `_window = null` — GC collects it; no `Hide()`, no reuse
- `Activate()` brings an already-open window to foreground instead of creating a second

### `MainWindow` (v0.1.0)

Placeholder settings window. Displays:
- GlassForge version (from `Assembly.GetExecutingAssembly().GetName().Version`)
- Active backend name (e.g. "Win11_23H2")
- Current capability map (all false in v0.1.0 — shown as a status list)
- Active preset name (read from `AppSettings`)

No editable controls yet — full settings UI comes in v0.2.0+.

---

## 4. Theme Model Extraction

Four files ported from Nexus (`O:\ObsidianVaults\Ideaverse\Areas\Projects\NexusSystemMonitor\src\NexusMonitor.Core\`) into `GlassForge.Core`:

| Nexus source | GlassForge destination | Changes |
|---|---|---|
| `Core/Models/ThemePreset.cs` | `Models/ThemePreset.cs` | Namespace: `NexusMonitor.Core.Models` → `GlassForge.Core.Models` |
| `Core/Themes/BuiltInThemePresets.cs` | `Themes/BuiltInThemePresets.cs` | Namespace only |
| `Core/Themes/SurfaceSwatchPalettes.cs` | `Themes/SurfaceSwatchPalettes.cs` | Namespace only |
| `Core/Themes/ThemePresetService.cs` | `Themes/ThemePresetService.cs` | Namespace + storage path → `%AppData%\GlassForge\custom-themes.json` |

No behavior changes. `ThemePresetService` uses `System.Text.Json` — no Avalonia dependency, no WPF dependency. Verified by `ThemePresetServiceTests`.

---

## 5. Settings & Diagnostics Persistence

### `AppSettings` (`GlassForge.Core/Settings/AppSettings.cs`)

```csharp
public class AppSettings
{
    public string ActivePresetName { get; set; } = "Crystal Glass";
    public bool SmartTintEnabled { get; set; } = false;
    public float TaskbarOpacity { get; set; } = 0.85f;
    public string TaskbarBackdropMode { get; set; } = "Acrylic";
}
```

New properties added per phase; `System.Text.Json` ignores unknown properties on load (forward-compatible deserialize).

### `SettingsService` (`GlassForge.Core/Settings/SettingsService.cs`)

- Storage: `%AppData%\GlassForge\config.json`
- `Load()`: deserialize; on `FileNotFoundException` or `JsonException` → return `new AppSettings()` (defaults)
- `Save(AppSettings)`: serialize to `config.tmp` in same directory, then `File.Replace("config.tmp", "config.json", null)` — atomic, no partial writes
- Directory created on first save if absent

### `DiagnosticsWriter` (`GlassForge.Shell/DiagnosticsWriter.cs`)

Written to `%AppData%\GlassForge\diagnostics.json` at every startup. Overwrites each run (fresh state snapshot):

```json
{
  "timestamp": "2026-05-23T20:00:00Z",
  "buildNumber": 22631,
  "ubr": 4169,
  "backendName": "Win11_23H2",
  "capabilities": {
    "supportsSystemBackdropType": false,
    "supportsCaptionColor": false,
    "supportsBorderColor": false,
    "supportsImmersiveDarkMode": false,
    "supportsWindowCompositionAttribute": false
  },
  "workingSetBytes": 14123008
}
```

`workingSetBytes` sampled via `Environment.WorkingSet` immediately after startup sequence completes.

---

## 6. Tests (`GlassForge.Tests`)

| Test class | Coverage |
|---|---|
| `WindowsBuildDetectorTests` | Pass fake `Func<OSVERSIONINFOEX>` to `Detect(testProbe)` — verifies build and UBR extracted correctly |
| `ShellBackendFactoryTests` | Build boundaries: 22621, 22630, 22631, 26099, 26100, 26199, 99999 → correct backend type |
| `CapabilityMapTests` | All-false in v0.1.0; `Probe` updates `Current`; thread-safety (concurrent reads during re-probe) |
| `ThemePresetServiceTests` | Load/save round-trip, custom preset CRUD, missing file → empty list |
| `SettingsServiceTests` | Load defaults on missing file, load defaults on malformed JSON, save/load round-trip, atomic write verified |

No UI tests. Target: all tests pass on any Windows or non-Windows CI runner (Core + Shell logic only; no P/Invoke calls in tests).

---

## Out of Scope for v0.1.0

- Remote `compat.json` loader — deferred to v0.1.1
- Lightweight auto-updater — deferred to v0.1.1
- Any visual shell modification (taskbar, title bars, window frames) — v0.2.0+
- Full settings UI — v0.2.0+
- SkiaSharp / wallpaper analysis — v0.5.0
