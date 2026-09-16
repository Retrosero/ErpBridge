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
        group.MapPut("/data-source", SetDataSourceAsync).WithName("AdminMobileSetDataSource");
        group.MapGet("/approvals", ApprovalsAsync).WithName("AdminMobileApprovals");
        return routes;
    }

    /// <summary>
    /// Read-only view of the tenant's approval queue for support. <c>status</c> takes the
    /// same values as the phone's list; default <c>all</c>. Deciding stays with the company.
    /// </summary>
    private static async Task<IResult> ApprovalsAsync(Guid tenantId, [FromServices] CentralApiDbContext db, string? status, int? take, CancellationToken ct)
    {
        if (!await db.Tenants.AsNoTracking().AnyAsync(t => t.Id == tenantId, ct)) return TenantNotFound();
        var statuses = MobileApprovalEndpoints.ParseStatuses(string.IsNullOrWhiteSpace(status) ? "all" : status);
        if (statuses is null)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_STATUS", Message = "Unknown status." });
        var rows = await db.ApprovalRequests.AsNoTracking()
            .Where(r => r.TenantId == tenantId && statuses.Contains(r.Status))
            .OrderByDescending(r => r.RequestedSeq)
            .Take(Math.Clamp(take ?? MobileApprovalEndpoints.DefaultTake, 1, MobileApprovalEndpoints.MaxTake))
            .ToListAsync(ct);
        return JsonResults.Ok(rows.Select(ErpBridge.CentralApi.Approvals.ApprovalService.ToDto).ToArray());
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
            DataSource = tenant.DataSource,
            ApprovalRules = MobileApprovalEndpoints.RulesDto(await ErpBridge.CentralApi.Approvals.ApprovalService.RulesAsync(db, tenantId, ct), viewer: null),
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

    /// <summary>
    /// Decides where the company's data lives. The switch is refused once data from
    /// the other side exists, because the two would silently overwrite each other:
    /// an ERP upload knows nothing of cards created on phones, and a native tenant's
    /// sales would never reach an ERP.
    /// </summary>
    private static async Task<IResult> SetDataSourceAsync(Guid tenantId, [FromBody] SetDataSourceRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var value = body?.DataSource?.Trim().ToLowerInvariant();
        if (!TenantDataSources.IsValid(value))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_DATA_SOURCE", Message = "dataSource must be erp or native." });
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        if (db.Database.IsRelational())
        {
            // Same lock agent registration takes, so the checks below and a
            // registration cannot interleave.
            await db.Tenants.Where(t => t.Id == tenantId)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.NativeLockVersion, t => t.NativeLockVersion + 1), ct);
        }
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct);
        if (tenant is null) return TenantNotFound();
        if (tenant.DataSource == value) return Results.NoContent();

        if (value == TenantDataSources.Native)
        {
            // Documents already waiting for an agent would never be booked by the
            // native processor, nor reach an ERP any more.
            var hasErpData = await db.Agents.AnyAsync(a => a.TenantId == tenantId, ct)
                || await db.Jobs.AnyAsync(j => j.TenantId == tenantId && (j.Status == JobStatus.Pending || j.Status == JobStatus.Processing), ct)
                || await db.BootstrapSnapshots.AnyAsync(s => s.TenantId == tenantId, ct)
                || await db.BootstrapPackages.AnyAsync(p => p.TenantId == tenantId, ct)
                // Route plans and visits (Faz 39) are the team's in either mode and survive the switch.
                || await db.MobileRecords.AnyAsync(r => r.TenantId == tenantId
                    && r.SourceDatabase != Native.NativeDocumentProcessor.SourceName
                    && r.SourceDatabase != Team.TeamDocumentProcessor.SourceName, ct);
            if (hasErpData)
                return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError { ErrorCode = "TENANT_HAS_ERP_DATA", Message = "The tenant already has an ERP agent or ERP data." });
        }
        else if (await db.MobileRecords.AnyAsync(r => r.TenantId == tenantId && r.SourceDatabase == Native.NativeDocumentProcessor.SourceName, ct))
        {
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError { ErrorCode = "TENANT_HAS_NATIVE_DATA", Message = "Phones already created data for this tenant." });
        }

        tenant.DataSource = value!;
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
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
