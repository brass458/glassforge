namespace GlassForge.Tests;

using GlassForge.Core.Settings;
using GlassForge.Shell;
using GlassForge.Shell.Abstractions;

public class TaskbarEffectServiceTests
{
    private class SpyBackend : IShellBackend
    {
        public string Name => "Spy";
        public int MinBuild => 0;
        public int MaxBuild => int.MaxValue;
        public ShellCapabilities ProbeCapabilities() => new();
        public int ApplyCalls { get; private set; }
        public int RemoveCalls { get; private set; }
        public AppSettings? LastAppliedSettings { get; private set; }
        public void ApplyTaskbarEffect(IntPtr hwnd, AppSettings settings)
        {
            ApplyCalls++;
            LastAppliedSettings = settings;
        }
        public void RemoveTaskbarEffect(IntPtr hwnd) => RemoveCalls++;
    }

    private static IntPtr ValidHwnd => new IntPtr(99);  // non-zero, "valid"
    private static IntPtr InvalidHwnd => IntPtr.Zero;

    // IsWindow stub: returns true for ValidHwnd, false for zero
    private static bool IsWindowStub(IntPtr hwnd) => hwnd != IntPtr.Zero;

    [Fact]
    public void Apply_CallsApplyTaskbarEffect_WhenHwndValidAndEffectEnabled()
    {
        var spy = new SpyBackend();
        var svc = new TaskbarEffectService(
            findWindow: (cls, _) => ValidHwnd,
            isWindow: IsWindowStub);
        var settings = new AppSettings { TaskbarEffectEnabled = true };

        bool result = svc.Apply(spy, settings);

        Assert.True(result);
        Assert.Equal(1, spy.ApplyCalls);
        Assert.Equal(0, spy.RemoveCalls);
    }

    [Fact]
    public void Apply_CallsRemoveTaskbarEffect_WhenEffectDisabled()
    {
        var spy = new SpyBackend();
        var svc = new TaskbarEffectService(
            findWindow: (cls, _) => ValidHwnd,
            isWindow: IsWindowStub);
        var settings = new AppSettings { TaskbarEffectEnabled = false };

        bool result = svc.Apply(spy, settings);

        Assert.True(result);
        Assert.Equal(0, spy.ApplyCalls);
        Assert.Equal(1, spy.RemoveCalls);
    }

    [Fact]
    public void Apply_ReturnsFalse_WhenHwndInvalid()
    {
        var spy = new SpyBackend();
        var svc = new TaskbarEffectService(
            findWindow: (cls, _) => InvalidHwnd,
            isWindow: IsWindowStub);

        bool result = svc.Apply(spy, new AppSettings());

        Assert.False(result);
        Assert.Equal(0, spy.ApplyCalls);
        Assert.Equal(0, spy.RemoveCalls);
    }

    [Fact]
    public void Remove_CallsRemoveTaskbarEffect_WhenHwndValid()
    {
        var spy = new SpyBackend();
        var svc = new TaskbarEffectService(
            findWindow: (cls, _) => ValidHwnd,
            isWindow: IsWindowStub);

        svc.Remove(spy);

        Assert.Equal(1, spy.RemoveCalls);
    }

    [Fact]
    public void Remove_DoesNotThrow_WhenHwndInvalid()
    {
        var spy = new SpyBackend();
        var svc = new TaskbarEffectService(
            findWindow: (cls, _) => InvalidHwnd,
            isWindow: IsWindowStub);

        svc.Remove(spy);

        Assert.Equal(0, spy.RemoveCalls);
    }

    [Fact]
    public void Apply_PassesUpdatedSettings_OnSecondCall()
    {
        var spy = new SpyBackend();
        var svc = new TaskbarEffectService(
            findWindow: (cls, _) => ValidHwnd,
            isWindow: IsWindowStub);

        svc.Apply(spy, new AppSettings { TaskbarOpacity = 0.5f });
        svc.Apply(spy, new AppSettings { TaskbarOpacity = 0.9f });

        Assert.Equal(2, spy.ApplyCalls);
        Assert.Equal(0.9f, spy.LastAppliedSettings!.TaskbarOpacity);
    }
}
