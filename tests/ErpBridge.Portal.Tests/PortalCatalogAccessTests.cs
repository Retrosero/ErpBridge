using System.Net;
using System.Text.Json;
using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// GOAL_MUSTERI_KATALOGU P4: the customer's catalog access sheet (opened from the customer page and the catalog's
/// "Müşteri erişimleri" tab): read only when opened, created with the visibility the screen chose, the issued password
/// shown once, and the share message with its WhatsApp link.
/// </summary>
public sealed class PortalCatalogAccessTests : PortalPageTestContext
{
    private const string Customers = "/api/v1/portal/customers";
    private const string Catalog = "/api/v1/customer-catalog";
    private const string ByCustomer = Catalog + "/accounts/by-customer?code=C%2F1";
    private const string Accounts = Catalog + "/accounts";
    private const string Password = "Xy7kP2mQ9a";
    private static readonly Guid AccountId = Guid.Parse("6f0f7a1e-0000-4000-8000-000000000001");
    private static readonly Guid VeliId = Guid.Parse("6f0f7a1e-0000-4000-8000-0000000000ff");

    private static string LedgerPath => Customers + $"/ledger?code=C%2F1&from={Fmt.Today().Year}-01-01&page=1&pageSize=50";

    private static object Card() => new
    {
        customerCode = "C/1", title = "Bakkal Ali", balance = 700m, phone = "0532 000 00 00", isLocked = false, dataSource = "native",
    };

    private static object Ledger() => new
    {
        customerCode = "C/1", opening = 0m, closing = 0m, totalDebit = 0m, totalCredit = 0m, items = Array.Empty<object>(), total = 0, page = 1, pageSize = 50,
    };

    private static object Settings() => new
    {
        isEnabled = true, defaultPriceListNo = (int?)null, effectiveDefaultPriceListNo = 1, revision = 7, tenantCode = "ABCD2345",
        publicUrl = "https://sipariscepte.appsgo.cloud/ABCD2345",
        priceLists = new[] { new { no = 1, name = "Perakende", includesVat = true }, new { no = 2, name = "Toptan", includesVat = false } },
        imageQuota = new { usedBytes = 0L, limitBytes = 1024L * 1024 * 1024 },
        counts = new { categories = 2, products = 3, visibleProducts = 2, accounts = 1, openOrders = 0 },
    };

    private static object Categories() => new
    {
        revision = 7,
        items = new[] { "Icecek", "Temizlik" }.Select(key => new
        {
            key, name = key, sortOrder = (int?)null, hidden = false, productCount = 2, hiddenCount = 1, noDiscountCount = 0, cartonOnlyCount = 0,
        }),
    };

    private static object Product(string code, string name, bool hidden) => new
    {
        stockCode = code, name, unit = "Adet", brand = (string?)null, categoryKey = "Icecek", sortOrder = (int?)null, hidden, noDiscount = false,
        cartonOnly = false, cartonQuantity = (int?)null, erpCartonQuantity = (int?)null, listPrice = (decimal?)150m, inStock = true, imageCount = 0,
        thumbUrl = (string?)null,
    };

    private static object Found() => new
    {
        revision = 7, truncated = false, items = new[] { Product("CAY-1", "Çay 1 kg", hidden: false), Product("GIZLI-1", "Gizli çay", hidden: true) },
    };

    private static object Account(string mode = "all", object[]? rules = null, int? priceListNo = 2) => new
    {
        id = AccountId, customerCode = "C/1", customerName = "Bakkal Ali", username = "bakkal.ali", isActive = true, discountPercent = 10m,
        priceListNo, visibility = new { mode, rules = rules ?? Array.Empty<object>() },
        showStatement = true, showInvoices = false, showPurchased = false, canOrder = true, responsibleUserId = (Guid?)null,
        lastLoginAtMs = (long?)null, openOrderCount = 0, createdAtMs = 1790000000000L, createdByName = "Firma Sahibi", updatedAtMs = 1790000000000L,
    };

    private static object Lookup(object? account) => new { account, customerName = "Bakkal Ali", suggestedUsername = "bakkal.ali" };

    private static JsonElement Body(FakeCentralApi api, HttpMethod method, string path) =>
        JsonDocument.Parse(api.Requests.Last(r => r.Method == method && r.PathAndQuery == path).Body!).RootElement;

    private static IEnumerable<string> Rules(JsonElement visibility) =>
        visibility.GetProperty("rules").EnumerateArray().Select(r => $"{r.GetProperty("type").GetString()}:{r.GetProperty("key").GetString()}:{r.GetProperty("effect").GetString()}");

    private FakeCentralApi SetupCustomerPage(bool module = true)
    {
        var state = PortalTestSetup.State() with { Modules = module ? ["customer_catalog"] : null };
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: state);
        Services.GetRequiredService<NavigationManager>().NavigateTo("cari?kod=C%2F1");
        api.Answer(Customers + "/card?code=C%2F1", Card());
        api.Answer(LedgerPath, Ledger());
        api.Answer(ByCustomer, Lookup(null));
        api.Answer(Catalog + "/settings", Settings());
        api.Answer(Catalog + "/categories", Categories());
        api.Answer(Catalog + "/products?q=cay", Found());
        api.Answer("/api/v1/android/account/users", new
        {
            seats = new { max = 3, used = 2 },
            users = new[]
            {
                new { id = VeliId, username = "veli", fullName = "Veli Plasiyer", role = "SALES", roles = new[] { "SALES" }, isActive = true },
                new { id = Guid.NewGuid(), username = "eski", fullName = "Eski Kişi", role = "SALES", roles = new[] { "SALES" }, isActive = false },
            },
        });
        return api;
    }

    private IRenderedComponent<Cari> RenderCustomer()
    {
        var cut = Render<Cari>();
        cut.WaitForAssertion(() => cut.Find("#ledger-empty"));
        return cut;
    }

    private static void OpenSheet(IRenderedComponent<Cari> cut)
    {
        cut.Find("#customer-catalog-access").Click();
        cut.WaitForAssertion(() => cut.Find("#access-username"));
    }

    /// <param name="enter">Searches with the Enter key, which must not save the form, instead of the button.</param>
    private static void SearchProducts(IRenderedComponent<Cari> cut, bool enter = false)
    {
        cut.Find("#access-product-search").Input("cay");
        if (enter) cut.Find("#access-product-search").KeyDown(new KeyboardEventArgs { Key = "Enter" });
        else cut.Find("#access-product-search-go").Click();
        cut.WaitForAssertion(() => cut.Find("#access-product-results li[data-stock='GIZLI-1']"));
    }

    [Fact]
    public void The_customer_page_reads_no_catalog_data_and_offers_the_button_only_with_the_module()
    {
        var api = SetupCustomerPage();

        var cut = RenderCustomer();

        cut.Find("#customer-catalog-access").TextContent.Should().Contain("Katalog erişimi");
        cut.FindAll("#catalog-access-sheet").Should().BeEmpty();
        api.Requests.Should().NotContain(r => r.PathAndQuery.StartsWith(Catalog, StringComparison.Ordinal), "the access is read when the sheet opens");
        api.Requests.Last().PathAndQuery.Should().Be(LedgerPath);
    }

    [Fact]
    public void Without_the_module_the_customer_page_has_no_catalog_button()
    {
        SetupCustomerPage(module: false);

        var cut = RenderCustomer();

        cut.FindAll("#customer-catalog-access").Should().BeEmpty();
    }

    [Fact]
    public void Creating_an_access_sends_the_form_and_shows_the_issued_password_once()
    {
        var api = SetupCustomerPage();
        var cut = RenderCustomer();

        OpenSheet(cut);

        api.Requests.Should().Contain(r => r.Method == HttpMethod.Get && r.PathAndQuery == ByCustomer);
        cut.Find("#access-none").Should().NotBeNull();
        cut.Find("#access-username").GetAttribute("value").Should().Be("bakkal.ali", "the server suggests the username");
        cut.Find("#access-password-auto").HasAttribute("checked").Should().BeTrue("the server makes the password by default");
        cut.FindAll("#access-responsible option").Select(o => o.TextContent).Should().Equal("Seçilmedi (carinin plasiyeri)", "Veli Plasiyer");

        cut.Find("#access-discount").Input("10");
        cut.Find("#access-discount-preview").TextContent.Should().Contain("100,00 TL → 90,00 TL").And.Contain("İskontosuz işaretli ürünlere uygulanmaz");
        cut.Find("#access-price-list").Change("2");
        cut.Find("#access-statement").Change(true);
        cut.Find("#access-responsible").Change(VeliId.ToString());
        api.Answer(Accounts, new { account = Account(), issuedPassword = Password }, HttpStatusCode.Created);
        cut.Find("#access-save").Click();

        cut.WaitForAssertion(() => cut.Find("#access-issued-password").TextContent.Should().Be(Password));
        var body = Body(api, HttpMethod.Post, Accounts);
        body.GetProperty("customerCode").GetString().Should().Be("C/1");
        body.GetProperty("username").GetString().Should().Be("bakkal.ali");
        body.GetProperty("password").ValueKind.Should().Be(JsonValueKind.Null, "an empty password asks the server to make one");
        body.GetProperty("discountPercent").GetDecimal().Should().Be(10m);
        body.GetProperty("priceListNo").GetInt32().Should().Be(2);
        body.GetProperty("visibility").GetProperty("mode").GetString().Should().Be("all");
        Rules(body.GetProperty("visibility")).Should().BeEmpty("the main catalog has no rules");
        body.GetProperty("showStatement").GetBoolean().Should().BeTrue();
        body.GetProperty("showInvoices").GetBoolean().Should().BeFalse();
        body.GetProperty("canOrder").GetBoolean().Should().BeTrue();
        body.GetProperty("isActive").GetBoolean().Should().BeTrue();
        body.GetProperty("responsibleUserId").GetGuid().Should().Be(VeliId);

        // The share message leaves the password out until it is asked for.
        cut.Find("#access-message").TextContent.Should().Be(
            "Merhaba Bakkal Ali, Ege Dağıtım ürün kataloğumuza https://sipariscepte.appsgo.cloud/ABCD2345 adresinden girebilirsiniz. Kullanıcı adınız: bakkal.ali");
        var whatsApp = cut.Find("#access-whatsapp");
        whatsApp.GetAttribute("href").Should().StartWith("https://wa.me/905320000000?text=Merhaba%20Bakkal%20Ali%2C%20Ege%20Da%C4%9F%C4%B1t%C4%B1m%20")
            .And.Contain("https%3A%2F%2Fsipariscepte.appsgo.cloud%2FABCD2345").And.NotContain(Password);
        whatsApp.GetAttribute("target").Should().Be("_blank");
        whatsApp.GetAttribute("rel").Should().Be("noopener");
        cut.Find("#access-include-password").HasAttribute("checked").Should().BeFalse();
        cut.Find("#access-include-password").Change(true);
        cut.Find("#access-message").TextContent.Should().EndWith(", şifreniz: " + Password);
        cut.Find("#access-whatsapp").GetAttribute("href").Should().EndWith("%2C%20%C5%9Fifreniz%3A%20" + Password);

        // Closed and opened again: the password is gone for good.
        api.Answer(ByCustomer, Lookup(Account()));
        cut.Find("#catalog-access-sheet .sheet-close").Click();
        cut.FindAll("#catalog-access-sheet").Should().BeEmpty();
        cut.Markup.Should().NotContain(Password);
        OpenSheet(cut);
        cut.WaitForAssertion(() => cut.Find("#access-facts"));
        cut.FindAll("#access-issued").Should().BeEmpty();
        cut.FindAll("#access-include-password").Should().BeEmpty();
        cut.Markup.Should().NotContain(Password);
    }

    [Theory]
    [InlineData("main", "all", new[] { "product:GIZLI-1:allow" })]
    [InlineData("except", "all", new[] { "category:Temizlik:deny", "product:CAY-1:deny" })]
    [InlineData("only", "only", new[] { "category:Temizlik:allow", "product:CAY-1:allow" })]
    public void The_visibility_choice_goes_as_mode_and_rules(string choice, string mode, string[] rules)
    {
        var api = SetupCustomerPage();
        var cut = RenderCustomer();
        OpenSheet(cut);

        cut.Find($"#access-visibility [data-mode='{choice}']").Click();
        if (choice == "main")
        {
            SearchProducts(cut, enter: true);
            cut.FindAll("#access-product-results li[data-stock='CAY-1'] .product-reveal").Should().BeEmpty("only a product the company hid is revealed");
            cut.Find("#access-product-results li[data-stock='GIZLI-1'] .product-reveal").Click();
            cut.Find("#access-revealed [data-stock='GIZLI-1']").TextContent.Should().Contain("Gizli çay");
        }
        else
        {
            cut.WaitForAssertion(() => cut.Find("#access-categories [data-value='Temizlik'] input"));
            cut.Find("#access-categories [data-value='Temizlik'] input").Change(true);
            SearchProducts(cut);
            cut.Find("#access-product-results li[data-stock='CAY-1'] .product-pick").Click();
            cut.Find("#access-products [data-stock='CAY-1']").Should().NotBeNull();
        }
        api.Answer(Accounts, new { account = Account(), issuedPassword = Password }, HttpStatusCode.Created);
        cut.Find("#access-save").Click();

        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.Method == HttpMethod.Post && r.PathAndQuery == Accounts));
        var visibility = Body(api, HttpMethod.Post, Accounts).GetProperty("visibility");
        visibility.GetProperty("mode").GetString().Should().Be(mode);
        Rules(visibility).Should().Equal(rules);
    }

    [Fact]
    public void A_password_of_ones_own_must_be_8_to_72_characters()
    {
        var api = SetupCustomerPage();
        var cut = RenderCustomer();
        OpenSheet(cut);

        cut.Find("#access-password-manual").Change(true);
        cut.Find("#access-password").Change("kisa");
        cut.Find("#access-save").Click();

        cut.Find("#access-error").TextContent.Should().Contain("8–72");
        api.Requests.Should().NotContain(r => r.PathAndQuery == Accounts);

        cut.Find("#access-password").Change("uzunsifre1");
        api.Answer(Accounts, new { account = Account(), issuedPassword = (string?)null }, HttpStatusCode.Created);
        cut.Find("#access-save").Click();

        cut.WaitForAssertion(() => cut.Find("#access-facts"));
        Body(api, HttpMethod.Post, Accounts).GetProperty("password").GetString().Should().Be("uzunsifre1");
        cut.FindAll("#access-issued").Should().BeEmpty("a password the user chose is not shown back");
    }

    [Fact]
    public void An_existing_access_saves_the_whole_form_resets_the_password_once_and_is_deleted_after_confirming()
    {
        var api = SetupCustomerPage();
        api.Answer(ByCustomer, Lookup(Account("only", [new { type = "category", key = "Icecek", effect = "allow" }])));
        var cut = RenderCustomer();
        OpenSheet(cut);
        cut.WaitForAssertion(() => cut.Find("#access-categories [data-value='Icecek'] input").HasAttribute("checked").Should().BeTrue());

        cut.Find("#access-created").TextContent.Should().Contain("Firma Sahibi");
        cut.Find("#access-last-login").TextContent.Should().Be("Henüz giriş yapmadı");
        cut.FindAll("#access-password-auto").Should().BeEmpty("an existing access changes its password with the reset button");
        cut.Find("#access-price-list").Change("");
        api.Answer($"{Accounts}/{AccountId}", new { account = Account("only", [new { type = "category", key = "Icecek", effect = "allow" }], priceListNo: null) });
        cut.Find("#access-save").Click();

        cut.WaitForAssertion(() => cut.Find("#access-notice").TextContent.Should().Contain("kaydedildi"));
        var patch = Body(api, HttpMethod.Patch, $"{Accounts}/{AccountId}");
        patch.GetProperty("priceListNo").ValueKind.Should().Be(JsonValueKind.Null, "the company's default list is sent as null");
        patch.TryGetProperty("customerCode", out _).Should().BeFalse();
        patch.TryGetProperty("password", out _).Should().BeFalse();
        patch.GetProperty("visibility").GetProperty("mode").GetString().Should().Be("only");
        Rules(patch.GetProperty("visibility")).Should().Equal("category:Icecek:allow");

        api.Answer($"{Accounts}/{AccountId}/password", new { issuedPassword = Password });
        cut.Find("#access-reset-password").Click();
        cut.WaitForAssertion(() => cut.Find("#access-issued-password").TextContent.Should().Be(Password));
        Body(api, HttpMethod.Put, $"{Accounts}/{AccountId}/password").GetProperty("password").ValueKind.Should().Be(JsonValueKind.Null);

        api.Answer($"{Accounts}/{AccountId}/revoke-sessions", new { }, HttpStatusCode.NoContent);
        cut.Find("#access-revoke").Click();
        cut.WaitForAssertion(() => cut.Find("#access-notice").TextContent.Should().Contain("oturumları kapatıldı"));

        cut.Find("#access-delete").Click();
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Delete, "the first click only asks");
        api.Answer(ByCustomer, Lookup(null));
        cut.Find("#access-delete-confirm").Click();

        cut.WaitForAssertion(() => cut.Find("#access-none"));
        api.Requests.Should().Contain(r => r.Method == HttpMethod.Delete && r.PathAndQuery == $"{Accounts}/{AccountId}");
        cut.FindAll("#access-issued").Should().BeEmpty();
    }

    [Fact]
    public void The_accesses_tab_lists_accounts_and_a_row_opens_the_same_sheet_with_the_customers_phone()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { Modules = ["customer_catalog"] });
        Services.GetRequiredService<NavigationManager>().NavigateTo("katalog?sekme=erisimler");
        api.Answer(Catalog + "/settings", Settings());
        api.Answer(Accounts + "?page=1", new
        {
            items = new[]
            {
                new
                {
                    id = AccountId, customerCode = "C/1", customerName = "Bakkal Ali", username = "bakkal.ali", isActive = true, discountPercent = 12.5m,
                    priceListNo = (int?)2, lastLoginAtMs = (long?)null, openOrderCount = 3,
                },
            },
            total = 1,
        });
        api.Answer(ByCustomer, Lookup(Account()));
        api.Answer(Customers + "/card?code=C%2F1", Card());

        var cut = Render<Katalog>();
        cut.WaitForAssertion(() => cut.Find("#catalog-accounts tr[data-customer='C/1']"));

        var row = cut.Find("#catalog-accounts tr[data-customer='C/1']");
        row.TextContent.Should().Contain("bakkal.ali").And.Contain("%12,5").And.Contain("2 — Toptan").And.Contain("Hiç").And.Contain("3");
        cut.FindAll("#accounts-pager .pager-size").Should().BeEmpty("the server fixes the page size");
        row.Click();

        cut.WaitForAssertion(() => cut.Find("#access-whatsapp").GetAttribute("href").Should().StartWith("https://wa.me/905320000000?text="));
        api.Requests.Should().Contain(r => r.PathAndQuery == ByCustomer);
        api.Requests.Should().Contain(r => r.PathAndQuery == Customers + "/card?code=C%2F1", "a list row has no phone, so the sheet reads the card");
    }
}
