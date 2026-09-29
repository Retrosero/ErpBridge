using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Sync;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.SuspendedSales;

/// <summary>
/// Bekleyen siparişler: carts parked on a phone, shared by every phone of the company so anyone can finish
/// them. ERP and ERP-less companies alike; nothing goes to the ERP (a parked sale is a draft).
///
/// <para><b>Writes come as operations</b>, like SKT records (<see cref="Expiry.StockExpiryService"/>): a
/// phone queues what the user did offline and sends it in batches. An operation id applied before is answered
/// <c>duplicate</c>; a rejected operation leaves nothing behind (savepoint) and does not stop the batch.</para>
///
/// <list type="bullet">
/// <item><c>upsert</c> parks a sale, or overwrites one its creator (or a manager) parked.</item>
/// <item><c>claim</c> — any user opening the sale on a phone. It leaves every list; a sale already claimed or
/// deleted is refused with <c>SUSPENDED_SALE_TAKEN</c> naming who took it, so it is never completed twice.</item>
/// <item><c>delete</c> — cancelling it; only its creator or an ADMIN/MANAGER.</item>
/// </list>
///
/// <para><b>Change order.</b> Every applied change takes its number from the tenant's
/// <c>tenant_sync_counter</c> inside the writing transaction (rule 11). Claims and deletes are tombstones that
/// phones pull like any other change.</para>
/// </summary>
public sealed class SuspendedSaleService
{
    public const int MaxDocNoLength = 20;
    public const int MaxCustomerIdLength = 100;
    public const int MaxCustomerNameLength = 200;
    public const int MaxWarehouseLength = 100;
    public const int MaxNoteLength = 1000;
    public const int MaxCreatorNameLength = 120;
    public const int MaxLines = 500;
    public const int MaxBarcodeLength = 50;
    public const int MaxStockCodeLength = 50;
    public const int MaxProductNameLength = 200;
    public const int MaxLineNoteLength = 500;
    public const decimal MaxQuantity = 999_999_999m;
    public const decimal MaxAmount = 999_999_999_999m;
    public const int MaxOpsPerBatch = 200;
    public const int DefaultTake = 500;
    public const int MaxTake = 1000;

    public const string Upsert = "upsert";
    public const string Claim = "claim";
    public const string Delete = "delete";

    public const string ClosedClaimed = "claimed";
    public const string ClosedDeleted = "deleted";

    private static readonly JsonSerializerOptions LinesJson = new(JsonSerializerDefaults.Web);

    public static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>ADMIN or MANAGER: may delete (cancel) anyone's parked sale.</summary>
    public static bool CanManage(MobileUser user) =>
        RolePermissions.IsAdmin(user) || RolePermissions.Has(user, MobileUserRoles.Manager);

    // ---- reads ------------------------------------------------------------------------------

    /// <summary>The tenant's sales changed after <paramref name="since"/>, tombstones included, in change order.</summary>
    public async Task<SuspendedSaleListResponse> ListAsync(CentralApiDbContext db, Guid tenantId, long since, int? take, CancellationToken ct)
    {
        var limit = Math.Clamp(take ?? DefaultTake, 1, MaxTake);
        var rows = await db.SuspendedSales.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.UpdatedSeq > since)
            .OrderBy(s => s.UpdatedSeq)
            .Take(limit + 1)
            .ToListAsync(ct);
        var hasMore = rows.Count > limit;
        var page = hasMore ? rows.Take(limit).ToList() : rows;
        return new SuspendedSaleListResponse
        {
            Sales = page.Select(ToDto).ToArray(),
            LatestSeq = page.Count > 0 ? page[^1].UpdatedSeq : since,
            HasMore = hasMore,
        };
    }

    // ---- operations -------------------------------------------------------------------------

    private sealed class Rejected(string code, string message) : Exception(message)
    {
        public string Code { get; } = code;
    }

    private sealed record Fields(
        string DocNo, string? CustomerId, string CustomerName, string? Warehouse, string? Note,
        decimal TotalAmount, SuspendedSaleLineDto[] Lines);

    public async Task<SuspendedSaleOpsResponse> ApplyAsync(CentralApiDbContext db, Tenant tenant, MobileUser actor, IReadOnlyList<SuspendedSaleOp> ops, CancellationToken ct)
    {
        var touched = new HashSet<Guid>();
        var results = new List<SuspendedSaleOpResult>(ops.Count);
        var relational = db.Database.IsRelational();
        await using (var transaction = relational ? await db.Database.BeginTransactionAsync(ct) : null)
        {
            foreach (var op in ops.Take(MaxOpsPerBatch))
            {
                if (op.OpId == Guid.Empty)
                {
                    results.Add(Result(op, "rejected", "SUSPENDED_SALE_OP_ID_REQUIRED", "Her işlemin bir kimliği olmalı."));
                    continue;
                }
                if (await db.SuspendedSaleOpsApplied.AnyAsync(a => a.TenantId == tenant.Id && a.OpId == op.OpId, ct))
                {
                    results.Add(Result(op, "duplicate"));
                    continue;
                }
                if (transaction is not null) await transaction.CreateSavepointAsync("suspended_sale_op", ct);
                try
                {
                    var now = NowMs();
                    var id = await ApplyOneAsync(db, tenant, actor, op, now, ct);
                    db.SuspendedSaleOpsApplied.Add(new SuspendedSaleOpApplied { TenantId = tenant.Id, OpId = op.OpId, UserId = actor.Id, AppliedAtMs = now });
                    await db.SaveChangesAsync(ct);
                    touched.Add(id);
                    results.Add(Result(op, "applied"));
                }
                catch (Rejected rejected)
                {
                    if (transaction is not null) await transaction.RollbackToSavepointAsync("suspended_sale_op", ct);
                    db.ChangeTracker.Clear();
                    // The phone learns the sale's current state too, e.g. that someone else claimed it.
                    if (op.Sale is { } sale && sale.Id != Guid.Empty) touched.Add(sale.Id);
                    results.Add(Result(op, "rejected", rejected.Code, rejected.Message));
                }
            }
            if (transaction is not null) await transaction.CommitAsync(ct);
        }

        var sales = touched.Count == 0
            ? []
            : await db.SuspendedSales.AsNoTracking()
                .Where(s => s.TenantId == tenant.Id && touched.Contains(s.Id))
                .OrderBy(s => s.UpdatedSeq)
                .ToListAsync(ct);
        return new SuspendedSaleOpsResponse { Results = results.ToArray(), Sales = sales.Select(ToDto).ToArray() };
    }

    private static SuspendedSaleOpResult Result(SuspendedSaleOp op, string status, string? code = null, string? message = null) =>
        new() { OpId = op.OpId, Status = status, ErrorCode = code, Message = message };

    private static Task<Guid> ApplyOneAsync(CentralApiDbContext db, Tenant tenant, MobileUser actor, SuspendedSaleOp op, long now, CancellationToken ct) =>
        op.Type switch
        {
            Upsert => UpsertAsync(db, tenant, actor, op.Sale, now, ct),
            Claim => CloseAsync(db, tenant, actor, op.Sale, ClosedClaimed, now, ct),
            Delete => CloseAsync(db, tenant, actor, op.Sale, ClosedDeleted, now, ct),
            _ => throw new Rejected("SUSPENDED_SALE_OP_UNKNOWN", $"Bilinmeyen işlem: {Clip(op.Type, 32)}."),
        };

    /// <summary>Parks the sale, or overwrites it (its creator or a manager). A claimed or deleted sale stays closed.</summary>
    private static async Task<Guid> UpsertAsync(CentralApiDbContext db, Tenant tenant, MobileUser actor, SuspendedSaleInput? input, long now, CancellationToken ct)
    {
        var id = SaleId(input);
        var fields = Validate(input!);
        // The counter row lock comes first: a second phone creating the same id waits here and then finds
        // the first one's sale instead of colliding on the primary key.
        var seq = await NextSeqAsync(db, tenant.Id, ct);
        var sale = await db.SuspendedSales.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (sale is not null && sale.TenantId != tenant.Id) throw NotFound();
        if (sale is { IsDeleted: true }) throw Taken(sale);
        if (sale is null)
        {
            sale = new SuspendedSale
            {
                Id = id,
                TenantId = tenant.Id,
                CreatedByUserId = actor.Id,
                CreatedByName = Clip(actor.FullName, MaxCreatorNameLength),
                CreatedAtMs = now,
            };
            db.SuspendedSales.Add(sale);
        }
        else if (sale.CreatedByUserId != actor.Id && !CanManage(actor))
        {
            throw new Rejected("SUSPENDED_SALE_FORBIDDEN", "Bu bekleyen siparişi yalnız oluşturan kişi ya da yönetici değiştirebilir.");
        }
        sale.DocNo = fields.DocNo;
        sale.CustomerId = fields.CustomerId;
        sale.CustomerName = fields.CustomerName;
        sale.Warehouse = fields.Warehouse;
        sale.Note = fields.Note;
        sale.TotalAmount = fields.TotalAmount;
        sale.LinesJson = JsonSerializer.Serialize(fields.Lines, LinesJson);
        sale.LineCount = fields.Lines.Length;
        sale.UpdatedAtMs = now;
        sale.UpdatedSeq = seq;
        return id;
    }

    /// <summary>
    /// Claim (anyone) or delete (creator or manager). Either closes the sale for everyone; a sale already
    /// closed is refused with who took it, except deleting one's own deleted sale again, which is a no-op.
    /// </summary>
    private static async Task<Guid> CloseAsync(CentralApiDbContext db, Tenant tenant, MobileUser actor, SuspendedSaleInput? input, string reason, long now, CancellationToken ct)
    {
        var id = SaleId(input);
        // Counter lock first: two phones claiming the same sale at once queue here, and the second one then
        // reads the first one's claim.
        var seq = await NextSeqAsync(db, tenant.Id, ct);
        var sale = await db.SuspendedSales.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenant.Id, ct)
            ?? throw NotFound();
        if (reason == ClosedDeleted && sale.CreatedByUserId != actor.Id && !CanManage(actor))
            throw new Rejected("SUSPENDED_SALE_FORBIDDEN", "Bu bekleyen siparişi yalnız oluşturan kişi ya da yönetici silebilir.");
        if (sale.IsDeleted)
        {
            if (reason == ClosedDeleted && sale.ClosedReason == ClosedDeleted) return id;
            throw Taken(sale);
        }
        sale.IsDeleted = true;
        sale.ClosedReason = reason;
        sale.ClosedByName = Clip(actor.FullName, MaxCreatorNameLength);
        sale.UpdatedAtMs = now;
        sale.UpdatedSeq = seq;
        return id;
    }

    private static Guid SaleId(SuspendedSaleInput? input)
    {
        if (input is null) throw Invalid("Sipariş bilgisi eksik.");
        if (input.Id == Guid.Empty) throw Invalid("Sipariş kimliği gerekli.");
        return input.Id;
    }

    private static Fields Validate(SuspendedSaleInput input)
    {
        var lines = input.Lines ?? [];
        if (lines.Length == 0) throw Invalid("Bekleyen siparişte en az bir satır olmalı.");
        if (lines.Length > MaxLines) throw Invalid($"Bekleyen siparişte en çok {MaxLines} satır olabilir.");
        return new Fields(
            Required(input.DocNo, MaxDocNoLength, "Sipariş numarası"),
            Optional(input.CustomerId, MaxCustomerIdLength, "Cari"),
            Optional(input.CustomerName, MaxCustomerNameLength, "Cari adı") ?? "Perakende Müşteri",
            Optional(input.Warehouse, MaxWarehouseLength, "Depo"),
            Optional(input.Note, MaxNoteLength, "Not"),
            Amount(input.TotalAmount ?? 0m, "Toplam"),
            lines.Select(ValidateLine).ToArray());
    }

    private static SuspendedSaleLineDto ValidateLine(SuspendedSaleLineDto line)
    {
        if (line.Quantity <= 0 || line.Quantity > MaxQuantity) throw Invalid("Satır miktarı 0'dan büyük olmalı.");
        if (decimal.Round(line.Quantity, 3) != line.Quantity) throw Invalid("Miktar en çok 3 ondalık basamak içerebilir.");
        if (line.LineDiscountPercent < 0 || line.LineDiscountPercent > 100) throw Invalid("Satır iskontosu 0 ile 100 arasında olmalı.");
        return new SuspendedSaleLineDto
        {
            Barcode = Required(line.Barcode, MaxBarcodeLength, "Barkod"),
            StockCode = Optional(line.StockCode, MaxStockCodeLength, "Ürün kodu"),
            ProductName = Optional(line.ProductName, MaxProductNameLength, "Ürün adı"),
            Quantity = line.Quantity,
            Price = Amount(line.Price, "Fiyat"),
            LineDiscountPercent = line.LineDiscountPercent,
            Note = Optional(line.Note, MaxLineNoteLength, "Satır notu"),
        };
    }

    private static decimal Amount(decimal value, string field)
    {
        if (value < 0 || value > MaxAmount) throw Invalid($"{field} geçersiz.");
        return decimal.Round(value, 4);
    }

    private static string Required(string? value, int max, string field)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0) throw Invalid($"{field} boş olamaz.");
        if (trimmed.Length > max) throw Invalid($"{field} en çok {max} karakter olabilir.");
        return trimmed;
    }

    private static string? Optional(string? value, int max, string field) =>
        string.IsNullOrWhiteSpace(value) ? null : Required(value, max, field);

    private static Rejected Invalid(string message) => new("SUSPENDED_SALE_INVALID", message);

    private static Rejected NotFound() => new("SUSPENDED_SALE_NOT_FOUND", "Bekleyen sipariş bulunamadı.");

    private static Rejected Taken(SuspendedSale sale) => new(
        "SUSPENDED_SALE_TAKEN",
        sale.ClosedReason == ClosedDeleted
            ? $"Bu bekleyen sipariş {Who(sale)} silindi."
            : $"Bu bekleyen sipariş {Who(sale)} açıldı.");

    private static string Who(SuspendedSale sale) =>
        string.IsNullOrWhiteSpace(sale.ClosedByName) ? "başka bir kullanıcı tarafından" : $"{sale.ClosedByName} tarafından";

    private static string Clip(string? value, int max)
    {
        var trimmed = (value ?? string.Empty).Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }

    private static async Task<long> NextSeqAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct) =>
        db.Database.IsRelational()
            ? await MobileRecordProjector.ReserveAsync(db, tenantId, 1, ct)
            // The in-memory host has no transactions or counter row; time order is enough there.
            : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000 + Interlocked.Increment(ref _inMemorySeq) % 1000;

    private static long _inMemorySeq;

    // ---- mapping ----------------------------------------------------------------------------

    private static SuspendedSaleDto ToDto(SuspendedSale s) => new()
    {
        Id = s.Id,
        DocNo = s.DocNo,
        CustomerId = s.CustomerId,
        CustomerName = s.CustomerName,
        Warehouse = s.Warehouse,
        Note = s.Note,
        // numeric(18,2) reads back with trailing zeros; send the plain number.
        TotalAmount = s.TotalAmount / 1.000000000000000000000000000000000m,
        // A tombstone's lines are of no use to the phone.
        Lines = s.IsDeleted ? [] : JsonSerializer.Deserialize<SuspendedSaleLineDto[]>(s.LinesJson, LinesJson) ?? [],
        Deleted = s.IsDeleted,
        ClosedReason = s.ClosedReason,
        ClosedBy = s.ClosedByName,
        CreatedBy = s.CreatedByName,
        CreatedByUserId = s.CreatedByUserId,
        CreatedAtMs = s.CreatedAtMs,
        UpdatedAtMs = s.UpdatedAtMs,
        UpdatedSeq = s.UpdatedSeq,
    };
}
