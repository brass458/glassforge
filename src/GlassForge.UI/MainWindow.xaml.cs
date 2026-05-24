namespace GlassForge.UI;

using System.Reflection;
using System.Windows;
using System.Windows.Media;
using GlassForge.Core.Settings;
using GlassForge.Shell;

public partial class MainWindow : Window
{
    public MainWindow(AppSettings settings, CapabilityMap capabilityMap)
    {
        InitializeComponent();

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1.0";
        StatusText.Text = $"v{version} — {capabilityMap.BackendName}";

        CapabilityList.ItemsSource = new[]
        {
            new CapRow("System Backdrop Type",       capabilityMap.Current.SupportsSystemBackdropType),
            new CapRow("Caption Color",              capabilityMap.Current.SupportsCaptionColor),
            new CapRow("Border Color",               capabilityMap.Current.SupportsBorderColor),
            new CapRow("Immersive Dark Mode",        capabilityMap.Current.SupportsImmersiveDarkMode),
            new CapRow("Window Composition Attr.",   capabilityMap.Current.SupportsWindowCompositionAttribute),
        };
    }

    private record CapRow(string Label, bool Available)
    {
        public string Status => Available ? "Available" : "Not available (v0.1.0)";
        public Brush Color => Available
            ? Brushes.LightGreen
            : new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x88, 0x88, 0x88));
    }
}
