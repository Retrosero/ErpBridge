namespace ErpBridge.CentralApi.Contracts;

/// <summary>A count of documents and their total amount.</summary>
public sealed class PortalMoneyLine
{
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>GET /api/v1/portal/summary — one business day of the whole company.</summary>
public sealed class PortalSummaryResponse
{
    public string Date { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public PortalMoneyLine Sales { get; set; } = new();
    public PortalMoneyLine Collections { get; set; } = new();
    public PortalMoneyLine Disbursements { get; set; } = new();
    public PortalMoneyLine Returns { get; set; } = new();
    public int VisitsPlanned { get; set; }
    public int VisitsCompleted { get; set; }
    public int VisitsSkipped { get; set; }
    public int PendingApprovals { get; set; }
}

/// <summary>One salesperson's work over a date range.</summary>
public sealed class PortalUserActivity
{
    /// <summary>Null for documents no signed-in user sent (API keys, rows from before Faz 41).</summary>
    public Guid? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public PortalMoneyLine Sales { get; set; } = new();
    public PortalMoneyLine Collections { get; set; } = new();
    public PortalMoneyLine Disbursements { get; set; } = new();
    public PortalMoneyLine Returns { get; set; } = new();
    public int VisitsCompleted { get; set; }
    public int VisitsSkipped { get; set; }
}

/// <summary>GET /api/v1/portal/activity</summary>
public sealed class PortalActivityResponse
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public List<PortalUserActivity> Users { get; set; } = [];
}

/// <summary>A planned stop and what happened at it, or a visit made outside the plan.</summary>
public sealed class PortalVisitRow
{
    public string Username { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string StopId { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int VisitOrder { get; set; }

    /// <summary><c>PENDING</c> (planned, nothing recorded), <c>COMPLETED</c> or <c>SKIPPED</c>.</summary>
    public string Status { get; set; } = "PENDING";
    public string Note { get; set; } = string.Empty;
    public long? CompletedAt { get; set; }

    /// <summary>False for a visit recorded at a customer the day's plan did not list.</summary>
    public bool Planned { get; set; } = true;
}

/// <summary>GET /api/v1/portal/visits</summary>
public sealed class PortalVisitsResponse
{
    public string Date { get; set; } = string.Empty;
    public List<PortalVisitRow> Rows { get; set; } = [];
}

public sealed class PortalBalanceRow
{
    public string CustomerCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

/// <summary>GET /api/v1/portal/balances — customers who owe or are owed, largest first.</summary>
public sealed class PortalBalancesResponse
{
    public decimal TotalReceivable { get; set; }
    public decimal TotalPayable { get; set; }
    public List<PortalBalanceRow> Rows { get; set; } = [];
    public bool Truncated { get; set; }
}

public sealed class PortalStockRow
{
    public string StockCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

/// <summary>GET /api/v1/portal/stock</summary>
public sealed class PortalStockResponse
{
    public List<PortalStockRow> Rows { get; set; } = [];
    public bool Truncated { get; set; }
}

/// <summary>One warehouse's share of a product.</summary>
public sealed class PortalStockWarehouseQuantity
{
    public int WarehouseNo { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Reserved { get; set; }
}

/// <summary>A product's price on one price list.</summary>
public sealed class PortalStockPrice
{
    public int ListNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

/// <summary>A product row of GET /api/v1/portal/stock/search.</summary>
public sealed class PortalStockItem
{
    public string StockCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? MainGroup { get; set; }
    public string? SubGroup { get; set; }
    public string? Brand { get; set; }
    public string? Shelf { get; set; }
    public List<string> Barcodes { get; set; } = [];

    /// <summary>In the chosen warehouse, or across all of them.</summary>
    public decimal Quantity { get; set; }
    public decimal Reserved { get; set; }

    /// <summary>On the chosen price list; null when the product has no price there.</summary>
    public decimal? Price { get; set; }

    /// <summary>The latest movement of any warehouse; only an ERP reports it.</summary>
    public string? LastMovementDate { get; set; }

    public List<PortalStockWarehouseQuantity> Warehouses { get; set; } = [];
    public List<PortalStockPrice> Prices { get; set; } = [];
}

/// <summary>Counts over every product the filters match, not only the page.</summary>
public sealed class PortalStockSummary
{
    public int Products { get; set; }
    public int InStock { get; set; }
    public int OutOfStock { get; set; }
    public int Negative { get; set; }
}

/// <summary>GET /api/v1/portal/stock/search</summary>
public sealed class PortalStockSearchResponse
{
    public List<PortalStockItem> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public PortalStockSummary Summary { get; set; } = new();

    /// <summary>The price list the prices are read from; null when the company has no prices.</summary>
    public int? PriceList { get; set; }
    public int? WarehouseNo { get; set; }
}

public sealed class PortalFacetValue
{
    public string Code { get; set; } = string.Empty;
    public int Count { get; set; }

    /// <summary>For a sub group: the main group it sits under.</summary>
    public string? Parent { get; set; }
}

/// <summary>
/// Body of <c>POST /api/v1/portal/native/stock-cards</c> (GOAL_PANEL_ERPSIZ E1a) — creating and
/// editing share this shape; the product is found or made by <see cref="StockCode"/>, which never
/// changes once set. Wraps the same fields as the phone's <c>stock_card</c> document.
/// </summary>
public sealed class PortalStockCardRequest
{
    public string StockCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal? VatRate { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Aisle { get; set; }
    public string? Barcode { get; set; }
    public decimal? Price { get; set; }

    /// <summary>Taken only when the product has no stock yet; ignored when editing an existing card.</summary>
    public decimal? OpeningQuantity { get; set; }

    /// <summary>
    /// Set once per save attempt and resent unchanged on a retry, so a lost response does not open
    /// a second job; left empty, a fresh one is used and a retry becomes a new attempt.
    /// </summary>
    public string? OperationId { get; set; }
}

/// <summary>
/// Body of <c>POST /api/v1/portal/native/customer-cards</c> (GOAL_PANEL_ERPSIZ E2a) — creating and
/// editing share this shape; the customer is found or made by <see cref="CustomerCode"/>, which
/// never changes once set. Wraps the same fields as the phone's <c>customer_card</c> document.
/// </summary>
public sealed class PortalCustomerCardRequest
{
    public string CustomerCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TaxNo { get; set; }
    public string? TaxOffice { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? RegionCode { get; set; }

    /// <summary>Taken only when the customer has no balance yet; ignored when editing an existing card.</summary>
    public decimal? OpeningBalance { get; set; }
    public string? OperationId { get; set; }
}

/// <summary>
/// Body of <c>POST /api/v1/portal/native/collections</c> and <c>…/disbursements</c>
/// (GOAL_PANEL_ERPSIZ E3a) — same shape, the route decides which document type is booked.
/// </summary>
public sealed class PortalPaymentRequest
{
    public string CustomerCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? PaymentType { get; set; }

    /// <summary><c>yyyy-MM-dd</c>; defaults to today when absent.</summary>
    public string? OccurredAt { get; set; }
    public string? Description { get; set; }
    public string? OperationId { get; set; }
}

/// <summary>Body of <c>POST /api/v1/portal/native/ledger/{key}/void</c> (GOAL_PANEL_ERPSIZ E4a).</summary>
public sealed class PortalLedgerVoidRequest
{
    public string? Reason { get; set; }
    public string? OperationId { get; set; }
}

/// <summary>Body of <c>POST /api/v1/portal/native/ledger/{key}/edit</c> (GOAL_PANEL_ERPSIZ E4c) — a
/// void of the target plus a corrected re-booking of the same kind, in one transaction (D11).</summary>
public sealed class PortalLedgerEditRequest
{
    public decimal Amount { get; set; }

    /// <summary>Only meaningful for a manual adjustment; a collection/disbursement keeps its own direction.</summary>
    public bool? Debit { get; set; }
    public string? PaymentType { get; set; }
    public string? Description { get; set; }

    /// <summary>The corrected adjustment's reason; required when the target is a manual adjustment.</summary>
    public string? Reason { get; set; }
    public string? OccurredAt { get; set; }

    /// <summary>Why the original is being corrected — mandatory, the same as a plain void's.</summary>
    public string? VoidReason { get; set; }
    public string? OperationId { get; set; }
}

/// <summary>Body of <c>POST /api/v1/portal/native/ledger-adjustments</c> (GOAL_PANEL_ERPSIZ E4b) — a
/// manual correction of a customer's balance; <see cref="Reason"/> is mandatory, unlike a payment's.</summary>
public sealed class PortalLedgerAdjustmentRequest
{
    public string CustomerCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    /// <summary>True increases what the customer owes (borç), false decreases it (alacak).</summary>
    public bool Debit { get; set; }
    public string? Reason { get; set; }

    /// <summary><c>yyyy-MM-dd</c>; defaults to today when absent.</summary>
    public string? OccurredAt { get; set; }
    public string? OperationId { get; set; }
}

/// <summary>One line of a sale/purchase/return (GOAL_PANEL_ERPSIZ E5a).</summary>
public sealed class PortalNativeDocumentLineRequest
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    /// <summary>Defaults to <c>quantity * unitPrice</c> when absent — set only for a line-level discount.</summary>
    public decimal? LineTotal { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Body of <c>POST /api/v1/portal/native/sales-orders</c>/<c>…/purchase-receipts</c>/<c>…/sales-returns</c>
/// (GOAL_PANEL_ERPSIZ E5a) — the same shape for all three; the route decides which document type is
/// booked and, for a purchase, whether <see cref="PartyCode"/> is written as <c>supplierCode</c>.
/// </summary>
public sealed class PortalNativeDocumentRequest
{
    /// <summary>The customer for a sale/return, the supplier for a purchase.</summary>
    public string PartyCode { get; set; } = string.Empty;
    public List<PortalNativeDocumentLineRequest> Lines { get; set; } = [];

    /// <summary>Defaults to the lines' own total when absent.</summary>
    public decimal? Amount { get; set; }
    public string? PaymentType { get; set; }

    /// <summary><c>yyyy-MM-dd</c>; defaults to today when absent.</summary>
    public string? OccurredAt { get; set; }
    public string? Description { get; set; }
    public string? DocumentNo { get; set; }
    public string? OperationId { get; set; }
}

/// <summary>GET /api/v1/portal/native/stock-cards/{code} — fills the edit form with the card's current fields.</summary>
public sealed class PortalStockCardDetail
{
    public string StockCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal? VatRate { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Aisle { get; set; }
    public List<string> Barcodes { get; set; } = [];

    /// <summary>Every price list the product has a price on; a list it has none on is absent, never zero.</summary>
    public List<PortalStockPrice> Prices { get; set; } = [];
    public decimal Quantity { get; set; }
    public string? LastMovementDate { get; set; }
}

/// <summary>One row of GET /api/v1/portal/native/stock-cards/{code}/movements (GOAL_PANEL_ERPSIZ E6a).</summary>
public sealed class PortalStockMovementRow
{
    public string Id { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;

    /// <summary>sale, purchase, sale_return, count, void (a cancelled line's reversal), other.</summary>
    public string Kind { get; set; } = "other";
    public string? DocumentNo { get; set; }

    /// <summary>The customer/supplier of the document the line belongs to, when it has one.</summary>
    public string? CustomerCode { get; set; }
    public string? Description { get; set; }
    public decimal In { get; set; }
    public decimal Out { get; set; }

    /// <summary>The product's stock after this line (yürüyen stok).</summary>
    public decimal Balance { get; set; }
    public bool Voided { get; set; }
    public string? Reason { get; set; }
}

/// <summary>GET /api/v1/portal/native/stock-cards/{code}/movements — one product's movements, newest first.</summary>
public sealed class PortalStockMovementsResponse
{
    public string StockCode { get; set; } = string.Empty;
    public string? From { get; set; }
    public string? To { get; set; }

    /// <summary>Stock before the first line shown (devir): with no start date, what the product had before any movement.</summary>
    public decimal Opening { get; set; }
    public decimal Closing { get; set; }
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public List<PortalStockMovementRow> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

/// <summary>One line of <c>POST /api/v1/portal/native/stock-counts</c> (GOAL_PANEL_ERPSIZ E6b).</summary>
public sealed class PortalStockCountLine
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal CountedQuantity { get; set; }
}

/// <summary>
/// Body of <c>POST /api/v1/portal/native/stock-counts</c> (GOAL_PANEL_ERPSIZ E6b): what was counted. The difference
/// is taken against the stock the server holds when the count books, so a sale booked meanwhile is not lost.
/// </summary>
public sealed class PortalStockCountRequest
{
    public List<PortalStockCountLine> Lines { get; set; } = [];

    /// <summary>Why the stock is corrected (yıl sonu sayımı, fire, kırık…) — mandatory.</summary>
    public string? Reason { get; set; }

    /// <summary><c>yyyy-MM-dd</c>; defaults to today when absent.</summary>
    public string? OccurredAt { get; set; }
    public string? DocumentNo { get; set; }
    public string? OperationId { get; set; }
}

public sealed class PortalNamedNumber
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>GET /api/v1/portal/stock/facets — what the stock filters can offer.</summary>
public sealed class PortalStockFacetsResponse
{
    public List<PortalFacetValue> MainGroups { get; set; } = [];
    public List<PortalFacetValue> SubGroups { get; set; } = [];
    public List<PortalFacetValue> Brands { get; set; } = [];
    public List<PortalFacetValue> Shelves { get; set; } = [];
    public List<PortalNamedNumber> Warehouses { get; set; } = [];
    public List<PortalNamedNumber> PriceLists { get; set; } = [];

    /// <summary>False without an ERP: the idle-days filter has nothing to work on.</summary>
    public bool HasMovementDates { get; set; }
    public bool HasReserved { get; set; }
}

/// <summary>A customer row of GET /api/v1/portal/customers.</summary>
public sealed class PortalCustomerRow
{
    public string CustomerCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
}

/// <summary>GET /api/v1/portal/customers — every customer, paged; totals cover the whole filter.</summary>
public sealed class PortalCustomersResponse
{
    public List<PortalCustomerRow> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public decimal TotalReceivable { get; set; }
    public decimal TotalPayable { get; set; }
}

/// <summary>GET /api/v1/portal/customers/{code}</summary>
public sealed class PortalCustomerCard
{
    public string CustomerCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxOffice { get; set; }
    public string? TaxNo { get; set; }
    public string? Address { get; set; }
    public string? SalespersonCode { get; set; }
    public string? RegionCode { get; set; }
    public string? GroupCode { get; set; }
    public string? Currency { get; set; }
    public bool IsLocked { get; set; }
    public string DataSource { get; set; } = string.Empty;
}

/// <summary>One movement of a customer statement.</summary>
public sealed class PortalLedgerRow
{
    public string Id { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;

    /// <summary>sale, sale_return, purchase, purchase_return, collection, payment, other.</summary>
    public string Kind { get; set; } = "other";

    /// <summary>What the ERP or the phone called it, for the rare kind "other".</summary>
    public string? SourceType { get; set; }
    public string? DocumentNo { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }

    /// <summary>The balance after this movement, counted over every movement of the range (not only the shown kinds).</summary>
    public decimal Balance { get; set; }

    /// <summary>Set when the movement has lines to open: GET …/documents/{documentKey}.</summary>
    public string? DocumentKey { get; set; }

    /// <summary>GOAL_PANEL_ERPSIZ E4d — cancelled in place (D2 storno); its reversal is its own separate row.</summary>
    public bool Voided { get; set; }
    public Guid? VoidedByUserId { get; set; }

    /// <summary>The name behind <see cref="VoidedByUserId"/>, resolved by the caller; null until then.</summary>
    public string? VoidedBy { get; set; }
    public string? VoidedAt { get; set; }

    /// <summary>Why this entry was voided, or (for a manual adjustment) why it was made.</summary>
    public string? Reason { get; set; }

    /// <summary>Whether "Düzenle"/"İptal et" apply: a standalone collection/disbursement/manual
    /// adjustment that is not already voided — never a sale/purchase/return's own row (E5 instead, D11).</summary>
    public bool Editable { get; set; }
}

/// <summary>GET /api/v1/portal/customers/{code}/ledger</summary>
public sealed class PortalLedgerResponse
{
    public string CustomerCode { get; set; } = string.Empty;
    public string? From { get; set; }
    public string? To { get; set; }

    /// <summary>Balance before the first movement of the range.</summary>
    public decimal Opening { get; set; }
    public decimal Closing { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public List<PortalLedgerRow> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

/// <summary>One row of GET /api/v1/portal/payments — a collection or payment movement, any customer.</summary>
public sealed class PortalPaymentRow
{
    public string Id { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerTitle { get; set; } = string.Empty;

    /// <summary>collection or payment.</summary>
    public string Kind { get; set; } = string.Empty;
    public string? PaymentType { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? DocumentKey { get; set; }
}

/// <summary>One group's total in a GET /api/v1/portal/payments summary: a calendar day or a payment type.</summary>
public sealed class PortalPaymentGroupTotal
{
    public string Key { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

/// <summary>GET /api/v1/portal/payments — company-wide collections/payments (GOAL_PANEL_ERPSIZ E3c).</summary>
public sealed class PortalPaymentsResponse
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public List<PortalPaymentRow> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }

    /// <summary>Sums per calendar day of the whole filtered range (not only the page shown), oldest first.</summary>
    public List<PortalPaymentGroupTotal> DailyTotals { get; set; } = [];

    /// <summary>Sums per payment type of the whole filtered range ("kasa özeti"); a row with none groups under "Diğer".</summary>
    public List<PortalPaymentGroupTotal> PaymentTypeTotals { get; set; } = [];
}

public sealed class PortalDocumentLine
{
    public string StockCode { get; set; } = string.Empty;
    public string? Name { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public decimal? Tax { get; set; }
    public int? WarehouseNo { get; set; }
    public string? Description { get; set; }
}

/// <summary>GET /api/v1/portal/customers/{code}/documents/{documentKey}</summary>
public sealed class PortalDocumentResponse
{
    /// <summary>The document's own ledger-row id (GOAL_PANEL_ERPSIZ E5c) — what <c>POST …/documents/{key}/void</c>
    /// resolves internally as its <c>targetKey</c>; the caller never constructs or reads this itself.</summary>
    public string Id { get; set; } = string.Empty;
    public string DocumentKey { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;

    /// <summary>GOAL_PANEL_ERPSIZ E5b — the caller of <c>GET /portal/native/documents/{key}</c> does not
    /// already know the customer/supplier the way the older code-scoped <c>/customers/document</c> caller did.</summary>
    public string CustomerTitle { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Kind { get; set; } = "other";
    public string? DocumentNo { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public List<PortalDocumentLine> Lines { get; set; } = [];
    public bool LinesAvailable { get; set; }

    /// <summary>GOAL_PANEL_ERPSIZ E5c — whether <c>document_void</c> already cancelled this document.</summary>
    public bool Voided { get; set; }

    /// <summary>GOAL_PANEL_ERPSIZ E5e — how a native document was settled on the spot (its immediate-payment leg's
    /// <c>paymentType</c>: Nakit, Kredi Kartı…); null for an open-account document or an ERP invoice.</summary>
    public string? PaymentType { get; set; }
}

/// <summary>Body of <c>POST /api/v1/portal/native/documents/{key}/void</c> (GOAL_PANEL_ERPSIZ E5c) —
/// the document-level sibling of <see cref="PortalLedgerVoidRequest"/> (D11): cancels a whole sale/
/// purchase/return, not a standalone payment/adjustment.</summary>
public sealed class PortalDocumentVoidRequest
{
    public string? Reason { get; set; }
    public string? OperationId { get; set; }
}

/// <summary>
/// Body of <c>POST /api/v1/portal/native/documents/{key}/edit</c> (GOAL_PANEL_ERPSIZ E5d, D11): the whole
/// corrected document — the same fields as <see cref="PortalNativeDocumentRequest"/> — plus why the original
/// is being corrected. The document keeps its kind; <see cref="PartyCode"/> defaults to the original's party
/// and <see cref="OccurredAt"/> to the original's date. <see cref="DocumentNo"/> left empty (or equal to the
/// original's) gives the correction a revision number (<c>A-1</c> → <c>A-1-D1</c>).
/// </summary>
public sealed class PortalDocumentEditRequest
{
    public string? PartyCode { get; set; }
    public List<PortalNativeDocumentLineRequest> Lines { get; set; } = [];
    public decimal? Amount { get; set; }
    public string? PaymentType { get; set; }

    /// <summary><c>yyyy-MM-dd</c>; defaults to the original document's date when absent.</summary>
    public string? OccurredAt { get; set; }
    public string? Description { get; set; }
    public string? DocumentNo { get; set; }

    /// <summary>Why the original is being corrected — mandatory, the same as a plain void's reason.</summary>
    public string? VoidReason { get; set; }
    public string? OperationId { get; set; }
}

/// <summary>One row of GET /api/v1/portal/native/documents (GOAL_PANEL_ERPSIZ E5b) — a sale/purchase/return
/// invoice, any customer/supplier.</summary>
public sealed class PortalDocumentRow
{
    public string Id { get; set; } = string.Empty;
    public string DocumentKey { get; set; } = string.Empty;

    /// <summary>sale, sale_return, purchase, purchase_return.</summary>
    public string Kind { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? DocumentNo { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }

    /// <summary>GOAL_PANEL_ERPSIZ E5e — cancelled by <c>document_void</c> (or replaced by <c>document_edit</c>).</summary>
    public bool Voided { get; set; }
}

/// <summary>GET /api/v1/portal/native/documents — company-wide sale/purchase/return invoices (GOAL_PANEL_ERPSIZ E5b).</summary>
public sealed class PortalDocumentsResponse
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public List<PortalDocumentRow> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

/// <summary>One row of GET /api/v1/portal/native/audit (GOAL_PANEL_ERPSIZ E7b/D5).</summary>
public sealed class PortalAuditRow
{
    public Guid Id { get; set; }
    public string Entity { get; set; } = string.Empty;
    public string EntityKey { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}

/// <summary>
/// GET /api/v1/portal/native/audit — one card/movement's "Geçmiş" when <c>entity</c>+<c>key</c> are given,
/// otherwise the company-wide /denetim list within the date range.
/// </summary>
public sealed class PortalAuditResponse
{
    public List<PortalAuditRow> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
