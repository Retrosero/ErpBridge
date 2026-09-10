using System.Net;
using ErpBridge.Core.Stores;
using ErpBridge.RemoteApi.Authentication;
using ErpBridge.RemoteApi.Http;
using ErpBridge.RemoteApi.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace ErpBridge.RemoteApi.DependencyInjection;

/// <summary>DI helpers for registering the central API client.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register <see cref="IRemoteApiClient"/>, options, and a Polly-protected <see cref="HttpClient"/>
    /// reading configuration from the <c>CentralApi</c> section.
    /// </summary>
    public static IServiceCollection AddErpBridgeRemoteApi(this IServiceCollection services, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        services
            .AddOptions<CentralApiOptions>()
            .Bind(config.GetSection(CentralApiOptions.SectionName))
            .Validate(o => o.TimeoutSeconds > 0, "CentralApi.TimeoutSeconds must be positive.")
            .Validate(o => o.Retry.MaxAttempts >= 0, "CentralApi.Retry.MaxAttempts must be non-negative.")
            .Validate(o => o.Retry.InitialDelaySeconds > 0, "CentralApi.Retry.InitialDelaySeconds must be positive.")
            .ValidateOnStart();

        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();

        services.AddHttpClient<IRemoteApiClient, HttpRemoteApiClient>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<CentralApiOptions>>().Value;
                if (!string.IsNullOrWhiteSpace(opts.BaseUrl))
                {
                    client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
                }
                client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ErpBridge-Agent/1.0");
            })
            // BootstrapSyncService owns bootstrap retries because it must keep
            // one Idempotency-Key across attempts and can fall back to
            // mergeable sections. Letting HttpClient add another retry layer
            // here made one slow upload wait through both retry schedules
            // (several minutes) before the UI reported a timeout.
            .AddPolicyHandler((Func<HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>>)(request =>
                IsBootstrapRequest(request)
                    ? Policy.NoOpAsync<HttpResponseMessage>()
                    : BuildRetryPolicy()));

        return services;
    }

    /// <summary>
    /// Polly retry policy. Retries on transient HTTP failures and 429 responses using
    /// the canonical 5s/15s/60s/300s exponential backoff schedule (capped).
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> BuildRetryPolicy() => BuildRetryPolicy(CanonicalRetryDelays);

    /// <summary>
    /// True for every bootstrap route, not just the legacy single-shot
    /// <c>POST /api/v1/bootstrap</c>. The chunked upload lives under
    /// <c>/api/v1/bootstrap/upload/...</c>, so an exact match let those
    /// requests keep the HttpClient retry policy on top of the one
    /// BootstrapSyncService already runs: a failing <c>/complete</c> burned
    /// 5+15+60+300 s here before the service's own 5/15/60 s pipeline even saw
    /// the first failure, and the nine-section fallback repeated that. One
    /// unhealthy server turned a manual "bootstrap verisini oluştur" into an
    /// hour of silence in the UI. Bootstrap callers (upload, status probe,
    /// notify long-poll) all own their retry cadence.
    /// </summary>
    public static bool IsBootstrapRequest(HttpRequestMessage request)
    {
        var path = request.RequestUri?.AbsolutePath.TrimEnd('/');
        return path is not null
               && (string.Equals(path, BootstrapRoutePrefix, StringComparison.OrdinalIgnoreCase)
                   || path.StartsWith(BootstrapRoutePrefix + "/", StringComparison.OrdinalIgnoreCase));
    }

    private const string BootstrapRoutePrefix = "/api/v1/bootstrap";

    /// <summary>Canonical 5/15/60/300-second backoff schedule.</summary>
    public static readonly IReadOnlyList<TimeSpan> CanonicalRetryDelays = new[]
    {
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(15),
        TimeSpan.FromSeconds(60),
        TimeSpan.FromSeconds(300),
    };

    /// <summary>
    /// Build a Polly retry policy with a custom delay schedule. Exposed for
    /// tests that need fast retries; production code should use the parameterless
    /// <see cref="BuildRetryPolicy()"/>.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> BuildRetryPolicy(IEnumerable<TimeSpan> delays)
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests
                           || (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                delays,
                onRetry: static (outcome, delay, attempt, context) =>
                {
                    // Polly v7 onRetry callback. The actual retry outcome is also
                    // logged at the HttpClient level by HttpRemoteApiClient for the
                    // canonical attempt path; this hook is here for future per-retry
                    // observability (e.g. metrics).
                    _ = outcome;
                    _ = delay;
                    _ = attempt;
                    _ = context;
                });
    }

    private static IEnumerable<TimeSpan> BuildDelaySchedule(int initialSeconds, int maxAttempts)
    {
        // Canonical schedule: 5, 15, 60, 300 (cap) seconds.
        var schedule = new[] { 5, 15, 60, 300 };
        var attempts = Math.Max(0, maxAttempts);
        for (var i = 0; i < attempts; i++)
        {
            var seconds = i < schedule.Length ? schedule[i] : schedule[^1];
            yield return TimeSpan.FromSeconds(seconds);
        }
        _ = initialSeconds; // retained for future tuning; schedule is canonical for now.
    }
}
