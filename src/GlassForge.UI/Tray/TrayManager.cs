namespace GlassForge.UI.Tray;

using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using GlassForge.Core.Settings;
using GlassForge.Shell;
using Hardcodet.Wpf.TaskbarNotification;

public class TrayManager : IDisposable
{
    private TaskbarIcon? _taskbarIcon;
    private MainWindow? _window;
    private readonly AppSettings _settings;
    private readonly CapabilityMap _capabilityMap;
    private Action<AppSettings>? _onSettingsChanged;

    public TrayManager(AppSettings settings, CapabilityMap capabilityMap)
    {
        _settings = settings;
        _capabilityMap = capabilityMap;
    }

    public void Initialize(Action<AppSettings> onSettingsChanged)
    {
        _onSettingsChanged = onSettingsChanged;

        Icon trayIcon;
        var stream = Application.GetResourceStream(
            new Uri("pack://application:,,,/assets/glassforge.ico"))?.Stream;
        trayIcon = stream != null ? new Icon(stream) : SystemIcons.Application;

        _taskbarIcon = new TaskbarIcon
        {
            Icon = trayIcon,
            ToolTipText = "GlassForge"
        };
        _taskbarIcon.TrayLeftMouseDown += (_, _) => OpenSettings();

        var openItem = new MenuItem { Header = "Open Settings" };
        openItem.Click += (_, _) => OpenSettings();

        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) =>
        {
            _window?.Close();
            Application.Current.Shutdown();
        };

        _taskbarIcon.ContextMenu = new ContextMenu();
        _taskbarIcon.ContextMenu.Items.Add(openItem);
        _taskbarIcon.ContextMenu.Items.Add(new Separator());
        _taskbarIcon.ContextMenu.Items.Add(exitItem);
    }

    public void OpenSettings()
    {
        if (_window == null)
        {
            _window = new MainWindow(_settings, _capabilityMap, _onSettingsChanged!);
            _window.Closed += (_, _) => _window = null;
            _window.Show();
        }
        else
        {
            _window.Activate();
        }
    }

    public void Dispose() => _taskbarIcon?.Dispose();
}
