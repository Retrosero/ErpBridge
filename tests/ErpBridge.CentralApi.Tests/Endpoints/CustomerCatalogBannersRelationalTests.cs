using System.Net;
using System.Net.Http.Headers;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static ErpBridge.CentralApi.Tests.Endpoints.CatalogCustomerTestSupport;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_MUSTERI_KATALOGU S12: catalog banners are managed like the rest of the catalog (module, then permission), kept
/// in their order through the picture lock (the layout revision does not move), linked only to an https address or a
/// category or product the catalog has; their pictures are catalog pictures under "~banner" that never count as a
/// product's; a deleted banner takes its picture along. Customers see only the active banners inside their dates, and
/// no link to what their own catalog hides. Seed: A "Çay Rize" (Çay), B "Kahve Türk" (Kahve), C "IŞIK Ampul" (Diğer).
/// </summary>
public sealed class CustomerCatalogBannersRelationalTests : IClassFixture<CatalogHostFactory>
{
    private const string Banners = Base + "/banners";
    private const string Images = Base + "/images";
    private const string Pass = "musteri123";

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4, 5, 6, 7, 8];

    private readonly CatalogHostFactory _factory;

    public CustomerCatalogBannersRelationalTests(CatalogHostFactory factory) => _factory = factory;

    [Fact]
    public async Task Banners_are_kept_in_order_through_the_picture_lock()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        var image = await BannerImageAsync(c, "kampanya");

        var summer = await CreateAsync(c, new { title = "  Yaz kampanyası ", text = "Çaylarda indirim", imageId = image, linkType = "category", linkValue = "Çay" });
        summer.Should().Match<CatalogBannerDto>(b => b.Title == "Yaz kampanyası" && b.Text == "Çaylarda indirim" && b.ImageId == image
            && b.LinkType == "category" && b.LinkValue == "Çay" && b.LinkName == "Çay" && b.IsActive && b.Live && b.SortOrder == 0);
        summer.Image!.ThumbUrl.Should().StartWith($"https://img.test/{c.Code.ToUpperInvariant()}/banner/", "a banner picture is stored in its own area");
        var coffee = await CreateAsync(c, new { title = "Kahve", linkType = "product", linkValue = "b" });
        coffee.Should().Match<CatalogBannerDto>(b => b.LinkValue == "B" && b.LinkName == "Kahve Türk" && b.Image == null && b.SortOrder == 1);
        var site = await CreateAsync(c, new { title = "Web sitemiz", linkType = "URL", linkValue = " https://ornek.com/kampanya " });
        site.Should().Match<CatalogBannerDto>(b => b.LinkType == "url" && b.LinkValue == "https://ornek.com/kampanya" && b.LinkName == null);
        (await CreateAsync(c, new { title = "Bağlantısız" })).LinkType.Should().Be("none");

        (await SendAsync(_factory, HttpMethod.Put, Banners + "/order", c.Mudur, new { ids = new[] { site.Id, summer.Id } })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ListAsync(c)).Select(b => b.Title).Should().Equal("Web sitemiz", "Yaz kampanyası", "Kahve", "Bağlantısız");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Banners + "/order", c.Mudur, new { ids = new[] { site.Id, site.Id } }),
            HttpStatusCode.BadRequest, "INVALID_BODY");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Banners + "/order", c.Mudur, new { ids = new[] { Guid.NewGuid() } }),
            HttpStatusCode.NotFound, "CATALOG_BANNER_NOT_FOUND");

        var later = DateTimeOffset.UtcNow.AddDays(3).ToUnixTimeMilliseconds();
        var updated = await OkAsync<CatalogBannerDto>(await SendAsync(_factory, HttpMethod.Put, $"{Banners}/{summer.Id}", c.Mudur,
            new { title = "Kış kampanyası", imageId = image, linkType = "none", isActive = true, startsAtMs = later }));
        updated.Should().Match<CatalogBannerDto>(b => b.Title == "Kış kampanyası" && b.Text == "" && b.LinkType == "none" && b.LinkValue == ""
            && b.StartsAtMs == later && !b.Live && b.SortOrder == 1, "a PUT replaces every field; a banner that has not started is not live");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, $"{Banners}/{Guid.NewGuid()}", c.Mudur, new { title = "Yok" }),
            HttpStatusCode.NotFound, "CATALOG_BANNER_NOT_FOUND");

        // Banner writes move the picture revision only: a layout edit open on the panel stays current.
        var settings = await ReadAsync(_factory, db => db.CatalogSettings.AsNoTracking().SingleAsync(s => s.TenantId == c.Id));
        settings.Revision.Should().Be(0);
        settings.ImageRevision.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task A_banner_needs_a_title_or_picture_a_followable_link_and_dates_in_order()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        async Task Refused(object body, string code) =>
            await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Banners, c.Mudur, body), HttpStatusCode.BadRequest, code);

        await Refused(new { title = "A", linkType = "url", linkValue = "http://ornek.com/a" }, "INVALID_BANNER_LINK");
        await Refused(new { title = "A", linkType = "url", linkValue = "https://10.0.0.1/a" }, "INVALID_BANNER_LINK");
        await Refused(new { title = "A", linkType = "category", linkValue = "Yok böyle kategori" }, "INVALID_BANNER_LINK");
        await Refused(new { title = "A", linkType = "product", linkValue = "ZZZ" }, "INVALID_BANNER_LINK");
        await Refused(new { title = "A", linkType = "page", linkValue = "x" }, "INVALID_BANNER_LINK");
        await Refused(new { title = "A", startsAtMs = 2000L, endsAtMs = 2000L }, "INVALID_BANNER_DATES");
        await Refused(new { title = " ", text = "Yalnız metin" }, "INVALID_BODY");
        await Refused(new { title = new string('a', 121) }, "INVALID_BODY");
        await Refused(new { title = "A", text = new string('a', 301) }, "INVALID_BODY");

        // The picture must be one of this company's banner pictures, not a product's.
        var productImage = (await OkAsync<CatalogImageCreatedResponse>(await SendAsync(_factory, HttpMethod.Post, Images, c.Patron,
            new { stockCode = "A", sourceHash = "urun", source = "panel" }))).Image.Id;
        await Refused(new { title = "A", imageId = productImage }, "INVALID_BANNER_IMAGE");
        await Refused(new { title = "A", imageId = Guid.NewGuid() }, "INVALID_BANNER_IMAGE");

        for (var i = 0; i < CatalogBanners.MaxBanners; i++) await CreateAsync(c, new { title = $"Banner {i}" });
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Banners, c.Mudur, new { title = "Fazla" }), HttpStatusCode.Conflict, "CATALOG_BANNER_LIMIT");
    }

    [Fact]
    public async Task Banner_pictures_are_not_product_pictures()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        // Past a product's limit of eight: banner pictures have a limit of their own.
        for (var i = 0; i < 9; i++) await BannerImageAsync(c, "b" + i, upload: i == 0);

        var manifest = await OkAsync<CatalogImageManifestResponse>(await SendAsync(_factory, HttpMethod.Get, Images + "/manifest", c.Mudur));
        manifest.Items.Should().BeEmpty("the manifest lists products only");
        manifest.UsedBytes.Should().Be(2 * Png.Length, "the quota counts banner pictures too");
        (await OkAsync<CatalogProductsResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/products", c.Mudur)))
            .Items.Should().OnlyContain(p => p.ImageCount == 0 && p.ThumbUrl == null);

        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Images, c.Patron, new { stockCode = "~baska", sourceHash = "x", source = "panel" }),
            HttpStatusCode.BadRequest, "INVALID_BODY");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Images + "/links", c.Patron,
            new { items = new[] { new { stockCode = "~banner", links = new[] { new { url = "https://cdn.example.com/a.jpg", sourceHash = "l" } } } } }),
            HttpStatusCode.BadRequest, "INVALID_BODY");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Images + "/order?stockCode=~banner", c.Patron, new { ids = Array.Empty<Guid>() }),
            HttpStatusCode.BadRequest, "INVALID_BODY");
    }

    [Fact]
    public async Task A_banner_picture_left_behind_by_an_unsaved_edit_goes_after_a_day()
    {
        var c = await CompanyAsync(_factory);
        var used = await BannerImageAsync(c, "kullanilan", upload: false);
        var abandoned = await BannerImageAsync(c, "terk");
        var fresh = await BannerImageAsync(c, "yeni", upload: false);
        await CreateAsync(c, new { title = "A", imageId = used });
        var twoDaysAgo = DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeMilliseconds();
        await SeedAsync(_factory, db =>
        {
            foreach (var image in db.CatalogImages.Where(i => i.Id == used || i.Id == abandoned)) image.CreatedAtMs = twoDaysAgo;
        });

        await BannerImageAsync(c, "sonraki", upload: false);

        var left = await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().Where(i => i.TenantId == c.Id).Select(i => i.Id).ToListAsync());
        left.Should().Contain([used, fresh]).And.NotContain(abandoned).And.HaveCount(3);
        (await FileStatusesAsync(abandoned)).Should().BeEmpty("nobody's delete to undo: its files are purged");
        (await ReadAsync(_factory, db => db.TenantStorage.AsNoTracking().SingleAsync(s => s.TenantId == c.Id))).UsedBytes.Should().Be(0);
    }

    [Fact]
    public async Task Deleting_or_replacing_a_banner_picture_deletes_it_unless_another_banner_shows_it()
    {
        var c = await CompanyAsync(_factory);
        var first = await BannerImageAsync(c, "ilk");
        var second = await BannerImageAsync(c, "ikinci");
        var banner = await CreateAsync(c, new { title = "A", imageId = first });
        (await FileStatusesAsync(first)).Should().Equal(StoredFileStatuses.Active, StoredFileStatuses.Active);

        (await SendAsync(_factory, HttpMethod.Put, $"{Banners}/{banner.Id}", c.Mudur, new { title = "A", imageId = second })).StatusCode.Should().Be(HttpStatusCode.OK);
        (await ImageExistsAsync(first)).Should().BeFalse("the replaced picture goes");
        (await FileStatusesAsync(first)).Should().Equal([StoredFileStatuses.Trashed, StoredFileStatuses.Trashed], "its files wait in the trash");

        var twin = await CreateAsync(c, new { title = "B", imageId = second });
        (await SendAsync(_factory, HttpMethod.Delete, $"{Banners}/{twin.Id}", c.Mudur)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ImageExistsAsync(second)).Should().BeTrue("the other banner still shows it");

        (await SendAsync(_factory, HttpMethod.Delete, $"{Banners}/{banner.Id}", c.Mudur)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ImageExistsAsync(second)).Should().BeFalse();
        (await FileStatusesAsync(second)).Should().Equal([StoredFileStatuses.Trashed, StoredFileStatuses.Trashed], "its sizes go with it");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Delete, $"{Banners}/{banner.Id}", c.Mudur), HttpStatusCode.NotFound, "CATALOG_BANNER_NOT_FOUND");
        (await ListAsync(c)).Should().BeEmpty();

        // A picture deleted through the images API leaves its banner without one.
        var third = await BannerImageAsync(c, "ucuncu");
        var kept = await CreateAsync(c, new { title = "C", imageId = third });
        (await SendAsync(_factory, HttpMethod.Delete, $"{Images}/{third}", c.Mudur)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await ListAsync(c)).Single().Should().Match<CatalogBannerDto>(b => b.Id == kept.Id && b.ImageId == null && b.Image == null);
    }

    [Fact]
    public async Task Another_company_neither_changes_nor_sees_the_banners()
    {
        var c = await CompanyAsync(_factory);
        var other = await CompanyAsync(_factory);
        var image = await BannerImageAsync(c, "gizli");
        var banner = await CreateAsync(c, new { title = "A", imageId = image });

        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, $"{Banners}/{banner.Id}", other.Mudur, new { title = "B" }), HttpStatusCode.NotFound, "CATALOG_BANNER_NOT_FOUND");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Delete, $"{Banners}/{banner.Id}", other.Mudur), HttpStatusCode.NotFound, "CATALOG_BANNER_NOT_FOUND");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Banners + "/order", other.Mudur, new { ids = new[] { banner.Id } }), HttpStatusCode.NotFound, "CATALOG_BANNER_NOT_FOUND");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Banners, other.Mudur, new { title = "B", imageId = image }), HttpStatusCode.BadRequest, "INVALID_BANNER_IMAGE");
        (await ListAsync(other)).Should().BeEmpty();
        (await ListAsync(c)).Single().Title.Should().Be("A");
    }

    [Fact]
    public async Task Banners_are_managed_like_the_rest_of_the_catalog()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, Banners, c.Patron), HttpStatusCode.Forbidden, "MODULE_NOT_ENABLED");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Banners, c.Patron, new { title = "A" }), HttpStatusCode.Forbidden, "MODULE_NOT_ENABLED");

        await SetModulesAsync(_factory, c, TenantModules.CustomerCatalog);
        foreach (var token in new[] { c.Ali, c.Muhasebe })
        {
            await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, Banners, token), HttpStatusCode.Forbidden, "CATALOG_MANAGE_REQUIRED");
            await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Banners, token, new { title = "A" }), HttpStatusCode.Forbidden, "CATALOG_MANAGE_REQUIRED");
            await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Delete, $"{Banners}/{Guid.NewGuid()}", token), HttpStatusCode.Forbidden, "CATALOG_MANAGE_REQUIRED");
        }
        (await SendAsync(_factory, HttpMethod.Get, Banners, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task The_customer_sees_the_live_banners_in_order_and_only_links_their_catalog_allows()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        await AccountAsync(_factory, c, "C2", "kisitli", Pass,
            visibility: new { mode = "all", rules = new[] { new { type = "product", key = "B", effect = "deny" } } });
        var now = DateTimeOffset.UtcNow;
        long At(TimeSpan offset) => now.Add(offset).ToUnixTimeMilliseconds();
        var image = await BannerImageAsync(c, "vitrin");

        await CreateAsync(c, new { title = "Bitti", endsAtMs = At(TimeSpan.FromMinutes(-1)) });
        await CreateAsync(c, new { title = "Başlamadı", startsAtMs = At(TimeSpan.FromHours(1)) });
        await CreateAsync(c, new { title = "Pasif", isActive = false });
        var product = await CreateAsync(c, new { title = "Kahve haftası", text = "Türk kahvesi", imageId = image, linkType = "product", linkValue = "B",
            startsAtMs = At(TimeSpan.FromHours(-1)), endsAtMs = At(TimeSpan.FromHours(1)) });
        var category = await CreateAsync(c, new { title = "Kahveler", linkType = "category", linkValue = "Kahve" });
        var site = await CreateAsync(c, new { title = "Web sitemiz", linkType = "url", linkValue = "https://ornek.com/kampanya" });
        (await SendAsync(_factory, HttpMethod.Put, Banners + "/order", c.Mudur, new { ids = new[] { site.Id } })).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var yilmaz = await SignedInAsync(c, "yilmaz");
        var response = await GetAsync(yilmaz, Api(c) + "/banners");
        response.Headers.CacheControl!.Private.Should().BeTrue();
        response.Headers.CacheControl.NoStore.Should().BeTrue();
        var items = (await OkAsync<CatalogCustomerBannersResponse>(response)).Items;
        items.Select(b => b.Title).Should().Equal(["Web sitemiz", "Kahve haftası", "Kahveler"], "ended, not yet started and inactive banners are not shown");
        items[0].Link.Should().BeEquivalentTo(new CatalogCustomerBannerLinkDto { Type = "url", Value = "https://ornek.com/kampanya" });
        items[0].Image.Should().BeNull();
        items[1].Should().Match<CatalogCustomerBannerDto>(b => b.Id == product.Id && b.Text == "Türk kahvesi");
        items[1].Image!.Thumb.Should().StartWith($"https://img.test/{c.Code.ToUpperInvariant()}/banner/").And.EndWith("-s.png");
        items[1].Image!.Full.Should().StartWith($"https://img.test/{c.Code.ToUpperInvariant()}/banner/").And.EndWith("-l.png");
        items[1].Link.Should().BeEquivalentTo(new CatalogCustomerBannerLinkDto { Type = "product", Value = "B", ProductKey = "B" });
        items[2].Link.Should().BeEquivalentTo(new CatalogCustomerBannerLinkDto { Type = "category", Value = "Kahve", CategoryId = CatalogViewService.CategoryId("Kahve") });

        // B is hidden from this customer, and with it the only product of Kahve: the banners stay, their links go.
        var restricted = (await OkAsync<CatalogCustomerBannersResponse>(await GetAsync(await SignedInAsync(c, "kisitli"), Api(c) + "/banners"))).Items;
        restricted.Select(b => (b.Title, b.Link?.Type)).Should().Equal(("Web sitemiz", "url"), ("Kahve haftası", null), ("Kahveler", null));

        // A product hidden for everyone drops the link for every customer.
        await SeedAsync(_factory, db => db.CatalogProductSettings.Add(new CatalogProductSetting { TenantId = c.Id, StockCode = "B", IsHidden = true }));
        await SendAsync(_factory, HttpMethod.Put, $"{Banners}/{category.Id}", c.Mudur, new { title = "Kahveler", linkType = "category", linkValue = "Kahve" });
        (await OkAsync<CatalogCustomerBannersResponse>(await GetAsync(yilmaz, Api(c) + "/banners"))).Items
            .Single(b => b.Id == product.Id).Link.Should().BeNull();

        (await GetAsync(Browser(_factory), Api(c) + "/banners")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Without_banners_the_customer_gets_an_empty_list()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "bos", Pass);
        (await OkAsync<CatalogCustomerBannersResponse>(await GetAsync(await SignedInAsync(c, "bos"), Api(c) + "/banners"))).Items.Should().BeEmpty();
    }

    // ---- helpers ---------------------------------------------------------------------------

    private async Task<CatalogBannerDto> CreateAsync(CatalogCompany c, object body) =>
        await OkAsync<CatalogBannerDto>(await SendAsync(_factory, HttpMethod.Post, Banners, c.Mudur, body), HttpStatusCode.Created);

    private async Task<CatalogBannerDto[]> ListAsync(CatalogCompany c) =>
        (await OkAsync<CatalogBannersResponse>(await SendAsync(_factory, HttpMethod.Get, Banners, c.Mudur))).Items;

    /// <summary>A banner picture registered as the panel does (<c>POST images</c> under "~banner"), with both sizes uploaded.</summary>
    private async Task<Guid> BannerImageAsync(CatalogCompany c, string sourceHash, bool upload = true)
    {
        var id = (await OkAsync<CatalogImageCreatedResponse>(await SendAsync(_factory, HttpMethod.Post, Images, c.Patron,
            new { stockCode = CatalogBanners.ImageStockCode, sourceHash, source = "panel" }))).Image.Id;
        if (!upload) return id;
        foreach (var variant in new[] { "l", "s" })
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"{Images}/{id}/{variant}") { Content = new ByteArrayContent(Png) };
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", c.Patron);
            (await _factory.CreateClient().SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
        return id;
    }

    private Task<bool> ImageExistsAsync(Guid id) => ReadAsync(_factory, db => db.CatalogImages.AnyAsync(i => i.Id == id));

    /// <summary>The statuses of a picture's stored sizes, by variant (l, s).</summary>
    private Task<List<string>> FileStatusesAsync(Guid imageId) => ReadAsync(_factory, db => db.StoredFiles.AsNoTracking()
        .Where(f => f.OwnerType == CatalogImages.StoredFileOwnerType && f.OwnerKey == imageId.ToString("D")).OrderBy(f => f.Variant).Select(f => f.Status).ToListAsync());

    private async Task<HttpClient> SignedInAsync(CatalogCompany company, string username)
    {
        var browser = Browser(_factory);
        var login = await LoginAsync(browser, company, username, Pass);
        login.StatusCode.Should().Be(HttpStatusCode.OK, await login.Content.ReadAsStringAsync());
        return browser;
    }
}
