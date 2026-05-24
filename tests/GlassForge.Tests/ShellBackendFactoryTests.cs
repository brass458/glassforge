namespace GlassForge.Tests;

using GlassForge.Shell;
using GlassForge.Shell.Backends;

public class ShellBackendFactoryTests
{
    [Theory]
    [InlineData(22621, typeof(ShellBackend_22H2))]
    [InlineData(22630, typeof(ShellBackend_22H2))]
    [InlineData(22631, typeof(ShellBackend_23H2))]
    [InlineData(26099, typeof(ShellBackend_23H2))]
    [InlineData(26100, typeof(ShellBackend_24H2))]
    [InlineData(26199, typeof(ShellBackend_24H2))]
    [InlineData(26200, typeof(ShellBackend_Future))]
    [InlineData(99999, typeof(ShellBackend_Future))]
    [InlineData(0,     typeof(ShellBackend_Future))]
    public void Create_ReturnsCorrectBackendType(int buildNumber, Type expectedType)
    {
        var backend = ShellBackendFactory.Create(buildNumber);
        Assert.IsType(expectedType, backend);
    }

    [Theory]
    [InlineData(22621, "Win11_22H2")]
    [InlineData(22631, "Win11_23H2")]
    [InlineData(26100, "Win11_24H2")]
    [InlineData(99999, "Win11_Future")]
    public void Create_BackendHasCorrectName(int buildNumber, string expectedName)
    {
        var backend = ShellBackendFactory.Create(buildNumber);
        Assert.Equal(expectedName, backend.Name);
    }
}
