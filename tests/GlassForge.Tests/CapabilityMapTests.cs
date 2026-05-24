using GlassForge.Shell;
using GlassForge.Shell.Abstractions;

namespace GlassForge.Tests;

/// <summary>Task 6 — CapabilityMap TDD</summary>
public class CapabilityMapTests
{
    private sealed class StubBackend : IShellBackend
    {
        public int MinBuild => 0;
        public int MaxBuild => int.MaxValue;
        public bool ApplyCaptionColor(nint hwnd, uint colorRef)  => false;
        public bool ApplySystemBackdrop(nint hwnd, SystemBackdropType type) => false;
        public bool ApplyTaskbarTransparency(TaskbarStyle style)  => false;
        public void ResetTaskbar() { }

        public CapabilityProbeResult Probe() => new()
        {
            BuildNumber                        = 22621,
            SupportsCaptionColor               = true,
            SupportsBorderColor                = true,
            SupportsImmersiveDarkMode          = true,
            SupportsSystemBackdrop             = true,
            SupportsWindowCompositionAttribute = false,
            SupportsTaskbarTransparency        = false,
        };
    }

    [Fact]
    public void IsProbed_IsFalse_BeforeRefresh()
    {
        var map = new CapabilityMap();
        Assert.False(map.IsProbed);
    }

    [Fact]
    public void IsProbed_IsTrue_AfterRefresh()
    {
        var map = new CapabilityMap();
        map.Refresh(new StubBackend());
        Assert.True(map.IsProbed);
    }

    [Fact]
    public void Refresh_PopulatesCapabilityFlags_FromProbeResult()
    {
        var map = new CapabilityMap();
        map.Refresh(new StubBackend());

        Assert.Equal(22621, map.BuildNumber);
        Assert.True(map.SupportsCaptionColor);
        Assert.True(map.SupportsBorderColor);
        Assert.True(map.SupportsImmersiveDarkMode);
        Assert.True(map.SupportsSystemBackdrop);
        Assert.False(map.SupportsWindowCompositionAttribute);
        Assert.False(map.SupportsTaskbarTransparency);
    }

    [Fact]
    public void Refresh_UpdatesLastProbeTime()
    {
        var map    = new CapabilityMap();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        map.Refresh(new StubBackend());
        Assert.True(map.LastProbeTime >= before);
    }

    [Fact]
    public void GetLastResult_ReturnsNull_BeforeProbe()
    {
        var map = new CapabilityMap();
        Assert.Null(map.GetLastResult());
    }

    [Fact]
    public void GetLastResult_ReturnsProbeResult_AfterRefresh()
    {
        var map = new CapabilityMap();
        map.Refresh(new StubBackend());
        Assert.NotNull(map.GetLastResult());
    }

    [Fact]
    public void Refresh_IsThreadSafe_NoExceptions()
    {
        var map    = new CapabilityMap();
        var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        var threads = Enumerable.Range(0, 8).Select(_ => new Thread(() =>
        {
            try { map.Refresh(new StubBackend()); }
            catch (Exception ex) { errors.Add(ex); }
        })).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join(TimeSpan.FromSeconds(5)));

        Assert.Empty(errors);
    }
}
