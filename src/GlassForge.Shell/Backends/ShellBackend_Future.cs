namespace GlassForge.Shell.Backends;

using GlassForge.Core.Settings;
using GlassForge.Shell.Abstractions;

public sealed class ShellBackend_Future : IShellBackend
{
    public string Name => "Win11_Future";
    public int MinBuild => 26200;
    public int MaxBuild => int.MaxValue;
    public ShellCapabilities ProbeCapabilities() => new();
    public void ApplyTaskbarEffect(IntPtr hwnd, AppSettings settings) { }
    public void RemoveTaskbarEffect(IntPtr hwnd) { }
}
