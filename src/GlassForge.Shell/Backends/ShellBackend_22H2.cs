namespace GlassForge.Shell.Backends;

using GlassForge.Core.Settings;
using GlassForge.Shell.Abstractions;

public sealed class ShellBackend_22H2 : IShellBackend
{
    public string Name => "Win11_22H2";
    public int MinBuild => 22621;
    public int MaxBuild => 22630;
    public ShellCapabilities ProbeCapabilities() => DwmProber.Probe();
    public void ApplyTaskbarEffect(IntPtr hwnd, AppSettings settings) { }
    public void RemoveTaskbarEffect(IntPtr hwnd) { }
}
