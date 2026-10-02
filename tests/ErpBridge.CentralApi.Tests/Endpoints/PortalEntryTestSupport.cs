using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// A company for the panel's document entry (GOAL_PANEL_GIRIS): an ADMIN (patron), a MANAGER (mudur) and an ACCOUNTING
/// user (muhasebe) signed in to the panel, a SALES user (ali) signed in to the phone, and the manager on the phone too.
/// </summary>
internal sealed record EntryCompany(
    Guid Id, string Code, string AdminToken,
    string Patron, string Mudur, string Muhasebe, string AliPhone, string MudurPhone,
    Guid PatronId, Guid MudurId, Guid AliId);

internal static class PortalEntryTestSupport
{
    public const string Base = "/api/v1/portal/entry";

    public static async Task<EntryCompany> EntryCompanyAsync(SqliteCentralApiFactory factory, bool native = false)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await factory.SeedTenantAsync($"PG-{suffix}", $"Giriş firması {suffix}");
        var admin = await factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = factory.IssueAdminJwt(admin.Id);
        var client = factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";
        if (native)
            (await client.PutJsonAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PutJsonAsync($"{basePath}/subscription", new { seats = 10, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var ids = new Dictionary<string, Guid>();
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("mudur", "MANAGER"), ("ali", "SALES"), ("muhasebe", "ACCOUNTING") })
        {
            var created = await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username + " bey", password = Password, roles = new[] { role } }, adminToken);
            created.StatusCode.Should().Be(HttpStatusCode.Created, await created.Content.ReadAsStringAsync());
            ids[username] = (await created.ReadAsJsonAsync<MobileUserDto>()).Id;
        }
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        return new EntryCompany(
            tenant.Id, code, adminToken,
            await LoginAsync(factory, code, "patron", $"web-portal:patron-{suffix}", "portal"),
            await LoginAsync(factory, code, "mudur", $"web-portal:mudur-{suffix}", "portal"),
            await LoginAsync(factory, code, "muhasebe", $"web-portal:muhasebe-{suffix}", "portal"),
            await LoginAsync(factory, code, "ali", $"DEV-A-{suffix}", "android"),
            await LoginAsync(factory, code, "mudur", $"DEV-M-{suffix}", "android"),
            ids["patron"], ids["mudur"], ids["ali"]);
    }

    /// <summary>
    /// An ERP company's data as the agent sends it: lists 1 (VAT included) and 2; A "Çay Rize" 10 % VAT, 5 in stock, 100 in
    /// list 1 and 90 in list 2; B "Kahve Türk" with no VAT on its card, 50 in list 1, none in stock; customers C1 and C2;
    /// warehouses 1 and 3; bank 13. Company settings: invoice, warehouse 1, ERP user 1 unless <paramref name="defaultErpUser"/>
    /// is false; Ali's mapping: ERP user 7, warehouse 3, salesperson P1.
    /// </summary>
    public static Task SeedErpAsync(SqliteCentralApiFactory factory, EntryCompany company, bool defaultErpUser = true) => SeedAsync(factory, db =>
    {
        var id = company.Id;
        Record(db, id, "lookups", "price_list|1", new { kind = "price_list", code = "1", name = "Perakende", includesVat = true });
        Record(db, id, "lookups", "price_list|2", new { kind = "price_list", code = "2", name = "Bayi" });
        Record(db, id, "lookups", "warehouse|1", new { kind = "warehouse", code = "1", name = "Merkez" });
        Record(db, id, "lookups", "warehouse|3", new { kind = "warehouse", code = "3", name = "Şube" });
        Record(db, id, "lookups", "bank|13", new { kind = "bank", code = "13", name = "Ziraat POS" });
        Record(db, id, "lookups", "cash|001", new { kind = "cash", code = "001", name = "Merkez kasa" });
        Record(db, id, "stocks", "A", new { stockCode = "A", name = "Çay Rize", unit1 = "KG", kdvOrani = 10 });
        Record(db, id, "stocks", "B", new { stockCode = "B", name = "Kahve Türk", unit1 = "AD" });
        Record(db, id, "inventory", "A|1", new { stockCode = "A", warehouseNo = 1, quantity = 5 });
        Record(db, id, "prices", "A|1", new { stockCode = "A", listNumber = 1, price = 100 });
        Record(db, id, "prices", "A|2", new { stockCode = "A", listNumber = 2, price = 90 });
        Record(db, id, "prices", "B|1", new { stockCode = "B", listNumber = 1, price = 50 });
        Record(db, id, "barcodes", "8690001", new { barcode = "8690001", stockCode = "A" });
        Record(db, id, "customers", "C1", new { customerCode = "C1", title1 = "Yılmaz Market", title2 = "Ltd. Şti.", salespersonCode = "P1" });
        Record(db, id, "customers", "C2", new { customerCode = "C2", title1 = "Ak Gıda" });
        db.ErpWriteSettings.Add(new ErpWriteSettings
        {
            TenantId = id,
            SalesDocumentKind = SalesDocumentKinds.Invoice,
            DefaultWarehouseNo = 1,
            DefaultErpUserNo = defaultErpUser ? 1 : null,
            DefaultCashCode = "001",
            DefaultCardBankCode = "13",
            DefaultTransferBankCode = "13",
        });
        db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = company.AliId, TenantId = id, SalespersonCode = "P1", ErpUserNo = 7, WarehouseNo = 3 });
    });

    /// <summary>A company without an ERP: products and a customer opened from the panel, as its admin would.</summary>
    public static async Task SeedNativeAsync(SqliteCentralApiFactory factory, EntryCompany company)
    {
        var client = factory.CreateClient();
        foreach (var card in new object[]
        {
            new { stockCode = "A", name = "Çay Rize", unit = "KG", vatRate = 10m, price = 100m, openingQuantity = 5m },
            new { stockCode = "B", name = "Kahve Türk", unit = "AD", vatRate = 20m, price = 50m, openingQuantity = 0m },
        })
            (await client.PostJsonAsync("/api/v1/portal/native/stock-cards", card, company.Patron)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await client.PostJsonAsync("/api/v1/portal/native/customer-cards", new { customerCode = "C1", title = "Bakkal Ali", openingBalance = 0m }, company.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>Sets one of a user's permissions for this person (the editor's override).</summary>
    public static Task SetPermissionAsync(SqliteCentralApiFactory factory, Guid userId, string key, string value) => SeedAsync(factory, db =>
        db.MobileUserPermissionOverrides.Add(new MobileUserPermissionOverride { UserId = userId, Key = key, Value = value }));

    public static Task<HttpResponseMessage> PostAsync(SqliteCentralApiFactory factory, string path, string token, object body) =>
        SendAsync(factory, HttpMethod.Post, Base + path, token, body);

    public static Task<HttpResponseMessage> GetAsync(SqliteCentralApiFactory factory, string path, string token) =>
        SendAsync(factory, HttpMethod.Get, Base + path, token);

    private static async Task<string> LoginAsync(SqliteCentralApiFactory factory, string code, string username, string deviceId, string client)
    {
        var response = await factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.300", client });
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }
}
