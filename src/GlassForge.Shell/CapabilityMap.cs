using GlassForge.Shell.Abstractions;

namespace GlassForge.Shell;

/// <summary>
/// Runtime capability map populated by probing the active <see cref="IShellBackend"/>
/// at startup and after Windows updates.
/// All properties are set from the probe result — never assumed from build number alone.
/// </summary>
public sealed class CapabilityMap
{
    private CapabilityProbeResult? _lastResult;
    private readonly object _lock = new();

    /// <summary>Whether a probe has been run at least once.</summary>
    public bool IsProbed => _lastResult is not null;

    public bool SupportsSystemBackdrop               { get; private set; }
    public bool SupportsCaptionColor                 { get; private set; }
    public bool SupportsBorderColor                  { get; private set; }
    public bool SupportsImmersiveDarkMode             { get; private set; }
    public bool SupportsWindowCompositionAttribute   { get; private set; }
    public bool SupportsTaskbarTransparency           { get; private set; }
    public int  BuildNumber                          { get; private set; }
    public DateTimeOffset LastProbeTime              { get; private set; }

    /// <summary>
    /// Runs the backend probe and updates the capability map.
    /// Thread-safe; may be called from a UBR watcher callback.
    /// </summary>
    public void Refresh(IShellBackend backend)
    {
        var result = backend.Probe();
        lock (_lock)
        {
            _lastResult                       = result;
            SupportsSystemBackdrop             = result.SupportsSystemBackdrop;
            SupportsCaptionColor               = result.SupportsCaptionColor;
            SupportsBorderColor                = result.SupportsBorderColor;
            SupportsImmersiveDarkMode          = result.SupportsImmersiveDarkMode;
            SupportsWindowCompositionAttribute = result.SupportsWindowCompositionAttribute;
            SupportsTaskbarTransparency        = result.SupportsTaskbarTransparency;
            BuildNumber                        = result.BuildNumber;
            LastProbeTime                      = result.ProbedAt;
        }
    }

    /// <summary>Returns the raw probe result, or null if not yet probed.</summary>
    public CapabilityProbeResult? GetLastResult()
    {
        lock (_lock) return _lastResult;
    }
}
