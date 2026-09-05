using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps the <c>/api/v1/agents/*</c> endpoints onto the central API. Two
/// endpoints live here:
/// <list type="bullet">
///   <item><description>POST <c>/api/v1/agents/register</c> — public. Validates a license, mints a JWT.</description></item>
///   <item><description>POST <c>/api/v1/agents/heartbeat</c> — JWT-authenticated. Records last-seen timestamps.</description></item>
/// </list>
/// </summary>
public static class AgentsEndpoints
{
    /// <summary>Register an <see cref="IEndpointRouteBuilder"/> extension that maps both endpoints.</summary>
    public static IEndpointRouteBuilder MapAgentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/agents").WithTags("Agents");

        group.MapPost("/register", RegisterAsync)
            .WithName("AgentsRegister")
            .Produces<AgentRegisterResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status410Gone)
            .Produces<ApiError>(StatusCodes.Status409Conflict)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .AllowAnonymous()
            .RequireRateLimiting("Anonymous");

        group.MapPost("/heartbeat", HeartbeatAsync)
            .WithName("AgentsHeartbeat")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(Program.AgentPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        group.MapPost("/telemetry", TelemetryAsync)
            .WithName("AgentsTelemetry")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(Program.AgentPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        return routes;
    }

    /// <summary>
    /// Resolve the (tenant, license) tuple for a license key, returning
    /// <see cref="LicenseResolution.NotFound"/> when unknown and
    /// <see cref="LicenseResolution.Expired"/> when inactive or expired.
    /// </summary>
    private static async Task<LicenseResolution> ResolveLicenseAsync(CentralApiDbContext db, string licenseKey, CancellationToken ct)
    {
        var license = await db.Licenses
            .Include(l => l.Tenant)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey, ct);

        if (license is null) return LicenseResolution.NotFound();
        if (!license.IsActive) return LicenseResolution.Expired("License is inactive.");
        if (license.Tenant is { IsActive: false }) return LicenseResolution.Expired("Tenant is inactive.");
        if (license.ExpiresAtUtc is { } exp && exp <= DateTimeOffset.UtcNow)
            return LicenseResolution.Expired("License has expired.");

        return LicenseResolution.Ok(license);
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] AgentRegisterRequest body,
        [FromServices] CentralApiDbContext db,
        [FromServices] IJwtIssuer jwt,
        CancellationToken ct)
    {
        if (body is null) return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });
        if (string.IsNullOrWhiteSpace(body.LicenseKey)) return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_LICENSE_KEY", Message = "licenseKey is required." });
        if (string.IsNullOrWhiteSpace(body.MachineId)) return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_MACHINE_ID", Message = "machineId is required." });

        var resolution = await ResolveLicenseAsync(db, body.LicenseKey, ct);
        if (resolution.Status == LicenseStatus.NotFound)
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "LICENSE_NOT_FOUND", Message = "License key not recognised." });
        if (resolution.Status == LicenseStatus.Expired)
            return JsonResults.Status(StatusCodes.Status410Gone,
                new ApiError { ErrorCode = "LICENSE_EXPIRED", Message = resolution.Reason ?? "License expired." });

        var license = resolution.License!;
        var existing = await db.Agents.FirstOrDefaultAsync(a => a.TenantId == license.TenantId && a.MachineId == body.MachineId, ct);
        Agent agent;
        if (existing is null)
        {
            var registeredDeviceCount = await db.Agents.CountAsync(a => a.TenantId == license.TenantId, ct);
            if (registeredDeviceCount >= license.Tenant!.MaxDeviceCount)
                return JsonResults.Status(StatusCodes.Status409Conflict,
                    new ApiError { ErrorCode = "DEVICE_LIMIT_REACHED", Message = "The device limit for this customer has been reached." });

            agent = new Agent
            {
                Id = Guid.NewGuid(),
                TenantId = license.TenantId,
                MachineId = body.MachineId,
                RegisteredAtUtc = DateTimeOffset.UtcNow,
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            agent = existing;
            await db.SaveChangesAsync(ct);
        }

        var issued = jwt.Issue(agent.Id, agent.TenantId);
        return JsonResults.Ok(new AgentRegisterResponse
        {
            AgentId = agent.Id,
            Jwt = issued.Token,
            TenantId = agent.TenantId,
            ExpiresAtUtc = issued.ExpiresAtUtc,
        });
    }

    private static async Task<IResult> HeartbeatAsync(
        [FromBody] AgentHeartbeatRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null) return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });

        if (!http.User.TryGetAgentId(out var tokenAgentId) || !http.User.TryGetTenantId(out var tokenTenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "JWT missing sub/tenant claims." });

        // Identity and tenant come strictly from the token — never from the
        // body. Older agents send their machine name in agentId, while newer
        // ones may send the GUID, so body values are intentionally ignored.

        var agent = await db.Agents.FirstOrDefaultAsync(a => a.Id == tokenAgentId && a.TenantId == tokenTenantId, ct);
        if (agent is null)
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "AGENT_NOT_FOUND", Message = "Agent not registered for this tenant." });

        // LastHeartbeat is a liveness measurement, not the last successful
        // sync time. A healthy idle agent must remain online in the console.
        agent.LastHeartbeatAtUtc = DateTimeOffset.UtcNow;
        agent.LastStatus = body.Status;
        agent.LastQueueDepth = body.QueueDepth;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> TelemetryAsync(
        [FromBody] AgentTelemetryRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null
            || !http.User.TryGetTenantId(out var tenantId)
            || !http.User.TryGetAgentId(out var agentId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Agent identity is required." });

        var eventId = body.EventId?.Trim();
        if (string.IsNullOrWhiteSpace(eventId) || eventId.Length > 64 || !Guid.TryParse(eventId, out _))
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "INVALID_EVENT_ID", Message = "eventId must be a GUID." });

        var agent = await db.Agents.AsNoTracking().FirstOrDefaultAsync(
            item => item.Id == agentId && item.TenantId == tenantId,
            ct);
        if (agent is null)
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "AGENT_NOT_FOUND", Message = "Agent is not registered." });

        var exists = await db.MobileTelemetryEvents.AnyAsync(
            item => item.TenantId == tenantId && item.EventId == eventId,
            ct);
        if (!exists)
        {
            db.MobileTelemetryEvents.Add(new MobileTelemetryEvent
            {
                TenantId = tenantId,
                EventId = eventId,
                OccurredAtUtc = body.OccurredAtUtc ?? DateTimeOffset.UtcNow,
                ReceivedAtUtc = DateTimeOffset.UtcNow,
                Kind = Bound(body.Kind, 32, "desktop_exception"),
                Severity = Bound(body.Severity, 16, "ERROR"),
                AppVersion = Bound(body.AppVersion, 64, "unknown"),
                // Existing shared telemetry storage uses these legacy column
                // names. They intentionally hold the Windows runtime and
                // machine identity for desktop events.
                AndroidVersion = Bound(body.WindowsVersion, 32, "Windows"),
                DeviceModel = agent.MachineId,
                Screen = "Windows Agent",
                Operation = Bound(body.Operation, 120, "unknown"),
                ExceptionType = Bound(body.ExceptionType, 160, "Exception"),
                Message = Bound(body.Message, 1000, "No message"),
                StackTrace = Bound(body.StackTrace, 4000, string.Empty),
                BreadcrumbsJson = "[]",
            });
            await db.SaveChangesAsync(ct);
        }
        return Results.NoContent();
    }

    private static string Bound(string? value, int max, string fallback)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return fallback;
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }

    private enum LicenseStatus { Ok, NotFound, Expired }

    private sealed record LicenseResolution(LicenseStatus Status, License? License, string? Reason)
    {
        public static LicenseResolution Ok(License license) => new(LicenseStatus.Ok, license, null);
        public static LicenseResolution NotFound() => new(LicenseStatus.NotFound, null, null);
        public static LicenseResolution Expired(string reason) => new(LicenseStatus.Expired, null, reason);
    }
}
