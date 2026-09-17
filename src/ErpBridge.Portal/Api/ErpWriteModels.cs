using System.Text.Json.Serialization;

namespace ErpBridge.Portal.Api;

// Mirrors of the central API's PortalErpWriteContracts (goal ERP yazım Y1b/Y1c). Kept local on purpose.

/// <summary>Series per document kind; on a user, null means the company's series.</summary>
public sealed class ErpSeriesDto
{
    [JsonPropertyName("order")] public string? Order { get; set; }
    [JsonPropertyName("dispatch")] public string? Dispatch { get; set; }
    [JsonPropertyName("invoice")] public string? Invoice { get; set; }
    [JsonPropertyName("return")] public string? Return { get; set; }
    [JsonPropertyName("collection")] public string? Collection { get; set; }
}

public sealed class ErpWriteSettingsDto
{
    [JsonPropertyName("salesDocumentKind")] public string SalesDocumentKind { get; set; } = "order";
    [JsonPropertyName("orderApprovalMode")] public string OrderApprovalMode { get; set; } = "approved";
    [JsonPropertyName("series")] public ErpSeriesDto Series { get; set; } = new();
    [JsonPropertyName("defaultWarehouseNo")] public int? DefaultWarehouseNo { get; set; }
    [JsonPropertyName("defaultCashCode")] public string? DefaultCashCode { get; set; }
    [JsonPropertyName("defaultCardBankCode")] public string? DefaultCardBankCode { get; set; }
    [JsonPropertyName("defaultTransferBankCode")] public string? DefaultTransferBankCode { get; set; }
    [JsonPropertyName("defaultErpUserNo")] public int? DefaultErpUserNo { get; set; }
    [JsonPropertyName("defaultSalespersonCode")] public string? DefaultSalespersonCode { get; set; }
    [JsonPropertyName("defaultPriceListNo")] public int? DefaultPriceListNo { get; set; }
    [JsonPropertyName("chequePortfolioCode")] public string ChequePortfolioCode { get; set; } = "ÇEK";
    [JsonPropertyName("notePortfolioCode")] public string NotePortfolioCode { get; set; } = "SENET";
    [JsonPropertyName("responsibilityCenterCode")] public string? ResponsibilityCenterCode { get; set; }
    [JsonPropertyName("projectCode")] public string? ProjectCode { get; set; }
    [JsonPropertyName("deliveryDayOffset")] public int? DeliveryDayOffset { get; set; }
    [JsonPropertyName("updatedAtUtc")] public DateTimeOffset? UpdatedAtUtc { get; set; }
}

public sealed class UserErpMappingDto
{
    [JsonPropertyName("userId")] public Guid UserId { get; set; }
    [JsonPropertyName("salespersonCode")] public string? SalespersonCode { get; set; }
    [JsonPropertyName("warehouseNo")] public int? WarehouseNo { get; set; }
    [JsonPropertyName("cashCode")] public string? CashCode { get; set; }
    [JsonPropertyName("cardBankCode")] public string? CardBankCode { get; set; }
    [JsonPropertyName("transferBankCode")] public string? TransferBankCode { get; set; }
    [JsonPropertyName("erpUserNo")] public int? ErpUserNo { get; set; }
    [JsonPropertyName("series")] public ErpSeriesDto Series { get; set; } = new();
    [JsonPropertyName("updatedAtUtc")] public DateTimeOffset? UpdatedAtUtc { get; set; }
}

public sealed class ErpLookupItem
{
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

public sealed class ErpLookupsResponse
{
    [JsonPropertyName("warehouses")] public List<ErpLookupItem> Warehouses { get; set; } = [];
    [JsonPropertyName("cashAccounts")] public List<ErpLookupItem> CashAccounts { get; set; } = [];
    [JsonPropertyName("banks")] public List<ErpLookupItem> Banks { get; set; } = [];
    [JsonPropertyName("salespersons")] public List<ErpLookupItem> Salespersons { get; set; } = [];
    [JsonPropertyName("priceLists")] public List<ErpLookupItem> PriceLists { get; set; } = [];
    [JsonPropertyName("projects")] public List<ErpLookupItem> Projects { get; set; } = [];
}
