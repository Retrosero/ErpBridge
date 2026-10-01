using System.Net;
using System.Text.RegularExpressions;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using static ErpBridge.CentralApi.Tests.Endpoints.CatalogCustomerTestSupport;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_MUSTERI_KATALOGU §6–7 / W0: on the catalog host the shell answers every company path with the security
/// headers and a title, the files go out under a version taken from their contents and are cached for good, and
/// nothing else of the API is reachable; on any other host none of it exists.
/// </summary>
public sealed partial class CatalogWebTests : IClassFixture<CatalogHostFactory>
{
    private readonly CatalogHostFactory _factory;

    public CatalogWebTests(CatalogHostFactory factory) => _factory = factory;

    [Fact]
    public async Task The_shell_of_an_open_catalog_has_the_company_title_the_version_and_the_security_headers()
    {
        var c = await OpenCatalogAsync(_factory);
        await SeedAsync(_factory, db => db.Tenants.Single(t => t.Id == c.Id).Name = "Ak & <Gıda>");
        var browser = Browser(_factory);

        foreach (var path in new[] { "/" + c.Code, $"/{c.Code}/urun?kod=A", $"/{c.Code}/hesap/ekstre" })
        {
            var shell = await browser.GetAsync(path);
            shell.StatusCode.Should().Be(HttpStatusCode.OK, path);
            shell.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
            shell.Headers.CacheControl!.NoCache.Should().BeTrue();
            ShouldCarrySecurityHeaders(shell);
            var html = await shell.Content.ReadAsStringAsync();
            html.Should().Contain("<title>Ak &amp; &lt;Gıda&gt; · Müşteri Kataloğu</title>", "the company name is HTML-encoded");
            html.Should().MatchRegex("/assets/[0-9a-f]{10}/js/main.js").And.NotContain("%V%").And.NotContain("%TITLE%");
        }

        var unknown = await browser.GetAsync("/ZZZZ9999/sepet");
        unknown.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ShouldCarrySecurityHeaders(unknown);
        (await unknown.Content.ReadAsStringAsync()).Should().Contain("<title>Müşteri Kataloğu</title>", "the same page, without a company");
    }

    [Fact]
    public async Task Files_go_out_under_their_content_version_and_an_old_version_is_gone()
    {
        var c = await OpenCatalogAsync(_factory);
        var browser = Browser(_factory);
        var version = VersionIn(await (await browser.GetAsync("/" + c.Code)).Content.ReadAsStringAsync());
        version.Should().Be(CatalogWeb.VersionOf(CatalogHostFactory.WebRoot));

        var script = await browser.GetAsync($"/assets/{version}/js/main.js");
        script.StatusCode.Should().Be(HttpStatusCode.OK);
        script.Content.Headers.ContentType!.MediaType.Should().Be("text/javascript");
        script.Headers.CacheControl!.ToString().Should().Be("public, max-age=31536000, immutable");
        script.Headers.GetValues("X-Content-Type-Options").Should().Equal("nosniff");
        (await script.Content.ReadAsStringAsync()).Should().Contain("export const ready");

        foreach (var path in new[] { "/assets/0000000000/js/main.js", $"/assets/{version}/js/yok.js", "/assets" })
        {
            var gone = await browser.GetAsync(path);
            gone.StatusCode.Should().Be(HttpStatusCode.NotFound, path);
            gone.Headers.CacheControl!.NoStore.Should().BeTrue(path);
        }

        var robots = await browser.GetAsync("/robots.txt");
        robots.StatusCode.Should().Be(HttpStatusCode.OK);
        (await robots.Content.ReadAsStringAsync()).Should().Contain("Disallow: /");
        var icon = await browser.GetAsync("/favicon.svg");
        icon.StatusCode.Should().Be(HttpStatusCode.OK);
        icon.Content.Headers.ContentType!.MediaType.Should().Be("image/svg+xml");
    }

    [Fact]
    public async Task Only_the_customer_api_and_health_are_reachable_on_the_catalog_host()
    {
        var c = await OpenCatalogAsync(_factory);
        var browser = Browser(_factory);

        var admin = await browser.GetAsync("/api/v1/admin/tenants", c.AdminToken);
        admin.StatusCode.Should().Be(HttpStatusCode.NotFound, "the operator's API is not on the catalog's name");
        ShouldCarrySecurityHeaders(admin);
        (await browser.GetAsync(Base + "/settings", c.Mudur)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await browser.GetAsync("/api/v1/android/account/me", c.Ali)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        var root = await browser.GetAsync("/");
        root.StatusCode.Should().Be(HttpStatusCode.OK, "the bare name tells the visitor to use the link the company sent");
        (await root.Content.ReadAsStringAsync()).Should().Contain("/js/main.js");
        ShouldCarrySecurityHeaders(root);
        (await browser.PostJsonAsync("/" + c.Code, new { })).StatusCode.Should().Be(HttpStatusCode.NotFound);

        var info = await browser.GetAsync(Api(c) + "/info");
        info.StatusCode.Should().Be(HttpStatusCode.OK);
        ShouldCarrySecurityHeaders(info);
        (await browser.GetAsync("/health/live")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await browser.GetAsync("/api/v1/catalog/img/" + Guid.NewGuid() + "/s")).StatusCode.Should().Be(HttpStatusCode.NotFound, "a picture that does not exist");
    }

    [Fact]
    public async Task On_any_other_host_there_is_no_catalog_page_and_the_api_is_as_before()
    {
        var c = await OpenCatalogAsync(_factory);
        var api = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://lisans.test/") });

        var shell = await api.GetAsync("/" + c.Code);
        shell.StatusCode.Should().Be(HttpStatusCode.NotFound);
        shell.Headers.Contains("Content-Security-Policy").Should().BeFalse();
        (await api.GetAsync("/assets/" + CatalogWeb.VersionOf(CatalogHostFactory.WebRoot) + "/js/main.js")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await api.GetAsync(Base + "/settings", c.Mudur)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Without_a_catalog_host_nothing_is_served()
    {
        using var plain = new SqliteCentralApiFactory();
        var client = plain.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri(CatalogHostFactory.Origin + "/") });
        (await client.GetAsync("/ABCD2345")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.GetAsync("/robots.txt")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static void ShouldCarrySecurityHeaders(HttpResponseMessage response)
    {
        response.Headers.GetValues("Content-Security-Policy").Should().Equal(CatalogWeb.ContentSecurityPolicy);
        response.Headers.GetValues("X-Robots-Tag").Should().Equal("noindex");
        response.Headers.GetValues("Referrer-Policy").Should().Equal("same-origin");
        response.Headers.GetValues("Strict-Transport-Security").Should().Equal("max-age=31536000");
        response.Headers.GetValues("X-Content-Type-Options").Should().Equal("nosniff");
    }

    private static string VersionIn(string html) => AssetVersion().Match(html).Groups[1].Value;

    [GeneratedRegex("/assets/([0-9a-f]{10})/")]
    private static partial Regex AssetVersion();
}
