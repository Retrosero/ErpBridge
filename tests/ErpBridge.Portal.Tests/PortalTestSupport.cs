using System.Net;
using System.Text;
using System.Text.Json;
using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Session;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// bUnit context for portal pages. MudBlazor's popover service can only be disposed
/// asynchronously, so the container is released through xUnit's async hook first.
/// </summary>
public abstract class PortalPageTestContext : BunitContext, IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();
}

/// <summary>A central API that answers by path and records what it was sent.</summary>
public sealed class FakeCentralApi : HttpMessageHandler
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private readonly Dictionary<string, (HttpStatusCode Status, string Body)> _answers = new(StringComparer.OrdinalIgnoreCase);

    public List<(HttpMethod Method, string PathAndQuery, string? Authorization, string? Body)> Requests { get; } = [];

    public FakeCentralApi Answer(string pathAndQuery, object body, HttpStatusCode status = HttpStatusCode.OK)
    {
        _answers[pathAndQuery] = (status, JsonSerializer.Serialize(body, Web));
        return this;
    }

    public FakeCentralApi Fail(string pathAndQuery, HttpStatusCode status, string errorCode) =>
        Answer(pathAndQuery, new { errorCode, message = errorCode }, status);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri!.PathAndQuery;
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add((request.Method, path, request.Headers.Authorization?.ToString(), body));
        var (status, json) = _answers.TryGetValue(path, out var answer) ? answer : (HttpStatusCode.NotFound, "{\"errorCode\":\"NOT_FOUND\"}");
        return new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }
}

/// <summary>The tab storage, in memory.</summary>
public sealed class MemorySessionPersistence : ISessionPersistence
{
    public PortalSessionState? Stored { get; set; }
    public Task SaveAsync(PortalSessionState state) { Stored = state; return Task.CompletedTask; }
    public Task<PortalSessionState?> LoadAsync() => Task.FromResult(Stored);
    public Task ClearAsync() { Stored = null; return Task.CompletedTask; }
}

/// <summary>A clock the tests move.</summary>
public sealed class TestClock(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;
    public override DateTimeOffset GetUtcNow() => Now;
}

public static class PortalTestSetup
{
    public static readonly DateTimeOffset Now = new(2026, 9, 21, 9, 0, 0, TimeSpan.Zero);

    public static PortalSessionState State(string role = "ADMIN", string token = "tok-patron", string[]? roles = null) =>
        new(token, Now.AddDays(30), "Ege Dağıtım", "EGE123", "native", "patron", "Firma Sahibi", role, CanApprove: true, Roles: roles ?? [role]);

    /// <summary>Registers the portal's services around a fake API; returns the pieces a test inspects.</summary>
    /// <param name="popoverProvider">False when the test renders the layout, which brings its own.</param>
    public static (FakeCentralApi Api, PortalSession Session, MemorySessionPersistence Storage) Register(BunitContext context, PortalSessionState? signedIn = null, PortalSessionState? inTab = null, bool popoverProvider = true)
    {
        var api = new FakeCentralApi();
        var clock = new TestClock(Now);
        var session = new PortalSession(clock);
        // A test's signed-in session has just come from the server; a tab session is restored.
        if (signedIn is not null) session.SignIn(signedIn, fresh: true);
        var storage = new MemorySessionPersistence { Stored = inTab };
        context.Services.AddSingleton<TimeProvider>(clock);
        context.Services.AddSingleton(session);
        context.Services.AddSingleton<ISessionPersistence>(storage);
        context.Services.AddSingleton(new PortalApiClient(new HttpClient(api) { BaseAddress = new Uri("https://central.test/") }, session));
        // MudBlazor components call into their JS module; the tests only check markup and API calls.
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        if (popoverProvider) context.Render<MudPopoverProvider>();
        return (api, session, storage);
    }
}
