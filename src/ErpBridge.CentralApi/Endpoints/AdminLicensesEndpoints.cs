using System.Security.Cryptography;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/admin/licenses</c>: list, create, revoke. Admin-only.
/// </summary>
public static class AdminLicensesEndpoints
{
    private const string LicensePrefix = "LIC-";

    public static IEndpointRouteBuilder MapAdminLicensesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/licenses")
            .WithTags("Admin/Licenses")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/", ListAsync)
            .WithName("AdminLicensesList")
            .Produces<LicenseDto[]>(StatusCodes.Status200OK);

        group.MapPost("/", CreateAsync)
            .WithName("AdminLicensesCreate")
            .Produces<LicenseDto>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/revoke", RevokeAsync)
            .WithName("AdminLicensesRevoke")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        return routes;
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] Guid? tenantId,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var query = db.Licenses.AsNoTracking();
        if (tenantId.HasValue) query = query.Where(l => l.TenantId == tenantId.Value);
        var rows = await query.OrderByDescending(l => l.IssuedAtUtc).ToListAsync(ct);
        return JsonResults.Ok(rows.Select(ToDto).ToArray());
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateLicenseRequest body,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null || body.TenantId == Guid.Empty)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_TENANT", Message = "tenantId is required." });

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == body.TenantId, ct);
        if (tenant is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "TENANT_NOT_FOUND", Message = "Tenant not found." });

        var license = new License
        {
            Id = Guid.NewGuid(),
            TenantId = body.TenantId,
            LicenseKey = GenerateLicenseKey(),
            IssuedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = body.ExpiresAtUtc,
            IsActive = true,
        };
        db.Licenses.Add(license);
        await db.SaveChangesAsync(ct);
        return JsonResults.Status(StatusCodes.Status201Created, ToDto(license));
    }

    private static async Task<IResult> RevokeAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var license = await db.Licenses.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (license is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "LICENSE_NOT_FOUND", Message = "License not found." });
        license.IsActive = false;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    /// <summary>Generate a random license key with the <c>LIC-</c> prefix and 32 lowercase hex chars.</summary>
    private static string GenerateLicenseKey()
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);
        return LicensePrefix + Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static LicenseDto ToDto(License l) => new()
    {
        Id = l.Id,
        TenantId = l.TenantId,
        LicenseKey = l.LicenseKey,
        IssuedAtUtc = l.IssuedAtUtc,
        ExpiresAtUtc = l.ExpiresAtUtc,
        IsActive = l.IsActive,
    };
}
