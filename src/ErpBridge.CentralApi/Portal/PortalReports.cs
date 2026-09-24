using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Team;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Portal;

/// <summary>
/// Figures for the company manager portal (Faz 41). Everything is read from what the
/// server already holds for the phones: documents in <c>jobs</c>, cards and team data in
/// <c>mobile_records</c> — so the portal and the phones cannot disagree.
/// </summary>
public static class PortalReports
{
    public const string SalesOrder = "sales_order";
    public const string Collection = "collection";
    public const string Disbursement = "disbursement";

    /// <summary>
    /// A return's document types: <c>sales_return</c> without an ERP. With one, the lined <c>sales_return</c> a phone
    /// sends since goal ERP yazım Y4b (1.5.237) and the cash-book <c>return</c> older phones send — a phone sends one
    /// or the other, never both.
    /// </summary>
    public static IReadOnlyList<string> ReturnDocumentTypes(Tenant tenant) =>
        tenant.DataSource == TenantDataSources.Native
            ? [Native.NativeDocumentProcessor.SalesReturn]
            : [Native.NativeDocumentProcessor.SalesReturn, "return"];

    /// <summary>
    /// A document dated on a day can reach the server days later from an offline phone;
    /// jobs received this long after the day still count for it.
    /// </summary>
    public static readonly TimeSpan OfflineTolerance = TimeSpan.FromDays(7);

    public const int MaxRangeDays = 92;
    public const int MaxBalanceRows = 500;
    public const int MaxStockRows = 200;

    private static readonly TimeZoneInfo Istanbul = ResolveIstanbul();

    // ---- dates ------------------------------------------------------------

    /// <summary>The Istanbul day a server time falls on.</summary>
    public static DateOnly IstanbulDay(DateTimeOffset instant) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, Istanbul).DateTime);

    /// <summary>The UTC moment an Istanbul day starts; <c>day + 1</c> gives its (exclusive) end.</summary>
    public static DateTimeOffset IstanbulDayStartUtc(DateOnly day) =>
        new(TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), Istanbul), TimeSpan.Zero);

    /// <summary>
    /// The business day of a document: its <c>occurredAt</c> as the phone wrote it, in
    /// Istanbul time, or the day the server received it when that is missing or unreadable.
    /// Phones write ISO dates (sales) and <c>dd.MM.yyyy HH:mm</c> (the cash book).
    /// </summary>
    public static DateOnly BusinessDate(string? payloadJson, DateTimeOffset receivedAtUtc)
    {
        var occurredAt = ReadString(payloadJson, "occurredAt");
        if (occurredAt is not null)
        {
            string[] local = ["dd.MM.yyyy HH:mm", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.FFFFFFF"];
            if (DateTime.TryParseExact(occurredAt, local, CultureInfo.InvariantCulture, DateTimeStyles.None, out var wall))
                return DateOnly.FromDateTime(wall);
            if (DateTimeOffset.TryParse(occurredAt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var instant))
                return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, Istanbul).DateTime);
        }
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(receivedAtUtc, Istanbul).DateTime);
    }

    /// <summary>1 = Monday … 7 = Sunday, the numbering route stops use.</summary>
    public static int RouteDay(DateOnly date) => date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;

    // ---- documents --------------------------------------------------------

    public sealed record MoneyDocument(string DocumentType, Guid? CreatedByUserId, DateOnly BusinessDate, decimal Amount);

    /// <summary>
    /// Money documents dated within [from, to]. Without an ERP only booked documents count;
    /// with one, documents still on their way to the ERP count too — failed ones never do.
    /// Purchase payments are left out of disbursements: they belong to the purchase.
    /// </summary>
    public static async Task<List<MoneyDocument>> MoneyDocumentsAsync(
        CentralApiDbContext db, Tenant tenant, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var returnTypes = ReturnDocumentTypes(tenant);
        var types = new[] { SalesOrder, Collection, Disbursement }.Concat(returnTypes).ToArray();
        var statuses = tenant.DataSource == TenantDataSources.Native
            ? new[] { JobStatus.Succeeded }
            : new[] { JobStatus.Pending, JobStatus.Processing, JobStatus.Succeeded };

        var windowStart = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddDays(-1);
        var windowEnd = new DateTimeOffset(to.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).AddDays(1) + OfflineTolerance;

        var query = db.Jobs.AsNoTracking()
            .Where(j => j.TenantId == tenant.Id && types.Contains(j.DocumentType) && statuses.Contains(j.Status));
        // PostgreSQL filters the window itself; SQLite (tests) cannot translate DateTimeOffset
        // comparisons, so there the same window is applied after reading.
        var sqlite = db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
        if (!sqlite) query = query.Where(j => j.EnqueuedAtUtc >= windowStart && j.EnqueuedAtUtc < windowEnd);

        var rows = await query
            .Select(j => new { j.DocumentType, j.CreatedByUserId, j.EnqueuedAtUtc, j.PayloadJson })
            .ToListAsync(ct);

        var result = new List<MoneyDocument>();
        foreach (var row in rows)
        {
            if (row.EnqueuedAtUtc < windowStart || row.EnqueuedAtUtc >= windowEnd) continue;
            if (row.DocumentType == Disbursement
                && string.Equals(ReadString(row.PayloadJson, "approvalKind"), ApprovalKinds.Purchase, StringComparison.OrdinalIgnoreCase))
                continue;
            var date = BusinessDate(row.PayloadJson, row.EnqueuedAtUtc);
            if (date < from || date > to) continue;
            var documentType = returnTypes.Contains(row.DocumentType) ? "return" : row.DocumentType;
            result.Add(new MoneyDocument(documentType, row.CreatedByUserId, date, ReadDecimal(row.PayloadJson, "amount") ?? 0m));
        }
        return result;
    }

    public static void Add(PortalMoneyLine line, decimal amount)
    {
        line.Count++;
        line.Amount += amount;
    }

    public static PortalMoneyLine LineFor(string documentType, PortalMoneyLine sales, PortalMoneyLine collections, PortalMoneyLine disbursements, PortalMoneyLine returns) =>
        documentType switch
        {
            SalesOrder => sales,
            Collection => collections,
            Disbursement => disbursements,
            _ => returns,
        };

    // ---- routes -------------------------------------------------------------

    public sealed record RouteRecord(string Entity, string? PayloadJson);

    /// <summary>Planned stops of the day, joined with what was recorded, plus visits outside the plan.</summary>
    public static async Task<List<PortalVisitRow>> VisitsAsync(CentralApiDbContext db, Guid tenantId, DateOnly date, CancellationToken ct) =>
        BuildVisits(await RouteRecordsAsync(db, tenantId, ct), date);

    /// <summary>Reads the tenant's plans and visits once, for reports over many days.</summary>
    public static Task<List<RouteRecord>> RouteRecordsAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct) =>
        db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == tenantId && !r.IsDeleted
                        && (r.Entity == TeamDocumentProcessor.RoutePlansSection || r.Entity == TeamDocumentProcessor.RouteVisitsSection))
            .Select(r => new RouteRecord(r.Entity, r.PayloadJson))
            .ToListAsync(ct);

    public static List<PortalVisitRow> BuildVisits(IReadOnlyCollection<RouteRecord> records, DateOnly date)
    {
        var day = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var routeDay = RouteDay(date);
        var visits = new Dictionary<(string StopId, string Username), JsonElement>();
        var unplanned = new List<JsonElement>();
        var documents = new List<JsonDocument>();
        try
        {
            foreach (var record in records.Where(r => r.Entity == TeamDocumentProcessor.RouteVisitsSection && r.PayloadJson is not null))
            {
                var parsed = JsonDocument.Parse(record.PayloadJson!);
                documents.Add(parsed);
                var visit = parsed.RootElement;
                if (AndroidEndpoints.GetString(visit, "visitDate") != day) continue;
                var stopId = AndroidEndpoints.GetString(visit, "stopId") ?? string.Empty;
                var username = AndroidEndpoints.GetString(visit, "username") ?? string.Empty;
                if (stopId.Length > 0) visits[(stopId, username.ToLowerInvariant())] = visit;
                else unplanned.Add(visit);
            }

            var rows = new List<PortalVisitRow>();
            foreach (var record in records.Where(r => r.Entity == TeamDocumentProcessor.RoutePlansSection && r.PayloadJson is not null))
            {
                using var parsed = JsonDocument.Parse(record.PayloadJson!);
                var plan = parsed.RootElement;
                if (AndroidEndpoints.GetBoolean(plan, "isActive") == false) continue;
                var startDate = AndroidEndpoints.GetString(plan, "startDate");
                if (!string.IsNullOrEmpty(startDate) && string.CompareOrdinal(startDate, day) > 0) continue;
                var planId = AndroidEndpoints.GetString(plan, "planId") ?? string.Empty;
                var planName = AndroidEndpoints.GetString(plan, "name") ?? string.Empty;
                if (!plan.TryGetProperty("stops", out var stops) || stops.ValueKind != JsonValueKind.Array) continue;
                if (!plan.TryGetProperty("assignees", out var assignees) || assignees.ValueKind != JsonValueKind.Array) continue;

                foreach (var stop in stops.EnumerateArray().Where(s => AndroidEndpoints.GetInt32(s, "dayOfWeek") == routeDay))
                {
                    var stopId = AndroidEndpoints.GetString(stop, "stopId") ?? string.Empty;
                    foreach (var assignee in assignees.EnumerateArray())
                    {
                        var username = assignee.GetString() ?? string.Empty;
                        var row = new PortalVisitRow
                        {
                            Username = username,
                            PlanId = planId,
                            PlanName = planName,
                            StopId = stopId,
                            CustomerCode = AndroidEndpoints.GetString(stop, "customerCode") ?? string.Empty,
                            CustomerName = AndroidEndpoints.GetString(stop, "customerName") ?? string.Empty,
                            VisitOrder = AndroidEndpoints.GetInt32(stop, "visitOrder") ?? 0,
                        };
                        if (visits.Remove((stopId, username.ToLowerInvariant()), out var visit)) Fill(row, visit);
                        rows.Add(row);
                    }
                }
            }

            // A visit on a stop that is no longer in the day's plan still happened.
            foreach (var visit in visits.Values.Concat(unplanned))
            {
                var row = new PortalVisitRow
                {
                    Username = AndroidEndpoints.GetString(visit, "username") ?? string.Empty,
                    PlanId = AndroidEndpoints.GetString(visit, "planId") ?? string.Empty,
                    StopId = AndroidEndpoints.GetString(visit, "stopId") ?? string.Empty,
                    CustomerCode = AndroidEndpoints.GetString(visit, "customerCode") ?? string.Empty,
                    Planned = false,
                };
                Fill(row, visit);
                rows.Add(row);
            }

            return rows
                .OrderBy(r => r.Username, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Planned ? 0 : 1)
                .ThenBy(r => r.VisitOrder)
                .ToList();
        }
        finally
        {
            foreach (var document in documents) document.Dispose();
        }
    }

    private static void Fill(PortalVisitRow row, JsonElement visit)
    {
        row.Status = AndroidEndpoints.GetString(visit, "status") ?? "PENDING";
        row.Note = AndroidEndpoints.GetString(visit, "note") ?? string.Empty;
        row.CompletedAt = visit.TryGetProperty("completedAt", out var at) && at.TryGetInt64(out var ms) ? ms : null;
    }

    // ---- cards --------------------------------------------------------------

    /// <summary>The same balances the customers page lists (<see cref="PortalLedger.CustomersAsync(CentralApiDbContext, IMemoryCache, Guid, string, CancellationToken)"/>).</summary>
    public static async Task<PortalBalancesResponse> BalancesAsync(CentralApiDbContext db, IMemoryCache cache, Tenant tenant, string? search, CancellationToken ct)
    {
        var customers = await PortalLedger.CustomersAsync(db, cache, tenant.Id, tenant.DataSource, ct);

        var rows = new List<PortalBalanceRow>();
        foreach (var customer in customers.Values)
        {
            if (customer.Balance == 0m) continue;
            if (!Matches(search, customer.Code, customer.Title)) continue;
            rows.Add(new PortalBalanceRow { CustomerCode = customer.Code, Title = customer.Title, Balance = customer.Balance });
        }

        return new PortalBalancesResponse
        {
            TotalReceivable = rows.Where(r => r.Balance > 0).Sum(r => r.Balance),
            TotalPayable = rows.Where(r => r.Balance < 0).Sum(r => -r.Balance),
            Rows = rows.OrderByDescending(r => Math.Abs(r.Balance)).Take(MaxBalanceRows).ToList(),
            Truncated = rows.Count > MaxBalanceRows,
        };
    }

    public static async Task<PortalStockResponse> StockAsync(CentralApiDbContext db, Guid tenantId, string? search, bool outOfStockOnly, CancellationToken ct)
    {
        var records = await db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == tenantId && !r.IsDeleted && (r.Entity == "stocks" || r.Entity == "inventory"))
            .Select(r => new { r.Entity, r.StockKey, r.PayloadJson })
            .ToListAsync(ct);

        var quantities = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in records.Where(r => r.Entity == "inventory" && r.StockKey is not null && r.PayloadJson is not null))
        {
            using var parsed = JsonDocument.Parse(row.PayloadJson!);
            quantities[row.StockKey!] = quantities.GetValueOrDefault(row.StockKey!) + (AndroidEndpoints.GetDecimal(parsed.RootElement, "quantity") ?? 0m);
        }

        var rows = new List<PortalStockRow>();
        foreach (var row in records.Where(r => r.Entity == "stocks" && r.StockKey is not null && r.PayloadJson is not null))
        {
            using var parsed = JsonDocument.Parse(row.PayloadJson!);
            var item = new PortalStockRow
            {
                StockCode = row.StockKey!,
                Name = AndroidEndpoints.GetString(parsed.RootElement, "name") ?? string.Empty,
                Quantity = quantities.GetValueOrDefault(row.StockKey!),
            };
            if (outOfStockOnly && item.Quantity > 0) continue;
            if (!Matches(search, item.StockCode, item.Name)) continue;
            rows.Add(item);
        }

        var ordered = outOfStockOnly
            ? rows.OrderBy(r => r.Quantity).ThenBy(r => r.StockCode, StringComparer.OrdinalIgnoreCase)
            : rows.OrderBy(r => r.Name, StringComparer.CurrentCultureIgnoreCase);
        return new PortalStockResponse { Rows = ordered.Take(MaxStockRows).ToList(), Truncated = rows.Count > MaxStockRows };
    }

    // ---- helpers --------------------------------------------------------------

    private static bool Matches(string? search, params string[] values) =>
        string.IsNullOrWhiteSpace(search)
        || values.Any(v => v.Contains(search.Trim(), StringComparison.CurrentCultureIgnoreCase));

    internal static string? ReadString(string? json, string name)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var document = JsonDocument.Parse(json);
            return AndroidEndpoints.GetString(document.RootElement, name);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    internal static decimal? ReadDecimal(string? json, string name)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var document = JsonDocument.Parse(json);
            return AndroidEndpoints.GetDecimal(document.RootElement, name);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static TimeZoneInfo ResolveIstanbul()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            // Turkey has kept UTC+3 all year since 2016.
            return TimeZoneInfo.CreateCustomTimeZone("Europe/Istanbul", TimeSpan.FromHours(3), "Istanbul", "Istanbul");
        }
    }
}
