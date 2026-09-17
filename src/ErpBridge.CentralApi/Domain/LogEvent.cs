namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// One diagnostic event from any part of the system — the phone app, the Windows agent (desktop or
/// service), the portal, the admin console or this API itself (Log Merkezi, L0). Every writer goes
/// through <see cref="LogCenter.LogEventWriter"/>, which normalizes, scrubs and groups the row.
/// Times are also kept as Unix milliseconds: SQLite (relational tests) cannot compare or order
/// <see cref="DateTimeOffset"/>, and a bigint is what the list queries filter and sort on anyway.
/// </summary>
public sealed class LogEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Producer-side identity (a GUID); a repeat with the same (source, event id) is ignored.</summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>One of <see cref="LogCenter.LogSources"/>.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Empty for events that belong to no company (API start-up, an admin screen).</summary>
    public Guid? TenantId { get; set; }

    public DateTimeOffset OccurredAtUtc { get; set; }
    public long OccurredAtMs { get; set; }
    public DateTimeOffset ReceivedAtUtc { get; set; }
    public long ReceivedAtMs { get; set; }

    /// <summary>DEBUG, INFO, WARN, ERROR or FATAL (<see cref="LogCenter.LogSeverity"/>).</summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>Upper-case ASCII event type, e.g. CRASH, HTTP_ERROR, SYNC_ROUND, DESKTOP_EXCEPTION.</summary>
    public string Kind { get; set; } = string.Empty;

    public string Operation { get; set; } = string.Empty;
    public string Screen { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;

    public string AppVersion { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;

    /// <summary>The phone's random installation UUID (never a hardware id).</summary>
    public string? DeviceId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? AgentId { get; set; }
    public string? SessionId { get; set; }

    /// <summary><c>X-Correlation-Id</c> shared by every event one user action produced, across apps.</summary>
    public string? CorrelationId { get; set; }

    public string? HttpMethod { get; set; }
    public string? HttpRoute { get; set; }
    public int? HttpStatus { get; set; }
    public int? DurationMs { get; set; }

    /// <summary>How many identical occurrences a throttling producer folded into this row.</summary>
    public int RepeatCount { get; set; } = 1;

    /// <summary>Set for WARN and above: the <see cref="LogErrorGroup"/> this event belongs to.</summary>
    public Guid? FingerprintId { get; set; }

    /// <summary>Small JSON object of producer-specific context (no business data).</summary>
    public string PropertiesJson { get; set; } = "{}";

    public string BreadcrumbsJson { get; set; } = "[]";
}
