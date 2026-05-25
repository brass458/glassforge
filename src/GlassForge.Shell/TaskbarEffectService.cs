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
        {
            backend.RemoveTaskbarEffect(hwnd);
            foreach (var child in VisibleChildren(hwnd))
                backend.RemoveTaskbarEffect(child);
        }
        else
        {
            backend.ApplyTaskbarEffect(hwnd, settings);
            foreach (var child in VisibleChildren(hwnd))
                backend.ApplyTaskbarEffect(child, settings);
        }

        return true;
    }

    public void Remove(IShellBackend backend)
    {
        var hwnd = _findWindow("Shell_TrayWnd", null);
        if (!_isWindow(hwnd)) return;

        backend.RemoveTaskbarEffect(hwnd);
        foreach (var child in VisibleChildren(hwnd))
            backend.RemoveTaskbarEffect(child);
    }

    private static IEnumerable<IntPtr> VisibleChildren(IntPtr parent)
    {
        var child = NativeMethods.FindWindowEx(parent, IntPtr.Zero, null, null);
        while (child != IntPtr.Zero)
        {
            if (NativeMethods.IsWindowVisible(child))
                yield return child;
            child = NativeMethods.FindWindowEx(parent, child, null, null);
        }
    }
}
