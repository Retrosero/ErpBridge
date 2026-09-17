namespace ErpBridge.CentralApi.Contracts;

/// <summary>Series per document kind. On the company, null and empty both mean the ERP's series-less numbering.</summary>
public sealed class PortalErpSeriesDto
{
    public string? Order { get; set; }
    public string? Dispatch { get; set; }
    public string? Invoice { get; set; }
    public string? Return { get; set; }
    public string? Collection { get; set; }
}

/// <summary>GET/PUT /api/v1/portal/erp-settings — how the company's phone documents are written into its ERP (goal ERP yazım Y1b).</summary>
public sealed class PortalErpWriteSettingsDto
{
    /// <summary><c>order</c>, <c>dispatch</c> or <c>invoice</c>.</summary>
    public string SalesDocumentKind { get; set; } = Domain.SalesDocumentKinds.Order;

    /// <summary><c>approved</c> or <c>pending</c>.</summary>
    public string OrderApprovalMode { get; set; } = Domain.OrderApprovalModes.Approved;

    public PortalErpSeriesDto Series { get; set; } = new();
    public int? DefaultWarehouseNo { get; set; }
    public string? DefaultCashCode { get; set; }
    public string? DefaultCardBankCode { get; set; }
    public string? DefaultTransferBankCode { get; set; }
    public int? DefaultErpUserNo { get; set; }
    public string? DefaultSalespersonCode { get; set; }
    public int? DefaultPriceListNo { get; set; }
    public string ChequePortfolioCode { get; set; } = Domain.ErpWriteSettings.DefaultChequePortfolioCode;
    public string NotePortfolioCode { get; set; } = Domain.ErpWriteSettings.DefaultNotePortfolioCode;
    public string? ResponsibilityCenterCode { get; set; }
    public string? ProjectCode { get; set; }
    public int? DeliveryDayOffset { get; set; }

    /// <summary>Read only; null until the company saves its settings.</summary>
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}

/// <summary>
/// GET/PUT /api/v1/portal/users/{id}/erp-mapping — a user's ERP counterparts. Null means "use the
/// company's value"; an empty series means series-less for this user.
/// </summary>
public sealed class PortalUserErpMappingDto
{
    /// <summary>Read only.</summary>
    public Guid UserId { get; set; }

    public string? SalespersonCode { get; set; }
    public int? WarehouseNo { get; set; }
    public string? CashCode { get; set; }
    public string? CardBankCode { get; set; }
    public string? TransferBankCode { get; set; }
    public int? ErpUserNo { get; set; }
    public PortalErpSeriesDto Series { get; set; } = new();

    /// <summary>Read only.</summary>
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}

/// <summary>An ERP code the settings page can offer.</summary>
public sealed class PortalErpLookupItem
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// GET /api/v1/portal/erp-lookups — codes from the ERP data the agent sent to the server. A list is
/// empty when the ERP did not send it; the page then takes free text.
/// </summary>
public sealed class PortalErpLookupsResponse
{
    public IReadOnlyList<PortalErpLookupItem> Warehouses { get; set; } = [];
    public IReadOnlyList<PortalErpLookupItem> CashAccounts { get; set; } = [];
    public IReadOnlyList<PortalErpLookupItem> Banks { get; set; } = [];
    public IReadOnlyList<PortalErpLookupItem> Salespersons { get; set; } = [];
    public IReadOnlyList<PortalErpLookupItem> PriceLists { get; set; } = [];
    public IReadOnlyList<PortalErpLookupItem> Projects { get; set; } = [];
}

/// <summary>
/// GET /api/v1/portal/erp-documents — the documents sent to the ERP and what the agent did with them (goal ERP
/// yazım Y5a), newest first.
/// </summary>
public sealed class PortalErpDocumentsResponse
{
    public IReadOnlyList<PortalErpDocumentDto> Items { get; set; } = [];

    /// <summary>Documents matching the filters, across pages.</summary>
    public int Total { get; set; }

    public int Page { get; set; }
    public int PageSize { get; set; }
}

/// <summary>One document on its way to the ERP.</summary>
public sealed class PortalErpDocumentDto
{
    public Guid JobId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public Guid? UserId { get; set; }

    /// <summary>The sender's name; null for an API key or a removed user.</summary>
    public string? UserName { get; set; }

    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public decimal? Amount { get; set; }

    /// <summary><c>pending</c>, <c>retrying</c>, <c>written</c> or <c>failed</c>.</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>The ERP document number (<c>T-1234</c>) once written.</summary>
    public string? ErpDocumentNo { get; set; }

    public string? ErrorCode { get; set; }

    /// <summary>The agent's reason in Turkish.</summary>
    public string? Message { get; set; }

    public int Attempt { get; set; }
    public DateTimeOffset EnqueuedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public DateTimeOffset? NextAttemptAtUtc { get; set; }

    /// <summary>A failed document an administrator may send to the agent again.</summary>
    public bool CanRetry { get; set; }
}

/// <summary>POST /api/v1/portal/erp-documents/{jobId}/retry — the document is waiting for the agent again.</summary>
public sealed class PortalErpRetryResponse
{
    public Guid JobId { get; set; }
    public string State { get; set; } = string.Empty;
}
