namespace GlassForge.Tests;

using GlassForge.Core.Models;
using GlassForge.Core.Themes;

public class ThemePresetServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    private ThemePresetService MakeService() => new(_dir);

    public ThemePresetServiceTests() => Directory.CreateDirectory(_dir);
    public void Dispose() => Directory.Delete(_dir, recursive: true);

    [Fact]
    public void GetCustomPresets_ReturnsEmpty_WhenFileMissing()
    {
        var svc = MakeService();
        Assert.Empty(svc.GetCustomPresets());
    }

    [Fact]
    public void SaveAndLoad_RoundTrip()
    {
        var svc = MakeService();
        var preset = new ThemePreset { Name = "My Theme", AccentColorHex = "#FF0000" };
        svc.SaveCustomPreset(preset);

        var svc2 = MakeService();
        var loaded = svc2.GetCustomPresets();
        Assert.Single(loaded);
        Assert.Equal("My Theme", loaded[0].Name);
        Assert.Equal("#FF0000", loaded[0].AccentColorHex);
    }

    [Fact]
    public void Delete_RemovesPreset()
    {
        var svc = MakeService();
        svc.SaveCustomPreset(new ThemePreset { Name = "ToDelete" });
        svc.DeleteCustomPreset("ToDelete");

        var svc2 = MakeService();
        Assert.Empty(svc2.GetCustomPresets());
    }

    [Fact]
    public void Save_UpdatesExistingPresetWithSameName()
    {
        var svc = MakeService();
        svc.SaveCustomPreset(new ThemePreset { Name = "UpdateMe", AccentColorHex = "#111111" });
        svc.SaveCustomPreset(new ThemePreset { Name = "UpdateMe", AccentColorHex = "#222222" });

        var svc2 = MakeService();
        var loaded = svc2.GetCustomPresets();
        Assert.Single(loaded);
        Assert.Equal("#222222", loaded[0].AccentColorHex);
    }
}
