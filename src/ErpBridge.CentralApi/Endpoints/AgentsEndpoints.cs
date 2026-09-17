using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.LogCenter;
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
        // A company without an ERP must never get an agent: its uploads would
        // overwrite the cards, stock and balances the phones created.
        if (license.Tenant!.DataSource == TenantDataSources.Native)
            return ErpBridge.CentralApi.Native.NativeTenantGuard.Rejection();

        var existing = await db.Agents.FirstOrDefaultAsync(a => a.TenantId == license.TenantId && a.MachineId == body.MachineId, ct);
        Agent agent;
        if (existing is null)
        {
            var registeredDeviceCount = await db.Agents.CountAsync(a => a.TenantId == license.TenantId, ct);
            if (registeredDeviceCount >= license.Tenant!.MaxDeviceCount)
                return JsonResults.Status(StatusCodes.Status409Conflict,
                    new ApiError { ErrorCode = "DEVICE_LIMIT_REACHED", Message = "The device limit for this customer has been reached." });

            await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
            if (db.Database.IsRelational())
            {
                // Takes the tenant row lock and re-reads the data source under it, so
                // a concurrent switch to native cannot slip in between check and insert.
                var stillErp = await db.Tenants
                    .Where(t => t.Id == license.TenantId && t.DataSource == TenantDataSources.Erp)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.NativeLockVersion, t => t.NativeLockVersion + 1), ct);
                if (stillErp == 0) return ErpBridge.CentralApi.Native.NativeTenantGuard.Rejection();
            }

            agent = new Agent
            {
                Id = Guid.NewGuid(),
                TenantId = license.TenantId,
                MachineId = body.MachineId,
                RegisteredAtUtc = DateTimeOffset.UtcNow,
            };
            db.Agents.Add(agent);
            await db.SaveChangesAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);
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
        var now = DateTimeOffset.UtcNow;
        var status = Trim(body.Status, 32);
        var appVersion = Trim(body.AppVersion, 64) ?? agent.AppVersion;
        var hostKind = Trim(body.HostKind, 16) ?? agent.HostKind;
        var syncResult = Trim(body.LastSyncResult, 32);
        var lastError = body.LastError is null ? null : Trim(LogScrubber.Scrub(body.LastError), 1000);
        // Log Merkezi L3f (D11): history only when something changed, otherwise every 15 minutes.
        var changed = status != agent.LastStatus || appVersion != agent.AppVersion || hostKind != agent.HostKind
            || syncResult != agent.LastSyncResult || lastError != agent.LastError;
        var due = agent.LastHeartbeatLoggedAtUtc is not { } logged || now - logged >= HeartbeatLogInterval;

        agent.LastHeartbeatAtUtc = now;
        agent.LastStatus = body.Status;
        agent.LastQueueDepth = body.QueueDepth;
        agent.AppVersion = appVersion;
        agent.HostKind = hostKind;
        agent.LastSyncAtUtc = body.LastSyncAtUtc ?? agent.LastSyncAtUtc;
        agent.LastSyncResult = syncResult ?? agent.LastSyncResult;
        agent.LastError = lastError;
        if (changed || due)
        {
            db.AgentHeartbeatLogs.Add(new AgentHeartbeatLog
            {
                AgentId = agent.Id, TenantId = agent.TenantId, RecordedAtUtc = now, RecordedAtMs = now.ToUnixTimeMilliseconds(),
                Status = status, QueueDepth = body.QueueDepth, AppVersion = appVersion, HostKind = hostKind,
                LastSyncAtUtc = agent.LastSyncAtUtc, LastSyncResult = agent.LastSyncResult, LastError = lastError,
            });
            agent.LastHeartbeatLoggedAtUtc = now;
        }
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> TelemetryAsync(
        [FromBody] AgentTelemetryRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        [FromServices] ILogEventWriter logWriter,
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

        // Log Merkezi L1c: log_events is the only store; a failure is a 5xx the agent retries.
        await logWriter.WriteAsync(
        [
            new LogEventInput
            {
                EventId = eventId,
                Source = LogSources.WindowsAgent,
                TenantId = tenantId,
                AgentId = agentId,
                OccurredAtUtc = body.OccurredAtUtc,
                Severity = string.IsNullOrWhiteSpace(body.Severity) ? LogSeverity.Error : body.Severity,
                Kind = string.IsNullOrWhiteSpace(body.Kind) ? "desktop_exception" : body.Kind,
                Operation = body.Operation,
                ExceptionType = body.ExceptionType,
                Message = body.Message,
                StackTrace = body.StackTrace,
                AppVersion = body.AppVersion,
                OsVersion = body.WindowsVersion,
                DeviceModel = agent.MachineId,
                CorrelationId = http.Request.Headers[CorrelationId.HeaderName].FirstOrDefault(),
            },
        ], ct);
        return Results.NoContent();
    }

    private static readonly TimeSpan HeartbeatLogInterval = TimeSpan.FromMinutes(15);

    private static string? Trim(string? value, int max)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text)) return null;
        return text.Length <= max ? text : text[..max];
    }

    private enum LicenseStatus { Ok, NotFound, Expired }

    private sealed record LicenseResolution(LicenseStatus Status, License? License, string? Reason)
    {
        public static LicenseResolution Ok(License license) => new(LicenseStatus.Ok, license, null);
        public static LicenseResolution NotFound() => new(LicenseStatus.NotFound, null, null);
        public static LicenseResolution Expired(string reason) => new(LicenseStatus.Expired, null, reason);
    }
}
