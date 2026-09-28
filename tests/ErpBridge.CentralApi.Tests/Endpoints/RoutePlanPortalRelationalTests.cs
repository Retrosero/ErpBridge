using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Targets;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Route plans from the web panel (GOAL_HEDEF_RUT P4–P5, K14): the same <c>route_plan</c> document a phone sends, the team
/// scope on planning (panel and phone alike), and how well the plan was followed.
/// </summary>
public sealed class RoutePlanPortalRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private static readonly DateOnly Today = TargetService.Today();
    private static readonly int TodayRouteDay = PortalReports.RouteDay(Today);
    private static readonly int YesterdayRouteDay = PortalReports.RouteDay(Today.AddDays(-1));

    private readonly SqliteCentralApiFactory _factory;

    public RoutePlanPortalRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_plan_saved_in_the_panel_reaches_the_phones_like_one_planned_on_a_phone()
    {
        var c = await CompanyAsync();
        var saved = await SendJsonAsync<RoutePlanDto>(HttpMethod.Put, c.Patron, "/api/v1/portal/routes/plan-1", Plan("Pazartesi rutu", ["ali"],
            (TodayRouteDay, "C-001", "Bakkal Ali"), (TodayRouteDay, "C-002", "Market Veli")));

        saved.Stops.Should().HaveCount(2).And.OnlyContain(s => s.StopId.Length > 0, "a stop made in the panel gets its id on the server");
        saved.CanEdit.Should().BeTrue();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var record = db.MobileRecords.Single(r => r.TenantId == c.Id && r.Entity == "routePlans" && r.RecordKey == "plan-1");
            record.PayloadJson.Should().Contain("\"assignees\":[\"ali\"]").And.Contain("\"updatedBy\":\"patron\"");
        }
        var list = await GetJsonAsync<RoutePlansResponse>(c.Patron, "/api/v1/portal/routes");
        list.Plans.Single().Stops.Select(s => s.CustomerCode).Should().Equal("C-001", "C-002");
        list.People.Select(p => p.Username).Should().Contain(["ali", "mehmet", "sef"]);

        // Saving again keeps the stop ids, so visits recorded on them stay joined.
        var again = await SendJsonAsync<RoutePlanDto>(HttpMethod.Put, c.Patron, "/api/v1/portal/routes/plan-1", new RoutePlanDto
        {
            Name = "Pazartesi rutu", IsActive = true, Assignees = ["ali"], Stops = saved.Stops,
        });
        again.Stops.Select(s => s.StopId).Should().Equal(saved.Stops.Select(s => s.StopId));

        (await SendAsync(HttpMethod.Delete, c.Patron, "/api/v1/portal/routes/plan-1", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await GetJsonAsync<RoutePlansResponse>(c.Patron, "/api/v1/portal/routes")).Plans.Should().BeEmpty();
    }

    [Fact]
    public async Task A_manager_responsible_for_a_team_plans_only_for_its_people_on_the_panel_and_the_phone()
    {
        var c = await CompanyAsync();
        await SendJsonAsync<TeamDto>(HttpMethod.Post, c.Patron, "/api/v1/portal/teams",
            new { name = "Anadolu", kind = "TEAM", memberIds = new[] { c.AliId }, managerIds = new[] { c.SefId } }, HttpStatusCode.Created);
        await SendJsonAsync<RoutePlanDto>(HttpMethod.Put, c.Patron, "/api/v1/portal/routes/other", Plan("Mehmet'in rutu", ["mehmet"], (TodayRouteDay, "C-9", "Büfe")));

        (await SendJsonAsync<RoutePlanDto>(HttpMethod.Put, c.Sef, "/api/v1/portal/routes/mine", Plan("Ali", ["ali"], (TodayRouteDay, "C-1", "Bakkal"))))
            .PlanId.Should().Be("mine");
        var refused = await SendAsync(HttpMethod.Put, c.Sef, "/api/v1/portal/routes/wide", Plan("İkisi", ["ali", "mehmet"], (TodayRouteDay, "C-1", "Bakkal")));
        refused.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await refused.ReadAsJsonAsync<ApiError>()).Message.Should().Contain("mehmet").And.Contain("ekiplerinizde değil");
        (await SendAsync(HttpMethod.Delete, c.Sef, "/api/v1/portal/routes/other", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var visible = await GetJsonAsync<RoutePlansResponse>(c.Sef, "/api/v1/portal/routes");
        visible.Plans.Select(p => p.PlanId).Should().Equal("mine");
        visible.People.Select(p => p.Username).Should().BeEquivalentTo(["ali", "sef"]);

        // The phone's own planning obeys the same scope.
        var phone = await SendAsync(HttpMethod.Post, c.Sef, "/api/v1/ingest/jobs", new
        {
            externalId = "rp-phone", documentType = "route_plan",
            payload = new { planId = "phone-plan", name = "Telefon", isActive = true, stops = Array.Empty<object>(), assignees = new[] { "mehmet" } },
        }, c.Id);
        (await phone.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Failed");
    }

    [Fact]
    public async Task Compliance_counts_what_was_visited_skipped_missed_and_off_the_plan()
    {
        var c = await CompanyAsync();
        await SendJsonAsync<RoutePlanDto>(HttpMethod.Put, c.Patron, "/api/v1/portal/routes/p", Plan("Rut", ["ali"],
            (YesterdayRouteDay, "C-1", "Dün 1"), (YesterdayRouteDay, "C-2", "Dün 2"), (TodayRouteDay, "C-3", "Bugün")));
        await BackdateRouteJobsAsync(c, days: 3);
        var plan = await GetJsonAsync<RoutePlanDto>(c.Patron, "/api/v1/portal/routes/p");
        var yesterday = Today.AddDays(-1).ToString("yyyy-MM-dd");
        var first = plan.Stops.First(s => s.CustomerCode == "C-1").StopId;
        await PostVisitAsync(c, c.Ali, "v1", new { visitId = "v1", planId = "p", stopId = first, customerCode = "C-1", visitDate = yesterday, status = "COMPLETED" });
        await PostVisitAsync(c, c.Ali, "v2", new { visitId = "v2", customerCode = "C-9", visitDate = yesterday, status = "COMPLETED" });

        var report = await GetJsonAsync<RouteComplianceResponse>(c.Patron,
            $"/api/v1/portal/routes/compliance?from={yesterday}&to={Today:yyyy-MM-dd}");

        var ali = report.Rows.Single(r => r.Username == "ali");
        ali.Planned.Should().Be(3);
        ali.Completed.Should().Be(1);
        ali.Missed.Should().Be(1, "yesterday's second stop was never recorded; today's is still open");
        ali.Unplanned.Should().Be(1);
        ali.Compliance.Should().Be(33.3m);
        report.Days.Should().HaveCount(2);
        (await SendAsync(HttpMethod.Get, c.Patron, "/api/v1/portal/routes/compliance?from=2026-01-01&to=2026-09-01", null)).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Deleting the plan today does not rewrite yesterday (Codex, PR #217).
        (await SendAsync(HttpMethod.Delete, c.Patron, "/api/v1/portal/routes/p", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var after = await GetJsonAsync<RouteComplianceResponse>(c.Patron, $"/api/v1/portal/routes/compliance?from={yesterday}&to={yesterday}");
        after.Rows.Single(r => r.Username == "ali").Should().Match<RouteComplianceRow>(r => r.Planned == 2 && r.Completed == 1 && r.Unplanned == 1);
        (await SendAsync(HttpMethod.Get, c.Patron, "/api/v1/portal/routes/p", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_visit_counts_for_the_plan_it_names_when_two_plans_share_a_stop_id()
    {
        var c = await CompanyAsync();
        var yesterday = Today.AddDays(-1).ToString("yyyy-MM-dd");
        foreach (var planId in new[] { "A", "B" })
        {
            var response = await SendAsync(HttpMethod.Post, c.Patron, "/api/v1/ingest/jobs", new
            {
                externalId = "rp-" + planId, documentType = "route_plan",
                payload = new
                {
                    planId, name = planId, isActive = true, assignees = new[] { "ali" },
                    stops = new[] { new { stopId = "s1", dayOfWeek = YesterdayRouteDay, customerCode = "C-" + planId, customerName = planId, visitOrder = 1 } },
                },
            }, c.Id);
            (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded");
        }
        await BackdateRouteJobsAsync(c, days: 3);
        await PostVisitAsync(c, c.Ali, "vb", new { visitId = "vb", planId = "B", stopId = "s1", customerCode = "C-B", visitDate = yesterday, status = "COMPLETED" });

        var report = await GetJsonAsync<RouteComplianceResponse>(c.Patron, $"/api/v1/portal/routes/compliance?from={yesterday}&to={yesterday}");

        report.Rows.Single(r => r.Username == "ali").Should().Match<RouteComplianceRow>(r => r.Planned == 2 && r.Completed == 1 && r.Missed == 1 && r.Unplanned == 0);
    }

    [Fact]
    public async Task Field_users_do_not_plan_from_the_panel()
    {
        var c = await CompanyAsync();

        (await SendAsync(HttpMethod.Get, c.Ali, "/api/v1/portal/routes", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await SendAsync(HttpMethod.Put, c.Ali, "/api/v1/portal/routes/x", Plan("x", ["ali"]))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await SendAsync(HttpMethod.Put, c.Patron, "/api/v1/portal/routes/bad%20id", Plan("x", []))).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---- helpers -----------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Sef, string Ali, Guid SefId, Guid AliId);

    private static RoutePlanDto Plan(string name, string[] assignees, params (int Day, string Code, string Name)[] stops) => new()
    {
        Name = name,
        IsActive = true,
        Assignees = assignees,
        Stops = [.. stops.Select((s, i) => new RouteStopDto { DayOfWeek = s.Day, CustomerCode = s.Code, CustomerName = s.Name, VisitOrder = i + 1 })],
    };

    /// <summary>Plans saved "days ago": a plan is in force from the day it was saved.</summary>
    private async Task BackdateRouteJobsAsync(Company c, int days)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        foreach (var job in db.Jobs.Where(j => j.TenantId == c.Id && j.DocumentType == "route_plan").ToList())
        {
            job.EnqueuedAtUtc = job.EnqueuedAtUtc.AddDays(-days);
            job.CompletedAtUtc = job.CompletedAtUtc?.AddDays(-days);
        }
        await db.SaveChangesAsync();
    }

    private async Task PostVisitAsync(Company c, string token, string externalId, object payload)
    {
        var response = await SendAsync(HttpMethod.Post, token, "/api/v1/ingest/jobs", new { externalId, documentType = "visit", payload }, c.Id);
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded");
    }

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"RUT-{suffix}", $"Rut tenant {suffix}");
        var adminToken = _factory.IssueAdminJwt((await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local")).Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";
        (await PutAsync($"{basePath}/subscription", new { seats = 6, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("sef", "MANAGER"), ("ali", "SALES"), ("mehmet", "SALES") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        var patron = await LoginAsync(code, "patron", $"DEV-P-{suffix}");
        var people = (await GetJsonAsync<TeamsResponse>(patron, "/api/v1/portal/teams")).People.ToDictionary(p => p.Username, p => p.Id);
        return new Company(tenant.Id, patron, await LoginAsync(code, "sef", $"DEV-S-{suffix}"), await LoginAsync(code, "ali", $"DEV-A-{suffix}"),
            people["sef"], people["ali"]);
    }

    private async Task<T> GetJsonAsync<T>(string token, string path)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }

    private async Task<T> SendJsonAsync<T>(HttpMethod method, string token, string path, object body, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var response = await SendAsync(method, token, path, body);
        response.StatusCode.Should().Be(expected, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "rut-test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string token, string path, object? body, Guid? tenantId = null)
    {
        var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (tenantId is { } id) request.Headers.Add("X-Tenant-Id", id.ToString());
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
