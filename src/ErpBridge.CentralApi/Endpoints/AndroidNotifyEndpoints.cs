using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Long-polling <c>GET /api/v1/android/notify</c> for the mobile client. The
/// request blocks until either (a) new data lands for the caller's tenant — a
/// bootstrap snapshot upload <b>or</b> an agent change-set push — (b) the
/// <c>wait</c> budget elapses, or (c) the request is aborted.
///
/// <para>
/// This is the mobile counterpart of <see cref="BootstrapNotifyEndpoints"/>
/// (which is agent-JWT only). It lets the app fire a sync within seconds of an
/// ERP change instead of waiting for the hourly periodic worker. Both endpoints
/// share the same <see cref="IBootstrapNotificationHub"/>.
/// </para>
/// </summary>
public static class AndroidNotifyEndpoints
{
    private const int MinWaitSeconds = 1;
    private const int MaxWaitSeconds = 60;
    private const int DefaultWaitSeconds = 30;

    public static IEndpointRouteBuilder MapAndroidNotifyEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1/android/notify", NotifyAsync)
            .WithName("AndroidNotify")
            .WithTags("AndroidMobileSync")
            .Produces<BootstrapNotifyResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(Program.ApiKeyPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        return routes;
    }

    private static async Task<IResult> NotifyAsync(
        [FromServices] IBootstrapNotificationHub hub,
        HttpContext http,
        [FromQuery] int? wait,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });
        }

        var requested = wait.GetValueOrDefault(DefaultWaitSeconds);
        if (requested < MinWaitSeconds || requested > MaxWaitSeconds)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError
                {
                    ErrorCode = "INVALID_WAIT",
                    Message = $"wait must be between {MinWaitSeconds} and {MaxWaitSeconds} seconds.",
                });
        }

        var cursor = await hub.WaitAsync(tenantId, TimeSpan.FromSeconds(requested), ct).ConfigureAwait(false);
        if (cursor == DateTimeOffset.MinValue)
        {
            // Wait elapsed or connection aborted — the client reconnects.
            return Results.NoContent();
        }

        return JsonResults.Ok(new BootstrapNotifyResponse
        {
            Updated = true,
            LastPulledAtUtc = cursor,
        });
    }
}
