using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Portal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Targets;

public enum TargetFactKind : byte
{
    /// <summary>A sold line (or a sale without lines): revenue, quantity, a sales document.</summary>
    Sale,

    /// <summary>A returned line: counts against revenue and quantity.</summary>
    Return,

    Collection,
}

/// <summary>
/// One countable thing that happened (GOAL_HEDEF_RUT K6–K8): a sold or returned line, or a collection.
/// Attributed either to a user (documents the server holds) or to an ERP salesperson code (Mikro rows).
/// </summary>
/// <param name="Amount">Positive; <see cref="TargetFactKind.Return"/> is subtracted by the reader.</param>
/// <param name="DocumentKey">Groups lines into documents, for the document count.</param>
public sealed record TargetFact(
    DateOnly Day, TargetFactKind Kind, Guid? UserId, string? SalespersonCode, string? StockCode,
    decimal Amount, decimal Quantity, string? DocumentKey);

/// <summary>Everything the targets of a date range are measured against, for one company.</summary>
public sealed class TargetFactSet
{
    /// <summary><see cref="TargetSources"/>.</summary>
    public required string Source { get; init; }

    public required IReadOnlyList<TargetFact> Facts { get; init; }

    /// <summary>ERP companies: the users' documents not yet in the ERP (K8); never part of <see cref="Facts"/>.</summary>
    public required IReadOnlyList<TargetFact> Pending { get; init; }

    /// <summary>ERP salesperson code → user, for users with a code of their own (K7).</summary>
    public required IReadOnlyDictionary<string, Guid> UserOfSalesperson { get; init; }

    /// <summary>ERP companies measured from Mikro: users without a salesperson code of their own.</summary>
    public required IReadOnlySet<Guid> UsersWithoutSalesperson { get; init; }

    /// <summary>Route visits by user: planned stops and completed visits per day.</summary>
    public required IReadOnlyDictionary<Guid, IReadOnlyList<(DateOnly Day, bool Planned, bool Completed)>> Visits { get; init; }

    /// <summary>Product code → card, for category, sub-category and brand targets and names.</summary>
    public required IReadOnlyDictionary<string, PortalStockCatalog.Product> Products { get; init; }

    /// <summary>Brand and main-group names by code (agent 1.3.0); empty otherwise.</summary>
    public IReadOnlyDictionary<string, string> BrandNames { get; init; } = new Dictionary<string, string>();

    public IReadOnlyDictionary<string, string> MainGroupNames { get; init; } = new Dictionary<string, string>();

    public Guid? UserOf(TargetFact fact) =>
        fact.UserId ?? (fact.SalespersonCode is { } code && UserOfSalesperson.TryGetValue(code, out var user) ? user : null);
}

public static class TargetSources
{
    /// <summary>ERP-less company: documents the server booked.</summary>
    public const string ServerDocuments = "server-documents";

    /// <summary>ERP company: Mikro's own rows with their salesperson code.</summary>
    public const string ErpMirror = "erp-mirror";

    /// <summary>ERP company whose agent sends no salesperson code yet: the phones' documents instead (K8).</summary>
    public const string PhoneDocuments = "phone-documents";
}

/// <summary>
/// Reads the facts of a date range (GOAL_HEDEF_RUT K7): an ERP-less company's booked documents, or an
/// ERP company's Mikro rows mirrored in <c>mobile_records</c> (<c>stockTransactions</c>,
/// <c>customerTransactions</c>), falling back to the phones' documents while the agent sends no salesperson
/// code. A range's facts are kept for a minute (K15): every phone of a company asks on refresh.
/// </summary>
public sealed class TargetFactReader(IMemoryCache cache)
{
    public static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(1);

    private static readonly string[] MirrorEntities = ["stockTransactions", "customerTransactions"];

    /// <summary>Mikro sth_evraktip: 1 çıkış irsaliyesi, 4 çıkış faturası (satış); 3 giriş faturası (with the iade flag: satış iadesi).</summary>
    private static readonly HashSet<int> SaleDocumentTypes = [1, 4];
    private const int ReturnDocumentType = 3;

    /// <summary>Reader's <c>tip</c>: the Mikro sth_tip for normal rows, 2/3 for returns out/in.</summary>
    private const int OutgoingType = 1;
    private const int ReturnedInType = 3;

    public async Task<TargetFactSet> ReadAsync(CentralApiDbContext db, Tenant tenant, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var key = ("target-facts", tenant.Id, from, to);
        if (cache.TryGetValue(key, out TargetFactSet? cached)) return cached!;
        var set = await BuildAsync(db, tenant, from, to, ct);
        cache.Set(key, set, CacheLifetime);
        return set;
    }

    /// <summary>Drops a company's cached ranges; a target write does not need it, a test does.</summary>
    public static void Forget(IMemoryCache cache, Guid tenantId, DateOnly from, DateOnly to) => cache.Remove(("target-facts", tenantId, from, to));

    private async Task<TargetFactSet> BuildAsync(CentralApiDbContext db, Tenant tenant, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var catalog = await PortalStockCatalog.LoadAsync(db, cache, tenant.Id, ct);
        var products = catalog.Products
            .GroupBy(p => p.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var visits = await VisitsAsync(db, tenant.Id, from, to, ct);
        var native = tenant.DataSource == TenantDataSources.Native;
        var documentTypes = new[] { PortalReports.SalesOrder, PortalReports.Collection }.Concat(PortalReports.ReturnDocumentTypes(tenant)).ToArray();

        if (native)
        {
            var booked = await PortalReports.PhoneDocumentsAsync(db, tenant.Id, from, to, documentTypes, [JobStatus.Succeeded], ct);
            var (voided, replacements) = await NativeCorrectionsAsync(db, tenant.Id, ct);
            var facts = booked.Where(d => !voided.Contains(d.ExternalId)).SelectMany(FromDocument).ToList();
            facts.AddRange(replacements.Where(r => r.BusinessDate >= from && r.BusinessDate <= to).SelectMany(FromDocument));
            return new TargetFactSet
            {
                Source = TargetSources.ServerDocuments,
                Facts = facts,
                Pending = [],
                UserOfSalesperson = new Dictionary<string, Guid>(),
                UsersWithoutSalesperson = new HashSet<Guid>(),
                Visits = visits,
                Products = products,
                BrandNames = catalog.BrandNames,
                MainGroupNames = catalog.MainGroupNames,
            };
        }

        var documents = await PortalReports.PhoneDocumentsAsync(db, tenant.Id, from, to, documentTypes,
            [JobStatus.Pending, JobStatus.Processing, JobStatus.Succeeded], ct);
        var mirror = PortalRecordMirror<MirrorRow>.For(cache, "target-rows", tenant.Id, MirrorEntities, ParseMirrorRow);
        var rows = await mirror.RefreshAsync(db, null, ct) ?? [];

        // An agent that knows the salesperson column writes the field on every row, null or not.
        if (!rows.Any(r => r.SalespersonKnown))
        {
            // Until then the phones' documents stand in, the way the Plasiyerler report counts them.
            return new TargetFactSet
            {
                Source = TargetSources.PhoneDocuments,
                Facts = documents.SelectMany(FromDocument).ToList(),
                Pending = [],
                UserOfSalesperson = new Dictionary<string, Guid>(),
                UsersWithoutSalesperson = new HashSet<Guid>(),
                Visits = visits,
                Products = products,
                BrandNames = catalog.BrandNames,
                MainGroupNames = catalog.MainGroupNames,
            };
        }

        var users = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenant.Id && u.DeletedAtUtc == null)
            .Select(u => u.Id)
            .ToListAsync(ct);
        var codes = await db.MobileUserErpMappings.AsNoTracking()
            .Where(m => m.TenantId == tenant.Id && m.SalespersonCode != null && m.SalespersonCode != "")
            .Select(m => new { m.UserId, m.SalespersonCode })
            .ToListAsync(ct);
        var userOfCode = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in codes.Where(m => users.Contains(m.UserId)))
            userOfCode.TryAdd(mapping.SalespersonCode!.Trim(), mapping.UserId);

        return new TargetFactSet
        {
            Source = TargetSources.ErpMirror,
            Facts = rows.Where(r => r.Fact.Day >= from && r.Fact.Day <= to).Select(r => r.Fact).ToList(),
            Pending = documents.Where(d => d.Status is JobStatus.Pending or JobStatus.Processing).SelectMany(FromDocument).ToList(),
            UserOfSalesperson = userOfCode,
            UsersWithoutSalesperson = users.Where(u => !userOfCode.ContainsValue(u)).ToHashSet(),
            Visits = visits,
            Products = products,
            BrandNames = catalog.BrandNames,
            MainGroupNames = catalog.MainGroupNames,
        };
    }

    // ---- documents the server holds ------------------------------------------------------

    /// <summary>
    /// A phone document's facts: one per line (a line's value is <c>lineTotal</c>, else quantity × <c>unitPrice</c>, else
    /// quantity × <c>listUnitPrice</c> × <c>conditionPercent</c> — how the ERP-less book values it), or the document
    /// amount when it has no lines; a sale paid on the spot is also a collection, as the ERP-less book posts it.
    /// </summary>
    public static IEnumerable<TargetFact> FromDocument(PortalReports.PhoneDocument document)
    {
        if (string.IsNullOrWhiteSpace(document.PayloadJson)) yield break;
        using var parsed = JsonDocument.Parse(document.PayloadJson);
        var root = parsed.RootElement;
        if (root.ValueKind != JsonValueKind.Object) yield break;
        var amount = Math.Abs(AndroidEndpoints.GetDecimal(root, "amount") ?? 0m);

        if (document.DocumentType == PortalReports.Collection)
        {
            yield return new TargetFact(document.BusinessDate, TargetFactKind.Collection, document.CreatedByUserId, null, null, amount, 0m, document.ExternalId);
            yield break;
        }

        var kind = document.DocumentType == PortalReports.SalesOrder ? TargetFactKind.Sale : TargetFactKind.Return;
        var any = false;
        if (root.TryGetProperty("lines", out var lines) && lines.ValueKind == JsonValueKind.Array)
        {
            foreach (var line in lines.EnumerateArray().Where(l => l.ValueKind == JsonValueKind.Object))
            {
                var quantity = Math.Abs(AndroidEndpoints.GetDecimal(line, "quantity") ?? 0m);
                var value = AndroidEndpoints.GetDecimal(line, "lineTotal")
                            ?? (AndroidEndpoints.GetDecimal(line, "unitPrice") is { } unit ? quantity * unit : (decimal?)null)
                            ?? quantity * (AndroidEndpoints.GetDecimal(line, "listUnitPrice") ?? 0m) * (AndroidEndpoints.GetDecimal(line, "conditionPercent") ?? 1m);
                var stockCode = PortalRecords.Blank(AndroidEndpoints.GetFirstString(line, "productCode", "stockCode"));
                any = true;
                yield return new TargetFact(document.BusinessDate, kind, document.CreatedByUserId, null, stockCode, Math.Abs(value), quantity, document.ExternalId);
            }
        }
        if (!any)
            yield return new TargetFact(document.BusinessDate, kind, document.CreatedByUserId, null, null, amount, 0m, document.ExternalId);

        if (kind == TargetFactKind.Sale && AndroidEndpoints.GetString(root, "paymentType")?.Trim() is { } paymentType && Native.NativeDocumentProcessor.ImmediatePayments.Contains(paymentType))
            yield return new TargetFact(document.BusinessDate, TargetFactKind.Collection, document.CreatedByUserId, null, null, amount, 0m, document.ExternalId);
    }

    // ---- ERP-less corrections -----------------------------------------------------------------

    private static readonly string[] CorrectionTypes =
    [
        Native.NativeDocumentProcessor.DocumentVoid, Native.NativeDocumentProcessor.DocumentEdit,
        Native.NativeDocumentProcessor.LedgerVoid, Native.NativeDocumentProcessor.LedgerEdit,
    ];

    /// <summary>
    /// An ERP-less company's panel corrections (GOAL_PANEL_ERPSIZ E4/E5, storno): a void or edit job names the document it
    /// cancels by one of its ledger keys (<c>{job}|{suffix}</c>), and that document's own job stays <c>Succeeded</c>. So the
    /// cancelled jobs are left out, and each edit that is itself still standing counts in their place — as a document of
    /// the kind it re-booked, for the salesperson of the document it corrects (an administrator fixing a sale in the
    /// panel does not take the sale over), dated as the correction says or else as the original (Codex, PR #214).
    /// Corrections are rare manual work, so all of them are read, whatever their date.
    /// </summary>
    private static async Task<(HashSet<string> Voided, List<PortalReports.PhoneDocument> Replacements)> NativeCorrectionsAsync(
        CentralApiDbContext db, Guid tenantId, CancellationToken ct)
    {
        var corrections = await db.Jobs.AsNoTracking()
            .Where(j => j.TenantId == tenantId && j.Status == JobStatus.Succeeded && CorrectionTypes.Contains(j.DocumentType))
            .Select(j => new { j.ExternalId, j.DocumentType, j.PayloadJson })
            .ToListAsync(ct);
        var voided = new HashSet<string>(StringComparer.Ordinal);
        var targetOf = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var correction in corrections)
        {
            if (JobOfLedgerKey(PortalReports.ReadString(correction.PayloadJson, "targetKey")) is not { } target) continue;
            voided.Add(target);
            targetOf[correction.ExternalId] = target;
        }
        if (voided.Count == 0) return (voided, []);

        // The document at the start of each chain of edits: its author and date stand for every correction of it.
        string Root(string id)
        {
            for (var hops = 0; targetOf.TryGetValue(id, out var target) && hops < 50; hops++) id = target;
            return id;
        }
        var roots = targetOf.Values.Select(Root).Distinct(StringComparer.Ordinal).ToList();
        var originals = (await db.Jobs.AsNoTracking()
                .Where(j => j.TenantId == tenantId && roots.Contains(j.ExternalId))
                .Select(j => new { j.ExternalId, j.DocumentType, j.CreatedByUserId, j.EnqueuedAtUtc, j.PayloadJson })
                .ToListAsync(ct))
            .GroupBy(j => j.ExternalId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var replacements = new List<PortalReports.PhoneDocument>();
        foreach (var edit in corrections.Where(c => !voided.Contains(c.ExternalId)
                                                    && c.DocumentType is Native.NativeDocumentProcessor.DocumentEdit or Native.NativeDocumentProcessor.LedgerEdit))
        {
            if (!targetOf.TryGetValue(edit.ExternalId, out var target) || !originals.TryGetValue(Root(target), out var original)) continue;
            var originalDay = PortalReports.BusinessDate(original.PayloadJson, original.EnqueuedAtUtc);
            using var parsed = JsonDocument.Parse(edit.PayloadJson!);
            var root = parsed.RootElement;
            if (edit.DocumentType == Native.NativeDocumentProcessor.DocumentEdit)
            {
                var kind = AndroidEndpoints.GetString(root, "documentType");
                if (kind is not (PortalReports.SalesOrder or Native.NativeDocumentProcessor.SalesReturn)) continue;
                if (!root.TryGetProperty("document", out var document) || document.ValueKind != JsonValueKind.Object) continue;
                var payload = document.GetRawText();
                var day = AndroidEndpoints.GetString(document, "occurredAt") is null ? originalDay : PortalReports.BusinessDate(payload, original.EnqueuedAtUtc);
                replacements.Add(new PortalReports.PhoneDocument(edit.ExternalId, kind, JobStatus.Succeeded, original.CreatedByUserId, day, payload));
            }
            else if (original.DocumentType == PortalReports.Collection)
            {
                var amount = AndroidEndpoints.GetDecimal(root, "amount") ?? 0m;
                var day = AndroidEndpoints.GetString(root, "occurredAt") is null ? originalDay : PortalReports.BusinessDate(edit.PayloadJson, original.EnqueuedAtUtc);
                replacements.Add(new PortalReports.PhoneDocument(edit.ExternalId, PortalReports.Collection, JobStatus.Succeeded, original.CreatedByUserId, day,
                    JsonSerializer.Serialize(new { amount })));
            }
        }
        return (voided, replacements);
    }

    /// <summary>The job a native ledger row belongs to: its key without the last <c>|suffix</c>.</summary>
    private static string? JobOfLedgerKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        var bar = key.LastIndexOf('|');
        return bar <= 0 ? key : key[..bar];
    }

    // ---- Mikro rows ------------------------------------------------------------------------

    /// <param name="SalespersonKnown">The row came from an agent that reads the salesperson column (1.3.0+).</param>
    private sealed record MirrorRow(TargetFact Fact, bool SalespersonKnown);

    /// <summary>
    /// A Mikro row as a fact, or null for rows no target counts. Sales: outgoing (<c>tip</c> 1) irsaliye/fatura lines,
    /// net of their discounts, without VAT (<c>sth_tutar</c> is before VAT). Returns: incoming iade faturası lines.
    /// Collections: tahsilat rows, and peşin (closed) sales invoices, which Mikro settles on the spot.
    /// </summary>
    private static MirrorRow? ParseMirrorRow(string entity, JsonElement row)
    {
        if (string.Equals(AndroidEndpoints.GetString(row, "erp"), PortalRecords.NativeErp, StringComparison.OrdinalIgnoreCase)) return null;
        if (PortalRecords.ReadDay(AndroidEndpoints.GetString(row, "tarih")) is not { } day) return null;
        var known = row.TryGetProperty("plasiyerKod", out _);
        var salesperson = PortalRecords.Blank(AndroidEndpoints.GetString(row, "plasiyerKod"));

        if (entity == "stockTransactions")
        {
            var type = AndroidEndpoints.GetInt32(row, "tip");
            var documentType = AndroidEndpoints.GetInt32(row, "evrakTip") ?? -1;
            TargetFactKind kind;
            if (type == OutgoingType && SaleDocumentTypes.Contains(documentType)) kind = TargetFactKind.Sale;
            else if (type == ReturnedInType && documentType == ReturnDocumentType) kind = TargetFactKind.Return;
            else return null;
            if (PortalRecords.Blank(AndroidEndpoints.GetString(row, "cariKod")) is null) return null;
            var stockCode = PortalRecords.Blank(AndroidEndpoints.GetFirstString(row, "stokKod", "urunKod"));
            var quantity = kind == TargetFactKind.Sale ? AndroidEndpoints.GetDecimal(row, "cikisMiktar") : AndroidEndpoints.GetDecimal(row, "girisMiktar");
            var net = (AndroidEndpoints.GetDecimal(row, "tutar") ?? 0m) - (AndroidEndpoints.GetDecimal(row, "discountAmount") ?? 0m);
            var documentKey = AndroidEndpoints.GetInt32(row, "faturaRecno") is { } recNo and > 0
                ? PortalRecords.ErpDocumentKey(recNo)
                : $"{documentType}|{AndroidEndpoints.GetString(row, "evrakNo")}";
            return new MirrorRow(new TargetFact(day, kind, null, salesperson, stockCode, Math.Abs(net), Math.Abs(quantity ?? 0m), documentKey), known);
        }

        var transactionType = AndroidEndpoints.GetString(row, "type");
        var closed = AndroidEndpoints.GetBoolean(row, "kapali") ?? false;
        if (transactionType == "TAHSILAT" || (transactionType == "SATIS" && closed))
        {
            var amount = Math.Abs(AndroidEndpoints.GetDecimal(row, "tutar") ?? 0m);
            return new MirrorRow(new TargetFact(day, TargetFactKind.Collection, null, salesperson, null, amount, 0m, AndroidEndpoints.GetString(row, "id")), known);
        }
        return null;
    }

    // ---- visits ------------------------------------------------------------------------------

    private static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<(DateOnly Day, bool Planned, bool Completed)>>> VisitsAsync(
        CentralApiDbContext db, Guid tenantId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var records = await PortalReports.RouteRecordsAsync(db, tenantId, ct);
        var result = new Dictionary<Guid, List<(DateOnly, bool, bool)>>();
        if (records.Count == 0) return new Dictionary<Guid, IReadOnlyList<(DateOnly, bool, bool)>>();

        var users = await db.MobileUsers.AsNoTracking()
            .Where(u => u.TenantId == tenantId)
            .Select(u => new { u.Id, u.Username, u.DeletedAtUtc })
            .ToListAsync(ct);
        // A deleted user's name can be reused; visits then count for the current holder (as the activity report).
        var byName = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var user in users.OrderBy(u => u.DeletedAtUtc is null ? 1 : 0))
            byName[user.Username] = user.Id;

        for (var day = from; day <= to; day = day.AddDays(1))
        {
            foreach (var visit in PortalReports.BuildVisits(records, day))
            {
                if (!byName.TryGetValue(visit.Username, out var userId)) continue;
                if (!result.TryGetValue(userId, out var list)) result[userId] = list = [];
                list.Add((day, visit.Planned, visit.Status == "COMPLETED"));
            }
        }
        return result.ToDictionary(p => p.Key, p => (IReadOnlyList<(DateOnly, bool, bool)>)p.Value);
    }
}
