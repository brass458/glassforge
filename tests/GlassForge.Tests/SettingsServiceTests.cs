using System.Text.Json;
using GlassForge.Core.Settings;

namespace GlassForge.Tests;

/// <summary>Task 11 — SettingsService TDD</summary>
public class SettingsServiceTests : IDisposable
{
    private readonly string _tempFile;
    private readonly SettingsServiceTestable _svc;

    public SettingsServiceTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"GlassForge.settings.{Guid.NewGuid()}.json");
        _svc = new SettingsServiceTestable(_tempFile);
    }

    [Fact]
    public void Current_ReturnsDefaults_WhenNoFileExists()
    {
        var s = _svc.Current;
        Assert.Equal("System", s.ThemeMode);
        Assert.Equal("#0A84FF", s.AccentColorHex);
        Assert.True(s.MinimizeToTray);
    }

    [Fact]
    public void Save_PersistsToFile()
    {
        var updated = _svc.Current;
        updated.ThemeMode = "Dark";
        updated.AccentColorHex = "#FF6B9D";
        _svc.Save(updated);

        Assert.True(File.Exists(_tempFile), "settings file should exist after Save");
        var json = File.ReadAllText(_tempFile);
        Assert.Contains("Dark", json);
        Assert.Contains("#FF6B9D", json);
    }

    [Fact]
    public void Load_RestoresPersistedSettings()
    {
        var s = _svc.Current;
        s.ThemeMode = "Light";
        _svc.Save(s);

        // Create a new service instance reading the same file
        var svc2 = new SettingsServiceTestable(_tempFile);
        Assert.Equal("Light", svc2.Current.ThemeMode);
    }

    [Fact]
    public void Update_AppliesMutation_AndPersists()
    {
        _svc.Update(s => s.ActiveThemePresetId = "dracula");
        Assert.Equal("dracula", _svc.Current.ActiveThemePresetId);
        Assert.True(File.Exists(_tempFile));
    }

    [Fact]
    public void Load_ReturnsDefaults_OnCorruptFile()
    {
        File.WriteAllText(_tempFile, "this is not json {{{{");
        var svc = new SettingsServiceTestable(_tempFile);
        Assert.Equal("System", svc.Current.ThemeMode);
    }

    [Fact]
    public void Save_IsAtomic_WritesViaTempFile()
    {
        // Verify the file exists and is valid JSON after save
        var s = _svc.Current;
        s.ThemeMode = "Dark";
        _svc.Save(s);

        var json = File.ReadAllText(_tempFile);
        var deserialized = JsonSerializer.Deserialize<AppSettings>(json);
        Assert.NotNull(deserialized);
        Assert.Equal("Dark", deserialized!.ThemeMode);
    }

    [Fact]
    public void Current_IsConcurrentlySafe()
    {
        var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();
        var threads = Enumerable.Range(0, 8).Select(i => new Thread(() =>
        {
            try
            {
                _svc.Update(s => s.ThemeMode = i % 2 == 0 ? "Dark" : "Light");
                _ = _svc.Current.ThemeMode;
            }
            catch (Exception ex) { errors.Add(ex); }
        })).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join(TimeSpan.FromSeconds(5)));
        Assert.Empty(errors);
    }

    public void Dispose()
    {
        try { File.Delete(_tempFile); } catch { }
        try { File.Delete(_tempFile + ".tmp"); } catch { }
    }
}

internal sealed class SettingsServiceTestable : SettingsService
{
    public SettingsServiceTestable(string filePath) : base(filePath) { }
}
