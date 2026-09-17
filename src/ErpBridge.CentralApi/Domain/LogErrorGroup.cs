namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// One distinct problem: every WARN+ <see cref="LogEvent"/> whose fingerprint
/// (<see cref="LogCenter.ErrorFingerprint"/>) matches counts here, so a thousand repeats of the same crash
/// read as one line. Global, not per company — the same bug on many tenants is one group; affected
/// companies and devices are counted from the events.
/// </summary>
public sealed class LogErrorGroup
{
    public const string Open = "OPEN";
    public const string Resolved = "RESOLVED";
    public const string Ignored = "IGNORED";

    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Lower-case hex SHA-256 of the normalized error; unique.</summary>
    public string Fingerprint { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;

    /// <summary>Highest severity seen in the group.</summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>Message of the first event, scrubbed.</summary>
    public string SampleMessage { get; set; } = string.Empty;

    /// <summary>First application stack frame, without line numbers.</summary>
    public string TopFrame { get; set; } = string.Empty;

    public DateTimeOffset FirstSeenAtUtc { get; set; }
    public long FirstSeenMs { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }
    public long LastSeenMs { get; set; }
    public long TotalCount { get; set; }
    public string LastAppVersion { get; set; } = string.Empty;

    /// <summary><see cref="Open"/>, <see cref="Resolved"/> or <see cref="Ignored"/>.</summary>
    public string Status { get; set; } = Open;
    public DateTimeOffset? StatusChangedAtUtc { get; set; }
    public string? StatusChangedBy { get; set; }
    public string? Note { get; set; }

    /// <summary>Set when a resolved group received a new event and went back to open (alert trigger).</summary>
    public long? ReopenedAtMs { get; set; }
}
