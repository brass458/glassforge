namespace GlassForge.Tests;

using GlassForge.Shell;

public class DwmProberTests
{
    // Returns S_OK (0) for any attribute, and false from SWCA probe
    private static int AlwaysOk(IntPtr hwnd, uint attr, IntPtr pv, uint cb) => 0;
    // Returns E_FAIL for any attribute
    private static int AlwaysFail(IntPtr hwnd, uint attr, IntPtr pv, uint cb) => unchecked((int)0x80004005);

    [Fact]
    public void Probe_AllCapabilitiesTrue_WhenAllDwmReturnSOk()
    {
        var caps = DwmProber.Probe(testDwmGet: AlwaysOk, testSwca: _ => true);

        Assert.True(caps.SupportsSystemBackdropType);
        Assert.True(caps.SupportsCaptionColor);
        Assert.True(caps.SupportsBorderColor);
        Assert.True(caps.SupportsImmersiveDarkMode);
        Assert.True(caps.SupportsWindowCompositionAttribute);
    }

    [Fact]
    public void Probe_AllCapabilitiesFalse_WhenAllDwmReturnFail()
    {
        var caps = DwmProber.Probe(testDwmGet: AlwaysFail, testSwca: _ => false);

        Assert.False(caps.SupportsSystemBackdropType);
        Assert.False(caps.SupportsCaptionColor);
        Assert.False(caps.SupportsBorderColor);
        Assert.False(caps.SupportsImmersiveDarkMode);
        Assert.False(caps.SupportsWindowCompositionAttribute);
    }

    [Fact]
    public void Probe_PartialCapabilities_WhenMixedResults()
    {
        // Only DWMWA_SYSTEMBACKDROP_TYPE (38) and DWMWA_CAPTION_COLOR (35) succeed
        int Mixed(IntPtr h, uint attr, IntPtr pv, uint cb)
            => (attr == 38 || attr == 35) ? 0 : unchecked((int)0x80004005);

        var caps = DwmProber.Probe(testDwmGet: Mixed, testSwca: _ => false);

        Assert.True(caps.SupportsSystemBackdropType);
        Assert.True(caps.SupportsCaptionColor);
        Assert.False(caps.SupportsBorderColor);
        Assert.False(caps.SupportsImmersiveDarkMode);
        Assert.False(caps.SupportsWindowCompositionAttribute);
    }

    [Fact]
    public void Probe_SwcaTrue_WhenSwcaProbeReturnsTrue()
    {
        var caps = DwmProber.Probe(testDwmGet: AlwaysFail, testSwca: _ => true);

        Assert.True(caps.SupportsWindowCompositionAttribute);
        Assert.False(caps.SupportsSystemBackdropType);
    }

    [Fact]
    public void Probe_SwcaFalse_WhenSwcaProbeReturnsFalse()
    {
        var caps = DwmProber.Probe(testDwmGet: AlwaysOk, testSwca: _ => false);

        Assert.False(caps.SupportsWindowCompositionAttribute);
        Assert.True(caps.SupportsSystemBackdropType);
    }
}
