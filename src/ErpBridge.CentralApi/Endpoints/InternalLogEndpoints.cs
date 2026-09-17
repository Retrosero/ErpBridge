using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
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

    /// <summary>Largest accepted body. 200 events at the column bounds fit comfortably; anything bigger is refused unread.</summary>
    public const long MaxBodyBytes = 1024 * 1024;

    private static async Task<IResult> IngestAsync(
        HttpContext http,
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

        // The body is read only now — after the key — and bounded: this route is anonymous at the framework level.
        if (http.Request.ContentLength > MaxBodyBytes)
            return JsonResults.Status(StatusCodes.Status413PayloadTooLarge, new ApiError { ErrorCode = "BATCH_TOO_LARGE", Message = "Log batch body is too large." });
        if (http.Features.Get<IHttpMaxRequestBodySizeFeature>() is { IsReadOnly: false } limit)
            limit.MaxRequestBodySize = MaxBodyBytes;

        InternalLogBatch? body;
        try
        {
            body = await JsonSerializer.DeserializeAsync<InternalLogBatch>(http.Request.Body, Json, ct);
        }
        catch (Exception ex) when (ex is JsonException or BadHttpRequestException)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BATCH", Message = "Body is not a log batch." });
        }

        if (body is null || !AllowedSources.Contains(body.Source))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_SOURCE", Message = "source must be portal or admin." });
        // System.Text.Json accepts null for non-nullable members; check what was actually sent.
        if (body.Events is not { Count: > 0 } events || events.Count > InternalLogContract.MaxBatch)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BATCH", Message = $"A batch holds 1-{InternalLogContract.MaxBatch} events." });

        var inputs = events
            .Where(e => e?.EventId is { Length: > 0 and <= 64 } id && Guid.TryParse(id, out _))
            .Select(e => ShippedLogMapping.ToInput(body.Source, e.EventId, e.OccurredAtUtc, e.Severity, e.Category, e.Message,
                e.ExceptionType, e.StackTrace, e.AppVersion, e.Properties))
            .ToList();
        var result = inputs.Count == 0 ? new LogWriteResult(0, 0) : await writer.WriteAsync(inputs, ct);
        return JsonResults.Status(StatusCodes.Status202Accepted, new { accepted = result.Accepted, duplicate = events.Count - result.Accepted });
    }

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
}
