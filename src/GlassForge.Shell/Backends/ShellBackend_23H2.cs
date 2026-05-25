namespace GlassForge.Shell.Backends;

using GlassForge.Core.Settings;
using GlassForge.Shell.Abstractions;

public sealed class ShellBackend_23H2 : IShellBackend
{
    private readonly Action<NativeMethods.AccentState, int>? _testSwca;

    public ShellBackend_23H2() { }

    internal ShellBackend_23H2(Action<NativeMethods.AccentState, int>? testSwca)
        => _testSwca = testSwca;

    public string Name => "Win11_23H2";
    public int MinBuild => 22631;
    public int MaxBuild => 26099;

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
            NativeMethods.ApplyAccentPolicy(hwnd, state, gradient, isAcrylic ? 2 : 0);
    }

    public void RemoveTaskbarEffect(IntPtr hwnd)
    {
        if (_testSwca != null)
            _testSwca(NativeMethods.AccentState.ACCENT_DISABLED, 0);
        else
            NativeMethods.ApplyAccentPolicy(hwnd, NativeMethods.AccentState.ACCENT_DISABLED, 0);
    }
}
