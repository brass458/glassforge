namespace GlassForge.Tests;

using GlassForge.Core.Settings;
using GlassForge.Shell;
using GlassForge.Shell.Backends;

public class BackendEffectTests
{
    // Helper: captures (AccentState, GradientColor) from the testSwca delegate
    private static (NativeMethods.AccentState state, int gradient) ApplyAndCapture22H2(AppSettings settings)
    {
        NativeMethods.AccentState capturedState = default;
        int capturedGradient = -1;
        var backend = new ShellBackend_22H2(testSwca: (s, g) => { capturedState = s; capturedGradient = g; });
        backend.ApplyTaskbarEffect(new IntPtr(1), settings);
        return (capturedState, capturedGradient);
    }

    private static (NativeMethods.AccentState state, int gradient) ApplyAndCapture23H2(AppSettings settings)
    {
        NativeMethods.AccentState capturedState = default;
        int capturedGradient = -1;
        var backend = new ShellBackend_23H2(testSwca: (s, g) => { capturedState = s; capturedGradient = g; });
        backend.ApplyTaskbarEffect(new IntPtr(1), settings);
        return (capturedState, capturedGradient);
    }

    // ── 22H2 ──────────────────────────────────────────────────────────────────

    [Fact]
    public void ShellBackend_22H2_Acrylic_SendsAcrylicAccentState()
    {
        var (state, _) = ApplyAndCapture22H2(new AppSettings { TaskbarBackdropMode = "Acrylic" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, state);
    }

    [Fact]
    public void ShellBackend_22H2_None_SendsDisabledAccentState()
    {
        var (state, _) = ApplyAndCapture22H2(new AppSettings { TaskbarBackdropMode = "None" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_DISABLED, state);
    }

    [Fact]
    public void ShellBackend_22H2_HalfOpacity_SetsAlphaByte127()
    {
        var (_, gradient) = ApplyAndCapture22H2(new AppSettings { TaskbarBackdropMode = "Acrylic", TaskbarOpacity = 0.5f });
        int alpha = (gradient >> 24) & 0xFF;
        Assert.Equal(127, alpha);
    }

    // ── 23H2 ──────────────────────────────────────────────────────────────────

    [Fact]
    public void ShellBackend_23H2_Acrylic_SendsAcrylicAccentState()
    {
        var (state, _) = ApplyAndCapture23H2(new AppSettings { TaskbarBackdropMode = "Acrylic" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, state);
    }

    [Fact]
    public void ShellBackend_23H2_None_SendsDisabledAccentState()
    {
        var (state, _) = ApplyAndCapture23H2(new AppSettings { TaskbarBackdropMode = "None" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_DISABLED, state);
    }

    [Fact]
    public void ShellBackend_23H2_HalfOpacity_SetsAlphaByte127()
    {
        var (_, gradient) = ApplyAndCapture23H2(new AppSettings { TaskbarBackdropMode = "Acrylic", TaskbarOpacity = 0.5f });
        int alpha = (gradient >> 24) & 0xFF;
        Assert.Equal(127, alpha);
    }

    // ── 24H2 ──────────────────────────────────────────────────────────────────

    [Fact]
    public void ShellBackend_24H2_Acrylic_CallsDwmSetFirst()
    {
        int capturedDwmsbt = -1;
        var backend = new ShellBackend_24H2(
            testDwmSet: sbt => { capturedDwmsbt = sbt; return 0; },
            testSwca: null);
        backend.ApplyTaskbarEffect(new IntPtr(1), new AppSettings { TaskbarBackdropMode = "Acrylic" });
        Assert.Equal(NativeMethods.DWMSBT_TRANSIENTWINDOW, capturedDwmsbt);
    }

    [Fact]
    public void ShellBackend_24H2_None_SetsDwmsbtNone()
    {
        int capturedDwmsbt = -1;
        var backend = new ShellBackend_24H2(
            testDwmSet: sbt => { capturedDwmsbt = sbt; return 0; },
            testSwca: null);
        backend.ApplyTaskbarEffect(new IntPtr(1), new AppSettings { TaskbarBackdropMode = "None" });
        Assert.Equal(NativeMethods.DWMSBT_NONE, capturedDwmsbt);
    }

    [Fact]
    public void ShellBackend_24H2_Acrylic_FallsBackToSwca_WhenDwmSetFails()
    {
        NativeMethods.AccentState capturedState = default;
        var backend = new ShellBackend_24H2(
            testDwmSet: _ => unchecked((int)0x80004005),  // E_FAIL
            testSwca: (s, g) => { capturedState = s; });
        backend.ApplyTaskbarEffect(new IntPtr(1), new AppSettings { TaskbarBackdropMode = "Acrylic" });
        Assert.Equal(NativeMethods.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, capturedState);
    }

    // ── Future ────────────────────────────────────────────────────────────────

    [Fact]
    public void ShellBackend_Future_Acrylic_CallsDwmSetFirst()
    {
        int capturedDwmsbt = -1;
        var backend = new ShellBackend_Future(
            testDwmSet: sbt => { capturedDwmsbt = sbt; return 0; },
            testSwca: null);
        backend.ApplyTaskbarEffect(new IntPtr(1), new AppSettings { TaskbarBackdropMode = "Acrylic" });
        Assert.Equal(NativeMethods.DWMSBT_TRANSIENTWINDOW, capturedDwmsbt);
    }

    [Fact]
    public void ShellBackend_Future_None_SetsDwmsbtNone()
    {
        int capturedDwmsbt = -1;
        var backend = new ShellBackend_Future(
            testDwmSet: sbt => { capturedDwmsbt = sbt; return 0; },
            testSwca: null);
        backend.ApplyTaskbarEffect(new IntPtr(1), new AppSettings { TaskbarBackdropMode = "None" });
        Assert.Equal(NativeMethods.DWMSBT_NONE, capturedDwmsbt);
    }

    [Fact]
    public void ShellBackend_Future_HalfOpacity_Fallback_SetsAlphaByte127()
    {
        int capturedGradient = -1;
        var backend = new ShellBackend_Future(
            testDwmSet: _ => unchecked((int)0x80004005),  // E_FAIL — force SWCA fallback
            testSwca: (s, g) => { capturedGradient = g; });
        backend.ApplyTaskbarEffect(new IntPtr(1), new AppSettings { TaskbarBackdropMode = "Acrylic", TaskbarOpacity = 0.5f });
        int alpha = (capturedGradient >> 24) & 0xFF;
        Assert.Equal(127, alpha);
    }
}
