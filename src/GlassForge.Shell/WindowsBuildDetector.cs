using System.Runtime.InteropServices;

namespace GlassForge.Shell;

/// <summary>
/// Retrieves the true Windows build number via RtlGetVersion.
/// Environment.OSVersion lies on Windows 11 (reports 10.0 without a manifest),
/// so we always P/Invoke into ntdll directly.
/// </summary>
public static class WindowsBuildDetector
{
    [StructLayout(LayoutKind.Sequential)]
    private struct OSVERSIONINFOEX
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
        public byte   wProductType;
        public byte   wReserved;
    }

    [DllImport("ntdll.dll", SetLastError = false)]
    private static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

    private static int? _cachedBuild;

    /// <summary>
    /// Returns the true Windows build number (e.g. 22621 for 22H2, 22000 for 21H2).
    /// Reads the UBR from the registry and returns MajorBuild (e.g. 22621) — UBR details
    /// are available separately via <see cref="GetUpdateBuildRevision"/>.
    /// </summary>
    public static int GetBuildNumber()
    {
        if (_cachedBuild.HasValue) return _cachedBuild.Value;

        var info = new OSVERSIONINFOEX
        {
            dwOSVersionInfoSize = (uint)Marshal.SizeOf<OSVERSIONINFOEX>()
        };

        if (RtlGetVersion(ref info) == 0)
        {
            _cachedBuild = (int)info.dwBuildNumber;
            return _cachedBuild.Value;
        }

        // Fallback: Environment.OSVersion (may lie, but better than nothing)
        _cachedBuild = Environment.OSVersion.Version.Build;
        return _cachedBuild.Value;
    }

    /// <summary>
    /// Returns the Update Build Revision from the registry.
    /// UBR differentiates patch-level builds, e.g. 22621.2861 → UBR = 2861.
    /// Returns 0 if not available.
    /// </summary>
    public static int GetUpdateBuildRevision()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            return (int?)key?.GetValue("UBR") ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>Clears the cached build number (for testing).</summary>
    internal static void ResetCache() => _cachedBuild = null;
}
