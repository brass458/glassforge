namespace GlassForge.Core.Settings;

using System.Text.Json;

public class SettingsService
{
    private readonly string _directory;
    private string ConfigPath => Path.Combine(_directory, "config.json");
    private string TempPath => Path.Combine(_directory, "config.tmp");

    public SettingsService(string? directory = null)
    {
        _directory = directory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GlassForge");
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(ConfigPath)) return new AppSettings();
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppSettings>(json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(_directory);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(TempPath, json);
        if (File.Exists(ConfigPath))
            File.Replace(TempPath, ConfigPath, null);
        else
            File.Move(TempPath, ConfigPath);
    }
}
