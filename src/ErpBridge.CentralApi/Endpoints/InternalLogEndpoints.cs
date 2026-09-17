using System.Security.Cryptography;
using System.Text;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.LogCenter;
using ErpBridge.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Log Merkezi L2d — <c>POST /api/v1/internal/logs</c>: the Portal and the Admin console ship their warning+ log
/// lines here (they have no database access). Authenticated by a shared key (<c>Logs:InternalIngestKey</c>,
/// compared in constant time); without a configured key the route answers 404 as if it did not exist.
/// </summary>
public static class InternalLogEndpoints
{
    private static readonly string[] AllowedSources = [LogSources.Portal, LogSources.Admin];

    public static IEndpointRouteBuilder MapInternalLogEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost(InternalLogContract.Route, IngestAsync)
            .WithName("InternalLogIngest")
            .WithTags("Internal")
            .AllowAnonymous()
            .ExcludeFromDescription();
        return routes;
    }

    private static async Task<IResult> IngestAsync(
        HttpContext http,
        [FromBody] InternalLogBatch? body,
        [FromServices] IConfiguration configuration,
        [FromServices] ILogEventWriter writer,
        CancellationToken ct)
    {
        var expected = configuration[InternalLogContract.KeyConfig];
        if (string.IsNullOrWhiteSpace(expected) || expected.Length < InternalLogContract.MinKeyLength)
            return Results.NotFound();
        var presented = http.Request.Headers[InternalLogContract.KeyHeader].FirstOrDefault() ?? string.Empty;
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(presented), Encoding.UTF8.GetBytes(expected)))
            return JsonResults.Status(StatusCodes.Status401Unauthorized, new ApiError { ErrorCode = "INVALID_LOG_KEY", Message = "Invalid internal log key." });

        if (body is null || !AllowedSources.Contains(body.Source))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_SOURCE", Message = "source must be portal or admin." });
        if (body.Events.Count == 0 || body.Events.Count > InternalLogContract.MaxBatch)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BATCH", Message = $"A batch holds 1-{InternalLogContract.MaxBatch} events." });

        var inputs = body.Events
            .Where(e => e.EventId.Length <= 64 && Guid.TryParse(e.EventId, out _))
            .Select(e => ShippedLogMapping.ToInput(body.Source, e.EventId, e.OccurredAtUtc, e.Severity, e.Category, e.Message,
                e.ExceptionType, e.StackTrace, e.AppVersion, e.Properties))
            .ToList();
        var result = inputs.Count == 0 ? new LogWriteResult(0, 0) : await writer.WriteAsync(inputs, ct);
        return JsonResults.Status(StatusCodes.Status202Accepted, new { accepted = result.Accepted, duplicate = body.Events.Count - result.Accepted });
    }
}
