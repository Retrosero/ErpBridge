using System.Net;
using System.Text.Json;
using Bunit;
using ErpBridge.Portal.Pages.Giris;
using ErpBridge.Portal.Session;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// GOAL_PANEL_GIRIS P5: the panel's entry pages. The server prices and checks everything; a page shows the preview it gets
/// after each change, keeps Save closed while something stands in the way, and keeps one key per submission so a save sent
/// again after a lost answer is the same document.
/// </summary>
public sealed class PortalEntryPagesTests : PortalPageTestContext
{
    private const string Entry = "/api/v1/portal/entry/";
    private static readonly Guid AliId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

    private static object Context(string dataSource = "erp", bool onAccount = true, bool purchaseVat = false) => new
    {
        dataSource,
        purchasePricesIncludeVat = purchaseVat,
        today = "2026-09-21",
        kinds = new[] { "sale", "collection", "purchase", "return", "disbursement", "expense" },
        canSellOnAccount = onAccount,
        canSellBelowStock = true,
        owners = new object[]
        {
            new { userId = Guid.Parse("aaaaaaaa-0000-0000-0000-0000000000ff"), name = "Firma Sahibi", username = "patron", isSelf = true },
            new { userId = AliId, name = "Ali Bey", username = "ali", isSelf = false },
        },
        priceLists = new[] { new { no = 1, name = "Perakende", includesVat = true }, new { no = 2, name = "Bayi", includesVat = false } },
        banks = new[] { new { code = "13", name = "Ziraat POS" } },
        cashAccounts = new[] { new { code = "001", name = "Merkez kasa" } },
        expenseCards = new[] { new { code = "YAKIT", name = "Yakıt" } },
        vatRates = new[] { new { code = "4", name = "%20", rate = 20m } },
        expenseCategories = dataSource == "erp" ? Array.Empty<string>() : new[] { "Yemek", "Diğer" },
        warehouses = Array.Empty<object>(),
    };

    private static object Customers() => new { items = new[] { new { code = "C1", title = "Yılmaz Market" } }, total = 1 };

    private static object Products() => new
    {
        items = new[] { new { code = "A", name = "Çay Rize", unit = "KG", vatRate = 10m, prices = new Dictionary<string, decimal> { ["1"] = 100m }, defaultPriceListNo = 1, stock = 5m } },
        total = 1,
    };

    private static object SalePreview(object? refusal = null) => new
    {
        kind = "sale", dataSource = "erp", ownerUserId = AliId, ownerName = "Ali Bey", customerCode = "C1", customerName = "Yılmaz Market",
        occurredAt = "21.09.2026 12:00", priceListNo = 1, priceListName = "Perakende", priceIncludesVat = true,
        lines = new[] { new { productCode = "A", name = "Çay Rize", quantity = 1m, listUnitPrice = 100m, vatRate = 10m, gross = 90.91m, discount = 0m, net = 90.91m, vat = 9.09m, total = 100m } },
        gross = 90.91m, discount = 0m, vat = 9.09m, total = 100m, refusal, stockWarnings = Array.Empty<object>(), payments = Array.Empty<object>(),
    };

    private FakeCentralApi Setup(string dataSource = "erp", Dictionary<string, bool>? permissions = null, string? page = null)
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { DataSource = dataSource, Permissions = permissions });
        api.Answer(Entry + "context", Context(dataSource));
        api.Answer(Entry + "customers?q=yil&take=20", Customers());
        api.Answer(Entry + "products?q=cay&take=20", Products());
        if (page is not null) Services.GetRequiredService<NavigationManager>().NavigateTo(page);
        return api;
    }

    private static void PickCustomer<T>(IRenderedComponent<T> cut) where T : IComponent
    {
        cut.WaitForAssertion(() => cut.Find("#entry-customer-search"));
        cut.Find("#entry-customer-search").Input("yil");
        cut.Find("#entry-customer-search-go").Click();
        cut.WaitForAssertion(() => cut.Find("[data-customer='C1']"));
        cut.Find("[data-customer='C1']").Click();
    }

    private static void AddProduct<T>(IRenderedComponent<T> cut) where T : IComponent
    {
        cut.Find("#entry-product-search").Input("cay");
        cut.Find("#entry-product-search-go").Click();
        cut.WaitForAssertion(() => cut.Find("[data-product='A']"));
        cut.Find("[data-product='A']").Click();
    }

    private static JsonElement Body((HttpMethod Method, string PathAndQuery, string? Authorization, string? Body, string? ContentType) request) =>
        JsonDocument.Parse(request.Body!).RootElement.Clone();

    [Fact]
    public void A_sale_shows_the_servers_preview_and_saves_with_the_total_seen_and_the_chosen_owner()
    {
        var api = Setup();
        api.Answer(Entry + "sale/preview", SalePreview());
        var cut = Render<GirisSatis>();

        cut.WaitForAssertion(() => cut.Find("#entry-owner"));
        cut.Find("#entry-owner").Change(AliId.ToString());
        PickCustomer(cut);
        AddProduct(cut);

        cut.WaitForAssertion(() => cut.Find("#entry-total").TextContent.Should().Be("100,00 TL"));
        cut.Find("#entry-save").HasAttribute("disabled").Should().BeFalse();
        var preview = Body(api.Requests.Last(r => r.PathAndQuery == Entry + "sale/preview"));
        preview.GetProperty("customerCode").GetString().Should().Be("C1");
        preview.GetProperty("paymentType").GetString().Should().Be("Cari Borç");
        preview.GetProperty("ownerUserId").GetGuid().Should().Be(AliId);
        preview.GetProperty("operationId").ValueKind.Should().Be(JsonValueKind.Null, "a preview writes nothing and carries no key");

        api.Answer(Entry + "sale", new { documents = new[] { new { jobId = Guid.NewGuid(), externalId = "PNL-SO-x", documentType = "sales_order", status = "Pending" } }, idempotent = false }, HttpStatusCode.Created);
        cut.Find("#entry-save").Click();

        cut.WaitForAssertion(() => cut.Find("#entry-saved"));
        cut.Find("#entry-saved .entry-saved-state").TextContent.Should().Contain("ERP'ye yazılmayı bekliyor");
        var saved = Body(api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery == Entry + "sale"));
        saved.GetProperty("expectedTotal").GetDecimal().Should().Be(100m);
        Guid.TryParse(saved.GetProperty("operationId").GetString(), out _).Should().BeTrue();
        saved.GetProperty("ownerUserId").GetGuid().Should().Be(AliId);
    }

    [Fact]
    public void A_save_whose_answer_was_lost_is_sent_again_with_the_same_key()
    {
        var api = Setup();
        api.Answer(Entry + "sale/preview", SalePreview());
        var cut = Render<GirisSatis>();
        PickCustomer(cut);
        AddProduct(cut);
        cut.WaitForAssertion(() => cut.Find("#entry-total").TextContent.Should().Be("100,00 TL"));

        api.Fail(Entry + "sale", HttpStatusCode.BadGateway, "UPSTREAM");
        cut.Find("#entry-save").Click();
        cut.WaitForAssertion(() => cut.Find("#page-error"));

        // The document may be written: the form stays as it was sent, only Kaydet sends it again (Codex #252).
        cut.Find("#entry-unsettled").TextContent.Should().Contain("Kaydın sonucu alınamadı");
        cut.Find("#entry-product-search").HasAttribute("disabled").Should().BeTrue();
        cut.Find("#entry-owner").HasAttribute("disabled").Should().BeTrue();
        cut.Find("#entry-clear").HasAttribute("disabled").Should().BeTrue("a new document now could write the same sale twice");
        cut.Find("#entry-save").HasAttribute("disabled").Should().BeFalse();

        api.Answer(Entry + "sale", new { documents = new[] { new { jobId = Guid.NewGuid(), externalId = "PNL-SO-x", documentType = "sales_order", status = "Pending" } }, idempotent = true });
        cut.Find("#entry-save").Click();
        cut.WaitForAssertion(() => cut.Find("#entry-saved"));
        cut.FindAll("#entry-unsettled").Should().BeEmpty();

        var keys = api.Requests.Where(r => r.Method == HttpMethod.Post && r.PathAndQuery == Entry + "sale").Select(r => Body(r).GetProperty("operationId").GetString()).ToList();
        keys.Should().HaveCount(2);
        keys.Distinct().Should().ContainSingle("the retry is the same submission");
    }

    [Fact]
    public void A_refused_save_wrote_nothing_and_leaves_the_form_open()
    {
        var api = Setup();
        api.Answer(Entry + "sale/preview", SalePreview());
        var cut = Render<GirisSatis>();
        PickCustomer(cut);
        AddProduct(cut);
        cut.WaitForAssertion(() => cut.Find("#entry-total").TextContent.Should().Be("100,00 TL"));

        api.Fail(Entry + "sale", HttpStatusCode.Conflict, "ENTRY_LIMIT_EXCEEDED");
        cut.Find("#entry-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-error"));
        cut.FindAll("#entry-unsettled").Should().BeEmpty();
        cut.Find("#entry-product-search").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void Enter_in_a_search_searches_what_was_typed_and_never_saves()
    {
        var api = Setup();
        api.Answer(Entry + "sale/preview", SalePreview());
        var cut = Render<GirisSatis>();
        PickCustomer(cut);
        AddProduct(cut);
        cut.WaitForAssertion(() => cut.Find("#entry-total").TextContent.Should().Be("100,00 TL"));

        // No <form>: the browser has nothing to submit on Enter (Codex #252); the key searches the text as typed.
        cut.FindAll("form").Should().BeEmpty();
        cut.Find("#entry-product-search").Input("cay");
        cut.Find("#entry-product-search").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        cut.WaitForAssertion(() => cut.Find("[data-product='A']"));
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Post && r.PathAndQuery == Entry + "sale");
    }

    [Fact]
    public void A_purchase_line_starts_at_the_price_the_companys_supplier_setting_asks_for()
    {
        var api = Setup();
        api.Answer(Entry + "context", Context(purchaseVat: true));
        // List 2 does not include VAT; the company's supplier prices do: 100 + 10 % VAT.
        api.Answer(Entry + "products?q=cay&take=20", new
        {
            items = new[] { new { code = "A", name = "Çay Rize", unit = "KG", vatRate = 10m, prices = new Dictionary<string, decimal> { ["2"] = 100m }, defaultPriceListNo = 2, stock = 5m } },
            total = 1,
        });
        api.Answer(Entry + "purchase/preview", new { kind = "purchase", priceIncludesVat = true, total = 110m, lines = Array.Empty<object>(), payments = Array.Empty<object>(), stockWarnings = Array.Empty<object>() });
        var cut = Render<GirisAlis>();
        PickCustomer(cut);
        AddProduct(cut);

        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.PathAndQuery == Entry + "purchase/preview"));
        var line = Body(api.Requests.First(r => r.PathAndQuery == Entry + "purchase/preview")).GetProperty("lines")[0];
        line.GetProperty("unitPrice").GetDecimal().Should().Be(110m, "the first line is priced before any preview, from the context (Codex #252)");
        cut.Markup.Should().Contain("Birim fiyat (KDV dahil)");
    }

    [Fact]
    public void A_refusal_is_shown_and_save_stays_closed()
    {
        var api = Setup();
        api.Answer(Entry + "sale/preview", SalePreview(new { code = "ENTRY_LIMIT_EXCEEDED", message = "Satış tutarı 100 TL, sınırınız 50 TL.", key = "limit.sale.max_amount" }));
        var cut = Render<GirisSatis>();
        PickCustomer(cut);
        AddProduct(cut);

        cut.WaitForAssertion(() => cut.Find("#entry-refusal").TextContent.Should().Contain("sınırınız 50 TL"));
        cut.Find("#entry-refusal").GetAttribute("data-code").Should().Be("ENTRY_LIMIT_EXCEEDED");
        cut.Find("#entry-save").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Without_the_open_account_right_a_sale_starts_in_cash()
    {
        var api = Setup();
        api.Answer(Entry + "context", Context(onAccount: false));
        var cut = Render<GirisSatis>();

        cut.WaitForAssertion(() => cut.Find("#sale-payment"));
        cut.FindAll("#sale-payment option").Select(o => o.GetAttribute("value")).Should().Equal("Nakit", "Kredi Kartı");
    }

    [Fact]
    public void A_collection_sends_one_line_per_method()
    {
        var api = Setup();
        api.Answer(Entry + "collection/preview", new { kind = "collection", total = 150m, payments = Array.Empty<object>(), lines = Array.Empty<object>(), stockWarnings = Array.Empty<object>() });
        var cut = Render<GirisTahsilat>();
        PickCustomer(cut);

        cut.FindAll(".payment-amount")[0].Change("100");
        cut.Find("#collection-add-cheque").Click();
        cut.WaitForAssertion(() => cut.FindAll(".payment-amount").Should().HaveCount(2));
        cut.FindAll(".payment-amount")[1].Change("50");
        cut.Find(".payment-document-no").Change("27703");
        cut.Find(".payment-due").Change("2026-11-30");

        cut.WaitForAssertion(() => cut.Find("#entry-total").TextContent.Should().Be("150,00 TL"));
        var sent = Body(api.Requests.Last(r => r.PathAndQuery == Entry + "collection/preview"));
        var payments = sent.GetProperty("payments").EnumerateArray().ToList();
        payments.Select(p => p.GetProperty("method").GetString()).Should().Equal("cash", "cheque");
        payments[1].GetProperty("documentNo").GetString().Should().Be("27703");
        payments[1].GetProperty("dueDate").GetString().Should().Be("2026-11-30");
    }

    [Fact]
    public void An_erp_expense_needs_a_card_and_sends_the_vat_inside_the_amount()
    {
        var api = Setup();
        api.Answer(Entry + "expense/preview", new { kind = "expense", total = 120m, vat = 20m, payments = Array.Empty<object>(), lines = Array.Empty<object>(), stockWarnings = Array.Empty<object>() });
        var cut = Render<GirisGider>();

        cut.WaitForAssertion(() => cut.Find("#expense-card"));
        cut.Find("#expense-amount").Change("120");
        cut.Find("#expense-description").Change("Yakıt");
        api.Requests.Should().NotContain(r => r.PathAndQuery == Entry + "expense/preview", "no card yet");

        cut.Find("#expense-card").Change("YAKIT");
        cut.Find("#expense-vat-rate").Change("4");

        cut.WaitForAssertion(() => cut.Find("#entry-total").TextContent.Should().Be("120,00 TL"));
        var sent = Body(api.Requests.Last(r => r.PathAndQuery == Entry + "expense/preview"));
        sent.GetProperty("expenseCardCode").GetString().Should().Be("YAKIT");
        sent.GetProperty("vatPointer").GetInt32().Should().Be(4);
        sent.GetProperty("vatAmount").GetDecimal().Should().Be(20m, "120 with 20 % inside is 20 VAT");
    }

    [Fact]
    public void A_native_expense_picks_a_category()
    {
        var api = Setup(dataSource: "native");
        var cut = Render<GirisGider>();

        cut.WaitForAssertion(() => cut.Find("#expense-category"));
        cut.FindAll("#expense-card").Should().BeEmpty();
        cut.FindAll("#expense-category option").Select(o => o.TextContent).Should().Contain(["Yemek", "Diğer"]);
    }

    [Fact]
    public void A_return_offers_only_what_the_customer_was_sold_at_its_prices()
    {
        var api = Setup();
        api.Answer(Entry + "returnables?customerCode=C1", new
        {
            items = new[] { new { code = "A", name = "Çay Rize", vatRate = 10m, prices = new[] { new { unitPrice = 85m, lastSold = "2026-09-20" }, new { unitPrice = 90.91m, lastSold = "2026-09-10" } } } },
        });
        api.Answer(Entry + "return/preview", new { kind = "return", total = 93.5m, lines = Array.Empty<object>(), payments = Array.Empty<object>(), stockWarnings = Array.Empty<object>() });
        var cut = Render<GirisIade>();
        PickCustomer(cut);

        cut.WaitForAssertion(() => cut.FindAll("#return-products .pick-item").Should().HaveCount(2));
        cut.Find("[data-product='A'][data-price='85']").Click();

        cut.WaitForAssertion(() => cut.Find("#return-lines"));
        cut.Find(".line-condition").Change("50");
        var sent = Body(api.Requests.Last(r => r.PathAndQuery == Entry + "return/preview"));
        var line = sent.GetProperty("lines")[0];
        line.GetProperty("unitPrice").GetDecimal().Should().Be(85m);
        line.GetProperty("conditionPercent").GetDecimal().Should().Be(0.5m, "the form shows a percentage, the server takes a share");
    }

    [Fact]
    public void The_slip_prints_the_document_as_it_was_written()
    {
        var api = Setup(page: "giris-yazdir?is=" + Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"));
        api.Answer(Entry + "documents/bbbbbbbb-0000-0000-0000-000000000001", new
        {
            jobId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), externalId = "PNL-TH-x", documentType = "collection", kind = "collection", dataSource = "erp",
            state = "written", erpDocumentNo = "T-1234", customerName = "Yılmaz Market", amount = 150m, ownerName = "Ali Bey", enteredBy = "Firma Sahibi",
            enteredAtUtc = PortalTestSetup.Now,
            payload = new
            {
                occurredAt = "21.09.2026 12:00", counterparty = "Yılmaz Market", customerCode = "C1", amount = 150m, paymentType = "Çoklu Tahsilat",
                payments = new object[] { new { method = "cash", amount = 100m }, new { method = "cheque", amount = 50m, dueDate = "30.11.2026", cheque = new { no = "27703" } } },
            },
        });

        var cut = Render<GirisYazdir>();

        cut.WaitForAssertion(() => cut.Find("#print-title").TextContent.Should().Be("Tahsilat Makbuzu"));
        cut.Find("#print-number").TextContent.Should().Be("T-1234");
        cut.FindAll("#print-payments tbody tr").Should().HaveCount(2);
        cut.Find("#print-payments").TextContent.Should().Contain("No: 27703");
        cut.Find("#print-total").TextContent.Should().Be("150,00 TL");
    }

    [Fact]
    public void An_erp_return_slip_prints_the_refunded_share_of_each_line()
    {
        var id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");
        var api = Setup(page: "giris-yazdir?is=" + id);
        api.Answer(Entry + "documents/" + id, new
        {
            jobId = id, externalId = "PNL-SR-x", documentType = "sales_return", kind = "return", dataSource = "erp", state = "written", amount = 110m,
            enteredAtUtc = PortalTestSetup.Now,
            payload = new
            {
                occurredAt = "21.09.2026 12:00", counterparty = "Yılmaz Market", customerCode = "C1", amount = 110m, settlementMethod = "Cari Alacak",
                lines = new[] { new { productCode = "A", productTitle = "Çay Rize", quantity = 2m, unitPrice = 100m, conditionPercent = 0.5m } },
            },
        });

        var cut = Render<GirisYazdir>();

        // An ERP return's lines carry no total: 2 × 100 at half condition refunds 100 (Codex #252).
        cut.WaitForAssertion(() => cut.Find("#print-lines tbody tr td:last-child").TextContent.Should().Be("100,00 TL"));
    }

    [Fact]
    public void A_purchase_slip_shows_its_vat_and_a_vat_inclusive_total()
    {
        var id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003");
        var api = Setup(page: "giris-yazdir?is=" + id);
        api.Answer(Entry + "documents/" + id, new
        {
            jobId = id, externalId = "PNL-PR-x", documentType = "purchase_receipt", kind = "purchase", dataSource = "erp", state = "written", amount = 1100m,
            enteredAtUtc = PortalTestSetup.Now,
            payload = new
            {
                occurredAt = "2026-09-21T12:00:00+03:00", counterparty = "Toptancı", supplierCode = "C2", amount = 1000m, vatAmount = 100m, grossAmount = 1100m,
                lines = new[] { new { productCode = "A", productTitle = "Çay Rize", quantity = 10m, unitPrice = 100m, lineTotal = 1000m } },
            },
        });

        var cut = Render<GirisYazdir>();

        cut.WaitForAssertion(() => cut.Find("#print-total").TextContent.Should().Be("1.100,00 TL"));
        cut.Find("#print-vat").TextContent.Should().Be("100,00 TL");
        cut.Markup.Should().Contain("Genel toplam (KDV dahil)");
    }

    [Fact]
    public void The_menu_offers_the_entry_pages_the_users_modules_open()
    {
        PortalTestSetup.Register(this, popoverProvider: false, signedIn: PortalTestSetup.State() with
        {
            DataSource = "erp",
            Permissions = new Dictionary<string, bool> { ["module.sales"] = true, ["module.collection"] = false, ["module.expenses"] = true, ["portal.reports"] = true },
        });

        var layout = Render<ErpBridge.Portal.MainLayout>(p => p.Add(l => l.Body, (RenderFragment)(b => b.AddContent(0, "içerik"))));

        layout.WaitForAssertion(() => layout.Find("#nav-new-sale"));
        layout.FindAll("#nav-new-collection").Should().BeEmpty();
        layout.FindAll("#nav-new-expense").Should().ContainSingle();
        layout.FindAll("#nav-entries").Should().ContainSingle();
    }
}
