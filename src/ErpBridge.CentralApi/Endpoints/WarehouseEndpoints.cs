using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Notifications;
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

    /// <summary>
    /// Long-poll: answers at once when the queue changed after <c>sinceSeq</c>, otherwise waits up to
    /// <c>wait</c> seconds (0-25) for a change. The page then reads <c>fulfillments?changedSinceSeq</c>.
    /// </summary>
    private static async Task<IResult> EventsAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] ITenantEventHub events,
        long? sinceSeq, int? wait, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, RolePermissions.CanOperateWarehouse, ct);
        if (error is not null) return error;
        var since = sinceSeq ?? 0;
        var latest = await FulfillmentService.LatestSeqAsync(db, tenant!.Id, ct);
        var seconds = Math.Clamp(wait ?? 0, 0, MaxWaitSeconds);
        if (latest <= since && seconds > 0)
        {
            await events.WaitAsync(tenant.Id, TimeSpan.FromSeconds(seconds), ct);
            latest = await FulfillmentService.LatestSeqAsync(db, tenant.Id, ct);
        }
        return JsonResults.Ok(new PortalEventsResponse { LatestSeq = latest, Changed = latest > since });
    }

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

    private static IReadOnlyCollection<string>? ParseStatuses(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Equals("open", StringComparison.OrdinalIgnoreCase)) return FulfillmentStatuses.Open;
        if (value.Trim().Equals("all", StringComparison.OrdinalIgnoreCase)) return FulfillmentStatuses.All;
        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(p => p.ToUpperInvariant()).ToList();
        return parts.Count > 0 && parts.All(FulfillmentStatuses.All.Contains) ? parts : null;
    }
}
