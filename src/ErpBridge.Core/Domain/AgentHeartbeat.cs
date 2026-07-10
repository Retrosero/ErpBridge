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
}
