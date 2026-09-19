using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Writers;

/// <summary>
/// The rows of a sayım fişi (ERP yazım 3 Y3c, reference §14) without a database.
/// </summary>
public class MikroStockCountWriterTests
{
    private static readonly DateTime OccurredAt = new(2026, 9, 19, 16, 30, 0);

    private static StockCountCommand Command(params StockCountLine[] lines) => new(
        new ErpDocumentHeader("SAY-1", OccurredAt, CustomerCode: "", SalespersonCode: null, ErpUserNo: 1,
            Series: "", Description: "Depo sayımı", ExpectedTotal: 0m),
        WarehouseNo: 1,
        lines);

    [Fact]
    public void A_counted_line_carries_the_stock_code_quantity_and_main_unit()
    {
        var command = Command(new StockCountLine("59030", "8690000000001", 12m));

        var row = MikroStockCountWriter.LineRow(command, command.Lines[0], number: 24, index: 0);

        row.Should().Contain(new Dictionary<string, object?>
        {
            ["sym_depono"] = 1,
            ["sym_evrakno"] = 24,
            ["sym_satirno"] = 0,
            ["sym_Stokkodu"] = "59030",
            ["sym_barkod"] = "8690000000001",
            ["sym_miktar1"] = 12m,
            ["sym_birim_pntr"] = (byte)1,
        });
        row["sym_tarihi"].Should().Be(OccurredAt.Date, "sayım günün fişidir, saati taşımaz");
    }

    /// <summary>Canlı satırlarda barkod kolonu hiç boş değil; okutulmadıysa stok kodu yazılmış (§14).</summary>
    [Fact]
    public void A_line_counted_without_scanning_still_fills_the_barcode_column()
    {
        var command = Command(new StockCountLine("59030", null, 3m));

        MikroStockCountWriter.LineRow(command, command.Lines[0], number: 1, index: 0)["sym_barkod"]
            .Should().Be("59030");
    }

    /// <summary>Sayımda sıfır meşrudur: "hiç kalmamış" da bir sayım sonucudur.</summary>
    [Fact]
    public void Counting_zero_of_something_is_a_result_not_an_error()
    {
        var command = Command(new StockCountLine("59030", null, 0m));

        MikroStockCountWriter.LineRow(command, command.Lines[0], number: 1, index: 0)["sym_miktar1"].Should().Be(0m);
    }

    [Fact]
    public void Lines_are_numbered_in_the_order_they_were_counted()
    {
        var command = Command(
            new StockCountLine("A", null, 1m),
            new StockCountLine("B", null, 2m),
            new StockCountLine("C", null, 3m));

        var rows = command.Lines.Select((line, i) => MikroStockCountWriter.LineRow(command, line, number: 7, index: i)).ToList();

        rows.Select(r => r["sym_satirno"]).Should().Equal(0, 1, 2);
        rows.Select(r => r["sym_evrakno"]).Should().AllBeEquivalentTo(7, "bir sayım tek fiştir");
    }

    /// <summary>
    /// Sayımın serisi yoktur; numara deponun kendi dizisidir (K11). Bu yüzden numara kapsamı
    /// seri koşulu taşımamalı — taşısa boş seri filtresiyle yanlış MAX bulunurdu.
    /// </summary>
    [Fact]
    public void The_number_is_scoped_to_the_warehouse_and_not_to_a_series()
    {
        var (sql, _) = MikroWriteSession.BuildNextNumber(MikroDocumentNumbering.StockCount(warehouseNo: 3), series: string.Empty);

        sql.Should().Contain("SAYIM_SONUCLARI").And.Contain("sym_depono").And.Contain("MAX([sym_evrakno])");
        sql.Should().NotContain("@Series");
        sql.Should().Contain("UPDLOCK", "iki sayım aynı numarayı almamalı");
    }
}
