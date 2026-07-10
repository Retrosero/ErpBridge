using System.Text.Json;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>POST /api/v1/bootstrap</c>. Persists the caller's reference-data
/// snapshot as a <see cref="BootstrapPackage"/> row scoped to the tenant from
/// the JWT.
/// </summary>
public static class BootstrapEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Register an <see cref="IEndpointRouteBuilder"/> extension that maps the bootstrap endpoint.</summary>
    public static IEndpointRouteBuilder MapBootstrapEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1/bootstrap", BootstrapAsync)
            .WithName("Bootstrap")
            .WithTags("Bootstrap")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(Program.AgentPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);
        return routes;
    }

    private static async Task<IResult> BootstrapAsync(
        [FromBody] BootstrapRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (body is null) return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });
        if (string.IsNullOrWhiteSpace(body.SourceDatabase))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_SOURCE", Message = "sourceDatabase is required." });

        if (!http.User.TryGetTenantId(out var tenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "JWT missing tenant claim." });

        var payloadJson = body.Payload is null
            ? "{}"
            : JsonSerializer.Serialize(body.Payload, JsonOptions);

        var package = new BootstrapPackage
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PayloadJson = payloadJson,
            SourceDatabase = body.SourceDatabase,
            PulledAtUtc = body.PulledAtUtc,
            ReceivedAtUtc = DateTimeOffset.UtcNow,
        };
        db.BootstrapPackages.Add(package);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}
