namespace GlassForge.Core.Diagnostics;

/// <summary>
/// Captures the system state at a point in time for debugging and support.
/// Serialized to JSON by <see cref="DiagnosticsWriter"/>.
/// </summary>
public sealed class DiagnosticSnapshot
{
    public DateTimeOffset CapturedAt    { get; init; } = DateTimeOffset.UtcNow;
    public string AppVersion            { get; init; } = "";
    public string WindowsVersion        { get; init; } = "";
    public int    BuildNumber           { get; init; }
    public int    UpdateBuildRevision   { get; init; }
    public string ActiveBackend         { get; init; } = "";

    // Capability flags (from CapabilityMap at capture time)
    public bool SupportsSystemBackdrop             { get; init; }
    public bool SupportsCaptionColor               { get; init; }
    public bool SupportsBorderColor                { get; init; }
    public bool SupportsImmersiveDarkMode          { get; init; }
    public bool SupportsWindowCompositionAttribute { get; init; }
    public bool SupportsTaskbarTransparency        { get; init; }

    // Environment
    public string ActivePresetId    { get; init; } = "";
    public string LastCompatManifestUrl { get; init; } = "";
    public bool   CompatManifestLoaded  { get; init; }

    public IReadOnlyList<string> Notes { get; init; } = [];
}
