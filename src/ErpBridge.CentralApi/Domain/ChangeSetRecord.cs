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

    /// <summary>Source ERP kind (e.g. <c>Mikro</c>, <c>Logo</c>). Faz 20 — ERP-neutral.</summary>
    public string ErpType { get; set; } = "Mikro";

    /// <summary>Source database the change-set came from (e.g. <c>MikroDB_V15_02</c>).</summary>
    public string SourceDatabase { get; set; } = string.Empty;

    /// <summary>
    /// Stable ERP-neutral table identifier minted by the adapter's catalog
    /// (Mikro: the physical table name; Logo: a firm/period-independent
    /// logical name). Replaces the old Mikro-only <c>TabloId</c> int.
    /// </summary>
    public string TableKey { get; set; } = string.Empty;

    /// <summary>Physical table the change was captured from (e.g. <c>STOKLAR</c>).</summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>Highest insert/update change-log sequence the agent observed for this table. Part of the idempotency key.</summary>
    public long LastTriggerRecNo { get; set; }

    /// <summary>
    /// Highest delete change-log sequence the agent observed for this table.
    /// Separate from <see cref="LastTriggerRecNo"/> because deletes come from
    /// their own shadow sequence — a delete-only cycle advances this while
    /// <see cref="LastTriggerRecNo"/> stays put, so both are needed to tell
    /// one bundle from the next.
    /// </summary>
    public long LastDeleteRecNo { get; set; }

    /// <summary>JSON-serialised <see cref="ErpBridge.Shared.SyncTableChangeSet"/>.</summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>When the agent pulled the change-set from Mikro.</summary>
    public DateTimeOffset PulledAtUtc { get; set; }

    /// <summary>When the central API received it.</summary>
    public DateTimeOffset ReceivedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
