using System.Text.Json;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Tests.CustomerCatalog;

/// <summary>
/// GOAL_MUSTERI_KATALOGU §4 / S3: the catalog is built from the same stock records the phones receive — the category
/// the phone shows (Mikro sub-group name, the native card's <c>kategori</c>, else "Diğer"), the ERP carton, stock as the
/// phone rounds it, the price lists with their VAT flag — with the company's layout laid over it, and rebuilt only when
/// the stock or the layout revision moved.
/// </summary>
public sealed class CatalogViewRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    private readonly SqliteCentralApiFactory _factory;
    private long _seq = 3_000_000;

    public CatalogViewRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task An_erp_company_gets_the_phone_categories_cartons_stock_and_price_lists()
    {
        var tenantId = await TenantAsync();
        await SeedErpStockAsync(tenantId);

        var view = await LoadAsync(tenantId);

        view.Categories.Select(c => c.Key).Should().Equal("Çay", "Deterjan", "Diğer", "Kahve", "ZZ");
        view.Categories.Single(c => c.Key == "Çay").Products.Select(p => p.Code).Should().Equal("A2", "A");
        view.Categories.Single(c => c.Key == "Deterjan").Id.Should().Be(CatalogViewService.CategoryId("Deterjan")).And.MatchRegex("^[0-9a-f]{12}$");
        var a = view.Products["A"];
        a.Should().Match<CatalogProduct>(p => p.Name == "Çay Rize" && p.Unit == "KG" && p.Brand == "Doğuş" && p.VatRate == 10m);
        a.Barcodes.Should().Equal("8690001");
        a.Prices.Should().BeEquivalentTo(new Dictionary<int, decimal> { [1] = 100m, [2] = 90m });
        view.Products["A"].ErpCartonQuantity.Should().Be(12);
        view.Products["B"].ErpCartonQuantity.Should().Be(12, "\"12,0\" is a whole 12");
        view.Products["C"].ErpCartonQuantity.Should().BeNull("\"abc\" is no carton");
        view.Products["C"].CategoryKey.Should().Be("Deterjan", "the sub-group code 10 is read with its main group");
        view.Products["E"].CategoryKey.Should().Be("ZZ", "an unknown sub-group is shown by its code, as on the phone");
        view.Products["D"].CategoryKey.Should().Be("Diğer");
        view.Products.Values.Where(p => p.InStock).Select(p => p.Code).Order().Should().Equal("A", "C");
        view.Products["a"].Should().BeSameAs(a, "stock codes match case-insensitively");

        view.PriceLists.Should().Equal(new CatalogPriceList(1, "Perakende", true), new CatalogPriceList(2, "Toptan", false));
        view.Revision.Should().Be(0);
        view.IsEnabled.Should().BeFalse();
        view.DefaultPriceListNo.Should().BeNull();
        view.EffectiveDefaultPriceListNo.Should().Be(1);
        view.InMainCatalog(view.Products["E"]).Should().BeFalse("a product without a price in the company's list is not shown");
        view.InMainCatalog(a).Should().BeTrue();
    }

    [Fact]
    public async Task A_native_company_groups_by_the_cards_own_category()
    {
        var tenantId = await TenantAsync();
        await SeedAsync(tenantId, db =>
        {
            Add(db, tenantId, "stocks", "N1", new { stockCode = "N1", urunAd = "Ayran", birim = "AD", kategori = " İçecek ", kdvOrani = 1, koliAdet = "24" });
            Add(db, tenantId, "stocks", "N2", new { stockCode = "N2", urunAd = "Su", kategori = "İçecek" });
            Add(db, tenantId, "stocks", "N3", new { stockCode = "N3", urunAd = "Peçete" });
            Add(db, tenantId, "prices", "N1|2", new { stockCode = "N1", listNumber = 2, price = 15 });
            Add(db, tenantId, "prices", "N3|3", new { stockCode = "N3", listNumber = 3, price = 7 });
        });

        var view = await LoadAsync(tenantId);

        view.Categories.Select(c => (c.Key, c.Products.Count)).Should().Equal(("Diğer", 1), ("İçecek", 2));
        view.Products["N1"].ErpCartonQuantity.Should().Be(24);
        view.Products["N1"].VatRate.Should().Be(1m);
        view.PriceLists.Select(l => l.Name).Should().Equal("Liste 2", "Liste 3");
        view.EffectiveDefaultPriceListNo.Should().Be(2, "without list 1 the lowest list with prices");
    }

    [Fact]
    public async Task The_company_layout_orders_hides_and_flags_and_pictures_follow_their_order()
    {
        var tenantId = await TenantAsync();
        await SeedErpStockAsync(tenantId);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var file = new CatalogImage
        {
            TenantId = tenantId, StockCode = "A", SourceHash = "file", SortOrder = 1, HasSmall = true, HasLarge = true,
            Sha256Small = new string('a', 64), Sha256Large = new string('b', 64), SizeBytes = 10, CreatedAtMs = now,
        };
        var link = new CatalogImage
        {
            TenantId = tenantId, StockCode = "A", Kind = CatalogImageKinds.Link, Url = "https://cdn.example.com/a.jpg", SourceHash = "link",
            SortOrder = 0, CreatedAtMs = now,
        };
        var pending = new CatalogImage { TenantId = tenantId, StockCode = "A", SourceHash = "pending", SortOrder = 2, CreatedAtMs = now };
        await SeedAsync(tenantId, db =>
        {
            db.CatalogSettings.Add(new CatalogSettings { TenantId = tenantId, IsEnabled = true, DefaultPriceListNo = 2, Revision = 4, UpdatedAtMs = now });
            db.CatalogCategorySettings.AddRange(
                new CatalogCategorySetting { TenantId = tenantId, CategoryKey = "Kahve", SortOrder = 0, UpdatedAtMs = now },
                new CatalogCategorySetting { TenantId = tenantId, CategoryKey = "Deterjan", IsHidden = true, UpdatedAtMs = now });
            db.CatalogProductSettings.AddRange(
                new CatalogProductSetting { TenantId = tenantId, StockCode = "A", SortOrder = 0, NoDiscount = true, UpdatedAtMs = now },
                new CatalogProductSetting { TenantId = tenantId, StockCode = "B", CartonOnly = true, UpdatedAtMs = now },
                new CatalogProductSetting { TenantId = tenantId, StockCode = "C", CartonOnly = true, UpdatedAtMs = now },
                new CatalogProductSetting { TenantId = tenantId, StockCode = "D", CartonQuantity = 6, CartonOnly = true, IsHidden = true, UpdatedAtMs = now });
            db.CatalogImages.AddRange(file, link, pending);
        });

        var view = await LoadAsync(tenantId);

        view.Revision.Should().Be(4);
        view.IsEnabled.Should().BeTrue();
        view.EffectiveDefaultPriceListNo.Should().Be(2);
        view.Categories.Select(c => c.Key).Should().Equal("Kahve", "Çay", "Deterjan", "Diğer", "ZZ");
        view.Category("Deterjan")!.Hidden.Should().BeTrue();
        // An ordered product comes before the unordered ones.
        view.Category("Çay")!.Products.Select(p => p.Code).Should().Equal("A", "A2");

        var a = view.Products["A"];
        a.NoDiscount.Should().BeTrue();
        a.Pictures.Select(p => p.Id).Should().Equal(link.Id, file.Id);
        a.ThumbUrl.Should().Be("https://cdn.example.com/a.jpg");
        a.Pictures[1].ThumbUrl.Should().Be($"/api/v1/catalog/img/{file.Id}/s?h=aaaaaaaa");
        a.Pictures[1].FullUrl.Should().Be($"/api/v1/catalog/img/{file.Id}/l?h=bbbbbbbb");
        view.Products["B"].CartonOnly.Should().BeTrue("the ERP carton is 12");
        view.Products["C"].CartonOnly.Should().BeFalse("there is no carton to sell in");
        view.Products["D"].Should().Match<CatalogProduct>(p => p.CartonOnly && p.EffectiveCartonQuantity == 6 && p.Hidden);

        view.InMainCatalog(a).Should().BeTrue();
        view.InMainCatalog(view.Products["B"]).Should().BeFalse("B has no price in list 2");
        view.InMainCatalog(view.Products["C"]).Should().BeFalse("its category is hidden");
        view.InMainCatalog(view.Products["D"]).Should().BeFalse("it is hidden");
    }

    [Fact]
    public async Task The_view_is_rebuilt_only_when_the_stock_or_the_layout_revision_moves()
    {
        var tenantId = await TenantAsync();
        await SeedErpStockAsync(tenantId);
        var clock = new ManualClock(DateTimeOffset.UtcNow);
        var service = new CatalogViewService(new MemoryCache(new MemoryCacheOptions()), clock);

        // Two cold requests at once build it once.
        var cold = await Task.WhenAll(LoadAsync(tenantId, service), LoadAsync(tenantId, service));
        cold[1].Should().BeSameAs(cold[0]);
        var first = cold[0];
        (await LoadAsync(tenantId, service)).Should().BeSameAs(first, "nothing changed");

        // A layout write: the stock side is reused as it was.
        await SeedAsync(tenantId, db => db.CatalogSettings.Add(new CatalogSettings { TenantId = tenantId, Revision = 1, DefaultPriceListNo = 2 }));
        var relaid = await LoadAsync(tenantId, service);
        relaid.Should().NotBeSameAs(first);
        relaid.EffectiveDefaultPriceListNo.Should().Be(2);
        relaid.Stock.Should().BeSameAs(first.Stock);

        // A new card: staff see it at once; a customer within five seconds of the last look does not.
        await SeedAsync(tenantId, db => Add(db, tenantId, "stocks", "F", new { stockCode = "F", name = "Fındık", mainGroupCode = "GIDA", subGroupCode = "CAY" }));
        clock.Advance(TimeSpan.FromSeconds(2));
        (await LoadAsync(tenantId, service, forCustomer: true)).Products.Should().NotContainKey("F");
        clock.Advance(TimeSpan.FromSeconds(4));
        var refreshed = await LoadAsync(tenantId, service, forCustomer: true);
        refreshed.Products.Should().ContainKey("F");
        refreshed.Stock.Should().NotBeSameAs(first.Stock);

        await SeedAsync(tenantId, db => Add(db, tenantId, "stocks", "G", new { stockCode = "G", name = "Gofret" }));
        (await LoadAsync(tenantId, service, forCustomer: true)).Products.Should().NotContainKey("G");
        (await LoadAsync(tenantId, service)).Products.Should().ContainKey("G", "staff requests always bring the stock up to date");
    }

    [Theory]
    [InlineData("12", 12)]
    [InlineData("12,0", 12)]
    [InlineData(" 6 ", 6)]
    [InlineData("12.0", 12)]
    [InlineData("abc", null)]
    [InlineData("1", null)]
    [InlineData("0", null)]
    [InlineData("12,5", null)]
    [InlineData("-12", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void The_erp_carton_is_a_whole_number_above_one(string? text, int? expected) =>
        CatalogViewService.ErpCartonQuantity(text).Should().Be(expected);

    [Fact]
    public void Stock_is_rounded_per_warehouse_as_the_phone_does()
    {
        CatalogViewService.IsInStock(new Dictionary<int, decimal> { [1] = 0.5m }).Should().BeTrue();
        CatalogViewService.IsInStock(new Dictionary<int, decimal> { [1] = 0.4m }).Should().BeFalse();
        CatalogViewService.IsInStock(new Dictionary<int, decimal> { [1] = 0.4m, [2] = 0.4m }).Should().BeFalse("each warehouse rounds to 0");
        CatalogViewService.IsInStock(new Dictionary<int, decimal> { [1] = -2m, [2] = 3m }).Should().BeTrue();
        CatalogViewService.IsInStock(null).Should().BeFalse();
    }

    [Fact]
    public void The_category_key_is_the_trimmed_name_or_the_phones_fallback()
    {
        CatalogViewService.CategoryKey("  Çay  ").Should().Be("Çay");
        CatalogViewService.CategoryKey(null).Should().Be("Diğer");
        CatalogViewService.CategoryKey(" ").Should().Be("Diğer");
        CatalogViewService.CategoryKey(new string('x', 200)).Should().HaveLength(160);
        CatalogViewService.CategoryId("Çay").Should().Be(Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData("Çay"u8.ToArray()))[..12]);
    }

    private async Task SeedErpStockAsync(Guid tenantId) => await SeedAsync(tenantId, db =>
    {
        Add(db, tenantId, "lookups", "stock_sub_group|GIDA|CAY", new { kind = "stock_sub_group", code = "GIDA|CAY", name = "Çay", parentCode = "GIDA" });
        Add(db, tenantId, "lookups", "stock_sub_group|GIDA|KAHVE", new { kind = "stock_sub_group", code = "GIDA|KAHVE", name = "Kahve", parentCode = "GIDA" });
        Add(db, tenantId, "lookups", "stock_sub_group|TEMIZLIK|10", new { kind = "stock_sub_group", code = "TEMIZLIK|10", name = "Deterjan", parentCode = "TEMIZLIK" });
        Add(db, tenantId, "lookups", "stock_sub_group|BAKIM|10", new { kind = "stock_sub_group", code = "BAKIM|10", name = "Şampuan", parentCode = "BAKIM" });
        Add(db, tenantId, "lookups", "price_list|1", new { kind = "price_list", code = "1", name = "Perakende", includesVat = true });
        Add(db, tenantId, "lookups", "price_list|2", new { kind = "price_list", code = "2", name = "Toptan", includesVat = false });
        Add(db, tenantId, "lookups", "stock_brand|DOGUS", new { kind = "stock_brand", code = "DOGUS", name = "Doğuş" });
        Add(db, tenantId, "stocks", "A", new { stockCode = "A", name = "Çay Rize", unit1 = "KG", mainGroupCode = "GIDA", subGroupCode = "CAY", brandCode = "DOGUS", kdvOrani = 10, cartonCode = "12" });
        Add(db, tenantId, "stocks", "A2", new { stockCode = "A2", name = "Ada Çayı", mainGroupCode = "GIDA", subGroupCode = "CAY" });
        Add(db, tenantId, "stocks", "B", new { stockCode = "B", name = "Kahve Türk", mainGroupCode = "GIDA", subGroupCode = "KAHVE", cartonCode = "12,0" });
        Add(db, tenantId, "stocks", "C", new { stockCode = "C", name = "Yüzey Temizleyici", mainGroupCode = "TEMIZLIK", subGroupCode = "10", cartonCode = "abc" });
        Add(db, tenantId, "stocks", "D", new { stockCode = "D", name = "Poşet", mainGroupCode = "AMBALAJ" });
        Add(db, tenantId, "stocks", "E", new { stockCode = "E", name = "Eski Ürün", mainGroupCode = "X", subGroupCode = "ZZ" });
        Add(db, tenantId, "inventory", "A|1", new { stockCode = "A", warehouseNo = 1, quantity = 0.4 });
        Add(db, tenantId, "inventory", "A|2", new { stockCode = "A", warehouseNo = 2, quantity = 0.6 });
        Add(db, tenantId, "inventory", "B|1", new { stockCode = "B", warehouseNo = 1, quantity = 0.4 });
        Add(db, tenantId, "inventory", "C|1", new { stockCode = "C", warehouseNo = 1, quantity = 3 });
        Add(db, tenantId, "inventory", "E|1", new { stockCode = "E", warehouseNo = 1, quantity = -1 });
        Add(db, tenantId, "prices", "A|1", new { stockCode = "A", listNumber = 1, price = 100 });
        Add(db, tenantId, "prices", "A|2", new { stockCode = "A", listNumber = 2, price = 90 });
        Add(db, tenantId, "prices", "A2|1", new { stockCode = "A2", listNumber = 1, price = 40 });
        Add(db, tenantId, "prices", "B|1", new { stockCode = "B", listNumber = 1, price = 50 });
        Add(db, tenantId, "prices", "C|2", new { stockCode = "C", listNumber = 2, price = 20 });
        Add(db, tenantId, "prices", "D|1", new { stockCode = "D", listNumber = 1, price = 5 });
        Add(db, tenantId, "barcodes", "8690001", new { barcode = "8690001", stockCode = "A" });
    });

    private void Add(CentralApiDbContext db, Guid tenantId, string entity, string key, object payload) => db.MobileRecords.Add(new MobileRecord
    {
        TenantId = tenantId,
        Entity = entity,
        RecordKey = key,
        PayloadJson = JsonSerializer.Serialize(payload, Web),
        UpdatedSeq = Interlocked.Increment(ref _seq),
    });

    private async Task SeedAsync(Guid tenantId, Action<CentralApiDbContext> change)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        change(db);
        await db.SaveChangesAsync();
    }

    private async Task<CatalogView> LoadAsync(Guid tenantId, CatalogViewService? service = null, bool forCustomer = false)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return await (service ?? scope.ServiceProvider.GetRequiredService<CatalogViewService>()).LoadAsync(db, tenantId, forCustomer, CancellationToken.None);
    }

    private async Task<Guid> TenantAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var tenant = new Tenant { Name = $"Katalog {Guid.NewGuid():N}" };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        return tenant.Id;
    }
}
