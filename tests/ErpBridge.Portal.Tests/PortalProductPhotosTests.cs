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
}
