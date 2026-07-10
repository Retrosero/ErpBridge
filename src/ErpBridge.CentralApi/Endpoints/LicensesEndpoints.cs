using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>POST /api/v1/licenses/validate</c>. This is the only public endpoint
/// the agent calls before registration to confirm a license is legitimate.
/// </summary>
public static class LicensesEndpoints
{
    /// <summary>Register an <see cref="IEndpointRouteBuilder"/> extension that maps the validate endpoint.</summary>
    public static IEndpointRouteBuilder MapLicensesEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1/licenses/validate", ValidateAsync)
            .WithName("LicensesValidate")
            .WithTags("Licenses")
            .Produces<LicenseValidateResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status410Gone)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .AllowAnonymous()
            .RequireRateLimiting("Anonymous");
        return routes;
    }

    private static async Task<IResult> ValidateAsync(
        [FromBody] LicenseValidateRequest body,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.LicenseKey))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_LICENSE_KEY", Message = "licenseKey is required." });

        var license = await db.Licenses
            .Include(l => l.Tenant)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LicenseKey == body.LicenseKey, ct);

        if (license is null)
        {
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "LICENSE_NOT_FOUND", Message = "License key not recognised." });
        }

        if (!license.IsActive || (license.Tenant is { IsActive: false }))
        {
            return JsonResults.Status(StatusCodes.Status410Gone, new ApiError { ErrorCode = "LICENSE_INVALID", Message = "License is inactive." });
        }

        if (license.ExpiresAtUtc is { } exp && exp <= DateTimeOffset.UtcNow)
        {
            return JsonResults.Status(StatusCodes.Status410Gone, new ApiError { ErrorCode = "LICENSE_EXPIRED", Message = "License has expired." });
        }

        return JsonResults.Ok(new LicenseValidateResponse
        {
            Valid = true,
            TenantId = license.TenantId,
            ExpiresAtUtc = license.ExpiresAtUtc,
        });
    }
}
