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
/// Taking a customer's existing Fora settings in (P3c). What matters is what the import refuses to
/// guess: it never opens a mobile user, never stores the password Fora keeps, and never drops a
/// row it could not place.
/// </summary>
public sealed class ForaImportTests : IClassFixture<CentralApiFactory>
{
    private const string Set = "MobilKullanici";

    private readonly CentralApiFactory _factory;

    public ForaImportTests(CentralApiFactory factory) => _factory = factory;

    private sealed record World(Guid TenantId, Guid CompanyId, Guid UserId, string Token);

    private async Task<(World World, HttpClient Client)> SeedAsync(string code)
    {
        var (tenant, _) = await _factory.SeedTenantAsync($"FORA-{code}", $"Fora import {code}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-FORA-{code}");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();

        var company = new ErpCompany
        {
            TenantId = tenant.Id, Code = $"F{code}", Name = "Merkez",
            SourceDatabase = $"MikroDB_V16_{code}", CompanyNo = 1,
        };
        db.ErpCompanies.Add(company);

        var user = new MobileUser { TenantId = tenant.Id, Username = $"plasiyer{code}", FullName = "Plasiyer" };
        var gone = new MobileUser
        {
            TenantId = tenant.Id, Username = $"ayrilan{code}", FullName = "Ayrılan",
            IsActive = false, DeletedAtUtc = DateTimeOffset.UtcNow,
        };
        db.MobileUsers.AddRange(user, gone);

        db.AgentCompanyAssignments.Add(new AgentCompanyAssignment { AgentId = agent.Id, ErpCompanyId = company.Id });

        if (!db.ParameterCatalog.Any(e => e.CatalogMethod == Set && e.Program == "akilli"))
        {
            db.ParameterCatalog.AddRange(
                new ParameterCatalogEntry
                {
                    Program = "akilli", CatalogMethod = Set, ParametreId = 58,
                    Name = "DefaultKaynakDepoNo", DefaultValue = "1",
                    ScopeKind = ParameterScopeKinds.MobileUser, ScopeFields = "user", SourceBuild = "unknown",
                },
                new ParameterCatalogEntry
                {
                    Program = "akilli", CatalogMethod = Set, ParametreId = 89,
                    Name = "Goster_AnaMenu_Tahsilat", DefaultValue = "1",
                    ScopeKind = ParameterScopeKinds.MobileUser, ScopeFields = "user", SourceBuild = "unknown",
                });
        }

        await db.SaveChangesAsync();

        return (new World(tenant.Id, company.Id, user.Id, _factory.IssueTestJwt(agent.Id, tenant.Id)),
                _factory.CreateClient());
    }

    private static object Row(int id, string user, string value, string name = "P") => new
    {
        parametreProgram = "akilli",
        parametreUser = user,
        anaGrubu = "",
        altGrubu = "",
        parametreID = id,
        parametreAdi = name,
        parametreDegeri = value,
    };

    private Task<HttpResponseMessage> UploadAsync(HttpClient client, World w, params object[] rows) =>
        client.PostJsonAsync("/api/v1/agents/parameters/fora-import",
            new { erpCompanyId = w.CompanyId, rows }, w.Token);

    [Fact]
    public async Task A_scan_is_stored_as_a_proposal_not_applied()
    {
        var (w, client) = await SeedAsync("A");

        var response = await UploadAsync(client, w, Row(58, "plasiyerA", "3"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsJsonAsync<AgentParameterEndpoints.ForaImportResponse>();
        body!.Scanned.Should().Be(1);
        body.Matched.Should().Be(1);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();

        var batch = await db.ForaImportBatches.AsNoTracking().SingleAsync(b => b.Id == body.BatchId);
        batch.State.Should().Be(ForaImportStates.Proposed);

        // These are the settings the customer has been running, read out of a table we do not own.
        // Applying them silently would move settings nobody at our end chose.
        (await db.ParameterValues.AsNoTracking().CountAsync(v => v.TenantId == w.TenantId))
            .Should().Be(0);
    }

    [Fact]
    public async Task A_username_with_no_active_user_is_reported_and_no_user_is_opened()
    {
        var (w, client) = await SeedAsync("B");

        var body = await (await UploadAsync(client, w,
            Row(58, "plasiyerB", "3"),
            Row(58, "ayrilanB", "9"),
            Row(58, "hicyok", "7")))
            .ReadAsJsonAsync<AgentParameterEndpoints.ForaImportResponse>();

        // A username is a reusable label; opening a user from an import would hand a departed
        // plasiyer's permissions to a row nobody reviewed (D5, D5b).
        body!.UnmatchedUsers.Should().BeEquivalentTo(["ayrilanB", "hicyok"]);
        body.Matched.Should().Be(1);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();

        (await db.MobileUsers.AsNoTracking().CountAsync(u => u.TenantId == w.TenantId))
            .Should().Be(2, "the scan opened nobody");
    }

    [Fact]
    public async Task A_row_the_catalogue_does_not_know_is_kept_and_counted()
    {
        var (w, client) = await SeedAsync("C");

        var body = await (await UploadAsync(client, w,
            Row(58, "plasiyerC", "3"),
            Row(99999, "plasiyerC", "x")))
            .ReadAsJsonAsync<AgentParameterEndpoints.ForaImportResponse>();

        // The customer may run a Fora build newer than the catalogue. Dropping the row would make
        // the import look complete when it was not (R1).
        body!.Unknown.Should().Be(1);
        body.Scanned.Should().Be(2);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();

        var rows = await db.ForaImportRows.AsNoTracking()
            .Where(r => r.ForaImportBatchId == body.BatchId).ToListAsync();

        rows.Should().HaveCount(2);
        rows.Should().ContainSingle(r => r.ParametreId == 99999 && r.ParameterCatalogEntryId == null);
    }

    [Fact]
    public async Task The_password_Fora_keeps_is_never_stored()
    {
        var (w, client) = await SeedAsync("D");

        // Sifre is parameter 1 of MobilKullanici. It is excluded from the catalogue (D6), so it
        // cannot match, and what Fora encrypted with its own key stays where it was.
        var body = await (await UploadAsync(client, w, Row(1, "plasiyerD", "sifreli-deger", "Sifre")))
            .ReadAsJsonAsync<AgentParameterEndpoints.ForaImportResponse>();

        body!.Matched.Should().Be(0);
        body.Unknown.Should().Be(1);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();

        (await db.ParameterValues.AsNoTracking().AnyAsync(v => v.Value == "sifreli-deger"))
            .Should().BeFalse();
    }

    [Fact]
    public async Task A_value_that_equals_the_default_is_marked()
    {
        var (w, client) = await SeedAsync("E");

        var body = await (await UploadAsync(client, w, Row(58, "plasiyerE", "1")))
            .ReadAsJsonAsync<AgentParameterEndpoints.ForaImportResponse>();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();

        // Applying it would store nothing, and the reviewer should see that before wondering why.
        (await db.ForaImportRows.AsNoTracking().SingleAsync(r => r.ForaImportBatchId == body!.BatchId))
            .IsDefaultValue.Should().BeTrue();
    }

    [Fact]
    public async Task An_unassigned_company_is_refused()
    {
        var (mine, client) = await SeedAsync("F");
        var (theirs, _) = await SeedAsync("G");

        var response = await client.PostJsonAsync("/api/v1/agents/parameters/fora-import",
            new { erpCompanyId = theirs.CompanyId, rows = new[] { Row(58, "plasiyerG", "3") } }, mine.Token);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task The_endpoint_needs_an_agent_token()
    {
        var (w, _) = await SeedAsync("H");

        (await _factory.CreateClient().PostJsonAsync("/api/v1/agents/parameters/fora-import",
            new { erpCompanyId = w.CompanyId, rows = Array.Empty<object>() }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
