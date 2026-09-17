namespace ErpBridge.Core.Sync;

/// <summary>What the last sync iteration did, as the heartbeat reports it.</summary>
public sealed record AgentHealthSnapshot(DateTimeOffset? LastSyncAtUtc, string? LastSyncResult, string? LastError);

/// <summary>
/// Log Merkezi L3f: the sync loop writes its outcome here and the heartbeat (service or desktop) reads it, so the
/// console shows when the agent last synced and why it failed — the heartbeat used to send "now" and no error.
/// </summary>
public sealed class AgentHealth
{
    private AgentHealthSnapshot _snapshot = new(null, null, null);

    public AgentHealthSnapshot Snapshot => Volatile.Read(ref _snapshot);

    /// <param name="errorCode">Null for a successful iteration.</param>
    public void RecordSync(DateTimeOffset atUtc, string? errorCode, string? errorMessage)
    {
        var result = errorCode is null ? "OK" : "FAILED";
        var error = errorCode is null ? null : ErpBridge.Shared.ConnectionStringMasker.MaskSecrets($"{errorCode}: {errorMessage}");
        if (error is { Length: > 1000 }) error = error[..1000];
        Volatile.Write(ref _snapshot, new AgentHealthSnapshot(atUtc, result, error));
    }
}

/// <summary>
/// Log Merkezi L3g: the correlation id of the work in progress (a job being written into the ERP). Every central API
/// call made while it is set carries it as <c>X-Correlation-Id</c>, and log lines carry it in their scope.
/// </summary>
public static class AgentCorrelation
{
    private static readonly AsyncLocal<string?> CurrentValue = new();

    public static string? Current => CurrentValue.Value;

    /// <summary>Sets <paramref name="correlationId"/> until the returned scope is disposed.</summary>
    public static IDisposable Begin(string? correlationId)
    {
        var previous = CurrentValue.Value;
        CurrentValue.Value = string.IsNullOrWhiteSpace(correlationId) ? previous : correlationId;
        return new Restore(previous);
    }

    private sealed class Restore(string? previous) : IDisposable
    {
        public void Dispose() => CurrentValue.Value = previous;
    }
}
