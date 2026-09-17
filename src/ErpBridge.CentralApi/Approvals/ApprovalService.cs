using System.Linq.Expressions;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Native;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Approvals;

/// <summary>Outcome of an approval operation: a value, or an HTTP status with a stable error code.</summary>
public sealed record ApprovalResult<T>(T? Value, int StatusCode, ApiError? Error)
{
    public static ApprovalResult<T> Ok(T value, int status = 200) => new(value, status, null);

    public static ApprovalResult<T> Fail(int status, string code, string message) =>
        new(default, status, new ApiError { ErrorCode = code, Message = message });

    public bool Succeeded => Error is null;
}

/// <summary>
/// The company approval queue. Requests arrive through the phone's durable outbox
/// (<c>/ingest/jobs</c>, document type <c>approval_request</c>) and are decided by the
/// company's approvers — administrators and the managers they allow.
///
/// <para>Every state change is a single conditional update from the expected status:
/// of two approvers acting at the same moment exactly one succeeds, the other is told
/// who was first. Approval posts the request's documents in the same transaction, so
/// an approved request always has its documents and a failed document leaves the
/// request pending.</para>
/// </summary>
public sealed class ApprovalService
{
    public const string DocumentType = "approval_request";
    public const int MaxDocumentsPerRequest = 20;
    public const int MaxNoteLength = 500;

    private readonly NativeDocumentProcessor _native;
    private readonly IBootstrapNotificationHub _hub;
    private readonly FulfillmentService _warehouse;
    private readonly ITenantEventHub _portal;

    public ApprovalService(NativeDocumentProcessor native, IBootstrapNotificationHub hub, FulfillmentService warehouse, ITenantEventHub portal)
    {
        _native = native;
        _hub = hub;
        _warehouse = warehouse;
        _portal = portal;
    }

    // ---- rules ----------------------------------------------------------------

    /// <summary>The tenant's rules; a tenant that never saved any requires approval for everything.</summary>
    public static async Task<TenantApprovalRules> RulesAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct) =>
        await db.TenantApprovalRules.AsNoTracking().FirstOrDefaultAsync(r => r.TenantId == tenantId, ct)
        ?? new TenantApprovalRules { TenantId = tenantId };

    public async Task<ApprovalResult<TenantApprovalRules>> UpdateRulesAsync(
        CentralApiDbContext db, Tenant tenant, MobileUser actor, IReadOnlyDictionary<string, bool>? changes, CancellationToken ct)
    {
        if (!ApprovalPermissions.CanManageRules(actor))
            return ApprovalResult<TenantApprovalRules>.Fail(403, "APPROVAL_RULES_FORBIDDEN", "You are not allowed to change the approval rules.");
        if (changes is null || changes.Count == 0)
            return ApprovalResult<TenantApprovalRules>.Fail(400, "INVALID_APPROVAL_RULES", "rules must name at least one kind.");
        if (changes.Keys.FirstOrDefault(kind => !ApprovalKinds.IsValid(kind)) is { } unknown)
            return ApprovalResult<TenantApprovalRules>.Fail(400, "INVALID_APPROVAL_RULES", $"Unknown approval kind '{unknown}'.");

        var rules = await db.TenantApprovalRules.FirstOrDefaultAsync(r => r.TenantId == tenant.Id, ct);
        if (rules is null)
        {
            rules = new TenantApprovalRules { TenantId = tenant.Id };
            db.TenantApprovalRules.Add(rules);
        }
        foreach (var (kind, required) in changes) rules.Set(kind, required);
        rules.UpdatedByName = actor.FullName;
        rules.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        _hub.Publish(tenant.Id, rules.UpdatedAtUtc.Value);
        _portal.Publish(tenant.Id, TenantEventTopics.Approvals);
        return ApprovalResult<TenantApprovalRules>.Ok(rules);
    }

    // ---- reading --------------------------------------------------------------

    /// <summary>An approver sees every request of the company; anyone else only their own.</summary>
    public static IQueryable<ApprovalRequest> Visible(CentralApiDbContext db, Guid tenantId, MobileUser viewer)
    {
        var query = db.ApprovalRequests.AsNoTracking().Where(r => r.TenantId == tenantId);
        // Approvers see the kinds they decide plus their own requests; everyone else only their own.
        var kinds = ApprovalPermissions.DecidableKinds(viewer);
        if (kinds is null) return query;
        var decidable = kinds.ToList();
        return query.Where(r => r.RequestedByUserId == viewer.Id || decidable.Contains(r.Kind));
    }

    public async Task<ApprovalResult<ApprovalRequestDetailDto>> DetailAsync(
        CentralApiDbContext db, Tenant tenant, MobileUser viewer, Guid requestId, CancellationToken ct)
    {
        var request = await Visible(db, tenant.Id, viewer).FirstOrDefaultAsync(r => r.Id == requestId, ct);
        if (request is null)
            return ApprovalResult<ApprovalRequestDetailDto>.Fail(404, "APPROVAL_NOT_FOUND", "Approval request not found.");
        return ApprovalResult<ApprovalRequestDetailDto>.Ok(await DetailAsync(db, tenant, request, ct));
    }

    /// <summary>Builds the detail of a request the caller is already allowed to see.</summary>
    public static async Task<ApprovalRequestDetailDto> DetailAsync(CentralApiDbContext db, Tenant tenant, ApprovalRequest request, CancellationToken ct)
    {
        var events = await db.ApprovalRequestEvents.AsNoTracking()
            .Where(e => e.RequestId == request.Id)
            .OrderBy(e => e.AtSeq)
            .ToListAsync(ct);
        using var documents = JsonDocument.Parse(string.IsNullOrWhiteSpace(request.DocumentsJson) ? "[]" : request.DocumentsJson);
        var warnings = tenant.DataSource == TenantDataSources.Native && request.Status == ApprovalStatuses.Pending
            ? await NativeDocumentProcessor.StockShortagesAsync(db, tenant.Id, documents.RootElement, ct)
            : [];
        return new ApprovalRequestDetailDto
        {
            Request = ToDto(request),
            Documents = documents.RootElement.Clone(),
            Events = events.Select(e => new ApprovalEventDto { Action = e.Action, ByName = e.ByName, AtUtc = e.AtUtc, Note = e.Note }).ToArray(),
            Warnings = warnings.ToArray(),
        };
    }

    public static async Task<ApprovalSummaryDto> SummaryAsync(CentralApiDbContext db, Guid tenantId, MobileUser viewer, CancellationToken ct)
    {
        var visible = Visible(db, tenantId, viewer);
        var canDecide = ApprovalPermissions.CanDecide(viewer);
        return new ApprovalSummaryDto
        {
            PendingCount = await visible.CountAsync(r => r.Status == ApprovalStatuses.Pending, ct),
            LatestUpdatedSeq = await visible.Select(r => (long?)r.UpdatedSeq).MaxAsync(ct) ?? 0,
            CanApprove = canDecide,
            // The same rule DecideAsync enforces with SELF_APPROVAL_NOT_ALLOWED.
            CanApproveOwnRequests = canDecide && !await OtherApproverExistsAsync(db, tenantId, viewer.Id, kind: null, ct),
        };
    }

    public static ApprovalRequestDto ToDto(ApprovalRequest r)
    {
        using var summary = JsonDocument.Parse(string.IsNullOrWhiteSpace(r.SummaryJson) ? "{}" : r.SummaryJson);
        return new ApprovalRequestDto
        {
            Id = r.Id,
            ExternalId = r.ExternalId,
            Kind = r.Kind,
            CounterpartyName = r.CounterpartyName,
            Amount = r.Amount,
            Summary = summary.RootElement.Clone(),
            Status = r.Status,
            ReplacesRequestId = r.ReplacesRequestId,
            RequestedByUserId = r.RequestedByUserId,
            RequestedByName = r.RequestedByName,
            RequestedAtUtc = r.RequestedAtUtc,
            RequestedSeq = r.RequestedSeq,
            UpdatedSeq = r.UpdatedSeq,
            DecidedByName = r.DecidedByName,
            DecidedAtUtc = r.DecidedAtUtc,
            DecisionNote = r.DecisionNote,
        };
    }

    // ---- submitting -----------------------------------------------------------

    /// <summary>
    /// Stores a request sent by a signed-in user. Resending the same external id is a
    /// no-op. A request naming <c>replacesRequestId</c> corrects one of the sender's
    /// rejected requests, which is closed as <see cref="ApprovalStatuses.Resubmitted"/>.
    /// </summary>
    public async Task<ApprovalResult<ApprovalRequest>> SubmitAsync(
        CentralApiDbContext db, Tenant tenant, MobileUser requester, string externalId, string payloadJson, CancellationToken ct)
    {
        var existing = await db.ApprovalRequests.AsNoTracking()
            .FirstOrDefaultAsync(r => r.TenantId == tenant.Id && r.ExternalId == externalId, ct);
        if (existing is not null) return ApprovalResult<ApprovalRequest>.Ok(existing, 200);
        if (externalId.Length > 128)
            return ApprovalResult<ApprovalRequest>.Fail(400, "INVALID_EXTERNAL_ID", "externalId must be at most 128 characters.");

        using var document = JsonDocument.Parse(payloadJson);
        var root = document.RootElement;
        var kind = Text(root, "kind")?.ToLowerInvariant();
        if (!ApprovalKinds.IsValid(kind))
            return ApprovalResult<ApprovalRequest>.Fail(400, "INVALID_APPROVAL_KIND", "kind must be one of: " + string.Join(", ", ApprovalKinds.All) + ".");
        if (ApprovalKinds.CardKinds.Contains(kind!) && tenant.DataSource != TenantDataSources.Native)
            return ApprovalResult<ApprovalRequest>.Fail(409, "CARDS_REQUIRE_NATIVE_TENANT", "Product and customer cards can only be changed from the phone for a company without an ERP.");
        if (!root.TryGetProperty("documents", out var documents) || documents.ValueKind != JsonValueKind.Array
            || documents.GetArrayLength() is 0 or > MaxDocumentsPerRequest)
            return ApprovalResult<ApprovalRequest>.Fail(400, "INVALID_APPROVAL_DOCUMENTS", $"documents must hold 1-{MaxDocumentsPerRequest} documents.");

        foreach (var item in documents.EnumerateArray())
        {
            var documentType = Text(item, "documentType");
            var documentExternalId = Text(item, "externalId");
            if (documentType is null || documentExternalId is null || documentExternalId.Length > 128
                || !item.TryGetProperty("payload", out var payload) || payload.ValueKind != JsonValueKind.Object)
                return ApprovalResult<ApprovalRequest>.Fail(400, "INVALID_APPROVAL_DOCUMENTS", "Every document needs documentType, externalId (at most 128 characters) and an object payload.");
            if (RejectDocument(tenant, kind!, documentType) is { } problem)
                return ApprovalResult<ApprovalRequest>.Fail(problem.Status, problem.Code, problem.Message);
        }

        Guid? replaces = null;
        if (Text(root, "replacesRequestId") is { } replacesText)
        {
            if (!Guid.TryParse(replacesText, out var replacesId))
                return ApprovalResult<ApprovalRequest>.Fail(400, "INVALID_REPLACES_REQUEST", "replacesRequestId must be a request id.");
            var replaced = await db.ApprovalRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == replacesId && r.TenantId == tenant.Id, ct);
            if (replaced is null || replaced.RequestedByUserId != requester.Id)
                return ApprovalResult<ApprovalRequest>.Fail(404, "APPROVAL_NOT_FOUND", "The request to correct was not found among your requests.");
            if (replaced.Status != ApprovalStatuses.Rejected)
                return ApprovalResult<ApprovalRequest>.Fail(409, "APPROVAL_NOT_REJECTED", $"Only a rejected request can be corrected; this one is {replaced.Status.ToLowerInvariant()}.");
            replaces = replacesId;
        }

        var now = DateTimeOffset.UtcNow;
        var seq = now.ToUnixTimeMilliseconds();
        var counterparty = Text(root, "counterpartyName") ?? string.Empty;
        var request = new ApprovalRequest
        {
            TenantId = tenant.Id,
            ExternalId = externalId,
            Kind = kind!,
            CounterpartyName = counterparty.Length > 200 ? counterparty[..200] : counterparty,
            Amount = decimal.Round(Number(root, "amount") ?? 0, 2),
            SummaryJson = root.TryGetProperty("summary", out var summary) && summary.ValueKind == JsonValueKind.Object ? summary.GetRawText() : "{}",
            DocumentsJson = documents.GetRawText(),
            Status = ApprovalStatuses.Pending,
            ReplacesRequestId = replaces,
            RequestedByUserId = requester.Id,
            RequestedByName = requester.FullName,
            RequestedAtUtc = now,
            RequestedSeq = seq,
            UpdatedSeq = seq,
        };

        try
        {
            await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
            if (replaces is { } replacedId)
            {
                var replaced = await ClaimAsync(db, tenant.Id, replacedId, ApprovalStatuses.Rejected, ApprovalStatuses.Resubmitted, seq, ct);
                if (replaced is null)
                    return ApprovalResult<ApprovalRequest>.Fail(409, "APPROVAL_NOT_REJECTED", "The request was changed meanwhile and can no longer be corrected.");
                AddEvent(db, replaced, ApprovalActions.Resubmitted, requester, null, now);
            }
            db.ApprovalRequests.Add(request);
            AddEvent(db, request, ApprovalActions.Submitted, requester, null, now);
            await db.SaveChangesAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            // The same request raced in from a retry; the first one stands.
            db.ChangeTracker.Clear();
            var winner = await db.ApprovalRequests.AsNoTracking()
                .FirstOrDefaultAsync(r => r.TenantId == tenant.Id && r.ExternalId == externalId, ct);
            if (winner is null) throw;
            return ApprovalResult<ApprovalRequest>.Ok(winner, 200);
        }

        _hub.Publish(tenant.Id, now);
        _portal.Publish(tenant.Id, TenantEventTopics.Approvals);
        return ApprovalResult<ApprovalRequest>.Ok(request, 201);
    }

    // ---- deciding -------------------------------------------------------------

    /// <summary>Approves or rejects a pending request on behalf of an approver.</summary>
    public async Task<ApprovalResult<ApprovalRequest>> DecideAsync(
        CentralApiDbContext db, Tenant tenant, MobileUser actor, Guid requestId, bool approve, string? note, CancellationToken ct)
    {
        if (!ApprovalPermissions.CanDecide(actor))
            return ApprovalResult<ApprovalRequest>.Fail(403, "APPROVER_REQUIRED", "You are not allowed to approve or reject requests.");
        var current = await db.ApprovalRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == requestId && r.TenantId == tenant.Id, ct);
        if (current is null)
            return ApprovalResult<ApprovalRequest>.Fail(404, "APPROVAL_NOT_FOUND", "Approval request not found.");
        if (!ApprovalPermissions.CanDecide(actor, current.Kind))
            return ApprovalResult<ApprovalRequest>.Fail(403, "APPROVER_REQUIRED", "You are not allowed to decide this kind of request.");
        if (current.Status != ApprovalStatuses.Pending)
            return AlreadyDecided(current);
        // Nobody approves their own request while someone else could; a company with a
        // single approver would otherwise be locked out of its own work.
        if (approve && current.RequestedByUserId == actor.Id && await OtherApproverExistsAsync(db, tenant.Id, actor.Id, current.Kind, ct))
            return ApprovalResult<ApprovalRequest>.Fail(403, "SELF_APPROVAL_NOT_ALLOWED", "Your own request must be approved by another approver.");

        note = CleanNote(note);
        var now = DateTimeOffset.UtcNow;
        var status = approve ? ApprovalStatuses.Approved : ApprovalStatuses.Rejected;
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;

        var request = await ClaimAsync(db, tenant.Id, requestId, ApprovalStatuses.Pending, status, now.ToUnixTimeMilliseconds(), ct);
        if (request is null)
            return AlreadyDecided(await db.ApprovalRequests.AsNoTracking().FirstAsync(r => r.Id == requestId, ct));
        request.DecidedByUserId = actor.Id;
        request.DecidedByName = actor.FullName;
        request.DecidedAtUtc = now;
        request.DecisionNote = note;
        AddEvent(db, request, approve ? ApprovalActions.Approved : ApprovalActions.Rejected, actor, note, now);

        if (approve && await PostDocumentsAsync(db, tenant, request, now, ct) is { } failure)
        {
            // On a relational database disposing the transaction rolls the approval back.
            // A provider without transactions (the in-memory test host) already saved it
            // together with the failed booking, so it is put back to pending explicitly.
            if (transaction is null) await RevertClaimAsync(db, request, ct);
            return failure;
        }

        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        _hub.Publish(tenant.Id, now);
        _portal.Publish(tenant.Id, TenantEventTopics.Approvals);
        // An approved sale may have entered the warehouse queue.
        if (approve) _warehouse.Notify(tenant.Id);
        return ApprovalResult<ApprovalRequest>.Ok(request);
    }

    /// <summary>Puts a rejected request back into the pending queue.</summary>
    public async Task<ApprovalResult<ApprovalRequest>> ReopenAsync(
        CentralApiDbContext db, Tenant tenant, MobileUser actor, Guid requestId, string? note, CancellationToken ct)
    {
        if (!ApprovalPermissions.CanDecide(actor))
            return ApprovalResult<ApprovalRequest>.Fail(403, "APPROVER_REQUIRED", "You are not allowed to reopen requests.");
        var rejected = await db.ApprovalRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == requestId && r.TenantId == tenant.Id, ct);
        if (rejected is not null && !ApprovalPermissions.CanDecide(actor, rejected.Kind))
            return ApprovalResult<ApprovalRequest>.Fail(403, "APPROVER_REQUIRED", "You are not allowed to reopen this kind of request.");
        return await TransitionAsync(db, tenant, actor, requestId, ApprovalStatuses.Rejected, ApprovalStatuses.Pending, ApprovalActions.Reopened, note,
            request => request.DecidedByUserId = null, requesterOnly: false, ct);
    }

    /// <summary>The requester takes back a request nobody has decided yet.</summary>
    public Task<ApprovalResult<ApprovalRequest>> WithdrawAsync(
        CentralApiDbContext db, Tenant tenant, MobileUser actor, Guid requestId, string? note, CancellationToken ct) =>
        TransitionAsync(db, tenant, actor, requestId, ApprovalStatuses.Pending, ApprovalStatuses.Withdrawn, ApprovalActions.Withdrawn, note,
            request => request.DecidedByUserId = actor.Id, requesterOnly: true, ct);

    private async Task<ApprovalResult<ApprovalRequest>> TransitionAsync(
        CentralApiDbContext db, Tenant tenant, MobileUser actor, Guid requestId, string from, string to, string action, string? note,
        Action<ApprovalRequest> apply, bool requesterOnly, CancellationToken ct)
    {
        var current = await db.ApprovalRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == requestId && r.TenantId == tenant.Id, ct);
        if (current is null || (requesterOnly && current.RequestedByUserId != actor.Id))
            return ApprovalResult<ApprovalRequest>.Fail(404, "APPROVAL_NOT_FOUND", "Approval request not found.");
        if (current.Status != from) return WrongState(current, from);

        note = CleanNote(note);
        var now = DateTimeOffset.UtcNow;
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        var request = await ClaimAsync(db, tenant.Id, requestId, from, to, now.ToUnixTimeMilliseconds(), ct);
        if (request is null)
            return WrongState(await db.ApprovalRequests.AsNoTracking().FirstAsync(r => r.Id == requestId, ct), from);

        apply(request);
        // A reopened request is undecided again; a withdrawn one records who withdrew it.
        var reopened = to == ApprovalStatuses.Pending;
        request.DecidedByName = reopened ? null : actor.FullName;
        request.DecidedAtUtc = reopened ? null : now;
        request.DecisionNote = reopened ? null : note;
        AddEvent(db, request, action, actor, note, now);
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        _hub.Publish(tenant.Id, now);
        _portal.Publish(tenant.Id, TenantEventTopics.Approvals);
        return ApprovalResult<ApprovalRequest>.Ok(request);
    }

    /// <summary>Posts an approved request's documents. Returns the failure, or null when all were posted.</summary>
    private async Task<ApprovalResult<ApprovalRequest>?> PostDocumentsAsync(
        CentralApiDbContext db, Tenant tenant, ApprovalRequest request, DateTimeOffset now, CancellationToken ct)
    {
        using var documents = JsonDocument.Parse(request.DocumentsJson);
        foreach (var item in documents.RootElement.EnumerateArray())
        {
            var documentType = Text(item, "documentType")!;
            var externalId = Text(item, "externalId")!;
            // The company's data source may have changed since the request was sent.
            if (RejectDocument(tenant, request.Kind, documentType) is { } problem)
                return ApprovalResult<ApprovalRequest>.Fail(problem.Status, problem.Code, problem.Message);
            // A document that already reached the server is not posted twice.
            if (await db.Jobs.AnyAsync(j => j.TenantId == tenant.Id && j.DocumentType == documentType && j.ExternalId == externalId, ct))
                continue;

            var job = new Job
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                ExternalId = externalId,
                DocumentType = documentType,
                // Log Merkezi L3g: the approver's request; the agent's ERP write logs under it.
                CorrelationId = ErpBridge.CentralApi.LogCenter.CorrelationId.Current,
                PayloadJson = item.GetProperty("payload").GetRawText(),
                Status = JobStatus.Pending,
                EnqueuedAtUtc = now,
                // The salesperson who asked, not the approver: reports count their work.
                CreatedByUserId = request.RequestedByUserId,
            };
            if (tenant.DataSource == TenantDataSources.Native)
            {
                // Joins this transaction. Approval stands in for the administrator a
                // product card otherwise needs.
                var booked = await _native.IngestAsync(db, tenant.Id, job, callerIsAdmin: true, ct);
                if (booked.Status == JobStatus.Failed)
                    return ApprovalResult<ApprovalRequest>.Fail(422, "APPROVAL_DOCUMENT_FAILED", booked.LastError ?? "The document could not be booked.");
            }
            else
            {
                db.Jobs.Add(job);
            }
            // An approved sale goes to the warehouse in the approval's transaction (Faz 47).
            await _warehouse.EnqueueAsync(db, tenant, job, request.Id, ct);
        }
        return null;
    }

    // ---- helpers --------------------------------------------------------------

    /// <summary>
    /// Moves a request from <paramref name="from"/> to <paramref name="to"/> if it is still
    /// there and returns it tracked, or null when another caller moved it first. On a
    /// relational database the WHERE on the status is the lock.
    /// </summary>
    private static async Task<ApprovalRequest?> ClaimAsync(
        CentralApiDbContext db, Guid tenantId, Guid requestId, string from, string to, long seq, CancellationToken ct)
    {
        Expression<Func<ApprovalRequest, bool>> match = r => r.Id == requestId && r.TenantId == tenantId && r.Status == from;
        if (db.Database.IsRelational())
        {
            var claimed = await db.ApprovalRequests.Where(match)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, to).SetProperty(r => r.UpdatedSeq, seq), ct);
            return claimed == 0 ? null : await db.ApprovalRequests.FirstAsync(r => r.Id == requestId, ct);
        }
        var tracked = await db.ApprovalRequests.FirstOrDefaultAsync(match, ct);
        if (tracked is null) return null;
        tracked.Status = to;
        tracked.UpdatedSeq = seq;
        return tracked;
    }

    private static async Task RevertClaimAsync(CentralApiDbContext db, ApprovalRequest request, CancellationToken ct)
    {
        request.Status = ApprovalStatuses.Pending;
        request.DecidedByUserId = null;
        request.DecidedByName = null;
        request.DecidedAtUtc = null;
        request.DecisionNote = null;
        var decided = db.ChangeTracker.Entries<ApprovalRequestEvent>()
            .Where(e => e.Entity.RequestId == request.Id && e.Entity.Action == ApprovalActions.Approved)
            .Select(e => e.Entity)
            .ToList();
        db.ApprovalRequestEvents.RemoveRange(decided);
        await db.SaveChangesAsync(ct);
    }

    private static void AddEvent(CentralApiDbContext db, ApprovalRequest request, string action, MobileUser actor, string? note, DateTimeOffset now) =>
        db.ApprovalRequestEvents.Add(new ApprovalRequestEvent
        {
            TenantId = request.TenantId,
            RequestId = request.Id,
            Action = action,
            ByUserId = actor.Id,
            ByName = actor.FullName,
            AtUtc = now,
            AtSeq = now.ToUnixTimeMilliseconds(),
            Note = note,
        });

    /// <summary>
    /// Another active user who could decide a request of <paramref name="kind"/>; with no kind, one who
    /// decides anything (the summary's hint for the phone).
    /// </summary>
    private static async Task<bool> OtherApproverExistsAsync(CentralApiDbContext db, Guid tenantId, Guid userId, string? kind, CancellationToken ct)
    {
        var others = await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
            .Where(u => u.TenantId == tenantId && u.Id != userId && u.IsActive && u.DeletedAtUtc == null)
            .ToListAsync(ct);
        return others.Any(u => kind is null ? ApprovalPermissions.CanDecide(u) : ApprovalPermissions.CanDecide(u, kind));
    }

    private static ApprovalResult<ApprovalRequest> AlreadyDecided(ApprovalRequest request) =>
        ApprovalResult<ApprovalRequest>.Fail(409, "APPROVAL_ALREADY_DECIDED",
            $"Already {request.Status.ToLowerInvariant()} by {request.DecidedByName ?? "someone else"}.");

    private static ApprovalResult<ApprovalRequest> WrongState(ApprovalRequest request, string expected) =>
        ApprovalResult<ApprovalRequest>.Fail(409, "APPROVAL_STATE_CHANGED",
            $"The request is {request.Status.ToLowerInvariant()}{(request.DecidedByName is null ? "" : " by " + request.DecidedByName)}, not {expected.ToLowerInvariant()}.");

    private static string? CleanNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note)) return null;
        var trimmed = note.Trim();
        return trimmed.Length > MaxNoteLength ? trimmed[..MaxNoteLength] : trimmed;
    }

    /// <summary>Documents a request of <paramref name="kind"/> may not carry for this tenant, with the reason.</summary>
    private static (int Status, string Code, string Message)? RejectDocument(Tenant tenant, string kind, string documentType)
    {
        if (string.Equals(documentType, DocumentType, StringComparison.OrdinalIgnoreCase))
            return (400, "INVALID_APPROVAL_DOCUMENTS", "An approval request cannot contain another approval request.");
        var isCard = NativeDocumentProcessor.CardTypes.Contains(documentType);
        if (ApprovalKinds.CardKinds.Contains(kind))
        {
            // A card request carries exactly the card documents of its kind; Excel batches are not approved.
            if (ApprovalKinds.ForDocumentType(documentType) != kind)
                return (400, "INVALID_APPROVAL_DOCUMENTS", $"A {kind} request can only carry {kind} documents.");
        }
        else if (isCard)
        {
            return (400, "INVALID_APPROVAL_DOCUMENTS", "Product and customer cards are approved as product_card or customer_card requests.");
        }
        if (tenant.DataSource != TenantDataSources.Native && (isCard || NativeDocumentProcessor.NativeDocumentTypes.Contains(documentType)))
            return (409, "DOCUMENT_REQUIRES_NATIVE_TENANT", "This document is booked by the central API for a company without an ERP only.");
        return null;
    }

    private static string? Text(JsonElement item, string name) =>
        item.ValueKind == JsonValueKind.Object && item.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()!.Trim()
            : null;

    private static decimal? Number(JsonElement item, string name) =>
        item.ValueKind == JsonValueKind.Object && item.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number)
            ? number
            : null;
}
