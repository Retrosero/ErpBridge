using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Mikro.Versioning;
using FluentAssertions;

namespace ErpBridge.Erp.Mikro.Tests.Versioning;

public class MikroVersionDetectorTests
{
    [Theory]
    [InlineData("15.0.2000.0", MikroVersion.V15)]
    [InlineData("15.5.1234.5", MikroVersion.V15)]
    [InlineData("16.0.1.7", MikroVersion.V16)]
    [InlineData("16.10.50.4", MikroVersion.V16)]
    [InlineData("10.0.0.0", MikroVersion.Unknown)]
    [InlineData("not.a.version", MikroVersion.Unknown)]
    [InlineData("", MikroVersion.Unknown)]
    public void ParseVersionString_maps_major_numbers(string raw, MikroVersion expected)
    {
        var actual = MikroVersionDetector.ParseVersionString(raw);

        actual.Should().Be(expected);
    }

    [Fact]
    public void ParseVersionString_handles_null_as_unknown()
    {
        MikroVersionDetector.ParseVersionString(null).Should().Be(MikroVersion.Unknown);
    }

    [Fact]
    public void ParseVersionString_handles_missing_dot()
    {
        // Version string without any dot — we treat as Unknown.
        MikroVersionDetector.ParseVersionString("16").Should().Be(MikroVersion.Unknown);
    }
}
