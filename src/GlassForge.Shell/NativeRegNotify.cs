using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace GlassForge.Shell;

/// <summary>Internal helper that wraps RegNotifyChangeKeyValue for the UBR watcher.</summary>
internal static class NativeRegNotify
{
    private const int REG_NOTIFY_CHANGE_LAST_SET = 0x00000004;
    private const int INFINITE                   = -1;

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegNotifyChangeKeyValue(
        nint hKey,
        bool bWatchSubtree,
        int  dwNotifyFilter,
        nint hEvent,
        bool fAsynchronous);

    /// <summary>
    /// Blocks the calling thread until the given registry key is modified.
    /// Uses a synchronous (event-less) wait — appropriate for a background thread.
    /// </summary>
    internal static void WaitForChange(RegistryKey key)
    {
        // SafeRegistryHandle exposes the raw HKEY handle
        var hKey = key.Handle.DangerousGetHandle();
        RegNotifyChangeKeyValue(hKey, false, REG_NOTIFY_CHANGE_LAST_SET,
                                nint.Zero, false);
    }
}
