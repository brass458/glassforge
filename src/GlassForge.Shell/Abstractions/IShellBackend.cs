namespace GlassForge.Shell.Abstractions;

using GlassForge.Core.Settings;

public interface IShellBackend
{
    string Name { get; }
    int MinBuild { get; }
    int MaxBuild { get; }
    ShellCapabilities ProbeCapabilities();
    void ApplyTaskbarEffect(IntPtr hwnd, AppSettings settings);
    void RemoveTaskbarEffect(IntPtr hwnd);
}
