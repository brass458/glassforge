using GlassForge.Shell.Abstractions;

namespace GlassForge.Shell.Backends;

/// <summary>
/// Backend for Windows 11 21H2 (builds 22000–22620).
/// Supports: caption color, immersive dark mode.
/// Does NOT support DWMWA_SYSTEMBACKDROP_TYPE (added in 22H2/22621).
/// Taskbar transparency uses SetWindowCompositionAttribute (undocumented — may break).
/// </summary>
internal sealed class ShellBackend_22000 : IShellBackend
{
    public int MinBuild => 22000;
    public int MaxBuild => 22620;

    public bool ApplyCaptionColor(nint hwnd, uint colorRef)
    {
        var color = (int)colorRef;
        return DwmNative.SetAndVerify(hwnd, DwmNative.DWMWA_CAPTION_COLOR, color);
    }

    public bool ApplySystemBackdrop(nint hwnd, SystemBackdropType type)
        // DWMWA_SYSTEMBACKDROP_TYPE not available on this build
        => false;

    public bool ApplyTaskbarTransparency(TaskbarStyle style)
        // Undocumented path — stubbed for now; v0.2.0 will implement
        => false;

    public void ResetTaskbar() { }

    public CapabilityProbeResult Probe()
    {
        var build = WindowsBuildDetector.GetBuildNumber();
        // Use a hidden test window for round-trip verification in a real probe;
        // v0.1.0 returns static knowledge of this build range.
        return new CapabilityProbeResult
        {
            BuildNumber                        = build,
            SupportsCaptionColor               = true,
            SupportsBorderColor                = true,
            SupportsImmersiveDarkMode          = true,
            SupportsSystemBackdrop             = false,
            SupportsWindowCompositionAttribute = true,  // present but undocumented
            SupportsTaskbarTransparency        = false,  // not yet implemented
        };
    }
}
