namespace GlassForge.Tests;

using GlassForge.Core.Settings;

public class SettingsServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    private SettingsService MakeService() => new(_dir);

    public SettingsServiceTests() => Directory.CreateDirectory(_dir);
    public void Dispose() => Directory.Delete(_dir, recursive: true);

    [Fact]
    public void Load_ReturnsDefaults_WhenFileMissing()
    {
        var settings = MakeService().Load();
        Assert.Equal("GlassForge Default", settings.ActivePresetName);
        Assert.False(settings.SmartTintEnabled);
        Assert.Equal(0.85f, settings.TaskbarOpacity);
        Assert.Equal("Acrylic", settings.TaskbarBackdropMode);
    }

    [Fact]
    public void Load_ReturnsDefaults_WhenJsonMalformed()
    {
        File.WriteAllText(Path.Combine(_dir, "config.json"), "not valid json {{{");
        var settings = MakeService().Load();
        Assert.Equal("GlassForge Default", settings.ActivePresetName);
    }

    [Fact]
    public void SaveAndLoad_RoundTrip()
    {
        var service = MakeService();
        service.Save(new AppSettings { ActivePresetName = "Midnight Blue", TaskbarOpacity = 0.5f });
        var loaded = service.Load();
        Assert.Equal("Midnight Blue", loaded.ActivePresetName);
        Assert.Equal(0.5f, loaded.TaskbarOpacity);
    }

    [Fact]
    public void Save_WritesFile()
    {
        MakeService().Save(new AppSettings());
        Assert.True(File.Exists(Path.Combine(_dir, "config.json")));
    }

    [Fact]
    public void Save_LeavesNoTempFile()
    {
        MakeService().Save(new AppSettings());
        Assert.False(File.Exists(Path.Combine(_dir, "config.tmp")));
    }
}
