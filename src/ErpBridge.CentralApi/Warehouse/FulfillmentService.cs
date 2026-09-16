using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Native;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Sync;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Warehouse;

/// <summary>Outcome of a warehouse operation: a value, or an HTTP status with a stable error code.</summary>
public sealed record WarehouseResult<T>(T? Value, int StatusCode, ApiError? Error)
{
    public static WarehouseResult<T> Ok(T value, int status = 200) => new(value, status, null);

    public static WarehouseResult<T> Fail(int status, string code, string message) =>
        new(default, status, new ApiError { ErrorCode = code, Message = message });

    public bool Succeeded => Error is null;
}

/// <summary>
/// The warehouse queue (plan step 4, Faz 47): sales orders enter it once they are booked — straight
/// from the phone when the sale needs no approval, or when an approver approves it — and staff move
/// them through PENDING → PREPARING → PACKED → LOADED.
///
/// <para><b>One writer per step.</b> Every move is a conditional update from the status the caller saw
/// (<c>WHERE Status = expected</c>): of two people pressing "Başla" at once exactly one succeeds, the
/// other gets 409 <c>FULFILLMENT_STATE_CHANGED</c> naming who was first. Every move adds an
/// <see cref="OrderFulfillmentEvent"/>; the history is never changed.</para>
///
/// <para><b>Change order.</b> <see cref="OrderFulfillment.UpdatedSeq"/> comes from the tenant's
/// <c>tenant_sync_counter</c> inside the writing transaction (rule 11), so the portal's
/// <c>changedSinceSeq</c> never skips a change that commits late. Callers commit, then call
/// <see cref="Notify"/>.</para>
/// </summary>
public sealed class FulfillmentService
{
    /// <summary>How long the person who took a step may take it back without a manager.</summary>
    public static readonly TimeSpan UndoWindow = TimeSpan.FromMinutes(5);

    public const int MaxNoteLength = 500;
    public const int MaxPlateLength = 16;
    public const int MaxListSize = 500;

    /// <summary>Back-filling looks at most this many days back (panel goal D3) and queues at most this many orders.</summary>
    public const int MaxBackfillDays = 30;
    public const int MaxBackfillOrders = 2000;

    private const string SystemActor = "Sistem";

    private readonly ITenantEventHub _events;

    public FulfillmentService(ITenantEventHub events) => _events = events;

    /// <summary>Only sales orders are prepared by the warehouse.</summary>
    public static bool IsQueuedDocument(string documentType) =>
        string.Equals(documentType, NativeDocumentProcessor.SalesOrder, StringComparison.OrdinalIgnoreCase);

    /// <summary>Wakes the portal pages waiting on the company's queue. Call after commit.</summary>
    public void Notify(Guid tenantId) => _events.Publish(tenantId, TenantEventTopics.Warehouse);

    // ---- queueing ---------------------------------------------------------------------

    /// <summary>
    /// Queues a booked sales order when the company uses the warehouse module. Joins the caller's
    /// transaction (required on a relational database) and adds without saving. Returns null when
    /// nothing was queued: another document type, the module is off, the native booking failed, or
    /// the job is already queued.
    /// </summary>
    public async Task<OrderFulfillment?> EnqueueAsync(
        CentralApiDbContext db, Tenant tenant, Job job, Guid? approvalRequestId, CancellationToken ct, DateTimeOffset? queuedAtUtc = null)
    {
        if (!IsQueuedDocument(job.DocumentType)) return null;
        var native = tenant.DataSource == TenantDataSources.Native;
        // A sale the ledger refused never reaches the customer; an ERP job waits for the agent (V2).
        if (native && job.Status != JobStatus.Succeeded) return null;
        if (!await IsEnabledAsync(db, tenant.Id, ct)) return null;
        if (db.ChangeTracker.Entries<OrderFulfillment>().Any(e => e.Entity.SourceJobId == job.Id)
            || await db.OrderFulfillments.AnyAsync(f => f.TenantId == tenant.Id && f.SourceJobId == job.Id, ct))
            return null;

        var order = ReadOrder(job);
        var salesperson = job.CreatedByUserId is { } userId
            ? await db.MobileUsers.AsNoTracking().Where(u => u.Id == userId).Select(u => u.FullName).FirstOrDefaultAsync(ct)
            : null;
        var now = DateTimeOffset.UtcNow;
        var seq = await NextSeqAsync(db, tenant.Id, ct);
        var fulfillment = new OrderFulfillment
        {
            TenantId = tenant.Id,
            SourceJobId = job.Id,
            ApprovalRequestId = approvalRequestId,
            OrderNo = Clip(order.OrderNo ?? job.ExternalId, 64),
            CustomerCode = Clip(order.CustomerCode ?? string.Empty, 64),
            CustomerName = Clip(order.CustomerName ?? string.Empty, 200),
            SalespersonUserId = job.CreatedByUserId,
            SalespersonName = Clip(salesperson ?? string.Empty, 120),
            Amount = decimal.Round(order.Amount, 2),
            LineCount = order.Items.Count,
            ItemQuantity = order.Items.Sum(i => i.Quantity),
            ItemsJson = JsonSerializer.Serialize(order.Items, ItemJson),
            Status = FulfillmentStatuses.Pending,
            QueuedAtUtc = queuedAtUtc ?? now,
            QueuedSeq = seq,
            ErpState = native ? FulfillmentErpStates.None : FulfillmentErpStates.Pending,
            UpdatedSeq = seq,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        db.OrderFulfillments.Add(fulfillment);
        AddEvent(db, fulfillment, null, FulfillmentActions.Queued, actor: null, deviceId: null, note: null, now);
        return fulfillment;
    }

    /// <summary>
    /// Follows the agent's result for an ERP company's order (V2): written, failed (with an
    /// <c>ERP_FAILED</c> event carrying the error), or pending again after a retry. Joins the caller's
    /// transaction and adds without saving; returns whether the order changed.
    /// </summary>
    public async Task<bool> RecordErpResultAsync(CentralApiDbContext db, Job job, CancellationToken ct)
    {
        if (!IsQueuedDocument(job.DocumentType)) return false;
        var fulfillment = await db.OrderFulfillments.FirstOrDefaultAsync(f => f.TenantId == job.TenantId && f.SourceJobId == job.Id, ct);
        if (fulfillment is null || fulfillment.ErpState == FulfillmentErpStates.None) return false;
        var state = job.Status switch
        {
            JobStatus.Succeeded => FulfillmentErpStates.Written,
            JobStatus.Failed or JobStatus.DeadLetter => FulfillmentErpStates.Failed,
            _ => FulfillmentErpStates.Pending,
        };
        if (fulfillment.ErpState == state) return false;

        var now = DateTimeOffset.UtcNow;
        fulfillment.ErpState = state;
        fulfillment.UpdatedSeq = await NextSeqAsync(db, job.TenantId, ct);
        fulfillment.UpdatedAtUtc = now;
        if (state == FulfillmentErpStates.Failed)
            AddEvent(db, fulfillment, fulfillment.Status, FulfillmentActions.ErpFailed, actor: null, deviceId: null, CleanNote(job.LastError), now);
        return true;
    }

    /// <summary>
    /// Queues the sales orders of the last <paramref name="days"/> days that are not queued yet — for a
    /// company that has just turned the module on, whose recent orders arrived while it was off
    /// (panel goal P4a, D3). Each order waits from the moment its document arrived. Native orders
    /// count only when booked; an ERP order carries the state its job has reached (written, failed or
    /// still on its way). Pending or rejected approval requests are not jobs, so they are never
    /// queued. Idempotent: an order already in the queue is skipped.
    /// </summary>
    public async Task<WarehouseResult<WarehouseBackfillResponse>> BackfillAsync(
        CentralApiDbContext db, Tenant tenant, MobileUser actor, int days, CancellationToken ct)
    {
        if (!RolePermissions.CanManageWarehouse(actor))
            return WarehouseResult<WarehouseBackfillResponse>.Fail(403, "WAREHOUSE_MANAGER_REQUIRED", "Only administrators and managers can fill the warehouse queue.");
        if (days is < 0 or > MaxBackfillDays)
            return WarehouseResult<WarehouseBackfillResponse>.Fail(400, "INVALID_BACKFILL_DAYS", $"days must be 0-{MaxBackfillDays}.");
        if (!await IsEnabledAsync(db, tenant.Id, ct))
            return WarehouseResult<WarehouseBackfillResponse>.Fail(409, "WAREHOUSE_DISABLED", "Turn the warehouse module on before filling its queue.");
        if (days == 0) return WarehouseResult<WarehouseBackfillResponse>.Ok(new WarehouseBackfillResponse { Days = 0 });

        var since = DateTimeOffset.UtcNow.AddDays(-days);
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        // Take the company's counter lock before choosing: a second back-fill (or a sale being queued) waits
        // here and then sees these orders as queued, instead of racing into the unique index (Codex, PR #63).
        await NextSeqAsync(db, tenant.Id, ct);

        var salesOrder = NativeDocumentProcessor.SalesOrder;
        var query = db.Jobs.AsNoTracking()
            // Ingest accepts any casing of the document type (IsQueuedDocument compares case-insensitively).
            .Where(j => j.TenantId == tenant.Id && j.DocumentType.ToLower() == salesOrder)
            .Where(j => !db.OrderFulfillments.Any(f => f.TenantId == tenant.Id && f.SourceJobId == j.Id));
        if (tenant.DataSource == TenantDataSources.Native) query = query.Where(j => j.Status == JobStatus.Succeeded);
        List<Job> jobs;
        // PostgreSQL filters, orders and limits in the database; SQLite (tests) cannot translate DateTimeOffset.
        if (db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
            jobs = [.. (await query.ToListAsync(ct)).Where(j => j.EnqueuedAtUtc >= since).OrderBy(j => j.EnqueuedAtUtc).Take(MaxBackfillOrders)];
        else
            jobs = await query.Where(j => j.EnqueuedAtUtc >= since).OrderBy(j => j.EnqueuedAtUtc).Take(MaxBackfillOrders).ToListAsync(ct);
        if (jobs.Count == 0) return WarehouseResult<WarehouseBackfillResponse>.Ok(new WarehouseBackfillResponse { Days = days });

        var queued = 0;
        foreach (var job in jobs)
        {
            var fulfillment = await EnqueueAsync(db, tenant, job, approvalRequestId: null, ct, queuedAtUtc: job.EnqueuedAtUtc);
            if (fulfillment is null) continue;
            queued++;
            if (fulfillment.ErpState == FulfillmentErpStates.Pending)
            {
                fulfillment.ErpState = job.Status switch
                {
                    JobStatus.Succeeded => FulfillmentErpStates.Written,
                    JobStatus.Failed or JobStatus.DeadLetter => FulfillmentErpStates.Failed,
                    _ => FulfillmentErpStates.Pending,
                };
            }
        }
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        if (queued > 0) Notify(tenant.Id);
        return WarehouseResult<WarehouseBackfillResponse>.Ok(new WarehouseBackfillResponse { Days = days, Queued = queued });
    }

    // ---- reading ----------------------------------------------------------------------

    public static async Task<bool> IsEnabledAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct) =>
        await db.TenantWarehouseSettings.AsNoTracking().AnyAsync(s => s.TenantId == tenantId && s.Enabled, ct);

    /// <summary>
    /// The queue. With <paramref name="changedSinceSeq"/> every order changed after it is returned
    /// whatever its status, oldest change first, so a page can also drop the ones that left its view;
    /// otherwise the orders in <paramref name="statuses"/>, oldest first.
    ///
    /// <para><see cref="FulfillmentListResponse.LatestSeq"/> is a cursor that never skips a change: for a
    /// change page it is the last change returned (the caller asks again while <c>hasMore</c>); for a
    /// status list it is read <b>before</b> the rows, so a change committed in between comes back again
    /// rather than being lost.</para>
    /// </summary>
    public static async Task<FulfillmentListResponse> ListAsync(
        CentralApiDbContext db, Guid tenantId, IReadOnlyCollection<string> statuses, long? changedSinceSeq, int take, CancellationToken ct)
    {
        var limit = Math.Clamp(take, 1, MaxListSize);
        var query = db.OrderFulfillments.AsNoTracking().Where(f => f.TenantId == tenantId);
        if (changedSinceSeq is { } since)
        {
            var changes = await query.Where(f => f.UpdatedSeq > since).OrderBy(f => f.UpdatedSeq).Take(limit + 1).ToListAsync(ct);
            var page = changes.Take(limit).ToList();
            return new FulfillmentListResponse
            {
                LatestSeq = page.Count > 0 ? page[^1].UpdatedSeq : since,
                HasMore = changes.Count > limit,
                Items = page.Select(ToDto).ToArray(),
            };
        }

        var cursor = await LatestSeqAsync(db, tenantId, ct);
        var rows = await query.Where(f => statuses.Contains(f.Status)).OrderBy(f => f.QueuedSeq).Take(limit + 1).ToListAsync(ct);
        return new FulfillmentListResponse
        {
            LatestSeq = cursor,
            HasMore = rows.Count > limit,
            Items = rows.Take(limit).Select(ToDto).ToArray(),
        };
    }

    public static async Task<long> LatestSeqAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct) =>
        await db.OrderFulfillments.AsNoTracking().Where(f => f.TenantId == tenantId).MaxAsync(f => (long?)f.UpdatedSeq, ct) ?? 0;

    public static async Task<WarehouseResult<FulfillmentDetailResponse>> DetailAsync(CentralApiDbContext db, Guid tenantId, Guid id, CancellationToken ct)
    {
        var fulfillment = await db.OrderFulfillments.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId, ct);
        if (fulfillment is null) return NotFound<FulfillmentDetailResponse>();
        var events = await db.OrderFulfillmentEvents.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.FulfillmentId == id)
            .OrderBy(e => e.Id)
            .ToListAsync(ct);
        using var items = JsonDocument.Parse(string.IsNullOrWhiteSpace(fulfillment.ItemsJson) ? "[]" : fulfillment.ItemsJson);
        return WarehouseResult<FulfillmentDetailResponse>.Ok(new FulfillmentDetailResponse
        {
            Fulfillment = ToDto(fulfillment),
            Items = items.RootElement.Clone(),
            Events = events.Select(e => new FulfillmentEventDto
            {
                Action = e.Action,
                FromStatus = e.FromStatus,
                ToStatus = e.ToStatus,
                ActorUserId = e.ActorUserId,
                ActorName = e.ActorName,
                Note = e.Note,
                OccurredAtUtc = e.OccurredAtUtc,
            }).ToArray(),
        });
    }

    public static FulfillmentDto ToDto(OrderFulfillment f) => new()
    {
        Id = f.Id,
        OrderNo = f.OrderNo,
        CustomerCode = f.CustomerCode,
        CustomerName = f.CustomerName,
        SalespersonName = f.SalespersonName,
        Amount = f.Amount,
        LineCount = f.LineCount,
        ItemQuantity = f.ItemQuantity,
        Status = f.Status,
        QueuedAtUtc = f.QueuedAtUtc,
        StartedAtUtc = f.StartedAtUtc,
        PackedAtUtc = f.PackedAtUtc,
        LoadedAtUtc = f.LoadedAtUtc,
        AssigneeUserId = f.AssigneeUserId,
        AssigneeName = f.AssigneeName,
        VehiclePlate = f.VehiclePlate,
        ErpState = f.ErpState,
        ApprovalRequestId = f.ApprovalRequestId,
        UpdatedSeq = f.UpdatedSeq,
    };

    // ---- moving an order --------------------------------------------------------------

    /// <summary>
    /// Takes <paramref name="action"/> (start, pack, load, undo, cancel, reassign) on an order for
    /// <paramref name="actor"/>, whose roles were read from the database for this request.
    /// </summary>
    public async Task<WarehouseResult<FulfillmentDto>> ActAsync(
        CentralApiDbContext db, Guid tenantId, MobileUser actor, string? deviceId, Guid id, string action, FulfillmentActionRequest? body, CancellationToken ct)
    {
        action = action.ToUpperInvariant();
        var managing = action is FulfillmentActions.Cancel or FulfillmentActions.Reassign;
        if (managing ? !RolePermissions.CanManageWarehouse(actor) : !RolePermissions.CanOperateWarehouse(actor))
            return WarehouseResult<FulfillmentDto>.Fail(403, managing ? "WAREHOUSE_MANAGER_REQUIRED" : "WAREHOUSE_ROLE_REQUIRED",
                managing ? "Only administrators and managers can cancel or reassign orders." : "Your roles do not include warehouse work.");

        var current = await db.OrderFulfillments.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id && f.TenantId == tenantId, ct);
        if (current is null) return NotFound<FulfillmentDto>();

        var note = CleanNote(body?.Note);
        var now = DateTimeOffset.UtcNow;
        string from = current.Status;
        string to;
        switch (action)
        {
            case FulfillmentActions.Start:
                if (from != FulfillmentStatuses.Pending) return await StateChangedAsync(db, current, ct);
                to = FulfillmentStatuses.Preparing;
                break;
            case FulfillmentActions.Pack:
                if (from != FulfillmentStatuses.Preparing) return await StateChangedAsync(db, current, ct);
                to = FulfillmentStatuses.Packed;
                break;
            case FulfillmentActions.Load:
                if (from != FulfillmentStatuses.Packed) return await StateChangedAsync(db, current, ct);
                if (body?.VehiclePlate is { Length: > MaxPlateLength })
                    return WarehouseResult<FulfillmentDto>.Fail(400, "INVALID_VEHICLE_PLATE", $"vehiclePlate must be at most {MaxPlateLength} characters.");
                to = FulfillmentStatuses.Loaded;
                break;
            case FulfillmentActions.Undo:
                if (from is not (FulfillmentStatuses.Preparing or FulfillmentStatuses.Packed))
                    return WarehouseResult<FulfillmentDto>.Fail(409, "FULFILLMENT_CANNOT_UNDO", $"Only preparing or packed orders can be taken back; this one is {from.ToLowerInvariant()}.");
                // V6: the person who took the step, within five minutes; a manager any time.
                if (!RolePermissions.CanManageWarehouse(actor))
                {
                    var step = await db.OrderFulfillmentEvents.AsNoTracking()
                        .Where(e => e.TenantId == tenantId && e.FulfillmentId == id && e.ToStatus == from)
                        .OrderByDescending(e => e.Id)
                        .FirstOrDefaultAsync(ct);
                    if (step is null || step.ActorUserId != actor.Id || now - step.OccurredAtUtc > UndoWindow)
                        return WarehouseResult<FulfillmentDto>.Fail(403, "UNDO_NOT_ALLOWED",
                            $"Only the person who took the step can take it back, within {UndoWindow.TotalMinutes:0} minutes; ask a manager.");
                }
                to = from == FulfillmentStatuses.Packed ? FulfillmentStatuses.Preparing : FulfillmentStatuses.Pending;
                break;
            case FulfillmentActions.Cancel:
                if (!FulfillmentStatuses.Open.Contains(from)) return await StateChangedAsync(db, current, ct);
                to = FulfillmentStatuses.Cancelled;
                break;
            case FulfillmentActions.Reassign:
                if (from != FulfillmentStatuses.Preparing) return await StateChangedAsync(db, current, ct);
                to = from;
                break;
            default:
                return WarehouseResult<FulfillmentDto>.Fail(404, "UNKNOWN_FULFILLMENT_ACTION", "action must be start, pack, load, undo, cancel or reassign.");
        }

        MobileUser? assignee = null;
        if (action == FulfillmentActions.Reassign)
        {
            assignee = body?.AssigneeUserId is { } assigneeId
                ? await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == assigneeId && u.TenantId == tenantId && u.IsActive && u.DeletedAtUtc == null, ct)
                : null;
            if (assignee is null || !RolePermissions.CanOperateWarehouse(assignee))
                return WarehouseResult<FulfillmentDto>.Fail(400, "INVALID_ASSIGNEE", "assigneeUserId must name an active user of the company with a warehouse role.");
        }

        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var seq = await NextSeqAsync(db, tenantId, ct);
        // Reassignment keeps the status, so the claim also checks nobody moved the order since it was read.
        var claimed = await ClaimAsync(db, tenantId, id, from, to, action == FulfillmentActions.Reassign ? current.UpdatedSeq : null, seq, now, ct);
        if (claimed is null)
        {
            if (transaction is not null) await transaction.RollbackAsync(ct);
            return await StateChangedAsync(db, await db.OrderFulfillments.AsNoTracking().FirstAsync(f => f.Id == id, ct), ct);
        }

        switch (action)
        {
            case FulfillmentActions.Start:
                claimed.StartedAtUtc = now;
                claimed.AssigneeUserId = actor.Id;
                claimed.AssigneeName = actor.FullName;
                break;
            case FulfillmentActions.Pack:
                claimed.PackedAtUtc = now;
                break;
            case FulfillmentActions.Load:
                claimed.LoadedAtUtc = now;
                claimed.VehiclePlate = string.IsNullOrWhiteSpace(body?.VehiclePlate) ? null : body.VehiclePlate.Trim().ToUpperInvariant();
                break;
            case FulfillmentActions.Undo when to == FulfillmentStatuses.Pending:
                claimed.StartedAtUtc = null;
                claimed.AssigneeUserId = null;
                claimed.AssigneeName = null;
                break;
            case FulfillmentActions.Undo:
                claimed.PackedAtUtc = null;
                break;
            case FulfillmentActions.Reassign:
                claimed.AssigneeUserId = assignee!.Id;
                claimed.AssigneeName = assignee.FullName;
                note ??= "→ " + assignee.FullName;
                break;
        }
        AddEvent(db, claimed, from, action, actor, deviceId, note, now);
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        Notify(tenantId);
        return WarehouseResult<FulfillmentDto>.Ok(ToDto(claimed));
    }

    // ---- settings ---------------------------------------------------------------------

    public static async Task<WarehouseSettingsDto> SettingsAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct) =>
        ToDto(await db.TenantWarehouseSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct)
            ?? new TenantWarehouseSettings { TenantId = tenantId });

    public async Task<WarehouseResult<WarehouseSettingsDto>> UpdateSettingsAsync(
        CentralApiDbContext db, Guid tenantId, MobileUser actor, WarehouseSettingsDto? body, CancellationToken ct)
    {
        if (!RolePermissions.CanManageWarehouse(actor))
            return WarehouseResult<WarehouseSettingsDto>.Fail(403, "WAREHOUSE_MANAGER_REQUIRED", "Only administrators and managers can change the warehouse settings.");
        if (body is null)
            return WarehouseResult<WarehouseSettingsDto>.Fail(400, "INVALID_WAREHOUSE_SETTINGS", "Body required.");
        var minutes = new[] { body.PendingWarnMinutes, body.PendingCriticalMinutes, body.PreparingWarnMinutes, body.PreparingCriticalMinutes, body.PackedWarnMinutes };
        if (minutes.Any(m => m is < 1 or > 1440)
            || body.PendingWarnMinutes >= body.PendingCriticalMinutes
            || body.PreparingWarnMinutes >= body.PreparingCriticalMinutes)
            return WarehouseResult<WarehouseSettingsDto>.Fail(400, "INVALID_WAREHOUSE_SETTINGS",
                "Every threshold is 1-1440 minutes and each warning comes before its critical threshold.");

        var settings = await db.TenantWarehouseSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        if (settings is null)
        {
            settings = new TenantWarehouseSettings { TenantId = tenantId };
            db.TenantWarehouseSettings.Add(settings);
        }
        settings.Enabled = body.Enabled;
        settings.PendingWarnMinutes = body.PendingWarnMinutes;
        settings.PendingCriticalMinutes = body.PendingCriticalMinutes;
        settings.PreparingWarnMinutes = body.PreparingWarnMinutes;
        settings.PreparingCriticalMinutes = body.PreparingCriticalMinutes;
        settings.PackedWarnMinutes = body.PackedWarnMinutes;
        settings.UpdatedByUserId = actor.Id;
        settings.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        Notify(tenantId);
        return WarehouseResult<WarehouseSettingsDto>.Ok(ToDto(settings));
    }

    private static WarehouseSettingsDto ToDto(TenantWarehouseSettings s) => new()
    {
        Enabled = s.Enabled,
        PendingWarnMinutes = s.PendingWarnMinutes,
        PendingCriticalMinutes = s.PendingCriticalMinutes,
        PreparingWarnMinutes = s.PreparingWarnMinutes,
        PreparingCriticalMinutes = s.PreparingCriticalMinutes,
        PackedWarnMinutes = s.PackedWarnMinutes,
        UpdatedAtUtc = s.UpdatedAtUtc,
    };

    // ---- helpers ----------------------------------------------------------------------

    /// <summary>
    /// Moves the order from <paramref name="from"/> to <paramref name="to"/> if it is still there (and, when
    /// given, still at <paramref name="expectedSeq"/>) and returns it tracked; null when someone moved it first.
    /// On a relational database the WHERE is the lock.
    /// </summary>
    private static async Task<OrderFulfillment?> ClaimAsync(
        CentralApiDbContext db, Guid tenantId, Guid id, string from, string to, long? expectedSeq, long seq, DateTimeOffset now, CancellationToken ct)
    {
        var match = db.OrderFulfillments.Where(f => f.Id == id && f.TenantId == tenantId && f.Status == from);
        if (expectedSeq is { } expected) match = match.Where(f => f.UpdatedSeq == expected);
        if (db.Database.IsRelational())
        {
            var claimed = await match.ExecuteUpdateAsync(s => s
                .SetProperty(f => f.Status, to)
                .SetProperty(f => f.UpdatedSeq, seq)
                .SetProperty(f => f.UpdatedAtUtc, now), ct);
            return claimed == 0 ? null : await db.OrderFulfillments.FirstAsync(f => f.Id == id, ct);
        }
        var tracked = await match.FirstOrDefaultAsync(ct);
        if (tracked is null) return null;
        tracked.Status = to;
        tracked.UpdatedSeq = seq;
        tracked.UpdatedAtUtc = now;
        return tracked;
    }

    /// <summary>The next change number of the tenant; see the class remarks.</summary>
    private static async Task<long> NextSeqAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct) =>
        db.Database.IsRelational()
            ? await MobileRecordProjector.ReserveAsync(db, tenantId, 1, ct)
            // The in-memory test host has no transactions or counter row; time order is enough there.
            : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000 + Interlocked.Increment(ref _inMemorySeq) % 1000;

    private static long _inMemorySeq;

    private static async Task<WarehouseResult<FulfillmentDto>> StateChangedAsync(CentralApiDbContext db, OrderFulfillment current, CancellationToken ct)
    {
        var last = await db.OrderFulfillmentEvents.AsNoTracking()
            .Where(e => e.TenantId == current.TenantId && e.FulfillmentId == current.Id)
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync(ct);
        var by = last is null || last.ActorUserId is null ? string.Empty : " by " + last.ActorName;
        return WarehouseResult<FulfillmentDto>.Fail(409, "FULFILLMENT_STATE_CHANGED", $"The order is {current.Status.ToLowerInvariant()}{by}.");
    }

    private static WarehouseResult<T> NotFound<T>() =>
        WarehouseResult<T>.Fail(404, "FULFILLMENT_NOT_FOUND", "Order not found in the warehouse queue.");

    private static void AddEvent(CentralApiDbContext db, OrderFulfillment fulfillment, string? from, string action, MobileUser? actor, string? deviceId, string? note, DateTimeOffset now) =>
        db.OrderFulfillmentEvents.Add(new OrderFulfillmentEvent
        {
            TenantId = fulfillment.TenantId,
            Fulfillment = fulfillment,
            FulfillmentId = fulfillment.Id,
            FromStatus = from,
            ToStatus = fulfillment.Status,
            Action = action,
            ActorUserId = actor?.Id,
            ActorName = actor?.FullName ?? SystemActor,
            DeviceId = deviceId is { Length: > 128 } ? deviceId[..128] : deviceId,
            Note = note,
            OccurredAtUtc = now,
        });

    private static string? CleanNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note)) return null;
        var trimmed = note.Trim();
        return trimmed.Length > MaxNoteLength ? trimmed[..MaxNoteLength] : trimmed;
    }

    private static string Clip(string value, int max) => value.Length > max ? value[..max] : value;

    private static readonly JsonSerializerOptions ItemJson = new(JsonSerializerDefaults.Web);

    private sealed record PickItem(string StockCode, string Name, decimal Quantity, string? Unit);

    private sealed record SalesOrder(string? OrderNo, string? CustomerCode, string? CustomerName, decimal Amount, List<PickItem> Items);

    /// <summary>The phone's sales order: <c>mobileDocumentId, customerCode, counterparty, amount, lines[productCode|barcode, productTitle, quantity, unit, lineTotal]</c>.</summary>
    private static SalesOrder ReadOrder(Job job)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.PayloadJson) ? "{}" : job.PayloadJson);
        var root = document.RootElement;
        var items = new List<PickItem>();
        decimal linesTotal = 0;
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("lines", out var lines) && lines.ValueKind == JsonValueKind.Array)
        {
            foreach (var line in lines.EnumerateArray())
            {
                var quantity = Number(line, "quantity") ?? 0;
                linesTotal += Number(line, "lineTotal") ?? quantity * (Number(line, "unitPrice") ?? 0);
                items.Add(new PickItem(
                    Clip(Text(line, "productCode") ?? Text(line, "stockCode") ?? Text(line, "barcode") ?? string.Empty, 64),
                    Clip(Text(line, "productTitle") ?? Text(line, "name") ?? string.Empty, 200),
                    quantity,
                    Text(line, "unit")));
            }
        }
        return new SalesOrder(
            Text(root, "mobileDocumentId"),
            Text(root, "customerCode"),
            Text(root, "counterparty") ?? Text(root, "customerName"),
            Number(root, "amount") ?? linesTotal,
            items);
    }

    private static string? Text(JsonElement item, string name) =>
        item.ValueKind == JsonValueKind.Object && item.TryGetProperty(name, out var value)
            ? value.ValueKind switch
            {
                JsonValueKind.String when !string.IsNullOrWhiteSpace(value.GetString()) => value.GetString()!.Trim(),
                JsonValueKind.Number => value.GetRawText(),
                _ => null,
            }
            : null;

    private static decimal? Number(JsonElement item, string name) =>
        item.ValueKind == JsonValueKind.Object && item.TryGetProperty(name, out var value)
            ? value.ValueKind switch
            {
                JsonValueKind.Number when value.TryGetDecimal(out var number) => number,
                JsonValueKind.String when decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) => parsed,
                _ => null,
            }
            : null;
}
