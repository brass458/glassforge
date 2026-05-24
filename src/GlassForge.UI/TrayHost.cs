using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using GlassForge.Shell;

namespace GlassForge.UI;

/// <summary>
/// Manages the system tray icon and its context menu.
/// The settings <see cref="MainWindow"/> is created on first show and reused thereafter.
/// TrayHost owns the window lifetime.
/// </summary>
public sealed class TrayHost : IDisposable
{
    private readonly CapabilityMap _caps;
    private readonly NotifyIcon    _notifyIcon;
    private MainWindow?            _window;
    private bool                   _disposed;

    public TrayHost(CapabilityMap caps)
    {
        _caps = caps;
        _notifyIcon = BuildNotifyIcon();
    }

    private NotifyIcon BuildNotifyIcon()
    {
        var icon = new NotifyIcon
        {
            Text    = "GlassForge",
            Icon    = CreateFallbackIcon(),
            Visible = true,
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("Settings", null, (_, _) => ShowSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApplication());

        icon.ContextMenuStrip = menu;
        icon.DoubleClick     += (_, _) => ShowSettings();
        return icon;
    }

    private void ShowSettings()
    {
        if (_disposed) return;

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (_window is null)
            {
                _window = new MainWindow(_caps);
                _window.Closed += (_, _) => _window = null;
            }

            _window.Show();
            _window.WindowState = WindowState.Normal;
            _window.Activate();
        });
    }

    private static void ExitApplication()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
            System.Windows.Application.Current.Shutdown());
    }

    /// <summary>
    /// Creates a simple programmatic icon (16x16 blue square with 'G') as fallback
    /// until a real .ico file is added to resources.
    /// </summary>
    private static Icon CreateFallbackIcon()
    {
        try
        {
            using var bmp    = new Bitmap(16, 16);
            using var gfx    = Graphics.FromImage(bmp);
            using var brush  = new SolidBrush(Color.FromArgb(0x0A, 0x84, 0xFF));
            using var font   = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold);
            using var fgBrush = new SolidBrush(Color.White);
            gfx.Clear(Color.Transparent);
            gfx.FillRectangle(brush, 0, 0, 16, 16);
            gfx.DrawString("G", font, fgBrush, 2f, 1f);
            var handle = bmp.GetHicon();
            return Icon.FromHandle(handle);
        }
        catch
        {
            return SystemIcons.Application;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _window?.Close();
    }
}
