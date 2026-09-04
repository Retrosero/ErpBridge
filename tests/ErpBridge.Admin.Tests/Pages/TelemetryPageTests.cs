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
