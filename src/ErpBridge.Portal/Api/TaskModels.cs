namespace ErpBridge.Portal.Api;

// GOAL_PANEL_GIRIS P6: the phone's tasks and notifications (`/api/v1/android/tasks`, `/notifications`, GOAL_GOREVLER §3).
// Mirrors of the central API's TaskContracts, kept local on purpose. Times are unix milliseconds (UTC).

public sealed class TaskMemberDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class TaskSubtaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public string? DoneByName { get; set; }
    public long? DoneAtMs { get; set; }
    public Guid? AssigneeUserId { get; set; }
    public string? AssigneeName { get; set; }
    public long? DueAtMs { get; set; }
    public int SortOrder { get; set; }
}

public sealed class TaskAttachmentDto
{
    public Guid Id { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int SizeBytes { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public long CreatedAtMs { get; set; }
}

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public long CreatedAtMs { get; set; }
    public long UpdatedAtMs { get; set; }
    public long? StartAtMs { get; set; }
    public long? DueAtMs { get; set; }
    public long? CompletedAtMs { get; set; }
    public string? CompletedByName { get; set; }
    public bool RequiresPhoto { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public bool VisitReminder { get; set; }
    public long? VisitReminderFromMs { get; set; }
    public Guid? SeriesId { get; set; }
    public bool IsDeleted { get; set; }
    public long UpdatedSeq { get; set; }
    public TaskMemberDto[] Assignees { get; set; } = [];
    public TaskMemberDto[] Followers { get; set; } = [];
    public TaskSubtaskDto[] Subtasks { get; set; } = [];
    public TaskAttachmentDto[] Attachments { get; set; } = [];
    public int CommentCount { get; set; }
    public bool CanEdit { get; set; }
    public bool CanWork { get; set; }
}

public sealed class TaskCommentDto
{
    public Guid Id { get; set; }
    public Guid AuthorUserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public long CreatedAtMs { get; set; }
}

public sealed class TaskEventDto
{
    public string Action { get; set; } = string.Empty;
    public string ActorName { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public long OccurredAtMs { get; set; }
}

public sealed class TaskDetailDto : TaskDto
{
    public TaskCommentDto[] Comments { get; set; } = [];
    public TaskEventDto[] Events { get; set; } = [];
}

public sealed class TaskListResponse
{
    public TaskDto[] Tasks { get; set; } = [];
    public long LatestSeq { get; set; }
    public bool HasMore { get; set; }
}

public sealed class TaskSubtaskInput
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public Guid? AssigneeUserId { get; set; }
    public long? DueAtMs { get; set; }
}

/// <summary>One change, sent in <c>POST tasks/ops</c>; which fields count depends on <see cref="Type"/>.</summary>
public sealed class TaskOp
{
    public Guid OpId { get; set; } = Guid.NewGuid();
    public string? Type { get; set; }
    public Guid? TaskId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public long? StartAtMs { get; set; }
    public long? DueAtMs { get; set; }
    public bool? RequiresPhoto { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public bool? VisitReminder { get; set; }
    public long? VisitReminderFromMs { get; set; }
    public Guid[]? AssigneeIds { get; set; }
    public Guid[]? FollowerIds { get; set; }
    public TaskSubtaskInput[]? Subtasks { get; set; }
    public Guid? SubtaskId { get; set; }
    public Guid? AssigneeUserId { get; set; }
    public bool? IsDone { get; set; }
    public int? SortOrder { get; set; }
    public Guid? CommentId { get; set; }
    public string? Text { get; set; }
    public Guid? SeriesId { get; set; }
    public string? Frequency { get; set; }
    public int? Interval { get; set; }
    public int? Weekdays { get; set; }
    public int? MonthDay { get; set; }
    public int? TimeOfDayMinutes { get; set; }
    public int? DueAfterMinutes { get; set; }
    public long? EndsAtMs { get; set; }
    public string[]? SubtaskTitles { get; set; }
}

public sealed class TaskOpResult
{
    public Guid OpId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
}

public sealed class TaskOpsResponse
{
    public TaskOpResult[] Results { get; set; } = [];
    public TaskDto[] Tasks { get; set; } = [];
    public TaskSeriesDto[] Series { get; set; } = [];
}

public sealed class TaskPersonDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
}

public sealed class TaskSummaryDto
{
    public long LatestTaskSeq { get; set; }
    public long LatestNotificationSeq { get; set; }
    public int UnreadCount { get; set; }
    public int OpenAssignedCount { get; set; }
    public int OverdueCount { get; set; }
    public bool CanManage { get; set; }
}

public sealed class TaskSeriesDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public bool RequiresPhoto { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public bool VisitReminder { get; set; }
    public TaskMemberDto[] Assignees { get; set; } = [];
    public TaskMemberDto[] Followers { get; set; } = [];
    public string[] SubtaskTitles { get; set; } = [];
    public string Frequency { get; set; } = string.Empty;
    public int Interval { get; set; }
    public int Weekdays { get; set; }
    public int MonthDay { get; set; }
    public int TimeOfDayMinutes { get; set; }
    public int? DueAfterMinutes { get; set; }
    public long NextRunAtMs { get; set; }
    public long? EndsAtMs { get; set; }
    public bool IsActive { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public long UpdatedSeq { get; set; }
}

public sealed class UserNotificationDto
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Guid? TaskId { get; set; }
    public long CreatedAtMs { get; set; }
    public long? ReadAtMs { get; set; }
    public long Seq { get; set; }
}

public sealed class UserNotificationListResponse
{
    public UserNotificationDto[] Notifications { get; set; } = [];
    public long LatestSeq { get; set; }
    public bool HasMore { get; set; }
    public int UnreadCount { get; set; }
}

public sealed class TaskEventsResponse
{
    public long Version { get; set; }
}

/// <summary>The panel's words for task states and priorities, as the phone shows them.</summary>
public static class TaskText
{
    public static readonly string[] Priorities = ["LOW", "NORMAL", "HIGH", "URGENT"];

    public static string Priority(string priority) => priority switch
    {
        "LOW" => "Düşük",
        "HIGH" => "Yüksek",
        "URGENT" => "Acil",
        _ => "Normal",
    };

    public static string Status(string status) => status switch
    {
        "DONE" => "Tamamlandı",
        "CANCELLED" => "İptal",
        _ => "Açık",
    };

    public static string Frequency(string frequency) => frequency switch
    {
        "DAILY" => "Her gün",
        "WEEKLY" => "Her hafta",
        "MONTHLY" => "Her ay",
        _ => frequency,
    };

    /// <summary>Istanbul wall clock of a unix-ms moment.</summary>
    public static string When(long? ms) => ms is { } value ? Fmt.Time(DateTimeOffset.FromUnixTimeMilliseconds(value)) : "—";
}
