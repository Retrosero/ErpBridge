using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_MUSTERI_KATALOGU §5.1–5.2 / S5: pictures are registered by the server (idempotent by source hash), their two
/// sizes uploaded as raw bytes checked by their first bytes, size, per-product limit and company quota, a JPEG's EXIF
/// dropped; links are https only and never downloaded; the bytes are served anonymously and cached for good, only
/// while the company's module is on.
/// </summary>
public sealed class CustomerCatalogImagesRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private const string Images = Base + "/images";

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4, 5, 6, 7, 8];
    private static readonly byte[] Webp = [.. "RIFF"u8.ToArray(), 0x20, 0, 0, 0, .. "WEBPVP8 "u8.ToArray(), 9, 9, 9, 9];

    private readonly SqliteCentralApiFactory _factory;

    public CustomerCatalogImagesRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_file_picture_is_registered_once_and_its_two_sizes_follow()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);

        var created = await CreateAsync(c, new { stockCode = "a", sourceHash = "h1", source = "phone" });
        created.Should().Match<CatalogImageDto>(i => i.Kind == "file" && i.Source == "phone" && i.SourceHash == "h1" && !i.HasSmall && !i.HasLarge
            && i.ThumbUrl == null && i.FullUrl == null && i.SortOrder == 0);
        (await CreateAsync(c, new { stockCode = "A", sourceHash = "h1", source = "panel" })).Id.Should().Be(created.Id, "the same original is the same picture");

        (await UploadAsync(c, created.Id, "l", Png, "image/png")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await UploadAsync(c, created.Id, "s", Webp, "image/webp; charset=binary")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await UploadAsync(c, created.Id, "s", Webp, "image/webp")).StatusCode.Should().Be(HttpStatusCode.NoContent, "the same bytes again change nothing");

        var manifest = await OkAsync<CatalogImageManifestResponse>(await SendAsync(_factory, HttpMethod.Get, Images + "/manifest", c.Mudur));
        manifest.UsedBytes.Should().Be(Png.Length + Webp.Length);
        manifest.LimitBytes.Should().Be(1L << 30);
        var item = manifest.Items.Should().ContainSingle().Subject;
        item.StockCode.Should().Be("A", "the card's own code is stored");
        var image = item.Images.Should().ContainSingle().Subject;
        image.Should().Match<CatalogImageDto>(i => i.Id == created.Id && i.HasSmall && i.HasLarge);
        image.ThumbUrl.Should().Be($"/api/v1/catalog/img/{created.Id}/s?h={Sha(Webp)[..8]}");
        image.FullUrl.Should().Be($"/api/v1/catalog/img/{created.Id}/l?h={Sha(Png)[..8]}");
        var row = await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().SingleAsync(i => i.Id == created.Id));
        row.Should().Match<CatalogImage>(i => i.SizeBytes == Png.Length + Webp.Length && i.Sha256Large == Sha(Png) && i.ContentType == "image/webp");

        // The catalog shows it at once: every picture change moves the revision the view keys on.
        var product = (await OkAsync<CatalogProductsResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/products?q=8690001", c.Mudur))).Items.Single();
        product.ImageCount.Should().Be(1);
        product.ThumbUrl.Should().Be(image.ThumbUrl);
        (await OkAsync<CatalogSettingsDto>(await SendAsync(_factory, HttpMethod.Get, Base + "/settings", c.Mudur))).Revision.Should().Be(3);
    }

    [Fact]
    public async Task Bytes_must_be_a_picture_of_the_allowed_size_within_the_company_quota()
    {
        var c = await CompanyAsync(_factory);
        var id = (await CreateAsync(c, new { stockCode = "A", sourceHash = "h1", source = "panel" })).Id;

        await ShouldFailAsync(await UploadAsync(c, id, "l", Png, "image/jpeg"), HttpStatusCode.UnsupportedMediaType, "INVALID_IMAGE");
        await ShouldFailAsync(await UploadAsync(c, id, "l", "<svg/>"u8.ToArray(), "image/svg+xml"), HttpStatusCode.UnsupportedMediaType, "INVALID_IMAGE");
        await ShouldFailAsync(await UploadAsync(c, id, "l", [], "image/png"), HttpStatusCode.UnsupportedMediaType, "INVALID_IMAGE");
        await ShouldFailAsync(await UploadAsync(c, id, "l", Padded(Png, 1024 * 1024 + 1), "image/png"), HttpStatusCode.RequestEntityTooLarge, "IMAGE_TOO_LARGE");
        await ShouldFailAsync(await UploadAsync(c, id, "s", Padded(Png, 200 * 1024 + 1), "image/png"), HttpStatusCode.RequestEntityTooLarge, "IMAGE_TOO_LARGE");
        (await UploadAsync(c, id, "l", Padded(Png, 1024 * 1024), "image/png")).StatusCode.Should().Be(HttpStatusCode.NoContent, "1 MB itself is allowed");
        (await UploadAsync(c, id, "s", Padded(Png, 200 * 1024), "image/png")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await UploadAsync(c, id, "x", Png, "image/png")).StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Without Content-Length (chunked) the limit holds while reading.
        var chunked = new HttpRequestMessage(HttpMethod.Put, $"{Images}/{id}/s") { Content = new StreamContent(new MemoryStream(Padded(Png, 200 * 1024 + 1))) };
        chunked.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        chunked.Headers.TransferEncodingChunked = true;
        chunked.Headers.Authorization = new AuthenticationHeaderValue("Bearer", c.Patron);
        await ShouldFailAsync(await _factory.CreateClient().SendAsync(chunked), HttpStatusCode.RequestEntityTooLarge, "IMAGE_TOO_LARGE");

        // The company's pictures fill the quota but for a few bytes: a bigger picture is refused, the same size replaces.
        var quota = 1L << 30;
        await SeedAsync(_factory, db => db.CatalogImages.Add(new CatalogImage
        {
            TenantId = c.Id, StockCode = "B", SourceHash = "big", HasLarge = true, CreatedAtMs = 1,
            SizeBytes = (int)(quota - (1024 * 1024 + 200 * 1024) - 10),
        }));
        var other = (await CreateAsync(c, new { stockCode = "A", sourceHash = "h2", source = "panel" })).Id;
        await ShouldFailAsync(await UploadAsync(c, other, "s", Padded(Png, 100), "image/png"), HttpStatusCode.RequestEntityTooLarge, "CATALOG_IMAGE_QUOTA_EXCEEDED");
        (await UploadAsync(c, id, "s", Padded(Png, 200 * 1024 + 5), "image/png")).StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        (await UploadAsync(c, id, "l", [.. Padded(Png, 1024 * 1024 - 1), 7], "image/png")).StatusCode.Should().Be(HttpStatusCode.NoContent, "a replaced size frees its own bytes");
        (await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().SingleAsync(i => i.Id == id))).SizeBytes.Should().Be(1024 * 1024 + 200 * 1024);
    }

    [Fact]
    public async Task A_jpeg_loses_its_exif_and_keeps_the_rest()
    {
        var c = await CompanyAsync(_factory);
        var id = (await CreateAsync(c, new { stockCode = "A", sourceHash = "foto", source = "phone" })).Id;
        byte[] app0 = [0xFF, 0xE0, 0x00, 0x07, (byte)'J', (byte)'F', (byte)'I', (byte)'F', 0x00];
        byte[] exif = [0xFF, 0xE1, 0x00, 0x0C, (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0, 0, (byte)'G', (byte)'P', (byte)'S', 1];
        byte[] scan = [0xFF, 0xDA, 0x00, 0x02, 0x11, 0xE1, 0x22, 0xFF, 0xD9];
        byte[] jpeg = [0xFF, 0xD8, .. app0, .. exif, .. scan];

        (await UploadAsync(c, id, "l", jpeg, "image/jpeg")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var served = await _factory.CreateClient().GetAsync($"/api/v1/catalog/img/{id}/l");
        served.StatusCode.Should().Be(HttpStatusCode.OK);
        served.Content.Headers.ContentType!.MediaType.Should().Be("image/jpeg");
        (await served.Content.ReadAsByteArrayAsync()).Should().Equal([0xFF, 0xD8, .. app0, .. scan]);
        (await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().SingleAsync(i => i.Id == id))).SizeBytes.Should().Be(2 + app0.Length + scan.Length);
    }

    [Fact]
    public async Task A_product_holds_at_most_eight_pictures()
    {
        var c = await CompanyAsync(_factory);
        for (var i = 0; i < 8; i++)
            (await CreateAsync(c, new { stockCode = "A", sourceHash = $"h{i}", source = "phone" })).SortOrder.Should().Be(i);

        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Images, c.Patron, new { stockCode = "A", sourceHash = "h8", source = "phone" }),
            HttpStatusCode.Conflict, "CATALOG_IMAGE_LIMIT");
        (await SendAsync(_factory, HttpMethod.Post, Images, c.Patron, new { stockCode = "A", sourceHash = "h3", source = "phone" })).StatusCode
            .Should().Be(HttpStatusCode.OK, "a picture already there is found, not added");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Images, c.Patron, new { stockCode = "A", sourceHash = "h9", source = "camera" }),
            HttpStatusCode.BadRequest, "INVALID_BODY");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Images, c.Patron, new { stockCode = "", sourceHash = "h9", source = "phone" }),
            HttpStatusCode.BadRequest, "INVALID_BODY");
    }

    [Theory]
    [InlineData("http://cdn.example.com/a.jpg")]
    [InlineData("https://cdn.example.com:8443/a.jpg")]
    [InlineData("https://10.0.0.5/a.jpg")]
    [InlineData("https://[::1]/a.jpg")]
    [InlineData("https://localhost/a.jpg")]
    [InlineData("https://nas.local/a.jpg")]
    [InlineData("ftp://cdn.example.com/a.jpg")]
    [InlineData("cdn.example.com/a.jpg")]
    public async Task Only_https_named_hosts_on_port_443_are_links(string url)
    {
        var c = await CompanyAsync(_factory);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Images + "/links", c.Patron,
            new { items = new[] { new { stockCode = "A", links = new[] { new { url, sourceHash = "l1" } } } } }), HttpStatusCode.BadRequest, "INVALID_IMAGE_URL");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Images, c.Patron, new { stockCode = "A", sourceHash = "l1", source = "panel", url }),
            HttpStatusCode.BadRequest, "INVALID_IMAGE_URL");
    }

    [Fact]
    public void A_link_is_at_most_2048_characters_and_explicit_port_443_is_fine()
    {
        var prefix = "https://cdn.example.com/";
        CatalogImages.ValidLink(prefix + new string('a', 2048 - prefix.Length)).Should().NotBeNull();
        CatalogImages.ValidLink(prefix + new string('a', 2049 - prefix.Length)).Should().BeNull();
        CatalogImages.ValidLink(" https://cdn.example.com:443/a.jpg ").Should().Be("https://cdn.example.com:443/a.jpg");
        CatalogImages.ValidLink("https://foo.localhost/a.jpg").Should().BeNull();
    }

    [Fact]
    public async Task The_phone_links_replace_its_own_earlier_links_only()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        var panelLink = await CreateAsync(c, new { stockCode = "A", sourceHash = "panel", source = "panel", url = "https://panel.example.com/a.jpg" });
        panelLink.Should().Match<CatalogImageDto>(i => i.Kind == "link" && i.ThumbUrl == "https://panel.example.com/a.jpg" && i.FullUrl == i.ThumbUrl);
        var file = await CreateAsync(c, new { stockCode = "A", sourceHash = "file", source = "phone" });

        async Task<int> PutLinksAsync(params (string Code, (string Url, string Hash)[] Links)[] items) =>
            (await OkAsync<CatalogImageLinksResponse>(await SendAsync(_factory, HttpMethod.Put, Images + "/links", c.Patron, new
            {
                items = items.Select(i => new { stockCode = i.Code, links = i.Links.Select(l => new { url = l.Url, sourceHash = l.Hash }) }),
            }))).Updated;
        async Task<CatalogImageDto[]> PicturesOf(string code) =>
            (await OkAsync<CatalogImageManifestResponse>(await SendAsync(_factory, HttpMethod.Get, Images + "/manifest", c.Patron)))
                .Items.SingleOrDefault(i => i.StockCode == code)?.Images ?? [];
        async Task<long> RevisionAsync() => await ReadAsync(_factory, db => db.CatalogSettings.AsNoTracking().Where(s => s.TenantId == c.Id).Select(s => s.Revision).SingleAsync());

        (await PutLinksAsync(
            ("a", [("https://cdn.example.com/1.jpg", "l1"), ("https://cdn.example.com/2.jpg", "l2"), ("https://cdn.example.com/2b.jpg", "l2")]),
            ("B", [("https://cdn.example.com/b.jpg", "b1")]))).Should().Be(2);
        (await PicturesOf("A")).Select(i => (i.SourceHash, i.Kind, i.Source, i.Url)).Should().Equal(
            ("panel", "link", "panel", (string?)"https://panel.example.com/a.jpg"),
            ("file", "file", "phone", (string?)null),
            ("l1", "link", "phone", (string?)"https://cdn.example.com/1.jpg"),
            ("l2", "link", "phone", (string?)"https://cdn.example.com/2.jpg"));
        var revision = await RevisionAsync();

        // Sent again unchanged: nothing moves, not even the revision.
        (await PutLinksAsync(("A", [("https://cdn.example.com/1.jpg", "l1"), ("https://cdn.example.com/2.jpg", "l2")]))).Should().Be(0);
        (await RevisionAsync()).Should().Be(revision);

        // A new set: the phone's old links go, the panel's link and the file stay.
        (await PutLinksAsync(("A", [("https://cdn.example.com/3.jpg", "l3")]), ("B", []))).Should().Be(2);
        (await PicturesOf("A")).Select(i => i.SourceHash).Should().Equal("panel", "file", "l3");
        (await PicturesOf("B")).Should().BeEmpty();
        (await PicturesOf("A")).Should().Contain(i => i.Id == file.Id);

        // Links past the product's limit are not kept.
        (await PutLinksAsync(("A", [.. Enumerable.Range(0, 9).Select(i => ($"https://cdn.example.com/x{i}.jpg", $"x{i}"))]))).Should().Be(1);
        (await PicturesOf("A")).Should().HaveCount(8);

        var tooMany = Enumerable.Range(0, 501).Select(i => new { stockCode = $"S{i}", links = Array.Empty<object>() });
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Images + "/links", c.Patron, new { items = tooMany }), HttpStatusCode.BadRequest, "INVALID_BODY");
    }

    [Fact]
    public async Task Pictures_are_reordered_and_deleted_with_their_bytes()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        var first = await CreateAsync(c, new { stockCode = "A", sourceHash = "1", source = "panel" });
        var second = await CreateAsync(c, new { stockCode = "A", sourceHash = "2", source = "panel" });
        var third = await CreateAsync(c, new { stockCode = "A", sourceHash = "3", source = "panel" });
        (await UploadAsync(c, second.Id, "l", Png, "image/png")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await SendAsync(_factory, HttpMethod.Put, Images + "/order?stockCode=a", c.Patron, new { ids = new[] { third.Id, first.Id } }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        var manifest = await OkAsync<CatalogImageManifestResponse>(await SendAsync(_factory, HttpMethod.Get, Images + "/manifest", c.Patron));
        manifest.Items.Single().Images.Select(i => (i.Id, i.SortOrder)).Should().Equal((third.Id, 0), (first.Id, 1), (second.Id, 2));
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Images + "/order?stockCode=A", c.Patron, new { ids = new[] { Guid.NewGuid() } }),
            HttpStatusCode.NotFound, "CATALOG_IMAGE_NOT_FOUND");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Images + "/order?stockCode=A", c.Patron, new { ids = new[] { first.Id, first.Id } }),
            HttpStatusCode.BadRequest, "INVALID_BODY");

        (await SendAsync(_factory, HttpMethod.Delete, $"{Images}/{second.Id}", c.Patron)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Delete, $"{Images}/{second.Id}", c.Patron), HttpStatusCode.NotFound, "CATALOG_IMAGE_NOT_FOUND");
        (await ReadAsync(_factory, db => db.CatalogImageBlobs.AnyAsync(b => b.ImageId == second.Id))).Should().BeFalse();
        (await _factory.CreateClient().GetAsync($"/api/v1/catalog/img/{second.Id}/l")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Another_company_neither_changes_nor_sees_the_pictures()
    {
        var c = await CompanyAsync(_factory);
        var other = await CompanyAsync(_factory);
        var id = (await CreateAsync(c, new { stockCode = "A", sourceHash = "h1", source = "panel" })).Id;

        await ShouldFailAsync(await UploadAsync(other, id, "l", Png, "image/png"), HttpStatusCode.NotFound, "CATALOG_IMAGE_NOT_FOUND");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Delete, $"{Images}/{id}", other.Patron), HttpStatusCode.NotFound, "CATALOG_IMAGE_NOT_FOUND");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Images + "/order?stockCode=A", other.Patron, new { ids = new[] { id } }),
            HttpStatusCode.NotFound, "CATALOG_IMAGE_NOT_FOUND");
        (await OkAsync<CatalogImageManifestResponse>(await SendAsync(_factory, HttpMethod.Get, Images + "/manifest", other.Patron))).Items.Should().BeEmpty();
        (await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().SingleAsync(i => i.Id == id))).HasLarge.Should().BeFalse();
    }

    [Fact]
    public async Task Pictures_are_managed_like_the_rest_of_the_catalog()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, Images + "/manifest", c.Patron), HttpStatusCode.Forbidden, "MODULE_NOT_ENABLED");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Images, c.Patron, new { stockCode = "A", sourceHash = "h", source = "phone" }),
            HttpStatusCode.Forbidden, "MODULE_NOT_ENABLED");

        await SetModulesAsync(_factory, c, TenantModules.CustomerCatalog);
        foreach (var token in new[] { c.Ali, c.Muhasebe })
        {
            await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, Images + "/manifest", token), HttpStatusCode.Forbidden, "CATALOG_MANAGE_REQUIRED");
            await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Images, token, new { stockCode = "A", sourceHash = "h", source = "phone" }),
                HttpStatusCode.Forbidden, "CATALOG_MANAGE_REQUIRED");
            await ShouldFailAsync(await UploadAsync(c, Guid.NewGuid(), "l", Png, "image/png", token), HttpStatusCode.Forbidden, "CATALOG_MANAGE_REQUIRED");
        }
    }

    [Fact]
    public async Task The_anonymous_picture_is_cached_for_good_and_only_while_the_module_is_on()
    {
        var c = await CompanyAsync(_factory);
        var both = (await CreateAsync(c, new { stockCode = "A", sourceHash = "both", source = "panel" })).Id;
        var largeOnly = (await CreateAsync(c, new { stockCode = "A", sourceHash = "large", source = "panel" })).Id;
        var smallOnly = (await CreateAsync(c, new { stockCode = "A", sourceHash = "small", source = "panel" })).Id;
        var link = (await CreateAsync(c, new { stockCode = "A", sourceHash = "link", source = "panel", url = "https://cdn.example.com/a.jpg" })).Id;
        (await UploadAsync(c, both, "l", Png, "image/png")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await UploadAsync(c, both, "s", Webp, "image/webp")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await UploadAsync(c, largeOnly, "l", Png, "image/png")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await UploadAsync(c, smallOnly, "s", Webp, "image/webp")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var anonymous = _factory.CreateClient();

        var small = await anonymous.GetAsync($"/api/v1/catalog/img/{both}/s?h={Sha(Webp)[..8]}");
        small.StatusCode.Should().Be(HttpStatusCode.OK);
        (await small.Content.ReadAsByteArrayAsync()).Should().Equal(Webp);
        small.Content.Headers.ContentType!.MediaType.Should().Be("image/webp");
        small.Headers.CacheControl!.ToString().Should().Be("public, max-age=31536000, immutable");
        small.Headers.ETag!.Tag.Should().Be($"\"{Sha(Webp)}\"");
        small.Headers.GetValues("X-Content-Type-Options").Should().Equal("nosniff");
        small.Headers.GetValues("Cross-Origin-Resource-Policy").Should().Equal("same-site");

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/catalog/img/{both}/s");
        request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue($"\"{Sha(Webp)}\""));
        var notModified = await anonymous.SendAsync(request);
        notModified.StatusCode.Should().Be(HttpStatusCode.NotModified);
        (await notModified.Content.ReadAsByteArrayAsync()).Should().BeEmpty();
        notModified.Headers.ETag!.Tag.Should().Be($"\"{Sha(Webp)}\"");

        (await (await anonymous.GetAsync($"/api/v1/catalog/img/{largeOnly}/s")).Content.ReadAsByteArrayAsync()).Should().Equal(Png, "no small size: the large one");
        (await anonymous.GetAsync($"/api/v1/catalog/img/{smallOnly}/l")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await anonymous.GetAsync($"/api/v1/catalog/img/{smallOnly}/s")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await anonymous.GetAsync($"/api/v1/catalog/img/{link}/s")).StatusCode.Should().Be(HttpStatusCode.NotFound, "the server never serves a link");
        var unknown = await anonymous.GetAsync($"/api/v1/catalog/img/{Guid.NewGuid()}/l");
        unknown.StatusCode.Should().Be(HttpStatusCode.NotFound);
        unknown.Headers.CacheControl.Should().BeNull();

        await SetModulesAsync(_factory, c);
        (await anonymous.GetAsync($"/api/v1/catalog/img/{both}/l")).StatusCode.Should().Be(HttpStatusCode.NotFound, "the module is off");
    }

    private async Task<CatalogImageDto> CreateAsync(CatalogCompany c, object body)
    {
        var response = await SendAsync(_factory, HttpMethod.Post, Images, c.Patron, body);
        return (await OkAsync<CatalogImageCreatedResponse>(response)).Image;
    }

    private async Task<HttpResponseMessage> UploadAsync(CatalogCompany c, Guid id, string variant, byte[] data, string contentType, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{Images}/{id}/{variant}") { Content = new ByteArrayContent(data) };
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token ?? c.Patron);
        return await _factory.CreateClient().SendAsync(request);
    }

    private static byte[] Padded(byte[] header, int length)
    {
        var data = new byte[length];
        header.CopyTo(data, 0);
        return data;
    }

    private static string Sha(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
