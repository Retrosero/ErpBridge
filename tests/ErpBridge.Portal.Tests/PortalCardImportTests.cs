using System.Net;
using System.Text;
using System.Text.Json;
using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using ErpBridge.Portal.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>GOAL_PANEL_ERPSIZ E1d/E2c: importing products/customers from a file — preview, parts of 500, the server's report.</summary>
public sealed class PortalCardImportTests : PortalPageTestContext
{
    private const string CustomerBatch = "/api/v1/portal/native/customer-cards/batch";
    private const string StockBatch = "/api/v1/portal/native/stock-cards/batch";

    private IRenderedComponent<CardImport> RenderImport(string kind)
    {
        Render<MudBlazor.MudPopoverProvider>();
        return Render<CardImport>(p => p.Add(c => c.Kind, kind).Add(c => c.Open, true));
    }

    private static void Upload(IRenderedComponent<CardImport> cut, string text, string name = "dosya.csv") =>
        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(Encoding.UTF8.GetBytes(text), name));

    [Fact]
    public void A_customer_file_is_previewed_with_its_bad_rows_and_only_the_good_rows_are_sent()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(), popoverProvider: false);
        api.Answer(CustomerBatch, new { booked = 2, skipped = new[] { new { row = 3, reason = "REJECTED", message = "x" } } });
        var cut = RenderImport("customer");

        Upload(cut, "Kod;Unvan;Telefon;Açılış bakiyesi\nC-001;Bakkal Ali;0555;1.250,50\nC-002;Market Veli;;\n;Kodsuz;;\nC-001;Tekrar;;\nC-003;Sayısız;;on\n");

        cut.WaitForAssertion(() => cut.Find("#customer-import-summary").TextContent.Should().Contain("5 satır").And.Contain("2 geçerli").And.Contain("3 hatalı"));
        cut.Find("tr[data-row='4']").TextContent.Should().Contain("Kod boş");
        cut.Find("tr[data-row='5']").TextContent.Should().Contain("Kod dosyada birden fazla");
        cut.Find("tr[data-row='6']").TextContent.Should().Contain("Açılış bakiyesi sayı değil");

        cut.Find("#customer-import-confirm").Click();

        cut.WaitForAssertion(() => cut.Find("#customer-import-result").TextContent.Should().Contain("2 kart kaydedildi").And.Contain("4 satır atlandı"));
        var sent = JsonDocument.Parse(api.Requests.Single(r => r.PathAndQuery == CustomerBatch).Body!).RootElement;
        sent.GetProperty("rows").EnumerateArray().Select(r => r.GetInt32()).Should().Equal(2, 3);
        sent.GetProperty("cards")[0].GetProperty("openingBalance").GetDecimal().Should().Be(1250.50m);
        sent.GetProperty("cards")[1].GetProperty("customerCode").GetString().Should().Be("C-002");
        cut.Find("#customer-import-skipped").TextContent.Should().Contain("Satır 3: Kaydedilemedi").And.Contain("Satır 4");
    }

    [Fact]
    public void A_large_product_file_goes_in_parts_of_500_each_with_its_own_operation_id()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(), popoverProvider: false);
        api.Answer(StockBatch, new { booked = 500, skipped = Array.Empty<object>() });
        var cut = RenderImport("stock");
        var file = new StringBuilder("Kod;Ad;Barkod;Açılış miktarı\n");
        for (var i = 1; i <= 750; i++) file.Append($"P{i};Ürün {i};869{i:D6};{i}\n");

        Upload(cut, file.ToString(), "urunler.csv");
        cut.WaitForAssertion(() => cut.Find("#stock-import-confirm").TextContent.Should().Contain("750"));
        cut.Find("#stock-import-confirm").Click();

        cut.WaitForAssertion(() => api.Requests.Count(r => r.PathAndQuery == StockBatch).Should().Be(2));
        var parts = api.Requests.Where(r => r.PathAndQuery == StockBatch).Select(r => JsonDocument.Parse(r.Body!).RootElement).ToList();
        parts[0].GetProperty("cards").GetArrayLength().Should().Be(500);
        parts[1].GetProperty("cards").GetArrayLength().Should().Be(250);
        parts[1].GetProperty("rows")[0].GetInt32().Should().Be(502, "file row of the 501st product (the header is row 1)");
        parts.Select(p => p.GetProperty("operationId").GetString()).Distinct().Should().HaveCount(2);
    }

    [Fact]
    public void A_file_without_the_required_columns_says_so()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(), popoverProvider: false);
        var cut = RenderImport("stock");

        Upload(cut, "Ürün;Fiyat\nÇay;10\n");

        cut.WaitForAssertion(() => cut.Find("#stock-import-error").TextContent.Should().Contain("\"Kod\" ve \"Ad\""));
        cut.FindAll("#stock-import-confirm").Should().BeEmpty();
    }

    [Fact]
    public void The_stock_and_customer_pages_offer_the_import_only_to_a_native_admin()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "MANAGER"));
        api.Answer("/api/v1/portal/stock/facets", new
        {
            mainGroups = Array.Empty<object>(), subGroups = Array.Empty<object>(), brands = Array.Empty<object>(), shelves = Array.Empty<object>(),
            warehouses = Array.Empty<object>(), priceLists = Array.Empty<object>(), hasMovementDates = false, hasReserved = false,
        });
        api.Answer("/api/v1/portal/stock/search?sort=name&dir=asc&page=1&pageSize=50", new { items = Array.Empty<object>(), total = 0, page = 1, pageSize = 50, summary = new { } });
        Services.GetRequiredService<NavigationManager>().NavigateTo("stok");

        var cut = Render<Stok>();

        cut.WaitForAssertion(() => cut.Find("#stock-status"));
        cut.FindAll("#stock-import-open").Should().BeEmpty();
    }
}
