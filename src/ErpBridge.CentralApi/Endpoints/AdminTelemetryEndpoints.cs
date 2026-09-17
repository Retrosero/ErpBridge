using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.LogCenter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>Admin-only phone/agent diagnostics list over <c>log_events</c>, limited to recent bounded rows.</summary>
public static class AdminTelemetryEndpoints
{
    public static IEndpointRouteBuilder MapAdminTelemetryEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1/admin/telemetry", ListAsync)
            .WithName("AdminTelemetryList")
            .WithTags("Admin/Telemetry")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy)
            .Produces<MobileTelemetryEventDto[]>(StatusCodes.Status200OK);
        return routes;
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] Guid? tenantId,
        [FromQuery] string? severity,
        [FromQuery] int? take,
        [FromQuery] string? kind,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var count = Math.Clamp(take ?? 100, 1, 500);
        // Log Merkezi L1c: reads log_events (same response shape). Server, portal and admin events belong to the
        // Log Merkezi page, not this phone/agent diagnostics list.
        string[] sources = [LogSources.Android, LogSources.WindowsAgent, LogSources.WindowsService];
        var query = db.LogEvents.AsNoTracking().Where(row => sources.Contains(row.Source));
        if (tenantId.HasValue) query = query.Where(row => row.TenantId == tenantId.Value);
        if (!string.IsNullOrWhiteSpace(severity))
        {
            var stored = LogSeverity.Normalize(severity);
            query = query.Where(row => row.Severity == stored);
        }
        // Kinds are stored upper-case ASCII by the writer, so one comparison covers "crash", "CRASH", "desktop_exception".
        if (!string.IsNullOrWhiteSpace(kind))
        {
            var normalized = LogEventWriter.NormalizeKind(kind);
            query = query.Where(row => row.Kind == normalized);
        }
        var rows = await query.OrderByDescending(row => row.OccurredAtMs).Take(count).ToListAsync(ct);
        return JsonResults.Ok(rows.Select(row => new MobileTelemetryEventDto
        {
            Id = row.Id, TenantId = row.TenantId ?? Guid.Empty, OccurredAtUtc = row.OccurredAtUtc, ReceivedAtUtc = row.ReceivedAtUtc,
            Kind = row.Kind, Source = row.Source == LogSources.Android ? "Mobil" : "Windows Agent",
            Severity = row.Severity, AppVersion = row.AppVersion, AndroidVersion = row.OsVersion,
            DeviceModel = row.DeviceModel, Screen = row.Screen, Operation = row.Operation, ExceptionType = row.ExceptionType,
            Message = row.Message, StackTrace = row.StackTrace, HttpMethod = row.HttpMethod, HttpRoute = row.HttpRoute,
            HttpStatus = row.HttpStatus, CorrelationId = row.CorrelationId,
        }).ToArray());
    }
}
