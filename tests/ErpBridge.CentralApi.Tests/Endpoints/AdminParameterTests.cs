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

    private sealed record World(
        Guid TenantId, Guid CompanyId, Guid UserId, Guid OtherUserId, Guid DepotId, Guid MenuId,
        string CatalogMethod);

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
        var other = new MobileUser { TenantId = tenant.Id, Username = $"yeni{code}", FullName = "Yeni Plasiyer" };
        db.MobileUsers.AddRange(user, other);

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

        return new World(tenant.Id, company.Id, user.Id, other.Id, depot.Id, menu.Id, method);
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
    public async Task A_changed_parameter_says_who_moved_it_and_when()
    {
        var w = await SeedAsync("K");
        var (client, token) = await SignedInAsync();

        await client.PutJsonAsync("/api/v1/admin/parameters/values", new
        {
            tenantId = w.TenantId, erpCompanyId = w.CompanyId, mobileUserId = w.UserId,
            changes = new[] { new { catalogEntryId = w.DepotId, value = "3" } },
        }, token);

        var body = await (await client.GetAsync(ValuesUrl(w), token))
            .ReadAsJsonAsync<AdminParameterEndpoints.ParameterValuesResponse>();

        var depot = body!.Items.Single(i => i.CatalogEntryId == w.DepotId);
        depot.LastChangeSource.Should().Be(ParameterChangeSources.Panel);
        depot.LastChangedAtUtc.Should().NotBeNull();

        // A parameter sitting at its default has nobody to attribute it to, and asking the trail
        // about all 1,801 of them would read a table that mostly says nothing.
        var menu = body.Items.Single(i => i.CatalogEntryId == w.MenuId);
        menu.LastChangedAtUtc.Should().BeNull();
        menu.LastChangedBy.Should().BeNull();
    }

    [Fact]
    public async Task The_trail_comes_back_newest_first()
    {
        var w = await SeedAsync("P");
        var (client, token) = await SignedInAsync();

        await WriteAsync(client, token, w, w.UserId, w.DepotId, "3");
        await WriteAsync(client, token, w, w.UserId, w.DepotId, "5");
        await WriteAsync(client, token, w, w.UserId, w.DepotId, "7");

        var rows = await (await client.GetAsync(
            $"/api/v1/admin/parameters/audit?tenantId={w.TenantId}&catalogEntryId={w.DepotId}", token))
            .ReadAsJsonAsync<AdminParameterEndpoints.ParameterAuditDto[]>();

        // Ordered by when it happened. The row id is a GUID, so ordering by it would hand back
        // "the latest changes" in an order nobody can explain.
        rows!.Select(r => r.NewValue).Should().Equal("7", "5", "3");
    }

    [Fact]
    public async Task The_trail_can_be_narrowed_to_one_scope()
    {
        var w = await SeedAsync("R");
        var (client, token) = await SignedInAsync();

        await WriteAsync(client, token, w, w.UserId, w.DepotId, "3");
        await WriteAsync(client, token, w, w.OtherUserId, w.DepotId, "9");

        var mine = await (await client.GetAsync(
            $"/api/v1/admin/parameters/audit?tenantId={w.TenantId}&erpCompanyId={w.CompanyId}"
            + $"&catalogMethod={w.CatalogMethod}&mobileUserId={w.UserId}&scoped=true", token))
            .ReadAsJsonAsync<AdminParameterEndpoints.ParameterAuditDto[]>();

        // "What did this user's settings do" and "what did that one's" are different questions.
        mine.Should().ContainSingle();
        mine![0].NewValue.Should().Be("3");
    }

    [Fact]
    public async Task The_trail_can_be_narrowed_to_one_catalogue_set()
    {
        var w = await SeedAsync("S");
        var (client, token) = await SignedInAsync();

        await WriteAsync(client, token, w, w.UserId, w.DepotId, "3");

        var other = await (await client.GetAsync(
            $"/api/v1/admin/parameters/audit?tenantId={w.TenantId}&catalogMethod=BaskaBirKume", token))
            .ReadAsJsonAsync<AdminParameterEndpoints.ParameterAuditDto[]>();

        other.Should().BeEmpty();
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

    /// <summary>Writes one value into the named user's scope.</summary>
    private async Task WriteAsync(HttpClient client, string token, World w, Guid userId, Guid entryId, string value) =>
        await client.PutJsonAsync("/api/v1/admin/parameters/values", new
        {
            tenantId = w.TenantId, erpCompanyId = w.CompanyId, mobileUserId = userId,
            changes = new[] { new { catalogEntryId = entryId, value } },
        }, token);

    [Fact]
    public async Task Copying_a_user_leaves_the_target_holding_exactly_what_the_source_holds()
    {
        var w = await SeedAsync("L");
        var (client, token) = await SignedInAsync();

        await WriteAsync(client, token, w, w.UserId, w.DepotId, "3");
        await WriteAsync(client, token, w, w.OtherUserId, w.MenuId, "0");

        var response = await client.PostJsonAsync("/api/v1/admin/parameters/values/copy", new
        {
            tenantId = w.TenantId,
            erpCompanyId = w.CompanyId,
            catalogMethod = w.CatalogMethod,
            fromMobileUserId = w.UserId,
            toMobileUserId = w.OtherUserId,
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<AdminParameterEndpoints.ParameterCopyResponse>();

        body!.Copied.Should().Be(1);
        body.Written.Should().Be(1);

        // Leaving the target's own deviation in place would produce a third configuration nobody
        // chose: not the source's settings and not the target's either.
        body.Cleared.Should().Be(1);

        var target = await client.GetAsync(
            $"/api/v1/admin/parameters/values?tenantId={w.TenantId}&erpCompanyId={w.CompanyId}"
            + $"&catalogMethod={w.CatalogMethod}&mobileUserId={w.OtherUserId}&onlyOverridden=true", token);

        var items = (await target.ReadAsJsonAsync<AdminParameterEndpoints.ParameterValuesResponse>())!.Items;
        items.Should().ContainSingle();
        items[0].CatalogEntryId.Should().Be(w.DepotId);
        items[0].Value.Should().Be("3");
    }

    [Fact]
    public async Task Merging_adds_without_taking_anything_away()
    {
        var w = await SeedAsync("M");
        var (client, token) = await SignedInAsync();

        await WriteAsync(client, token, w, w.UserId, w.DepotId, "3");
        await WriteAsync(client, token, w, w.OtherUserId, w.MenuId, "0");

        var body = await (await client.PostJsonAsync("/api/v1/admin/parameters/values/copy", new
        {
            tenantId = w.TenantId, erpCompanyId = w.CompanyId, catalogMethod = w.CatalogMethod,
            fromMobileUserId = w.UserId, toMobileUserId = w.OtherUserId, merge = true,
        }, token)).ReadAsJsonAsync<AdminParameterEndpoints.ParameterCopyResponse>();

        body!.Cleared.Should().Be(0, "merge means 'also give them these'");

        var target = await client.GetAsync(
            $"/api/v1/admin/parameters/values?tenantId={w.TenantId}&erpCompanyId={w.CompanyId}"
            + $"&catalogMethod={w.CatalogMethod}&mobileUserId={w.OtherUserId}&onlyOverridden=true", token);

        (await target.ReadAsJsonAsync<AdminParameterEndpoints.ParameterValuesResponse>())!
            .Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task A_copy_is_attributable()
    {
        var w = await SeedAsync("N");
        var (client, token) = await SignedInAsync();

        await WriteAsync(client, token, w, w.UserId, w.DepotId, "3");

        await client.PostJsonAsync("/api/v1/admin/parameters/values/copy", new
        {
            tenantId = w.TenantId, erpCompanyId = w.CompanyId, catalogMethod = w.CatalogMethod,
            fromMobileUserId = w.UserId, toMobileUserId = w.OtherUserId,
        }, token);

        var audit = await client.GetAsync(
            $"/api/v1/admin/parameters/audit?tenantId={w.TenantId}&catalogEntryId={w.DepotId}", token);

        var rows = await audit.ReadAsJsonAsync<AdminParameterEndpoints.ParameterAuditDto[]>();

        // A bulk operation is still a change to each setting, and says where it came from.
        rows.Should().Contain(r => r.Source == ParameterChangeSources.Copy
                                   && r.MobileUserId == w.OtherUserId);
    }

    [Fact]
    public async Task Copying_a_scope_onto_itself_is_refused()
    {
        var w = await SeedAsync("O");
        var (client, token) = await SignedInAsync();

        var response = await client.PostJsonAsync("/api/v1/admin/parameters/values/copy", new
        {
            tenantId = w.TenantId, erpCompanyId = w.CompanyId, catalogMethod = w.CatalogMethod,
            fromMobileUserId = w.UserId, toMobileUserId = w.UserId,
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task The_endpoints_need_an_admin()
    {
        var w = await SeedAsync("H");
        var client = _factory.CreateClient();

        (await client.GetAsync(ValuesUrl(w))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
