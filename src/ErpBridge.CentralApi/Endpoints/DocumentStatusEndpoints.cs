using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>GET /api/v1/ingest/jobs/status?externalIds=…</c>: the phone asks where the documents it sent stand in
/// the ERP — written with its series and number, waiting, to be retried, or failed with the reason (goal ERP
/// yazım Y4d). Same authentication as <c>POST /api/v1/ingest/jobs</c>; the tenant comes from the token.
/// </summary>
public static class DocumentStatusEndpoints
{
    /// <summary>Most ids one call may ask about; the phone pages its outbox.</summary>
    public const int MaxIds = 100;

    public static IEndpointRouteBuilder MapDocumentStatusEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGroup("/api/v1/ingest").WithTags("Ingest")
            .MapGet("/jobs/status", StatusAsync)
            .WithName("IngestJobsStatus")
            .Produces<DocumentStatusResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(Program.AgentOrApiKeyPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);
        return routes;
    }

    private static async Task<IResult> StatusAsync(
        [FromQuery(Name = "externalIds")] string[]? externalIds,
        HttpContext http,
        CentralApiDbContext db,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });
        }

        // Repeated (?externalIds=a&externalIds=b) or comma-separated (?externalIds=a,b).
        var ids = (externalIds ?? [])
            .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (ids.Count == 0 || ids.Count > MaxIds)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError
            {
                ErrorCode = "INVALID_EXTERNAL_IDS",
                Message = $"externalIds must name 1 to {MaxIds} documents.",
            });
        }

        var jobs = await db.Jobs.AsNoTracking()
            .Where(j => j.TenantId == tenantId && ids.Contains(j.ExternalId))
            .Select(j => new { j.Id, j.ExternalId, j.DocumentType, j.Status, j.RetryCount, j.LastError, j.NextAttemptAtMs })
            .ToListAsync(ct);
        var jobIds = jobs.Select(j => j.Id).ToList();
        var acks = (await db.JobAcks.AsNoTracking()
                .Where(a => jobIds.Contains(a.JobId))
                .Select(a => new { a.JobId, a.ErrorCode, a.ErrorMessage, a.ErpDocumentSeries, a.ErpDocumentNumber, a.AckedAtUtc })
                .ToListAsync(ct))
            // DateTimeOffset ordering is done here: SQLite cannot order by it.
            .GroupBy(a => a.JobId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.AckedAtUtc).First());

        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var documents = jobs
            .OrderBy(j => ids.IndexOf(j.ExternalId))
            .Select(j =>
            {
                acks.TryGetValue(j.Id, out var ack);
                var retrying = j.Status == JobStatus.Pending && j.NextAttemptAtMs is { } next && next > nowMs;
                var state = j.Status switch
                {
                    JobStatus.Succeeded => "written",
                    JobStatus.Failed or JobStatus.DeadLetter => "failed",
                    _ when retrying => "retrying",
                    _ => "pending",
                };
                return new DocumentStatusDto
                {
                    ExternalId = j.ExternalId,
                    DocumentType = j.DocumentType,
                    State = state,
                    ErpDocumentNo = state == "written" ? DocumentNo(ack?.ErpDocumentSeries, ack?.ErpDocumentNumber) : null,
                    ErrorCode = state is "failed" or "retrying" ? ack?.ErrorCode : null,
                    Message = state is "failed" or "retrying" ? ack?.ErrorMessage ?? j.LastError : null,
                    Attempt = j.RetryCount,
                    NextAttemptAtMs = retrying ? j.NextAttemptAtMs : null,
                };
            })
            .ToList();
        return Results.Ok(new DocumentStatusResponse { Documents = documents });
    }

    private static string? DocumentNo(string? series, int? number) => number switch
    {
        null => null,
        _ when string.IsNullOrWhiteSpace(series) => number.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
        _ => $"{series.Trim()}-{number.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
    };
}
