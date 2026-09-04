using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps the long-polling <c>GET /api/v1/bootstrap/notify</c> endpoint. The
/// endpoint blocks the request thread until either (a) a new
/// <c>BootstrapPackage</c> lands for the caller's tenant, (b) the
/// <c>wait</c> query parameter elapses, or (c) the request is aborted.
/// </summary>
/// <remarks>
/// Phase 9 — the desktop UI's <c>BootstrapSignalService</c> consumes this
/// endpoint. The agent (Windows Service) does NOT call it; the agent is
/// already pushed-to by the central API via the normal /api/v1/bootstrap
/// POST, so it has its own awareness of new data. The notify endpoint is
/// exclusively for the operator-facing WPF UI.
/// </remarks>
public static class BootstrapNotifyEndpoints
{
    /// <summary>Minimum wait accepted from the client (defensive floor).</summary>
    private const int MinWaitSeconds = 1;

    /// <summary>Maximum wait accepted from the client (matches the cloudfl are/nginx proxy budget).</summary>
    private const int MaxWaitSeconds = 60;

    /// <summary>Default wait when the caller omits <c>wait</c>.</summary>
    private const int DefaultWaitSeconds = 30;

    /// <summary>Register the long-polling endpoint.</summary>
    public static IEndpointRouteBuilder MapBootstrapNotifyEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1/bootstrap/notify", NotifyAsync)
            .WithName("BootstrapNotify")
            .WithTags("Bootstrap")
            .Produces<BootstrapNotifyResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(Program.AgentPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);
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
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "JWT missing tenant claim." });
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

        var timeout = TimeSpan.FromSeconds(requested);
        var cursor = await hub.WaitAsync(tenantId, timeout, ct).ConfigureAwait(false);
        if (cursor == DateTimeOffset.MinValue)
        {
            // Either the caller's wait elapsed or the connection was aborted.
            // 204 No Content is the canonical "nothing to say" response — the
            // WPF signal service will reconnect immediately.
            return Results.NoContent();
        }

        return JsonResults.Ok(new BootstrapNotifyResponse
        {
            Updated = true,
            LastPulledAtUtc = cursor,
        });
    }
}
