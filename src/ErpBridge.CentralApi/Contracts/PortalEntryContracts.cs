namespace ErpBridge.CentralApi.Contracts;

// GOAL_PANEL_GIRIS: the panel enters the documents the phone enters (sale, collection, purchase, return, tediye,
// expense). The server builds Sipariş Cepte's own document body from these requests, so the ERP agent's translator and
// the native ledger take a panel entry exactly as they take the phone's.

/// <summary><c>GET /api/v1/portal/entry/context</c>: what the entry pages offer the signed-in user.</summary>
public sealed class PortalEntryContextResponse
{
    /// <summary><c>erp</c> or <c>native</c>.</summary>
    public string DataSource { get; set; } = string.Empty;

    /// <summary>Istanbul today, <c>yyyy-MM-dd</c>: the latest document date (K6: no future dates, any past one).</summary>
    public string Today { get; set; } = string.Empty;

    /// <summary>The kinds the user may enter: <c>sale</c>, <c>collection</c>, <c>purchase</c>, <c>return</c>, <c>disbursement</c>, <c>expense</c>.</summary>
    public List<string> Kinds { get; set; } = [];

    /// <summary>A sale on account ("Cari Borç") is the user's to make (<c>action.sale.open_account</c>).</summary>
    public bool CanSellOnAccount { get; set; }

    /// <summary>A sale may take stock below zero (<c>action.sale.negative_stock</c>).</summary>
    public bool CanSellBelowStock { get; set; }

    /// <summary>Whose name a document can go out in: the user, and the company's phone users.</summary>
    public List<PortalEntryOwnerDto> Owners { get; set; } = [];

    public List<PortalEntryPriceListDto> PriceLists { get; set; } = [];

    /// <summary>ERP company: the ERP's warehouses, cash accounts and banks (codes the documents carry).</summary>
    public List<PortalErpLookupItem> Warehouses { get; set; } = [];

    public List<PortalErpLookupItem> CashAccounts { get; set; } = [];

    public List<PortalErpLookupItem> Banks { get; set; } = [];
}

public sealed class PortalEntryOwnerDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsSelf { get; set; }
}

public sealed class PortalEntryPriceListDto
{
    public int No { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IncludesVat { get; set; }
}

/// <summary><c>GET /api/v1/portal/entry/customers?q=</c>: the customers a document can be made out to.</summary>
public sealed class PortalEntryCustomersResponse
{
    public List<PortalEntryCustomerDto> Items { get; set; } = [];
    public int Total { get; set; }
}

public sealed class PortalEntryCustomerDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Phone { get; set; }

    /// <summary>Only with <c>view.customer.balance</c>.</summary>
    public decimal? Balance { get; set; }
}

/// <summary><c>GET /api/v1/portal/entry/products?q=</c>: the products a line can take, with their list prices and stock.</summary>
public sealed class PortalEntryProductsResponse
{
    public List<PortalEntryProductDto> Items { get; set; } = [];
    public int Total { get; set; }
}

public sealed class PortalEntryProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? Barcode { get; set; }
    public decimal VatRate { get; set; }

    /// <summary>List number → price (only lists with a price).</summary>
    public Dictionary<int, decimal> Prices { get; set; } = [];

    /// <summary>The list a sale takes the product's price from unless the form picks one (the phone's headline list).</summary>
    public int? DefaultPriceListNo { get; set; }

    /// <summary>All warehouses together.</summary>
    public decimal Stock { get; set; }
}

/// <summary><c>POST /api/v1/portal/entry/sale[/preview]</c>.</summary>
public sealed class PortalEntrySaleRequest
{
    /// <summary>One per save attempt, resent on retry: the document's key (<c>PNL-SO-{operationId}</c>). A GUID.</summary>
    public string? OperationId { get; set; }

    /// <summary>Whose document it is (default: the user); an ERP company's agent writes it with that person's mapping.</summary>
    public Guid? OwnerUserId { get; set; }

    /// <summary><c>yyyy-MM-dd</c>; today when absent.</summary>
    public string? Date { get; set; }

    public string CustomerCode { get; set; } = string.Empty;

    /// <summary>The list every line is priced from; absent = each product's own headline list.</summary>
    public int? PriceListNo { get; set; }

    public List<PortalEntrySaleLineRequest> Lines { get; set; } = [];

    /// <summary>The order discount, 0–100 (the phone's "genel iskonto").</summary>
    public decimal GeneralDiscountPercent { get; set; }

    /// <summary><c>Cari Borç</c> (default), <c>Nakit</c> or <c>Kredi Kartı</c>.</summary>
    public string? PaymentType { get; set; }

    /// <summary>The card's ERP bank (a <c>bank</c> lookup) for <c>Kredi Kartı</c>.</summary>
    public string? BankCode { get; set; }

    public string? Note { get; set; }

    /// <summary>The total the user saw (create only): a price that moved since is not written silently.</summary>
    public decimal? ExpectedTotal { get; set; }
}

public sealed class PortalEntrySaleLineRequest
{
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>A whole number above zero, as on the phone.</summary>
    public decimal Quantity { get; set; }

    public decimal LineDiscountPercent { get; set; }
    public string? Note { get; set; }
}

/// <summary>What an entry would write: the priced lines and totals, whose document it is, and anything in the way.</summary>
public sealed class PortalEntryPreviewResponse
{
    public string Kind { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }

    /// <summary>The document date and time as the document carries it.</summary>
    public string OccurredAt { get; set; } = string.Empty;

    public int? PriceListNo { get; set; }
    public string? PriceListName { get; set; }
    public bool PriceIncludesVat { get; set; }
    public List<PortalEntryPricedLineDto> Lines { get; set; } = [];
    public decimal Gross { get; set; }
    public decimal Discount { get; set; }
    public decimal Vat { get; set; }
    public decimal Total { get; set; }

    /// <summary>Why the document cannot be saved (permission, limit, stock, ERP mapping); null when it can.</summary>
    public PortalEntryRefusalDto? Refusal { get; set; }

    /// <summary>Lines asking for more than is in stock (all warehouses together).</summary>
    public List<PortalEntryStockWarningDto> StockWarnings { get; set; } = [];
}

public sealed class PortalEntryPricedLineDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal Quantity { get; set; }
    public int? PriceListNo { get; set; }
    public decimal ListUnitPrice { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public decimal GeneralDiscountPercent { get; set; }
    public decimal VatRate { get; set; }
    public decimal Gross { get; set; }
    public decimal Discount { get; set; }
    public decimal Net { get; set; }
    public decimal Vat { get; set; }
    public decimal Total { get; set; }
    public string? Note { get; set; }
}

public sealed class PortalEntryRefusalDto
{
    /// <summary><c>ENTRY_MODULE_DENIED</c>, <c>ENTRY_LIMIT_EXCEEDED</c>, <c>ENTRY_NEGATIVE_STOCK</c>, <c>ERP_MAPPING_MISSING</c> or <c>ENTRY_DOCUMENT_INVALID</c>.</summary>
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    /// <summary>The permission gone over, when it is one.</summary>
    public string? Key { get; set; }
}

public sealed class PortalEntryStockWarningDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Requested { get; set; }
    public decimal Available { get; set; }
}

/// <summary>A saved entry: the job(s) it wrote — or found, when the same operation was sent again.</summary>
public sealed class PortalEntryCreateResponse
{
    public List<PortalEntryDocumentDto> Documents { get; set; } = [];
    public bool Idempotent { get; set; }
    public PortalEntryPreviewResponse? Preview { get; set; }
}

public sealed class PortalEntryDocumentDto
{
    public Guid JobId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
