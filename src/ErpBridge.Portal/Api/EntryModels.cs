using System.Text.Json;

namespace ErpBridge.Portal.Api;

// GOAL_PANEL_GIRIS: the panel's document entry (`/api/v1/portal/entry`). Mirrors of the central API's
// PortalEntryContracts, kept local on purpose (the panel does not reference the server's assembly).

public sealed class EntryContextDto
{
    public string DataSource { get; set; } = string.Empty;
    public string Today { get; set; } = string.Empty;
    public List<string> Kinds { get; set; } = [];
    public bool CanSellOnAccount { get; set; }
    public bool CanSellBelowStock { get; set; }
    public List<EntryOwnerDto> Owners { get; set; } = [];
    public List<EntryPriceListDto> PriceLists { get; set; } = [];
    public List<EntryLookupDto> Warehouses { get; set; } = [];
    public List<EntryLookupDto> CashAccounts { get; set; } = [];
    public List<EntryLookupDto> Banks { get; set; } = [];
    public List<EntryLookupDto> ExpenseCards { get; set; } = [];
    public List<EntryLookupDto> VatRates { get; set; } = [];
    public List<string> ExpenseCategories { get; set; } = [];

    /// <summary>ERP company: a purchase's typed price includes VAT (the company's supplier price setting).</summary>
    public bool PurchasePricesIncludeVat { get; set; }

    public bool IsErp => string.Equals(DataSource, "erp", StringComparison.OrdinalIgnoreCase);
}

public sealed class EntryOwnerDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsSelf { get; set; }
}

public sealed class EntryPriceListDto
{
    public int No { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IncludesVat { get; set; }
}

public sealed class EntryLookupDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? Rate { get; set; }
}

public sealed class EntryCustomersResponse
{
    public List<EntryCustomerDto> Items { get; set; } = [];
    public int Total { get; set; }
}

public sealed class EntryCustomerDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Phone { get; set; }
    public decimal? Balance { get; set; }
}

public sealed class EntryProductsResponse
{
    public List<EntryProductDto> Items { get; set; } = [];
    public int Total { get; set; }
}

public sealed class EntryProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? Barcode { get; set; }
    public decimal VatRate { get; set; }
    public Dictionary<int, decimal> Prices { get; set; } = [];
    public int? DefaultPriceListNo { get; set; }
    public decimal Stock { get; set; }
}

public sealed class EntryReturnablesResponse
{
    public List<EntryReturnableDto> Items { get; set; } = [];
}

public sealed class EntryReturnableDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? Barcode { get; set; }
    public decimal VatRate { get; set; }
    public List<EntrySoldPriceDto> Prices { get; set; } = [];
}

public sealed class EntrySoldPriceDto
{
    public decimal UnitPrice { get; set; }
    public string LastSold { get; set; } = string.Empty;
}

/// <summary>The fields every entry sends: the save's key, whose document it is, its date and the total the user saw.</summary>
public abstract class EntryRequestBase
{
    public string? OperationId { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? Date { get; set; }
    public decimal? ExpectedTotal { get; set; }
}

public sealed class EntrySaleRequest : EntryRequestBase
{
    public string CustomerCode { get; set; } = string.Empty;
    public int? PriceListNo { get; set; }
    public List<EntrySaleLineRequest> Lines { get; set; } = [];
    public decimal GeneralDiscountPercent { get; set; }
    public string? PaymentType { get; set; }
    public string? BankCode { get; set; }
    public string? Note { get; set; }
}

public sealed class EntrySaleLineRequest
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public string? Note { get; set; }
}

public sealed class EntryCollectionRequest : EntryRequestBase
{
    public string CustomerCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<EntryPaymentRequest> Payments { get; set; } = [];
}

public sealed class EntryPaymentRequest
{
    public string Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? BankCode { get; set; }
    public string? BankName { get; set; }
    public int? Installments { get; set; }
    public decimal? SurchargeAmount { get; set; }
    public string? Reference { get; set; }
    public string? DocumentNo { get; set; }
    public string? DueDate { get; set; }
}

public sealed class EntryDisbursementRequest : EntryRequestBase
{
    public string CustomerCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? PaymentType { get; set; }
    public string? BankCode { get; set; }
    public string? BankName { get; set; }
    public string? Description { get; set; }
}

public sealed class EntryExpenseRequest : EntryRequestBase
{
    public decimal Amount { get; set; }
    public string? ExpenseCardCode { get; set; }
    public string? Category { get; set; }
    public decimal VatAmount { get; set; }
    public int? VatPointer { get; set; }
    public string? PaymentType { get; set; }
    public string? AccountCode { get; set; }
    public string? Description { get; set; }
}

public sealed class EntryPurchaseRequest : EntryRequestBase
{
    public string SupplierCode { get; set; } = string.Empty;
    public string? Series { get; set; }
    public string? SequenceNo { get; set; }
    public List<EntryPurchaseLineRequest> Lines { get; set; } = [];
    public List<decimal> GeneralDiscountPercents { get; set; } = [];
}

public sealed class EntryPurchaseLineRequest
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public List<decimal> LineDiscountPercents { get; set; } = [];
}

public sealed class EntryReturnRequest : EntryRequestBase
{
    public string CustomerCode { get; set; } = string.Empty;
    public List<EntryReturnLineRequest> Lines { get; set; } = [];
    public string? SettlementMethod { get; set; }
    public string? BankCode { get; set; }
    public string? BankName { get; set; }
}

public sealed class EntryReturnLineRequest
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ConditionPercent { get; set; } = 1m;
    public string? Reason { get; set; }
}

public sealed class EntryPreviewDto
{
    public string Kind { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string OccurredAt { get; set; } = string.Empty;
    public int? PriceListNo { get; set; }
    public string? PriceListName { get; set; }
    public bool PriceIncludesVat { get; set; }
    public List<EntryPricedLineDto> Lines { get; set; } = [];
    public decimal Gross { get; set; }
    public decimal Discount { get; set; }
    public decimal Vat { get; set; }
    public decimal Total { get; set; }
    public EntryRefusalDto? Refusal { get; set; }
    public List<EntryStockWarningDto> StockWarnings { get; set; } = [];
    public List<EntryPaymentDto> Payments { get; set; } = [];
}

public sealed class EntryPricedLineDto
{
    public string ProductCode { get; set; } = string.Empty;
    public List<decimal> LineDiscountPercents { get; set; } = [];
    public decimal? ConditionPercent { get; set; }
    public string? Reason { get; set; }
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

public sealed class EntryRefusalDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Key { get; set; }
}

public sealed class EntryStockWarningDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Requested { get; set; }
    public decimal Available { get; set; }
}

public sealed class EntryPaymentDto
{
    public string Method { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Detail { get; set; }
}

public sealed class EntryCreateResponse
{
    public List<EntryDocumentDto> Documents { get; set; } = [];
    public bool Idempotent { get; set; }
    public EntryPreviewDto? Preview { get; set; }
}

public sealed class EntryDocumentDto
{
    public Guid JobId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public sealed class EntryDocumentDetailDto
{
    public Guid JobId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? ErpDocumentNo { get; set; }
    public string? Message { get; set; }
    public string? CustomerName { get; set; }
    public decimal? Amount { get; set; }
    public string? OwnerName { get; set; }
    public string? EnteredBy { get; set; }
    public DateTimeOffset EnteredAtUtc { get; set; }
    public JsonElement Payload { get; set; }
}

public sealed class EntryDocumentsResponse
{
    public List<EntryDocumentSummaryDto> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public sealed class EntryDocumentSummaryDto
{
    public Guid JobId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? ErpDocumentNo { get; set; }
    public string? CustomerName { get; set; }
    public decimal? Amount { get; set; }
    public string? OwnerName { get; set; }
    public string? EnteredBy { get; set; }
    public DateTimeOffset EnteredAtUtc { get; set; }
}

/// <summary>The panel's words for the entry kinds, their pages and their slips.</summary>
public static class EntryKinds
{
    public const string Sale = "sale";
    public const string Collection = "collection";
    public const string Purchase = "purchase";
    public const string Return = "return";
    public const string Disbursement = "disbursement";
    public const string Expense = "expense";

    public static string Label(string kind) => kind switch
    {
        Sale => "Satış",
        Collection => "Tahsilat",
        Purchase => "Alış",
        Return => "İade",
        Disbursement => "Tediye",
        Expense => "Gider",
        _ => kind,
    };

    public static string Page(string kind) => kind switch
    {
        Sale => "giris/satis",
        Collection => "giris/tahsilat",
        Purchase => "giris/alis",
        Return => "giris/iade",
        Disbursement => "giris/tediye",
        Expense => "giris/gider",
        _ => "girisler",
    };

    /// <summary>The state of a saved entry in words.</summary>
    public static string State(string state, bool erp) => state switch
    {
        "written" => erp ? "ERP'ye yazıldı" : "İşlendi",
        "retrying" => "Yeniden denenecek",
        "failed" => "Hata",
        _ => erp ? "ERP'ye yazılmayı bekliyor" : "Bekliyor",
    };
}
