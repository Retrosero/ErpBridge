using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

// Görevler ve bildirimler (docs/GOAL_GOREVLER.md §3). Times are unix milliseconds (UTC).

public sealed class TaskMemberDto
{
    [JsonPropertyName("userId")] public Guid UserId { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

public sealed class TaskSubtaskDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;

    [JsonPropertyName("isDone")] public bool IsDone { get; set; }

    [JsonPropertyName("doneByName")] public string? DoneByName { get; set; }

    [JsonPropertyName("doneAtMs")] public long? DoneAtMs { get; set; }

    [JsonPropertyName("assigneeUserId")] public Guid? AssigneeUserId { get; set; }

    [JsonPropertyName("assigneeName")] public string? AssigneeName { get; set; }

    [JsonPropertyName("dueAtMs")] public long? DueAtMs { get; set; }

    [JsonPropertyName("sortOrder")] public int SortOrder { get; set; }
}

public sealed class TaskAttachmentDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("contentType")] public string ContentType { get; set; } = string.Empty;

    [JsonPropertyName("sizeBytes")] public int SizeBytes { get; set; }

    [JsonPropertyName("uploadedByUserId")] public Guid UploadedByUserId { get; set; }

    [JsonPropertyName("uploadedByName")] public string UploadedByName { get; set; } = string.Empty;

    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }
}

public class TaskDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

    [JsonPropertyName("priority")] public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;

    [JsonPropertyName("createdByUserId")] public Guid CreatedByUserId { get; set; }

    [JsonPropertyName("createdByName")] public string CreatedByName { get; set; } = string.Empty;

    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }

    [JsonPropertyName("updatedAtMs")] public long UpdatedAtMs { get; set; }

    [JsonPropertyName("startAtMs")] public long? StartAtMs { get; set; }

    [JsonPropertyName("dueAtMs")] public long? DueAtMs { get; set; }

    [JsonPropertyName("completedAtMs")] public long? CompletedAtMs { get; set; }

    [JsonPropertyName("completedByName")] public string? CompletedByName { get; set; }

    [JsonPropertyName("requiresPhoto")] public bool RequiresPhoto { get; set; }

    [JsonPropertyName("customerCode")] public string? CustomerCode { get; set; }

    [JsonPropertyName("customerName")] public string? CustomerName { get; set; }

    [JsonPropertyName("seriesId")] public Guid? SeriesId { get; set; }

    [JsonPropertyName("isDeleted")] public bool IsDeleted { get; set; }

    [JsonPropertyName("updatedSeq")] public long UpdatedSeq { get; set; }

    [JsonPropertyName("assignees")] public TaskMemberDto[] Assignees { get; set; } = [];

    [JsonPropertyName("followers")] public TaskMemberDto[] Followers { get; set; } = [];

    [JsonPropertyName("subtasks")] public TaskSubtaskDto[] Subtasks { get; set; } = [];

    [JsonPropertyName("attachments")] public TaskAttachmentDto[] Attachments { get; set; } = [];

    [JsonPropertyName("commentCount")] public int CommentCount { get; set; }

    /// <summary>The caller may edit the task, its members and subtasks (creator or manager).</summary>
    [JsonPropertyName("canEdit")] public bool CanEdit { get; set; }

    /// <summary>The caller may complete it, tick subtasks and add pictures (also every assignee).</summary>
    [JsonPropertyName("canWork")] public bool CanWork { get; set; }
}

public sealed class TaskCommentDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("authorUserId")] public Guid AuthorUserId { get; set; }

    [JsonPropertyName("authorName")] public string AuthorName { get; set; } = string.Empty;

    [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;

    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }
}

public sealed class TaskEventDto
{
    [JsonPropertyName("action")] public string Action { get; set; } = string.Empty;

    [JsonPropertyName("actorName")] public string ActorName { get; set; } = string.Empty;

    [JsonPropertyName("detail")] public string? Detail { get; set; }

    [JsonPropertyName("occurredAtMs")] public long OccurredAtMs { get; set; }
}

public sealed class TaskDetailDto : TaskDto
{
    [JsonPropertyName("comments")] public TaskCommentDto[] Comments { get; set; } = [];

    [JsonPropertyName("events")] public TaskEventDto[] Events { get; set; } = [];
}

public sealed class TaskListResponse
{
    [JsonPropertyName("tasks")] public TaskDto[] Tasks { get; set; } = [];

    /// <summary>Pass back as <c>changedSinceSeq</c>; the page's last <c>updatedSeq</c>.</summary>
    [JsonPropertyName("latestSeq")] public long LatestSeq { get; set; }

    [JsonPropertyName("hasMore")] public bool HasMore { get; set; }
}

/// <summary>A new subtask inside <c>create_task</c> / <c>add_subtask</c>.</summary>
public sealed class TaskSubtaskInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("title")] public string? Title { get; set; }

    [JsonPropertyName("assigneeUserId")] public Guid? AssigneeUserId { get; set; }

    [JsonPropertyName("dueAtMs")] public long? DueAtMs { get; set; }
}

/// <summary>
/// One queued change from a phone. Which fields count depends on <see cref="Type"/> (goal §3);
/// <c>update_task</c>, <c>update_subtask</c> and <c>update_series</c> carry the full editable state, so a
/// null means "empty", never "unchanged".
/// </summary>
public sealed class TaskOp
{
    [JsonPropertyName("opId")] public Guid OpId { get; set; }

    [JsonPropertyName("type")] public string? Type { get; set; }

    [JsonPropertyName("taskId")] public Guid? TaskId { get; set; }

    [JsonPropertyName("title")] public string? Title { get; set; }

    [JsonPropertyName("description")] public string? Description { get; set; }

    [JsonPropertyName("priority")] public string? Priority { get; set; }

    [JsonPropertyName("startAtMs")] public long? StartAtMs { get; set; }

    [JsonPropertyName("dueAtMs")] public long? DueAtMs { get; set; }

    [JsonPropertyName("requiresPhoto")] public bool? RequiresPhoto { get; set; }

    [JsonPropertyName("customerCode")] public string? CustomerCode { get; set; }

    [JsonPropertyName("customerName")] public string? CustomerName { get; set; }

    [JsonPropertyName("assigneeIds")] public Guid[]? AssigneeIds { get; set; }

    [JsonPropertyName("followerIds")] public Guid[]? FollowerIds { get; set; }

    [JsonPropertyName("subtasks")] public TaskSubtaskInput[]? Subtasks { get; set; }

    [JsonPropertyName("subtaskId")] public Guid? SubtaskId { get; set; }

    [JsonPropertyName("assigneeUserId")] public Guid? AssigneeUserId { get; set; }

    [JsonPropertyName("isDone")] public bool? IsDone { get; set; }

    [JsonPropertyName("sortOrder")] public int? SortOrder { get; set; }

    [JsonPropertyName("commentId")] public Guid? CommentId { get; set; }

    [JsonPropertyName("text")] public string? Text { get; set; }

    [JsonPropertyName("seriesId")] public Guid? SeriesId { get; set; }

    [JsonPropertyName("frequency")] public string? Frequency { get; set; }

    [JsonPropertyName("interval")] public int? Interval { get; set; }

    [JsonPropertyName("weekdays")] public int? Weekdays { get; set; }

    [JsonPropertyName("monthDay")] public int? MonthDay { get; set; }

    [JsonPropertyName("timeOfDayMinutes")] public int? TimeOfDayMinutes { get; set; }

    [JsonPropertyName("dueAfterMinutes")] public int? DueAfterMinutes { get; set; }

    [JsonPropertyName("endsAtMs")] public long? EndsAtMs { get; set; }

    [JsonPropertyName("subtaskTitles")] public string[]? SubtaskTitles { get; set; }
}

public sealed class TaskOpsRequest
{
    [JsonPropertyName("ops")] public TaskOp[]? Ops { get; set; }
}

public sealed class TaskOpResult
{
    [JsonPropertyName("opId")] public Guid OpId { get; set; }

    /// <summary><c>applied</c>, <c>duplicate</c> (already applied earlier) or <c>rejected</c> (drop it; do not retry).</summary>
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;

    [JsonPropertyName("errorCode")] public string? ErrorCode { get; set; }

    [JsonPropertyName("message")] public string? Message { get; set; }
}

public sealed class TaskOpsResponse
{
    [JsonPropertyName("results")] public TaskOpResult[] Results { get; set; } = [];

    /// <summary>The current state of every task the batch touched and the caller can still see.</summary>
    [JsonPropertyName("tasks")] public TaskDto[] Tasks { get; set; } = [];

    [JsonPropertyName("series")] public TaskSeriesDto[] Series { get; set; } = [];
}

public sealed class TaskPersonDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("roles")] public string[] Roles { get; set; } = [];
}

public sealed class TaskSummaryDto
{
    [JsonPropertyName("latestTaskSeq")] public long LatestTaskSeq { get; set; }

    [JsonPropertyName("latestNotificationSeq")] public long LatestNotificationSeq { get; set; }

    [JsonPropertyName("unreadCount")] public int UnreadCount { get; set; }

    /// <summary>Open tasks the caller is an assignee of and that have started.</summary>
    [JsonPropertyName("openAssignedCount")] public int OpenAssignedCount { get; set; }

    [JsonPropertyName("overdueCount")] public int OverdueCount { get; set; }

    /// <summary>ADMIN or MANAGER: assigns anyone and sees every task.</summary>
    [JsonPropertyName("canManage")] public bool CanManage { get; set; }
}

public sealed class TaskSeriesDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

    [JsonPropertyName("priority")] public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("requiresPhoto")] public bool RequiresPhoto { get; set; }

    [JsonPropertyName("customerCode")] public string? CustomerCode { get; set; }

    [JsonPropertyName("customerName")] public string? CustomerName { get; set; }

    [JsonPropertyName("assignees")] public TaskMemberDto[] Assignees { get; set; } = [];

    [JsonPropertyName("followers")] public TaskMemberDto[] Followers { get; set; } = [];

    [JsonPropertyName("subtaskTitles")] public string[] SubtaskTitles { get; set; } = [];

    [JsonPropertyName("frequency")] public string Frequency { get; set; } = string.Empty;

    [JsonPropertyName("interval")] public int Interval { get; set; }

    [JsonPropertyName("weekdays")] public int Weekdays { get; set; }

    [JsonPropertyName("monthDay")] public int MonthDay { get; set; }

    [JsonPropertyName("timeOfDayMinutes")] public int TimeOfDayMinutes { get; set; }

    [JsonPropertyName("dueAfterMinutes")] public int? DueAfterMinutes { get; set; }

    [JsonPropertyName("nextRunAtMs")] public long NextRunAtMs { get; set; }

    [JsonPropertyName("endsAtMs")] public long? EndsAtMs { get; set; }

    [JsonPropertyName("isActive")] public bool IsActive { get; set; }

    [JsonPropertyName("createdByName")] public string CreatedByName { get; set; } = string.Empty;

    [JsonPropertyName("updatedSeq")] public long UpdatedSeq { get; set; }
}

public sealed class UserNotificationDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body")] public string Body { get; set; } = string.Empty;

    [JsonPropertyName("taskId")] public Guid? TaskId { get; set; }

    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }

    [JsonPropertyName("readAtMs")] public long? ReadAtMs { get; set; }

    [JsonPropertyName("seq")] public long Seq { get; set; }
}

public sealed class UserNotificationListResponse
{
    [JsonPropertyName("notifications")] public UserNotificationDto[] Notifications { get; set; } = [];

    [JsonPropertyName("latestSeq")] public long LatestSeq { get; set; }

    [JsonPropertyName("hasMore")] public bool HasMore { get; set; }

    [JsonPropertyName("unreadCount")] public int UnreadCount { get; set; }
}

public sealed class MarkNotificationsReadRequest
{
    [JsonPropertyName("ids")] public Guid[]? Ids { get; set; }

    [JsonPropertyName("all")] public bool All { get; set; }
}

public sealed class TaskEventsResponse
{
    /// <summary>Pass back as <c>version</c>; a different number means something changed.</summary>
    [JsonPropertyName("version")] public long Version { get; set; }
}
