namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A to-do of a company (<c>docs/GOAL_GOREVLER.md</c>). Kept only in the central database for ERP and
/// ERP-less companies alike: an ERP has no place for it and the agent never sees it.
///
/// <para>Ids of the task and of everything inside it are made by the phone, so a task created offline
/// keeps its id when the queued operation reaches the server. Times are unix milliseconds (UTC): the
/// SQLite test host cannot compare <see cref="DateTimeOffset"/>.</para>
/// </summary>
public sealed class WorkTask
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>One of <see cref="WorkTaskPriorities"/>.</summary>
    public string Priority { get; set; } = WorkTaskPriorities.Normal;

    /// <summary>One of <see cref="WorkTaskStatuses"/>.</summary>
    public string Status { get; set; } = WorkTaskStatuses.Open;

    public Guid CreatedByUserId { get; set; }

    /// <summary>Snapshot: stays readable after the user is renamed or deleted.</summary>
    public string CreatedByName { get; set; } = string.Empty;

    public long CreatedAtMs { get; set; }

    public long UpdatedAtMs { get; set; }

    /// <summary>A scheduled task: assignees hear of it only from this moment.</summary>
    public long? StartAtMs { get; set; }

    public long? DueAtMs { get; set; }

    public long? CompletedAtMs { get; set; }

    public Guid? CompletedByUserId { get; set; }

    public string? CompletedByName { get; set; }

    /// <summary>Completing needs at least one picture ("kanıt fotoğrafı").</summary>
    public bool RequiresPhoto { get; set; }

    public string? CustomerCode { get; set; }

    public string? CustomerName { get; set; }

    /// <summary>The repeating series this task is an occurrence of.</summary>
    public Guid? SeriesId { get; set; }

    /// <summary>When the assignees of a scheduled task were told; null until then.</summary>
    public long? StartNotifiedAtMs { get; set; }

    public long? DueSoonNotifiedAtMs { get; set; }

    public long? OverdueNotifiedAtMs { get; set; }

    public bool IsDeleted { get; set; }

    public long? DeletedAtMs { get; set; }

    /// <summary>The tenant's change order from <c>tenant_sync_counter</c> (rule 11); any change inside the task bumps it.</summary>
    public long UpdatedSeq { get; set; }

    public List<WorkTaskMember> Members { get; set; } = [];

    public List<WorkTaskSubtask> Subtasks { get; set; } = [];
}

/// <summary>An assignee or follower of a task.</summary>
public sealed class WorkTaskMember
{
    public Guid TaskId { get; set; }

    public WorkTask? Task { get; set; }

    public Guid UserId { get; set; }

    /// <summary>One of <see cref="WorkTaskMemberRoles"/>.</summary>
    public string Role { get; set; } = WorkTaskMemberRoles.Assignee;

    public string UserName { get; set; } = string.Empty;
}

public sealed class WorkTaskSubtask
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public WorkTask? Task { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsDone { get; set; }

    public Guid? DoneByUserId { get; set; }

    public string? DoneByName { get; set; }

    public long? DoneAtMs { get; set; }

    public Guid? AssigneeUserId { get; set; }

    public string? AssigneeName { get; set; }

    public long? DueAtMs { get; set; }

    public int SortOrder { get; set; }

    public bool IsDeleted { get; set; }
}

public sealed class WorkTaskComment
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TaskId { get; set; }

    public Guid AuthorUserId { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public long CreatedAtMs { get; set; }

    public bool IsDeleted { get; set; }
}

/// <summary>A picture on a task; the bytes are in <see cref="WorkTaskAttachmentBlob"/> so lists never load them.</summary>
public sealed class WorkTaskAttachment
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TaskId { get; set; }

    public Guid UploadedByUserId { get; set; }

    public string UploadedByName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public int SizeBytes { get; set; }

    public long CreatedAtMs { get; set; }

    public bool IsDeleted { get; set; }

    public long? DeletedAtMs { get; set; }
}

public sealed class WorkTaskAttachmentBlob
{
    public Guid AttachmentId { get; set; }

    public byte[] Data { get; set; } = [];
}

/// <summary>One step of a task's life. Rows are only ever added.</summary>
public sealed class WorkTaskEvent
{
    public long Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TaskId { get; set; }

    /// <summary>One of <see cref="WorkTaskActions"/>.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Null for what the system does (a series occurrence, a reminder).</summary>
    public Guid? ActorUserId { get; set; }

    public string ActorName { get; set; } = string.Empty;

    public string? Detail { get; set; }

    public long OccurredAtMs { get; set; }
}

/// <summary>
/// A repeating task: the template of every occurrence and the rule that says when the next one is due.
/// Times of day are Europe/Istanbul local time (goal K12).
/// </summary>
public sealed class WorkTaskSeries
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string CreatedByName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Priority { get; set; } = WorkTaskPriorities.Normal;

    public bool RequiresPhoto { get; set; }

    public string? CustomerCode { get; set; }

    public string? CustomerName { get; set; }

    /// <summary><c>[{ userId, userName }]</c>.</summary>
    public string AssigneesJson { get; set; } = "[]";

    public string FollowersJson { get; set; } = "[]";

    /// <summary><c>["alt görev başlığı", ...]</c>, copied into every occurrence.</summary>
    public string SubtasksJson { get; set; } = "[]";

    /// <summary>One of <see cref="WorkTaskFrequencies"/>.</summary>
    public string Frequency { get; set; } = WorkTaskFrequencies.Weekly;

    /// <summary>Every n days / weeks / months.</summary>
    public int Interval { get; set; } = 1;

    /// <summary>Weekly: bit mask, Monday = 1 … Sunday = 64.</summary>
    public int Weekdays { get; set; }

    /// <summary>Monthly: day of month 1-31 (a shorter month uses its last day).</summary>
    public int MonthDay { get; set; }

    /// <summary>Local time of day the occurrence appears, minutes after midnight.</summary>
    public int TimeOfDayMinutes { get; set; } = 9 * 60;

    /// <summary>The occurrence is due this long after it appears; null = no due time.</summary>
    public int? DueAfterMinutes { get; set; }

    public long NextRunAtMs { get; set; }

    public long? EndsAtMs { get; set; }

    public bool IsActive { get; set; } = true;

    public long CreatedAtMs { get; set; }

    public long UpdatedAtMs { get; set; }

    public long UpdatedSeq { get; set; }
}

/// <summary>An operation id the server already applied; a phone retrying a batch is not applied twice.</summary>
public sealed class WorkTaskOpApplied
{
    public Guid TenantId { get; set; }

    public Guid OpId { get; set; }

    public Guid UserId { get; set; }

    public long AppliedAtMs { get; set; }
}

/// <summary>One notification in a user's in-app inbox (goal §3).</summary>
public sealed class UserNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    /// <summary>One of <see cref="UserNotificationKinds"/>.</summary>
    public string Kind { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public Guid? TaskId { get; set; }

    public long CreatedAtMs { get; set; }

    public long? ReadAtMs { get; set; }

    /// <summary>The tenant's change order; the phone asks for notifications after the last one it saw.</summary>
    public long Seq { get; set; }
}

public static class WorkTaskPriorities
{
    public const string Low = "LOW";
    public const string Normal = "NORMAL";
    public const string High = "HIGH";
    public const string Urgent = "URGENT";

    public static readonly IReadOnlyList<string> All = [Low, Normal, High, Urgent];
}

public static class WorkTaskStatuses
{
    public const string Open = "OPEN";
    public const string Done = "DONE";
    public const string Cancelled = "CANCELLED";
}

public static class WorkTaskMemberRoles
{
    public const string Assignee = "ASSIGNEE";
    public const string Follower = "FOLLOWER";
}

public static class WorkTaskFrequencies
{
    public const string Daily = "DAILY";
    public const string Weekly = "WEEKLY";
    public const string Monthly = "MONTHLY";

    public static readonly IReadOnlyList<string> All = [Daily, Weekly, Monthly];
}

public static class WorkTaskActions
{
    public const string Created = "CREATED";
    public const string Updated = "UPDATED";
    public const string MembersChanged = "MEMBERS_CHANGED";
    public const string Completed = "COMPLETED";
    public const string Reopened = "REOPENED";
    public const string Cancelled = "CANCELLED";
    public const string Deleted = "DELETED";
    public const string SubtaskAdded = "SUBTASK_ADDED";
    public const string SubtaskChanged = "SUBTASK_CHANGED";
    public const string SubtaskDone = "SUBTASK_DONE";
    public const string SubtaskUndone = "SUBTASK_UNDONE";
    public const string SubtaskDeleted = "SUBTASK_DELETED";
    public const string Commented = "COMMENTED";
    public const string PhotoAdded = "PHOTO_ADDED";
    public const string PhotoDeleted = "PHOTO_DELETED";
}

public static class UserNotificationKinds
{
    public const string TaskAssigned = "TASK_ASSIGNED";
    public const string TaskCompleted = "TASK_COMPLETED";
    public const string TaskReopened = "TASK_REOPENED";
    public const string TaskCommented = "TASK_COMMENTED";
    public const string TaskDueSoon = "TASK_DUE_SOON";
    public const string TaskOverdue = "TASK_OVERDUE";
}
