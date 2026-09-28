using System.Net;
using System.Text;
using Bunit;
using ErpBridge.Admin.Api;
using ErpBridge.Admin.Auth;
using ErpBridge.Admin.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Admin.Tests.Pages;

/// <summary>
/// Lisanslar sayfası ürünleri ayrı tutar: her ürünün kendi sekmesi, rengi ve öneki vardır; form açık sekmenin
/// ürününe kilitlidir, bir ürünün lisansı öbürünün listesinde görünmez.
/// </summary>
public sealed class LicensesPageTests : BunitContext
{
    private const string TenantId = "77777777-7777-7777-7777-777777777777";
    private readonly Handler _handler = new();

    public LicensesPageTests()
    {
        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(new HttpClient(_handler) { BaseAddress = new Uri("https://central.example") }, tokenStore));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void The_default_tab_lists_only_ErpBridge_licenses_and_counts_each_product_separately()
    {
        var cut = Render<Licenses>();
        cut.WaitForState(() => cut.FindAll("article.license-card").Count > 0);

        cut.FindAll("article.license-card").Should().OnlyContain(c => c.GetAttribute("data-product") == "erpbridge");
        cut.Find("#license-tab-erpbridge").GetAttribute("aria-selected").Should().Be("true");
        cut.Find("#license-tab-go .license-product-tab__count").TextContent.Should().Be("1/2");
        cut.Find("#license-context").TextContent.Should().Contain("LIC-");
        cut.Find("#license-create").TextContent.Should().Contain("ErpBridge lisansı oluştur");
    }

    [Fact]
    public void The_Go_tab_lists_only_Go_licenses_with_their_prefix_and_computer()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("licenses?urun=go");

        var cut = Render<Licenses>();
        cut.WaitForState(() => cut.FindAll("article.license-card").Count == 2);

        cut.FindAll("article.license-card").Should().OnlyContain(c => c.GetAttribute("data-product") == "go");
        cut.Find("#license-context").TextContent.Should().Contain("GO-").And.Contain("tek bilgisayara");
        cut.Find(".license-card__key .license-prefix").TextContent.Should().Be("GO-");
        cut.Markup.Should().Contain("DEPO-PC").And.NotContain("Telefon girişi oluştur");
    }

    [Fact]
    public void A_license_created_on_the_Go_tab_is_requested_as_a_Go_license()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("licenses?urun=go");
        var cut = Render<Licenses>();
        cut.WaitForState(() => cut.FindAll("article.license-card").Count == 2);

        cut.Find(".license-issue select").Change(TenantId);
        cut.Find("#license-create").Click();

        cut.WaitForState(() => cut.FindAll("#license-issued-key").Count == 1);
        _handler.CreatedBody.Should().Contain("\"product\":\"go\"");
        cut.Find(".license-notice--success strong").TextContent.Should().Be("Go (pazaryeri) lisansı oluşturuldu");
    }

    [Fact]
    public void Switching_tabs_changes_the_query_string()
    {
        var cut = Render<Licenses>();
        cut.WaitForState(() => cut.FindAll("article.license-card").Count > 0);

        cut.Find("#license-tab-go").Click();

        Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("?urun=go");
        cut.WaitForState(() => cut.FindAll("article.license-card").All(c => c.GetAttribute("data-product") == "go"));
    }

    private sealed class Handler : HttpMessageHandler
    {
        public string? CreatedBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string json;
            var status = HttpStatusCode.OK;
            if (request.Method == HttpMethod.Post && request.RequestUri!.AbsolutePath == "/api/v1/admin/licenses")
            {
                CreatedBody = await request.Content!.ReadAsStringAsync(cancellationToken);
                json = $$"""{"id":"{{Guid.NewGuid()}}","tenantId":"{{TenantId}}","licenseKey":"GO-ffffffffffffffffffffffffffffffff","issuedAtUtc":"2026-09-28T10:00:00Z","isActive":true,"product":"go"}""";
                status = HttpStatusCode.Created;
            }
            else
            {
                json = request.RequestUri!.AbsolutePath switch
                {
                    "/api/v1/admin/tenants" => $$"""[{"id":"{{TenantId}}","name":"Gürbüz Oyuncak","isActive":true,"maxDeviceCount":1}]""",
                    "/api/v1/admin/licenses" => $$$"""
                        [
                         {"id":"{{{Guid.NewGuid()}}}","tenantId":"{{{TenantId}}}","licenseKey":"LIC-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","issuedAtUtc":"2026-09-01T10:00:00Z","isActive":true,"product":"erpbridge"},
                         {"id":"{{{Guid.NewGuid()}}}","tenantId":"{{{TenantId}}}","licenseKey":"GO-bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb","issuedAtUtc":"2026-09-28T10:00:00Z","isActive":true,"product":"go",
                          "goInstallation":{"machineName":"DEPO-PC","appVersion":"1.0.0","activatedAtUtc":"2026-09-28T10:05:00Z","lastSeenAtUtc":"2026-09-28T12:00:00Z"}},
                         {"id":"{{{Guid.NewGuid()}}}","tenantId":"{{{TenantId}}}","licenseKey":"GO-cccccccccccccccccccccccccccccccc","issuedAtUtc":"2026-09-20T10:00:00Z","isActive":false,"product":"go"}
                        ]
                        """,
                    _ => "[]",
                };
            }

            return new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        }
    }
}
