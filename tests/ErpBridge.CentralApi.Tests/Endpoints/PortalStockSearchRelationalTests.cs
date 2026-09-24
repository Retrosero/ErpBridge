using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// The stock page (panel goal P2): every product filtered, sorted and paged on the server,
/// from the same stock records the phones receive — Mikro's field names and the native
/// cards' alike.
/// </summary>
public sealed class PortalStockSearchRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private static readonly DateOnly Today = PortalReports.BusinessDate(null, DateTimeOffset.UtcNow);

    private readonly SqliteCentralApiFactory _factory;

    public PortalStockSearchRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task An_erp_company_filters_sorts_and_pages_every_product()
    {
        var c = await CompanyAsync(native: false);
        await SeedErpStockAsync(c.Id);

        var all = await SearchAsync(c.Patron, "");
        all.Total.Should().Be(3);
        all.Items.Select(i => i.StockCode).Should().Equal("A", "B", "C");
        all.PriceList.Should().Be(1);
        all.Summary.Should().BeEquivalentTo(new PortalStockSummary { Products = 3, InStock = 1, OutOfStock = 2, Negative = 1 });
        var a = all.Items[0];
        a.Should().Match<PortalStockItem>(i => i.MainGroup == "GIDA" && i.SubGroup == "CAY" && i.Brand == "DOGUS" && i.Shelf == "R1" && i.Unit == "KG");
        a.Quantity.Should().Be(15m);
        a.Reserved.Should().Be(2m);
        a.Price.Should().Be(100m);
        a.Barcodes.Should().Equal("8690001");
        a.LastMovementDate.Should().Be(Today.AddDays(-3).ToString("yyyy-MM-dd"));
        a.Warehouses.Select(w => (w.Name, w.Quantity)).Should().Equal(("Merkez", 10m), ("Şube", 5m));
        a.Prices.Select(p => (p.Name, p.Price)).Should().Equal(("Perakende", 100m), ("Toptan", 90m));

        (await SearchAsync(c.Patron, "q=8690001")).Items.Select(i => i.StockCode).Should().Equal("A");
        (await SearchAsync(c.Patron, "q=kahve")).Items.Select(i => i.StockCode).Should().Equal("B");
        (await SearchAsync(c.Patron, "mainGroup=GIDA")).Total.Should().Be(2);
        (await SearchAsync(c.Patron, "brand=DOGUS&brand=kurukahveci")).Total.Should().Be(2);
        (await SearchAsync(c.Patron, "subGroup=KAHVE&shelf=R2")).Items.Select(i => i.StockCode).Should().Equal("B");

        var branch = await SearchAsync(c.Patron, "warehouse=2");
        branch.Items.Should().ContainSingle().Which.Quantity.Should().Be(5m);

        (await SearchAsync(c.Patron, "status=negative")).Items.Select(i => i.StockCode).Should().Equal("C");
        (await SearchAsync(c.Patron, "status=out")).Items.Select(i => i.StockCode).Should().Equal("B", "C");
        (await SearchAsync(c.Patron, "status=in")).Items.Select(i => i.StockCode).Should().Equal("A");
        (await SearchAsync(c.Patron, "status=below&below=0")).Items.Select(i => i.StockCode).Should().Equal("B", "C");
        (await SearchAsync(c.Patron, "minQty=0&maxQty=10")).Items.Select(i => i.StockCode).Should().Equal("B");

        (await SearchAsync(c.Patron, "sort=price&dir=desc")).Items.Select(i => i.StockCode).Should().Equal("A", "B", "C");
        (await SearchAsync(c.Patron, "sort=price")).Items.Select(i => i.StockCode).Should().Equal("B", "A", "C"); // a product without a price sorts last
        (await SearchAsync(c.Patron, "sort=qty&dir=desc")).Items.Select(i => i.StockCode).Should().Equal("A", "B", "C");
        (await SearchAsync(c.Patron, "sort=brand")).Items.Select(i => i.StockCode).Should().Equal("A", "B", "C");
        var toptan = await SearchAsync(c.Patron, "priceList=2&minPrice=50");
        toptan.Items.Select(i => (i.StockCode, i.Price)).Should().Equal(("A", (decimal?)90m));

        (await SearchAsync(c.Patron, "idleDays=30")).Items.Select(i => i.StockCode).Should().Equal("C");

        var page = await SearchAsync(c.Patron, "pageSize=2&page=2");
        page.Total.Should().Be(3);
        page.Items.Select(i => i.StockCode).Should().Equal("C");
        page.Summary.Products.Should().Be(3, "the summary covers every match, not the page");

        var facets = await GetJsonAsync<PortalStockFacetsResponse>(c.Patron, "/api/v1/portal/stock/facets");
        facets.MainGroups.Select(f => (f.Code, f.Count)).Should().Equal(("GIDA", 2), ("TEMIZLIK", 1));
        facets.SubGroups.Select(f => (f.Code, f.Parent)).Should().Equal(("CAY", "GIDA"), ("KAHVE", "GIDA"));
        facets.Brands.Select(f => f.Code).Should().Equal("DOGUS", "KURUKAHVECI");
        facets.Warehouses.Select(w => (w.Number, w.Name)).Should().Equal((1, "Merkez"), (2, "Şube"));
        facets.PriceLists.Select(p => (p.Number, p.Name)).Should().Equal((1, "Perakende"), (2, "Toptan"));
        facets.HasMovementDates.Should().BeTrue();
        facets.HasReserved.Should().BeTrue();
    }

    [Fact]
    public async Task Bad_filters_are_refused_and_a_salesperson_is_kept_out()
    {
        var c = await CompanyAsync(native: false);

        foreach (var query in new[] { "status=gone", "sort=colour", "dir=up", "idleDays=-1" })
        {
            var response = await _factory.CreateClient().GetAsync("/api/v1/portal/stock/search?" + query, c.Patron);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest, query);
            (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_QUERY");
        }
        (await _factory.CreateClient().GetAsync("/api/v1/portal/stock/search", c.Ali)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _factory.CreateClient().GetAsync("/api/v1/portal/stock/facets", c.Ali)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await SearchAsync(c.Patron, "pageSize=5000")).PageSize.Should().Be(250);
    }

    [Fact]
    public async Task A_native_company_reads_its_cards_and_sees_new_products_and_sales_at_once()
    {
        var c = await CompanyAsync(native: true);
        await PostAsync(c, c.Patron, "stock_card", "CARD-S1", new { stockCode = "CAY-1", name = "Çay 1 kg", barcode = "8690000000011", price = 150, openingQuantity = 40, category = "İçecek", brand = "Doğuş", aisle = "A-3", unit = "Paket" });
        await PostAsync(c, c.Patron, "customer_card", "CARD-C1", new { customerCode = "C-001", title = "Bakkal Ali" });

        var first = await SearchAsync(c.Patron, "");
        first.Items.Should().ContainSingle().Which.Should().Match<PortalStockItem>(i =>
            i.MainGroup == "İçecek" && i.Brand == "Doğuş" && i.Shelf == "A-3" && i.Unit == "Paket" && i.Quantity == 40m && i.Price == 150m);

        // A new card and a sale change the stock records; the cached catalogue must not hide them.
        await PostAsync(c, c.Patron, "stock_card", "CARD-S2", new { stockCode = "SEKER-1", name = "Şeker 1 kg", price = 40, openingQuantity = 0 });
        await PostAsync(c, c.Ali, "sales_order", "SO-1", new
        {
            mobileDocumentId = "SO-1", occurredAt = Today.ToString("yyyy-MM-dd") + "T10:00:00", counterparty = "Bakkal Ali", customerCode = "C-001",
            amount = 450, paymentType = "Cari Borç",
            lines = new[] { new { productCode = "CAY-1", productTitle = "Çay 1 kg", quantity = 3, unitPrice = 150, lineTotal = 450 } },
        });

        var second = await SearchAsync(c.Patron, "sort=code");
        second.Items.Select(i => (i.StockCode, i.Quantity)).Should().Equal(("CAY-1", 37m), ("SEKER-1", 0m));
        var facets = await GetJsonAsync<PortalStockFacetsResponse>(c.Patron, "/api/v1/portal/stock/facets");
        facets.HasMovementDates.Should().BeTrue("booked sale lines are movements");
        second.Items[0].LastMovementDate.Should().Be(Today.ToString("yyyy-MM-dd"));
        second.Items[1].LastMovementDate.Should().BeNull("the new card has not moved");
        facets.Warehouses.Should().ContainSingle();

        // A deleted card leaves the list; only the rows changed since the last read are applied.
        await PostAsync(c, c.Patron, "stock_card_delete", "DEL-S2", new { stockCode = "SEKER-1" });
        (await SearchAsync(c.Patron, "")).Items.Select(i => i.StockCode).Should().Equal("CAY-1");
    }

    // ---- setup ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Ali);

    [Fact]
    public async Task A_new_removed_or_redated_movement_moves_the_last_movement_date_without_rebuilding_the_stock_side()
    {
        var c = await CompanyAsync(native: false);
        await SeedErpStockAsync(c.Id);
        var cache = _factory.Services.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
        async Task<PortalStockCatalog.Catalog> LoadAsync()
        {
            using var scope = _factory.Services.CreateScope();
            return await PortalStockCatalog.LoadAsync(scope.ServiceProvider.GetRequiredService<CentralApiDbContext>(), cache, c.Id, CancellationToken.None);
        }
        async Task ChangeAsync(Action<CentralApiDbContext> change)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            change(db);
            await db.SaveChangesAsync();
        }
        string? LastOf(PortalStockCatalog.Catalog catalog, string code) =>
            catalog.Products.Single(p => p.Code == code).LastMovement?.ToString("yyyy-MM-dd");
        string Day(int daysAgo) => Today.AddDays(-daysAgo).ToString("yyyy-MM-dd");

        var before = await LoadAsync();
        LastOf(before, "C").Should().Be(Day(400));
        (await LoadAsync()).Should().BeSameAs(before, "nothing changed");

        // A sale: one new line. The stock side (cards, prices, quantities) is reused as it was.
        await ChangeAsync(db => db.MobileRecords.Add(new MobileRecord
        {
            TenantId = c.Id, Entity = "stockTransactions", RecordKey = "903", UpdatedSeq = 1_100_001,
            PayloadJson = JsonSerializer.Serialize(new { id = "903", erp = "MIKRO", stokKod = "c", tarih = Day(10) + "T00:00:00", cikisMiktar = 1 }, Web),
        }));
        var afterSale = await LoadAsync();
        LastOf(afterSale, "C").Should().Be(Day(10));
        afterSale.Products.Single(p => p.Code == "A").Should().BeSameAs(before.Products.Single(p => p.Code == "A"));
        (await SearchAsync(c.Patron, "q=Yüzey")).Items.Single().LastMovementDate.Should().Be(Day(10));

        // The line is deleted: the date goes back to the older lines.
        await ChangeAsync(db =>
        {
            var row = db.MobileRecords.Single(r => r.TenantId == c.Id && r.Entity == "stockTransactions" && r.RecordKey == "903");
            row.IsDeleted = true;
            row.UpdatedSeq = 1_100_002;
        });
        LastOf(await LoadAsync(), "C").Should().Be(Day(400));

        // An older line is re-dated.
        await ChangeAsync(db =>
        {
            var row = db.MobileRecords.Single(r => r.TenantId == c.Id && r.Entity == "stockTransactions" && r.RecordKey == "902");
            row.PayloadJson = JsonSerializer.Serialize(new { id = "902", erp = "MIKRO", stokKod = "C", tarih = Day(5) + "T00:00:00", cikisMiktar = 1 }, Web);
            row.UpdatedSeq = 1_100_003;
        });
        LastOf(await LoadAsync(), "C").Should().Be(Day(5));

        // The page asks for search and facets at once: a changed catalogue is built once, not twice.
        await ChangeAsync(db => db.MobileRecords.Add(new MobileRecord
        {
            TenantId = c.Id, Entity = "stockTransactions", RecordKey = "904", UpdatedSeq = 1_100_004,
            PayloadJson = JsonSerializer.Serialize(new { id = "904", erp = "MIKRO", stokKod = "B", tarih = Day(1) + "T00:00:00", cikisMiktar = 1 }, Web),
        }));
        var both = await Task.WhenAll(LoadAsync(), LoadAsync());
        both[0].Should().BeSameAs(both[1]);
        LastOf(both[0], "B").Should().Be(Day(1));
        PortalStockCatalog.Facets(both[0]).Should().BeSameAs(PortalStockCatalog.Facets(both[1]), "facets are worked out once per catalogue");
    }

    private async Task SeedErpStockAsync(Guid tenantId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var seq = 1_000_000L;
        void Add(string entity, string key, string? stockKey, object payload) => db.MobileRecords.Add(new MobileRecord
        {
            TenantId = tenantId,
            Entity = entity,
            RecordKey = key,
            StockKey = stockKey,
            PayloadJson = JsonSerializer.Serialize(payload, Web),
            UpdatedSeq = ++seq,
        });

        Add("stocks", "A", "A", new { stockCode = "A", name = "Çay Rize 1 kg", unit1 = "KG", mainGroupCode = "GIDA", subGroupCode = "CAY", brandCode = "DOGUS", shelfCode = "R1" });
        Add("stocks", "B", "B", new { stockCode = "B", name = "Kahve Türk 100 g", unit1 = "AD", mainGroupCode = "GIDA", subGroupCode = "KAHVE", brandCode = "KURUKAHVECI", shelfCode = "R2" });
        Add("stocks", "C", "C", new { stockCode = "C", name = "Yüzey Temizleyici", unit1 = "AD", mainGroupCode = "TEMIZLIK" });
        Add("inventory", "A|1", "A", new { stockCode = "A", warehouseNo = 1, quantity = 10, reservedQuantity = 2, lastMovementDate = Today.AddDays(-3).ToString("yyyy-MM-dd") });
        Add("inventory", "A|2", "A", new { stockCode = "A", warehouseNo = 2, quantity = 5, reservedQuantity = 0, lastMovementDate = Today.AddDays(-40).ToString("yyyy-MM-dd") });
        Add("inventory", "B|1", "B", new { stockCode = "B", warehouseNo = 1, quantity = 0, reservedQuantity = 0 });
        Add("inventory", "C|1", "C", new { stockCode = "C", warehouseNo = 1, quantity = -3, reservedQuantity = 0, lastMovementDate = (string?)null });
        // Mikro's reader sends no last-movement date; the movement mirror carries it.
        Add("stockTransactions", "901", null, new { id = "901", erp = "MIKRO", stokKod = "C", tarih = Today.AddDays(-400).ToString("yyyy-MM-dd") + "T00:00:00", cikisMiktar = 1, miktar = -1 });
        Add("stockTransactions", "902", null, new { id = "902", erp = "MIKRO", stokKod = "C", tarih = Today.AddDays(-500).ToString("yyyy-MM-dd") + "T00:00:00", cikisMiktar = 1, miktar = -1 });
        // A warehouse known only from the lookups holds nothing and is not offered.
        Add("lookups", "warehouse|9", null, new { kind = "warehouse", code = "9", name = "Boş depo" });
        Add("prices", "A|1", "A", new { stockCode = "A", listNumber = 1, price = 100 });
        Add("prices", "A|2", "A", new { stockCode = "A", listNumber = 2, price = 90 });
        Add("prices", "B|1", "B", new { stockCode = "B", listNumber = 1, price = 50 });
        Add("prices", "C|2", "C", new { stockCode = "C", listNumber = 2, price = 20 });
        Add("barcodes", "8690001", "A", new { barcode = "8690001", stockCode = "A" });
        Add("lookups", "warehouse|1", null, new { kind = "warehouse", code = "1", name = "Merkez" });
        Add("lookups", "warehouse|2", null, new { kind = "warehouse", code = "2", name = "Şube" });
        Add("lookups", "price_list|1", null, new { kind = "price_list", code = "1", name = "Perakende" });
        Add("lookups", "price_list|2", null, new { kind = "price_list", code = "2", name = "Toptan" });
        await db.SaveChangesAsync();
    }

    private Task<PortalStockSearchResponse> SearchAsync(string token, string query) =>
        GetJsonAsync<PortalStockSearchResponse>(token, "/api/v1/portal/stock/search" + (query.Length == 0 ? "" : "?" + query));

    private async Task<Company> CompanyAsync(bool native)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"STOCK-{suffix}", $"Stock tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        if (native)
            (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("ali", "SALES") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        var company = new Company(tenant.Id, await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "ali", $"DEV-A-{suffix}"));

        var rules = ApprovalKinds.All.ToDictionary(kind => kind, _ => false);
        (await SendAsync(HttpMethod.Put, company.Patron, tenant.Id, "/api/v1/android/approvals/rules", new { rules })).StatusCode.Should().Be(HttpStatusCode.OK);
        return company;
    }

    private async Task PostAsync(Company c, string token, string documentType, string externalId, object payload)
    {
        var response = await SendAsync(HttpMethod.Post, token, c.Id, "/api/v1/ingest/jobs", new { externalId, documentType, payload });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded");
    }

    private async Task<T> GetJsonAsync<T>(string token, string path)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "portal-test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string token, Guid tenantId, string path, object body)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return await _factory.CreateClient().SendAsync(request);
    }

    private async Task<HttpResponseMessage> PutAsync(string path, object value, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(value, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }
}
