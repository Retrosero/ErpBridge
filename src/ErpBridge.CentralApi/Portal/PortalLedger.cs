using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Endpoints;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Portal;

/// <summary>
/// Customers, their statements and invoice lines for the panel (panel goal P3), read from the
/// records the phones receive: <c>customers</c>/<c>customerAddresses</c>, the ledger mirror
/// <c>customerTransactions</c> (CARI_HESAP_HAREKETLERI) and the line mirror
/// <c>stockTransactions</c> (STOK_HAREKETLERI), each kept by a <see cref="PortalRecordMirror{T}"/>.
///
/// <para>Two spellings meet here. Mikro rows carry <c>tutar</c>, <c>borcMu</c>,
/// <c>type</c> = SATIS/TAHSILAT/… and join invoice lines by <c>cha_recno</c> = <c>faturaRecno</c>.
/// Native rows (<c>erp = NATIVE</c>) carry <c>meblag</c>/<c>amount</c>, a Turkish <c>type</c>
/// (Satış, Tahsilat, İade…) and no record numbers: their lines join by customer and
/// <c>evrakNo</c> (<see cref="PortalRecords.ParseLine"/>).</para>
/// </summary>
public static class PortalLedger
{
    public const int MaxPageSize = 250;
    public static readonly string[] Kinds = ["sale", "sale_return", "purchase", "purchase_return", "collection", "payment", "other"];
    private static readonly HashSet<string> DocumentKinds = ["sale", "sale_return", "purchase", "purchase_return"];

    private static readonly string[] CustomerEntities = ["customers", "customerAddresses"];
    private static readonly string[] MovementEntities = ["customerTransactions"];
    private static readonly TimeSpan ViewLifetime = TimeSpan.FromMinutes(30);

    public sealed record Customer(
        string Code, string Title, decimal Balance, string? Phone, string? Email, string? TaxOffice, string? TaxNo,
        string? SalespersonCode, string? RegionCode, string? GroupCode, string? Currency, bool IsLocked, string? City, string? Address);

    public sealed record Movement(
        string Customer, string Id, DateTime Date, string Kind, string? SourceType, string? DocumentNo, string? Description,
        decimal Debit, decimal Credit, string? DocumentKey);

    public sealed record Movements(
        IReadOnlyDictionary<string, List<Movement>> ByCustomer,
        IReadOnlyDictionary<string, List<PortalRecords.LinePart>> LinesByDocument,
        IReadOnlyDictionary<string, string> StockNames);

    private abstract record CustomerPart;

    private sealed record CardPart(Customer Customer) : CustomerPart;

    private sealed record AddressPart(string Code, int No, string? City, string? Text) : CustomerPart;

    private sealed record CachedCustomers(long Version, IReadOnlyDictionary<string, Customer> Customers);

    private sealed record CachedMovements(long LedgerVersion, long LinesVersion, long StockVersion, Movements Movements);

    // ---- loading ------------------------------------------------------------

    public static async Task<IReadOnlyDictionary<string, Customer>> CustomersAsync(CentralApiDbContext db, IMemoryCache cache, Guid tenantId, CancellationToken ct)
    {
        var mirror = PortalRecordMirror<CustomerPart>.For(cache, "customers", tenantId, CustomerEntities, ParseCustomer);
        var key = ("portal-customers", tenantId);
        cache.TryGetValue(key, out CachedCustomers? cached);
        var parts = await mirror.RefreshAsync(db, cached?.Version, ct);
        if (parts is null && cached is not null) return cached.Customers;
        var items = parts ?? [];

        var addresses = new Dictionary<string, AddressPart>(StringComparer.OrdinalIgnoreCase);
        foreach (var address in items.OfType<AddressPart>())
            if (!addresses.TryGetValue(address.Code, out var existing) || address.No < existing.No)
                addresses[address.Code] = address;
        var customers = new Dictionary<string, Customer>(StringComparer.OrdinalIgnoreCase);
        foreach (var card in items.OfType<CardPart>())
        {
            var customer = card.Customer;
            customers[customer.Code] = addresses.TryGetValue(customer.Code, out var address)
                ? customer with { City = address.City, Address = address.Text }
                : customer;
        }
        cache.Set(key, new CachedCustomers(mirror.Version, customers), ViewLifetime);
        return customers;
    }

    public static async Task<Movements> MovementsAsync(CentralApiDbContext db, IMemoryCache cache, Guid tenantId, CancellationToken ct)
    {
        var ledgerMirror = PortalRecordMirror<Movement>.For(cache, "ledger", tenantId, MovementEntities, ParseMovement);
        var lineMirror = PortalRecords.Lines(cache, tenantId);
        var stockMirror = PortalRecordMirror<PortalRecords.StockPart>.For(cache, "stock", tenantId, PortalRecords.StockEntities, PortalRecords.ParseStock);
        var key = ("portal-ledger", tenantId);
        cache.TryGetValue(key, out CachedMovements? cached);

        var ledger = await ledgerMirror.RefreshAsync(db, cached?.LedgerVersion, ct);
        var lines = await lineMirror.RefreshAsync(db, cached?.LinesVersion, ct);
        var stock = await stockMirror.RefreshAsync(db, cached?.StockVersion, ct);
        if (cached is not null && ledger is null && lines is null && stock is null) return cached.Movements;
        ledger ??= await ledgerMirror.RefreshAsync(db, null, ct);
        lines ??= await lineMirror.RefreshAsync(db, null, ct);
        stock ??= await stockMirror.RefreshAsync(db, null, ct);

        var linesByDocument = new Dictionary<string, List<PortalRecords.LinePart>>(StringComparer.Ordinal);
        foreach (var line in lines!)
        {
            if (line.DocumentKey is null) continue;
            if (!linesByDocument.TryGetValue(line.DocumentKey, out var list)) linesByDocument[line.DocumentKey] = list = [];
            list.Add(line);
        }

        var byCustomer = new Dictionary<string, List<Movement>>(StringComparer.OrdinalIgnoreCase);
        foreach (var movement in ledger!)
        {
            // A movement opens only when its lines are actually in the mirror.
            var shown = movement.DocumentKey is not null && !linesByDocument.ContainsKey(movement.DocumentKey) ? movement with { DocumentKey = null } : movement;
            if (!byCustomer.TryGetValue(shown.Customer, out var list)) byCustomer[shown.Customer] = list = [];
            list.Add(shown);
        }
        foreach (var list in byCustomer.Values)
            list.Sort((a, b) => a.Date != b.Date ? a.Date.CompareTo(b.Date) : string.CompareOrdinal(a.Id, b.Id));

        var stockNames = stock!.OfType<PortalRecords.CardPart>()
            .Where(c => c.Name.Length > 0)
            .GroupBy(c => c.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Name, StringComparer.OrdinalIgnoreCase);

        var movements = new Movements(byCustomer, linesByDocument, stockNames);
        cache.Set(key, new CachedMovements(ledgerMirror.Version, lineMirror.Version, stockMirror.Version, movements), ViewLifetime);
        return movements;
    }

    private static CustomerPart? ParseCustomer(string entity, JsonElement item)
    {
        var code = PortalRecords.Blank(AndroidEndpoints.GetString(item, "customerCode"));
        if (code is null) return null;
        if (entity == "customerAddresses")
        {
            var city = PortalRecords.Blank(AndroidEndpoints.GetString(item, "city"));
            return new AddressPart(
                code,
                AndroidEndpoints.GetInt32(item, "addressNo") ?? int.MaxValue,
                city,
                AndroidEndpoints.JoinAddressLine(
                    AndroidEndpoints.GetString(item, "neighborhood"), AndroidEndpoints.GetString(item, "avenue"), AndroidEndpoints.GetString(item, "street"),
                    AndroidEndpoints.GetString(item, "streetName"), AndroidEndpoints.GetString(item, "apartmentNo"),
                    AndroidEndpoints.GetString(item, "district"), city));
        }
        return new CardPart(new Customer(
            code,
            AndroidEndpoints.JoinAddressLine(AndroidEndpoints.GetString(item, "title1"), AndroidEndpoints.GetString(item, "title2")) ?? code,
            AndroidEndpoints.GetDecimal(item, "balance") ?? 0m,
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "phone")),
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "email")),
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "taxOffice")),
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "taxNo")),
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "salespersonCode")),
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "regionCode")),
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "groupCode")),
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "currency")),
            AndroidEndpoints.GetBoolean(item, "isLocked") ?? false,
            null,
            null));
    }

    private static Movement? ParseMovement(string entity, JsonElement row)
    {
        var customer = PortalRecords.Blank(AndroidEndpoints.GetFirstString(row, "cariKod", "customerCode"));
        if (customer is null) return null;
        var native = string.Equals(AndroidEndpoints.GetString(row, "erp"), PortalRecords.NativeErp, StringComparison.OrdinalIgnoreCase);
        var sourceType = PortalRecords.Blank(AndroidEndpoints.GetString(row, "type"));
        var kind = KindOf(sourceType);
        var amount = Math.Abs(AndroidEndpoints.GetDecimal(row, "tutar") ?? AndroidEndpoints.GetDecimal(row, "meblag") ?? AndroidEndpoints.GetDecimal(row, "amount") ?? 0m);
        var debit = AndroidEndpoints.GetBoolean(row, "borcMu") ?? AndroidEndpoints.GetInt32(row, "tip") == 0;
        var documentNo = PortalRecords.Blank(AndroidEndpoints.GetString(row, "evrakNo"));
        string? documentKey = null;
        if (DocumentKinds.Contains(kind))
        {
            documentKey = native
                ? documentNo is null ? null : PortalRecords.NativeDocumentKey(customer, documentNo)
                : AndroidEndpoints.GetInt32(row, "cha_recno") is { } recNo and > 0 ? PortalRecords.ErpDocumentKey(recNo) : null;
        }
        return new Movement(
            customer,
            AndroidEndpoints.GetString(row, "id") ?? AndroidEndpoints.GetString(row, "erpRef") ?? string.Empty,
            PortalRecords.ReadDateTime(AndroidEndpoints.GetString(row, "tarih")) ?? DateTime.MinValue,
            kind,
            kind == "other" ? sourceType : null,
            documentNo,
            PortalRecords.Blank(AndroidEndpoints.GetString(row, "aciklama")),
            debit ? amount : 0m,
            debit ? 0m : amount,
            documentKey);
    }

    // ---- queries --------------------------------------------------------------


    public static PortalCustomersResponse Search(
        IReadOnlyDictionary<string, Customer> customers, string? search, string balance, string sort, bool descending, int page, int pageSize)
    {
        IEnumerable<Customer> matches = customers.Values;
        if (search?.Trim() is { Length: > 0 } q)
            matches = matches.Where(c => c.Code.Contains(q, StringComparison.CurrentCultureIgnoreCase)
                                         || c.Title.Contains(q, StringComparison.CurrentCultureIgnoreCase)
                                         || (c.Phone?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
        matches = balance switch
        {
            "receivable" => matches.Where(c => c.Balance > 0),
            "payable" => matches.Where(c => c.Balance < 0),
            "nonzero" => matches.Where(c => c.Balance != 0),
            _ => matches,
        };
        var filtered = matches.ToList();

        var byText = StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), CompareOptions.IgnoreCase);
        IOrderedEnumerable<Customer> ordered = sort switch
        {
            "code" => descending ? filtered.OrderByDescending(c => c.Code, byText) : filtered.OrderBy(c => c.Code, byText),
            "balance" => descending ? filtered.OrderByDescending(c => c.Balance) : filtered.OrderBy(c => c.Balance),
            "absBalance" => descending ? filtered.OrderByDescending(c => Math.Abs(c.Balance)) : filtered.OrderBy(c => Math.Abs(c.Balance)),
            _ => descending ? filtered.OrderByDescending(c => c.Title, byText) : filtered.OrderBy(c => c.Title, byText),
        };
        return new PortalCustomersResponse
        {
            Items = ordered.ThenBy(c => c.Code, byText)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(c => new PortalCustomerRow { CustomerCode = c.Code, Title = c.Title, Balance = c.Balance, Phone = c.Phone, City = c.City })
                .ToList(),
            Total = filtered.Count,
            Page = page,
            PageSize = pageSize,
            TotalReceivable = filtered.Where(c => c.Balance > 0).Sum(c => c.Balance),
            TotalPayable = filtered.Where(c => c.Balance < 0).Sum(c => -c.Balance),
        };
    }

    public static PortalCustomerCard Card(Customer c, string dataSource) => new()
    {
        CustomerCode = c.Code,
        Title = c.Title,
        Balance = c.Balance,
        Phone = c.Phone,
        Email = c.Email,
        TaxOffice = c.TaxOffice,
        TaxNo = c.TaxNo,
        Address = c.Address,
        SalespersonCode = c.SalespersonCode,
        RegionCode = c.RegionCode,
        GroupCode = c.GroupCode,
        Currency = c.Currency,
        IsLocked = c.IsLocked,
        DataSource = dataSource,
    };

    /// <summary>
    /// A statement. The running balance is anchored to the card balance (the figure the phone
    /// shows): the last movement ends on it, so a mirror holding only recent history still
    /// adds up. Opening = card balance − movements from <paramref name="from"/> on.
    /// </summary>
    public static PortalLedgerResponse Statement(
        Customer customer, Movements movements, DateOnly? from, DateOnly? to, IReadOnlyCollection<string> kinds, int page, int pageSize)
    {
        var all = movements.ByCustomer.TryGetValue(customer.Code, out var list) ? list : [];
        var start = from?.ToDateTime(TimeOnly.MinValue);
        var endExclusive = to?.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var fromOn = start is null ? all : all.Where(m => m.Date >= start).ToList();
        var opening = customer.Balance - fromOn.Sum(m => m.Debit - m.Credit);

        var running = opening;
        var rows = new List<PortalLedgerRow>();
        decimal totalDebit = 0, totalCredit = 0;
        foreach (var m in fromOn)
        {
            if (endExclusive is not null && m.Date >= endExclusive) break;
            running += m.Debit - m.Credit;
            if (kinds.Count > 0 && !kinds.Contains(m.Kind)) continue;
            totalDebit += m.Debit;
            totalCredit += m.Credit;
            rows.Add(new PortalLedgerRow
            {
                Id = m.Id,
                Date = m.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Kind = m.Kind,
                SourceType = m.SourceType,
                DocumentNo = m.DocumentNo,
                Description = m.Description,
                Debit = m.Debit,
                Credit = m.Credit,
                Balance = running,
                DocumentKey = m.DocumentKey,
            });
        }

        return new PortalLedgerResponse
        {
            CustomerCode = customer.Code,
            From = from?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            To = to?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Opening = opening,
            Closing = running,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            // Newest first on screen, like the phone's statement.
            Items = Enumerable.Reverse(rows).Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Total = rows.Count,
            Page = page,
            PageSize = pageSize,
        };
    }

    public static PortalDocumentResponse? Document(Customer customer, Movements movements, string documentKey)
    {
        if (!movements.ByCustomer.TryGetValue(customer.Code, out var list)) return null;
        var movement = list.FirstOrDefault(m => string.Equals(m.DocumentKey, documentKey, StringComparison.Ordinal));
        if (movement is null) return null;
        var lines = movements.LinesByDocument.TryGetValue(documentKey, out var found) ? found : [];
        return new PortalDocumentResponse
        {
            DocumentKey = documentKey,
            CustomerCode = customer.Code,
            Date = movement.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Kind = movement.Kind,
            DocumentNo = movement.DocumentNo,
            Description = movement.Description,
            Amount = movement.Debit + movement.Credit,
            LinesAvailable = lines.Count > 0,
            Lines = lines.Select(l => new PortalDocumentLine
            {
                StockCode = l.StockCode,
                Name = movements.StockNames.TryGetValue(l.StockCode, out var name) ? name : null,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Amount = l.Amount,
                Tax = l.Tax,
                WarehouseNo = l.WarehouseNo,
                Description = l.Description,
            }).ToList(),
        };
    }

    // ---- helpers --------------------------------------------------------------

    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>
    /// Mikro <c>type</c> (SATIS…, ASCII) or the native booking's Turkish type (Satış, İade…) → one kind.
    /// Upper-cased with Turkish rules: the invariant culture leaves "ı" alone, so "Satış" would not match.
    /// </summary>
    public static string KindOf(string? sourceType) => sourceType?.Trim().ToUpper(Turkish) switch
    {
        "SATIS" or "SATIŞ" => "sale",
        "SATIS_IADE" or "SATIS_İADE" or "İADE" => "sale_return",
        "ALIS" or "ALIŞ" => "purchase",
        "ALIS_IADE" or "ALIS_İADE" => "purchase_return",
        "TAHSILAT" or "TAHSİLAT" => "collection",
        "TEDIYE" or "TEDİYE" or "İADE ÖDEMESİ" => "payment",
        _ => "other",
    };

}
