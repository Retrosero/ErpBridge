using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Expenses;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>GET /api/v1/portal/expense-receipts</c> (GOAL_DEPOLAMA_R2 S5): the company's expense and vehicle receipts for
/// the panel's "Gider fişleri" page, newest first, each with a presigned address valid for <c>Storage:PresignMinutes</c>
/// (the page shows the photo at once; it asks <c>GET /api/v1/storage/files/{fileId}/link</c> again to open it later) and
/// its document as the server holds it, matched by the phone's document id. By upload day (Istanbul) or by document ids.
/// Who sees every receipt (<see cref="ExpenseReceipts.CanSeeAll"/>: admin, manager, accounting, the panel ledger) only.
/// </summary>
public static class PortalExpenseReceiptEndpoints
{
    public const int MaxItems = 500;
    public const int MaxDocumentIds = 200;

    public static IEndpointRouteBuilder MapPortalExpenseReceiptEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGroup("/api/v1/portal")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy)
            .MapGet("/expense-receipts", ListAsync).WithName("PortalExpenseReceipts")
            .Produces<PortalExpenseReceiptsResponse>(StatusCodes.Status200OK);
        return routes;
    }

    /// <param name="documentId">The receipts of these documents (any day); without it, <paramref name="from"/>–<paramref name="to"/>.</param>
    private static async Task<IResult> ListAsync(HttpContext http, string? from, string? to, string[]? documentId, [FromServices] CentralApiDbContext db,
        [FromServices] IObjectStore store, [FromServices] IOptions<StorageOptions> options, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        if (!ExpenseReceipts.CanSeeAll(access.User!))
            return Error(StatusCodes.Status403Forbidden, "PORTAL_REQUIRES_MANAGER", "Fişleri yalnız yönetici ve muhasebe görebilir.");
        var tenantId = access.Tenant!.Id;

        var today = PortalReports.IstanbulDay(DateTimeOffset.UtcNow);
        var start = today.AddDays(-30);
        var end = today;
        if (!string.IsNullOrWhiteSpace(from) && !TryDay(from, out start)) return Error(StatusCodes.Status400BadRequest, "INVALID_DATE", "from yyyy-MM-dd olmalı.");
        if (!string.IsNullOrWhiteSpace(to) && !TryDay(to, out end)) return Error(StatusCodes.Status400BadRequest, "INVALID_DATE", "to yyyy-MM-dd olmalı.");
        if (end < start) return Error(StatusCodes.Status400BadRequest, "INVALID_RANGE", "to is before from.");
        if (end.DayNumber - start.DayNumber + 1 > PortalReports.MaxRangeDays)
            return Error(StatusCodes.Status400BadRequest, "RANGE_TOO_LONG", $"At most {PortalReports.MaxRangeDays} days.");

        var ids = (documentId ?? []).Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()).Distinct(StringComparer.Ordinal).ToList();
        if (ids.Count > MaxDocumentIds || ids.Any(id => !ExpenseReceipts.IsDocumentId(id)))
            return Error(StatusCodes.Status400BadRequest, "INVALID_DOCUMENT_ID", $"En çok {MaxDocumentIds} geçerli belge kimliği verilebilir.");

        var query = db.ExpenseAttachments.AsNoTracking().Where(a => a.TenantId == tenantId && !a.IsDeleted);
        if (ids.Count > 0)
        {
            query = query.Where(a => ids.Contains(a.DocumentExternalId));
        }
        else
        {
            var startMs = PortalReports.IstanbulDayStartUtc(start).ToUnixTimeMilliseconds();
            var endMs = PortalReports.IstanbulDayStartUtc(end.AddDays(1)).ToUnixTimeMilliseconds();
            query = query.Where(a => a.CreatedAtMs >= startMs && a.CreatedAtMs < endMs);
        }
        var rows = await query.OrderByDescending(a => a.CreatedAtMs).ThenBy(a => a.Id).Take(MaxItems + 1).ToListAsync(ct);
        var truncated = rows.Count > MaxItems;
        if (truncated) rows.RemoveAt(rows.Count - 1);

        var fileIds = rows.Select(r => r.StoredFileId).ToList();
        var files = await db.StoredFiles.AsNoTracking()
            .Where(f => f.TenantId == tenantId && fileIds.Contains(f.Id) && f.Status == StoredFileStatuses.Active)
            .ToDictionaryAsync(f => f.Id, ct);
        var documents = await ExpenseReceipts.DocumentsAsync(db, tenantId, [.. rows.Select(r => r.DocumentExternalId).Distinct(StringComparer.Ordinal)], ct);
        var validFor = TimeSpan.FromMinutes(Math.Clamp(options.Value.PresignMinutes, 1, 60));

        var items = new List<PortalExpenseReceiptDto>(rows.Count);
        foreach (var row in rows)
        {
            if (!files.TryGetValue(row.StoredFileId, out var file)) continue;
            string? url = null;
            if (store.IsAvailable)
            {
                try
                {
                    url = (await store.PresignGetAsync(file.Bucket, file.ObjectKey, validFor, ct)).AbsoluteUri;
                }
                catch (StorageUnavailableException)
                {
                    // hata-sessiz: the list still shows the receipt; opening it asks the link endpoint, which answers 503.
                }
            }
            items.Add(new PortalExpenseReceiptDto
            {
                Id = row.Id,
                FileId = row.StoredFileId,
                DocumentId = row.DocumentExternalId,
                Kind = row.Kind,
                ContentType = row.ContentType,
                SizeBytes = row.SizeBytes,
                CreatedAtMs = row.CreatedAtMs,
                CreatedByName = row.CreatedByName,
                Url = url,
                Document = documents.GetValueOrDefault(row.DocumentExternalId),
            });
        }
        http.Response.Headers.CacheControl = "private, no-store";
        return JsonResults.Ok(new PortalExpenseReceiptsResponse
        {
            From = start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            To = end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Items = [.. items],
            Truncated = truncated,
        });
    }

    private static bool TryDay(string text, out DateOnly day) =>
        DateOnly.TryParseExact(text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day);

    private static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });
}
