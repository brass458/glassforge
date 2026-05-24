using GlassForge.Shell.Abstractions;

namespace GlassForge.Shell.Backends;

/// <summary>
/// Backend for Windows 11 24H2 and later (builds 26100+).
/// Same documented DWM API surface as 22621 backend.
/// Kept as a separate class so 24H2-specific undocumented APIs can be added here
/// without touching the 22H2 backend — isolation is the point of the per-build design.
/// </summary>
internal sealed class ShellBackend_26100 : IShellBackend
{
    public int MinBuild => 26100;
    public int MaxBuild => int.MaxValue - 1; // open-ended until a new backend is needed

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
            SupportsTaskbarTransparency        = false,
        };
    }
}
