using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Storage;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using SkiaSharp;
using static ErpBridge.CentralApi.Tests.Endpoints.CatalogCustomerTestSupport;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// A company whose only pictures are the XML sync's copies (GOAL_DEPOLAMA_R2 S7; no catalog picture, no product photo):
/// its customers see them everywhere the web catalog shows a product — the product list, the product detail, the cart's
/// quote and an order's lines — by their CDN addresses, and the panel's catalog management tells which picture stands in.
/// Seed as in <see cref="CustomerCatalogBrowseRelationalTests"/>: A, B and C priced in list 1 (100, 50, 30); A and C in stock.
/// </summary>
public sealed class CustomerCatalogXmlPicturesRelationalTests : IClassFixture<CatalogHostFactory>
{
    private const string Pass = "musteri123";

    private readonly CatalogHostFactory _factory;

    public CustomerCatalogXmlPicturesRelationalTests(CatalogHostFactory factory) => _factory = factory;

    [Fact]
    public async Task A_company_with_only_xml_pictures_shows_them_to_its_customers_and_in_the_panel()
    {
        var c = await OpenCatalogAsync(_factory);
        // C in stock too, so an order can carry a product without any picture.
        await SeedAsync(_factory, db => Record(db, c.Id, "inventory", "C|1", new { stockCode = "C", warehouseNo = 1, quantity = 5 }));
        await SetModulesAsync(_factory, c, TenantModules.CustomerCatalog, TenantModules.XmlImport);
        var host = $"https://cdn-{Guid.NewGuid():N}.example.com";
        await SeedAsync(_factory, db => db.TenantXmlFeedSettings.Add(new TenantXmlFeedSettings
        {
            TenantId = c.Id,
            Url = host + "/feed.xml",
            RecordPath = "Urunler/Urun",
            MappingJson = """{"CODE":["StokKodu"],"IMAGE":["Resimler/Resim"]}""",
            DownloadImages = true,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        }));
        string[] a = [Serve(host + "/a1.jpg", SKColors.Red), Serve(host + "/a2.jpg", SKColors.Blue)];
        string[] b = [Serve(host + "/b1.jpg", SKColors.Green)];
        _factory.Fetcher.Serve(host + "/feed.xml", Encoding.UTF8.GetBytes(
            "<Urunler>" + Product("A", a) + Product("B", b) + "</Urunler>"));

        // The customer opens the catalog before the sync: the cached view must not hide what the sync adds.
        await AccountAsync(_factory, c, "C1", "xmlmusteri", Pass);
        var browser = Browser(_factory);
        (await LoginAsync(browser, c, "xmlmusteri", Pass)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(browser, Api(c) + "/products"))).Items.Should().OnlyContain(p => p.Thumb == null);

        using (var scope = _factory.Services.CreateScope())
            (await scope.ServiceProvider.GetRequiredService<XmlImageSync>().RunAsync(c.Id, CancellationToken.None)).Status.Should().Be(XmlImageSyncStatuses.Ok);
        var photo = await UploadPhotoAsync(c.Ali, "B");

        var folder = $"https://img.test/{c.Code.ToUpperInvariant()}/xml/";
        var products = await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(browser, Api(c) + "/products"));
        var tea = products.Items.Single(p => p.Code == "A");
        tea.Thumb.Should().StartWith(folder).And.EndWith("-s.webp");
        products.Items.Single(p => p.Code == "B").Thumb.Should().Be(photo.ThumbUrl, "the company's own photo comes before the feed's");
        products.Items.Single(p => p.Code == "C").Thumb.Should().BeNull();
        (await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(browser, Api(c) + "/products?hasImage=true"))).Items.Select(p => p.Code)
            .Should().BeEquivalentTo(["A", "B"]);

        var detail = await OkAsync<CatalogCustomerProductDetailDto>(await GetAsync(browser, Api(c) + "/products/detail?key=A"));
        detail.Images.Should().HaveCount(2).And.OnlyContain(i => i.Thumb.StartsWith(folder) && i.Thumb.EndsWith("-s.webp") && i.Full.StartsWith(folder) && i.Full.EndsWith("-l.webp"));
        detail.Images[0].Thumb.Should().Be(tea.Thumb, "the feed's order");

        var quote = await OkAsync<CatalogQuoteDto>(await SendAsync(browser, HttpMethod.Post, Api(c) + "/cart/quote", new { lines = new[] { new { key = "A", quantity = 1m } } }));
        quote.Lines.Single().Thumb.Should().Be(tea.Thumb, "a cart filled before the sync takes the picture from the quote");

        var created = await OkAsync<CatalogOrderResponse>(await SendAsync(browser, HttpMethod.Post, Api(c) + "/orders", new
        {
            requestId = Guid.NewGuid(),
            lines = new[] { new { key = "A", quantity = 1m }, new { key = "C", quantity = 1m } },
            expectedTotal = 130m,
        }), HttpStatusCode.Created);
        var order = await OkAsync<CatalogCustomerOrderDetailDto>(await GetAsync(browser, Api(c) + "/orders/detail?id=" + created.Order.Id));
        order.Lines.Select(l => (l.Code, l.Thumb)).Should().Equal(("A", tea.Thumb), ("C", null));

        // The pictures are https addresses on the CDN, which the catalog's CSP allows.
        var page = await browser.GetAsync("/" + c.Code);
        page.Headers.GetValues("Content-Security-Policy").Single().Should().Contain("img-src 'self' https:");

        // The panel: ImageCount and ThumbUrl stay the catalog's own (the phone counts them); the shown picture is told apart.
        var managed = (await OkAsync<CatalogProductsResponse>(await CustomerCatalogTestSupport.SendAsync(_factory, HttpMethod.Get, Base + "/products", c.Mudur))).Items;
        managed.Single(p => p.StockCode == "A").Should().Match<CatalogProductDto>(p =>
            p.ImageCount == 0 && p.ThumbUrl == null && p.ShownThumbUrl == tea.Thumb && p.ShownSource == CatalogPictureSources.Xml);
        managed.Single(p => p.StockCode == "B").Should().Match<CatalogProductDto>(p =>
            p.ImageCount == 0 && p.ShownThumbUrl == photo.ThumbUrl && p.ShownSource == CatalogPictureSources.Product);
        managed.Single(p => p.StockCode == "C").Should().Match<CatalogProductDto>(p => p.ShownThumbUrl == null && p.ShownSource == null);

        // A catalog picture of its own takes over, for the customer and in the panel.
        await CustomerCatalogTestSupport.SendAsync(_factory, HttpMethod.Put, Base + "/images/links", c.Mudur,
            new { items = new[] { new { stockCode = "A", links = new[] { new { url = "https://cdn.example.com/own.jpg", sourceHash = "own" } } } } });
        (await OkAsync<CatalogCustomerProductDetailDto>(await GetAsync(browser, Api(c) + "/products/detail?key=A"))).Images.Select(i => i.Thumb)
            .Should().Equal("https://cdn.example.com/own.jpg");
        (await OkAsync<CatalogCustomerOrderDetailDto>(await GetAsync(browser, Api(c) + "/orders/detail?id=" + created.Order.Id))).Lines[0].Thumb
            .Should().Be("https://cdn.example.com/own.jpg");
        (await OkAsync<CatalogProductsResponse>(await CustomerCatalogTestSupport.SendAsync(_factory, HttpMethod.Get, Base + "/products", c.Mudur))).Items
            .Single(p => p.StockCode == "A").Should().Match<CatalogProductDto>(p =>
                p.ImageCount == 1 && p.ShownThumbUrl == "https://cdn.example.com/own.jpg" && p.ShownSource == CatalogPictureSources.Catalog);
    }

    private string Serve(string url, SKColor color)
    {
        using var bitmap = new SKBitmap(60, 40);
        using (var canvas = new SKCanvas(bitmap)) canvas.Clear(color);
        using var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 90);
        _factory.Fetcher.Serve(url, data.ToArray());
        return url;
    }

    private static string Product(string code, string[] images) =>
        $"<Urun><StokKodu>{code}</StokKodu><Resimler>{string.Concat(images.Select(i => $"<Resim>{i}</Resim>"))}</Resimler></Urun>";

    private async Task<ProductImageDto> UploadPhotoAsync(string token, string stockCode)
    {
        using var bitmap = new SKBitmap(60, 40);
        using (var canvas = new SKCanvas(bitmap)) canvas.Clear(SKColors.Teal);
        using var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 90);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/storage/products/images?stockCode={stockCode}") { Content = new ByteArrayContent(data.ToArray()) };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await OkAsync<ProductImageDto>(await _factory.CreateClient().SendAsync(request));
    }
}
