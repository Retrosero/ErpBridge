using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>/api/v1/admin/audit/changeset</c>.
///
/// <para>These endpoints shipped without coverage, which is how they went live
/// resolving the tenant from <c>http.User.TryGetTenantId</c>. Admin principals
/// carry no tenant claim — <c>IJwtIssuer.IssueForAdmin</c> mints
/// <c>sub</c>/<c>scope</c>/<c>jti</c> only — so every admin request was answered
/// with 401 and the "Sync geçmişi" page could never render a row. The admin
/// console already sent <c>?tenantId=</c>; the server just ignored it.</para>
/// </summary>
public class AdminAuditTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminAuditTests(CentralApiFactory factory) => _factory = factory;

    private async Task SeedAuditAsync(Guid tenantId, string table, string direction, int rowCount)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        db.ChangeSetAuditEntries.Add(new ChangeSetAuditEntry
        {
            TenantId = tenantId,
            SourceDatabase = "MIKRO",
            TableKey = table.ToLowerInvariant(),
            TableName = table,
            Direction = direction,
            FirstTriggerRecNo = 1,
            LastTriggerRecNo = 10,
            RowCount = rowCount,
            PulledAtUtc = DateTimeOffset.UtcNow,
            ReceivedAtUtc = DateTimeOffset.UtcNow,
            AgentId = Guid.NewGuid().ToString(),
            IdempotencyKey = Guid.NewGuid().ToString("N"),
        });
        await db.SaveChangesAsync();
    }

    private async Task<(Guid TenantId, string Token)> SeedTenantAndAdminAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var admin = await _factory.SeedAdminAsync($"audit-{suffix}@test.local");
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync($"AUDIT-{suffix}", $"Audit Tenant {suffix}");
        return (tenant.Id, token);
    }

    [Fact]
    public async Task List_returns_rows_for_an_admin_token()
    {
        var (tenantId, token) = await SeedTenantAndAdminAsync();
        await SeedAuditAsync(tenantId, "STOKLAR", "changed", 5);
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/admin/audit/changeset?tenantId={tenantId}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "an admin JWT carries no tenant claim, so the tenant must come from the query string");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("total").GetInt32().Should().Be(1);
        body.RootElement.GetProperty("items")[0].GetProperty("table").GetString().Should().Be("STOKLAR");
    }

    [Fact]
    public async Task List_scopes_to_the_requested_tenant_only()
    {
        var (mine, token) = await SeedTenantAndAdminAsync();
        var (other, _) = await _factory.SeedTenantAsync(
            $"AUDIT-OTHER-{Guid.NewGuid():N}"[..24], $"Other {Guid.NewGuid():N}"[..20]);
        await SeedAuditAsync(mine, "STOKLAR", "changed", 5);
        await SeedAuditAsync(other.Id, "CARI_HESAPLAR", "changed", 9);
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/admin/audit/changeset?tenantId={mine}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("total").GetInt32().Should().Be(1);
        body.RootElement.GetProperty("items")[0].GetProperty("table").GetString().Should().Be("STOKLAR");
    }

    [Fact]
    public async Task List_can_filter_by_table()
    {
        var (tenantId, token) = await SeedTenantAndAdminAsync();
        await SeedAuditAsync(tenantId, "STOKLAR", "changed", 5);
        await SeedAuditAsync(tenantId, "CARI_HESAPLAR", "deleted", 2);
        var client = _factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/v1/admin/audit/changeset?tenantId={tenantId}&table=CARI_HESAPLAR", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("total").GetInt32().Should().Be(1);
        body.RootElement.GetProperty("items")[0].GetProperty("direction").GetString().Should().Be("deleted");
    }

    [Fact]
    public async Task Csv_export_returns_a_row_for_an_admin_token()
    {
        var (tenantId, token) = await SeedTenantAndAdminAsync();
        await SeedAuditAsync(tenantId, "STOKLAR", "changed", 5);
        var client = _factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/v1/admin/audit/changeset/export.csv?tenantId={tenantId}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var csv = await response.Content.ReadAsStringAsync();
        csv.Should().Contain("STOKLAR");
    }

    [Fact]
    public async Task Detail_returns_the_entry_for_an_admin_token()
    {
        var (tenantId, token) = await SeedTenantAndAdminAsync();
        await SeedAuditAsync(tenantId, "STOKLAR", "changed", 5);
        var client = _factory.CreateClient();

        var list = await client.GetAsync($"/api/v1/admin/audit/changeset?tenantId={tenantId}", token);
        using var listBody = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        var id = listBody.RootElement.GetProperty("items")[0].GetProperty("id").GetGuid();

        var response = await client.GetAsync($"/api/v1/admin/audit/changeset/{id}?tenantId={tenantId}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        // The detail endpoint projects the entity property directly, so it is
        // "tableName" here while the list endpoint renames it to "table".
        body.RootElement.GetProperty("tableName").GetString().Should().Be("STOKLAR");
    }

    [Fact]
    public async Task Missing_tenant_is_a_400_not_a_401()
    {
        var (_, token) = await SeedTenantAndAdminAsync();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/admin/audit/changeset", token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Anonymous_access_is_rejected()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/admin/audit/changeset?tenantId={Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }
}
