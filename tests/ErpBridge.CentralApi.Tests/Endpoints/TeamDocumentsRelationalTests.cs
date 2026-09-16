using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Route plans and visits (Faz 39). A manager plans a route on one phone and every
/// assigned salesperson's phone receives it through <c>/api/v1/android/sync/pull</c>;
/// a visit closed on one phone reaches the manager's. They are team data, not ERP
/// data, so they work the same with and without an ERP.
/// Relational because booking, projection and the cursor share one transaction.
/// </summary>
public sealed class TeamDocumentsRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public TeamDocumentsRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    public static TheoryData<bool> BothDataSources => new() { true, false };

    [Theory]
    [MemberData(nameof(BothDataSources))]
    public async Task A_route_planned_by_the_admin_reaches_the_salesperson_phone(bool native)
    {
        var t = await TenantAsync(native);

        var posted = await PostAsync(t.AdminToken, t, "route_plan", "RP-1", Plan("PLAN-1", assignees: ["ali"]));

        posted.Status.Should().Be("Succeeded");
        var plan = (await PullAllAsync(t.SalesToken, t)).Single(c => c.Entity == "rotaPlanlari" && c.Key == "PLAN-1");
        plan.Deleted.Should().BeFalse();
        plan.Data.GetProperty("name").GetString().Should().Be("Pazartesi Merkez");
        plan.Data.GetProperty("startDate").GetString().Should().Be("2026-09-21");
        plan.Data.GetProperty("updatedBy").GetString().Should().Be("patron");
        var stops = plan.Data.GetProperty("stops").EnumerateArray().ToList();
        stops.Should().HaveCount(2);
        stops[0].GetProperty("customerCode").GetString().Should().Be("C-001");
        stops[0].GetProperty("dayOfWeek").GetInt32().Should().Be(1);
        plan.Data.GetProperty("assignees").EnumerateArray().Select(a => a.GetString()).Should().Equal("ali");
    }

    [Fact]
    public async Task A_manager_plans_routes_too()
    {
        var t = await TenantAsync(native: true);

        (await PostAsync(t.ManagerToken, t, "route_plan", "RP-M", Plan("PLAN-M", assignees: ["ali"])))
            .Status.Should().Be("Succeeded");
    }

    [Fact]
    public async Task A_salesperson_cannot_plan_or_delete_a_route()
    {
        var t = await TenantAsync(native: true);

        var plan = await PostAsync(t.SalesToken, t, "route_plan", "RP-S", Plan("PLAN-S", assignees: ["ali"]));
        var delete = await PostAsync(t.SalesToken, t, "route_plan_delete", "RPD-S", new { planId = "PLAN-S" });

        plan.Status.Should().Be("Failed");
        delete.Status.Should().Be("Failed");
        (await JobErrorAsync(t.Id, "RP-S")).Should().Contain("administrators and managers");
        (await PullAllAsync(t.SalesToken, t)).Should().NotContain(c => c.Entity == "rotaPlanlari");
    }

    [Fact]
    public async Task Saving_a_plan_again_replaces_it_on_every_phone()
    {
        var t = await TenantAsync(native: true);
        await PostAsync(t.AdminToken, t, "route_plan", "RP-V1", Plan("PLAN-2", assignees: ["ali"]));
        var cursor = await CursorAtEndAsync(t);

        var edited = new
        {
            planId = "PLAN-2", name = "Pazartesi Merkez (yeni)", isActive = true,
            stops = new[] { new { stopId = "S-9", dayOfWeek = 3, customerCode = "C-009", customerName = "Yeni Bakkal", visitOrder = 0 } },
            assignees = new[] { "ali", "mehmet" },
        };
        (await PostAsync(t.AdminToken, t, "route_plan", "RP-V2", edited)).Status.Should().Be("Succeeded");

        var change = (await PullAllAsync(t.SalesToken, t, cursor)).Single(c => c.Entity == "rotaPlanlari");
        change.Key.Should().Be("PLAN-2");
        change.Data.GetProperty("name").GetString().Should().Be("Pazartesi Merkez (yeni)");
        change.Data.GetProperty("stops").GetArrayLength().Should().Be(1);
        change.Data.GetProperty("assignees").GetArrayLength().Should().Be(2);
    }

    [Theory]
    [MemberData(nameof(BothDataSources))]
    public async Task Deleting_a_plan_removes_it_from_every_phone(bool native)
    {
        var t = await TenantAsync(native);
        await PostAsync(t.AdminToken, t, "route_plan", "RP-D", Plan("PLAN-D", assignees: ["ali"]));
        var cursor = await CursorAtEndAsync(t);

        (await PostAsync(t.AdminToken, t, "route_plan_delete", "RPD-D", new { planId = "PLAN-D" })).Status.Should().Be("Succeeded");

        var change = (await PullAllAsync(t.SalesToken, t, cursor)).Single(c => c.Entity == "rotaPlanlari");
        change.Key.Should().Be("PLAN-D");
        change.Deleted.Should().BeTrue();
    }

    [Fact]
    public async Task Deleting_a_plan_the_server_never_had_is_not_a_failure()
    {
        var t = await TenantAsync(native: true);

        var delete = await PostAsync(t.AdminToken, t, "route_plan_delete", "RPD-X", new { planId = "PLAN-NEVER" });

        delete.Status.Should().Be("Succeeded");
        (await JobErrorAsync(t.Id, "RPD-X")).Should().Contain("nothing to delete");
    }

    [Fact]
    public async Task A_misspelt_assignee_fails_the_plan_instead_of_hiding_it_from_everyone()
    {
        var t = await TenantAsync(native: true);

        var posted = await PostAsync(t.AdminToken, t, "route_plan", "RP-TYPO", Plan("PLAN-T", assignees: ["alii"]));

        posted.Status.Should().Be("Failed");
        (await JobErrorAsync(t.Id, "RP-TYPO")).Should().Contain("alii");
    }

    [Theory]
    [MemberData(nameof(BothDataSources))]
    public async Task A_visit_reaches_the_manager_under_the_name_of_the_user_who_sent_it(bool native)
    {
        var t = await TenantAsync(native);

        var visit = await PostAsync(t.SalesToken, t, "visit", "V-1", new
        {
            visitId = "VISIT-1", planId = "PLAN-1", stopId = "S-1", customerCode = "C-001",
            visitDate = "2026-09-21", status = "completed", note = "Sipariş alındı",
            completedAt = 1790000000000L,
            username = "patron", // a phone cannot record a visit for someone else
        });

        visit.Status.Should().Be("Succeeded");
        var change = (await PullAllAsync(t.AdminToken, t)).Single(c => c.Entity == "rotaZiyaretleri" && c.Key == "VISIT-1");
        change.Data.GetProperty("username").GetString().Should().Be("ali");
        change.Data.GetProperty("status").GetString().Should().Be("COMPLETED");
        change.Data.GetProperty("note").GetString().Should().Be("Sipariş alındı");
        change.Data.GetProperty("completedAt").GetInt64().Should().Be(1790000000000L);
    }

    [Fact]
    public async Task An_invalid_visit_is_kept_as_failed_and_reaches_no_phone()
    {
        var t = await TenantAsync(native: true);

        var badStatus = await PostAsync(t.SalesToken, t, "visit", "V-BAD1", new { visitId = "VB-1", customerCode = "C-001", visitDate = "2026-09-21", status = "MAYBE" });
        var badDate = await PostAsync(t.SalesToken, t, "visit", "V-BAD2", new { visitId = "VB-2", customerCode = "C-001", visitDate = "21.09.2026", status = "SKIPPED" });

        badStatus.Status.Should().Be("Failed");
        badDate.Status.Should().Be("Failed");
        (await PullAllAsync(t.AdminToken, t)).Should().NotContain(c => c.Entity == "rotaZiyaretleri");
    }

    [Fact]
    public async Task Sending_the_same_visit_again_records_it_once()
    {
        var t = await TenantAsync(native: true);
        var body = new { visitId = "VISIT-R", customerCode = "C-001", visitDate = "2026-09-21", status = "SKIPPED" };

        await PostAsync(t.SalesToken, t, "visit", "V-R", body);
        var again = await PostAsync(t.SalesToken, t, "visit", "V-R", body);

        again.Idempotent.Should().BeTrue();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.Jobs.CountAsync(j => j.TenantId == t.Id && j.ExternalId == "V-R")).Should().Be(1);
    }

    [Fact]
    public async Task Route_plans_do_not_count_as_erp_data_when_the_company_drops_its_erp()
    {
        var t = await TenantAsync(native: false);
        (await PostAsync(t.AdminToken, t, "route_plan", "RP-SW", Plan("PLAN-SW", assignees: ["ali"]))).Status.Should().Be("Succeeded");

        var switched = await PutAsync($"/api/v1/admin/tenants/{t.Id}/mobile/data-source", new { dataSource = "native" }, await AdminTokenAsync());

        switched.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PullAllAsync(t.SalesToken, t)).Should().Contain(c => c.Entity == "rotaPlanlari" && c.Key == "PLAN-SW" && !c.Deleted);
    }

    [Fact]
    public async Task An_api_key_carries_no_person_and_cannot_send_team_documents()
    {
        var (tenant, _) = await _factory.SeedTenantAsync($"TEAM-KEY-{Guid.NewGuid():N}"[..20], "Api key tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var response = await SendAsync(_factory.CreateClient(), raw, tenant.Id, "/api/v1/ingest/jobs",
            new { externalId = "V-KEY", documentType = "visit", payload = new { visitId = "VK", customerCode = "C", visitDate = "2026-09-21", status = "SKIPPED" } });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await response.Content.ReadAsStringAsync()).Should().Contain("TEAM_DOCUMENT_REQUIRES_MOBILE_USER");
    }

    // ---- helpers -----------------------------------------------------------

    private sealed record TeamTenant(Guid Id, string Code, string AdminToken, string ManagerToken, string SalesToken);

    private sealed record Change(string Entity, string Key, bool Deleted, JsonElement Data);

    private static object Plan(string planId, string[] assignees) => new
    {
        planId,
        name = "Pazartesi Merkez",
        description = "Merkez bölge",
        startDate = "2026-09-21",
        isActive = true,
        stops = new[]
        {
            new { stopId = planId + "-S1", dayOfWeek = 1, customerCode = "C-001", customerName = "Bakkal Ali", visitOrder = 0 },
            new { stopId = planId + "-S2", dayOfWeek = 1, customerCode = "C-002", customerName = "Market Veli", visitOrder = 1 },
        },
        assignees,
    };

    private async Task<TeamTenant> TenantAsync(bool native)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"TEAM-{suffix}", $"Team tenant {suffix}");
        var adminToken = await AdminTokenAsync();
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        if (native)
            (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("sef", "MANAGER"), ("ali", "SALES"), ("mehmet", "SALES") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var overview = await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();
        var code = overview.TenantCode!;

        return new TeamTenant(tenant.Id, code,
            await LoginAsync(code, "patron", $"DEV-P-{suffix}"),
            await LoginAsync(code, "sef", $"DEV-M-{suffix}"),
            await LoginAsync(code, "ali", $"DEV-A-{suffix}"));
    }

    private async Task<IngestJobResponse> PostAsync(string token, TeamTenant t, string documentType, string externalId, object payload)
    {
        var response = await SendAsync(_factory.CreateClient(), token, t.Id, "/api/v1/ingest/jobs", new { externalId, documentType, payload });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        return await response.ReadAsJsonAsync<IngestJobResponse>();
    }

    private async Task<string?> JobErrorAsync(Guid tenantId, string externalId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return (await db.Jobs.SingleAsync(j => j.TenantId == tenantId && j.ExternalId == externalId)).LastError;
    }

    private async Task<List<Change>> PullAllAsync(string token, TeamTenant t, string? cursor = null)
    {
        var changes = new List<Change>();
        var client = _factory.CreateClient();
        while (true)
        {
            var response = await SendAsync(client, token, t.Id, "/api/v1/android/sync/pull", new { cursor, limit = 500 });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = document.RootElement;
            foreach (var change in root.GetProperty("changes").EnumerateArray())
            {
                changes.Add(new Change(
                    change.GetProperty("entity").GetString()!, change.GetProperty("key").GetString()!,
                    change.GetProperty("deleted").GetBoolean(),
                    change.TryGetProperty("data", out var data) ? data.Clone() : default));
            }
            cursor = root.GetProperty("nextCursor").GetString();
            if (!root.GetProperty("hasMore").GetBoolean()) return changes;
        }
    }

    private async Task<string> CursorAtEndAsync(TeamTenant t)
    {
        var client = _factory.CreateClient();
        string? cursor = null;
        while (true)
        {
            var response = await SendAsync(client, t.SalesToken, t.Id, "/api/v1/android/sync/pull", new { cursor, limit = 500 });
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            cursor = document.RootElement.GetProperty("nextCursor").GetString();
            if (!document.RootElement.GetProperty("hasMore").GetBoolean()) return cursor!;
        }
    }

    private async Task<string> AdminTokenAsync()
    {
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        return _factory.IssueAdminJwt(admin.Id);
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.233" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private static async Task<HttpResponseMessage> SendAsync(HttpClient client, string token, Guid tenantId, string path, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return await client.SendAsync(request);
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
