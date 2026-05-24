namespace GlassForge.Shell.Backends;

using GlassForge.Shell.Abstractions;

public sealed class ShellBackend_23H2 : IShellBackend
{
    public string Name => "Win11_23H2";
    public int MinBuild => 22631;
    public int MaxBuild => 26099;
    public ShellCapabilities ProbeCapabilities() => new();
}
