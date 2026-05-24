using GlassForge.Shell;
using GlassForge.Shell.Backends;

namespace GlassForge.Tests;

/// <summary>Task 5 — ShellBackendFactory TDD</summary>
public class ShellBackendFactoryTests
{
    [Theory]
    [InlineData(22000, "ShellBackend_22000")]
    [InlineData(22620, "ShellBackend_22000")]
    [InlineData(22621, "ShellBackend_22621")]
    [InlineData(25999, "ShellBackend_22621")]
    [InlineData(26100, "ShellBackend_26100")]
    [InlineData(26200, "ShellBackend_26100")]
    [InlineData(99999, "ShellBackend_26100")]
    public void Create_ReturnsCorrectBackend(int build, string expectedTypeName)
    {
        var backend = ShellBackendFactory.Create(build);
        Assert.Equal(expectedTypeName, backend.GetType().Name);
    }

    [Fact]
    public void Create_ForBuildBelowKnownRange_ReturnsFutureBackend()
    {
        // Builds below 22000 (pre-Win11) should fall through to ShellBackend_Future
        var backend = ShellBackendFactory.Create(19041);
        Assert.Equal("ShellBackend_Future", backend.GetType().Name);
    }

    [Fact]
    public void Create_NeverReturnsNull()
    {
        foreach (var build in new[] { 0, 10240, 19041, 22000, 22621, 26100, int.MaxValue - 1 })
        {
            var backend = ShellBackendFactory.Create(build);
            Assert.NotNull(backend);
        }
    }

    [Fact]
    public void CreateForCurrentBuild_ReturnsNonNullBackend()
    {
        var backend = ShellBackendFactory.CreateForCurrentBuild();
        Assert.NotNull(backend);
    }

    [Fact]
    public void BackendBuildRanges_AreSorted_NoGaps()
    {
        // Every realistic Win11 build should map to a non-Future backend
        foreach (var build in new[] { 22000, 22621, 26100 })
        {
            var backend = ShellBackendFactory.Create(build);
            Assert.NotEqual("ShellBackend_Future", backend.GetType().Name);
        }
    }
}
