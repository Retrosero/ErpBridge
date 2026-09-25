using ErpBridge.CentralApi.Endpoints;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>Codex #195: audit fields are cut by Unicode scalars, the way PostgreSQL varchar(n) counts characters.</summary>
public sealed class PortalNativeAuditFitTests
{
    [Fact]
    public void A_short_value_is_kept_as_is() =>
        PortalNativeWriteHelpers.Fit("İptal: müşteri vazgeçti", 500).Should().Be("İptal: müşteri vazgeçti");

    [Fact]
    public void Emoji_count_as_one_character_each_and_are_never_split()
    {
        var reason = string.Concat(Enumerable.Repeat("😀", 450));

        PortalNativeWriteHelpers.Fit(reason, 500).Should().Be(reason, "450 emoji are 450 characters, within varchar(500)");

        var cut = PortalNativeWriteHelpers.Fit(string.Concat(Enumerable.Repeat("😀", 600)), 500);
        cut.EnumerateRunes().Count().Should().Be(500);
        cut.Should().EndWith("…");
        cut.Where(char.IsSurrogate).Count().Should().Be(2 * 499, "every emoji kept whole: no unpaired surrogate");
    }
}
