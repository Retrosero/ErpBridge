using System.Globalization;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Sync;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Expiry;

/// <summary>
/// SKT (son kullanma tarihi) kayıtları: which product sits on which shelf with which expiry date. Entered
/// from the phones, shared by every phone of the company, ERP and ERP-less companies alike; nothing goes to
/// the ERP (knowledge base rule 29).
///
/// <para><b>Writes come as operations</b>, like tasks (<see cref="Tasks.TaskService"/>): a phone queues what
/// the user did offline and sends it in batches. An operation id applied before is answered <c>duplicate</c>;
/// a rejected operation leaves nothing behind (savepoint) and does not stop the rest of the batch.</para>
///
/// <para><b>Change order.</b> Every applied change takes its number from the tenant's
/// <c>tenant_sync_counter</c> inside the writing transaction (rule 11), so <c>changedSinceSeq</c> never steps
/// over a change that commits late. Deletes are tombstones that phones pull like any other change.</para>
/// </summary>
public sealed class StockExpiryService
{
    public const int MaxStockCodeLength = 50;
    public const int MaxBarcodeLength = 50;
    public const int MaxProductNameLength = 200;
    public const int MaxLocationLength = 50;
    public const int MaxWarehouseLength = 100;
    public const int MaxNoteLength = 500;
    public const int MaxCreatorNameLength = 120;
    public const decimal MaxQuantity = 999_999_999m;
    public const int MinYear = 2000;
    public const int MaxYear = 2100;
    public const int MaxOpsPerBatch = 200;
    public const int DefaultTake = 500;
    public const int MaxTake = 1000;

    public const string Upsert = "upsert";
    public const string Delete = "delete";

    public static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // ---- reads ------------------------------------------------------------------------------

    /// <summary>The tenant's records changed after <paramref name="since"/>, deleted ones included, in change order.</summary>
    public async Task<ExpiryListResponse> ListAsync(CentralApiDbContext db, Guid tenantId, long since, int? take, CancellationToken ct)
    {
        var limit = Math.Clamp(take ?? DefaultTake, 1, MaxTake);
        var rows = await db.StockExpiryRecords.AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.UpdatedSeq > since)
            .OrderBy(r => r.UpdatedSeq)
            .Take(limit + 1)
            .ToListAsync(ct);
        var hasMore = rows.Count > limit;
        var page = hasMore ? rows.Take(limit).ToList() : rows;
        return new ExpiryListResponse
        {
            Records = page.Select(ToDto).ToArray(),
            LatestSeq = page.Count > 0 ? page[^1].UpdatedSeq : since,
            HasMore = hasMore,
        };
    }

    // ---- operations -------------------------------------------------------------------------

    private sealed class Rejected(string code, string message) : Exception(message)
    {
        public string Code { get; } = code;
    }

    /// <summary>Validated, trimmed editable fields of an <c>upsert</c>.</summary>
    private sealed record Fields(
        string StockCode, string? Barcode, string? ProductName, string Location, string? Warehouse,
        DateOnly ExpiryDate, decimal? Quantity, string? Note, bool Closed);

    public async Task<ExpiryOpsResponse> ApplyAsync(CentralApiDbContext db, Tenant tenant, MobileUser actor, IReadOnlyList<ExpiryOp> ops, CancellationToken ct)
    {
        var touched = new HashSet<Guid>();
        var results = new List<ExpiryOpResult>(ops.Count);
        var relational = db.Database.IsRelational();
        await using (var transaction = relational ? await db.Database.BeginTransactionAsync(ct) : null)
        {
            foreach (var op in ops.Take(MaxOpsPerBatch))
            {
                if (op.OpId == Guid.Empty)
                {
                    results.Add(Result(op, "rejected", "EXPIRY_OP_ID_REQUIRED", "Her işlemin bir kimliği olmalı."));
                    continue;
                }
                if (await db.StockExpiryOpsApplied.AnyAsync(a => a.TenantId == tenant.Id && a.OpId == op.OpId, ct))
                {
                    results.Add(Result(op, "duplicate"));
                    continue;
                }
                if (transaction is not null) await transaction.CreateSavepointAsync("expiry_op", ct);
                try
                {
                    var now = NowMs();
                    var id = await ApplyOneAsync(db, tenant, actor, op, now, ct);
                    db.StockExpiryOpsApplied.Add(new StockExpiryOpApplied { TenantId = tenant.Id, OpId = op.OpId, UserId = actor.Id, AppliedAtMs = now });
                    await db.SaveChangesAsync(ct);
                    touched.Add(id);
                    results.Add(Result(op, "applied"));
                }
                catch (Rejected rejected)
                {
                    if (transaction is not null) await transaction.RollbackToSavepointAsync("expiry_op", ct);
                    db.ChangeTracker.Clear();
                    results.Add(Result(op, "rejected", rejected.Code, rejected.Message));
                }
            }
            if (transaction is not null) await transaction.CommitAsync(ct);
        }

        var records = touched.Count == 0
            ? []
            : await db.StockExpiryRecords.AsNoTracking()
                .Where(r => r.TenantId == tenant.Id && touched.Contains(r.Id))
                .OrderBy(r => r.UpdatedSeq)
                .ToListAsync(ct);
        return new ExpiryOpsResponse { Results = results.ToArray(), Records = records.Select(ToDto).ToArray() };
    }

    private static ExpiryOpResult Result(ExpiryOp op, string status, string? code = null, string? message = null) =>
        new() { OpId = op.OpId, Status = status, ErrorCode = code, Message = message };

    /// <summary>Applies one operation to the tracked context and returns the id of the record it touched.</summary>
    private static Task<Guid> ApplyOneAsync(CentralApiDbContext db, Tenant tenant, MobileUser actor, ExpiryOp op, long now, CancellationToken ct) =>
        op.Type switch
        {
            Upsert => UpsertAsync(db, tenant, actor, op.Record, now, ct),
            Delete => DeleteAsync(db, tenant, op.Record, now, ct),
            _ => throw new Rejected("EXPIRY_OP_UNKNOWN", $"Bilinmeyen işlem: {Clip(op.Type, 32)}."),
        };

    /// <summary>Creates the record, or overwrites its editable fields (last write wins). A deleted record stays deleted.</summary>
    private static async Task<Guid> UpsertAsync(CentralApiDbContext db, Tenant tenant, MobileUser actor, ExpiryRecordInput? input, long now, CancellationToken ct)
    {
        var id = RecordId(input);
        var fields = Validate(input!);
        // The counter row lock comes first: a second phone creating the same id waits here and then finds
        // the first one's record instead of colliding on the primary key.
        var seq = await NextSeqAsync(db, tenant.Id, ct);
        // Ids are unique across companies; another company's record is simply not there for this caller.
        var record = await db.StockExpiryRecords.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (record is not null && record.TenantId != tenant.Id) throw NotFound();
        if (record is { IsDeleted: true }) throw new Rejected("EXPIRY_NOT_FOUND", "Kayıt silinmiş.");
        if (record is null)
        {
            record = new StockExpiryRecord
            {
                Id = id,
                TenantId = tenant.Id,
                CreatedByUserId = actor.Id,
                CreatedByName = Clip(actor.FullName, MaxCreatorNameLength),
                CreatedAtMs = now,
            };
            db.StockExpiryRecords.Add(record);
        }
        record.StockCode = fields.StockCode;
        record.Barcode = fields.Barcode;
        record.ProductName = fields.ProductName;
        record.Location = fields.Location;
        record.Warehouse = fields.Warehouse;
        record.ExpiryDate = fields.ExpiryDate;
        record.Quantity = fields.Quantity;
        record.Note = fields.Note;
        record.IsClosed = fields.Closed;
        record.UpdatedAtMs = now;
        record.UpdatedSeq = seq;
        return id;
    }

    /// <summary>Soft delete; deleting a deleted record again is a no-op that still answers <c>applied</c>.</summary>
    private static async Task<Guid> DeleteAsync(CentralApiDbContext db, Tenant tenant, ExpiryRecordInput? input, long now, CancellationToken ct)
    {
        var id = RecordId(input);
        var record = await db.StockExpiryRecords.FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenant.Id, ct)
            ?? throw NotFound();
        if (record.IsDeleted) return id;
        record.IsDeleted = true;
        record.UpdatedAtMs = now;
        record.UpdatedSeq = await NextSeqAsync(db, tenant.Id, ct);
        return id;
    }

    private static Guid RecordId(ExpiryRecordInput? input)
    {
        if (input is null) throw Invalid("Kayıt bilgisi eksik.");
        if (input.Id == Guid.Empty) throw Invalid("Kayıt kimliği gerekli.");
        return input.Id;
    }

    private static Fields Validate(ExpiryRecordInput input) => new(
        Required(input.StockCode, MaxStockCodeLength, "Ürün kodu"),
        Optional(input.Barcode, MaxBarcodeLength, "Barkod"),
        Optional(input.ProductName, MaxProductNameLength, "Ürün adı"),
        Required(input.Location, MaxLocationLength, "Reyon/raf"),
        Optional(input.Warehouse, MaxWarehouseLength, "Depo"),
        ExpiryDate(input.ExpiryDate),
        Quantity(input.Quantity),
        Optional(input.Note, MaxNoteLength, "Not"),
        input.Closed ?? false);

    private static string Required(string? value, int max, string field)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0) throw Invalid($"{field} boş olamaz.");
        if (trimmed.Length > max) throw Invalid($"{field} en çok {max} karakter olabilir.");
        return trimmed;
    }

    private static string? Optional(string? value, int max, string field) =>
        string.IsNullOrWhiteSpace(value) ? null : Required(value, max, field);

    private static DateOnly ExpiryDate(string? value)
    {
        if (!DateOnly.TryParseExact((value ?? string.Empty).Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            throw Invalid("Son kullanma tarihi okunamadı.");
        if (date.Year < MinYear || date.Year > MaxYear)
            throw Invalid($"Son kullanma tarihi {MinYear}–{MaxYear} yılları arasında olmalı.");
        return date;
    }

    private static decimal? Quantity(decimal? value)
    {
        if (value is not { } quantity) return null;
        if (quantity < 0 || quantity > MaxQuantity) throw Invalid("Miktar 0 ile 999.999.999 arasında olmalı.");
        if (decimal.Round(quantity, 3) != quantity) throw Invalid("Miktar en çok 3 ondalık basamak içerebilir.");
        return quantity;
    }

    private static Rejected Invalid(string message) => new("EXPIRY_INVALID", message);

    private static Rejected NotFound() => new("EXPIRY_NOT_FOUND", "Kayıt bulunamadı.");

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

    private static ExpiryRecordDto ToDto(StockExpiryRecord r) => new()
    {
        Id = r.Id,
        StockCode = r.StockCode,
        Barcode = r.Barcode,
        ProductName = r.ProductName,
        Location = r.Location,
        Warehouse = r.Warehouse,
        ExpiryDate = r.ExpiryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        // numeric(18,3) reads back as 12.500; send the plain number.
        Quantity = r.Quantity is { } q ? q / 1.000000000000000000000000000000000m : null,
        Note = r.Note,
        Closed = r.IsClosed,
        Deleted = r.IsDeleted,
        CreatedBy = r.CreatedByName,
        CreatedAtMs = r.CreatedAtMs,
        UpdatedAtMs = r.UpdatedAtMs,
        UpdatedSeq = r.UpdatedSeq,
    };
}
