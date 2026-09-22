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

    private readonly List<(HttpMethod Method, string PathAndQuery, string? Authorization, string? Body)> _requests = [];

    /// <summary>
    /// A snapshot of what was sent so far. Pages with live loops (desk, TV board) call the API from background
    /// tasks while the test reads, so the log is copied under a lock rather than exposed as a live list.
    /// </summary>
    public IReadOnlyList<(HttpMethod Method, string PathAndQuery, string? Authorization, string? Body)> Requests
    {
        get { lock (_requests) return [.. _requests]; }
    }

    public FakeCentralApi Answer(string pathAndQuery, object body, HttpStatusCode status = HttpStatusCode.OK)
    {
        lock (_answers) _answers[pathAndQuery] = (status, JsonSerializer.Serialize(body, Web));
        return this;
    }

    private readonly List<(HttpMethod Method, string PathPrefix, HttpStatusCode Status, string Body)> _prefixAnswers = [];

    /// <summary>
    /// Answers a request whose query the test cannot predict (a client-generated idempotency key,
    /// for example) — matched by method and path prefix once no exact <see cref="Answer"/> fits.
    /// </summary>
    public FakeCentralApi AnswerPrefix(HttpMethod method, string pathPrefix, object body, HttpStatusCode status = HttpStatusCode.OK)
    {
        lock (_prefixAnswers) _prefixAnswers.Add((method, pathPrefix, status, JsonSerializer.Serialize(body, Web)));
        return this;
    }

    private readonly Dictionary<string, TaskCompletionSource> _holds = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Holds the answer to a path until the returned source is completed: a slow server.</summary>
    public TaskCompletionSource Hold(string pathAndQuery)
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _holds[pathAndQuery] = gate;
        return gate;
    }

    public FakeCentralApi Fail(string pathAndQuery, HttpStatusCode status, string errorCode) =>
        Answer(pathAndQuery, new { errorCode, message = errorCode }, status);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri!.PathAndQuery;
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        lock (_requests) _requests.Add((request.Method, path, request.Headers.Authorization?.ToString(), body));
        if (_holds.Remove(path, out var gate)) await gate.Task.WaitAsync(cancellationToken);
        (HttpStatusCode Status, string Body)? found;
        lock (_answers) found = _answers.TryGetValue(path, out var answer) ? answer : null;
        if (found is null)
        {
            lock (_prefixAnswers)
            {
                var match = _prefixAnswers.FirstOrDefault(a => a.Method == request.Method && path.StartsWith(a.PathPrefix, StringComparison.OrdinalIgnoreCase));
                if (match.PathPrefix is not null) found = (match.Status, match.Body);
            }
        }
        var (status, json) = found ?? (HttpStatusCode.NotFound, "{\"errorCode\":\"NOT_FOUND\"}");
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

/// <summary>A TV's stored pairing, in memory.</summary>
public sealed class MemoryDisplaySessionStore : IDisplaySessionStore
{
    public DisplaySession? Stored { get; set; }
    public Task<DisplaySession?> LoadAsync() => Task.FromResult(Stored);
    public Task SaveAsync(DisplaySession session) { Stored = session; return Task.CompletedTask; }
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
    /// <param name="kioskTiming">The TV board's rhythm; by default shortened to a few dozen milliseconds.</param>
    public static (FakeCentralApi Api, PortalSession Session, MemorySessionPersistence Storage) Register(BunitContext context, PortalSessionState? signedIn = null, PortalSessionState? inTab = null, bool popoverProvider = true, KioskTiming? kioskTiming = null)
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
        context.Services.AddSingleton(new DisplayApiClient(new HttpClient(api) { BaseAddress = new Uri("https://central.test/") }));
        context.Services.AddSingleton<IDisplaySessionStore>(new MemoryDisplaySessionStore());
        // The board's rhythm, shortened so a test sees pairing, polling and page turns within a second.
        context.Services.AddSingleton(kioskTiming ?? new KioskTiming
        {
            PairingPoll = TimeSpan.FromMilliseconds(40),
            MinPollGap = TimeSpan.FromMilliseconds(40),
            Retry = TimeSpan.FromMilliseconds(40),
            Tick = TimeSpan.FromMilliseconds(40),
            Rotate = TimeSpan.FromMilliseconds(150),
            CardsPerPage = 3,
        });
        // MudBlazor components call into their JS module; the tests only check markup and API calls.
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        if (popoverProvider) context.Render<MudPopoverProvider>();
        return (api, session, storage);
    }
}
