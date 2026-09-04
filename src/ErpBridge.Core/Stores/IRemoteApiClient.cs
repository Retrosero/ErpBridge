using ErpBridge.Core.Domain;
using ErpBridge.Erp.Abstractions.Sync;

namespace ErpBridge.Core.Stores;

/// <summary>
/// HTTP client contract for the central SaaS API. Implemented by
/// <c>ErpBridge.RemoteApi.Http.HttpRemoteApiClient</c> (Phase 1) and consumed by
/// the Agent worker poll loop and bootstrap push.
/// </summary>
public interface IRemoteApiClient
{
    /// <summary>Validate a license key with the central API.</summary>
    Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey, CancellationToken ct = default);

    /// <summary>
    /// Register this machine against a license key, minting a JWT the agent will
    /// use for all subsequent authenticated calls (bootstrap push, heartbeat, ...).
    /// </summary>
    Task<AgentRegistrationResult> RegisterAgentAsync(string licenseKey, string machineId, CancellationToken ct = default);

    /// <summary>Fetch pending jobs from the central queue.</summary>
    Task<IReadOnlyList<RemoteJob>> GetPendingJobsAsync(CancellationToken ct = default);

    /// <summary>Acknowledge a job as succeeded/failed.</summary>
    Task SendAckAsync(JobAck ack, CancellationToken ct = default);

    /// <summary>Push bootstrap data (cari/stok/fiyat/...) to the central API.</summary>
    Task PushBootstrapDataAsync(SyncPackage package, CancellationToken ct = default);

    /// <summary>
    /// Returns whether the central API already has a bootstrap snapshot for
    /// this tenant and, when it does, the cursor for an incremental read.
    /// </summary>
    Task<BootstrapRemoteStatus> GetBootstrapStatusAsync(CancellationToken ct = default)
        => Task.FromResult(new BootstrapRemoteStatus(false, null));

    /// <summary>
    /// Long-poll: block until the central API publishes a new bootstrap
    /// package for the caller's tenant, or <paramref name="wait"/> elapses,
    /// or <paramref name="ct"/> is cancelled. The default implementation
    /// short-circuits to "no update" so older servers don't have to ship
    /// the signal endpoint.
    /// </summary>
    Task<BootstrapRemoteSignal> WaitForBootstrapUpdateAsync(
        TimeSpan wait,
        CancellationToken ct = default)
        => Task.FromResult(new BootstrapRemoteSignal(false, null));

    /// <summary>Send a periodic agent heartbeat.</summary>
    Task SendHeartbeatAsync(AgentHeartbeat heartbeat, CancellationToken ct = default);
}

public sealed record BootstrapRemoteStatus(bool HasSnapshot, DateTimeOffset? LastPulledAtUtc);

/// <summary>
/// Outcome of <see cref="IRemoteApiClient.WaitForBootstrapUpdateAsync"/>.
/// <see cref="Updated"/> is <c>true</c> when the server returned a fresh
/// cursor inside the wait window; <c>false</c> on timeout or
/// cancellation. <see cref="LastPulledAtUtc"/> is the cursor the server
/// stamped on the most recent successful push; <c>null</c> when the wait
/// timed out with no update.
/// </summary>
public sealed record BootstrapRemoteSignal(bool Updated, DateTimeOffset? LastPulledAtUtc);

/// <summary>Outcome of <see cref="IRemoteApiClient.RegisterAgentAsync"/>.</summary>
public sealed class AgentRegistrationResult
{
    /// <summary>True when the central API accepted the registration and returned a JWT.</summary>
    public bool Success { get; set; }

    /// <summary>JWT the agent should use as <c>Authorization: Bearer ...</c> on subsequent calls.</summary>
    public string Jwt { get; set; } = string.Empty;

    /// <summary>Agent id assigned by the central API (stored alongside the registration).</summary>
    public Guid AgentId { get; set; }

    /// <summary>Tenant id the license belongs to.</summary>
    public Guid TenantId { get; set; }

    /// <summary>UTC timestamp the JWT expires (the agent must re-register after this).</summary>
    public DateTimeOffset? ExpiresAtUtc { get; set; }

    /// <summary>Stable error code (e.g. <c>LICENSE_NOT_FOUND</c>, <c>LICENSE_EXPIRED</c>, <c>NETWORK</c>).</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Human-readable error message.</summary>
    public string? ErrorMessage { get; set; }
}
