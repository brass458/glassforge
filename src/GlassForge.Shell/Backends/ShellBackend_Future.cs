namespace GlassForge.Shell.Backends;

using GlassForge.Core.Settings;
using GlassForge.Shell.Abstractions;

public sealed class ShellBackend_Future : IShellBackend
{
    private readonly Action<NativeMethods.AccentState, int>? _testSwca;

    public ShellBackend_Future() { }

    internal ShellBackend_Future(Action<NativeMethods.AccentState, int>? testSwca)
        => _testSwca = testSwca;

    public string Name => "Win11_Future";
    public int MinBuild => 26200;
    public int MaxBuild => int.MaxValue;

    public ShellCapabilities ProbeCapabilities() => DwmProber.Probe();

    public void ApplyTaskbarEffect(IntPtr hwnd, AppSettings settings)
    {
        var state = settings.TaskbarBackdropMode switch
        {
            "Acrylic" => NativeMethods.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
            "Blur"    => NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND,
            _         => NativeMethods.AccentState.ACCENT_DISABLED,
        };
        bool isAcrylic = state == NativeMethods.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
        int gradient = isAcrylic ? (int)(settings.TaskbarOpacity * 255) << 24 : 0;

        if (_testSwca != null)
            _testSwca(state, gradient);
        else
            NativeMethods.ApplyAccentPolicy(hwnd, state, gradient);
    }

    public void RemoveTaskbarEffect(IntPtr hwnd)
    {
        if (_testSwca != null)
            _testSwca(NativeMethods.AccentState.ACCENT_DISABLED, 0);
        else
            NativeMethods.ApplyAccentPolicy(hwnd, NativeMethods.AccentState.ACCENT_DISABLED, 0);
    }
}
