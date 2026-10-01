using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

// Müşteri kataloğu, müşteri tarafı: /api/v1/catalog/{code} (docs/GOAL_MUSTERI_KATALOGU.md §5.2), the web catalog the
// company's customers use. The session is an HttpOnly cookie, never in a body. A product's key is its stock code and
// travels in the query or the body, never in the path. Prices are the server's; the browser only shows them.

// ---- session -----------------------------------------------------------------------------

public sealed class CatalogInfoResponse
{
    [JsonPropertyName("companyName")] public string CompanyName { get; set; } = string.Empty;

    /// <summary>The company code as the catalog's address writes it (upper case).</summary>
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
}

public sealed class CatalogLoginRequest
{
    [JsonPropertyName("username")] public string? Username { get; set; }

    [JsonPropertyName("password")] public string? Password { get; set; }

    /// <summary>A 30-day cookie; otherwise a browser-session cookie over a 12-hour token.</summary>
    [JsonPropertyName("remember")] public bool Remember { get; set; }
}

public sealed class CatalogLoginResponse
{
    [JsonPropertyName("me")] public CatalogMeDto Me { get; set; } = new();
}

public sealed class CatalogMeDto
{
    [JsonPropertyName("companyName")] public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;

    [JsonPropertyName("customer")] public CatalogCustomerDto Customer { get; set; } = new();

    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;

    [JsonPropertyName("discountPercent")] public decimal DiscountPercent { get; set; }

    /// <summary>The list the customer buys from; null when the company has no prices at all.</summary>
    [JsonPropertyName("priceList")] public CatalogPriceListDto? PriceList { get; set; }

    [JsonPropertyName("features")] public CatalogFeaturesDto Features { get; set; } = new();

    /// <summary>Only when the statement is shown to the customer; otherwise null.</summary>
    [JsonPropertyName("balance")] public CatalogBalanceDto? Balance { get; set; }
}

public sealed class CatalogCustomerDto
{
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

public sealed class CatalogFeaturesDto
{
    [JsonPropertyName("order")] public bool Order { get; set; }

    [JsonPropertyName("statement")] public bool Statement { get; set; }

    [JsonPropertyName("invoices")] public bool Invoices { get; set; }

    [JsonPropertyName("purchased")] public bool Purchased { get; set; }
}

public sealed class CatalogBalanceDto
{
    /// <summary>Positive: the customer owes the company.</summary>
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
}

public sealed class CatalogChangePasswordRequest
{
    [JsonPropertyName("current")] public string? Current { get; set; }

    [JsonPropertyName("next")] public string? Next { get; set; }
}

// ---- browsing ----------------------------------------------------------------------------

public sealed class CatalogCustomerCategoriesResponse
{
    /// <summary>Only categories with something the customer sees, in the catalog's order.</summary>
    [JsonPropertyName("items")] public CatalogCustomerCategoryDto[] Items { get; set; } = [];
}

public sealed class CatalogCustomerCategoryDto
{
    /// <summary>The first 12 hex digits of SHA-256(category key): what <c>products?category=</c> takes.</summary>
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary>Products the customer sees in it.</summary>
    [JsonPropertyName("count")] public int Count { get; set; }
}

public sealed class CatalogCustomerProductsResponse
{
    [JsonPropertyName("items")] public CatalogCustomerProductDto[] Items { get; set; } = [];

    [JsonPropertyName("total")] public int Total { get; set; }

    [JsonPropertyName("page")] public int Page { get; set; }

    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
}

/// <summary><c>CProduct</c>: one product as the customer sees it.</summary>
public class CatalogCustomerProductDto
{
    /// <summary>What the cart and <c>products/detail?key=</c> take: the stock code.</summary>
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;

    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("unit")] public string? Unit { get; set; }

    [JsonPropertyName("brand")] public string? Brand { get; set; }

    [JsonPropertyName("categoryId")] public string CategoryId { get; set; } = string.Empty;

    [JsonPropertyName("price")] public CatalogCustomerPriceDto Price { get; set; } = new();

    /// <summary>The carton (at least 2); null when the product has none.</summary>
    [JsonPropertyName("box")] public CatalogBoxDto? Box { get; set; }

    /// <summary>Whether it can be ordered now; the quantity in stock is never shown (K6).</summary>
    [JsonPropertyName("inStock")] public bool InStock { get; set; }

    /// <summary>The first picture: a relative <c>/api/v1/catalog/img/…</c> path or an https link.</summary>
    [JsonPropertyName("thumb")] public string? Thumb { get; set; }
}

public sealed class CatalogCustomerProductDetailDto : CatalogCustomerProductDto
{
    [JsonPropertyName("images")] public CatalogCustomerImageDto[] Images { get; set; } = [];
}

public sealed class CatalogCustomerImageDto
{
    [JsonPropertyName("thumb")] public string Thumb { get; set; } = string.Empty;

    [JsonPropertyName("full")] public string Full { get; set; } = string.Empty;
}

public sealed class CatalogCustomerPriceDto
{
    /// <summary>The list price (struck through when <see cref="Net"/> is lower).</summary>
    [JsonPropertyName("list")] public decimal List { get; set; }

    /// <summary>The list price less the customer's discount, two decimals.</summary>
    [JsonPropertyName("net")] public decimal Net { get; set; }

    /// <summary>The customer's discount; 0 on a product the company sells without discount.</summary>
    [JsonPropertyName("discountPercent")] public decimal DiscountPercent { get; set; }

    /// <summary>The list's prices carry the VAT: "KDV dahil", else "KDV hariç".</summary>
    [JsonPropertyName("includesVat")] public bool IncludesVat { get; set; }
}

public sealed class CatalogBoxDto
{
    [JsonPropertyName("qty")] public int Qty { get; set; }

    /// <summary>Sold only in whole cartons: the quantity must be a multiple of <see cref="Qty"/>.</summary>
    [JsonPropertyName("only")] public bool Only { get; set; }
}

// ---- quote -------------------------------------------------------------------------------

public sealed class CatalogQuoteRequest
{
    [JsonPropertyName("lines")] public CatalogCartLineDto[]? Lines { get; set; }
}

public sealed class CatalogCartLineDto
{
    [JsonPropertyName("key")] public string? Key { get; set; }

    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
}

/// <summary>The cart priced by the server, line by line as the phone books the sale (<c>CatalogPricing</c>).</summary>
public sealed class CatalogQuoteDto
{
    [JsonPropertyName("lines")] public CatalogQuoteLineDto[] Lines { get; set; } = [];

    /// <summary>The lines without an issue.</summary>
    [JsonPropertyName("totals")] public CatalogQuoteTotalsDto Totals { get; set; } = new();
}

public sealed class CatalogQuoteLineDto
{
    [JsonPropertyName("key")] public string? Key { get; set; }

    /// <summary>Null (as name, unit, box, price) for a product the customer does not see (<c>NOT_AVAILABLE</c>).</summary>
    [JsonPropertyName("code")] public string? Code { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("unit")] public string? Unit { get; set; }

    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }

    [JsonPropertyName("box")] public CatalogBoxDto? Box { get; set; }

    [JsonPropertyName("price")] public CatalogCustomerPriceDto? Price { get; set; }

    [JsonPropertyName("vatRate")] public decimal? VatRate { get; set; }

    /// <summary>VAT-exclusive list amount (an inclusive list price is first divided by 1 + rate).</summary>
    [JsonPropertyName("gross")] public decimal Gross { get; set; }

    [JsonPropertyName("discount")] public decimal Discount { get; set; }

    [JsonPropertyName("vat")] public decimal Vat { get; set; }

    [JsonPropertyName("total")] public decimal Total { get; set; }

    /// <summary><c>NOT_AVAILABLE</c>, <c>OUT_OF_STOCK</c>, <c>CARTON_MULTIPLE</c>, <c>INVALID_QUANTITY</c> or null.</summary>
    [JsonPropertyName("issue")] public string? Issue { get; set; }
}

public sealed class CatalogQuoteTotalsDto
{
    [JsonPropertyName("gross")] public decimal Gross { get; set; }

    [JsonPropertyName("discount")] public decimal Discount { get; set; }

    [JsonPropertyName("vat")] public decimal Vat { get; set; }

    [JsonPropertyName("total")] public decimal Total { get; set; }
}
