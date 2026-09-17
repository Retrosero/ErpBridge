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

/// <summary>Goal ERP yazım Y5b: a job's retry times, agent results and the ERP context an agent would get.</summary>
public sealed class JobsPageTests : BunitContext
{
    private const string JobId = "66666666-6666-6666-6666-666666666666";

    [Fact]
    public void Opening_a_failed_job_shows_when_it_is_tried_again_the_results_and_the_erp_context()
    {
        var handler = new Handler();
        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://central.example") }, tokenStore));
        JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = Render<Jobs>();
        cut.WaitForState(() => cut.FindAll(".admin-record__summary").Count == 1);

        cut.Find(".admin-record__summary").Click();

        cut.WaitForState(() => cut.FindAll("#job-erp-context").Count == 1);
        cut.Find("#job-next-attempt").TextContent.Should().NotBe("—");
        cut.Find("#job-retryable").TextContent.Should().Contain("Evet");
        cut.Find("#job-error-code").TextContent.Should().Be("ERP_UNAVAILABLE");
        cut.Find("#job-acks").TextContent.Should().Contain("Yeniden denenecek").And.Contain("Mikro'ya ulaşılamadı.");
        cut.Find("#job-erp-context").TextContent.Should().Contain("\"salesDocumentKind\": \"invoice\"").And.Contain("\"cashCode\": \"001\"");
    }

    [Fact]
    public void A_failed_job_offers_retry_even_though_the_server_spells_the_status_capitalized()
    {
        var handler = new Handler();
        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://central.example") }, tokenStore));
        JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = Render<Jobs>();
        cut.WaitForState(() => cut.FindAll(".admin-record__summary").Count == 1);
        cut.Find(".admin-record__summary").Click();

        cut.WaitForState(() => cut.FindAll("button.btn-warning").Count == 1);
    }

    private sealed class Handler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var next = DateTimeOffset.UtcNow.AddMinutes(5).ToString("O");
            var json = request.RequestUri!.AbsolutePath switch
            {
                "/api/v1/admin/jobs" => $$"""[{"id":"{{JobId}}","tenantId":"77777777-7777-7777-7777-777777777777","externalId":"MOB-TH-1","documentType":"collection","status":"Failed","retryCount":2,"lastError":"Mikro'ya ulaşılamadı.","enqueuedAtUtc":"2026-09-17T10:00:00Z","nextAttemptAtUtc":"{{next}}"}]""",
                $"/api/v1/admin/jobs/{JobId}" => $$"""
                    {"id":"{{JobId}}","tenantId":"77777777-7777-7777-7777-777777777777","externalId":"MOB-TH-1","documentType":"collection","status":"Failed","retryCount":2,
                     "enqueuedAtUtc":"2026-09-17T10:00:00Z","nextAttemptAtUtc":"{{next}}","payloadJson":"{}","retryable":true,"lastErrorCode":"ERP_UNAVAILABLE",
                     "acks":[{"status":"retry","errorCode":"ERP_UNAVAILABLE","errorMessage":"Mikro'ya ulaşılamadı.","ackedAtUtc":"2026-09-17T10:05:00Z"}],
                     "erpContext":{"salesDocumentKind":"invoice","cashCode":"001"} }
                    """,
                _ => "[]",
            };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }
}
