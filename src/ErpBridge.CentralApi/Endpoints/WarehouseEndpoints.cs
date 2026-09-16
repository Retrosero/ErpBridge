using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using System.Globalization;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Warehouse;
using Microsoft.AspNetCore.Mvc;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps the warehouse queue under <c>/api/v1/portal</c> (plan step 4, Faz 47). Signed in with the company
/// account like the rest of the portal; every call re-checks the user and reads the roles from the database.
/// Reading and moving orders needs a warehouse role (ADMIN, MANAGER, WAREHOUSE); cancelling, reassigning
/// and the settings need ADMIN or MANAGER.
/// </summary>
public static class WarehouseEndpoints
{
    /// <summary>The longest a long-poll holds the connection; below common proxy idle timeouts.</summary>
    public const int MaxWaitSeconds = 25;

    public static IEndpointRouteBuilder MapWarehouseEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal")
            .WithTags("Warehouse")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapGet("/fulfillments", ListAsync).WithName("WarehouseFulfillmentsList");
        group.MapGet("/fulfillments/{id:guid}", DetailAsync).WithName("WarehouseFulfillmentDetail");
        group.MapPost("/fulfillments/{id:guid}/{action}", ActAsync).WithName("WarehouseFulfillmentAction");
        group.MapGet("/warehouse/settings", GetSettingsAsync).WithName("WarehouseSettingsGet");
        group.MapPut("/warehouse/settings", PutSettingsAsync).WithName("WarehouseSettingsPut");
        group.MapGet("/warehouse/dashboard", DashboardAsync).WithName("WarehouseDashboard");
        group.MapGet("/warehouse/performance", PerformanceAsync).WithName("WarehousePerformance");
        group.MapPost("/warehouse/backfill", BackfillAsync).WithName("WarehouseBackfill");
        group.MapGet("/events", EventsAsync).WithName("PortalEvents");
        return routes;
    }

    /// <summary>
    /// <c>status</c>: comma-separated statuses, <c>open</c> (default: pending, preparing, packed) or <c>all</c>.
    /// <c>changedSinceSeq</c>: every order changed after it, whatever its status.
    /// </summary>
    private static async Task<IResult> ListAsync(HttpContext http, [FromServices] CentralApiDbContext db,
        string? status, long? changedSinceSeq, int? take, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, RolePermissions.CanOperateWarehouse, ct);
        if (error is not null) return error;
        var statuses = ParseStatuses(status);
        if (statuses is null)
            return JsonResults.Status(400, new ApiError
            {
                ErrorCode = "INVALID_STATUS",
                Message = "status must be open, all or a comma-separated list of: " + string.Join(", ", FulfillmentStatuses.All.Select(s => s.ToLowerInvariant())) + ".",
            });
        return JsonResults.Ok(await FulfillmentService.ListAsync(db, tenant!.Id, statuses, changedSinceSeq, take ?? FulfillmentService.MaxListSize, ct));
    }

    private static async Task<IResult> DetailAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, RolePermissions.CanOperateWarehouse, ct);
        if (error is not null) return error;
        var result = await FulfillmentService.DetailAsync(db, tenant!.Id, id, ct);
        return result.Succeeded ? JsonResults.Ok(result.Value) : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static async Task<IResult> ActAsync(Guid id, string action, HttpContext http, [FromBody] FulfillmentActionRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] FulfillmentService warehouse, CancellationToken ct)
    {
        // The service tells cancel/reassign (managers) from the steps (warehouse staff).
        var (tenant, user, error) = await AuthorizeAsync(http, db, _ => true, ct);
        if (error is not null) return error;
        var result = await warehouse.ActAsync(db, tenant!.Id, user!, http.User.FindFirst(CentralApiClaims.DeviceId)?.Value, id, action, body, ct);
        return result.Succeeded ? JsonResults.Ok(result.Value) : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static async Task<IResult> GetSettingsAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, RolePermissions.CanOperateWarehouse, ct);
        if (error is not null) return error;
        return JsonResults.Ok(await FulfillmentService.SettingsAsync(db, tenant!.Id, ct));
    }

    private static async Task<IResult> PutSettingsAsync(HttpContext http, [FromBody] WarehouseSettingsDto? body,
        [FromServices] CentralApiDbContext db, [FromServices] FulfillmentService warehouse, CancellationToken ct)
    {
        var (tenant, user, error) = await AuthorizeAsync(http, db, RolePermissions.CanOperateWarehouse, ct);
        if (error is not null) return error;
        var result = await warehouse.UpdateSettingsAsync(db, tenant!.Id, user!, body, ct);
        return result.Succeeded ? JsonResults.Ok(result.Value) : JsonResults.Status(result.StatusCode, result.Error);
    }

    /// <summary>The warehouse today for the manager's home page (plan step 8). <c>date</c>: yyyy-MM-dd, default today in Istanbul.</summary>
    private static async Task<IResult> DashboardAsync(HttpContext http, [FromServices] CentralApiDbContext db, string? date, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeReportsAsync(http, db, ct);
        if (error is not null) return error;
        if (!TryDay(date, out var day)) return BadDate("date");
        return JsonResults.Ok(await FulfillmentReports.DashboardAsync(db, tenant!.Id, day, DateTimeOffset.UtcNow, ct));
    }

    /// <summary>Totals, packers and slowest orders of <c>from</c>..<c>to</c> (at most <see cref="PortalReports.MaxRangeDays"/> days).</summary>
    private static async Task<IResult> PerformanceAsync(HttpContext http, [FromServices] CentralApiDbContext db, string? from, string? to, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeReportsAsync(http, db, ct);
        if (error is not null) return error;
        if (!TryDay(from, out var start)) return BadDate("from");
        if (!TryDay(to, out var end)) return BadDate("to");
        if (end < start) return JsonResults.Status(400, new ApiError { ErrorCode = "INVALID_RANGE", Message = "to is before from." });
        if (end.DayNumber - start.DayNumber + 1 > PortalReports.MaxRangeDays)
            return JsonResults.Status(400, new ApiError { ErrorCode = "RANGE_TOO_LONG", Message = $"At most {PortalReports.MaxRangeDays} days." });
        return JsonResults.Ok(await FulfillmentReports.PerformanceAsync(db, tenant!.Id, start, end, ct));
    }

    /// <summary>Queues the sales orders of the last <c>days</c> (default 2, at most 30) that are not queued yet.</summary>
    private static async Task<IResult> BackfillAsync(HttpContext http, [FromBody] WarehouseBackfillRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] FulfillmentService warehouse, CancellationToken ct)
    {
        var (tenant, user, error) = await AuthorizeAsync(http, db, RolePermissions.CanOperateWarehouse, ct);
        if (error is not null) return error;
        var result = await warehouse.BackfillAsync(db, tenant!, user!, body?.Days ?? 2, ct);
        return result.Succeeded ? JsonResults.Ok(result.Value) : JsonResults.Status(result.StatusCode, result.Error);
    }

    /// <summary>
    /// Long-poll for the portal's live pages. Topics: the warehouse queue (<c>sinceSeq</c>, its change
    /// counter, for a warehouse role) and approval requests (<c>approvalsVersion</c>, the hub's publish
    /// count for the tenant; Faz 48). Answers at once when a topic the caller asked about moved, otherwise
    /// waits up to <c>wait</c> seconds (0-25). The page then reads the topic's list.
    ///
    /// <para>Approvals are followed by a version rather than their <c>UpdatedSeq</c>: those are milliseconds
    /// taken before commit (rule 16), so a late commit can carry a smaller number than one already seen.
    /// The version grows on every publish, so a change published between two polls — with no waiter
    /// registered — still shows as a different version (Codex, PR #57).</para>
    /// </summary>
    private static async Task<IResult> EventsAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] ITenantEventHub events,
        long? sinceSeq, long? approvalsVersion, int? wait, CancellationToken ct)
    {
        var (tenant, user, error) = await AuthorizeAsync(http, db, CanFollowEvents, ct);
        if (error is not null) return error;
        var warehouse = RolePermissions.CanOperateWarehouse(user!);
        var watchWarehouse = warehouse && (sinceSeq is not null || approvalsVersion is null);
        var since = sinceSeq ?? 0;
        var seconds = Math.Clamp(wait ?? 0, 0, MaxWaitSeconds);
        // Subscribe before reading: a change that commits between the read and the wait still wakes us.
        // (WaitAsync registers the waiter before it returns.) An unused waiter ends with its timeout.
        var waiting = seconds > 0 ? events.WaitAsync(tenant!.Id, TimeSpan.FromSeconds(seconds), http.RequestAborted) : null;

        async Task<(long Queue, long Approvals)> ReadAsync() => (
            warehouse ? await FulfillmentService.LatestSeqAsync(db, tenant!.Id, ct) : 0,
            events.Version(tenant!.Id, TenantEventTopics.Approvals));
        bool Moved((long Queue, long Approvals) now) =>
            (watchWarehouse && now.Queue > since) || (approvalsVersion is { } seen && now.Approvals != seen);

        var latest = await ReadAsync();
        if (!Moved(latest) && waiting is not null)
        {
            await waiting;
            latest = await ReadAsync();
        }
        return JsonResults.Ok(new PortalEventsResponse
        {
            LatestSeq = latest.Queue,
            ApprovalsVersion = latest.Approvals,
            Changed = Moved(latest),
        });
    }

    /// <summary>Anyone with a portal role follows the topics their roles open.</summary>
    private static bool CanFollowEvents(MobileUser user) =>
        RolePermissions.CanOperateWarehouse(user) || RolePermissions.CanUsePortal(user);

    private static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeAsync(
        HttpContext http, CentralApiDbContext db, Func<MobileUser, bool> allowed, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access;
        if (!allowed(access.User!))
            return (null, null, JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
            {
                ErrorCode = "WAREHOUSE_ROLE_REQUIRED",
                Message = "Your roles do not include warehouse work.",
            }));
        return access;
    }

    /// <summary>Warehouse reports are company reports: administrators and managers, like the other reports.</summary>
    private static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeReportsAsync(
        HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access;
        if (!RolePermissions.CanViewReports(access.User!))
            return (null, null, JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
            {
                ErrorCode = "PORTAL_REQUIRES_MANAGER",
                Message = "The manager portal is for company administrators and managers.",
            }));
        return access;
    }

    /// <summary>An absent day means today in Istanbul.</summary>
    private static bool TryDay(string? value, out DateOnly day)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            day = PortalReports.IstanbulDay(DateTimeOffset.UtcNow);
            return true;
        }
        return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day);
    }

    private static IResult BadDate(string name) =>
        JsonResults.Status(400, new ApiError { ErrorCode = "INVALID_DATE", Message = $"{name} must be yyyy-MM-dd." });

    private static IReadOnlyCollection<string>? ParseStatuses(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Equals("open", StringComparison.OrdinalIgnoreCase)) return FulfillmentStatuses.Open;
        if (value.Trim().Equals("all", StringComparison.OrdinalIgnoreCase)) return FulfillmentStatuses.All;
        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(p => p.ToUpperInvariant()).ToList();
        return parts.Count > 0 && parts.All(FulfillmentStatuses.All.Contains) ? parts : null;
    }
}
