namespace GlassForge.UI;

using System.Windows;
using GlassForge.Core.Settings;
using GlassForge.Shell;
using GlassForge.Shell.Abstractions;
using GlassForge.UI.Tray;

public partial class App : Application
{
    private UbrWatcher? _ubrWatcher;
    private TrayManager? _trayManager;
    private CapabilityMap? _capabilityMap;
    private TaskbarEffectService? _taskbarEffectService;
    private IShellBackend? _currentBackend;
    private AppSettings? _currentSettings;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var (build, ubr) = WindowsBuildDetector.Detect();
        _currentBackend = ShellBackendFactory.Create(build);
        _capabilityMap = new CapabilityMap();
        _capabilityMap.Probe(_currentBackend);
        DiagnosticsWriter.Write(build, ubr, _currentBackend.Name, _capabilityMap.Current);

        _ubrWatcher = new UbrWatcher();
        _ubrWatcher.UbrChanged += newUbr =>
        {
            var (newBuild, _) = WindowsBuildDetector.Detect();
            _currentBackend = ShellBackendFactory.Create(newBuild);
            _capabilityMap.Probe(_currentBackend);
            DiagnosticsWriter.Write(newBuild, newUbr, _currentBackend.Name, _capabilityMap.Current);
            if (_currentSettings != null)
                _taskbarEffectService?.Apply(_currentBackend, _currentSettings);
        };
        _ubrWatcher.Start();

        var settingsService = new SettingsService();
        _currentSettings = settingsService.Load();
        _taskbarEffectService = new TaskbarEffectService();

        Action<AppSettings> onSettingsChanged = settings =>
        {
            _currentSettings = settings;
            settingsService.Save(settings);
            if (_currentBackend != null)
                _taskbarEffectService.Apply(_currentBackend, settings);
        };

        _trayManager = new TrayManager(_currentSettings, _capabilityMap);
        _trayManager.Initialize(onSettingsChanged);

        if (_currentSettings.TaskbarEffectEnabled)
            _taskbarEffectService.Apply(_currentBackend, _currentSettings);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_currentBackend != null)
            _taskbarEffectService?.Remove(_currentBackend);
        _ubrWatcher?.Dispose();
        _trayManager?.Dispose();
        base.OnExit(e);
    }
}
