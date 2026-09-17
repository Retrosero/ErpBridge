using System.Net;
using System.Text;
using Bunit;
using ErpBridge.Admin.Api;
using ErpBridge.Admin.Auth;
using ErpBridge.Admin.Pages;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Admin.Tests.Pages;

public sealed class DashboardLogCardTests : BunitContext
{
    [Fact]
    public void Error_card_counts_the_last_24_hours_of_the_log_centre_and_links_to_it()
    {
        var handler = new Handler();
        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://central.example") }, tokenStore));
        JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = Render<Dashboard>();

        cut.WaitForState(() => cut.FindAll("a[href='logs?minSeverity=ERROR']").Count == 1);
        cut.Find("a[href='logs?minSeverity=ERROR'] strong").TextContent.Should().Be("17");
        handler.FacetQuery.Should().Contain("minSeverity=ERROR").And.Contain("from=");
    }

    private sealed class Handler : HttpMessageHandler
    {
        public string? FacetQuery { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var json = "[]";
            if (request.RequestUri!.AbsolutePath == "/api/v1/admin/logs/facets")
            {
                FacetQuery = request.RequestUri.Query;
                json = """{"total":17,"sources":[],"severities":[],"kinds":[],"appVersions":[]}""";
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }
}
