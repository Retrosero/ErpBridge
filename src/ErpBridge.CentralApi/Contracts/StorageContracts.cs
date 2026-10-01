using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

/// <summary>One area's share of the company storage (active files only).</summary>
public sealed class StorageAreaUsageDto
{
    [JsonPropertyName("area")] public string Area { get; set; } = string.Empty;
    [JsonPropertyName("usedBytes")] public long UsedBytes { get; set; }
    [JsonPropertyName("fileCount")] public int FileCount { get; set; }
}

/// <summary>
/// <c>GET /api/v1/storage/usage</c> (GOAL_DEPOLAMA_R2 S8). Everyone sees the total, the quota and how much of the total is
/// in the trash; the breakdown by area and the trash's file count only who manages storage (<c>action.storage.manage</c>)
/// — omitted otherwise. <see cref="UsedBytes"/> includes the trash: a file counts until it is purged (T8).
/// </summary>
public sealed class StorageUsageResponse
{
    /// <summary>False while the R2 settings are missing: uploads answer 503.</summary>
    [JsonPropertyName("available")] public bool Available { get; set; }
    [JsonPropertyName("usedBytes")] public long UsedBytes { get; set; }
    [JsonPropertyName("quotaBytes")] public long QuotaBytes { get; set; }
    [JsonPropertyName("freeBytes")] public long FreeBytes { get; set; }

    [JsonPropertyName("areas"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StorageAreaUsageDto[]? Areas { get; set; }

    /// <summary>Bytes in the trash: part of <see cref="UsedBytes"/> until purged; emptying the trash frees them (T8).</summary>
    [JsonPropertyName("trashedBytes")] public long TrashedBytes { get; set; }

    [JsonPropertyName("trashedCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TrashedCount { get; set; }
}

/// <summary><c>GET/PUT /api/v1/admin/tenants/{id}/storage</c>: the operator's view of a company's storage.</summary>
public sealed class AdminTenantStorageResponse
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("available")] public bool Available { get; set; }
    [JsonPropertyName("usedBytes")] public long UsedBytes { get; set; }
    [JsonPropertyName("reservedBytes")] public long ReservedBytes { get; set; }

    /// <summary>The quota in force: the company's own, else the default.</summary>
    [JsonPropertyName("quotaBytes")] public long QuotaBytes { get; set; }
    [JsonPropertyName("defaultQuotaBytes")] public long DefaultQuotaBytes { get; set; }

    /// <summary>The company's own quota; null = the default applies.</summary>
    [JsonPropertyName("customQuotaBytes")] public long? CustomQuotaBytes { get; set; }
    [JsonPropertyName("recountedAtMs")] public long? RecountedAtMs { get; set; }
    [JsonPropertyName("areas")] public StorageAreaUsageDto[] Areas { get; set; } = [];
    [JsonPropertyName("trashedBytes")] public long TrashedBytes { get; set; }
    [JsonPropertyName("trashedCount")] public int TrashedCount { get; set; }
}

/// <summary><c>PUT /api/v1/admin/tenants/{id}/storage</c>: null puts the company back on the default quota.</summary>
public sealed class SetTenantStorageRequest
{
    [JsonPropertyName("quotaBytes")] public long? QuotaBytes { get; set; }
}

/// <summary><c>POST /api/v1/admin/tenants/{id}/storage/recount</c>.</summary>
public sealed class AdminStorageRecountResponse
{
    [JsonPropertyName("usedBytesBefore")] public long UsedBytesBefore { get; set; }
    [JsonPropertyName("usedBytesAfter")] public long UsedBytesAfter { get; set; }
    [JsonPropertyName("storage")] public AdminTenantStorageResponse Storage { get; set; } = new();
}

/// <summary><c>GET /api/v1/storage/files/{id}/link</c>: the redirect's address as data, for a page that shows the file itself.</summary>
public sealed class StoredFileLinkResponse
{
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;

    /// <summary>When a presigned address stops working (unix ms); null for a public CDN address.</summary>
    [JsonPropertyName("expiresAtMs")] public long? ExpiresAtMs { get; set; }
}

/// <summary>A receipt photo of a phone expense or vehicle maintenance document (GOAL_DEPOLAMA_R2 S5).</summary>
public sealed class ExpenseAttachmentDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    /// <summary>The phone's document id the receipt belongs to (the cash-log id, <c>mobileDocumentId</c>).</summary>
    [JsonPropertyName("documentId")] public string DocumentId { get; set; } = string.Empty;

    /// <summary><c>expense</c> or <c>vehicle_maintenance</c>.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("contentType")] public string ContentType { get; set; } = string.Empty;
    [JsonPropertyName("sizeBytes")] public int SizeBytes { get; set; }
    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }
    [JsonPropertyName("createdByUserId")] public Guid CreatedByUserId { get; set; }
    [JsonPropertyName("createdByName")] public string CreatedByName { get; set; } = string.Empty;
}

/// <summary><c>GET /api/v1/android/expenses/{docId}/attachments</c>: the document's receipts the user may see.</summary>
public sealed class ExpenseAttachmentListResponse
{
    [JsonPropertyName("items")] public ExpenseAttachmentDto[] Items { get; set; } = [];
}

/// <summary>The receipt's document as the server holds it (absent while the document has not reached the server).</summary>
public sealed class ExpenseDocumentDto
{
    /// <summary>The job's document type: <c>expense</c>, or <c>disbursement</c> from an older phone.</summary>
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("amount")] public decimal? Amount { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("counterparty")] public string? Counterparty { get; set; }

    /// <summary>As the phone wrote it (<c>dd.MM.yyyy HH:mm</c>).</summary>
    [JsonPropertyName("occurredAt")] public string? OccurredAt { get; set; }
    [JsonPropertyName("expenseCardCode")] public string? ExpenseCardCode { get; set; }
}

/// <summary>One receipt on the panel: the photo's short-lived address and its document.</summary>
public sealed class PortalExpenseReceiptDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    /// <summary>The stored file: a fresh address comes from <c>GET /api/v1/storage/files/{fileId}/link</c>.</summary>
    [JsonPropertyName("fileId")] public Guid FileId { get; set; }
    [JsonPropertyName("documentId")] public string DocumentId { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("contentType")] public string ContentType { get; set; } = string.Empty;
    [JsonPropertyName("sizeBytes")] public int SizeBytes { get; set; }
    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }
    [JsonPropertyName("createdByName")] public string CreatedByName { get; set; } = string.Empty;

    /// <summary>A presigned address valid for <c>Storage:PresignMinutes</c>; null while the store is not configured.</summary>
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("document")] public ExpenseDocumentDto? Document { get; set; }
}

/// <summary><c>GET /api/v1/portal/expense-receipts</c>.</summary>
public sealed class PortalExpenseReceiptsResponse
{
    [JsonPropertyName("from")] public string From { get; set; } = string.Empty;
    [JsonPropertyName("to")] public string To { get; set; } = string.Empty;
    [JsonPropertyName("items")] public PortalExpenseReceiptDto[] Items { get; set; } = [];

    /// <summary>True when the list was cut at the endpoint's limit: narrow the dates.</summary>
    [JsonPropertyName("truncated")] public bool Truncated { get; set; }
}

/// <summary>A product photo (GOAL_DEPOLAMA_R2 S6): its two WebP sizes by their CDN addresses.</summary>
public sealed class ProductImageDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("sortOrder")] public int SortOrder { get; set; }

    /// <summary>400 px WebP, <c>https://img.appsgo.cloud/{FIRMAKODU}/product/…-s.webp</c>.</summary>
    [JsonPropertyName("thumbUrl")] public string ThumbUrl { get; set; } = string.Empty;

    /// <summary>1280 px WebP.</summary>
    [JsonPropertyName("fullUrl")] public string FullUrl { get; set; } = string.Empty;
    [JsonPropertyName("width")] public int Width { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }
    [JsonPropertyName("sizeBytes")] public long SizeBytes { get; set; }
    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }
    [JsonPropertyName("createdByName")] public string CreatedByName { get; set; } = string.Empty;
}

/// <summary><c>GET /api/v1/storage/products/images?stockCode=</c>: one product's photos in order.</summary>
public sealed class ProductImagesResponse
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("items")] public ProductImageDto[] Items { get; set; } = [];
}

/// <summary><c>GET /api/v1/storage/products/images/manifest</c>: every product with photos (the phone's picture order).</summary>
public sealed class ProductImageManifestResponse
{
    [JsonPropertyName("items")] public ProductImagesResponse[] Items { get; set; } = [];
}

/// <summary><c>PUT /api/v1/storage/products/images/order?stockCode=</c>.</summary>
public sealed class ProductImageOrderRequest
{
    [JsonPropertyName("ids")] public Guid[]? Ids { get; set; }
}
