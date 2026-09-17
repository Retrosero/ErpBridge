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

    // Log Merkezi L3f: all nullable — an older agent sends none of them and stays a first-class citizen.

    /// <summary>Agent build from the last heartbeat.</summary>
    public string? LastAppVersion { get; set; }

    /// <summary><c>service</c> or <c>ui</c>: which host sent the last heartbeat.</summary>
    public string? LastHostKind { get; set; }

    /// <summary>The ERP this agent serves, as it reported it.</summary>
    public string? LastErpKind { get; set; }

    /// <summary>The ERP edition the adapter probed (e.g. <c>V15</c>).</summary>
    public string? LastErpVersion { get; set; }

    /// <summary>When the agent last finished a sync round successfully — not when it last said hello.</summary>
    public DateTimeOffset? LastSyncAtUtc { get; set; }

    /// <summary><c>ok</c> or <c>failed</c>: how the last sync round ended.</summary>
    public string? LastSyncResult { get; set; }

    /// <summary>The stable code behind <see cref="LastError"/>.</summary>
    public string? LastErrorCode { get; set; }

    /// <summary>
    /// The last error the agent reported, masked. It used to be sent on every heartbeat and thrown away; a
    /// support call then had nothing to start from but "the agent says it is running".
    /// </summary>
    public string? LastError { get; set; }

    public ICollection<AgentCompanyAssignment> CompanyAssignments { get; set; } = new List<AgentCompanyAssignment>();
}
