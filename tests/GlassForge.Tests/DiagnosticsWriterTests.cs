using GlassForge.Core.Diagnostics;

namespace GlassForge.Tests;

/// <summary>Task 8 — DiagnosticsWriter TDD</summary>
public class DiagnosticsWriterTests : IDisposable
{
    // Write to a temp directory so tests don't touch %APPDATA%\GlassForge
    private readonly string _tempDir;
    private readonly DiagnosticsWriterTestable _writer;

    public DiagnosticsWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "GlassForge.Tests." + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
        _writer = new DiagnosticsWriterTestable(_tempDir);
    }

    [Fact]
    public void Write_CreatesFile()
    {
        var snapshot = new DiagnosticSnapshot { BuildNumber = 22621 };
        var path = _writer.Write(snapshot);
        Assert.True(File.Exists(path), $"Expected file to exist at: {path}");
    }

    [Fact]
    public void Write_FileContainsJson()
    {
        var snapshot = new DiagnosticSnapshot { BuildNumber = 12345, ActiveBackend = "TestBackend" };
        var path = _writer.Write(snapshot);
        var content = File.ReadAllText(path);
        Assert.Contains("12345", content);
        Assert.Contains("TestBackend", content);
    }

    [Fact]
    public void Write_ReturnsNonEmptyPath_OnSuccess()
    {
        var path = _writer.Write(new DiagnosticSnapshot());
        Assert.False(string.IsNullOrEmpty(path));
    }

    [Fact]
    public void ListFiles_ReturnsEmpty_WhenNoFiles()
    {
        var files = _writer.ListFiles();
        Assert.Empty(files);
    }

    [Fact]
    public void ListFiles_ReturnsFiles_AfterWrite()
    {
        _writer.Write(new DiagnosticSnapshot());
        _writer.Write(new DiagnosticSnapshot());
        var files = _writer.ListFiles();
        Assert.Equal(2, files.Count);
    }

    [Fact]
    public void Prune_DeletesOldFiles_BeyondRetainCount()
    {
        for (int i = 0; i < 5; i++)
        {
            _writer.Write(new DiagnosticSnapshot());
            Thread.Sleep(10); // ensure distinct timestamps
        }
        _writer.Prune(retainCount: 3);
        var remaining = _writer.ListFiles();
        Assert.Equal(3, remaining.Count);
    }

    [Fact]
    public void Write_DoesNotThrow_OnInvalidPath()
    {
        var badWriter = new DiagnosticsWriterTestable(@"\\?\invalid_path_that_cannot_exist_abc");
        var ex = Record.Exception(() => badWriter.Write(new DiagnosticSnapshot()));
        Assert.Null(ex); // must never throw
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }
}

/// <summary>Testable subclass that writes to a configurable directory.</summary>
internal sealed class DiagnosticsWriterTestable : DiagnosticsWriter
{
    private readonly string _dir;

    public DiagnosticsWriterTestable(string dir) : base(dir)
    {
        _dir = dir;
    }
}
