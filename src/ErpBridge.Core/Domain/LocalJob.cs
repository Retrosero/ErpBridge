namespace ErpBridge.Core.Domain;

/// <summary>
/// Lifecycle states for a durable local queue entry.
/// </summary>
public enum LocalJobStatus
{
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
}

/// <summary>
/// Durable local queue entry. Persisted in the <c>local_jobs</c> SQLite table and
/// consumed by the Agent worker poll loop.
/// </summary>
public sealed class LocalJob
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string JobType { get; set; } = string.Empty;

    public string ExternalId { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public LocalJobStatus Status { get; set; } = LocalJobStatus.Pending;

    public int RetryCount { get; set; }

    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
