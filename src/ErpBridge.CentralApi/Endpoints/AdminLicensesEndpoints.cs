using System.Security.Cryptography;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/admin/licenses</c>: list, create, revoke, change the end date and release the
/// computer a Go license is bound to. Admin-only.
/// </summary>
public static class AdminLicensesEndpoints
{
    private const string LicensePrefix = "LIC-";
    private const string GoLicensePrefix = "GO-";

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

        group.MapPut("/{id:guid}/expiry", UpdateExpiryAsync)
            .WithName("AdminLicensesUpdateExpiry")
            .Produces<LicenseDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/go-installation/release", ReleaseGoInstallationAsync)
            .WithName("AdminLicensesReleaseGoInstallation")
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

        var goIds = rows.Where(l => l.Product == LicenseProducts.Go).Select(l => l.Id).ToList();
        var installations = goIds.Count == 0
            ? new Dictionary<Guid, GoInstallation>()
            : await db.GoInstallations.AsNoTracking()
                .Where(i => goIds.Contains(i.LicenseId))
                .ToDictionaryAsync(i => i.LicenseId, ct);

        return JsonResults.Ok(rows.Select(l => ToDto(l, installations.GetValueOrDefault(l.Id))).ToArray());
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateLicenseRequest body,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null || body.TenantId == Guid.Empty)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_TENANT", Message = "tenantId is required." });

        var product = string.IsNullOrWhiteSpace(body.Product) ? LicenseProducts.ErpBridge : body.Product.Trim().ToLowerInvariant();
        if (!LicenseProducts.IsValid(product))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "UNKNOWN_PRODUCT", Message = "product must be 'erpbridge' or 'go'." });

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == body.TenantId, ct);
        if (tenant is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "TENANT_NOT_FOUND", Message = "Tenant not found." });

        var license = new License
        {
            Id = Guid.NewGuid(),
            TenantId = body.TenantId,
            LicenseKey = GenerateLicenseKey(product),
            Product = product,
            IssuedAtUtc = DateTimeOffset.UtcNow,
            // The admin panel's date picker sends a DateTimeOffset carrying the
            // browser's local offset (e.g. +03:00). Npgsql's `timestamp with
            // time zone` only accepts offset 0, so normalise to UTC — same
            // instant, offset 0 — before persisting.
            ExpiresAtUtc = body.ExpiresAtUtc?.ToUniversalTime(),
            IsActive = true,
        };
        db.Licenses.Add(license);
        await db.SaveChangesAsync(ct);
        return JsonResults.Status(StatusCodes.Status201Created, ToDto(license, null));
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

    /// <summary>
    /// Renewal keeps the key: the customer's installed app picks the new end date up on its next
    /// renewal call, without typing anything.
    /// </summary>
    private static async Task<IResult> UpdateExpiryAsync(
        Guid id,
        [FromBody] UpdateLicenseExpiryRequest body,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var license = await db.Licenses.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (license is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "LICENSE_NOT_FOUND", Message = "License not found." });
        license.ExpiresAtUtc = body?.ExpiresAtUtc?.ToUniversalTime();
        await db.SaveChangesAsync(ct);
        var installation = license.Product == LicenseProducts.Go
            ? await db.GoInstallations.AsNoTracking().FirstOrDefaultAsync(i => i.LicenseId == id, ct)
            : null;
        return JsonResults.Ok(ToDto(license, installation));
    }

    /// <summary>
    /// Frees a Go license for another computer. The released machine keeps syncing only until its
    /// current token runs out (at most the offline allowance), then its renewal answers 409.
    /// </summary>
    private static async Task<IResult> ReleaseGoInstallationAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var installation = await db.GoInstallations.FirstOrDefaultAsync(i => i.LicenseId == id, ct);
        if (installation is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "INSTALLATION_NOT_FOUND", Message = "This license is not bound to a computer." });
        db.GoInstallations.Remove(installation);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    /// <summary>Random key: <c>LIC-</c> (ErpBridge) or <c>GO-</c> (Go) and 32 lowercase hex chars.</summary>
    private static string GenerateLicenseKey(string product)
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);
        var prefix = product == LicenseProducts.Go ? GoLicensePrefix : LicensePrefix;
        return prefix + Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static LicenseDto ToDto(License l, GoInstallation? installation) => new()
    {
        Id = l.Id,
        TenantId = l.TenantId,
        LicenseKey = l.LicenseKey,
        IssuedAtUtc = l.IssuedAtUtc,
        ExpiresAtUtc = l.ExpiresAtUtc,
        IsActive = l.IsActive,
        Product = l.Product,
        GoInstallation = installation is null ? null : new GoInstallationDto
        {
            MachineName = installation.MachineName,
            AppVersion = installation.AppVersion,
            ActivatedAtUtc = installation.ActivatedAtUtc,
            LastSeenAtUtc = installation.LastSeenAtUtc,
        },
    };
}
