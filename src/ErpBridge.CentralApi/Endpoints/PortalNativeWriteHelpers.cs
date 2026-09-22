using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Native;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Shared by every <c>/api/v1/portal/native/*</c> card endpoint (GOAL_PANEL_ERPSIZ E1/E2): the
/// admin + native-tenant gate (D3/D4) and the one path into <see cref="NativeDocumentProcessor"/>
/// (D1) — a second door into the same booking engine the phone's <c>/api/v1/ingest/jobs</c> uses,
/// never a second engine.
/// </summary>
internal static class PortalNativeWriteHelpers
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeForNativeWriteAsync(
        HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access;
        if (!RolePermissions.CanEditNativeData(access.User!))
            return (null, null, JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError
            {
                ErrorCode = "ROLE_NOT_ALLOWED",
                Message = "Only company administrators can manage cards from the portal.",
            }));
        if (access.Tenant!.DataSource != TenantDataSources.Native)
            return (null, null, JsonResults.Status(StatusCodes.Status409Conflict, new ApiError
            {
                ErrorCode = "TENANT_IS_NOT_NATIVE",
                Message = "This company's cards are managed in its ERP, not the portal.",
            }));
        return access;
    }

    /// <summary>
    /// The job's idempotency key. A caller that wants a lost response to retry safely (rather than
    /// racing a second job, or — for a delete — failing because the row is already gone) supplies its
    /// own <paramref name="operationId"/> once per save/delete attempt and resends the same one on
    /// retry; without one a fresh key is used, so a caller-less request still works.
    /// </summary>
    public static string OperationKey(string prefix, string code, string? operationId) =>
        $"{prefix}-{code}-{(string.IsNullOrWhiteSpace(operationId) ? Guid.NewGuid().ToString("N") : operationId.Trim())}";

    /// <summary>
    /// Books one document through <see cref="NativeDocumentProcessor"/>, the same way the native
    /// branch of <c>IngestEndpoints</c> does for the phone: idempotent on
    /// (tenant, documentType, externalId), and a race with a concurrent retry resolves to whichever
    /// row committed first rather than surfacing the unique-index violation.
    /// </summary>
    public static async Task<IResult> BookNativeDocumentAsync(
        HttpContext http, CentralApiDbContext db, Tenant tenant, MobileUser user,
        string documentType, string externalId, object payload, string rejectedErrorCode, CancellationToken ct)
    {
        var existing = await db.Jobs.AsNoTracking()
            .FirstOrDefaultAsync(j => j.TenantId == tenant.Id && j.DocumentType == documentType && j.ExternalId == externalId, ct);
        if (existing is not null) return JobResult(existing, idempotent: true, StatusCodes.Status200OK);

        var job = new Job
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            ExternalId = externalId,
            DocumentType = documentType,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            Status = JobStatus.Pending,
            EnqueuedAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = user.Id,
            CorrelationId = ErpBridge.CentralApi.LogCenter.CorrelationId.Of(http),
        };

        var processor = http.RequestServices.GetRequiredService<NativeDocumentProcessor>();
        try
        {
            var booked = await processor.IngestAsync(db, tenant.Id, job, RolePermissions.IsAdmin(user), ct);
            if (booked.Status == JobStatus.Failed)
                return JsonResults.Status(StatusCodes.Status422UnprocessableEntity, new ApiError
                {
                    ErrorCode = rejectedErrorCode,
                    Message = booked.LastError ?? "The card could not be saved.",
                });
            return JobResult(booked, idempotent: false, StatusCodes.Status201Created);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            var winner = await db.Jobs.AsNoTracking()
                .FirstOrDefaultAsync(j => j.TenantId == tenant.Id && j.DocumentType == documentType && j.ExternalId == externalId, ct);
            if (winner is null) throw;
            return JobResult(winner, idempotent: true, StatusCodes.Status200OK);
        }
    }

    private static IResult JobResult(Job job, bool idempotent, int statusCode) => JsonResults.Status(statusCode, new IngestJobResponse
    {
        JobId = job.Id,
        TenantId = job.TenantId,
        ExternalId = job.ExternalId,
        DocumentType = job.DocumentType,
        Status = job.Status.ToString(),
        Idempotent = idempotent,
    });
}
