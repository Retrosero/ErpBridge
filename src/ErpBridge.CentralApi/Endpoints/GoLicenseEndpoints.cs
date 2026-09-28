using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.GoLicensing;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>POST /api/v1/go/license/activate</c>: the Go desktop app's only call to this server.
/// It binds a Go license to one computer and returns a signed token the app verifies offline.
/// The app repeats the call every few hours; each success extends the offline allowance.
/// </summary>
public static class GoLicenseEndpoints
{
    public static IEndpointRouteBuilder MapGoLicenseEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1/go/license/activate", ActivateAsync)
            .WithName("GoLicenseActivate")
            .WithTags("Go")
            .Produces<GoLicenseActivateResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status410Gone)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable)
            .AllowAnonymous()
            .RequireRateLimiting(Program.AnonymousRateLimitPolicy);
        return routes;
    }

    private static async Task<IResult> ActivateAsync(
        [FromBody] GoLicenseActivateRequest? body,
        [FromServices] CentralApiDbContext db,
        [FromServices] IGoLicenseSigner signer,
        [FromServices] IOptions<GoLicenseOptions> options,
        CancellationToken ct)
    {
        var licenseKey = body?.LicenseKey?.Trim();
        var machineId = body?.MachineId?.Trim();
        if (string.IsNullOrEmpty(licenseKey))
            return Error(StatusCodes.Status400BadRequest, "MISSING_LICENSE_KEY", "licenseKey is required.");
        if (string.IsNullOrEmpty(machineId) || machineId.Length > GoInstallation.MachineIdMaxLength)
            return Error(StatusCodes.Status400BadRequest, "INVALID_MACHINE_ID", "machineId is required (at most 128 characters).");

        if (!signer.IsConfigured)
            return Error(StatusCodes.Status503ServiceUnavailable, "GO_LICENSING_UNAVAILABLE", "Go licensing is not configured on this server.");

        var license = await db.Licenses
            .Include(l => l.Tenant)
            .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey, ct);

        // A key of another product is reported exactly like an unknown key.
        if (license is null || license.Product != LicenseProducts.Go)
            return Error(StatusCodes.Status404NotFound, "LICENSE_NOT_FOUND", "License key not recognised.");
        if (!license.IsActive || license.Tenant is { IsActive: false })
            return Error(StatusCodes.Status410Gone, "LICENSE_REVOKED", "License is inactive.");

        var now = DateTimeOffset.UtcNow;
        if (license.ExpiresAtUtc is { } expires && expires <= now)
            return Error(StatusCodes.Status410Gone, "LICENSE_EXPIRED", "License has expired.");

        var machineName = Clip(body!.MachineName, GoInstallation.MachineNameMaxLength);
        var appVersion = Clip(body.AppVersion, GoInstallation.AppVersionMaxLength);

        var installation = await db.GoInstallations.FirstOrDefaultAsync(i => i.LicenseId == license.Id, ct);
        if (installation is null)
        {
            installation = new GoInstallation
            {
                Id = Guid.NewGuid(),
                LicenseId = license.Id,
                TenantId = license.TenantId,
                MachineId = machineId,
                ActivatedAtUtc = now,
            };
            db.GoInstallations.Add(installation);
        }
        else if (!string.Equals(installation.MachineId, machineId, StringComparison.Ordinal))
        {
            return Error(StatusCodes.Status409Conflict, "DEVICE_LIMIT_REACHED", "This license is already active on another computer.");
        }

        installation.LastSeenAtUtc = now;
        installation.MachineName = machineName ?? installation.MachineName;
        installation.AppVersion = appVersion ?? installation.AppVersion;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Two first activations raced; the unique index on LicenseId kept one machine.
            return Error(StatusCodes.Status409Conflict, "DEVICE_LIMIT_REACHED", "This license is already active on another computer.");
        }

        var modules = await db.TenantModules
            .AsNoTracking()
            .Where(m => m.TenantId == license.TenantId && m.ModuleKey.StartsWith("go_"))
            .Select(m => m.ModuleKey)
            .OrderBy(k => k)
            .ToArrayAsync(ct);

        var validUntil = now + options.Value.OfflineAllowance;
        if (license.ExpiresAtUtc is { } end && end < validUntil) validUntil = end;

        var claims = new GoLicenseClaims
        {
            LicenseId = license.Id,
            TenantId = license.TenantId,
            TenantName = license.Tenant?.Name ?? string.Empty,
            MachineId = machineId,
            IssuedAtUtc = now,
            ValidUntilUtc = validUntil,
            LicenseExpiresAtUtc = license.ExpiresAtUtc,
            Modules = modules,
        };

        return JsonResults.Ok(new GoLicenseActivateResponse
        {
            Token = signer.Sign(claims),
            TenantName = claims.TenantName,
            ValidUntilUtc = claims.ValidUntilUtc,
            LicenseExpiresAtUtc = claims.LicenseExpiresAtUtc,
            Modules = modules,
        });
    }

    private static string? Clip(string? value, int max)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }

    private static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });
}
