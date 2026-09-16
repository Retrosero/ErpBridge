using ErpBridge.CentralApi.Approvals;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/android/approvals</c>. Every approver of the company — its
/// administrators and the managers they allow — sees and decides the same queue; any
/// other user sees only the requests they sent. Requests are created through the
/// durable outbox (<c>/ingest/jobs</c>, document type <c>approval_request</c>).
/// </summary>
public static class MobileApprovalEndpoints
{
    public const int DefaultTake = 100;
    public const int MaxTake = 500;

    private delegate Task<ApprovalResult<ApprovalRequest>> Transition(
        ApprovalService approvals, CentralApiDbContext db, Tenant tenant, MobileUser actor, Guid id, string? note, CancellationToken ct);

    public static IEndpointRouteBuilder MapMobileApprovalEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/android/approvals")
            .WithTags("Android/Approvals")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapGet("/", ListAsync).WithName("MobileApprovalsList");
        group.MapGet("/summary", SummaryAsync).WithName("MobileApprovalsSummary");
        group.MapGet("/rules", GetRulesAsync).WithName("MobileApprovalRulesGet");
        group.MapPut("/rules", UpdateRulesAsync).WithName("MobileApprovalRulesUpdate");
        group.MapGet("/{id:guid}", DetailAsync).WithName("MobileApprovalsDetail");
        MapTransition(group, "approve", "MobileApprovalsApprove", (a, db, t, u, id, note, ct) => a.DecideAsync(db, t, u, id, approve: true, note, ct));
        MapTransition(group, "reject", "MobileApprovalsReject", (a, db, t, u, id, note, ct) => a.DecideAsync(db, t, u, id, approve: false, note, ct));
        MapTransition(group, "reopen", "MobileApprovalsReopen", (a, db, t, u, id, note, ct) => a.ReopenAsync(db, t, u, id, note, ct));
        MapTransition(group, "withdraw", "MobileApprovalsWithdraw", (a, db, t, u, id, note, ct) => a.WithdrawAsync(db, t, u, id, note, ct));
        return routes;
    }

    private static void MapTransition(RouteGroupBuilder group, string action, string name, Transition transition) =>
        group.MapPost("/{id:guid}/" + action, async (Guid id, HttpContext http, [FromBody] ApprovalDecisionRequest? body,
                [FromServices] CentralApiDbContext db, [FromServices] ApprovalService approvals, CancellationToken ct) =>
            {
                var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
                if (access.Error is not null) return access.Error;
                var result = await transition(approvals, db, access.Tenant!, access.User!, id, body?.Note, ct);
                return result.Succeeded
                    ? JsonResults.Ok(ApprovalService.ToDto(result.Value!))
                    : JsonResults.Status(result.StatusCode, result.Error);
            })
            .WithName(name);

    /// <summary>
    /// <c>status</c> is a comma-separated list (pending, approved, rejected, withdrawn,
    /// resubmitted) or <c>all</c>; default pending. <c>changedSinceSeq</c> returns only
    /// requests whose <c>updatedSeq</c> is at least that value. <c>beforeSeq</c> and
    /// <c>beforeExternalId</c> page back through older requests: pass the <c>requestedSeq</c> and
    /// <c>externalId</c> of the last one on screen (two requests can share a millisecond sequence,
    /// the external id breaks the tie). <c>kind</c> is a comma-separated list of request kinds.
    /// <c>order</c>: <c>newest</c> (default) or <c>oldest</c> — the portal's approval desk works the
    /// longest-waiting requests first, and with more than <c>take</c> pending the oldest must not fall
    /// outside the page. Without these the list is unchanged. Every row carries <c>canDecide</c> for the caller.
    /// </summary>
    private static async Task<IResult> ListAsync(HttpContext http, [FromServices] CentralApiDbContext db,
        string? status, long? changedSinceSeq, int? take, long? beforeSeq, string? beforeExternalId, string? kind, string? order, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;

        var statuses = ParseStatuses(status);
        if (statuses is null)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError
            {
                ErrorCode = "INVALID_STATUS",
                Message = "status must be all or a comma-separated list of: " + string.Join(", ", ApprovalStatuses.All.Select(s => s.ToLowerInvariant())) + ".",
            });

        var kinds = ParseKinds(kind);
        if (kinds is null)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError
            {
                ErrorCode = "INVALID_KIND",
                Message = "kind must be a comma-separated list of: " + string.Join(", ", ApprovalKinds.All) + ".",
            });
        var oldestFirst = string.Equals(order, "oldest", StringComparison.OrdinalIgnoreCase);
        if (!oldestFirst && !string.IsNullOrWhiteSpace(order) && !string.Equals(order, "newest", StringComparison.OrdinalIgnoreCase))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_ORDER", Message = "order must be newest or oldest." });

        var query = ApprovalService.Visible(db, access.Tenant!.Id, access.User!).Where(r => statuses.Contains(r.Status));
        if (changedSinceSeq is { } since) query = query.Where(r => r.UpdatedSeq >= since);
        if (beforeSeq is { } before)
        {
            query = string.IsNullOrEmpty(beforeExternalId)
                ? query.Where(r => r.RequestedSeq < before)
                : query.Where(r => r.RequestedSeq < before || (r.RequestedSeq == before && string.Compare(r.ExternalId, beforeExternalId) < 0));
        }
        if (kinds.Count > 0) query = query.Where(r => kinds.Contains(r.Kind));
        query = oldestFirst
            ? query.OrderBy(r => r.RequestedSeq).ThenBy(r => r.ExternalId)
            : query.OrderByDescending(r => r.RequestedSeq).ThenByDescending(r => r.ExternalId);
        var rows = await query.Take(Math.Clamp(take ?? DefaultTake, 1, MaxTake)).ToListAsync(ct);
        var viewer = access.User!;
        return JsonResults.Ok(rows.Select(row =>
        {
            var dto = ApprovalService.ToDto(row);
            dto.CanDecide = ApprovalPermissions.CanDecide(viewer, row.Kind);
            return dto;
        }).ToArray());
    }

    private static async Task<IResult> DetailAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] ApprovalService approvals, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var result = await approvals.DetailAsync(db, access.Tenant!, access.User!, id, ct);
        return result.Succeeded ? JsonResults.Ok(result.Value) : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static async Task<IResult> SummaryAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(await ApprovalService.SummaryAsync(db, access.Tenant!.Id, access.User!, ct));
    }

    private static async Task<IResult> GetRulesAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(RulesDto(await ApprovalService.RulesAsync(db, access.Tenant!.Id, ct), access.User!));
    }

    private static async Task<IResult> UpdateRulesAsync(HttpContext http, [FromBody] UpdateApprovalRulesRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] ApprovalService approvals, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var result = await approvals.UpdateRulesAsync(db, access.Tenant!, access.User!, body?.Rules, ct);
        return result.Succeeded ? JsonResults.Ok(RulesDto(result.Value!, access.User!)) : JsonResults.Status(result.StatusCode, result.Error);
    }

    internal static ApprovalRulesDto RulesDto(TenantApprovalRules rules, MobileUser? viewer) => new()
    {
        Rules = rules.ToMap(),
        CanManage = viewer is not null && ApprovalPermissions.CanManageRules(viewer),
        UpdatedByName = rules.UpdatedByName,
        UpdatedAtUtc = rules.UpdatedAtUtc,
    };

    /// <summary>An absent kind means every kind (empty list); an unknown one is refused (null).</summary>
    internal static List<string>? ParseKinds(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind)) return [];
        var parsed = new List<string>();
        foreach (var part in kind.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var match = ApprovalKinds.All.FirstOrDefault(k => k.Equals(part, StringComparison.OrdinalIgnoreCase));
            if (match is null) return null;
            if (!parsed.Contains(match)) parsed.Add(match);
        }
        return parsed;
    }

    internal static List<string>? ParseStatuses(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return [ApprovalStatuses.Pending];
        if (status.Trim().Equals("all", StringComparison.OrdinalIgnoreCase)) return ApprovalStatuses.All.ToList();
        var parsed = new List<string>();
        foreach (var part in status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var match = ApprovalStatuses.All.FirstOrDefault(s => s.Equals(part, StringComparison.OrdinalIgnoreCase));
            if (match is null) return null;
            if (!parsed.Contains(match)) parsed.Add(match);
        }
        return parsed.Count == 0 ? null : parsed;
    }
}
