using ErpBridge.Erp.Mikro.Writers;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Writers;

/// <summary>
/// Pins the Mikro code values to the Fora Mikro enum ordinals. A wrong value posts a
/// document under another kind in Mikro (the dispatch writer once wrote a çıkış
/// faturası code, the collection writer a satış faturası code).
/// </summary>
public class MikroDocumentCodesTests
{
    [Fact]
    public void Cari_hareket_codes_match_fora_enums()
    {
        MikroCodes.ChaEvrakTip.AlisFaturasi.Should().Be(0);
        MikroCodes.ChaEvrakTip.TahsilatMakbuzu.Should().Be(1);
        MikroCodes.ChaEvrakTip.SatisFaturasi.Should().Be(63);
        MikroCodes.ChaEvrakTip.TediyeMakbuzu.Should().Be(64);

        MikroCodes.ChaCinsi.Nakit.Should().Be(0);
        MikroCodes.ChaCinsi.MusteriCeki.Should().Be(1);
        MikroCodes.ChaCinsi.MusteriSenedi.Should().Be(2);
        MikroCodes.ChaCinsi.ToptanFatura.Should().Be(6);
        MikroCodes.ChaCinsi.PerakendeFaturasi.Should().Be(7);
        MikroCodes.ChaCinsi.MusteriHavaleSozu.Should().Be(17);
        MikroCodes.ChaCinsi.MusteriKrediKarti.Should().Be(19);

        MikroCodes.HesapCinsi.Carimiz.Should().Be(0);
        MikroCodes.HesapCinsi.Bankamiz.Should().Be(2);
        MikroCodes.HesapCinsi.Kasamiz.Should().Be(4);
    }

    [Fact]
    public void Stok_hareket_and_odeme_emri_codes_match_fora_enums()
    {
        MikroCodes.SthEvrakTip.CikisIrsaliyesi.Should().Be(1);
        MikroCodes.SthEvrakTip.GirisFaturasi.Should().Be(3);
        MikroCodes.SthEvrakTip.CikisFaturasi.Should().Be(4);
        MikroCodes.SthTip.Giris.Should().Be(0);
        MikroCodes.SthTip.Cikis.Should().Be(1);

        MikroCodes.SckTip.MusteriCeki.Should().Be(0);
        MikroCodes.SckTip.MusteriSenedi.Should().Be(1);
        MikroCodes.SckTip.MusteriHavaleSozu.Should().Be(4);
        MikroCodes.SckTip.MusteriKrediKarti.Should().Be(6);
    }

    [Fact]
    public void Legacy_writer_aliases_use_the_corrected_codes()
    {
        MikroStockMovementCodes.DispatchEvrakTip.Should().Be(MikroCodes.SthEvrakTip.CikisIrsaliyesi);
        MikroStockMovementCodes.InvoiceEvrakTip.Should().Be(MikroCodes.SthEvrakTip.CikisFaturasi);
        MikroLedgerCodes.InvoiceEvrakTip.Should().Be(MikroCodes.ChaEvrakTip.SatisFaturasi);
        MikroLedgerCodes.InvoiceCinsi.Should().Be(MikroCodes.ChaCinsi.ToptanFatura);
        MikroCollectionWriter.CollectionEvrakTip.Should().Be(MikroCodes.ChaEvrakTip.TahsilatMakbuzu);
    }
}
