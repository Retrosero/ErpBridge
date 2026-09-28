using System.Text.RegularExpressions;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Native;
using ErpBridge.CentralApi.Portal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/portal/native/stock-cards/batch</c> and <c>…/customer-cards/batch</c> (GOAL_PANEL_ERPSIZ E1d/E2c): an
/// ERP-less company's admin imports products or customers from a spreadsheet. Wraps the phone's own
/// <c>stock_card_batch</c>/<c>customer_card_batch</c> documents (D1) — one call carries at most
/// <see cref="NativeDocumentProcessor.MaxCardsPerBatch"/> cards, the portal sends a larger file in parts.
///
/// <para>A bad row never spoils the file: the endpoint skips what it can already see is wrong (missing code/name, a code
/// or barcode twice in the file, a barcode another product owns) and the processor skips what it rejects; both come back
/// as <see cref="PortalCardBatchSkip"/> rows numbered the way the caller numbered them (<c>firstRow</c>).</para>
/// </summary>
public static partial class PortalNativeCardImportEndpoints
{
    private const string RejectedErrorCode = "CARD_BATCH_REJECTED";

    public static IEndpointRouteBuilder MapPortalNativeCardImportEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal/native")
            .WithTags("Portal")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapPost("/stock-cards/batch", ImportStockCardsAsync).WithName("PortalImportNativeStockCards");
        group.MapPost("/customer-cards/batch", ImportCustomerCardsAsync).WithName("PortalImportNativeCustomerCards");
        return routes;
    }

    private static async Task<IResult> ImportStockCardsAsync(
        HttpContext http, [FromBody] PortalStockCardBatchRequest? body, [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var (tenant, user, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        if (body is null || body.Cards.Count == 0) return Invalid("At least one card is required.");
        if (body.Cards.Count > NativeDocumentProcessor.MaxCardsPerBatch)
            return Invalid($"At most {NativeDocumentProcessor.MaxCardsPerBatch} cards per call; send a larger file in parts.");

        var catalog = await PortalStockCatalog.LoadAsync(db, cache, tenant!.Id, ct);
        var barcodeOwners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var product in catalog.Products)
            foreach (var barcode in product.Barcodes)
                barcodeOwners.TryAdd(barcode, product.Code);

        var skipped = new List<PortalCardBatchSkip>();
        var cards = new List<object>();
        var rows = new List<int>();
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var barcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (body.Rows is { } stockRows && stockRows.Count != body.Cards.Count) return Invalid("rows must list one row per card.");
        for (var i = 0; i < body.Cards.Count; i++)
        {
            var row = body.Rows?[i] ?? body.FirstRow + i;
            var card = body.Cards[i];
            var code = card.StockCode?.Trim();
            var barcode = string.IsNullOrWhiteSpace(card.Barcode) ? null : card.Barcode.Trim();
            var reason = string.IsNullOrWhiteSpace(code) ? "CODE_REQUIRED"
                : code.Length > 64 ? "CODE_TOO_LONG"
                : string.IsNullOrWhiteSpace(card.Name) ? "NAME_REQUIRED"
                : !codes.Add(code) ? "DUPLICATE_CODE"
                : barcode is not null && !barcodes.Add(barcode) ? "DUPLICATE_BARCODE"
                : barcode is not null && barcodeOwners.TryGetValue(barcode, out var owner) && !string.Equals(owner, code, StringComparison.OrdinalIgnoreCase) ? "BARCODE_IN_USE"
                : null;
            if (reason is not null)
            {
                skipped.Add(new PortalCardBatchSkip { Row = row, Reason = reason });
                continue;
            }
            rows.Add(row);
            cards.Add(new
            {
                stockCode = code, name = card.Name.Trim(), unit = card.Unit, vatRate = card.VatRate, category = card.Category,
                brand = card.Brand, aisle = card.Aisle, barcode, price = card.Price, openingQuantity = card.OpeningQuantity,
            });
        }

        return await BookAsync(http, db, tenant, user!, NativeDocumentProcessor.StockCardBatch, "portal-stock-import", body.OperationId,
            cards, rows, skipped, "stock_card", $"Toplu ürün içe aktarma: {cards.Count} kart", ct,
            new NativeBookingOptions(KeepBarcodesWithTheirOwners: true));
    }

    private static async Task<IResult> ImportCustomerCardsAsync(
        HttpContext http, [FromBody] PortalCustomerCardBatchRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, user, error) = await PortalNativeWriteHelpers.AuthorizeForNativeWriteAsync(http, db, ct);
        if (error is not null) return error;
        if (body is null || body.Cards.Count == 0) return Invalid("At least one card is required.");
        if (body.Cards.Count > NativeDocumentProcessor.MaxCardsPerBatch)
            return Invalid($"At most {NativeDocumentProcessor.MaxCardsPerBatch} cards per call; send a larger file in parts.");

        var skipped = new List<PortalCardBatchSkip>();
        var cards = new List<object>();
        var rows = new List<int>();
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (body.Rows is { } customerRows && customerRows.Count != body.Cards.Count) return Invalid("rows must list one row per card.");
        for (var i = 0; i < body.Cards.Count; i++)
        {
            var row = body.Rows?[i] ?? body.FirstRow + i;
            var card = body.Cards[i];
            var code = card.CustomerCode?.Trim();
            var reason = string.IsNullOrWhiteSpace(code) ? "CODE_REQUIRED"
                : code.Length > 64 ? "CODE_TOO_LONG"
                : string.IsNullOrWhiteSpace(card.Title) ? "NAME_REQUIRED"
                : !codes.Add(code) ? "DUPLICATE_CODE"
                : null;
            if (reason is not null)
            {
                skipped.Add(new PortalCardBatchSkip { Row = row, Reason = reason });
                continue;
            }
            rows.Add(row);
            cards.Add(new
            {
                customerCode = code, title = card.Title.Trim(), taxNo = card.TaxNo, taxOffice = card.TaxOffice, phone = card.Phone,
                email = card.Email, regionCode = card.RegionCode, openingBalance = card.OpeningBalance,
            });
        }

        return await BookAsync(http, db, tenant!, user!, NativeDocumentProcessor.CustomerCardBatch, "portal-customer-import", body.OperationId,
            cards, rows, skipped, "customer_card", $"Toplu müşteri içe aktarma: {cards.Count} kart", ct);
    }

    private static async Task<IResult> BookAsync(
        HttpContext http, CentralApiDbContext db, Tenant tenant, MobileUser user, string documentType, string keyPrefix, string? operationId,
        List<object> cards, List<int> rows, List<PortalCardBatchSkip> skipped, string auditEntity, string auditSummary, CancellationToken ct,
        NativeBookingOptions? options = null)
    {
        if (cards.Count == 0)
            return JsonResults.Ok(new PortalCardBatchResponse { Booked = 0, Skipped = skipped });

        var externalId = PortalNativeWriteHelpers.OperationKey(keyPrefix, "toplu", operationId);
        var replay = await db.Jobs.AsNoTracking()
            .AnyAsync(j => j.TenantId == tenant.Id && j.DocumentType == documentType && j.ExternalId == externalId, ct);
        var audit = new PortalNativeWriteHelpers.AuditInfo(auditEntity, "toplu", "import", auditSummary, BeforeJson: null);
        var result = await PortalNativeWriteHelpers.BookNativeDocumentAsync(
            http, db, tenant, user, documentType, externalId, new { cards }, RejectedErrorCode, ct, audit, options);

        var job = await db.Jobs.AsNoTracking()
            .FirstOrDefaultAsync(j => j.TenantId == tenant.Id && j.DocumentType == documentType && j.ExternalId == externalId
                                      && j.Status == JobStatus.Succeeded, ct);
        if (job is null) return result; // refused before or by the processor: its own error answer

        // The processor notes "N cards booked, M skipped. #3: … #7: …" (1-based within the cards sent); map back to rows.
        var rejected = SkipNote().Matches(job.LastError ?? string.Empty)
            .Select(m => (Index: int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), Message: m.Groups[2].Value.Trim()))
            .Where(r => r.Index >= 1 && r.Index <= rows.Count)
            .Select(r => new PortalCardBatchSkip { Row = rows[r.Index - 1], Reason = "REJECTED", Message = r.Message })
            .ToList();
        var rejectedCount = TotalSkipped().Match(job.LastError ?? string.Empty) is { Success: true } total
            ? int.Parse(total.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture)
            : rejected.Count;
        return JsonResults.Ok(new PortalCardBatchResponse
        {
            JobId = job.Id,
            Booked = cards.Count - rejectedCount,
            Skipped = [.. skipped, .. rejected],
            Idempotent = replay,
        });
    }

    [GeneratedRegex(@"#(\d+): (.*?)(?= #\d+: |$)")]
    private static partial Regex SkipNote();

    [GeneratedRegex(@"(\d+) skipped\.")]
    private static partial Regex TotalSkipped();

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_CARD_BATCH", Message = message });
}
