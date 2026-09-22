using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>GOAL_PANEL_ERPSIZ E7b/D5: the /denetim page — who changed a card/payment from the portal, and when.</summary>
public sealed class PortalNativeAuditPageTests : PortalPageTestContext
{
    private const string UsersPath = "/api/v1/android/account/users";

    private static string DefaultQuery() =>
        $"/api/v1/portal/native/audit?from={Fmt.IsoDay(Fmt.Today().AddDays(-6))}&to={Fmt.IsoDay(Fmt.Today())}&page=1&pageSize=50";

    private static object Users() => new
    {
        seats = new { max = 5, used = 1, status = "active" },
        users = new object[] { new { id = Guid.NewGuid(), username = "patron", fullName = "Firma Sahibi", role = "ADMIN", isActive = true } },
    };

    private static object Row(Guid id, string entity, string key, string action, string summary) => new
    {
        id, entity, entityKey = key, action, summary, beforeJson = (string?)null, afterJson = (string?)null,
        userId = Guid.NewGuid(), userName = "Firma Sahibi", createdAtUtc = DateTimeOffset.UtcNow,
    };

    private static readonly Guid A1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid A2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static object Page(params object[] items) => new { items, total = items.Length, page = 1, pageSize = 50 };

    [Fact]
    public void Lists_every_write_with_its_action_and_summary()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(UsersPath, Users());
        api.Answer(DefaultQuery(), Page(
            Row(A1, "stock_card", "CAY-1", "create", "Ürün oluşturuldu: CAY-1 — Çay 1 kg"),
            Row(A2, "collection", "C-001", "create", "Tahsilat: 300,00 TL — C-001")));

        var cut = Render<Denetim>();

        cut.WaitForAssertion(() => cut.FindAll("#audit-table tbody tr").Count.Should().Be(2));
        cut.Find("tr[data-audit='11111111-1111-1111-1111-111111111111']").TextContent.Should().Contain("Ürün").And.Contain("Oluşturuldu").And.Contain("CAY-1");
        cut.Find("tr[data-audit='22222222-2222-2222-2222-222222222222']").TextContent.Should().Contain("Tahsilat").And.Contain("300,00");
        cut.Find("#audit-range").TextContent.Should().Contain("2");
    }

    [Fact]
    public void An_empty_range_shows_the_empty_state()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(UsersPath, Users());
        api.Answer(DefaultQuery(), Page());

        var cut = Render<Denetim>();

        cut.WaitForAssertion(() => cut.Find("#audit-empty"));
    }

    [Fact]
    public void Filtering_by_entity_asks_the_server_again()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(UsersPath, Users());
        api.Answer(DefaultQuery(), Page(Row(A1, "stock_card", "CAY-1", "create", "Ürün oluşturuldu: CAY-1")));
        var filtered = $"/api/v1/portal/native/audit?entity=collection&from={Fmt.IsoDay(Fmt.Today().AddDays(-6))}&to={Fmt.IsoDay(Fmt.Today())}&page=1&pageSize=50";
        api.Answer(filtered, Page(Row(A2, "collection", "C-001", "create", "Tahsilat: 100,00 TL — C-001")));

        var cut = Render<Denetim>();
        cut.WaitForAssertion(() => cut.FindAll("#audit-table tbody tr").Count.Should().Be(1));

        cut.Find("#audit-entity").Change("collection");
        cut.Find("#audit-filter").Submit();

        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.Method == HttpMethod.Get && r.PathAndQuery == filtered));
    }

    [Fact]
    public void An_erp_company_sees_why_there_is_nothing()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { DataSource = "erp" });

        var cut = Render<Denetim>();

        cut.WaitForAssertion(() => cut.Find("#native-audit-not-applicable"));
    }

    [Fact]
    public void A_manager_cannot_open_the_page()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "MANAGER"));
        var nav = Services.GetRequiredService<NavigationManager>();

        Render<Denetim>();

        nav.Uri.Should().NotContain("denetim", "only ADMIN opens the audit trail (D4)");
    }
}
