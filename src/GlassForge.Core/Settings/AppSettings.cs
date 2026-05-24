namespace GlassForge.Core.Settings;

public class AppSettings
{
    // ── Theme ──────────────────────────────────────────────────────────────────
    /// <summary>"System" | "Dark" | "Light"</summary>
    public string ThemeMode          { get; set; } = "System";
    public string AccentColorHex     { get; set; } = "#0A84FF";
    /// <summary>"" = derived from AccentColorHex</summary>
    public string TextAccentColorHex { get; set; } = "";

    // Custom surface colors — "" = use preset defaults
    public string CustomWindowBgHex  { get; set; } = "";
    public string CustomSurfaceBgHex { get; set; } = "";
    public string CustomSidebarBgHex { get; set; } = "";

    // ── Crystal Glass ──────────────────────────────────────────────────────────
    public bool   IsGlassEnabled    { get; set; } = true;
    /// <summary>0 = fully transparent, 1 = fully opaque</summary>
    public double GlassOpacity      { get; set; } = 0.80;
    /// <summary>"None" | "Blur" | "Acrylic" | "Mica"</summary>
    public string BackdropBlurMode  { get; set; } = "Acrylic";
    public bool   IsSpecularEnabled { get; set; } = true;
    public double SpecularIntensity { get; set; } = 0.55;

    // ── Typography ─────────────────────────────────────────────────────────────
    /// <summary>"" = system default font</summary>
    public string FontFamily         { get; set; } = "";
    public double FontSizeMultiplier { get; set; } = 1.0;

    // ── Active preset ──────────────────────────────────────────────────────────
    /// <summary>"" = custom / no preset active</summary>
    public string ActiveThemePresetId { get; set; } = "";

    // ── Tray / window behavior ─────────────────────────────────────────────────
    public bool MinimizeToTray { get; set; } = true;
    /// <summary>"" = always ask, "Tray" = minimize to tray, "Exit" = close application</summary>
    public string CloseAction  { get; set; } = "";

    // ── Session persistence ────────────────────────────────────────────────────
    public double LastWindowWidth  { get; set; } = 0;
    public double LastWindowHeight { get; set; } = 0;
    public int    LastWindowX      { get; set; } = -1;
    public int    LastWindowY      { get; set; } = -1;
    public string LastWindowState  { get; set; } = "Normal";
}
