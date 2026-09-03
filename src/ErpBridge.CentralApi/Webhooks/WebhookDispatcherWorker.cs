using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Webhooks;

/// <summary>
/// Background hosted service that drains the <c>webhook_deliveries</c> table.
/// On each tick:
/// <list type="number">
///   <item><description>Load up to <see cref="BatchSize"/> rows in Pending or Failed-with-NextRetryAtUtc-now state.</description></item>
///   <item><description>POST the JSON body to the endpoint URL.</description></item>
///   <item><description>Stamp <c>ErpBridge-Timestamp</c> and <c>ErpBridge-Signature</c> headers (HMAC-SHA256).</description></item>
///   <item><description>Update the row: Delivered on 2xx, Failed with the next backoff slot, DeadLetter past <see cref="MaxAttempts"/>.</description></item>
/// </list>
/// Exponential backoff: 1m, 5m, 30m, 2h, 12h, then dead-letter.
/// </summary>
public sealed class WebhookDispatcherWorker : BackgroundService
{
    /// <summary>Seconds between dispatcher ticks.</summary>
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(5);

    /// <summary>Max rows drained per tick to keep request latency bounded.</summary>
    private const int BatchSize = 25;

    /// <summary>Max attempts before the row goes to DeadLetter.</summary>
    private const int MaxAttempts = 6;

    /// <summary>HTTP client timeout per attempt.</summary>
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(10);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>Backoff schedule in seconds. Index = <see cref="WebhookDelivery.AttemptCount"/>.</summary>
    private static readonly int[] BackoffSeconds = new[]
    {
        60,      // 1 min   after attempt 1
        300,     // 5 min   after attempt 2
        1_800,   // 30 min  after attempt 3
        7_200,   // 2 hour  after attempt 4
        43_200,  // 12 hour after attempt 5
    };

    private readonly IServiceProvider _services;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<WebhookDispatcherWorker> _logger;

    public WebhookDispatcherWorker(
        IServiceProvider services,
        IHttpClientFactory httpFactory,
        ILogger<WebhookDispatcherWorker> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run on a dedicated background thread so the dispatcher tick doesn't
        // compete with the request-handling thread pool.
        _ = Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DrainOnceAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // A single failed tick must not kill the worker. Log and
                    // continue; the next tick will retry.
                    _logger.LogWarning(ex, "Webhook dispatcher tick failed.");
                }

                try { await Task.Delay(TickInterval, stoppingToken).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
            }
        }, stoppingToken);

        // Park the hosted service thread until shutdown.
        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
    }

    private async Task DrainOnceAsync(CancellationToken ct)
    {
        // Each tick uses a fresh DI scope so the DbContext isn't shared across
        // ticks — the in-memory test provider is keyed off the scope too.
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();

        var now = DateTimeOffset.UtcNow;
        var dueRows = await db.WebhookDeliveries
            .Where(d => d.Endpoint!.IsActive
                && ((d.Status == WebhookDeliveryStatus.Pending)
                    || (d.Status == WebhookDeliveryStatus.Failed
                        && d.NextRetryAtUtc != null
                        && d.NextRetryAtUtc <= now)))
            .OrderBy(d => d.CreatedAtUtc)
            .Take(BatchSize)
            .ToListAsync(ct);

        foreach (var row in dueRows)
        {
            ct.ThrowIfCancellationRequested();
            await DeliverAsync(db, row, ct).ConfigureAwait(false);
        }
    }

    private async Task DeliverAsync(CentralApiDbContext db, WebhookDelivery row, CancellationToken ct)
    {
        var endpoint = await db.WebhookEndpoints.AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == row.EndpointId, ct);
        if (endpoint is null || !endpoint.IsActive)
        {
            // Endpoint was deleted/disabled between enqueue and dispatch —
            // mark the row Delivered? No — we'd be lying. Mark Failed with
            // no retry so it shows up as a one-off DeadLetter-ish row.
            row.Status = WebhookDeliveryStatus.Failed;
            row.LastError = "Endpoint missing or inactive.";
            row.AttemptCount += 1;
            row.LastAttemptAtUtc = DateTimeOffset.UtcNow;
            row.NextRetryAtUtc = null;
            await db.SaveChangesAsync(ct);
            return;
        }

        row.AttemptCount += 1;
        row.LastAttemptAtUtc = DateTimeOffset.UtcNow;

        try
        {
            if (!WebhookTargetValidator.TryParsePublicHttpsUri(endpoint.Url, out var targetUri, out var targetError))
            {
                await MarkFailedAsync(db, row, "Unsafe webhook target: " + targetError);
                await db.SaveChangesAsync(ct);
                return;
            }

            var resolvedTargetError = await WebhookTargetValidator.ValidateResolvedTargetAsync(targetUri!, ct);
            if (resolvedTargetError is not null)
            {
                await MarkFailedAsync(db, row, resolvedTargetError);
                await db.SaveChangesAsync(ct);
                return;
            }

            using var http = _httpFactory.CreateClient("WebhookDispatcher");
            http.Timeout = HttpTimeout;

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signature = ComputeSignature(endpoint.SigningSecret, timestamp, row.PayloadJson);

            using var req = new HttpRequestMessage(HttpMethod.Post, targetUri)
            {
                Content = new StringContent(row.PayloadJson, Encoding.UTF8, "application/json"),
            };
            req.Headers.TryAddWithoutValidation("ErpBridge-Timestamp", timestamp);
            req.Headers.TryAddWithoutValidation("ErpBridge-Signature", "sha256=" + signature);
            req.Headers.TryAddWithoutValidation("ErpBridge-Event", row.EventType);
            req.Headers.TryAddWithoutValidation("ErpBridge-Delivery-Id", row.Id.ToString());

            using var resp = await http.SendAsync(req, ct).ConfigureAwait(false);
            row.LastResponseCode = (int)resp.StatusCode;

            if (resp.IsSuccessStatusCode)
            {
                row.Status = WebhookDeliveryStatus.Delivered;
                row.LastError = null;
                row.NextRetryAtUtc = null;

                // Bump the endpoint's LastDeliveredAtUtc so the admin UI
                // can show "last delivered 2 min ago" without scanning the
                // delivery table.
                var ep = await db.WebhookEndpoints.FirstOrDefaultAsync(w => w.Id == endpoint.Id, ct);
                if (ep is not null)
                {
                    ep.LastDeliveredAtUtc = DateTimeOffset.UtcNow;
                }
            }
            else
            {
                await MarkFailedAsync(db, row, $"HTTP {(int)resp.StatusCode}: {resp.ReasonPhrase}");
            }
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            await MarkFailedAsync(db, row, $"Timeout after {HttpTimeout.TotalSeconds}s: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            await MarkFailedAsync(db, row, $"HTTP error: {ex.Message}");
        }
        catch (Exception ex)
        {
            await MarkFailedAsync(db, row, $"Unexpected: {ex.Message}");
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private static async Task MarkFailedAsync(CentralApiDbContext db, WebhookDelivery row, string error)
    {
        row.LastError = error;
        if (row.AttemptCount >= MaxAttempts)
        {
            row.Status = WebhookDeliveryStatus.DeadLetter;
            row.NextRetryAtUtc = null;
        }
        else
        {
            row.Status = WebhookDeliveryStatus.Failed;
            var delaySeconds = BackoffSeconds[Math.Min(row.AttemptCount - 1, BackoffSeconds.Length - 1)];
            row.NextRetryAtUtc = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Compute the HMAC-SHA256 signature. The receiver verifies by
    /// recomputing <c>HMAC(secret, "{timestamp}.{body}")</c> and comparing
    /// to the value after the <c>sha256=</c> prefix.
    /// </summary>
    public static string ComputeSignature(string secret, string timestamp, string body)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(timestamp + "." + body);
        var hash = HMACSHA256.HashData(keyBytes, dataBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
