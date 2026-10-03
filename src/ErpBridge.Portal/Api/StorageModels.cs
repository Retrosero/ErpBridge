using System.Text.Json.Serialization;

namespace ErpBridge.Portal.Api;

/// <summary><c>GET /api/v1/storage/files/{id}/link</c>: where a stored file loads now (a presigned address expires).</summary>
public sealed class StoredFileLink
{
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
    [JsonPropertyName("expiresAtMs")] public long? ExpiresAtMs { get; set; }
}

/// <summary><c>GET /api/v1/portal/expense-receipts</c> (GOAL_DEPOLAMA_R2 S5).</summary>
public sealed class ExpenseReceiptsResponse
{
    [JsonPropertyName("from")] public string From { get; set; } = string.Empty;
    [JsonPropertyName("to")] public string To { get; set; } = string.Empty;
    [JsonPropertyName("items")] public ExpenseReceiptDto[] Items { get; set; } = [];
    [JsonPropertyName("truncated")] public bool Truncated { get; set; }
}

/// <summary>A receipt photo of a phone's expense or vehicle maintenance document.</summary>
public sealed class ExpenseReceiptDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("fileId")] public Guid FileId { get; set; }
    [JsonPropertyName("documentId")] public string DocumentId { get; set; } = string.Empty;

    /// <summary><c>expense</c> or <c>vehicle_maintenance</c>.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("sizeBytes")] public int SizeBytes { get; set; }
    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }
    [JsonPropertyName("createdByName")] public string CreatedByName { get; set; } = string.Empty;

    /// <summary>A presigned address for a few minutes; null while the server's file store is not set up.</summary>
    [JsonPropertyName("url")] public string? Url { get; set; }

    /// <summary>The document as the server holds it; null while it has not reached the server.</summary>
    [JsonPropertyName("document")] public ExpenseReceiptDocumentDto? Document { get; set; }
}

public sealed class ExpenseReceiptDocumentDto
{
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("amount")] public decimal? Amount { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("counterparty")] public string? Counterparty { get; set; }
    [JsonPropertyName("occurredAt")] public string? OccurredAt { get; set; }
    [JsonPropertyName("expenseCardCode")] public string? ExpenseCardCode { get; set; }
}

/// <summary>
/// <c>GET /api/v1/storage/products/images?stockCode=</c> (GOAL_DEPOLAMA_R2 S6): a product's own photos in order, and the
/// server's copies of its XML feed pictures (S7) in the feed's order.
/// </summary>
public sealed class ProductImagesDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("items")] public ProductImageDto[] Items { get; set; } = [];

    /// <summary>Read-only: the XML sync writes and removes them.</summary>
    [JsonPropertyName("xmlItems")] public ProductXmlImageDto[] XmlItems { get; set; } = [];
}

/// <summary>A picture the server copied from the company's XML feed: its 400 px and 1280 px WebP by their CDN addresses.</summary>
public sealed class ProductXmlImageDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("position")] public int Position { get; set; }
    [JsonPropertyName("thumbUrl")] public string ThumbUrl { get; set; } = string.Empty;
    [JsonPropertyName("fullUrl")] public string FullUrl { get; set; } = string.Empty;
}

/// <summary>A product photo: the server's 400 px and 1280 px WebP by their CDN addresses.</summary>
public sealed class ProductImageDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("sortOrder")] public int SortOrder { get; set; }
    [JsonPropertyName("thumbUrl")] public string ThumbUrl { get; set; } = string.Empty;
    [JsonPropertyName("fullUrl")] public string FullUrl { get; set; } = string.Empty;
    [JsonPropertyName("width")] public int Width { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }
    [JsonPropertyName("sizeBytes")] public long SizeBytes { get; set; }
    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }
    [JsonPropertyName("createdByName")] public string CreatedByName { get; set; } = string.Empty;
}

// ---- Depolama page (GOAL_DEPOLAMA_R2 P1) ----------------------------------------------------------------

/// <summary><c>GET /api/v1/storage/usage</c>: the company's one storage quota; areas and the trash count for who manages storage.</summary>
public sealed class StorageUsageDto
{
    [JsonPropertyName("available")] public bool Available { get; set; }
    [JsonPropertyName("usedBytes")] public long UsedBytes { get; set; }
    [JsonPropertyName("quotaBytes")] public long QuotaBytes { get; set; }
    [JsonPropertyName("freeBytes")] public long FreeBytes { get; set; }
    [JsonPropertyName("areas")] public StorageAreaUsageDto[]? Areas { get; set; }

    /// <summary>Part of <see cref="UsedBytes"/>: freed when the trash is emptied.</summary>
    [JsonPropertyName("trashedBytes")] public long TrashedBytes { get; set; }
    [JsonPropertyName("trashedCount")] public int? TrashedCount { get; set; }
}

public sealed class StorageAreaUsageDto
{
    [JsonPropertyName("area")] public string Area { get; set; } = string.Empty;
    [JsonPropertyName("usedBytes")] public long UsedBytes { get; set; }
    [JsonPropertyName("fileCount")] public int FileCount { get; set; }
}

/// <summary><c>GET /api/v1/storage/trash</c>.</summary>
public sealed class StorageTrashDto
{
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("totalBytes")] public long TotalBytes { get; set; }
    [JsonPropertyName("trashDays")] public int TrashDays { get; set; }
    [JsonPropertyName("items")] public StorageTrashItemDto[] Items { get; set; } = [];
}

public sealed class StorageTrashItemDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("area")] public string Area { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("sizeBytes")] public long SizeBytes { get; set; }
    [JsonPropertyName("fileCount")] public int FileCount { get; set; }
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;
    [JsonPropertyName("trashedAtMs")] public long TrashedAtMs { get; set; }
    [JsonPropertyName("trashedByName")] public string? TrashedByName { get; set; }
    [JsonPropertyName("daysLeft")] public int DaysLeft { get; set; }

    /// <summary>False for files whose record is gone: they can only be deleted for good.</summary>
    [JsonPropertyName("restorable")] public bool Restorable { get; set; }
    [JsonPropertyName("thumbUrl")] public string? ThumbUrl { get; set; }
}

/// <summary><c>POST /api/v1/storage/trash/restore</c>: every item's outcome.</summary>
public sealed class StorageRestoreResultDto
{
    [JsonPropertyName("restored")] public int Restored { get; set; }
    [JsonPropertyName("restoredBytes")] public long RestoredBytes { get; set; }
    [JsonPropertyName("failed")] public int Failed { get; set; }
    [JsonPropertyName("items")] public StorageRestoreItemDto[] Items { get; set; } = [];
}

public sealed class StorageRestoreItemDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("restored")] public bool Restored { get; set; }
    [JsonPropertyName("reason")] public string? Reason { get; set; }
}

/// <summary><c>POST /api/v1/storage/trash/purge</c>.</summary>
public sealed class StoragePurgeResultDto
{
    [JsonPropertyName("purged")] public int Purged { get; set; }
    [JsonPropertyName("purgedBytes")] public long PurgedBytes { get; set; }
    [JsonPropertyName("failed")] public int Failed { get; set; }
}

/// <summary><c>GET /api/v1/storage/cleanup/summary</c>: the "Alan aç" groups.</summary>
public sealed class StorageCleanupSummaryDto
{
    [JsonPropertyName("days")] public int Days { get; set; }
    [JsonPropertyName("groups")] public StorageCleanupGroupDto[] Groups { get; set; } = [];
}

public sealed class StorageCleanupGroupDto
{
    [JsonPropertyName("group")] public string Group { get; set; } = string.Empty;
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("bytes")] public long Bytes { get; set; }

    /// <summary>Deleted for good instead of the trash (XML pictures: the feed downloads them again).</summary>
    [JsonPropertyName("purgesDirectly")] public bool PurgesDirectly { get; set; }
}

/// <summary><c>GET /api/v1/storage/cleanup/candidates</c>.</summary>
public sealed class StorageCleanupCandidatesDto
{
    [JsonPropertyName("group")] public string Group { get; set; } = string.Empty;
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("totalBytes")] public long TotalBytes { get; set; }
    [JsonPropertyName("items")] public StorageCleanupCandidateDto[] Items { get; set; } = [];
}

public sealed class StorageCleanupCandidateDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("area")] public string Area { get; set; } = string.Empty;
    [JsonPropertyName("sizeBytes")] public long SizeBytes { get; set; }
    [JsonPropertyName("thumbUrl")] public string? ThumbUrl { get; set; }
    [JsonPropertyName("extra")] public string? Extra { get; set; }
}

/// <summary><c>POST /api/v1/storage/cleanup</c>.</summary>
public sealed class StorageCleanupResultDto
{
    [JsonPropertyName("group")] public string Group { get; set; } = string.Empty;
    [JsonPropertyName("trashedCount")] public int TrashedCount { get; set; }
    [JsonPropertyName("trashedBytes")] public long TrashedBytes { get; set; }
    [JsonPropertyName("purgedCount")] public int PurgedCount { get; set; }
    [JsonPropertyName("purgedBytes")] public long PurgedBytes { get; set; }
    [JsonPropertyName("remaining")] public int Remaining { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
}

/// <summary><c>GET /api/v1/storage/xml-images/status</c> (and <c>POST …/sync</c>'s answer).</summary>
public sealed class XmlImageStatusDto
{
    [JsonPropertyName("configured")] public bool Configured { get; set; }
    [JsonPropertyName("moduleEnabled")] public bool ModuleEnabled { get; set; }
    [JsonPropertyName("downloadImages")] public bool DownloadImages { get; set; }
    [JsonPropertyName("storageAvailable")] public bool StorageAvailable { get; set; }
    [JsonPropertyName("requestedAtMs")] public long? RequestedAtMs { get; set; }
    [JsonPropertyName("startedAtMs")] public long? StartedAtMs { get; set; }
    [JsonPropertyName("finishedAtMs")] public long? FinishedAtMs { get; set; }

    /// <summary><c>ok</c>, <c>partial</c>, <c>quota</c>, <c>failed</c>; null before the first run.</summary>
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("imageCount")] public int ImageCount { get; set; }
    [JsonPropertyName("imageBytes")] public long ImageBytes { get; set; }
    [JsonPropertyName("productCount")] public int ProductCount { get; set; }
}

/// <summary>Turkish sizes and the quota levels of the storage screens.</summary>
public static class StorageText
{
    /// <summary>"3,2 GB", "12,5 MB", "850 KB".</summary>
    public static string Bytes(long bytes) => bytes switch
    {
        >= 1L << 30 => (bytes / (double)(1L << 30)).ToString("N1", Fmt.Turkish) + " GB",
        >= 1L << 20 => (bytes / (double)(1L << 20)).ToString("N1", Fmt.Turkish) + " MB",
        >= 1L << 10 => (bytes / (double)(1L << 10)).ToString("N0", Fmt.Turkish) + " KB",
        _ => bytes.ToString("N0", Fmt.Turkish) + " B",
    };

    /// <summary>Share of the quota in use, 0–100.</summary>
    public static double Percent(long used, long quota) => quota <= 0 ? (used > 0 ? 100 : 0) : Math.Min(100, used * 100.0 / quota);

    /// <summary><c>ok</c>; <c>warn</c> from 80 % (yellow); <c>full</c> from 95 % (red) — GOAL_DEPOLAMA_R2 §5.</summary>
    public static string Level(long used, long quota) => Percent(used, quota) switch
    {
        >= 95 => "full",
        >= 80 => "warn",
        _ => "ok",
    };

    public static string AreaLabel(string area) => area switch
    {
        "product" => "Ürün",
        "xml" => "XML",
        "catalog" => "Katalog",
        "banner" => "Banner",
        "task" => "Görev",
        "expense" => "Gider",
        "vehicle" => "Araç",
        _ => area,
    };

    public static string SourceLabel(string source) => source switch
    {
        "user" => "Kullanıcı sildi",
        "cleanup" => "Alan aç",
        "sweep" => "Kaydı olmayan dosya",
        "owner_deleted" => "Kaydı silinmiş",
        _ => source,
    };
}
