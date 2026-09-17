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

        group.MapGet("/failures", ListFailuresAsync)
            .WithName("AdminJobFailuresList")
            .Produces<JobFailureDto[]>(StatusCodes.Status200OK);

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

        var detail = ToDetail(job);
        // Ordered here: SQLite (tests) cannot order by DateTimeOffset.
        var acks = (await db.JobAcks.AsNoTracking().Where(a => a.JobId == job.Id).ToListAsync(ct))
            .OrderByDescending(a => a.AckedAtUtc)
            .ToList();
        detail.Acks = acks.Select(a => new JobAckDto
        {
            Status = a.Status,
            ErrorCode = a.ErrorCode,
            ErrorMessage = a.ErrorMessage,
            ErpDocumentNo = ErpDocumentStates.DocumentNo(a.ErpDocumentSeries, a.ErpDocumentNumber),
            AckedAtUtc = a.AckedAtUtc,
        }).ToList();
        var last = acks.FirstOrDefault();
        detail.Retryable = last is null ? null : string.Equals(last.Status, "retry", StringComparison.OrdinalIgnoreCase);
        detail.LastErrorCode = last?.ErrorCode;
        detail.ErpDocumentNo = job.Status == JobStatus.Succeeded ? ErpDocumentStates.DocumentNo(last?.ErpDocumentSeries, last?.ErpDocumentNumber) : null;

        // What an agent would be told if it took the job now (goal ERP yazım Y5b).
        if (await db.Tenants.AsNoTracking().AnyAsync(t => t.Id == job.TenantId && t.DataSource == TenantDataSources.Erp, ct))
        {
            var settings = await db.ErpWriteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == job.TenantId, ct);
            var creator = job.CreatedByUserId;
            var mapping = creator is { } userId
                ? await db.MobileUserErpMappings.AsNoTracking().FirstOrDefaultAsync(m => m.TenantId == job.TenantId && m.UserId == userId, ct)
                : null;
            var username = creator is { } uid
                ? await db.MobileUsers.AsNoTracking().Where(u => u.TenantId == job.TenantId && u.Id == uid).Select(u => u.Username).FirstOrDefaultAsync(ct)
                : null;
            detail.ErpContext = ErpBridge.CentralApi.ErpWrite.ErpWriteContextBuilder.Build(settings, mapping, username);
        }
        return JsonResults.Ok(detail);
    }

    private static async Task<IResult> ListFailuresAsync(
        [FromQuery] Guid? tenantId,
        [FromQuery] int? take,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        var takeClamped = Math.Clamp(take ?? DefaultTake, 1, MaxTake);
        var query = db.JobAcks.AsNoTracking()
            .Include(ack => ack.Job)
            .Where(ack => ack.Status == "failed");

        if (tenantId.HasValue)
            query = query.Where(ack => ack.Job!.TenantId == tenantId.Value);

        var rows = await query.OrderByDescending(ack => ack.AckedAtUtc).Take(takeClamped).ToListAsync(ct);
        return JsonResults.Ok(rows.Where(ack => ack.Job is not null).Select(ack => new JobFailureDto
        {
            JobId = ack.JobId,
            TenantId = ack.Job!.TenantId,
            ExternalId = ack.Job.ExternalId,
            DocumentType = ack.Job.DocumentType,
            ErrorCode = ack.ErrorCode,
            ErrorMessage = ack.ErrorMessage,
            OccurredAtUtc = ack.AckedAtUtc,
        }).ToArray());
    }

    private static async Task<IResult> RetryAsync(
        Guid id,
        [FromServices] CentralApiDbContext db,
        [FromServices] ErpBridge.CentralApi.Warehouse.FulfillmentService warehouse,
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
        job.LeasedUntilMs = null;
        job.NextAttemptAtMs = null;
        // A retried order is waiting for the ERP again (Faz 47).
        bool orderChanged;
        await using (var transaction = db.Database.IsRelational() && ErpBridge.CentralApi.Warehouse.FulfillmentService.IsQueuedDocument(job.DocumentType)
            ? await db.Database.BeginTransactionAsync(ct)
            : null)
        {
            orderChanged = await warehouse.RecordErpResultAsync(db, job, ct);
            await db.SaveChangesAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);
        }
        if (orderChanged) warehouse.Notify(job.TenantId);
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
        NextAttemptAtUtc = Instant(j.NextAttemptAtMs),
        LeasedUntilUtc = Instant(j.LeasedUntilMs),
    };

    private static DateTimeOffset? Instant(long? unixMs) => unixMs is { } ms ? DateTimeOffset.FromUnixTimeMilliseconds(ms) : null;

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
        NextAttemptAtUtc = Instant(j.NextAttemptAtMs),
        LeasedUntilUtc = Instant(j.LeasedUntilMs),
        CreatedByUserId = j.CreatedByUserId,
    };
}
