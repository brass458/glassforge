namespace GlassForge.Shell.Abstractions;

public interface IShellBackend
{
    string Name { get; }
    int MinBuild { get; }
    int MaxBuild { get; }
    ShellCapabilities ProbeCapabilities();
}
