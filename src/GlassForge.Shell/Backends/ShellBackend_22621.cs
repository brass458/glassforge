using GlassForge.Shell.Abstractions;

namespace GlassForge.Shell.Backends;

/// <summary>
/// Backend for Windows 11 22H2 / 23H2 (builds 22621–25999).
/// Supports: DWMWA_SYSTEMBACKDROP_TYPE (Mica/Acrylic/MicaAlt), caption color, border color, dark mode.
/// Taskbar transparency uses SetWindowCompositionAttribute (undocumented — may break after updates).
/// </summary>
internal sealed class ShellBackend_22621 : IShellBackend
{
    public int MinBuild => 22621;
    public int MaxBuild => 25999;

    public bool ApplyCaptionColor(nint hwnd, uint colorRef)
    {
        var color = (int)colorRef;
        return DwmNative.SetAndVerify(hwnd, DwmNative.DWMWA_CAPTION_COLOR, color);
    }

    public bool ApplySystemBackdrop(nint hwnd, SystemBackdropType type)
    {
        var val = (int)type;
        return DwmNative.SetAndVerify(hwnd, DwmNative.DWMWA_SYSTEMBACKDROP_TYPE, val);
    }

    public bool ApplyTaskbarTransparency(TaskbarStyle style)
        // Undocumented path — stubbed for now; v0.2.0 will implement
        => false;

    public void ResetTaskbar() { }

    public CapabilityProbeResult Probe()
    {
        var build = WindowsBuildDetector.GetBuildNumber();
        return new CapabilityProbeResult
        {
            BuildNumber                        = build,
            SupportsCaptionColor               = true,
            SupportsBorderColor                = true,
            SupportsImmersiveDarkMode          = true,
            SupportsSystemBackdrop             = true,
            SupportsWindowCompositionAttribute = true,
            SupportsTaskbarTransparency        = false, // not yet implemented
        };
    }
}
