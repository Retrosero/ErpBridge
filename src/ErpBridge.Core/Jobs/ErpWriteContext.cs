namespace ErpBridge.Core.Jobs;

/// <summary>
/// The company's ERP write settings merged with the document creator's ERP counterparts, as the
/// central API sends them with each leased job (<c>erpContext</c>, goal GOAL_ERP_YAZIM Y1d). A
/// user's value already replaced the company default on the server; null means neither is set.
/// JSON names are camelCase (<c>JsonSerializerDefaults.Web</c>).
/// </summary>
public sealed record ErpWriteContext(
    string SalesDocumentKind,
    string OrderApprovalMode,
    ErpWriteSeries Series,
    int? WarehouseNo,
    string? CashCode,
    string? CardBankCode,
    string? TransferBankCode,
    int? ErpUserNo,
    string? SalespersonCode,
    int? PriceListNo,
    string ChequePortfolioCode,
    string NotePortfolioCode,
    string? CreatedByUsername = null);

/// <summary>Series per document kind; an empty series is the ERP's series-less numbering.</summary>
public sealed record ErpWriteSeries(string Order, string Dispatch, string Invoice, string Return, string Collection);
