using System.Net;
using System.Net.Http.Headers;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Endpoints;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>
/// GOAL_DEPOLAMA_R2 S6: a product photo from the phone or the panel is made into a 1280 px and a 400 px WebP in the public
/// bucket (area <c>product</c>, owner the product), counts against the company quota, needs <c>action.products.photo</c>
/// but no module (an ERP company too: only the picture changes), keeps its order, goes to the trash when deleted, and
/// shows in the web catalog for a product without catalog pictures of its own.
/// </summary>
public sealed class ProductImageRelationalTests : IClassFixture<StorageCentralApiFactory>
{
    private const string Images = "/api/v1/storage/products/images";

    private readonly StorageCentralApiFactory _factory;

    public ProductImageRelationalTests(StorageCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_photo_becomes_two_webp_sizes_in_the_public_bucket_and_the_same_photo_is_the_same_picture()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        await SeedCatalogAsync(_factory, c.Id);
        var photo = Photo(4000, 3000, SKColors.Teal);

        var response = await UploadAsync(c.Ali, "b", photo);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        var dto = await response.ReadAsJsonAsync<ProductImageDto>();
        dto.Should().Match<ProductImageDto>(d => d.StockCode == "B" && d.SortOrder == 0 && d.Width == 1280 && d.Height == 960 && d.CreatedByName == "ali bey");
        var folder = $"https://img.test/{c.Code.ToUpperInvariant()}/product/";
        dto.ThumbUrl.Should().StartWith(folder).And.EndWith("-s.webp");
        dto.FullUrl.Should().StartWith(folder).And.EndWith("-l.webp");

        var files = await ReadAsync(_factory, db => db.StoredFiles.AsNoTracking().Where(f => f.TenantId == c.Id).OrderBy(f => f.Variant).ToListAsync());
        files.Should().HaveCount(2).And.OnlyContain(f => f.Area == "product" && f.Bucket == "public" && f.OwnerType == "product" && f.OwnerKey == "B"
            && f.ContentType == "image/webp" && f.CreatedByUserId == c.AliId);
        using (var small = SKBitmap.Decode(_factory.Store.Bytes("public", files.Single(f => f.Variant == "s").ObjectKey)))
            (small.Width, small.Height).Should().Be((400, 300));
        dto.SizeBytes.Should().Be(files.Sum(f => f.SizeBytes));
        (await ReadAsync(_factory, db => db.TenantStorage.AsNoTracking().SingleAsync(s => s.TenantId == c.Id))).UsedBytes.Should().Be(dto.SizeBytes);

        var again = await (await UploadAsync(c.Ali, "B", photo)).ReadAsJsonAsync<ProductImageDto>();
        again.Id.Should().Be(dto.Id, "a retried upload is the same picture");
        (await ReadAsync(_factory, db => db.StoredFiles.CountAsync(f => f.TenantId == c.Id))).Should().Be(2);

        var list = await (await SendAsync(_factory, HttpMethod.Get, Images + "?stockCode=b", c.Muhasebe)).ReadAsJsonAsync<ProductImagesResponse>();
        list.StockCode.Should().Be("B");
        list.Items.Should().ContainSingle().Which.FullUrl.Should().Be(dto.FullUrl, "every user of the company reads the photos");
    }

    [Fact]
    public async Task Photos_need_their_permission_are_checked_and_count_against_the_quota()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        await ShouldFailAsync(await UploadAsync(c.Muhasebe, "A", Photo(50, 50, SKColors.Red)), HttpStatusCode.Forbidden, "PRODUCT_PHOTO_FORBIDDEN");
        await ShouldFailAsync(await UploadAsync(c.Ali, " ", Photo(50, 50, SKColors.Red)), HttpStatusCode.BadRequest, "INVALID_STOCK_CODE");
        await ShouldFailAsync(await UploadAsync(c.Ali, "A", [0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3]), HttpStatusCode.UnsupportedMediaType, "INVALID_IMAGE");
        await ShouldFailAsync(await UploadAsync(c.Ali, "A", "<svg/>"u8.ToArray(), "image/svg+xml"), HttpStatusCode.UnsupportedMediaType, "INVALID_IMAGE");
        await ShouldFailAsync(await UploadAsync(c.Ali, "A", [0xFF, 0xD8, .. new byte[10 * 1024 * 1024]]), HttpStatusCode.RequestEntityTooLarge, "PRODUCT_IMAGE_TOO_LARGE");

        for (var i = 0; i < 8; i++) (await UploadAsync(c.Ali, "A", Photo(40 + i, 40, SKColors.Navy))).StatusCode.Should().Be(HttpStatusCode.OK);
        await ShouldFailAsync(await UploadAsync(c.Ali, "A", Photo(30, 30, SKColors.Navy)), HttpStatusCode.Conflict, "PRODUCT_IMAGE_LIMIT");

        var used = (await ReadAsync(_factory, db => db.TenantStorage.AsNoTracking().SingleAsync(s => s.TenantId == c.Id))).UsedBytes;
        await SeedAsync(_factory, db => db.TenantStorage.Single(s => s.TenantId == c.Id).QuotaBytes = used + 10);
        var refused = await UploadAsync(c.Ali, "B", Photo(300, 300, SKColors.Navy));
        refused.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        (await refused.ReadAsJsonAsync<ErpBridge.CentralApi.Storage.StorageErrorResponse>()).Should().Match<ErpBridge.CentralApi.Storage.StorageErrorResponse>(e =>
            e.ErrorCode == "STORAGE_QUOTA_EXCEEDED" && e.UsedBytes == used && e.QuotaBytes == used + 10);
        (await ReadAsync(_factory, db => db.StoredFiles.CountAsync(f => f.TenantId == c.Id))).Should().Be(16, "a refused photo leaves nothing behind");
        (await ReadAsync(_factory, db => db.TenantStorage.AsNoTracking().SingleAsync(s => s.TenantId == c.Id))).UsedBytes.Should().Be(used);
    }

    [Fact]
    public async Task Photos_keep_their_order_and_a_deleted_one_goes_to_the_trash()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        var first = await (await UploadAsync(c.Ali, "A", Photo(60, 60, SKColors.Red))).ReadAsJsonAsync<ProductImageDto>();
        var second = await (await UploadAsync(c.Mudur, "A", Photo(60, 60, SKColors.Blue))).ReadAsJsonAsync<ProductImageDto>();
        second.SortOrder.Should().Be(1);

        (await SendAsync(_factory, HttpMethod.Put, Images + "/order?stockCode=A", c.Ali, new { ids = new[] { second.Id } })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var manifest = await (await SendAsync(_factory, HttpMethod.Get, Images + "/manifest", c.Ali)).ReadAsJsonAsync<ProductImageManifestResponse>();
        manifest.Items.Single().Items.Select(i => i.Id).Should().Equal(second.Id, first.Id);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Images + "/order?stockCode=A", c.Ali, new { ids = new[] { Guid.NewGuid() } }),
            HttpStatusCode.NotFound, "PRODUCT_IMAGE_NOT_FOUND");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Delete, $"{Images}/{first.Id}", c.Muhasebe), HttpStatusCode.Forbidden, "PRODUCT_PHOTO_FORBIDDEN");

        (await SendAsync(_factory, HttpMethod.Delete, $"{Images}/{first.Id}", c.Ali)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Delete, $"{Images}/{first.Id}", c.Ali), HttpStatusCode.NotFound, "PRODUCT_IMAGE_NOT_FOUND");
        (await ReadAsync(_factory, db => db.StoredFiles.AsNoTracking().Where(f => f.TenantId == c.Id && f.Status == StoredFileStatuses.Trashed).CountAsync()))
            .Should().Be(2, "both sizes wait in the trash");
        (await (await SendAsync(_factory, HttpMethod.Get, Images + "?stockCode=A", c.Ali)).ReadAsJsonAsync<ProductImagesResponse>())
            .Items.Select(i => i.Id).Should().Equal(second.Id);

        var other = await CompanyAsync(_factory, withModule: false);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Delete, $"{Images}/{second.Id}", other.Patron), HttpStatusCode.NotFound, "PRODUCT_IMAGE_NOT_FOUND");
        (await (await SendAsync(_factory, HttpMethod.Get, Images + "/manifest", other.Patron)).ReadAsJsonAsync<ProductImageManifestResponse>()).Items.Should().BeEmpty();
    }

    [Fact]
    public async Task The_catalog_shows_a_products_photos_when_it_has_no_catalog_picture()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        var photo = await (await UploadAsync(c.Ali, "B", Photo(80, 80, SKColors.Green))).ReadAsJsonAsync<ProductImageDto>();
        (await UploadAsync(c.Ali, "A", Photo(80, 80, SKColors.Yellow))).StatusCode.Should().Be(HttpStatusCode.OK);
        var picture = (await OkAsync<CatalogImageCreatedResponse>(await SendAsync(_factory, HttpMethod.Post, Base + "/images", c.Patron,
            new { stockCode = "A", sourceHash = "katalog", source = "panel", url = "https://cdn.example.com/a.jpg" }))).Image;

        using var scope = _factory.Services.CreateScope();
        var view = await scope.ServiceProvider.GetRequiredService<CatalogViewService>()
            .LoadAsync(scope.ServiceProvider.GetRequiredService<CentralApiDbContext>(), c.Id, forCustomer: true, CancellationToken.None);
        var coffee = view.Products["B"];
        coffee.Pictures.Should().BeEmpty("the catalog has none of its own");
        coffee.ShownPictures.Should().ContainSingle().Which.Should().Be(new CatalogPicture(photo.Id, photo.ThumbUrl, photo.FullUrl));
        coffee.ShownThumbUrl.Should().Be(photo.ThumbUrl);
        var tea = view.Products["A"];
        tea.ShownPictures.Select(p => p.Id).Should().Equal([picture.Id], "the catalog's own picture comes first");
        tea.ProductPhotos.Should().ContainSingle();
    }

    private Task<HttpResponseMessage> UploadAsync(string token, string stockCode, byte[] data, string contentType = "image/jpeg")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{Images}?stockCode={Uri.EscapeDataString(stockCode)}") { Content = new ByteArrayContent(data) };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _factory.CreateClient().SendAsync(request);
    }

    private static byte[] Photo(int width, int height, SKColor color)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap)) canvas.Clear(color);
        using var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 90);
        return data.ToArray();
    }
}
