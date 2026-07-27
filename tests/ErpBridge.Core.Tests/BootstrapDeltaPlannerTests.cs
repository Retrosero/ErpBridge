using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions.Sync;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Core.Tests;

public sealed class BootstrapDeltaPlannerTests
{
    [Fact]
    public void Create_reports_only_changed_rows_and_missing_rows_as_deletes()
    {
        var planner = new BootstrapDeltaPlanner();
        var before = Package(Stock("A", "Eski"), Stock("B", "Silinecek"));
        var after = Package(Stock("A", "Yeni"), Stock("C", "Yeni kayit"));

        var delta = planner.Create(after, planner.Snapshot(before));

        delta.Upserts["stocks"].Select(row => row.Key).Should().BeEquivalentTo("A", "C");
        delta.Deletes["stocks"].Should().ContainSingle().Which.Should().Be("B");
    }

    private static SyncPackage Package(params StockPayload[] stocks) => SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST") with { Stocks = stocks };

    private static StockPayload Stock(string code, string name) => new(
        StockCode: code, Name: name, ShortName: null, ForeignName: null, DefaultTaxPointer: null,
        Unit1: null, Unit1Factor: null, Unit2: null, Unit2Factor: null, Unit3: null, Unit3Factor: null,
        MainGroupCode: null, SubGroupCode: null, SectorCode: null, BrandCode: null, ModelCode: null,
        ManufacturerCode: null, ShelfCode: null, BedenliTakip: false, RenkDetayli: false,
        StandardCost: null, Currency: null, Barcodes: Array.Empty<BarcodePayload>());
}
