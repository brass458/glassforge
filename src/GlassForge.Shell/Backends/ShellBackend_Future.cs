namespace GlassForge.Shell.Backends;

using GlassForge.Core.Settings;
using GlassForge.Shell.Abstractions;

public sealed class ShellBackend_Future : IShellBackend
{
    private readonly Func<int, int>? _testDwmSet;
    private readonly Action<NativeMethods.AccentState, int>? _testSwca;

    public ShellBackend_Future() { }

    internal ShellBackend_Future(Func<int, int>? testDwmSet, Action<NativeMethods.AccentState, int>? testSwca)
    {
        _testDwmSet = testDwmSet;
        _testSwca = testSwca;
    }

    public string Name => "Win11_Future";
    public int MinBuild => 26200;
    public int MaxBuild => int.MaxValue;

    public ShellCapabilities ProbeCapabilities() => DwmProber.Probe();

    public void ApplyTaskbarEffect(IntPtr hwnd, AppSettings settings)
    {
        if (settings.TaskbarBackdropMode is "Acrylic" or "Blur")
        {
            int hr = _testDwmSet != null
                ? _testDwmSet(NativeMethods.DWMSBT_TRANSIENTWINDOW)
                : CallDwmSet(hwnd, NativeMethods.DWMSBT_TRANSIENTWINDOW);

            if (hr == 0) return;

            var state = settings.TaskbarBackdropMode == "Acrylic"
                ? NativeMethods.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND
                : NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND;
            int gradient = (int)(settings.TaskbarOpacity * 255) << 24;

            if (_testSwca != null)
                _testSwca(state, gradient);
            else
                NativeMethods.ApplyAccentPolicy(hwnd, state, gradient);
        }
        else
        {
            int none = NativeMethods.DWMSBT_NONE;
            if (_testDwmSet != null)
                _testDwmSet(none);
            else
                CallDwmSet(hwnd, none);
        }
    }

    public void RemoveTaskbarEffect(IntPtr hwnd)
    {
        if (_testSwca != null)
            _testSwca(NativeMethods.AccentState.ACCENT_DISABLED, 0);
        else
            NativeMethods.ApplyAccentPolicy(hwnd, NativeMethods.AccentState.ACCENT_DISABLED, 0);
    }

    private static int CallDwmSet(IntPtr hwnd, int sbt)
    {
        int val = sbt;
        return NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_SYSTEMBACKDROP_TYPE, ref val, 4);
    }
}
