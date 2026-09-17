using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Writers.Documents;

/// <summary>
/// Writes a phone return as a Mikro V15 sales return invoice (goal ERP yazım Y3f, reference §4, D11): Mikro
/// books it as a purchase invoice with the return flag (<c>cha_evrak_tip=0</c>, credit, <c>cha_normal_Iade=1</c>;
/// lines <c>sth_evraktip=3</c>, incoming) and so shares the purchase invoices' numbers. An open return credits
/// the customer; a refunded one is closed to the cash box or bank the money left from. A damaged item's
/// condition difference is the line's first discount and its reason the line note; the original invoice is
/// not linked (Fora leaves <c>sth_iade_evrak_*</c> empty).
/// </summary>
public sealed class MikroSalesReturnWriter(MikroDocumentWriteRunner runner)
{
    /// <summary>Job document type the ledger records a phone return under.</summary>
    public const string DocumentType = "sales_return";

    private readonly MikroDocumentWriteRunner _runner = runner ?? throw new ArgumentNullException(nameof(runner));

    public Task<ErpWriteResult> WriteAsync(SalesReturnCommand command, MikroConnectionSettings settings, CancellationToken ct = default)
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

    /// <summary>Writes the return inside an open session; the caller commits.</summary>
    public static async Task<MikroWrittenDocument> WriteAsync(MikroWriteSession session, SalesReturnCommand command, CancellationToken ct)
    {
        var header = command.Header;
        var lookup = new MikroDocumentLookup(session);
        var customer = await lookup.CustomerAsync(header.CustomerCode, MikroCustomerUse.SaleReturn, isOrder: false, ct).ConfigureAwait(false);
        await lookup.EnsureWarehouseAsync(command.WarehouseNo, ct).ConfigureAwait(false);
        await lookup.EnsureSalespersonAsync(header.SalespersonCode, ct).ConfigureAwait(false);
        var listIncludesVat = await lookup.PriceListIncludesVatAsync(command.PriceListNo, ct).ConfigureAwait(false);
        var closing = await ClosingAsync(lookup, command, ct).ConfigureAwait(false);

        var priced = new List<MikroPricedLine>(command.Lines.Count);
        foreach (var line in command.Lines)
        {
            var stock = await lookup.StockAsync(line.StockCode, forSale: false, ct).ConfigureAwait(false);
            priced.Add(MikroPriceCalculator.ReturnLine(line.ListUnitPrice, line.Quantity, line.ConditionRatio, stock.VatPointer, stock.VatRate, listIncludesVat));
        }
        var document = new MikroPricedDocument(priced);
        MikroPriceCalculator.EnsureTotal(document, header.ExpectedTotal);

        var number = await session.NextNumberAsync(MikroDocumentNumbering.SalesReturnInvoice, header.Series, ct).ConfigureAwait(false);
        var headerRecno = await session.InsertAsync(MikroTables.CariHareket,
            HeaderRow(command, customer, closing, document, number, Guid.NewGuid()), ct).ConfigureAwait(false);
        for (var i = 0; i < command.Lines.Count; i++)
        {
            await session.InsertAsync(MikroTables.StokHareket, LineRow(command, i, priced[i], number, headerRecno), ct).ConfigureAwait(false);
        }
        await session.InsertAsync(MikroTables.EvrakAciklama, DescriptionRow(header.Series, number, header.Description), ct).ConfigureAwait(false);

        return new MikroWrittenDocument(MikroTables.CariHareket.Name, MikroCodes.ChaEvrakTip.AlisFaturasi, header.Series, number, headerRecno);
    }

    private static async Task<MikroSalesInvoiceWriter.Closing?> ClosingAsync(MikroDocumentLookup lookup, SalesReturnCommand command, CancellationToken ct)
    {
        if (command.Settlement == ReturnSettlement.Open) return null;
        var code = command.SettlementAccountCode
            ?? throw new MikroWriteException(ErpWriteError.ErpMappingMissing(command.Settlement == ReturnSettlement.Cash ? "kasa kodu" : "banka kodu"));
        if (command.Settlement == ReturnSettlement.Cash)
        {
            await lookup.EnsureCashBoxAsync(code, MikroCashBoxKind.Cash, ct).ConfigureAwait(false);
            return new MikroSalesInvoiceWriter.Closing(MikroCodes.HesapCinsi.Kasamiz, code, 0);
        }

        await lookup.EnsureBankAsync(code, ct).ConfigureAwait(false);
        return new MikroSalesInvoiceWriter.Closing(MikroCodes.HesapCinsi.Bankamiz, code, 1);
    }

    internal static Dictionary<string, object?> HeaderRow(
        SalesReturnCommand command, MikroCustomer customer, MikroSalesInvoiceWriter.Closing? closing, MikroPricedDocument document, int number, Guid uuid)
    {
        var header = command.Header;
        var day = header.OccurredAt.Date;
        return new Dictionary<string, object?>
        {
            ["cha_evrak_tip"] = MikroCodes.ChaEvrakTip.AlisFaturasi,
            ["cha_evrakno_seri"] = header.Series,
            ["cha_evrakno_sira"] = number,
            ["cha_satir_no"] = 0,
            ["cha_tarihi"] = day,
            ["cha_belge_tarih"] = day,
            ["cha_tip"] = MikroCodes.ChaTip.Alacak,
            ["cha_cinsi"] = MikroCodes.ChaCinsi.ToptanFatura,
            ["cha_normal_Iade"] = MikroCodes.NormalIade.Iade,
            ["cha_tpoz"] = closing is null ? MikroCodes.ChaTpoz.Acik : MikroCodes.ChaTpoz.Kapali,
            ["cha_cari_cins"] = closing?.AccountKind ?? MikroCodes.HesapCinsi.Carimiz,
            ["cha_kod"] = closing?.AccountCode ?? customer.Code,
            ["cha_ciro_cari_kodu"] = customer.Code,
            ["cha_grupno"] = closing?.GroupNo ?? 0,
            ["cha_satici_kodu"] = header.SalespersonCode,
            ["cha_srmrkkodu"] = header.ResponsibilityCenterCode,
            ["cha_projekodu"] = header.ProjectCode,
            ["cha_aciklama"] = closing is null ? null : MikroSalesInvoiceWriter.Cut(customer.Title, MikroSalesInvoiceWriter.HeaderNoteLength),
            ["cha_d_kur"] = 1d,
            ["cha_altd_kur"] = 1d,
            ["cha_karsid_kur"] = 1d,
            ["cha_meblag"] = document.Total,
            ["cha_aratoplam"] = document.Gross,
            ["cha_ft_iskonto1"] = document.Discount1,
            ["cha_vergi1"] = document.VatBucket(1),
            ["cha_vergi2"] = document.VatBucket(2),
            ["cha_vergi3"] = document.VatBucket(3),
            ["cha_vergi4"] = document.VatBucket(4),
            ["cha_vergi5"] = document.VatBucket(5),
            ["cha_uuid"] = uuid.ToString("D").ToUpperInvariant(),
        };
    }

    internal static Dictionary<string, object?> LineRow(SalesReturnCommand command, int index, MikroPricedLine priced, int number, int headerRecno)
    {
        var header = command.Header;
        var line = command.Lines[index];
        var day = header.OccurredAt.Date;
        var row = new Dictionary<string, object?>
        {
            ["sth_tarih"] = day,
            ["sth_belge_tarih"] = day,
            ["sth_malkbl_sevk_tarihi"] = day,
            ["sth_tip"] = MikroCodes.SthTip.Giris,
            ["sth_cins"] = MikroCodes.SthCins.Toptan,
            ["sth_normal_iade"] = MikroCodes.NormalIade.Iade,
            ["sth_evraktip"] = MikroCodes.SthEvrakTip.GirisFaturasi,
            ["sth_evrakno_seri"] = header.Series,
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
            ["sth_iskonto1"] = priced.Discount1,
            ["sth_vergi_pntr"] = priced.VatPointer,
            ["sth_vergi"] = priced.Vat,
            ["sth_fat_recid_recno"] = headerRecno,
            ["sth_giris_depo_no"] = command.WarehouseNo,
            ["sth_cikis_depo_no"] = command.WarehouseNo,
            ["sth_adres_no"] = 1,
            ["sth_fiyat_liste_no"] = command.PriceListNo,
            ["sth_aciklama"] = MikroSalesInvoiceWriter.Cut(line.Reason, MikroSalesInvoiceWriter.LineNoteLength),
            ["sth_proje_kodu"] = header.ProjectCode,
            ["sth_cari_srm_merkezi"] = header.ResponsibilityCenterCode,
            ["sth_stok_srm_merkezi"] = header.ResponsibilityCenterCode,
            ["sth_isk_mas1"] = 0,
        };
        for (var m = 2; m <= 10; m++) row[$"sth_isk_mas{m}"] = 1;
        return row;
    }

    /// <summary>The return's description row (<c>51 / hareket 1 / evrak 0</c>), as Mikro opens it.</summary>
    internal static Dictionary<string, object?> DescriptionRow(string series, int number, string? description)
    {
        var row = MikroSalesInvoiceWriter.DescriptionRow(series, number, description);
        row["egk_hareket_tip"] = 1;
        row["egk_evr_tip"] = MikroCodes.ChaEvrakTip.AlisFaturasi;
        row["egk_evr_doksayisi"] = 0;
        return row;
    }
}
