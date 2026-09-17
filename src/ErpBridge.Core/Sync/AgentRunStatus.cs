using ErpBridge.Shared;

namespace ErpBridge.Core.Sync;

/// <summary>
/// Log Merkezi L3f: what the agent last did, shared between whoever did it and whoever reports it. The sync
/// loop and the job worker write here; the heartbeat (Windows service and desktop app alike) reads it.
///
/// <para>Before this existed, <c>HeartbeatWorker.RecordSuccessfulSync</c> and <c>RecordError</c> were seams
/// nobody ever called: every heartbeat claimed "last sync = now" whether the agent had synced or had been
/// failing since Tuesday, and the desktop app sent no sync time at all.</para>
/// </summary>
public sealed class AgentRunStatus
{
    /// <summary>The error text is bounded like the heartbeat's own field; the server bounds it again.</summary>
    public const int MaxError = 1024;

    private readonly Lock _gate = new();
    private DateTimeOffset? _lastSyncAtUtc;
    private string? _lastSyncResult;
    private string? _lastErrorCode;
    private string? _lastError;
    private string? _erpVersion;
    private DateTimeOffset? _erpVersionAtUtc;

    /// <summary>A successful or failed sync round.</summary>
    public void RecordSync(bool success, DateTimeOffset atUtc, string? errorCode = null, string? errorMessage = null)
    {
        lock (_gate)
        {
            _lastSyncResult = success ? "ok" : "failed";
            if (success)
            {
                // Only a round that actually worked moves the clock — that is the whole point of the field.
                _lastSyncAtUtc = atUtc;
                _lastErrorCode = null;
                _lastError = null;
                return;
            }
            _lastErrorCode = Bound(errorCode, 64);
            _lastError = Mask(errorMessage);
        }
    }

    /// <summary>An error outside a sync round: a job that could not be written, a queue that would not take it.</summary>
    public void RecordError(string? errorCode, string? errorMessage)
    {
        lock (_gate)
        {
            _lastErrorCode = Bound(errorCode, 64) ?? _lastErrorCode;
            _lastError = Mask(errorMessage) ?? _lastError;
        }
    }

    /// <summary>
    /// The ERP edition the adapter probed (e.g. <c>V15</c>). Kept here so the heartbeat does not re-probe the
    /// ERP every minute for a value that changes when the customer upgrades, which is to say almost never.
    /// </summary>
    public void RecordErpVersion(string? version, DateTimeOffset atUtc)
    {
        lock (_gate)
        {
            _erpVersion = Bound(version, 64);
            _erpVersionAtUtc = atUtc;
        }
    }

    /// <summary>True when the ERP version is unknown or older than <paramref name="maxAge"/>.</summary>
    public bool NeedsErpVersion(DateTimeOffset now, TimeSpan maxAge)
    {
        lock (_gate)
        {
            return _erpVersionAtUtc is not { } probed || now - probed > maxAge;
        }
    }

    /// <summary>What the next heartbeat should carry.</summary>
    public AgentRunSnapshot Read()
    {
        lock (_gate)
        {
            return new AgentRunSnapshot(_lastSyncAtUtc, _lastSyncResult, _lastErrorCode, _lastError, _erpVersion);
        }
    }

    /// <summary>Called after a heartbeat went out, so the panel shows the newest failure rather than the oldest.</summary>
    public void ClearError()
    {
        lock (_gate)
        {
            _lastErrorCode = null;
            _lastError = null;
        }
    }

    private static string? Bound(string? value, int max) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, max)];

    /// <summary>A connection string in an error message must not travel to the server.</summary>
    private static string? Mask(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var masked = ConnectionStringMasker.MaskSecrets(value);
        return masked.Length <= MaxError ? masked : masked[..MaxError];
    }
}

/// <summary>An immutable read of <see cref="AgentRunStatus"/>.</summary>
public sealed record AgentRunSnapshot(
    DateTimeOffset? LastSyncAtUtc,
    string? LastSyncResult,
    string? LastErrorCode,
    string? LastError,
    string? ErpVersion = null);
