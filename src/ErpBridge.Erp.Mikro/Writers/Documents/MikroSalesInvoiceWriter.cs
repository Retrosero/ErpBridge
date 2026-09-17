using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Writers.Documents;

/// <summary>
/// Writes a phone sale as a Mikro V15 sales invoice (goal ERP yazım Y3c, reference §2–§3): one
/// <c>CARI_HESAP_HAREKETLERI</c> header, a <c>STOK_HAREKETLERI</c> row per line and the
/// <c>EVRAK_ACIKLAMALARI</c> row Mikro opens with every invoice. An open invoice debits the customer;
/// a paid one is closed to the cash box (nakit) or bank (kart, havale), so the money lands there and
/// the customer's balance does not move.
/// </summary>
public sealed class MikroSalesInvoiceWriter(MikroDocumentWriteRunner runner)
{
    public const int DescriptionLineLength = 127;
    public const int DescriptionLines = 10;
    public const int HeaderNoteLength = 40;
    public const int LineNoteLength = 50;

    /// <summary>Job document type the ledger records a phone sale under.</summary>
    public const string DocumentType = "sales_order";

    private readonly MikroDocumentWriteRunner _runner = runner ?? throw new ArgumentNullException(nameof(runner));

    public Task<ErpWriteResult> WriteAsync(SalesDocumentCommand command, MikroConnectionSettings settings, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(settings);
        if (command.Kind != SalesDocumentKind.Invoice)
            throw new ArgumentException("Only invoices are written here; orders and dispatch notes have their own writers.", nameof(command));

        var header = command.Header;
        return _runner.RunAsync(
            settings,
            new MikroWriteRequest(DocumentType, header.ExternalId, header.ErpUserNo, settings.DatabaseName),
            (session, token) => WriteWithPaymentsAsync(session, command, token),
            ct);
    }

    /// <summary>
    /// The invoice and, for money taken with an open sale (a part or mixed payment, D10), the collection
    /// receipt in the same session: one commit or none (goal ERP yazım Y3h). The ledger records the sale
    /// only; the receipt names the invoice in its description.
    /// </summary>
    public static async Task<MikroWrittenDocument> WriteWithPaymentsAsync(MikroWriteSession session, SalesDocumentCommand command, CancellationToken ct)
    {
        if (command.ExtraPayments is { Count: > 0 } && command.Settlement != SalesSettlement.Open)
            throw new ArgumentException("A sale closed to a cash box or bank takes no separate receipt.", nameof(command));

        var invoice = await WriteAsync(session, command, ct).ConfigureAwait(false);
        if (command.ExtraPayments is { Count: > 0 } payments)
        {
            await MikroCollectionReceiptWriter.WriteAsync(session, ReceiptFor(command, invoice, payments), ct).ConfigureAwait(false);
        }
        return invoice;
    }

    internal static CollectionCommand ReceiptFor(SalesDocumentCommand command, MikroWrittenDocument invoice, IReadOnlyList<CollectionPayment> payments)
    {
        var header = command.Header;
        var invoiceName = string.IsNullOrEmpty(invoice.Series) ? invoice.Number.ToString(System.Globalization.CultureInfo.InvariantCulture) : $"{invoice.Series}-{invoice.Number}";
        return new CollectionCommand(
            header with
            {
                Series = command.ExtraPaymentsSeries ?? string.Empty,
                Description = $"{invoiceName} satış faturasının tahsilatı",
                ExpectedTotal = payments.Sum(p => p.Amount),
            },
            payments);
    }

    /// <summary>Writes the invoice inside an open session; the caller commits.</summary>
    public static async Task<MikroWrittenDocument> WriteAsync(MikroWriteSession session, SalesDocumentCommand command, CancellationToken ct)
    {
        var header = command.Header;
        var lookup = new MikroDocumentLookup(session);
        var customer = await lookup.CustomerAsync(header.CustomerCode, MikroCustomerUse.Sale, isOrder: false, ct).ConfigureAwait(false);
        await lookup.EnsureWarehouseAsync(command.WarehouseNo, ct).ConfigureAwait(false);
        await lookup.EnsureSalespersonAsync(header.SalespersonCode, ct).ConfigureAwait(false);
        var listIncludesVat = await lookup.PriceListIncludesVatAsync(command.PriceListNo, ct).ConfigureAwait(false);

        var closing = await ClosingAsync(lookup, command, ct).ConfigureAwait(false);

        var priced = new List<MikroPricedLine>(command.Lines.Count);
        foreach (var line in command.Lines)
        {
            var stock = await lookup.StockAsync(line.StockCode, forSale: true, ct).ConfigureAwait(false);
            priced.Add(MikroPriceCalculator.SaleLine(
                line.ListUnitPrice, line.Quantity, line.LineDiscountPercent, line.CustomerDiscountPercent, line.GeneralDiscountPercent,
                stock.VatPointer, stock.VatRate, listIncludesVat));
        }
        var document = new MikroPricedDocument(priced);
        MikroPriceCalculator.EnsureTotal(document, header.ExpectedTotal);

        var number = await session.NextNumberAsync(MikroDocumentNumbering.SalesInvoice, header.Series, ct).ConfigureAwait(false);
        var headerRecno = await session.InsertAsync(MikroTables.CariHareket,
            HeaderRow(command, customer, closing, document, number, Guid.NewGuid()), ct).ConfigureAwait(false);
        for (var i = 0; i < command.Lines.Count; i++)
        {
            await session.InsertAsync(MikroTables.StokHareket,
                LineRow(command, i, priced[i], number, headerRecno), ct).ConfigureAwait(false);
        }
        await session.InsertAsync(MikroTables.EvrakAciklama, DescriptionRow(header.Series, number, header.Description), ct).ConfigureAwait(false);

        return new MikroWrittenDocument(MikroTables.CariHareket.Name, MikroCodes.ChaEvrakTip.SatisFaturasi, header.Series, number, headerRecno);
    }

    /// <summary>Where a paid invoice is closed: cash box for cash, bank for card and transfer; null when open.</summary>
    internal sealed record Closing(byte AccountKind, string AccountCode, byte GroupNo);

    private static async Task<Closing?> ClosingAsync(MikroDocumentLookup lookup, SalesDocumentCommand command, CancellationToken ct)
    {
        if (command.Settlement == SalesSettlement.Open) return null;
        var code = command.SettlementAccountCode
            ?? throw new MikroWriteException(ErpWriteError.ErpMappingMissing(command.Settlement == SalesSettlement.Cash ? "kasa kodu" : "banka kodu"));
        if (command.Settlement == SalesSettlement.Cash)
        {
            await lookup.EnsureCashBoxAsync(code, MikroCashBoxKind.Cash, ct).ConfigureAwait(false);
            return new Closing(MikroCodes.HesapCinsi.Kasamiz, code, 0);
        }

        await lookup.EnsureBankAsync(code, ct).ConfigureAwait(false);
        return new Closing(MikroCodes.HesapCinsi.Bankamiz, code, 1);
    }

    internal static Dictionary<string, object?> HeaderRow(
        SalesDocumentCommand command, MikroCustomer customer, Closing? closing, MikroPricedDocument document, int number, Guid uuid)
    {
        var header = command.Header;
        var day = header.OccurredAt.Date;
        return new Dictionary<string, object?>
        {
            ["cha_evrak_tip"] = MikroCodes.ChaEvrakTip.SatisFaturasi,
            ["cha_evrakno_seri"] = header.Series,
            ["cha_evrakno_sira"] = number,
            ["cha_satir_no"] = 0,
            ["cha_tarihi"] = day,
            ["cha_belge_tarih"] = day,
            ["cha_tip"] = MikroCodes.ChaTip.Borc,
            ["cha_cinsi"] = MikroCodes.ChaCinsi.ToptanFatura,
            ["cha_normal_Iade"] = MikroCodes.NormalIade.Normal,
            ["cha_tpoz"] = closing is null ? MikroCodes.ChaTpoz.Acik : MikroCodes.ChaTpoz.Kapali,
            ["cha_cari_cins"] = closing?.AccountKind ?? MikroCodes.HesapCinsi.Carimiz,
            ["cha_kod"] = closing?.AccountCode ?? customer.Code,
            ["cha_ciro_cari_kodu"] = customer.Code,
            ["cha_grupno"] = closing?.GroupNo ?? 0,
            ["cha_satici_kodu"] = header.SalespersonCode,
            ["cha_srmrkkodu"] = header.ResponsibilityCenterCode,
            ["cha_projekodu"] = header.ProjectCode,
            // Fora writes the customer's title on a closed invoice, where cha_kod is the cash box or bank.
            ["cha_aciklama"] = closing is null ? null : Cut(customer.Title, HeaderNoteLength),
            ["cha_d_kur"] = 1d,
            ["cha_altd_kur"] = 1d,
            ["cha_karsid_kur"] = 1d,
            ["cha_meblag"] = document.Total,
            ["cha_aratoplam"] = document.Gross,
            ["cha_ft_iskonto1"] = document.Discount1,
            ["cha_ft_iskonto2"] = document.Discount2,
            ["cha_ft_iskonto3"] = document.Discount3,
            ["cha_vergi1"] = document.VatBucket(1),
            ["cha_vergi2"] = document.VatBucket(2),
            ["cha_vergi3"] = document.VatBucket(3),
            ["cha_vergi4"] = document.VatBucket(4),
            ["cha_vergi5"] = document.VatBucket(5),
            ["cha_uuid"] = uuid.ToString("D").ToUpperInvariant(),
        };
    }

    internal static Dictionary<string, object?> LineRow(SalesDocumentCommand command, int index, MikroPricedLine priced, int number, int headerRecno)
    {
        var header = command.Header;
        var line = command.Lines[index];
        var day = header.OccurredAt.Date;
        var row = new Dictionary<string, object?>
        {
            ["sth_tarih"] = day,
            ["sth_belge_tarih"] = day,
            ["sth_malkbl_sevk_tarihi"] = day,
            ["sth_tip"] = MikroCodes.SthTip.Cikis,
            ["sth_cins"] = MikroCodes.SthCins.Toptan,
            ["sth_normal_iade"] = MikroCodes.NormalIade.Normal,
            ["sth_evraktip"] = MikroCodes.SthEvrakTip.CikisFaturasi,
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
            ["sth_iskonto2"] = priced.Discount2,
            ["sth_iskonto3"] = priced.Discount3,
            ["sth_vergi_pntr"] = priced.VatPointer,
            ["sth_vergi"] = priced.Vat,
            ["sth_fat_recid_recno"] = headerRecno,
            ["sth_giris_depo_no"] = command.WarehouseNo,
            ["sth_cikis_depo_no"] = command.WarehouseNo,
            ["sth_adres_no"] = 1,
            ["sth_fiyat_liste_no"] = command.PriceListNo,
            ["sth_aciklama"] = Cut(line.Note, LineNoteLength),
            ["sth_proje_kodu"] = header.ProjectCode,
            ["sth_cari_srm_merkezi"] = header.ResponsibilityCenterCode,
            ["sth_stok_srm_merkezi"] = header.ResponsibilityCenterCode,
            // The first discount applies to the gross amount, every later one to what the previous left (reference §1).
            ["sth_isk_mas1"] = 0,
        };
        for (var m = 2; m <= 10; m++) row[$"sth_isk_mas{m}"] = 1;
        return row;
    }

    /// <summary>
    /// Mikro opens this row with every invoice it saves (<c>egk_evr_doksayisi=1</c>, no text); the phone's
    /// note goes into <c>egk_evracik1..10</c>, 127 characters each.
    /// </summary>
    internal static Dictionary<string, object?> DescriptionRow(string series, int number, string? description)
    {
        var row = new Dictionary<string, object?>
        {
            ["egk_dosyano"] = MikroCodes.FileId.CariHesapHareketleri,
            ["egk_hareket_tip"] = 0,
            ["egk_evr_tip"] = MikroCodes.ChaEvrakTip.SatisFaturasi,
            ["egk_evr_seri"] = series,
            ["egk_evr_sira"] = number,
            ["egk_evr_ustkod"] = string.Empty,
            ["egk_evr_doksayisi"] = 1,
        };
        var text = (description ?? string.Empty).Trim();
        for (var i = 0; i < DescriptionLines && i * DescriptionLineLength < text.Length; i++)
        {
            row[$"egk_evracik{i + 1}"] = text.Substring(i * DescriptionLineLength, Math.Min(DescriptionLineLength, text.Length - i * DescriptionLineLength));
        }
        return row;
    }

    /// <summary>Free text is cut to the column; a code is never cut (the session refuses it instead).</summary>
    internal static string? Cut(string? text, int width) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim() is { } t && t.Length > width ? t[..width] : text.Trim();
}
