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

    /// <summary>
    /// License key last presented by this agent at registration. Optional —
    /// an admin can detach a license without deleting the agent row.
    /// </summary>
    public string? LicenseKey { get; set; }

    public DateTimeOffset RegisteredAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastHeartbeatAtUtc { get; set; }

    /// <summary>Free-form status string the agent reports ("ok", "degraded", ...).</summary>
    public string? LastStatus { get; set; }

    public int LastQueueDepth { get; set; }
}