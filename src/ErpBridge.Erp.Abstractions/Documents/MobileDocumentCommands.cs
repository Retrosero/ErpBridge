namespace ErpBridge.Erp.Abstractions.Documents;

// ERP-independent commands a phone document becomes before an adapter writes it
// (goal GOAL_ERP_YAZIM, Y2a). The agent's translator fills them from the phone body plus the
// company's ERP settings; amounts are the phone's, codes are the ERP's. Discounts, VAT and
// every ERP column are the adapter's business.

/// <summary>
/// What every written document shares.
/// <list type="bullet">
/// <item><description><c>ExternalId</c>: The phone's document id; the idempotency key together with the document type.</description></item>
/// <item><description><c>OccurredAt</c>: Document date as the phone recorded it (local time).</description></item>
/// <item><description><c>CustomerCode</c>: ERP customer code; required.</description></item>
/// <item><description><c>ErpUserNo</c>: ERP user number stamped as creator.</description></item>
/// <item><description><c>Series</c>: Document series; empty is the ERP's series-less numbering.</description></item>
/// <item><description><c>ExpectedTotal</c>: The total the phone showed (VAT included); the adapter refuses to write when its own total differs.</description></item>
/// <item><description><c>ResponsibilityCenterCode</c>, <c>ProjectCode</c>: The company's ERP responsibility center and project stamped on the document; null when the company uses none.</description></item>
/// </list>
/// </summary>
public sealed record ErpDocumentHeader(
    string ExternalId,
    DateTime OccurredAt,
    string CustomerCode,
    string? SalespersonCode,
    int ErpUserNo,
    string Series,
    string? Description,
    decimal ExpectedTotal,
    string? ResponsibilityCenterCode = null,
    string? ProjectCode = null);

public enum SalesDocumentKind
{
    Order,
    Dispatch,
    Invoice,
}

public enum OrderApprovalMode
{
    Approved,
    Pending,
}

/// <summary>How a sale is paid: on account, or closed to a cash box / bank at once.</summary>
public enum SalesSettlement
{
    Open,
    Cash,
    Card,
    Transfer,
}

/// <summary>
/// <list type="bullet">
/// <item><description><c>ListUnitPrice</c>: Price-list price of one unit, before discounts.</description></item>
/// <item><description><c>LineDiscountPercent</c>: Applied to the gross amount.</description></item>
/// <item><description><c>CustomerDiscountPercent</c>: Applied to what the line discount left.</description></item>
/// <item><description><c>GeneralDiscountPercent</c>: Applied to what the customer discount left.</description></item>
/// </list>
/// </summary>
public sealed record SalesDocumentLine(
    string StockCode,
    decimal Quantity,
    int UnitPointer,
    decimal ListUnitPrice,
    decimal LineDiscountPercent,
    decimal CustomerDiscountPercent,
    decimal GeneralDiscountPercent,
    string? Note = null);

/// <summary>
/// <list type="bullet">
/// <item><description><c>SettlementAccountCode</c>: Cash-box code for <see cref="SalesSettlement.Cash"/>, bank code for card and transfer; null when open.</description></item>
/// <item><description><c>ExtraPayments</c>: Payments taken with an open sale that are not a single closing settlement (a part payment, mixed methods). Written as a collection receipt in the same transaction, with <c>ExtraPaymentsSeries</c>.</description></item>
/// <item><description><c>DeliveryDate</c>: When an order or dispatch note is to be delivered; null means the document day.</description></item>
/// </list>
/// </summary>
public sealed record SalesDocumentCommand(
    ErpDocumentHeader Header,
    SalesDocumentKind Kind,
    int WarehouseNo,
    int PriceListNo,
    OrderApprovalMode ApprovalMode,
    SalesSettlement Settlement,
    string? SettlementAccountCode,
    IReadOnlyList<SalesDocumentLine> Lines,
    IReadOnlyList<CollectionPayment>? ExtraPayments = null,
    string? ExtraPaymentsSeries = null,
    DateTime? DeliveryDate = null);

public enum ReturnSettlement
{
    Open,
    Cash,
    Bank,
}

/// <summary>
/// <list type="bullet">
/// <item><description><c>ConditionRatio</c>: Share of the price refunded for this line, 0..1 (1 = undamaged).</description></item>
/// </list>
/// </summary>
public sealed record SalesReturnLine(
    string StockCode,
    decimal Quantity,
    int UnitPointer,
    decimal ListUnitPrice,
    decimal ConditionRatio,
    string? Reason = null);

public sealed record SalesReturnCommand(
    ErpDocumentHeader Header,
    int WarehouseNo,
    int PriceListNo,
    ReturnSettlement Settlement,
    string? SettlementAccountCode,
    IReadOnlyList<SalesReturnLine> Lines);

public enum CollectionMethod
{
    Cash,
    Card,
    Transfer,
    Cheque,
    Note,
}

public sealed record ChequeDetails(string No, string? BankName, string? Branch, string? AccountNo, string? Drawer);

public sealed record NoteDetails(string No, string? Debtor);

/// <summary>
/// <list type="bullet">
/// <item><description><c>AccountCode</c>: Cash-box code (cash), bank code (card, transfer) or portfolio cash-box code (cheque, note).</description></item>
/// <item><description><c>DueDate</c>: Due date; the document date for cash, card and transfer.</description></item>
/// <item><description><c>SurchargeAmount</c>: Card instalment surcharge; noted in the description, not posted.</description></item>
/// </list>
/// </summary>
public sealed record CollectionPayment(
    CollectionMethod Method,
    decimal Amount,
    DateTime DueDate,
    string AccountCode,
    int? Installments = null,
    decimal? SurchargeAmount = null,
    ChequeDetails? Cheque = null,
    NoteDetails? Note = null);

/// <summary>One receipt; each payment becomes one line of it.</summary>
public sealed record CollectionCommand(ErpDocumentHeader Header, IReadOnlyList<CollectionPayment> Payments);
