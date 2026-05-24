namespace GlassForge.Tests;

using GlassForge.Shell;

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
}
