using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Expenses;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/android/expenses/{docId}/attachments</c> (GOAL_DEPOLAMA_R2 S5): receipt photos of the phone's expense
/// and vehicle maintenance documents, the task picture pattern. <c>docId</c> is the document's phone id (the cash-log id,
/// sent as <c>mobileDocumentId</c>), so a receipt may go up before its document reaches the server. <c>PUT</c> takes the
/// raw picture (JPEG/PNG/WebP, at most 2 MB) under an id the phone made — a retried upload is the same receipt — into the
/// central file store's private bucket (area <c>expense</c>, or <c>vehicle</c> for <c>?kind=vehicle_maintenance</c>).
/// <c>GET</c> streams it to whoever sees it (<see cref="ExpenseReceipts"/>) with an immutable cache header, like a task
/// picture; <c>DELETE</c> (uploader or storage manager) sends it to the trash. Every refusal of a receipt the user may
/// not see is the same 404.
/// </summary>
public static class MobileExpenseAttachmentEndpoints
{
    public const string BasePath = "/api/v1/android/expenses";

    /// <summary>The phone sends ~1600 px, a few hundred KB.</summary>
    public const int MaxBytes = 2 * 1024 * 1024;

    public const int MaxPerDocument = 5;

    public static IEndpointRouteBuilder MapMobileExpenseAttachmentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(BasePath)
            .WithTags("Expenses")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("/{docId}/attachments", ListAsync).WithName("MobileExpenseAttachments");
        group.MapPut("/{docId}/attachments/{id:guid}", UploadAsync).WithName("MobileExpenseAttachmentUpload");
        group.MapGet("/{docId}/attachments/{id:guid}", DownloadAsync).WithName("MobileExpenseAttachmentDownload");
        group.MapDelete("/{docId}/attachments/{id:guid}", DeleteAsync).WithName("MobileExpenseAttachmentDelete");
        return routes;
    }

    private static async Task<IResult> ListAsync(string docId, HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        if (!ExpenseReceipts.IsDocumentId(docId)) return InvalidDocument();
        var user = access.User!;
        var rows = await db.ExpenseAttachments.AsNoTracking()
            .Where(a => a.TenantId == access.Tenant!.Id && a.DocumentExternalId == docId && !a.IsDeleted)
            .OrderBy(a => a.CreatedAtMs).ToListAsync(ct);
        return JsonResults.Ok(new ExpenseAttachmentListResponse { Items = [.. rows.Where(r => ExpenseReceipts.CanSee(user, r)).Select(ExpenseReceipts.ToDto)] });
    }

    /// <summary>
    /// Checks come first (document id, kind, size, type, the same id, whose document it is, the per-document limit), then
    /// the file (the store reserves the company quota and commits on its own), then the receipt row. Over the quota:
    /// <c>413 STORAGE_QUOTA_EXCEEDED {usedBytes, quotaBytes}</c>; the store unreachable: <c>503 STORAGE_UNAVAILABLE</c>.
    /// </summary>
    private static async Task<IResult> UploadAsync(string docId, Guid id, string? kind, HttpContext http, [FromServices] CentralApiDbContext db,
        [FromServices] FileStore files, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        if (!ExpenseReceipts.IsDocumentId(docId)) return InvalidDocument();
        var receiptKind = string.IsNullOrWhiteSpace(kind) ? ExpenseAttachmentKinds.Expense : kind.Trim().ToLowerInvariant();
        if (!ExpenseAttachmentKinds.IsKnown(receiptKind))
            return Error(StatusCodes.Status400BadRequest, "INVALID_EXPENSE_KIND", "kind 'expense' ya da 'vehicle_maintenance' olmalı.");

        if (http.Request.ContentLength is { } declared && declared > MaxBytes) return TooLarge();
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await http.Request.Body.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + read > MaxBytes) return TooLarge();
            buffer.Write(chunk, 0, read);
        }
        var data = buffer.ToArray();
        var type = ImageBytes.MediaType(http.Request.ContentType);
        if (data.Length == 0 || !ImageBytes.ContentTypes.Contains(type) || !ImageBytes.LooksLike(type, data)) return StorageErrors.InvalidImage().ToResult(http);

        var tenantId = access.Tenant!.Id;
        var user = access.User!;
        if (await db.ExpenseAttachments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct) is { } existing)
            return SameReceipt(existing, tenantId, docId) ? JsonResults.Ok(ExpenseReceipts.ToDto(existing)) : Exists();

        // Someone else's document takes receipts only from who sees every receipt (an office user adding the paper).
        if (!ExpenseReceipts.CanSeeAll(user))
        {
            var owner = await db.Jobs.AsNoTracking().Where(j => j.TenantId == tenantId && j.ExternalId == docId).Select(j => j.CreatedByUserId).FirstOrDefaultAsync(ct);
            var others = await db.ExpenseAttachments.AsNoTracking().AnyAsync(a => a.TenantId == tenantId && a.DocumentExternalId == docId && a.CreatedByUserId != user.Id, ct);
            if ((owner is { } ownerId && ownerId != user.Id) || others)
                return Error(StatusCodes.Status403Forbidden, "EXPENSE_FORBIDDEN", "Bu belgeye fiş ekleyemezsiniz.");
        }
        var count = await db.ExpenseAttachments.CountAsync(a => a.TenantId == tenantId && a.DocumentExternalId == docId && !a.IsDeleted, ct);
        if (count >= MaxPerDocument) return LimitReached();

        var stored = await files.PutAsync(tenantId, ExpenseAttachmentKinds.AreaOf(receiptKind), ExpenseReceipts.StoredFileOwnerType, docId,
            StoredFileVariants.Original, type, data, user.Id, ct);
        if (!stored.Succeeded) return stored.Error!.ToResult(http);
        var file = stored.Value!;

        var receipt = new ExpenseAttachment
        {
            Id = id,
            TenantId = tenantId,
            DocumentExternalId = docId,
            Kind = receiptKind,
            StoredFileId = file.Id,
            ContentType = file.ContentType,
            SizeBytes = (int)file.SizeBytes,
            CreatedAtMs = file.CreatedAtMs,
            CreatedByUserId = user.Id,
            CreatedByName = Clip(user.FullName, 120),
        };
        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            // The company's counter row is the lock: two uploads to one document at once count one after the other, so
            // both cannot see four receipts and make six.
            await db.TenantStorage.Where(s => s.TenantId == tenantId).ExecuteUpdateAsync(u => u.SetProperty(s => s.UpdatedAtMs, s => s.UpdatedAtMs), ct);
            if (await db.ExpenseAttachments.CountAsync(a => a.TenantId == tenantId && a.DocumentExternalId == docId && !a.IsDeleted, ct) >= MaxPerDocument)
            {
                await transaction.RollbackAsync(ct);
                await files.PurgeAllAsync(tenantId, [file.Id], ct);
                return LimitReached();
            }
            db.ExpenseAttachments.Add(receipt);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            // The same id sent twice at the same moment: the one that landed is the answer, this file is nobody's.
            db.ChangeTracker.Clear();
            await files.PurgeAllAsync(tenantId, [file.Id], ct);
            var landed = await db.ExpenseAttachments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
            if (landed is null) throw;
            return SameReceipt(landed, tenantId, docId) ? JsonResults.Ok(ExpenseReceipts.ToDto(landed)) : Exists();
        }
        return JsonResults.Ok(ExpenseReceipts.ToDto(receipt));
    }

    private static IResult LimitReached() =>
        Error(StatusCodes.Status409Conflict, "EXPENSE_ATTACHMENT_LIMIT", $"Bir belgeye en çok {MaxPerDocument} fiş eklenebilir.");

    private static async Task<IResult> DownloadAsync(string docId, Guid id, HttpContext http, [FromServices] CentralApiDbContext db,
        [FromServices] FileStore files, [FromServices] IObjectStore store, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var receipt = await FindAsync(db, access.Tenant!.Id, docId, id, ct);
        if (receipt is null || !ExpenseReceipts.CanSee(access.User!, receipt)) return NotFound();
        return await files.FindAsync(receipt.TenantId, receipt.StoredFileId, ct) is { Status: StoredFileStatuses.Active } file
            ? await StoredFileResults.StreamAsync(http, store, file, "EXPENSE_ATTACHMENT_NOT_FOUND", "Fiş bulunamadı.", ct)
            : NotFound();
    }

    /// <summary>The receipt is marked deleted and its file goes to the trash (a user's delete: restorable for the trash period).</summary>
    private static async Task<IResult> DeleteAsync(string docId, Guid id, HttpContext http, [FromServices] CentralApiDbContext db,
        [FromServices] FileStore files, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var user = access.User!;
        var tenantId = access.Tenant!.Id;
        var receipt = await db.ExpenseAttachments.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId && a.DocumentExternalId == docId, ct);
        if (receipt is null || !ExpenseReceipts.CanSee(user, receipt)) return NotFound();
        if (receipt.IsDeleted) return Results.NoContent();
        if (!ExpenseReceipts.CanDelete(user, receipt))
            return Error(StatusCodes.Status403Forbidden, "EXPENSE_FORBIDDEN", "Bu fişi yalnız ekleyen ya da yönetici silebilir.");
        receipt.IsDeleted = true;
        receipt.DeletedAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        // The trash item (S9): the panel can bring the receipt back for the trash period.
        await StorageTrash.AddAsync(db, tenantId, ExpenseAttachmentKinds.AreaOf(receipt.Kind), StorageTrashKinds.ExpenseAttachment, docId, [receipt.StoredFileId],
            StorageTrash.RowSnapshot(receipt.Id), StorageTrashSources.User, user.Id, receipt.DeletedAtMs.Value, ct);
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
        await files.TrashAllAsync(tenantId, [receipt.StoredFileId], user.Id, ct);
        return Results.NoContent();
    }

    private static Task<ExpenseAttachment?> FindAsync(CentralApiDbContext db, Guid tenantId, string docId, Guid id, CancellationToken ct) =>
        db.ExpenseAttachments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId && a.DocumentExternalId == docId && !a.IsDeleted, ct);

    private static bool SameReceipt(ExpenseAttachment receipt, Guid tenantId, string docId) =>
        receipt.TenantId == tenantId && receipt.DocumentExternalId == docId && !receipt.IsDeleted;

    private static string Clip(string? text, int max) => text is null ? string.Empty : text.Length <= max ? text : text[..max];

    private static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });

    private static IResult InvalidDocument() =>
        Error(StatusCodes.Status400BadRequest, "INVALID_DOCUMENT_ID", "Belge kimliği harf, rakam ve .-_: içerebilir, en çok 128 karakter.");

    private static IResult TooLarge() =>
        Error(StatusCodes.Status413PayloadTooLarge, "EXPENSE_ATTACHMENT_TOO_LARGE", $"Fiş fotoğrafı en çok {MaxBytes / 1024 / 1024} MB olabilir.");

    private static IResult Exists() => Error(StatusCodes.Status409Conflict, "EXPENSE_ATTACHMENT_EXISTS", "Bu kimlikle başka bir fiş var.");

    private static IResult NotFound() => Error(StatusCodes.Status404NotFound, "EXPENSE_ATTACHMENT_NOT_FOUND", "Fiş bulunamadı.");
}
