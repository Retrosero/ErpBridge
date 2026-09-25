namespace ErpBridge.CentralApi.Tasks;

/// <summary><c>Tasks</c> configuration section (docs/GOAL_GOREVLER.md K9).</summary>
public sealed class TaskOptions
{
    public const string SectionName = "Tasks";

    /// <summary>Largest picture accepted; the phone sends ~300 KB.</summary>
    public int MaxAttachmentBytes { get; set; } = 2 * 1024 * 1024;

    public int MaxAttachmentsPerTask { get; set; } = 10;

    /// <summary>Live pictures of one company, all tasks together.</summary>
    public long TenantAttachmentQuotaBytes { get; set; } = 1024L * 1024 * 1024;

    public bool SchedulerEnabled { get; set; } = true;

    public int SchedulerIntervalSeconds { get; set; } = 60;

    /// <summary>"Bitiş yaklaşıyor" is sent this long before the due time.</summary>
    public int DueSoonMinutes { get; set; } = 60;

    /// <summary>Pictures of deleted tasks, deleted pictures and applied operation ids are removed after this.</summary>
    public int PurgeAfterDays { get; set; } = 30;
}
