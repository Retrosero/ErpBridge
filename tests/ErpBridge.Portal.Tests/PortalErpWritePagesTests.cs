using System.Net;
using Bunit;
using ErpBridge.Portal.Pages;
using ErpBridge.Portal.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>Goal ERP yazım Y1c: the ERP write settings page and the users page's Mikro counterparts.</summary>
public sealed class PortalErpWritePagesTests : PortalPageTestContext
{
    private static readonly Guid AliId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static PortalSessionState ErpAdmin() => PortalTestSetup.State() with { DataSource = "erp" };

    private static object Settings() => new
    {
        salesDocumentKind = "order", orderApprovalMode = "approved",
        series = new { order = "", dispatch = "", invoice = "", @return = "", collection = "" },
        chequePortfolioCode = "ÇEK", notePortfolioCode = "SENET",
    };

    private static object Lookups() => new
    {
        warehouses = new[] { new { code = "1", name = "Merkez" }, new { code = "2", name = "Şube" } },
        cashAccounts = new[] { new { code = "001", name = "Merkez kasa" }, new { code = "002", name = "Araç kasası" } },
        banks = new[] { new { code = "14", name = "Ziraat" } },
        salespersons = Array.Empty<object>(),
        priceLists = new[] { new { code = "1", name = "Perakende" } },
        projects = Array.Empty<object>(),
    };

    [Fact]
    public void An_administrator_saves_the_document_kind_series_and_accounts()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: ErpAdmin());
        api.Answer("/api/v1/portal/erp-settings", Settings());
        api.Answer("/api/v1/portal/erp-lookups", Lookups());

        var cut = Render<ErpAktarim>();
        cut.WaitForAssertion(() => cut.Find("#erp-settings"));

        cut.Find("#erp-kind").Change("invoice");
        cut.Find("#erp-series-invoice").Change(" T ");
        cut.Find("#erp-warehouse").Change("2");
        cut.Find("#erp-cash").Change("001");
        cut.Find("#erp-salesperson").Change(" PLS01 ");
        cut.Find("#erp-user-no").Change("4");
        cut.Find("#erp-settings").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("kaydedildi"));
        var saved = api.Requests.Single(r => r.Method == HttpMethod.Put).Body;
        saved.Should().Contain("\"salesDocumentKind\":\"invoice\"")
            .And.Contain("\"invoice\":\"T\"")
            .And.Contain("\"defaultWarehouseNo\":2")
            .And.Contain("\"defaultCashCode\":\"001\"")
            .And.Contain("\"defaultSalespersonCode\":\"PLS01\"", "no salesperson list came from the ERP, so it is free text")
            .And.Contain("\"defaultErpUserNo\":4");
        cut.Find("#erp-cash").TagName.Should().Be("SELECT", "the ERP sent its cash boxes");
        cut.Find("#erp-salesperson").TagName.Should().Be("INPUT");
    }

    [Fact]
    public void A_setting_the_server_refuses_is_explained()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: ErpAdmin());
        api.Answer("/api/v1/portal/erp-settings", Settings());
        api.Answer("/api/v1/portal/erp-lookups", Lookups());

        var cut = Render<ErpAktarim>();
        cut.WaitForAssertion(() => cut.Find("#erp-settings"));
        api.Fail("/api/v1/portal/erp-settings", HttpStatusCode.BadRequest, "INVALID_ERP_SETTINGS");

        cut.Find("#erp-settings").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("seri en çok 6"));
    }

    [Fact]
    public void A_company_without_an_erp_sees_that_there_is_nothing_to_set()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());

        var cut = Render<ErpAktarim>();

        cut.WaitForAssertion(() => cut.Find("#erp-not-connected"));
        api.Requests.Should().NotContain(r => r.PathAndQuery.Contains("erp-"));
    }

    [Fact]
    public void Only_an_administrator_opens_the_erp_settings()
    {
        PortalRoles.Allows([PortalRoles.Admin], PortalArea.ErpWrite).Should().BeTrue();
        foreach (var role in new[] { PortalRoles.Manager, PortalRoles.Accounting, PortalRoles.Warehouse, PortalRoles.Sales })
            PortalRoles.Allows([role], PortalArea.ErpWrite).Should().BeFalse(role);
    }

    [Fact]
    public void A_users_mikro_counterparts_are_saved_with_blanks_left_to_the_company()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: ErpAdmin());
        api.Answer("/api/v1/android/account/users", new
        {
            seats = new { max = 5, used = 2, status = "active" },
            users = new object[]
            {
                new { id = Guid.NewGuid(), username = "patron", fullName = "Firma Sahibi", role = "ADMIN", canApprove = true, isActive = true },
                new { id = AliId, username = "ali", fullName = "Ali Yılmaz", role = "SALES", canApprove = false, isActive = true },
            },
        });
        api.Answer("/api/v1/portal/erp-lookups", Lookups());
        api.Answer($"/api/v1/portal/users/{AliId}/erp-mapping", new { userId = AliId, cashCode = "001", series = new { invoice = "A" } });

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("tr[data-user=ali] .erp-btn"));
        cut.Find("tr[data-user=patron] .erp-btn").Should().NotBeNull("an administrator who also sells has counterparts too");

        cut.Find("tr[data-user=ali] .erp-btn").Click();
        cut.WaitForAssertion(() => cut.Find("#user-erp-edit"));
        cut.Find("#map-cash").GetAttribute("value").Should().Be("001");

        cut.Find("#map-cash").Change("002");
        cut.Find("#map-series-invoice").Change("");
        cut.Find("#map-series-collection").Change("ALI");
        cut.Find("#user-erp-edit form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Ali Yılmaz için Mikro karşılıkları kaydedildi"));
        var saved = api.Requests.Single(r => r.Method == HttpMethod.Put).Body;
        saved.Should().Contain("\"cashCode\":\"002\"")
            .And.Contain("\"invoice\":null", "an emptied box goes back to the company's series")
            .And.Contain("\"collection\":\"ALI\"")
            .And.Contain("\"warehouseNo\":null");
        cut.FindAll("#user-erp-edit").Should().BeEmpty();
    }

    [Fact]
    public void A_company_without_an_erp_has_no_mikro_button()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/android/account/users", new
        {
            seats = new { max = 5, used = 1, status = "active" },
            users = new object[] { new { id = AliId, username = "ali", fullName = "Ali Yılmaz", role = "SALES", canApprove = false, isActive = true } },
        });

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("tr[data-user=ali]"));

        cut.FindAll(".erp-btn").Should().BeEmpty();
    }
}
