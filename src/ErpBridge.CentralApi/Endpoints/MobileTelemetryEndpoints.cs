using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

public static class MobileTelemetryEndpoints
{
    private const int MaxEvents = 20;
    private const long MaxBodyBytes = 256 * 1024;
    private static readonly HashSet<string> AllowedKinds = new(StringComparer.OrdinalIgnoreCase)
    {
        "crash", "handled_exception", "http_error", "sync_failure", "database_error",
        "anr", "native_crash", "low_memory", "worker_failure"
    };
    private static readonly HashSet<string> AllowedSeverities = new(StringComparer.OrdinalIgnoreCase)
    {
        "warning", "error", "critical"
    };

    public static IEndpointRouteBuilder MapMobileTelemetryEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1/mobile/telemetry/batch", IngestAsync)
            .WithMetadata(new RequestSizeLimitAttribute(MaxBodyBytes))
            .WithTags("Mobile Telemetry")
            .RequireAuthorization(Program.MobilePolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy)
            .Produces<MobileTelemetryBatchResponse>()
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);
        return routes;
    }

    private static async Task<IResult> IngestAsync(
        MobileTelemetryBatchRequest request,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        if (!MobileLicensingEndpoints.TryDevice(http, out var deviceId, out var tenantId))
            return Error("INVALID_DEVICE_TOKEN", "Device token is invalid.", StatusCodes.Status401Unauthorized);
        if (http.Request.ContentLength is > MaxBodyBytes)
            return Error("PAYLOAD_TOO_LARGE", "Telemetry payload exceeds 256 KB.", StatusCodes.Status413PayloadTooLarge);
        if (request.Events.Count is < 1 or > MaxEvents)
            return Error("INVALID_BATCH", $"A telemetry batch must contain 1-{MaxEvents} events.");

        var device = await db.MobileDevices.Include(x => x.Tenant)
            .FirstOrDefaultAsync(x => x.Id == deviceId && x.TenantId == tenantId, ct);
        if (device is null || !device.IsActive || device.Tenant is not { IsActive: true })
            return Error("DEVICE_REVOKED", "This device is no longer licensed.", StatusCodes.Status403Forbidden);

        var invalid = request.Events.FirstOrDefault(x =>
            x.EventId == Guid.Empty
            || !AllowedKinds.Contains(x.Kind)
            || !AllowedSeverities.Contains(x.Severity)
            || x.Breadcrumbs.Count > 20);
        if (invalid is not null)
            return Error("INVALID_EVENT", "Telemetry event id, kind, severity, or breadcrumbs are invalid.");

        var incomingIds = request.Events.Select(x => x.EventId).Distinct().ToArray();
        var existingIds = await db.TelemetryEvents.AsNoTracking()
            .Where(x => incomingIds.Contains(x.EventId))
            .Select(x => x.EventId)
            .ToListAsync(ct);
        var existingSet = existingIds.ToHashSet();
        var now = DateTimeOffset.UtcNow;
        var accepted = 0;

        var uniqueNewEvents = request.Events
            .Where(x => !existingSet.Contains(x.EventId))
            .GroupBy(x => x.EventId)
            .Select(x => x.First());
        foreach (var item in uniqueNewEvents)
        {
            var kind = item.Kind.Trim().ToLowerInvariant();
            var severity = item.Severity.Trim().ToLowerInvariant();
            var exceptionType = TelemetrySanitizer.Clean(item.ExceptionType, 256);
            var stack = TelemetrySanitizer.Clean(item.StackTrace, 32_000);
            var screen = TelemetrySanitizer.Clean(item.Screen, 128);
            var route = TelemetrySanitizer.NormalizeRoute(item.HttpRoute);
            var fingerprint = TelemetrySanitizer.Fingerprint(kind, exceptionType, stack, screen, item.HttpMethod, route, item.HttpStatus);
            var issue = await db.TelemetryIssues.FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Fingerprint == fingerprint, ct);
            var occurred = item.OccurredAtUtc == default || item.OccurredAtUtc > now.AddMinutes(5)
                ? now
                : item.OccurredAtUtc;
            var title = TelemetrySanitizer.Clean(
                exceptionType ?? item.Message ?? $"{kind} error", 240) ?? $"{kind} error";

            if (issue is null)
            {
                issue = new TelemetryIssue
                {
                    TenantId = tenantId,
                    Fingerprint = fingerprint,
                    Kind = kind,
                    Severity = severity,
                    Title = title,
                    FirstSeenAtUtc = occurred,
                    LastSeenAtUtc = occurred,
                    OccurrenceCount = 0,
                };
                db.TelemetryIssues.Add(issue);
            }
            else if (issue.Status == TelemetryIssueStatus.Resolved)
            {
                issue.Status = TelemetryIssueStatus.Open;
                issue.ResolvedAtUtc = null;
            }

            issue.LastSeenAtUtc = occurred > issue.LastSeenAtUtc ? occurred : issue.LastSeenAtUtc;
            issue.OccurrenceCount += 1;
            issue.LastAppVersion = TelemetrySanitizer.Clean(item.AppVersion, 64);
            issue.LastDeviceId = deviceId;
            issue.Severity = MaxSeverity(issue.Severity, severity);

            var breadcrumbs = item.Breadcrumbs.Take(20).Select(x => new
            {
                timestampUtc = x.TimestampUtc,
                category = TelemetrySanitizer.Clean(x.Category, 32),
                message = TelemetrySanitizer.Clean(x.Message, 180),
            });
            db.TelemetryEvents.Add(new TelemetryEvent
            {
                EventId = item.EventId,
                TenantId = tenantId,
                MobileDeviceId = deviceId,
                TelemetryIssueId = issue.Id,
                OccurredAtUtc = occurred,
                ReceivedAtUtc = now,
                Kind = kind,
                Severity = severity,
                AppVersion = TelemetrySanitizer.Clean(item.AppVersion, 64),
                AndroidVersion = TelemetrySanitizer.Clean(item.AndroidVersion, 64),
                DeviceModel = TelemetrySanitizer.Clean(item.DeviceModel, 128),
                Screen = screen,
                Operation = TelemetrySanitizer.Clean(item.Operation, 128),
                ExceptionType = exceptionType,
                Message = TelemetrySanitizer.Clean(item.Message, 4_000),
                StackTrace = stack,
                HttpMethod = TelemetrySanitizer.Clean(item.HttpMethod, 16)?.ToUpperInvariant(),
                HttpRoute = string.IsNullOrWhiteSpace(route) ? null : route,
                HttpStatus = item.HttpStatus is >= 100 and <= 599 ? item.HttpStatus : null,
                CorrelationId = TelemetrySanitizer.Clean(item.CorrelationId, 128),
                BreadcrumbsJson = JsonSerializer.Serialize(breadcrumbs),
            });
            accepted += 1;
        }

        device.LastSeenAtUtc = now;
        var latestVersion = request.Events.Select(x => TelemetrySanitizer.Clean(x.AppVersion, 64))
            .LastOrDefault(x => !string.IsNullOrWhiteSpace(x));
        if (latestVersion is not null) device.AppVersion = latestVersion;
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(new MobileTelemetryBatchResponse
        {
            Accepted = accepted,
            Duplicates = request.Events.Count - accepted,
        });
    }

    private static string MaxSeverity(string current, string incoming)
    {
        static int Rank(string value) => value.ToLowerInvariant() switch
        {
            "critical" => 3,
            "error" => 2,
            _ => 1,
        };
        return Rank(incoming) > Rank(current) ? incoming : current;
    }

    private static IResult Error(string code, string message, int status = StatusCodes.Status400BadRequest) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });
}
