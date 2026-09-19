using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// A sayım fişi written into a Mikro test copy (ERP yazım 3 Y3c), inside a session that is rolled back.
/// Writing it for real is what proves the two things unit tests cannot: that the number continues the
/// warehouse's own sequence, and that writing a count leaves every stock figure untouched (K9).
/// </summary>
[Collection(MikroWriteTestDatabase.Collection)]
public class MikroStockCountWriterLiveTests
{
    [Fact]
    public async Task A_count_is_one_receipt_of_lines_that_continues_the_warehouse_sequence()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var warehouse = await conn.ExecuteScalarAsync<int>("SELECT TOP 1 dep_no FROM DEPOLAR ORDER BY dep_no");
        var codes = (await conn.QueryAsync<string>("SELECT TOP 3 sto_kod FROM STOKLAR ORDER BY sto_kod")).ToList();
        if (codes.Count < 3) return;

        var highest = await conn.ExecuteScalarAsync<int?>(
            "SELECT MAX(sym_evrakno) FROM SAYIM_SONUCLARI WHERE sym_depono = @warehouse", new { warehouse }) ?? 0;

        var command = new StockCountCommand(
            new ErpDocumentHeader($"ERPBT-SY-{Guid.NewGuid():N}", new DateTime(2026, 9, 19, 16, 30, 0), CustomerCode: "",
                SalespersonCode: null, ErpUserNo: 1, Series: "", Description: "ErpBridge test sayımı", ExpectedTotal: 0m),
            warehouse,
            [
                new StockCountLine(codes[0], "8690000000001", 12m),
                new StockCountLine(codes[1], null, 0m),
                new StockCountLine(codes[2], null, 7.5m),
            ]);

        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);
        var stockBefore = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM STOK_HAREKETLERI", transaction: session.Transaction);

        var written = await MikroStockCountWriter.WriteAsync(session, command, CancellationToken.None);

        written.Number.Should().Be(highest + 1, "sayım numarası deponun kendi dizisini sürdürür");

        var rows = (await conn.QueryAsync(
                "SELECT * FROM SAYIM_SONUCLARI WHERE sym_depono = @warehouse AND sym_evrakno = @Number ORDER BY sym_satirno",
                new { warehouse, written.Number }, session.Transaction))
            .Cast<IDictionary<string, object?>>().ToList();

        rows.Should().HaveCount(3);
        rows.Select(r => r["sym_Stokkodu"]).Should().Equal(codes[0], codes[1], codes[2]);
        rows.Select(r => Convert.ToDecimal(r["sym_miktar1"])).Should().Equal(12m, 0m, 7.5m);
        rows[1]["sym_barkod"].Should().Be(codes[1], "okutulmadıysa barkod kolonuna stok kodu yazılır");
        rows[0]["sym_RECno"].Should().Be(written.HeaderRecNo);
        // Referans §1: Mikro arkasında NULL bırakmaz.
        rows.SelectMany(r => r.Where(c => c.Value is null)).Should().BeEmpty();

        // K9: sayım yazmak stoğu hareket ettirmez — uygulamak Mikro'nun kendi adımıdır.
        var stockAfter = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM STOK_HAREKETLERI", transaction: session.Transaction);
        stockAfter.Should().Be(stockBefore, "sayım fişi stok hareketi üretmez");
        // Commit edilmedi: oturum kapanınca her şey geri alınır.
    }
}
