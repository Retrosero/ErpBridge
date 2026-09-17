namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// How an ERP company's phone documents are written into the ERP (goal GOAL_ERP_YAZIM, Y1a).
/// One row per tenant; when it is missing the defaults apply. The agent receives it with each
/// leased job (<c>erpContext</c>, Y1d), merged with the document creator's
/// <see cref="MobileUserErpMapping"/>: a user's value wins, an empty one falls back to this row.
/// Codes are Mikro codes (kasa, banka, temsilci…) and are validated against Mikro by the agent.
/// </summary>
public sealed class ErpWriteSettings
{
    public const int SeriesMaxLength = 6;
    public const int CodeMaxLength = 25;
    public const string DefaultChequePortfolioCode = "ÇEK";
    public const string DefaultNotePortfolioCode = "SENET";

    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    /// <summary>Which Mikro document a phone sale becomes (<see cref="SalesDocumentKinds"/>).</summary>
    public string SalesDocumentKind { get; set; } = SalesDocumentKinds.Order;

    /// <summary>Whether a sales order lands approved or waiting for approval (<see cref="OrderApprovalModes"/>).</summary>
    public string OrderApprovalMode { get; set; } = OrderApprovalModes.Approved;

    /// <summary>Series per document kind; empty is Mikro's series-less numbering.</summary>
    public string OrderSeries { get; set; } = string.Empty;
    public string DispatchSeries { get; set; } = string.Empty;
    public string InvoiceSeries { get; set; } = string.Empty;
    public string ReturnSeries { get; set; } = string.Empty;
    public string CollectionSeries { get; set; } = string.Empty;

    public int? DefaultWarehouseNo { get; set; }
    public string? DefaultCashCode { get; set; }
    public string? DefaultCardBankCode { get; set; }
    public string? DefaultTransferBankCode { get; set; }
    public int? DefaultErpUserNo { get; set; }
    public string? DefaultSalespersonCode { get; set; }
    public int? DefaultPriceListNo { get; set; }

    /// <summary>Kasa codes Mikro keeps customer cheques and notes in.</summary>
    public string ChequePortfolioCode { get; set; } = DefaultChequePortfolioCode;
    public string NotePortfolioCode { get; set; } = DefaultNotePortfolioCode;

    public string? ResponsibilityCenterCode { get; set; }
    public string? ProjectCode { get; set; }

    /// <summary>Order delivery date = document date + this many days (null: same day).</summary>
    public int? DeliveryDayOffset { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}

public static class SalesDocumentKinds
{
    public const string Order = "order";
    public const string Dispatch = "dispatch";
    public const string Invoice = "invoice";
    public static readonly IReadOnlyList<string> All = [Order, Dispatch, Invoice];
}

public static class OrderApprovalModes
{
    public const string Approved = "approved";
    public const string Pending = "pending";
    public static readonly IReadOnlyList<string> All = [Approved, Pending];
}

/// <summary>
/// A phone user's ERP counterparts (goal Y1a). Every value is optional: null means "use the
/// company's <see cref="ErpWriteSettings"/>". A separate table so <c>mobile_users</c> keeps its shape.
/// </summary>
public sealed class MobileUserErpMapping
{
    public Guid UserId { get; set; }
    public MobileUser? User { get; set; }
    public Guid TenantId { get; set; }

    public string? SalespersonCode { get; set; }
    public int? WarehouseNo { get; set; }
    public string? CashCode { get; set; }
    public string? CardBankCode { get; set; }
    public string? TransferBankCode { get; set; }
    public int? ErpUserNo { get; set; }

    public string? OrderSeries { get; set; }
    public string? DispatchSeries { get; set; }
    public string? InvoiceSeries { get; set; }
    public string? ReturnSeries { get; set; }
    public string? CollectionSeries { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}
