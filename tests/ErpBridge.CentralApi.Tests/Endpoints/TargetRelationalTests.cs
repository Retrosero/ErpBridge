using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Targets;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Targets and teams (GOAL_HEDEF_RUT): who may set what, the team scope, and actual figures from an ERP-less
/// company's documents, an ERP company's Mikro rows (by salesperson code) and the phones' documents as fallback.
/// </summary>
public sealed class TargetRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private static readonly DateOnly Today = TargetService.Today();
    private static readonly string Day = Today.ToString("yyyy-MM-dd");
    private static readonly TargetPeriod Month = TargetPeriod.Of(TargetPeriodTypes.Monthly, Today);

    private readonly SqliteCentralApiFactory _factory;

    public TargetRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Field_users_neither_read_nor_write_the_panel_targets()
    {
        var c = await CompanyAsync(native: true);

        await ExpectAsync(await GetAsync(c.Ali, "/api/v1/portal/targets"), HttpStatusCode.Forbidden, "PORTAL_REQUIRES_MANAGER");
        await ExpectAsync(await SendAsync(HttpMethod.Put, c.Ali, "/api/v1/portal/targets", Save(Revenue("USER", c.AliId, 100m))), HttpStatusCode.Forbidden, "PORTAL_REQUIRES_MANAGER");
        await ExpectAsync(await GetAsync(c.Ali, "/api/v1/android/targets/team"), HttpStatusCode.Forbidden, "TARGETS_REQUIRE_MANAGER");
        (await GetAsync(c.Sef, "/api/v1/portal/targets")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetAsync(c.Ali, "/api/v1/android/targets/mine")).StatusCode.Should().Be(HttpStatusCode.OK, "everyone reads their own");
    }

    [Fact]
    public async Task Without_an_erp_the_booked_documents_are_the_actual_figures()
    {
        var c = await CompanyAsync(native: true);
        (await SaveAsync(c.Patron, Revenue("USER", c.AliId, 10_000m), Revenue("COMPANY", null, 50_000m),
            new TargetInput { PeriodType = "MONTHLY", PeriodKey = Month.Key, Metric = "COLLECTION", OwnerKind = "USER", OwnerId = c.AliId, Value = 2_000m },
            new TargetInput { PeriodType = "MONTHLY", PeriodKey = Month.Key, Metric = "PRODUCT", Measure = "QUANTITY", ItemCode = "CAY-1", OwnerKind = "USER", OwnerId = c.AliId, Value = 20m }))
            .Saved.Should().Be(4);
        await PostAsync(c, c.Ali, "sales_order", "SO-1", Sale("SO-1", 3, 150m, "Cari Borç"));
        await PostAsync(c, c.Ali, "sales_order", "SO-2", Sale("SO-2", 1, 150m, "Nakit"));
        await PostAsync(c, c.Ali, "collection", "TAH-1", new { mobileDocumentId = "TAH-1", occurredAt = Day + "T15:00:00", counterparty = "Bakkal Ali", customerCode = "C-001", amount = 200, paymentType = "Nakit" });
        await PostAsync(c, c.Mehmet, "sales_order", "SO-M", Sale("SO-M", 2, 150m, "Cari Borç"));

        var board = await GetJsonAsync<TargetBoardResponse>(c.Patron, $"/api/v1/portal/targets?periodType=MONTHLY&periodKey={Month.Key}");
        var ali = board.Owners.Single(o => o.OwnerKind == "USER" && o.OwnerId == c.AliId);
        var company = board.Owners.Single(o => o.OwnerKind == "COMPANY");

        board.Source.Should().Be(TargetSources.ServerDocuments);
        ali.Summary.Revenue.Should().Be(600m);
        ali.Summary.Collection.Should().Be(350m, "the cash sale is collected on the spot");
        ali.Summary.DocumentCount.Should().Be(2);
        ali.Targets.Single(t => t.Metric == "REVENUE").Actual.Should().Be(600m);
        ali.Targets.Single(t => t.Metric == "PRODUCT").Actual.Should().Be(4m);
        company.Summary.Revenue.Should().Be(900m);
        company.Targets.Single(t => t.Metric == "REVENUE").ChildrenSum.Should().Be(10_000m);

        var mine = await GetJsonAsync<MyTargetsResponse>(c.Ali, $"/api/v1/android/targets/mine?date={Day}");
        mine.Periods.Select(p => p.PeriodType).Should().Equal("DAILY", "WEEKLY", "MONTHLY");
        mine.Periods[2].Targets.Single(t => t.Metric == "REVENUE").Actual.Should().Be(600m);
        mine.Periods[0].Targets.Should().Contain(t => t.Metric == "REVENUE" && t.Derived, "the day's share of the month");
        mine.CanViewTeam.Should().BeFalse();
    }

    [Fact]
    public async Task A_save_is_all_or_nothing_and_a_repeated_one_is_not_applied_twice()
    {
        var c = await CompanyAsync(native: true);
        var bad = new TargetsSaveRequest
        {
            OperationId = Guid.NewGuid(),
            Items = [Revenue("USER", c.AliId, 100m), new TargetInput { PeriodType = "MONTHLY", PeriodKey = "2026-13", Metric = "REVENUE", OwnerKind = "USER", OwnerId = c.AliId, Value = 1 }],
        };
        var refused = await SendAsync(HttpMethod.Put, c.Patron, "/api/v1/portal/targets", bad);
        refused.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await refused.ReadAsJsonAsync<TargetsSaveResponse>()).Errors.Single().Index.Should().Be(1);

        var request = Save(Revenue("USER", c.AliId, 100m));
        (await SendJsonAsync<TargetsSaveResponse>(HttpMethod.Put, c.Patron, "/api/v1/portal/targets", request)).Saved.Should().Be(1);
        (await SendJsonAsync<TargetsSaveResponse>(HttpMethod.Put, c.Patron, "/api/v1/portal/targets", request)).Duplicate.Should().BeTrue();

        (await SaveAsync(c.Patron, Revenue("USER", c.AliId, null))).Deleted.Should().Be(1);
        var board = await GetJsonAsync<TargetBoardResponse>(c.Patron, $"/api/v1/portal/targets?periodType=MONTHLY&periodKey={Month.Key}");
        board.Owners.Single(o => o.OwnerId == c.AliId).Targets.Should().NotContain(t => t.Metric == "REVENUE" && !t.Derived);
    }

    [Fact]
    public async Task A_manager_responsible_for_a_team_works_only_inside_it()
    {
        var c = await CompanyAsync(native: true);
        var team = await CreateTeamAsync(c.Patron, new { name = "Anadolu", kind = "TEAM", memberIds = new[] { c.AliId }, managerIds = new[] { c.SefId } });

        (await SaveAsync(c.Sef, Revenue("USER", c.AliId, 100m), Revenue("TEAM", team.Id, 500m))).Saved.Should().Be(2);
        await ExpectAsync(await SendAsync(HttpMethod.Put, c.Sef, "/api/v1/portal/targets", Save(Revenue("USER", c.MehmetId, 100m))), HttpStatusCode.Forbidden, "TARGET_OUT_OF_SCOPE");
        await ExpectAsync(await SendAsync(HttpMethod.Put, c.Sef, "/api/v1/portal/targets", Save(Revenue("COMPANY", null, 100m))), HttpStatusCode.Forbidden, "TARGET_OUT_OF_SCOPE");
        (await SaveAsync(c.Mudur, Revenue("COMPANY", null, 100m), Revenue("USER", c.MehmetId, 100m))).Saved.Should().Be(2, "a manager without a team sees the whole company");

        var board = await GetJsonAsync<TargetBoardResponse>(c.Sef, "/api/v1/portal/targets");
        board.WholeCompany.Should().BeFalse();
        board.Owners.Select(o => o.OwnerKind == "TEAM" ? "team" : o.OwnerId == c.AliId ? "ali" : o.OwnerId == c.SefId ? "sef" : "other")
            .Should().BeEquivalentTo(["team", "ali", "sef"]);
        await ExpectAsync(await SendAsync(HttpMethod.Post, c.Sef, "/api/v1/portal/teams", new { name = "Yeni", kind = "TEAM" }), HttpStatusCode.Forbidden, "TEAM_ADMIN_REQUIRED");
        (await GetJsonAsync<TargetBoardResponse>(c.Sef, "/api/v1/android/targets/team")).Owners.Should().HaveCount(3);
    }

    [Fact]
    public async Task A_person_belongs_to_one_team_and_a_region_with_teams_cannot_be_deleted()
    {
        var c = await CompanyAsync(native: true);
        var region = await CreateTeamAsync(c.Patron, new { name = "Marmara", kind = "REGION" });
        var a = await CreateTeamAsync(c.Patron, new { name = "A", kind = "TEAM", parentId = region.Id, memberIds = new[] { c.AliId, c.MehmetId } });
        var b = await CreateTeamAsync(c.Patron, new { name = "B", kind = "TEAM", memberIds = new[] { c.AliId } });

        var teams = await GetJsonAsync<TeamsResponse>(c.Patron, "/api/v1/portal/teams");
        teams.Teams.Single(t => t.Id == a.Id).MemberIds.Should().Equal(c.MehmetId);
        teams.Teams.Single(t => t.Id == b.Id).MemberIds.Should().Equal(c.AliId);
        teams.People.Single(p => p.Id == c.AliId).TeamId.Should().Be(b.Id);

        await ExpectAsync(await SendAsync(HttpMethod.Delete, c.Patron, $"/api/v1/portal/teams/{region.Id}", null), HttpStatusCode.Conflict, "TEAM_HAS_CHILDREN");
        await ExpectAsync(await SendAsync(HttpMethod.Post, c.Patron, "/api/v1/portal/teams", new { name = "a", kind = "TEAM" }), HttpStatusCode.Conflict, "TEAM_NAME_TAKEN");
        await ExpectAsync(await SendAsync(HttpMethod.Put, c.Patron, $"/api/v1/portal/teams/{b.Id}", new { name = "B", managerIds = new[] { c.AliId } }), HttpStatusCode.BadRequest, "TEAM_INVALID");
        (await SendAsync(HttpMethod.Delete, c.Patron, $"/api/v1/portal/teams/{a.Id}", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await SendAsync(HttpMethod.Delete, c.Patron, $"/api/v1/portal/teams/{region.Id}", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task With_an_erp_mikro_rows_count_by_salesperson_code_and_phone_documents_show_as_pending()
    {
        var c = await CompanyAsync(native: false);
        await SeedAsync(c, db =>
        {
            db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = c.AliId, TenantId = c.Id, SalespersonCode = "PL01" });
            var seq = 3_100_000L;
            void Add(string entity, string key, object payload) => db.MobileRecords.Add(new MobileRecord
            {
                TenantId = c.Id, Entity = entity, RecordKey = key, UpdatedSeq = ++seq, PayloadJson = JsonSerializer.Serialize(payload, Web),
            });
            var tarih = Day + "T00:00:00";
            Add("stocks", "CAY-1", new { stockCode = "CAY-1", name = "Çay 1 kg", mainGroupCode = "ICECEK", brandCode = "RIZE" });
            // A sales invoice line of PL01: 1.000 before a 100 discount, without VAT.
            Add("stockTransactions", "1", new { id = "1", erp = "MIKRO", stokKod = "CAY-1", tarih, tip = 1, evrakTip = 4, evrakNo = "F-1", cikisMiktar = 10m, tutar = 1000m, discountAmount = 100m, cariKod = "C-1", faturaRecno = 11, plasiyerKod = "PL01" });
            // Its customer's return, a warehouse transfer (not a sale), and an office sale of another salesperson.
            Add("stockTransactions", "2", new { id = "2", erp = "MIKRO", stokKod = "CAY-1", tarih, tip = 3, evrakTip = 3, evrakNo = "I-1", girisMiktar = 2m, tutar = 200m, cariKod = "C-1", faturaRecno = 12, plasiyerKod = "PL01" });
            Add("stockTransactions", "3", new { id = "3", erp = "MIKRO", stokKod = "CAY-1", tarih, tip = 2, evrakTip = 2, evrakNo = "D-1", cikisMiktar = 5m, tutar = 500m, cariKod = "", plasiyerKod = (string?)null });
            Add("stockTransactions", "4", new { id = "4", erp = "MIKRO", stokKod = "CAY-1", tarih, tip = 1, evrakTip = 1, evrakNo = "IR-1", cikisMiktar = 1m, tutar = 400m, cariKod = "C-2", plasiyerKod = "PL99" });
            Add("customerTransactions", "t1", new { id = "t1", erp = "MIKRO", cariKod = "C-1", tarih, tutar = 300m, borcMu = false, type = "TAHSILAT", plasiyerKod = "PL01" });
        });
        (await SaveAsync(c.Patron, Revenue("USER", c.AliId, 2_000m),
            new TargetInput { PeriodType = "MONTHLY", PeriodKey = Month.Key, Metric = "BRAND", ItemCode = "RIZE", OwnerKind = "USER", OwnerId = c.AliId, Value = 1_000m })).Saved.Should().Be(2);
        (await PostAsync(c, c.Ali, "sales_order", "ERP-SO", Sale("ERP-SO", 2, 100m, "Cari Borç"))).Status.Should().Be("Pending");

        var board = await GetJsonAsync<TargetBoardResponse>(c.Patron, $"/api/v1/portal/targets?periodType=MONTHLY&periodKey={Month.Key}");
        var ali = board.Owners.Single(o => o.OwnerId == c.AliId);

        board.Source.Should().Be(TargetSources.ErpMirror);
        ali.Summary.Revenue.Should().Be(700m);
        ali.Summary.Collection.Should().Be(300m);
        ali.Summary.PendingRevenue.Should().Be(200m);
        ali.Targets.Single(t => t.Metric == "BRAND").Actual.Should().Be(700m);
        ali.Warning.Should().BeNull();
        board.Owners.Single(o => o.OwnerId == c.MehmetId).Warning.Should().Contain("Plasiyer kodu eşlenmemiş");
        board.Owners.Single(o => o.OwnerKind == "COMPANY").Summary.Revenue.Should().Be(1_100m, "the office sale counts for the company");
    }

    [Fact]
    public async Task While_the_agent_sends_no_salesperson_the_phone_documents_stand_in()
    {
        var c = await CompanyAsync(native: false);
        await SeedAsync(c, db => db.MobileRecords.Add(new MobileRecord
        {
            TenantId = c.Id, Entity = "stockTransactions", RecordKey = "1", UpdatedSeq = 3_200_001,
            PayloadJson = JsonSerializer.Serialize(new { id = "1", erp = "MIKRO", stokKod = "CAY-1", tarih = Day + "T00:00:00", tip = 1, evrakTip = 4, cikisMiktar = 1m, tutar = 999m, cariKod = "C-1" }, Web),
        }));
        await PostAsync(c, c.Ali, "sales_order", "ERP-SO", Sale("ERP-SO", 2, 100m, "Cari Borç"));

        var mine = await GetJsonAsync<MyTargetsResponse>(c.Ali, "/api/v1/android/targets/mine");

        mine.Source.Should().Be(TargetSources.PhoneDocuments);
        mine.Warnings.Should().ContainSingle();
        mine.Periods[2].Summary.Revenue.Should().Be(200m);
    }

    [Fact]
    public async Task Copying_raises_last_months_targets_and_distributing_shares_a_value_out()
    {
        var c = await CompanyAsync(native: true);
        var previous = Month.Previous();
        (await SaveAsync(c.Patron, Revenue("USER", c.AliId, 1_000m, previous.Key), Revenue("USER", c.MehmetId, 2_000m, previous.Key))).Saved.Should().Be(2);

        var copy = await SendJsonAsync<TargetPreviewResponse>(HttpMethod.Post, c.Patron, "/api/v1/portal/targets/copy", new
        {
            periodType = "MONTHLY", fromPeriodKey = previous.Key, toPeriodKey = Month.Key, percent = 10, apply = true, operationId = Guid.NewGuid(),
        });
        copy.Items.Select(i => i.Value).Should().BeEquivalentTo(new decimal?[] { 1_100m, 2_200m });
        copy.Result!.Saved.Should().Be(2);

        var shares = await SendJsonAsync<TargetPreviewResponse>(HttpMethod.Post, c.Patron, "/api/v1/portal/targets/distribute", new
        {
            periodType = "MONTHLY", periodKey = Month.Key, metric = "REVENUE", sourceOwnerKind = "COMPANY", value = 1_000m, method = "EQUAL", targetLevel = "USER",
        });
        shares.Items.Should().HaveCount(5, "every phone user of the company: patron, sef, mudur, ali, mehmet");
        shares.Items.Sum(i => i.Value).Should().Be(1_000m);
    }

    [Fact]
    public async Task Another_companys_people_are_never_an_owner()
    {
        var mine = await CompanyAsync(native: true);
        var other = await CompanyAsync(native: true);

        var refused = await SendAsync(HttpMethod.Put, mine.Patron, "/api/v1/portal/targets", Save(Revenue("USER", other.AliId, 100m)));

        refused.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await refused.Content.ReadAsStringAsync()).Should().Contain("TARGET_INVALID_OWNER");
        (await GetJsonAsync<TargetBoardResponse>(mine.Patron, "/api/v1/portal/targets")).Owners.Should().NotContain(o => o.OwnerId == other.AliId);
    }

    // ---- helpers -----------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Sef, string Mudur, string Ali, string Mehmet, Guid SefId, Guid AliId, Guid MehmetId);

    private static TargetInput Revenue(string ownerKind, Guid? ownerId, decimal? value, string? periodKey = null) => new()
    {
        PeriodType = "MONTHLY", PeriodKey = periodKey ?? Month.Key, Metric = "REVENUE", OwnerKind = ownerKind, OwnerId = ownerId, Value = value,
    };

    private static TargetsSaveRequest Save(params TargetInput[] items) => new() { OperationId = Guid.NewGuid(), Items = items };

    private Task<TargetsSaveResponse> SaveAsync(string token, params TargetInput[] items) =>
        SendJsonAsync<TargetsSaveResponse>(HttpMethod.Put, token, "/api/v1/portal/targets", Save(items));

    private Task<TeamDto> CreateTeamAsync(string token, object body) => SendJsonAsync<TeamDto>(HttpMethod.Post, token, "/api/v1/portal/teams", body, HttpStatusCode.Created);

    private static object Sale(string id, int quantity, decimal unitPrice, string paymentType) => new
    {
        mobileDocumentId = id,
        occurredAt = Day + "T10:00:00",
        counterparty = "Bakkal Ali",
        customerCode = "C-001",
        amount = quantity * unitPrice,
        paymentType,
        lines = new[] { new { barcode = "8690000000011", productCode = "CAY-1", productTitle = "Çay 1 kg", quantity, unitPrice, lineTotal = quantity * unitPrice } },
    };

    private async Task SeedAsync(Company c, Action<CentralApiDbContext> seed)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        seed(db);
        await db.SaveChangesAsync();
    }

    private async Task<Company> CompanyAsync(bool native)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"HEDEF-{suffix}", $"Hedef tenant {suffix}");
        var adminToken = _factory.IssueAdminJwt((await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local")).Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        if (native)
            (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 8, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("sef", "MANAGER"), ("mudur", "MANAGER"), ("ali", "SALES"), ("mehmet", "SALES") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;

        var patron = await LoginAsync(code, "patron", $"DEV-P-{suffix}");
        var people = (await GetJsonAsync<TeamsResponse>(patron, "/api/v1/portal/teams")).People.ToDictionary(p => p.Username, p => p.Id);
        var company = new Company(tenant.Id, patron,
            await LoginAsync(code, "sef", $"DEV-S-{suffix}"), await LoginAsync(code, "mudur", $"DEV-D-{suffix}"),
            await LoginAsync(code, "ali", $"DEV-A-{suffix}"), await LoginAsync(code, "mehmet", $"DEV-M-{suffix}"),
            people["sef"], people["ali"], people["mehmet"]);

        var rules = ApprovalKinds.All.ToDictionary(kind => kind, _ => false);
        (await SendAsync(HttpMethod.Put, patron, "/api/v1/android/approvals/rules", new { rules })).StatusCode.Should().Be(HttpStatusCode.OK);
        if (native)
        {
            await PostAsync(company, patron, "stock_card", "CARD-S1", new { stockCode = "CAY-1", name = "Çay 1 kg", barcode = "8690000000011", price = 150, openingQuantity = 40, kategori = "ICECEK" });
            await PostAsync(company, patron, "customer_card", "CARD-C1", new { customerCode = "C-001", title = "Bakkal Ali" });
        }
        return company;
    }

    private async Task<IngestJobResponse> PostAsync(Company c, string token, string documentType, string externalId, object payload)
    {
        var response = await SendAsync(HttpMethod.Post, token, "/api/v1/ingest/jobs", new { externalId, documentType, payload }, c.Id);
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

    private async Task<T> SendJsonAsync<T>(HttpMethod method, string token, string path, object body, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var response = await SendAsync(method, token, path, body);
        response.StatusCode.Should().Be(expected, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }

    private static async Task ExpectAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        var text = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(status, text);
        text.Should().Contain(code);
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "hedef-test" });
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
