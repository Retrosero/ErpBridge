using System.Text.Json;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>POST /api/v1/ingest/jobs</c>. Customer backend / mobile app calls
/// this with their API key + tenant id to enqueue a job for the Windows
/// agent. Idempotent on (tenantId, documentType, externalId) — re-enqueuing
/// the same triple returns the existing job rather than creating a duplicate.
/// </summary>
public static class IngestEndpoints
{
    /// <summary>Hard upper bound on the serialized payload. Larger payloads get a 413 before persistence.</summary>
    private const int MaxPayloadBytes = 256 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IEndpointRouteBuilder MapIngestEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/ingest").WithTags("Ingest");

        group.MapPost("/jobs", IngestAsync)
            .WithName("IngestJobs")
            .Produces<IngestJobResponse>(StatusCodes.Status200OK)
            .Produces<IngestJobResponse>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status413PayloadTooLarge)
            .RequireAuthorization(Program.ApiKeyPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        return routes;
    }

    private static async Task<IResult> IngestAsync(
        [FromBody] IngestJobRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        // ---- 1. Body validation ----
        if (body is null)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });
        if (string.IsNullOrWhiteSpace(body.ExternalId))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_EXTERNAL_ID", Message = "externalId is required." });
        if (string.IsNullOrWhiteSpace(body.DocumentType))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_DOCUMENT_TYPE", Message = "documentType is required." });

        // ---- 2. Tenant from token, never from body. ----
        if (!http.User.TryGetTenantId(out var tenantId))
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });

        // ---- 3. Tenant must be active. ----
        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, ct);
        if (tenant is null || !tenant.IsActive)
            return JsonResults.Status(StatusCodes.Status403Forbidden,
                new ApiError { ErrorCode = "TENANT_INACTIVE", Message = "Tenant is inactive." });

        // ---- 4. Serialize payload and enforce size cap before persistence. ----
        string payloadJson;
        try
        {
            payloadJson = body.Payload is null
                ? "{}"
                : JsonSerializer.Serialize(body.Payload, JsonOptions);
        }
        catch (NotSupportedException ex)
        {
            return JsonResults.Status(StatusCodes.Status400BadRequest,
                new ApiError { ErrorCode = "INVALID_PAYLOAD", Message = $"payload could not be serialized: {ex.Message}" });
        }
        if (System.Text.Encoding.UTF8.GetByteCount(payloadJson) > MaxPayloadBytes)
            return JsonResults.Status(StatusCodes.Status413PayloadTooLarge,
                new ApiError { ErrorCode = "PAYLOAD_TOO_LARGE", Message = $"Payload exceeds {MaxPayloadBytes} bytes." });

        // ---- 5. Idempotent insert. The unique index on
        //         (TenantId, DocumentType, ExternalId) backs this up; we
        //         also do an explicit lookup so we can return the existing
        //         job id rather than rely on a 500 from the unique
        //         violation. ----
        var existing = await db.Jobs.AsNoTracking()
            .FirstOrDefaultAsync(j =>
                j.TenantId == tenantId &&
                j.DocumentType == body.DocumentType &&
                j.ExternalId == body.ExternalId, ct);

        if (existing is not null)
        {
            return JsonResults.Ok(new IngestJobResponse
            {
                JobId = existing.Id,
                TenantId = existing.TenantId,
                ExternalId = existing.ExternalId,
                DocumentType = existing.DocumentType,
                Status = existing.Status.ToString(),
                Idempotent = true,
            });
        }

        var job = new Job
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExternalId = body.ExternalId,
            DocumentType = body.DocumentType,
            PayloadJson = payloadJson,
            Status = JobStatus.Pending,
            EnqueuedAtUtc = DateTimeOffset.UtcNow,
        };
        db.Jobs.Add(job);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // A concurrent insert beat us. Re-read the winner and return it.
            var winner = await db.Jobs.AsNoTracking()
                .FirstOrDefaultAsync(j =>
                    j.TenantId == tenantId &&
                    j.DocumentType == body.DocumentType &&
                    j.ExternalId == body.ExternalId, ct);
            if (winner is not null)
            {
                return JsonResults.Ok(new IngestJobResponse
                {
                    JobId = winner.Id,
                    TenantId = winner.TenantId,
                    ExternalId = winner.ExternalId,
                    DocumentType = winner.DocumentType,
                    Status = winner.Status.ToString(),
                    Idempotent = true,
                });
            }
            throw;
        }

        return JsonResults.Status(StatusCodes.Status201Created, new IngestJobResponse
        {
            JobId = job.Id,
            TenantId = job.TenantId,
            ExternalId = job.ExternalId,
            DocumentType = job.DocumentType,
            Status = job.Status.ToString(),
            Idempotent = false,
        });
    }
}