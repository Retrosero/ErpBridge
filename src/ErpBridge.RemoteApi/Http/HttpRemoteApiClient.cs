using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.RemoteApi.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErpBridge.RemoteApi.Http;

/// <summary>
/// Default <see cref="IRemoteApiClient"/> implementation. Uses a Polly-protected
/// <see cref="HttpClient"/> and System.Text.Json for (de)serialization.
///
/// All five endpoints defined in <c>docs/api-contracts.md</c> are bound here:
///   - POST /api/v1/licenses/validate
///   - GET  /api/v1/jobs/pending
///   - POST /api/v1/jobs/ack
///   - POST /api/v1/bootstrap
///   - POST /api/v1/agents/heartbeat
///
/// Per the contract, every POST carries an <c>Idempotency-Key</c> header so a
/// retried request after a network blip is not interpreted as a new operation
/// by the central API. For <see cref="SendAckAsync"/> the key is the job id
/// (the natural unit of work). For other POSTs the key is generated per call.
/// </summary>
public sealed class HttpRemoteApiClient : IRemoteApiClient
{
    /// <summary>Canonical HTTP header name for the central API idempotency key.</summary>
    public const string IdempotencyKeyHeader = "Idempotency-Key";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IOptionsMonitor<CentralApiOptions> _options;
    private readonly ILogger<HttpRemoteApiClient> _logger;

    /// <summary>Creates a new client bound to the supplied <paramref name="http"/> and <paramref name="options"/>.</summary>
    public HttpRemoteApiClient(HttpClient http, IOptionsMonitor<CentralApiOptions> options, ILogger<HttpRemoteApiClient> logger)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            throw new ArgumentException("License key must not be empty.", nameof(licenseKey));
        }

        var opts = _options.CurrentValue;
        using var request = BuildRequest(HttpMethod.Post, "/api/v1/licenses/validate", opts, idempotencyKey: NewIdempotencyKey(licenseKey));
        request.Content = SerializeJson(new { licenseKey });

        // A 404 from /licenses/validate means "license not found / invalid" —
        // surface that as a clean invalid result rather than throwing.
        var (result, notFound) = await SendAsyncAllowNotFound<LicenseValidationResult>(request, opts, ct);
        if (notFound)
        {
            return new LicenseValidationResult
            {
                Valid = false,
                ErrorCode = "LICENSE_NOT_FOUND",
                ErrorMessage = "Central API did not recognise the license key.",
            };
        }
        return result ?? new LicenseValidationResult
        {
            Valid = false,
            ErrorCode = "EMPTY_RESPONSE",
            ErrorMessage = "Central API returned no body.",
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RemoteJob>> GetPendingJobsAsync(CancellationToken ct = default)
    {
        var opts = _options.CurrentValue;
        using var request = BuildRequest(HttpMethod.Get, "/api/v1/jobs/pending?take=50", opts, idempotencyKey: null);
        var jobs = await SendAsync<List<RemoteJob>>(request, opts, ct);
        return (IReadOnlyList<RemoteJob>)(jobs ?? new List<RemoteJob>());
    }

    /// <inheritdoc />
    public async Task SendAckAsync(JobAck ack, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ack);

        var opts = _options.CurrentValue;
        // The job id IS the idempotency key for acks — the central API must
        // treat a re-ack of the same job as a no-op (or a status update), never
        // as a fresh operation.
        var idempotencyKey = string.IsNullOrWhiteSpace(ack.JobId)
            ? NewIdempotencyKey(ack.JobId)
            : $"ack:{ack.JobId}";

        using var request = BuildRequest(HttpMethod.Post, "/api/v1/jobs/ack", opts, idempotencyKey);
        request.Content = SerializeJson(ack);
        await SendNoContentAsync(request, opts, ct);
    }

    /// <inheritdoc />
    public async Task PushBootstrapDataAsync(SyncPackage package, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        var opts = _options.CurrentValue;
        // Each bootstrap push is a fresh idempotent operation; the key is a
        // per-call GUID. Retries within the same call reuse the same key.
        using var request = BuildRequest(HttpMethod.Post, "/api/v1/bootstrap", opts, NewIdempotencyKey("bootstrap"));
        request.Content = SerializeJson(package);
        await SendNoContentAsync(request, opts, ct);
    }

    /// <inheritdoc />
    public async Task SendHeartbeatAsync(AgentHeartbeat heartbeat, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(heartbeat);

        var opts = _options.CurrentValue;
        using var request = BuildRequest(HttpMethod.Post, "/api/v1/agents/heartbeat", opts, NewIdempotencyKey("hb"));
        request.Content = SerializeJson(heartbeat);
        await SendNoContentAsync(request, opts, ct);
    }

    /// <summary>
    /// Build a stable idempotency key for non-ack POSTs. The same call (and any
    /// Polly retries against the same <see cref="HttpRequestMessage"/>) reuses
    /// the value, so the central API sees one logical operation.
    /// </summary>
    private static string NewIdempotencyKey(string hint) =>
        $"{hint}:{Guid.NewGuid():N}";

    private static HttpContent SerializeJson<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }

    private HttpRequestMessage BuildRequest(HttpMethod method, string path, CentralApiOptions opts, string? idempotencyKey)
    {
        var request = new HttpRequestMessage(method, path);

        var baseUrl = (opts.BaseUrl ?? string.Empty).TrimEnd('/');
        if (!string.IsNullOrEmpty(baseUrl) && _http.BaseAddress is null)
        {
            _http.BaseAddress = new Uri(baseUrl + "/", UriKind.Absolute);
        }

        if (!string.IsNullOrEmpty(opts.Jwt))
        {
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + opts.Jwt);
        }

        request.Headers.TryAddWithoutValidation("Accept", "application/json");
        request.Headers.TryAddWithoutValidation("User-Agent", "ErpBridge-Agent/1.0");

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.Headers.TryAddWithoutValidation(IdempotencyKeyHeader, idempotencyKey);
        }

        _logger.LogDebug("Prepared {Method} {Path} (timeout={Timeout}s, hasJwt={HasJwt}, hasIdempotencyKey={HasKey})",
            method, path, opts.TimeoutSeconds, !string.IsNullOrEmpty(opts.Jwt), !string.IsNullOrEmpty(idempotencyKey));

        return request;
    }

    private async Task<T?> SendAsync<T>(HttpRequestMessage request, CentralApiOptions opts, CancellationToken ct)
        where T : class
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(opts.TimeoutSeconds));

        try
        {
            using var response = await _http.SendAsync(request, timeout.Token).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Central API call {Path} timed out after {Timeout}s", request.RequestUri, opts.TimeoutSeconds);
            throw;
        }
    }

    /// <summary>
    /// Like <see cref="SendAsync{T}"/> but reports a 404 as a distinct outcome
    /// (so the caller can return a structured "not found" result instead of a
    /// generic deserialization error). Used by <see cref="ValidateLicenseAsync"/>.
    /// </summary>
    private async Task<(T? Result, bool NotFound)> SendAsyncAllowNotFound<T>(
        HttpRequestMessage request, CentralApiOptions opts, CancellationToken ct)
        where T : class
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(opts.TimeoutSeconds));

        try
        {
            using var response = await _http.SendAsync(request, timeout.Token).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return (null, true);
            }
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return (null, false);
            }
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<T>(JsonOptions, timeout.Token).ConfigureAwait(false);
            return (body, false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Central API call {Path} timed out after {Timeout}s", request.RequestUri, opts.TimeoutSeconds);
            throw;
        }
    }

    private async Task SendNoContentAsync(HttpRequestMessage request, CentralApiOptions opts, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(opts.TimeoutSeconds));

        try
        {
            using var response = await _http.SendAsync(request, timeout.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Central API call {Path} timed out after {Timeout}s", request.RequestUri, opts.TimeoutSeconds);
            throw;
        }
    }
}
