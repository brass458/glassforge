namespace GlassForge.Tests;

using GlassForge.Core.Settings;
using GlassForge.Shell;
using GlassForge.Shell.Backends;

public class BackendEffectTests
{
    private static (NativeMethods.AccentState state, int gradient) Capture(
        Action<NativeMethods.AccentState, int>? testSwca,
        AppSettings settings,
        System.Func<Action<NativeMethods.AccentState, int>?, object> makeBackend)
        => throw new InvalidOperationException("use typed helpers");

    private static (NativeMethods.AccentState state, int gradient) Apply22H2(AppSettings settings)
    {
        NativeMethods.AccentState st = default; int g = -1;
        new ShellBackend_22H2(testSwca: (s, v) => { st = s; g = v; }).ApplyTaskbarEffect(new IntPtr(1), settings);
        return (st, g);
    }

    private static (NativeMethods.AccentState state, int gradient) Apply23H2(AppSettings settings)
    {
        NativeMethods.AccentState st = default; int g = -1;
        new ShellBackend_23H2(testSwca: (s, v) => { st = s; g = v; }).ApplyTaskbarEffect(new IntPtr(1), settings);
        return (st, g);
    }

    private static (NativeMethods.AccentState state, int gradient) Apply24H2(AppSettings settings)
    {
        NativeMethods.AccentState st = default; int g = -1;
        new ShellBackend_24H2(testSwca: (s, v) => { st = s; g = v; }).ApplyTaskbarEffect(new IntPtr(1), settings);
        return (st, g);
    }

    private static (NativeMethods.AccentState state, int gradient) ApplyFuture(AppSettings settings)
    {
        NativeMethods.AccentState st = default; int g = -1;
        new ShellBackend_Future(testSwca: (s, v) => { st = s; g = v; }).ApplyTaskbarEffect(new IntPtr(1), settings);
        return (st, g);
    }

    // ── 22H2 ──────────────────────────────────────────────────────────────────

    [Fact]
    public void ShellBackend_22H2_Acrylic_SendsAcrylicAccentState()
    {
        var (state, _) = Apply22H2(new AppSettings { TaskbarBackdropMode = "Acrylic" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, state);
    }

    [Fact]
    public void ShellBackend_22H2_None_SendsDisabledAccentState()
    {
        var (state, _) = Apply22H2(new AppSettings { TaskbarBackdropMode = "None" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_DISABLED, state);
    }

    [Fact]
    public void ShellBackend_22H2_HalfOpacity_SetsAlphaByte127()
    {
        var (_, gradient) = Apply22H2(new AppSettings { TaskbarBackdropMode = "Acrylic", TaskbarOpacity = 0.5f });
        Assert.Equal(127, (gradient >> 24) & 0xFF);
    }

    // ── 23H2 ──────────────────────────────────────────────────────────────────

    [Fact]
    public void ShellBackend_23H2_Acrylic_SendsAcrylicAccentState()
    {
        var (state, _) = Apply23H2(new AppSettings { TaskbarBackdropMode = "Acrylic" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, state);
    }

    [Fact]
    public void ShellBackend_23H2_None_SendsDisabledAccentState()
    {
        var (state, _) = Apply23H2(new AppSettings { TaskbarBackdropMode = "None" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_DISABLED, state);
    }

    [Fact]
    public void ShellBackend_23H2_HalfOpacity_SetsAlphaByte127()
    {
        var (_, gradient) = Apply23H2(new AppSettings { TaskbarBackdropMode = "Acrylic", TaskbarOpacity = 0.5f });
        Assert.Equal(127, (gradient >> 24) & 0xFF);
    }

    // ── 24H2 ──────────────────────────────────────────────────────────────────

    [Fact]
    public void ShellBackend_24H2_Acrylic_SendsAcrylicAccentState()
    {
        var (state, _) = Apply24H2(new AppSettings { TaskbarBackdropMode = "Acrylic" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, state);
    }

    [Fact]
    public void ShellBackend_24H2_None_SendsDisabledAccentState()
    {
        var (state, _) = Apply24H2(new AppSettings { TaskbarBackdropMode = "None" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_DISABLED, state);
    }

    [Fact]
    public void ShellBackend_24H2_HalfOpacity_SetsAlphaByte127()
    {
        var (_, gradient) = Apply24H2(new AppSettings { TaskbarBackdropMode = "Acrylic", TaskbarOpacity = 0.5f });
        Assert.Equal(127, (gradient >> 24) & 0xFF);
    }

    // ── Future ────────────────────────────────────────────────────────────────

    [Fact]
    public void ShellBackend_Future_Acrylic_SendsAcrylicAccentState()
    {
        var (state, _) = ApplyFuture(new AppSettings { TaskbarBackdropMode = "Acrylic" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, state);
    }

    [Fact]
    public void ShellBackend_Future_None_SendsDisabledAccentState()
    {
        var (state, _) = ApplyFuture(new AppSettings { TaskbarBackdropMode = "None" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_DISABLED, state);
    }

    [Fact]
    public void ShellBackend_Future_HalfOpacity_SetsAlphaByte127()
    {
        var (_, gradient) = ApplyFuture(new AppSettings { TaskbarBackdropMode = "Acrylic", TaskbarOpacity = 0.5f });
        Assert.Equal(127, (gradient >> 24) & 0xFF);
    }
}
