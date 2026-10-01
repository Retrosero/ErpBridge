using System.Net;
using System.Security.Cryptography;
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
/// GOAL_MUSTERI_KATALOGU P2–P3: the catalog management page (general settings, category and product layout saved with
/// the revision read, 409 read again, unsaved changes guarded) and the product sheet's images.
/// </summary>
public sealed class PortalCatalogPagesTests : PortalPageTestContext
{
    private const string SettingsPath = "/api/v1/customer-catalog/settings";
    private const string CategoriesPath = "/api/v1/customer-catalog/categories";
    private const string ProductsSavePath = "/api/v1/customer-catalog/products";
    private const string ManifestPath = "/api/v1/customer-catalog/images/manifest";
    private const string ImagesPath = "/api/v1/customer-catalog/images";

    private static string ProductsPath(string category) => $"{ProductsSavePath}?category={category}";

    private static object Settings(bool enabled = true, int? defaultList = null, long revision = 7) => new
    {
        isEnabled = enabled, defaultPriceListNo = defaultList, effectiveDefaultPriceListNo = 1, revision, tenantCode = "ABCD2345",
        publicUrl = "https://sipariscepte.appsgo.cloud/ABCD2345",
        priceLists = new[] { new { no = 1, name = "Perakende", includesVat = true }, new { no = 2, name = "Toptan", includesVat = false } },
        imageQuota = new { usedBytes = 50L * 1024 * 1024, limitBytes = 1024L * 1024 * 1024 },
        counts = new { categories = 3, products = 3, visibleProducts = 2, accounts = 4, openOrders = 1 },
    };

    private static object Categories(long revision, params string[] keys) => new
    {
        revision,
        items = keys.Select(key => new
        {
            key, name = key, sortOrder = (int?)null, hidden = false, productCount = 3, hiddenCount = 0, noDiscountCount = 1, cartonOnlyCount = 0,
        }),
    };

    private static object Product(string code, string name, int? erpCarton = null, bool noDiscount = false, bool inStock = true, int imageCount = 0, string? thumb = null) => new
    {
        stockCode = code, name, unit = "Adet", brand = (string?)null, categoryKey = "Icecek", sortOrder = (int?)null, hidden = false, noDiscount,
        cartonOnly = false, cartonQuantity = (int?)null, erpCartonQuantity = erpCarton, listPrice = (decimal?)150m, inStock, imageCount, thumbUrl = thumb,
    };

    private static object Products(long revision = 7) => new
    {
        revision,
        truncated = false,
        items = new[]
        {
            Product("CAY-1", "Çay 1 kg", erpCarton: 12, imageCount: 1, thumb: "/api/v1/catalog/img/aaa/s?h=1234abcd"),
            Product("CAY-2", "Çay 5 kg", inStock: false),
            Product("KAHVE", "Kahve", noDiscount: true),
        },
    };

    private (FakeCentralApi Api, NavigationManager Nav) Setup(string address = "katalog?sekme=kategoriler")
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { Modules = ["customer_catalog"] });
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo(address);
        api.Answer(SettingsPath, Settings());
        api.Answer(CategoriesPath, Categories(7, "Icecek", "Temizlik", "Diger"));
        api.Answer(ProductsPath("Icecek"), Products());
        api.Answer(ProductsPath("Temizlik"), new { revision = 7, truncated = false, items = Array.Empty<object>() });
        return (api, nav);
    }

    private IRenderedComponent<Katalog> RenderLayoutTab()
    {
        var cut = Render<Katalog>();
        cut.WaitForAssertion(() => cut.Find("#catalog-products tr[data-stock='CAY-1']"));
        return cut;
    }

    private static JsonElement Body(FakeCentralApi api, HttpMethod method, string path) =>
        JsonDocument.Parse(api.Requests.Single(r => r.Method == method && r.PathAndQuery == path).Body!).RootElement;

    [Fact]
    public void The_page_stays_closed_without_the_module()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("katalog");

        Render<Katalog>();

        nav.Uri.Should().Be(nav.BaseUri, "an admin of a company without customer_catalog lands on the summary");
        api.Requests.Should().NotContain(r => r.PathAndQuery.StartsWith("/api/v1/customer-catalog", StringComparison.Ordinal));
    }

    [Fact]
    public void A_restored_session_learns_the_module_from_me_and_opens_the_page()
    {
        // Saved in the browser before sessions carried modules.
        var (api, _, storage) = PortalTestSetup.Register(this, inTab: PortalTestSetup.State() with { RememberMe = true });
        api.Answer("/api/v1/android/account/me", new
        {
            user = new { username = "patron", fullName = "Firma Sahibi", role = "ADMIN", roles = new[] { "ADMIN" }, canApprove = true },
            tenantName = "Ege Dağıtım",
            modules = new[] { "customer_catalog" },
        });
        api.Answer(SettingsPath, Settings());
        Services.GetRequiredService<NavigationManager>().NavigateTo("katalog");

        var cut = Render<Katalog>();

        cut.WaitForAssertion(() => cut.Find("#catalog-general"));
        storage.Stored!.Modules.Should().Equal("customer_catalog");
    }

    [Fact]
    public void General_settings_save_the_switch_and_the_list_with_the_revision_read()
    {
        var (api, nav) = Setup("katalog");
        api.Answer(SettingsPath, Settings(enabled: false));
        JSInterop.Setup<bool>("portalClipboard.copy", "https://sipariscepte.appsgo.cloud/ABCD2345").SetResult(true);

        var cut = Render<Katalog>();
        cut.WaitForAssertion(() => cut.Find("#catalog-general"));

        cut.Find("#catalog-closed").TextContent.Should().Be("Katalog kapalı");
        cut.Find("#catalog-vat").TextContent.Should().Be("KDV dahil", "the automatic list is list 1, which includes VAT");
        cut.Find("#catalog-url").TextContent.Should().Be("https://sipariscepte.appsgo.cloud/ABCD2345");
        cut.Find("#catalog-url-open").GetAttribute("target").Should().Be("_blank");
        cut.Find("#catalog-quota").TextContent.Should().Contain("50,0 MB / 1,0 GB");
        cut.Find("#catalog-count-products").TextContent.Should().Contain("2").And.Contain("3 ürünün");
        cut.Find("#catalog-settings-save").HasAttribute("disabled").Should().BeTrue();
        nav.Uri.Should().EndWith("/katalog");

        cut.Find("#catalog-url-copy").Click();
        cut.WaitForAssertion(() => cut.Find("#catalog-url-copied").TextContent.Should().Be("Kopyalandı."));

        cut.Find("#catalog-enabled").Change(true);
        cut.Find("#catalog-price-list").Change("2");
        cut.Find("#catalog-vat").TextContent.Should().Be("KDV hariç");
        api.Answer(SettingsPath, Settings(enabled: true, defaultList: 2, revision: 8));
        cut.Find("#catalog-settings-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("katalog açık"));
        var sent = Body(api, HttpMethod.Put, SettingsPath);
        sent.GetProperty("revision").GetInt64().Should().Be(7);
        sent.GetProperty("isEnabled").GetBoolean().Should().BeTrue();
        sent.GetProperty("defaultPriceListNo").GetInt32().Should().Be(2);
        cut.FindAll("#catalog-closed").Should().BeEmpty();
        cut.Find("#catalog-settings-save").HasAttribute("disabled").Should().BeTrue("the saved values are the form's again");
    }

    [Fact]
    public void Moving_and_hiding_categories_saves_the_whole_list_in_its_new_order()
    {
        var (api, nav) = Setup();
        var cut = RenderLayoutTab();
        nav.Uri.Should().EndWith("/katalog?sekme=kategoriler&kategori=Icecek");
        cut.Find("#catalog-save").HasAttribute("disabled").Should().BeTrue();

        cut.Find("[data-category='Temizlik'] .category-up").Click();
        cut.Find("[data-category='Diger'] .category-hide").Click();

        cut.FindAll("#catalog-categories li").Select(li => li.GetAttribute("data-category")).Should().Equal("Temizlik", "Icecek", "Diger");
        cut.Find("[data-category='Diger'] .category-hide").TextContent.Should().Be("Gizli");
        cut.Find("#catalog-save").TextContent.Should().Contain("(3)", "two categories changed place and one was hidden");
        api.Answer(CategoriesPath, Categories(8, "Temizlik", "Icecek", "Diger"));
        cut.Find("#catalog-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("3 değişiklik kaydedildi"));
        var sent = Body(api, HttpMethod.Put, CategoriesPath);
        sent.GetProperty("revision").GetInt64().Should().Be(7);
        sent.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("key").GetString()).Should().Equal("Temizlik", "Icecek", "Diger");
        sent.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("hidden").GetBoolean()).Should().Equal(false, false, true);
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Put && r.PathAndQuery == ProductsSavePath, "no product changed");
        api.Requests.Count(r => r.Method == HttpMethod.Get && r.PathAndQuery == CategoriesPath).Should().Be(2, "the saved layout is read back");
    }

    [Fact]
    public void Select_all_then_no_discount_sends_only_the_products_that_changed()
    {
        var (api, _) = Setup();
        var cut = RenderLayoutTab();

        cut.Find("#catalog-select-all").Click();
        cut.Find("#catalog-selected-count").TextContent.Should().Be("3 ürün seçili");
        cut.Find("#bulk-no-discount").Click();

        cut.Find("#catalog-save").TextContent.Should().Contain("(2)", "Kahve was already without discount");
        api.Answer(ProductsSavePath, new { revision = 8 });
        cut.Find("#catalog-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("2 değişiklik kaydedildi"));
        var sent = Body(api, HttpMethod.Put, ProductsSavePath);
        sent.GetProperty("revision").GetInt64().Should().Be(7);
        var items = sent.GetProperty("items").EnumerateArray().ToList();
        items.Select(i => i.GetProperty("stockCode").GetString()).Should().Equal("CAY-1", "CAY-2");
        items.Should().OnlyContain(i => i.GetProperty("noDiscount").GetBoolean() && !i.GetProperty("hidden").GetBoolean());
        items.Should().OnlyContain(i => i.GetProperty("sortOrder").ValueKind == JsonValueKind.Null, "the order did not change, so each keeps its place");
    }

    [Fact]
    public void Carton_only_needs_a_carton_size_the_catalog_or_the_erp_gives()
    {
        var (api, _) = Setup();
        var cut = RenderLayoutTab();

        cut.Find("tr[data-stock='CAY-2'] .product-carton-only").Click();
        cut.Find("#page-error").TextContent.Should().Contain("önce koli adedini girin");
        cut.Find("#catalog-save").HasAttribute("disabled").Should().BeTrue();

        cut.Find("tr[data-stock='CAY-1'] .product-carton-only").Click();
        cut.Find("tr[data-stock='CAY-2'] .product-carton").Change("6");
        cut.Find("tr[data-stock='CAY-2'] .product-carton-only").Click();
        api.Answer(ProductsSavePath, new { revision = 8 });
        cut.Find("#catalog-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice"));
        var items = Body(api, HttpMethod.Put, ProductsSavePath).GetProperty("items").EnumerateArray().ToList();
        items[0].GetProperty("stockCode").GetString().Should().Be("CAY-1");
        items[0].GetProperty("cartonOnly").GetBoolean().Should().BeTrue("the ERP's carton of 12 is enough");
        items[0].GetProperty("cartonQuantity").ValueKind.Should().Be(JsonValueKind.Null);
        items[1].GetProperty("cartonOnly").GetBoolean().Should().BeTrue();
        items[1].GetProperty("cartonQuantity").GetInt32().Should().Be(6);
    }

    [Fact]
    public void Bulk_carton_only_skips_and_names_the_products_without_a_carton_size()
    {
        var (api, _) = Setup();
        var cut = RenderLayoutTab();

        cut.Find("#catalog-select-all").Click();
        cut.Find("#bulk-carton-only").Click();

        cut.Find("#page-error").TextContent.Should().Contain("2 üründe koli adedi yok");
        cut.Find("tr[data-stock='CAY-1'] .product-carton-only").ClassList.Should().Contain("is-on", "the ERP gives it a carton of 12");
        cut.Find("tr[data-stock='CAY-2'] .product-carton-only").ClassList.Should().Contain("is-off");
        api.Answer(ProductsSavePath, new { revision = 8 });
        cut.Find("#catalog-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("1 değişiklik kaydedildi"));
        var items = Body(api, HttpMethod.Put, ProductsSavePath).GetProperty("items").EnumerateArray().ToList();
        items.Should().ContainSingle().Which.GetProperty("stockCode").GetString().Should().Be("CAY-1");
        items[0].GetProperty("cartonOnly").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void A_catalog_changed_elsewhere_is_read_again_and_the_edits_are_dropped()
    {
        var (api, _) = Setup();
        var cut = RenderLayoutTab();
        api.Fail(ProductsSavePath, HttpStatusCode.Conflict, "CATALOG_CHANGED");

        cut.Find("tr[data-stock='CAY-1'] .product-down").Click();
        cut.FindAll("#catalog-products tbody tr").Select(tr => tr.GetAttribute("data-stock")).Should().Equal("CAY-2", "CAY-1", "KAHVE");
        cut.Find("#catalog-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("başka biri tarafından değiştirildi"));
        var sent = Body(api, HttpMethod.Put, ProductsSavePath).GetProperty("items").EnumerateArray().ToList();
        sent.Select(i => i.GetProperty("stockCode").GetString()).Should().Equal("CAY-2", "CAY-1", "KAHVE");
        sent.Select(i => i.GetProperty("sortOrder").GetInt32()).Should().Equal([1, 2, 3], "a new order numbers the whole category");
        api.Requests.Count(r => r.Method == HttpMethod.Get && r.PathAndQuery == CategoriesPath).Should().Be(2);
        api.Requests.Count(r => r.Method == HttpMethod.Get && r.PathAndQuery == ProductsPath("Icecek")).Should().Be(2);
        cut.FindAll("#catalog-products tbody tr").Select(tr => tr.GetAttribute("data-stock")).Should().Equal("CAY-1", "CAY-2", "KAHVE");
        cut.Find("#catalog-save").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Unsaved_changes_hold_a_category_or_tab_switch_until_saved_or_dropped()
    {
        var (api, nav) = Setup();
        var cut = RenderLayoutTab();

        cut.Find("tr[data-stock='CAY-1'] .product-hidden").Click();
        cut.Find("[data-category='Temizlik'] .category-open").Click();

        cut.Find("#catalog-unsaved").TextContent.Should().Contain("1 değişiklik kaydedilmedi");
        api.Requests.Should().NotContain(r => r.PathAndQuery == ProductsPath("Temizlik"));
        cut.Find("#catalog-unsaved-cancel").Click();
        cut.FindAll("#catalog-unsaved").Should().BeEmpty();

        cut.Find("#catalog-tabs [data-tab='genel']").Click();
        cut.Find("#catalog-unsaved").Should().NotBeNull("leaving the tab asks too");
        cut.Find("#catalog-unsaved-cancel").Click();

        cut.Find("[data-category='Temizlik'] .category-open").Click();
        cut.Find("#catalog-unsaved-discard").Click();

        cut.WaitForAssertion(() => cut.Find("#catalog-products-empty"));
        api.Requests.Should().Contain(r => r.PathAndQuery == ProductsPath("Temizlik"));
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Put);
        nav.Uri.Should().EndWith("kategori=Temizlik");
        cut.Find("#catalog-save").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void An_image_goes_up_as_a_record_then_its_large_and_small_variants_in_order()
    {
        var (api, _) = Setup();
        var existing = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var added = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        object Image(Guid id, int sortOrder, string source) => new
        {
            id, kind = "file", url = (string?)null, sourceHash = "h" + sortOrder, source, sortOrder, hasSmall = true, hasLarge = true,
            thumbUrl = $"/api/v1/catalog/img/{id:N}/s?h=1234abcd", fullUrl = $"/api/v1/catalog/img/{id:N}/l?h=1234abcd",
        };
        api.Answer(ManifestPath, new { usedBytes = 1000L, limitBytes = 1024L * 1024 * 1024, items = new[] { new { stockCode = "CAY-1", images = new[] { Image(existing, 0, "phone") } } } });
        var cut = RenderLayoutTab();

        cut.Find("tr[data-stock='CAY-1'] .product-settings").Click();
        cut.WaitForAssertion(() => cut.FindAll("#sheet-images [data-image]").Should().ContainSingle());
        cut.Find($"[data-image='{existing}'] img").GetAttribute("src").Should().Be(
            $"https://sipariscepte.appsgo.cloud/api/v1/catalog/img/{existing:N}/s?h=1234abcd", "the panel loads images from the public catalog host");
        cut.Find($"[data-image='{existing}']").TextContent.Should().Contain("Telefondan").And.Contain("Kapak");

        api.Answer(ImagesPath, new { image = new { id = added, kind = "file", sourceHash = "x", source = "panel", sortOrder = 1, hasSmall = false, hasLarge = false } });
        api.AnswerPrefix(HttpMethod.Put, $"{ImagesPath}/{added}/", new { }, HttpStatusCode.NoContent);
        api.Answer(ManifestPath, new
        {
            usedBytes = 2000L, limitBytes = 1024L * 1024 * 1024,
            items = new[] { new { stockCode = "CAY-1", images = new[] { Image(existing, 0, "phone"), Image(added, 1, "panel") } } },
        });
        var original = Encoding.ASCII.GetBytes("jpeg-bytes");
        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(original, "foto.jpg", contentType: "image/jpeg"));

        cut.WaitForAssertion(() => cut.FindAll("#sheet-images [data-image]").Should().HaveCount(2));
        var writes = api.Requests.Where(r => r.Method != HttpMethod.Get).ToList();
        writes.Select(r => $"{r.Method} {r.PathAndQuery}").Should().Equal(
            $"POST {ImagesPath}", $"PUT {ImagesPath}/{added}/l", $"PUT {ImagesPath}/{added}/s");
        var record = JsonDocument.Parse(writes[0].Body!).RootElement;
        record.GetProperty("stockCode").GetString().Should().Be("CAY-1");
        record.GetProperty("source").GetString().Should().Be("panel");
        record.GetProperty("sourceHash").GetString().Should().Be(Convert.ToHexStringLower(SHA256.HashData(original)), "the hash is of the file as picked");
        record.TryGetProperty("url", out _).Should().BeFalse();
        writes[1].Body.Should().Be("foto.jpg@1280");
        writes[1].ContentType.Should().Be("image/jpeg");
        writes[2].Body.Should().Be("foto.jpg@400");
        cut.Find($"[data-image='{added}']").TextContent.Should().Contain("Panelden");
        cut.Find("tr[data-stock='CAY-1'] .cell-sub").TextContent.Should().Contain("2 görsel", "the list's row follows the sheet");
    }

    [Fact]
    public void A_rate_limited_upload_stops_and_asks_to_wait()
    {
        var (api, _) = Setup();
        api.Answer(ManifestPath, new { usedBytes = 0L, limitBytes = 1024L * 1024 * 1024, items = Array.Empty<object>() });
        api.Answer(ImagesPath, new { errorCode = "RATE_LIMITED" }, HttpStatusCode.TooManyRequests);
        var cut = RenderLayoutTab();
        cut.Find("tr[data-stock='CAY-1'] .product-settings").Click();
        cut.WaitForAssertion(() => cut.Find("#sheet-images-empty"));

        cut.FindComponent<InputFile>().UploadFiles(
            InputFileContent.CreateFromBinary(Encoding.ASCII.GetBytes("bir"), "bir.jpg", contentType: "image/jpeg"),
            InputFileContent.CreateFromBinary(Encoding.ASCII.GetBytes("iki"), "iki.png", contentType: "image/png"));

        cut.WaitForAssertion(() => cut.Find("#sheet-image-error").TextContent.Should().Contain("biraz bekleyip"));
        api.Requests.Count(r => r.Method == HttpMethod.Post).Should().Be(1, "the second file waits for the user instead of hitting the limit again");
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Put);
    }

    [Fact]
    public void A_link_image_must_be_https_and_is_registered_with_its_address()
    {
        var (api, _) = Setup();
        var link = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        api.Answer(ManifestPath, new { usedBytes = 0L, limitBytes = 1024L * 1024 * 1024, items = Array.Empty<object>() });
        var cut = RenderLayoutTab();
        cut.Find("tr[data-stock='CAY-2'] .product-settings").Click();
        cut.WaitForAssertion(() => cut.Find("#sheet-images-empty"));

        cut.Find("#sheet-image-link").Change("http://ornek.com/cay.jpg");
        cut.Find("#sheet-image-link-add").Click();
        cut.Find("#sheet-image-error").TextContent.Should().Contain("https://");
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Post);

        api.Answer(ImagesPath, new { image = new { id = link, kind = "link", url = "https://ornek.com/cay.jpg", sourceHash = "x", source = "panel", sortOrder = 0, thumbUrl = "https://ornek.com/cay.jpg" } });
        api.Answer(ManifestPath, new
        {
            usedBytes = 0L, limitBytes = 1024L * 1024 * 1024,
            items = new[] { new { stockCode = "CAY-2", images = new[] { new { id = link, kind = "link", url = "https://ornek.com/cay.jpg", sourceHash = "x", source = "panel", sortOrder = 0, thumbUrl = "https://ornek.com/cay.jpg" } } } },
        });
        cut.Find("#sheet-image-link").Change("https://ornek.com/cay.jpg");
        cut.Find("#sheet-image-link-add").Click();

        cut.WaitForAssertion(() => cut.Find($"[data-image='{link}'] img").GetAttribute("src").Should().Be("https://ornek.com/cay.jpg"));
        var record = Body(api, HttpMethod.Post, ImagesPath);
        record.GetProperty("stockCode").GetString().Should().Be("CAY-2");
        record.GetProperty("url").GetString().Should().Be("https://ornek.com/cay.jpg");
        record.GetProperty("sourceHash").GetString().Should().HaveLength(64);
        cut.Find($"[data-image='{link}']").TextContent.Should().Contain("Bağlantı");
    }

    [Fact]
    public void The_sheet_edits_the_rows_flags_and_orders_or_deletes_images_at_once()
    {
        var (api, _) = Setup();
        var first = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        var second = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        object Image(Guid id, int sortOrder) => new
        {
            id, kind = "file", sourceHash = "h" + sortOrder, source = "panel", sortOrder, hasSmall = true, hasLarge = true,
            thumbUrl = $"/api/v1/catalog/img/{id:N}/s?h=1234abcd",
        };
        api.Answer(ManifestPath, new { usedBytes = 0L, limitBytes = 1024L * 1024 * 1024, items = new[] { new { stockCode = "CAY-1", images = new[] { Image(first, 0), Image(second, 1) } } } });
        api.AnswerPrefix(HttpMethod.Put, ImagesPath + "/order", new { }, HttpStatusCode.NoContent);
        api.AnswerPrefix(HttpMethod.Delete, ImagesPath + "/", new { }, HttpStatusCode.NoContent);
        var cut = RenderLayoutTab();
        cut.Find("tr[data-stock='CAY-1'] .product-settings").Click();
        cut.WaitForAssertion(() => cut.FindAll("#sheet-images [data-image]").Should().HaveCount(2));

        cut.Find("#sheet-hidden").Change(true);
        cut.Find("tr[data-stock='CAY-1'] .product-hidden").ClassList.Should().Contain("is-on", "the sheet and the row edit the same product");
        cut.Find("#catalog-save").TextContent.Should().Contain("(1)");
        cut.Find("#sheet-carton-only").HasAttribute("disabled").Should().BeFalse("the ERP gives this product a carton of 12");

        cut.Find($"[data-image='{second}'] .image-left").Click();
        cut.WaitForAssertion(() => cut.FindAll("#sheet-images [data-image]").Select(f => f.GetAttribute("data-image")).Should().Equal(second.ToString(), first.ToString()));
        var order = api.Requests.Single(r => r.Method == HttpMethod.Put && r.PathAndQuery.StartsWith(ImagesPath + "/order", StringComparison.Ordinal));
        order.PathAndQuery.Should().Be($"{ImagesPath}/order?stockCode=CAY-1");
        JsonDocument.Parse(order.Body!).RootElement.GetProperty("ids").EnumerateArray().Select(i => i.GetGuid()).Should().Equal(second, first);

        cut.Find($"[data-image='{first}'] .image-delete").Click();
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Delete, "the first click only asks");
        cut.Find($"[data-image='{first}'] .image-delete-confirm").Click();

        cut.WaitForAssertion(() => cut.FindAll("#sheet-images [data-image]").Should().ContainSingle());
        api.Requests.Should().Contain(r => r.Method == HttpMethod.Delete && r.PathAndQuery == $"{ImagesPath}/{first}");
        cut.Find("tr[data-stock='CAY-1'] .cell-sub").TextContent.Should().Contain("1 görsel");
        cut.Find("tr[data-stock='CAY-1'] img.catalog-thumb").GetAttribute("src").Should().Contain(second.ToString("N"), "the new cover shows in the list");
    }

    [Fact]
    public void The_sheet_signs_out_when_the_server_ended_the_session()
    {
        var (api, nav) = Setup();
        api.Fail(ManifestPath, HttpStatusCode.Unauthorized, "SESSION_REVOKED");
        var cut = RenderLayoutTab();

        cut.Find("tr[data-stock='CAY-1'] .product-settings").Click();

        cut.WaitForAssertion(() => nav.Uri.Should().EndWith("/login?reason=SESSION_REVOKED"));
        Services.GetRequiredService<ErpBridge.Portal.Session.PortalSession>().IsSignedIn.Should().BeFalse();
    }

    [Theory]
    [InlineData("MODULE_NOT_ENABLED")]
    [InlineData("CATALOG_MANAGE_REQUIRED")]
    [InlineData("CATALOG_CHANGED")]
    [InlineData("UNKNOWN_PRICE_LIST")]
    [InlineData("CARTON_QUANTITY_REQUIRED")]
    [InlineData("CUSTOMER_NOT_FOUND")]
    [InlineData("INVALID_IMAGE")]
    [InlineData("IMAGE_TOO_LARGE")]
    [InlineData("CATALOG_IMAGE_LIMIT")]
    [InlineData("CATALOG_IMAGE_QUOTA_EXCEEDED")]
    [InlineData("STORAGE_QUOTA_EXCEEDED")]
    [InlineData("STORAGE_UNAVAILABLE")]
    [InlineData("STORED_FILE_NOT_FOUND")]
    [InlineData("INVALID_IMAGE_URL")]
    [InlineData("CATALOG_ORDER_TAKEN")]
    [InlineData("CATALOG_ORDER_CLOSED")]
    [InlineData("CATALOG_ORDER_ALREADY_CONVERTED")]
    [InlineData("CATALOG_ORDER_NOT_FOUND")]
    [InlineData("INVALID_CARTON_QUANTITY")]
    [InlineData("INVALID_VISIBILITY")]
    [InlineData("INVALID_RESPONSIBLE_USER")]
    [InlineData("CATALOG_IMAGE_NOT_FOUND")]
    [InlineData("INVALID_BODY")]
    [InlineData("RATE_LIMITED")]
    [InlineData("HTTP_429")]
    public void Catalog_refusals_read_in_turkish(string code) =>
        PortalMessages.For(code).Should().NotContain(code, "an unknown code falls back to a message quoting it");

    [Fact]
    public void An_unknown_code_takes_the_servers_message_only_when_it_is_turkish()
    {
        PortalMessages.For("NEW_CODE", "Koli adedi 2 ile 100000 arasında olmalı.").Should().Be("Koli adedi 2 ile 100000 arasında olmalı.");
        PortalMessages.For("NEW_CODE", "Body required.").Should().Be(PortalMessages.For("NEW_CODE"));
        PortalMessages.For("NEW_CODE", null).Should().Be(PortalMessages.For("NEW_CODE"));
        PortalMessages.For("INVALID_DISCOUNT", "İskonto hatalı ölçü.").Should().Be(PortalMessages.For("INVALID_DISCOUNT"), "a known code keeps the panel's own text");
        PortalMessages.Knows("INVALID_BODY").Should().BeTrue();
        PortalMessages.Knows("NEW_CODE").Should().BeFalse();
    }

    [Fact]
    public void Image_addresses_use_the_catalog_host_and_fall_back_to_the_api()
    {
        var api = new Uri("https://lisans.appsgo.cloud/");

        CatalogImageAddress.Resolve("/api/v1/catalog/img/x/s?h=1", "https://sipariscepte.appsgo.cloud/ABCD2345", api)
            .Should().Be("https://sipariscepte.appsgo.cloud/api/v1/catalog/img/x/s?h=1");
        CatalogImageAddress.Resolve("/api/v1/catalog/img/x/s?h=1", null, api).Should().Be("https://lisans.appsgo.cloud/api/v1/catalog/img/x/s?h=1");
        CatalogImageAddress.Resolve("https://cdn.ornek.com/a.jpg", "https://sipariscepte.appsgo.cloud/ABCD2345", api).Should().Be("https://cdn.ornek.com/a.jpg");
        CatalogImageAddress.Resolve(null, "https://sipariscepte.appsgo.cloud/ABCD2345", api).Should().BeNull();
    }
}
