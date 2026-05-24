using System.Windows;
using GlassForge.Core.Diagnostics;
using GlassForge.Core.Settings;
using GlassForge.Shell;
// Alias WinForms Application to avoid ambiguity with System.Windows.Application
using WinFormsApplication = System.Windows.Forms.Application;

namespace GlassForge.UI;

/// <summary>
/// Application entry point.
/// Startup sequence:
///   1. Build detector reads true Windows build via RtlGetVersion
///   2. ShellBackendFactory selects the matching per-build backend
///   3. CapabilityMap.Refresh() probes + records capabilities
///   4. UbrWatcher starts; on UBR change it re-probes and writes diagnostics
///   5. TrayHost creates the system tray icon (no window shown yet)
/// The MainWindow is demand-created on first tray icon activation.
/// </summary>
public partial class App : Application
{
    private CapabilityMap?  _caps;
    private UbrWatcher?     _ubrWatcher;
    private TrayHost?       _trayHost;
    private SettingsService? _settings;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Detect build + select backend
        var buildNumber = WindowsBuildDetector.GetBuildNumber();
        var backend     = ShellBackendFactory.Create(buildNumber);

        // 2. Probe capabilities
        _caps = new CapabilityMap();
        _caps.Refresh(backend);

        // 3. Load settings
        _settings = new SettingsService();

        // 4. Write initial diagnostic snapshot
        WriteSnapshot(buildNumber, backend.GetType().Name);

        // 5. Start UBR watcher — re-probe after Windows Update
        _ubrWatcher = new UbrWatcher();
        _ubrWatcher.BuildRevisionChanged += (_, args) =>
        {
            // Re-select backend for new UBR (build number doesn't change within a patch,
            // but capabilities might — Windows Update can break undocumented APIs)
            var newBuildNumber = WindowsBuildDetector.GetBuildNumber();
            var newBackend     = ShellBackendFactory.Create(newBuildNumber);
            _caps.Refresh(newBackend);
            WriteSnapshot(newBuildNumber, newBackend.GetType().Name,
                          $"UBR changed: {args.PreviousUbr} → {args.NewUbr}");
        };
        _ubrWatcher.Start();

        // 6. Create tray host — app is now running headlessly
        _trayHost = new TrayHost(_caps);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayHost?.Dispose();
        _ubrWatcher?.Dispose();
        base.OnExit(e);
    }

    private void WriteSnapshot(int buildNumber, string backendName, string? note = null)
    {
        try
        {
            var writer   = new DiagnosticsWriter();
            var snapshot = new DiagnosticSnapshot
            {
                BuildNumber           = buildNumber,
                UpdateBuildRevision   = WindowsBuildDetector.GetUpdateBuildRevision(),
                WindowsVersion        = Environment.OSVersion.VersionString,
                AppVersion            = System.Reflection.Assembly
                                              .GetExecutingAssembly()
                                              .GetName().Version?.ToString(3) ?? "0.1.0",
                ActiveBackend         = backendName,
                SupportsSystemBackdrop             = _caps?.SupportsSystemBackdrop ?? false,
                SupportsCaptionColor               = _caps?.SupportsCaptionColor ?? false,
                SupportsBorderColor                = _caps?.SupportsBorderColor ?? false,
                SupportsImmersiveDarkMode          = _caps?.SupportsImmersiveDarkMode ?? false,
                SupportsWindowCompositionAttribute = _caps?.SupportsWindowCompositionAttribute ?? false,
                SupportsTaskbarTransparency        = _caps?.SupportsTaskbarTransparency ?? false,
                Notes = note is null ? [] : [note],
            };
            writer.Write(snapshot);
            writer.Prune(retainCount: 10);
        }
        catch { /* diagnostics are never allowed to crash the app */ }
    }
}

