namespace ErpBridge.Core.Domain;

/// <summary>Periodic heartbeat sent from the agent to the central API.</summary>
public sealed class AgentHeartbeat
{
    public string AgentId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? LastSyncAtUtc { get; set; }
    public int QueueDepth { get; set; }
    public string? LastError { get; set; }

    // Log Merkezi L3f (optional; older servers ignore them).
    public string? AppVersion { get; set; }

    /// <summary><c>service</c> or <c>ui</c>.</summary>
    public string? HostKind { get; set; }

    /// <summary><c>OK</c>, <c>FAILED</c>, or null before the first sync.</summary>
    public string? LastSyncResult { get; set; }
}
