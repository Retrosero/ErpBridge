using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>Admin-only telemetry search, intentionally limited to recent bounded rows.</summary>
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
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var count = Math.Clamp(take ?? 100, 1, 500);
        var query = db.MobileTelemetryEvents.AsNoTracking();
        if (tenantId.HasValue) query = query.Where(row => row.TenantId == tenantId.Value);
        if (!string.IsNullOrWhiteSpace(severity))
            query = query.Where(row => row.Severity == severity.Trim().ToUpperInvariant());
        var rows = await query.OrderByDescending(row => row.OccurredAtUtc).Take(count).ToListAsync(ct);
        return JsonResults.Ok(rows.Select(row => new MobileTelemetryEventDto
        {
            Id = row.Id, TenantId = row.TenantId, OccurredAtUtc = row.OccurredAtUtc, ReceivedAtUtc = row.ReceivedAtUtc,
            Kind = row.Kind, Source = row.Kind.StartsWith("desktop_", StringComparison.OrdinalIgnoreCase) ? "Windows Agent" : "Mobil",
            Severity = row.Severity, AppVersion = row.AppVersion, AndroidVersion = row.AndroidVersion,
            DeviceModel = row.DeviceModel, Screen = row.Screen, Operation = row.Operation, ExceptionType = row.ExceptionType,
            Message = row.Message, StackTrace = row.StackTrace, HttpMethod = row.HttpMethod, HttpRoute = row.HttpRoute,
            HttpStatus = row.HttpStatus, CorrelationId = row.CorrelationId,
        }).ToArray());
    }
}
