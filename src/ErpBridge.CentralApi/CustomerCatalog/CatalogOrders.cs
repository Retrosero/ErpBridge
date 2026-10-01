using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>
/// One line of an order request as the server priced it when the customer sent it (<c>catalog_orders.LinesJson</c>).
/// Staff turn the request into a sale from these: the stock code, the list price of the request's list and the
/// customer's discount on the line (GOAL_MUSTERI_KATALOGU §5.3).
/// </summary>
public sealed record CatalogOrderLine(
    [property: JsonPropertyName("stockCode")] string StockCode,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("unit")] string? Unit,
    [property: JsonPropertyName("quantity")] decimal Quantity,
    [property: JsonPropertyName("cartonQuantity")] int? CartonQuantity,
    [property: JsonPropertyName("listPrice")] decimal ListPrice,
    [property: JsonPropertyName("discountPercent")] decimal DiscountPercent,
    [property: JsonPropertyName("net")] decimal Net,
    [property: JsonPropertyName("vatRate")] decimal VatRate,
    [property: JsonPropertyName("gross")] decimal Gross,
    [property: JsonPropertyName("discount")] decimal Discount,
    [property: JsonPropertyName("vat")] decimal Vat,
    [property: JsonPropertyName("total")] decimal Total);

/// <summary>The rules around a customer's order request (GOAL_MUSTERI_KATALOGU §5.2, §5.3): its number, its lines, who hears of it.</summary>
public static class CatalogOrders
{
    public const string NumberPrefix = "KT-";

    /// <summary>The company code's alphabet: 32 symbols, no I or O, no 0 or 1 (read out over the phone).</summary>
    private const string NumberAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private const int NumberLength = 6;

    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public static string NewNumber() => NumberPrefix + string.Create(NumberLength, 0, static (span, _) =>
    {
        for (var i = 0; i < span.Length; i++) span[i] = NumberAlphabet[RandomNumberGenerator.GetInt32(NumberAlphabet.Length)];
    });

    /// <summary>A number no request of the company has yet; the unique index backs it up.</summary>
    public static async Task<string> FreeNumberAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct)
    {
        while (true)
        {
            var no = NewNumber();
            if (!await db.CatalogOrders.AnyAsync(o => o.TenantId == tenantId && o.No == no, ct)) return no;
        }
    }

    public static string LinesJson(IEnumerable<CatalogOrderLine> lines) => JsonSerializer.Serialize(lines, Web);

    public static IReadOnlyList<CatalogOrderLine> Lines(CatalogOrder order) =>
        JsonSerializer.Deserialize<List<CatalogOrderLine>>(order.LinesJson, Web) ?? [];

    /// <summary>
    /// Holds the row for the rest of the caller's transaction (a no-op update is the portable <c>FOR UPDATE</c>: a row lock
    /// in PostgreSQL, the write lock in SQLite), so two writers of the same request — or of the same account's requests —
    /// take turns. Nothing to hold outside a relational transaction.
    /// </summary>
    public static async Task LockAsync(CentralApiDbContext db, Guid tenantId, Guid orderId, CancellationToken ct)
    {
        if (db.Database.IsRelational() && db.Database.CurrentTransaction is not null)
            await db.CatalogOrders.Where(o => o.Id == orderId && o.TenantId == tenantId)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.UpdatedAtMs, o => o.UpdatedAtMs), ct);
    }

    /// <summary>
    /// The staff member a new request goes to: the account's responsible user while active, else the user whose
    /// salesperson code is the customer's (Mikro <c>cari_temsilci_kodu</c>; <c>TargetFacts</c>' <c>userOfCode</c>), else none.
    /// </summary>
    public static async Task<Guid?> AssigneeAsync(CentralApiDbContext db, Guid tenantId, Guid? responsibleUserId, string? salespersonCode, CancellationToken ct)
    {
        var active = db.MobileUsers.AsNoTracking().Where(u => u.TenantId == tenantId && u.IsActive && u.DeletedAtUtc == null);
        if (responsibleUserId is { } responsible && await active.AnyAsync(u => u.Id == responsible, ct)) return responsible;
        var code = salespersonCode?.Trim();
        if (string.IsNullOrEmpty(code)) return null;
        var mappings = await db.MobileUserErpMappings.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.SalespersonCode != null && m.SalespersonCode != "")
            .Select(m => new { m.UserId, m.SalespersonCode })
            .ToListAsync(ct);
        var candidates = mappings.Where(m => string.Equals(m.SalespersonCode!.Trim(), code, StringComparison.OrdinalIgnoreCase))
            .Select(m => m.UserId).ToList();
        if (candidates.Count == 0) return null;
        var activeIds = await active.Where(u => candidates.Contains(u.Id)).Select(u => u.Id).ToListAsync(ct);
        var found = candidates.FirstOrDefault(activeIds.Contains);
        return found == Guid.Empty ? null : found;
    }

    /// <summary>Active users who manage the catalog: they see every request and hear of every new one.</summary>
    public static async Task<List<Guid>> ManagersAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct)
    {
        var users = await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
            .Where(u => u.TenantId == tenantId && u.IsActive && u.DeletedAtUtc == null)
            .ToListAsync(ct);
        // The permission is locked to its role defaults, so the roles alone decide it.
        return users.Where(RolePermissions.CanManageCustomerCatalog).Select(u => u.Id).ToList();
    }

    /// <summary>"Yeni müşteri siparişi: {cari}" / "{No} · {n} kalem · {toplam} TL", one per recipient (§5.3).</summary>
    public static void AddNotifications(CentralApiDbContext db, CatalogOrder order, IEnumerable<Guid> recipients, long seq)
    {
        foreach (var userId in recipients.Where(id => id != Guid.Empty).Distinct())
        {
            db.UserNotifications.Add(new UserNotification
            {
                TenantId = order.TenantId,
                UserId = userId,
                Kind = UserNotificationKinds.CatalogOrderNew,
                Title = Clip("Yeni müşteri siparişi: " + order.CustomerName, 200),
                Body = Clip($"{order.No} · {order.LineCount} kalem · {order.Total.ToString("N2", Turkish)} TL", 500),
                TaskId = null,
                CreatedAtMs = order.SubmittedAtMs,
                Seq = seq,
            });
        }
    }

    private static string Clip(string value, int max) => value.Length <= max ? value : value[..max];
}
