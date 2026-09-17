using System.Text.Json.Serialization;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.LogCenter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Log Merkezi L3c — <c>POST /api/v1/agents/logs/batch</c>: masked warning+ log lines from a Windows agent's outbox.
/// Company and agent come from the agent token; <c>hostKind</c> picks the source (<c>service</c> → windows_service,
/// anything else → windows_agent). Replays of a batch are harmless: event ids are deduplicated.
/// </summary>
public static class AgentLogEndpoints
{
    public const int MaxBatch = 200;

    public static IEndpointRouteBuilder MapAgentLogEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1/agents/logs/batch", IngestAsync)
            .WithName("AgentsLogBatch")
            .WithTags("Agents")
            .Produces(StatusCodes.Status202Accepted)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(Program.AgentPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);
        return routes;
    }

    private static async Task<IResult> IngestAsync(
        [FromBody] AgentLogBatchRequest? body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        [FromServices] ILogEventWriter writer,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId) || !http.User.TryGetAgentId(out var agentId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized, new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Agent identity is required." });
        if (body?.Events is not { Count: > 0 } || body.Events.Count > MaxBatch)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BATCH", Message = $"A batch holds 1-{MaxBatch} events." });

        var machine = await db.Agents.AsNoTracking().Where(a => a.Id == agentId && a.TenantId == tenantId).Select(a => a.MachineId).FirstOrDefaultAsync(ct);
        if (machine is null)
            return JsonResults.Status(StatusCodes.Status401Unauthorized, new ApiError { ErrorCode = "AGENT_NOT_FOUND", Message = "Agent is not registered." });

        var source = string.Equals(body.HostKind, "service", StringComparison.OrdinalIgnoreCase) ? LogSources.WindowsService : LogSources.WindowsAgent;
        var inputs = body.Events
            .Where(e => e.EventId is { Length: > 0 and <= 64 } id && Guid.TryParse(id, out _))
            .Select(e => new LogEventInput
            {
                EventId = e.EventId,
                Source = source,
                TenantId = tenantId,
                AgentId = agentId,
                OccurredAtUtc = e.OccurredAtUtc,
                Severity = e.Severity,
                Kind = e.Kind,
                Operation = string.IsNullOrWhiteSpace(e.Operation) ? e.Category : e.Operation,
                Message = e.Message,
                ExceptionType = e.ExceptionType,
                StackTrace = e.StackTrace,
                AppVersion = e.AppVersion,
                OsVersion = e.OsVersion,
                DeviceModel = machine,
                CorrelationId = e.CorrelationId,
                RepeatCount = e.RepeatCount,
                PropertiesJson = PropertiesJson(e),
            })
            .ToList();
        var result = inputs.Count == 0 ? new LogWriteResult(0, 0) : await writer.WriteAsync(inputs, ct);
        return JsonResults.Status(StatusCodes.Status202Accepted, new { accepted = result.Accepted, duplicate = body.Events.Count - result.Accepted });
    }

    private static string? PropertiesJson(AgentLogBatchEvent e)
    {
        var values = new Dictionary<string, string>(e.Properties ?? []);
        if (!string.IsNullOrWhiteSpace(e.Category)) values["category"] = e.Category;
        return values.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(values);
    }
}

public sealed class AgentLogBatchRequest
{
    [JsonPropertyName("hostKind")] public string? HostKind { get; set; }
    [JsonPropertyName("events")] public List<AgentLogBatchEvent> Events { get; set; } = [];
}

public sealed class AgentLogBatchEvent
{
    [JsonPropertyName("eventId")] public string? EventId { get; set; }
    [JsonPropertyName("occurredAtUtc")] public DateTimeOffset? OccurredAtUtc { get; set; }
    [JsonPropertyName("severity")] public string? Severity { get; set; }
    [JsonPropertyName("kind")] public string? Kind { get; set; }
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("operation")] public string? Operation { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("exceptionType")] public string? ExceptionType { get; set; }
    [JsonPropertyName("stackTrace")] public string? StackTrace { get; set; }
    [JsonPropertyName("correlationId")] public string? CorrelationId { get; set; }
    [JsonPropertyName("repeatCount")] public int RepeatCount { get; set; } = 1;
    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }
    [JsonPropertyName("osVersion")] public string? OsVersion { get; set; }
    [JsonPropertyName("properties")] public Dictionary<string, string>? Properties { get; set; }
}
