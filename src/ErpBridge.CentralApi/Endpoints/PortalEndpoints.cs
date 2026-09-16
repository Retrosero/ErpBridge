using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Portal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal</c>: read-only figures for the company manager portal (Faz 41).
///
/// <para>The caller signs in with the same company account as the phone
/// (<c>/api/v1/android/account/login</c>). The company comes from the token, never from
/// the request, and every call re-checks the user, device and subscription. Only
/// administrators and managers may read; a salesperson gets 403
/// <c>PORTAL_REQUIRES_MANAGER</c>. Approvals and users are served by the existing
/// <c>/api/v1/android/approvals</c> and <c>/api/v1/android/account/users</c> endpoints.</para>
/// </summary>
public static class PortalEndpoints
{
    public static IEndpointRouteBuilder MapPortalEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapGet("/summary", SummaryAsync).WithName("PortalSummary");
        group.MapGet("/activity", ActivityAsync).WithName("PortalActivity");
        group.MapGet("/visits", VisitsAsync).WithName("PortalVisits");
        group.MapGet("/balances", BalancesAsync).WithName("PortalBalances");
        group.MapGet("/stock", StockAsync).WithName("PortalStock");
        return routes;
    }

    private static async Task<IResult> SummaryAsync(HttpContext http, [FromServices] CentralApiDbContext db, string? date, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeManagerAsync(http, db, ct);
        if (error is not null) return error;
        if (!TryDay(date, out var day)) return BadDate("date");

        var response = new PortalSummaryResponse
        {
            Date = Format(day),
            DataSource = tenant!.DataSource,
        };
        foreach (var document in await PortalReports.MoneyDocumentsAsync(db, tenant, day, day, ct))
            PortalReports.Add(PortalReports.LineFor(document.DocumentType, response.Sales, response.Collections, response.Disbursements, response.Returns), document.Amount);

        var visits = await PortalReports.VisitsAsync(db, tenant.Id, day, ct);
        response.VisitsPlanned = visits.Count(v => v.Planned);
        response.VisitsCompleted = visits.Count(v => v.Status == "COMPLETED");
        response.VisitsSkipped = visits.Count(v => v.Status == "SKIPPED");
        response.PendingApprovals = await db.ApprovalRequests.AsNoTracking()
            .CountAsync(r => r.TenantId == tenant.Id && r.Status == ApprovalStatuses.Pending, ct);
        return JsonResults.Ok(response);
    }

    private static async Task<IResult> ActivityAsync(HttpContext http, [FromServices] CentralApiDbContext db, string? from, string? to, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeManagerAsync(http, db, ct);
        if (error is not null) return error;
        if (!TryDay(from, out var start)) return BadDate("from");
        if (!TryDay(to, out var end)) return BadDate("to");
        if (end < start) return JsonResults.Status(400, new ApiError { ErrorCode = "INVALID_RANGE", Message = "to is before from." });
        if (end.DayNumber - start.DayNumber + 1 > PortalReports.MaxRangeDays)
            return JsonResults.Status(400, new ApiError { ErrorCode = "RANGE_TOO_LONG", Message = $"At most {PortalReports.MaxRangeDays} days." });

        var users = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenant!.Id)
            .Select(u => new { u.Id, u.Username, u.FullName, u.Role, u.DeletedAtUtc })
            .ToListAsync(ct);
        var byId = new Dictionary<Guid, PortalUserActivity>();
        var byName = new Dictionary<string, PortalUserActivity>(StringComparer.OrdinalIgnoreCase);
        foreach (var user in users)
        {
            var activity = new PortalUserActivity { UserId = user.Id, Username = user.Username, FullName = user.FullName, Role = user.Role };
            byId[user.Id] = activity;
            // A deleted user's name can be reused; visits then count for the current holder.
            if (user.DeletedAtUtc is null || !byName.ContainsKey(user.Username)) byName[user.Username] = activity;
        }
        PortalUserActivity? unattributed = null;

        foreach (var document in await PortalReports.MoneyDocumentsAsync(db, tenant!, start, end, ct))
        {
            PortalUserActivity target;
            if (document.CreatedByUserId is { } id && byId.TryGetValue(id, out var known)) target = known;
            else target = unattributed ??= new PortalUserActivity { Username = string.Empty, FullName = "Belirsiz (hesapsız veya eski kayıt)" };
            PortalReports.Add(PortalReports.LineFor(document.DocumentType, target.Sales, target.Collections, target.Disbursements, target.Returns), document.Amount);
        }

        var routeRecords = await PortalReports.RouteRecordsAsync(db, tenant!.Id, ct);
        for (var day = start; day <= end; day = day.AddDays(1))
        {
            foreach (var visit in PortalReports.BuildVisits(routeRecords, day))
            {
                if (!byName.TryGetValue(visit.Username, out var user)) continue;
                if (visit.Status == "COMPLETED") user.VisitsCompleted++;
                else if (visit.Status == "SKIPPED") user.VisitsSkipped++;
            }
        }

        var rows = byId.Values
            .Where(u => u.Sales.Count + u.Collections.Count + u.Disbursements.Count + u.Returns.Count + u.VisitsCompleted + u.VisitsSkipped > 0
                        || users.Any(x => x.Id == u.UserId && x.DeletedAtUtc is null))
            .OrderByDescending(u => u.Sales.Amount)
            .ThenBy(u => u.Username, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (unattributed is not null) rows.Add(unattributed);

        return JsonResults.Ok(new PortalActivityResponse { From = Format(start), To = Format(end), Users = rows });
    }

    private static async Task<IResult> VisitsAsync(HttpContext http, [FromServices] CentralApiDbContext db, string? date, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeManagerAsync(http, db, ct);
        if (error is not null) return error;
        if (!TryDay(date, out var day)) return BadDate("date");
        return JsonResults.Ok(new PortalVisitsResponse { Date = Format(day), Rows = await PortalReports.VisitsAsync(db, tenant!.Id, day, ct) });
    }

    private static async Task<IResult> BalancesAsync(HttpContext http, [FromServices] CentralApiDbContext db, string? search, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeManagerAsync(http, db, ct);
        if (error is not null) return error;
        return JsonResults.Ok(await PortalReports.BalancesAsync(db, tenant!.Id, search, ct));
    }

    private static async Task<IResult> StockAsync(HttpContext http, [FromServices] CentralApiDbContext db, string? search, bool? outOfStock, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeManagerAsync(http, db, ct);
        if (error is not null) return error;
        return JsonResults.Ok(await PortalReports.StockAsync(db, tenant!.Id, search, outOfStock == true, ct));
    }

    // ---- helpers --------------------------------------------------------------

    private static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeManagerAsync(
        HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access;
        if (access.User!.Role is not (MobileUserRoles.Admin or MobileUserRoles.Manager))
            return (null, null, JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
            {
                ErrorCode = "PORTAL_REQUIRES_MANAGER",
                Message = "The manager portal is for company administrators and managers.",
            }));
        return access;
    }

    /// <summary>An absent day means today in Istanbul.</summary>
    private static bool TryDay(string? value, out DateOnly day)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            day = PortalReports.BusinessDate(null, DateTimeOffset.UtcNow);
            return true;
        }
        return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day);
    }

    private static string Format(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static IResult BadDate(string name) =>
        JsonResults.Status(400, new ApiError { ErrorCode = "INVALID_DATE", Message = $"{name} must be yyyy-MM-dd." });
}
