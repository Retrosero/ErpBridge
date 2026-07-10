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

    /// <summary>Fetch pending jobs from the central queue.</summary>
    Task<IReadOnlyList<RemoteJob>> GetPendingJobsAsync(CancellationToken ct = default);

    /// <summary>Acknowledge a job as succeeded/failed.</summary>
    Task SendAckAsync(JobAck ack, CancellationToken ct = default);

    /// <summary>Push bootstrap data (cari/stok/fiyat/...) to the central API.</summary>
    Task PushBootstrapDataAsync(SyncPackage package, CancellationToken ct = default);

    /// <summary>Send a periodic agent heartbeat.</summary>
    Task SendHeartbeatAsync(AgentHeartbeat heartbeat, CancellationToken ct = default);
}
