using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
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

    /// <param name="RecNo">Mikro's <c>cha_recno</c>: the order of movements on the same day (null for native rows).</param>
    /// <param name="Closed">
    /// A peşin invoice Mikro closed to a kasa/banka (<c>kapali</c>): it belongs to the customer in
    /// <c>ciroCariKod</c> but never moved their balance, so statements leave it out.
    /// </param>
    /// <param name="OtherSide">
    /// Any other kasa/banka-side Mikro row (<c>cha_cari_cins</c> ≠ 0): its <c>cariKod</c> is a kasa or bank code
    /// that may equal a customer code. Mikro's balance counts only <c>cha_cari_cins = 0</c>, so neither does the panel.
    /// </param>
    public sealed record Movement(
        string Customer, string Id, DateTime Date, string Kind, string? SourceType, string? DocumentNo, string? Description,
        decimal Debit, decimal Credit, string? DocumentKey, long? RecNo = null, bool Closed = false, string? PaymentType = null,
        bool Voided = false, Guid? VoidedByUserId = null, DateTime? VoidedAt = null, string? VoidReason = null, bool OtherSide = false)
    {
        /// <summary>Moves the customer's balance and shows on their statement.</summary>
        public bool CustomerSide => !Closed && !OtherSide;
    }

    /// <summary>
    /// Oldest first. Mikro movements of one day share a midnight timestamp, so they follow their record
    /// number as a number ("2" before "10"; Codex, PR #62); native rows carry the time of day.
    /// </summary>
    private static int Chronological(Movement a, Movement b)
    {
        var byDate = a.Date.CompareTo(b.Date);
        if (byDate != 0) return byDate;
        var byRecord = (a.RecNo ?? long.MaxValue).CompareTo(b.RecNo ?? long.MaxValue);
        return byRecord != 0 ? byRecord : string.CompareOrdinal(a.Id, b.Id);
    }

    public sealed record Movements(
        IReadOnlyDictionary<string, List<Movement>> ByCustomer,
        IReadOnlyDictionary<string, List<PortalRecords.LinePart>> LinesByDocument,
        IReadOnlyDictionary<string, string> StockNames);

    private abstract record CustomerPart;

    private sealed record CardPart(Customer Customer) : CustomerPart;

    private sealed record AddressPart(string Code, int No, string? City, string? Text) : CustomerPart;

    private sealed record CachedCustomers(long Version, IReadOnlyDictionary<string, Customer> Customers);

    /// <param name="Any">Whether the company has any ledger rows at all.</param>
    private sealed record CachedBalances(long Version, IReadOnlyDictionary<string, decimal> Balances, bool Any);

    private sealed record CachedLedgerCustomers(
        IReadOnlyDictionary<string, Customer> Cards, IReadOnlyDictionary<string, decimal> Balances, IReadOnlyDictionary<string, Customer> Customers);

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

    /// <summary>
    /// The customers with the balance the panel shows. A native company's card balance is the book itself
    /// (<c>native_customer_balances</c>, opening balance included), so it stands. An ERP company's card balance is
    /// the agent's snapshot, which the agent re-sends only when the customer card itself changes: a new invoice or
    /// collection leaves it stale. There the balance is Sipariş Cepte's: the sum of the customer's mirrored ledger
    /// rows (debit +, credit −, kasa/banka-side rows such as closed peşin invoices left out; Siparis_Cepte
    /// <c>AppDatabase.pageForBrowse</c>, <c>CustomerDetailLoader</c>). The agent mirrors the whole ledger, and
    /// Mikro's card balance is that same sum, so a customer without rows is at zero — even when their card still
    /// shows the balance of an invoice since deleted (Codex, PR #181). Only a company with no ledger rows at all
    /// (an agent that sends none) keeps the card balances.
    /// </summary>
    public static async Task<IReadOnlyDictionary<string, Customer>> CustomersAsync(
        CentralApiDbContext db, IMemoryCache cache, Guid tenantId, string dataSource, CancellationToken ct)
    {
        var cards = await CustomersAsync(db, cache, tenantId, ct);
        if (string.Equals(dataSource, TenantDataSources.Native, StringComparison.Ordinal)) return cards;
        var ledger = await LedgerBalancesAsync(db, cache, tenantId, ct);
        if (!ledger.Any) return cards;

        var key = ("portal-customers-ledger", tenantId);
        if (cache.TryGetValue(key, out CachedLedgerCustomers? cached)
            && ReferenceEquals(cached!.Cards, cards) && ReferenceEquals(cached.Balances, ledger.Balances))
            return cached.Customers;
        var customers = new Dictionary<string, Customer>(cards.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var (code, card) in cards)
            customers[code] = card with { Balance = ledger.Balances.GetValueOrDefault(code) };
        cache.Set(key, new CachedLedgerCustomers(cards, ledger.Balances, customers), ViewLifetime);
        return customers;
    }

    /// <summary>Each customer's ledger sum, from the ledger mirror alone — the list must not wait for invoice lines.</summary>
    private static async Task<CachedBalances> LedgerBalancesAsync(CentralApiDbContext db, IMemoryCache cache, Guid tenantId, CancellationToken ct)
    {
        var mirror = PortalRecordMirror<Movement>.For(cache, "ledger", tenantId, MovementEntities, ParseMovement);
        var key = ("portal-ledger-balances", tenantId);
        cache.TryGetValue(key, out CachedBalances? cached);
        var ledger = await mirror.RefreshAsync(db, cached?.Version, ct);
        if (ledger is null && cached is not null) return cached;

        var balances = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var movement in ledger ?? [])
        {
            // A kasa/banka-side row (a closed invoice among them) never moved the customer's balance.
            if (!movement.CustomerSide) continue;
            balances[movement.Customer] = balances.GetValueOrDefault(movement.Customer) + movement.Debit - movement.Credit;
        }
        var result = new CachedBalances(mirror.Version, balances, ledger is { Count: > 0 });
        cache.Set(key, result, ViewLifetime);
        return result;
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
            // An invoice keeps its key even when no lines are mirrored: it opens and says so (Codex, PR #62).
            if (!byCustomer.TryGetValue(movement.Customer, out var list)) byCustomer[movement.Customer] = list = [];
            list.Add(movement);
        }
        foreach (var list in byCustomer.Values)
            list.Sort(Chronological);

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
        // A closed invoice's cariKod is the kasa/banka it posted to; its customer is ciroCariKod.
        var closed = AndroidEndpoints.GetBoolean(row, "kapali") ?? false;
        var counterparty = PortalRecords.Blank(AndroidEndpoints.GetString(row, "ciroCariKod"));
        var customer = (closed ? counterparty : null)
            ?? PortalRecords.Blank(AndroidEndpoints.GetFirstString(row, "cariKod", "customerCode"));
        if (customer is null) return null;
        // Mikro counts only cha_cari_cins = 0. An agent before G4 sends no cariCins; there a ciro code marks the
        // kasa/banka side (the reader fills it only for cha_cari_cins <> 0).
        var otherSide = !closed && (AndroidEndpoints.GetInt32(row, "cariCins") is { } side ? side != 0 : counterparty is not null);
        var native = string.Equals(AndroidEndpoints.GetString(row, "erp"), PortalRecords.NativeErp, StringComparison.OrdinalIgnoreCase);
        var sourceType = PortalRecords.Blank(AndroidEndpoints.GetString(row, "type"));
        var kind = KindOf(sourceType);
        var amount = Math.Abs(AndroidEndpoints.GetDecimal(row, "tutar") ?? AndroidEndpoints.GetDecimal(row, "meblag") ?? AndroidEndpoints.GetDecimal(row, "amount") ?? 0m);
        var debit = AndroidEndpoints.GetBoolean(row, "borcMu") ?? AndroidEndpoints.GetInt32(row, "tip") == 0;
        var documentNo = PortalRecords.Blank(AndroidEndpoints.GetString(row, "evrakNo"));
        var recNo = native ? null : AndroidEndpoints.GetInt32(row, "cha_recno") is { } number and > 0 ? number : (int?)null;
        string? documentKey = null;
        if (DocumentKinds.Contains(kind))
        {
            documentKey = native
                ? documentNo is null ? null : PortalRecords.NativeDocumentKey(customer, documentNo)
                : recNo is { } invoiceRecNo ? PortalRecords.ErpDocumentKey(invoiceRecNo) : null;
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
            documentKey,
            recNo,
            closed,
            PortalRecords.Blank(AndroidEndpoints.GetString(row, "paymentType")),
            AndroidEndpoints.GetBoolean(row, "voided") ?? false,
            Guid.TryParse(AndroidEndpoints.GetString(row, "voidedByUserId"), out var voidedBy) ? voidedBy : null,
            PortalRecords.ReadDateTime(AndroidEndpoints.GetString(row, "voidedAt")),
            PortalRecords.Blank(AndroidEndpoints.GetString(row, "voidReason")),
            otherSide);
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
    /// A statement. The running balance is anchored to the customer's balance as the panel lists it
    /// (<see cref="CustomersAsync(CentralApiDbContext, IMemoryCache, Guid, string, CancellationToken)"/>): the last
    /// movement ends on it. Opening = balance − movements from <paramref name="from"/> on. For an ERP company that
    /// balance is the ledger sum itself, so the opening is exactly the movements before <paramref name="from"/>; a
    /// native company's card balance also carries its opening balance.
    ///
    /// <para><paramref name="includeVoided"/> (GOAL_PANEL_ERPSIZ E4d) only hides a voided original from
    /// the rows shown — its reversal still shows (E4a always books one) and every balance figure here
    /// (opening/closing/running) always counts every movement, voided or not, exactly like the
    /// <paramref name="kinds"/> filter already does: hiding a row from the list never hides its effect
    /// on the number the phone's own card balance agrees with.</para>
    /// </summary>
    public static PortalLedgerResponse Statement(
        Customer customer, Movements movements, DateOnly? from, DateOnly? to, IReadOnlyCollection<string> kinds, bool includeVoided, int page, int pageSize)
    {
        // Like Mikro's cari föyü: a closed invoice or other kasa/banka-side row is not a balance movement.
        var all = movements.ByCustomer.TryGetValue(customer.Code, out var list) ? list.Where(m => m.CustomerSide).ToList() : [];
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
            if (!includeVoided && m.Voided) continue;
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
                Voided = m.Voided,
                VoidedByUserId = m.VoidedByUserId,
                VoidedAt = m.VoidedAt?.ToString("O", CultureInfo.InvariantCulture),
                Reason = m.VoidReason,
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
        return movement is null ? null : BuildDocumentResponse(customer.Code, customer.Title, movement, movements, documentKey);
    }

    /// <summary>
    /// The same document, found by its key alone (GOAL_PANEL_ERPSIZ E5b) — for a caller that does not
    /// already know which customer/supplier it belongs to (a company-wide documents list, an id typed
    /// into a URL). Scans every customer's movements once; <see cref="Movements"/> is already the whole
    /// tenant in memory (E3c's Payments does the same), so this stays a single cached-data pass, no
    /// extra database round trip.
    /// </summary>
    public static PortalDocumentResponse? DocumentByKey(IReadOnlyDictionary<string, Customer> customers, Movements movements, string documentKey)
    {
        foreach (var (customerCode, list) in movements.ByCustomer)
        {
            var movement = list.FirstOrDefault(m => string.Equals(m.DocumentKey, documentKey, StringComparison.Ordinal));
            if (movement is null) continue;
            var title = customers.TryGetValue(customerCode, out var customer) ? customer.Title : customerCode;
            return BuildDocumentResponse(customerCode, title, movement, movements, documentKey);
        }
        return null;
    }

    private static PortalDocumentResponse BuildDocumentResponse(string customerCode, string customerTitle, Movement movement, Movements movements, string documentKey)
    {
        var lines = movements.LinesByDocument.TryGetValue(documentKey, out var found) ? found : [];
        return new PortalDocumentResponse
        {
            Id = movement.Id,
            DocumentKey = documentKey,
            CustomerCode = customerCode,
            CustomerTitle = customerTitle,
            Date = movement.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Kind = movement.Kind,
            DocumentNo = movement.DocumentNo,
            Description = movement.Description,
            Amount = movement.Debit + movement.Credit,
            Voided = movement.Voided,
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

    private static readonly string[] PaymentKinds = ["collection", "payment"];

    /// <summary>
    /// Every sale/purchase/return invoice across the company, any customer/supplier (GOAL_PANEL_ERPSIZ
    /// E5b) — the line-carrying kinds, i.e. exactly <see cref="DocumentKinds"/>. Same shape as E3c's
    /// <see cref="Payments"/>: a caller-resolved job lookup for the creator, everything else already
    /// cached in <paramref name="movements"/>.
    /// </summary>
    public static PortalDocumentsResponse Documents(
        IReadOnlyDictionary<string, Customer> customers, Movements movements, DateOnly from, DateOnly to,
        IReadOnlyCollection<string> kinds, string? customerCode, Guid? userId,
        Func<string, Guid?> creatorOf, IReadOnlyDictionary<Guid, string> userNames, int page, int pageSize)
    {
        var wanted = kinds.Count > 0 ? kinds : DocumentKinds;
        var start = from.ToDateTime(TimeOnly.MinValue);
        var endExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var code = customerCode?.Trim();

        var matching = movements.ByCustomer.Values
            .SelectMany(list => list)
            .Where(m => !m.Closed && m.DocumentKey is not null && wanted.Contains(m.Kind) && m.Date >= start && m.Date < endExclusive)
            .Where(m => code is null || string.Equals(m.Customer, code, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (userId is { } wantedUser)
            matching = matching.Where(m => creatorOf(ExternalIdOf(m.Id)) == wantedUser).ToList();

        var ordered = matching.OrderByDescending(m => m.Date).ThenByDescending(m => m.Id, StringComparer.Ordinal).ToList();
        var pageItems = ordered.Skip((page - 1) * pageSize).Take(pageSize).Select(m =>
        {
            var creator = creatorOf(ExternalIdOf(m.Id));
            return new PortalDocumentRow
            {
                Id = m.Id,
                DocumentKey = m.DocumentKey!,
                Kind = m.Kind,
                Date = m.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                DocumentNo = m.DocumentNo,
                CustomerCode = m.Customer,
                CustomerTitle = customers.TryGetValue(m.Customer, out var customer) ? customer.Title : m.Customer,
                Amount = m.Debit + m.Credit,
                UserId = creator,
                UserName = creator is { } id && userNames.TryGetValue(id, out var name) ? name : null,
            };
        }).ToList();

        return new PortalDocumentsResponse
        {
            From = from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            To = to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Items = pageItems,
            Total = matching.Count,
            Page = page,
            PageSize = pageSize,
        };
    }

    /// <summary>
    /// The job that created a movement, from its id (<c>PostToCustomerAsync</c> always writes
    /// <c>"{externalId}|{suffix}"</c>): strips the last <c>|</c>-separated segment. A row with no
    /// <c>|</c> (an ERP-agent row, never one of ours) yields itself and simply matches no job.
    /// </summary>
    public static string ExternalIdOf(string movementId)
    {
        var index = movementId.LastIndexOf('|');
        return index < 0 ? movementId : movementId[..index];
    }

    /// <summary>
    /// Every collection/payment movement of the company, any customer (GOAL_PANEL_ERPSIZ E3c).
    /// <paramref name="creatorOf"/> and <paramref name="userNames"/> are resolved by the caller
    /// (a job lookup keyed by <see cref="ExternalIdOf"/>) because this method never touches the
    /// database — everything else here reads the already-cached <paramref name="movements"/>.
    /// </summary>
    public static PortalPaymentsResponse Payments(
        IReadOnlyDictionary<string, Customer> customers, Movements movements, DateOnly from, DateOnly to,
        IReadOnlyCollection<string> kinds, string? customerCode, Guid? userId,
        Func<string, Guid?> creatorOf, IReadOnlyDictionary<Guid, string> userNames, int page, int pageSize)
    {
        var wanted = kinds.Count > 0 ? kinds : PaymentKinds;
        var start = from.ToDateTime(TimeOnly.MinValue);
        var endExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var code = customerCode?.Trim();

        var matching = movements.ByCustomer.Values
            .SelectMany(list => list)
            .Where(m => !m.Closed && wanted.Contains(m.Kind) && m.Date >= start && m.Date < endExclusive)
            .Where(m => code is null || string.Equals(m.Customer, code, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (userId is { } wantedUser)
            matching = matching.Where(m => creatorOf(ExternalIdOf(m.Id)) == wantedUser).ToList();

        var dailyTotals = matching.GroupBy(m => m.Date.Date)
            .OrderBy(g => g.Key)
            .Select(g => new PortalPaymentGroupTotal { Key = g.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Debit = g.Sum(m => m.Debit), Credit = g.Sum(m => m.Credit) })
            .ToList();
        var typeTotals = matching.GroupBy(m => m.PaymentType ?? "Diğer", StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Sum(m => m.Debit + m.Credit))
            .Select(g => new PortalPaymentGroupTotal { Key = g.Key, Debit = g.Sum(m => m.Debit), Credit = g.Sum(m => m.Credit) })
            .ToList();

        var ordered = matching.OrderByDescending(m => m.Date).ThenByDescending(m => m.Id, StringComparer.Ordinal).ToList();
        var pageItems = ordered.Skip((page - 1) * pageSize).Take(pageSize).Select(m =>
        {
            var creator = creatorOf(ExternalIdOf(m.Id));
            return new PortalPaymentRow
            {
                Id = m.Id,
                Date = m.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                CustomerCode = m.Customer,
                CustomerTitle = customers.TryGetValue(m.Customer, out var customer) ? customer.Title : m.Customer,
                Kind = m.Kind,
                PaymentType = m.PaymentType,
                Description = m.Description,
                Debit = m.Debit,
                Credit = m.Credit,
                UserId = creator,
                UserName = creator is { } id && userNames.TryGetValue(id, out var name) ? name : null,
                DocumentKey = m.DocumentKey,
            };
        }).ToList();

        return new PortalPaymentsResponse
        {
            From = from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            To = to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Items = pageItems,
            Total = matching.Count,
            Page = page,
            PageSize = pageSize,
            TotalDebit = matching.Sum(m => m.Debit),
            TotalCredit = matching.Sum(m => m.Credit),
            DailyTotals = dailyTotals,
            PaymentTypeTotals = typeTotals,
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
