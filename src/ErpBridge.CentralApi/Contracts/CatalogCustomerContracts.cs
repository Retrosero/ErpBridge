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
    [JsonPropertyName("brands")] public string[] Brands { get; set; } = [];

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

/// <summary><c>GET banners</c>: the live banners in their order (S12); empty when there are none.</summary>
public sealed class CatalogCustomerBannersResponse
{
    [JsonPropertyName("items")] public CatalogCustomerBannerDto[] Items { get; set; } = [];
}

public sealed class CatalogCustomerBannerDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;

    [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;

    [JsonPropertyName("image")] public CatalogCustomerImageDto? Image { get; set; }

    /// <summary>Where a click goes; null when nowhere, or when the target is not in this customer's catalog.</summary>
    [JsonPropertyName("link")] public CatalogCustomerBannerLinkDto? Link { get; set; }
}

public sealed class CatalogCustomerBannerLinkDto
{
    /// <summary><c>category</c>, <c>product</c> or <c>url</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;

    /// <summary>The category name, the stock code or the https address.</summary>
    [JsonPropertyName("value")] public string Value { get; set; } = string.Empty;

    /// <summary>For <c>category</c>: the id <c>products?category=</c> takes.</summary>
    [JsonPropertyName("categoryId")] public string? CategoryId { get; set; }

    /// <summary>For <c>product</c>: the key <c>products/detail?key=</c> takes.</summary>
    [JsonPropertyName("productKey")] public string? ProductKey { get; set; }
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

    /// <summary>
    /// The product's picture now (as <c>products</c>' <c>thumb</c>): the cart takes it over the one kept when the product
    /// went in, so a picture added later (an XML sync, a product photo) shows in the cart too.
    /// </summary>
    [JsonPropertyName("thumb")] public string? Thumb { get; set; }

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

// ---- order requests ----------------------------------------------------------------------

/// <summary><c>POST orders</c>: the cart as a request; the server prices it again and compares with what the page showed.</summary>
public sealed class CatalogOrderRequest
{
    /// <summary>Made by the page once per cart: a repeated submit returns the same request.</summary>
    [JsonPropertyName("requestId")] public Guid RequestId { get; set; }

    [JsonPropertyName("lines")] public CatalogCartLineDto[]? Lines { get; set; }

    [JsonPropertyName("note")] public string? Note { get; set; }

    /// <summary>The total the customer saw; more than 0,05 off the server's is 409 <c>PRICE_CHANGED</c>.</summary>
    [JsonPropertyName("expectedTotal")] public decimal? ExpectedTotal { get; set; }
}

/// <summary>409 <c>PRICE_CHANGED</c> and 422 <c>CART_INVALID</c>: the error with the cart priced as it is now.</summary>
public sealed class CatalogQuoteErrorDto
{
    [JsonPropertyName("errorCode")] public string ErrorCode { get; set; } = string.Empty;

    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;

    [JsonPropertyName("traceId")] public string? TraceId { get; set; }

    [JsonPropertyName("quote")] public CatalogQuoteDto Quote { get; set; } = new();
}

public sealed class CatalogOrderResponse
{
    [JsonPropertyName("order")] public CatalogCustomerOrderDto Order { get; set; } = new();
}

public sealed class CatalogCustomerOrdersResponse
{
    [JsonPropertyName("items")] public CatalogCustomerOrderDto[] Items { get; set; } = [];
}

/// <summary><c>COrder</c>: one of the customer's requests.</summary>
public class CatalogCustomerOrderDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("no")] public string No { get; set; } = string.Empty;

    /// <summary><c>NEW</c> (Alındı), <c>CLAIMED</c> (İnceleniyor), <c>COMPLETED</c> (Siparişe çevrildi), <c>REJECTED</c> (Reddedildi).</summary>
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;

    [JsonPropertyName("total")] public decimal Total { get; set; }

    [JsonPropertyName("lineCount")] public int LineCount { get; set; }

    [JsonPropertyName("submittedAtMs")] public long SubmittedAtMs { get; set; }

    [JsonPropertyName("rejectReason")] public string? RejectReason { get; set; }
}

public sealed class CatalogCustomerOrderDetailDto : CatalogCustomerOrderDto
{
    [JsonPropertyName("note")] public string? Note { get; set; }

    [JsonPropertyName("lines")] public CatalogCustomerOrderLineDto[] Lines { get; set; } = [];
}

public sealed class CatalogCustomerOrderLineDto
{
    [JsonPropertyName("thumb")] public string? Thumb { get; set; }

    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;

    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }

    /// <summary>The unit price after the customer's discount, as the request was priced.</summary>
    [JsonPropertyName("net")] public decimal Net { get; set; }

    [JsonPropertyName("total")] public decimal Total { get; set; }
}

// ---- account: statement, invoices, purchased ---------------------------------------------

public sealed class CatalogStatementResponse
{
    /// <summary>The customer's balance now (positive: they owe the company).</summary>
    [JsonPropertyName("balance")] public decimal Balance { get; set; }

    /// <summary>Newest first; no description (staff notes stay inside the company).</summary>
    [JsonPropertyName("rows")] public CatalogStatementRowDto[] Rows { get; set; } = [];
}

public sealed class CatalogStatementRowDto
{
    /// <summary><c>yyyy-MM-dd</c>.</summary>
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;

    /// <summary><c>sale</c>, <c>sale_return</c>, <c>purchase</c>, <c>purchase_return</c>, <c>collection</c>, <c>payment</c> or <c>other</c>.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("documentNo")] public string? DocumentNo { get; set; }

    [JsonPropertyName("debit")] public decimal Debit { get; set; }

    [JsonPropertyName("credit")] public decimal Credit { get; set; }

    /// <summary>The running balance after this row.</summary>
    [JsonPropertyName("balance")] public decimal Balance { get; set; }
}

public sealed class CatalogInvoicesResponse
{
    [JsonPropertyName("items")] public CatalogInvoiceDto[] Items { get; set; } = [];

    [JsonPropertyName("total")] public int Total { get; set; }
}

public class CatalogInvoiceDto
{
    /// <summary>What <c>invoices/detail?key=</c> takes; may hold <c>|</c> and <c>/</c>, so it travels URL-encoded in the query.</summary>
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;

    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;

    [JsonPropertyName("documentNo")] public string? DocumentNo { get; set; }

    /// <summary><c>sale</c> or <c>sale_return</c>.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("total")] public decimal Total { get; set; }
}

public sealed class CatalogInvoiceDetailDto : CatalogInvoiceDto
{
    [JsonPropertyName("lines")] public CatalogInvoiceLineDto[] Lines { get; set; } = [];
}

public sealed class CatalogInvoiceLineDto
{
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }

    [JsonPropertyName("unitPrice")] public decimal UnitPrice { get; set; }

    [JsonPropertyName("amount")] public decimal Amount { get; set; }

    /// <summary>The product's catalog key when the customer sees it (to order it again); otherwise null.</summary>
    [JsonPropertyName("productKey")] public string? ProductKey { get; set; }
}

public sealed class CatalogPurchasedResponse
{
    [JsonPropertyName("items")] public CatalogPurchasedDto[] Items { get; set; } = [];

    [JsonPropertyName("total")] public int Total { get; set; }
}

public sealed class CatalogPurchasedDto
{
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary><c>yyyy-MM-dd</c> of the latest invoice with it.</summary>
    [JsonPropertyName("lastDate")] public string LastDate { get; set; } = string.Empty;

    [JsonPropertyName("totalQuantity")] public decimal TotalQuantity { get; set; }

    /// <summary>On how many invoices.</summary>
    [JsonPropertyName("times")] public int Times { get; set; }

    /// <summary>The product as the customer sees it now; null when it is not in their catalog.</summary>
    [JsonPropertyName("product")] public CatalogCustomerProductDto? Product { get; set; }
}
