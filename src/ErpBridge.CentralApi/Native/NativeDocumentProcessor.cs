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
/// <summary>
/// How a trusted caller wants a document booked — never read from the document itself, so a phone cannot ask for it.
/// </summary>
/// <param name="CountAgainstCurrentLevel">GOAL_PANEL_ERPSIZ E6b: a portal <c>stock_count</c> takes its difference against
/// the stock booked now (read under the tenant lock), not against the <c>expectedQuantity</c> a device saw offline.</param>
public sealed record NativeBookingOptions(bool CountAgainstCurrentLevel = false);

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

    /// <summary>Corrects one ledger entry (GOAL_PANEL_ERPSIZ E4c, D11): <see cref="LedgerVoid"/> + a
    /// re-booked entry of the same kind, in the entry's own single transaction.</summary>
    public const string LedgerEdit = "ledger_edit";

    /// <summary>Cancels one whole sale/purchase/return document (GOAL_PANEL_ERPSIZ E5c, D11): the
    /// document's own ledger row(s) and every line's stock effect are reversed together — the
    /// document-level sibling of <see cref="LedgerVoid"/>, which never touches a sale/purchase/return's
    /// own row (only a standalone <see cref="Collection"/>/<see cref="Disbursement"/>/<see cref="LedgerAdjustment"/>).</summary>
    public const string DocumentVoid = "document_void";

    /// <summary>Corrects one whole sale/purchase/return document (GOAL_PANEL_ERPSIZ E5d, D11): <see cref="DocumentVoid"/>
    /// + a re-booked document of the same kind, in one transaction — the document-level sibling of <see cref="LedgerEdit"/>.
    /// The corrected document is booked under this job's own external id, so it is itself voidable and editable again.</summary>
    public const string DocumentEdit = "document_edit";

    /// <summary>Cancels one whole <see cref="StockCount"/> (GOAL_PANEL_ERPSIZ E6b): every line it booked is reversed,
    /// storno-style — the stock-side sibling of <see cref="DocumentVoid"/> for a document with no ledger effect.</summary>
    public const string StockVoid = "stock_void";

    /// <summary>The job document types <see cref="LedgerVoid"/> may target, keyed by the movement's own external id —
    /// including an earlier <see cref="LedgerEdit"/>, whose one row is the corrected entry (it keeps its kind, so it is
    /// again a standalone collection/disbursement/adjustment).</summary>
    public static readonly IReadOnlySet<string> VoidableLedgerJobTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Collection, Disbursement, LedgerAdjustment, LedgerEdit };

    /// <summary>The document kinds <see cref="DocumentEdit"/> re-books; a document keeps its kind across an edit.</summary>
    public static readonly IReadOnlySet<string> EditableDocumentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { SalesOrder, SalesReturn, PurchaseReceipt };

    /// <summary>The job document types <see cref="DocumentVoid"/>/<see cref="DocumentEdit"/> may target: the three
    /// document kinds themselves, and an earlier <see cref="DocumentEdit"/> (whose own rows are the corrected document).</summary>
    public static readonly IReadOnlySet<string> VoidableDocumentJobTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { SalesOrder, SalesReturn, PurchaseReceipt, DocumentEdit };

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
    /// <param name="options">Booking modes only a trusted caller (a portal endpoint) sets; null for the phone.</param>
    public async Task<Job> IngestAsync(CentralApiDbContext db, Guid tenantId, Job job, bool callerIsAdmin, CancellationToken ct, NativeBookingOptions? options = null)
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
        var booking = new Booking(tenantId, job.ExternalId, now, job.CreatedByUserId) { Options = options ?? new NativeBookingOptions() };
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
                LedgerEdit => callerIsAdmin
                    ? await BookLedgerEditAsync(db, booking, document.RootElement, ct)
                    : "Only company administrators can edit a ledger entry.",
                DocumentVoid => callerIsAdmin
                    ? await BookDocumentVoidAsync(db, booking, document.RootElement, ct)
                    : "Only company administrators can void a document.",
                DocumentEdit => callerIsAdmin
                    ? await BookDocumentEditAsync(db, booking, document.RootElement, ct)
                    : "Only company administrators can edit a document.",
                StockVoid => callerIsAdmin
                    ? await BookStockVoidAsync(db, booking, document.RootElement, ct)
                    : "Only company administrators can void a stock count.",
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
        var reason = Text(document, "reason");
        // The portal (E6b) counts against the stock booked right now, read under the tenant lock; the phone
        // sends what it saw offline, so a sale booked after its count survives (the difference rule above).
        // The mode comes from the trusted caller, never from the payload (Codex #192).
        var againstCurrentLevel = booking.Options.CountAgainstCurrentLevel;
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
            var expected = againstCurrentLevel
                ? (await LevelAsync(db, booking.TenantId, stockCode, ct)).Row.Quantity
                : Number(line, "expectedQuantity") ?? 0;
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
                ["aciklama"] = $"Sayım farkı (sayılan {counted}){(countedBy is null ? "" : $" · {countedBy}")}{(reason is null ? "" : $" · {reason}")}",
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
    /// Cancels one collection/disbursement/manual adjustment (GOAL_PANEL_ERPSIZ E4a, D2's storno
    /// pattern) — the endpoint-level shell around <see cref="VoidLedgerEntryAsync"/> that just needs a
    /// reason, not the original's own fields back.
    /// </summary>
    private async Task<string?> BookLedgerVoidAsync(CentralApiDbContext db, Booking booking, JsonElement document, CancellationToken ct)
    {
        var targetKey = Text(document, "targetKey");
        if (targetKey is null) return "targetKey is required.";
        var reason = Text(document, "reason");
        if (reason is null) return "A void needs a reason.";
        var (error, _, _, _, _) = await VoidLedgerEntryAsync(db, booking, targetKey, reason, Text(document, "occurredAt") ?? booking.Stamp, ct);
        return error;
    }

    /// <summary>
    /// Edits one ledger entry (GOAL_PANEL_ERPSIZ E4c, D11's void+reissue pattern): <see cref="VoidLedgerEntryAsync"/>
    /// cancels the original, then a corrected entry of the <b>same kind</b> is booked (a collection stays a
    /// collection; a manual adjustment stays one and may also change its borç/alacak direction, since that
    /// is the one kind where the caller chooses it in the first place). Both steps share this booking's one
    /// transaction: <see cref="NativeDocumentProcessor.IngestAsync"/> never projects or commits a booking
    /// that returned an error, so an invalid correction rolls the cancellation back too — nothing is left
    /// half-applied.
    /// </summary>
    private async Task<string?> BookLedgerEditAsync(CentralApiDbContext db, Booking booking, JsonElement document, CancellationToken ct)
    {
        var targetKey = Text(document, "targetKey");
        if (targetKey is null) return "targetKey is required.";
        var voidReason = Text(document, "voidReason");
        if (voidReason is null) return "An edit needs a reason for the correction.";
        var amount = decimal.Round(Number(document, "amount") ?? 0, 2);
        if (amount <= 0) return "The corrected entry needs a positive amount.";
        var occurredAt = Text(document, "occurredAt") ?? booking.Stamp;

        var (error, customerCode, sourceType, originalDebit, documentNo) = await VoidLedgerEntryAsync(db, booking, targetKey, voidReason, occurredAt, ct);
        if (error is not null) return error;

        var isAdjustment = sourceType == "Düzeltme";
        var debit = isAdjustment ? Bool(document, "debit") ?? originalDebit : originalDebit;
        var description = Text(document, "description") ?? Text(document, "reason");
        if (isAdjustment && description is null) return "The corrected adjustment needs a reason.";

        await PostToCustomerAsync(db, booking, customerCode!, amount, debit, sourceType!, documentNo ?? booking.ExternalId,
            occurredAt, description, suffix: "edit", ct, isAdjustment ? null : Text(document, "paymentType"));
        return null;
    }

    /// <summary>
    /// The shared half of <see cref="BookLedgerVoidAsync"/> and <see cref="BookLedgerEditAsync"/>: checks
    /// the target's own job is one <see cref="VoidableLedgerJobTypes"/> allows here (E4a's D11 exclusion
    /// — a sale/purchase/return's own row is never voidable here, only via <see cref="BookDocumentVoidAsync"/>),
    /// then reverses it via <see cref="ReverseLedgerRowAsync"/>.
    /// </summary>
    private async Task<(string? Error, string? CustomerCode, string? SourceType, bool Debit, string? DocumentNo)> VoidLedgerEntryAsync(
        CentralApiDbContext db, Booking booking, string targetKey, string reason, string occurredAt, CancellationToken ct)
    {
        // A sale's immediate-payment leg has kind "collection" too, but its job is sales_order/
        // sales_return/purchase_receipt, not a standalone Collection/Disbursement/LedgerAdjustment — excluded here.
        var jobExternalId = ExternalIdOfLedgerKey(targetKey);
        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.TenantId == booking.TenantId && j.ExternalId == jobExternalId, ct);
        if (job is null || !VoidableLedgerJobTypes.Contains(job.DocumentType))
            return ("Only a collection, a disbursement or a manual adjustment can be voided here.", null, null, false, null);
        return await ReverseLedgerRowAsync(db, booking, targetKey, reason, occurredAt, ct);
    }

    /// <summary>
    /// Marks one <c>customerTransactions</c> row <c>voided</c> in place (upserted under its own key, so
    /// the historical amount/type never change) and books a reversing entry — the exact opposite of its
    /// balance effect — under a new key (D2's storno pattern). Shared by <see cref="VoidLedgerEntryAsync"/>
    /// (which checks the row is a standalone payment/adjustment first) and <see cref="BookDocumentVoidAsync"/>
    /// (which already validated the whole document's job type once, for every leg it reverses). Returns
    /// the original row's customer/type/debit/document number so a caller that needs to re-book a
    /// corrected entry of the same kind (the edit path) does not have to read the row a second time.
    /// </summary>
    private async Task<(string? Error, string? CustomerCode, string? SourceType, bool Debit, string? DocumentNo)> ReverseLedgerRowAsync(
        CentralApiDbContext db, Booking booking, string targetKey, string reason, string occurredAt, CancellationToken ct)
    {
        var record = await db.MobileRecords
            .FirstOrDefaultAsync(r => r.TenantId == booking.TenantId && r.Entity == "customerTransactions" && r.RecordKey == targetKey && !r.IsDeleted, ct);
        if (record?.PayloadJson is null) return ("The transaction was not found.", null, null, false, null);

        string customerCode, sourceType, documentNo;
        decimal amount;
        bool debit;
        using (var originalDoc = JsonDocument.Parse(record.PayloadJson))
        {
            var row = originalDoc.RootElement;
            if (Bool(row, "voided") == true) return ("This transaction is already void.", null, null, false, null);
            customerCode = Text(row, "cariKod", "customerCode") ?? "";
            sourceType = Text(row, "type") ?? "Hareket";
            documentNo = Text(row, "evrakNo") ?? booking.ExternalId;
            amount = Math.Abs(Number(row, "meblag") ?? Number(row, "amount") ?? Number(row, "tutar") ?? 0m);
            debit = Bool(row, "borcMu") ?? false;
        }
        if (customerCode.Length == 0) return ("The transaction names no customer.", null, null, false, null);
        if (amount <= 0) return ("The transaction has no amount to reverse.", null, null, false, null);

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
            ["tarih"] = occurredAt,
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
        return (null, customerCode, sourceType, debit, documentNo);
    }

    /// <summary>The job that created a ledger movement, from its id (<c>PostToCustomerAsync</c> always writes
    /// <c>"{externalId}|{suffix}"</c>): strips the last <c>|</c>-separated segment. Mirrors <c>PortalLedger.ExternalIdOf</c>
    /// on the read side; kept separate because the write engine does not depend on the portal's read layer.</summary>
    private static string ExternalIdOfLedgerKey(string movementId)
    {
        var index = movementId.LastIndexOf('|');
        return index < 0 ? movementId : movementId[..index];
    }

    /// <summary>
    /// Cancels one whole sale/purchase/return document (GOAL_PANEL_ERPSIZ E5c, D11) — the endpoint-level
    /// shell around <see cref="VoidDocumentAsync"/>, which just needs a reason.
    /// </summary>
    private async Task<string?> BookDocumentVoidAsync(CentralApiDbContext db, Booking booking, JsonElement document, CancellationToken ct)
    {
        var targetKey = Text(document, "targetKey");
        if (targetKey is null) return "targetKey is required.";
        var reason = Text(document, "reason");
        if (reason is null) return "A void needs a reason.";
        var result = await VoidDocumentAsync(db, booking, targetKey, reason, Text(document, "occurredAt") ?? booking.Stamp, ct);
        return result.Error;
    }

    /// <summary>
    /// Edits one whole sale/purchase/return document (GOAL_PANEL_ERPSIZ E5d, D11's void+reissue pattern, the
    /// document-level sibling of <see cref="BookLedgerEditAsync"/>): <see cref="VoidDocumentAsync"/> reverses the
    /// original's ledger and stock effect, then the corrected <c>document</c> is booked as a new document of the
    /// <b>same kind</b> through the very writer the kind already uses (<see cref="BookSaleAsync"/>,
    /// <see cref="BookSalesReturnAsync"/>, <see cref="BookPurchaseReceiptAsync"/>) — every rule a new document
    /// obeys holds for the correction too. Both halves share this booking's one transaction; a correction the
    /// writer rejects rolls the void back with it.
    ///
    /// <para>The reversal is dated at the original's own date, so a statement for any period shows only the
    /// corrected document. The correction keeps the original's date unless it names another. It gets a
    /// revision number (<c>A-1</c> → <c>A-1-D1</c> → <c>A-1-D2</c>) unless it names a different one: a
    /// native document is keyed by party + number (<c>PortalRecords.NativeDocumentKey</c>), and the voided
    /// original keeps its number, so reusing it would merge the two documents' lines.</para>
    /// </summary>
    private async Task<string?> BookDocumentEditAsync(CentralApiDbContext db, Booking booking, JsonElement document, CancellationToken ct)
    {
        var targetKey = Text(document, "targetKey");
        if (targetKey is null) return "targetKey is required.";
        var voidReason = Text(document, "voidReason");
        if (voidReason is null) return "An edit needs a reason for the correction.";
        var documentType = Text(document, "documentType");
        if (documentType is null || !EditableDocumentTypes.Contains(documentType))
            return "documentType must be sales_order, sales_return or purchase_receipt.";
        if (!document.TryGetProperty("document", out var corrected) || corrected.ValueKind != JsonValueKind.Object)
            return "document (the corrected document) is required.";

        var original = await VoidDocumentAsync(db, booking, targetKey, voidReason, occurredAt: null, ct);
        if (original.Error is not null) return original.Error;
        if (!string.Equals(original.DocumentType, documentType, StringComparison.OrdinalIgnoreCase))
            return "An edit keeps the document's kind (a sale stays a sale).";

        var numberField = string.Equals(documentType, PurchaseReceipt, StringComparison.OrdinalIgnoreCase) ? "invoiceNo" : "mobileDocumentId";
        var requested = Text(corrected, numberField);
        var party = Text(corrected, "supplierCode", "customerCode");
        string number;
        if (requested is not null && !string.Equals(requested, original.DocumentNo, StringComparison.Ordinal))
        {
            if (party is not null && await DocumentNumberInUseAsync(db, booking.TenantId, party, requested, ct))
                return $"Document number {requested} is already used for {party}.";
            number = requested;
        }
        else
        {
            number = RevisionNumber(original.DocumentNo ?? booking.ExternalId);
            // A revision someone already typed by hand (or an earlier chain on another party's number) is skipped.
            while (party is not null && await DocumentNumberInUseAsync(db, booking.TenantId, party, number, ct))
                number = RevisionNumber(number);
        }
        var node = JsonNode.Parse(corrected.GetRawText())!.AsObject();
        node[numberField] = number;
        if (node["occurredAt"] is null) node["occurredAt"] = original.OccurredAt;
        var correctedDocument = JsonSerializer.SerializeToElement(node);

        return documentType.ToLowerInvariant() switch
        {
            SalesOrder => await BookSaleAsync(db, booking, correctedDocument, ct),
            SalesReturn => await BookSalesReturnAsync(db, booking, correctedDocument, ct),
            _ => await BookPurchaseReceiptAsync(db, booking, correctedDocument, ct),
        };
    }

    /// <summary>
    /// Whether <paramref name="party"/> already has a native document numbered <paramref name="documentNo"/> — a
    /// ledger or stock row with that <c>cariKod</c> + <c>evrakNo</c>, the pair <c>PortalRecords.NativeDocumentKey</c>
    /// groups a document by. <c>evrakNo</c> lives only in the JSON payload: on PostgreSQL the <c>jsonb</c> column is
    /// asked by containment (<c>@&gt;</c>; a text <c>LIKE</c> on <c>jsonb</c> does not exist there), elsewhere (the
    /// SQLite test host) the payload text is pre-filtered. Each candidate is then checked exactly.
    /// </summary>
    private static async Task<bool> DocumentNumberInUseAsync(CentralApiDbContext db, Guid tenantId, string party, string documentNo, CancellationToken ct)
    {
        var candidates = await DocumentNumberCandidates(db, tenantId, documentNo).ToListAsync(ct);
        foreach (var payload in candidates)
        {
            using var row = JsonDocument.Parse(payload);
            if (string.Equals(Text(row.RootElement, "evrakNo"), documentNo, StringComparison.Ordinal)
                && string.Equals(Text(row.RootElement, "cariKod", "customerCode"), party, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>The payloads that may carry <paramref name="documentNo"/>; see <see cref="DocumentNumberInUseAsync"/>.</summary>
    internal static IQueryable<string> DocumentNumberCandidates(CentralApiDbContext db, Guid tenantId, string documentNo)
    {
        var rows = db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == tenantId && (r.Entity == "customerTransactions" || r.Entity == "stockTransactions") && !r.IsDeleted
                        && r.PayloadJson != null);
        if (db.Database.IsNpgsql())
        {
            var contained = JsonSerializer.Serialize(new Dictionary<string, string> { ["evrakNo"] = documentNo });
            rows = rows.Where(r => EF.Functions.JsonContains(r.PayloadJson!, contained));
        }
        else
        {
            var escaped = JsonSerializer.Serialize(documentNo);
            var quoted = "\"" + documentNo + "\"";
            rows = rows.Where(r => r.PayloadJson!.Contains(escaped) || r.PayloadJson!.Contains(quoted));
        }
        return rows.Select(r => r.PayloadJson!);
    }

    /// <summary>
    /// Whether a movement row was written by job <paramref name="jobExternalId"/>: its key is exactly
    /// <c>"{job}|{suffix}"</c> or <c>"{job}|{suffix}|void"</c> (the reversal). A bare prefix test would also take the rows
    /// of a job whose own id starts with <c>"{job}|"</c>.
    /// </summary>
    internal static bool OwnedByJob(string recordKey, string jobExternalId)
    {
        if (!recordKey.StartsWith(jobExternalId + "|", StringComparison.Ordinal)) return false;
        var rest = recordKey.AsSpan(jobExternalId.Length + 1);
        var bar = rest.IndexOf('|');
        return bar < 0 ? rest.Length > 0 : bar > 0 && rest[(bar + 1)..].SequenceEqual("void");
    }

    /// <summary>The next revision of a document number: <c>A-1</c> → <c>A-1-D1</c>, <c>A-1-D1</c> → <c>A-1-D2</c>.</summary>
    internal static string RevisionNumber(string documentNo)
    {
        var index = documentNo.LastIndexOf("-D", StringComparison.Ordinal);
        if (index > 0 && int.TryParse(documentNo.AsSpan(index + 2), NumberStyles.None, CultureInfo.InvariantCulture, out var revision))
            return $"{documentNo[..index]}-D{(revision + 1).ToString(CultureInfo.InvariantCulture)}";
        return documentNo + "-D1";
    }

    /// <summary>
    /// Reverses one whole sale/purchase/return document (GOAL_PANEL_ERPSIZ E5c, D11): every
    /// <c>customerTransactions</c> row the document's own job booked — its own line (Satış/Alış/İade) and,
    /// when it settled on the spot, its immediate-payment leg (<see cref="ImmediatePayments"/>) — is
    /// reversed via <see cref="ReverseLedgerRowAsync"/>, and every <c>stockTransactions</c> line the same
    /// job wrote is reversed via <see cref="ReverseStockLineAsync"/>. All in this booking's one
    /// transaction, the way every other native document is booked whole or not at all. A null
    /// <paramref name="occurredAt"/> dates each ledger reversal at its own row's date (the edit path).
    /// Returns the document's kind (its job's document type, or for an earlier edit the kind that edit
    /// re-booked), number and date, so an edit can re-book a correction of the same kind.
    /// </summary>
    private async Task<(string? Error, string? DocumentType, string? DocumentNo, string? OccurredAt)> VoidDocumentAsync(
        CentralApiDbContext db, Booking booking, string targetKey, string reason, string? occurredAt, CancellationToken ct)
    {
        var jobExternalId = ExternalIdOfLedgerKey(targetKey);
        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.TenantId == booking.TenantId && j.ExternalId == jobExternalId, ct);
        var documentType = job is null || !VoidableDocumentJobTypes.Contains(job.DocumentType) ? null : DocumentTypeOf(job);
        if (documentType is null)
            return ("Only a sale, purchase or return document can be voided here.", null, null, null);

        var legs = await db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == booking.TenantId && r.Entity == "customerTransactions"
                        && r.RecordKey.StartsWith(jobExternalId + "|") && !r.IsDeleted)
            .Select(r => new { r.RecordKey, r.PayloadJson })
            .ToListAsync(ct);
        // Keys are "{job}|{suffix}" (and "…|void" once reversed); a prefix alone would also catch another job
        // whose own id happens to start with "{job}|" (Codex #192).
        legs = legs.Where(l => OwnedByJob(l.RecordKey, jobExternalId)).ToList();
        if (legs.Count == 0) return ("The document was not found.", null, null, null);
        string? documentNo = null, documentDate = null;
        var legDates = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var leg in legs)
        {
            if (leg.PayloadJson is null) continue;
            using var legDoc = JsonDocument.Parse(leg.PayloadJson);
            if (Bool(legDoc.RootElement, "voided") == true) return ("This document is already void.", null, null, null);
            legDates[leg.RecordKey] = Text(legDoc.RootElement, "tarih");
            if (leg.RecordKey == targetKey)
            {
                documentNo = Text(legDoc.RootElement, "evrakNo");
                documentDate = Text(legDoc.RootElement, "tarih");
            }
        }

        foreach (var leg in legs)
        {
            var legDate = occurredAt ?? legDates.GetValueOrDefault(leg.RecordKey) ?? booking.Stamp;
            var (error, _, _, _, _) = await ReverseLedgerRowAsync(db, booking, leg.RecordKey, reason, legDate, ct);
            if (error is not null) return (error, null, null, null);
        }

        var lines = await db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == booking.TenantId && r.Entity == "stockTransactions"
                        && r.RecordKey.StartsWith(jobExternalId + "|") && !r.IsDeleted)
            .Select(r => new { r.RecordKey, r.PayloadJson })
            .ToListAsync(ct);
        foreach (var line in lines.Where(l => OwnedByJob(l.RecordKey, jobExternalId)))
        {
            if (line.PayloadJson is null) continue;
            await ReverseStockLineAsync(db, booking, line.RecordKey, line.PayloadJson, reason, ct);
        }
        return (null, documentType, documentNo, documentDate);
    }

    /// <summary>The document kind a job's own rows are: the job's type, or for a <see cref="DocumentEdit"/> the
    /// kind it re-booked (its payload's <c>documentType</c>, which that edit already validated).</summary>
    private static string? DocumentTypeOf(Job job)
    {
        if (!string.Equals(job.DocumentType, DocumentEdit, StringComparison.OrdinalIgnoreCase)) return job.DocumentType;
        if (string.IsNullOrWhiteSpace(job.PayloadJson)) return null;
        using var payload = JsonDocument.Parse(job.PayloadJson);
        return Text(payload.RootElement, "documentType") is { } type && EditableDocumentTypes.Contains(type) ? type : null;
    }

    /// <summary>
    /// Cancels one whole stock count (GOAL_PANEL_ERPSIZ E6b): every <c>stockTransactions</c> line the count's job
    /// booked is reversed via <see cref="ReverseStockLineAsync"/>, in this booking's one transaction.
    /// <c>targetKey</c> is the count's job id.
    /// </summary>
    private async Task<string?> BookStockVoidAsync(CentralApiDbContext db, Booking booking, JsonElement document, CancellationToken ct)
    {
        var targetKey = Text(document, "targetKey");
        if (targetKey is null) return "targetKey is required.";
        var reason = Text(document, "reason");
        if (reason is null) return "A void needs a reason.";

        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.TenantId == booking.TenantId && j.ExternalId == targetKey, ct);
        if (job is null || !string.Equals(job.DocumentType, StockCount, StringComparison.OrdinalIgnoreCase))
            return "Only a stock count can be voided here.";

        var lines = await db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == booking.TenantId && r.Entity == "stockTransactions"
                        && r.RecordKey.StartsWith(targetKey + "|") && !r.IsDeleted)
            .Select(r => new { r.RecordKey, r.PayloadJson })
            .ToListAsync(ct);
        var own = lines.Where(l => l.PayloadJson is not null && OwnedByJob(l.RecordKey, targetKey)
                                   && !l.RecordKey.EndsWith("|void", StringComparison.Ordinal)).ToList();
        if (own.Count == 0) return "The count changed no stock; there is nothing to cancel.";
        foreach (var line in own)
        {
            using var row = JsonDocument.Parse(line.PayloadJson!);
            if (Bool(row.RootElement, "voided") == true) return "This count is already void.";
        }
        foreach (var line in own)
            await ReverseStockLineAsync(db, booking, line.RecordKey, line.PayloadJson!, reason, ct);
        return null;
    }

    /// <summary>
    /// Reverses one stock movement line a voided document created (GOAL_PANEL_ERPSIZ E5c) — the stock-side
    /// counterpart of <see cref="ReverseLedgerRowAsync"/>: marks the original <c>stockTransactions</c> row
    /// <c>voided</c> in place and books an opposite-signed movement under a new key, so the quantity the
    /// original row moved returns to the level (D2's storno pattern, applied to stock).
    /// </summary>
    private async Task ReverseStockLineAsync(CentralApiDbContext db, Booking booking, string lineKey, string payloadJson, string reason, CancellationToken ct)
    {
        JsonElement row;
        using (var lineDoc = JsonDocument.Parse(payloadJson))
        {
            row = lineDoc.RootElement.Clone();
            if (Bool(row, "voided") == true) return;
        }
        var stockCode = Text(row, "stokKod", "urunKod");
        if (stockCode is null) return;
        var signedQuantity = Number(row, "miktar") ?? 0;
        var documentNo = Text(row, "evrakNo");
        var partyCode = Text(row, "cariKod");
        var occurredAt = Text(row, "tarih") ?? booking.Stamp;
        var unitPrice = Number(row, "birimFiyat") ?? 0;
        var lineTotal = Number(row, "tutar") ?? 0;

        var level = await LevelAsync(db, booking.TenantId, stockCode, ct);
        level.Row.Quantity -= signedQuantity;
        level.Row.UpdatedAtUtc = booking.Now;
        level.Row.LastMovementAtUtc = booking.Now;
        booking.AddInventory(level.Row);

        var originalNode = JsonNode.Parse(payloadJson)!.AsObject();
        originalNode["voided"] = true;
        originalNode["voidedByUserId"] = booking.UserId;
        originalNode["voidedAt"] = booking.Stamp;
        originalNode["voidReason"] = reason;
        originalNode["updatedAt"] = booking.Stamp;
        booking.Add("stockTransactions", originalNode);

        var reversedQuantity = -signedQuantity;
        var incoming = reversedQuantity > 0;
        booking.Add("stockTransactions", new JsonObject
        {
            ["id"] = $"{lineKey}|void",
            ["erp"] = "NATIVE",
            ["stokKod"] = stockCode,
            ["urunKod"] = stockCode,
            ["tarih"] = occurredAt,
            ["tip"] = incoming ? 0 : 1,
            ["cins"] = 0,
            ["evrakNo"] = documentNo,
            [incoming ? "girisMiktar" : "cikisMiktar"] = Math.Abs(reversedQuantity),
            ["miktar"] = reversedQuantity,
            ["birimFiyat"] = unitPrice,
            ["tutar"] = lineTotal,
            ["cariKod"] = partyCode,
            [incoming ? "girisDepoNo" : "cikisDepoNo"] = NativeLedgerDefaults.WarehouseNo,
            ["aciklama"] = $"İptal: {reason}",
            ["voidsKey"] = lineKey,
            ["updatedAt"] = booking.Stamp,
        });
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
        public NativeBookingOptions Options { get; init; } = new();
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
