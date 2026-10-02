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

/// <summary>
/// <c>GET /api/v1/storage/products/images?stockCode=</c>: one product's photos in order, and the server's copies of its
/// XML feed pictures (GOAL_DEPOLAMA_R2 S7) in the feed's order — the phone shows the photos, then these, then its own copy.
/// </summary>
public sealed class ProductImagesResponse
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("items")] public ProductImageDto[] Items { get; set; } = [];

    /// <summary>Read-only: the XML sync writes them. Empty in the manifest's groups (it lists them in its own <c>xmlItems</c>).</summary>
    [JsonPropertyName("xmlItems")] public XmlImageDto[] XmlItems { get; set; } = [];
}

/// <summary>
/// <c>GET /api/v1/storage/products/images/manifest</c>: every product with photos (the phone's picture order), and every
/// product with server copies of its XML pictures.
/// </summary>
public sealed class ProductImageManifestResponse
{
    [JsonPropertyName("items")] public ProductImagesResponse[] Items { get; set; } = [];

    [JsonPropertyName("xmlItems")] public XmlProductImagesDto[] XmlItems { get; set; } = [];
}

/// <summary>A picture the server copied from the company's XML feed (GOAL_DEPOLAMA_R2 S7): its two WebP sizes by their CDN addresses.</summary>
public sealed class XmlImageDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;

    /// <summary>Place in the feed's order (0 first).</summary>
    [JsonPropertyName("position")] public int Position { get; set; }

    /// <summary>The address in the feed it was copied from: the phone matches its own XML picture by it.</summary>
    [JsonPropertyName("sourceUrl")] public string SourceUrl { get; set; } = string.Empty;

    /// <summary>400 px WebP, <c>https://img.appsgo.cloud/{FIRMAKODU}/xml/…-s.webp</c>.</summary>
    [JsonPropertyName("thumbUrl")] public string ThumbUrl { get; set; } = string.Empty;

    /// <summary>1280 px WebP.</summary>
    [JsonPropertyName("fullUrl")] public string FullUrl { get; set; } = string.Empty;
    [JsonPropertyName("width")] public int Width { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }
    [JsonPropertyName("sizeBytes")] public long SizeBytes { get; set; }
    [JsonPropertyName("updatedAtMs")] public long UpdatedAtMs { get; set; }
}

/// <summary>One product's XML pictures in the manifest.</summary>
public sealed class XmlProductImagesDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("items")] public XmlImageDto[] Items { get; set; } = [];
}

/// <summary>
/// <c>GET /api/v1/storage/xml-images/status</c> (and the body of <c>POST …/sync</c>'s 202): the company's XML picture
/// sync for who manages storage.
/// </summary>
public sealed class XmlImageSyncStatusResponse
{
    /// <summary>A feed is saved (<c>tenant_xml_feed_settings</c>).</summary>
    [JsonPropertyName("configured")] public bool Configured { get; set; }

    /// <summary>The company has the XML product module.</summary>
    [JsonPropertyName("moduleEnabled")] public bool ModuleEnabled { get; set; }

    /// <summary>The feed's "download images" switch; off = the server copies nothing.</summary>
    [JsonPropertyName("downloadImages")] public bool DownloadImages { get; set; }

    /// <summary>False while the file store is not configured: the sync cannot store anything.</summary>
    [JsonPropertyName("storageAvailable")] public bool StorageAvailable { get; set; }

    /// <summary>A run is waiting since (unix ms); null = none.</summary>
    [JsonPropertyName("requestedAtMs")] public long? RequestedAtMs { get; set; }
    [JsonPropertyName("startedAtMs")] public long? StartedAtMs { get; set; }
    [JsonPropertyName("finishedAtMs")] public long? FinishedAtMs { get; set; }

    /// <summary><c>ok</c>, <c>partial</c>, <c>quota</c> or <c>failed</c>; null before the first run.</summary>
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("stats")] public Storage.XmlImageSyncStats? Stats { get; set; }

    /// <summary>Pictures held now and their two sizes' bytes together.</summary>
    [JsonPropertyName("imageCount")] public int ImageCount { get; set; }
    [JsonPropertyName("imageBytes")] public long ImageBytes { get; set; }
    [JsonPropertyName("productCount")] public int ProductCount { get; set; }
}

/// <summary><c>PUT /api/v1/storage/products/images/order?stockCode=</c>.</summary>
public sealed class ProductImageOrderRequest
{
    [JsonPropertyName("ids")] public Guid[]? Ids { get; set; }
}

// ---- trash and clean-up (GOAL_DEPOLAMA_R2 S9) ---------------------------------------------------------

/// <summary>One deletion in the storage trash.</summary>
public sealed class StorageTrashItemDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    /// <summary><c>product</c>, <c>xml</c>, <c>catalog</c>, <c>banner</c>, <c>task</c>, <c>expense</c>, <c>vehicle</c>.</summary>
    [JsonPropertyName("area")] public string Area { get; set; } = string.Empty;

    /// <summary><c>product_image</c>, <c>catalog_image</c>, <c>banner</c>, <c>banner_image</c>, <c>task_attachment</c>, <c>expense_attachment</c>, <c>files</c>.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;

    /// <summary>A stock code, a banner title, a task title or a document id.</summary>
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("sizeBytes")] public long SizeBytes { get; set; }
    [JsonPropertyName("fileCount")] public int FileCount { get; set; }

    /// <summary><c>user</c>, <c>cleanup</c>, <c>sweep</c> or <c>owner_deleted</c>.</summary>
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;
    [JsonPropertyName("trashedAtMs")] public long TrashedAtMs { get; set; }
    [JsonPropertyName("trashedByName")] public string? TrashedByName { get; set; }

    /// <summary>Whole days before it is deleted for good (0 = on the next daily pass).</summary>
    [JsonPropertyName("daysLeft")] public int DaysLeft { get; set; }

    /// <summary>False for files whose record is gone (the sweep, a long-deleted task): they can only be purged.</summary>
    [JsonPropertyName("restorable")] public bool Restorable { get; set; }

    /// <summary>The small picture: a CDN address, or a short presigned one for a private file; null when none.</summary>
    [JsonPropertyName("thumbUrl")] public string? ThumbUrl { get; set; }
}

/// <summary><c>GET /api/v1/storage/trash?page=</c>: newest first, 50 a page.</summary>
public sealed class StorageTrashResponse
{
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("totalBytes")] public long TotalBytes { get; set; }

    /// <summary>Days an item stays before it is deleted for good (<c>Storage:TrashDays</c>).</summary>
    [JsonPropertyName("trashDays")] public int TrashDays { get; set; }
    [JsonPropertyName("items")] public StorageTrashItemDto[] Items { get; set; } = [];
}

/// <summary><c>POST /api/v1/storage/trash/restore</c> and <c>…/purge</c>: the items, or (purge only) <c>all: true</c>.</summary>
public sealed class StorageTrashRequest
{
    [JsonPropertyName("ids")] public Guid[]? Ids { get; set; }
    [JsonPropertyName("all")] public bool All { get; set; }
}

/// <summary>One item's restore.</summary>
public sealed class StorageTrashRestoreItemDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("restored")] public bool Restored { get; set; }

    /// <summary>Why it stayed in the trash (Turkish), e.g. the product's photo limit is full.</summary>
    [JsonPropertyName("reason")] public string? Reason { get; set; }
}

/// <summary><c>POST /api/v1/storage/trash/restore</c>: every item's outcome; a failed one stays in the trash.</summary>
public sealed class StorageTrashRestoreResponse
{
    [JsonPropertyName("restored")] public int Restored { get; set; }
    [JsonPropertyName("restoredBytes")] public long RestoredBytes { get; set; }
    [JsonPropertyName("failed")] public int Failed { get; set; }
    [JsonPropertyName("items")] public StorageTrashRestoreItemDto[] Items { get; set; } = [];
}

/// <summary><c>POST /api/v1/storage/trash/purge</c>: deleted for good, the quota freed now.</summary>
public sealed class StorageTrashPurgeResponse
{
    [JsonPropertyName("purged")] public int Purged { get; set; }
    [JsonPropertyName("purgedBytes")] public long PurgedBytes { get; set; }

    /// <summary>Items the store did not answer for; they stay and the daily pass tries again.</summary>
    [JsonPropertyName("failed")] public int Failed { get; set; }
}

/// <summary>A clean-up group's size.</summary>
public sealed class StorageCleanupGroupDto
{
    /// <summary><c>missing_products</c>, <c>out_of_stock</c>, <c>closed_tasks</c>, <c>ended_banners</c>, <c>xml_unused</c>.</summary>
    [JsonPropertyName("group")] public string Group { get; set; } = string.Empty;
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("bytes")] public long Bytes { get; set; }

    /// <summary>True for <c>xml_unused</c>: deleted for good, not trashed (the feed makes them again).</summary>
    [JsonPropertyName("purgesDirectly")] public bool PurgesDirectly { get; set; }
}

/// <summary><c>GET /api/v1/storage/cleanup/summary?days=</c>.</summary>
public sealed class StorageCleanupSummaryResponse
{
    /// <summary>The age of a closed task whose pictures count (default 90).</summary>
    [JsonPropertyName("days")] public int Days { get; set; }
    [JsonPropertyName("groups")] public StorageCleanupGroupDto[] Groups { get; set; } = [];
}

/// <summary>A record whose files the clean-up can free.</summary>
public sealed class StorageCleanupCandidateDto
{
    /// <summary>The owner's id (photo, picture, banner, task picture, XML picture): what <c>POST cleanup</c> takes.</summary>
    [JsonPropertyName("id")] public Guid Id { get; set; }

    /// <summary><c>product_image</c>, <c>catalog_image</c>, <c>xml_image</c>, <c>task_attachment</c> or <c>banner</c>.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("area")] public string Area { get; set; } = string.Empty;
    [JsonPropertyName("sizeBytes")] public long SizeBytes { get; set; }
    [JsonPropertyName("thumbUrl")] public string? ThumbUrl { get; set; }

    /// <summary>A short Turkish note: what it is, when the task closed, why the XML pictures are unused.</summary>
    [JsonPropertyName("extra")] public string? Extra { get; set; }
}

/// <summary><c>GET /api/v1/storage/cleanup/candidates?group=&amp;page=&amp;days=</c>: biggest first, 50 a page.</summary>
public sealed class StorageCleanupCandidatesResponse
{
    [JsonPropertyName("group")] public string Group { get; set; } = string.Empty;
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("totalBytes")] public long TotalBytes { get; set; }
    [JsonPropertyName("items")] public StorageCleanupCandidateDto[] Items { get; set; } = [];
}

/// <summary><c>POST /api/v1/storage/cleanup</c>: the chosen candidates of a group, or all of it.</summary>
public sealed class StorageCleanupRequest
{
    [JsonPropertyName("group")] public string? Group { get; set; }
    [JsonPropertyName("ids")] public Guid[]? Ids { get; set; }
    [JsonPropertyName("all")] public bool All { get; set; }

    /// <summary><c>closed_tasks</c>: the same age the list was read with (default 90).</summary>
    [JsonPropertyName("days")] public int? Days { get; set; }
}

/// <summary><c>POST /api/v1/storage/cleanup</c>'s answer.</summary>
public sealed class StorageCleanupResponse
{
    [JsonPropertyName("group")] public string Group { get; set; } = string.Empty;

    /// <summary>Records moved to the trash (restorable for the trash period; their bytes count until the trash is emptied).</summary>
    [JsonPropertyName("trashedCount")] public int TrashedCount { get; set; }
    [JsonPropertyName("trashedBytes")] public long TrashedBytes { get; set; }

    /// <summary>XML pictures deleted for good (the feed downloads them again when wanted); their bytes are free now.</summary>
    [JsonPropertyName("purgedCount")] public int PurgedCount { get; set; }
    [JsonPropertyName("purgedBytes")] public long PurgedBytes { get; set; }

    /// <summary>Candidates left past one request's limit: send again.</summary>
    [JsonPropertyName("remaining")] public int Remaining { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
}
