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
/// The agent's side of the parameter system (P1g): pull what one company's Mikro parameter table
/// should contain, and report back what happened when it was written.
///
/// Always per company. An agent can be assigned to several, each a different Mikro database with
/// its own warehouses and document series, so a call that did not name one would write a
/// company's settings into another's database.
/// </summary>
public static class AgentParameterEndpoints
{
    /// <summary>The only catalogue set the mirror carries today: the phone's 1,801 parameters.</summary>
    private const string MirroredSet = "MobilKullanici";

    private const string MirroredProgram = "akilli";

    public static IEndpointRouteBuilder MapAgentParameterEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/agents/parameters")
            .WithTags("Agents/Parameters")
            .RequireAuthorization(Program.AgentPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        group.MapGet("/companies", CompaniesAsync)
            .WithName("AgentParameterCompanies")
            .Produces<AgentCompanyDto[]>();

        group.MapGet("", DesiredStateAsync)
            .WithName("AgentParametersDesiredState")
            .Produces<AgentParameterStateResponse>()
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);

        group.MapPost("/fora-import", ForaImportAsync)
            .WithName("AgentParametersForaImport")
            .Produces<ForaImportResponse>()
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);

        group.MapPost("/report", ReportAsync)
            .WithName("AgentParametersReport")
            .Produces<AgentParameterReportResponse>()
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);

        return routes;
    }

    /// <summary>
    /// The companies this agent is assigned to.
    ///
    /// The agent knows the Mikro database it is pointed at, not the id the centre uses for it, so
    /// it needs this to turn one into the other. Returning the whole assignment rather than the
    /// one match also means an agent that later serves several databases needs configuration, not
    /// a different endpoint.
    /// </summary>
    private static async Task<IResult> CompaniesAsync(
        [FromServices] CentralApiDbContext db, HttpContext http, CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Tenant id missing." });
        }

        if (!http.User.TryGetAgentId(out var agentId))
        {
            return JsonResults.Ok(Array.Empty<AgentCompanyDto>());
        }

        var rows = await db.AgentCompanyAssignments.AsNoTracking()
            .Where(a => a.AgentId == agentId)
            .Join(db.ErpCompanies.AsNoTracking().Where(c => c.TenantId == tenantId && c.IsActive),
                a => a.ErpCompanyId, c => c.Id,
                (a, c) => new AgentCompanyDto(c.Id, c.Code, c.Name, c.SourceDatabase, c.CompanyNo, c.BranchNo))
            .OrderBy(c => c.SourceDatabase)
            .ToArrayAsync(ct);

        return JsonResults.Ok(rows);
    }

    /// <summary>
    /// Everything <c>_ERPB_PARAMETRELER</c> should hold for one company — the complete desired
    /// state, not a delta.
    ///
    /// Complete on purpose: the agent makes the table match, so a row the centre no longer lists
    /// is a row the agent deletes. A delta would need both sides to agree on what was sent last,
    /// and a single missed run would leave a stale row overriding a default nobody chose.
    ///
    /// Only deviations are listed, the way Fora stores them (D3): a parameter at its default has
    /// no row, because that is what the default means. Only active users are published (D5b) — a
    /// username is reusable and a deleted user's permissions must not reach whoever holds the name
    /// next.
    /// </summary>
    private static async Task<IResult> DesiredStateAsync(
        [FromQuery] Guid erpCompanyId,
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

        if (erpCompanyId == Guid.Empty)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError
            {
                ErrorCode = "ERP_COMPANY_REQUIRED",
                Message = "erpCompanyId is required: an agent can serve several companies.",
            });
        }

        var company = await ResolveCompanyAsync(db, tenantId, erpCompanyId, http, ct);
        if (company is null)
        {
            return NotAssigned();
        }

        var users = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.IsActive && u.DeletedAtUtc == null)
            .Select(u => new { u.Id, u.Username })
            .ToDictionaryAsync(u => u.Id, u => u.Username, ct);

        var scopes = users.Keys
            .Select(id => ParameterScope.ForMobileUser(tenantId, erpCompanyId, id))
            .ToList();

        var revisions = await resolver.RevisionsAsync(scopes, ct);

        // Deviations only (D3): the mirror is a copy of what Fora would have stored, so a
        // parameter left at its default must have no row on either side.
        var overrides = await db.ParameterValues.AsNoTracking()
            .Where(v => v.TenantId == tenantId && v.ErpCompanyId == erpCompanyId && v.MobileUserId != null)
            .Join(db.ParameterCatalog.AsNoTracking().Where(e => e.CatalogMethod == MirroredSet),
                v => v.ParameterCatalogEntryId, e => e.Id, (v, e) => new { Value = v, Entry = e })
            .ToListAsync(ct);

        var rows = overrides
            .Where(x => users.ContainsKey(x.Value.MobileUserId!.Value))
            .Select(x => new AgentParameterRow(
                x.Value.ParameterCatalogEntryId,
                MirroredProgram,
                users[x.Value.MobileUserId!.Value],
                x.Value.MobileUserId!.Value,
                x.Entry.AnaGrubu,
                x.Entry.AltGrubu,
                x.Entry.ParametreId,
                x.Entry.Name,
                x.Value.Value,
                x.Value.UpdatedAtUtc))
            .OrderBy(r => r.ParametreUser, StringComparer.Ordinal)
            .ThenBy(r => r.ParametreID)
            .ToArray();

        return JsonResults.Ok(new AgentParameterStateResponse(
            tenantId, erpCompanyId, company.SourceDatabase, revisions.Values.Sum(), rows.Length, rows));
    }

    /// <summary>
    /// Records what a mirror run did, including rows it found already changed by hand.
    ///
    /// Drift is recorded, never written back: the mirror is one-way and the centre is the single
    /// source of truth (K2, D8). The panel shows it so a customer who edits a setting in Fora
    /// finds out before the next run overwrites it.
    /// </summary>
    private static async Task<IResult> ReportAsync(
        [FromBody] AgentParameterReportRequest body,
        [FromServices] CentralApiDbContext db,
        HttpContext http,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Tenant id missing." });
        }

        if (body is null || body.ErpCompanyId == Guid.Empty)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError
            {
                ErrorCode = "ERP_COMPANY_REQUIRED",
                Message = "erpCompanyId is required: an agent can serve several companies.",
            });
        }

        var company = await ResolveCompanyAsync(db, tenantId, body.ErpCompanyId, http, ct);
        if (company is null)
        {
            return NotAssigned();
        }

        http.User.TryGetAgentId(out var agentId);

        var report = new ParameterMirrorReport
        {
            TenantId = tenantId,
            ErpCompanyId = body.ErpCompanyId,
            AgentId = agentId == Guid.Empty ? null : agentId,
            AppliedRevision = body.AppliedRevision,
            Inserted = body.Inserted,
            Updated = body.Updated,
            Deleted = body.Deleted,
            Failed = body.Failed,
            ErrorText = Trim(body.ErrorText, 2000),
        };

        // A drift row naming a parameter this catalogue does not have would be unreadable later,
        // so it is dropped rather than stored as a dangling reference.
        var known = body.Drifts is { Count: > 0 }
            ? await db.ParameterCatalog.AsNoTracking()
                .Where(e => body.Drifts.Select(d => d.CatalogEntryId).Contains(e.Id))
                .Select(e => e.Id)
                .ToListAsync(ct)
            : [];

        foreach (var drift in body.Drifts ?? [])
        {
            if (!known.Contains(drift.CatalogEntryId))
            {
                continue;
            }

            report.Drifts.Add(new ParameterMirrorDrift
            {
                ParameterCatalogEntryId = drift.CatalogEntryId,
                MobileUserId = drift.MobileUserId,
                Scope1 = drift.Scope1 ?? "",
                Scope2 = drift.Scope2 ?? "",
                ExpectedValue = drift.ExpectedValue ?? "",
                FoundValue = drift.FoundValue,
            });
        }

        report.Drifted = report.Drifts.Count;

        db.ParameterMirrorReports.Add(report);
        await db.SaveChangesAsync(ct);

        return JsonResults.Ok(new AgentParameterReportResponse(report.Id, report.Drifted));
    }

    /// <summary>
    /// Takes a read-only scan of a customer's existing Fora settings and stores it as a proposal
    /// (P3c).
    ///
    /// A proposal, never a change. These are the settings the customer has been running, read out
    /// of a table we do not own; applying them here would move settings nobody at our end chose.
    /// A person reviews the batch and applies it (P3d).
    /// </summary>
    private static async Task<IResult> ForaImportAsync(
        [FromBody] ForaImportRequest body,
        [FromServices] CentralApiDbContext db,
        HttpContext http,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Tenant id missing." });
        }

        if (body is null || body.ErpCompanyId == Guid.Empty)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError
            {
                ErrorCode = "ERP_COMPANY_REQUIRED",
                Message = "erpCompanyId is required: a tenant can have several Fora installations.",
            });
        }

        if (await ResolveCompanyAsync(db, tenantId, body.ErpCompanyId, http, ct) is null)
        {
            return NotAssigned();
        }

        http.User.TryGetAgentId(out var agentId);

        var rows = body.Rows ?? [];

        // The catalogue is what turns a stored row into a parameter: a (set, id) pair means
        // nothing without it, and the mobile user's Sifre is not in it at all (D6), so a password
        // Fora encrypted with its own key is dropped here rather than stored anywhere of ours.
        var catalog = await db.ParameterCatalog.AsNoTracking()
            .Where(e => e.Program == MirroredProgram)
            .Select(e => new { e.Id, e.CatalogMethod, e.ParametreId, e.DefaultValue })
            .ToListAsync(ct);

        var byId = catalog
            .Where(e => e.CatalogMethod == MirroredSet)
            .ToDictionary(e => e.ParametreId, e => e);

        // Only active users. A username is a reusable label, so matching a departed plasiyer's
        // rows onto whoever holds the name now would hand over their permissions (D5, D5b).
        var users = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.IsActive && u.DeletedAtUtc == null)
            .Select(u => new { u.Id, u.Username })
            .ToListAsync(ct);

        var byUsername = users
            .GroupBy(u => u.Username, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        var batch = new ForaImportBatch
        {
            TenantId = tenantId,
            ErpCompanyId = body.ErpCompanyId,
            AgentId = agentId == Guid.Empty ? null : agentId,
            ScannedRows = rows.Count,
        };

        foreach (var row in rows)
        {
            var entry = byId.GetValueOrDefault(row.ParametreID);
            var userId = byUsername.GetValueOrDefault(row.ParametreUser ?? "");

            batch.Rows.Add(new ForaImportRow
            {
                ParametreProgram = Trim(row.ParametreProgram, 40) ?? "",
                ParametreUser = Trim(row.ParametreUser, 40) ?? "",
                AnaGrubu = Trim(row.AnaGrubu, 100) ?? "",
                AltGrubu = Trim(row.AltGrubu, 100) ?? "",
                ParametreId = row.ParametreID,
                ParametreAdi = Trim(row.ParametreAdi, 100) ?? "",
                ParametreDegeri = row.ParametreDegeri ?? "",
                ParameterCatalogEntryId = entry?.Id,
                MobileUserId = userId == Guid.Empty ? null : userId,

                // Fora stores only deviations, but a customer's table can still hold a row equal
                // to the default — it happens when a default changed between Fora versions. The
                // reviewer should see that applying it would store nothing.
                IsDefaultValue = entry is not null
                    && string.Equals(entry.DefaultValue, row.ParametreDegeri ?? "", StringComparison.Ordinal),
            });
        }

        batch.MatchedRows = batch.Rows.Count(r => r.ParameterCatalogEntryId is not null && r.MobileUserId is not null);

        db.ForaImportBatches.Add(batch);
        await db.SaveChangesAsync(ct);

        var unmatchedUsers = batch.Rows
            .Where(r => r.MobileUserId is null)
            .Select(r => r.ParametreUser)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(u => u, StringComparer.Ordinal)
            .ToArray();

        return JsonResults.Ok(new ForaImportResponse(
            batch.Id,
            batch.ScannedRows,
            batch.MatchedRows,
            batch.Rows.Count(r => r.ParameterCatalogEntryId is null),
            unmatchedUsers));
    }

    /// <summary>
    /// The company, when this tenant owns it and this agent is assigned to it; null otherwise.
    /// </summary>
    private static async Task<ErpCompany?> ResolveCompanyAsync(
        CentralApiDbContext db, Guid tenantId, Guid erpCompanyId, HttpContext http, CancellationToken ct)
    {
        var company = await db.ErpCompanies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == erpCompanyId && c.TenantId == tenantId, ct);

        if (company is null)
        {
            return null;
        }

        if (!http.User.TryGetAgentId(out var agentId))
        {
            return null;
        }

        var assigned = await db.AgentCompanyAssignments.AsNoTracking()
            .AnyAsync(a => a.AgentId == agentId && a.ErpCompanyId == erpCompanyId, ct);

        return assigned ? company : null;
    }

    /// <summary>
    /// Deliberately the same answer for "not yours", "does not exist" and "not assigned": an agent
    /// has no business learning which companies other tenants own.
    /// </summary>
    private static IResult NotAssigned() =>
        JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
        {
            ErrorCode = "ERP_COMPANY_NOT_ASSIGNED",
            Message = "This agent is not assigned to that ERP company.",
        });

    private static string? Trim(string? value, int max) =>
        string.IsNullOrEmpty(value) ? value : value.Length <= max ? value : value[..max];

    /// <summary>One company the agent serves, named the way the agent can recognise it.</summary>
    public sealed record AgentCompanyDto(
        Guid Id,
        string Code,
        string Name,
        string SourceDatabase,
        int CompanyNo,
        int BranchNo);

    /// <summary>One row of the desired state, in the shape Mikro's parameter table stores.</summary>
    public sealed record AgentParameterRow(
        Guid CatalogEntryId,
        string ParametreProgram,
        string ParametreUser,
        Guid MobileUserId,
        string AnaGrubu,
        string AltGrubu,
        int ParametreID,
        string ParametreAdi,
        string ParametreDegeri,
        DateTimeOffset UpdatedAtUtc);

    public sealed record AgentParameterStateResponse(
        Guid TenantId,
        Guid ErpCompanyId,
        string SourceDatabase,
        long Revision,
        int Count,
        IReadOnlyList<AgentParameterRow> Items);

    public sealed record AgentParameterDriftDto(
        Guid CatalogEntryId,
        Guid? MobileUserId,
        string? Scope1,
        string? Scope2,
        string? ExpectedValue,
        string? FoundValue);

    public sealed record AgentParameterReportRequest(
        Guid ErpCompanyId,
        long AppliedRevision,
        int Inserted,
        int Updated,
        int Deleted,
        int Failed,
        string? ErrorText,
        IReadOnlyList<AgentParameterDriftDto>? Drifts);

    public sealed record AgentParameterReportResponse(Guid ReportId, int Drifted);

    /// <summary>One row exactly as Fora stored it.</summary>
    public sealed record ForaImportRowDto(
        string? ParametreProgram,
        string? ParametreUser,
        string? AnaGrubu,
        string? AltGrubu,
        int ParametreID,
        string? ParametreAdi,
        string? ParametreDegeri);

    public sealed record ForaImportRequest(Guid ErpCompanyId, IReadOnlyList<ForaImportRowDto>? Rows);

    /// <param name="Unknown">Rows whose parameter the catalogue does not declare (R1).</param>
    /// <param name="UnmatchedUsers">Usernames with no active mobile user; no user is opened for them.</param>
    public sealed record ForaImportResponse(
        Guid BatchId,
        int Scanned,
        int Matched,
        int Unknown,
        IReadOnlyList<string> UnmatchedUsers);
}
