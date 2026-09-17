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

    /// <summary>How long an agent owns a leased job before another lease may take it (goal ERP yazım Y1e).</summary>
    public static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(10);

    /// <summary>Leases a job gets before it is given up as failed.</summary>
    public const int MaxAttempts = 10;

    /// <summary>Wait before the next attempt after a retryable failure, by attempt number (the last repeats).</summary>
    public static readonly TimeSpan[] RetryDelays =
        [TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(8), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(60)];

    public static TimeSpan RetryDelay(int attempt) => RetryDelays[Math.Clamp(attempt, 1, RetryDelays.Length) - 1];

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
        [FromServices] IWebhookDispatcher webhooks,
        [FromServices] ErpBridge.CentralApi.Warehouse.FulfillmentService warehouse,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "JWT missing tenant claim." });

        var takeClamped = Math.Clamp(take ?? DefaultTake, 1, MaxTake);
        var now = (http.RequestServices.GetService<TimeProvider>() ?? TimeProvider.System).GetUtcNow();
        var nowMs = now.ToUnixTimeMilliseconds();

        // A job whose agent kept dying on it is given up instead of being leased forever.
        var abandoned = await db.Jobs
            .Where(j => j.TenantId == tenantId && j.Status == JobStatus.Processing
                        && j.LeasedUntilMs != null && j.LeasedUntilMs <= nowMs && j.RetryCount >= MaxAttempts)
            .ToListAsync(ct);
        foreach (var job in abandoned)
        {
            job.Status = JobStatus.Failed;
            job.CompletedAtUtc = now;
            job.LeasedUntilMs = null;
            job.LastError = $"Ajan belgeyi {MaxAttempts} denemede tamamlayamadı.";
            // Same bookkeeping as a failed ack: warehouse state in a transaction, then webhook and wake-up (PR #83 Codex).
            await CompleteAsync(db, job, "job.failed", webhooks, warehouse, ct);
        }

        var query = db.Jobs
            .Where(j => j.TenantId == tenantId
                        && ((j.Status == JobStatus.Pending && (j.NextAttemptAtMs == null || j.NextAttemptAtMs <= nowMs))
                            || (j.Status == JobStatus.Processing && j.LeasedUntilMs != null && j.LeasedUntilMs <= nowMs)));

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(j => j.DocumentType == type);

        var leased = await query
            .OrderBy(j => j.EnqueuedAtUtc)
            .Take(takeClamped)
            .ToListAsync(ct);

        // Settings are read at lease time, so a mapping fixed before a retry is what the agent gets. They are
        // read before the lease is saved: a failed read must not leave jobs Processing with no agent (PR #82 Codex).
        var contexts = new Dictionary<Guid, JobErpContextResponse>();
        if (leased.Count > 0 && await db.Tenants.AsNoTracking().AnyAsync(t => t.Id == tenantId && t.DataSource == TenantDataSources.Erp, ct))
        {
            var settings = await db.ErpWriteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
            var creatorIds = leased.Where(j => j.CreatedByUserId is not null).Select(j => j.CreatedByUserId!.Value).Distinct().ToList();
            var mappings = await db.MobileUserErpMappings.AsNoTracking()
                .Where(m => m.TenantId == tenantId && creatorIds.Contains(m.UserId)).ToDictionaryAsync(m => m.UserId, ct);
            var usernames = await db.MobileUsers.AsNoTracking()
                .Where(u => u.TenantId == tenantId && creatorIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Username, ct);
            foreach (var job in leased)
            {
                var creator = job.CreatedByUserId;
                contexts[job.Id] = ErpBridge.CentralApi.ErpWrite.ErpWriteContextBuilder.Build(
                    settings,
                    creator is { } id && mappings.TryGetValue(id, out var mapping) ? mapping : null,
                    creator is { } uid && usernames.TryGetValue(uid, out var username) ? username : null);
            }
        }

        if (leased.Count > 0)
        {
            foreach (var job in leased)
            {
                job.Status = JobStatus.Processing;
                job.RetryCount += 1;
                job.LeasedUntilMs = nowMs + (long)LeaseDuration.TotalMilliseconds;
                job.NextAttemptAtMs = null;
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
                Attempt = j.RetryCount,
                ErpContext = contexts.GetValueOrDefault(j.Id),
                CorrelationId = j.CorrelationId,
            })
            .ToList();
        return JsonResults.Ok(response);
    }

    private static async Task<IResult> AckAsync(
        [FromBody] JobAckRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        [FromServices] IWebhookDispatcher webhooks,
        [FromServices] ErpBridge.CentralApi.Warehouse.FulfillmentService warehouse,
        [FromServices] ErpBridge.CentralApi.LogCenter.ILogEventWriter logs,
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

        // An agent whose lease expired and was handed to another agent must not overwrite that lease's
        // outcome (PR #83 Codex). Agents that send no attempt keep the old behaviour.
        if (body.Attempt is { } attempt && attempt != job.RetryCount)
        {
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError
            {
                ErrorCode = "STALE_LEASE",
                Message = "This job was leased again after this attempt; the result was not applied.",
            });
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
        var clock = http.RequestServices.GetService<TimeProvider>() ?? TimeProvider.System;
        job.LeasedUntilMs = null;
        if (string.Equals(body.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
        {
            job.Status = JobStatus.Succeeded;
            job.CompletedAtUtc = DateTimeOffset.UtcNow;
            job.LastError = null;
            eventType = "job.succeeded";
        }
        else if (string.Equals(body.Status, "failed", StringComparison.OrdinalIgnoreCase)
                 && body.Retryable == true && job.RetryCount < MaxAttempts)
        {
            // The ERP was unreachable (goal ERP yazım Y1e): the same document may well go through later.
            job.Status = JobStatus.Pending;
            job.CompletedAtUtc = null;
            job.NextAttemptAtMs = clock.GetUtcNow().Add(RetryDelay(job.RetryCount)).ToUnixTimeMilliseconds();
            job.LastError = body.ErrorMessage ?? body.ErrorCode ?? "Agent reported a retryable failure.";
            ack.Status = "retry";
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

        await CompleteAsync(db, job, eventType, webhooks, warehouse, ct);
        if (ack.Status is "failed" or "retry") await LogWriteFailureAsync(logs, job, ack, agentId, http, ct);
        return Results.NoContent();
    }

    /// <summary>
    /// An ERP write the agent could not do goes to the Log Centre (goal ERP yazım Y5b), so a mapping gap or a
    /// refused document shows up with the company's other errors. Never fails the ack.
    /// </summary>
    private static async Task LogWriteFailureAsync(
        ErpBridge.CentralApi.LogCenter.ILogEventWriter logs, Job job, JobAckRecord ack, Guid agentId, HttpContext http, CancellationToken ct)
    {
        try
        {
            var retry = ack.Status == "retry";
            await logs.WriteAsync(
            [
                new ErpBridge.CentralApi.LogCenter.LogEventInput
                {
                    Source = ErpBridge.CentralApi.LogCenter.LogSources.WindowsAgent,
                    TenantId = job.TenantId,
                    AgentId = agentId,
                    UserId = job.CreatedByUserId,
                    Severity = retry ? ErpBridge.CentralApi.LogCenter.LogSeverity.Warn : ErpBridge.CentralApi.LogCenter.LogSeverity.Error,
                    Kind = retry ? "ERP_WRITE_RETRY" : "ERP_WRITE_FAILED",
                    Operation = $"erp.write.{job.DocumentType}",
                    Message = string.IsNullOrWhiteSpace(ack.ErrorCode) ? ack.ErrorMessage : $"{ack.ErrorCode}: {ack.ErrorMessage}",
                    // The job's own id, not this request's: that is what ties the failure back to the phone
                    // document that caused it. Falls back to the request for jobs booked before L3g.
                    CorrelationId = job.CorrelationId ?? ErpBridge.CentralApi.LogCenter.CorrelationId.Of(http),
                    PropertiesJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        jobId = job.Id, externalId = job.ExternalId, documentType = job.DocumentType,
                        errorCode = ack.ErrorCode, attempt = job.RetryCount,
                    }),
                },
            ], ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The log centre is best effort here: the agent's result is already saved.
        }
    }

    /// <summary>
    /// Saves a job's new state: the warehouse sees whether the order reached the ERP (V2, Faz 47) in the
    /// same transaction, then terminal webhooks go out and the warehouse page wakes up.
    /// </summary>
    /// <param name="eventType"><c>job.succeeded</c>, <c>job.failed</c>, or empty when the job is not terminal.</param>
    private static async Task CompleteAsync(
        CentralApiDbContext db, Job job, string eventType, IWebhookDispatcher webhooks,
        ErpBridge.CentralApi.Warehouse.FulfillmentService warehouse, CancellationToken ct)
    {
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
    }
}
