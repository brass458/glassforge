namespace GlassForge.Tests;

using GlassForge.Shell;

public class WindowsBuildDetectorTests
{
    [Fact]
    public void Detect_WithTestProbe_ReturnsProbedBuild()
    {
        var (build, _) = WindowsBuildDetector.Detect(() => (22631, 4169));
        Assert.Equal(22631, build);
    }

    [Fact]
    public void Detect_WithTestProbe_ReturnsProbedUbr()
    {
        var (_, ubr) = WindowsBuildDetector.Detect(() => (22631, 4169));
        Assert.Equal(4169, ubr);
    }

    [Fact]
    public void Detect_WithNullProbe_ReturnsBuildAboveZero()
    {
        var (build, _) = WindowsBuildDetector.Detect();
        Assert.True(build > 0, $"Expected build > 0, got {build}");
    }

    [Fact]
    public void Detect_WithNullProbe_ReturnsUbrAtLeastZero()
    {
        var (_, ubr) = WindowsBuildDetector.Detect();
        Assert.True(ubr >= 0, $"Expected ubr >= 0, got {ubr}");
    }
}
