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
    public async Task<AgentRegistrationResult> RegisterAgentAsync(string licenseKey, string machineId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            return new AgentRegistrationResult { Success = false, ErrorCode = "MISSING_LICENSE_KEY", ErrorMessage = "Lisans anahtarı boş olamaz." };
        }
        if (string.IsNullOrWhiteSpace(machineId))
        {
            return new AgentRegistrationResult { Success = false, ErrorCode = "MISSING_MACHINE_ID", ErrorMessage = "Makine kimliği boş olamaz." };
        }

        var opts = _options.CurrentValue;
        // Registration is anonymous (no JWT yet) and idempotent per (license, machine) pair —
        // reuse the same key if the caller retries.
        var idemKey = $"register:{licenseKey}:{machineId}";
        using var request = BuildRequest(HttpMethod.Post, "/api/v1/agents/register", opts, idempotencyKey: idemKey);
        request.Content = SerializeJson(new { licenseKey, machineId });

        try
        {
            using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<RegisterResponseDto>(JsonOptions, ct).ConfigureAwait(false);
                if (body is null || string.IsNullOrWhiteSpace(body.Jwt))
                {
                    return new AgentRegistrationResult { Success = false, ErrorCode = "EMPTY_RESPONSE", ErrorMessage = "Central API boş cevap döndü." };
                }
                return new AgentRegistrationResult
                {
                    Success = true,
                    Jwt = body.Jwt,
                    AgentId = body.AgentId,
                    TenantId = body.TenantId,
                    ExpiresAtUtc = body.ExpiresAtUtc,
                };
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new AgentRegistrationResult { Success = false, ErrorCode = "LICENSE_NOT_FOUND", ErrorMessage = "Lisans anahtarı tanınmadı." };
            }
            if (response.StatusCode == HttpStatusCode.Gone)
            {
                return new AgentRegistrationResult { Success = false, ErrorCode = "LICENSE_EXPIRED", ErrorMessage = "Lisans süresi dolmuş veya pasif." };
            }
            var errBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            _logger.LogWarning("Register failed: {Status} {Body}", response.StatusCode, errBody);
            return new AgentRegistrationResult
            {
                Success = false,
                ErrorCode = $"HTTP_{(int)response.StatusCode}",
                ErrorMessage = $"Central API hata döndü: {(int)response.StatusCode} {response.ReasonPhrase}",
            };
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Register timed out after {Timeout}s", opts.TimeoutSeconds);
            return new AgentRegistrationResult { Success = false, ErrorCode = "TIMEOUT", ErrorMessage = "Zaman aşımı." };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Register network error");
            return new AgentRegistrationResult { Success = false, ErrorCode = "NETWORK", ErrorMessage = ex.Message };
        }
    }

    private sealed class RegisterResponseDto
    {
        [System.Text.Json.Serialization.JsonPropertyName("agentId")]
        public Guid AgentId { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("jwt")]
        public string Jwt { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonPropertyName("tenantId")]
        public Guid TenantId { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("expiresAtUtc")]
        public DateTimeOffset? ExpiresAtUtc { get; set; }
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
        // The central API expects a BootstrapRequest envelope: { sourceDatabase,
        // pulledAtUtc, payload: <SyncPackage> }. The agent previously sent the
        // raw SyncPackage, which the server deserialized with `body.Payload`
        // null and silently stored an empty `{}` jsonb — the push "succeeded"
        // but no data actually landed. Wrap explicitly here so the payload
        // survives the round-trip.
        var envelope = new BootstrapRequestEnvelope(
            SourceDatabase: package.SourceDatabase,
            PulledAtUtc: new DateTimeOffset(package.PulledAtUtc, TimeSpan.Zero),
            Payload: package);
        // Each bootstrap push is a fresh idempotent operation; the key is a
        // per-call GUID. Retries within the same call reuse the same key.
        using var request = BuildRequest(HttpMethod.Post, "/api/v1/bootstrap", opts, NewIdempotencyKey("bootstrap"));
        request.Content = SerializeJson(envelope);
        await SendNoContentAsync(request, opts, ct, classifyBootstrapFailure: true);
    }

    /// <summary>
    /// Wire shape for <c>POST /api/v1/bootstrap</c>. Mirrors
    /// <c>ErpBridge.CentralApi.Contracts.BootstrapRequest</c> — we cannot
    /// reference the central API project (it would invert the dependency
    /// arrow), so the shape is duplicated here and pinned by contract.
    /// </summary>
    private sealed record BootstrapRequestEnvelope(
        [property: System.Text.Json.Serialization.JsonPropertyName("sourceDatabase")] string SourceDatabase,
        [property: System.Text.Json.Serialization.JsonPropertyName("pulledAtUtc")] DateTimeOffset PulledAtUtc,
        [property: System.Text.Json.Serialization.JsonPropertyName("payload")] object Payload)
    {
        // Explicit ctor so the call site can use the camelCase property aliases.
        // Without this, the record's primary ctor parameters must be the
        // PascalCase property names (SourceDatabase, PulledAtUtc, Payload),
        // which is less readable at the push call site.
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

    private async Task SendNoContentAsync(
        HttpRequestMessage request,
        CentralApiOptions opts,
        CancellationToken ct,
        bool classifyBootstrapFailure = false)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(opts.TimeoutSeconds));

        try
        {
            using var response = await _http.SendAsync(request, timeout.Token).ConfigureAwait(false);
            if (classifyBootstrapFailure && !response.IsSuccessStatusCode)
            {
                var (errorCode, message) = await ReadApiErrorAsync(response, timeout.Token).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500)
                {
                    throw new TransientPushException($"Central API returned {errorCode}: {message}");
                }

                throw new BootstrapPermanentPushException(errorCode, message);
            }
            response.EnsureSuccessStatusCode();
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Central API call {Path} timed out after {Timeout}s", request.RequestUri, opts.TimeoutSeconds);
            throw;
        }
    }

    private static async Task<(string ErrorCode, string Message)> ReadApiErrorAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        var fallbackCode = $"HTTP_{(int)response.StatusCode}";
        var fallbackMessage = response.ReasonPhrase ?? "Central API isteği reddetti.";
        var raw = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(raw)) return (fallbackCode, fallbackMessage);

        try
        {
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;
            var errorCode = root.TryGetProperty("errorCode", out var code)
                && code.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(code.GetString())
                ? code.GetString()!
                : fallbackCode;
            var message = root.TryGetProperty("message", out var text)
                && text.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(text.GetString())
                ? text.GetString()!
                : fallbackMessage;
            return (errorCode, message);
        }
        catch (JsonException)
        {
            return (fallbackCode, fallbackMessage);
        }
    }
}
