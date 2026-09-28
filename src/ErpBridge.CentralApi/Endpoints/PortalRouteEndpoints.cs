using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Targets;
using ErpBridge.CentralApi.Team;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal/routes</c> (GOAL_HEDEF_RUT P4–P5, K14): route plans from the web panel and how well they were
/// followed. There is no second route engine — a save is the same <c>route_plan</c> document a phone sends, booked by
/// <see cref="TeamDocumentProcessor"/> (KB rule 17), so the phones receive it through the feed as before. Reading needs
/// the reports right; planning <see cref="RolePermissions.CanPlanRoutes"/> within the caller's <see cref="TeamScope"/>.
/// </summary>
public static class PortalRouteEndpoints
{
    public const int MaxRangeDays = 92;

    public static IEndpointRouteBuilder MapPortalRouteEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/routes")
            .WithTags("Portal/Routes")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("/", ListAsync).WithName("PortalRoutePlans");
        group.MapPut("/{planId}", SaveAsync).WithName("PortalRoutePlanSave");
        group.MapDelete("/{planId}", DeleteAsync).WithName("PortalRoutePlanDelete");
        group.MapGet("/compliance", ComplianceAsync).WithName("PortalRouteCompliance");
        return routes;
    }

    private static async Task<IResult> ListAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, scope, error) = await AuthorizeAsync(http, db, plan: false, ct);
        if (error is not null) return error;
        var people = await PeopleAsync(db, tenant!.Id, scope!, ct);
        var ids = people.ToDictionary(p => p.Username, p => p.Id, StringComparer.OrdinalIgnoreCase);
        var plans = new List<RoutePlanDto>();
        foreach (var plan in await PlansAsync(db, tenant.Id, ct))
        {
            var inScope = plan.Assignees.Where(ids.ContainsKey).ToList();
            var visible = scope!.WholeCompany || inScope.Count > 0
                          || (plan.Assignees.Length == 0 && string.Equals(plan.UpdatedBy, scope.User.Username, StringComparison.OrdinalIgnoreCase));
            if (!visible) continue;
            plan.CanEdit = RolePermissions.CanPlanRoutes(scope.User) && (scope.WholeCompany || inScope.Count == plan.Assignees.Length);
            plans.Add(plan);
        }
        return JsonResults.Ok(new RoutePlansResponse
        {
            Plans = [.. plans.OrderBy(p => p.IsActive ? 0 : 1).ThenBy(p => p.Name, StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), true))],
            People = [.. people.Select(p => new RoutePersonDto { Username = p.Username, FullName = p.Name, TeamName = p.TeamName })],
            CanPlan = RolePermissions.CanPlanRoutes(scope!.User),
            WholeCompany = scope.WholeCompany,
        });
    }

    private static async Task<IResult> SaveAsync(HttpContext http, string planId, [FromBody] RoutePlanDto? body, [FromServices] CentralApiDbContext db,
        [FromServices] TeamDocumentProcessor team, CancellationToken ct)
    {
        var (tenant, scope, error) = await AuthorizeAsync(http, db, plan: true, ct);
        if (error is not null) return error;
        if (!ValidId(planId)) return Fail(StatusCodes.Status400BadRequest, "ROUTE_INVALID", "planId en çok 64 harf, rakam, - ya da _ olabilir.");
        body ??= new RoutePlanDto();
        var payload = JsonSerializer.Serialize(new
        {
            planId,
            name = body.Name,
            description = body.Description,
            startDate = body.StartDate,
            isActive = body.IsActive,
            // A stop made in the panel gets its id here; an existing one keeps it, so visits recorded on it stay joined.
            stops = body.Stops.Select(s => new
            {
                stopId = string.IsNullOrWhiteSpace(s.StopId) ? Guid.NewGuid().ToString("N") : s.StopId,
                dayOfWeek = s.DayOfWeek,
                customerCode = s.CustomerCode,
                customerName = s.CustomerName,
                visitOrder = s.VisitOrder,
            }),
            assignees = body.Assignees,
        });
        var job = await BookAsync(db, team, tenant!, scope!.User, TeamDocumentProcessor.RoutePlan, planId, payload, http, ct);
        if (job.Status != JobStatus.Succeeded)
            return Fail(job.LastError?.Contains("not in your teams", StringComparison.Ordinal) == true ? StatusCodes.Status403Forbidden : StatusCodes.Status400BadRequest,
                "ROUTE_INVALID", Translate(job.LastError));
        var saved = (await PlansAsync(db, tenant!.Id, ct)).FirstOrDefault(p => p.PlanId == planId);
        if (saved is not null) saved.CanEdit = true;
        return JsonResults.Ok(saved!);
    }

    private static async Task<IResult> DeleteAsync(HttpContext http, string planId, [FromServices] CentralApiDbContext db,
        [FromServices] TeamDocumentProcessor team, CancellationToken ct)
    {
        var (tenant, scope, error) = await AuthorizeAsync(http, db, plan: true, ct);
        if (error is not null) return error;
        if (!ValidId(planId)) return Fail(StatusCodes.Status400BadRequest, "ROUTE_INVALID", "planId geçersiz.");
        var job = await BookAsync(db, team, tenant!, scope!.User, TeamDocumentProcessor.RoutePlanDelete, planId, JsonSerializer.Serialize(new { planId }), http, ct);
        return job.Status == JobStatus.Succeeded
            ? Results.NoContent()
            : Fail(StatusCodes.Status403Forbidden, "ROUTE_INVALID", Translate(job.LastError));
    }

    /// <summary>
    /// Planned stops against what was recorded, per person and per day over [from, to] (at most 92 days). A planned stop of a
    /// past day with nothing recorded is missed; today's are still open. Only the people the caller sees are counted.
    /// </summary>
    private static async Task<IResult> ComplianceAsync(HttpContext http, [FromServices] CentralApiDbContext db,
        string? from, string? to, Guid? teamId, CancellationToken ct)
    {
        var (tenant, scope, error) = await AuthorizeAsync(http, db, plan: false, ct);
        if (error is not null) return error;
        var today = TargetService.Today();
        if (!Day(from, today.AddDays(-((7 + (int)today.DayOfWeek - 1) % 7)), out var start) || !Day(to, today, out var end))
            return Fail(StatusCodes.Status400BadRequest, "INVALID_DATE", "from ve to yyyy-MM-dd olmalı.");
        if (end < start) return Fail(StatusCodes.Status400BadRequest, "INVALID_RANGE", "to, from'dan önce.");
        if (end.DayNumber - start.DayNumber + 1 > MaxRangeDays) return Fail(StatusCodes.Status400BadRequest, "RANGE_TOO_LONG", $"En çok {MaxRangeDays} gün.");

        var people = await PeopleAsync(db, tenant!.Id, scope!, ct);
        if (teamId is { } only && scope!.SeesTeam(only))
        {
            var members = scope.Directory.MembersOf(only);
            people = [.. people.Where(p => members.Contains(p.Id))];
        }
        var rows = people.ToDictionary(p => p.Username, p => new RouteComplianceRow { Username = p.Username, FullName = p.Name, TeamName = p.TeamName },
            StringComparer.OrdinalIgnoreCase);
        var records = await PortalReports.RouteRecordsAsync(db, tenant.Id, ct);
        var days = new List<RouteComplianceDay>();
        for (var day = start; day <= end; day = day.AddDays(1))
        {
            var total = new RouteComplianceDay { Date = TargetPeriod.Format(day) };
            foreach (var visit in PortalReports.BuildVisits(records, day))
            {
                if (!rows.TryGetValue(visit.Username, out var row)) continue;
                if (!visit.Planned)
                {
                    if (visit.Status == "COMPLETED") row.Unplanned++;
                    continue;
                }
                row.Planned++;
                total.Planned++;
                switch (visit.Status)
                {
                    case "COMPLETED": row.Completed++; total.Completed++; break;
                    case "SKIPPED": row.Skipped++; total.Skipped++; break;
                    default:
                        if (day < today) { row.Missed++; total.Missed++; }
                        break;
                }
            }
            days.Add(total);
        }
        foreach (var row in rows.Values)
            row.Compliance = row.Planned > 0 ? Math.Round(100m * row.Completed / row.Planned, 1) : null;

        return JsonResults.Ok(new RouteComplianceResponse
        {
            From = TargetPeriod.Format(start),
            To = TargetPeriod.Format(end),
            Rows = [.. rows.Values.Where(r => r.Planned + r.Unplanned > 0).OrderBy(r => r.Compliance ?? 101m).ThenBy(r => r.FullName, StringComparer.CurrentCultureIgnoreCase)],
            Days = [.. days],
        });
    }

    // ---- helpers ------------------------------------------------------------------------------

    private sealed record Person(Guid Id, string Username, string Name, string? TeamName);

    /// <summary>Active phone users the caller sees, with their team.</summary>
    private static async Task<List<Person>> PeopleAsync(CentralApiDbContext db, Guid tenantId, TeamScope scope, CancellationToken ct)
    {
        var users = await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
            .Where(u => u.TenantId == tenantId && u.DeletedAtUtc == null && u.IsActive)
            .ToListAsync(ct);
        return users
            .Where(u => RolePermissions.CanUsePhone(u) && !RolePermissions.IsWarehouseOnlyOnPhone(u) && scope.SeesUser(u.Id))
            .Select(u => new Person(u.Id, u.Username, string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName,
                scope.Directory.TeamOfUser.TryGetValue(u.Id, out var team) && scope.Directory.Teams.TryGetValue(team, out var t) ? t.Name : null))
            .OrderBy(p => p.Name, StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), true))
            .ToList();
    }

    private static async Task<List<RoutePlanDto>> PlansAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct)
    {
        var payloads = await db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.Entity == TeamDocumentProcessor.RoutePlansSection && !r.IsDeleted && r.PayloadJson != null)
            .Select(r => r.PayloadJson!)
            .ToListAsync(ct);
        var plans = new List<RoutePlanDto>(payloads.Count);
        foreach (var payload in payloads)
        {
            using var parsed = JsonDocument.Parse(payload);
            var root = parsed.RootElement;
            plans.Add(new RoutePlanDto
            {
                PlanId = AndroidEndpoints.GetString(root, "planId") ?? string.Empty,
                Name = AndroidEndpoints.GetString(root, "name") ?? string.Empty,
                Description = AndroidEndpoints.GetString(root, "description") ?? string.Empty,
                StartDate = AndroidEndpoints.GetString(root, "startDate") ?? string.Empty,
                IsActive = AndroidEndpoints.GetBoolean(root, "isActive") ?? true,
                UpdatedBy = AndroidEndpoints.GetString(root, "updatedBy"),
                UpdatedAtUtc = AndroidEndpoints.GetString(root, "updatedAtUtc"),
                Assignees = root.TryGetProperty("assignees", out var a) && a.ValueKind == JsonValueKind.Array
                    ? [.. a.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!)]
                    : [],
                Stops = root.TryGetProperty("stops", out var s) && s.ValueKind == JsonValueKind.Array
                    ? [.. s.EnumerateArray().Select(x => new RouteStopDto
                    {
                        StopId = AndroidEndpoints.GetString(x, "stopId") ?? string.Empty,
                        DayOfWeek = AndroidEndpoints.GetInt32(x, "dayOfWeek") ?? 1,
                        CustomerCode = AndroidEndpoints.GetString(x, "customerCode") ?? string.Empty,
                        CustomerName = AndroidEndpoints.GetString(x, "customerName") ?? string.Empty,
                        VisitOrder = AndroidEndpoints.GetInt32(x, "visitOrder") ?? 0,
                    }).OrderBy(x => x.DayOfWeek).ThenBy(x => x.VisitOrder)]
                    : [],
            });
        }
        return plans;
    }

    /// <summary>Books a team document for the caller exactly as <c>/ingest/jobs</c> does for a phone.</summary>
    private static async Task<Job> BookAsync(CentralApiDbContext db, TeamDocumentProcessor team, Tenant tenant, MobileUser caller,
        string documentType, string planId, string payload, HttpContext http, CancellationToken ct)
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            // Every save is a new document (the phones' rule: a repeated external id is "already booked").
            ExternalId = $"portal-route:{planId}:{Guid.NewGuid():N}",
            DocumentType = documentType,
            PayloadJson = payload,
            Status = JobStatus.Pending,
            EnqueuedAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = caller.Id,
            CorrelationId = LogCenter.CorrelationId.Of(http),
        };
        return await team.IngestAsync(db, tenant.Id, job, caller, ct);
    }

    /// <summary>The processor speaks English to support; the panel reads Turkish.</summary>
    private static string Translate(string? error) => error switch
    {
        null => "Rut planı kaydedilemedi.",
        _ when error.StartsWith("These people are not in your teams", StringComparison.Ordinal) =>
            "Bu kişiler sizin ekiplerinizde değil: " + error[(error.IndexOf(':') + 1)..].Trim().TrimEnd('.'),
        _ when error.StartsWith("name is required", StringComparison.Ordinal) => "Rut adı gerekli (en çok 120 karakter).",
        _ when error.StartsWith("Unknown users", StringComparison.Ordinal) => "Bilinmeyen kullanıcı: " + error[(error.IndexOf(':') + 1)..].Trim().TrimEnd('.'),
        _ when error.Contains("customerCode is required", StringComparison.Ordinal) => "Her durakta bir cari olmalı.",
        _ when error.Contains("at most 500 stops", StringComparison.Ordinal) => "Bir rutta en çok 500 durak olabilir.",
        _ when error.StartsWith("startDate", StringComparison.Ordinal) => "Başlangıç tarihi okunamadı.",
        _ => error,
    };

    private static bool ValidId(string planId) =>
        planId.Length is > 0 and <= TeamDocumentProcessor.MaxIdLength && planId.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');

    private static bool Day(string? value, DateOnly fallback, out DateOnly day)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            day = fallback;
            return true;
        }
        return TargetPeriod.TryParseDay(value, out day);
    }

    private static async Task<(Tenant? Tenant, TeamScope? Scope, IResult? Error)> AuthorizeAsync(HttpContext http, CentralApiDbContext db, bool plan, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return (null, null, access.Error);
        if (!RolePermissions.CanViewReports(access.User!))
            return (null, null, Fail(StatusCodes.Status403Forbidden, "PORTAL_REQUIRES_MANAGER", "The manager portal is for company administrators and managers."));
        if (plan && !RolePermissions.CanPlanRoutes(access.User!))
            return (null, null, Fail(StatusCodes.Status403Forbidden, "ROUTES_REQUIRE_MANAGER", "Rut planını yalnız admin ve yöneticiler yapar."));
        return (access.Tenant, await TeamScope.LoadAsync(db, access.User!, ct), null);
    }

    private static IResult Fail(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });
}
