using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Webhooks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/jobs/*</c>. Two endpoints:
/// <list type="bullet">
///   <item><description>GET <c>/api/v1/jobs/pending</c> — leases pending jobs for the caller's tenant.</description></item>
///   <item><description>POST <c>/api/v1/jobs/ack</c> — records the agent's outcome (succeeded/failed) and persists a JobAckRow.</description></item>
/// </list>
/// Lease semantics: GET transitions <c>Pending</c> → <c>Processing</c> in the
/// same transaction. Re-poll returns the new state (not the leased one) so a
/// dead agent doesn't strand jobs forever — the lease expires with retries.
/// </summary>
public static class JobsEndpoints
{
    private const int DefaultTake = 50;
    private const int MaxTake = 200;

    /// <summary>Register an <see cref="IEndpointRouteBuilder"/> extension that maps both endpoints.</summary>
    public static IEndpointRouteBuilder MapJobsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/jobs").WithTags("Jobs").RequireAuthorization(Program.AgentPolicy);

        group.MapGet("/pending", PendingAsync)
            .WithName("JobsPending")
            .Produces<JobResponse[]>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        group.MapPost("/ack", AckAsync)
            .WithName("JobsAck")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        return routes;
    }

    private static async Task<IResult> PendingAsync(
        [FromQuery] int? take,
        [FromQuery] string? type,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "JWT missing tenant claim." });

        var takeClamped = Math.Clamp(take ?? DefaultTake, 1, MaxTake);
        var query = db.Jobs
            .Where(j => j.TenantId == tenantId && j.Status == JobStatus.Pending);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(j => j.DocumentType == type);

        var leased = await query
            .OrderBy(j => j.EnqueuedAtUtc)
            .Take(takeClamped)
            .ToListAsync(ct);

        if (leased.Count > 0)
        {
            foreach (var job in leased)
            {
                job.Status = JobStatus.Processing;
                job.RetryCount += 1;
            }
            await db.SaveChangesAsync(ct);
        }

        var response = leased
            .Select(j => new JobResponse
            {
                JobId = j.Id,
                ExternalId = j.ExternalId,
                DocumentType = j.DocumentType,
                Payload = j.PayloadJson,
                EnqueuedAtUtc = j.EnqueuedAtUtc,
            })
            .ToList();
        return JsonResults.Ok(response);
    }

    private static async Task<IResult> AckAsync(
        [FromBody] JobAckRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        [FromServices] IWebhookDispatcher webhooks,
        CancellationToken ct)
    {
        if (body is null) return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });
        if (body.JobId == Guid.Empty) return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_JOB_ID", Message = "jobId is required." });
        if (string.IsNullOrWhiteSpace(body.Status))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_STATUS", Message = "status is required." });

        if (!http.User.TryGetAgentId(out var agentId) || !http.User.TryGetTenantId(out var tenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "JWT missing sub/tenant claims." });

        var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == body.JobId, ct);
        if (job is null || job.TenantId != tenantId)
        {
            // If a prior ack exists, treat the call as a no-op idempotent retry and return 204.
            var existingAck = await db.JobAcks.AsNoTracking().FirstOrDefaultAsync(a => a.JobId == body.JobId, ct);
            if (existingAck is not null) return Results.NoContent();
            return JsonResults.Status(StatusCodes.Status404NotFound,
                new ApiError { ErrorCode = "JOB_NOT_FOUND", Message = "Job not found for this tenant." });
        }

        // Job row exists; if it is already in a terminal state the call is a duplicate ack.
        if (job.Status == JobStatus.Succeeded || job.Status == JobStatus.Failed || job.Status == JobStatus.DeadLetter)
        {
            return Results.NoContent();
        }

        var ack = new JobAckRecord
        {
            JobId = job.Id,
            Status = body.Status,
            ErrorCode = body.ErrorCode,
            ErrorMessage = body.ErrorMessage,
            ErpDocumentSeries = body.ErpDocumentSeries,
            ErpDocumentNumber = body.ErpDocumentNumber,
            ErpRecno = body.ErpRecno,
            ErpGuid = body.ErpGuid,
        };
        db.JobAcks.Add(ack);

        // Snapshot the fields we want to publish to webhook subscribers
        // before any further mutations on `job`. The dispatcher captures
        // these into the delivery row verbatim.
        var eventType = string.Empty;
        if (string.Equals(body.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
        {
            job.Status = JobStatus.Succeeded;
            job.CompletedAtUtc = DateTimeOffset.UtcNow;
            job.LastError = null;
            eventType = "job.succeeded";
        }
        else if (string.Equals(body.Status, "failed", StringComparison.OrdinalIgnoreCase))
        {
            job.Status = JobStatus.Failed;
            job.CompletedAtUtc = DateTimeOffset.UtcNow;
            job.LastError = body.ErrorMessage ?? body.ErrorCode ?? "Agent reported failure.";
            eventType = "job.failed";
        }
        else
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_STATUS", Message = "status must be 'succeeded' or 'failed'." });
        }

        await db.SaveChangesAsync(ct);

        // Fan out webhooks AFTER persisting the job state change so a
        // dispatcher failure can't roll back the ack. A misbehaving webhook
        // subscription must never block the agent's acknowledgement.
        if (!string.IsNullOrEmpty(eventType))
        {
            try
            {
                await webhooks.EnqueueJobTerminalAsync(job, eventType, ct);
            }
            catch (Exception)
            {
                // Telemetry-grade: webhook scheduling must not fail the ack.
                // The admin UI surfaces scheduled deliveries and any that
                // never made it past Pending.
            }
        }

        return Results.NoContent();
    }
}
