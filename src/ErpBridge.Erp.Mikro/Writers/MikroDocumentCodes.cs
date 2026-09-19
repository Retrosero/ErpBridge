namespace ErpBridge.Erp.Mikro.Writers;

/// <summary>
/// Mikro V15 numeric code taxonomies for the document tables. Values are the
/// ordinals of the Fora Mikro enums (<c>enum_cha_evrak_tip</c>, <c>enum_cha_cinsi</c>,
/// <c>enum_sth_evraktip</c>, …) and were checked against rows Mikro itself wrote in
/// <c>MikroDB_V15_02</c>. Source of truth: <c>docs/mikro-yazim-referansi.md</c>.
/// </summary>
public static class MikroCodes
{
    /// <summary><c>*_fileid</c> — Mikro's table file numbers.</summary>
    public static class FileId
    {
        public const short CariHesapHareketleri = 51;
        public const short StokHareketleri = 16;
        public const short OdemeEmirleri = 54;
        public const short EvrakAciklamalari = 66;
    }

    /// <summary><c>cha_evrak_tip</c> (<c>enum_cha_evrak_tip</c>).</summary>
    public static class ChaEvrakTip
    {
        /// <summary>Alış faturası; with <c>cha_normal_Iade=1</c> a satıştan iade faturası.</summary>
        public const byte AlisFaturasi = 0;
        public const byte TahsilatMakbuzu = 1;
        /// <summary>Satış faturası; with <c>cha_normal_Iade=1</c> an alıştan iade faturası.</summary>
        public const byte SatisFaturasi = 63;
        public const byte TediyeMakbuzu = 64;
    }

    /// <summary><c>cha_tip</c> — accounting direction.</summary>
    public static class ChaTip
    {
        public const byte Borc = 0;
        public const byte Alacak = 1;
    }

    /// <summary><c>cha_cinsi</c> (<c>enum_cha_cinsi</c>).</summary>
    public static class ChaCinsi
    {
        public const byte Nakit = 0;
        public const byte MusteriCeki = 1;
        public const byte MusteriSenedi = 2;
        public const byte ToptanFatura = 6;
        public const byte PerakendeFaturasi = 7;
        public const byte MusteriHavaleSozu = 17;
        public const byte MusteriKrediKarti = 19;

        // Tediyenin "firma" ailesi (referans §11): para çıkarken müşteri değil firma tarafı yazılır.
        public const byte FirmaCeki = 3;
        public const byte FirmaSenedi = 4;
        public const byte FirmaHavaleEmri = 20;
        public const byte FirmaKrediKarti = 22;
    }

    /// <summary><c>cha_normal_Iade</c> / <c>sth_normal_iade</c>.</summary>
    public static class NormalIade
    {
        public const byte Normal = 0;
        public const byte Iade = 1;
    }

    /// <summary>
    /// <c>cha_cari_cins</c> and <c>cha_kasa_hizmet</c> share one taxonomy
    /// (<c>enum_cha_cari_cins</c>). A closed invoice posts to <see cref="Kasamiz"/>
    /// or <see cref="Bankamiz"/>; a receipt line names its cash/bank in <c>cha_kasa_hizmet</c>.
    /// </summary>
    public static class HesapCinsi
    {
        public const byte Carimiz = 0;
        public const byte Bankamiz = 2;
        public const byte Kasamiz = 4;
    }

    /// <summary><c>cha_tpoz</c> — open (açık hesap) or closed (kasadan/bankadan kapalı).</summary>
    public static class ChaTpoz
    {
        public const byte Acik = 0;
        public const byte Kapali = 1;
    }

    /// <summary><c>cha_sntck_poz</c> / <c>sck_sonpoz</c> (<c>enum_cha_sntck_poz</c>).</summary>
    public static class EvrakPozisyonu
    {
        public const byte Portfoyde = 0;
        public const byte Tahsilde = 2;
    }

    /// <summary>
    /// <c>cha_karsidgrupno</c> / <c>sck_nerede_cari_grupno</c> — the bank account group a
    /// receipt line lands in (canlı veri: kredi kartı 7, havale 9).
    /// </summary>
    public static class BankaGrupNo
    {
        public const byte KrediKarti = 7;
        public const byte Havale = 9;
    }

    /// <summary><c>sth_evraktip</c> (<c>enum_sth_evraktip</c>).</summary>
    public static class SthEvrakTip
    {
        public const byte CikisIrsaliyesi = 1;
        public const byte GirisFaturasi = 3;
        public const byte CikisFaturasi = 4;
    }

    /// <summary><c>sth_tip</c> (<c>enum_sth_tip</c>).</summary>
    public static class SthTip
    {
        public const byte Giris = 0;
        public const byte Cikis = 1;
    }

    /// <summary><c>sth_cins</c> (<c>enum_sth_cins</c>).</summary>
    public static class SthCins
    {
        public const byte Toptan = 0;
        public const byte Perakende = 1;
    }

    /// <summary><c>sck_tip</c> (<c>enum_sck_tip</c>).</summary>
    public static class SckTip
    {
        public const byte MusteriCeki = 0;
        public const byte MusteriSenedi = 1;
        public const byte MusteriHavaleSozu = 4;
        public const byte MusteriKrediKarti = 6;
    }
}

/// <summary>
/// Codes the legacy dispatch-note and invoice writers bind for <c>STOK_HAREKETLERI</c>.
/// Kept as aliases of <see cref="MikroCodes"/> so both writers post the same kind.
/// </summary>
public static class MikroStockMovementCodes
{
    /// <summary><c>sth_tip</c> — shipping goods is an out-movement.</summary>
    public const byte OutboundTip = MikroCodes.SthTip.Cikis;

    /// <summary><c>sth_cins</c> — toptan.</summary>
    public const byte NormalCins = MikroCodes.SthCins.Toptan;

    /// <summary><c>sth_normal_iade</c> — normal.</summary>
    public const byte NormalMovement = MikroCodes.NormalIade.Normal;

    /// <summary><c>sth_evraktip</c> of a satış irsaliyesi (was 4 — that is a çıkış faturası).</summary>
    public const byte DispatchEvrakTip = MikroCodes.SthEvrakTip.CikisIrsaliyesi;

    /// <summary><c>sth_evraktip</c> of a satış faturası line (was 63 — not a stock document kind).</summary>
    public const byte InvoiceEvrakTip = MikroCodes.SthEvrakTip.CikisFaturasi;

    /// <summary>
    /// <c>sth_birim_pntr</c> — selects which of the stock card's units the
    /// quantity is expressed in. <c>1</c> is birim1, the card's primary unit.
    /// </summary>
    public const byte DefaultUnitPointer = 1;

    /// <summary><c>sth_vergi_pntr</c> — KDV rate slot; the real value comes from the stock card.</summary>
    public const byte DefaultTaxPointer = 1;
}

/// <summary>
/// Codes for <c>CARI_HESAP_HAREKETLERI</c> rows produced by the legacy invoice writer.
/// </summary>
public static class MikroLedgerCodes
{
    /// <summary>
    /// <c>cha_tip</c> — the accounting <b>direction</b>. A sales invoice debits the customer.
    ///
    /// <para>
    /// This must stay consistent with the bootstrap reader's balance query
    /// (<c>SUM(CASE WHEN cha_tip = 0 THEN cha_meblag ELSE -cha_meblag END)</c>)
    /// — an inverted value silently flips every customer's balance.
    /// </para>
    /// </summary>
    public const byte InvoiceDebitTip = MikroCodes.ChaTip.Borc;

    /// <summary><c>cha_evrak_tip</c> — satış faturası.</summary>
    public const byte InvoiceEvrakTip = MikroCodes.ChaEvrakTip.SatisFaturasi;

    /// <summary><c>cha_cinsi</c> of a satış faturası header — toptan fatura (0 would be nakit).</summary>
    public const byte InvoiceCinsi = MikroCodes.ChaCinsi.ToptanFatura;

    /// <summary><c>cha_normal_Iade</c> — normal.</summary>
    public const byte NormalMovement = MikroCodes.NormalIade.Normal;
}
