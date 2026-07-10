namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// An outbound HTTPS URL we POST to when something interesting happens for
/// the tenant (today: a job reaches a terminal state). The
/// <see cref="SigningSecret"/> is used to compute an HMAC-SHA256 signature
/// over the JSON body so the receiver can verify the call came from us.
/// Unlike <see cref="ApiKey"/>, the secret must be retrievable in cleartext
/// at send time, so it is stored as-is (DB column encrypted at rest by
/// Coolify/Postgres TDE in production).
/// </summary>
public sealed class WebhookEndpoint
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Absolute https URL we POST to. http is rejected at creation time.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// HMAC-SHA256 secret shared with the receiver. Returned in cleartext
    /// exactly once at creation (and on rotate); subsequent GETs expose
    /// only <see cref="SigningSecretPrefix"/>.
    /// </summary>
    public string SigningSecret { get; set; } = string.Empty;

    /// <summary>First 8 chars of the plaintext secret, for at-a-glance identification.</summary>
    public string SigningSecretPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Event subscriptions. Empty array = subscribed to all supported events
    /// (today: <c>job.succeeded</c>, <c>job.failed</c>). The array lets us
    /// add a third event type without a schema change.
    /// </summary>
    public string[] SubscribedEvents { get; set; } = Array.Empty<string>();

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastDeliveredAtUtc { get; set; }

    public ICollection<WebhookDelivery> Deliveries { get; set; } = new List<WebhookDelivery>();
}