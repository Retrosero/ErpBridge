namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Log Merkezi L3f (plan D11): sampled heartbeat history of an agent — a row when status, version, sync result or error
/// changed, otherwise at most one every 15 minutes. Gaps between rows are the agent's offline periods.
/// </summary>
public sealed class AgentHeartbeatLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AgentId { get; set; }
    public Guid TenantId { get; set; }
    public DateTimeOffset RecordedAtUtc { get; set; }
    public long RecordedAtMs { get; set; }
    public string? Status { get; set; }
    public int QueueDepth { get; set; }
    public string? AppVersion { get; set; }
    public string? HostKind { get; set; }
    public DateTimeOffset? LastSyncAtUtc { get; set; }
    public string? LastSyncResult { get; set; }
    public string? LastError { get; set; }
}
