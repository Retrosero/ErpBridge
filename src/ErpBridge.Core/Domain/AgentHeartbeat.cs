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

    // Log Merkezi L3f: all optional, so an older server (and an older agent) keeps working unchanged.

    /// <summary>The agent build, so the panel can tell which machines are behind.</summary>
    public string? AppVersion { get; set; }

    /// <summary>Which host is speaking: <c>service</c> (Windows service) or <c>ui</c> (the desktop app).</summary>
    public string? HostKind { get; set; }

    /// <summary>The ERP this agent serves (<c>Mikro</c>, <c>Logo</c>, ...).</summary>
    public string? ErpKind { get; set; }

    /// <summary>The ERP version as the adapter probed it (e.g. <c>V15</c>), when known.</summary>
    public string? ErpVersion { get; set; }

    /// <summary><c>ok</c> or <c>failed</c> — the outcome of the last sync round, not of this heartbeat.</summary>
    public string? LastSyncResult { get; set; }

    /// <summary>The stable code behind <see cref="LastError"/>, for grouping in the panel.</summary>
    public string? LastErrorCode { get; set; }
}
