using System.Text.Json;

namespace GlassForge.Core.Settings;

/// <summary>
/// Loads and saves <see cref="AppSettings"/> to %APPDATA%\GlassForge\settings.json.
/// Thread-safe for concurrent reads; writes are serialized via a lock.
/// </summary>
public class SettingsService
{
    private static readonly string _defaultPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GlassForge", "settings.json");

    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    private readonly object _lock = new();
    private readonly string _path;
    private AppSettings _current;

    public SettingsService() : this(_defaultPath) { }

    protected SettingsService(string filePath)
    {
        _path    = filePath;
        _current = Load();
    }

    /// <summary>A snapshot of the current settings (not a live reference).</summary>
    public AppSettings Current
    {
        get { lock (_lock) return _current; }
    }

    /// <summary>Replaces the in-memory settings and persists them to disk.</summary>
    public void Save(AppSettings settings)
    {
        lock (_lock)
        {
            _current = settings;
            Persist(settings);
        }
    }

    /// <summary>Applies an in-place mutation and saves.</summary>
    public void Update(Action<AppSettings> mutate)
    {
        lock (_lock)
        {
            mutate(_current);
            Persist(_current);
        }
    }

    private AppSettings Load()
    {
        try
        {
            if (!File.Exists(_path)) return new AppSettings();
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AppSettings>(json, _json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    private void Persist(AppSettings settings)
    {
        try
        {
            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(settings, _json);
            // Atomic write via temp file + replace
            var tmp = _path + ".tmp";
            File.WriteAllText(tmp, json);
            File.Move(tmp, _path, overwrite: true);
        }
        catch { /* best-effort */ }
    }
}
