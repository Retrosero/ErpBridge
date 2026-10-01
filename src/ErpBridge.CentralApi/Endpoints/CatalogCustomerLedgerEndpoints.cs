using System.Globalization;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Portal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using static ErpBridge.CentralApi.Endpoints.CustomerCatalogManageEndpoints;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// The customer's own account under <c>/api/v1/catalog/{code}</c> (docs/GOAL_MUSTERI_KATALOGU.md §5.2, S9): statement,
/// invoices and what they bought before, each behind the account's flag (403 <c>FEATURE_DISABLED</c>). Read from the same
/// ledger and line mirrors as the panel (<see cref="PortalLedger"/>), narrowed for a customer: no descriptions (staff
/// notes), no voiding details. A document opens only from the customer's own invoice list — the same filter as the list,
/// so a kasa/banka-side row whose code equals the customer's, or another customer's key, is not found.
/// </summary>
internal static class CatalogCustomerLedgerEndpoints
{
    public const int InvoicePageSize = 50;

    private static readonly CompareInfo Turkish = CultureInfo.GetCultureInfo("tr-TR").CompareInfo;
    private static readonly StringComparer ByText = StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), CompareOptions.IgnoreCase);

    public static void Map(RouteGroupBuilder signedIn)
    {
        signedIn.MapGet("/statement", StatementAsync).WithName("CatalogStatement");
        signedIn.MapGet("/invoices", InvoicesAsync).WithName("CatalogInvoices");
        signedIn.MapGet("/invoices/detail", InvoiceDetailAsync).WithName("CatalogInvoiceDetail");
        signedIn.MapGet("/purchased", PurchasedAsync).WithName("CatalogPurchased");
    }

    /// <summary>The customer's statement, newest first, the running balance on each row; <c>from</c>/<c>to</c> are <c>yyyy-MM-dd</c>.</summary>
    private static async Task<IResult> StatementAsync(HttpContext http, string? from, string? to,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var session = CatalogSession.Of(http);
        if (!session.Account.ShowStatement) return FeatureDisabled();
        if (!TryDay(from, out var fromDay) || !TryDay(to, out var toDay)) return InvalidBody("from ve to yyyy-MM-dd biçiminde olmalı.");
        var (customer, movements) = await LedgerAsync(session, db, cache, ct);
        var statement = PortalLedger.Statement(customer, movements, fromDay, toDay, [], includeVoided: false, page: 1, pageSize: int.MaxValue);
        return JsonResults.Ok(new CatalogStatementResponse
        {
            Balance = customer.Balance,
            Rows = [.. statement.Items.Select(r => new CatalogStatementRowDto
            {
                Date = r.Date,
                Kind = r.Kind,
                DocumentNo = r.DocumentNo,
                Debit = r.Debit,
                Credit = r.Credit,
                Balance = r.Balance,
            })],
        });
    }

    private static async Task<IResult> InvoicesAsync(HttpContext http, string? from, string? to, int? page,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, CancellationToken ct)
    {
        var session = CatalogSession.Of(http);
        if (!session.Account.ShowInvoices) return FeatureDisabled();
        if (!TryDay(from, out var fromDay) || !TryDay(to, out var toDay)) return InvalidBody("from ve to yyyy-MM-dd biçiminde olmalı.");
        var (customer, movements) = await LedgerAsync(session, db, cache, ct);
        var start = fromDay?.ToDateTime(TimeOnly.MinValue);
        var endExclusive = toDay?.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var invoices = Invoices(customer, movements)
            .Where(m => (start is null || m.Date >= start) && (endExclusive is null || m.Date < endExclusive))
            .OrderByDescending(m => m.Date).ThenByDescending(m => m.RecNo ?? 0).ThenByDescending(m => m.Id, StringComparer.Ordinal)
            .ToList();
        var pageNo = Math.Max(1, page ?? 1);
        return JsonResults.Ok(new CatalogInvoicesResponse
        {
            Items = [.. invoices.Skip((int)Math.Min((long)(pageNo - 1) * InvoicePageSize, int.MaxValue)).Take(InvoicePageSize)
                .Select(m => Fill(new CatalogInvoiceDto(), m))],
            Total = invoices.Count,
        });
    }

    /// <summary>One invoice of the customer's list, with its lines; a key outside that list is 404 whatever it names.</summary>
    private static async Task<IResult> InvoiceDetailAsync(HttpContext http, string? key,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var session = CatalogSession.Of(http);
        if (!session.Account.ShowInvoices) return FeatureDisabled();
        var (customer, movements) = await LedgerAsync(session, db, cache, ct);
        var invoice = string.IsNullOrEmpty(key) ? null : Invoices(customer, movements).FirstOrDefault(m => m.DocumentKey == key);
        if (invoice is null || PortalLedger.Document(customer, movements, invoice.DocumentKey!) is not { } document)
            return Error(StatusCodes.Status404NotFound, "NOT_FOUND", "Fatura bulunamadı.");
        var view = await CustomerCatalogPublicEndpoints.CustomerViewAsync(http, db, views, ct);
        var detail = Fill(new CatalogInvoiceDetailDto(), invoice);
        detail.Lines = [.. document.Lines.Select(l => new CatalogInvoiceLineDto
        {
            Code = l.StockCode,
            Name = l.Name,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            Amount = l.Amount,
            ProductKey = view.Find(l.StockCode)?.Code,
        })];
        return JsonResults.Ok(detail);
    }

    /// <summary>
    /// The products on the customer's sale invoices (not cancelled ones), one row each: the latest date, the quantity
    /// altogether and on how many invoices; the product as the catalog shows it when the customer sees it.
    /// </summary>
    private static async Task<IResult> PurchasedAsync(HttpContext http, string? q, int? page,
        [FromServices] CentralApiDbContext db, [FromServices] IMemoryCache cache, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var session = CatalogSession.Of(http);
        if (!session.Account.ShowPurchased) return FeatureDisabled();
        var (customer, movements) = await LedgerAsync(session, db, cache, ct);
        var bought = new Dictionary<string, Bought>(StringComparer.OrdinalIgnoreCase);
        foreach (var sale in Invoices(customer, movements).Where(m => m.Kind == "sale" && !m.Voided))
        {
            foreach (var line in movements.LinesByDocument.GetValueOrDefault(sale.DocumentKey!) ?? [])
            {
                if (!bought.TryGetValue(line.StockCode, out var row)) bought[line.StockCode] = row = new Bought();
                if (sale.Date > row.Last) row.Last = sale.Date;
                row.Quantity += line.Quantity;
                row.Documents.Add(sale.DocumentKey!);
            }
        }

        var rows = bought.Select(b => (Code: b.Key, Name: movements.StockNames.GetValueOrDefault(b.Key) ?? b.Key, b.Value.Last, b.Value.Quantity, Times: b.Value.Documents.Count));
        var search = q?.Trim();
        if (search is { Length: >= CustomerCatalogPublicEndpoints.MinQueryLength })
            rows = rows.Where(r => Matches(r.Name, search) || Matches(r.Code, search));
        var sorted = rows.OrderByDescending(r => r.Last).ThenBy(r => r.Name, ByText).ThenBy(r => r.Code, ByText).ToList();
        var view = await CustomerCatalogPublicEndpoints.CustomerViewAsync(http, db, views, ct);
        var pageNo = Math.Max(1, page ?? 1);
        var size = CustomerCatalogPublicEndpoints.DefaultPageSize;
        return JsonResults.Ok(new CatalogPurchasedResponse
        {
            Items = [.. sorted.Skip((int)Math.Min((long)(pageNo - 1) * size, int.MaxValue)).Take(size).Select(r => new CatalogPurchasedDto
            {
                Code = r.Code,
                Name = r.Name,
                LastDate = Day(r.Last),
                TotalQuantity = r.Quantity,
                Times = r.Times,
                Product = view.Find(r.Code) is { } product ? CustomerCatalogPublicEndpoints.ProductOf<CatalogCustomerProductDto>(view, product) : null,
            })],
            Total = sorted.Count,
        });
    }

    /// <summary>
    /// The customer's invoices: sales and sale returns on their side of the ledger (closed peşin ones too), one row per
    /// document. A kasa/banka-side row is left out even when its code equals the customer's.
    /// </summary>
    private static IEnumerable<PortalLedger.Movement> Invoices(PortalLedger.Customer customer, PortalLedger.Movements movements) =>
        (movements.ByCustomer.GetValueOrDefault(customer.Code) ?? [])
            .Where(m => m.Kind is "sale" or "sale_return" && !m.OtherSide && m.DocumentKey is not null)
            .DistinctBy(m => m.DocumentKey, StringComparer.Ordinal);

    private static async Task<(PortalLedger.Customer Customer, PortalLedger.Movements Movements)> LedgerAsync(
        CatalogSession session, CentralApiDbContext db, IMemoryCache cache, CancellationToken ct)
    {
        var account = session.Account;
        var customers = await PortalLedger.CustomersAsync(db, cache, session.Tenant.Id, session.Tenant.DataSource, ct);
        // A customer gone from the mirror still has an account; it shows an empty ledger.
        var customer = customers.GetValueOrDefault(account.CustomerCode)
            ?? new PortalLedger.Customer(account.CustomerCode, account.CustomerName, 0m, null, null, null, null, null, null, null, null, false, null, null);
        return (customer, await PortalLedger.MovementsAsync(db, cache, session.Tenant.Id, ct));
    }

    private sealed class Bought
    {
        public DateTime Last { get; set; } = DateTime.MinValue;

        public decimal Quantity { get; set; }

        public HashSet<string> Documents { get; } = new(StringComparer.Ordinal);
    }

    private static T Fill<T>(T dto, PortalLedger.Movement m) where T : CatalogInvoiceDto
    {
        dto.Key = m.DocumentKey!;
        dto.Date = Day(m.Date);
        dto.DocumentNo = m.DocumentNo;
        dto.Kind = m.Kind;
        dto.Total = m.Debit + m.Credit;
        return dto;
    }

    private static bool TryDay(string? text, out DateOnly? day)
    {
        day = null;
        if (string.IsNullOrWhiteSpace(text)) return true;
        if (!DateOnly.TryParseExact(text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)) return false;
        day = parsed;
        return true;
    }

    private static string Day(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static bool Matches(string text, string query) =>
        Turkish.IndexOf(text, query, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

    private static IResult FeatureDisabled() =>
        Error(StatusCodes.Status403Forbidden, "FEATURE_DISABLED", "Bu bölüm hesabınızda açık değil.");
}
