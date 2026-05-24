using System.Runtime.InteropServices;

namespace GlassForge.Shell.Backends;

/// <summary>
/// P/Invoke declarations for documented DWM APIs.
/// All backends share these; undocumented API calls live only inside their
/// respective ShellBackend_* class.
/// </summary>
internal static class DwmNative
{
    // DWMWA attribute constants
    internal const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    internal const int DWMWA_SYSTEMBACKDROP_TYPE     = 38;
    internal const int DWMWA_CAPTION_COLOR           = 35;
    internal const int DWMWA_BORDER_COLOR            = 34;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    internal static extern int DwmSetWindowAttribute(nint hwnd, int attr, ref int pvAttribute, int cbAttribute);

    [DllImport("dwmapi.dll", PreserveSig = true)]
    internal static extern int DwmGetWindowAttribute(nint hwnd, int attr, out int pvAttribute, int cbAttribute);

    /// <summary>
    /// Sets a DWM int attribute and then reads it back to confirm the effect was applied.
    /// Returns true only if both operations succeed and the read-back value matches.
    /// </summary>
    internal static bool SetAndVerify(nint hwnd, int attr, int value)
    {
        if (DwmSetWindowAttribute(hwnd, attr, ref value, sizeof(int)) != 0) return false;
        if (DwmGetWindowAttribute(hwnd, attr, out var readBack, sizeof(int)) != 0) return false;
        return readBack == value;
    }
}
