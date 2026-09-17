using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ErpBridge.CentralApi.LogCenter;
using ErpBridge.CentralApi.Tests.Support;
using ErpBridge.Diagnostics;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ErpBridge.CentralApi.Tests.LogCenter;

public sealed class BufferedLogProviderTests
{
    [Fact]
    public async Task Ships_warnings_with_well_known_context_and_skips_the_rest()
    {
        var provider = new CapturingProvider(new BufferedLogOptions { FlushInterval = TimeSpan.FromMilliseconds(50), ExcludedCategoryPrefixes = ["Noisy."] });
        var scopes = new LoggerExternalScopeProvider();
        provider.SetScopeProvider(scopes);
        var logger = provider.CreateLogger("ErpBridge.Test");

        using (scopes.Push(new Dictionary<string, object> { ["CorrelationId"] = "corr-1", ["TenantId"] = "t-1", ["SecretPath"] = "/customers/C-001" }))
        {
            logger.LogInformation("not shipped");
            logger.LogWarning("Slow {HttpRoute} took {DurationMs} ms for {Customer}", "/api/x", 4200, "Bakkal Ali");
            provider.CreateLogger("Noisy.Thing").LogError("excluded");
        }
        await provider.FlushAndStopAsync(TimeSpan.FromSeconds(5));

        var line = provider.Lines.Should().ContainSingle().Subject;
        line.Severity.Should().Be("WARN");
        line.Category.Should().Be("ErpBridge.Test");
        line.Message.Should().Be("Slow /api/x took 4200 ms for Bakkal Ali");
        line.Properties.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["CorrelationId"] = "corr-1", ["TenantId"] = "t-1", ["HttpRoute"] = "/api/x", ["DurationMs"] = "4200",
        }, "only well-known keys travel as context");
    }

    [Fact]
    public async Task A_failing_target_never_throws_into_the_caller_and_lines_logged_while_sending_are_ignored()
    {
        var provider = new CapturingProvider(new BufferedLogOptions { FlushInterval = TimeSpan.FromMilliseconds(20) }) { Fail = true };
        var logger = provider.CreateLogger("ErpBridge.Test");
        provider.LoggerUsedWhileSending = logger;

        var act = () => logger.LogError(new InvalidOperationException("boom"), "failed");
        act.Should().NotThrow();
        await WaitUntilAsync(() => provider.FailedCount == 1);
        provider.Fail = false;
        logger.LogError("after recovery");
        await provider.FlushAndStopAsync(TimeSpan.FromSeconds(5));

        provider.Lines.Select(l => l.Message).Should().Contain("after recovery").And.NotContain("logged while sending");
        provider.Lines.Should().Contain(l => l.Properties.GetValueOrDefault("Kind") == "LOG_SHIPPING_LOSS", "a lost batch is reported once the target is back");
    }

    [Fact]
    public async Task A_full_queue_drops_instead_of_blocking()
    {
        var gate = new TaskCompletionSource();
        var provider = new CapturingProvider(new BufferedLogOptions { Capacity = 2, BatchSize = 1, FlushInterval = TimeSpan.FromMilliseconds(1) }) { Gate = gate.Task };
        var logger = provider.CreateLogger("ErpBridge.Test");

        for (var i = 0; i < 50; i++) logger.LogWarning("line {N}", i);

        provider.DroppedCount.Should().BeGreaterThan(0);
        gate.SetResult();
        await provider.FlushAndStopAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Remote_provider_posts_the_batch_with_the_key()
    {
        var handler = new RecordingHandler();
        var provider = new RemoteLogProvider(new RemoteLogOptions
        {
            Endpoint = new Uri("https://central.example/api/v1/internal/logs"), Key = new string('k', 40), Source = "portal",
            AppVersion = "1.2.3", FlushInterval = TimeSpan.FromMilliseconds(20),
        }, handler);

        provider.CreateLogger("ErpBridge.Portal.Pages").LogError(new TimeoutException("slow"), "Stock page failed");
        await provider.FlushAndStopAsync(TimeSpan.FromSeconds(5));

        handler.Key.Should().Be(new string('k', 40));
        var batch = JsonSerializer.Deserialize<InternalLogBatch>(handler.Body!)!;
        batch.Source.Should().Be("portal");
        var shipped = batch.Events.Should().ContainSingle().Subject;
        shipped.Severity.Should().Be("ERROR");
        shipped.ExceptionType.Should().Be(typeof(TimeoutException).FullName);
        shipped.AppVersion.Should().Be("1.2.3");
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (var i = 0; i < 200 && !condition(); i++) await Task.Delay(25);
        condition().Should().BeTrue();
    }

    private sealed class CapturingProvider(BufferedLogOptions options) : BufferedLogProvider(options)
    {
        public List<ShippedLog> Lines { get; } = [];
        public bool Fail { get; set; }
        public Task? Gate { get; init; }
        public ILogger? LoggerUsedWhileSending { get; set; }

        protected override async Task SendAsync(IReadOnlyList<ShippedLog> batch, CancellationToken ct)
        {
            if (Gate is not null) await Gate;
            LoggerUsedWhileSending?.LogError("logged while sending");
            if (Fail) throw new HttpRequestException("target down");
            lock (Lines) Lines.AddRange(batch);
        }
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public string? Key { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Key = request.Headers.GetValues(InternalLogContract.KeyHeader).Single();
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}

public sealed class CorrelationIdHandlerTests
{
    [Fact]
    public async Task Adds_an_id_and_logs_5xx_and_failures_without_the_query()
    {
        var logs = new List<(LogLevel Level, string Message)>();
        using var factory = LoggerFactory.Create(builder => builder.AddProvider(new ListProvider(logs)).SetMinimumLevel(LogLevel.Trace));
        var inner = new StubHandler();
        var handler = new CorrelationIdHandler(factory.CreateLogger<CorrelationIdHandler>()) { InnerHandler = inner };
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://central.example/") };

        inner.Status = HttpStatusCode.OK;
        await client.GetAsync("api/v1/portal/customers/card?code=C-001");
        Guid.TryParse(inner.LastCorrelationId, out _).Should().BeTrue();
        logs.Should().BeEmpty("a successful call is not logged");

        inner.Status = HttpStatusCode.BadGateway;
        await client.GetAsync("api/v1/portal/customers/card?code=C-001");
        logs.Should().ContainSingle().Which.Message.Should().Contain("502").And.Contain("/api/v1/portal/customers/card").And.NotContain("C-001").And.Contain(inner.LastCorrelationId!);

        inner.Throw = true;
        var act = () => client.GetAsync("api/v1/portal/stock");
        await act.Should().ThrowAsync<HttpRequestException>();
        logs.Should().HaveCount(2);
        logs[1].Message.Should().Contain("UPSTREAM_UNREACHABLE");
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        public HttpStatusCode Status { get; set; }
        public bool Throw { get; set; }
        public string? LastCorrelationId { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastCorrelationId = request.Headers.GetValues(CorrelationIdHandler.HeaderName).Single();
            if (Throw) throw new HttpRequestException("connection refused");
            return Task.FromResult(new HttpResponseMessage(Status));
        }
    }

    private sealed class ListProvider(List<(LogLevel, string)> target) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new L(target);
        public void Dispose() { }

        private sealed class L(List<(LogLevel, string)> target) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
                target.Add((logLevel, formatter(state, exception)));
        }
    }
}

public sealed class InternalLogEndpointTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly string Key = "internal-log-key-" + new string('x', 32);
    private readonly SqliteCentralApiFactory _factory;

    public InternalLogEndpointTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Without_a_configured_key_the_route_does_not_exist()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync(InternalLogContract.Route, Batch("portal"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Wrong_key_and_foreign_source_are_rejected()
    {
        using var host = WithKey();
        var wrong = new HttpRequestMessage(HttpMethod.Post, InternalLogContract.Route) { Content = JsonContent.Create(Batch("portal")) };
        wrong.Headers.Add(InternalLogContract.KeyHeader, "nope");
        (await host.CreateClient().SendAsync(wrong)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        (await SendAsync(host, Batch("android"))).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Malformed_or_oversized_batches_are_refused_without_a_500()
    {
        using var host = WithKey();
        async Task<HttpResponseMessage> Raw(string json, long? declaredLength = null)
        {
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            if (declaredLength is { } length) content.Headers.ContentLength = length;
            var request = new HttpRequestMessage(HttpMethod.Post, InternalLogContract.Route) { Content = content };
            request.Headers.Add(InternalLogContract.KeyHeader, Key);
            return await host.CreateClient().SendAsync(request);
        }

        (await Raw("{\"source\":\"portal\",\"events\":null}")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Raw("{\"source\":\"portal\",\"events\":[null,{\"eventId\":null}]}")).StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await Raw("not json")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Raw(new string(' ', 2 * 1024 * 1024) + "{}")).StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
    }

    [Fact]
    public async Task Portal_lines_are_stored_with_their_context()
    {
        using var host = WithKey();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var batch = Batch("portal", tenantId, userId);

        var response = await SendAsync(host, batch);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        using var db = _factory.CreateDbContext();
        var row = await db.LogEvents.AsNoTracking().SingleAsync(e => e.EventId == batch.Events[0].EventId);
        row.Source.Should().Be(LogSources.Portal);
        row.TenantId.Should().Be(tenantId);
        row.UserId.Should().Be(userId);
        row.CorrelationId.Should().Be("portal-corr");
        row.Operation.Should().Be("ErpBridge.Portal.Pages.Stock");
        row.Kind.Should().Be(ShippedLogMapping.DefaultKind);
        row.Message.Should().NotContain("hunter2");
        row.PropertiesJson.Should().Contain("ErpBridge.Portal.Pages.Stock");
    }

    private Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> WithKey() =>
        _factory.WithWebHostBuilder(builder => builder.UseSetting(InternalLogContract.KeyConfig, Key));

    private static async Task<HttpResponseMessage> SendAsync(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> host, InternalLogBatch batch)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, InternalLogContract.Route) { Content = JsonContent.Create(batch) };
        request.Headers.Add(InternalLogContract.KeyHeader, Key);
        return await host.CreateClient().SendAsync(request);
    }

    private static InternalLogBatch Batch(string source, Guid? tenantId = null, Guid? userId = null) => new()
    {
        Source = source,
        Events =
        [
            new InternalLogEvent
            {
                EventId = Guid.NewGuid().ToString(), OccurredAtUtc = DateTimeOffset.UtcNow, Severity = "ERROR", Category = "ErpBridge.Portal.Pages.Stock",
                Message = "Stock page failed; Password=hunter2", ExceptionType = "System.TimeoutException", AppVersion = "1.0.0",
                Properties = new Dictionary<string, string>
                {
                    ["TenantId"] = tenantId?.ToString() ?? string.Empty, ["UserId"] = userId?.ToString() ?? string.Empty, ["CorrelationId"] = "portal-corr",
                },
            },
        ],
    };
}

public sealed class RequestOutcomeLoggingTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public RequestOutcomeLoggingTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_slow_request_is_stored_with_route_template_and_company()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"SLOW-{suffix}");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, $"AK-SLOW-{suffix}", scopes: new[] { "mobile:read" });
        // Every request counts as slow; the database logger is on as in production.
        using var host = _factory.WithWebHostBuilder(builder => builder
            .UseSetting(RequestOutcomeLogging.SlowRequestConfig, "-1")
            .UseSetting(DatabaseLogProvider.EnabledConfig, "true"));
        var client = host.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/mobile/telemetry/batch")
        {
            Content = JsonContent.Create(new { events = new[] { new { eventId = Guid.NewGuid().ToString(), kind = "CRASH", severity = "INFO" } } }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        request.Headers.Add("X-Tenant-Id", tenant.Id.ToString());
        request.Headers.Add(CorrelationId.HeaderName, "slow-" + suffix);

        (await client.SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.OK);

        Domain.LogEvent? row = null;
        for (var i = 0; i < 100 && row is null; i++)
        {
            await Task.Delay(100);
            using var db = _factory.CreateDbContext();
            row = await db.LogEvents.AsNoTracking().FirstOrDefaultAsync(e => e.CorrelationId == "slow-" + suffix && e.Kind == RequestOutcomeLogging.SlowRequestKind);
        }
        row.Should().NotBeNull("the database logger flushes within a few seconds");
        row!.Source.Should().Be(LogSources.CentralApi);
        row.Severity.Should().Be("WARN");
        row.TenantId.Should().Be(tenant.Id);
        row.HttpRoute.Should().Be("/api/v1/mobile/telemetry/batch");
        row.HttpMethod.Should().Be("POST");
        row.HttpStatus.Should().Be(200);
        row.DurationMs.Should().NotBeNull();
    }
}

public sealed class RequestCorrelationScopeTests
{
    [Fact]
    public async Task The_code_an_error_page_shows_is_the_correlation_id_of_the_logged_exception()
    {
        var provider = new ScopeCapture();
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders().AddProvider(provider);
        await using var app = builder.Build();
        app.UseRequestCorrelationScope();
        app.UseExceptionHandler(new Microsoft.AspNetCore.Builder.ExceptionHandlerOptions
        {
            ExceptionHandler = context => context.Response.WriteAsync(context.TraceIdentifier),
        });
        app.MapGet("/boom", (Func<string>)(() => throw new InvalidOperationException("boom")));
        await app.StartAsync();

        var response = await app.GetTestClient().GetAsync("/boom");
        var shown = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        provider.Errors.Should().Contain(e => e.Exception is InvalidOperationException && e.CorrelationId == shown);
    }

    private sealed class ScopeCapture : ILoggerProvider, ISupportExternalScope
    {
        private IExternalScopeProvider _scopes = new LoggerExternalScopeProvider();
        public List<(Exception? Exception, string? CorrelationId)> Errors { get; } = [];
        public ILogger CreateLogger(string categoryName) => new Logger(this);
        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopes = scopeProvider;
        public void Dispose() { }

        private sealed class Logger(ScopeCapture owner) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => owner._scopes.Push(state);
            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel < LogLevel.Error) return;
                string? id = null;
                owner._scopes.ForEachScope((scope, _) =>
                {
                    if (scope is IEnumerable<KeyValuePair<string, object>> pairs)
                        foreach (var (key, value) in pairs) if (key == "CorrelationId") id = value?.ToString();
                }, (object?)null);
                lock (owner.Errors) owner.Errors.Add((exception, id));
            }
        }
    }
}
