using ErpBridge.Portal.Api;

namespace ErpBridge.Portal.Shared.Tasks;

/// <summary>
/// The task form's state (GOAL_PANEL_GIRIS P6) and the ops it becomes: a new task is one <c>create_task</c> (with its
/// subtasks); an edited one is <c>update_task</c> — the whole editable state, a null meaning "empty" (GOAL_GOREVLER §3) —
/// and <c>set_members</c> when the people changed. Times are Istanbul wall clock in the form, unix ms on the wire.
/// </summary>
public sealed class TaskEditorForm
{
    public Guid? TaskId { get; init; }

    /// <summary>A new task's (or series') id: made with the form, kept across its sends.</summary>
    public Guid NewId { get; } = Guid.NewGuid();

    /// <summary>
    /// The id of the form's op, made once and kept while a send has no answer (Codex #253): sent again it is the same op —
    /// the server answers a known op id <c>duplicate</c> — never a second task. <see cref="Answered"/> makes new ones: an op the
    /// server applied must not swallow a later edit as its duplicate.
    /// </summary>
    public Guid OpId { get; private set; } = Guid.NewGuid();

    private Guid _membersOpId = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "NORMAL";
    public string Start { get; set; } = string.Empty;
    public string Due { get; set; } = string.Empty;
    public bool RequiresPhoto { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public bool VisitReminder { get; set; }
    public HashSet<Guid> Assignees { get; set; } = [];
    public HashSet<Guid> Followers { get; set; } = [];

    /// <summary>New subtasks of a new task, one title per line.</summary>
    public string SubtaskLines { get; set; } = string.Empty;

    private HashSet<Guid> _assigneesBefore = [];
    private HashSet<Guid> _followersBefore = [];

    public static TaskEditorForm New(Guid? me) => new() { Assignees = me is { } self ? [self] : [] };

    /// <summary>The server answered the last send (applied or refused): the next send is new ops.</summary>
    public void Answered()
    {
        OpId = Guid.NewGuid();
        _membersOpId = Guid.NewGuid();
    }

    public static TaskEditorForm From(TaskDto task)
    {
        var form = new TaskEditorForm
        {
            TaskId = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = string.IsNullOrEmpty(task.Priority) ? "NORMAL" : task.Priority,
            Start = Fmt.LocalInput(task.StartAtMs),
            Due = Fmt.LocalInput(task.DueAtMs),
            RequiresPhoto = task.RequiresPhoto,
            CustomerCode = task.CustomerCode,
            CustomerName = task.CustomerName,
            VisitReminder = task.VisitReminder,
            Assignees = [.. task.Assignees.Select(a => a.UserId)],
            Followers = [.. task.Followers.Select(f => f.UserId)],
        };
        form._assigneesBefore = [.. form.Assignees];
        form._followersBefore = [.. form.Followers];
        return form;
    }

    public IReadOnlyList<TaskOp> ToOps()
    {
        var customer = string.IsNullOrWhiteSpace(CustomerCode) ? null : CustomerCode.Trim();
        if (TaskId is not { } id)
        {
            return
            [
                new TaskOp
                {
                    OpId = OpId,
                    Type = "create_task",
                    TaskId = NewId,
                    Title = Title.Trim(),
                    Description = Description,
                    Priority = Priority,
                    StartAtMs = Fmt.FromLocalInput(Start),
                    DueAtMs = Fmt.FromLocalInput(Due),
                    RequiresPhoto = RequiresPhoto,
                    CustomerCode = customer,
                    CustomerName = customer is null ? null : CustomerName,
                    VisitReminder = customer is not null && VisitReminder,
                    AssigneeIds = [.. Assignees],
                    FollowerIds = [.. Followers],
                    Subtasks = [.. SubtaskLines.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(title => new TaskSubtaskInput { Id = Guid.NewGuid(), Title = title })],
                },
            ];
        }

        var ops = new List<TaskOp>
        {
            new()
            {
                OpId = OpId,
                Type = "update_task",
                TaskId = id,
                Title = Title.Trim(),
                Description = Description,
                Priority = Priority,
                StartAtMs = Fmt.FromLocalInput(Start),
                DueAtMs = Fmt.FromLocalInput(Due),
                RequiresPhoto = RequiresPhoto,
                CustomerCode = customer,
                CustomerName = customer is null ? null : CustomerName,
                VisitReminder = customer is not null && VisitReminder,
            },
        };
        if (!Assignees.SetEquals(_assigneesBefore) || !Followers.SetEquals(_followersBefore))
            ops.Add(new TaskOp { OpId = _membersOpId, Type = "set_members", TaskId = id, AssigneeIds = [.. Assignees], FollowerIds = [.. Followers] });
        return ops;
    }
}
