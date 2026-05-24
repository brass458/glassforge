using GlassForge.Shell.Abstractions;
using GlassForge.Shell.Backends;

namespace GlassForge.Shell;

/// <summary>
/// Selects the most appropriate <see cref="IShellBackend"/> for the current Windows build.
/// Falls back to <see cref="ShellBackend_Future"/> for unrecognized builds so the app
/// remains functional rather than crashing.
/// </summary>
public static class ShellBackendFactory
{
    private static readonly IShellBackend[] _backends =
    [
        new ShellBackend_26100(),
        new ShellBackend_22621(),
        new ShellBackend_22000(),
        // Future is the catch-all — must be last
        new ShellBackend_Future(),
    ];

    /// <summary>
    /// Returns the backend whose [MinBuild, MaxBuild] range contains <paramref name="buildNumber"/>.
    /// Always returns a non-null backend (worst case: ShellBackend_Future).
    /// </summary>
    public static IShellBackend Create(int buildNumber)
    {
        foreach (var backend in _backends)
        {
            if (buildNumber >= backend.MinBuild && buildNumber <= backend.MaxBuild)
                return backend;
        }
        // Unreachable in practice because ShellBackend_Future covers int.MaxValue,
        // but be defensive.
        return _backends[^1];
    }

    /// <summary>
    /// Convenience overload that reads the build number from <see cref="WindowsBuildDetector"/>.
    /// </summary>
    public static IShellBackend CreateForCurrentBuild()
        => Create(WindowsBuildDetector.GetBuildNumber());
}
