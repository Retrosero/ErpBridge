using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Portal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// The portal's list of documents sent to the ERP (goal ERP yazım Y5a): type, sender, customer, state, the ERP
/// document number, the agent's reason and attempts. Administrators, managers and accounting read it; only an
/// administrator sends a failed document to the agent again. A company without an ERP gets 409
/// <c>ERP_NOT_CONNECTED</c>.
/// </summary>
public static class PortalErpDocumentsEndpoints
{
    public const int MaxPageSize = 100;
    public const int MaxRangeDays = 31;

    public static IEndpointRouteBuilder MapPortalErpDocumentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapGet("/erp-documents", ListAsync).WithName("PortalErpDocuments");
        group.MapPost("/erp-documents/{jobId:guid}/retry", RetryAsync).WithName("PortalRetryErpDocument");
        return routes;
    }

    private static async Task<IResult> ListAsync(
        HttpContext http, [FromServices] CentralApiDbContext db,
        string? from, string? to, string? state, string? documentType, Guid? userId, string? customer, int? page, int? pageSize,
        CancellationToken ct)
    {
        var (tenant, user, error) = await AuthorizeAsync(http, db, RolePermissions.CanViewLedger, requireAdmin: false, ct);
        if (error is not null) return error;

        var today = PortalReports.IstanbulDay(DateTimeOffset.UtcNow);
        if (!TryDay(from, today.AddDays(-6), out var start)) return Invalid("from must be yyyy-MM-dd.");
        if (!TryDay(to, today, out var end)) return Invalid("to must be yyyy-MM-dd.");
        if (end < start) return Invalid("to is before from.");
        if (end.DayNumber - start.DayNumber + 1 > MaxRangeDays) return Invalid($"At most {MaxRangeDays} days.");
        var wantedState = state?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(wantedState) && !ErpDocumentStates.All.Contains(wantedState))
            return Invalid("state must be pending, retrying, written or failed.");
        var size = pageSize ?? 50;
        if (size is < 1 or > MaxPageSize) return Invalid($"pageSize must be 1 to {MaxPageSize}.");
        var pageNo = page ?? 1;
        if (pageNo < 1) return Invalid("page must be 1 or more.");

        var windowStart = PortalReports.IstanbulDayStartUtc(start);
        var windowEnd = PortalReports.IstanbulDayStartUtc(end.AddDays(1));
        var query = db.Jobs.AsNoTracking().Where(j => j.TenantId == tenant!.Id);
        if (!string.IsNullOrEmpty(wantedState))
        {
            var statuses = ErpDocumentStates.Statuses(wantedState);
            query = query.Where(j => statuses.Contains(j.Status));
        }
        if (!string.IsNullOrWhiteSpace(documentType))
        {
            var type = documentType.Trim();
            query = query.Where(j => j.DocumentType == type);
        }
        if (userId is { } sender) query = query.Where(j => j.CreatedByUserId == sender);
        // PostgreSQL filters the window itself; SQLite (tests) cannot compare DateTimeOffset, so there it is applied after reading.
        var sqlite = db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
        if (!sqlite) query = query.Where(j => j.EnqueuedAtUtc >= windowStart && j.EnqueuedAtUtc < windowEnd);

        var rows = await query
            .Select(j => new { j.Id, j.ExternalId, j.DocumentType, j.CreatedByUserId, j.PayloadJson, j.Status, j.RetryCount, j.LastError, j.NextAttemptAtMs, j.EnqueuedAtUtc, j.CompletedAtUtc })
            .ToListAsync(ct);

        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var customerFilter = customer?.Trim();
        var matching = rows
            .Where(r => r.EnqueuedAtUtc >= windowStart && r.EnqueuedAtUtc < windowEnd)
            .Select(r => new
            {
                Row = r,
                State = ErpDocumentStates.Of(r.Status, r.NextAttemptAtMs, nowMs),
                CustomerCode = PortalReports.ReadString(r.PayloadJson, "customerCode"),
                CustomerName = PortalReports.ReadString(r.PayloadJson, "counterparty"),
            })
            .Where(r => string.IsNullOrEmpty(wantedState) || r.State == wantedState)
            .Where(r => string.IsNullOrEmpty(customerFilter)
                || (r.CustomerCode?.Contains(customerFilter, StringComparison.OrdinalIgnoreCase) ?? false)
                || (r.CustomerName?.Contains(customerFilter, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderByDescending(r => r.Row.EnqueuedAtUtc)
            .ToList();
        var pageRows = matching.Skip((pageNo - 1) * size).Take(size).ToList();

        var jobIds = pageRows.Select(r => r.Row.Id).ToList();
        var acks = (await db.JobAcks.AsNoTracking()
                .Where(a => jobIds.Contains(a.JobId))
                .Select(a => new { a.JobId, a.ErrorCode, a.ErrorMessage, a.ErpDocumentSeries, a.ErpDocumentNumber, a.AckedAtUtc })
                .ToListAsync(ct))
            .GroupBy(a => a.JobId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.AckedAtUtc).First());
        var senderIds = pageRows.Select(r => r.Row.CreatedByUserId).OfType<Guid>().Distinct().ToList();
        var names = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenant!.Id && senderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName, ct);
        var admin = RolePermissions.IsAdmin(user!);

        return JsonResults.Ok(new PortalErpDocumentsResponse
        {
            Total = matching.Count,
            Page = pageNo,
            PageSize = size,
            Items = pageRows.Select(r =>
            {
                acks.TryGetValue(r.Row.Id, out var ack);
                var failedOrRetrying = r.State is ErpDocumentStates.Failed or ErpDocumentStates.Retrying;
                return new PortalErpDocumentDto
                {
                    JobId = r.Row.Id,
                    ExternalId = r.Row.ExternalId,
                    DocumentType = r.Row.DocumentType,
                    UserId = r.Row.CreatedByUserId,
                    UserName = r.Row.CreatedByUserId is { } id && names.TryGetValue(id, out var name) ? name : null,
                    CustomerCode = r.CustomerCode,
                    CustomerName = r.CustomerName,
                    Amount = PortalReports.ReadDecimal(r.Row.PayloadJson, "amount"),
                    State = r.State,
                    ErpDocumentNo = r.State == ErpDocumentStates.Written ? ErpDocumentStates.DocumentNo(ack?.ErpDocumentSeries, ack?.ErpDocumentNumber) : null,
                    ErrorCode = failedOrRetrying ? ack?.ErrorCode : null,
                    Message = failedOrRetrying ? ack?.ErrorMessage ?? r.Row.LastError : null,
                    Attempt = r.Row.RetryCount,
                    EnqueuedAtUtc = r.Row.EnqueuedAtUtc,
                    CompletedAtUtc = r.Row.CompletedAtUtc,
                    NextAttemptAtUtc = r.State == ErpDocumentStates.Retrying && r.Row.NextAttemptAtMs is { } next
                        ? DateTimeOffset.FromUnixTimeMilliseconds(next)
                        : null,
                    CanRetry = admin && r.State == ErpDocumentStates.Failed,
                };
            }).ToList(),
        });
    }

    /// <summary>
    /// Sends a failed document to the agent again with a fresh set of attempts. The agent's document ledger
    /// (<c>_ERPB_EVRAK_ESLESME</c>) keeps an earlier write from being booked twice.
    /// </summary>
    private static async Task<IResult> RetryAsync(
        HttpContext http, Guid jobId, [FromServices] CentralApiDbContext db,
        [FromServices] ErpBridge.CentralApi.Warehouse.FulfillmentService warehouse, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, RolePermissions.IsAdmin, requireAdmin: true, ct);
        if (error is not null) return error;

        var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.TenantId == tenant!.Id, ct);
        if (job is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "JOB_NOT_FOUND", Message = "Document not found for this company." });
        if (job.Status is not (JobStatus.Failed or JobStatus.DeadLetter))
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError { ErrorCode = "JOB_NOT_RETRYABLE", Message = "Only a failed document can be sent again." });

        job.Status = JobStatus.Pending;
        job.RetryCount = 0;
        job.NextAttemptAtMs = null;
        job.LeasedUntilMs = null;
        job.CompletedAtUtc = null;
        job.LastError = null;
        // A retried order waits for the ERP again on the warehouse queue too (as the admin retry does, Faz 47).
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
        return JsonResults.Ok(new PortalErpRetryResponse { JobId = job.Id, State = ErpDocumentStates.Pending });
    }

    private static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeAsync(
        HttpContext http, CentralApiDbContext db, Func<MobileUser, bool> allowed, bool requireAdmin, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin, ct);
        if (access.Error is not null) return access;
        if (!allowed(access.User!))
        {
            return (null, null, JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
            {
                ErrorCode = "PORTAL_REQUIRES_MANAGER",
                Message = "The ERP document list is for company administrators, managers and accounting.",
            }));
        }
        if (access.Tenant!.DataSource != TenantDataSources.Erp)
        {
            return (null, null, JsonResults.Status(StatusCodes.Status409Conflict, new ApiError
            {
                ErrorCode = "ERP_NOT_CONNECTED",
                Message = "This company keeps its books on the server; there is no ERP to write into.",
            }));
        }
        return access;
    }

    private static bool TryDay(string? value, DateOnly fallback, out DateOnly day)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            day = fallback;
            return true;
        }
        return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day);
    }

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_QUERY", Message = message });
}
