using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.LogCenter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Log Merkezi L1a — the operator's read side of <c>log_events</c>. Tenant is an optional filter from the query
/// string (an admin token has no tenant claim, KB rule 10). Newest first, keyset-paged on
/// (<c>OccurredAtMs</c>, <c>EventId</c>) so a page never repeats or skips a row while new events arrive.
/// </summary>
public static class AdminLogEndpoints
{
    private const int DefaultTake = 50;
    private const int MaxTake = 200;
    private const int ListMessageLength = 300;
    private const int FacetLimit = 30;
    private static readonly TimeSpan DefaultFacetWindow = TimeSpan.FromHours(24);

    public static IEndpointRouteBuilder MapAdminLogEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/logs").WithTags("Admin/Logs")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("", ListAsync).WithName("AdminLogList")
            .Produces<AdminLogPageDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest);
        group.MapGet("/facets", FacetsAsync).WithName("AdminLogFacets")
            .Produces<AdminLogFacetsDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest);
        group.MapGet("/{id:guid}", DetailAsync).WithName("AdminLogDetail")
            .Produces<AdminLogEventDetailDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);
        return routes;
    }

    private static async Task<IResult> ListAsync(
        HttpContext http,
        [FromQuery] string? before,
        [FromQuery] int? take,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var (query, errorCode, message) = LogQuery.Parse(http.Request.Query);
        if (query is null) return BadRequest(errorCode!, message!);
        if (!TryParseCursor(before, out var cursor)) return BadRequest("INVALID_CURSOR", "before must be a cursor returned by a previous page.");

        var count = Math.Clamp(take ?? DefaultTake, 1, MaxTake);
        var rows = query.Apply(db.LogEvents.AsNoTracking(), db.Database.IsNpgsql());
        if (cursor is { } position)
        {
            var (ms, eventId) = position;
            rows = rows.Where(e => e.OccurredAtMs < ms || (e.OccurredAtMs == ms && string.Compare(e.EventId, eventId) < 0));
        }

        var page = await rows
            .OrderByDescending(e => e.OccurredAtMs).ThenByDescending(e => e.EventId)
            .Take(count + 1)
            .Select(e => new AdminLogEventDto
            {
                Id = e.Id, EventId = e.EventId, Source = e.Source, TenantId = e.TenantId, OccurredAtUtc = e.OccurredAtUtc, OccurredAtMs = e.OccurredAtMs,
                ReceivedAtUtc = e.ReceivedAtUtc, Severity = e.Severity, Kind = e.Kind, Operation = e.Operation, Screen = e.Screen,
                Message = e.Message.Length > ListMessageLength ? e.Message.Substring(0, ListMessageLength) : e.Message,
                ExceptionType = e.ExceptionType, AppVersion = e.AppVersion, OsVersion = e.OsVersion, DeviceModel = e.DeviceModel,
                DeviceId = e.DeviceId, UserId = e.UserId, AgentId = e.AgentId, SessionId = e.SessionId, CorrelationId = e.CorrelationId,
                HttpMethod = e.HttpMethod, HttpRoute = e.HttpRoute, HttpStatus = e.HttpStatus, DurationMs = e.DurationMs,
                RepeatCount = e.RepeatCount, FingerprintId = e.FingerprintId,
            })
            .ToListAsync(ct);

        string? next = null;
        if (page.Count > count)
        {
            page.RemoveAt(count);
            var last = page[^1];
            next = FormatCursor(last.OccurredAtMs, last.EventId);
        }
        await FillNamesAsync(db, page, ct);
        return JsonResults.Ok(new AdminLogPageDto { Items = page, NextBefore = next });
    }

    private static async Task<IResult> DetailAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var row = await db.LogEvents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (row is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "LOG_EVENT_NOT_FOUND", Message = "Log event not found." });

        var detail = new AdminLogEventDetailDto
        {
            Id = row.Id, EventId = row.EventId, Source = row.Source, TenantId = row.TenantId, OccurredAtUtc = row.OccurredAtUtc,
            ReceivedAtUtc = row.ReceivedAtUtc, Severity = row.Severity, Kind = row.Kind, Operation = row.Operation, Screen = row.Screen,
            Message = row.Message, ExceptionType = row.ExceptionType, AppVersion = row.AppVersion, OsVersion = row.OsVersion,
            DeviceModel = row.DeviceModel, DeviceId = row.DeviceId, UserId = row.UserId, AgentId = row.AgentId, SessionId = row.SessionId,
            CorrelationId = row.CorrelationId, HttpMethod = row.HttpMethod, HttpRoute = row.HttpRoute, HttpStatus = row.HttpStatus,
            DurationMs = row.DurationMs, RepeatCount = row.RepeatCount, FingerprintId = row.FingerprintId,
            StackTrace = row.StackTrace, PropertiesJson = row.PropertiesJson, BreadcrumbsJson = row.BreadcrumbsJson,
        };
        if (row.FingerprintId is { } groupId)
        {
            detail.Group = await db.LogErrorGroups.AsNoTracking().Where(g => g.Id == groupId)
                .Select(g => new AdminLogGroupSummaryDto
                {
                    Id = g.Id, Status = g.Status, TotalCount = g.TotalCount, FirstSeenAtUtc = g.FirstSeenAtUtc, LastSeenAtUtc = g.LastSeenAtUtc,
                })
                .FirstOrDefaultAsync(ct);
        }
        await FillNamesAsync(db, [detail], ct);
        return JsonResults.Ok(detail);
    }

    private static async Task<IResult> FacetsAsync(
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var (parsed, errorCode, message) = LogQuery.Parse(http.Request.Query);
        if (parsed is null) return BadRequest(errorCode!, message!);

        // Facets always describe a bounded window: the last 24 hours unless the caller chose one.
        var toMs = parsed.ToMs ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var fromMs = parsed.FromMs ?? toMs - (long)DefaultFacetWindow.TotalMilliseconds;
        var query = parsed with { FromMs = fromMs, ToMs = toMs };
        var rows = query.Apply(db.LogEvents.AsNoTracking(), db.Database.IsNpgsql());

        var sources = await rows.GroupBy(e => e.Source).Select(g => new AdminLogFacetDto { Value = g.Key, Count = g.Count() }).ToListAsync(ct);
        var severities = await rows.GroupBy(e => e.Severity).Select(g => new AdminLogFacetDto { Value = g.Key, Count = g.Count() }).ToListAsync(ct);
        var kinds = await rows.GroupBy(e => e.Kind).Select(g => new AdminLogFacetDto { Value = g.Key, Count = g.Count() })
            .OrderByDescending(f => f.Count).Take(FacetLimit).ToListAsync(ct);
        var versions = await rows.Where(e => e.AppVersion != "").GroupBy(e => e.AppVersion)
            .Select(g => new AdminLogFacetDto { Value = g.Key, Count = g.Count() })
            .OrderByDescending(f => f.Count).Take(FacetLimit).ToListAsync(ct);

        return JsonResults.Ok(new AdminLogFacetsDto
        {
            FromUtc = DateTimeOffset.FromUnixTimeMilliseconds(fromMs),
            ToUtc = DateTimeOffset.FromUnixTimeMilliseconds(toMs),
            Total = sources.Sum(f => f.Count),
            Sources = sources.OrderByDescending(f => f.Count).ToList(),
            Severities = severities.OrderByDescending(f => LogSeverity.Rank(f.Value)).ToList(),
            Kinds = kinds,
            AppVersions = versions,
        });
    }

    /// <summary>Company, user and agent names for display; one query per kind of id on the page.</summary>
    private static async Task FillNamesAsync(CentralApiDbContext db, IReadOnlyCollection<AdminLogEventDto> items, CancellationToken ct)
    {
        var tenantIds = items.Where(i => i.TenantId is not null).Select(i => i.TenantId!.Value).Distinct().ToArray();
        var userIds = items.Where(i => i.UserId is not null).Select(i => i.UserId!.Value).Distinct().ToArray();
        var agentIds = items.Where(i => i.AgentId is not null).Select(i => i.AgentId!.Value).Distinct().ToArray();

        var tenants = tenantIds.Length == 0 ? new Dictionary<Guid, string>()
            : await db.Tenants.AsNoTracking().Where(t => tenantIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, t => t.Name, ct);
        var users = userIds.Length == 0 ? new Dictionary<Guid, string>()
            : await db.MobileUsers.AsNoTracking().Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName, ct);
        var agents = agentIds.Length == 0 ? new Dictionary<Guid, string>()
            : await db.Agents.AsNoTracking().Where(a => agentIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id, a => a.MachineId, ct);

        foreach (var item in items)
        {
            if (item.TenantId is { } t && tenants.TryGetValue(t, out var tenantName)) item.TenantName = tenantName;
            if (item.UserId is { } u && users.TryGetValue(u, out var userName)) item.UserName = userName;
            if (item.AgentId is { } a && agents.TryGetValue(a, out var agentName)) item.AgentName = agentName;
        }
    }

    internal static string FormatCursor(long occurredAtMs, string eventId) =>
        occurredAtMs.ToString(CultureInfo.InvariantCulture) + "_" + eventId;

    private static bool TryParseCursor(string? raw, out (long Ms, string EventId)? cursor)
    {
        cursor = null;
        if (string.IsNullOrWhiteSpace(raw)) return true;
        var separator = raw.IndexOf('_');
        if (separator <= 0 || separator == raw.Length - 1
            || !long.TryParse(raw.AsSpan(0, separator), NumberStyles.None, CultureInfo.InvariantCulture, out var ms))
            return false;
        cursor = (ms, raw[(separator + 1)..]);
        return true;
    }

    private static IResult BadRequest(string code, string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = code, Message = message });
}
