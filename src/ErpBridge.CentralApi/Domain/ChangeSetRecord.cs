namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Persisted bundle from the agent's trigger-based change-set reader. One
/// row per (tenant, sourceDatabase, table) tuple, with the jsonb payload
/// carrying the new / changed / deleted chunks for that table. The unique
/// constraint on (TenantId, SourceDatabase, TableName, LastTriggerRecNo)
/// makes a duplicate push a no-op so a network-retry storm cannot store
/// the same bundle twice.
/// </summary>
public sealed class ChangeSetRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>Mikro database the change-set came from (e.g. <c>MikroDB_V15_02</c>).</summary>
    public string SourceDatabase { get; set; } = string.Empty;

    /// <summary>Tracked table name (e.g. <c>STOKLAR</c>, <c>CARI_HESAPLAR</c>).</summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>Stable Mikro-side numeric identifier (matches ErpBridge's <c>TrackedTableCatalog</c>).</summary>
    public int TabloId { get; set; }

    /// <summary>Highest <c>TriggerRECno</c> the agent observed for this table. The unique-key component that makes the row idempotent.</summary>
    public long LastTriggerRecNo { get; set; }

    /// <summary>JSON-serialised <see cref="ErpBridge.Shared.SyncTableChangeSet"/>.</summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>When the agent pulled the change-set from Mikro.</summary>
    public DateTimeOffset PulledAtUtc { get; set; }

    /// <summary>When the central API received it.</summary>
    public DateTimeOffset ReceivedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
