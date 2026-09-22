using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_PANEL_ERPSIZ E7b/D5: every write made from the portal to a native tenant's own books gets an
/// audit row (who, when, what, before/after) — a card's "Geçmiş" and the company-wide /denetim list.
/// </summary>
public sealed class PortalNativeAuditRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public PortalNativeAuditRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Creating_and_editing_a_stock_card_leaves_a_before_after_trail()
    {
        var c = await CompanyAsync();

        var create = await PostAsync(c, "stock-cards", new { stockCode = "CAY-1", name = "Çay 1 kg", price = 150, openingQuantity = 10 });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var edit = await PostAsync(c, "stock-cards", new { stockCode = "CAY-1", name = "Çay 1 kg (500 gr)", price = 180 });
        edit.StatusCode.Should().Be(HttpStatusCode.Created);

        var history = await GetJsonAsync<PortalAuditResponse>(c.Patron, "/api/v1/portal/native/audit?entity=stock_card&key=CAY-1");

        history.Total.Should().Be(2);
        history.Items[0].Should().Match<PortalAuditRow>(a => a.Action == "edit" && a.Summary.Contains("düzenlendi") && a.Summary.Contains("CAY-1"));
        JsonDocument.Parse(history.Items[0].BeforeJson!).RootElement.GetProperty("name").GetString().Should().Be("Çay 1 kg");
        JsonDocument.Parse(history.Items[0].BeforeJson!).RootElement.GetProperty("price").GetDecimal().Should().Be(150);
        JsonDocument.Parse(history.Items[0].AfterJson!).RootElement.GetProperty("name").GetString().Should().Be("Çay 1 kg (500 gr)");
        history.Items[1].Should().Match<PortalAuditRow>(a => a.Action == "create" && a.BeforeJson == null);
        history.Items.Should().OnlyContain(a => a.UserName == "patron");
    }

    [Fact]
    public async Task Deleting_a_stock_card_records_its_last_state_as_before_and_no_after()
    {
        var c = await CompanyAsync();
        await PostAsync(c, "stock-cards", new { stockCode = "CAY-1", name = "Çay 1 kg", price = 150 });

        var response = await _factory.CreateClient().SendAsync(WithAuth(new HttpRequestMessage(HttpMethod.Delete, "/api/v1/portal/native/stock-cards/CAY-1"), c.Patron));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var history = await GetJsonAsync<PortalAuditResponse>(c.Patron, "/api/v1/portal/native/audit?entity=stock_card&key=CAY-1");
        var delete = history.Items.Single(a => a.Action == "delete");
        delete.AfterJson.Should().BeNull();
        JsonDocument.Parse(delete.BeforeJson!).RootElement.GetProperty("name").GetString().Should().Be("Çay 1 kg");
    }

    [Fact]
    public async Task A_collection_and_a_disbursement_are_recorded_under_their_own_entity()
    {
        var c = await CompanyAsync();
        await PostAsync(c, "customer-cards", new { customerCode = "C-001", title = "Bakkal Ali" });

        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 300 });
        await PostAsync(c, "disbursements", new { customerCode = "C-001", amount = 50 });

        var collections = await GetJsonAsync<PortalAuditResponse>(c.Patron, "/api/v1/portal/native/audit?entity=collection&key=C-001");
        collections.Items.Should().ContainSingle().Which.Summary.Should().Contain("Tahsilat").And.Contain("300,00");
        var disbursements = await GetJsonAsync<PortalAuditResponse>(c.Patron, "/api/v1/portal/native/audit?entity=disbursement&key=C-001");
        disbursements.Items.Should().ContainSingle().Which.Summary.Should().Contain("Tediye").And.Contain("50,00");
    }

    [Fact]
    public async Task An_idempotent_replay_does_not_add_a_second_audit_row()
    {
        var c = await CompanyAsync();

        await PostAsync(c, "stock-cards", new { stockCode = "CAY-1", name = "Çay 1 kg", operationId = "OP-1" });
        var retry = await PostAsync(c, "stock-cards", new { stockCode = "CAY-1", name = "Çay 1 kg", operationId = "OP-1" });
        retry.StatusCode.Should().Be(HttpStatusCode.OK, "the second call is an idempotent replay, not a new write");

        var history = await GetJsonAsync<PortalAuditResponse>(c.Patron, "/api/v1/portal/native/audit?entity=stock_card&key=CAY-1");
        history.Total.Should().Be(1);
    }

    [Fact]
    public async Task The_company_wide_list_spans_every_entity_and_is_bounded_to_a_date_range()
    {
        var c = await CompanyAsync();
        await PostAsync(c, "stock-cards", new { stockCode = "CAY-1", name = "Çay 1 kg" });
        await PostAsync(c, "customer-cards", new { customerCode = "C-001", title = "Bakkal Ali" });
        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100 });

        var all = await GetJsonAsync<PortalAuditResponse>(c.Patron, "/api/v1/portal/native/audit");
        all.Total.Should().Be(3);
        all.Items.Select(i => i.Entity).Should().Contain(["stock_card", "customer_card", "collection"]);

        var pastRange = await GetJsonAsync<PortalAuditResponse>(c.Patron, "/api/v1/portal/native/audit?from=2020-01-01&to=2020-01-02");
        pastRange.Total.Should().Be(0, "the write happened today, outside a 2020 window");
    }

    [Fact]
    public async Task A_manager_cannot_read_the_audit_trail()
    {
        var c = await CompanyAsync();
        (await _factory.CreateClient().GetAsync("/api/v1/portal/native/audit", c.Manager)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_key_without_an_entity_is_rejected()
    {
        var c = await CompanyAsync();
        await ExpectAsync(c.Patron, "/api/v1/portal/native/audit?key=CAY-1", HttpStatusCode.BadRequest);
    }

    // ---- setup ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Manager);

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"AUD-{suffix}", $"Audit tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("yonetici", "MANAGER") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;

        return new Company(tenant.Id, await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "yonetici", $"DEV-M-{suffix}"));
    }

    private Task<HttpResponseMessage> PostAsync(Company c, string kind, object body) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/{kind}", body, c.Patron);

    private async Task<T> GetJsonAsync<T>(string token, string path)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }

    private async Task ExpectAsync(string token, string path, HttpStatusCode status)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(status, path);
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "portal-test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
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

    private static HttpRequestMessage WithAuth(HttpRequestMessage request, string token)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
