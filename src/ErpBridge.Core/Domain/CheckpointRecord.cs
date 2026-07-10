namespace ErpBridge.Core.Domain;

/// <summary>
/// Resume-cursor persisted per (tenantId, syncScope) to make incremental reads
/// resumable across agent restarts.
/// </summary>
public sealed class CheckpointRecord
{
    public long Id { get; set; }

    public string TenantId { get; set; } = string.Empty;

    public string SyncScope { get; set; } = string.Empty;

    public DateTime? LastSuccessAt { get; set; }

    public string? LastToken { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
