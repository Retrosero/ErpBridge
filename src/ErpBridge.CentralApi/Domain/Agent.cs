namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A Windows Sync Agent registered against a tenant. The combination of
/// (TenantId, MachineId) is unique — a single physical machine is bound to a
/// single tenant.
/// </summary>
public sealed class Agent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string MachineId { get; set; } = string.Empty;

    public DateTimeOffset RegisteredAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastHeartbeatAtUtc { get; set; }

    /// <summary>Free-form status string the agent reports ("ok", "degraded", ...).</summary>
    public string? LastStatus { get; set; }

    public int LastQueueDepth { get; set; }

    // Log Merkezi L3f — reported by the heartbeat; null until an agent build that sends them.
    public string? AppVersion { get; set; }

    /// <summary><c>service</c> or <c>ui</c>.</summary>
    public string? HostKind { get; set; }
    public DateTimeOffset? LastSyncAtUtc { get; set; }

    /// <summary><c>OK</c> or <c>FAILED</c>.</summary>
    public string? LastSyncResult { get; set; }

    /// <summary>Scrubbed last error the agent reported (it used to be discarded).</summary>
    public string? LastError { get; set; }

    /// <summary>When the last <see cref="AgentHeartbeatLog"/> row was written (history is sampled, not every beat).</summary>
    public DateTimeOffset? LastHeartbeatLoggedAtUtc { get; set; }

    public ICollection<AgentCompanyAssignment> CompanyAssignments { get; set; } = new List<AgentCompanyAssignment>();
}
