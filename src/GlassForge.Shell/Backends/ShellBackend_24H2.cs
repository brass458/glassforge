namespace GlassForge.Shell.Backends;

using GlassForge.Shell.Abstractions;

public sealed class ShellBackend_24H2 : IShellBackend
{
    public string Name => "Win11_24H2";
    public int MinBuild => 26100;
    public int MaxBuild => 26199;
    public ShellCapabilities ProbeCapabilities() => new();
}
