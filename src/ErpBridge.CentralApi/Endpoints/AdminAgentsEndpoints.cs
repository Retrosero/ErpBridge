using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/admin/agents</c>: list (across all tenants), get one.
/// Admin-only. Agents query is global — admins need a tenant-agnostic view to
/// triage "this tenant's machine is offline" style questions.
/// </summary>
public static class AdminAgentsEndpoints
{
    public static IEndpointRouteBuilder MapAdminAgentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/agents")
            .WithTags("Admin/Agents")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/", ListAsync)
            .WithName("AdminAgentsList")
            .Produces<AgentDto[]>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetAsync)
            .WithName("AdminAgentsGet")
            .Produces<AgentDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        return routes;
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] Guid? tenantId,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var query = db.Agents.AsNoTracking();
        if (tenantId.HasValue) query = query.Where(a => a.TenantId == tenantId.Value);
        var rows = await query.OrderByDescending(a => a.LastHeartbeatAtUtc ?? a.RegisteredAtUtc).ToListAsync(ct);
        return JsonResults.Ok(rows.Select(ToDto).ToArray());
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var agent = await db.Agents.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
        if (agent is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "AGENT_NOT_FOUND", Message = "Agent not found." });
        return JsonResults.Ok(ToDto(agent));
    }

    private static AgentDto ToDto(Domain.Agent a) => new()
    {
        Id = a.Id,
        TenantId = a.TenantId,
        MachineId = a.MachineId,
        LicenseKey = a.LicenseKey,
        RegisteredAtUtc = a.RegisteredAtUtc,
        LastHeartbeatAtUtc = a.LastHeartbeatAtUtc,
        LastStatus = a.LastStatus,
        LastQueueDepth = a.LastQueueDepth,
    };
}
