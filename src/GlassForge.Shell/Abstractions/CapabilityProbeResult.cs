namespace GlassForge.Shell.Abstractions;

/// <summary>
/// Result of a capability probe against the runtime Windows environment.
/// All flags represent verified capabilities (round-trip read-back confirmed).
/// </summary>
public sealed class CapabilityProbeResult
{
    /// <summary>DwmSetWindowAttribute DWMWA_SYSTEMBACKDROP_TYPE is supported and verifiable.</summary>
    public bool SupportsSystemBackdrop { get; init; }

    /// <summary>DwmSetWindowAttribute DWMWA_CAPTION_COLOR is supported and verifiable.</summary>
    public bool SupportsCaptionColor { get; init; }

    /// <summary>DwmSetWindowAttribute DWMWA_BORDER_COLOR is supported and verifiable.</summary>
    public bool SupportsBorderColor { get; init; }

    /// <summary>DwmSetWindowAttribute DWMWA_USE_IMMERSIVE_DARK_MODE is supported.</summary>
    public bool SupportsImmersiveDarkMode { get; init; }

    /// <summary>Undocumented SetWindowCompositionAttribute is present on this build (may break after updates).</summary>
    public bool SupportsWindowCompositionAttribute { get; init; }

    /// <summary>Taskbar transparency via this backend is functional.</summary>
    public bool SupportsTaskbarTransparency { get; init; }

    /// <summary>Build number at probe time.</summary>
    public int BuildNumber { get; init; }

    /// <summary>UTC timestamp when the probe was performed.</summary>
    public DateTimeOffset ProbedAt { get; init; } = DateTimeOffset.UtcNow;
}
