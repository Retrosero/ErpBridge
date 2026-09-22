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
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_PANEL_ERPSIZ E3c: GET /api/v1/portal/payments — every collection/payment across the
/// company (not one customer's statement), with date range, kind/customer/user filters and the
/// cash-box summary (daily and payment-type totals).
/// </summary>
public sealed class PortalPaymentsRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    // Computed fresh on each read, not a `static readonly` field frozen at type load — a run
    // straddling midnight could then disagree with the server's own "today". Matches the endpoint's
    // own default-range computation (PortalEndpoints.PaymentsAsync), which is Istanbul business time.
    private static DateOnly Today => PortalReports.BusinessDate(null, DateTimeOffset.UtcNow);

    // A movement's own stored date (booking.Stamp) is raw UTC, and PortalLedger.Payments's DailyTotals
    // groups by that raw date — not the Istanbul business day <see cref="Today"/> uses for the
    // endpoint's default range. The two agree except in the few hours a UTC day and an Istanbul (UTC+3)
    // day disagree, which is exactly the gap this property exists to be honest about.
    private static DateOnly MovementDay => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly SqliteCentralApiFactory _factory;

    public PortalPaymentsRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Lists_collections_and_disbursements_across_every_customer_with_the_creator_attributed()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", "Bakkal Ali", 0m);
        await SeedCustomerAsync(c, "C-002", "Market Veli", 0m);

        (await PostAsync(c, "collections", new { customerCode = "C-001", amount = 300, paymentType = "Nakit" })).StatusCode.Should().Be(HttpStatusCode.Created);
        (await PostAsync(c, "disbursements", new { customerCode = "C-002", amount = 120, paymentType = "EFT / Havale", description = "Kasım gideri" })).StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await GetJsonAsync<PortalPaymentsResponse>(c.Patron, "/api/v1/portal/payments");

        response.Items.Should().HaveCount(2);
        response.TotalCredit.Should().Be(300m);
        response.TotalDebit.Should().Be(120m);
        var patronId = await UserIdAsync(c.Id, "patron");
        response.Items.Should().OnlyContain(i => i.UserId == patronId && i.UserName == "patron");

        var collection = response.Items.Single(i => i.Kind == "collection");
        collection.Should().Match<PortalPaymentRow>(i => i.CustomerCode == "C-001" && i.CustomerTitle == "Bakkal Ali" && i.PaymentType == "Nakit" && i.Credit == 300m);

        var payment = response.Items.Single(i => i.Kind == "payment");
        payment.Should().Match<PortalPaymentRow>(i => i.CustomerCode == "C-002" && i.PaymentType == "EFT / Havale" && i.Description == "Kasım gideri" && i.Debit == 120m);
    }

    [Fact]
    public async Task Defaults_to_the_current_month_and_widens_with_an_explicit_from()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", "Bakkal Ali", 0m);
        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100 });
        await PostIngestAsync(c, "collection", "OLD-1", new
        {
            mobileDocumentId = "OLD-1", occurredAt = "2020-01-15T10:00:00", counterparty = "Bakkal Ali", customerCode = "C-001", amount = 50, paymentType = "Nakit",
        });

        var thisMonth = await GetJsonAsync<PortalPaymentsResponse>(c.Patron, "/api/v1/portal/payments");
        thisMonth.Items.Should().ContainSingle().Which.Debit.Should().Be(0m);
        thisMonth.From.Should().Be(new DateOnly(Today.Year, Today.Month, 1).ToString("yyyy-MM-dd"));

        var widened = await GetJsonAsync<PortalPaymentsResponse>(c.Patron, "/api/v1/portal/payments?from=2020-01-01");
        widened.Items.Should().HaveCount(2);
        widened.TotalCredit.Should().Be(150m);
    }

    [Fact]
    public async Task Filters_by_kind_customer_and_user_and_totals_by_day_and_payment_type()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", "Bakkal Ali", 0m);
        await SeedCustomerAsync(c, "C-002", "Market Veli", 0m);
        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 300, paymentType = "Nakit" });
        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 200, paymentType = "EFT / Havale" });
        await PostAsync(c, "disbursements", new { customerCode = "C-002", amount = 90, paymentType = "Nakit" });
        // Booked by "ali" (SALES) directly against the ingest queue, so the user filter has two creators to tell apart.
        await PostIngestAsync(c, "disbursement", "DISB-ALI", new { mobileDocumentId = "DISB-ALI", counterparty = "Market Veli", customerCode = "C-002", amount = 40, paymentType = "Nakit" }, c.Ali);

        (await GetJsonAsync<PortalPaymentsResponse>(c.Patron, "/api/v1/portal/payments?kind=collection")).Items.Should().HaveCount(2).And.OnlyContain(i => i.Kind == "collection");
        (await GetJsonAsync<PortalPaymentsResponse>(c.Patron, "/api/v1/portal/payments?kind=payment")).Items.Should().HaveCount(2).And.OnlyContain(i => i.Kind == "payment");
        (await GetJsonAsync<PortalPaymentsResponse>(c.Patron, "/api/v1/portal/payments?customer=C-002")).Items.Should().HaveCount(2).And.OnlyContain(i => i.CustomerCode == "C-002");

        var aliId = await UserIdAsync(c.Id, "ali");
        var byAli = await GetJsonAsync<PortalPaymentsResponse>(c.Patron, $"/api/v1/portal/payments?userId={aliId}");
        byAli.Items.Should().ContainSingle().Which.Should().Match<PortalPaymentRow>(i => i.CustomerCode == "C-002" && i.Debit == 40m);

        var all = await GetJsonAsync<PortalPaymentsResponse>(c.Patron, "/api/v1/portal/payments");
        all.Items.Should().HaveCount(4);
        all.DailyTotals.Should().ContainSingle().Which.Should().Match<PortalPaymentGroupTotal>(t => t.Key == MovementDay.ToString("yyyy-MM-dd") && t.Credit == 500m && t.Debit == 130m);
        all.PaymentTypeTotals.Should().Contain(t => t.Key == "Nakit" && t.Credit == 300m && t.Debit == 130m);
        all.PaymentTypeTotals.Should().Contain(t => t.Key == "EFT / Havale" && t.Credit == 200m && t.Debit == 0m);
    }

    [Fact]
    public async Task An_erp_agent_row_is_listed_without_a_resolvable_user()
    {
        var c = await CompanyAsync();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            db.MobileRecords.Add(new MobileRecord
            {
                TenantId = c.Id, Entity = "customerTransactions", RecordKey = "mikro-1", UpdatedSeq = 5_000_001,
                PayloadJson = JsonSerializer.Serialize(new
                {
                    id = "mikro-1", erp = "MIKRO", cariKod = "C-001", tarih = Today.ToString("yyyy-MM-dd") + "T00:00:00", tutar = 250, borcMu = false, type = "TAHSILAT",
                }, Web),
            });
            await db.SaveChangesAsync();
        }

        var response = await GetJsonAsync<PortalPaymentsResponse>(c.Patron, "/api/v1/portal/payments");

        response.Items.Should().ContainSingle().Which.Should().Match<PortalPaymentRow>(i => i.UserId == null && i.UserName == null && i.Credit == 250m);
    }

    [Fact]
    public async Task A_salesperson_cannot_read_the_company_wide_list()
    {
        var c = await CompanyAsync();
        (await _factory.CreateClient().GetAsync("/api/v1/portal/payments", c.Ali)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task An_invalid_kind_or_range_is_rejected()
    {
        var c = await CompanyAsync();
        await ExpectAsync(c.Patron, "/api/v1/portal/payments?kind=sale", HttpStatusCode.BadRequest, "INVALID_QUERY");
        await ExpectAsync(c.Patron, "/api/v1/portal/payments?from=2026-02-01&to=2026-01-01", HttpStatusCode.BadRequest, "INVALID_RANGE");
    }

    // ---- setup ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Ali);

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"PMT-{suffix}", $"Payments tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("ali", "SALES") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var overview = await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();
        var company = new Company(tenant.Id, await LoginAsync(overview.TenantCode!, "patron", $"DEV-P-{suffix}"), await LoginAsync(overview.TenantCode!, "ali", $"DEV-A-{suffix}"));

        var rules = ApprovalKinds.All.ToDictionary(kind => kind, _ => false);
        (await SendAsync(HttpMethod.Put, company.Patron, tenant.Id, "/api/v1/android/approvals/rules", new { rules })).StatusCode.Should().Be(HttpStatusCode.OK);
        return company;
    }

    private async Task SeedCustomerAsync(Company c, string code, string title, decimal openingBalance) =>
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/customer-cards", new { customerCode = code, title, openingBalance }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

    private Task<HttpResponseMessage> PostAsync(Company c, string kind, object body) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/{kind}", body, c.Patron);

    private async Task PostIngestAsync(Company c, string documentType, string externalId, object payload, string? token = null)
    {
        var response = await SendAsync(HttpMethod.Post, token ?? c.Patron, c.Id, "/api/v1/ingest/jobs", new { externalId, documentType, payload });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded");
    }

    private async Task<Guid> UserIdAsync(Guid tenantId, string username)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return (await db.MobileUsers.SingleAsync(u => u.TenantId == tenantId && u.Username == username)).Id;
    }

    private async Task<T> GetJsonAsync<T>(string token, string path)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }

    private async Task ExpectAsync(string token, string path, HttpStatusCode status, string errorCode)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(status, path);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be(errorCode, path);
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
