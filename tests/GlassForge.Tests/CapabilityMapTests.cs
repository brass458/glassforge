namespace GlassForge.Tests;

using GlassForge.Shell;
using GlassForge.Shell.Backends;

public class CapabilityMapTests
{
    [Fact]
    public void Current_IsNotNullAfterProbe()
    {
        var map = new CapabilityMap();
        map.Probe(ShellBackendFactory.Create(22631));
        Assert.NotNull(map.Current);
    }

    [Fact]
    public void Current_AllFalseInV010()
    {
        var map = new CapabilityMap();
        map.Probe(ShellBackendFactory.Create(22631));
        Assert.False(map.Current.SupportsSystemBackdropType);
        Assert.False(map.Current.SupportsCaptionColor);
        Assert.False(map.Current.SupportsBorderColor);
        Assert.False(map.Current.SupportsImmersiveDarkMode);
        Assert.False(map.Current.SupportsWindowCompositionAttribute);
    }

    [Fact]
    public void Probe_UpdatesCurrentOnSecondCall()
    {
        var map = new CapabilityMap();
        map.Probe(ShellBackendFactory.Create(22631));
        var first = map.Current;
        map.Probe(ShellBackendFactory.Create(26100));
        Assert.NotSame(first, map.Current);
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
        var backend1 = new ShellBackend_22H2();
        var backend2 = new ShellBackend_23H2();
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
