using System.Diagnostics;
using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit.Abstractions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// How long the stock page's catalogue takes on a large company (GOAL_PANEL_DUZELTMELER G5): the first load,
/// an unchanged reload and the reload after one sale. A measurement, not a gate — opt in with
/// <c>ERPBridge_RUN_TIMING=1</c>; the numbers go to the test output.
/// </summary>
public sealed class PortalStockCatalogTimingTests : IClassFixture<SqliteCentralApiFactory>
{
    private const int Products = 20_000;
    private const int Lines = 200_000;
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    private readonly SqliteCentralApiFactory _factory;
    private readonly ITestOutputHelper _output;

    public PortalStockCatalogTimingTests(SqliteCentralApiFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task A_large_catalogue_loads_once_and_a_sale_reloads_it_quickly()
    {
        if (Environment.GetEnvironmentVariable("ERPBridge_RUN_TIMING") != "1") return;

        var (tenant, _) = await _factory.SeedTenantAsync($"TIMING-{Guid.NewGuid():N}"[..20], "Timing");
        var seq = 5_000_000L;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            db.ChangeTracker.AutoDetectChangesEnabled = false;
            for (var i = 0; i < Products; i++)
            {
                var code = "S" + i.ToString("D6");
                db.MobileRecords.Add(Row(tenant.Id, "stocks", code, ++seq, new { stockCode = code, name = "Ürün " + i, unit1 = "AD", mainGroupCode = "G" + (i % 40), brandCode = "M" + (i % 90) }));
                db.MobileRecords.Add(Row(tenant.Id, "inventory", code + "|1", ++seq, new { stockCode = code, warehouseNo = 1, quantity = i % 50 }));
                db.MobileRecords.Add(Row(tenant.Id, "prices", code + "|1", ++seq, new { stockCode = code, listNumber = 1, price = 10 + i % 300 }));
            }
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            for (var i = 0; i < Lines; i++)
            {
                db.MobileRecords.Add(Row(tenant.Id, "stockTransactions", "L" + i, ++seq, new
                {
                    id = "L" + i, erp = "MIKRO", stokKod = "S" + (i % Products).ToString("D6"),
                    tarih = new DateTime(2024, 1, 1).AddDays(i % 900).ToString("yyyy-MM-dd") + "T00:00:00",
                    cikisMiktar = 1, miktar = -1, birimFiyat = 10, tutar = 10, cariKod = "C" + (i % 500), faturaRecno = i, aciklama = "satış satırı",
                }));
                if (i % 20_000 == 19_999)
                {
                    await db.SaveChangesAsync();
                    db.ChangeTracker.Clear();
                }
            }
            await db.SaveChangesAsync();
        }

        var cache = _factory.Services.GetRequiredService<IMemoryCache>();
        async Task<TimeSpan> TimeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var watch = Stopwatch.StartNew();
            var catalog = await PortalStockCatalog.LoadAsync(db, cache, tenant.Id, CancellationToken.None);
            PortalStockCatalog.Search(catalog, Query(), DateOnly.FromDateTime(DateTime.Today));
            PortalStockCatalog.Facets(catalog);
            return watch.Elapsed;
        }

        GC.Collect();
        var memoryBefore = GC.GetTotalMemory(forceFullCollection: true);
        var first = await TimeAsync();
        var memoryAfter = GC.GetTotalMemory(forceFullCollection: true);
        var unchanged = await TimeAsync();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            db.MobileRecords.Add(Row(tenant.Id, "stockTransactions", "SALE-1", ++seq, new { id = "SALE-1", erp = "MIKRO", stokKod = "S000001", tarih = DateTime.Today.ToString("yyyy-MM-dd") + "T00:00:00", cikisMiktar = 1 }));
            await db.SaveChangesAsync();
        }
        var afterSale = await TimeAsync();

        _output.WriteLine($"{Products} ürün, {Lines} hareket: ilk {first.TotalMilliseconds:F0} ms, değişmemiş {unchanged.TotalMilliseconds:F0} ms, " +
                          $"tek satış sonrası {afterSale.TotalMilliseconds:F0} ms, bellek ~{(memoryAfter - memoryBefore) / 1_048_576} MB");
        afterSale.Should().BeLessThan(first);
    }

    private static StockQuery Query() => new(
        null, [], [], [], [], null, null, null, null, null, null, "all", null, null, "name", false, 1, 50);

    private static MobileRecord Row(Guid tenantId, string entity, string key, long seq, object payload) => new()
    {
        TenantId = tenantId, Entity = entity, RecordKey = key, UpdatedSeq = seq, PayloadJson = JsonSerializer.Serialize(payload, Web),
    };
}
