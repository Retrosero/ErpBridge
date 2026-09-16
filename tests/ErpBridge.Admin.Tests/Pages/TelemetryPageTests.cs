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

public sealed class TelemetryPageTests : BunitContext
{
    [Fact]
    public void Copy_icon_copies_raw_error_and_stack_trace()
    {
        var eventId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(new HttpClient(new TelemetryHandler(eventId, tenantId)) { BaseAddress = new Uri("https://central.example") }, tokenStore));
        JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = Render<Telemetry>();
        cut.WaitForState(() => cut.FindAll(".admin-record__summary").Count == 1);
        cut.Find(".admin-record__summary").Click();

        cut.Find(".admin-copy-button").Click();

        cut.Find(".admin-copy-status").TextContent.Should().Contain("panoya kopyalandı");
        var clipboardCall = JSInterop.Invocations.Single(invocation => invocation.Identifier == "navigator.clipboard.writeText");
        var copiedText = clipboardCall.Arguments[0] as string;
        copiedText.Should().Be($"Ham hata:{Environment.NewLine}FirebaseException: Permission denied{Environment.NewLine}{Environment.NewLine}İz kaydı:{Environment.NewLine}at Example.Sync()");
    }

    [Fact]
    public void Http_error_shows_status_method_and_route()
    {
        var events = $$"""[{"id":"{{Guid.NewGuid()}}","tenantId":"{{Guid.NewGuid()}}","occurredAtUtc":"2026-09-16T18:47:00Z","kind":"HTTP_ERROR","severity":"ERROR","operation":"api_request","exceptionType":"HttpException","message":"HTTP request failed after 742ms","httpMethod":"POST","httpRoute":"/api/v1/android/sync/pull","httpStatus":409}]""";
        var handler = new RecordingHandler(events);
        var cut = RenderPage(handler);

        cut.WaitForState(() => cut.FindAll(".admin-record__summary").Count == 1);
        cut.Find(".admin-record__summary").TextContent.Should().Contain("HTTP 409 yanıtı").And.Contain("409 POST /api/v1/android/sync/pull");
        cut.Find(".admin-record__summary").Click();
        cut.Find(".admin-record__detail").TextContent.Should().Contain("409 POST /api/v1/android/sync/pull");
    }

    [Fact]
    public void Sync_round_filter_asks_for_all_severities_and_shows_the_measurement()
    {
        var events = $$"""[{"id":"{{Guid.NewGuid()}}","tenantId":"{{Guid.NewGuid()}}","occurredAtUtc":"2026-09-17T09:00:00Z","kind":"SYNC_ROUND","severity":"INFO","operation":"sync_round","message":"trigger=live busyMs=29500 totalMs=31000 | cari:SKIP:5:0"}]""";
        var handler = new RecordingHandler(events);
        var cut = RenderPage(handler);
        cut.WaitForState(() => handler.TelemetryQueries.Count == 1);

        cut.Find("select[aria-label='Kayıt türü']").Change("SYNC_ROUND");
        cut.Find("button.btn-primary").Click();

        cut.WaitForState(() => handler.TelemetryQueries.Count == 2);
        handler.TelemetryQueries[1].Should().Contain("kind=SYNC_ROUND").And.Contain("severity=&");
        cut.Find(".admin-record__summary").TextContent.Should().Contain("Senkron turu").And.Contain("busyMs=29500");
        cut.FindAll(".admin-record--error").Should().BeEmpty();
    }

    private IRenderedComponent<Telemetry> RenderPage(HttpMessageHandler handler)
    {
        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://central.example") }, tokenStore));
        JSInterop.Mode = JSRuntimeMode.Loose;
        return Render<Telemetry>();
    }

    private sealed class RecordingHandler(string telemetryJson) : HttpMessageHandler
    {
        public List<string> TelemetryQueries { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var isTelemetry = request.RequestUri?.AbsolutePath.EndsWith("/telemetry", StringComparison.Ordinal) == true;
            if (isTelemetry) lock (TelemetryQueries) TelemetryQueries.Add(request.RequestUri!.Query);
            var json = isTelemetry ? telemetryJson : "[]";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }

    private sealed class TelemetryHandler(Guid eventId, Guid tenantId) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var json = request.RequestUri?.AbsolutePath.EndsWith("/telemetry", StringComparison.Ordinal) == true
                ? $$"""[{"id":"{{eventId}}","tenantId":"{{tenantId}}","occurredAtUtc":"2026-09-04T09:35:00Z","severity":"ERROR","exceptionType":"FirebaseException","message":"Permission denied","stackTrace":"at Example.Sync()"}]"""
                : "[]";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }
}
