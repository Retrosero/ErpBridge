using System.IdentityModel.Tokens.Jwt;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Parameters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Parametre Yönetimi (P1e) — what the panel reads and writes.
///
/// Every call names an ERP company, because a tenant can own several and each is a different
/// Mikro database with its own warehouses and document series. A parameter is always identified
/// by its catalogue entry, never by name: 862 of the 3,365 distinct names appear in more than one
/// set, so a name does not say which parameter is meant.
/// </summary>
public static class AdminParameterEndpoints
{
    public static IEndpointRouteBuilder MapAdminParameterEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/parameters")
            .WithTags("Admin/Parameters")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/sets", ListSetsAsync).Produces<ParameterSetDto[]>();
        group.MapGet("/values", ListValuesAsync)
            .Produces<ParameterValuesResponse>()
            .Produces<ApiError>(StatusCodes.Status400BadRequest);
        group.MapPut("/values", WriteAsync)
            .Produces<ParameterWriteResponse>()
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ParameterConfirmationRequired>(StatusCodes.Status409Conflict);
        group.MapPost("/values/reset", ResetAsync)
            .Produces<ParameterWriteResponse>()
            .Produces<ApiError>(StatusCodes.Status400BadRequest);
        group.MapPost("/values/copy", CopyAsync)
            .Produces<ParameterCopyResponse>()
            .Produces<ApiError>(StatusCodes.Status400BadRequest);
        group.MapGet("/audit", ListAuditAsync).Produces<ParameterAuditDto[]>();

        return routes;
    }

    /// <summary>The catalogue sets a panel can offer, with how each one is addressed.</summary>
    private static async Task<IResult> ListSetsAsync(CentralApiDbContext db, CancellationToken ct)
    {
        // Grouped in memory: there are a couple of dozen sets, and grouping by four columns is
        // not translated by every provider the tests run against.
        var rows = await db.ParameterCatalog.AsNoTracking()
            .Select(e => new { e.Program, e.CatalogMethod, e.ScopeKind, e.ScopeFields, HasEditor = e.Editor != null })
            .ToListAsync(ct);

        var sets = rows
            .GroupBy(e => new { e.Program, e.CatalogMethod, e.ScopeKind, e.ScopeFields })
            .Select(g => new ParameterSetDto(
                g.Key.Program,
                g.Key.CatalogMethod,
                g.Key.ScopeKind,
                g.Key.ScopeFields,
                g.Count(),
                g.Count(e => e.HasEditor)))
            .OrderBy(s => s.Program, StringComparer.Ordinal)
            .ThenBy(s => s.CatalogMethod, StringComparer.Ordinal)
            .ToArray();

        return JsonResults.Ok(sets);
    }

    /// <summary>
    /// One set's parameters as they apply to a scope: the value in force, its default, and
    /// whether someone has moved it.
    /// </summary>
    private static async Task<IResult> ListValuesAsync(
        [FromQuery] Guid tenantId,
        [FromQuery] Guid erpCompanyId,
        [FromQuery] string? catalogMethod,
        [FromQuery] Guid? mobileUserId,
        [FromQuery] string? scope1,
        [FromQuery] string? scope2,
        [FromQuery] bool? onlyOverridden,
        CentralApiDbContext db,
        ParameterResolver resolver,
        CancellationToken ct)
    {
        if (tenantId == Guid.Empty || erpCompanyId == Guid.Empty || string.IsNullOrWhiteSpace(catalogMethod))
        {
            return Invalid("tenantId, erpCompanyId and catalogMethod are required.");
        }

        var scope = new ParameterScope(tenantId, erpCompanyId, mobileUserId, scope1 ?? "", scope2 ?? "");
        var values = await resolver.ResolveAsync(scope, catalogMethod, ct);

        if (onlyOverridden == true)
        {
            values = values.Where(v => v.IsOverridden).ToList();
        }

        var lastChange = await LastChangeAsync(db, scope, values, ct);

        return JsonResults.Ok(new ParameterValuesResponse(
            catalogMethod,
            await resolver.RevisionAsync(scope, ct),
            values.Count,
            values.Select(v => ToDto(v, lastChange.GetValueOrDefault(v.Entry.Id))).ToArray()));
    }

    private static async Task<IResult> WriteAsync(
        [FromBody] ParameterWriteRequest body,
        HttpContext http,
        CentralApiDbContext db,
        ParameterResolver resolver,
        CancellationToken ct)
    {
        if (body?.Changes is not { Count: > 0 })
        {
            return Invalid("At least one change is required.");
        }

        if (body.TenantId == Guid.Empty || body.ErpCompanyId == Guid.Empty)
        {
            return Invalid("tenantId and erpCompanyId are required.");
        }

        var scope = new ParameterScope(
            body.TenantId, body.ErpCompanyId, body.MobileUserId, body.Scope1 ?? "", body.Scope2 ?? "");

        // A VAT rate decides what an invoice totals, so changing one takes a deliberate second
        // step (D17). Checked here and not only in the panel: a guard in the screen is bypassed by
        // anything that calls the API directly.
        if (!body.ConfirmSensitive)
        {
            var ids = body.Changes.Select(c => c.CatalogEntryId).ToList();

            var sensitive = (await db.ParameterCatalog.AsNoTracking()
                    .Where(e => ids.Contains(e.Id))
                    .ToListAsync(ct))
                .Where(ParameterSensitivity.ChangesAmounts)
                .Select(e => e.Name)
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToArray();

            if (sensitive.Length > 0)
            {
                return JsonResults.Status(StatusCodes.Status409Conflict, new ParameterConfirmationRequired(
                    "PARAMETER_CONFIRMATION_REQUIRED",
                    "Bu değişiklik vergi oranlarını etkiliyor; onaylanması gerekiyor.",
                    sensitive));
            }
        }

        var by = PanelContext(http);
        var results = new List<ParameterWriteResultDto>();

        foreach (var change in body.Changes)
        {
            try
            {
                var outcome = await resolver.SetAsync(scope, change.CatalogEntryId, change.Value ?? "", by, ct);
                results.Add(new ParameterWriteResultDto(change.CatalogEntryId, outcome.ToString()));
            }
            catch (ParameterResolver.ScopeMismatchException ex)
            {
                // The scope cannot address this parameter. Saying so beats writing a row no read
                // would ever find and the mirror could not place in Mikro.
                return JsonResults.Status(StatusCodes.Status400BadRequest,
                    new ApiError { ErrorCode = "PARAMETER_SCOPE_MISMATCH", Message = ex.Message });
            }
        }

        return JsonResults.Ok(new ParameterWriteResponse(
            await resolver.RevisionAsync(scope, ct), results.ToArray()));
    }

    /// <summary>
    /// Copies one scope's settings onto another inside the same company — a new plasiyer set up
    /// like an existing one.
    ///
    /// Replaces by default: afterwards the target holds exactly what the source holds for that
    /// set, so a parameter the source leaves at its default is put back on the target too.
    /// "Copy Ali's settings to Veli" that quietly left some of Veli's old deviations in place
    /// would produce a third configuration nobody chose. <c>merge</c> is there for the operator
    /// who means "also give Veli these", and says so.
    /// </summary>
    private static async Task<IResult> CopyAsync(
        [FromBody] ParameterCopyRequest body,
        HttpContext http,
        CentralApiDbContext db,
        ParameterResolver resolver,
        CancellationToken ct)
    {
        if (body is null || body.TenantId == Guid.Empty || body.ErpCompanyId == Guid.Empty
            || string.IsNullOrWhiteSpace(body.CatalogMethod))
        {
            return Invalid("tenantId, erpCompanyId and catalogMethod are required.");
        }

        var from = new ParameterScope(
            body.TenantId, body.ErpCompanyId, body.FromMobileUserId, body.FromScope1 ?? "", body.FromScope2 ?? "");
        var to = new ParameterScope(
            body.TenantId, body.ErpCompanyId, body.ToMobileUserId, body.ToScope1 ?? "", body.ToScope2 ?? "");

        if (from == to)
        {
            return Invalid("The source and the target are the same scope.");
        }

        var entryIds = await db.ParameterCatalog.AsNoTracking()
            .Where(e => e.CatalogMethod == body.CatalogMethod)
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (entryIds.Count == 0)
        {
            return Invalid($"No catalogue set named '{body.CatalogMethod}'.");
        }

        var source = (await resolver.OverridesAsync(from, ct))
            .Where(v => entryIds.Contains(v.ParameterCatalogEntryId))
            .ToDictionary(v => v.ParameterCatalogEntryId, v => v.Value);

        var by = PanelContext(http) with { Source = ParameterChangeSources.Copy };
        var written = 0;
        var cleared = 0;

        try
        {
            foreach (var (entryId, value) in source)
            {
                if (await resolver.SetAsync(to, entryId, value, by, ct) != ParameterResolver.WriteOutcome.Unchanged)
                {
                    written++;
                }
            }

            if (!body.Merge)
            {
                var stale = (await resolver.OverridesAsync(to, ct))
                    .Where(v => entryIds.Contains(v.ParameterCatalogEntryId)
                                && !source.ContainsKey(v.ParameterCatalogEntryId))
                    .Select(v => v.ParameterCatalogEntryId)
                    .ToList();

                foreach (var entryId in stale)
                {
                    await resolver.ResetAsync(to, entryId, by, ct);
                    cleared++;
                }
            }
        }
        catch (ParameterResolver.ScopeMismatchException ex)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "PARAMETER_SCOPE_MISMATCH", Message = ex.Message });
        }

        return JsonResults.Ok(new ParameterCopyResponse(
            source.Count, written, cleared, await resolver.RevisionAsync(to, ct)));
    }

    /// <summary>Puts parameters back to their catalogue defaults by removing the stored rows.</summary>
    private static async Task<IResult> ResetAsync(
        [FromBody] ParameterResetRequest body,
        HttpContext http,
        ParameterResolver resolver,
        CancellationToken ct)
    {
        if (body?.CatalogEntryIds is not { Count: > 0 })
        {
            return Invalid("At least one catalogEntryId is required.");
        }

        if (body.TenantId == Guid.Empty || body.ErpCompanyId == Guid.Empty)
        {
            return Invalid("tenantId and erpCompanyId are required.");
        }

        var scope = new ParameterScope(
            body.TenantId, body.ErpCompanyId, body.MobileUserId, body.Scope1 ?? "", body.Scope2 ?? "");

        var by = PanelContext(http) with { Source = ParameterChangeSources.Reset };
        var results = new List<ParameterWriteResultDto>();

        foreach (var id in body.CatalogEntryIds)
        {
            try
            {
                var outcome = await resolver.ResetAsync(scope, id, by, ct);
                results.Add(new ParameterWriteResultDto(id, outcome.ToString()));
            }
            catch (ParameterResolver.ScopeMismatchException ex)
            {
                return JsonResults.Status(StatusCodes.Status400BadRequest,
                    new ApiError { ErrorCode = "PARAMETER_SCOPE_MISMATCH", Message = ex.Message });
            }
        }

        return JsonResults.Ok(new ParameterWriteResponse(
            await resolver.RevisionAsync(scope, ct), results.ToArray()));
    }

    /// <summary>Who changed what, newest first. Credential values are masked at write time.</summary>
    /// <summary>
    /// The change trail, newest first, narrowed to whatever the caller names: a company, a scope,
    /// one catalogue set, or one parameter.
    /// </summary>
    private static async Task<IResult> ListAuditAsync(
        [FromQuery] Guid tenantId,
        [FromQuery] Guid? erpCompanyId,
        [FromQuery] Guid? catalogEntryId,
        [FromQuery] string? catalogMethod,
        [FromQuery] Guid? mobileUserId,
        [FromQuery] string? scope1,
        [FromQuery] string? scope2,
        [FromQuery] bool? scoped,
        [FromQuery] int? limit,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        if (tenantId == Guid.Empty)
        {
            return Invalid("tenantId is required.");
        }

        var query = db.ParameterAudit.AsNoTracking()
            .Include(e => e.CatalogEntry)
            .Where(e => e.TenantId == tenantId);

        if (erpCompanyId is { } company && company != Guid.Empty)
        {
            query = query.Where(e => e.ErpCompanyId == company);
        }

        if (catalogEntryId is { } entry && entry != Guid.Empty)
        {
            query = query.Where(e => e.ParameterCatalogEntryId == entry);
        }

        if (!string.IsNullOrWhiteSpace(catalogMethod))
        {
            query = query.Where(e => e.CatalogEntry!.CatalogMethod == catalogMethod);
        }

        // Narrowing to one scope needs every dimension, including the empty ones: a user's history
        // and a company-wide setting's history are different questions, and "user is null" is part
        // of the second one's answer.
        if (scoped == true)
        {
            var s1 = scope1 ?? "";
            var s2 = scope2 ?? "";

            query = query.Where(e => e.MobileUserId == mobileUserId && e.Scope1 == s1 && e.Scope2 == s2);
        }
        else if (mobileUserId is { } user && user != Guid.Empty)
        {
            query = query.Where(e => e.MobileUserId == user);
        }

        // Ordered by when it happened, not by the row's id: the id is a GUID, so ordering by it
        // would hand back "the latest changes" in an order nobody can explain.
        var rows = await query
            .OrderByDescending(e => e.AtUtc)
            .Take(Math.Clamp(limit ?? 200, 1, 1000))
            .ToListAsync(ct);

        return JsonResults.Ok(rows.Select(e => new ParameterAuditDto(
            e.Id,
            e.ErpCompanyId,
            e.ParameterCatalogEntryId,
            e.CatalogEntry?.Program ?? string.Empty,
            e.CatalogEntry?.Name ?? string.Empty,
            e.MobileUserId,
            e.Scope1,
            e.Scope2,
            e.Outcome,
            e.OldValue,
            e.NewValue,
            e.IsMasked,
            e.Source,
            e.AdminUserId,
            e.Actor,
            e.AtUtc)).ToArray());
    }

    /// <summary>Attributes a change to the signed-in admin so the trail is not anonymous.</summary>
    private static ParameterResolver.ChangeContext PanelContext(HttpContext http)
    {
        var sub = http.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var email = http.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        return new ParameterResolver.ChangeContext(
            ParameterChangeSources.Panel,
            Guid.TryParse(sub, out var adminId) ? adminId : null,
            email ?? string.Empty);
    }

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest,
            new ApiError { ErrorCode = "INVALID_PARAMETER_REQUEST", Message = message });

    private static ParameterValueDto ToDto(
        ParameterResolver.Effective effective, LastChange? lastChange) => new(
        effective.Entry.Id,
        effective.Entry.ParametreId,
        effective.Entry.Name,
        effective.Entry.Label,
        effective.Entry.Editor,
        effective.Entry.ReferenceKind,
        effective.Entry.OptionsJson,
        effective.Entry.TabPath,
        effective.Value,
        effective.Entry.DefaultValue,
        effective.IsOverridden,
        effective.Entry.IsImplemented,
        effective.Entry.IsDeprecated,
        effective.OverriddenAtUtc,
        lastChange?.Actor,
        lastChange?.Source,
        lastChange?.AtUtc,
        ParameterSensitivity.IsTaxTable(effective.Entry),
        ParameterSensitivity.ChangesAmounts(effective.Entry));

    /// <summary>Who last moved a parameter in this scope, and when.</summary>
    private sealed record LastChange(string Actor, string Source, DateTimeOffset AtUtc);

    /// <summary>
    /// The latest trail entry per parameter, for the ones that are actually off their default.
    ///
    /// Only those: a set runs to 1,801 parameters and the overrides are a handful, so asking about
    /// all of them would read a trail that mostly says nothing. A parameter that is at its default
    /// has nobody to attribute it to.
    /// </summary>
    private static async Task<Dictionary<Guid, LastChange>> LastChangeAsync(
        CentralApiDbContext db,
        ParameterScope scope,
        IReadOnlyList<ParameterResolver.Effective> values,
        CancellationToken ct)
    {
        var ids = values.Where(v => v.IsOverridden).Select(v => v.Entry.Id).ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        var rows = await db.ParameterAudit.AsNoTracking()
            .Where(a => a.TenantId == scope.TenantId
                        && a.ErpCompanyId == scope.ErpCompanyId
                        && a.MobileUserId == scope.MobileUserId
                        && a.Scope1 == scope.Scope1
                        && a.Scope2 == scope.Scope2
                        && ids.Contains(a.ParameterCatalogEntryId))
            .Select(a => new { a.ParameterCatalogEntryId, a.Actor, a.Source, a.AtUtc })
            .ToListAsync(ct);

        // Ordered in memory: SQLite cannot ORDER BY a DateTimeOffset.
        return rows
            .GroupBy(a => a.ParameterCatalogEntryId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(a => a.AtUtc)
                    .Select(a => new LastChange(a.Actor, a.Source, a.AtUtc))
                    .First());
    }

    /// <param name="ParameterCount">How many parameters the set declares.</param>
    /// <param name="WithEditor">How many of them a Fora editor exposes, i.e. have layout metadata.</param>
    public sealed record ParameterSetDto(
        string Program,
        string CatalogMethod,
        string ScopeKind,
        string ScopeFields,
        int ParameterCount,
        int WithEditor);

    /// <param name="Revision">The scope counter, so a caller can tell whether its copy is current.</param>
    public sealed record ParameterValuesResponse(
        string CatalogMethod,
        long Revision,
        int Count,
        ParameterValueDto[] Items);

    /// <param name="IsOverridden">True when a stored row moves this away from its default.</param>
    /// <param name="IsImplemented">False means the mobile app ignores it in this release (D16).</param>
    public sealed record ParameterValueDto(
        Guid CatalogEntryId,
        int ParametreId,
        string Name,
        string? Label,
        string? Editor,
        string? ReferenceKind,
        string? OptionsJson,
        string? TabPath,
        string Value,
        string DefaultValue,
        bool IsOverridden,
        bool IsImplemented,
        bool IsDeprecated,
        DateTimeOffset? OverriddenAtUtc,
        string? LastChangedBy,
        string? LastChangeSource,
        DateTimeOffset? LastChangedAtUtc,
        bool IsTaxTable,
        bool ChangesAmounts);

    /// <param name="ConfirmSensitive">
    /// Set once the caller has been told which VAT rates the batch touches and has said to go
    /// ahead. Without it a batch containing one is refused with 409 (D17).
    /// </param>
    public sealed record ParameterWriteRequest(
        Guid TenantId,
        Guid ErpCompanyId,
        Guid? MobileUserId,
        string? Scope1,
        string? Scope2,
        IReadOnlyList<ParameterChangeDto> Changes,
        bool ConfirmSensitive = false);

    /// <param name="Sensitive">The VAT parameters the batch would change, by name.</param>
    public sealed record ParameterConfirmationRequired(
        string ErrorCode,
        string Message,
        IReadOnlyList<string> Sensitive);

    public sealed record ParameterChangeDto(Guid CatalogEntryId, string? Value);

    public sealed record ParameterResetRequest(
        Guid TenantId,
        Guid ErpCompanyId,
        Guid? MobileUserId,
        string? Scope1,
        string? Scope2,
        IReadOnlyList<Guid> CatalogEntryIds);

    /// <param name="Merge">
    /// False (the default) leaves the target holding exactly what the source holds. True adds the
    /// source's settings without removing the target's own.
    /// </param>
    public sealed record ParameterCopyRequest(
        Guid TenantId,
        Guid ErpCompanyId,
        string CatalogMethod,
        Guid? FromMobileUserId,
        string? FromScope1,
        string? FromScope2,
        Guid? ToMobileUserId,
        string? ToScope1,
        string? ToScope2,
        bool Merge);

    /// <param name="Copied">Settings the source had.</param>
    /// <param name="Written">Of those, the ones that actually moved the target.</param>
    /// <param name="Cleared">Target settings removed because the source did not have them.</param>
    public sealed record ParameterCopyResponse(int Copied, int Written, int Cleared, long Revision);

    /// <param name="Outcome"><c>Unchanged</c>, <c>Inserted</c>, <c>Updated</c> or <c>Deleted</c>.</param>
    public sealed record ParameterWriteResultDto(Guid CatalogEntryId, string Outcome);

    public sealed record ParameterWriteResponse(long Revision, ParameterWriteResultDto[] Results);

    public sealed record ParameterAuditDto(
        Guid Id,
        Guid ErpCompanyId,
        Guid CatalogEntryId,
        string Program,
        string Name,
        Guid? MobileUserId,
        string Scope1,
        string Scope2,
        string Outcome,
        string? OldValue,
        string? NewValue,
        bool IsMasked,
        string Source,
        Guid? AdminUserId,
        string Actor,
        DateTimeOffset AtUtc);
}
