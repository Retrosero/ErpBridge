using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Read-only endpoints that expose the parameter snapshot to Android clients
/// and to the admin UI. The agent push lives in
/// <see cref="ParameterEndpoints"/>; this class only handles GETs.
/// </summary>
public static class ParameterReadEndpoints
{
    public static IEndpointRouteBuilder MapParameterReadEndpoints(this IEndpointRouteBuilder routes)
    {
        // Android path — public-ish, guarded by the api-key policy.
        var androidGroup = routes.MapGroup("/api/v1/android/parameters").WithTags("AndroidParameters");
        androidGroup.MapGet("", ListAsync)
            .WithName("AndroidParameters")
            .RequireAuthorization(Program.ApiKeyPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);

        // Admin path — guarded by the admin policy.
        var adminGroup = routes.MapGroup("/api/v1/admin/parameters").WithTags("AdminParameters");
        adminGroup.MapGet("", AdminListAsync)
            .WithName("AdminParameterList")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        return routes;
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] string? sourceDatabase,
        [FromServices] CentralApiDbContext db,
        HttpContext http,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Tenant id missing." });
        }

        var query = db.Parameters.AsNoTracking().Where(p => p.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(sourceDatabase))
            query = query.Where(p => p.SourceDatabase == sourceDatabase);

        var rows = await query
            .OrderBy(p => p.ParametreProgram).ThenBy(p => p.ParametreUser).ThenBy(p => p.ParametreID)
            .Select(p => new
            {
                p.ParametreProgram,
                p.ParametreUser,
                p.ParametreAnaGrubu,
                p.ParametreAltGrubu,
                p.ParametreID,
                p.ParametreAdi,
                p.ParametreDegeri,
                p.UpdatedAtUtc,
            })
            .ToListAsync(ct);

        return JsonResults.Ok(new { tenantId, count = rows.Count, items = rows });
    }

    private static async Task<IResult> AdminListAsync(
        [FromQuery] string? sourceDatabase,
        [FromQuery] string? program,
        [FromQuery] string? user,
        [FromQuery] int? page,
        [FromQuery] int? size,
        [FromServices] CentralApiDbContext db,
        HttpContext http,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Tenant id missing." });
        }

        var pageSize = Math.Clamp(size ?? 200, 1, 1000);
        var pageIndex = Math.Max(1, page ?? 1);

        var query = db.Parameters.AsNoTracking().Where(p => p.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(sourceDatabase))
            query = query.Where(p => p.SourceDatabase == sourceDatabase);
        if (!string.IsNullOrWhiteSpace(program))
            query = query.Where(p => p.ParametreProgram == program);
        if (!string.IsNullOrWhiteSpace(user))
            query = query.Where(p => p.ParametreUser == user);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.ParametreProgram).ThenBy(p => p.ParametreUser).ThenBy(p => p.ParametreID)
            .Skip((pageIndex - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return JsonResults.Ok(new { tenantId, page = pageIndex, size = pageSize, total, items });
    }
}
