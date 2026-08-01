using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

public class AndroidEndpointsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;
    public AndroidEndpointsTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Pull_with_mobile_device_token_returns_only_the_callers_latest_snapshot()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("ANDROID-PULL", "Android tenant");
        var (otherTenant, _) = await _factory.SeedTenantAsync("ANDROID-OTHER", "Other tenant");
        await _factory.SeedBootstrapPackageAsync(tenant.Id, "{\"customers\":[{\"code\":\"C001\"}],\"stocks\":[{\"code\":\"S001\"}]}");
        await _factory.SeedBootstrapPackageAsync(otherTenant.Id, "{\"customers\":[{\"code\":\"OTHER\"}]}");
        await AuthorizeMobileDeviceAsync(client, tenant.Id, "ANDROID-PULL-DEVICE");
        var response = await client.PostAsync("/api/v1/android/pull", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("C001").And.Contain("S001").And.NotContain("OTHER");
    }

    [Fact]
    public async Task Section_with_mobile_device_token_returns_requested_collection()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("ANDROID-SECTION", "Section tenant");
        await _factory.SeedBootstrapPackageAsync(tenant.Id, "{\"customers\":[{\"code\":\"C001\"}],\"stocks\":[{\"code\":\"S001\"}]}");
        await AuthorizeMobileDeviceAsync(client, tenant.Id, "ANDROID-SECTION-DEVICE");
        var response = await client.PostAsync("/api/v1/android/sync/urun", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("S001").And.NotContain("C001");
    }

    [Theory]
    [InlineData("/api/v1/android/sync/cari", "customers", "C002")]
    [InlineData("/api/v1/android/sync/urun", "stocks", "S002")]
    public async Task Catalog_sections_return_only_the_requested_page(
        string route,
        string collectionName,
        string expectedSecondPageCode)
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-PAGE-{suffix}", "Paged Android tenant");
        var payload = $$"""
            {
              "customers":[{"code":"C001"},{"code":"C002"},{"code":"C003"}],
              "stocks":[{"stockCode":"S001"},{"stockCode":"S002"},{"stockCode":"S003"}]
            }
            """;
        await _factory.SeedBootstrapPackageAsync(tenant.Id, payload);
        var device = await _factory.SeedMobileDeviceAsync(tenant.Id, $"ANDROID-PAGE-{suffix}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", _factory.IssueMobileJwt(device.Id, tenant.Id));

        var response = await client.PostAsJsonAsync(route, new { page = 2, pageSize = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        root.GetProperty("page").GetInt32().Should().Be(2);
        root.GetProperty("pageSize").GetInt32().Should().Be(1);
        root.GetProperty("total").GetInt32().Should().Be(3);
        root.GetProperty("items").GetArrayLength().Should().Be(1);
        root.GetProperty("items")[0].GetProperty(collectionName == "customers" ? "code" : "stockCode")
            .GetString().Should().Be(expectedSecondPageCode);
    }

    [Theory]
    [InlineData("/api/v1/android/sync/bankalar", "BANK-001", "Ana Banka", "BR-01", "TR001")]
    [InlineData("/api/v1/android/sync/kasalar", "CASH-001", "Merkez Kasa", "", "")]
    public async Task Cash_and_bank_endpoints_project_only_the_requested_account_kind(
        string route,
        string expectedCode,
        string expectedName,
        string expectedBranchOrEmpty,
        string expectedAccountOrEmpty)
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-CASH-BANK-{suffix}", "Cash and bank tenant");
        const string payload = """
            {
              "cashAndBank": [
                { "code":"CASH-001", "name":"Merkez Kasa", "kind":"cash", "currency":"0" },
                { "code":"BANK-001", "name":"Ana Banka", "kind":"bank", "branch":"BR-01", "accountNo":"TR001", "tcmbCode":"0062", "currency":"0" }
              ]
            }
            """;
        await _factory.SeedBootstrapPackageAsync(tenant.Id, payload);
        var device = await _factory.SeedMobileDeviceAsync(tenant.Id, $"ANDROID-CASH-BANK-{suffix}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", _factory.IssueMobileJwt(device.Id, tenant.Id));

        var response = await client.PostAsJsonAsync(route, new { page = 1, pageSize = 50 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        root.GetProperty("total").GetInt32().Should().Be(1);
        var item = root.GetProperty("items")[0];
        item.GetProperty("kod").GetString().Should().Be(expectedCode);
        item.GetProperty("isim").GetString().Should().Be(expectedName);
        if (expectedBranchOrEmpty.Length > 0)
        {
            item.GetProperty("sube").GetString().Should().Be(expectedBranchOrEmpty);
            item.GetProperty("hesapNumarasi").GetString().Should().Be(expectedAccountOrEmpty);
        }
    }

    [Fact]
    public async Task Product_catalog_joins_barcode_price_and_inventory_by_stock_code()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-PRODUCT-{suffix}", "Product catalog tenant");
        const string payload = """
            {
              "stocks": [{
                "stockCode":"S001",
                "name":"Joined product",
                "shelfCode":"R-12",
                "sectorCode":"ADET",
                "packageCode":"KUTU",
                "brandCode":"MARKA-1",
                "cartonCode":"24",
                "barcodes":[]
              }],
              "barcodes": [
                {"barcode":"869000000001","stockCode":"S001","unitPointer":1},
                {"barcode":"869000000002","stockCode":"S001","unitPointer":1}
              ],
              "prices": [
                {"stockCode":"S001","listNumber":2,"price":90.0},
                {"stockCode":"S001","listNumber":1,"price":125.5}
              ],
              "lookups": [
                {"kind":"price_list","code":"1","name":"SATIŞ FİYATI"},
                {"kind":"price_list","code":"2","name":"E-TİCARET"}
              ],
              "inventory": [
                {"stockCode":"S001","warehouseNo":1,"quantity":7.0},
                {"stockCode":"S001","warehouseNo":2,"quantity":3.0}
              ]
            }
            """;
        await _factory.SeedBootstrapPackageAsync(tenant.Id, payload);
        await AuthorizeMobileDeviceAsync(client, tenant.Id, $"ANDROID-PRODUCT-{suffix}");

        var response = await client.PostAsync("/api/v1/android/sync/urun", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var product = document.RootElement.GetProperty("items")[0];
        product.GetProperty("stockCode").GetString().Should().Be("S001");
        product.GetProperty("barkod").GetString().Should().Be("869000000001");
        product.GetProperty("barcodes").GetArrayLength().Should().Be(2);
        product.GetProperty("reyonKod").GetString().Should().Be("R-12");
        product.GetProperty("olcu").GetString().Should().Be("ADET");
        product.GetProperty("ambalaj").GetString().Should().Be("KUTU");
        product.GetProperty("marka").GetString().Should().Be("MARKA-1");
        product.GetProperty("koliAdet").GetString().Should().Be("24");
        product.GetProperty("satis_fiyati").GetDecimal().Should().Be(125.5m);
        product.GetProperty("customPrices").GetProperty("SATIŞ FİYATI").GetDecimal().Should().Be(125.5m);
        product.GetProperty("customPrices").GetProperty("E-TİCARET").GetDecimal().Should().Be(90m);
        product.GetProperty("stok").GetInt32().Should().Be(10);
        product.GetProperty("stockByWarehouse").GetProperty("Depo 1").GetInt32().Should().Be(7);
        product.GetProperty("stockByWarehouse").GetProperty("Depo 2").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task Price_list_definitions_return_erp_names()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-PRICE-NAMES-{suffix}", "Price names tenant");
        await _factory.SeedBootstrapPackageAsync(tenant.Id,
            """{"lookups":[{"kind":"price_list","code":"1","name":"SATIŞ FİYATI"},{"kind":"price_list","code":"2","name":"E-TİCARET"}]}""");
        await AuthorizeMobileDeviceAsync(client, tenant.Id, $"ANDROID-PRICE-NAMES-{suffix}");

        var response = await client.PostAsync("/api/v1/android/sync/stokSatisFiyatListeTanimlari", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("SATIŞ FİYATI").And.Contain("E-TİCARET");
    }

    [Theory]
    [InlineData("/api/v1/android/sync/cariAdresler", "ADDRESS-001")]
    [InlineData("/api/v1/android/sync/cariYetkililer", "CONTACT-001")]
    [InlineData("/api/v1/android/sync/barkodlar", "BARCODE-001")]
    [InlineData("/api/v1/android/sync/satisSartlari", "CONDITION-001")]
    public async Task New_section_with_mobile_device_token_returns_requested_collection(string route, string expectedMarker)
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-NEW-{suffix}", "Android new sections tenant");
        const string payload = """
            {
              "customerAddresses": [{"marker":"ADDRESS-001"}],
              "customerContacts": [{"marker":"CONTACT-001"}],
              "barcodes": [{"marker":"BARCODE-001"}],
              "salesConditions": [{"marker":"CONDITION-001"}]
            }
            """;
        await _factory.SeedBootstrapPackageAsync(tenant.Id, payload);
        await AuthorizeMobileDeviceAsync(client, tenant.Id, $"ANDROID-NEW-{suffix}");

        var response = await client.PostAsync(route, content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain(expectedMarker);
        foreach (var otherMarker in new[] { "ADDRESS-001", "CONTACT-001", "BARCODE-001", "CONDITION-001" }.Where(marker => marker != expectedMarker))
            body.Should().NotContain(otherMarker);
    }

    [Fact]
    public async Task Pull_with_an_api_key_is_unauthorized()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("ANDROID-FORBIDDEN", "Forbidden tenant");
        await _factory.SeedBootstrapPackageAsync(tenant.Id, "{\"customers\":[]}");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, "AK-ANDROID-FORBIDDEN", scopes: new[] { "ingest:write" });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenant.Id.ToString());
        var response = await client.PostAsync("/api/v1/android/pull", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/v1/android/sync/cariHareketleri", "customerTransactions", "CH-002")]
    [InlineData("/api/v1/android/sync/stokHareket", "stockTransactions", "SH-002")]
    public async Task Movement_section_returns_requested_page(string route, string section, string expectedMarker)
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-MOVE-{suffix}", "Movement tenant");
        var prefix = section == "customerTransactions" ? "CH" : "SH";
        var payload = $$"""
            { "{{section}}": [
              { "id": "{{prefix}}-001" },
              { "id": "{{prefix}}-002" },
              { "id": "{{prefix}}-003" }
            ] }
            """;
        await _factory.SeedBootstrapPackageAsync(tenant.Id, payload);
        await AuthorizeMobileDeviceAsync(client, tenant.Id, $"ANDROID-MOVE-{suffix}");

        var response = await client.PostAsJsonAsync(route, new { page = 2, pageSize = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain(expectedMarker).And.Contain("\"total\":3");
        body.Should().NotContain($"{prefix}-001").And.NotContain($"{prefix}-003");
    }

    [Fact]
    public async Task Stock_movement_section_filters_by_stock_code_and_orders_latest_first()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-STOCK-MOVE-{suffix}", "Stock movement tenant");
        const string payload = """
            {
              "stockTransactions": [
                {"id":"OLD","stokKod":"S001","updatedAt":"2026-01-01T10:00:00Z"},
                {"id":"OTHER","stokKod":"S999","updatedAt":"2026-07-01T10:00:00Z"},
                {"id":"NEW","stokKod":"S001","updatedAt":"2026-06-01T10:00:00Z"}
              ]
            }
            """;
        await _factory.SeedBootstrapPackageAsync(tenant.Id, payload);
        await AuthorizeMobileDeviceAsync(client, tenant.Id, $"ANDROID-STOCK-MOVE-{suffix}");

        var response = await client.PostAsJsonAsync(
            "/api/v1/android/sync/stokHareket",
            new { page = 1, pageSize = 50, since = "S001" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        root.GetProperty("total").GetInt32().Should().Be(2);
        var items = root.GetProperty("items").EnumerateArray().ToArray();
        items[0].GetProperty("id").GetString().Should().Be("NEW");
        items[1].GetProperty("id").GetString().Should().Be("OLD");
        (await response.Content.ReadAsStringAsync()).Should().NotContain("OTHER");
    }

    [Fact]
    public async Task Invoice_movement_returns_only_lines_linked_to_each_customer_transaction_recno()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-INVOICE-{suffix}", "Invoice tenant");
        const string payload = """
            {
              "stocks": [
                { "stockCode": "S001", "name": "Correct product one" },
                { "stockCode": "S002", "name": "Correct product two" },
                { "stockCode": "S999", "name": "Wrong customer product" }
              ],
              "customerTransactions": [
                { "erpRef": "CH-101", "erp": "MIKRO", "cariKod": "C001", "cha_recno": 101, "evrakNo": "FAT-1", "tutar": 100 },
                { "erpRef": "CH-102", "erp": "MIKRO", "cariKod": "C001", "cha_recno": 102, "evrakNo": "FAT-2", "tutar": 200 },
                { "erpRef": "CH-999", "erp": "MIKRO", "cariKod": "C999", "cha_recno": 999, "evrakNo": "FAT-999", "tutar": 999 }
              ],
              "stockTransactions": [
                { "erpRef": "SH-1", "stokKod": "S001", "faturaRecno": 101, "miktar": 1, "birimFiyat": 100 },
                { "erpRef": "SH-2", "stokKod": "S002", "faturaRecno": 102, "miktar": 2, "birimFiyat": 100 },
                { "erpRef": "SH-999", "stokKod": "S999", "faturaRecno": 999, "miktar": 9, "birimFiyat": 111 },
                { "erpRef": "SH-STRAY", "stokKod": "S999", "faturaRecno": 777, "miktar": 7, "birimFiyat": 77 }
              ]
            }
            """;
        await _factory.SeedBootstrapPackageAsync(tenant.Id, payload);
        await AuthorizeMobileDeviceAsync(client, tenant.Id, $"ANDROID-INVOICE-{suffix}");

        var response = await client.PostAsJsonAsync(
            "/api/v1/android/sync/faturaHareket",
            new { page = 1, pageSize = 200, since = "C001" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        root.GetProperty("total").GetInt32().Should().Be(2);
        var invoices = root.GetProperty("items").EnumerateArray().ToArray();
        var firstLines = invoices.Single(item => item.GetProperty("erpRef").GetString() == "CH-101")
            .GetProperty("satirlar").EnumerateArray().ToArray();
        var secondLines = invoices.Single(item => item.GetProperty("erpRef").GetString() == "CH-102")
            .GetProperty("satirlar").EnumerateArray().ToArray();

        firstLines.Should().ContainSingle();
        firstLines[0].GetProperty("erpRef").GetString().Should().Be("SH-1");
        firstLines[0].GetProperty("stokAd").GetString().Should().Be("Correct product one");
        firstLines[0].GetProperty("sth_fat_recid_recno").GetInt32().Should().Be(101);
        secondLines.Should().ContainSingle();
        secondLines[0].GetProperty("erpRef").GetString().Should().Be("SH-2");
        (await response.Content.ReadAsStringAsync()).Should().NotContain("SH-999").And.NotContain("SH-STRAY");
    }

    [Fact]
    public async Task Pull_with_the_short_tenant_header_and_mobile_device_token_returns_data()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("ANDROID-SHORT-ID", "Short-id tenant");
        await _factory.SeedBootstrapPackageAsync(tenant.Id, "{\"customers\":[{\"code\":\"C001\"}]}");
        await AuthorizeMobileDeviceAsync(client, tenant.Id, "ANDROID-SHORT-ID-DEVICE");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenant.Id.ToString("N")[..8]);

        var response = await client.PostAsync("/api/v1/android/pull", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("C001");
    }

    [Fact]
    public async Task Pull_with_a_different_tenant_device_cannot_read_another_tenants_snapshot()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("ANDROID-WRONG-SHORT-ID", "Wrong short-id tenant");
        var (otherTenant, _) = await _factory.SeedTenantAsync("ANDROID-OTHER-DEVICE", "Other device tenant");
        await _factory.SeedBootstrapPackageAsync(tenant.Id, "{\"customers\":[{\"code\":\"PRIVATE\"}]}");
        await AuthorizeMobileDeviceAsync(client, otherTenant.Id, "ANDROID-OTHER-DEVICE");

        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenant.Id.ToString("N")[..8]);

        var response = await client.PostAsync("/api/v1/android/pull", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().NotContain("PRIVATE");
    }

    private async Task AuthorizeMobileDeviceAsync(HttpClient client, Guid tenantId, string installationId)
    {
        var device = await _factory.SeedMobileDeviceAsync(tenantId, installationId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", _factory.IssueMobileJwt(device.Id, tenantId));
    }
}
