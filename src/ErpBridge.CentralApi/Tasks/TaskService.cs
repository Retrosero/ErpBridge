using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Tasks;

/// <summary>Outcome of a task call: a value, or an HTTP status with a stable error code.</summary>
public sealed record TaskResult<T>(T? Value, int StatusCode, ApiError? Error)
{
    public static TaskResult<T> Ok(T value) => new(value, 200, null);

    public static TaskResult<T> Fail(int status, string code, string message) =>
        new(default, status, new ApiError { ErrorCode = code, Message = message });

    public bool Succeeded => Error is null;
}

/// <summary>
/// Tasks and the in-app notifications they produce (docs/GOAL_GOREVLER.md).
///
/// <para><b>Writes come as operations.</b> A phone queues what the user did offline and sends it in
/// batches (<see cref="ApplyAsync"/>). Every operation has an id; an id applied before is answered
/// <c>duplicate</c> and not applied again, so a batch that timed out can simply be sent again. A
/// rejected operation leaves nothing behind (savepoint) and does not stop the rest of the batch.</para>
///
/// <para><b>Change order.</b> Tasks, series and notifications take their numbers from the tenant's
/// <c>tenant_sync_counter</c> inside the writing transaction (rule 11), so <c>changedSinceSeq</c> never
/// steps over a change that commits late. After commit the <c>tasks</c> topic wakes long-polls.</para>
/// </summary>
public sealed class TaskService
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 4000;
    public const int MaxSubtaskTitleLength = 300;
    public const int MaxCommentLength = 2000;
    public const int MaxMembers = 50;
    public const int MaxSubtasks = 100;
    public const int MaxOpsPerBatch = 200;
    public const int DefaultTake = 200;
    public const int MaxTake = 500;

    public static readonly IReadOnlySet<string> ImageTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    private const string SystemActor = "Sistem";

    private readonly ITenantEventHub _events;
    private readonly TaskOptions _options;

    public TaskService(ITenantEventHub events, IOptions<TaskOptions> options)
    {
        _events = events;
        _options = options.Value;
    }

    public TaskOptions Options => _options;

    /// <summary>ADMIN or MANAGER: assigns anyone, sees every task of the company (goal K3, K4).</summary>
    public static bool CanManage(MobileUser user) =>
        RolePermissions.IsAdmin(user) || RolePermissions.Has(user, MobileUserRoles.Manager);

    public void Notify(Guid tenantId) => _events.Publish(tenantId, TenantEventTopics.Tasks);

    public static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // ---- visibility and rights ------------------------------------------------------------

    /// <summary>
    /// The tasks <paramref name="user"/> may see. A scheduled task stays hidden from its assignees until it
    /// has started — the scheduler marks that (<see cref="WorkTask.StartNotifiedAtMs"/>) when it tells them and
    /// bumps the change number, so the task arrives together with its notification.
    /// </summary>
    public static IQueryable<WorkTask> Visible(CentralApiDbContext db, Guid tenantId, MobileUser user, long nowMs)
    {
        var query = db.WorkTasks.Where(t => t.TenantId == tenantId);
        if (CanManage(user)) return query;
        var id = user.Id;
        return query.Where(t => t.CreatedByUserId == id
            || ((t.StartNotifiedAtMs != null || t.StartAtMs == null || t.StartAtMs <= nowMs)
                && (t.Members.Any(m => m.UserId == id) || t.Subtasks.Any(s => s.AssigneeUserId == id && !s.IsDeleted))));
    }

    public static bool CanEdit(WorkTask task, MobileUser user) => CanManage(user) || task.CreatedByUserId == user.Id;

    public static bool IsAssignee(WorkTask task, Guid userId) =>
        task.Members.Any(m => m.UserId == userId && m.Role == WorkTaskMemberRoles.Assignee);

    /// <summary>Completes, reopens, ticks subtasks and adds pictures.</summary>
    public static bool CanWork(WorkTask task, MobileUser user) =>
        CanEdit(task, user) || IsAssignee(task, user.Id) || task.Subtasks.Any(s => !s.IsDeleted && s.AssigneeUserId == user.Id);

    public static bool CanSee(WorkTask task, MobileUser user, long nowMs) =>
        CanManage(user) || task.CreatedByUserId == user.Id
        || ((task.StartNotifiedAtMs is not null || IsStarted(task, nowMs)) && (task.Members.Any(m => m.UserId == user.Id) || task.Subtasks.Any(s => !s.IsDeleted && s.AssigneeUserId == user.Id)));

    public static bool IsStarted(WorkTask task, long nowMs) => task.StartAtMs is not { } start || start <= nowMs;

    // ---- reads ------------------------------------------------------------------------------

    public async Task<TaskListResponse> ListAsync(CentralApiDbContext db, Guid tenantId, MobileUser user, long since, int? take, CancellationToken ct)
    {
        var limit = Math.Clamp(take ?? DefaultTake, 1, MaxTake);
        var now = NowMs();
        var rows = await Visible(db, tenantId, user, now).AsNoTracking()
            .Where(t => t.UpdatedSeq > since)
            .OrderBy(t => t.UpdatedSeq)
            .Take(limit + 1)
            .Include(t => t.Members)
            .Include(t => t.Subtasks)
            .AsSplitQuery()
            .ToListAsync(ct);
        var hasMore = rows.Count > limit;
        var page = hasMore ? rows.Take(limit).ToList() : rows;
        var dtos = await MapAsync(db, page, user, ct);
        return new TaskListResponse
        {
            Tasks = dtos,
            LatestSeq = page.Count > 0 ? page[^1].UpdatedSeq : since,
            HasMore = hasMore,
        };
    }

    public async Task<TaskResult<TaskDetailDto>> DetailAsync(CentralApiDbContext db, Guid tenantId, MobileUser user, Guid id, CancellationToken ct)
    {
        var task = await LoadAsync(db, tenantId, id, tracking: false, ct);
        if (task is null || !CanSee(task, user, NowMs()))
            return TaskResult<TaskDetailDto>.Fail(404, "TASK_NOT_FOUND", "Görev bulunamadı.");
        var detail = new TaskDetailDto();
        await FillAsync(db, detail, task, user, ct);
        detail.Comments = await db.WorkTaskComments.AsNoTracking()
            .Where(c => c.TaskId == id && !c.IsDeleted)
            .OrderBy(c => c.CreatedAtMs)
            .Select(c => new TaskCommentDto { Id = c.Id, AuthorUserId = c.AuthorUserId, AuthorName = c.AuthorName, Text = c.Text, CreatedAtMs = c.CreatedAtMs })
            .ToArrayAsync(ct);
        detail.Events = await db.WorkTaskEvents.AsNoTracking()
            .Where(e => e.TaskId == id)
            .OrderBy(e => e.Id)
            .Select(e => new TaskEventDto { Action = e.Action, ActorName = e.ActorName, Detail = e.Detail, OccurredAtMs = e.OccurredAtMs })
            .ToArrayAsync(ct);
        return TaskResult<TaskDetailDto>.Ok(detail);
    }

    public async Task<TaskSummaryDto> SummaryAsync(CentralApiDbContext db, Guid tenantId, MobileUser user, CancellationToken ct)
    {
        var now = NowMs();
        var visible = Visible(db, tenantId, user, now).AsNoTracking();
        var mine = visible.Where(t => !t.IsDeleted && t.Status == WorkTaskStatuses.Open
            && (t.StartNotifiedAtMs != null || t.StartAtMs == null || t.StartAtMs <= now)
            && t.Members.Any(m => m.UserId == user.Id && m.Role == WorkTaskMemberRoles.Assignee));
        var notifications = db.UserNotifications.AsNoTracking().Where(n => n.TenantId == tenantId && n.UserId == user.Id);
        return new TaskSummaryDto
        {
            LatestTaskSeq = await visible.MaxAsync(t => (long?)t.UpdatedSeq, ct) ?? 0,
            LatestNotificationSeq = await notifications.MaxAsync(n => (long?)n.Seq, ct) ?? 0,
            UnreadCount = await notifications.CountAsync(n => n.ReadAtMs == null, ct),
            OpenAssignedCount = await mine.CountAsync(ct),
            OverdueCount = await mine.CountAsync(t => t.DueAtMs != null && t.DueAtMs < now, ct),
            CanManage = CanManage(user),
        };
    }

    /// <summary>Everyone who can be given a task: the company's active users.</summary>
    public static async Task<TaskPersonDto[]> PeopleAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct)
    {
        var users = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.IsActive && u.DeletedAtUtc == null)
            .Include(u => u.Roles)
            .ToListAsync(ct);
        return users
            .OrderBy(u => u.FullName, StringComparer.Create(new System.Globalization.CultureInfo("tr-TR"), true))
            .Select(u => new TaskPersonDto { Id = u.Id, FullName = u.FullName, Roles = RolePermissions.Of(u).ToArray() })
            .ToArray();
    }

    public static async Task<TaskSeriesDto[]> SeriesAsync(CentralApiDbContext db, Guid tenantId, MobileUser user, CancellationToken ct)
    {
        var query = db.WorkTaskSeries.AsNoTracking().Where(s => s.TenantId == tenantId);
        if (!CanManage(user)) query = query.Where(s => s.CreatedByUserId == user.Id);
        var rows = await query.OrderByDescending(s => s.UpdatedSeq).ToListAsync(ct);
        return rows.Select(ToDto).ToArray();
    }

    public async Task<UserNotificationListResponse> NotificationsAsync(CentralApiDbContext db, Guid tenantId, Guid userId, long since, int? take, CancellationToken ct)
    {
        var limit = Math.Clamp(take ?? DefaultTake, 1, MaxTake);
        var mine = db.UserNotifications.AsNoTracking().Where(n => n.TenantId == tenantId && n.UserId == userId);
        var rows = await mine.Where(n => n.Seq > since).OrderBy(n => n.Seq).ThenBy(n => n.CreatedAtMs).Take(limit + 1).ToListAsync(ct);
        var hasMore = rows.Count > limit;
        var page = hasMore ? rows.Take(limit).ToList() : rows;
        return new UserNotificationListResponse
        {
            Notifications = page.Select(ToDto).ToArray(),
            LatestSeq = page.Count > 0 ? page[^1].Seq : since,
            HasMore = hasMore,
            UnreadCount = await mine.CountAsync(n => n.ReadAtMs == null, ct),
        };
    }

    /// <summary>Marks the caller's notifications read; another user's ids are ignored. Returns how many changed.</summary>
    public static async Task<int> MarkReadAsync(CentralApiDbContext db, Guid tenantId, Guid userId, MarkNotificationsReadRequest request, CancellationToken ct)
    {
        var now = NowMs();
        var query = db.UserNotifications.Where(n => n.TenantId == tenantId && n.UserId == userId && n.ReadAtMs == null);
        if (!request.All)
        {
            var ids = (request.Ids ?? []).Distinct().Take(MaxTake).ToArray();
            if (ids.Length == 0) return 0;
            query = query.Where(n => ids.Contains(n.Id));
        }
        var rows = await query.ToListAsync(ct);
        foreach (var row in rows) row.ReadAtMs = now;
        await db.SaveChangesAsync(ct);
        return rows.Count;
    }

    // ---- operations -------------------------------------------------------------------------

    private sealed class Rejected(string code, string message) : Exception(message)
    {
        public string Code { get; } = code;
    }

    private sealed class Batch
    {
        public required CentralApiDbContext Db { get; init; }
        public required Tenant Tenant { get; init; }
        public required MobileUser Actor { get; init; }
        public long Now { get; set; }
        public HashSet<Guid> Tasks { get; } = [];
        public HashSet<Guid> Series { get; } = [];
    }

    public async Task<TaskOpsResponse> ApplyAsync(CentralApiDbContext db, Tenant tenant, MobileUser actor, IReadOnlyList<TaskOp> ops, CancellationToken ct)
    {
        var batch = new Batch { Db = db, Tenant = tenant, Actor = actor, Now = NowMs() };
        var results = new List<TaskOpResult>(ops.Count);
        var relational = db.Database.IsRelational();
        await using (var transaction = relational ? await db.Database.BeginTransactionAsync(ct) : null)
        {
            foreach (var op in ops.Take(MaxOpsPerBatch))
            {
                if (op.OpId == Guid.Empty)
                {
                    results.Add(Result(op, "rejected", "TASK_OP_ID_REQUIRED", "Her işlemin bir kimliği olmalı."));
                    continue;
                }
                if (await db.WorkTaskOpsApplied.AnyAsync(a => a.TenantId == tenant.Id && a.OpId == op.OpId, ct))
                {
                    results.Add(Result(op, "duplicate"));
                    continue;
                }
                if (transaction is not null) await transaction.CreateSavepointAsync("task_op", ct);
                try
                {
                    batch.Now = NowMs();
                    await ApplyOneAsync(batch, op, ct);
                    db.WorkTaskOpsApplied.Add(new WorkTaskOpApplied { TenantId = tenant.Id, OpId = op.OpId, UserId = actor.Id, AppliedAtMs = batch.Now });
                    await db.SaveChangesAsync(ct);
                    results.Add(Result(op, "applied"));
                }
                catch (Rejected rejected)
                {
                    if (transaction is not null) await transaction.RollbackToSavepointAsync("task_op", ct);
                    db.ChangeTracker.Clear();
                    results.Add(Result(op, "rejected", rejected.Code, rejected.Message));
                }
            }
            if (transaction is not null) await transaction.CommitAsync(ct);
        }
        if (batch.Tasks.Count > 0 || batch.Series.Count > 0) Notify(tenant.Id);

        var now = NowMs();
        var tasks = batch.Tasks.Count == 0
            ? []
            : await db.WorkTasks.AsNoTracking()
                .Where(t => t.TenantId == tenant.Id && batch.Tasks.Contains(t.Id))
                .Include(t => t.Members).Include(t => t.Subtasks).AsSplitQuery()
                .ToListAsync(ct);
        var series = batch.Series.Count == 0
            ? []
            : await db.WorkTaskSeries.AsNoTracking().Where(s => s.TenantId == tenant.Id && batch.Series.Contains(s.Id)).ToListAsync(ct);
        return new TaskOpsResponse
        {
            Results = results.ToArray(),
            Tasks = await MapAsync(db, tasks.Where(t => CanSee(t, actor, now)).ToList(), actor, ct),
            Series = series.Select(ToDto).ToArray(),
        };
    }

    private static TaskOpResult Result(TaskOp op, string status, string? code = null, string? message = null) =>
        new() { OpId = op.OpId, Status = status, ErrorCode = code, Message = message };

    private async Task ApplyOneAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        switch (op.Type)
        {
            case "create_task": await CreateTaskAsync(b, op, ct); break;
            case "update_task": await UpdateTaskAsync(b, op, ct); break;
            case "set_members": await SetMembersAsync(b, op, ct); break;
            case "complete_task": await CompleteAsync(b, op, ct); break;
            case "reopen_task": await ReopenAsync(b, op, ct); break;
            case "cancel_task": await CancelAsync(b, op, ct); break;
            case "delete_task": await DeleteAsync(b, op, ct); break;
            case "add_subtask": await AddSubtaskAsync(b, op, ct); break;
            case "update_subtask": await UpdateSubtaskAsync(b, op, ct); break;
            case "toggle_subtask": await ToggleSubtaskAsync(b, op, ct); break;
            case "delete_subtask": await DeleteSubtaskAsync(b, op, ct); break;
            case "add_comment": await AddCommentAsync(b, op, ct); break;
            case "create_series": await CreateSeriesAsync(b, op, ct); break;
            case "update_series": await UpdateSeriesAsync(b, op, ct); break;
            case "stop_series": await StopSeriesAsync(b, op, ct); break;
            default: throw new Rejected("TASK_OP_UNKNOWN", $"Bilinmeyen işlem: {op.Type}.");
        }
    }

    private async Task CreateTaskAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var id = op.TaskId ?? throw new Rejected("TASK_ID_REQUIRED", "Görev kimliği gerekli.");
        if (await b.Db.WorkTasks.AnyAsync(t => t.Id == id, ct))
            throw new Rejected("TASK_EXISTS", "Bu kimlikle bir görev zaten var.");
        var title = Title(op.Title);
        var priority = Priority(op.Priority);
        var subtasks = op.Subtasks ?? [];
        if (subtasks.Length > MaxSubtasks) throw new Rejected("TASK_TOO_MANY_SUBTASKS", $"En çok {MaxSubtasks} alt görev olabilir.");
        var members = await MembersAsync(b, op.AssigneeIds, op.FollowerIds, ct);
        var subtaskUsers = await UsersAsync(b, subtasks.Where(s => s.AssigneeUserId is not null).Select(s => s.AssigneeUserId!.Value), ct);
        CheckCanAssign(b.Actor, subtaskUsers.Keys);

        var seq = await NextSeqAsync(b.Db, b.Tenant.Id, ct);
        var task = new WorkTask
        {
            Id = id,
            TenantId = b.Tenant.Id,
            Title = title,
            Description = Clip(op.Description, MaxDescriptionLength),
            Priority = priority,
            Status = WorkTaskStatuses.Open,
            CreatedByUserId = b.Actor.Id,
            CreatedByName = Clip(b.Actor.FullName, 120),
            CreatedAtMs = b.Now,
            UpdatedAtMs = b.Now,
            StartAtMs = op.StartAtMs,
            DueAtMs = op.DueAtMs,
            RequiresPhoto = op.RequiresPhoto ?? false,
            CustomerCode = ClipOrNull(op.CustomerCode, 64),
            CustomerName = ClipOrNull(op.CustomerName, 200),
            UpdatedSeq = seq,
        };
        task.Members.AddRange(members);
        var order = 0;
        foreach (var input in subtasks)
        {
            task.Subtasks.Add(new WorkTaskSubtask
            {
                Id = input.Id == Guid.Empty ? Guid.NewGuid() : input.Id,
                Title = SubtaskTitle(input.Title),
                AssigneeUserId = input.AssigneeUserId,
                AssigneeName = input.AssigneeUserId is { } a ? subtaskUsers[a].FullName : null,
                DueAtMs = input.DueAtMs,
                SortOrder = order++,
            });
        }
        b.Db.WorkTasks.Add(task);
        AddEvent(b, task, WorkTaskActions.Created, null);
        if (IsStarted(task, b.Now))
        {
            task.StartNotifiedAtMs = b.Now;
            AssignmentNotices(b, task, AssigneesOf(task), task.Subtasks, seq);
        }
        b.Tasks.Add(id);
    }

    private async Task UpdateTaskAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var task = await EditableAsync(b, op, ct);
        var title = Title(op.Title);
        var priority = Priority(op.Priority);
        var wasStarted = IsStarted(task, b.Now) || task.StartNotifiedAtMs is not null;
        if (task.DueAtMs != op.DueAtMs)
        {
            task.DueSoonNotifiedAtMs = null;
            task.OverdueNotifiedAtMs = null;
        }
        task.Title = title;
        task.Description = Clip(op.Description, MaxDescriptionLength);
        task.Priority = priority;
        task.StartAtMs = op.StartAtMs;
        task.DueAtMs = op.DueAtMs;
        task.RequiresPhoto = op.RequiresPhoto ?? false;
        task.CustomerCode = ClipOrNull(op.CustomerCode, 64);
        task.CustomerName = ClipOrNull(op.CustomerName, 200);
        var seq = await TouchAsync(b, task, ct);
        AddEvent(b, task, WorkTaskActions.Updated, null);
        // Moved from the future to now: the assignees hear of it now, not from the scheduler.
        if (!wasStarted && IsStarted(task, b.Now) && task.StartNotifiedAtMs is null)
        {
            task.StartNotifiedAtMs = b.Now;
            AssignmentNotices(b, task, AssigneesOf(task), task.Subtasks.Where(s => !s.IsDeleted), seq);
        }
        else if (!IsStarted(task, b.Now))
        {
            task.StartNotifiedAtMs = null;
        }
    }

    private async Task SetMembersAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var task = await EditableAsync(b, op, ct);
        var before = AssigneesOf(task).ToHashSet();
        var members = await MembersAsync(b, op.AssigneeIds, op.FollowerIds, ct);
        // Keep the rows that stay: removing and re-adding the same (task, user, role) key in one save
        // is two tracked instances of one key, which EF refuses.
        foreach (var gone in task.Members.Where(m => !members.Any(n => n.UserId == m.UserId && n.Role == m.Role)).ToList())
        {
            task.Members.Remove(gone);
            b.Db.WorkTaskMembers.Remove(gone);
        }
        foreach (var added in members.Where(n => !task.Members.Any(m => m.UserId == n.UserId && m.Role == n.Role)))
            task.Members.Add(added);
        var seq = await TouchAsync(b, task, ct);
        AddEvent(b, task, WorkTaskActions.MembersChanged,
            Clip("Atananlar: " + string.Join(", ", members.Where(m => m.Role == WorkTaskMemberRoles.Assignee).Select(m => m.UserName)), 500));
        if (task.StartNotifiedAtMs is not null || IsStarted(task, b.Now))
        {
            task.StartNotifiedAtMs ??= b.Now;
            AssignmentNotices(b, task, AssigneesOf(task).Where(id => !before.Contains(id)), [], seq);
        }
    }

    private async Task CompleteAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var task = await WorkableAsync(b, op, ct);
        if (task.Status == WorkTaskStatuses.Done) return;
        if (task.Status == WorkTaskStatuses.Cancelled) throw new Rejected("TASK_CLOSED", "İptal edilmiş görev tamamlanamaz.");
        if (task.RequiresPhoto && !await b.Db.WorkTaskAttachments.AnyAsync(a => a.TaskId == task.Id && !a.IsDeleted, ct))
            throw new Rejected("TASK_PHOTO_REQUIRED", "Bu görev fotoğraf eklenmeden tamamlanamaz.");
        task.Status = WorkTaskStatuses.Done;
        task.CompletedAtMs = b.Now;
        task.CompletedByUserId = b.Actor.Id;
        task.CompletedByName = Clip(b.Actor.FullName, 120);
        var seq = await TouchAsync(b, task, ct);
        AddEvent(b, task, WorkTaskActions.Completed, null);
        AddNotices(b, Recipients(task.CreatedByUserId, FollowersOf(task)), UserNotificationKinds.TaskCompleted,
            "Görev tamamlandı: " + task.Title, $"{b.Actor.FullName} görevi tamamladı.", task.Id, seq);
    }

    private async Task ReopenAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var task = await WorkableAsync(b, op, ct);
        if (task.Status == WorkTaskStatuses.Open) return;
        task.Status = WorkTaskStatuses.Open;
        task.CompletedAtMs = null;
        task.CompletedByUserId = null;
        task.CompletedByName = null;
        var seq = await TouchAsync(b, task, ct);
        AddEvent(b, task, WorkTaskActions.Reopened, Clip(op.Text, 500));
        AddNotices(b, AssigneesOf(task), UserNotificationKinds.TaskReopened,
            "Görev yeniden açıldı: " + task.Title, $"{b.Actor.FullName} görevi yeniden açtı.", task.Id, seq);
    }

    private async Task CancelAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var task = await EditableAsync(b, op, ct);
        if (task.Status == WorkTaskStatuses.Cancelled) return;
        task.Status = WorkTaskStatuses.Cancelled;
        await TouchAsync(b, task, ct);
        AddEvent(b, task, WorkTaskActions.Cancelled, Clip(op.Text, 500));
    }

    private async Task DeleteAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var task = await EditableAsync(b, op, ct, allowDeleted: true);
        if (task.IsDeleted) return;
        task.IsDeleted = true;
        task.DeletedAtMs = b.Now;
        await TouchAsync(b, task, ct);
        AddEvent(b, task, WorkTaskActions.Deleted, null);
    }

    private async Task AddSubtaskAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var task = await EditableAsync(b, op, ct);
        var id = op.SubtaskId ?? throw new Rejected("TASK_SUBTASK_ID_REQUIRED", "Alt görev kimliği gerekli.");
        if (task.Subtasks.Any(s => s.Id == id) || await b.Db.WorkTaskSubtasks.AnyAsync(s => s.Id == id, ct))
            throw new Rejected("TASK_SUBTASK_EXISTS", "Bu kimlikle bir alt görev zaten var.");
        if (task.Subtasks.Count(s => !s.IsDeleted) >= MaxSubtasks)
            throw new Rejected("TASK_TOO_MANY_SUBTASKS", $"En çok {MaxSubtasks} alt görev olabilir.");
        var title = SubtaskTitle(op.Title);
        var assignee = await OptionalUserAsync(b, op.AssigneeUserId, ct);
        var subtask = new WorkTaskSubtask
        {
            Id = id,
            TaskId = task.Id,
            Title = title,
            AssigneeUserId = assignee?.Id,
            AssigneeName = assignee?.FullName,
            DueAtMs = op.DueAtMs,
            SortOrder = op.SortOrder ?? (task.Subtasks.Count == 0 ? 0 : task.Subtasks.Max(s => s.SortOrder) + 1),
        };
        task.Subtasks.Add(subtask);
        var seq = await TouchAsync(b, task, ct);
        AddEvent(b, task, WorkTaskActions.SubtaskAdded, Clip(title, 500));
        if (task.StartNotifiedAtMs is not null) AssignmentNotices(b, task, [], [subtask], seq);
    }

    private async Task UpdateSubtaskAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var task = await EditableAsync(b, op, ct);
        var subtask = Subtask(task, op);
        var title = SubtaskTitle(op.Title);
        var assignee = await OptionalUserAsync(b, op.AssigneeUserId, ct);
        var newAssignee = assignee is not null && assignee.Id != subtask.AssigneeUserId;
        subtask.Title = title;
        subtask.AssigneeUserId = assignee?.Id;
        subtask.AssigneeName = assignee?.FullName;
        subtask.DueAtMs = op.DueAtMs;
        if (op.SortOrder is { } sort) subtask.SortOrder = sort;
        var seq = await TouchAsync(b, task, ct);
        AddEvent(b, task, WorkTaskActions.SubtaskChanged, Clip(title, 500));
        if (newAssignee && task.StartNotifiedAtMs is not null) AssignmentNotices(b, task, [], [subtask], seq);
    }

    private async Task ToggleSubtaskAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var task = await WorkableAsync(b, op, ct);
        var subtask = Subtask(task, op);
        var done = op.IsDone ?? !subtask.IsDone;
        if (subtask.IsDone == done) return;
        subtask.IsDone = done;
        subtask.DoneAtMs = done ? b.Now : null;
        subtask.DoneByUserId = done ? b.Actor.Id : null;
        subtask.DoneByName = done ? Clip(b.Actor.FullName, 120) : null;
        await TouchAsync(b, task, ct);
        AddEvent(b, task, done ? WorkTaskActions.SubtaskDone : WorkTaskActions.SubtaskUndone, Clip(subtask.Title, 500));
    }

    private async Task DeleteSubtaskAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var task = await EditableAsync(b, op, ct);
        var subtask = Subtask(task, op);
        subtask.IsDeleted = true;
        await TouchAsync(b, task, ct);
        AddEvent(b, task, WorkTaskActions.SubtaskDeleted, Clip(subtask.Title, 500));
    }

    private async Task AddCommentAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var task = await VisibleTaskAsync(b, op, ct);
        var id = op.CommentId ?? throw new Rejected("TASK_COMMENT_ID_REQUIRED", "Yorum kimliği gerekli.");
        var text = (op.Text ?? string.Empty).Trim();
        if (text.Length == 0) throw new Rejected("TASK_COMMENT_EMPTY", "Yorum boş olamaz.");
        if (text.Length > MaxCommentLength) throw new Rejected("TASK_COMMENT_TOO_LONG", $"Yorum en çok {MaxCommentLength} karakter olabilir.");
        if (await b.Db.WorkTaskComments.AnyAsync(c => c.Id == id, ct)) throw new Rejected("TASK_COMMENT_EXISTS", "Bu yorum zaten eklenmiş.");
        b.Db.WorkTaskComments.Add(new WorkTaskComment
        {
            Id = id,
            TenantId = b.Tenant.Id,
            TaskId = task.Id,
            AuthorUserId = b.Actor.Id,
            AuthorName = Clip(b.Actor.FullName, 120),
            Text = text,
            CreatedAtMs = b.Now,
        });
        var seq = await TouchAsync(b, task, ct);
        AddEvent(b, task, WorkTaskActions.Commented, null);
        AddNotices(b, Recipients(task.CreatedByUserId, AssigneesOf(task).Concat(FollowersOf(task))), UserNotificationKinds.TaskCommented,
            "Yeni yorum: " + task.Title, Clip($"{b.Actor.FullName}: {text}", 500), task.Id, seq);
    }

    private async Task CreateSeriesAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var id = op.SeriesId ?? throw new Rejected("TASK_SERIES_ID_REQUIRED", "Seri kimliği gerekli.");
        if (await b.Db.WorkTaskSeries.AnyAsync(s => s.Id == id, ct)) throw new Rejected("TASK_SERIES_EXISTS", "Bu kimlikle bir seri zaten var.");
        var series = new WorkTaskSeries
        {
            Id = id,
            TenantId = b.Tenant.Id,
            CreatedByUserId = b.Actor.Id,
            CreatedByName = Clip(b.Actor.FullName, 120),
            CreatedAtMs = b.Now,
        };
        await FillSeriesAsync(b, series, op, ct);
        b.Db.WorkTaskSeries.Add(series);
        b.Series.Add(id);
    }

    private async Task UpdateSeriesAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var series = await EditableSeriesAsync(b, op, ct);
        await FillSeriesAsync(b, series, op, ct);
        b.Series.Add(series.Id);
    }

    private async Task StopSeriesAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var series = await EditableSeriesAsync(b, op, ct);
        if (!series.IsActive) return;
        series.IsActive = false;
        series.UpdatedAtMs = b.Now;
        series.UpdatedSeq = await NextSeqAsync(b.Db, b.Tenant.Id, ct);
        b.Series.Add(series.Id);
    }

    private async Task FillSeriesAsync(Batch b, WorkTaskSeries series, TaskOp op, CancellationToken ct)
    {
        var frequency = op.Frequency ?? string.Empty;
        if (!WorkTaskFrequencies.All.Contains(frequency))
            throw new Rejected("TASK_SERIES_INVALID", "Tekrar sıklığı günlük, haftalık ya da aylık olmalı.");
        var members = await MembersAsync(b, op.AssigneeIds, op.FollowerIds, ct);
        var titles = (op.SubtaskTitles ?? []).Select(t => t?.Trim() ?? string.Empty).Where(t => t.Length > 0).Take(MaxSubtasks)
            .Select(t => Clip(t, MaxSubtaskTitleLength)).ToArray();
        series.Title = Title(op.Title);
        series.Description = Clip(op.Description, MaxDescriptionLength);
        series.Priority = Priority(op.Priority);
        series.RequiresPhoto = op.RequiresPhoto ?? false;
        series.CustomerCode = ClipOrNull(op.CustomerCode, 64);
        series.CustomerName = ClipOrNull(op.CustomerName, 200);
        series.AssigneesJson = MembersJson(members, WorkTaskMemberRoles.Assignee);
        series.FollowersJson = MembersJson(members, WorkTaskMemberRoles.Follower);
        series.SubtasksJson = JsonSerializer.Serialize(titles);
        series.Frequency = frequency;
        series.Interval = Math.Clamp(op.Interval ?? 1, 1, 52);
        series.Weekdays = (op.Weekdays ?? 0) & TaskSchedule.AllWeekdays;
        series.MonthDay = Math.Clamp(op.MonthDay ?? 1, 1, 31);
        series.TimeOfDayMinutes = Math.Clamp(op.TimeOfDayMinutes ?? 9 * 60, 0, 24 * 60 - 1);
        series.DueAfterMinutes = op.DueAfterMinutes is { } due ? Math.Clamp(due, 1, 60 * 24 * 60) : null;
        series.EndsAtMs = op.EndsAtMs;
        series.IsActive = true;
        series.UpdatedAtMs = b.Now;
        series.NextRunAtMs = TaskSchedule.NextRunAfter(series, b.Now)
            ?? throw new Rejected("TASK_SERIES_INVALID", "Bu kurala göre gelecekte oluşacak bir görev yok (gün seçimini ve bitiş tarihini kontrol edin).");
        series.UpdatedSeq = await NextSeqAsync(b.Db, b.Tenant.Id, ct);
    }

    // ---- attachments ------------------------------------------------------------------------

    public async Task<TaskResult<TaskAttachmentDto>> AddAttachmentAsync(
        CentralApiDbContext db, Tenant tenant, MobileUser user, Guid taskId, Guid attachmentId, string? contentType, byte[] data, CancellationToken ct)
    {
        var type = (contentType ?? string.Empty).Split(';')[0].Trim().ToLowerInvariant();
        if (!ImageTypes.Contains(type) || !LooksLike(type, data))
            return TaskResult<TaskAttachmentDto>.Fail(415, "TASK_ATTACHMENT_TYPE", "Yalnız JPEG, PNG ya da WEBP resim eklenebilir.");
        if (data.Length == 0 || data.Length > _options.MaxAttachmentBytes)
            return TaskResult<TaskAttachmentDto>.Fail(413, "TASK_ATTACHMENT_TOO_LARGE", $"Resim en çok {_options.MaxAttachmentBytes / 1024 / 1024} MB olabilir.");

        var existing = await db.WorkTaskAttachments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
        if (existing is not null)
        {
            return existing.TenantId == tenant.Id && existing.TaskId == taskId && !existing.IsDeleted
                ? TaskResult<TaskAttachmentDto>.Ok(ToDto(existing))
                : TaskResult<TaskAttachmentDto>.Fail(409, "TASK_ATTACHMENT_EXISTS", "Bu kimlikle başka bir resim var.");
        }

        var now = NowMs();
        var task = await LoadAsync(db, tenant.Id, taskId, tracking: true, ct);
        if (task is null || task.IsDeleted || !CanSee(task, user, now))
            return TaskResult<TaskAttachmentDto>.Fail(404, "TASK_NOT_FOUND", "Görev bulunamadı.");
        if (!CanWork(task, user))
            return TaskResult<TaskAttachmentDto>.Fail(403, "TASK_FORBIDDEN", "Bu göreve resim ekleyemezsiniz.");
        var count = await db.WorkTaskAttachments.CountAsync(a => a.TaskId == taskId && !a.IsDeleted, ct);
        if (count >= _options.MaxAttachmentsPerTask)
            return TaskResult<TaskAttachmentDto>.Fail(409, "TASK_ATTACHMENT_LIMIT", $"Bir göreve en çok {_options.MaxAttachmentsPerTask} resim eklenebilir.");
        var used = await db.WorkTaskAttachments.Where(a => a.TenantId == tenant.Id && !a.IsDeleted).SumAsync(a => (long)a.SizeBytes, ct);
        if (used + data.Length > _options.TenantAttachmentQuotaBytes)
            return TaskResult<TaskAttachmentDto>.Fail(413, "TASK_ATTACHMENT_QUOTA", "Firmanın resim depolama alanı doldu; eski görevlerin resimlerini silin.");

        var attachment = new WorkTaskAttachment
        {
            Id = attachmentId,
            TenantId = tenant.Id,
            TaskId = taskId,
            UploadedByUserId = user.Id,
            UploadedByName = Clip(user.FullName, 120),
            ContentType = type,
            SizeBytes = data.Length,
            CreatedAtMs = now,
        };
        await using (var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null)
        {
            db.WorkTaskAttachments.Add(attachment);
            db.WorkTaskAttachmentBlobs.Add(new WorkTaskAttachmentBlob { AttachmentId = attachmentId, Data = data });
            task.UpdatedSeq = await NextSeqAsync(db, tenant.Id, ct);
            task.UpdatedAtMs = now;
            db.WorkTaskEvents.Add(new WorkTaskEvent { TenantId = tenant.Id, TaskId = taskId, Action = WorkTaskActions.PhotoAdded, ActorUserId = user.Id, ActorName = Clip(user.FullName, 120), OccurredAtMs = now });
            await db.SaveChangesAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);
        }
        Notify(tenant.Id);
        return TaskResult<TaskAttachmentDto>.Ok(ToDto(attachment));
    }

    public async Task<TaskResult<bool>> DeleteAttachmentAsync(CentralApiDbContext db, Tenant tenant, MobileUser user, Guid taskId, Guid attachmentId, CancellationToken ct)
    {
        var now = NowMs();
        var task = await LoadAsync(db, tenant.Id, taskId, tracking: true, ct);
        if (task is null || !CanSee(task, user, now)) return TaskResult<bool>.Fail(404, "TASK_NOT_FOUND", "Görev bulunamadı.");
        var attachment = await db.WorkTaskAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskId == taskId && a.TenantId == tenant.Id, ct);
        if (attachment is null || attachment.IsDeleted) return TaskResult<bool>.Ok(true);
        if (attachment.UploadedByUserId != user.Id && !CanEdit(task, user))
            return TaskResult<bool>.Fail(403, "TASK_FORBIDDEN", "Bu resmi yalnız ekleyen ya da görevi yöneten silebilir.");
        await using (var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null)
        {
            attachment.IsDeleted = true;
            attachment.DeletedAtMs = now;
            task.UpdatedSeq = await NextSeqAsync(db, tenant.Id, ct);
            task.UpdatedAtMs = now;
            db.WorkTaskEvents.Add(new WorkTaskEvent { TenantId = tenant.Id, TaskId = taskId, Action = WorkTaskActions.PhotoDeleted, ActorUserId = user.Id, ActorName = Clip(user.FullName, 120), OccurredAtMs = now });
            await db.SaveChangesAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);
        }
        Notify(tenant.Id);
        return TaskResult<bool>.Ok(true);
    }

    public async Task<TaskResult<(byte[] Data, string ContentType)>> ReadAttachmentAsync(
        CentralApiDbContext db, Guid tenantId, MobileUser user, Guid taskId, Guid attachmentId, CancellationToken ct)
    {
        var task = await LoadAsync(db, tenantId, taskId, tracking: false, ct);
        if (task is null || !CanSee(task, user, NowMs()))
            return TaskResult<(byte[], string)>.Fail(404, "TASK_NOT_FOUND", "Görev bulunamadı.");
        var attachment = await db.WorkTaskAttachments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskId == taskId && a.TenantId == tenantId && !a.IsDeleted, ct);
        var blob = attachment is null ? null : await db.WorkTaskAttachmentBlobs.AsNoTracking().FirstOrDefaultAsync(x => x.AttachmentId == attachmentId, ct);
        return blob is null
            ? TaskResult<(byte[], string)>.Fail(404, "TASK_ATTACHMENT_NOT_FOUND", "Resim bulunamadı.")
            : TaskResult<(byte[], string)>.Ok((blob.Data, attachment!.ContentType));
    }

    /// <summary>A header that matches the declared type: a renamed file or a script is refused.</summary>
    internal static bool LooksLike(string type, byte[] data) => type switch
    {
        "image/jpeg" => data.Length > 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF,
        "image/png" => data.Length > 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47,
        "image/webp" => data.Length > 12 && data[0] == (byte)'R' && data[1] == (byte)'I' && data[2] == (byte)'F' && data[3] == (byte)'F'
            && data[8] == (byte)'W' && data[9] == (byte)'E' && data[10] == (byte)'B' && data[11] == (byte)'P',
        _ => false,
    };

    // ---- scheduler --------------------------------------------------------------------------

    /// <summary>
    /// One pass of the scheduler (goal §3): series occurrences, scheduled starts, "bitiş yaklaşıyor",
    /// "gecikti" and the 30-day clean-up. Every task is its own transaction. Returns how many things it did.
    /// </summary>
    public async Task<int> RunSchedulerAsync(CentralApiDbContext db, long nowMs, CancellationToken ct)
    {
        var woken = new HashSet<Guid>();
        var done = 0;

        var series = await db.WorkTaskSeries.Where(s => s.IsActive && s.NextRunAtMs <= nowMs).OrderBy(s => s.NextRunAtMs).Take(100).ToListAsync(ct);
        foreach (var item in series)
        {
            await InTransactionAsync(db, async () => await MaterializeAsync(db, item, nowMs, ct), ct);
            woken.Add(item.TenantId);
            done++;
        }

        var starting = await db.WorkTasks.Include(t => t.Members).Include(t => t.Subtasks).AsSplitQuery()
            .Where(t => t.Status == WorkTaskStatuses.Open && !t.IsDeleted && t.StartNotifiedAtMs == null && t.StartAtMs != null && t.StartAtMs <= nowMs)
            .Take(200).ToListAsync(ct);
        foreach (var task in starting)
        {
            await InTransactionAsync(db, async () =>
            {
                var seq = await NextSeqAsync(db, task.TenantId, ct);
                task.UpdatedSeq = seq;
                task.StartNotifiedAtMs = nowMs;
                var b = new Batch { Db = db, Tenant = new Tenant { Id = task.TenantId }, Actor = System(task.CreatedByName), Now = nowMs };
                AssignmentNotices(b, task, AssigneesOf(task), task.Subtasks.Where(s => !s.IsDeleted), seq, fromName: task.CreatedByName);
                await db.SaveChangesAsync(ct);
            }, ct);
            woken.Add(task.TenantId);
            done++;
        }

        var soonLimit = nowMs + _options.DueSoonMinutes * 60_000L;
        var dueSoon = await db.WorkTasks.Include(t => t.Members)
            .Where(t => t.Status == WorkTaskStatuses.Open && !t.IsDeleted && t.DueSoonNotifiedAtMs == null
                && t.DueAtMs != null && t.DueAtMs > nowMs && t.DueAtMs <= soonLimit && (t.StartAtMs == null || t.StartAtMs <= nowMs))
            .Take(200).ToListAsync(ct);
        foreach (var task in dueSoon)
        {
            await InTransactionAsync(db, async () =>
            {
                task.DueSoonNotifiedAtMs = nowMs;
                var seq = await NextSeqAsync(db, task.TenantId, ct);
                AddNoticesTo(db, task.TenantId, AssigneesOf(task), null, UserNotificationKinds.TaskDueSoon,
                    "Görevin süresi doluyor: " + task.Title, "Bitiş: " + TaskSchedule.Format(task.DueAtMs!.Value), task.Id, seq, nowMs);
                await db.SaveChangesAsync(ct);
            }, ct);
            woken.Add(task.TenantId);
            done++;
        }

        var overdue = await db.WorkTasks.Include(t => t.Members)
            .Where(t => t.Status == WorkTaskStatuses.Open && !t.IsDeleted && t.OverdueNotifiedAtMs == null
                && t.DueAtMs != null && t.DueAtMs <= nowMs && (t.StartAtMs == null || t.StartAtMs <= nowMs))
            .Take(200).ToListAsync(ct);
        foreach (var task in overdue)
        {
            await InTransactionAsync(db, async () =>
            {
                task.OverdueNotifiedAtMs = nowMs;
                task.DueSoonNotifiedAtMs ??= nowMs;
                var seq = await NextSeqAsync(db, task.TenantId, ct);
                AddNoticesTo(db, task.TenantId, Recipients(task.CreatedByUserId, AssigneesOf(task)), null, UserNotificationKinds.TaskOverdue,
                    "Görev gecikti: " + task.Title, "Bitiş tarihi geçti: " + TaskSchedule.Format(task.DueAtMs!.Value), task.Id, seq, nowMs);
                await db.SaveChangesAsync(ct);
            }, ct);
            woken.Add(task.TenantId);
            done++;
        }

        done += await PurgeAsync(db, nowMs, ct);
        foreach (var tenantId in woken) Notify(tenantId);
        return done;
    }

    /// <summary>Creates the series' due occurrence and moves the series to the one after it.</summary>
    private async Task MaterializeAsync(CentralApiDbContext db, WorkTaskSeries series, long nowMs, CancellationToken ct)
    {
        var runAt = series.NextRunAtMs;
        var assignees = ReadMembers(series.AssigneesJson);
        var followers = ReadMembers(series.FollowersJson);
        var activeIds = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == series.TenantId && u.IsActive && u.DeletedAtUtc == null)
            .Select(u => u.Id).ToListAsync(ct);
        var active = activeIds.ToHashSet();
        var seq = await NextSeqAsync(db, series.TenantId, ct);
        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            TenantId = series.TenantId,
            Title = series.Title,
            Description = series.Description,
            Priority = series.Priority,
            Status = WorkTaskStatuses.Open,
            CreatedByUserId = series.CreatedByUserId,
            CreatedByName = series.CreatedByName,
            CreatedAtMs = nowMs,
            UpdatedAtMs = nowMs,
            StartAtMs = runAt,
            DueAtMs = series.DueAfterMinutes is { } minutes ? runAt + minutes * 60_000L : null,
            RequiresPhoto = series.RequiresPhoto,
            CustomerCode = series.CustomerCode,
            CustomerName = series.CustomerName,
            SeriesId = series.Id,
            StartNotifiedAtMs = nowMs,
            UpdatedSeq = seq,
        };
        foreach (var m in assignees.Where(m => active.Contains(m.UserId)))
            task.Members.Add(new WorkTaskMember { UserId = m.UserId, UserName = m.Name, Role = WorkTaskMemberRoles.Assignee });
        foreach (var m in followers.Where(m => active.Contains(m.UserId) && assignees.All(a => a.UserId != m.UserId)))
            task.Members.Add(new WorkTaskMember { UserId = m.UserId, UserName = m.Name, Role = WorkTaskMemberRoles.Follower });
        var order = 0;
        foreach (var title in JsonSerializer.Deserialize<string[]>(series.SubtasksJson) ?? [])
            task.Subtasks.Add(new WorkTaskSubtask { Id = Guid.NewGuid(), Title = title, SortOrder = order++ });
        db.WorkTasks.Add(task);
        db.WorkTaskEvents.Add(new WorkTaskEvent { TenantId = task.TenantId, TaskId = task.Id, Action = WorkTaskActions.Created, ActorName = SystemActor, Detail = "Tekrarlayan görev", OccurredAtMs = nowMs });
        AddNoticesTo(db, task.TenantId, AssigneesOf(task), null, UserNotificationKinds.TaskAssigned,
            "Yeni görev: " + task.Title, AssignedBody(series.CreatedByName, task), task.Id, seq, nowMs);

        // A server that was down for several periods creates one occurrence, not a backlog.
        var next = TaskSchedule.NextRunAfter(series, Math.Max(runAt, nowMs));
        series.UpdatedAtMs = nowMs;
        series.UpdatedSeq = seq;
        if (next is { } n) series.NextRunAtMs = n;
        else series.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    private async Task<int> PurgeAsync(CentralApiDbContext db, long nowMs, CancellationToken ct)
    {
        if (!db.Database.IsRelational()) return 0;
        var cutoff = nowMs - _options.PurgeAfterDays * 24L * 60 * 60 * 1000;
        var deletedTasks = db.WorkTasks.Where(t => t.IsDeleted && t.DeletedAtMs != null && t.DeletedAtMs < cutoff).Select(t => t.Id);
        var removed = await db.WorkTaskAttachments
            .Where(a => (a.IsDeleted && a.DeletedAtMs != null && a.DeletedAtMs < cutoff) || deletedTasks.Contains(a.TaskId))
            .ExecuteDeleteAsync(ct);
        removed += await db.WorkTaskOpsApplied.Where(a => a.AppliedAtMs < cutoff).ExecuteDeleteAsync(ct);
        return removed;
    }

    private static async Task InTransactionAsync(CentralApiDbContext db, Func<Task> work, CancellationToken ct)
    {
        if (!db.Database.IsRelational())
        {
            await work();
            return;
        }
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await work();
        await transaction.CommitAsync(ct);
    }

    // ---- helpers ----------------------------------------------------------------------------

    private static Task<WorkTask?> LoadAsync(CentralApiDbContext db, Guid tenantId, Guid id, bool tracking, CancellationToken ct)
    {
        var query = db.WorkTasks.Where(t => t.TenantId == tenantId && t.Id == id).Include(t => t.Members).Include(t => t.Subtasks).AsSplitQuery();
        return (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(ct);
    }

    private static async Task<WorkTask> VisibleTaskAsync(Batch b, TaskOp op, CancellationToken ct, bool allowDeleted = false)
    {
        var id = op.TaskId ?? throw new Rejected("TASK_ID_REQUIRED", "Görev kimliği gerekli.");
        var task = await LoadAsync(b.Db, b.Tenant.Id, id, tracking: true, ct);
        if (task is null || !CanSee(task, b.Actor, b.Now)) throw new Rejected("TASK_NOT_FOUND", "Görev bulunamadı.");
        if (task.IsDeleted && !allowDeleted) throw new Rejected("TASK_DELETED", "Görev silinmiş.");
        return task;
    }

    private static async Task<WorkTask> EditableAsync(Batch b, TaskOp op, CancellationToken ct, bool allowDeleted = false)
    {
        var task = await VisibleTaskAsync(b, op, ct, allowDeleted);
        if (!CanEdit(task, b.Actor)) throw new Rejected("TASK_FORBIDDEN", "Bu görevi yalnız oluşturan ya da yönetici değiştirebilir.");
        return task;
    }

    private static async Task<WorkTask> WorkableAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var task = await VisibleTaskAsync(b, op, ct);
        if (!CanWork(task, b.Actor)) throw new Rejected("TASK_FORBIDDEN", "Bu görevde işlem yapma yetkiniz yok.");
        return task;
    }

    private static async Task<WorkTaskSeries> EditableSeriesAsync(Batch b, TaskOp op, CancellationToken ct)
    {
        var id = op.SeriesId ?? throw new Rejected("TASK_SERIES_ID_REQUIRED", "Seri kimliği gerekli.");
        var series = await b.Db.WorkTaskSeries.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == b.Tenant.Id, ct)
            ?? throw new Rejected("TASK_SERIES_NOT_FOUND", "Tekrarlayan görev bulunamadı.");
        if (!CanManage(b.Actor) && series.CreatedByUserId != b.Actor.Id)
            throw new Rejected("TASK_FORBIDDEN", "Bu tekrarlayan görevi yalnız oluşturan ya da yönetici değiştirebilir.");
        return series;
    }

    private static WorkTaskSubtask Subtask(WorkTask task, TaskOp op)
    {
        var id = op.SubtaskId ?? throw new Rejected("TASK_SUBTASK_ID_REQUIRED", "Alt görev kimliği gerekli.");
        return task.Subtasks.FirstOrDefault(s => s.Id == id && !s.IsDeleted) ?? throw new Rejected("TASK_SUBTASK_NOT_FOUND", "Alt görev bulunamadı.");
    }

    private static async Task<long> TouchAsync(Batch b, WorkTask task, CancellationToken ct)
    {
        var seq = await NextSeqAsync(b.Db, b.Tenant.Id, ct);
        task.UpdatedSeq = seq;
        task.UpdatedAtMs = b.Now;
        b.Tasks.Add(task.Id);
        return seq;
    }

    private static async Task<List<WorkTaskMember>> MembersAsync(Batch b, Guid[]? assigneeIds, Guid[]? followerIds, CancellationToken ct)
    {
        var assignees = (assigneeIds ?? []).Where(id => id != Guid.Empty).Distinct().ToArray();
        var followers = (followerIds ?? []).Where(id => id != Guid.Empty && !assignees.Contains(id)).Distinct().ToArray();
        if (assignees.Length > MaxMembers || followers.Length > MaxMembers)
            throw new Rejected("TASK_TOO_MANY_MEMBERS", $"En çok {MaxMembers} atanan ve {MaxMembers} takipçi olabilir.");
        var users = await UsersAsync(b, assignees.Concat(followers), ct);
        CheckCanAssign(b.Actor, assignees);
        return assignees.Select(id => new WorkTaskMember { UserId = id, UserName = users[id].FullName, Role = WorkTaskMemberRoles.Assignee })
            .Concat(followers.Select(id => new WorkTaskMember { UserId = id, UserName = users[id].FullName, Role = WorkTaskMemberRoles.Follower }))
            .ToList();
    }

    private static async Task<MobileUser?> OptionalUserAsync(Batch b, Guid? id, CancellationToken ct)
    {
        if (id is not { } userId || userId == Guid.Empty) return null;
        var users = await UsersAsync(b, [userId], ct);
        CheckCanAssign(b.Actor, [userId]);
        return users[userId];
    }

    /// <summary>Active users of the tenant; an unknown, inactive or deleted one rejects the operation.</summary>
    private static async Task<Dictionary<Guid, MobileUser>> UsersAsync(Batch b, IEnumerable<Guid> ids, CancellationToken ct)
    {
        var wanted = ids.Distinct().ToArray();
        if (wanted.Length == 0) return [];
        var users = await b.Db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == b.Tenant.Id && wanted.Contains(u.Id) && u.IsActive && u.DeletedAtUtc == null)
            .ToDictionaryAsync(u => u.Id, ct);
        if (users.Count != wanted.Length) throw new Rejected("TASK_UNKNOWN_USER", "Seçilen kişilerden biri artık aktif değil.");
        return users;
    }

    /// <summary>Goal K3: a manager assigns anyone, everybody else only themselves.</summary>
    private static void CheckCanAssign(MobileUser actor, IEnumerable<Guid> assignees)
    {
        if (CanManage(actor)) return;
        if (assignees.Any(id => id != actor.Id))
            throw new Rejected("TASK_ASSIGN_FORBIDDEN", "Başkasına görev yalnız yöneticiler atayabilir.");
    }

    private static IEnumerable<Guid> AssigneesOf(WorkTask task) =>
        task.Members.Where(m => m.Role == WorkTaskMemberRoles.Assignee).Select(m => m.UserId);

    private static IEnumerable<Guid> FollowersOf(WorkTask task) =>
        task.Members.Where(m => m.Role == WorkTaskMemberRoles.Follower).Select(m => m.UserId);

    private static IEnumerable<Guid> Recipients(Guid first, IEnumerable<Guid> rest) => rest.Prepend(first);

    private static void AssignmentNotices(Batch b, WorkTask task, IEnumerable<Guid> assignees, IEnumerable<WorkTaskSubtask> subtasks, long seq, string? fromName = null)
    {
        var from = fromName ?? b.Actor.FullName;
        var told = new HashSet<Guid>();
        foreach (var id in assignees.Where(id => id != b.Actor.Id && told.Add(id)))
            AddNoticesTo(b.Db, task.TenantId, [id], null, UserNotificationKinds.TaskAssigned, "Yeni görev: " + task.Title, AssignedBody(from, task), task.Id, seq, b.Now);
        foreach (var subtask in subtasks.Where(s => s.AssigneeUserId is { } a && a != b.Actor.Id))
            AddNoticesTo(b.Db, task.TenantId, [subtask.AssigneeUserId!.Value], null, UserNotificationKinds.TaskAssigned,
                "Yeni alt görev: " + subtask.Title, $"{from} · {task.Title}", task.Id, seq, b.Now);
    }

    private static string AssignedBody(string from, WorkTask task) =>
        task.DueAtMs is { } due ? $"{from} size bir görev atadı. Bitiş: {TaskSchedule.Format(due)}" : $"{from} size bir görev atadı.";

    private static void AddNotices(Batch b, IEnumerable<Guid> recipients, string kind, string title, string body, Guid taskId, long seq) =>
        AddNoticesTo(b.Db, b.Tenant.Id, recipients, b.Actor.Id, kind, title, body, taskId, seq, b.Now);

    /// <summary>One notification per distinct recipient; the person who did the thing is never told about it.</summary>
    private static void AddNoticesTo(CentralApiDbContext db, Guid tenantId, IEnumerable<Guid> recipients, Guid? except,
        string kind, string title, string body, Guid? taskId, long seq, long nowMs)
    {
        foreach (var userId in recipients.Where(id => id != Guid.Empty && id != except).Distinct())
        {
            db.UserNotifications.Add(new UserNotification
            {
                TenantId = tenantId,
                UserId = userId,
                Kind = kind,
                Title = Clip(title, 200),
                Body = Clip(body, 500),
                TaskId = taskId,
                CreatedAtMs = nowMs,
                Seq = seq,
            });
        }
    }

    private static void AddEvent(Batch b, WorkTask task, string action, string? detail) =>
        b.Db.WorkTaskEvents.Add(new WorkTaskEvent
        {
            TenantId = task.TenantId,
            TaskId = task.Id,
            Action = action,
            ActorUserId = b.Actor.Id,
            ActorName = Clip(b.Actor.FullName, 120),
            Detail = detail,
            OccurredAtMs = b.Now,
        });

    private static MobileUser System(string name) => new() { Id = Guid.Empty, FullName = name };

    private static string Title(string? value)
    {
        var title = (value ?? string.Empty).Trim();
        if (title.Length == 0) throw new Rejected("TASK_TITLE_REQUIRED", "Görev başlığı boş olamaz.");
        return Clip(title, MaxTitleLength);
    }

    private static string SubtaskTitle(string? value)
    {
        var title = (value ?? string.Empty).Trim();
        if (title.Length == 0) throw new Rejected("TASK_SUBTASK_TITLE_REQUIRED", "Alt görev başlığı boş olamaz.");
        return Clip(title, MaxSubtaskTitleLength);
    }

    private static string Priority(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return WorkTaskPriorities.Normal;
        var priority = value.Trim().ToUpperInvariant();
        return WorkTaskPriorities.All.Contains(priority) ? priority : throw new Rejected("TASK_PRIORITY_INVALID", "Öncelik LOW, NORMAL, HIGH ya da URGENT olmalı.");
    }

    private static string Clip(string? value, int max)
    {
        var trimmed = (value ?? string.Empty).Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }

    private static string? ClipOrNull(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : Clip(value, max);

    private sealed record StoredMember(Guid UserId, string Name);

    private static string MembersJson(IEnumerable<WorkTaskMember> members, string role) =>
        JsonSerializer.Serialize(members.Where(m => m.Role == role).Select(m => new StoredMember(m.UserId, m.UserName)));

    private static List<StoredMember> ReadMembers(string json) =>
        JsonSerializer.Deserialize<List<StoredMember>>(json) ?? [];

    private static async Task<long> NextSeqAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct) =>
        db.Database.IsRelational()
            ? await MobileRecordProjector.ReserveAsync(db, tenantId, 1, ct)
            // The in-memory host has no transactions or counter row; time order is enough there.
            : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000 + Interlocked.Increment(ref _inMemorySeq) % 1000;

    private static long _inMemorySeq;

    // ---- mapping ----------------------------------------------------------------------------

    private static async Task<TaskDto[]> MapAsync(CentralApiDbContext db, IReadOnlyList<WorkTask> tasks, MobileUser viewer, CancellationToken ct)
    {
        if (tasks.Count == 0) return [];
        var ids = tasks.Select(t => t.Id).ToArray();
        var attachments = (await db.WorkTaskAttachments.AsNoTracking().Where(a => ids.Contains(a.TaskId) && !a.IsDeleted).ToListAsync(ct))
            .GroupBy(a => a.TaskId).ToDictionary(g => g.Key, g => g.OrderBy(a => a.CreatedAtMs).ToList());
        var comments = await db.WorkTaskComments.AsNoTracking().Where(c => ids.Contains(c.TaskId) && !c.IsDeleted)
            .GroupBy(c => c.TaskId).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        return tasks.Select(t =>
        {
            var dto = new TaskDto();
            Fill(dto, t, attachments.GetValueOrDefault(t.Id) ?? [], comments.GetValueOrDefault(t.Id), viewer);
            return dto;
        }).ToArray();
    }

    private static async Task FillAsync(CentralApiDbContext db, TaskDto dto, WorkTask task, MobileUser viewer, CancellationToken ct)
    {
        var attachments = await db.WorkTaskAttachments.AsNoTracking().Where(a => a.TaskId == task.Id && !a.IsDeleted).OrderBy(a => a.CreatedAtMs).ToListAsync(ct);
        var comments = await db.WorkTaskComments.AsNoTracking().CountAsync(c => c.TaskId == task.Id && !c.IsDeleted, ct);
        Fill(dto, task, attachments, comments, viewer);
    }

    private static void Fill(TaskDto dto, WorkTask t, IEnumerable<WorkTaskAttachment> attachments, int commentCount, MobileUser viewer)
    {
        dto.Id = t.Id;
        dto.Title = t.Title;
        dto.Description = t.Description;
        dto.Priority = t.Priority;
        dto.Status = t.Status;
        dto.CreatedByUserId = t.CreatedByUserId;
        dto.CreatedByName = t.CreatedByName;
        dto.CreatedAtMs = t.CreatedAtMs;
        dto.UpdatedAtMs = t.UpdatedAtMs;
        dto.StartAtMs = t.StartAtMs;
        dto.DueAtMs = t.DueAtMs;
        dto.CompletedAtMs = t.CompletedAtMs;
        dto.CompletedByName = t.CompletedByName;
        dto.RequiresPhoto = t.RequiresPhoto;
        dto.CustomerCode = t.CustomerCode;
        dto.CustomerName = t.CustomerName;
        dto.SeriesId = t.SeriesId;
        dto.IsDeleted = t.IsDeleted;
        dto.UpdatedSeq = t.UpdatedSeq;
        dto.Assignees = t.Members.Where(m => m.Role == WorkTaskMemberRoles.Assignee).Select(m => new TaskMemberDto { UserId = m.UserId, Name = m.UserName }).ToArray();
        dto.Followers = t.Members.Where(m => m.Role == WorkTaskMemberRoles.Follower).Select(m => new TaskMemberDto { UserId = m.UserId, Name = m.UserName }).ToArray();
        dto.Subtasks = t.Subtasks.Where(s => !s.IsDeleted).OrderBy(s => s.SortOrder).Select(s => new TaskSubtaskDto
        {
            Id = s.Id,
            Title = s.Title,
            IsDone = s.IsDone,
            DoneByName = s.DoneByName,
            DoneAtMs = s.DoneAtMs,
            AssigneeUserId = s.AssigneeUserId,
            AssigneeName = s.AssigneeName,
            DueAtMs = s.DueAtMs,
            SortOrder = s.SortOrder,
        }).ToArray();
        dto.Attachments = attachments.Select(ToDto).ToArray();
        dto.CommentCount = commentCount;
        dto.CanEdit = CanEdit(t, viewer);
        dto.CanWork = CanWork(t, viewer);
    }

    private static TaskAttachmentDto ToDto(WorkTaskAttachment a) => new()
    {
        Id = a.Id,
        ContentType = a.ContentType,
        SizeBytes = a.SizeBytes,
        UploadedByUserId = a.UploadedByUserId,
        UploadedByName = a.UploadedByName,
        CreatedAtMs = a.CreatedAtMs,
    };

    private static UserNotificationDto ToDto(UserNotification n) => new()
    {
        Id = n.Id,
        Kind = n.Kind,
        Title = n.Title,
        Body = n.Body,
        TaskId = n.TaskId,
        CreatedAtMs = n.CreatedAtMs,
        ReadAtMs = n.ReadAtMs,
        Seq = n.Seq,
    };

    private static TaskSeriesDto ToDto(WorkTaskSeries s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Description = s.Description,
        Priority = s.Priority,
        RequiresPhoto = s.RequiresPhoto,
        CustomerCode = s.CustomerCode,
        CustomerName = s.CustomerName,
        Assignees = ReadMembers(s.AssigneesJson).Select(m => new TaskMemberDto { UserId = m.UserId, Name = m.Name }).ToArray(),
        Followers = ReadMembers(s.FollowersJson).Select(m => new TaskMemberDto { UserId = m.UserId, Name = m.Name }).ToArray(),
        SubtaskTitles = JsonSerializer.Deserialize<string[]>(s.SubtasksJson) ?? [],
        Frequency = s.Frequency,
        Interval = s.Interval,
        Weekdays = s.Weekdays,
        MonthDay = s.MonthDay,
        TimeOfDayMinutes = s.TimeOfDayMinutes,
        DueAfterMinutes = s.DueAfterMinutes,
        NextRunAtMs = s.NextRunAtMs,
        EndsAtMs = s.EndsAtMs,
        IsActive = s.IsActive,
        CreatedByName = s.CreatedByName,
        UpdatedSeq = s.UpdatedSeq,
    };
}
