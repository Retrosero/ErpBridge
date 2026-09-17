using System.Diagnostics;
using System.Text.Json;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.Diagnostics;

namespace ErpBridge.CentralApi.LogCenter;

/// <summary>
/// Log Merkezi L2b: this API's own warning+ log lines go to <c>log_events</c> (source <c>central_api</c>) through
/// <see cref="ILogEventWriter"/>, in batches, off the request path.
/// </summary>
public sealed class DatabaseLogProvider : BufferedLogProvider
{
    public const string EnabledConfig = "Logs:Database:Enabled";

    /// <summary>
    /// Never stored: EF Core (a database failure would log about itself forever), and the two places that already
    /// store an unhandled exception with more context.
    /// </summary>
    public static readonly IReadOnlyList<string> Excluded =
    [
        "Microsoft.EntityFrameworkCore",
        UnhandledExceptionHandler.LoggerCategory,
        "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware",
    ];

    private readonly IServiceProvider _services;
    private readonly string? _appVersion = typeof(DatabaseLogProvider).Assembly.GetName().Version?.ToString();

    public DatabaseLogProvider(IServiceProvider services, BufferedLogOptions options) : base(options)
    {
        _services = services;
    }

    protected override async Task SendAsync(IReadOnlyList<ShippedLog> batch, CancellationToken ct)
    {
        using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var writer = scope.ServiceProvider.GetRequiredService<ILogEventWriter>();
        await writer.WriteAsync(batch.Select(line => ShippedLogMapping.ToInput(LogSources.CentralApi, line.EventId, line.OccurredAtUtc,
            line.Severity, line.Category, line.Message, line.ExceptionType, line.StackTrace, _appVersion, line.Properties)).ToList(), ct);
    }

    /// <summary>Registers the provider unless <c>Logs:Database:Enabled</c> is false (default: off in the Test environment).</summary>
    public static void Register(WebApplicationBuilder builder, IConfiguration cfg)
    {
        var enabled = cfg.GetValue<bool?>(EnabledConfig) ?? !builder.Environment.IsEnvironment("Test");
        if (!enabled) return;
        var minimum = Enum.TryParse<LogLevel>(cfg["Logs:Database:MinimumLevel"], ignoreCase: true, out var level) ? level : LogLevel.Warning;
        builder.Services.AddSingleton<ILoggerProvider>(services => new DatabaseLogProvider(services, new BufferedLogOptions
        {
            MinimumLevel = minimum,
            ExcludedCategoryPrefixes = [.. Excluded],
        }));
    }
}

/// <summary>A shipped log line (from this API or from the Portal/Admin) as log centre input.</summary>
public static class ShippedLogMapping
{
    public const string DefaultKind = "SERVER_LOG";

    public static LogEventInput ToInput(string source, string? eventId, DateTimeOffset? occurredAtUtc, string? severity, string? category,
        string? message, string? exceptionType, string? stackTrace, string? appVersion, IReadOnlyDictionary<string, string>? properties)
    {
        properties ??= new Dictionary<string, string>();
        string? Get(string key) => properties.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;
        Guid? Id(string key) => Guid.TryParse(Get(key), out var id) ? id : null;
        int? Number(string key) => int.TryParse(Get(key), out var n) ? n : null;

        var extra = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(category)) extra["category"] = category;
        if (Get("EventName") is { } eventName) extra["eventName"] = eventName;

        return new LogEventInput
        {
            EventId = eventId,
            Source = source,
            TenantId = Id("TenantId"),
            UserId = Id("UserId"),
            AgentId = Id("AgentId"),
            OccurredAtUtc = occurredAtUtc,
            Severity = severity,
            Kind = Get("Kind") ?? DefaultKind,
            Operation = Get("Operation") ?? category,
            Message = message,
            ExceptionType = exceptionType,
            StackTrace = stackTrace,
            AppVersion = appVersion,
            CorrelationId = Get("CorrelationId"),
            HttpMethod = Get("HttpMethod"),
            HttpRoute = Get("HttpRoute"),
            HttpStatus = Number("HttpStatus"),
            DurationMs = Number("DurationMs"),
            PropertiesJson = extra.Count == 0 ? null : JsonSerializer.Serialize(extra),
        };
    }
}

/// <summary>
/// Log Merkezi L2c: puts the caller (company, mobile user or agent) into the logging scope for everything the
/// request logs, and records a request that ended in 5xx or ran longer than <c>Logs:SlowRequestMs</c> (3000).
/// An exception is not recorded here — it propagates to <see cref="UnhandledExceptionHandler"/>, which stores it.
/// Long-polls (<c>wait=</c>, <c>/notify</c>, <c>/events</c>) are slow on purpose and never count as slow.
/// </summary>
public static class RequestOutcomeLogging
{
    public const string LoggerCategory = "ErpBridge.CentralApi.Requests";
    public const string SlowRequestConfig = "Logs:SlowRequestMs";
    public const string ServerErrorKind = "HTTP_5XX";
    public const string SlowRequestKind = "SLOW_REQUEST";

    public static IApplicationBuilder UseRequestOutcomeLogging(this IApplicationBuilder app)
    {
        var slowMs = app.ApplicationServices.GetRequiredService<IConfiguration>().GetValue(SlowRequestConfig, 3000);
        var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(LoggerCategory);
        return app.Use(async (context, next) =>
        {
            var started = Stopwatch.GetTimestamp();
            using (logger.BeginScope(new CallerScope(context)))
            {
                await next(context);

                var elapsedMs = (int)Math.Min(int.MaxValue, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
                var status = context.Response.StatusCode;
                var serverError = status >= 500;
                var slow = elapsedMs > slowMs && !IsLongPoll(context);
                if (!serverError && !slow) return;

                // The route template, never the raw path: paths carry customer codes and ids.
                var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "(unmatched)";
                var kind = serverError ? ServerErrorKind : SlowRequestKind;
                logger.Log(serverError ? LogLevel.Error : LogLevel.Warning,
                    "{Kind}: {HttpMethod} {HttpRoute} answered {HttpStatus} in {DurationMs} ms",
                    kind, context.Request.Method, route, status, elapsedMs);
            }
        });
    }

    /// <summary>
    /// Company, mobile user and agent of the request, read from <see cref="HttpContext.User"/> when a line is logged —
    /// not when the scope opens: API-key and policy-specific schemes authenticate later, inside the endpoint.
    /// </summary>
    private sealed class CallerScope(HttpContext context) : IEnumerable<KeyValuePair<string, object?>>
    {
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            var user = context.User;
            if (user.TryGetTenantId(out var tenantId)) yield return new("TenantId", tenantId);
            var scope = user.FindFirst(CentralApiClaims.Scope)?.Value;
            if (scope == CentralApiClaims.MobileUserScope && Guid.TryParse(user.FindFirst("sub")?.Value, out var userId)) yield return new("UserId", userId);
            if (scope == "agent" && user.TryGetAgentId(out var agentId)) yield return new("AgentId", agentId);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => string.Join(", ", this.Select(pair => $"{pair.Key}:{pair.Value}"));
    }

    private static bool IsLongPoll(HttpContext context) =>
        context.Request.Query.ContainsKey("wait")
        || context.Request.Path.Value?.EndsWith("/notify", StringComparison.OrdinalIgnoreCase) == true
        || context.Request.Path.Value?.EndsWith("/events", StringComparison.OrdinalIgnoreCase) == true;
}
