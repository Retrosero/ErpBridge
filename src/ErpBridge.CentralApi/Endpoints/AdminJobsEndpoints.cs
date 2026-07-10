using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/admin/jobs</c>: list with status/take filters, get one
/// (payload included), and retry. Admin-only. The retry endpoint resets a
/// job from <see cref="JobStatus.Processing"/> or terminal state back to
/// <see cref="JobStatus.Pending"/> while incrementing <c>RetryCount</c>.
/// </summary>
public static class AdminJobsEndpoints
{
    private const int DefaultTake = 50;
    private const int MaxTake = 200;

    public static IEndpointRouteBuilder MapAdminJobsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/admin/jobs")
            .WithTags("Admin/Jobs")
            .RequireAuthorization(Program.AdminPolicy)
            .RequireRateLimiting(Program.PerAdminRateLimitPolicy);

        group.MapGet("/", ListAsync)
            .WithName("AdminJobsList")
            .Produces<JobDto[]>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", DetailAsync)
            .WithName("AdminJobsDetail")
            .Produces<JobDetailDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/retry", RetryAsync)
            .WithName("AdminJobsRetry")
            .Produces<JobDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status400BadRequest);

        return routes;
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] string? status,
        [FromQuery] int? take,
        [FromQuery] Guid? tenantId,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var takeClamped = Math.Clamp(take ?? DefaultTake, 1, MaxTake);
        var query = db.Jobs.AsNoTracking();
        if (tenantId.HasValue) query = query.Where(j => j.TenantId == tenantId.Value);
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!TryParseStatus(status, out var parsed))
            {
                return JsonResults.Status(StatusCodes.Status400BadRequest,
                    new ApiError { ErrorCode = "INVALID_STATUS", Message = "status must be one of: pending, processing, succeeded, failed, deadLetter." });
            }
            query = query.Where(j => j.Status == parsed);
        }
        var rows = await query.OrderByDescending(j => j.EnqueuedAtUtc).Take(takeClamped).ToListAsync(ct);
        return JsonResults.Ok(rows.Select(ToDto).ToArray());
    }

    private static async Task<IResult> DetailAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id, ct);
        if (job is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "JOB_NOT_FOUND", Message = "Job not found." });
        return JsonResults.Ok(ToDetail(job));
    }

    private static async Task<IResult> RetryAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == id, ct);
        if (job is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "JOB_NOT_FOUND", Message = "Job not found." });

        // Reset to Pending regardless of the previous state. Increment retry
        // count so the next lease is observable in metrics; clear LastError so
        // stale failure context doesn't leak into the new lease lifecycle.
        job.Status = JobStatus.Pending;
        job.RetryCount += 1;
        job.LastError = null;
        job.CompletedAtUtc = null;
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(ToDto(job));
    }

    private static bool TryParseStatus(string raw, out JobStatus status)
    {
        switch (raw.Trim().ToLowerInvariant())
        {
            case "pending": status = JobStatus.Pending; return true;
            case "processing": status = JobStatus.Processing; return true;
            case "succeeded": status = JobStatus.Succeeded; return true;
            case "failed": status = JobStatus.Failed; return true;
            case "deadletter": status = JobStatus.DeadLetter; return true;
            default: status = JobStatus.Pending; return false;
        }
    }

    private static JobDto ToDto(Job j) => new()
    {
        Id = j.Id,
        TenantId = j.TenantId,
        ExternalId = j.ExternalId,
        DocumentType = j.DocumentType,
        Status = j.Status.ToString(),
        RetryCount = j.RetryCount,
        LastError = j.LastError,
        EnqueuedAtUtc = j.EnqueuedAtUtc,
        CompletedAtUtc = j.CompletedAtUtc,
    };

    private static JobDetailDto ToDetail(Job j) => new()
    {
        Id = j.Id,
        TenantId = j.TenantId,
        ExternalId = j.ExternalId,
        DocumentType = j.DocumentType,
        Status = j.Status.ToString(),
        RetryCount = j.RetryCount,
        LastError = j.LastError,
        EnqueuedAtUtc = j.EnqueuedAtUtc,
        CompletedAtUtc = j.CompletedAtUtc,
        PayloadJson = j.PayloadJson,
    };
}
