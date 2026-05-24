using System.Reflection;
using System.Windows;
using GlassForge.Shell;

namespace GlassForge.UI;

/// <summary>
/// Settings window — created on demand when the tray icon is clicked.
/// Follows the design principle: WPF window is demand-created and fully disposed on close.
/// </summary>
public partial class MainWindow : Window
{
    private readonly CapabilityMap _caps;

    public MainWindow(CapabilityMap caps)
    {
        InitializeComponent();
        _caps = caps;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionLabel.Text = $"v{version?.ToString(3) ?? "0.1.0"}";

        if (_caps.IsProbed)
        {
            BuildInfo.Text =
                $"Windows Build: {_caps.BuildNumber}  |  " +
                $"UBR: {WindowsBuildDetector.GetUpdateBuildRevision()}  |  " +
                $"Last probe: {_caps.LastProbeTime:yyyy-MM-dd HH:mm:ss} UTC";

            CapabilityInfo.Text =
                $"SystemBackdrop: {Bool(_caps.SupportsSystemBackdrop)}  " +
                $"CaptionColor: {Bool(_caps.SupportsCaptionColor)}  " +
                $"BorderColor: {Bool(_caps.SupportsBorderColor)}  " +
                $"ImmersiveDark: {Bool(_caps.SupportsImmersiveDarkMode)}  " +
                $"TaskbarTransparency: {Bool(_caps.SupportsTaskbarTransparency)}";
        }
        else
        {
            CapabilityInfo.Text = "Capabilities not yet probed.";
            BuildInfo.Text      = $"Windows Build: {WindowsBuildDetector.GetBuildNumber()}";
        }

        StatusLabel.Text = "Running in system tray.";
    }

    private static string Bool(bool v) => v ? "Yes" : "No";

    private void CloseToTray_Click(object sender, RoutedEventArgs e)
        => Hide();

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Hide instead of close so re-opening from tray is instant
        e.Cancel = true;
        Hide();
    }
}