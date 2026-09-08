namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Durable ERP -> Android event queue. Android owns the cursor, so rows are
/// append-only and are not marked processed per device.
/// </summary>
public sealed class MobileSyncQueueItem
{
    public long Sequence { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string SourceDatabase { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string RecordKey { get; set; } = string.Empty;
    public string? SourceRecordKey { get; set; }
    public long TriggerRecNo { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
