namespace GlassForge.Shell;

using System.Runtime.InteropServices;

internal static class NativeMethods
{
    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ACCENT_POLICY
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;  // AABBGGRR
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWCOMPOSITIONATTRIBDATA
    {
        public int Attribute;  // WCA_ACCENT_POLICY = 19
        public IntPtr Data;
        public int SizeOfData;
    }

    internal const int WCA_ACCENT_POLICY = 19;

    internal const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    internal const uint DWMWA_BORDER_COLOR = 34;
    internal const uint DWMWA_CAPTION_COLOR = 35;
    internal const uint DWMWA_SYSTEMBACKDROP_TYPE = 38;

    internal const int DWMSBT_NONE = 1;
    internal const int DWMSBT_TRANSIENTWINDOW = 3;

    internal const int HWND_MESSAGE = -3;
    internal const uint WS_POPUP = 0x80000000;
    internal const uint WS_EX_TOOLWINDOW = 0x00000080;
    internal const uint WS_EX_NOACTIVATE = 0x08000000;

    [DllImport("dwmapi.dll")]
    internal static extern int DwmGetWindowAttribute(
        IntPtr hwnd, uint dwAttribute, IntPtr pvAttribute, uint cbAttribute);

    [DllImport("dwmapi.dll")]
    internal static extern int DwmSetWindowAttribute(
        IntPtr hwnd, uint dwAttribute, ref int pvAttribute, uint cbAttribute);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool SetWindowCompositionAttribute(
        IntPtr hwnd, ref WINDOWCOMPOSITIONATTRIBDATA data);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    internal static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string? lpWindowName,
        uint dwStyle, int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    internal static extern bool DestroyWindow(IntPtr hWnd);

    internal static void ApplyAccentPolicy(IntPtr hwnd, AccentState state, int gradientColor, int accentFlags = 0)
    {
        var policy = new ACCENT_POLICY
        {
            AccentState = state,
            AccentFlags = accentFlags,
            GradientColor = gradientColor,
            AnimationId = 0,
        };
        var handle = GCHandle.Alloc(policy, GCHandleType.Pinned);
        try
        {
            var data = new WINDOWCOMPOSITIONATTRIBDATA
            {
                Attribute = WCA_ACCENT_POLICY,
                Data = handle.AddrOfPinnedObject(),
                SizeOfData = Marshal.SizeOf<ACCENT_POLICY>(),
            };
            SetWindowCompositionAttribute(hwnd, ref data);
        }
        finally
        {
            handle.Free();
        }
    }
}
