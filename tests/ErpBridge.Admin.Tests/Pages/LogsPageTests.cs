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

public sealed class LogsPageTests : BunitContext
{
    private static readonly Guid EventId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    private static readonly Guid GroupId = Guid.Parse("99999999-8888-7777-6666-555555555555");

    [Fact]
    public void Lists_events_with_source_severity_company_and_summary()
    {
        var handler = new LogHandler();
        var cut = RenderPage(handler);

        cut.WaitForState(() => cut.FindAll("button.log-row").Count == 1);
        var row = cut.Find("button.log-row");
        row.TextContent.Should().Contain("Telefon").And.Contain("Hata").And.Contain("Bakkal Ali AŞ").And.Contain("Ayşe Plasiyer")
            .And.Contain("HTTP 500").And.Contain("500 POST /api/v1/ingest/jobs (812 ms)");
        handler.ListQueries.Single().Should().Contain("take=50").And.Contain("from=");
        cut.Find("[data-source='android'] .log-chip__count").TextContent.Should().Be("7");
        cut.Find(".log-summary").TextContent.Should().Contain("9").And.Contain("Hata: 4");
    }

    [Fact]
    public void Filters_go_to_the_url_and_the_list_reloads_with_them()
    {
        var handler = new LogHandler();
        var cut = RenderPage(handler);
        cut.WaitForState(() => handler.ListQueries.Count == 1);

        cut.Find("select[aria-label='En düşük seviye']").Change("ERROR");
        cut.Find("input[aria-label='Mesajda ara']").Change("timeout");
        cut.Find("form.log-filters").Submit();

        var nav = Services.GetRequiredService<NavigationManager>();
        nav.Uri.Should().EndWith("logs?minSeverity=ERROR&q=timeout");
        cut.WaitForState(() => handler.ListQueries.Count == 2);
        handler.ListQueries[1].Should().Contain("minSeverity=ERROR").And.Contain("q=timeout");

        cut.Find("[data-source='windows_agent']").Click();
        nav.Uri.Should().EndWith("logs?source=windows_agent&minSeverity=ERROR&q=timeout");
        cut.WaitForState(() => handler.ListQueries.Count == 3);
        handler.FacetQueries.Last().Should().NotContain("source=", "chip counts ignore the source filter");
    }

    [Fact]
    public void Opening_a_row_shows_the_detail_and_its_links_filter_the_list()
    {
        var handler = new LogHandler();
        var cut = RenderPage(handler);
        cut.WaitForState(() => cut.FindAll("button.log-row").Count == 1);

        cut.Find("button.log-row").Click();

        var nav = Services.GetRequiredService<NavigationManager>();
        nav.Uri.Should().EndWith($"logs?log={EventId}");
        cut.WaitForState(() => cut.FindAll(".log-drawer__body").Count == 1);
        var drawer = cut.Find(".log-drawer");
        drawer.TextContent.Should().Contain("ingest failed").And.Contain("at Sales.Save()").And.Contain("sales screen")
            .And.Contain("wifi").And.Contain("12 olay").And.Contain("corr-42");

        cut.FindAll(".log-link").Single(b => b.TextContent.Contains("corr-42")).Click();
        nav.Uri.Should().Contain("correlationId=corr-42").And.NotContain("log=");
        cut.WaitForState(() => cut.FindAll(".log-drawer").Count == 0);
    }

    [Fact]
    public void Load_more_asks_for_the_next_page_with_the_cursor()
    {
        var handler = new LogHandler { NextBefore = "1726560000000_abc" };
        var cut = RenderPage(handler);
        cut.WaitForState(() => cut.FindAll(".log-more button").Count == 1);

        cut.Find(".log-more button").Click();

        cut.WaitForState(() => handler.ListQueries.Count == 2);
        handler.ListQueries[1].Should().Contain("before=1726560000000_abc");
    }

    [Fact]
    public void Invalid_advanced_id_is_reported_instead_of_navigating()
    {
        var handler = new LogHandler();
        var cut = RenderPage(handler);
        cut.WaitForState(() => handler.ListQueries.Count == 1);

        cut.FindAll("button").Single(b => b.TextContent == "Gelişmiş").Click();
        cut.Find("input[aria-label='Kullanıcı kimliği']").Change("not-a-guid");
        cut.Find("form.log-filters").Submit();

        cut.Find(".log-validation").TextContent.Should().Contain("GUID");
        Services.GetRequiredService<NavigationManager>().Uri.Should().NotContain("userId");
    }

    [Fact]
    public void Link_with_filters_opens_prefilled()
    {
        var handler = new LogHandler();
        var cut = RenderPage(handler, $"logs?range=7d&source=portal,central_api&tenantId={TenantId}&kind=CRASH");

        cut.WaitForState(() => handler.ListQueries.Count == 1);
        handler.ListQueries[0].Should().Contain("source=portal%2Ccentral_api").And.Contain($"tenantId={TenantId}").And.Contain("kind=CRASH");
        cut.Find("select[aria-label='Zaman aralığı']").GetAttribute("value").Should().Be("7d");
        cut.Find("[data-source='portal']").GetAttribute("aria-pressed").Should().Be("true");
        cut.Find("input[aria-label='Tür']").GetAttribute("value").Should().Be("CRASH", "an advanced filter opens the advanced panel");
    }

    private IRenderedComponent<Logs> RenderPage(LogHandler handler, string startUri = "logs")
    {
        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://central.example") }, tokenStore));
        JSInterop.Mode = JSRuntimeMode.Loose;
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo(startUri);
        return Render<Logs>();
    }

    private sealed class LogHandler : HttpMessageHandler
    {
        public List<string> ListQueries { get; } = [];
        public List<string> FacetQueries { get; } = [];
        public string? NextBefore { get; init; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            string json;
            if (path == "/api/v1/admin/logs")
            {
                lock (ListQueries) ListQueries.Add(Uri.UnescapeDataString(request.RequestUri.Query) + "|" + request.RequestUri.Query);
                var next = NextBefore is null || ListQueries.Count > 1 ? "null" : $"\"{NextBefore}\"";
                json = $$$"""{"items":[{"id":"{{{EventId}}}","eventId":"e1","source":"android","tenantId":"{{{TenantId}}}","tenantName":"Bakkal Ali AŞ","occurredAtUtc":"2026-09-17T09:15:00Z","severity":"ERROR","kind":"HTTP_ERROR","operation":"api_request","message":"ingest failed","userName":"Ayşe Plasiyer","deviceModel":"Pixel 8","httpMethod":"POST","httpRoute":"/api/v1/ingest/jobs","httpStatus":500,"durationMs":812,"repeatCount":1,"fingerprintId":"{{{GroupId}}}"}],"nextBefore":{{{next}}}}""";
            }
            else if (path == "/api/v1/admin/logs/facets")
            {
                lock (FacetQueries) FacetQueries.Add(request.RequestUri.Query);
                json = """{"total":9,"sources":[{"value":"android","count":7},{"value":"central_api","count":2}],"severities":[{"value":"ERROR","count":4},{"value":"INFO","count":5}],"kinds":[],"appVersions":[]}""";
            }
            else if (path == $"/api/v1/admin/logs/{EventId}")
            {
                json = $$$"""{"id":"{{{EventId}}}","eventId":"e1","source":"android","tenantId":"{{{TenantId}}}","tenantName":"Bakkal Ali AŞ","occurredAtUtc":"2026-09-17T09:15:00Z","receivedAtUtc":"2026-09-17T09:15:02Z","severity":"ERROR","kind":"HTTP_ERROR","operation":"api_request","message":"ingest failed","exceptionType":"HttpException","stackTrace":"at Sales.Save()","correlationId":"corr-42","httpMethod":"POST","httpRoute":"/api/v1/ingest/jobs","httpStatus":500,"repeatCount":1,"fingerprintId":"{{{GroupId}}}","propertiesJson":"{\"network\":\"wifi\"}","breadcrumbsJson":"[{\"timestampUtc\":\"2026-09-17T09:14:58Z\",\"category\":\"nav\",\"message\":\"sales screen\"}]","group":{"id":"{{{GroupId}}}","status":"OPEN","totalCount":12,"firstSeenAtUtc":"2026-09-10T08:00:00Z","lastSeenAtUtc":"2026-09-17T09:15:00Z"}}""";
            }
            else
            {
                json = $$"""[{"id":"{{TenantId}}","name":"Bakkal Ali AŞ"}]""";
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }
}
