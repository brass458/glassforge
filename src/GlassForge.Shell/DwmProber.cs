namespace GlassForge.Shell;

using System.Runtime.InteropServices;
using GlassForge.Shell.Abstractions;

internal static class DwmProber
{
    internal static ShellCapabilities Probe(
        Func<IntPtr, uint, IntPtr, uint, int>? testDwmGet = null,
        Func<IntPtr, bool>? testSwca = null)
    {
        if (testDwmGet != null || testSwca != null)
            return ProbeWithSeams(IntPtr.Zero, testDwmGet ?? ((_, _, _, _) => 0), testSwca ?? (_ => false));

        var hwnd = NativeMethods.CreateWindowEx(
            NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOACTIVATE,
            "Static", null,
            NativeMethods.WS_POPUP,
            0, 0, 0, 0,
            new IntPtr(NativeMethods.HWND_MESSAGE),
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

        try
        {
            return ProbeWithSeams(hwnd, NativeMethods.DwmGetWindowAttribute, ProbeSwca);
        }
        finally
        {
            if (hwnd != IntPtr.Zero)
                NativeMethods.DestroyWindow(hwnd);
        }
    }

    private static ShellCapabilities ProbeWithSeams(
        IntPtr hwnd,
        Func<IntPtr, uint, IntPtr, uint, int> dwmGet,
        Func<IntPtr, bool> swca)
    {
        bool Check(uint attr)
        {
            IntPtr buf = Marshal.AllocHGlobal(4);
            try { return dwmGet(hwnd, attr, buf, 4) == 0; }
            finally { Marshal.FreeHGlobal(buf); }
        }

        return new ShellCapabilities
        {
            SupportsSystemBackdropType         = Check(NativeMethods.DWMWA_SYSTEMBACKDROP_TYPE),
            SupportsCaptionColor               = Check(NativeMethods.DWMWA_CAPTION_COLOR),
            SupportsBorderColor                = Check(NativeMethods.DWMWA_BORDER_COLOR),
            SupportsImmersiveDarkMode          = Check(NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE),
            SupportsWindowCompositionAttribute = swca(hwnd),
        };
    }

    private static bool ProbeSwca(IntPtr hwnd)
    {
        NativeMethods.ApplyAccentPolicy(hwnd, NativeMethods.AccentState.ACCENT_DISABLED, 0);
        return true;  // SetWindowCompositionAttribute exists on all Win11 builds we target
    }
}
