using System.Net;
using System.Net.Http.Headers;
using ErpBridge.CentralApi.Webhooks;

namespace ErpBridge.CentralApi.Storage;

/// <summary>What became of one outside download.</summary>
public enum FetchStatus
{
    /// <summary>200: the body was written to the destination.</summary>
    Ok,

    /// <summary>304: the copy the server has is still the source's.</summary>
    NotModified,

    /// <summary>404 or 410: the source is gone.</summary>
    NotFound,

    /// <summary>The body is bigger than the limit; what was written is to be thrown away.</summary>
    TooLarge,

    /// <summary>Refused address, network error, timeout, too many redirects or another HTTP status.</summary>
    Failed,
}

/// <summary>
/// One outside download's limits and its conditional headers (<c>If-None-Match</c> / <c>If-Modified-Since</c>, from the
/// last download).
/// </summary>
public sealed record FetchRequest(long MaxBytes, TimeSpan Timeout, string? ETag = null, string? LastModified = null)
{
    /// <summary>A product picture of the XML feed: 10 MB in 20 s (GOAL_DEPOLAMA_R2 §4).</summary>
    public static FetchRequest Image(string? etag = null, string? lastModified = null) =>
        new(10L * 1024 * 1024, TimeSpan.FromSeconds(20), etag, lastModified);

    /// <summary>The XML feed itself: 100 MB in 120 s.</summary>
    public static FetchRequest Feed() => new(100L * 1024 * 1024, TimeSpan.FromSeconds(120));
}

/// <summary>
/// The outcome; <see cref="ETag"/>/<see cref="LastModified"/> are the source's (on 304 the ones sent, unless it sent new
/// ones). <see cref="Reason"/> is short and never holds the address (it may carry a query with a key).
/// </summary>
public sealed record FetchResult(FetchStatus Status, long Bytes = 0, string? ETag = null, string? LastModified = null, string? Reason = null, int? HttpStatus = null);

/// <summary>Downloads from outside addresses the company gave (the XML feed and its pictures); tests replace it.</summary>
public interface IExternalFetcher
{
    /// <summary>GETs <paramref name="url"/> into <paramref name="destination"/> (written only for a 200).</summary>
    Task<FetchResult> GetAsync(Uri url, FetchRequest request, Stream destination, CancellationToken ct);
}

/// <summary>
/// The server's only way out to an address a company typed in (GOAL_DEPOLAMA_R2 S7, SSRF): only http/https without user
/// info, never a local host name, never a literal address that is not public (<see cref="WebhookTargetValidator.IsPublicAddress"/>).
/// The client's handler (registered in <c>Program</c>) connects through <see cref="WebhookTargetValidator.ConnectPublicAsync"/>:
/// the name is resolved, <b>every</b> address checked and the socket pinned to a checked one, so a second DNS answer
/// cannot rebind it; no proxy; redirects are not followed by the handler but here, at most <see cref="MaxRedirects"/>,
/// each target checked again. One timeout covers the whole request, redirects and body included; the byte limit is
/// counted while streaming (a gzip body counts unpacked).
/// </summary>
public sealed class SafeHttpFetcher : IExternalFetcher
{
    /// <summary>The named <see cref="HttpClient"/> (its handler pins the connection to a checked address).</summary>
    public const string ClientName = "ExternalFetcher";

    public const int MaxRedirects = 3;

    private readonly HttpClient _http;
    private readonly ILogger<SafeHttpFetcher> _logger;

    public SafeHttpFetcher(HttpClient http, ILogger<SafeHttpFetcher> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>The handler the named client must use: no proxy, no automatic redirect, connections pinned to public addresses.</summary>
    public static SocketsHttpHandler CreateHandler() => new()
    {
        UseProxy = false,
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.All,
        ConnectCallback = WebhookTargetValidator.ConnectPublicAsync,
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        ConnectTimeout = TimeSpan.FromSeconds(10),
    };

    /// <summary>
    /// Null when <paramref name="url"/> may be asked; else why not. The host name's addresses are checked again at connect
    /// time by the handler; a literal address is refused here, before any connection.
    /// </summary>
    public static string? CheckAddress(Uri url)
    {
        if (!url.IsAbsoluteUri || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)) return "only http/https";
        if (!string.IsNullOrEmpty(url.UserInfo)) return "user info in address";
        var host = url.IdnHost.TrimEnd('.');
        if (host.Length == 0
            || string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".local", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase))
            return "local host name";
        if (url.HostNameType is UriHostNameType.IPv4 or UriHostNameType.IPv6
            && IPAddress.TryParse(url.DnsSafeHost, out var literal) && !WebhookTargetValidator.IsPublicAddress(literal))
            return "non-public address";
        return null;
    }

    public async Task<FetchResult> GetAsync(Uri url, FetchRequest request, Stream destination, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(request.Timeout);
        var current = url;
        try
        {
            for (var hop = 0; ; hop++)
            {
                if (CheckAddress(current) is { } refused) return Fail(refused);
                using var message = new HttpRequestMessage(HttpMethod.Get, current);
                if (!string.IsNullOrEmpty(request.ETag) && EntityTagHeaderValue.TryParse(request.ETag, out var tag))
                    message.Headers.IfNoneMatch.Add(tag);
                if (!string.IsNullOrEmpty(request.LastModified) && DateTimeOffset.TryParse(request.LastModified, System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeUniversal, out var since))
                    message.Headers.IfModifiedSince = since;

                using var response = await _http.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                var status = (int)response.StatusCode;
                if (status is 301 or 302 or 303 or 307 or 308)
                {
                    if (hop >= MaxRedirects) return Fail("too many redirects", status);
                    if (response.Headers.Location is not { } location) return Fail("redirect without location", status);
                    current = location.IsAbsoluteUri ? location : new Uri(current, location);
                    continue;
                }

                var etag = Clip(response.Headers.ETag?.ToString(), Domain.XmlImage.MaxETagLength);
                var lastModified = response.Content.Headers.LastModified?.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
                if (response.StatusCode == HttpStatusCode.NotModified)
                    return new FetchResult(FetchStatus.NotModified, 0, etag ?? request.ETag, lastModified ?? request.LastModified, HttpStatus: status);
                if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                    return new FetchResult(FetchStatus.NotFound, Reason: "not found", HttpStatus: status);
                if (response.StatusCode != HttpStatusCode.OK) return Fail("HTTP " + status, status);
                if (response.Content.Headers.ContentLength is { } declared && declared > request.MaxBytes)
                    return new FetchResult(FetchStatus.TooLarge, Reason: "too large", HttpStatus: status);

                await using var body = await response.Content.ReadAsStreamAsync(timeout.Token);
                var buffer = new byte[81920];
                long total = 0;
                int read;
                while ((read = await body.ReadAsync(buffer, timeout.Token)) > 0)
                {
                    total += read;
                    if (total > request.MaxBytes) return new FetchResult(FetchStatus.TooLarge, Reason: "too large", HttpStatus: status);
                    await destination.WriteAsync(buffer.AsMemory(0, read), timeout.Token);
                }
                return new FetchResult(FetchStatus.Ok, total, etag, lastModified, HttpStatus: status);
            }
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return Fail("timeout");
        }
        catch (HttpRequestException ex)
        {
            // The host only: an address may carry a key in its query.
            _logger.LogInformation("Outside download from {Host} failed: {Reason}", current.Host, ex.HttpRequestError);
            return Fail("network: " + ex.HttpRequestError);
        }
        catch (IOException)
        {
            return Fail("connection closed");
        }
    }

    private static FetchResult Fail(string reason, int? status = null) => new(FetchStatus.Failed, Reason: reason, HttpStatus: status);

    private static string? Clip(string? value, int max) => value is null || value.Length > max ? null : value;
}
