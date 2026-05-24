using Microsoft.Win32;

namespace GlassForge.Shell;

/// <summary>
/// Watches HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\UBR for changes.
/// When Windows Update increments the UBR (Update Build Revision), the watcher
/// raises <see cref="BuildRevisionChanged"/> so GlassForge can re-probe capabilities
/// and reload the remote compat manifest.
/// </summary>
public sealed class UbrWatcher : IDisposable
{
    private const string KeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    private RegistryKey? _key;
    private Thread?      _thread;
    private volatile bool _disposed;
    private int _lastKnownUbr;

    /// <summary>Raised on the watcher thread when the UBR value changes.</summary>
    public event EventHandler<UbrChangedEventArgs>? BuildRevisionChanged;

    public UbrWatcher()
    {
        _lastKnownUbr = WindowsBuildDetector.GetUpdateBuildRevision();
    }

    /// <summary>Starts watching. Safe to call multiple times; only one thread runs.</summary>
    public void Start()
    {
        if (_thread is not null || _disposed) return;

        _key = Registry.LocalMachine.OpenSubKey(KeyPath, writable: false);
        if (_key is null) return; // can't watch — graceful no-op

        _thread = new Thread(WatchLoop)
        {
            IsBackground = true,
            Name         = "GlassForge.UbrWatcher",
        };
        _thread.Start();
    }

    private void WatchLoop()
    {
        while (!_disposed && _key is not null)
        {
            try
            {
                // Block until the registry key changes (or the handle is closed)
                NativeRegNotify.WaitForChange(_key);

                if (_disposed) break;

                var newUbr = WindowsBuildDetector.GetUpdateBuildRevision();
                if (newUbr != _lastKnownUbr)
                {
                    var args = new UbrChangedEventArgs(_lastKnownUbr, newUbr);
                    _lastKnownUbr = newUbr;
                    BuildRevisionChanged?.Invoke(this, args);
                }
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch
            {
                // If the wait fails, back off briefly and retry rather than tight-looping
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _key?.Dispose();
        _key = null;
    }
}

public sealed class UbrChangedEventArgs(int previousUbr, int newUbr) : EventArgs
{
    public int PreviousUbr { get; } = previousUbr;
    public int NewUbr      { get; } = newUbr;
}
