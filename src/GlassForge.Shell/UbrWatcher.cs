namespace GlassForge.Shell;

using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32;

public sealed class UbrWatcher : IDisposable
{
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegNotifyChangeKeyValue(
        IntPtr hKey,
        bool bWatchSubtree,
        uint dwNotifyFilter,
        IntPtr hEvent,
        bool fAsynchronous);

    private const uint REG_NOTIFY_CHANGE_LAST_SET = 0x00000004;

    public event Action<int>? UbrChanged;

    private readonly Func<int> _readUbr;
    private int _cachedUbr;
    private RegistryKey? _key;
    private AutoResetEvent? _waitHandle;
    private RegisteredWaitHandle? _registeredWait;
    private volatile bool _disposed;

    public UbrWatcher(Func<int>? readUbr = null)
    {
        _readUbr = readUbr ?? ReadUbrFromRegistry;
    }

    public void Start()
    {
        _cachedUbr = _readUbr();
        _key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion")
            ?? throw new InvalidOperationException(
                "GlassForge could not open the Windows version registry key for UBR monitoring.");
        _waitHandle = new AutoResetEvent(false);
        RegisterNotification();
        ScheduleWait();
    }

    private void RegisterNotification()
    {
        if (_key == null || _waitHandle == null || _disposed) return;
        int result = RegNotifyChangeKeyValue(
            _key.Handle.DangerousGetHandle(),
            false,
            REG_NOTIFY_CHANGE_LAST_SET,
            _waitHandle.SafeWaitHandle.DangerousGetHandle(),
            true);
        if (result != 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    private void ScheduleWait()
    {
        _registeredWait = ThreadPool.RegisterWaitForSingleObject(
            _waitHandle!, OnRegistryChanged, null, Timeout.Infinite, executeOnlyOnce: true);
    }

    private void OnRegistryChanged(object? state, bool timedOut)
    {
        if (_disposed) return;
        var newUbr = _readUbr();
        if (newUbr != _cachedUbr)
        {
            _cachedUbr = newUbr;
            UbrChanged?.Invoke(newUbr);
        }
        if (!_disposed)
        {
            RegisterNotification();
            ScheduleWait();
        }
    }

    private static int ReadUbrFromRegistry()
    {
        using var key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        return (int)(key?.GetValue("UBR") ?? 0);
    }

    public void Dispose()
    {
        _disposed = true;
        _registeredWait?.Unregister(_waitHandle);
        _waitHandle?.Dispose();
        _key?.Dispose();
    }
}
