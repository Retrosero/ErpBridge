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
    public string DocumentKey { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Kind { get; set; } = "other";
    public string? DocumentNo { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public List<PortalDocumentLine> Lines { get; set; } = [];
    public bool LinesAvailable { get; set; }
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
