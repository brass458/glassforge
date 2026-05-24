namespace GlassForge.Shell;

using GlassForge.Shell.Abstractions;
using GlassForge.Shell.Backends;

public static class ShellBackendFactory
{
    public static IShellBackend Create(int buildNumber) => buildNumber switch
    {
        >= 22621 and <= 22630 => new ShellBackend_22H2(),
        >= 22631 and <= 26099 => new ShellBackend_23H2(),
        >= 26100 and <= 26199 => new ShellBackend_24H2(),
        _ => new ShellBackend_Future()
    };
}
