using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

public static class AdminTelemetryEndpoints
{
    public static IEndpointRouteBuilder MapAdminTelemetryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/telemetry")
            .WithTags("Admin/Telemetry")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);
        group.MapGet("/summary", SummaryAsync);
        group.MapGet("/issues", ListAsync);
        group.MapGet("/issues/{id:guid}", DetailAsync);
        group.MapPatch("/issues/{id:guid}/status", UpdateStatusAsync);
        return routes;
    }

    private static async Task<IResult> SummaryAsync(
        [FromQuery] Guid? tenantId,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        var since = DateTimeOffset.UtcNow.AddHours(-24);
        var events = db.TelemetryEvents.AsNoTracking().Where(x => x.OccurredAtUtc >= since);
        var issues = db.TelemetryIssues.AsNoTracking().AsQueryable();
        var devices = db.MobileDevices.AsNoTracking().Where(x => x.IsActive);
        if (tenantId.HasValue)
        {
            events = events.Where(x => x.TenantId == tenantId.Value);
            issues = issues.Where(x => x.TenantId == tenantId.Value);
            devices = devices.Where(x => x.TenantId == tenantId.Value);
        }

        var crashKinds = new[] { "crash", "anr", "native_crash" };
        var crashes = await events.CountAsync(x => crashKinds.Contains(x.Kind), ct);
        var affected = await events.Select(x => x.MobileDeviceId).Distinct().CountAsync(ct);
        var active = await devices.CountAsync(ct);
        var crashedDevices = await events.Where(x => crashKinds.Contains(x.Kind))
            .Select(x => x.MobileDeviceId).Distinct().CountAsync(ct);
        var openCritical = await issues.CountAsync(
            x => x.Status == TelemetryIssueStatus.Open && x.Severity == "critical", ct);
        var rate = active == 0 ? 100m : Math.Round((active - Math.Min(active, crashedDevices)) * 100m / active, 1);
        return JsonResults.Ok(new TelemetrySummaryDto
        {
            CrashesLast24Hours = crashes,
            OpenCriticalIssues = openCritical,
            AffectedDevices = affected,
            ActiveDevices = active,
            CrashFreeDeviceRate = rate,
        });
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] Guid? tenantId,
        [FromQuery] Guid? deviceId,
        [FromQuery] string? status,
        [FromQuery] string? severity,
        [FromQuery] string? kind,
        [FromQuery] string? appVersion,
        [FromQuery] string? search,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize == 0 ? 25 : pageSize, 1, 100);
        var query = db.TelemetryIssues.AsNoTracking().Include(x => x.Tenant).AsQueryable();
        if (tenantId.HasValue) query = query.Where(x => x.TenantId == tenantId.Value);
        if (deviceId.HasValue) query = query.Where(x => x.Events.Any(e => e.MobileDeviceId == deviceId.Value));
        if (TryStatus(status, out var parsedStatus)) query = query.Where(x => x.Status == parsedStatus);
        if (!string.IsNullOrWhiteSpace(severity)) query = query.Where(x => x.Severity == severity.ToLower());
        if (!string.IsNullOrWhiteSpace(kind)) query = query.Where(x => x.Kind == kind.ToLower());
        if (!string.IsNullOrWhiteSpace(appVersion)) query = query.Where(x => x.LastAppVersion == appVersion);
        if (from.HasValue) query = query.Where(x => x.LastSeenAtUtc >= from.Value);
        if (to.HasValue) query = query.Where(x => x.LastSeenAtUtc <= to.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(term) || x.Fingerprint.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.LastSeenAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return JsonResults.Ok(new TelemetryIssueListResponse
        {
            Items = rows.Select(x => ToDto(x)).ToArray(),
            Total = total,
            Page = page,
            PageSize = pageSize,
        });
    }

    private static async Task<IResult> DetailAsync(Guid id, CentralApiDbContext db, CancellationToken ct)
    {
        var issue = await db.TelemetryIssues.AsNoTracking().Include(x => x.Tenant)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (issue is null) return Error("ISSUE_NOT_FOUND", "Telemetry issue not found.", 404);
        var events = await db.TelemetryEvents.AsNoTracking().Include(x => x.MobileDevice)
            .Where(x => x.TelemetryIssueId == id)
            .OrderByDescending(x => x.OccurredAtUtc).Take(50).ToListAsync(ct);
        return JsonResults.Ok(new TelemetryIssueDetailDto
        {
            Issue = ToDto(issue),
            Events = events.Select(ToEventDto).ToArray(),
        });
    }

    private static async Task<IResult> UpdateStatusAsync(
        Guid id,
        UpdateTelemetryIssueStatusRequest request,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        if (!TryStatus(request.Status, out var status))
            return Error("INVALID_STATUS", "Status must be open, resolved, or ignored.");
        var issue = await db.TelemetryIssues.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (issue is null) return Error("ISSUE_NOT_FOUND", "Telemetry issue not found.", 404);
        issue.Status = status;
        issue.ResolvedAtUtc = status == TelemetryIssueStatus.Resolved ? DateTimeOffset.UtcNow : null;
        await db.SaveChangesAsync(ct);
        var tenantName = await db.Tenants.Where(x => x.Id == issue.TenantId).Select(x => x.Name).FirstOrDefaultAsync(ct);
        return JsonResults.Ok(ToDto(issue, tenantName));
    }

    private static bool TryStatus(string? value, out TelemetryIssueStatus status)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            status = default;
            return false;
        }
        return Enum.TryParse(value, true, out status);
    }

    private static TelemetryIssueDto ToDto(TelemetryIssue issue, string? tenantName = null) => new()
    {
        Id = issue.Id,
        TenantId = issue.TenantId,
        TenantName = tenantName ?? issue.Tenant?.Name ?? string.Empty,
        Fingerprint = issue.Fingerprint,
        Kind = issue.Kind,
        Severity = issue.Severity,
        Title = issue.Title,
        Status = issue.Status.ToString().ToLowerInvariant(),
        FirstSeenAtUtc = issue.FirstSeenAtUtc,
        LastSeenAtUtc = issue.LastSeenAtUtc,
        OccurrenceCount = issue.OccurrenceCount,
        LastAppVersion = issue.LastAppVersion,
        LastDeviceId = issue.LastDeviceId,
    };

    private static TelemetryEventDto ToEventDto(TelemetryEvent item) => new()
    {
        Id = item.Id,
        EventId = item.EventId,
        DeviceId = item.MobileDeviceId,
        DeviceName = item.MobileDevice?.DisplayName ?? string.Empty,
        OccurredAtUtc = item.OccurredAtUtc,
        Kind = item.Kind,
        Severity = item.Severity,
        AppVersion = item.AppVersion,
        AndroidVersion = item.AndroidVersion,
        DeviceModel = item.DeviceModel,
        Screen = item.Screen,
        Operation = item.Operation,
        ExceptionType = item.ExceptionType,
        Message = item.Message,
        StackTrace = item.StackTrace,
        HttpMethod = item.HttpMethod,
        HttpRoute = item.HttpRoute,
        HttpStatus = item.HttpStatus,
        CorrelationId = item.CorrelationId,
        BreadcrumbsJson = item.BreadcrumbsJson,
    };

    private static IResult Error(string code, string message, int status = 400) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });
}
