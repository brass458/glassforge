namespace GlassForge.Tests;

using GlassForge.Shell;
using GlassForge.Shell.Abstractions;

public class DiagnosticsWriterTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    private string FilePath => Path.Combine(_dir, "diagnostics.json");

    public DiagnosticsWriterTests() => Directory.CreateDirectory(_dir);
    public void Dispose() => Directory.Delete(_dir, recursive: true);

    [Fact]
    public void Write_CreatesFile()
    {
        DiagnosticsWriter.Write(22631, 4169, "Win11_23H2", new ShellCapabilities(), FilePath);
        Assert.True(File.Exists(FilePath));
    }

    [Fact]
    public void Write_ContainsBuildNumber()
    {
        DiagnosticsWriter.Write(22631, 4169, "Win11_23H2", new ShellCapabilities(), FilePath);
        Assert.Contains("22631", File.ReadAllText(FilePath));
    }

    [Fact]
    public void Write_ContainsBackendName()
    {
        DiagnosticsWriter.Write(22631, 4169, "Win11_23H2", new ShellCapabilities(), FilePath);
        Assert.Contains("Win11_23H2", File.ReadAllText(FilePath));
    }

    [Fact]
    public void Write_ContainsUbr()
    {
        DiagnosticsWriter.Write(22631, 4169, "Win11_23H2", new ShellCapabilities(), FilePath);
        Assert.Contains("4169", File.ReadAllText(FilePath));
    }

    [Fact]
    public void Write_OverwritesPreviousFile()
    {
        DiagnosticsWriter.Write(22631, 4169, "Win11_23H2", new ShellCapabilities(), FilePath);
        DiagnosticsWriter.Write(26100, 1234, "Win11_24H2", new ShellCapabilities(), FilePath);
        var json = File.ReadAllText(FilePath);
        Assert.Contains("26100", json);
        Assert.DoesNotContain("22631", json);
    }
}
