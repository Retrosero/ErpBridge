using System.Net;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Parameters;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// The agent's side: what one company's Mikro parameter table should hold, and what the agent
/// reports back after writing it.
/// </summary>
public sealed class AgentParameterTests : IClassFixture<CentralApiFactory>
{
    private const string Set = "MobilKullanici";

    private readonly CentralApiFactory _factory;

    public AgentParameterTests(CentralApiFactory factory) => _factory = factory;

    private sealed record World(
        Guid TenantId, Guid CompanyId, Guid OtherCompanyId, Guid UserId, Guid DepotId,
        Guid MenuId, string Token);

    private async Task<(World World, HttpClient Client)> SeedAsync(string code, bool assign = true)
    {
        var (tenant, _) = await _factory.SeedTenantAsync($"AGT-PRM-{code}", $"Ajan parametre {code}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-PRM-{code}");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();

        var company = new ErpCompany
        {
            TenantId = tenant.Id, Code = $"F{code}", Name = "Merkez",
            SourceDatabase = $"MikroDB_V16_{code}", CompanyNo = 1,
        };

        // A second company the same agent also serves: settings must not leak between them.
        var other = new ErpCompany
        {
            TenantId = tenant.Id, Code = $"S{code}", Name = "Şube",
            SourceDatabase = $"MikroDB_V16_{code}_2", CompanyNo = 2,
        };
        db.ErpCompanies.AddRange(company, other);

        var user = new MobileUser { TenantId = tenant.Id, Username = $"plasiyer{code}", FullName = "Plasiyer" };
        db.MobileUsers.Add(user);

        if (assign)
        {
            db.AgentCompanyAssignments.AddRange(
                new AgentCompanyAssignment { AgentId = agent.Id, ErpCompanyId = company.Id },
                new AgentCompanyAssignment { AgentId = agent.Id, ErpCompanyId = other.Id });
        }

        if (!db.ParameterCatalog.Any(e => e.CatalogMethod == Set))
        {
            db.ParameterCatalog.AddRange(
                new ParameterCatalogEntry
                {
                    Program = "akilli", CatalogMethod = Set, ParametreId = 58,
                    Name = "DefaultKaynakDepoNo", DefaultValue = "1", Editor = "integer", EditorOrder = 1,
                    ScopeKind = ParameterScopeKinds.MobileUser, ScopeFields = "user", SourceBuild = "unknown",
                },
                new ParameterCatalogEntry
                {
                    Program = "akilli", CatalogMethod = Set, ParametreId = 89,
                    Name = "Goster_AnaMenu_Tahsilat", DefaultValue = "1", Editor = "boolean", EditorOrder = 0,
                    ScopeKind = ParameterScopeKinds.MobileUser, ScopeFields = "user", SourceBuild = "unknown",
                });
        }

        await db.SaveChangesAsync();

        var depotId = db.ParameterCatalog.Single(e => e.CatalogMethod == Set && e.ParametreId == 58).Id;
        var menuId = db.ParameterCatalog.Single(e => e.CatalogMethod == Set && e.ParametreId == 89).Id;

        var world = new World(
            tenant.Id, company.Id, other.Id, user.Id, depotId, menuId,
            _factory.IssueTestJwt(agent.Id, tenant.Id));

        return (world, _factory.CreateClient());
    }

    private async Task WriteAsync(World w, Guid companyId, Guid catalogEntryId, string value)
    {
        using var scope = _factory.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<ParameterResolver>();

        await resolver.SetAsync(
            ParameterScope.ForMobileUser(w.TenantId, companyId, w.UserId), catalogEntryId, value,
            new ParameterResolver.ChangeContext(ParameterChangeSources.Panel, Actor: "test"));
    }

    private static string StateUrl(Guid companyId) =>
        $"/api/v1/agents/parameters?erpCompanyId={companyId}";

    [Fact]
    public async Task Only_deviations_are_mirrored()
    {
        var (w, client) = await SeedAsync("A");
        await WriteAsync(w, w.CompanyId, w.DepotId, "3");

        var body = await (await client.GetAsync(StateUrl(w.CompanyId), w.Token))
            .ReadAsJsonAsync<AgentParameterEndpoints.AgentParameterStateResponse>();

        // Fora stores deviations only, and the mirror is a copy of that: a parameter left at its
        // default must have no row on either side.
        body!.Items.Should().ContainSingle();
        body.Items[0].ParametreID.Should().Be(58);
        body.Items[0].ParametreDegeri.Should().Be("3");
        body.Items[0].ParametreProgram.Should().Be("akilli");
        body.SourceDatabase.Should().Be("MikroDB_V16_A");
    }

    [Fact]
    public async Task The_row_carries_the_username_the_mirror_writes()
    {
        var (w, client) = await SeedAsync("B");
        await WriteAsync(w, w.CompanyId, w.MenuId, "0");

        var body = await (await client.GetAsync(StateUrl(w.CompanyId), w.Token))
            .ReadAsJsonAsync<AgentParameterEndpoints.AgentParameterStateResponse>();

        // Mikro addresses a mobile user's settings by ParametreUser, so the translation from the
        // id happens here rather than leaving the agent to guess it.
        body!.Items[0].ParametreUser.Should().Be("plasiyerB");
        body.Items[0].MobileUserId.Should().Be(w.UserId);
    }

    [Fact]
    public async Task One_companys_settings_never_reach_anothers_database()
    {
        var (w, client) = await SeedAsync("C");
        await WriteAsync(w, w.CompanyId, w.DepotId, "3");
        await WriteAsync(w, w.OtherCompanyId, w.DepotId, "9");

        var mine = await (await client.GetAsync(StateUrl(w.CompanyId), w.Token))
            .ReadAsJsonAsync<AgentParameterEndpoints.AgentParameterStateResponse>();

        mine!.Items.Should().ContainSingle();
        mine.Items[0].ParametreDegeri.Should().Be("3", "the branch's own value is 9");
    }

    [Fact]
    public async Task A_deleted_users_settings_are_not_mirrored()
    {
        var (w, client) = await SeedAsync("D");
        await WriteAsync(w, w.CompanyId, w.DepotId, "3");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var user = db.MobileUsers.Single(u => u.Id == w.UserId);
            user.IsActive = false;
            user.DeletedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }

        // A username is reusable; mirroring a deleted user's rows would hand their permissions to
        // whoever is opened under that name next (D5b, R8).
        (await (await client.GetAsync(StateUrl(w.CompanyId), w.Token))
            .ReadAsJsonAsync<AgentParameterEndpoints.AgentParameterStateResponse>())!
            .Items.Should().BeEmpty();
    }

    [Fact]
    public async Task The_company_has_to_be_named()
    {
        var (w, client) = await SeedAsync("E");

        (await client.GetAsync("/api/v1/agents/parameters", w.Token))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest,
                "an agent can serve several companies and each is a different Mikro database");
    }

    [Fact]
    public async Task An_unassigned_company_is_refused()
    {
        var (w, client) = await SeedAsync("F", assign: false);

        (await client.GetAsync(StateUrl(w.CompanyId), w.Token))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Another_tenants_company_looks_the_same_as_an_unassigned_one()
    {
        var (mine, client) = await SeedAsync("G");
        var (theirs, _) = await SeedAsync("H");

        var response = await client.GetAsync(StateUrl(theirs.CompanyId), mine.Token);

        // Same answer as "not assigned": an agent has no business learning which companies other
        // tenants own.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_run_reports_what_it_wrote_and_what_it_found_changed_by_hand()
    {
        var (w, client) = await SeedAsync("I");
        await WriteAsync(w, w.CompanyId, w.DepotId, "3");

        var response = await client.PostJsonAsync("/api/v1/agents/parameters/report", new
        {
            erpCompanyId = w.CompanyId,
            appliedRevision = 1,
            inserted = 1,
            updated = 0,
            deleted = 2,
            failed = 0,
            drifts = new[]
            {
                new
                {
                    catalogEntryId = w.DepotId,
                    mobileUserId = w.UserId,
                    expectedValue = "3",
                    foundValue = "7",
                },
            },
        }, w.Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<AgentParameterEndpoints.AgentParameterReportResponse>();
        body!.Drifted.Should().Be(1);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();

        var stored = await db.ParameterMirrorReports.AsNoTracking()
            .Include(r => r.Drifts)
            .SingleAsync(r => r.Id == body.ReportId);

        stored.Inserted.Should().Be(1);
        stored.Deleted.Should().Be(2);
        stored.AppliedRevision.Should().Be(1);
        stored.AgentId.Should().NotBeNull("a report has to say which machine wrote it");

        // Drift is recorded, never written back: the mirror is one-way (D8).
        var drift = stored.Drifts.Single();
        drift.ExpectedValue.Should().Be("3");
        drift.FoundValue.Should().Be("7");

        (await db.ParameterValues.AsNoTracking()
            .SingleAsync(v => v.ParameterCatalogEntryId == w.DepotId && v.ErpCompanyId == w.CompanyId))
            .Value.Should().Be("3", "what the agent found in Mikro never becomes the truth");
    }

    [Fact]
    public async Task A_drift_naming_an_unknown_parameter_is_dropped_rather_than_stored()
    {
        var (w, client) = await SeedAsync("J");

        var response = await client.PostJsonAsync("/api/v1/agents/parameters/report", new
        {
            erpCompanyId = w.CompanyId,
            appliedRevision = 0,
            drifts = new[] { new { catalogEntryId = Guid.NewGuid(), expectedValue = "1", foundValue = "2" } },
        }, w.Token);

        // Storing it would leave a row pointing at nothing, which nobody could read later.
        (await response.ReadAsJsonAsync<AgentParameterEndpoints.AgentParameterReportResponse>())!
            .Drifted.Should().Be(0);
    }

    [Fact]
    public async Task Reporting_for_an_unassigned_company_is_refused()
    {
        var (w, client) = await SeedAsync("K", assign: false);

        (await client.PostJsonAsync("/api/v1/agents/parameters/report", new
        {
            erpCompanyId = w.CompanyId, appliedRevision = 0,
        }, w.Token)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task The_endpoints_need_an_agent_token()
    {
        var (w, client) = await SeedAsync("L");

        (await client.GetAsync(StateUrl(w.CompanyId)))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
