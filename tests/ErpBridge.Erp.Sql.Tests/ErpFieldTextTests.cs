using ErpBridge.Erp.Sql;
using FluentAssertions;

namespace ErpBridge.Erp.Sql.Tests;

/// <summary>
/// The identifier/free-text split is a correctness rule, not a style choice:
/// truncating a <c>cari_kod</c> posts the document against a different customer,
/// while truncating an açıklama only loses words. These tests pin both sides.
/// </summary>
public class ErpFieldTextTests
{
    [Fact]
    public void Identifier_that_fits_is_returned_trimmed()
    {
        ErpFieldText.Identifier("  120.01.0042  ", 25, "CARI_HESAPLAR.cari_kod")
            .Should().Be("120.01.0042");
    }

    /// <summary>
    /// The failure that motivated this: a shortened code can match a *different*
    /// existing account, so the document would post to the wrong customer.
    /// </summary>
    [Fact]
    public void Identifier_that_overflows_throws_instead_of_truncating()
    {
        var act = () => ErpFieldText.Identifier(new string('X', 30), 25, "CARI_HESAPLAR.cari_kod");

        act.Should().Throw<ErpFieldTooLongException>()
           .Which.Should().Match<ErpFieldTooLongException>(e =>
               e.Field == "CARI_HESAPLAR.cari_kod" && e.ActualLength == 30 && e.MaxLength == 25);
    }

    /// <summary>
    /// Mikro's evrak series column holds six characters, so a plausible-looking
    /// receipt series like "THS-2026" already overflows. This is the case that
    /// makes the rule bite in ordinary use.
    /// </summary>
    [Fact]
    public void Realistic_evrak_series_overflow_is_caught()
    {
        var act = () => ErpFieldText.Identifier("THS-2026", 6, "SIPARISLER.sip_evrakno_seri");

        act.Should().Throw<ErpFieldTooLongException>();
    }

    [Fact]
    public void Identifier_with_unknown_width_is_not_checked()
    {
        // A column the provider could not resolve must not block the write; the
        // schema contract test is what catches a genuinely missing column.
        ErpFieldText.Identifier(new string('X', 500), null, "X.y").Should().HaveLength(500);
    }

    [Fact]
    public void Null_identifier_becomes_empty()
    {
        ErpFieldText.Identifier(null, 25, "X.y").Should().BeEmpty();
    }

    [Fact]
    public void Free_text_is_truncated_rather_than_rejected()
    {
        // Losing the tail of a description beats failing the whole document.
        ErpFieldText.FreeText(new string('a', 100), 40).Should().HaveLength(40);
    }

    [Fact]
    public void Free_text_that_fits_is_returned_trimmed()
    {
        ErpFieldText.FreeText("  Temmuz tahsilatı  ", 40).Should().Be("Temmuz tahsilatı");
    }

    [Fact]
    public void Free_text_handles_null_and_unknown_width()
    {
        ErpFieldText.FreeText(null, 40).Should().BeEmpty();
        ErpFieldText.FreeText("abc", null).Should().Be("abc");
    }

    [Fact]
    public void Exception_message_explains_why_it_refused()
    {
        var ex = new ErpFieldTooLongException("SIPARISLER.sip_musteri_kod", 30, 25);

        ex.Message.Should().Contain("30").And.Contain("25")
          .And.Contain("different record");
    }
}
