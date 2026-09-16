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
/// The company manager portal (Faz 41): one day of the company, each salesperson's work,
/// the route day, balances and stock — read from the same records the phones get.
/// Relational because the figures come from booked documents and projections.
/// </summary>
public sealed class PortalRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    // Documents cannot reach the server before they happen, so the business day is today.
    private static readonly DateOnly Today = PortalReports.BusinessDate(null, DateTimeOffset.UtcNow);
    private static readonly string Day = Today.ToString("yyyy-MM-dd");
    private static readonly string CashBookDay = Today.ToString("dd.MM.yyyy");
    private static readonly string Yesterday = Today.AddDays(-1).ToString("yyyy-MM-dd");
    private static readonly int TodayRouteDay = PortalReports.RouteDay(Today);
    private static readonly int OtherRouteDay = TodayRouteDay % 7 + 1;

    private readonly SqliteCentralApiFactory _factory;

    public PortalRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Only_administrators_and_managers_open_the_portal()
    {
        var c = await CompanyAsync(native: true);

        (await GetAsync(c.Ali, "/api/v1/portal/summary")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await (await GetAsync(c.Ali, "/api/v1/portal/summary")).Content.ReadAsStringAsync()).Should().Contain("PORTAL_REQUIRES_MANAGER");
        (await GetAsync(c.Sef, "/api/v1/portal/summary")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetAsync(c.Patron, "/api/v1/portal/summary")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _factory.CreateClient().GetAsync("/api/v1/portal/summary")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_manager_sees_only_the_company_in_the_token()
    {
        var mine = await CompanyAsync(native: true);
        var other = await CompanyAsync(native: true);
        await PostAsync(other, other.Ali, "sales_order", "OTHER-SO", Sale("OTHER-SO", 10, 100, Day + "T09:00:00"));

        var summary = await GetJsonAsync<PortalSummaryResponse>(mine.Patron, $"/api/v1/portal/summary?date={Day}");

        summary.Sales.Count.Should().Be(0, "another company's sale must never show");
    }

    [Fact]
    public async Task The_day_summary_counts_booked_money_documents_of_that_business_day()
    {
        var c = await CompanyAsync(native: true);
        await PostAsync(c, c.Ali, "sales_order", "SO-1", Sale("SO-1", 3, 150, Day + "T10:00:00"));
        // The cash book writes dd.MM.yyyy HH:mm.
        await PostAsync(c, c.Ali, "collection", "TAH-1", new { mobileDocumentId = "TAH-1", occurredAt = CashBookDay + " 15:30", counterparty = "Bakkal Ali", customerCode = "C-001", amount = 200, paymentType = "Nakit" });
        await PostAsync(c, c.Ali, "disbursement", "GID-1", new { mobileDocumentId = "GID-1", occurredAt = CashBookDay + " 16:00", counterparty = "Gider: Yakıt", amount = 30, paymentType = "Nakit" });
        // A purchase payment belongs to the purchase; a failed sale was never booked; yesterday is yesterday.
        await PostAsync(c, c.Ali, "disbursement", "ALIS-1", new { mobileDocumentId = "ALIS-1", occurredAt = CashBookDay + " 16:30", counterparty = "Bakkal Ali", amount = 500, paymentType = "Nakit", approvalKind = "purchase" });
        await PostAsync(c, c.Ali, "sales_order", "SO-BAD", new { mobileDocumentId = "SO-BAD", occurredAt = Day + "T11:00:00", counterparty = "Olmayan", amount = 999, lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 999 } } });
        await PostAsync(c, c.Ali, "sales_order", "SO-OLD", Sale("SO-OLD", 1, 150, Yesterday + "T10:00:00"));

        var summary = await GetJsonAsync<PortalSummaryResponse>(c.Patron, $"/api/v1/portal/summary?date={Day}");

        summary.DataSource.Should().Be("native");
        summary.Sales.Count.Should().Be(1);
        summary.Sales.Amount.Should().Be(450m);
        summary.Collections.Amount.Should().Be(200m);
        summary.Disbursements.Count.Should().Be(1);
        summary.Disbursements.Amount.Should().Be(30m);
    }

    [Fact]
    public async Task With_an_erp_documents_still_on_their_way_to_the_erp_count()
    {
        var c = await CompanyAsync(native: false);
        (await PostAsync(c, c.Ali, "sales_order", "ERP-SO", Sale("ERP-SO", 2, 100, Day + "T10:00:00"))).Status.Should().Be("Pending");

        var summary = await GetJsonAsync<PortalSummaryResponse>(c.Patron, $"/api/v1/portal/summary?date={Day}");

        summary.DataSource.Should().Be("erp");
        summary.Sales.Amount.Should().Be(200m);
    }

    [Fact]
    public async Task Activity_attributes_each_document_to_the_user_who_sent_it()
    {
        var c = await CompanyAsync(native: true);
        await PostAsync(c, c.Ali, "sales_order", "A-SO", Sale("A-SO", 3, 150, Day + "T10:00:00"));
        await PostAsync(c, c.Mehmet, "collection", "M-TAH", new { mobileDocumentId = "M-TAH", occurredAt = Day + "T12:00:00", counterparty = "Bakkal Ali", customerCode = "C-001", amount = 100, paymentType = "Nakit" });
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(c.Id, raw);
        await PostAsync(c, raw, "sales_order", "KEY-SO", Sale("KEY-SO", 1, 50, Day + "T13:00:00"));

        var activity = await GetJsonAsync<PortalActivityResponse>(c.Patron, $"/api/v1/portal/activity?from={Day}&to={Day}");

        activity.Users.Single(u => u.Username == "ali").Sales.Amount.Should().Be(450m);
        activity.Users.Single(u => u.Username == "mehmet").Collections.Amount.Should().Be(100m);
        var unknown = activity.Users.Single(u => u.UserId == null);
        unknown.Sales.Amount.Should().Be(50m, "an API key carries no person");
    }

    [Fact]
    public async Task The_route_day_joins_planned_stops_with_what_was_recorded()
    {
        var c = await CompanyAsync(native: true);
        await PostAsync(c, c.Patron, "route_plan", "RP-1", new
        {
            planId = "PLAN-1", name = "Pazartesi", isActive = true,
            stops = new[]
            {
                new { stopId = "S-1", dayOfWeek = TodayRouteDay, customerCode = "C-001", customerName = "Bakkal Ali", visitOrder = 1 },
                new { stopId = "S-2", dayOfWeek = TodayRouteDay, customerCode = "C-002", customerName = "Market Veli", visitOrder = 2 },
                new { stopId = "S-3", dayOfWeek = OtherRouteDay, customerCode = "C-003", customerName = "Başka gün", visitOrder = 1 },
            },
            assignees = new[] { "ali" },
        });
        await PostAsync(c, c.Ali, "visit", "V-1", new { visitId = "V-1", planId = "PLAN-1", stopId = "S-1", customerCode = "C-001", visitDate = Day, status = "COMPLETED", note = "Sipariş" });
        await PostAsync(c, c.Ali, "visit", "V-9", new { visitId = "V-9", customerCode = "C-009", visitDate = Day, status = "COMPLETED" });

        var visits = await GetJsonAsync<PortalVisitsResponse>(c.Patron, $"/api/v1/portal/visits?date={Day}");
        var summary = await GetJsonAsync<PortalSummaryResponse>(c.Patron, $"/api/v1/portal/summary?date={Day}");
        var activity = await GetJsonAsync<PortalActivityResponse>(c.Patron, $"/api/v1/portal/activity?from={Day}&to={Day}");

        visits.Rows.Where(r => r.Planned).Select(r => (r.StopId, r.Status)).Should().Equal(("S-1", "COMPLETED"), ("S-2", "PENDING"));
        visits.Rows.Single(r => !r.Planned).CustomerCode.Should().Be("C-009");
        visits.Rows.Single(r => r.StopId == "S-1").Note.Should().Be("Sipariş");
        summary.VisitsPlanned.Should().Be(2, "the other day's stop is not on today's route");
        summary.VisitsCompleted.Should().Be(2);
        activity.Users.Single(u => u.Username == "ali").VisitsCompleted.Should().Be(2);
    }

    [Fact]
    public async Task Balances_and_stock_match_what_the_phones_receive()
    {
        var c = await CompanyAsync(native: true);
        await PostAsync(c, c.Ali, "sales_order", "SO-B", Sale("SO-B", 3, 150, Day + "T10:00:00"));
        await PostAsync(c, c.Ali, "collection", "TAH-B", new { mobileDocumentId = "TAH-B", counterparty = "Bakkal Ali", customerCode = "C-001", amount = 200, paymentType = "Nakit" });

        var balances = await GetJsonAsync<PortalBalancesResponse>(c.Patron, "/api/v1/portal/balances");
        var stock = await GetJsonAsync<PortalStockResponse>(c.Patron, "/api/v1/portal/stock?search=çay");

        balances.Rows.Single(r => r.CustomerCode == "C-001").Balance.Should().Be(250m);
        balances.Rows.Single(r => r.CustomerCode == "C-001").Title.Should().Be("Bakkal Ali");
        balances.TotalReceivable.Should().Be(250m);
        stock.Rows.Single(r => r.StockCode == "CAY-1").Quantity.Should().Be(37m);
        (await GetJsonAsync<PortalStockResponse>(c.Patron, "/api/v1/portal/stock?outOfStock=true")).Rows.Should().NotContain(r => r.StockCode == "CAY-1");
    }

    [Fact]
    public async Task A_document_received_more_than_a_week_after_its_day_is_not_counted_for_it()
    {
        var c = await CompanyAsync(native: true);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            foreach (var (id, receivedDaysLater) in new[] { ("LATE-3", 3), ("LATE-10", 10) })
            {
                db.Jobs.Add(new Job
                {
                    TenantId = c.Id, ExternalId = id, DocumentType = "collection", Status = JobStatus.Succeeded,
                    PayloadJson = JsonSerializer.Serialize(new { occurredAt = Day + "T10:00:00", amount = 10 }),
                    EnqueuedAtUtc = new DateTimeOffset(Today.ToDateTime(new TimeOnly(7, 0)), TimeSpan.Zero).AddDays(receivedDaysLater),
                });
            }
            await db.SaveChangesAsync();
        }

        var summary = await GetJsonAsync<PortalSummaryResponse>(c.Patron, $"/api/v1/portal/summary?date={Day}");

        summary.Collections.Count.Should().Be(1);
    }

    [Fact]
    public async Task Bad_dates_and_ranges_are_refused()
    {
        var c = await CompanyAsync(native: true);

        (await GetAsync(c.Patron, "/api/v1/portal/summary?date=21.09.2026")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await GetAsync(c.Patron, "/api/v1/portal/activity?from=2026-09-21&to=2026-09-01")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await GetAsync(c.Patron, "/api/v1/portal/activity?from=2026-01-01&to=2026-09-01")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---- helpers -----------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Sef, string Ali, string Mehmet);

    private static object Sale(string id, int quantity, decimal unitPrice, string occurredAt) => new
    {
        mobileDocumentId = id,
        occurredAt,
        transactionType = "Satış",
        counterparty = "Bakkal Ali",
        customerCode = "C-001",
        amount = quantity * unitPrice,
        paymentType = "Cari Borç",
        lines = new[] { new { barcode = "8690000000011", productCode = "CAY-1", productTitle = "Çay 1 kg", quantity, unitPrice, lineTotal = quantity * unitPrice } },
    };

    private async Task<Company> CompanyAsync(bool native)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"PORTAL-{suffix}", $"Portal tenant {suffix}");
        var adminToken = await AdminTokenAsync();
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        if (native)
            (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("sef", "MANAGER"), ("ali", "SALES"), ("mehmet", "SALES") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;

        var company = new Company(tenant.Id,
            await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "sef", $"DEV-S-{suffix}"),
            await LoginAsync(code, "ali", $"DEV-A-{suffix}"), await LoginAsync(code, "mehmet", $"DEV-M-{suffix}"));

        // Documents are booked directly here; the approval centre has its own tests.
        var rules = ApprovalKinds.All.ToDictionary(kind => kind, _ => false);
        (await SendAsync(HttpMethod.Put, company.Patron, tenant.Id, "/api/v1/android/approvals/rules", new { rules })).StatusCode.Should().Be(HttpStatusCode.OK);
        if (native)
        {
            await PostAsync(company, company.Patron, "stock_card", "CARD-S1", new { stockCode = "CAY-1", name = "Çay 1 kg", barcode = "8690000000011", price = 150, openingQuantity = 40 });
            await PostAsync(company, company.Patron, "customer_card", "CARD-C1", new { customerCode = "C-001", title = "Bakkal Ali" });
        }
        return company;
    }

    private async Task<IngestJobResponse> PostAsync(Company c, string token, string documentType, string externalId, object payload)
    {
        var response = await SendAsync(HttpMethod.Post, token, c.Id, "/api/v1/ingest/jobs", new { externalId, documentType, payload });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        return await response.ReadAsJsonAsync<IngestJobResponse>();
    }

    private Task<HttpResponseMessage> GetAsync(string token, string path) => _factory.CreateClient().GetAsync(path, token);

    private async Task<T> GetJsonAsync<T>(string token, string path)
    {
        var response = await GetAsync(token, path);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }

    private async Task<string> AdminTokenAsync()
    {
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        return _factory.IssueAdminJwt(admin.Id);
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
