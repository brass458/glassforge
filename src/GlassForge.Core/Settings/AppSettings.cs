namespace GlassForge.Core.Settings;

public class AppSettings
{
    public string ActivePresetName { get; set; } = "GlassForge Default";
    public bool SmartTintEnabled { get; set; } = false;
    public float TaskbarOpacity { get; set; } = 0.85f;
    public string TaskbarBackdropMode { get; set; } = "Acrylic";
}
