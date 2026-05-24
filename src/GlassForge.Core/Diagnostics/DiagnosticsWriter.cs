using System.Text;
using System.Text.Json;

namespace GlassForge.Core.Diagnostics;

/// <summary>
/// Writes structured diagnostic snapshots to %APPDATA%\GlassForge\diagnostics\.
/// Called on first run, after each UBR change, and on demand from the settings UI.
/// Files are named by UTC timestamp so history is preserved across Windows updates.
/// </summary>
public class DiagnosticsWriter
{
    private static readonly string _defaultDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GlassForge", "diagnostics");

    private readonly string _dir;

    public DiagnosticsWriter() : this(_defaultDir) { }

    protected DiagnosticsWriter(string dir)
    {
        _dir = dir;
    }

    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    /// <summary>
    /// Writes a diagnostic snapshot file and returns its full path.
    /// Any write failure is swallowed — diagnostics must never crash the app.
    /// </summary>
    public string Write(DiagnosticSnapshot snapshot)
    {
        var path = "";
        try
        {
            Directory.CreateDirectory(_dir);
            var stamp  = snapshot.CapturedAt.ToString("yyyy-MM-ddTHH-mm-ss-fffZ");
            var unique = Guid.NewGuid().ToString("N")[..6];
            path = Path.Combine(_dir, $"diag-{stamp}-{unique}.json");
            var json = JsonSerializer.Serialize(snapshot, _json);
            File.WriteAllText(path, json, Encoding.UTF8);
        }
        catch { /* never crash — diagnostics are best-effort */ }
        return path;
    }

    /// <summary>Returns paths of all existing diagnostic files, newest first.</summary>
    public IReadOnlyList<string> ListFiles()
    {
        try
        {
            if (!Directory.Exists(_dir)) return [];
            return Directory.GetFiles(_dir, "diag-*.json", SearchOption.TopDirectoryOnly)
                            .OrderByDescending(f => f)
                            .ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>Deletes diagnostic files older than <paramref name="retainCount"/>.</summary>
    public void Prune(int retainCount = 10)
    {
        try
        {
            var files = ListFiles();
            foreach (var file in files.Skip(retainCount))
                File.Delete(file);
        }
        catch { }
    }
}
