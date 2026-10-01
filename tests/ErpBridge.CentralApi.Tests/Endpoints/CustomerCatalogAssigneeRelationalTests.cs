using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Team;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static ErpBridge.CentralApi.Tests.Endpoints.CatalogCustomerTestSupport;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_MUSTERI_KATALOGU S11: who a new order request goes to — the account's responsible user, then the user mapped to the
/// customer's salesperson code, to the salesperson on its addresses, to the company's default salesperson, then the active
/// route plan that stops at the customer (ERP-less companies too); else only the managers. A passive or deleted user is
/// skipped at every step. The panel previews the same rule (<c>notifyPreview</c>), the users list carries the mapped code and
/// the menu badge reads the counts alone.
/// </summary>
public sealed class CustomerCatalogAssigneeRelationalTests : IClassFixture<CatalogHostFactory>
{
    private readonly CatalogHostFactory _factory;

    public CustomerCatalogAssigneeRelationalTests(CatalogHostFactory factory) => _factory = factory;

    [Fact]
    public async Task The_responsible_user_comes_first_while_active()
    {
        var c = await CompanyAsync(_factory);
        var mudur = await UserIdAsync(c, "mudur");
        await SeedAsync(_factory, db =>
        {
            Record(db, c.Id, "customers", "C1", new { customerCode = "C1", title1 = "Yılmaz", salespersonCode = "P1" });
            db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = c.AliId, TenantId = c.Id, SalespersonCode = "P1" });
        });

        (await AssignAsync(c, "C1", responsible: mudur)).Should().Be(new CatalogAssignee(mudur, CatalogAssigneeSources.Responsible));

        await SeedAsync(_factory, db => db.MobileUsers.Single(u => u.Id == mudur).IsActive = false);
        (await AssignAsync(c, "C1", responsible: mudur)).Should().Be(new CatalogAssignee(c.AliId, CatalogAssigneeSources.Salesperson),
            "a passive responsible user falls through to the salesperson");
    }

    [Fact]
    public async Task The_customers_salesperson_code_maps_to_an_active_user()
    {
        var c = await CompanyAsync(_factory);
        await SeedAsync(_factory, db =>
        {
            Record(db, c.Id, "customers", "C1", new { customerCode = "C1", title1 = "Yılmaz", salespersonCode = " p1 " });
            db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = c.AliId, TenantId = c.Id, SalespersonCode = "P1 " });
        });

        (await AssignAsync(c, "C1")).Should().Be(new CatalogAssignee(c.AliId, CatalogAssigneeSources.Salesperson), "codes match trimmed and in any case");

        await SeedAsync(_factory, db => db.MobileUsers.Single(u => u.Id == c.AliId).DeletedAtUtc = DateTimeOffset.UtcNow);
        (await AssignAsync(c, "C1")).Should().Be(new CatalogAssignee(null, CatalogAssigneeSources.ManagersOnly), "a deleted user hears of nothing");
    }

    [Fact]
    public async Task The_salesperson_on_the_customers_addresses_comes_when_the_cards_code_maps_to_nobody()
    {
        var c = await CompanyAsync(_factory);
        var mudur = await UserIdAsync(c, "mudur");
        await SeedAsync(_factory, db =>
        {
            Record(db, c.Id, "customers", "C1", new { customerCode = "C1", title1 = "Yılmaz", salespersonCode = "NOBODY" });
            Record(db, c.Id, "customerAddresses", "C1|1", new { customerCode = "C1", addressNo = 1, city = "İzmir" });
            Record(db, c.Id, "customerAddresses", "C1|3", new { customerCode = "C1", addressNo = 3, salespersonCode = "P3" });
            Record(db, c.Id, "customerAddresses", "C1|2", new { customerCode = "C1", addressNo = 2, salespersonCode = "P2" });
            db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = c.AliId, TenantId = c.Id, SalespersonCode = "P2" });
            db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = mudur, TenantId = c.Id, SalespersonCode = "P3" });
        });

        var customers = await ReadAsync(_factory, db => PortalLedger.CustomersAsync(db, Cache, c.Id, CancellationToken.None));
        customers["C1"].Should().Match<PortalLedger.Customer>(x => x.AddressSalespersonCode == "P2" && x.City == "İzmir",
            "the lowest-numbered address that names a salesperson; the address text still comes from the first address");
        (await AssignAsync(c, "C1")).Should().Be(new CatalogAssignee(c.AliId, CatalogAssigneeSources.Address));
    }

    [Fact]
    public async Task The_companys_default_salesperson_comes_next()
    {
        var c = await CompanyAsync(_factory);
        await SeedAsync(_factory, db =>
        {
            Record(db, c.Id, "customers", "C1", new { customerCode = "C1", title1 = "Yılmaz" });
            db.ErpWriteSettings.Add(new ErpWriteSettings { TenantId = c.Id, DefaultSalespersonCode = "MERKEZ" });
            db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = c.AliId, TenantId = c.Id, SalespersonCode = "merkez" });
        });

        (await AssignAsync(c, "C1")).Should().Be(new CatalogAssignee(c.AliId, CatalogAssigneeSources.Default));
    }

    [Fact]
    public async Task The_current_route_plan_that_stops_at_the_customer_names_its_first_active_assignee()
    {
        var c = await CompanyAsync(_factory);
        var mudur = await UserIdAsync(c, "mudur");
        await SeedAsync(_factory, db =>
        {
            Record(db, c.Id, "customers", "C1", new { customerCode = "C1", title1 = "Yılmaz", salespersonCode = "NOBODY" });
            db.MobileUsers.Single(u => u.Id == mudur).IsActive = false;
            // Passed over: switched off, starting in the future, not stopping at C1.
            Plan(db, c.Id, "off", "2026-01-01", isActive: false, ["ali"], "C1");
            Plan(db, c.Id, "future", "2999-01-01", isActive: true, ["ali"], "C1");
            Plan(db, c.Id, "elsewhere", "2026-09-01", isActive: true, ["ali"], "C2");
            // The newest current plan wins; its first assignee is passive, so the next one hears of it.
            Plan(db, c.Id, "old", "2026-01-01", isActive: true, ["patron"], "C1");
            Plan(db, c.Id, "new", "2026-06-01", isActive: true, ["mudur", "ALI"], "c1");
        });

        (await AssignAsync(c, "C1")).Should().Be(new CatalogAssignee(c.AliId, CatalogAssigneeSources.Route));
    }

    [Fact]
    public async Task Without_any_rule_only_the_managers_hear_of_it()
    {
        var c = await CompanyAsync(_factory);
        await SeedAsync(_factory, db => Record(db, c.Id, "customers", "C1", new { customerCode = "C1", title1 = "Yılmaz" }));

        (await AssignAsync(c, "C1")).Should().Be(new CatalogAssignee(null, CatalogAssigneeSources.ManagersOnly));
        (await AssignAsync(c, "UNKNOWN")).Should().Be(new CatalogAssignee(null, CatalogAssigneeSources.ManagersOnly), "a customer without a card too");
    }

    [Fact]
    public async Task Without_an_ERP_a_request_goes_to_the_route_plans_assignee_and_the_panel_previews_it()
    {
        var c = await OpenCatalogAsync(_factory);
        await SeedAsync(_factory, db =>
        {
            db.Tenants.Single(t => t.Id == c.Id).DataSource = TenantDataSources.Native;
            Plan(db, c.Id, "pazartesi", "", isActive: true, ["ali"], "C2");
        });
        await AccountAsync(_factory, c, "C2", "akgida", "musteri123");

        var preview = (await OkAsync<CatalogAccountByCustomerResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/accounts/by-customer?code=C2", c.Mudur))).NotifyPreview;
        preview.Should().BeEquivalentTo(new CatalogNotifyPreviewDto { UserId = c.AliId, UserName = "ali bey", Source = "route" });

        var browser = Browser(_factory);
        (await LoginAsync(browser, c, "akgida", "musteri123")).StatusCode.Should().Be(HttpStatusCode.OK);
        var order = (await OkAsync<CatalogOrderResponse>(await SendAsync(browser, HttpMethod.Post, Api(c) + "/orders", new
        {
            requestId = Guid.NewGuid(),
            lines = new[] { new { key = "A", quantity = 1m } },
            expectedTotal = 100m,
        }), HttpStatusCode.Created)).Order;
        (await ReadAsync(_factory, db => db.CatalogOrders.AsNoTracking().SingleAsync(o => o.Id == order.Id))).AssignedUserId.Should().Be(c.AliId);
        (await ReadAsync(_factory, db => db.UserNotifications.AsNoTracking().AnyAsync(n => n.TenantId == c.Id && n.UserId == c.AliId))).Should().BeTrue();
    }

    [Fact]
    public async Task The_preview_names_the_saved_responsible_user_and_says_when_nobody_hears_of_it()
    {
        var c = await OpenCatalogAsync(_factory);
        var mudur = await UserIdAsync(c, "mudur");

        var nobody = (await OkAsync<CatalogAccountByCustomerResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/accounts/by-customer?code=C1", c.Mudur))).NotifyPreview;
        nobody.Should().BeEquivalentTo(new CatalogNotifyPreviewDto { UserId = null, UserName = null, Source = "managersOnly" }, "C1's P1 is mapped to nobody");

        var account = await AccountAsync(_factory, c, "C1", "yilmaz");
        (await SendAsync(_factory, HttpMethod.Patch, $"{Base}/accounts/{account}", c.Mudur, new { responsibleUserId = mudur })).StatusCode.Should().Be(HttpStatusCode.OK);
        var responsible = (await OkAsync<CatalogAccountByCustomerResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/accounts/by-customer?code=C1", c.Mudur))).NotifyPreview;
        responsible.Should().BeEquivalentTo(new CatalogNotifyPreviewDto { UserId = mudur, UserName = "mudur bey", Source = "responsible" });
    }

    [Fact]
    public async Task The_users_list_carries_the_mapped_salesperson_code_and_the_badge_reads_the_counts_alone()
    {
        var c = await OpenCatalogAsync(_factory);
        await SeedAsync(_factory, db =>
        {
            db.MobileUserErpMappings.Add(new MobileUserErpMapping { UserId = c.AliId, TenantId = c.Id, SalespersonCode = " P1 " });
            foreach (var (no, status, assigned) in new[] { ("KT-CNT001", CatalogOrderStatuses.New, (Guid?)c.AliId), ("KT-CNT002", CatalogOrderStatuses.New, null), ("KT-CNT003", CatalogOrderStatuses.Rejected, c.AliId) })
                db.CatalogOrders.Add(new CatalogOrder
                {
                    Id = Guid.NewGuid(), TenantId = c.Id, AccountId = Guid.NewGuid(), No = no, CustomerCode = "C1", CustomerName = "Yılmaz", AccountUsername = "yilmaz",
                    Status = status, LinesJson = "[]", AssignedUserId = assigned, SubmittedAtMs = 1, UpdatedAtMs = 1,
                });
        });

        var users = await OkAsync<MobileUserListResponse>(await SendAsync(_factory, HttpMethod.Get, "/api/v1/android/account/users", c.Patron));
        users.Users.Single(u => u.Username == "ali").SalespersonCode.Should().Be("P1");
        users.Users.Single(u => u.Username == "mudur").SalespersonCode.Should().BeNull();

        (await OkAsync<CatalogOrderCountsDto>(await SendAsync(_factory, HttpMethod.Get, Base + "/orders/counts", c.Mudur)))
            .Should().BeEquivalentTo(new CatalogOrderCountsDto { New = 2, Rejected = 1 }, "a manager counts every request");
        (await OkAsync<CatalogOrderCountsDto>(await SendAsync(_factory, HttpMethod.Get, Base + "/orders/counts", c.Ali)))
            .Should().BeEquivalentTo(new CatalogOrderCountsDto { New = 1, Rejected = 1 }, "the others count their own, as the list shows them");
        var company = await CompanyAsync(_factory, withModule: false);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, Base + "/orders/counts", company.Patron), HttpStatusCode.Forbidden, "MODULE_NOT_ENABLED");
    }

    private IMemoryCache Cache => _factory.Services.GetRequiredService<IMemoryCache>();

    private async Task<CatalogAssignee> AssignAsync(CatalogCompany company, string customerCode, Guid? responsible = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var card = (await PortalLedger.CustomersAsync(db, Cache, company.Id, CancellationToken.None)).GetValueOrDefault(customerCode);
        return await CatalogOrders.AssigneeAsync(db, Cache, company.Id, responsible, customerCode, card, CancellationToken.None);
    }

    private Task<Guid> UserIdAsync(CatalogCompany company, string username) =>
        ReadAsync(_factory, db => db.MobileUsers.Where(u => u.TenantId == company.Id && u.Username == username).Select(u => u.Id).SingleAsync());

    /// <summary>A route plan as <see cref="TeamDocumentProcessor"/> projects it.</summary>
    private static void Plan(CentralApiDbContext db, Guid tenantId, string planId, string startDate, bool isActive, string[] assignees, params string[] customers) =>
        Record(db, tenantId, TeamDocumentProcessor.RoutePlansSection, planId, new
        {
            planId,
            name = planId,
            description = "",
            startDate,
            isActive,
            stops = customers.Select((code, i) => new { stopId = $"{planId}-{i}", dayOfWeek = 1, customerCode = code, customerName = code, visitOrder = i }).ToArray(),
            assignees,
            updatedBy = "patron",
            updatedAtUtc = "2026-09-30T10:00:00.0000000+00:00",
        });
}
