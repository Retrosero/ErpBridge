using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.ErpWrite;

/// <summary>
/// Goal ERP yazım Y1b: the portal endpoints where a company administrator sets how phone documents are
/// written into the ERP, per company and per user, and picks ERP codes from the data the agent sent.
/// </summary>
public sealed class PortalErpWriteEndpointsTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private readonly SqliteCentralApiFactory _factory;

    public PortalErpWriteEndpointsTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Only_a_company_administrator_reads_or_changes_the_settings()
    {
        var c = await CompanyAsync(native: false);

        foreach (var token in new[] { c.Sef, c.Ali })
        {
            var response = await SendAsync(HttpMethod.Get, token, "/api/v1/portal/erp-settings");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            (await response.Content.ReadAsStringAsync()).Should().Contain("ADMIN_REQUIRED");
            (await SendAsync(HttpMethod.Put, token, "/api/v1/portal/erp-settings", Settings())).StatusCode.Should().Be(HttpStatusCode.Forbidden);
            (await SendAsync(HttpMethod.Get, token, "/api/v1/portal/erp-lookups")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        (await _factory.CreateClient().GetAsync("/api/v1/portal/erp-settings")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_company_without_an_erp_has_no_erp_settings()
    {
        var c = await CompanyAsync(native: true);

        var response = await SendAsync(HttpMethod.Get, c.Patron, "/api/v1/portal/erp-settings");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync()).Should().Contain("ERP_NOT_CONNECTED");
    }

    [Fact]
    public async Task Saved_settings_come_back_trimmed_and_reach_the_database()
    {
        var c = await CompanyAsync(native: false);
        (await ReadAsync<PortalErpWriteSettingsDto>(await SendAsync(HttpMethod.Get, c.Patron, "/api/v1/portal/erp-settings")))
            .Should().Match<PortalErpWriteSettingsDto>(s => s.SalesDocumentKind == "order" && s.ChequePortfolioCode == "ÇEK" && s.UpdatedAtUtc == null);

        var saved = await ReadAsync<PortalErpWriteSettingsDto>(await SendAsync(HttpMethod.Put, c.Patron, "/api/v1/portal/erp-settings", Settings()));

        saved.SalesDocumentKind.Should().Be("invoice");
        saved.Series.Invoice.Should().Be("T");
        saved.Series.Order.Should().BeEmpty("a blank series is series-less");
        saved.DefaultCashCode.Should().Be("001");
        saved.DefaultSalespersonCode.Should().BeNull("a blank code is no code");
        saved.UpdatedAtUtc.Should().NotBeNull();
        await using var db = _factory.CreateDbContext();
        (await db.ErpWriteSettings.SingleAsync(s => s.TenantId == c.Id)).Should().Match<ErpWriteSettings>(s =>
            s.SalesDocumentKind == "invoice" && s.InvoiceSeries == "T" && s.DefaultWarehouseNo == 1 && s.DeliveryDayOffset == 2);
    }

    [Theory]
    [InlineData("salesDocumentKind", "proforma")]
    [InlineData("orderApprovalMode", "maybe")]
    [InlineData("series", "ABCDEFG")]
    [InlineData("defaultCashCode", "12345678901234567890123456")]
    [InlineData("chequePortfolioCode", " ")]
    [InlineData("defaultErpUserNo", 40000)]
    [InlineData("defaultWarehouseNo", 0)]
    [InlineData("deliveryDayOffset", -1)]
    public async Task Settings_the_erp_cannot_take_are_refused(string field, object value)
    {
        var c = await CompanyAsync(native: false);
        var body = Settings();
        body[field] = field == "series" ? new Dictionary<string, object?> { ["invoice"] = value } : value;

        var response = await SendAsync(HttpMethod.Put, c.Patron, "/api/v1/portal/erp-settings", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("INVALID_ERP_SETTINGS");
    }

    [Fact]
    public async Task A_user_mapping_keeps_null_for_the_company_value_and_empty_for_series_less()
    {
        var c = await CompanyAsync(native: false);
        var ali = await UserIdAsync(c, "ali");
        var path = $"/api/v1/portal/users/{ali}/erp-mapping";
        (await ReadAsync<PortalUserErpMappingDto>(await SendAsync(HttpMethod.Get, c.Patron, path)))
            .Should().Match<PortalUserErpMappingDto>(m => m.UserId == ali && m.CashCode == null && m.Series.Invoice == null);

        var saved = await ReadAsync<PortalUserErpMappingDto>(await SendAsync(HttpMethod.Put, c.Patron, path, new
        {
            salespersonCode = " PLS01 ", warehouseNo = 3, cashCode = "", erpUserNo = 4,
            series = new { invoice = "", collection = "ALI" },
        }));

        saved.Should().Match<PortalUserErpMappingDto>(m =>
            m.SalespersonCode == "PLS01" && m.WarehouseNo == 3 && m.CashCode == null && m.ErpUserNo == 4
            && m.Series.Invoice == "" && m.Series.Collection == "ALI" && m.Series.Order == null);
        (await SendAsync(HttpMethod.Put, c.Patron, path, new { series = new { order = "TOOLONG" } })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await SendAsync(HttpMethod.Put, c.Patron, path, new { erpUserNo = -1 })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Another_company_s_user_or_an_unknown_one_is_not_found()
    {
        var mine = await CompanyAsync(native: false);
        var other = await CompanyAsync(native: false);
        var theirs = await UserIdAsync(other, "ali");

        foreach (var id in new[] { theirs, Guid.NewGuid() })
        {
            (await SendAsync(HttpMethod.Get, mine.Patron, $"/api/v1/portal/users/{id}/erp-mapping")).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await SendAsync(HttpMethod.Put, mine.Patron, $"/api/v1/portal/users/{id}/erp-mapping", new { cashCode = "001" })).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        await using var db = _factory.CreateDbContext();
        (await db.MobileUserErpMappings.AnyAsync(m => m.UserId == theirs)).Should().BeFalse();
    }

    [Fact]
    public async Task Lookups_list_the_erp_codes_the_agent_sent()
    {
        var c = await CompanyAsync(native: false);
        await using (var db = _factory.CreateDbContext())
        {
            var seq = 1000L;
            void Add(string entity, string kind, string code, string name, bool deleted = false) => db.MobileRecords.Add(new MobileRecord
            {
                TenantId = c.Id, Entity = entity, RecordKey = $"{kind}|{code}", UpdatedSeq = seq++, IsDeleted = deleted,
                PayloadJson = JsonSerializer.Serialize(new { kind, code, name }),
            });
            Add("lookups", "warehouse", "10", "Şube");
            Add("lookups", "warehouse", "2", "Depo 2");
            Add("lookups", "price_list", "1", "Perakende");
            Add("lookups", "salesperson", "PLS01", "Ali Veli");
            Add("lookups", "project", "P1", "Proje");
            Add("cashAndBank", "cash", "001", "Merkez kasa");
            Add("cashAndBank", "bank", "14", "Ziraat");
            Add("cashAndBank", "bank", "99", "Kapanmış", deleted: true);
            await db.SaveChangesAsync();
        }

        var lookups = await ReadAsync<PortalErpLookupsResponse>(await SendAsync(HttpMethod.Get, c.Patron, "/api/v1/portal/erp-lookups"));

        lookups.Warehouses.Select(w => w.Code).Should().Equal("2", "10");
        lookups.CashAccounts.Should().ContainSingle(a => a.Code == "001" && a.Name == "Merkez kasa");
        lookups.Banks.Select(b => b.Code).Should().Equal("14");
        lookups.Salespersons.Should().ContainSingle(s => s.Code == "PLS01");
        lookups.PriceLists.Should().ContainSingle(p => p.Code == "1");
        lookups.Projects.Should().ContainSingle(p => p.Code == "P1");
    }

    // ---- helpers --------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Code, string Patron, string Sef, string Ali);

    private static Dictionary<string, object?> Settings() => new()
    {
        ["salesDocumentKind"] = "Invoice",
        ["orderApprovalMode"] = "approved",
        ["series"] = new Dictionary<string, object?> { ["invoice"] = " T ", ["order"] = "" },
        ["defaultWarehouseNo"] = 1,
        ["defaultCashCode"] = "001",
        ["defaultCardBankCode"] = "14",
        ["defaultTransferBankCode"] = "04",
        ["defaultErpUserNo"] = 1,
        ["defaultSalespersonCode"] = "  ",
        ["defaultPriceListNo"] = 1,
        ["chequePortfolioCode"] = "ÇEK",
        ["notePortfolioCode"] = "SENET",
        ["deliveryDayOffset"] = 2,
    };

    private async Task<Company> CompanyAsync(bool native)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ERPSET-{suffix}", $"Erp settings tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        if (native)
            (await AdminPutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await AdminPutAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("sef", "MANAGER"), ("ali", "SALES") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        return new Company(tenant.Id, code,
            await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "sef", $"DEV-S-{suffix}"), await LoginAsync(code, "ali", $"DEV-A-{suffix}"));
    }

    private async Task<Guid> UserIdAsync(Company c, string username)
    {
        var users = await ReadAsync<MobileUserListResponse>(await SendAsync(HttpMethod.Get, c.Patron, "/api/v1/android/account/users"));
        return users.Users.Single(u => u.Username == username).Id;
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "portal-test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string token, string path, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }

    private async Task<HttpResponseMessage> AdminPutAsync(string path, object value, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(value, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }
}
