using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Native;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal/native/ledger/{key}/void</c> (GOAL_PANEL_ERPSIZ E4a): cancels a collection or
/// disbursement — the storno pattern (D2) applied to a standalone cash-book movement, never a
/// sale/purchase/return's own cari etkisi (E5's <c>document_void</c> instead, D11). Same shape as every
/// other native write endpoint — a second door into <see cref="NativeDocumentProcessor"/> via
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
    private const string RejectedErrorCode = "LEDGER_VOID_REJECTED";

    public static IEndpointRouteBuilder MapPortalNativeLedgerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/native")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/ledger/{key}/void", VoidAsync).WithName("PortalVoidNativeLedgerEntry");
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

        // Only a standalone collection/disbursement is voidable here (E4a); a sale's own row —
        // even its immediate-payment leg — belongs to E5's document_void instead (D11).
        var externalId = key.Contains('|') ? key[..key.LastIndexOf('|')] : key;
        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.TenantId == tenant!.Id && j.ExternalId == externalId, ct);
        if (job is null || !NativeDocumentProcessor.VoidableLedgerJobTypes.Contains(job.DocumentType))
            return Invalid("Only a collection or a disbursement can be cancelled here.");

        var payload = new { targetKey = key, reason };
        var audit = new PortalNativeWriteHelpers.AuditInfo(
            Entity: job.DocumentType.ToLowerInvariant(), EntityKey: row.TryGetProperty("customerCode", out var code) ? code.GetString() ?? key : key,
            Action: "void", Summary: $"İptal edildi ({(job.DocumentType == NativeDocumentProcessor.Collection ? "Tahsilat" : "Tediye")}): {reason}",
            BeforeJson: record.PayloadJson);
        return await PortalNativeWriteHelpers.BookNativeDocumentAsync(
            http, db, tenant!, user!, NativeDocumentProcessor.LedgerVoid,
            PortalNativeWriteHelpers.OperationKey("portal-ledger-void", key, body?.OperationId), payload, RejectedErrorCode, ct, audit);
    }

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_VOID", Message = message });
}
