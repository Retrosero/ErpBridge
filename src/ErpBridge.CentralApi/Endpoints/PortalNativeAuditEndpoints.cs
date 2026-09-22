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
/// Maps <c>/api/v1/portal/native/audit</c> (GOAL_PANEL_ERPSIZ E7b/D5): who changed a card or a
/// payment from the portal, and when. With <c>entity</c>+<c>key</c> it is one row's "Geçmiş"
/// (every write, unbounded by date); without <c>key</c> it is the company-wide <c>/denetim</c> list,
/// bounded to a date range like the ERP document list. Same gate as every native write
/// (<see cref="PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync"/>): the log exists because a
/// single ADMIN can change everything, so only that ADMIN needs to review the trail.
/// </summary>
public static class PortalNativeAuditEndpoints
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;
    private const int MaxRangeDays = 92;

    public static IEndpointRouteBuilder MapPortalNativeAuditEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/native")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapGet("/audit", ListAsync).WithName("PortalNativeAudit");
        return routes;
    }

    private static async Task<IResult> ListAsync(
        HttpContext http, [FromServices] CentralApiDbContext db,
        string? entity, string? key, string? from, string? to, Guid? userId, int? page, int? pageSize, CancellationToken ct)
    {
        var (tenant, _, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        if (!string.IsNullOrWhiteSpace(key) && string.IsNullOrWhiteSpace(entity))
            return Invalid("entity is required together with key.");

        var size = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);
        var pageNo = Math.Max(1, page ?? 1);
        var query = db.NativeAuditLogEntries.AsNoTracking().Where(a => a.TenantId == tenant!.Id);
        if (!string.IsNullOrWhiteSpace(entity)) query = query.Where(a => a.Entity == entity);
        if (!string.IsNullOrWhiteSpace(key)) query = query.Where(a => a.EntityKey == key);
        if (userId is { } wanted) query = query.Where(a => a.UserId == wanted);

        // A single row's Geçmiş is naturally small (its own writes) and unbounded by date; the
        // company-wide list is not, so it gets the same bounded window the ERP document list uses.
        var sqlite = db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
        DateTimeOffset? windowStart = null, windowEnd = null;
        if (string.IsNullOrWhiteSpace(key))
        {
            var today = PortalReports.BusinessDate(null, DateTimeOffset.UtcNow);
            if (!TryDay(from, today.AddDays(-6), out var start)) return Invalid("from must be yyyy-MM-dd.");
            if (!TryDay(to, today, out var end)) return Invalid("to must be yyyy-MM-dd.");
            if (end < start) return Invalid("to is before from.");
            if (end.DayNumber - start.DayNumber + 1 > MaxRangeDays) return Invalid($"At most {MaxRangeDays} days.");
            windowStart = PortalReports.IstanbulDayStartUtc(start);
            windowEnd = PortalReports.IstanbulDayStartUtc(end.AddDays(1));
            if (!sqlite) query = query.Where(a => a.CreatedAtUtc >= windowStart && a.CreatedAtUtc < windowEnd);
        }

        // Ordered in memory, not in SQL: SQLite (tests only) cannot ORDER BY a DateTimeOffset column.
        var rows = (await query.ToListAsync(ct)).OrderByDescending(a => a.CreatedAtUtc).ToList();
        if (windowStart is not null) rows = rows.Where(a => a.CreatedAtUtc >= windowStart && a.CreatedAtUtc < windowEnd).ToList();

        var total = rows.Count;
        var page_ = rows.Skip((pageNo - 1) * size).Take(size).Select(a => new PortalAuditRow
        {
            Id = a.Id,
            Entity = a.Entity,
            EntityKey = a.EntityKey,
            Action = a.Action,
            Summary = a.Summary,
            BeforeJson = a.BeforeJson,
            AfterJson = a.AfterJson,
            UserId = a.UserId,
            UserName = a.UserName,
            CreatedAtUtc = a.CreatedAtUtc,
        }).ToList();

        return JsonResults.Ok(new PortalAuditResponse { Items = page_, Total = total, Page = pageNo, PageSize = size });
    }

    private static bool TryDay(string? value, DateOnly fallback, out DateOnly day)
    {
        if (string.IsNullOrWhiteSpace(value)) { day = fallback; return true; }
        return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day);
    }

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_QUERY", Message = message });
}
