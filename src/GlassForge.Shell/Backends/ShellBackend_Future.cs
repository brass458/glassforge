using GlassForge.Shell.Abstractions;

namespace GlassForge.Shell.Backends;

/// <summary>
/// Forward-compatible fallback backend for Windows builds beyond the known range.
/// Uses only fully documented DWM APIs; makes no assumptions about undocumented calls.
/// When GlassForge encounters an unrecognized build, this backend is used so the
/// app remains functional in a degraded-but-safe state rather than crashing.
/// </summary>
internal sealed class ShellBackend_Future : IShellBackend
{
    /// <summary>Activated for any build above the highest known backend's MaxBuild.</summary>
    public int MinBuild => int.MaxValue;
    public int MaxBuild => int.MaxValue;

    public bool ApplyCaptionColor(nint hwnd, uint colorRef)
    {
        // Attempt via documented API — if Microsoft removes it, this returns false gracefully.
        var color = (int)colorRef;
        return DwmNative.SetAndVerify(hwnd, DwmNative.DWMWA_CAPTION_COLOR, color);
    }

    public bool ApplySystemBackdrop(nint hwnd, SystemBackdropType type)
    {
        var val = (int)type;
        return DwmNative.SetAndVerify(hwnd, DwmNative.DWMWA_SYSTEMBACKDROP_TYPE, val);
    }

    public bool ApplyTaskbarTransparency(TaskbarStyle style)
        => false; // never attempt undocumented APIs on unknown builds

    public void ResetTaskbar() { }

    public CapabilityProbeResult Probe()
    {
        var build = WindowsBuildDetector.GetBuildNumber();
        // Probe conservatively: try each documented API and report what actually works.
        // For v0.1.0 we return a safe all-false result for unknown future builds.
        return new CapabilityProbeResult
        {
            BuildNumber                        = build,
            SupportsCaptionColor               = false,
            SupportsBorderColor                = false,
            SupportsImmersiveDarkMode          = false,
            SupportsSystemBackdrop             = false,
            SupportsWindowCompositionAttribute = false,
            SupportsTaskbarTransparency        = false,
        };
    }
}
