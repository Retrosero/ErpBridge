using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Team;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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

/// <summary>Why a request went to its assignee (<see cref="CatalogOrders.AssigneeAsync"/>); the panel's "who hears of it" names it.</summary>
public static class CatalogAssigneeSources
{
    public const string Responsible = "responsible";
    public const string Salesperson = "salesperson";
    public const string Address = "address";
    public const string Default = "default";
    public const string Route = "route";

    /// <summary>Nobody matched: the catalog managers alone hear of the request.</summary>
    public const string ManagersOnly = "managersOnly";
}

/// <summary>A request's assignee (null: nobody) and the rule that chose them (<see cref="CatalogAssigneeSources"/>).</summary>
public sealed record CatalogAssignee(Guid? UserId, string Source);

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
    /// The staff member a new request goes to and why (GOAL_MUSTERI_KATALOGU §5.3, S11), the first that names an active,
    /// undeleted user: the account's responsible user; the user mapped to the customer's salesperson code (Mikro
    /// <c>cari_temsilci_kodu</c>; <c>TargetFacts</c>' <c>userOfCode</c>); the one mapped to the salesperson on the customer's
    /// addresses (<c>adr_temsilci_kodu</c>); the one mapped to the company's default salesperson
    /// (<see cref="ErpWriteSettings.DefaultSalespersonCode"/>); the first active assignee of the current route plan that stops
    /// at the customer (the newest by start date; ERP-less companies too). Else nobody: only the catalog managers hear of it.
    /// One query each for users, settings and mappings; the route plans come from a cached mirror, so a request scans nothing.
    /// </summary>
    public static async Task<CatalogAssignee> AssigneeAsync(
        CentralApiDbContext db, IMemoryCache cache, Guid tenantId, Guid? responsibleUserId, string customerCode, PortalLedger.Customer? card, CancellationToken ct)
    {
        var users = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenantId && u.IsActive && u.DeletedAtUtc == null)
            .Select(u => new { u.Id, u.Username })
            .ToListAsync(ct);
        var active = users.Select(u => u.Id).ToHashSet();
        if (responsibleUserId is { } responsible && active.Contains(responsible)) return new(responsible, CatalogAssigneeSources.Responsible);

        var codes = new List<(string Code, string Source)>();
        if (Code(card?.SalespersonCode) is { } own) codes.Add((own, CatalogAssigneeSources.Salesperson));
        if (Code(card?.AddressSalespersonCode) is { } onAddress) codes.Add((onAddress, CatalogAssigneeSources.Address));
        var companyDefault = Code(await db.ErpWriteSettings.AsNoTracking()
            .Where(s => s.TenantId == tenantId).Select(s => s.DefaultSalespersonCode).FirstOrDefaultAsync(ct));
        if (companyDefault is not null) codes.Add((companyDefault, CatalogAssigneeSources.Default));
        if (codes.Count > 0)
        {
            var mappings = await db.MobileUserErpMappings.AsNoTracking()
                .Where(m => m.TenantId == tenantId && m.SalespersonCode != null && m.SalespersonCode != "")
                .Select(m => new { m.UserId, m.SalespersonCode })
                .ToListAsync(ct);
            foreach (var (code, source) in codes)
            {
                var found = mappings.FirstOrDefault(m => active.Contains(m.UserId) && string.Equals(m.SalespersonCode!.Trim(), code, StringComparison.OrdinalIgnoreCase));
                if (found is not null) return new(found.UserId, source);
            }
        }

        var today = PortalReports.IstanbulDay(DateTimeOffset.UtcNow).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var byName = users.GroupBy(u => u.Username, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
        var plans = (await RoutePlansAsync(db, cache, tenantId, ct))
            .Where(p => p.IsActive && (p.StartDate.Length == 0 || string.CompareOrdinal(p.StartDate, today) <= 0) && p.Customers.Contains(customerCode))
            .OrderByDescending(p => p.StartDate, StringComparer.Ordinal)
            .ThenByDescending(p => p.UpdatedAtUtc, StringComparer.Ordinal)
            .ThenBy(p => p.PlanId, StringComparer.Ordinal);
        foreach (var plan in plans)
            foreach (var username in plan.Assignees)
                if (byName.TryGetValue(username, out var id)) return new(id, CatalogAssigneeSources.Route);
        return new(null, CatalogAssigneeSources.ManagersOnly);
    }

    private static string? Code(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>A route plan as the assignee rule needs it (<c>mobile_records</c> <c>routePlans</c>, <see cref="TeamDocumentProcessor"/>).</summary>
    private sealed record RoutePlan(string PlanId, bool IsActive, string StartDate, string UpdatedAtUtc, IReadOnlySet<string> Customers, IReadOnlyList<string> Assignees);

    private sealed record CachedRoutePlans(long Version, IReadOnlyList<RoutePlan> Plans);

    private static readonly string[] RoutePlanEntities = [TeamDocumentProcessor.RoutePlansSection];

    /// <summary>The company's route plans, from a mirror that reads only the rows changed since its last read.</summary>
    private static async Task<IReadOnlyList<RoutePlan>> RoutePlansAsync(CentralApiDbContext db, IMemoryCache cache, Guid tenantId, CancellationToken ct)
    {
        var mirror = PortalRecordMirror<RoutePlan>.For(cache, "catalog-route-plans", tenantId, RoutePlanEntities, ParseRoutePlan);
        var key = ("catalog-route-plans", tenantId);
        cache.TryGetValue(key, out CachedRoutePlans? cached);
        var plans = await mirror.RefreshAsync(db, cached?.Version, ct);
        if (plans is null && cached is not null) return cached.Plans;
        var current = plans ?? [];
        cache.Set(key, new CachedRoutePlans(mirror.Version, current), TimeSpan.FromMinutes(30));
        return current;
    }

    /// <summary>A plan without stops or assignees routes nobody, so it is not kept.</summary>
    private static RoutePlan? ParseRoutePlan(string entity, JsonElement plan)
    {
        var customers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (plan.TryGetProperty("stops", out var stops) && stops.ValueKind == JsonValueKind.Array)
            foreach (var stop in stops.EnumerateArray())
                if (Code(AndroidEndpoints.GetString(stop, "customerCode")) is { } code) customers.Add(code);
        List<string> assignees = plan.TryGetProperty("assignees", out var names) && names.ValueKind == JsonValueKind.Array
            ? [.. names.EnumerateArray().Where(a => a.ValueKind == JsonValueKind.String).Select(a => a.GetString()!.Trim()).Where(a => a.Length > 0)]
            : [];
        return customers.Count == 0 || assignees.Count == 0
            ? null
            : new RoutePlan(
                AndroidEndpoints.GetString(plan, "planId") ?? string.Empty,
                AndroidEndpoints.GetBoolean(plan, "isActive") ?? true,
                AndroidEndpoints.GetString(plan, "startDate") ?? string.Empty,
                AndroidEndpoints.GetString(plan, "updatedAtUtc") ?? string.Empty,
                customers,
                assignees);
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
