namespace ErpBridge.Core.Domain;

public static class LiveSyncScopes
{
    public const string Status = "live-monitor";
    public static string ChangeVersion(string database) => "live-change:" + database.Trim().ToLowerInvariant();
    public static string Detected(string section) => "live-detected:" + section.Trim().ToLowerInvariant();
}

public sealed record LiveSyncState(
    string Status,
    string Mode,
    DateTimeOffset? LastDetectedAtUtc,
    DateTimeOffset? LastTransferredAtUtc,
    string? Message = null);
