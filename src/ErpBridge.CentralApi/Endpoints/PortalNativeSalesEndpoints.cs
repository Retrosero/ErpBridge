using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Native;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal/native/sales-orders</c>, <c>…/purchase-receipts</c> and <c>…/sales-returns</c>
/// (GOAL_PANEL_ERPSIZ E5a): a company without an ERP enters a sale/purchase/return invoice from the
/// portal instead of the phone. Wraps the <c>sales_order</c>/<c>purchase_receipt</c>/<c>sales_return</c>
/// documents <see cref="NativeDocumentProcessor"/> already books for the phone (Faz 33) — a second door
/// into the same engine (D1), not a second one. Line validation (product must exist, quantity &gt; 0,
/// amount not negative) and the immediate-payment rule (Faz 33: a cash sale books its own collection in
/// the same transaction, so the open balance does not move) are unchanged, both already proven by the
/// phone's own tests.
/// </summary>
public static class PortalNativeSalesEndpoints
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public static IEndpointRouteBuilder MapPortalNativeSalesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/native")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/sales-orders", PostSaleAsync).WithName("PortalPostNativeSalesOrder");
        group.MapPost("/purchase-receipts", PostPurchaseAsync).WithName("PortalPostNativePurchaseReceipt");
        group.MapPost("/sales-returns", PostReturnAsync).WithName("PortalPostNativeSalesReturn");
        return routes;
    }

    private static Task<IResult> PostSaleAsync(
        HttpContext http, [FromBody] PortalNativeDocumentRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct) =>
        PostAsync(http, body, db, NativeDocumentProcessor.SalesOrder, "portal-sale", "Satış", linesRequired: true, ct);

    private static Task<IResult> PostPurchaseAsync(
        HttpContext http, [FromBody] PortalNativeDocumentRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct) =>
        PostAsync(http, body, db, NativeDocumentProcessor.PurchaseReceipt, "portal-purchase", "Alış", linesRequired: false, ct);

    private static Task<IResult> PostReturnAsync(
        HttpContext http, [FromBody] PortalNativeDocumentRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct) =>
        PostAsync(http, body, db, NativeDocumentProcessor.SalesReturn, "portal-return", "İade", linesRequired: true, ct);

    private static async Task<IResult> PostAsync(
        HttpContext http, PortalNativeDocumentRequest? body, CentralApiDbContext db, string documentType, string keyPrefix,
        string kindLabel, bool linesRequired, CancellationToken ct)
    {
        var (tenant, user, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        if (body is null) return Invalid("Body required.");

        var document = await BuildDocumentAsync(db, tenant!.Id, body, documentType, linesRequired, ct);
        if (document.Error is not null) return Invalid(document.Error);
        var party = document.Party!;
        var audit = new PortalNativeWriteHelpers.AuditInfo(
            Entity: documentType, EntityKey: party, Action: "create",
            Summary: $"{kindLabel}: {document.Total.ToString("0.00", Turkish)} TL — {party}", BeforeJson: null);
        return await PortalNativeWriteHelpers.BookNativeDocumentAsync(
            http, db, tenant, user!, documentType,
            PortalNativeWriteHelpers.OperationKey(keyPrefix, party, body.OperationId), document.Payload!, "SALES_DOCUMENT_REJECTED", ct, audit);
    }

    /// <summary>
    /// Validates a sale/purchase/return body and builds the document the processor books — shared by the
    /// create endpoints here and E5d's edit (<c>POST …/documents/{key}/edit</c>), so a correction is checked
    /// exactly like a new document. Returns a plain 400 message on failure.
    /// </summary>
    internal static async Task<(Dictionary<string, object?>? Payload, string? Party, decimal Total, string? Error)> BuildDocumentAsync(
        CentralApiDbContext db, Guid tenantId, PortalNativeDocumentRequest body, string documentType, bool linesRequired, CancellationToken ct)
    {
        var partyField = documentType == NativeDocumentProcessor.PurchaseReceipt ? "supplierCode" : "customerCode";
        var party = body.PartyCode?.Trim();
        if (string.IsNullOrWhiteSpace(party)) return (null, null, 0, $"{partyField} is required.");
        if (linesRequired && body.Lines.Count == 0) return (null, null, 0, "At least one line is required.");

        // The party is checked here (not left to the processor) so a typo is reported as a plain 400
        // rather than the generic 422 the booking failure path returns for every rejection — the same
        // reasoning E3a's collection/disbursement endpoints already use.
        var exists = await db.MobileRecords.AsNoTracking()
            .AnyAsync(r => r.TenantId == tenantId && r.Entity == "customers" && r.RecordKey == party && !r.IsDeleted, ct);
        if (!exists) return (null, null, 0, $"No customer or supplier with code '{party}'.");

        string? occurredAt = null;
        if (!string.IsNullOrWhiteSpace(body.OccurredAt))
        {
            if (!DateOnly.TryParseExact(body.OccurredAt, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
                return (null, null, 0, "occurredAt must be yyyy-MM-dd.");
            occurredAt = day.ToDateTime(new TimeOnly(12, 0)).ToString("O", CultureInfo.InvariantCulture);
        }

        var lines = body.Lines.Select(l => new
        {
            productCode = l.ProductCode,
            quantity = l.Quantity,
            unitPrice = l.UnitPrice,
            lineTotal = l.LineTotal,
            reason = l.Reason,
        }).ToList();
        var payload = new Dictionary<string, object?>
        {
            [partyField] = party,
            ["lines"] = lines,
            ["amount"] = body.Amount,
            ["paymentType"] = body.PaymentType,
            ["occurredAt"] = occurredAt,
            ["description"] = body.Description,
            [documentType == NativeDocumentProcessor.PurchaseReceipt ? "invoiceNo" : "mobileDocumentId"] = body.DocumentNo,
        };
        var total = body.Amount ?? body.Lines.Sum(l => l.LineTotal ?? l.Quantity * l.UnitPrice);
        return (payload, party, total, null);
    }

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_SALES_DOCUMENT", Message = message });
}
