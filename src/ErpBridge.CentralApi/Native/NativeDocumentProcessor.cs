using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    public const string CustomerCard = "customer_card";
    public const string SalesOrder = "sales_order";
    public const string Collection = "collection";

    /// <summary>Marks rows this class produced, the way an agent marks rows with its ERP name.</summary>
    public const string SourceName = "native";

    /// <summary>Document types only a native tenant accepts; an ERP agent would not know them.</summary>
    public static readonly IReadOnlySet<string> CardTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { StockCard, CustomerCard };

    /// <summary>Payment types that settle a sale on the spot, so the sale leaves no open balance.</summary>
    private static readonly HashSet<string> ImmediatePayments = new(StringComparer.OrdinalIgnoreCase)
    {
        "Nakit", "Kredi Kartı", "Kredi Karti", "Bank Kartı", "Banka Kartı", "EFT / Havale", "Havale", "EFT", "POS",
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
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        if (db.Database.IsRelational())
        {
            await db.Tenants.Where(t => t.Id == tenantId)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.NativeLockVersion, t => t.NativeLockVersion + 1), ct);
        }

        var now = DateTimeOffset.UtcNow;
        var booking = new Booking(tenantId, job.ExternalId, now);
        string? error;
        using (var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.PayloadJson) ? "{}" : job.PayloadJson))
        {
            error = job.DocumentType.ToLowerInvariant() switch
            {
                StockCard => callerIsAdmin
                    ? await BookStockCardAsync(db, booking, document.RootElement, ct)
                    : "Only company administrators can create or change products.",
                CustomerCard => await BookCustomerCardAsync(db, booking, document.RootElement, ct),
                SalesOrder => await BookSaleAsync(db, booking, document.RootElement, ct),
                Collection => await BookCollectionAsync(db, booking, document.RootElement, ct),
                // Cash movements, expenses and counts are kept as records but move
                // neither stock nor a customer balance yet.
                _ => null,
            };
        }

        if (error is null) await booking.FinishAsync(db, ct);
        else DiscardLedgerChanges(db);

        job.Status = error is null ? JobStatus.Succeeded : JobStatus.Failed;
        job.LastError = error;
        job.CompletedAtUtc = now;
        db.Jobs.Add(job);
        await db.SaveChangesAsync(ct);

        if (error is null && booking.Sections.Count > 0)
        {
            var sections = booking.Sections
                .Select(pair => new MobileRecordProjector.SectionRows(pair.Key, pair.Value))
                .ToList();
            await _projector.ProjectAsync(db, tenantId, sections, fullUpload: false, SourceName, ct);
            await db.SaveChangesAsync(ct);
        }

        if (transaction is not null) await transaction.CommitAsync(ct);

        if (error is null && booking.Sections.Count > 0)
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
            booking.Add("barcodes", new JsonObject { ["barcode"] = barcode, ["stockCode"] = code });
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

    private async Task<string?> BookSaleAsync(CentralApiDbContext db, Booking booking, JsonElement sale, CancellationToken ct)
    {
        var customer = await ResolveCustomerAsync(db, booking.TenantId, sale, ct);
        if (customer is null) return "The sale names no known customer (customerCode or an exact customer title is required).";
        if (!sale.TryGetProperty("lines", out var lines) || lines.ValueKind != JsonValueKind.Array || lines.GetArrayLength() == 0)
            return "A sale needs at least one line.";

        var documentNo = Text(sale, "mobileDocumentId") ?? booking.ExternalId;
        var occurredAt = Text(sale, "occurredAt") ?? booking.Stamp;
        var description = Text(sale, "description");
        decimal linesTotal = 0;
        var lineNo = 0;
        foreach (var line in lines.EnumerateArray())
        {
            lineNo++;
            var quantity = Number(line, "quantity") ?? 0;
            if (quantity <= 0) return $"Line {lineNo} has no quantity.";
            var stockCode = Text(line, "productCode") ?? await StockCodeForBarcodeAsync(db, booking.TenantId, Text(line, "barcode"), ct);
            if (stockCode is null) return $"Line {lineNo} names no known product.";
            var unitPrice = Number(line, "unitPrice") ?? 0;
            var lineTotal = Number(line, "lineTotal") ?? quantity * unitPrice;
            linesTotal += lineTotal;

            var level = await LevelAsync(db, booking.TenantId, stockCode, ct);
            level.Row.Quantity -= quantity;
            level.Row.UpdatedAtUtc = booking.Now;
            booking.AddInventory(level.Row);
            booking.Add("stockTransactions", new JsonObject
            {
                ["id"] = $"{booking.ExternalId}|{lineNo}",
                ["erp"] = "NATIVE",
                ["stokKod"] = stockCode,
                ["urunKod"] = stockCode,
                ["tarih"] = occurredAt,
                ["tip"] = 1,
                ["cins"] = 0,
                ["evrakNo"] = documentNo,
                ["cikisMiktar"] = quantity,
                ["miktar"] = -quantity,
                ["birimFiyat"] = unitPrice,
                ["tutar"] = lineTotal,
                ["cariKod"] = customer,
                ["cikisDepoNo"] = NativeLedgerDefaults.WarehouseNo,
                ["aciklama"] = description,
                ["updatedAt"] = booking.Stamp,
            });
        }

        var amount = decimal.Round(Number(sale, "amount") ?? linesTotal, 2);
        await PostToCustomerAsync(db, booking, customer, amount, debit: true, "Satış", documentNo, occurredAt, description, suffix: "sale", ct);

        // A sale paid on the spot is also a collection of the same amount: the
        // customer's history shows both and the open balance does not move.
        if (Text(sale, "paymentType") is { } paymentType && ImmediatePayments.Contains(paymentType.Trim()))
            await PostToCustomerAsync(db, booking, customer, amount, debit: false, "Tahsilat", documentNo, occurredAt, paymentType, suffix: "payment", ct);
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
            Text(collection, "description") ?? Text(collection, "paymentType"), suffix: "collection", ct);
        return null;
    }

    private async Task PostToCustomerAsync(
        CentralApiDbContext db, Booking booking, string customerCode, decimal amount, bool debit,
        string type, string documentNo, string occurredAt, string? description, string suffix, CancellationToken ct)
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
            else if (entry.State == EntityState.Modified)
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

    /// <summary>What one document changes, collected before it is projected.</summary>
    private sealed class Booking(Guid tenantId, string externalId, DateTimeOffset now)
    {
        public Guid TenantId { get; } = tenantId;
        public string ExternalId { get; } = externalId;
        public DateTimeOffset Now { get; } = now;
        public string Stamp { get; } = now.ToString("O", CultureInfo.InvariantCulture);
        public Dictionary<string, List<JsonElement>> Sections { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, JsonObject> Customers { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, NativeCustomerBalance> CustomerBalances { get; } = new(StringComparer.OrdinalIgnoreCase);
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
