using System.Net;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Parameters;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// The panel's side of the parameter system: read a set as it applies to someone, change values,
/// put them back, and see who changed what.
/// </summary>
public sealed class AdminParameterTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminParameterTests(CentralApiFactory factory) => _factory = factory;

    private sealed record World(Guid TenantId, Guid CompanyId, Guid UserId, Guid DepotId, Guid MenuId, string CatalogMethod);

    /// <summary>
    /// The test host does not seed the 4,700-row catalogue, so these tests plant the two
    /// parameters they need.
    /// </summary>
    private async Task<World> SeedAsync(string code)
    {
        // The class shares one factory, so the catalogue is shared too: each test gets its own
        // set name rather than seeing every other test's parameters.
        var method = $"MobilKullanici{code}";
        var (tenant, _) = await _factory.SeedTenantAsync($"CT parameters {code}", $"CT-PRM-{code}");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();

        var company = new ErpCompany
        {
            TenantId = tenant.Id, Code = $"F{code}", Name = "Merkez", SourceDatabase = "MikroDB_V15_02",
            CompanyNo = 1, BranchNo = 0, WarehouseNo = 0,
        };
        db.ErpCompanies.Add(company);

        var user = new MobileUser { TenantId = tenant.Id, Username = $"plasiyer{code}", FullName = "Plasiyer" };
        db.MobileUsers.Add(user);

        var depot = new ParameterCatalogEntry
        {
            Program = "akilli", CatalogMethod = method, ParametreId = 58,
            Name = "DefaultKaynakDepoNo", DefaultValue = "1", Label = "Kaynak depo no :",
            Editor = "integer", EditorOrder = 1, TabPath = "Parametreler / Tanımlamalar",
            ScopeKind = ParameterScopeKinds.MobileUser, ScopeFields = "user", SourceBuild = "unknown",
        };

        var menu = new ParameterCatalogEntry
        {
            Program = "akilli", CatalogMethod = method, ParametreId = 89,
            Name = "Goster_AnaMenu_Tahsilat", DefaultValue = "1", Label = "Tahsilat girebilir",
            Editor = "boolean", EditorOrder = 0, TabPath = "Evrak girişi",
            ScopeKind = ParameterScopeKinds.MobileUser, ScopeFields = "user", SourceBuild = "unknown",
        };

        db.ParameterCatalog.AddRange(depot, menu);
        await db.SaveChangesAsync();

        return new World(tenant.Id, company.Id, user.Id, depot.Id, menu.Id, method);
    }

    private async Task<(HttpClient Client, string Token)> SignedInAsync()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        return (client, _factory.IssueAdminJwt(admin.Id));
    }

    private static string ValuesUrl(World w) =>
        $"/api/v1/admin/parameters/values?tenantId={w.TenantId}&erpCompanyId={w.CompanyId}"
        + $"&catalogMethod={w.CatalogMethod}&mobileUserId={w.UserId}";

    [Fact]
    public async Task A_set_reads_back_with_defaults_until_something_is_changed()
    {
        var w = await SeedAsync("A");
        var (client, token) = await SignedInAsync();

        var response = await client.GetAsync(ValuesUrl(w), token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsJsonAsync<AdminParameterEndpoints.ParameterValuesResponse>();

        body!.Revision.Should().Be(0, "nothing has been changed in this scope yet");
        body.Items.Should().HaveCount(2);
        body.Items.Should().OnlyContain(i => !i.IsOverridden);

        // Listed the way Fora's own editor lists them, and carrying what the panel renders from.
        body.Items[0].Name.Should().Be("Goster_AnaMenu_Tahsilat");
        body.Items[0].Label.Should().Be("Tahsilat girebilir");
        body.Items[0].Editor.Should().Be("boolean");
        body.Items[0].Value.Should().Be("1");
        body.Items[0].IsImplemented.Should().BeFalse("nothing is honoured by the mobile app yet (D16)");
    }

    [Fact]
    public async Task Writing_a_value_moves_the_revision_and_reads_back()
    {
        var w = await SeedAsync("B");
        var (client, token) = await SignedInAsync();

        var write = await client.PutJsonAsync("/api/v1/admin/parameters/values", new
        {
            tenantId = w.TenantId,
            erpCompanyId = w.CompanyId,
            mobileUserId = w.UserId,
            changes = new[] { new { catalogEntryId = w.DepotId, value = "3" } },
        }, token);

        write.StatusCode.Should().Be(HttpStatusCode.OK);
        var written = await write.ReadAsJsonAsync<AdminParameterEndpoints.ParameterWriteResponse>();
        written!.Results.Single().Outcome.Should().Be("Inserted");
        written.Revision.Should().Be(1);

        var read = await (await client.GetAsync(ValuesUrl(w), token))
            .ReadAsJsonAsync<AdminParameterEndpoints.ParameterValuesResponse>();

        var depot = read!.Items.Single(i => i.CatalogEntryId == w.DepotId);
        depot.Value.Should().Be("3");
        depot.DefaultValue.Should().Be("1", "the panel shows what it would go back to");
        depot.IsOverridden.Should().BeTrue();
    }

    [Fact]
    public async Task Writing_the_default_stores_nothing_and_reset_removes_a_deviation()
    {
        var w = await SeedAsync("C");
        var (client, token) = await SignedInAsync();

        var noop = await client.PutJsonAsync("/api/v1/admin/parameters/values", new
        {
            tenantId = w.TenantId, erpCompanyId = w.CompanyId, mobileUserId = w.UserId,
            changes = new[] { new { catalogEntryId = w.DepotId, value = "1" } },
        }, token);

        (await noop.ReadAsJsonAsync<AdminParameterEndpoints.ParameterWriteResponse>())!
            .Results.Single().Outcome.Should().Be("Unchanged");

        await client.PutJsonAsync("/api/v1/admin/parameters/values", new
        {
            tenantId = w.TenantId, erpCompanyId = w.CompanyId, mobileUserId = w.UserId,
            changes = new[] { new { catalogEntryId = w.DepotId, value = "9" } },
        }, token);

        var reset = await client.PostJsonAsync("/api/v1/admin/parameters/values/reset", new
        {
            tenantId = w.TenantId, erpCompanyId = w.CompanyId, mobileUserId = w.UserId,
            catalogEntryIds = new[] { w.DepotId },
        }, token);

        (await reset.ReadAsJsonAsync<AdminParameterEndpoints.ParameterWriteResponse>())!
            .Results.Single().Outcome.Should().Be("Deleted");

        var onlyChanged = await client.GetAsync(ValuesUrl(w) + "&onlyOverridden=true", token);
        (await onlyChanged.ReadAsJsonAsync<AdminParameterEndpoints.ParameterValuesResponse>())!
            .Items.Should().BeEmpty();
    }

    [Fact]
    public async Task A_scope_that_cannot_address_the_parameter_is_rejected()
    {
        var w = await SeedAsync("D");
        var (client, token) = await SignedInAsync();

        // No mobile user named, for a parameter that belongs to one.
        var response = await client.PutJsonAsync("/api/v1/admin/parameters/values", new
        {
            tenantId = w.TenantId,
            erpCompanyId = w.CompanyId,
            changes = new[] { new { catalogEntryId = w.DepotId, value = "3" } },
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task The_company_is_required()
    {
        var w = await SeedAsync("E");
        var (client, token) = await SignedInAsync();

        var response = await client.GetAsync(
            $"/api/v1/admin/parameters/values?tenantId={w.TenantId}&catalogMethod={w.CatalogMethod}", token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "a tenant can own several companies and a value belongs to exactly one");
    }

    [Fact]
    public async Task The_trail_shows_who_changed_what()
    {
        var w = await SeedAsync("F");
        var (client, token) = await SignedInAsync();

        await client.PutJsonAsync("/api/v1/admin/parameters/values", new
        {
            tenantId = w.TenantId, erpCompanyId = w.CompanyId, mobileUserId = w.UserId,
            changes = new[] { new { catalogEntryId = w.MenuId, value = "0" } },
        }, token);

        var audit = await client.GetAsync(
            $"/api/v1/admin/parameters/audit?tenantId={w.TenantId}&catalogEntryId={w.MenuId}", token);

        var rows = await audit.ReadAsJsonAsync<AdminParameterEndpoints.ParameterAuditDto[]>();

        rows.Should().ContainSingle();
        rows![0].Name.Should().Be("Goster_AnaMenu_Tahsilat");
        rows[0].Outcome.Should().Be("Inserted");
        rows[0].NewValue.Should().Be("0");
        rows[0].Source.Should().Be(ParameterChangeSources.Panel);
        rows[0].AdminUserId.Should().NotBeNull("a change has to be attributable to someone");
    }

    [Fact]
    public async Task The_sets_listing_says_how_each_one_is_addressed()
    {
        await SeedAsync("G");
        var (client, token) = await SignedInAsync();

        var response = await client.GetAsync("/api/v1/admin/parameters/sets", token);
        var sets = await response.ReadAsJsonAsync<AdminParameterEndpoints.ParameterSetDto[]>();

        var mobile = sets!.Single(s => s.CatalogMethod == "MobilKullaniciG");
        mobile.Program.Should().Be("akilli");
        mobile.ScopeKind.Should().Be(ParameterScopeKinds.MobileUser);
        mobile.ScopeFields.Should().Be("user");
    }

    [Fact]
    public async Task The_endpoints_need_an_admin()
    {
        var w = await SeedAsync("H");
        var client = _factory.CreateClient();

        (await client.GetAsync(ValuesUrl(w))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
