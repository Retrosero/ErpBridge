using System.Text.Json;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Warehouse;
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
            .RequireAuthorization(Program.AgentOrApiKeyPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        // Tahsilat (Wave 4): customer backend / mobile app enqueues a
        // collection document for the agent. The route is fixed to
        // documentType="collection" so callers cannot accidentally mislabel.
        group.MapPost("/collections", IngestCollectionAsync)
            .WithName("IngestCollections")
            .Produces<IngestJobResponse>(StatusCodes.Status200OK)
            .Produces<IngestJobResponse>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status413PayloadTooLarge)
            .RequireAuthorization(Program.AgentOrApiKeyPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        // Tahsilat (Wave 4): payment order counterpart. The route is fixed to
        // documentType="payment_order".
        group.MapPost("/payment-orders", IngestPaymentOrderAsync)
            .WithName("IngestPaymentOrders")
            .Produces<IngestJobResponse>(StatusCodes.Status200OK)
            .Produces<IngestJobResponse>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status413PayloadTooLarge)
            .RequireAuthorization(Program.AgentOrApiKeyPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        // İrsaliye (Wave 4A): customer backend / mobile app enqueues a
        // dispatch-note document for the agent. The route is fixed to
        // documentType="dispatch_note" so callers cannot accidentally
        // mislabel a sevkiyat irsaliyesi as a satış siparişi.
        group.MapPost("/dispatch-notes", IngestDispatchNoteAsync)
            .WithName("IngestDispatchNotes")
            .Produces<IngestJobResponse>(StatusCodes.Status200OK)
            .Produces<IngestJobResponse>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status413PayloadTooLarge)
            .RequireAuthorization(Program.AgentOrApiKeyPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        // Fatura (Wave 4B): customer backend / mobile app enqueues an
        // invoice document for the agent. The route is fixed to
        // documentType="invoice" so callers cannot accidentally mislabel a
        // fatura as a sevkiyat irsaliyesi or a tahsilat.
        group.MapPost("/invoices", IngestInvoiceAsync)
            .WithName("IngestInvoice")
            .Produces<IngestJobResponse>(StatusCodes.Status200OK)
            .Produces<IngestJobResponse>(StatusCodes.Status201Created)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status413PayloadTooLarge)
            .RequireAuthorization(Program.AgentOrApiKeyPolicy)
            .RequireRateLimiting(Program.PerAgentRateLimitPolicy);

        return routes;
    }

    private static async Task<IResult> IngestAsync(
        [FromBody] IngestJobRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        return await IngestTypedAsync(body, http, db, forcedDocumentType: null, ct);
    }

    /// <summary>
    /// <c>POST /api/v1/ingest/collections</c> — Tahsilat (Wave 4) route. The
    /// documentType is fixed to <c>"collection"</c> so callers cannot accidentally
    /// mislabel a tahsilat job as a sales order or vice versa.
    /// </summary>
    private static Task<IResult> IngestCollectionAsync(
        [FromBody] IngestJobRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        return IngestTypedAsync(body, http, db, forcedDocumentType: "collection", ct);
    }

    /// <summary>
    /// <c>POST /api/v1/ingest/payment-orders</c> — Tahsilat (Wave 4) route. The
    /// documentType is fixed to <c>"payment_order"</c>.
    /// </summary>
    private static Task<IResult> IngestPaymentOrderAsync(
        [FromBody] IngestJobRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        return IngestTypedAsync(body, http, db, forcedDocumentType: "payment_order", ct);
    }

    /// <summary>
    /// <c>POST /api/v1/ingest/dispatch-notes</c> — İrsaliye (Wave 4A) route. The
    /// documentType is fixed to <c>"dispatch_note"</c> so callers cannot
    /// accidentally mislabel a sevkiyat irsaliyesi as a different document type.
    /// </summary>
    private static Task<IResult> IngestDispatchNoteAsync(
        [FromBody] IngestJobRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        return IngestTypedAsync(body, http, db, forcedDocumentType: "dispatch_note", ct);
    }

    /// <summary>
    /// <c>POST /api/v1/ingest/invoices</c> — Fatura (Wave 4B) route. The
    /// documentType is fixed to <c>"invoice"</c> so callers cannot accidentally
    /// mislabel a fatura as a sevkiyat irsaliyesi or a tahsilat.
    /// </summary>
    private static Task<IResult> IngestInvoiceAsync(
        [FromBody] IngestJobRequest body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        return IngestTypedAsync(body, http, db, forcedDocumentType: "invoice", ct);
    }

    /// <summary>
    /// Shared body for the three ingest routes. <paramref name="forcedDocumentType"/>
    /// (when non-null) overrides the body's <c>DocumentType</c> — used by the
    /// typed collection / payment-order routes that fix the value at the URL.
    /// </summary>
    private static async Task<IResult> IngestTypedAsync(
        IngestJobRequest body,
        HttpContext http,
        CentralApiDbContext db,
        string? forcedDocumentType,
        CancellationToken ct)
    {
        // ---- 1. Body validation ----
        if (body is null)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_BODY", Message = "Body required." });
        if (string.IsNullOrWhiteSpace(body.ExternalId))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "MISSING_EXTERNAL_ID", Message = "externalId is required." });

        // The /jobs route keeps the body's documentType; the typed Tahsilat
        // routes ignore the body value and use the route-fixed one instead.
        var documentType = forcedDocumentType ?? body.DocumentType;
        if (string.IsNullOrWhiteSpace(documentType))
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

        // ---- 4a. Documents come from the phone app. A portal session decides and reports;
        //          it never books sales or cash movements in someone's name. ----
        if (ErpBridge.CentralApi.Mobile.MobileUserAccess.IsMobileUser(http.User)
            && CentralApiClaims.ClientOf(http.User) == CentralApiClaims.PortalClient)
            return JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
            {
                ErrorCode = "PORTAL_CANNOT_SUBMIT_DOCUMENTS",
                Message = "Documents are sent from the phone app, not the web portal.",
            });

        // ---- 4a2. A warehouse-only user prepares orders on the phone; the app hides selling, and the
        //           server refuses the documents too (panel goal D2). ----
        if (ErpBridge.CentralApi.Mobile.MobileUserAccess.IsMobileUser(http.User)
            && Guid.TryParse(http.User.FindFirst("sub")?.Value, out var warehouseSenderId)
            && await db.MobileUsers.AsNoTracking().Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == warehouseSenderId && u.TenantId == tenantId, ct) is { } warehouseSender
            && RolePermissions.IsWarehouseOnlyOnPhone(warehouseSender))
            return JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
            {
                ErrorCode = "ROLE_NOT_ALLOWED",
                Message = "A warehouse-only user cannot send documents.",
            });

        // ---- 4b. An approval request waits for a company administrator. ----
        if (string.Equals(documentType, ErpBridge.CentralApi.Approvals.ApprovalService.DocumentType, StringComparison.OrdinalIgnoreCase))
        {
            if (!ErpBridge.CentralApi.Mobile.MobileUserAccess.IsMobileUser(http.User)
                || !Guid.TryParse(http.User.FindFirst("sub")?.Value, out var requesterId))
                return JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
                {
                    ErrorCode = "APPROVAL_REQUIRES_MOBILE_USER",
                    Message = "Approval requests are sent by signed-in company users.",
                });
            var requester = await db.MobileUsers.AsNoTracking().Include(u => u.Roles).FirstAsync(u => u.Id == requesterId, ct);
            var approvals = http.RequestServices.GetRequiredService<ErpBridge.CentralApi.Approvals.ApprovalService>();
            var submitted = await approvals.SubmitAsync(db, tenant, requester, body.ExternalId, payloadJson, ct);
            if (!submitted.Succeeded) return JsonResults.Status(submitted.StatusCode, submitted.Error);
            return JsonResults.Status(submitted.StatusCode, new IngestJobResponse
            {
                JobId = submitted.Value!.Id,
                TenantId = tenant.Id,
                ExternalId = submitted.Value.ExternalId,
                DocumentType = ErpBridge.CentralApi.Approvals.ApprovalService.DocumentType,
                Status = submitted.Value.Status,
                Idempotent = submitted.StatusCode == StatusCodes.Status200OK,
            });
        }

        // ---- 5. Idempotent insert. The unique index on
        //         (TenantId, DocumentType, ExternalId) backs this up; we
        //         also do an explicit lookup so we can return the existing
        //         job id rather than rely on a 500 from the unique
        //         violation. ----
        var existing = await db.Jobs.AsNoTracking()
            .FirstOrDefaultAsync(j =>
                j.TenantId == tenantId &&
                j.DocumentType == documentType &&
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

        // ---- 5b. A signed-in user cannot post around the company's approval rules.
        //          API keys and agents carry no person, so no rule applies to them. ----
        if (ErpBridge.CentralApi.Mobile.MobileUserAccess.IsMobileUser(http.User)
            && ApprovalKinds.ForDocument(documentType, payloadJson) is { } approvalKind
            && (await ErpBridge.CentralApi.Approvals.ApprovalService.RulesAsync(db, tenantId, ct)).Requires(approvalKind))
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError
            {
                ErrorCode = "APPROVAL_REQUIRED",
                Message = $"The company requires approval for {approvalKind}; send it as an approval_request.",
            });

        var job = new Job
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExternalId = body.ExternalId,
            DocumentType = documentType,
            PayloadJson = payloadJson,
            Status = JobStatus.Pending,
            EnqueuedAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = ErpBridge.CentralApi.Mobile.MobileUserAccess.IsMobileUser(http.User)
                && Guid.TryParse(http.User.FindFirst("sub")?.Value, out var senderId) ? senderId : null,
        };

        // ---- 5c. Route plans and visits are the team's, not the ERP's: booked here for
        //          every tenant, since an agent has no writer for them (Faz 39). ----
        if (ErpBridge.CentralApi.Team.TeamDocumentProcessor.DocumentTypes.Contains(documentType))
        {
            if (!ErpBridge.CentralApi.Mobile.MobileUserAccess.IsMobileUser(http.User)
                || !Guid.TryParse(http.User.FindFirst("sub")?.Value, out var callerId))
                return JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
                {
                    ErrorCode = "TEAM_DOCUMENT_REQUIRES_MOBILE_USER",
                    Message = "Route plans and visits are sent by signed-in company users.",
                });
            var caller = await db.MobileUsers.AsNoTracking().Include(u => u.Roles).FirstAsync(u => u.Id == callerId, ct);
            var team = http.RequestServices.GetRequiredService<ErpBridge.CentralApi.Team.TeamDocumentProcessor>();
            try
            {
                var booked = await team.IngestAsync(db, tenantId, job, caller, ct);
                return JsonResults.Status(StatusCodes.Status201Created, new IngestJobResponse
                {
                    JobId = booked.Id,
                    TenantId = booked.TenantId,
                    ExternalId = booked.ExternalId,
                    DocumentType = booked.DocumentType,
                    Status = booked.Status.ToString(),
                    Idempotent = false,
                });
            }
            catch (DbUpdateException)
            {
                // The same document raced in from a retry; the winner already booked it.
                db.ChangeTracker.Clear();
                var winner = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j =>
                    j.TenantId == tenantId && j.DocumentType == documentType && j.ExternalId == body.ExternalId, ct);
                if (winner is null) throw;
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
        }

        // ---- 6. A tenant without an ERP is booked here, not by an agent. ----
        if (tenant.DataSource == TenantDataSources.Native)
        {
            var processor = http.RequestServices.GetRequiredService<ErpBridge.CentralApi.Native.NativeDocumentProcessor>();
            try
            {
                var callerIsAdmin = await CallerIsAdminAsync(http, db, ct);
                Job booked;
                if (FulfillmentService.IsQueuedDocument(documentType))
                {
                    // A booked sale enters the warehouse queue in the booking's own transaction (Faz 47).
                    var warehouse = http.RequestServices.GetRequiredService<FulfillmentService>();
                    OrderFulfillment? queued;
                    await using (var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null)
                    {
                        booked = await processor.IngestAsync(db, tenantId, job, callerIsAdmin, ct);
                        queued = await warehouse.EnqueueAsync(db, tenant, booked, approvalRequestId: null, ct);
                        if (queued is not null) await db.SaveChangesAsync(ct);
                        if (transaction is not null) await transaction.CommitAsync(ct);
                    }
                    // The booking joined this transaction, so waking the phones is left to the caller.
                    if (booked.Status == JobStatus.Succeeded && db.Database.IsRelational())
                        http.RequestServices.GetRequiredService<ErpBridge.CentralApi.Notifications.IBootstrapNotificationHub>().Publish(tenantId, DateTimeOffset.UtcNow);
                    if (queued is not null) warehouse.Notify(tenantId);
                }
                else
                {
                    booked = await processor.IngestAsync(db, tenantId, job, callerIsAdmin, ct);
                }
                return JsonResults.Status(StatusCodes.Status201Created, new IngestJobResponse
                {
                    JobId = booked.Id,
                    TenantId = booked.TenantId,
                    ExternalId = booked.ExternalId,
                    DocumentType = booked.DocumentType,
                    Status = booked.Status.ToString(),
                    Idempotent = false,
                });
            }
            catch (DbUpdateException)
            {
                // The same document raced in from a retry; the winner already booked it.
                db.ChangeTracker.Clear();
                var winner = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j =>
                    j.TenantId == tenantId && j.DocumentType == documentType && j.ExternalId == body.ExternalId, ct);
                if (winner is null) throw;
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
        }

        // Product and customer cards are created in the ERP for an ERP tenant; an
        // agent has no writer for them, so they would sit in its queue forever.
        if (ErpBridge.CentralApi.Native.NativeDocumentProcessor.CardTypes.Contains(documentType))
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError
            {
                ErrorCode = "CARDS_REQUIRE_NATIVE_TENANT",
                Message = "Product and customer cards can only be created from the phone for a company without an ERP.",
            });
        if (ErpBridge.CentralApi.Native.NativeDocumentProcessor.NativeDocumentTypes.Contains(documentType))
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError
            {
                ErrorCode = "DOCUMENT_REQUIRES_NATIVE_TENANT",
                Message = "This document is booked by the central API for a company without an ERP only; an ERP agent has no writer for it.",
            });

        db.Jobs.Add(job);
        var erpWarehouse = http.RequestServices.GetRequiredService<FulfillmentService>();
        OrderFulfillment? erpQueued = null;

        try
        {
            // A sales order enters the warehouse queue before the agent writes it to the ERP (V2, Faz 47).
            await using var transaction = FulfillmentService.IsQueuedDocument(documentType) && db.Database.IsRelational()
                ? await db.Database.BeginTransactionAsync(ct)
                : null;
            erpQueued = await erpWarehouse.EnqueueAsync(db, tenant, job, approvalRequestId: null, ct);
            await db.SaveChangesAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            // A concurrent insert beat us. Re-read the winner and return it.
            db.ChangeTracker.Clear();
            var winner = await db.Jobs.AsNoTracking()
                .FirstOrDefaultAsync(j =>
                    j.TenantId == tenantId &&
                    j.DocumentType == documentType &&
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
        if (erpQueued is not null) erpWarehouse.Notify(tenantId);

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

    /// <summary>
    /// A signed-in mobile user is an administrator only if their row says so now;
    /// API keys and agents act for the whole tenant.
    /// </summary>
    private static async Task<bool> CallerIsAdminAsync(HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        if (!ErpBridge.CentralApi.Mobile.MobileUserAccess.IsMobileUser(http.User)) return true;
        if (!Guid.TryParse(http.User.FindFirst("sub")?.Value, out var userId)) return false;
        return await db.MobileUsers.AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.Roles.Any(r => r.Role == MobileUserRoles.Admin) && u.IsActive && u.DeletedAtUtc == null, ct);
    }
}