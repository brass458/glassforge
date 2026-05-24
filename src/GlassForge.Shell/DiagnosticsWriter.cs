namespace GlassForge.Shell;

using System.Text.Json;
using GlassForge.Shell.Abstractions;

public static class DiagnosticsWriter
{
    public static void Write(int buildNumber, int ubr, string backendName,
        ShellCapabilities capabilities, string? outputPath = null)
    {
        var path = outputPath ?? DefaultPath();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            var data = new
            {
                timestamp = DateTime.UtcNow,
                buildNumber,
                ubr,
                backendName,
                capabilities = new
                {
                    supportsSystemBackdropType = capabilities.SupportsSystemBackdropType,
                    supportsCaptionColor = capabilities.SupportsCaptionColor,
                    supportsBorderColor = capabilities.SupportsBorderColor,
                    supportsImmersiveDarkMode = capabilities.SupportsImmersiveDarkMode,
                    supportsWindowCompositionAttribute = capabilities.SupportsWindowCompositionAttribute
                },
                workingSetBytes = Environment.WorkingSet
            };

            File.WriteAllText(path,
                JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static string DefaultPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GlassForge", "diagnostics.json");
}
