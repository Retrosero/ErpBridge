using System.Security.Claims;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Mobile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/admin/tenants/{tenantId}/mobile</c>: the operator's view of a
/// tenant's paid seats, users and phones. Seats are sold outside the app; after
/// a payment the operator records the subscription here and, for a new company,
/// creates its first administrator. The tenant comes from the route, never from
/// the admin token (admin tokens carry no tenant claim).
/// </summary>
public static class AdminMobileSeatsEndpoints
{
    public static IEndpointRouteBuilder MapAdminMobileSeatsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/tenants/{tenantId:guid}/mobile")
            .WithTags("Admin/MobileSeats")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/", OverviewAsync).WithName("AdminMobileOverview")
            .Produces<TenantMobileOverviewResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);
        group.MapPut("/subscription", SetSubscriptionAsync).WithName("AdminMobileSetSubscription");
        group.MapPost("/users", CreateUserAsync).WithName("AdminMobileCreateUser");
        group.MapPatch("/users/{userId:guid}", UpdateUserAsync).WithName("AdminMobileUpdateUser");
        group.MapDelete("/users/{userId:guid}", DeleteUserAsync).WithName("AdminMobileDeleteUser");
        group.MapPatch("/devices/{deviceId:guid}", UpdateDeviceAsync).WithName("AdminMobileUpdateDevice");
        return routes;
    }

    private static async Task<IResult> OverviewAsync(Guid tenantId, [FromServices] CentralApiDbContext db, [FromServices] MobileSeatService seats, CancellationToken ct)
    {
        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, ct);
        if (tenant is null) return TenantNotFound();

        var users = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.DeletedAtUtc == null)
            .OrderBy(u => u.Username).ToListAsync(ct);
        // DateTimeOffset ordering is applied in memory: SQLite (used by the
        // relational tests) cannot translate it, and these lists are small.
        var subscriptions = (await db.TenantSubscriptions.AsNoTracking().Where(s => s.TenantId == tenantId).ToListAsync(ct))
            .OrderByDescending(s => s.CreatedAtUtc).ToList();
        var devices = (await db.MobileDevices.AsNoTracking().Include(d => d.LastUser).Where(d => d.TenantId == tenantId).ToListAsync(ct))
            .OrderByDescending(d => d.LastSeenAtUtc).ToList();

        return JsonResults.Ok(new TenantMobileOverviewResponse
        {
            TenantId = tenant.Id,
            TenantCode = tenant.Code,
            Seats = await seats.GetUsageAsync(tenantId, ct),
            Subscriptions = subscriptions.Select(ToDto).ToArray(),
            Users = users.Select(MobileAccountEndpoints.ToDto).ToArray(),
            Devices = devices.Select(d => new MobileDeviceDto
            {
                Id = d.Id,
                DeviceId = d.DeviceId,
                LastUserId = d.LastUserId,
                LastUsername = d.LastUser?.Username,
                AppVersion = d.AppVersion,
                IsActive = d.IsActive,
                FirstSeenAtUtc = d.FirstSeenAtUtc,
                LastSeenAtUtc = d.LastSeenAtUtc,
            }).ToArray(),
        });
    }

    private static async Task<IResult> SetSubscriptionAsync(Guid tenantId, HttpContext http, [FromBody] SetSubscriptionRequest? body, [FromServices] MobileSeatService seats, CancellationToken ct)
    {
        if (body is null) return BadBody();
        Guid? adminId = Guid.TryParse(http.User.FindFirstValue("sub"), out var parsed) ? parsed : null;
        var result = await seats.SetSubscriptionAsync(tenantId, body, adminId, ct);
        return result.Succeeded ? JsonResults.Ok(ToDto(result.Value!)) : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static async Task<IResult> CreateUserAsync(Guid tenantId, [FromBody] CreateMobileUserRequest? body, [FromServices] MobileSeatService seats, CancellationToken ct)
    {
        if (body is null) return BadBody();
        var result = await seats.CreateUserAsync(tenantId, body, ct);
        return result.Succeeded
            ? JsonResults.Status(StatusCodes.Status201Created, MobileAccountEndpoints.ToDto(result.Value!))
            : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static async Task<IResult> UpdateUserAsync(Guid tenantId, Guid userId, [FromBody] UpdateMobileUserRequest? body, [FromServices] MobileSeatService seats, CancellationToken ct)
    {
        if (body is null) return BadBody();
        var result = await seats.UpdateUserAsync(tenantId, userId, body, ct);
        return result.Succeeded ? JsonResults.Ok(MobileAccountEndpoints.ToDto(result.Value!)) : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static async Task<IResult> DeleteUserAsync(Guid tenantId, Guid userId, [FromServices] MobileSeatService seats, CancellationToken ct)
    {
        var result = await seats.DeleteUserAsync(tenantId, userId, ct);
        return result.Succeeded ? Results.NoContent() : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static async Task<IResult> UpdateDeviceAsync(Guid tenantId, Guid deviceId, [FromBody] UpdateMobileDeviceRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        if (body?.IsActive is not { } isActive) return BadBody();
        var device = await db.MobileDevices.FirstOrDefaultAsync(d => d.Id == deviceId && d.TenantId == tenantId, ct);
        if (device is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "DEVICE_NOT_FOUND", Message = "Device not found." });
        device.IsActive = isActive;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static SubscriptionDto ToDto(TenantSubscription s) => new()
    {
        Id = s.Id,
        Seats = s.Seats,
        StartsAtUtc = s.StartsAtUtc,
        EndsAtUtc = s.EndsAtUtc,
        Source = s.Source,
        Reference = s.Reference,
        Note = s.Note,
        IsCurrent = s.IsCurrent,
        CreatedAtUtc = s.CreatedAtUtc,
        CreatedByAdminId = s.CreatedByAdminId,
    };

    private static IResult TenantNotFound() =>
        JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "TENANT_NOT_FOUND", Message = "Tenant not found." });

    private static IResult BadBody() =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });
}
