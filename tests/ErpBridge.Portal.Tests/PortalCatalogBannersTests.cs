using System.Net;
using System.Text;
using System.Text.Json;
using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// GOAL_MUSTERI_KATALOGU P8: the catalog page's banners tab (list, order, state) and the banner sheet — a picture under
/// "~banner" in a wide box, a title and text, a link to a category, a product or an https address, Istanbul dates.
/// </summary>
public sealed class PortalCatalogBannersTests : PortalPageTestContext
{
    private const string SettingsPath = "/api/v1/customer-catalog/settings";
    private const string BannersPath = "/api/v1/customer-catalog/banners";
    private const string ImagesPath = "/api/v1/customer-catalog/images";
    private const string CategoriesPath = "/api/v1/customer-catalog/categories";

    private static readonly Guid First = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Second = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Picture = Guid.Parse("33333333-3333-3333-3333-333333333333");

    // PortalTestSetup.Now = 2026-09-21 09:00 UTC (12:00 in Istanbul).
    private static readonly long Now = PortalTestSetup.Now.ToUnixTimeMilliseconds();

    private static object Settings() => new
    {
        isEnabled = true, defaultPriceListNo = (int?)null, effectiveDefaultPriceListNo = 1, revision = 7, tenantCode = "ABCD2345",
        publicUrl = "https://sipariscepte.appsgo.cloud/ABCD2345",
        priceLists = new[] { new { no = 1, name = "Perakende", includesVat = true } },
        imageQuota = new { usedBytes = 0L, limitBytes = 1024L * 1024 * 1024 },
        counts = new { categories = 2, products = 3, visibleProducts = 3, accounts = 0, openOrders = 0 },
    };

    private static object Banner(Guid id, string title, int sortOrder, bool isActive = true, bool live = true, long? startsAtMs = null, long? endsAtMs = null,
        string linkType = "none", string linkValue = "", string? linkName = null, bool picture = false) => new
    {
        id, title, text = title + " metni", imageId = picture ? Picture : (Guid?)null,
        image = picture ? new { id = Picture, kind = "file", sourceHash = "h", source = "panel", sortOrder = 0, hasSmall = true, hasLarge = true,
            thumbUrl = $"/api/v1/catalog/img/{Picture}/s?h=1234abcd", fullUrl = $"/api/v1/catalog/img/{Picture}/l?h=1234abcd" } : null,
        linkType, linkValue, linkName, sortOrder, isActive, startsAtMs, endsAtMs, live,
    };

    private (FakeCentralApi Api, NavigationManager Nav) Setup(params object[] banners)
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { Modules = ["customer_catalog"] });
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("katalog?sekme=bannerlar");
        api.Answer(SettingsPath, Settings());
        api.Answer(BannersPath, new { items = banners });
        api.Answer(CategoriesPath, new
        {
            revision = 7,
            items = new[] { "Çay", "Kahve" }.Select(key => new { key, name = key, sortOrder = (int?)null, hidden = false, productCount = 2, hiddenCount = 0, noDiscountCount = 0, cartonOnlyCount = 0 }),
        });
        return (api, nav);
    }

    private static JsonElement Body(FakeCentralApi api, HttpMethod method, string path) =>
        JsonDocument.Parse(api.Requests.Last(r => r.Method == method && r.PathAndQuery == path).Body!).RootElement;

    [Fact]
    public void The_tab_lists_the_banners_in_order_with_their_state_and_moves_them_at_once()
    {
        var day = TimeSpan.FromDays(1).TotalMilliseconds;
        var (api, _) = Setup(
            Banner(First, "Yaz kampanyası", 0, picture: true, linkType: "category", linkValue: "Çay", linkName: "Çay",
                startsAtMs: Now - (long)day, endsAtMs: Now + (long)day),
            Banner(Second, "Kış", 1, isActive: true, live: false, startsAtMs: Now + 2 * (long)day, linkType: "product", linkValue: "ESKI", linkName: null),
            Banner(Guid.NewGuid(), "Kapalı", 2, isActive: false, live: false, linkType: "url", linkValue: "https://ornek.com/a"));
        api.AnswerPrefix(HttpMethod.Put, BannersPath + "/order", new { }, HttpStatusCode.NoContent);
        var cut = Render<Katalog>();

        cut.WaitForAssertion(() => cut.FindAll("#catalog-banners tbody tr").Should().HaveCount(3));
        var first = cut.Find($"tr[data-banner='{First}']");
        first.QuerySelector("img.banner-thumb")!.GetAttribute("src").Should().Be(
            $"https://sipariscepte.appsgo.cloud/api/v1/catalog/img/{Picture}/s?h=1234abcd", "pictures load from the public catalog host");
        first.QuerySelector(".banner-link")!.TextContent.Should().Be("Kategori: Çay");
        first.QuerySelector(".banner-dates-cell")!.TextContent.Should().Be("20.09.2026 – 22.09.2026", "Istanbul days of the stored instants");
        first.QuerySelector(".banner-state")!.TextContent.Should().Be("Yayında");
        var second = cut.Find($"tr[data-banner='{Second}']");
        second.QuerySelector(".banner-state")!.TextContent.Should().Be("Başlamadı");
        second.QuerySelector(".banner-link")!.TextContent.Should().Contain("katalogda yok");
        cut.FindAll(".banner-state").Last().TextContent.Should().Be("Pasif");

        cut.Find($"tr[data-banner='{Second}'] .banner-up").Click();

        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.Method == HttpMethod.Put && r.PathAndQuery == BannersPath + "/order"));
        Body(api, HttpMethod.Put, BannersPath + "/order").GetProperty("ids").EnumerateArray().Select(i => i.GetGuid()).Take(2).Should().Equal(Second, First);
        api.Requests.Count(r => r.Method == HttpMethod.Get && r.PathAndQuery == BannersPath).Should().Be(2, "the list is read again after the move");
    }

    [Fact]
    public void A_new_banner_uploads_a_wide_picture_then_saves_its_link_and_istanbul_days()
    {
        var (api, _) = Setup();
        api.Answer(ImagesPath, new { image = new { id = Picture, kind = "file", sourceHash = "x", source = "panel", sortOrder = 0, hasSmall = false, hasLarge = false } });
        api.AnswerPrefix(HttpMethod.Put, $"{ImagesPath}/{Picture}/", new { }, HttpStatusCode.NoContent);
        var cut = Render<Katalog>();
        cut.WaitForAssertion(() => cut.Find("#banners-empty"));

        cut.Find("#catalog-banner-new").Click();
        cut.WaitForAssertion(() => cut.Find("#catalog-banner-sheet"));
        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(Encoding.ASCII.GetBytes("afis"), "afis.jpg", contentType: "image/jpeg"));

        cut.WaitForAssertion(() => cut.Find("#banner-preview img").GetAttribute("src").Should().StartWith($"https://sipariscepte.appsgo.cloud/api/v1/catalog/img/{Picture}/l?h="));
        var writes = api.Requests.Where(r => r.Method != HttpMethod.Get).ToList();
        writes.Select(r => $"{r.Method} {r.PathAndQuery}").Should().Equal($"POST {ImagesPath}", $"PUT {ImagesPath}/{Picture}/l", $"PUT {ImagesPath}/{Picture}/s");
        JsonDocument.Parse(writes[0].Body!).RootElement.GetProperty("stockCode").GetString().Should().Be("~banner");
        writes[1].Body.Should().Be("afis.jpg@1920x720", "a banner is fitted in a wide box");
        writes[2].Body.Should().Be("afis.jpg@800x300");

        cut.Find("#banner-title").Input("Yaz kampanyası");
        cut.Find("#banner-text").Input("Çaylarda indirim");
        cut.Find("#banner-preview").TextContent.Should().Contain("Yaz kampanyası").And.Contain("Çaylarda indirim");
        cut.Find("#banner-link-type").Change("category");
        cut.WaitForAssertion(() => cut.FindAll("#banner-link-category option").Should().HaveCount(3));
        cut.Find("#banner-link-category").Change("Kahve");
        cut.Find("#banner-starts").Change("2026-10-01");
        cut.Find("#banner-ends").Change("2026-10-31");
        // The fake answers by path: the save gets this body too, which is enough for the page to carry on.
        api.Answer(BannersPath, new { items = new[] { Banner(First, "Yaz kampanyası", 0, picture: true) } });
        cut.Find("#banner-save").Click();

        cut.WaitForAssertion(() => cut.FindAll("#catalog-banner-sheet").Should().BeEmpty());
        var saved = Body(api, HttpMethod.Post, BannersPath);
        saved.GetProperty("title").GetString().Should().Be("Yaz kampanyası");
        saved.GetProperty("text").GetString().Should().Be("Çaylarda indirim");
        saved.GetProperty("imageId").GetGuid().Should().Be(Picture);
        saved.GetProperty("linkType").GetString().Should().Be("category");
        saved.GetProperty("linkValue").GetString().Should().Be("Kahve");
        saved.GetProperty("isActive").GetBoolean().Should().BeTrue();
        saved.GetProperty("startsAtMs").GetInt64().Should().Be(new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(3)).ToUnixTimeMilliseconds());
        saved.GetProperty("endsAtMs").GetInt64().Should().Be(new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.FromHours(3)).ToUnixTimeMilliseconds(),
            "the last day counts to its end: the next day's start is sent");
        cut.WaitForAssertion(() => cut.Find($"tr[data-banner='{First}']"));
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Delete, "the saved picture stays");
    }

    [Fact]
    public void A_product_link_is_picked_from_a_search_and_an_outside_link_must_be_https()
    {
        var (api, _) = Setup();
        api.Answer("/api/v1/customer-catalog/products?q=kahve", new
        {
            revision = 7, truncated = false,
            items = new[] { new { stockCode = "KHV-1", name = "Türk Kahvesi", unit = "Adet", categoryKey = "Kahve", hidden = false, noDiscount = false, cartonOnly = false, listPrice = 50m, inStock = true, imageCount = 0 } },
        });
        var cut = Render<Katalog>();
        cut.WaitForElement("#catalog-banner-new").Click();

        cut.Find("#banner-save").Click();
        cut.Find("#banner-error").TextContent.Should().Contain("Başlık ya da görsel");

        cut.Find("#banner-title").Input("Kahve haftası");
        cut.Find("#banner-link-type").Change("product");
        cut.Find("#banner-save").Click();
        cut.Find("#banner-error").TextContent.Should().Contain("ürün seçin");
        cut.Find("#banner-product-search").Change("kahve");
        cut.Find("#banner-product-search-go").Click();
        cut.WaitForElement("#banner-product-results [data-stock='KHV-1']").Click();
        cut.Find("#banner-link-product").TextContent.Should().Contain("Türk Kahvesi").And.Contain("KHV-1");

        cut.Find("#banner-link-type").Change("url");
        cut.Find("#banner-link-url").Change("http://ornek.com/kampanya");
        cut.Find("#banner-save").Click();
        cut.Find("#banner-error").TextContent.Should().Contain("https://");
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Post && r.PathAndQuery == BannersPath, "nothing is sent until the form is right");

        cut.Find("#banner-link-type").Change("product");
        cut.Find("#banner-product-search").Change("kahve");
        cut.Find("#banner-product-search-go").Click();
        cut.WaitForElement("#banner-product-results [data-stock='KHV-1']").Click();
        api.Answer(BannersPath, new { errorCode = "INVALID_BANNER_LINK", message = "Bağlantı verilen ürün katalogda yok." }, HttpStatusCode.BadRequest);
        cut.Find("#banner-save").Click();

        cut.WaitForAssertion(() => cut.Find("#banner-error").TextContent.Should().Be(PortalMessages.For("INVALID_BANNER_LINK")));
        var sent = Body(api, HttpMethod.Post, BannersPath);
        sent.GetProperty("linkType").GetString().Should().Be("product");
        sent.GetProperty("linkValue").GetString().Should().Be("KHV-1");
        sent.GetProperty("imageId").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public void Closing_without_saving_takes_back_the_uploaded_picture()
    {
        var (api, _) = Setup();
        api.Answer(ImagesPath, new { image = new { id = Picture, kind = "file", sourceHash = "x", source = "panel", sortOrder = 0, hasSmall = false, hasLarge = false } });
        api.AnswerPrefix(HttpMethod.Put, $"{ImagesPath}/{Picture}/", new { }, HttpStatusCode.NoContent);
        api.AnswerPrefix(HttpMethod.Delete, ImagesPath + "/", new { }, HttpStatusCode.NoContent);
        var cut = Render<Katalog>();
        cut.WaitForElement("#catalog-banner-new").Click();
        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(Encoding.ASCII.GetBytes("afis"), "afis.jpg", contentType: "image/jpeg"));
        cut.WaitForAssertion(() => cut.Find("#banner-preview img"));

        cut.Find("#banner-cancel").Click();

        cut.WaitForAssertion(() => cut.FindAll("#catalog-banner-sheet").Should().BeEmpty());
        api.Requests.Should().Contain(r => r.Method == HttpMethod.Delete && r.PathAndQuery == $"{ImagesPath}/{Picture}");
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Post && r.PathAndQuery == BannersPath);
    }

    [Fact]
    public void An_existing_banner_opens_with_its_values_and_is_deleted_after_a_confirmation()
    {
        var (api, _) = Setup(Banner(First, "Yaz kampanyası", 0, picture: true, linkType: "url", linkValue: "https://ornek.com/yaz",
            startsAtMs: new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.FromHours(3)).ToUnixTimeMilliseconds(),
            endsAtMs: new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(3)).ToUnixTimeMilliseconds()));
        api.AnswerPrefix(HttpMethod.Delete, BannersPath + "/", new { }, HttpStatusCode.NoContent);
        var cut = Render<Katalog>();
        cut.WaitForElement($"tr[data-banner='{First}'] .banner-edit").Click();

        cut.Find("#banner-title").GetAttribute("value").Should().Be("Yaz kampanyası");
        cut.Find("#banner-link-url").GetAttribute("value").Should().Be("https://ornek.com/yaz");
        cut.Find("#banner-starts").GetAttribute("value").Should().Be("2026-09-01");
        cut.Find("#banner-ends").GetAttribute("value").Should().Be("2026-09-30", "the stored end is the next day's start");
        cut.Find("#banner-image-kind").TextContent.Should().Be("Yüklenen görsel");

        cut.Find("#banner-delete").Click();
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Delete, "the first click only asks");
        api.Answer(BannersPath, new { items = Array.Empty<object>() });
        cut.Find("#banner-delete-confirm").Click();

        cut.WaitForAssertion(() => cut.Find("#banners-empty"));
        api.Requests.Should().ContainSingle(r => r.Method == HttpMethod.Delete).Which.PathAndQuery.Should().Be($"{BannersPath}/{First}");
    }

    [Theory]
    [InlineData("CATALOG_BANNER_NOT_FOUND")]
    [InlineData("CATALOG_BANNER_LIMIT")]
    [InlineData("INVALID_BANNER_LINK")]
    [InlineData("INVALID_BANNER_DATES")]
    [InlineData("INVALID_BANNER_IMAGE")]
    public void Banner_refusals_read_in_turkish(string code) =>
        PortalMessages.For(code).Should().NotContain(code);

    [Fact]
    public void Istanbul_days_turn_into_instants_and_back()
    {
        var start = Fmt.DayStartMs(new DateOnly(2026, 10, 1));
        start.Should().Be(new DateTimeOffset(2026, 9, 30, 21, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds());
        Fmt.DayOfMs(start).Should().Be(new DateOnly(2026, 10, 1));
        Fmt.DayOfMs(start - 1).Should().Be(new DateOnly(2026, 9, 30));
    }
}
