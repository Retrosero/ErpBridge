namespace ErpBridge.CentralApi.Options;

/// <summary>
/// Configures the <see cref="Workers.AuditRetentionWorker"/> cleanup cadence.
/// Defaults: 365-day retention, 6 AM UTC run-time, every 24 hours.
/// </summary>
public sealed class AuditRetentionOptions
{
    /// <summary>Configuration section to bind against.</summary>
    public const string SectionName = "AuditRetention";

    /// <summary>How many days of audit log to keep. Older rows are deleted.</summary>
    public int RetentionDays { get; set; } = 365;

    /// <summary>Whether the retention worker is enabled at all. Set to <c>false</c> in tests.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>UTC hour-of-day when the worker runs (0-23).</summary>
    public int RunAtHourUtc { get; set; } = 6;

    /// <summary>Maximum rows deleted per run. Protects against runaway deletes on a misconfigured clock.</summary>
    public int MaxDeletesPerRun { get; set; } = 100_000;

    /// <summary>
    /// How long a mobile tombstone stays readable. Far shorter than the audit
    /// retention because it bounds something different: not how much history is
    /// kept, but how long a device may stay offline and still be brought up to
    /// date incrementally. Past this, the device is told to resync from scratch.
    /// </summary>
    public int MobileTombstoneDays { get; set; } = 30;
}
