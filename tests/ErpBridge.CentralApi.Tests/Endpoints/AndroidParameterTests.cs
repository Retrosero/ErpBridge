using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Parameters;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// What the phone receives. The contract is the one older clients already speak; only where the
/// values come from has changed (D13).
/// </summary>
public sealed class AndroidParameterTests : IClassFixture<CentralApiFactory>
{
    private const string Set = "MobilKullanici";

    private readonly CentralApiFactory _factory;

    public AndroidParameterTests(CentralApiFactory factory) => _factory = factory;

    private sealed record World(Guid TenantId, Guid CompanyId, string Database, Guid UserId, Guid DepotId);

    private async Task<(World World, HttpClient Client)> SeedAsync(string code)
    {
        var (tenant, _) = await _factory.SeedTenantAsync($"AND-PRM-{code}", $"Android parametre {code}");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();

        var database = $"MikroDB_V16_{code}";
        var company = new ErpCompany
        {
            TenantId = tenant.Id, Code = $"F{code}", Name = "Merkez", SourceDatabase = database,
            CompanyNo = 1, BranchNo = 0, WarehouseNo = 0,
        };
        db.ErpCompanies.Add(company);

        var user = new MobileUser { TenantId = tenant.Id, Username = $"plasiyer{code}", FullName = "Plasiyer" };
        db.MobileUsers.Add(user);

        // The catalogue is shared by the whole class, so these two rows are planted once; every
        // test seeds its own tenant and sees the same two parameters.
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

        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(
            tenant.Id, $"AK-PRM-{code}", scopes: new[] { "mobile:read" });

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenant.Id.ToString());

        return (new World(tenant.Id, company.Id, database, user.Id, depotId), client);
    }

    private static async Task<JsonElement> BodyAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

    private async Task WriteAsync(World w, string value)
    {
        using var scope = _factory.Services.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<ParameterResolver>();

        await resolver.SetAsync(
            ParameterScope.ForMobileUser(w.TenantId, w.CompanyId, w.UserId), w.DepotId, value,
            new ParameterResolver.ChangeContext(ParameterChangeSources.Panel, Actor: "test"));
    }

    [Fact]
    public async Task Every_parameter_comes_back_even_the_ones_nobody_has_changed()
    {
        var (w, client) = await SeedAsync("A");

        var response = await client.GetAsync($"/api/v1/android/parameters?sourceDatabase={w.Database}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = (await BodyAsync(response)).GetProperty("items").EnumerateArray().ToList();

        // The old mirror only held deviations, so an untouched parameter was simply absent and the
        // phone fell back to whatever it had hard-coded. Now Fora's own default is returned.
        items.Should().HaveCount(2);
        items.Should().OnlyContain(i => i.GetProperty("parametreProgram").GetString() == "akilli");

        var depot = items.Single(i => i.GetProperty("parametreID").GetInt32() == 58);
        depot.GetProperty("parametreDegeri").GetString().Should().Be("1");
        depot.GetProperty("parametreAdi").GetString().Should().Be("DefaultKaynakDepoNo");
    }

    [Fact]
    public async Task The_row_still_names_the_user_the_way_an_older_client_matches_on()
    {
        var (w, client) = await SeedAsync("B");

        var items = (await BodyAsync(await client.GetAsync("/api/v1/android/parameters")))
            .GetProperty("items").EnumerateArray().ToList();

        items.Should().OnlyContain(i => i.GetProperty("parametreUser").GetString() == "plasiyerB");
        items.Should().OnlyContain(i => i.GetProperty("sourceDatabase").GetString() == w.Database);
    }

    [Fact]
    public async Task A_changed_value_replaces_the_default()
    {
        var (w, client) = await SeedAsync("C");
        await WriteAsync(w, "7");

        var items = (await BodyAsync(await client.GetAsync("/api/v1/android/parameters")))
            .GetProperty("items").EnumerateArray().ToList();

        items.Single(i => i.GetProperty("parametreID").GetInt32() == 58)
            .GetProperty("parametreDegeri").GetString().Should().Be("7");
    }

    [Fact]
    public async Task An_unchanged_answer_is_not_downloaded_twice()
    {
        var (_, client) = await SeedAsync("D");

        var first = await client.GetAsync("/api/v1/android/parameters");
        var etag = first.Headers.ETag;
        etag.Should().NotBeNull("the phone pulls 1,801 rows per user and needs a cheap poll");

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/android/parameters");
        request.Headers.IfNoneMatch.Add(etag!);

        (await client.SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task A_change_makes_the_held_copy_stale()
    {
        var (w, client) = await SeedAsync("E");

        var etag = (await client.GetAsync("/api/v1/android/parameters")).Headers.ETag!;
        await WriteAsync(w, "4");

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/android/parameters");
        request.Headers.IfNoneMatch.Add(etag);

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.ETag.Should().NotBe(etag);
    }

    [Fact]
    public async Task A_deleted_user_is_not_published()
    {
        var (w, client) = await SeedAsync("F");
        await WriteAsync(w, "5");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var user = db.MobileUsers.Single(u => u.Id == w.UserId);
            user.IsActive = false;
            user.DeletedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
        }

        // A username is reusable, so a deleted user's permissions must not reach a phone logged in
        // as whoever holds that name next (D5b, R8).
        (await BodyAsync(await client.GetAsync("/api/v1/android/parameters")))
            .GetProperty("items").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Another_tenants_parameters_are_never_returned()
    {
        var (mine, client) = await SeedAsync("G");
        var (theirs, _) = await SeedAsync("H");
        await WriteAsync(theirs, "99");

        var items = (await BodyAsync(await client.GetAsync("/api/v1/android/parameters")))
            .GetProperty("items").EnumerateArray().ToList();

        items.Should().OnlyContain(i => i.GetProperty("sourceDatabase").GetString() == mine.Database);
        items.Should().NotContain(i => i.GetProperty("parametreDegeri").GetString() == "99");
    }

    [Fact]
    public async Task An_unknown_database_matches_nothing_rather_than_everything()
    {
        var (_, client) = await SeedAsync("I");

        (await BodyAsync(await client.GetAsync("/api/v1/android/parameters?sourceDatabase=YOK")))
            .GetProperty("items").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task The_endpoint_needs_a_key()
    {
        await SeedAsync("J");

        (await _factory.CreateClient().GetAsync("/api/v1/android/parameters"))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
