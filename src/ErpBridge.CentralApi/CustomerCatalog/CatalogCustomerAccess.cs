using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>Whether a company's catalog is open to its customers (GOAL_MUSTERI_KATALOGU §5.2).</summary>
public static class CatalogCustomerAccess
{
    /// <summary>Open: the company is active, the operator's module is on and the company published its catalog.</summary>
    public static async Task<bool> IsOpenAsync(CentralApiDbContext db, Tenant tenant, CancellationToken ct) =>
        tenant.IsActive
        && await db.TenantModules.AsNoTracking().AnyAsync(m => m.TenantId == tenant.Id && m.ModuleKey == TenantModules.CustomerCatalog, ct)
        && await db.CatalogSettings.AsNoTracking().AnyAsync(s => s.TenantId == tenant.Id && s.IsEnabled, ct);

    /// <summary>The company of an open catalog by its code (any case), or null — an unknown and a closed code look the same.</summary>
    public static async Task<Tenant?> OpenCompanyAsync(CentralApiDbContext db, string? code, CancellationToken ct)
    {
        if (code is null || !CatalogCookies.CodePattern().IsMatch(code)) return null;
        var upper = code.ToUpperInvariant();
        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Code == upper, ct);
        return tenant is not null && await IsOpenAsync(db, tenant, ct) ? tenant : null;
    }
}

/// <summary>
/// One customer's view of the company catalog (GOAL_MUSTERI_KATALOGU §4): the products their visibility allows that
/// have a price in their list (no falling back to another list, T9), at their discount (none on a <c>noDiscount</c>
/// product). Built per request over the cached <see cref="CatalogView"/>.
/// </summary>
public sealed class CatalogCustomerView(CatalogView view, CatalogAccount account)
{
    private readonly CatalogVisibility _visibility = CatalogVisibility.Parse(account.VisibilityJson);

    public CatalogView Catalog => view;

    /// <summary>The customer's own list, else the company default (§4).</summary>
    public int? PriceListNo { get; } = view.EffectivePriceListNo(account.PriceListNo);

    public CatalogPriceList? PriceList => PriceListNo is { } no ? view.PriceList(no) : null;

    /// <summary>The list's prices carry the VAT ("KDV dahil", K5).</summary>
    public bool IncludesVat => PriceList?.IncludesVat ?? false;

    public bool Sees(CatalogProduct product) =>
        product.PriceIn(PriceListNo) is not null
        && _visibility.IsVisible(product.Code, product.Hidden, product.CategoryKey, view.Category(product.CategoryKey)?.Hidden == true);

    /// <summary>Everything the customer sees, in the catalog's order.</summary>
    public IEnumerable<CatalogProduct> Products => view.Categories.SelectMany(c => c.Products).Where(Sees);

    /// <summary>A product the customer sees, by its stock code (any case); null otherwise — a hidden one is not found.</summary>
    public CatalogProduct? Find(string? key)
    {
        var code = key?.Trim();
        return !string.IsNullOrEmpty(code) && view.Products.TryGetValue(code, out var product) && Sees(product) ? product : null;
    }

    public decimal ListPrice(CatalogProduct product) => product.PriceIn(PriceListNo) ?? 0m;

    public decimal DiscountPercent(CatalogProduct product) => CatalogPricing.DiscountFor(product.NoDiscount, account.DiscountPercent);
}
