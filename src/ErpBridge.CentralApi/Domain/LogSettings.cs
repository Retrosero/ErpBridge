namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// How long the log centre keeps events (plan D8). A single row, <see cref="SingletonId"/>; when it is missing the
/// defaults apply. Operators change it from the admin console (plan L6d).
/// </summary>
public sealed class LogSettings
{
    public const int SingletonId = 1;
    public const int DefaultInfoRetentionDays = 14;
    public const int DefaultWarnRetentionDays = 90;
    public const int MinRetentionDays = 1;
    public const int MaxRetentionDays = 730;

    public int Id { get; set; } = SingletonId;

    /// <summary>DEBUG and INFO events (screen views, sync rounds) — high volume, short life.</summary>
    public int InfoRetentionDays { get; set; } = DefaultInfoRetentionDays;

    /// <summary>WARN, ERROR and FATAL events, and error groups that stopped happening.</summary>
    public int WarnRetentionDays { get; set; } = DefaultWarnRetentionDays;

    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}
