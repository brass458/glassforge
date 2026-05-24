using GlassForge.Shell;

namespace GlassForge.Tests;

/// <summary>Task 4 — WindowsBuildDetector TDD</summary>
public class WindowsBuildDetectorTests
{
    [Fact]
    public void GetBuildNumber_ReturnsPositiveValue()
    {
        var build = WindowsBuildDetector.GetBuildNumber();
        Assert.True(build > 0, $"Expected positive build number, got {build}");
    }

    [Fact]
    public void GetBuildNumber_ReturnsAtLeastWindows10()
    {
        // Windows 10 first build = 10240
        var build = WindowsBuildDetector.GetBuildNumber();
        Assert.True(build >= 10240, $"Build {build} is below Windows 10 minimum (10240)");
    }

    [Fact]
    public void GetBuildNumber_IsCached_ReturnsSameValueOnRepeatCalls()
    {
        var first  = WindowsBuildDetector.GetBuildNumber();
        var second = WindowsBuildDetector.GetBuildNumber();
        Assert.Equal(first, second);
    }

    [Fact]
    public void GetUpdateBuildRevision_ReturnsNonNegativeValue()
    {
        var ubr = WindowsBuildDetector.GetUpdateBuildRevision();
        Assert.True(ubr >= 0, $"UBR should be >= 0, got {ubr}");
    }
}
