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

        // Log Merkezi L1c: log_events is the only store. A failure here is a real 5xx — the phone keeps the
        // batch and retries — not something to swallow as before, when the legacy table was the source of truth.
        var inputs = ToLogInputs(body.Events, http, tenantId);
        var result = inputs.Count == 0 ? new LogWriteResult(0, 0) : await logWriter.WriteAsync(inputs, ct);
        var accepted = result.Accepted;
        return JsonResults.Ok(new MobileTelemetryBatchResponse { Accepted = accepted, Duplicate = body.Events.Count - accepted });
    }

    /// <summary>
    /// The phone's events as log centre input; the user and device come from a signed-in token (an API-key session
    /// may send <c>deviceId</c>). Events without a GUID id are dropped, as they always were.
    /// </summary>
    private static List<LogEventInput> ToLogInputs(IReadOnlyList<MobileTelemetryEventRequest> events, HttpContext http, Guid tenantId)
    {
        var mobileUser = ErpBridge.CentralApi.Mobile.MobileUserAccess.IsMobileUser(http.User);
        Guid? userId = mobileUser && Guid.TryParse(http.User.FindFirst("sub")?.Value, out var parsedUser) ? parsedUser : null;
        var tokenDevice = mobileUser ? http.User.FindFirst(CentralApiClaims.DeviceId)?.Value : null;
        return events
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
                RepeatCount = source.RepeatCount is { } repeat && repeat > 0 ? repeat : 1,
                PropertiesJson = source.Properties is { ValueKind: JsonValueKind.Object } properties ? properties.GetRawText() : null,
                BreadcrumbsJson = source.Breadcrumbs is { ValueKind: JsonValueKind.Array } breadcrumbs ? breadcrumbs.GetRawText() : null,
            })
            .ToList();
    }

    private static string Bound(string? value, int max)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }
    private static string? NullOrBound(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : Bound(value, max);
    private static string SerializeBreadcrumbs(JsonElement? breadcrumbs) => breadcrumbs is { ValueKind: JsonValueKind.Array } value
        ? Bound(value.GetRawText(), 4000) : "[]";
}
