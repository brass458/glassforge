namespace GlassForge.Shell.Abstractions;

public record ShellCapabilities
{
    public bool SupportsSystemBackdropType { get; init; }
    public bool SupportsCaptionColor { get; init; }
    public bool SupportsBorderColor { get; init; }
    public bool SupportsImmersiveDarkMode { get; init; }
    public bool SupportsWindowCompositionAttribute { get; init; }
    public bool SupportsSystemTransparency { get; init; }
}
