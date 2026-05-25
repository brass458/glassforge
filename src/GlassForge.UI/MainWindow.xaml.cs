namespace GlassForge.UI;

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GlassForge.Core.Settings;
using GlassForge.Shell;

public partial class MainWindow : Window
{
    private AppSettings _settings;
    private readonly Action<AppSettings> _onSettingsChanged;
    private bool _loading = true;

    public MainWindow(AppSettings settings, CapabilityMap capabilityMap, Action<AppSettings> onSettingsChanged)
    {
        InitializeComponent();

        _settings = settings;
        _onSettingsChanged = onSettingsChanged;

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.2.0";
        StatusText.Text = $"v{version} — {capabilityMap.BackendName}";

        EffectEnabled.IsChecked = settings.TaskbarEffectEnabled;
        OpacitySlider.Value = settings.TaskbarOpacity * 100;
        OpacityLabel.Text = $"{(int)(settings.TaskbarOpacity * 100)}%";
        UpdateControlsEnabled(settings.TaskbarEffectEnabled);

        foreach (ComboBoxItem item in BackdropMode.Items)
        {
            if ((string?)item.Tag == settings.TaskbarBackdropMode)
            {
                BackdropMode.SelectedItem = item;
                break;
            }
        }
        if (BackdropMode.SelectedItem == null) BackdropMode.SelectedIndex = 0;

        CapabilityList.ItemsSource = new[]
        {
            new CapRow("System Backdrop Type",      capabilityMap.Current.SupportsSystemBackdropType),
            new CapRow("Caption Color",             capabilityMap.Current.SupportsCaptionColor),
            new CapRow("Border Color",              capabilityMap.Current.SupportsBorderColor),
            new CapRow("Immersive Dark Mode",       capabilityMap.Current.SupportsImmersiveDarkMode),
            new CapRow("Window Composition Attr.",  capabilityMap.Current.SupportsWindowCompositionAttribute),
            new CapRow("System Transparency",       capabilityMap.Current.SupportsSystemTransparency),
        };

        EffectEnabled.Checked   += (_, _) => OnSettingChanged();
        EffectEnabled.Unchecked += (_, _) => OnSettingChanged();
        BackdropMode.SelectionChanged += (_, _) => OnSettingChanged();
        OpacitySlider.ValueChanged    += (_, _) =>
        {
            OpacityLabel.Text = $"{(int)OpacitySlider.Value}%";
            OnSettingChanged();
        };

        _loading = false;
    }

    private void OnSettingChanged()
    {
        if (_loading) return;

        _settings.TaskbarEffectEnabled = EffectEnabled.IsChecked == true;
        _settings.TaskbarOpacity = (float)(OpacitySlider.Value / 100.0);
        _settings.TaskbarBackdropMode =
            (BackdropMode.SelectedItem as ComboBoxItem)?.Tag as string ?? "Acrylic";

        UpdateControlsEnabled(_settings.TaskbarEffectEnabled);
        _onSettingsChanged(_settings);
    }

    private void UpdateControlsEnabled(bool enabled)
    {
        BackdropMode.IsEnabled = enabled;
        OpacitySlider.IsEnabled = enabled;
    }

    private record CapRow(string Label, bool Available)
    {
        public string Status => Available ? "✓ Available" : "Not available";
        public Brush Color => Available
            ? Brushes.LightGreen
            : new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x88, 0x88, 0x88));
    }
}
