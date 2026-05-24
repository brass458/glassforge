namespace GlassForge.Shell;

using System.Runtime.InteropServices;
using Microsoft.Win32;

public static class WindowsBuildDetector
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct OSVERSIONINFOEX
    {
        public uint dwOSVersionInfoSize;
        public uint dwMajorVersion;
        public uint dwMinorVersion;
        public uint dwBuildNumber;
        public uint dwPlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szCSDVersion;
        public ushort wServicePackMajor;
        public ushort wServicePackMinor;
        public ushort wSuiteMask;
        public byte wProductType;
        public byte wReserved;
    }

    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

    public static (int Build, int Ubr) Detect(Func<(int Build, int Ubr)>? testProbe = null)
    {
        if (testProbe != null) return testProbe();

        var info = new OSVERSIONINFOEX
        {
            dwOSVersionInfoSize = (uint)Marshal.SizeOf<OSVERSIONINFOEX>()
        };
        RtlGetVersion(ref info);
        var build = (int)info.dwBuildNumber;

        using var key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        var ubr = (int)(key?.GetValue("UBR") ?? 0);

        return (build, ubr);
    }
}
