using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Native;
using ErpBridge.CentralApi.Portal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal/native/stock-counts</c> (GOAL_PANEL_ERPSIZ E6b): a company without an ERP counts stock
/// from the portal and books the difference, and can cancel a count. Wraps the phone's own <c>stock_count</c>
/// document (D1) — with one difference: the portal counts against the stock the server holds at booking time
/// (<c>againstCurrentLevel</c>, read under the tenant lock), not against a quantity a device saw offline.
/// </summary>
public static class PortalNativeStockCountsEndpoints
{
    private const string RejectedErrorCode = "STOCK_COUNT_REJECTED";

    public static IEndpointRouteBuilder MapPortalNativeStockCountsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/native")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/stock-counts", PostAsync).WithName("PortalPostNativeStockCount");
        group.MapPost("/stock-counts/{key}/void", VoidAsync).WithName("PortalVoidNativeStockCount");
        return routes;
    }

    private static async Task<IResult> PostAsync(
        HttpContext http, [FromBody] PortalStockCountRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, user, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        if (body is null || body.Lines.Count == 0) return Invalid("At least one line is required.");
        var reason = body.Reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason)) return Invalid("A stock count needs a reason.");
        if (body.Lines.Any(l => string.IsNullOrWhiteSpace(l.ProductCode))) return Invalid("Every line needs a productCode.");
        if (body.Lines.Any(l => l.CountedQuantity < 0)) return Invalid("A counted quantity cannot be negative.");
        if (body.Lines.GroupBy(l => l.ProductCode.Trim(), StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
            return Invalid("A product is counted once per count.");

        string? occurredAt = null;
        if (!string.IsNullOrWhiteSpace(body.OccurredAt))
        {
            if (!DateOnly.TryParseExact(body.OccurredAt, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
                return Invalid("occurredAt must be yyyy-MM-dd.");
            occurredAt = day.ToDateTime(new TimeOnly(12, 0)).ToString("O", CultureInfo.InvariantCulture);
        }

        var payload = new Dictionary<string, object?>
        {
            ["status"] = "COMPLETED",
            ["reason"] = reason,
            ["occurredAt"] = occurredAt,
            ["mobileDocumentId"] = string.IsNullOrWhiteSpace(body.DocumentNo) ? null : body.DocumentNo.Trim(),
            ["lines"] = body.Lines.Select(l => new { productCode = l.ProductCode.Trim(), countedQuantity = l.CountedQuantity }).ToList(),
        };
        var label = body.Lines.Count == 1 ? body.Lines[0].ProductCode.Trim() : $"{body.Lines.Count} ürün";
        // The audit trail files a count under its own job id — the key its void later names too.
        var operationKey = PortalNativeWriteHelpers.OperationKey("portal-stock-count", "sayim", body.OperationId);
        var audit = new PortalNativeWriteHelpers.AuditInfo(
            Entity: NativeDocumentProcessor.StockCount, EntityKey: operationKey, Action: "create",
            Summary: $"Sayım: {label} — {reason}", BeforeJson: null);
        return await PortalNativeWriteHelpers.BookNativeDocumentAsync(
            http, db, tenant!, user!, NativeDocumentProcessor.StockCount, operationKey, payload, RejectedErrorCode, ct, audit,
            new NativeBookingOptions(CountAgainstCurrentLevel: true));
    }

    /// <summary>
    /// Cancels a whole count (<c>stock_void</c>): every line it booked is reversed. <paramref name="key"/> is the count's
    /// job id or any of its movement ids (as the E6a movement list shows them).
    /// </summary>
    private static async Task<IResult> VoidAsync(
        string key, HttpContext http, [FromBody] PortalLedgerVoidRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, user, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        var reason = body?.Reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason)) return Invalid("A void needs a reason.");

        var externalId = await CountJobIdAsync(db, tenant!.Id, key, ct);
        if (externalId is null)
            return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = "STOCK_COUNT_NOT_FOUND", Message = "No stock count with that key for this company." });

        var operationKey = PortalNativeWriteHelpers.OperationKey("portal-stock-void", "sayim", body?.OperationId);
        if (await PortalNativeWriteHelpers.ReplayAsync(db, tenant.Id, NativeDocumentProcessor.StockVoid, operationKey, ct) is { } replay) return replay;

        var lines = await db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == tenant.Id && r.Entity == "stockTransactions" && r.RecordKey.StartsWith(externalId + "|") && !r.IsDeleted)
            .Select(r => new { r.RecordKey, r.PayloadJson })
            .ToListAsync(ct);
        if (lines.Where(l => NativeDocumentProcessor.OwnedByJob(l.RecordKey, externalId)).Any(l => IsVoided(l.PayloadJson)))
            return JsonResults.Status(StatusCodes.Status409Conflict, new ApiError { ErrorCode = "ALREADY_VOIDED", Message = "This count was already cancelled." });

        var audit = new PortalNativeWriteHelpers.AuditInfo(
            Entity: NativeDocumentProcessor.StockCount, EntityKey: externalId, Action: "void",
            Summary: $"İptal edildi (Sayım): {reason}", BeforeJson: null);
        return await PortalNativeWriteHelpers.BookNativeDocumentAsync(
            http, db, tenant, user!, NativeDocumentProcessor.StockVoid,
            operationKey, new { targetKey = externalId, reason }, RejectedErrorCode, ct, audit);
    }

    /// <summary>The count's own job id for <paramref name="key"/> (the id itself, or a movement id under it), or null.</summary>
    private static async Task<string?> CountJobIdAsync(CentralApiDbContext db, Guid tenantId, string key, CancellationToken ct)
    {
        foreach (var candidate in new[] { key, PortalLedger.ExternalIdOf(key) }.Distinct(StringComparer.Ordinal))
        {
            var found = await db.Jobs.AsNoTracking()
                .AnyAsync(j => j.TenantId == tenantId && j.ExternalId == candidate && j.DocumentType == NativeDocumentProcessor.StockCount, ct);
            if (found) return candidate;
        }
        return null;
    }

    private static bool IsVoided(string? payload)
    {
        if (payload is null) return false;
        using var row = JsonDocument.Parse(payload);
        return row.RootElement.TryGetProperty("voided", out var voided) && voided.ValueKind == JsonValueKind.True;
    }

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_STOCK_COUNT", Message = message });
}
