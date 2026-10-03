using Bunit;
using ErpBridge.Portal.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>GOAL_DEPOLAMA_R2 S6: a product's own photos on the Stok page's product detail.</summary>
public sealed class PortalProductPhotosTests : PortalPageTestContext
{
    private const string List = "/api/v1/storage/products/images?stockCode=A";

    private static readonly Guid First = Guid.NewGuid();
    private static readonly Guid Second = Guid.NewGuid();

    private static object Photo(Guid id, int order) => new
    {
        id, stockCode = "A", sortOrder = order, thumbUrl = $"https://img.test/ABC/product/{id:N}-s.webp", fullUrl = $"https://img.test/ABC/product/{id:N}-l.webp",
        width = 1280, height = 960, sizeBytes = 1000, createdAtMs = 1L, createdByName = "Ali Saha",
    };

    private static object Photos(params object[] items) => new { stockCode = "A", items };

    private static object Xml(Guid id, int position) => new
    {
        id, stockCode = "A", position, sourceUrl = $"https://tedarikci.example.com/{position}.jpg",
        thumbUrl = $"https://img.test/ABC/xml/{id:N}-s.webp", fullUrl = $"https://img.test/ABC/xml/{id:N}-l.webp",
        width = 1280, height = 960, sizeBytes = 1000, createdAtMs = 1L, updatedAtMs = 1L,
    };

    [Fact]
    public void Shows_the_photos_and_sends_a_picked_file_as_it_is()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(List, Photos(Photo(First, 0)));
        api.AnswerPrefix(HttpMethod.Post, "/api/v1/storage/products/images?stockCode=A", Photo(Second, 1));

        var cut = Render<ProductPhotos>(p => p.Add(x => x.StockCode, "A").Add(x => x.CanEdit, true));

        cut.WaitForAssertion(() => cut.FindAll(".product-photo").Should().ContainSingle());
        cut.Find($"[data-photo='{First}'] img").GetAttribute("src").Should().EndWith("-s.webp");
        cut.Find($"[data-photo='{First}'] a").GetAttribute("href").Should().EndWith("-l.webp");

        api.Answer(List, Photos(Photo(First, 0), Photo(Second, 1)));
        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary([0xFF, 0xD8, 0xFF, 0xE0, 1, 2], "raf.jpg", contentType: "image/jpeg"));

        cut.WaitForAssertion(() => cut.FindAll(".product-photo").Count.Should().Be(2));
        var upload = api.Requests.Single(r => r.Method == HttpMethod.Post);
        upload.PathAndQuery.Should().Be("/api/v1/storage/products/images?stockCode=A");
        upload.ContentType.Should().Be("image/jpeg", "the server shrinks it; the panel sends the file as picked");
    }

    [Fact]
    public void Makes_a_cover_and_deletes_and_without_the_permission_only_shows()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(List, Photos(Photo(First, 0), Photo(Second, 1)));
        api.Answer("/api/v1/storage/products/images/order?stockCode=A", new { });
        api.Answer($"/api/v1/storage/products/images/{Second:D}", new { });

        var cut = Render<ProductPhotos>(p => p.Add(x => x.StockCode, "A").Add(x => x.CanEdit, true));
        cut.WaitForAssertion(() => cut.FindAll(".product-photo").Count.Should().Be(2));
        cut.FindAll($"[data-photo='{First}'] .photo-cover").Should().BeEmpty("the first is the cover already");

        cut.Find($"[data-photo='{Second}'] .photo-cover button, [data-photo='{Second}'] button.photo-cover").Click();
        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.Method == HttpMethod.Put && r.Body!.Contains(Second.ToString())));

        cut.Find($"[data-photo='{Second}'] .photo-delete button, [data-photo='{Second}'] button.photo-delete").Click();
        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.Method == HttpMethod.Delete && r.PathAndQuery.EndsWith(Second.ToString("D"))));

        var readOnly = Render<ProductPhotos>(p => p.Add(x => x.StockCode, "A").Add(x => x.CanEdit, false));
        readOnly.WaitForAssertion(() => readOnly.FindAll(".product-photo").Count.Should().Be(2));
        readOnly.FindAll("button").Should().BeEmpty();
        readOnly.FindComponents<InputFile>().Should().BeEmpty();
    }

    [Fact]
    public void The_xml_copies_follow_the_companys_photos_read_only()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var xmlFirst = Guid.NewGuid();
        var xmlSecond = Guid.NewGuid();
        api.Answer(List, new { stockCode = "A", items = new[] { Photo(First, 0) }, xmlItems = new[] { Xml(xmlFirst, 0), Xml(xmlSecond, 1) } });

        var cut = Render<ProductPhotos>(p => p.Add(x => x.StockCode, "A").Add(x => x.CanEdit, true));

        cut.WaitForAssertion(() => cut.FindAll(".product-photo").Count.Should().Be(3));
        cut.FindAll(".product-photo").Select(f => f.GetAttribute("data-photo") ?? f.GetAttribute("data-xml-photo"))
            .Should().Equal([First.ToString(), xmlFirst.ToString(), xmlSecond.ToString()], "the company's photos first, then the feed's in its order");
        cut.Find($"[data-xml-photo='{xmlFirst}'] img").GetAttribute("src").Should().EndWith("-s.webp");
        cut.Find($"[data-xml-photo='{xmlFirst}'] a").GetAttribute("href").Should().EndWith("-l.webp");
        cut.Find($"[data-xml-photo='{xmlFirst}'] figcaption").TextContent.Should().Contain("XML'den");
        cut.FindAll("[data-xml-photo] button").Should().BeEmpty("the XML sync owns them: no cover, no delete");
        cut.Find("#product-photos-xml-hint");
        cut.FindAll("#product-photos-empty").Should().BeEmpty();
    }

    [Fact]
    public void Only_xml_copies_are_not_an_empty_list_and_nothing_at_all_is()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var xml = Guid.NewGuid();
        api.Answer(List, new { stockCode = "A", items = Array.Empty<object>(), xmlItems = new[] { Xml(xml, 0) } });

        var cut = Render<ProductPhotos>(p => p.Add(x => x.StockCode, "A").Add(x => x.CanEdit, false));
        cut.WaitForAssertion(() => cut.Find($"[data-xml-photo='{xml}']"));
        cut.FindAll("#product-photos-empty").Should().BeEmpty();

        api.Answer("/api/v1/storage/products/images?stockCode=B", new { stockCode = "B", items = Array.Empty<object>(), xmlItems = Array.Empty<object>() });
        var empty = Render<ProductPhotos>(p => p.Add(x => x.StockCode, "B").Add(x => x.CanEdit, false));
        empty.WaitForAssertion(() => empty.Find("#product-photos-empty").TextContent.Should().Be("Fotoğraf yok."));
        empty.FindAll(".product-photo").Should().BeEmpty();
    }
}
