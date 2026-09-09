namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Append-only audit row for one direction of one change-set bundle. Faz 15
/// adds this table so the operator can answer "what did the agent send for
/// STOKLAR at 14:23 last Tuesday?" — the existing
/// <see cref="ChangeSetRecord"/> snapshot only keeps the most recent bundle.
///
/// <para>
/// Three rows are written per accepted bundle (one each for <c>new</c>,
/// <c>changed</c>, and <c>deleted</c>) so the operator can audit each
/// direction independently. The unique index on
/// <c>(TenantId, IdempotencyKey, Direction)</c> makes a retry of the same
/// bundle a no-op on the audit side too — the second insert swallows a
/// 23505 SQLState unique violation.
/// </para>
/// </summary>
public sealed class ChangeSetAuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>Mikro database the change-set came from (e.g. <c>MikroDB_V15_02</c>).</summary>
    public string SourceDatabase { get; set; } = string.Empty;

    /// <summary>Source ERP kind (e.g. <c>Mikro</c>, <c>Logo</c>). Faz 20 — ERP-neutral.</summary>
    public string ErpType { get; set; } = "Mikro";

    /// <summary>ERP-neutral table identifier minted by the adapter's catalog. Replaces the old Mikro-only <c>TabloId</c> int.</summary>
    public string TableKey { get; set; } = string.Empty;

    /// <summary>Physical table the change was captured from (e.g. <c>STOKLAR</c>).</summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>One of <c>new</c>, <c>changed</c>, <c>deleted</c>.</summary>
    public string Direction { get; set; } = string.Empty;

    /// <summary>Lowest <c>TriggerRECno</c> in this direction's chunk (0 when the chunk is empty).</summary>
    public long FirstTriggerRecNo { get; set; }

    /// <summary>Highest <c>TriggerRECno</c> in this direction's chunk. The agent uses this to advance its watermark.</summary>
    public long LastTriggerRecNo { get; set; }

    /// <summary>Number of rows included in this direction's payload.</summary>
    public int RowCount { get; set; }

    /// <summary>Direction's payload as a JSON string (jsonb in PostgreSQL).</summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>SHA-256 of <see cref="PayloadJson"/> in lowercase hex. Lets the audit viewer skip re-hashing on the read path.</summary>
    public string PayloadSha256 { get; set; } = string.Empty;

    /// <summary>When the agent pulled the change-set from Mikro.</summary>
    public DateTimeOffset PulledAtUtc { get; set; }

    /// <summary>When the central API received the bundle.</summary>
    public DateTimeOffset ReceivedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Agent id (the JWT <c>sub</c>) that pushed the bundle. Empty when the auth scheme does not expose one.</summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>Idempotency key for the bundle. The unique index on (TenantId, IdempotencyKey, Direction) rejects duplicate retries.</summary>
    public string IdempotencyKey { get; set; } = string.Empty;
}
