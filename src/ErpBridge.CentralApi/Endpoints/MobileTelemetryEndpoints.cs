using System.Text.Json;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.LogCenter;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>Receives bounded, scrubbed diagnostics from the Android app.</summary>
public static class MobileTelemetryEndpoints
{
    private const string TelemetryScope = "mobile:telemetry";
    private const string LegacyMobileScope = "mobile:read";
    private const int MaxBatchSize = 50;

    public static IEndpointRouteBuilder MapMobileTelemetryEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1/mobile/telemetry/batch", IngestAsync)
            .WithName("MobileTelemetryBatch")
            .WithTags("Mobile telemetry")
            .RequireAuthorization(Program.MobileClientPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy)
            .Produces<MobileTelemetryBatchResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);
        return routes;
    }

    private static async Task<IResult> IngestAsync(
        MobileTelemetryBatchRequest? body,
        HttpContext http,
        CentralApiDbContext db,
        ILogEventWriter logWriter,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        if (body?.Events is not { Count: > 0 })
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "EVENTS_REQUIRED", Message = "At least one telemetry event is required." });
        if (body.Events.Count > MaxBatchSize)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "BATCH_TOO_LARGE", Message = $"A telemetry batch may contain at most {MaxBatchSize} events." });
        if (!http.User.TryGetTenantId(out var tenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized, new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });

        // A signed-in mobile user reports for their own tenant (state checked by MobileClientPolicy).
        var allowed = ErpBridge.CentralApi.Mobile.MobileUserAccess.IsMobileUser(http.User);
        if (!allowed)
        {
            if (!Guid.TryParse(http.User.FindFirst(ApiKeyClaims.ApiKeyId)?.Value, out var keyId))
                return JsonResults.Status(StatusCodes.Status401Unauthorized, new ApiError { ErrorCode = "INVALID_API_KEY", Message = "API key identity is missing." });
            allowed = await db.ApiKeys.AsNoTracking().AnyAsync(key => key.Id == keyId && key.TenantId == tenantId && key.IsActive
                && (key.Scopes.Contains(TelemetryScope) || key.Scopes.Contains(LegacyMobileScope) || key.Scopes.Contains("*")), ct);
        }
        if (!allowed)
            return JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError { ErrorCode = "MOBILE_TELEMETRY_SCOPE_REQUIRED", Message = "API key requires the mobile:telemetry scope." });

        var eventIds = body.Events.Select(e => e.EventId?.Trim()).Where(id => !string.IsNullOrWhiteSpace(id)).Cast<string>().Distinct(StringComparer.Ordinal).ToArray();
        var existing = await db.MobileTelemetryEvents.AsNoTracking().Where(e => e.TenantId == tenantId && eventIds.Contains(e.EventId)).Select(e => e.EventId).ToListAsync(ct);
        var known = existing.ToHashSet(StringComparer.Ordinal);
        var accepted = 0;
        foreach (var source in body.Events)
        {
            var eventId = source.EventId?.Trim();
            if (string.IsNullOrWhiteSpace(eventId) || eventId.Length > 64 || !Guid.TryParse(eventId, out _) || !known.Add(eventId))
                continue;
            db.MobileTelemetryEvents.Add(ToEntity(source, tenantId, eventId));
            accepted++;
        }
        if (accepted > 0) await db.SaveChangesAsync(ct);
        await CopyToLogCenterAsync(body.Events, http, tenantId, logWriter, loggerFactory, ct);
        return JsonResults.Ok(new MobileTelemetryBatchResponse { Accepted = accepted, Duplicate = body.Events.Count - accepted });
    }

    /// <summary>
    /// Log Merkezi (L0d): the same events also go to <c>log_events</c>, with the user and device taken from a
    /// signed-in token. The legacy table above stays the response's source of truth; a failure here is
    /// logged and never fails the phone's upload (the phone would retry the batch forever).
    /// </summary>
    private static async Task CopyToLogCenterAsync(IReadOnlyList<MobileTelemetryEventRequest> events, HttpContext http, Guid tenantId,
        ILogEventWriter logWriter, ILoggerFactory loggerFactory, CancellationToken ct)
    {
        var mobileUser = ErpBridge.CentralApi.Mobile.MobileUserAccess.IsMobileUser(http.User);
        Guid? userId = mobileUser && Guid.TryParse(http.User.FindFirst("sub")?.Value, out var parsedUser) ? parsedUser : null;
        var tokenDevice = mobileUser ? http.User.FindFirst(CentralApiClaims.DeviceId)?.Value : null;
        var inputs = events
            .Where(source => source.EventId?.Trim() is { Length: > 0 and <= 64 } id && Guid.TryParse(id, out _))
            .Select(source => new LogEventInput
            {
                EventId = source.EventId!.Trim(),
                Source = LogSources.Android,
                TenantId = tenantId,
                OccurredAtUtc = source.OccurredAtUtc,
                Severity = source.Severity,
                Kind = source.Kind,
                Operation = source.Operation,
                Screen = source.Screen,
                Message = source.Message,
                ExceptionType = source.ExceptionType,
                StackTrace = source.StackTrace,
                AppVersion = source.AppVersion,
                OsVersion = source.AndroidVersion,
                DeviceModel = source.DeviceModel,
                DeviceId = string.IsNullOrWhiteSpace(tokenDevice) ? source.DeviceId : tokenDevice,
                UserId = userId,
                SessionId = source.SessionId,
                CorrelationId = source.CorrelationId,
                HttpMethod = source.HttpMethod,
                HttpRoute = source.HttpRoute,
                HttpStatus = source.HttpStatus,
                PropertiesJson = source.Properties is { ValueKind: JsonValueKind.Object } properties ? properties.GetRawText() : null,
                BreadcrumbsJson = source.Breadcrumbs is { ValueKind: JsonValueKind.Array } breadcrumbs ? breadcrumbs.GetRawText() : null,
            })
            .ToList();
        if (inputs.Count == 0) return;
        try
        {
            await logWriter.WriteAsync(inputs, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            loggerFactory.CreateLogger(typeof(MobileTelemetryEndpoints)).LogWarning(ex, "Copying {Count} phone telemetry events to the log centre failed.", inputs.Count);
        }
    }

    private static MobileTelemetryEvent ToEntity(MobileTelemetryEventRequest source, Guid tenantId, string eventId) => new()
    {
        TenantId = tenantId,
        EventId = eventId,
        OccurredAtUtc = source.OccurredAtUtc ?? DateTimeOffset.UtcNow,
        ReceivedAtUtc = DateTimeOffset.UtcNow,
        Kind = Bound(source.Kind, 32), Severity = Bound(source.Severity, 16), AppVersion = Bound(source.AppVersion, 64),
        AndroidVersion = Bound(source.AndroidVersion, 32), DeviceModel = Bound(source.DeviceModel, 128), Screen = Bound(source.Screen, 120),
        Operation = Bound(source.Operation, 120), ExceptionType = Bound(source.ExceptionType, 160), Message = Bound(source.Message, 1000),
        StackTrace = Bound(source.StackTrace, 4000), HttpMethod = NullOrBound(source.HttpMethod, 16), HttpRoute = NullOrBound(source.HttpRoute, 300),
        HttpStatus = source.HttpStatus is >= 100 and <= 599 ? source.HttpStatus : null, CorrelationId = NullOrBound(source.CorrelationId, 128),
        BreadcrumbsJson = SerializeBreadcrumbs(source.Breadcrumbs),
    };

    private static string Bound(string? value, int max)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }
    private static string? NullOrBound(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : Bound(value, max);
    private static string SerializeBreadcrumbs(JsonElement? breadcrumbs) => breadcrumbs is { ValueKind: JsonValueKind.Array } value
        ? Bound(value.GetRawText(), 4000) : "[]";
}
