using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// A sale with money taken separately (goal ERP yazım Y3h): the open invoice and its receipt are one transaction.
/// The success case is rolled back; the failure case commits nothing by design. Nothing is left in the test copy.
/// </summary>
[Collection(MikroWriteTestDatabase.Collection)]
public class MikroMixedPaymentSaleLiveTests
{
    private const string InvoiceSeries = "ERPBT7";
    private const string ReceiptSeries = "ERPBT8";

    private static async Task<SalesDocumentCommand> CommandAsync(Microsoft.Data.SqlClient.SqlConnection conn, string bankForCard)
    {
        var customer = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 cari_kod FROM CARI_HESAPLAR WHERE ISNULL(cari_hareket_tipi, 0) = 0 ORDER BY cari_kod"))!;
        var (stock, pointer) = await conn.QueryFirstAsync<(string, byte)>(
            "SELECT TOP 1 sto_kod, sto_toptan_vergi FROM STOKLAR WHERE ISNULL(sto_satis_dursun, 0) = 0 AND ISNULL(sto_pasif_fl, 0) = 0 ORDER BY sto_kod");
        var rate = Convert.ToDecimal(await conn.ExecuteScalarAsync<double>("SELECT CAST(dbo.fn_VergiYuzde(@pointer) AS float)", new { pointer }));
        var (list, includesVat) = await conn.QueryFirstAsync<(int, bool)>("SELECT TOP 1 sfl_sirano, CAST(ISNULL(sfl_kdvdahil, 0) AS bit) FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI ORDER BY sfl_sirano");
        var warehouse = await conn.ExecuteScalarAsync<int>("SELECT TOP 1 dep_no FROM DEPOLAR ORDER BY dep_no");
        var cash = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 0 ORDER BY kas_kod"))!;
        var total = MikroPriceCalculator.SaleLine(100m, 4m, 0m, 0m, 0m, pointer, rate, includesVat).Total;
        var day = new DateTime(2026, 9, 17);
        return new SalesDocumentCommand(
            new ErpDocumentHeader($"ERPBT-MIX-{Guid.NewGuid():N}", day.AddHours(10), customer, null, 1, InvoiceSeries, null, total),
            SalesDocumentKind.Invoice, warehouse, list, OrderApprovalMode.Approved, SalesSettlement.Open, null,
            [new SalesDocumentLine(stock, 4m, 1, 100m, 0m, 0m, 0m)],
            [new CollectionPayment(CollectionMethod.Cash, 100m, day, cash), new CollectionPayment(CollectionMethod.Card, 50m, day, bankForCard)],
            ReceiptSeries);
    }

    [Fact]
    public async Task The_invoice_and_its_receipt_are_written_together()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var bank = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 ban_kod FROM BANKALAR ORDER BY ban_kod"))!;
        var command = await CommandAsync(conn, bank);
        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);

        var invoice = await MikroSalesInvoiceWriter.WriteWithPaymentsAsync(session, command, CancellationToken.None);

        (await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 63 AND cha_evrakno_seri = @InvoiceSeries AND cha_evrakno_sira = @Number",
            new { InvoiceSeries, invoice.Number }, session.Transaction)).Should().Be(1);
        var receipt = (await conn.QueryAsync<(short Line, string Note, decimal Amount)>(
            "SELECT cha_satir_no, cha_aciklama, CAST(cha_meblag AS decimal(18,2)) FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 1 AND cha_evrakno_seri = @ReceiptSeries ORDER BY cha_satir_no",
            new { ReceiptSeries }, session.Transaction)).ToList();
        receipt.Select(r => r.Amount).Should().Equal(100m, 50m);
        (await conn.ExecuteScalarAsync<string>("SELECT egk_evracik1 FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano = 51 AND egk_evr_tip = 1 AND egk_evr_seri = @ReceiptSeries",
            new { ReceiptSeries }, session.Transaction)).Should().Be($"{InvoiceSeries}-{invoice.Number} satış faturasının tahsilatı");
        // Not committed: disposing the session rolls both back.
    }

    [Fact]
    public async Task A_receipt_that_cannot_be_written_takes_the_invoice_with_it()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var command = await CommandAsync(conn, bankForCard: "ERPBT-YOK");
        var runner = new MikroDocumentWriteRunner(new MikroConnectionFactory(), new MikroDocumentLedger(), cache: null, NullLogger<MikroDocumentWriteRunner>.Instance);
        var settings = new MikroConnectionSettings(MikroWriteTestDatabase.Server, string.Empty, string.Empty, MikroWriteTestDatabase.Database!, IntegratedSecurity: true);

        var result = await runner.RunAsync(settings, new MikroWriteRequest(MikroSalesInvoiceWriter.DocumentType, command.Header.ExternalId, 1),
            (session, ct) => MikroSalesInvoiceWriter.WriteWithPaymentsAsync(session, command, ct));

        result.ErrorCode.Should().Be(ErpWriteError.BankAccountNotFoundCode);
        (await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM CARI_HESAP_HAREKETLERI WHERE cha_evrakno_seri IN (@InvoiceSeries, @ReceiptSeries)", new { InvoiceSeries, ReceiptSeries }))
            .Should().Be(0, "the invoice written before the receipt failed is rolled back");
        (await new MikroDocumentLedger().FindAsync(conn, null, MikroSalesInvoiceWriter.DocumentType, command.Header.ExternalId)).Should().BeNull();
    }
}
