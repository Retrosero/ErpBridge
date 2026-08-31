using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/admin/tenants</c>: list, create, get, patch. Admin-only.
/// </summary>
public static class AdminTenantsEndpoints
{
    public static IEndpointRouteBuilder MapAdminTenantsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/tenants")
            .WithTags("Admin/Tenants")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/", ListAsync)
            .WithName("AdminTenantsList")
            .Produces<TenantDto[]>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/", CreateAsync)
            .WithName("AdminTenantsCreate")
            .Produces<TenantDto>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetAsync)
            .WithName("AdminTenantsGet")
            .Produces<TenantDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}", PatchAsync)
            .WithName("AdminTenantsPatch")
            .Produces<TenantDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        return routes;
    }

    private static async Task<IResult> ListAsync(
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var rows = await db.Tenants.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);
        var dtos = rows.Select(ToDto).ToArray();
        return JsonResults.Ok(dtos);
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateTenantRequest body,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.Name))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_NAME", Message = "name is required." });
        if (body.MaxDeviceCount < 1)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_DEVICE_LIMIT", Message = "maxDeviceCount must be at least 1." });

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = body.Name.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsActive = true,
            MaxDeviceCount = body.MaxDeviceCount,
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);
        return JsonResults.Status(StatusCodes.Status201Created, ToDto(tenant));
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tenant is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "TENANT_NOT_FOUND", Message = "Tenant not found." });
        return JsonResults.Ok(ToDto(tenant));
    }

    private static async Task<IResult> PatchAsync(
        Guid id,
        [FromBody] PatchTenantRequest body,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tenant is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "TENANT_NOT_FOUND", Message = "Tenant not found." });

        if (body.IsActive.HasValue) tenant.IsActive = body.IsActive.Value;
        if (body.MaxDeviceCount is { } maxDeviceCount)
        {
            if (maxDeviceCount < 1)
                return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_DEVICE_LIMIT", Message = "maxDeviceCount must be at least 1." });
            tenant.MaxDeviceCount = maxDeviceCount;
        }
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(ToDto(tenant));
    }

    private static TenantDto ToDto(Tenant t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        CreatedAtUtc = t.CreatedAtUtc,
        IsActive = t.IsActive,
        MaxDeviceCount = t.MaxDeviceCount,
    };
}
