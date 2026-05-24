namespace GlassForge.Shell;

using GlassForge.Core.Settings;
using GlassForge.Shell.Abstractions;

public class TaskbarEffectService
{
    private readonly Func<string, string?, IntPtr> _findWindow;
    private readonly Func<IntPtr, bool> _isWindow;

    public TaskbarEffectService(
        Func<string, string?, IntPtr>? findWindow = null,
        Func<IntPtr, bool>? isWindow = null)
    {
        _findWindow = findWindow ?? NativeMethods.FindWindow;
        _isWindow = isWindow ?? NativeMethods.IsWindow;
    }

    public bool Apply(IShellBackend backend, AppSettings settings)
    {
        var hwnd = _findWindow("Shell_TrayWnd", null);
        if (!_isWindow(hwnd)) return false;

        if (!settings.TaskbarEffectEnabled)
            backend.RemoveTaskbarEffect(hwnd);
        else
            backend.ApplyTaskbarEffect(hwnd, settings);

        return true;
    }

    public void Remove(IShellBackend backend)
    {
        var hwnd = _findWindow("Shell_TrayWnd", null);
        if (_isWindow(hwnd))
            backend.RemoveTaskbarEffect(hwnd);
    }
}
