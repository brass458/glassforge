namespace GlassForge.Tests;

using GlassForge.Core.Settings;
using GlassForge.Shell;
using GlassForge.Shell.Abstractions;
using GlassForge.Shell.Backends;

public class CapabilityMapTests
{
    private class TestBackend : IShellBackend
    {
        private readonly ShellCapabilities _caps;
        public TestBackend(ShellCapabilities caps) => _caps = caps;
        public string Name => "Test";
        public int MinBuild => 0;
        public int MaxBuild => int.MaxValue;
        public ShellCapabilities ProbeCapabilities() => _caps;
        public void ApplyTaskbarEffect(IntPtr hwnd, AppSettings settings) { }
        public void RemoveTaskbarEffect(IntPtr hwnd) { }
    }

    [Fact]
    public void Current_IsNotNullAfterProbe()
    {
        var map = new CapabilityMap();
        map.Probe(new TestBackend(new ShellCapabilities()));
        Assert.NotNull(map.Current);
    }

    [Fact]
    public void Current_ReflectsCapabilitiesReturnedByBackend()
    {
        var map = new CapabilityMap();
        var caps = new ShellCapabilities { SupportsSystemBackdropType = true };
        map.Probe(new TestBackend(caps));
        Assert.True(map.Current.SupportsSystemBackdropType);
        Assert.False(map.Current.SupportsCaptionColor);
    }

    [Fact]
    public void Probe_UpdatesCurrentOnSecondCall()
    {
        var map = new CapabilityMap();
        map.Probe(new TestBackend(new ShellCapabilities()));
        var first = map.Current;
        map.Probe(new TestBackend(new ShellCapabilities { SupportsBorderColor = true }));
        Assert.NotSame(first, map.Current);
        Assert.True(map.Current.SupportsBorderColor);
    }

    [Fact]
    public void BackendName_ReflectsProvedBackend()
    {
        var map = new CapabilityMap();
        map.Probe(ShellBackendFactory.Create(22631));
        Assert.Equal("Win11_23H2", map.BackendName);
    }

    [Fact]
    public void BackendName_UpdatesOnReprobe()
    {
        var map = new CapabilityMap();
        map.Probe(ShellBackendFactory.Create(22631));
        map.Probe(ShellBackendFactory.Create(26100));
        Assert.Equal("Win11_24H2", map.BackendName);
    }

    [Fact]
    public async Task Probe_IsThreadSafe_ConcurrentReadsDoNotThrow()
    {
        var map = new CapabilityMap();
        var backend1 = new TestBackend(new ShellCapabilities());
        var backend2 = new TestBackend(new ShellCapabilities { SupportsSystemBackdropType = true });
        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        var tasks = Enumerable.Range(0, 8).Select(i => Task.Run(() =>
        {
            try
            {
                for (int j = 0; j < 100; j++)
                {
                    if (j % 20 == 0)
                        map.Probe(i % 2 == 0 ? backend1 : backend2);
                    _ = map.Current;
                    _ = map.BackendName;
                }
            }
            catch (Exception ex) { exceptions.Add(ex); }
        })).ToArray();

        await Task.WhenAll(tasks);
        Assert.Empty(exceptions);
    }
}
