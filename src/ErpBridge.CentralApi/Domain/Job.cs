namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Lifecycle states of a job in the central queue. Stored as an integer in
/// PostgreSQL (0..4) to keep the table portable.
/// </summary>
public enum JobStatus
{
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
    DeadLetter = 4,
}

/// <summary>
/// A unit of work enqueued for an agent (e.g. write a sales order into Mikro).
/// The unique (TenantId, DocumentType, ExternalId) index gives idempotency:
/// re-enqueuing the same payload will not create a duplicate row.
/// </summary>
public sealed class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>The id assigned by the upstream (mobile/web) app; used for idempotency.</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>e.g. "sales_order", "invoice". Drives dispatch on the agent side.</summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Target ERP kind (e.g. <c>Mikro</c>, <c>Logo</c>). Faz 20 — lets a
    /// multi-ERP tenant route a write to the right adapter; defaults to
    /// <c>Mikro</c> for rows enqueued before the column existed.
    /// </summary>
    public string ErpType { get; set; } = "Mikro";

    /// <summary>Document-typed payload serialized as JSON (stored as jsonb in PostgreSQL).</summary>
    public string PayloadJson { get; set; } = "{}";

    public JobStatus Status { get; set; } = JobStatus.Pending;

    public int RetryCount { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset EnqueuedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAtUtc { get; set; }
}