using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Native;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal/native/ledger/{key}/void</c> (GOAL_PANEL_ERPSIZ E4a) and
/// <c>…/ledger-adjustments</c> (E4b): cancelling or manually correcting a customer's balance — the
/// storno pattern (D2) applied to standalone cash-book movements, never a sale/purchase/return's own
/// cari etkisi (E5's <c>document_void</c> instead, D11). Same shape as every other native write
/// endpoint — a second door into <see cref="NativeDocumentProcessor"/> via
/// <see cref="PortalNativeWriteHelpers"/> (D1), not a second engine.
///
/// <para>The 404/409/400 checks here are a pre-check for the common (non-racing) case, so a caller sees
/// the precise reason a plain retry would not. <see cref="NativeDocumentProcessor"/> repeats the same
/// checks inside the tenant's booking lock, so a genuine race (two void requests for the same entry at
/// once) can never double-void — the loser there is a 422 from the shared rejection path instead of the
/// precise 409 a sequential retry gets here, which is the one case this pre-check cannot promise.</para>
/// </summary>
public static class PortalNativeLedgerEndpoints
{
    private const string VoidRejectedErrorCode = "LEDGER_VOID_REJECTED";
    private const string AdjustmentRejectedErrorCode = "LEDGER_ADJUSTMENT_REJECTED";
    private const string EditRejectedErrorCode = "LEDGER_EDIT_REJECTED";
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public static IEndpointRouteBuilder MapPortalNativeLedgerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/native")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/ledger/{key}/void", VoidAsync).WithName("PortalVoidNativeLedgerEntry");
        group.MapPost("/ledger/{key}/edit", EditAsync).WithName("PortalEditNativeLedgerEntry");
        group.MapPost("/ledger-adjustments", AdjustAsync).WithName("PortalPostNativeLedgerAdjustment");
        return routes;
    }

    private static async Task<IResult> VoidAsync(
        string key, HttpContext http, [FromBody] PortalLedgerVoidRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, user, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        if (string.IsNullOrWhiteSpace(key)) return Invalid("A ledger key is required.");
        var reason = body?.Reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason)) return Invalid("A void needs a reason.");

        var record = await db.MobileRecords.AsNoTracking()
            .FirstOrDefaultAsync(r => r.TenantId == tenant!.Id && r.Entity == "customerTransactions" && r.RecordKey == key && !r.IsDeleted, ct);
        if (record?.PayloadJson is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "LEDGER_ENTRY_NOT_FOUND", Message = "No ledger entry with that key for this company." });

        using var document = System.Text.Json.JsonDocument.Parse(record.PayloadJson);
        var row = document.RootElement;
        if (row.TryGetProperty("voided", out var voided) && voided.ValueKind == System.Text.Json.JsonValueKind.True)
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError { ErrorCode = "ALREADY_VOIDED", Message = "This entry was already cancelled." });

        // Only a standalone collection/disbursement/manual adjustment is voidable here (E4a/E4b); a
        // sale's own row — even its immediate-payment leg — belongs to E5's document_void instead (D11).
        var externalId = key.Contains('|') ? key[..key.LastIndexOf('|')] : key;
        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.TenantId == tenant!.Id && j.ExternalId == externalId, ct);
        if (job is null || !NativeDocumentProcessor.VoidableLedgerJobTypes.Contains(job.DocumentType))
            return Invalid("Only a collection, a disbursement or a manual adjustment can be cancelled here.");

        var voidedKind = job.DocumentType switch
        {
            NativeDocumentProcessor.Collection => "Tahsilat",
            NativeDocumentProcessor.Disbursement => "Tediye",
            _ => "Düzeltme",
        };
        var payload = new { targetKey = key, reason };
        var audit = new PortalNativeWriteHelpers.AuditInfo(
            Entity: job.DocumentType.ToLowerInvariant(), EntityKey: row.TryGetProperty("customerCode", out var code) ? code.GetString() ?? key : key,
            Action: "void", Summary: $"İptal edildi ({voidedKind}): {reason}", BeforeJson: record.PayloadJson);
        return await PortalNativeWriteHelpers.BookNativeDocumentAsync(
            http, db, tenant!, user!, NativeDocumentProcessor.LedgerVoid,
            PortalNativeWriteHelpers.OperationKey("portal-ledger-void", key, body?.OperationId), payload, VoidRejectedErrorCode, ct, audit);
    }

    /// <summary>
    /// GOAL_PANEL_ERPSIZ E4c: void + reissue in the entry's own key's one transaction (D11). The kind
    /// (collection/disbursement/manual adjustment) never changes — only a manual adjustment's own
    /// borç/alacak direction may, since that is the one kind where the caller chooses it at all; a
    /// collection or disbursement keeps the direction its own document type already fixes.
    /// </summary>
    private static async Task<IResult> EditAsync(
        string key, HttpContext http, [FromBody] PortalLedgerEditRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, user, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        if (string.IsNullOrWhiteSpace(key)) return Invalid("A ledger key is required.");
        var voidReason = body?.VoidReason?.Trim();
        if (string.IsNullOrWhiteSpace(voidReason)) return Invalid("An edit needs a reason for the correction.");
        if (body is null || body.Amount <= 0) return Invalid("amount must be positive.");

        var record = await db.MobileRecords.AsNoTracking()
            .FirstOrDefaultAsync(r => r.TenantId == tenant!.Id && r.Entity == "customerTransactions" && r.RecordKey == key && !r.IsDeleted, ct);
        if (record?.PayloadJson is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "LEDGER_ENTRY_NOT_FOUND", Message = "No ledger entry with that key for this company." });

        using var document = System.Text.Json.JsonDocument.Parse(record.PayloadJson);
        var row = document.RootElement;
        if (row.TryGetProperty("voided", out var voided) && voided.ValueKind == System.Text.Json.JsonValueKind.True)
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError { ErrorCode = "ALREADY_VOIDED", Message = "This entry was already cancelled." });

        var externalId = key.Contains('|') ? key[..key.LastIndexOf('|')] : key;
        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.TenantId == tenant!.Id && j.ExternalId == externalId, ct);
        if (job is null || !NativeDocumentProcessor.VoidableLedgerJobTypes.Contains(job.DocumentType))
            return Invalid("Only a collection, a disbursement or a manual adjustment can be edited here.");

        var isAdjustment = job.DocumentType == NativeDocumentProcessor.LedgerAdjustment;
        var reason = body.Description?.Trim() is { Length: > 0 } d ? d : body.Reason?.Trim();
        if (isAdjustment && string.IsNullOrWhiteSpace(reason)) return Invalid("The corrected adjustment needs a reason.");

        string? occurredAt = null;
        if (!string.IsNullOrWhiteSpace(body.OccurredAt))
        {
            if (!DateOnly.TryParseExact(body.OccurredAt, "yyyy-MM-dd", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var day))
                return Invalid("occurredAt must be yyyy-MM-dd.");
            occurredAt = day.ToDateTime(new TimeOnly(12, 0)).ToString("O", CultureInfo.InvariantCulture);
        }

        var voidedKind = job.DocumentType switch
        {
            NativeDocumentProcessor.Collection => "Tahsilat",
            NativeDocumentProcessor.Disbursement => "Tediye",
            _ => "Düzeltme",
        };
        var payload = new
        {
            targetKey = key, voidReason, amount = body.Amount, debit = body.Debit,
            paymentType = body.PaymentType, description = body.Description, reason = body.Reason, occurredAt,
        };
        var audit = new PortalNativeWriteHelpers.AuditInfo(
            Entity: job.DocumentType.ToLowerInvariant(), EntityKey: row.TryGetProperty("customerCode", out var code) ? code.GetString() ?? key : key,
            Action: "edit", Summary: $"Düzenlendi ({voidedKind}): {voidReason}", BeforeJson: record.PayloadJson);
        return await PortalNativeWriteHelpers.BookNativeDocumentAsync(
            http, db, tenant!, user!, NativeDocumentProcessor.LedgerEdit,
            PortalNativeWriteHelpers.OperationKey("portal-ledger-edit", key, body.OperationId), payload, EditRejectedErrorCode, ct, audit);
    }

    private static async Task<IResult> AdjustAsync(
        HttpContext http, [FromBody] PortalLedgerAdjustmentRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, user, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        if (body is null) return Invalid("Body required.");

        var code = body.CustomerCode?.Trim();
        if (string.IsNullOrWhiteSpace(code)) return Invalid("customerCode is required.");
        if (body.Amount <= 0) return Invalid("amount must be positive.");
        var reason = body.Reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason)) return Invalid("A reason is required.");

        string? occurredAt = null;
        if (!string.IsNullOrWhiteSpace(body.OccurredAt))
        {
            if (!DateOnly.TryParseExact(body.OccurredAt, "yyyy-MM-dd", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var day))
                return Invalid("occurredAt must be yyyy-MM-dd.");
            occurredAt = day.ToDateTime(new TimeOnly(12, 0)).ToString("O", CultureInfo.InvariantCulture);
        }

        // customerCode is checked here (not left to the processor) so a typo is reported as a plain
        // 400 rather than the generic 422 the booking failure path returns for every rejection.
        var exists = await db.MobileRecords.AsNoTracking()
            .AnyAsync(r => r.TenantId == tenant!.Id && r.Entity == "customers" && r.RecordKey == code && !r.IsDeleted, ct);
        if (!exists) return Invalid("The customer does not exist.");

        var payload = new { customerCode = code, amount = body.Amount, debit = body.Debit, reason, occurredAt };
        var audit = new PortalNativeWriteHelpers.AuditInfo(
            Entity: NativeDocumentProcessor.LedgerAdjustment, EntityKey: code, Action: "create",
            Summary: $"Manuel düzeltme ({(body.Debit ? "borç" : "alacak")}): {body.Amount.ToString("0.00", Turkish)} TL — {reason}", BeforeJson: null);
        return await PortalNativeWriteHelpers.BookNativeDocumentAsync(
            http, db, tenant!, user!, NativeDocumentProcessor.LedgerAdjustment,
            PortalNativeWriteHelpers.OperationKey("portal-ledger-adjustment", code, body.OperationId), payload, AdjustmentRejectedErrorCode, ct, audit);
    }

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_LEDGER_REQUEST", Message = message });
}
