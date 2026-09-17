using System.Globalization;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Writers.Documents;

/// <summary>
/// A phone sale as the Mikro document the company chose (K1: order, dispatch note or invoice), and for
/// money taken with an open document its collection receipt in the same transaction (D10, goal ERP yazım
/// Y3h): one commit or none. The ledger records the sale; the receipt names the document it belongs to.
/// </summary>
public sealed class MikroSalesDocumentWriter(MikroDocumentWriteRunner runner)
{
    /// <summary>Job document type the ledger records a phone sale under.</summary>
    public const string DocumentType = "sales_order";

    private readonly MikroDocumentWriteRunner _runner = runner ?? throw new ArgumentNullException(nameof(runner));

    public Task<ErpWriteResult> WriteAsync(SalesDocumentCommand command, MikroConnectionSettings settings, CancellationToken ct = default)
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

    /// <summary>Writes the document and its receipt inside an open session; the caller commits.</summary>
    public static async Task<MikroWrittenDocument> WriteAsync(MikroWriteSession session, SalesDocumentCommand command, CancellationToken ct)
    {
        if (command.ExtraPayments is { Count: > 0 } && command.Settlement != SalesSettlement.Open)
            throw new ArgumentException("A sale closed to a cash box or bank takes no separate receipt.", nameof(command));
        if (command.Kind != SalesDocumentKind.Invoice && command.Settlement != SalesSettlement.Open)
            throw new ArgumentException("Only an invoice can be closed to a cash box or bank; a paid order or dispatch note carries a receipt.", nameof(command));

        var document = command.Kind switch
        {
            SalesDocumentKind.Invoice => await MikroSalesInvoiceWriter.WriteAsync(session, command, ct).ConfigureAwait(false),
            SalesDocumentKind.Order => await MikroSalesOrderDocumentWriter.WriteAsync(session, command, ct).ConfigureAwait(false),
            _ => await MikroSalesDispatchWriter.WriteAsync(session, command, ct).ConfigureAwait(false),
        };

        if (command.ExtraPayments is { Count: > 0 } payments)
        {
            await MikroCollectionReceiptWriter.WriteAsync(session, ReceiptFor(command, document, payments), ct).ConfigureAwait(false);
        }
        return document;
    }

    internal static CollectionCommand ReceiptFor(SalesDocumentCommand command, MikroWrittenDocument document, IReadOnlyList<CollectionPayment> payments)
    {
        var name = string.IsNullOrEmpty(document.Series) ? document.Number.ToString(CultureInfo.InvariantCulture) : $"{document.Series}-{document.Number}";
        var kind = command.Kind switch
        {
            SalesDocumentKind.Order => "siparişin",
            SalesDocumentKind.Dispatch => "irsaliyenin",
            _ => "satış faturasının",
        };
        return new CollectionCommand(
            command.Header with
            {
                Series = command.ExtraPaymentsSeries ?? string.Empty,
                Description = $"{name} {kind} tahsilatı",
                ExpectedTotal = payments.Sum(p => p.Amount),
            },
            payments);
    }

    /// <summary>The checks and prices every sale kind shares: customer, warehouse, salesperson, list, stocks, total.</summary>
    internal static async Task<(MikroCustomer Customer, MikroPricedDocument Document)> PriceAsync(
        MikroDocumentLookup lookup, SalesDocumentCommand command, bool isOrder, CancellationToken ct)
    {
        var header = command.Header;
        var customer = await lookup.CustomerAsync(header.CustomerCode, MikroCustomerUse.Sale, isOrder, ct).ConfigureAwait(false);
        await lookup.EnsureWarehouseAsync(command.WarehouseNo, ct).ConfigureAwait(false);
        await lookup.EnsureSalespersonAsync(header.SalespersonCode, ct).ConfigureAwait(false);
        var listIncludesVat = await lookup.PriceListIncludesVatAsync(command.PriceListNo, ct).ConfigureAwait(false);

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
        return (customer, document);
    }
}

/// <summary>
/// A phone sale as a Mikro V15 customer order (goal ERP yazım Y3d, reference §7): a <c>SIPARISLER</c> row per
/// line with Fora's values. An approved order (K7) carries the approving Mikro user and may be called into a
/// dispatch note or invoice; a pending one waits for approval in Mikro.
/// </summary>
public static class MikroSalesOrderDocumentWriter
{
    public const short FileId = 21;
    public const int NoteLength = 50;

    public static async Task<MikroWrittenDocument> WriteAsync(MikroWriteSession session, SalesDocumentCommand command, CancellationToken ct)
    {
        var (_, document) = await MikroSalesDocumentWriter.PriceAsync(new MikroDocumentLookup(session), command, isOrder: true, ct).ConfigureAwait(false);
        var number = await session.NextNumberAsync(MikroDocumentNumbering.SalesOrder, command.Header.Series, ct).ConfigureAwait(false);
        var first = 0;
        for (var i = 0; i < command.Lines.Count; i++)
        {
            var recno = await session.InsertAsync(MikroTables.Siparis, Row(command, i, document.Lines[i], number, session.ErpUserNo), ct).ConfigureAwait(false);
            if (i == 0) first = recno;
        }
        return new MikroWrittenDocument(MikroTables.Siparis.Name, 0, command.Header.Series, number, first);
    }

    internal static Dictionary<string, object?> Row(SalesDocumentCommand command, int index, MikroPricedLine priced, int number, short erpUserNo)
    {
        var header = command.Header;
        var line = command.Lines[index];
        var day = header.OccurredAt.Date;
        var approved = command.ApprovalMode == OrderApprovalMode.Approved;
        var row = new Dictionary<string, object?>
        {
            ["sip_fileid"] = FileId,
            ["sip_tarih"] = day,
            ["sip_teslim_tarih"] = (command.DeliveryDate ?? day).Date,
            ["sip_tip"] = 0,
            ["sip_cins"] = 0,
            ["sip_evrakno_seri"] = header.Series,
            ["sip_evrakno_sira"] = number,
            ["sip_satirno"] = index,
            ["sip_belge_tarih"] = day,
            ["sip_satici_kod"] = header.SalespersonCode,
            ["sip_musteri_kod"] = header.CustomerCode,
            ["sip_stok_kod"] = line.StockCode,
            ["sip_b_fiyat"] = priced.UnitPrice,
            ["sip_miktar"] = priced.Quantity,
            ["sip_birim_pntr"] = line.UnitPointer,
            ["sip_tutar"] = priced.Gross,
            ["sip_iskonto_1"] = priced.Discount1,
            ["sip_iskonto_2"] = priced.Discount2,
            ["sip_iskonto_3"] = priced.Discount3,
            ["sip_vergi_pntr"] = priced.VatPointer,
            ["sip_vergi"] = priced.Vat,
            ["sip_masvergi_pntr"] = 4,
            ["sip_aciklama"] = MikroSalesInvoiceWriter.Cut(line.Note, NoteLength),
            ["sip_aciklama2"] = index == 0 ? MikroSalesInvoiceWriter.Cut(header.Description, NoteLength) : null,
            ["sip_depono"] = command.WarehouseNo,
            ["sip_OnaylayanKulNo"] = approved ? erpUserNo : (short)0,
            ["sip_cagrilabilir_fl"] = approved,
            ["sip_cari_sormerk"] = header.ResponsibilityCenterCode,
            ["sip_stok_sormerk"] = header.ResponsibilityCenterCode,
            ["sip_projekodu"] = header.ProjectCode,
            ["sip_doviz_kuru"] = 1d,
            ["sip_alt_doviz_kuru"] = 1d,
            ["sip_adresno"] = 1,
            ["sip_fiyat_liste_no"] = command.PriceListNo,
            // Discounts apply the way the invoice lines do: the first to the gross, each later one to what is left.
            ["sip_iskonto1"] = 0,
        };
        for (var m = 2; m <= 6; m++) row[$"sip_iskonto{m}"] = 1;
        return row;
    }
}

/// <summary>
/// A phone sale as a Mikro V15 sales dispatch note (goal ERP yazım Y3e, reference §7): the invoice's stock lines
/// as an outgoing dispatch (<c>sth_evraktip=1</c>), not yet tied to an invoice, with the delivery date; no
/// customer movement until it is invoiced.
/// </summary>
public static class MikroSalesDispatchWriter
{
    public static async Task<MikroWrittenDocument> WriteAsync(MikroWriteSession session, SalesDocumentCommand command, CancellationToken ct)
    {
        var (_, document) = await MikroSalesDocumentWriter.PriceAsync(new MikroDocumentLookup(session), command, isOrder: false, ct).ConfigureAwait(false);
        var header = command.Header;
        var number = await session.NextNumberAsync(MikroDocumentNumbering.SalesDispatch, header.Series, ct).ConfigureAwait(false);
        var first = 0;
        for (var i = 0; i < command.Lines.Count; i++)
        {
            var recno = await session.InsertAsync(MikroTables.StokHareket, Row(command, i, document.Lines[i], number), ct).ConfigureAwait(false);
            if (i == 0) first = recno;
        }

        if (!string.IsNullOrWhiteSpace(header.Description))
        {
            var description = MikroSalesInvoiceWriter.DescriptionRow(header.Series, number, header.Description);
            description["egk_dosyano"] = MikroCodes.FileId.StokHareketleri;
            description["egk_hareket_tip"] = 1;
            description["egk_evr_tip"] = MikroCodes.SthEvrakTip.CikisIrsaliyesi;
            description["egk_evr_doksayisi"] = 0;
            await session.InsertAsync(MikroTables.EvrakAciklama, description, ct).ConfigureAwait(false);
        }

        return new MikroWrittenDocument(MikroTables.StokHareket.Name, MikroCodes.SthEvrakTip.CikisIrsaliyesi, header.Series, number, first);
    }

    internal static Dictionary<string, object?> Row(SalesDocumentCommand command, int index, MikroPricedLine priced, int number)
    {
        var row = MikroSalesInvoiceWriter.LineRow(command, index, priced, number, headerRecno: 0);
        row["sth_evraktip"] = MikroCodes.SthEvrakTip.CikisIrsaliyesi;
        row["sth_fat_recid_recno"] = 0;
        row["sth_malkbl_sevk_tarihi"] = (command.DeliveryDate ?? command.Header.OccurredAt).Date;
        return row;
    }
}
