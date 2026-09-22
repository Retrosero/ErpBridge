using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Sync;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Native;

/// <summary>
/// The book of record for a tenant without an ERP.
///
/// <para>For an ERP tenant a document posted by a phone becomes a job an agent
/// writes into the ERP; the ERP then changes stock and balances and the agent
/// uploads the result. A native tenant has no agent, so this class plays both
/// parts at once: it stores the job, applies its effect to
/// <see cref="NativeStockLevel"/> and <see cref="NativeCustomerBalance"/>, and
/// projects the resulting ERP-shaped rows (<c>stocks</c>, <c>inventory</c>,
/// <c>customers</c>, movements…) into <see cref="MobileRecord"/> — the same rows
/// an agent would have produced, so every device reads them through the one
/// <c>/api/v1/android/sync/pull</c> path and the assembler needs no special case.</para>
///
/// <para>Everything happens in one transaction that first takes the tenant's
/// native lock, so documents from several phones are booked one at a time and a
/// job can never be stored without its effect, or the effect without the job.</para>
/// </summary>
public sealed class NativeDocumentProcessor
{
    public const string StockCard = "stock_card";
    public const string StockCardDelete = "stock_card_delete";
    public const string StockCardBatch = "stock_card_batch";
    public const string CustomerCardBatch = "customer_card_batch";

    /// <summary>Cards per batch document; keeps a batch well inside the ingest payload cap.</summary>
    public const int MaxCardsPerBatch = 500;
    public const string CustomerCard = "customer_card";
    public const string SalesOrder = "sales_order";
    public const string Collection = "collection";

    /// <summary>Money paid out from the phone's cash book: to a customer, a purchase payment or an expense.</summary>
    public const string Disbursement = "disbursement";
    public const string SalesReturn = "sales_return";
    public const string PurchaseReceipt = "purchase_receipt";
    public const string StockCount = "stock_count";

    /// <summary>Cancels one customer-ledger movement (GOAL_PANEL_ERPSIZ E4a/D2): the original is marked
    /// voided, a reversing entry is booked. Only for a standalone <see cref="Collection"/>/<see cref="Disbursement"/>/
    /// <see cref="LedgerAdjustment"/> — a sale/purchase/return's own cari etkisi is E5's <c>document_void</c> instead (D11).</summary>
    public const string LedgerVoid = "ledger_void";

    /// <summary>A manual correction of one customer's balance (GOAL_PANEL_ERPSIZ E4b): a mandatory reason,
    /// never a payment or a sale — for fixing a balance no other document type can express.</summary>
    public const string LedgerAdjustment = "ledger_adjustment";

    /// <summary>The job document types <see cref="LedgerVoid"/> may target, keyed by the movement's own external id.</summary>
    public static readonly IReadOnlySet<string> VoidableLedgerJobTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Collection, Disbursement, LedgerAdjustment };

    /// <summary>
    /// Line-carrying documents only the central API books. An ERP agent has no
    /// writer for them, so for an ERP tenant they would wait in its queue forever.
    /// </summary>
    public static readonly IReadOnlySet<string> NativeDocumentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { SalesReturn, PurchaseReceipt };

    /// <summary>
    /// Whether only a company without an ERP can take this document. A phone's own body (it names itself
    /// with <c>mobileDocumentId</c>) goes through the ERP agent's translator, so an ERP company accepts it:
    /// the lined return since goal ERP yazım Y3f/Y4b, the purchase invoice since ERP yazım 3 Y3a/Y1c.
    /// A body that is not the phone's own stays native-only, because nothing in the agent can read it.
    /// </summary>
    public static bool RequiresNativeTenant(string documentType, string? payloadJson) =>
        NativeDocumentTypes.Contains(documentType)
        && !ErpBridge.Core.Jobs.MobileDocumentTranslator.IsMobileDocument(payloadJson);

    /// <summary>Marks rows this class produced, the way an agent marks rows with its ERP name.</summary>
    public const string SourceName = "native";

    /// <summary>Document types only a native tenant accepts; an ERP agent would not know them.</summary>
    public static readonly IReadOnlySet<string> CardTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { StockCard, StockCardDelete, CustomerCard, StockCardBatch, CustomerCardBatch };

    /// <summary>Payment types that settle a sale on the spot, so the sale leaves no open balance.</summary>
    private static readonly HashSet<string> ImmediatePayments = new(StringComparer.OrdinalIgnoreCase)
    {
        "Nakit", "Kredi Kartı", "Kredi Karti", "Bank Kartı", "Banka Kartı", "EFT / Havale", "Havale", "EFT", "POS", "Banka İade",
    };

    private readonly MobileRecordProjector _projector;
    private readonly IBootstrapNotificationHub _hub;
    private readonly ILogger<NativeDocumentProcessor> _logger;

    public NativeDocumentProcessor(MobileRecordProjector projector, IBootstrapNotificationHub hub, ILogger<NativeDocumentProcessor> logger)
    {
        _projector = projector;
        _hub = hub;
        _logger = logger;
    }

    /// <summary>
    /// Stores <paramref name="job"/> and books it. A document that cannot be booked
    /// (unknown customer, missing fields) is stored as <see cref="JobStatus.Failed"/>
    /// with the reason, so support sees it and the phone does not retry it forever.
    /// Infrastructure errors throw and roll everything back.
    /// </summary>
    /// <param name="callerIsAdmin">Whether the signed-in user is a company administrator; product cards need one.</param>
    public async Task<Job> IngestAsync(CentralApiDbContext db, Guid tenantId, Job job, bool callerIsAdmin, CancellationToken ct)
    {
        // An approval posts its documents inside its own transaction; the booking joins
        // it so a failed document rolls the approval back too. The caller then commits
        // and wakes the devices.
        var ownsTransaction = db.Database.IsRelational() && db.Database.CurrentTransaction is null;
        await using var transaction = ownsTransaction ? await db.Database.BeginTransactionAsync(ct) : null;
        if (db.Database.IsRelational())
        {
            await db.Tenants.Where(t => t.Id == tenantId)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.NativeLockVersion, t => t.NativeLockVersion + 1), ct);
        }

        var now = DateTimeOffset.UtcNow;
        var booking = new Booking(tenantId, job.ExternalId, now, job.CreatedByUserId);
        string? error;
        using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.PayloadJson) ? "{}" : job.PayloadJson))
        {
            error = job.DocumentType.ToLowerInvariant() switch
            {
                StockCard => callerIsAdmin
                    ? await BookStockCardAsync(db, booking, document.RootElement, ct)
                    : "Only company administrators can create or change products.",
                StockCardDelete => callerIsAdmin
                    ? await BookStockCardDeleteAsync(db, booking, document.RootElement, ct)
                    : "Only company administrators can delete products.",
                CustomerCard => await BookCustomerCardAsync(db, booking, document.RootElement, ct),
                // An Excel import sends hundreds of cards; one document per card would
                // take minutes against the per-user rate limit.
                StockCardBatch => callerIsAdmin
                    ? await BookBatchAsync(booking, document.RootElement, card => BookStockCardAsync(db, booking, card, ct))
                    : "Only company administrators can create or change products.",
                CustomerCardBatch => await BookBatchAsync(booking, document.RootElement, card => BookCustomerCardAsync(db, booking, card, ct)),
                SalesOrder => await BookSaleAsync(db, booking, document.RootElement, ct),
                Collection => await BookCollectionAsync(db, booking, document.RootElement, ct),
                Disbursement => await BookDisbursementAsync(db, booking, document.RootElement, ct),
                SalesReturn => await BookSalesReturnAsync(db, booking, document.RootElement, ct),
                PurchaseReceipt => await BookPurchaseReceiptAsync(db, booking, document.RootElement, ct),
                StockCount => await BookStockCountAsync(db, booking, document.RootElement, ct),
                LedgerVoid => callerIsAdmin
                    ? await BookLedgerVoidAsync(db, booking, document.RootElement, ct)
                    : "Only company administrators can void a ledger entry.",
                LedgerAdjustment => callerIsAdmin
                    ? await BookLedgerAdjustmentAsync(db, booking, document.RootElement, ct)
                    : "Only company administrators can adjust a customer's balance.",
                // Other cash-book documents (return and purchase payments already booked by
                // their own documents, cash transfers) are kept as records only.
                _ => null,
            };
        }

        if (error is null) await booking.FinishAsync(db, ct);
        else DiscardLedgerChanges(db);

        job.Status = error is null ? JobStatus.Succeeded : JobStatus.Failed;
        job.LastError = error ?? booking.Warning;
        job.CompletedAtUtc = now;
        db.Jobs.Add(job);
        await db.SaveChangesAsync(ct);

        var devicesAffected = false;
        if (error is null && booking.Sections.Count > 0)
        {
            var sections = booking.Sections
                .Select(pair => new MobileRecordProjector.SectionRows(pair.Key, pair.Value))
                .ToList();
            await _projector.ProjectAsync(db, tenantId, sections, fullUpload: false, SourceName, ct);
            await db.SaveChangesAsync(ct);
            devicesAffected = true;
        }

        if (error is null && (booking.DeletedStockCodes.Count > 0 || booking.StaleRecords.Count > 0))
        {
            var removed = await _projector.ApplyDeletesAsync(db, tenantId,
                booking.DeletedStockCodes.Select(code => new MobileRecordProjector.DeletedRecord("STOKLAR", code)).ToList(), ct);
            removed += await _projector.TombstoneAsync(db, tenantId, booking.StaleRecords, ct);
            await db.SaveChangesAsync(ct);
            devicesAffected |= removed > 0;
        }

        if (transaction is not null) await transaction.CommitAsync(ct);

        if (devicesAffected && ownsTransaction)
            _hub.Publish(tenantId, now);
        else if (error is not null)
            _logger.LogWarning("Native {DocumentType} {ExternalId} for tenant {TenantId} was not booked: {Error}",
                job.DocumentType, job.ExternalId, tenantId, error);
        return job;
    }

    // ---- cards -----------------------------------------------------------

    private async Task<string?> BookStockCardAsync(CentralApiDbContext db, Booking booking, JsonElement card, CancellationToken ct)
    {
        var code = Text(card, "stockCode", "productCode", "code");
        var name = Text(card, "name", "title");
        if (code is null || code.Length > 64) return "stockCode is required (at most 64 characters).";
        if (name is null) return "name is required.";

        var barcode = Text(card, "barcode");
        booking.Add("stocks", new JsonObject
        {
            ["stockCode"] = code,
            ["name"] = name,
            ["urunAd"] = name,
            ["birim"] = Text(card, "unit"),
            ["kdvOrani"] = Number(card, "vatRate"),
            ["kategori"] = Text(card, "category"),
            ["marka"] = Text(card, "brand"),
            ["brandCode"] = Text(card, "brand"),
            ["shelfCode"] = Text(card, "aisle"),
            ["updatedAt"] = booking.Stamp,
        });
        if (barcode is not null)
        {
            booking.Add("barcodes", new JsonObject { ["barcode"] = barcode, ["stockCode"] = code });
            // The card lists one barcode. One it listed before would keep resolving to
            // this product at the till, so it is removed when the barcode changes.
            booking.StaleRecords.AddRange(await db.MobileRecords
                .Where(r => r.TenantId == booking.TenantId && r.Entity == "barcodes" && r.StockKey == code
                            && r.RecordKey != barcode && !r.IsDeleted)
                .ToListAsync(ct));
        }
        if (Number(card, "price") is { } price and > 0)
            booking.Add("prices", new JsonObject { ["stockCode"] = code, ["listNumber"] = 1, ["price"] = price });

        // An opening quantity is taken only when the product has no stock yet; later
        // card edits must not overwrite what sales have already moved.
        var level = await LevelAsync(db, booking.TenantId, code, ct);
        if (level.IsNew && Number(card, "openingQuantity") is { } opening)
            level.Row.Quantity = opening;
        booking.AddInventory(level.Row);
        return null;
    }

    /// <summary>
    /// Books every valid card of a batch. A card that fails validation is skipped and
    /// named in the job's note rather than failing its neighbours — one empty Excel row
    /// must not discard the other few hundred. Validation happens before a card
    /// touches the ledger, so a skipped card leaves nothing behind.
    /// </summary>
    private static async Task<string?> BookBatchAsync(Booking booking, JsonElement batch, Func<JsonElement, Task<string?>> bookCard)
    {
        if (!batch.TryGetProperty("cards", out var cards) || cards.ValueKind != JsonValueKind.Array || cards.GetArrayLength() == 0)
            return "A batch needs a non-empty cards array.";
        if (cards.GetArrayLength() > MaxCardsPerBatch)
            return $"A batch may carry at most {MaxCardsPerBatch} cards.";

        var skipped = new List<string>();
        var booked = 0;
        var index = 0;
        foreach (var card in cards.EnumerateArray())
        {
            index++;
            if (await bookCard(card) is { } problem) skipped.Add($"#{index}: {problem}");
            else booked++;
        }
        if (booked == 0) return "No card in the batch could be booked. " + string.Join(" ", skipped.Take(10));
        if (skipped.Count > 0)
            booking.Warning = $"{booked} cards booked, {skipped.Count} skipped. " + string.Join(" ", skipped.Take(20));
        return null;
    }

    private async Task<string?> BookStockCardDeleteAsync(CentralApiDbContext db, Booking booking, JsonElement card, CancellationToken ct)
    {
        var code = Text(card, "stockCode", "productCode", "code");
        if (code is null) return "stockCode is required.";
        if (!await StockCardExistsAsync(db, booking.TenantId, code, ct)) return "The product does not exist.";

        var level = await db.NativeStockLevels.Where(l => l.TenantId == booking.TenantId && l.StockCode == code).ToListAsync(ct);
        if (level.Any(l => l.LastMovementAtUtc is not null))
            return "The product has sales or other movements and cannot be deleted.";

        db.NativeStockLevels.RemoveRange(level);
        // The card, its barcodes, prices and stock are tombstoned together; devices
        // receive the product as deleted.
        booking.DeletedStockCodes.Add(code);
        return null;
    }

    private async Task<string?> BookCustomerCardAsync(CentralApiDbContext db, Booking booking, JsonElement card, CancellationToken ct)
    {
        var code = Text(card, "customerCode", "code");
        var title = Text(card, "title", "name");
        if (code is null || code.Length > 64) return "customerCode is required (at most 64 characters).";
        if (title is null) return "title is required.";

        var balance = await BalanceAsync(db, booking.TenantId, code, ct);
        if (balance.IsNew && Number(card, "openingBalance") is { } opening)
            balance.Row.Balance = decimal.Round(opening, 2);

        booking.Customers[code] = new JsonObject
        {
            ["customerCode"] = code,
            ["title1"] = title,
            ["taxNo"] = Text(card, "taxNo"),
            ["taxOffice"] = Text(card, "taxOffice"),
            ["phone"] = Text(card, "phone"),
            ["email"] = Text(card, "email"),
            ["regionCode"] = Text(card, "regionCode"),
            ["currency"] = "TRY",
            ["balance"] = balance.Row.Balance,
            ["updatedAt"] = booking.Stamp,
        };
        booking.CustomerBalances[code] = balance.Row;
        return null;
    }

    // ---- documents -------------------------------------------------------

    /// <summary>Which way a document moves stock, and how its movement rows are written.</summary>
    private readonly record struct StockDirection(int Sign, int MovementType, string QuantityField, string WarehouseField);

    /// <summary>Stock leaves the warehouse (sale).</summary>
    private static readonly StockDirection Outgoing = new(-1, 1, "cikisMiktar", "cikisDepoNo");

    /// <summary>Stock enters the warehouse (purchase).</summary>
    private static readonly StockDirection Incoming = new(+1, 0, "girisMiktar", "girisDepoNo");

    /// <summary>A customer's goods come back into the warehouse (sales return).</summary>
    private static readonly StockDirection ReturnedIn = new(+1, 2, "girisMiktar", "girisDepoNo");

    /// <summary>
    /// Validates every line first, then moves stock and writes one movement row per
    /// line. Returns the error, or null and the lines' total. Nothing touches the
    /// ledger until all lines are known to be valid, so a bad third line cannot
    /// leave the first two booked.
    /// </summary>
    private async Task<(string? Error, decimal Total)> BookLinesAsync(
        CentralApiDbContext db, Booking booking, JsonElement document, StockDirection direction,
        string partyCode, string documentNo, string occurredAt, string? description, bool linesRequired, CancellationToken ct)
    {
        if (!document.TryGetProperty("lines", out var lines) || lines.ValueKind != JsonValueKind.Array || lines.GetArrayLength() == 0)
            return (linesRequired ? "The document needs at least one line." : null, 0);

        var parsed = new List<(string StockCode, decimal Quantity, decimal UnitPrice, decimal LineTotal, string? Note)>();
        var lineNo = 0;
        foreach (var line in lines.EnumerateArray())
        {
            lineNo++;
            var quantity = Number(line, "quantity") ?? 0;
            if (quantity <= 0) return ($"Line {lineNo} has no quantity.", 0);
            var stockCode = Text(line, "productCode") ?? await StockCodeForBarcodeAsync(db, booking.TenantId, Text(line, "barcode"), ct);
            // A code that names no product card would move stock of a product nobody
            // can see. The phone sends cards before documents, so a real product is known.
            if (stockCode is null || !await StockCardExistsAsync(db, booking.TenantId, stockCode, ct))
                return ($"Line {lineNo} names no known product.", 0);
            var unitPrice = Number(line, "unitPrice") ?? 0;
            var lineTotal = Number(line, "lineTotal") ?? quantity * unitPrice;
            if (unitPrice < 0 || lineTotal < 0) return ($"Line {lineNo} has a negative price.", 0);
            parsed.Add((stockCode, quantity, unitPrice, lineTotal, Text(line, "reason")));
        }

        lineNo = 0;
        foreach (var line in parsed)
        {
            lineNo++;
            var level = await LevelAsync(db, booking.TenantId, line.StockCode, ct);
            level.Row.Quantity += direction.Sign * line.Quantity;
            level.Row.UpdatedAtUtc = booking.Now;
            level.Row.LastMovementAtUtc = booking.Now;
            booking.AddInventory(level.Row);
            booking.Add("stockTransactions", new JsonObject
            {
                ["id"] = $"{booking.ExternalId}|{lineNo}",
                ["erp"] = "NATIVE",
                ["stokKod"] = line.StockCode,
                ["urunKod"] = line.StockCode,
                ["tarih"] = occurredAt,
                ["tip"] = direction.MovementType,
                ["cins"] = 0,
                ["evrakNo"] = documentNo,
                [direction.QuantityField] = line.Quantity,
                ["miktar"] = direction.Sign * line.Quantity,
                ["birimFiyat"] = line.UnitPrice,
                ["tutar"] = line.LineTotal,
                ["cariKod"] = partyCode,
                [direction.WarehouseField] = NativeLedgerDefaults.WarehouseNo,
                ["aciklama"] = line.Note ?? description,
                ["updatedAt"] = booking.Stamp,
            });
        }
        return (null, parsed.Sum(line => line.LineTotal));
    }

    private async Task<string?> BookSaleAsync(CentralApiDbContext db, Booking booking, JsonElement sale, CancellationToken ct)
    {
        var customer = await ResolveCustomerAsync(db, booking.TenantId, sale, ct);
        if (customer is null) return "The sale names no known customer (customerCode or an exact customer title is required).";

        var documentNo = Text(sale, "mobileDocumentId") ?? booking.ExternalId;
        var occurredAt = Text(sale, "occurredAt") ?? booking.Stamp;
        var description = Text(sale, "description");
        var (error, linesTotal) = await BookLinesAsync(db, booking, sale, Outgoing, customer, documentNo, occurredAt, description, linesRequired: true, ct);
        if (error is not null) return error;

        var amount = decimal.Round(Number(sale, "amount") ?? linesTotal, 2);
        // A negative debit would silently reduce the customer's debt.
        if (amount < 0) return "A sale cannot have a negative total.";
        await PostToCustomerAsync(db, booking, customer, amount, debit: true, "Satış", documentNo, occurredAt, description, suffix: "sale", ct);

        // A sale paid on the spot is also a collection of the same amount: the
        // customer's history shows both and the open balance does not move.
        if (Text(sale, "paymentType") is { } paymentType && ImmediatePayments.Contains(paymentType.Trim()))
            await PostToCustomerAsync(db, booking, customer, amount, debit: false, "Tahsilat", documentNo, occurredAt, paymentType, suffix: "payment", ct, paymentType);
        return null;
    }

    /// <summary>
    /// A customer returns goods: stock comes back in and the customer is credited.
    /// When the money is handed back on the spot (cash, bank transfer) a payment to
    /// the customer is booked as well, so the open balance does not move.
    /// </summary>
    private async Task<string?> BookSalesReturnAsync(CentralApiDbContext db, Booking booking, JsonElement document, CancellationToken ct)
    {
        var customer = await ResolveCustomerAsync(db, booking.TenantId, document, ct);
        if (customer is null) return "The return names no known customer (customerCode or an exact customer title is required).";

        var documentNo = Text(document, "mobileDocumentId") ?? booking.ExternalId;
        var occurredAt = Text(document, "occurredAt") ?? booking.Stamp;
        var description = Text(document, "description");
        var (error, linesTotal) = await BookLinesAsync(db, booking, document, ReturnedIn, customer, documentNo, occurredAt, description, linesRequired: true, ct);
        if (error is not null) return error;

        var amount = decimal.Round(Number(document, "amount") ?? linesTotal, 2);
        if (amount < 0) return "A return cannot have a negative total.";
        await PostToCustomerAsync(db, booking, customer, amount, debit: false, "İade", documentNo, occurredAt, description, suffix: "return", ct);
        if (Text(document, "paymentType") is { } paymentType && ImmediatePayments.Contains(paymentType.Trim()))
            await PostToCustomerAsync(db, booking, customer, amount, debit: true, "İade Ödemesi", documentNo, occurredAt, paymentType, suffix: "refund", ct, paymentType);
        return null;
    }

    /// <summary>
    /// Goods bought from a supplier: stock comes in and the supplier (a customer card)
    /// is credited, i.e. the company owes it. Paid on the spot, a payment to the
    /// supplier is booked as well. Lines are optional — a purchase of items that are
    /// not in the catalogue still owes the supplier — but every line given must name
    /// a known product.
    /// </summary>
    private async Task<string?> BookPurchaseReceiptAsync(CentralApiDbContext db, Booking booking, JsonElement document, CancellationToken ct)
    {
        var supplier = Text(document, "supplierCode") is { } code
            ? await ResolveCustomerAsync(db, booking.TenantId, JsonSerializer.SerializeToElement(new { customerCode = code }), ct)
            : await ResolveCustomerAsync(db, booking.TenantId, document, ct);
        if (supplier is null) return "The purchase names no known supplier (supplierCode or an exact supplier title is required).";

        var documentNo = Text(document, "invoiceNo") ?? Text(document, "mobileDocumentId") ?? booking.ExternalId;
        var occurredAt = Text(document, "occurredAt") ?? booking.Stamp;
        var description = Text(document, "description");
        var (error, linesTotal) = await BookLinesAsync(db, booking, document, Incoming, supplier, documentNo, occurredAt, description, linesRequired: false, ct);
        if (error is not null) return error;

        var amount = decimal.Round(Number(document, "amount") ?? linesTotal, 2);
        if (amount <= 0) return "A purchase needs a positive total.";
        await PostToCustomerAsync(db, booking, supplier, amount, debit: false, "Alış", documentNo, occurredAt, description, suffix: "purchase", ct);
        if (Text(document, "paymentType") is { } paymentType && ImmediatePayments.Contains(paymentType.Trim()))
            await PostToCustomerAsync(db, booking, supplier, amount, debit: true, "Tediye", documentNo, occurredAt, paymentType, suffix: "payment", ct, paymentType);
        return null;
    }

    /// <summary>
    /// A completed stock count. Each line moves stock by the difference the counter
    /// found — <c>countedQuantity - expectedQuantity</c> — not to the counted number.
    ///
    /// <para>Counts are made offline and uploaded later. A sale another phone booked
    /// after the count must survive it: setting stock to the counted number would
    /// erase that sale, while applying the difference keeps it. Lines without a
    /// difference are skipped. Every line is validated before the ledger moves.</para>
    /// </summary>
    private async Task<string?> BookStockCountAsync(CentralApiDbContext db, Booking booking, JsonElement document, CancellationToken ct)
    {
        if (Text(document, "status") is { } status && !string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            return "Only a completed stock count changes stock.";
        if (!document.TryGetProperty("lines", out var lines) || lines.ValueKind != JsonValueKind.Array || lines.GetArrayLength() == 0)
            return "A stock count needs at least one line.";

        var documentNo = Text(document, "mobileDocumentId") ?? booking.ExternalId;
        var occurredAt = Text(document, "occurredAt") ?? booking.Stamp;
        var countedBy = Text(document, "countedBy");
        var differences = new List<(string StockCode, decimal Difference, decimal Counted)>();
        var lineNo = 0;
        foreach (var line in lines.EnumerateArray())
        {
            lineNo++;
            var stockCode = Text(line, "productCode") ?? await StockCodeForBarcodeAsync(db, booking.TenantId, Text(line, "barcode"), ct);
            if (stockCode is null || !await StockCardExistsAsync(db, booking.TenantId, stockCode, ct))
                return $"Line {lineNo} names no known product.";
            if (Number(line, "countedQuantity") is not { } counted || counted < 0)
                return $"Line {lineNo} has no counted quantity.";
            var expected = Number(line, "expectedQuantity") ?? 0;
            if (counted != expected) differences.Add((stockCode, counted - expected, counted));
        }

        lineNo = 0;
        foreach (var (stockCode, difference, counted) in differences)
        {
            lineNo++;
            var level = await LevelAsync(db, booking.TenantId, stockCode, ct);
            level.Row.Quantity += difference;
            level.Row.UpdatedAtUtc = booking.Now;
            level.Row.LastMovementAtUtc = booking.Now;
            booking.AddInventory(level.Row);
            var incoming = difference > 0;
            booking.Add("stockTransactions", new JsonObject
            {
                ["id"] = $"{booking.ExternalId}|{lineNo}",
                ["erp"] = "NATIVE",
                ["stokKod"] = stockCode,
                ["urunKod"] = stockCode,
                ["tarih"] = occurredAt,
                ["tip"] = incoming ? 0 : 1,
                ["cins"] = 0,
                ["evrakNo"] = documentNo,
                [incoming ? "girisMiktar" : "cikisMiktar"] = Math.Abs(difference),
                ["miktar"] = difference,
                ["birimFiyat"] = 0,
                ["tutar"] = 0,
                [incoming ? "girisDepoNo" : "cikisDepoNo"] = NativeLedgerDefaults.WarehouseNo,
                ["aciklama"] = $"Sayım farkı (sayılan {counted}){(countedBy is null ? "" : $" · {countedBy}")}",
                ["updatedAt"] = booking.Stamp,
            });
        }
        return null;
    }

    private async Task<string?> BookCollectionAsync(CentralApiDbContext db, Booking booking, JsonElement collection, CancellationToken ct)
    {
        var customer = await ResolveCustomerAsync(db, booking.TenantId, collection, ct);
        if (customer is null) return "The collection names no known customer (customerCode or an exact customer title is required).";
        var amount = decimal.Round(Number(collection, "amount") ?? 0, 2);
        if (amount <= 0) return "A collection needs a positive amount.";

        var documentNo = Text(collection, "mobileDocumentId") ?? booking.ExternalId;
        await PostToCustomerAsync(db, booking, customer, amount, debit: false, "Tahsilat", documentNo,
            Text(collection, "occurredAt") ?? booking.Stamp,
            Text(collection, "description") ?? Text(collection, "paymentType"), suffix: "collection", ct,
            Text(collection, "paymentType"));
        return null;
    }

    /// <summary>
    /// A payment out of the phone's cash book (Faz 40). Paid to a customer, it debits the
    /// customer — the mirror of a collection; before this the server kept it as a record
    /// only and every such customer's balance stayed too low.
    /// <list type="bullet">
    /// <item>A purchase's payment (<c>approvalKind = purchase</c>) is already booked by its
    /// <c>purchase_receipt</c>; booking it again would debit the supplier twice.</item>
    /// <item>An expense or other payment that names no customer stays a record.</item>
    /// <item>A <c>customerCode</c> that names no customer fails, as a collection does.</item>
    /// </list>
    /// </summary>
    private async Task<string?> BookDisbursementAsync(CentralApiDbContext db, Booking booking, JsonElement disbursement, CancellationToken ct)
    {
        if (string.Equals(Text(disbursement, "approvalKind"), ApprovalKinds.Purchase, StringComparison.OrdinalIgnoreCase))
            return null;
        var amount = decimal.Round(Number(disbursement, "amount") ?? 0, 2);
        if (amount <= 0) return "A disbursement needs a positive amount.";

        var customer = await ResolveCustomerAsync(db, booking.TenantId, disbursement, ct);
        if (customer is null)
        {
            if (Text(disbursement, "customerCode") is not null)
                return "The disbursement names no known customer.";
            booking.Warning = "Recorded without a customer (expense or other payment).";
            return null;
        }

        var documentNo = Text(disbursement, "mobileDocumentId") ?? booking.ExternalId;
        await PostToCustomerAsync(db, booking, customer, amount, debit: true, "Tediye", documentNo,
            Text(disbursement, "occurredAt") ?? booking.Stamp,
            Text(disbursement, "description") ?? Text(disbursement, "paymentType"), suffix: "disbursement", ct,
            Text(disbursement, "paymentType"));
        return null;
    }

    /// <summary>
    /// A manual correction of one customer's balance (GOAL_PANEL_ERPSIZ E4b) — for fixing a balance no
    /// other document expresses (an opening balance typed wrong, a write-off). A mandatory reason, so
    /// the statement always says why; the reason is stored apart from <c>aciklama</c> the way a payment's
    /// own <c>paymentType</c> is, so the statement's free-text description column stays free for either.
    /// </summary>
    private async Task<string?> BookLedgerAdjustmentAsync(CentralApiDbContext db, Booking booking, JsonElement document, CancellationToken ct)
    {
        var customer = await ResolveCustomerAsync(db, booking.TenantId, document, ct);
        if (customer is null) return "The adjustment names no known customer (customerCode or an exact customer title is required).";
        var amount = decimal.Round(Number(document, "amount") ?? 0, 2);
        if (amount <= 0) return "An adjustment needs a positive amount.";
        var reason = Text(document, "reason");
        if (reason is null) return "An adjustment needs a reason.";
        var debit = Bool(document, "debit");
        if (debit is null) return "debit is required (true to increase what the customer owes, false to decrease it).";

        var documentNo = Text(document, "mobileDocumentId") ?? booking.ExternalId;
        // Suffix matches the LedgerAdjustment document type constant, the way "collection"/"disbursement"
        // already do — so a movement's own key always names the job that created it (ExternalIdOfLedgerKey).
        await PostToCustomerAsync(db, booking, customer, amount, debit: debit.Value, "Düzeltme", documentNo,
            Text(document, "occurredAt") ?? booking.Stamp, reason, suffix: LedgerAdjustment, ct);
        return null;
    }

    /// <summary>
    /// Cancels one collection/disbursement (GOAL_PANEL_ERPSIZ E4a, D2's storno pattern): the original
    /// <c>customerTransactions</c> row is marked <c>voided</c> in place (upserted under its own key, so
    /// the historical amount/type never change) and a reversing entry — the exact opposite of its
    /// balance effect — is booked under a new key. Never a sale/purchase/return's own row: those are
    /// E5's <c>document_void</c> (D11), enforced here by requiring the movement's own job to be a
    /// standalone <see cref="Collection"/>/<see cref="Disbursement"/> (<see cref="VoidableLedgerJobTypes"/>),
    /// not a job that also moved stock or wrote lines.
    /// </summary>
    private async Task<string?> BookLedgerVoidAsync(CentralApiDbContext db, Booking booking, JsonElement document, CancellationToken ct)
    {
        var targetKey = Text(document, "targetKey");
        if (targetKey is null) return "targetKey is required.";
        var reason = Text(document, "reason");
        if (reason is null) return "A void needs a reason.";

        var record = await db.MobileRecords
            .FirstOrDefaultAsync(r => r.TenantId == booking.TenantId && r.Entity == "customerTransactions" && r.RecordKey == targetKey && !r.IsDeleted, ct);
        if (record?.PayloadJson is null) return "The transaction was not found.";

        string customerCode, sourceType, documentNo;
        decimal amount;
        bool debit;
        using (var originalDoc = JsonDocument.Parse(record.PayloadJson))
        {
            var row = originalDoc.RootElement;
            if (Bool(row, "voided") == true) return "This transaction is already void.";
            customerCode = Text(row, "cariKod", "customerCode") ?? "";
            sourceType = Text(row, "type") ?? "Hareket";
            documentNo = Text(row, "evrakNo") ?? booking.ExternalId;
            amount = Math.Abs(Number(row, "meblag") ?? Number(row, "amount") ?? Number(row, "tutar") ?? 0m);
            debit = Bool(row, "borcMu") ?? false;
        }
        if (customerCode.Length == 0) return "The transaction names no customer.";
        if (amount <= 0) return "The transaction has no amount to reverse.";

        // A sale's immediate-payment leg has kind "collection" too, but its job is sales_order/
        // sales_return/purchase_receipt, not a standalone Collection/Disbursement — excluded here.
        var jobExternalId = ExternalIdOfLedgerKey(targetKey);
        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.TenantId == booking.TenantId && j.ExternalId == jobExternalId, ct);
        if (job is null || !VoidableLedgerJobTypes.Contains(job.DocumentType))
            return "Only a collection or a disbursement can be voided here.";

        var balance = await BalanceAsync(db, booking.TenantId, customerCode, ct);
        balance.Row.Balance += debit ? -amount : amount;
        balance.Row.UpdatedAtUtc = booking.Now;
        booking.CustomerBalances[customerCode] = balance.Row;

        var originalNode = JsonNode.Parse(record.PayloadJson)!.AsObject();
        originalNode["voided"] = true;
        originalNode["voidedByUserId"] = booking.UserId;
        originalNode["voidedAt"] = booking.Stamp;
        originalNode["voidReason"] = reason;
        originalNode["updatedAt"] = booking.Stamp;
        booking.Add("customerTransactions", originalNode);

        booking.Add("customerTransactions", new JsonObject
        {
            ["id"] = $"{targetKey}|void",
            ["erp"] = "NATIVE",
            ["cariKod"] = customerCode,
            ["customerCode"] = customerCode,
            ["tarih"] = Text(document, "occurredAt") ?? booking.Stamp,
            ["evrakNo"] = documentNo,
            ["type"] = $"İptal: {sourceType}",
            ["tip"] = debit ? 1 : 0,
            ["borcMu"] = !debit,
            ["meblag"] = amount,
            ["amount"] = amount,
            ["aciklama"] = reason,
            ["voidsKey"] = targetKey,
            ["updatedAt"] = booking.Stamp,
        });
        return null;
    }

    /// <summary>The job that created a ledger movement, from its id (<c>PostToCustomerAsync</c> always writes
    /// <c>"{externalId}|{suffix}"</c>): strips the last <c>|</c>-separated segment. Mirrors <c>PortalLedger.ExternalIdOf</c>
    /// on the read side; kept separate because the write engine does not depend on the portal's read layer.</summary>
    private static string ExternalIdOfLedgerKey(string movementId)
    {
        var index = movementId.LastIndexOf('|');
        return index < 0 ? movementId : movementId[..index];
    }

    private async Task PostToCustomerAsync(
        CentralApiDbContext db, Booking booking, string customerCode, decimal amount, bool debit,
        string type, string documentNo, string occurredAt, string? description, string suffix, CancellationToken ct,
        string? paymentType = null)
    {
        var balance = await BalanceAsync(db, booking.TenantId, customerCode, ct);
        balance.Row.Balance += debit ? amount : -amount;
        balance.Row.UpdatedAtUtc = booking.Now;
        booking.CustomerBalances[customerCode] = balance.Row;

        booking.Add("customerTransactions", new JsonObject
        {
            ["id"] = $"{booking.ExternalId}|{suffix}",
            ["erp"] = "NATIVE",
            ["cariKod"] = customerCode,
            ["customerCode"] = customerCode,
            ["tarih"] = occurredAt,
            ["evrakNo"] = documentNo,
            ["type"] = type,
            ["tip"] = debit ? 0 : 1,
            ["borcMu"] = debit,
            ["meblag"] = amount,
            ["amount"] = amount,
            ["aciklama"] = description,
            // Kept apart from aciklama (which a caller may overwrite with free text) so the cash-box
            // summary (GOAL_PANEL_ERPSIZ E3c) can total by payment type even when a description is set.
            ["paymentType"] = paymentType,
            ["updatedAt"] = booking.Stamp,
        });
    }

    /// <summary>
    /// A document is booked whole or not at all. A sale whose third line names an
    /// unknown product has already moved the first two lines' stock in memory;
    /// those changes must not be saved together with the failed job.
    /// </summary>
    private static void DiscardLedgerChanges(CentralApiDbContext db)
    {
        var entries = db.ChangeTracker.Entries<NativeStockLevel>().Cast<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry>()
            .Concat(db.ChangeTracker.Entries<NativeCustomerBalance>())
            .ToList();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.State = EntityState.Detached;
            }
            else if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                entry.CurrentValues.SetValues(entry.OriginalValues);
                entry.State = EntityState.Unchanged;
            }
        }
    }

    // ---- lookups ---------------------------------------------------------

    /// <summary>The customer code a document names, or null when it names none that exists.</summary>
    private static async Task<string?> ResolveCustomerAsync(CentralApiDbContext db, Guid tenantId, JsonElement document, CancellationToken ct)
    {
        if (Text(document, "customerCode") is { } code)
        {
            var exists = await db.MobileRecords.AsNoTracking()
                .AnyAsync(r => r.TenantId == tenantId && r.Entity == "customers" && r.RecordKey == code && !r.IsDeleted, ct);
            return exists ? code : null;
        }

        // Older app builds only sent the customer's title. Accept it when it names
        // exactly one customer; guessing between two would book to the wrong one.
        if (Text(document, "counterparty") is not { } title) return null;
        var payloads = await db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.Entity == "customers" && !r.IsDeleted)
            .Select(r => new { r.RecordKey, r.PayloadJson })
            .ToListAsync(ct);
        var matches = payloads
            .Where(r => r.PayloadJson is not null && string.Equals(TitleOf(r.PayloadJson), title, StringComparison.OrdinalIgnoreCase))
            .Select(r => r.RecordKey)
            .ToList();
        return matches.Count == 1 ? matches[0] : null;
    }

    private static string? TitleOf(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        return Text(document.RootElement, "title1");
    }

    /// <summary>
    /// Products the sales documents in <paramref name="documents"/> (an approval
    /// request's <c>[{ documentType, payload }]</c>) ask more of than is on hand. Stock
    /// may go negative, so this only warns the approver.
    /// </summary>
    public static async Task<IReadOnlyList<StockWarningDto>> StockShortagesAsync(
        CentralApiDbContext db, Guid tenantId, JsonElement documents, CancellationToken ct)
    {
        var wanted = new Dictionary<string, StockWarningDto>(StringComparer.Ordinal);
        if (documents.ValueKind != JsonValueKind.Array) return [];
        foreach (var item in documents.EnumerateArray())
        {
            if (!string.Equals(Text(item, "documentType"), SalesOrder, StringComparison.OrdinalIgnoreCase)
                || !item.TryGetProperty("payload", out var payload)
                || !payload.TryGetProperty("lines", out var lines) || lines.ValueKind != JsonValueKind.Array)
                continue;
            foreach (var line in lines.EnumerateArray())
            {
                var code = Text(line, "productCode") ?? await StockCodeForBarcodeAsync(db, tenantId, Text(line, "barcode"), ct);
                var quantity = Number(line, "quantity") ?? 0;
                if (code is null || quantity <= 0) continue;
                if (!wanted.TryGetValue(code, out var entry))
                    wanted[code] = entry = new StockWarningDto { StockCode = code, Title = Text(line, "productTitle") ?? code };
                entry.Requested += quantity;
            }
        }

        var shortages = new List<StockWarningDto>();
        foreach (var entry in wanted.Values)
        {
            // Summed in memory: SQLite cannot aggregate decimal columns.
            var levels = await db.NativeStockLevels.AsNoTracking()
                .Where(l => l.TenantId == tenantId && l.StockCode == entry.StockCode)
                .Select(l => l.Quantity)
                .ToListAsync(ct);
            entry.OnHand = levels.Sum();
            if (entry.OnHand < entry.Requested) shortages.Add(entry);
        }
        return shortages;
    }

    private static async Task<string?> StockCodeForBarcodeAsync(CentralApiDbContext db, Guid tenantId, string? barcode, CancellationToken ct)
    {
        if (barcode is null) return null;
        var payload = await db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.Entity == "barcodes" && r.RecordKey == barcode && !r.IsDeleted)
            .Select(r => r.PayloadJson)
            .FirstOrDefaultAsync(ct);
        if (payload is null) return null;
        using var document = JsonDocument.Parse(payload);
        return Text(document.RootElement, "stockCode");
    }

    private static Task<bool> StockCardExistsAsync(CentralApiDbContext db, Guid tenantId, string stockCode, CancellationToken ct) =>
        db.MobileRecords.AsNoTracking()
            .AnyAsync(r => r.TenantId == tenantId && r.Entity == "stocks" && r.RecordKey == stockCode && !r.IsDeleted, ct);

    private static async Task<(NativeStockLevel Row, bool IsNew)> LevelAsync(CentralApiDbContext db, Guid tenantId, string stockCode, CancellationToken ct)
    {
        var tracked = db.NativeStockLevels.Local.FirstOrDefault(l => l.TenantId == tenantId && l.StockCode == stockCode && l.WarehouseNo == NativeLedgerDefaults.WarehouseNo);
        if (tracked is not null) return (tracked, false);
        var row = await db.NativeStockLevels.FirstOrDefaultAsync(l => l.TenantId == tenantId && l.StockCode == stockCode && l.WarehouseNo == NativeLedgerDefaults.WarehouseNo, ct);
        if (row is not null) return (row, false);
        row = new NativeStockLevel { TenantId = tenantId, StockCode = stockCode, WarehouseNo = NativeLedgerDefaults.WarehouseNo };
        db.NativeStockLevels.Add(row);
        return (row, true);
    }

    private static async Task<(NativeCustomerBalance Row, bool IsNew)> BalanceAsync(CentralApiDbContext db, Guid tenantId, string customerCode, CancellationToken ct)
    {
        var tracked = db.NativeCustomerBalances.Local.FirstOrDefault(b => b.TenantId == tenantId && b.CustomerCode == customerCode);
        if (tracked is not null) return (tracked, false);
        var row = await db.NativeCustomerBalances.FirstOrDefaultAsync(b => b.TenantId == tenantId && b.CustomerCode == customerCode, ct);
        if (row is not null) return (row, false);
        row = new NativeCustomerBalance { TenantId = tenantId, CustomerCode = customerCode };
        db.NativeCustomerBalances.Add(row);
        return (row, true);
    }

    private static string? Text(JsonElement item, params string[] names)
    {
        if (item.ValueKind != JsonValueKind.Object) return null;
        foreach (var name in names)
        {
            if (!item.TryGetProperty(name, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) continue;
            var text = value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
            if (!string.IsNullOrWhiteSpace(text)) return text.Trim();
        }
        return null;
    }

    private static decimal? Number(JsonElement item, string name)
    {
        if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number)) return number;
        return value.ValueKind == JsonValueKind.String
               && decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out number)
            ? number
            : null;
    }

    private static bool? Bool(JsonElement item, string name)
    {
        if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty(name, out var value)) return null;
        return value.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null };
    }

    /// <summary>What one document changes, collected before it is projected.</summary>
    private sealed class Booking(Guid tenantId, string externalId, DateTimeOffset now, Guid? userId)
    {
        public Guid TenantId { get; } = tenantId;
        public string ExternalId { get; } = externalId;
        public DateTimeOffset Now { get; } = now;
        public Guid? UserId { get; } = userId;
        public string Stamp { get; } = now.ToString("O", CultureInfo.InvariantCulture);
        public Dictionary<string, List<JsonElement>> Sections { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, JsonObject> Customers { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, NativeCustomerBalance> CustomerBalances { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<string> DeletedStockCodes { get; } = [];

        /// <summary>A note kept on a succeeded job, e.g. the rows a batch skipped.</summary>
        public string? Warning { get; set; }
        public List<MobileRecord> StaleRecords { get; } = [];
        private readonly Dictionary<string, NativeStockLevel> _levels = new(StringComparer.OrdinalIgnoreCase);

        public void Add(string section, JsonObject row)
        {
            if (!Sections.TryGetValue(section, out var rows)) Sections[section] = rows = [];
            rows.Add(JsonSerializer.SerializeToElement(row));
        }

        public void AddInventory(NativeStockLevel level) => _levels[level.StockCode] = level;

        /// <summary>Adds the final state of every stock level and customer this document touched.</summary>
        public async Task FinishAsync(CentralApiDbContext db, CancellationToken ct)
        {
            foreach (var level in _levels.Values)
            {
                Add("inventory", new JsonObject
                {
                    ["stockCode"] = level.StockCode,
                    ["warehouseNo"] = level.WarehouseNo,
                    ["quantity"] = level.Quantity,
                });
            }

            foreach (var (code, balance) in CustomerBalances)
            {
                if (!Customers.TryGetValue(code, out var card))
                {
                    // The customer card itself did not change: re-send the stored card
                    // with the new balance, which is the field the app shows.
                    var payload = await db.MobileRecords.AsNoTracking()
                        .Where(r => r.TenantId == TenantId && r.Entity == "customers" && r.RecordKey == code && !r.IsDeleted)
                        .Select(r => r.PayloadJson)
                        .FirstOrDefaultAsync(ct);
                    if (payload is null) continue;
                    card = JsonNode.Parse(payload)!.AsObject();
                }
                card["balance"] = balance.Balance;
                card["updatedAt"] = Stamp;
                Add("customers", card);
            }
        }
    }
}
