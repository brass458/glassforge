namespace GlassForge.Shell;

using GlassForge.Shell.Abstractions;

public class CapabilityMap
{
    private readonly object _lock = new();
    private ShellCapabilities _current = new();
    private string _backendName = "Unknown";

    public ShellCapabilities Current
    {
        get { lock (_lock) return _current; }
    }

    public string BackendName
    {
        get { lock (_lock) return _backendName; }
    }

    public void Probe(IShellBackend backend)
    {
        var capabilities = backend.ProbeCapabilities();
        lock (_lock)
        {
            _current = capabilities;
            _backendName = backend.Name;
        }
    }
}
