using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Writers.Documents;

/// <summary>
/// Writes a phone purchase as a Mikro V15 alış faturası (ERP yazım 3 Y3a, reference §10 and §15):
/// a <c>CARI_HESAP_HAREKETLERI</c> header (<c>cha_evrak_tip=0</c>, credit, <c>cha_cinsi=6</c>) and one
/// incoming <c>STOK_HAREKETLERI</c> row per line (<c>sth_evraktip=3</c>, <c>sth_tip=0</c>).
///
/// <para>The invoice is written <b>open</b>. Its payment is a separate tediye document (K5): on the phone
/// a field purchase produces both, and Mikro shows them as what they are — goods in, money out. Closing
/// the two into one document would hide the payment the phone actually recorded.</para>
///
/// <para>Two numbering facts, both from §15. The document shares its number space with the satış iadesi,
/// which Mikro also books as <c>cha_evrak_tip=0</c>, so MAX+1 must not filter on the return flag. And the
/// series is the <b>supplier's own</b>: Mikro has no series definition table, so when the phone names no
/// series the writer continues the one that supplier's invoices already use (K7).</para>
/// </summary>
public sealed class MikroPurchaseInvoiceWriter(MikroDocumentWriteRunner runner)
{
    /// <summary>Job document type the ledger records a phone purchase under.</summary>
    public const string DocumentType = "purchase_receipt";

    private readonly MikroDocumentWriteRunner _runner = runner ?? throw new ArgumentNullException(nameof(runner));

    public Task<ErpWriteResult> WriteAsync(PurchaseInvoiceCommand command, MikroConnectionSettings settings, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(settings);
        var header = command.Header;
        return _runner.RunAsync(
            settings,
            new MikroWriteRequest(DocumentType, header.ExternalId, header.ErpUserNo, settings.DatabaseName),
            (session, token) => WriteAsync(session, command, token),
            ct);
    }

    /// <summary>Writes the invoice inside an open session; the caller commits.</summary>
    public static async Task<MikroWrittenDocument> WriteAsync(MikroWriteSession session, PurchaseInvoiceCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var header = command.Header;
        if (command.Lines.Count == 0) throw new MikroWriteException(ErpWriteError.InvalidDocument());

        var lookup = new MikroDocumentLookup(session);
        var supplier = await lookup.CustomerAsync(header.CustomerCode, MikroCustomerUse.Purchase, isOrder: false, ct).ConfigureAwait(false);
        await lookup.EnsureWarehouseAsync(command.WarehouseNo, ct).ConfigureAwait(false);
        await lookup.EnsureSalespersonAsync(header.SalespersonCode, ct).ConfigureAwait(false);

        var priced = new List<MikroPricedLine>(command.Lines.Count);
        foreach (var line in command.Lines)
        {
            if (line.Quantity <= 0) throw new MikroWriteException(ErpWriteError.InvalidQuantity(priced.Count + 1));
            var stock = await lookup.StockAsync(line.StockCode, forSale: false, ct).ConfigureAwait(false);
            // K6: the VAT is the stock card's, not something the phone carries.
            priced.Add(MikroPriceCalculator.PurchaseLine(line.UnitPrice, line.Quantity, stock.VatPointer, stock.VatRate, command.PricesIncludeVat));
        }

        var document = new MikroPricedDocument(priced);
        MikroPriceCalculator.EnsurePurchaseTotal(document, header.ExpectedTotal, command.PricesIncludeVat);

        var series = string.IsNullOrEmpty(header.Series)
            ? await lookup.PurchaseSeriesAsync(supplier.Code, ct).ConfigureAwait(false)
            : header.Series;
        var number = await session.NextNumberAsync(MikroDocumentNumbering.PurchaseInvoice, series, ct).ConfigureAwait(false);

        var headerRecno = await session.InsertAsync(MikroTables.CariHareket,
            HeaderRow(command, supplier, document, series, number, Guid.NewGuid()), ct).ConfigureAwait(false);
        for (var i = 0; i < command.Lines.Count; i++)
        {
            await session.InsertAsync(MikroTables.StokHareket, LineRow(command, i, priced[i], series, number, headerRecno), ct).ConfigureAwait(false);
        }
        await session.InsertAsync(MikroTables.EvrakAciklama,
            MikroSalesReturnWriter.DescriptionRow(series, number, Note(command)), ct).ConfigureAwait(false);

        return new MikroWrittenDocument(MikroTables.CariHareket.Name, MikroCodes.ChaEvrakTip.AlisFaturasi, series, number, headerRecno);
    }

    /// <summary>
    /// What the document says it is. The supplier's own invoice number is worth keeping where a person
    /// will look for it, and <c>cha_belge_no</c> is not that place — the live company fills it on 44 of
    /// 1025 purchases (§15), so the note is where it actually gets read.
    /// </summary>
    internal static string? Note(PurchaseInvoiceCommand command) =>
        string.IsNullOrWhiteSpace(command.SupplierInvoiceNo)
            ? command.Header.Description
            : $"{command.Header.Description} (Fatura no: {command.SupplierInvoiceNo})".TrimStart();

    internal static Dictionary<string, object?> HeaderRow(
        PurchaseInvoiceCommand command, MikroCustomer supplier, MikroPricedDocument document, string series, int number, Guid uuid)
    {
        var header = command.Header;
        var day = header.OccurredAt.Date;
        return new Dictionary<string, object?>
        {
            ["cha_evrak_tip"] = MikroCodes.ChaEvrakTip.AlisFaturasi,
            ["cha_evrakno_seri"] = series,
            ["cha_evrakno_sira"] = number,
            ["cha_satir_no"] = 0,
            ["cha_tarihi"] = day,
            ["cha_belge_tarih"] = day,
            // Mal girdi, borç tedarikçiye: alacak.
            ["cha_tip"] = MikroCodes.ChaTip.Alacak,
            ["cha_cinsi"] = MikroCodes.ChaCinsi.ToptanFatura,
            // Satış iadesiyle ayrıldığı tek yer: iade bayrağı.
            ["cha_normal_Iade"] = MikroCodes.NormalIade.Normal,
            // Açık hesap: ödemesi ayrı tediye evrağı (K5).
            ["cha_tpoz"] = MikroCodes.ChaTpoz.Acik,
            ["cha_cari_cins"] = MikroCodes.HesapCinsi.Carimiz,
            ["cha_kod"] = supplier.Code,
            ["cha_grupno"] = 0,
            ["cha_satici_kodu"] = header.SalespersonCode,
            ["cha_srmrkkodu"] = header.ResponsibilityCenterCode,
            ["cha_projekodu"] = header.ProjectCode,
            ["cha_belge_no"] = command.SupplierInvoiceNo,
            ["cha_aciklama"] = null,
            ["cha_d_kur"] = 1d,
            ["cha_altd_kur"] = 1d,
            ["cha_karsid_kur"] = 1d,
            ["cha_meblag"] = document.Total,
            ["cha_aratoplam"] = document.Gross,
            ["cha_vergi1"] = document.VatBucket(1),
            ["cha_vergi2"] = document.VatBucket(2),
            ["cha_vergi3"] = document.VatBucket(3),
            ["cha_vergi4"] = document.VatBucket(4),
            ["cha_vergi5"] = document.VatBucket(5),
            ["cha_uuid"] = uuid.ToString("D").ToUpperInvariant(),
        };
    }

    internal static Dictionary<string, object?> LineRow(
        PurchaseInvoiceCommand command, int index, MikroPricedLine priced, string series, int number, int headerRecno)
    {
        var header = command.Header;
        var line = command.Lines[index];
        var day = header.OccurredAt.Date;
        var row = new Dictionary<string, object?>
        {
            ["sth_tarih"] = day,
            ["sth_belge_tarih"] = day,
            ["sth_malkbl_sevk_tarihi"] = day,
            // Mal içeri giriyor.
            ["sth_tip"] = MikroCodes.SthTip.Giris,
            ["sth_cins"] = MikroCodes.SthCins.Toptan,
            ["sth_normal_iade"] = MikroCodes.NormalIade.Normal,
            ["sth_evraktip"] = MikroCodes.SthEvrakTip.GirisFaturasi,
            ["sth_evrakno_seri"] = series,
            ["sth_evrakno_sira"] = number,
            ["sth_satirno"] = index,
            ["sth_stok_kod"] = line.StockCode,
            ["sth_cari_cinsi"] = MikroCodes.HesapCinsi.Carimiz,
            ["sth_cari_kodu"] = header.CustomerCode,
            ["sth_plasiyer_kodu"] = header.SalespersonCode,
            ["sth_har_doviz_kuru"] = 1d,
            ["sth_alt_doviz_kuru"] = 1d,
            ["sth_stok_doviz_kuru"] = 1d,
            ["sth_miktar"] = priced.Quantity,
            ["sth_miktar2"] = priced.Quantity,
            ["sth_birim_pntr"] = line.UnitPointer,
            ["sth_tutar"] = priced.Gross,
            ["sth_iskonto1"] = 0,
            ["sth_vergi_pntr"] = priced.VatPointer,
            ["sth_vergi"] = priced.Vat,
            ["sth_fat_recid_recno"] = headerRecno,
            ["sth_giris_depo_no"] = command.WarehouseNo,
            ["sth_cikis_depo_no"] = command.WarehouseNo,
            ["sth_adres_no"] = 1,
            // Alışta fiyat listesi yoktur (§10).
            ["sth_fiyat_liste_no"] = 0,
            ["sth_proje_kodu"] = header.ProjectCode,
            ["sth_cari_srm_merkezi"] = header.ResponsibilityCenterCode,
            ["sth_stok_srm_merkezi"] = header.ResponsibilityCenterCode,
            ["sth_isk_mas1"] = 0,
        };
        for (var m = 2; m <= 10; m++) row[$"sth_isk_mas{m}"] = 1;
        return row;
    }
}
