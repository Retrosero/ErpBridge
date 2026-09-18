using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Parameters;
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

    /// <summary>Mikro's <c>ParametreProgram</c> the phone reads. It has exactly one set.</summary>
    private const string AndroidProgram = "akilli";

    /// <summary>The only catalogue set of <see cref="AndroidProgram"/>: 1,801 per-user parameters.</summary>
    private const string AndroidSet = "MobilKullanici";

    /// <summary>
    /// The parameters the phone applies, in the shape it has always received them.
    ///
    /// The values now come from the catalogue and the stored deviations rather than from the
    /// imported <c>_FORA_PARAMETRELER</c> mirror (D13), so a parameter nobody has changed is
    /// returned at Fora's own default instead of being absent and left to whatever the phone
    /// happened to hard-code. The row shape is unchanged: <c>ParametreUser</c> still carries the
    /// username, because that is what an older client matches on.
    ///
    /// Only active users are published (D5b). A username is reusable, so a deleted user's rows
    /// would otherwise reach a phone logged in as the new holder of that name.
    ///
    /// The whole answer runs to 1,801 rows per user, so it carries an <c>ETag</c>: a client that
    /// sends it back gets 304 and downloads nothing.
    /// </summary>
    private static async Task<IResult> ListAsync(
        [FromQuery] string? sourceDatabase,
        [FromServices] CentralApiDbContext db,
        [FromServices] ParameterResolver resolver,
        HttpContext http,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Tenant id missing." });
        }

        // No sourceDatabase means every company of the tenant, which is what the old endpoint did
        // when the filter was left off.
        var companyQuery = db.ErpCompanies.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.IsActive);

        if (!string.IsNullOrWhiteSpace(sourceDatabase))
        {
            companyQuery = companyQuery.Where(c => c.SourceDatabase == sourceDatabase);
        }

        var companies = await companyQuery
            .Select(c => new { c.Id, c.SourceDatabase })
            .OrderBy(c => c.SourceDatabase)
            .ToListAsync(ct);

        var users = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.IsActive && u.DeletedAtUtc == null)
            .Select(u => new { u.Id, u.Username })
            .OrderBy(u => u.Username)
            .ToListAsync(ct);

        var scopes = (from c in companies
                      from u in users
                      select ParameterScope.ForMobileUser(tenantId, c.Id, u.Id)).ToList();

        var revisions = await resolver.RevisionsAsync(scopes, ct);

        // The catalogue is part of the answer: reseeding it can move a default, and the client
        // would otherwise keep a copy that no longer matches.
        var catalogStamp = await db.ParameterCatalog.AsNoTracking()
            .Where(e => e.CatalogMethod == AndroidSet)
            .Select(e => (DateTimeOffset?)e.UpdatedAtUtc)
            .MaxAsync(ct);

        var etag = ETag(scopes, revisions, catalogStamp);
        http.Response.Headers.ETag = etag;

        if (http.Request.Headers.IfNoneMatch.Contains(etag))
        {
            return Results.StatusCode(StatusCodes.Status304NotModified);
        }

        var companyDatabase = companies.ToDictionary(c => c.Id, c => c.SourceDatabase);
        var username = users.ToDictionary(u => u.Id, u => u.Username);

        var resolved = await resolver.ResolveManyAsync(scopes, AndroidSet, ct);

        var items = resolved
            .SelectMany(s => s.Values.Select(v => new
            {
                sourceDatabase = companyDatabase[s.Scope.ErpCompanyId],
                ParametreProgram = AndroidProgram,
                ParametreUser = username[s.Scope.MobileUserId!.Value],
                v.Entry.AnaGrubu,
                v.Entry.AltGrubu,
                ParametreID = v.Entry.ParametreId,
                ParametreAdi = v.Entry.Name,
                ParametreDegeri = v.Value,
                UpdatedAtUtc = v.OverriddenAtUtc ?? v.Entry.UpdatedAtUtc,
            }))
            .OrderBy(i => i.sourceDatabase, StringComparer.Ordinal)
            .ThenBy(i => i.ParametreUser, StringComparer.Ordinal)
            .ThenBy(i => i.ParametreID)
            .ToList();

        return JsonResults.Ok(new
        {
            tenantId,
            // Informational: compare it for equality, not for order. A user being removed can
            // lower it, which is exactly the case the ETag covers and a counter cannot.
            revision = revisions.Values.Sum(),
            count = items.Count,
            items,
        });
    }

    /// <summary>
    /// Identifies the answer by what it is built from: which scopes are covered, where each one
    /// stands, and which catalogue produced the defaults.
    /// </summary>
    private static string ETag(
        IReadOnlyCollection<ParameterScope> scopes,
        IReadOnlyDictionary<ParameterScope, long> revisions,
        DateTimeOffset? catalogStamp)
    {
        var material = new StringBuilder()
            .Append(catalogStamp?.UtcTicks.ToString(CultureInfo.InvariantCulture) ?? "-")
            .Append('\n');

        foreach (var line in scopes
                     .Select(s => $"{s.ErpCompanyId:N}:{s.MobileUserId:N}:{revisions.GetValueOrDefault(s)}")
                     .OrderBy(l => l, StringComparer.Ordinal))
        {
            material.Append(line).Append('\n');
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material.ToString()));
        return $"\"{Convert.ToHexString(hash.AsSpan(0, 16)).ToLowerInvariant()}\"";
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
