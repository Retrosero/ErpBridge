namespace ErpBridge.Erp.Mikro.Writers.Session;

/// <summary>
/// One place a document number is taken: a table's rows whose key columns equal the given values.
/// </summary>
/// <param name="Table">The table.</param>
/// <param name="SeriesColumn">Its <c>*_evrakno_seri</c> column.</param>
/// <param name="NumberColumn">Its <c>*_evrakno_sira</c> column.</param>
/// <param name="Keys">The unique index's leading columns that tell this document kind apart, with their values.</param>
public sealed record MikroNumberSource(MikroTable Table, string SeriesColumn, string NumberColumn, IReadOnlyList<(string Column, int Value)> Keys);

/// <summary>
/// Every table a document's number appears in. The next number is the highest one used in any
/// of them plus one, so the number is free for the header, the lines and the description row
/// alike — the equivalent of Fora's <c>YeniSeriNoBul</c> followed by <c>EvrakVarMi</c>, which
/// look at a single table only.
/// </summary>
public sealed record MikroNumberScope(string Name, IReadOnlyList<MikroNumberSource> Sources);

/// <summary>
/// Number scopes of the documents the agent writes, keyed like Mikro's unique indexes
/// (reference §1 "Seri/sıra", "Tekil indeksler"). A satış iadesi is an alış faturası with the iade
/// flag, so it shares the alış invoices' sequence.
/// </summary>
public static class MikroDocumentNumbering
{
    private static MikroNumberSource Cha(int evrakTip) =>
        new(MikroTables.CariHareket, "cha_evrakno_seri", "cha_evrakno_sira", [("cha_evrak_tip", evrakTip)]);

    private static MikroNumberSource Sth(int evrakTip) =>
        new(MikroTables.StokHareket, "sth_evrakno_seri", "sth_evrakno_sira", [("sth_evraktip", evrakTip)]);

    private static MikroNumberSource Description(int dosyaNo, int hareketTip, int evrakTip) =>
        new(MikroTables.EvrakAciklama, "egk_evr_seri", "egk_evr_sira", [("egk_dosyano", dosyaNo), ("egk_hareket_tip", hareketTip), ("egk_evr_tip", evrakTip)]);

    /// <summary>Satış faturası: CHA 63, STH 4, açıklama (51, 0, 63).</summary>
    public static readonly MikroNumberScope SalesInvoice = new("satış faturası",
    [
        Cha(MikroCodes.ChaEvrakTip.SatisFaturasi),
        Sth(MikroCodes.SthEvrakTip.CikisFaturasi),
        Description(MikroTables.CariHareket.FileId, 0, MikroCodes.ChaEvrakTip.SatisFaturasi),
    ]);

    /// <summary>Satış iadesi (alış faturası + iade): CHA 0, STH 3, açıklama (51, 1, 0).</summary>
    public static readonly MikroNumberScope SalesReturnInvoice = new("satış iadesi faturası",
    [
        Cha(MikroCodes.ChaEvrakTip.AlisFaturasi),
        Sth(MikroCodes.SthEvrakTip.GirisFaturasi),
        Description(MikroTables.CariHareket.FileId, 1, MikroCodes.ChaEvrakTip.AlisFaturasi),
    ]);

    /// <summary>Tahsilat makbuzu: CHA 1, açıklama (51, 1, 1).</summary>
    public static readonly MikroNumberScope CollectionReceipt = new("tahsilat makbuzu",
    [
        Cha(MikroCodes.ChaEvrakTip.TahsilatMakbuzu),
        Description(MikroTables.CariHareket.FileId, 1, MikroCodes.ChaEvrakTip.TahsilatMakbuzu),
    ]);

    /// <summary>Tediye makbuzu: CHA 64, açıklama (51, 0, 64) — tahsilatın aynası, açıklama hareket tipi ters.</summary>
    public static readonly MikroNumberScope DisbursementReceipt = new("tediye makbuzu",
    [
        Cha(MikroCodes.ChaEvrakTip.TediyeMakbuzu),
        Description(MikroTables.CariHareket.FileId, 0, MikroCodes.ChaEvrakTip.TediyeMakbuzu),
    ]);

    /// <summary>Satış irsaliyesi: STH 1, açıklama (16, 1, 1).</summary>
    public static readonly MikroNumberScope SalesDispatch = new("satış irsaliyesi",
    [
        Sth(MikroCodes.SthEvrakTip.CikisIrsaliyesi),
        Description(MikroTables.StokHareket.FileId, 1, MikroCodes.SthEvrakTip.CikisIrsaliyesi),
    ]);

    /// <summary>Alınan sipariş: SIPARISLER tip 0, cins 0.</summary>
    public static readonly MikroNumberScope SalesOrder = new("satış siparişi",
    [
        new(MikroTables.Siparis, "sip_evrakno_seri", "sip_evrakno_sira", [("sip_tip", 0), ("sip_cins", 0)]),
    ]);
}
