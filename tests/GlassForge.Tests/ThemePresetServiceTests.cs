using GlassForge.Core.Settings;
using GlassForge.Core.Themes;

namespace GlassForge.Tests;

/// <summary>Task 10 — ThemePresetService tests</summary>
public class ThemePresetServiceTests
{
    [Fact]
    public void BuiltInPresets_NotEmpty()
    {
        var svc = new ThemePresetService();
        Assert.NotEmpty(svc.AllPresets);
    }

    [Fact]
    public void AllPresets_ContainsGlassForgeDefault()
    {
        var svc = new ThemePresetService();
        Assert.Contains(svc.AllPresets, p => p.Id == "glassforge-default");
    }

    [Fact]
    public void AllPresets_Contains19BuiltIns()
    {
        // 19 presets ported from Nexus (glassforge-default replaces nexus-default)
        var svc    = new ThemePresetService();
        var builtIn = svc.AllPresets.Where(p => p.IsBuiltIn).ToList();
        Assert.Equal(19, builtIn.Count);
    }

    [Fact]
    public void AllPresets_HaveUniqueIds()
    {
        var svc = new ThemePresetService();
        var ids = svc.AllPresets.Select(p => p.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void AllPresets_HaveNonEmptyNames()
    {
        var svc = new ThemePresetService();
        Assert.All(svc.AllPresets, p => Assert.False(string.IsNullOrWhiteSpace(p.Name)));
    }

    [Fact]
    public void AllPresets_GlassOpacity_InRange()
    {
        var svc = new ThemePresetService();
        Assert.All(svc.AllPresets, p =>
        {
            Assert.True(p.GlassOpacity >= 0.0 && p.GlassOpacity <= 1.0,
                        $"Preset '{p.Id}' has GlassOpacity={p.GlassOpacity} out of [0,1]");
        });
    }

    [Fact]
    public void AllPresets_ValidThemeModes()
    {
        var valid = new[] { "Dark", "Light", "System" };
        var svc   = new ThemePresetService();
        Assert.All(svc.AllPresets, p =>
            Assert.Contains(p.ThemeMode, valid));
    }

    [Fact]
    public void SurfaceSwatchPalettes_ReturnsNonEmpty_ForKnownPreset()
    {
        var palette = SurfaceSwatchPalettes.GetPalette("neon", isDark: true);
        Assert.NotEmpty(palette);
        Assert.Equal(8, palette.Length);
    }

    [Fact]
    public void SurfaceSwatchPalettes_ReturnsDarkDefault_ForUnknownPreset()
    {
        var palette = SurfaceSwatchPalettes.GetPalette("unknown-preset-xyz", isDark: true);
        Assert.NotEmpty(palette);
    }

    [Fact]
    public void SurfaceSwatchPalettes_ReturnsLightDefault_ForUnknownPreset()
    {
        var dark  = SurfaceSwatchPalettes.GetPalette("", isDark: true);
        var light = SurfaceSwatchPalettes.GetPalette("", isDark: false);
        // Should be different palettes
        Assert.NotEqual(dark[0].Hex, light[0].Hex);
    }
}
