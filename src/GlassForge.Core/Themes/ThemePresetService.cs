namespace GlassForge.Core.Themes;

using System.Text.Json;
using GlassForge.Core.Models;

/// <summary>
/// Manages built-in and user-saved theme presets.
/// User presets are persisted to %APPDATA%\GlassForge\custom-themes.json
/// (or an injected directory for testing).
/// </summary>
public class ThemePresetService
{
    private static readonly JsonSerializerOptions _jsonOpts = new() { WriteIndented = true };

    private readonly string _customPresetsPath;
    private readonly List<ThemePreset> _userPresets = new();

    public ThemePresetService() : this(DefaultDirectory()) { }

    public ThemePresetService(string directory)
    {
        _customPresetsPath = Path.Combine(directory, "custom-themes.json");
        LoadUserPresets();
    }

    private static string DefaultDirectory() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GlassForge");

    /// <summary>All presets: built-ins first, then user-saved.</summary>
    public IReadOnlyList<ThemePreset> AllPresets =>
        [.. BuiltInThemePresets.All, .. _userPresets];

    /// <summary>Returns only user-saved presets.</summary>
    public IReadOnlyList<ThemePreset> GetCustomPresets() => _userPresets.AsReadOnly();

    /// <summary>
    /// Saves a custom preset. If a preset with the same Name already exists, it is replaced.
    /// </summary>
    public void SaveCustomPreset(ThemePreset preset)
    {
        preset.IsBuiltIn = false;
        if (string.IsNullOrEmpty(preset.Id))
            preset.Id = Guid.NewGuid().ToString();

        var existing = _userPresets.FindIndex(p => p.Name == preset.Name);
        if (existing >= 0)
            _userPresets[existing] = preset;
        else
            _userPresets.Add(preset);

        PersistUserPresets();
    }

    /// <summary>Deletes a user preset by name. No-op if not found.</summary>
    public void DeleteCustomPreset(string name)
    {
        var idx = _userPresets.FindIndex(p => p.Name == name);
        if (idx < 0) return;
        _userPresets.RemoveAt(idx);
        PersistUserPresets();
    }

    private void LoadUserPresets()
    {
        try
        {
            if (!File.Exists(_customPresetsPath)) return;
            var json = File.ReadAllText(_customPresetsPath);
            var list = JsonSerializer.Deserialize<List<ThemePreset>>(json, _jsonOpts);
            if (list is null) return;
            foreach (var p in list) p.IsBuiltIn = false;
            _userPresets.AddRange(list);
        }
        catch { /* ignore corrupt file */ }
    }

    private void PersistUserPresets()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_customPresetsPath)!);
            File.WriteAllText(_customPresetsPath, JsonSerializer.Serialize(_userPresets, _jsonOpts));
        }
        catch { /* ignore write failures */ }
    }
}
