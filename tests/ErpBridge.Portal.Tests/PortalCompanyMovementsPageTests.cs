using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>GOAL_PANEL_ERPSIZ E4e: the company-wide /hareketler list.</summary>
public sealed class PortalCompanyMovementsPageTests : PortalPageTestContext
{
    private const string UsersPath = "/api/v1/android/account/users";
    private static DateOnly MonthStart => new(Fmt.Today().Year, Fmt.Today().Month, 1);
    private static string Base => $"/api/v1/portal/movements?from={Fmt.IsoDay(MonthStart)}&to={Fmt.IsoDay(Fmt.Today())}";

    private static object Row(string id, string kind, string customer, decimal debit, decimal credit, string? documentKey = null, bool voided = false, string? sourceType = null) => new
    {
        id, date = Fmt.IsoDay(Fmt.Today()), customerCode = customer, customerTitle = customer + " Ünvan", kind, sourceType, documentNo = "E-1",
        documentKey, description = "Not", paymentType = (string?)null, debit, credit, userName = "Firma Sahibi", voided, reason = voided ? "Vazgeçti" : null,
    };

    private static object Page(params object[] items) => new
    {
        items, total = items.Length, page = 1, pageSize = 50, totalDebit = 450m, totalCredit = 100m,
    };

    private (FakeCentralApi Api, NavigationManager Nav) Setup(string role = "ADMIN", params object[] rows)
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: role));
        api.Answer(UsersPath, new { seats = new { max = 5, used = 1, status = "active" }, users = Array.Empty<object>() });
        api.Answer(Base + "&page=1&pageSize=50", Page(rows));
        return (api, Services.GetRequiredService<NavigationManager>());
    }

    [Fact]
    public void Lists_movements_of_every_kind_with_totals()
    {
        Setup(rows: [Row("s|sale", "sale", "C-001", 450m, 0m, documentKey: "dC-001|E-1"), Row("c|collection", "collection", "C-002", 0m, 100m),
                     Row("v|sale|void", "other", "C-003", 0m, 150m, sourceType: "İptal: Satış")]);

        var cut = Render<Hareketler>();

        cut.WaitForAssertion(() => cut.FindAll("#movements-table tbody tr").Count.Should().Be(3));
        cut.Find("tr[data-movement='s|sale']").TextContent.Should().Contain("Satış").And.Contain("C-001 Ünvan").And.Contain("450,00");
        cut.Find("tr[data-movement='v|sale|void']").TextContent.Should().Contain("İptal: Satış");
        cut.Find("#movements-debit").TextContent.Should().Contain("450,00");
        cut.Find("#movements-count").TextContent.Should().Contain("3");
    }

    [Fact]
    public void Filters_go_to_the_server()
    {
        var (api, _) = Setup(rows: []);
        var filtered = Base + "&kind=collection&customer=C-001&minAmount=50&maxAmount=500&includeVoided=true&page=1&pageSize=50";
        api.Answer(filtered, Page());
        var cut = Render<Hareketler>();
        cut.WaitForAssertion(() => cut.Find("#movements-empty"));

        cut.Find("#movements-kind").Change("collection");
        cut.Find("#movements-customer").Change("C-001");
        cut.Find("#movements-min").Change("50");
        cut.Find("#movements-max").Change("500");
        cut.Find("#movements-voided").Change(true);
        cut.Find("#movements-filter").Submit();

        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.Method == HttpMethod.Get && r.PathAndQuery == filtered));
    }

    [Fact]
    public void An_invoice_row_opens_its_document_and_any_other_row_the_customer_statement()
    {
        var (_, nav) = Setup(rows: [Row("s|sale", "sale", "C-001", 450m, 0m, documentKey: "dC-001|E-1"), Row("c|collection", "collection", "C-002", 0m, 100m)]);
        var cut = Render<Hareketler>();
        cut.WaitForAssertion(() => cut.Find("tr[data-movement='s|sale']"));

        cut.Find("tr[data-movement='s|sale']").Click();
        nav.Uri.Should().EndWith("evraklar?belge=dC-001%7CE-1");

        cut.Find("tr[data-movement='c|collection']").Click();
        nav.Uri.Should().EndWith("cari?kod=C-002");
    }

    [Fact]
    public void An_accountant_reads_the_list_without_the_admin_only_user_list()
    {
        var (api, _) = Setup(role: "ACCOUNTING", rows: [Row("c|collection", "collection", "C-002", 0m, 100m)]);

        var cut = Render<Hareketler>();

        cut.WaitForAssertion(() => cut.FindAll("#movements-table tbody tr").Count.Should().Be(1));
        api.Requests.Should().NotContain(r => r.PathAndQuery == UsersPath);
        cut.FindAll("#movements-user").Should().BeEmpty();
    }
}
