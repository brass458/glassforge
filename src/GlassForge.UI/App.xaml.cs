namespace GlassForge.UI;

using System.Windows;
using GlassForge.Core.Settings;
using GlassForge.Shell;
using GlassForge.UI.Tray;

public partial class App : Application
{
    private UbrWatcher? _ubrWatcher;
    private TrayManager? _trayManager;
    private CapabilityMap? _capabilityMap;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Detect Windows build + UBR
        var (build, ubr) = WindowsBuildDetector.Detect();

        // 2. Select backend for this build
        var backend = ShellBackendFactory.Create(build);

        // 3. Probe capabilities (all-false in v0.1.0)
        _capabilityMap = new CapabilityMap();
        _capabilityMap.Probe(backend);

        // 4. Write diagnostics snapshot
        DiagnosticsWriter.Write(build, ubr, backend.Name, _capabilityMap.Current);

        // 5. Watch for Windows Update (UBR change)
        _ubrWatcher = new UbrWatcher();
        _ubrWatcher.UbrChanged += newUbr =>
        {
            var (newBuild, _) = WindowsBuildDetector.Detect();
            var newBackend = ShellBackendFactory.Create(newBuild);
            _capabilityMap.Probe(newBackend);
            DiagnosticsWriter.Write(newBuild, newUbr, newBackend.Name, _capabilityMap.Current);
        };
        _ubrWatcher.Start();

        // 6. Load settings
        var settingsService = new SettingsService();
        var settings = settingsService.Load();

        // 7. Initialize headless tray host
        _trayManager = new TrayManager(settings, _capabilityMap);
        _trayManager.Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _ubrWatcher?.Dispose();
        _trayManager?.Dispose();
        base.OnExit(e);
    }
}
