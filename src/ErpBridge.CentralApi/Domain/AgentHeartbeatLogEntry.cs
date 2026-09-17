namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Log Merkezi L3f: a short history of what the agents reported, so "since when has this been failing?" has an
/// answer. A row is written only when something meaningful changed or when the last row is more than
/// <see cref="MinInterval"/> old — a heartbeat every minute must not become 1.440 rows a day per agent.
/// </summary>
public sealed class AgentHeartbeatLogEntry
{
    /// <summary>The quietest an unchanged agent still leaves a trace.</summary>
    public static readonly TimeSpan MinInterval = TimeSpan.FromMinutes(15);

    public long Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AgentId { get; set; }

    public DateTimeOffset ReceivedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public string? Status { get; set; }

    public int QueueDepth { get; set; }

    public DateTimeOffset? LastSyncAtUtc { get; set; }

    public string? LastSyncResult { get; set; }

    public string? LastErrorCode { get; set; }

    /// <summary>Masked by the agent and again on the way in.</summary>
    public string? LastError { get; set; }

    public string? AppVersion { get; set; }

    public string? HostKind { get; set; }

    public string? ErpKind { get; set; }

    public string? ErpVersion { get; set; }

    /// <summary>
    /// What makes two heartbeats "the same": everything a reader would notice. The queue depth is deliberately
    /// in here — a queue that starts growing is exactly the change worth a row.
    /// </summary>
    public string Signature() => string.Join('|',
        Status, QueueDepth, LastSyncResult, LastErrorCode, LastError, AppVersion, HostKind, ErpKind, ErpVersion);
}
