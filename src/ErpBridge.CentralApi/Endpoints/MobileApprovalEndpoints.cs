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
    /// requests whose <c>updatedSeq</c> is at least that value.
    /// </summary>
    private static async Task<IResult> ListAsync(HttpContext http, [FromServices] CentralApiDbContext db,
        string? status, long? changedSinceSeq, int? take, CancellationToken ct)
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

        var query = ApprovalService.Visible(db, access.Tenant!.Id, access.User!).Where(r => statuses.Contains(r.Status));
        if (changedSinceSeq is { } since) query = query.Where(r => r.UpdatedSeq >= since);
        var rows = await query.OrderByDescending(r => r.RequestedSeq).Take(Math.Clamp(take ?? DefaultTake, 1, MaxTake)).ToListAsync(ct);
        return JsonResults.Ok(rows.Select(ApprovalService.ToDto).ToArray());
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
