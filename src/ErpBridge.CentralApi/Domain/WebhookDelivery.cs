namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Lifecycle of a webhook delivery row. Stored as int for portability —
/// matches the convention used by <see cref="JobStatus"/>.
/// </summary>
public enum WebhookDeliveryStatus
{
    /// <summary>Newly enqueued; dispatcher hasn't tried yet.</summary>
    Pending = 0,

    /// <summary>HTTP 2xx received from the receiver.</summary>
    Delivered = 1,

    /// <summary>Last attempt got a non-2xx (or network error). Retry-eligible per the dispatcher's backoff schedule.</summary>
    Failed = 2,

    /// <summary>Exhausted retries. Kept for audit; surface in admin UI.</summary>
    DeadLetter = 3,
}

/// <summary>
/// One row per outbound webhook attempt sequence. The dispatcher creates one
/// row per event and increments <see cref="AttemptCount"/> on every retry;
/// terminal states (<see cref="WebhookDeliveryStatus.Delivered"/>,
/// <see cref="WebhookDeliveryStatus.DeadLetter"/>) close the row.
/// </summary>
public sealed class WebhookDelivery
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EndpointId { get; set; }

    public WebhookEndpoint? Endpoint { get; set; }

    /// <summary>Denormalized tenant id for tenant-scoped admin queries without joining to endpoints.</summary>
    public Guid TenantId { get; set; }

    /// <summary>e.g. <c>job.succeeded</c>, <c>job.failed</c>.</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>The originating job, when the event is job-related. Null for other future events.</summary>
    public Guid? JobId { get; set; }

    /// <summary>Serialized JSON body the dispatcher POSTs verbatim.</summary>
    public string PayloadJson { get; set; } = "{}";

    public WebhookDeliveryStatus Status { get; set; } = WebhookDeliveryStatus.Pending;

    public int AttemptCount { get; set; }

    public DateTimeOffset? LastAttemptAtUtc { get; set; }

    public int? LastResponseCode { get; set; }

    public string? LastError { get; set; }

    /// <summary>Earliest UTC the dispatcher is allowed to retry. Null while the row is in a terminal state.</summary>
    public DateTimeOffset? NextRetryAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}