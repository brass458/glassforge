namespace GlassForge.Shell.Abstractions;

/// <summary>
/// Abstraction over the Windows shell APIs that vary between OS builds.
/// Each concrete implementation targets a specific build range.
/// Only documented Win32 APIs (DwmSetWindowAttribute etc.) may be called
/// from cross-build code; undocumented APIs are isolated inside ShellBackend_* implementations.
/// </summary>
public interface IShellBackend
{
    /// <summary>The inclusive minimum Windows build number this backend supports.</summary>
    int MinBuild { get; }

    /// <summary>The inclusive maximum Windows build number this backend supports (int.MaxValue = open-ended).</summary>
    int MaxBuild { get; }

    // ── Taskbar ────────────────────────────────────────────────────────────────

    /// <summary>Apply transparency/blur to the system taskbar.</summary>
    bool ApplyTaskbarTransparency(TaskbarStyle style);

    /// <summary>Restore the taskbar to its default appearance.</summary>
    void ResetTaskbar();

    // ── Title bars ────────────────────────────────────────────────────────────

    /// <summary>
    /// Apply caption color and backdrop to a specific window.
    /// Returns true if DWM confirmed the change (round-trip read-back verify).
    /// </summary>
    bool ApplyCaptionColor(nint hwnd, uint colorRef);

    /// <summary>Apply the system backdrop type (Mica, Acrylic, etc.) to a window.</summary>
    bool ApplySystemBackdrop(nint hwnd, SystemBackdropType type);

    // ── Capability probe ──────────────────────────────────────────────────────

    /// <summary>
    /// Probe the runtime environment to verify which capabilities are actually
    /// supported on this installation, including round-trip verification.
    /// </summary>
    CapabilityProbeResult Probe();
}
