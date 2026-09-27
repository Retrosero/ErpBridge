using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

// SKT (son kullanma tarihi) kayıtları: /api/v1/android/expiry. Times are unix milliseconds (UTC);
// the expiry date is a calendar day, "yyyy-MM-dd".

public sealed class ExpiryRecordDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;

    [JsonPropertyName("barcode")] public string? Barcode { get; set; }

    [JsonPropertyName("productName")] public string? ProductName { get; set; }

    /// <summary>Reyon / raf.</summary>
    [JsonPropertyName("location")] public string Location { get; set; } = string.Empty;

    [JsonPropertyName("warehouse")] public string? Warehouse { get; set; }

    /// <summary><c>yyyy-MM-dd</c>.</summary>
    [JsonPropertyName("expiryDate")] public string ExpiryDate { get; set; } = string.Empty;

    [JsonPropertyName("quantity")] public decimal? Quantity { get; set; }

    [JsonPropertyName("note")] public string? Note { get; set; }

    [JsonPropertyName("closed")] public bool Closed { get; set; }

    /// <summary>Tombstone: the phone drops its copy.</summary>
    [JsonPropertyName("deleted")] public bool Deleted { get; set; }

    /// <summary>Display name of the user who entered it.</summary>
    [JsonPropertyName("createdBy")] public string CreatedBy { get; set; } = string.Empty;

    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }

    [JsonPropertyName("updatedAtMs")] public long UpdatedAtMs { get; set; }

    [JsonPropertyName("updatedSeq")] public long UpdatedSeq { get; set; }
}

public sealed class ExpiryListResponse
{
    /// <summary>Deleted records included (<c>deleted=true</c>), so phones remove them.</summary>
    [JsonPropertyName("records")] public ExpiryRecordDto[] Records { get; set; } = [];

    /// <summary>Pass back as <c>changedSinceSeq</c>; the page's last <c>updatedSeq</c>.</summary>
    [JsonPropertyName("latestSeq")] public long LatestSeq { get; set; }

    [JsonPropertyName("hasMore")] public bool HasMore { get; set; }
}

/// <summary>
/// The full editable state of a record. <c>upsert</c> overwrites every field (a null means "empty",
/// never "unchanged"); <c>delete</c> reads only <see cref="Id"/>.
/// </summary>
public sealed class ExpiryRecordInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("stockCode")] public string? StockCode { get; set; }

    [JsonPropertyName("barcode")] public string? Barcode { get; set; }

    [JsonPropertyName("productName")] public string? ProductName { get; set; }

    [JsonPropertyName("location")] public string? Location { get; set; }

    [JsonPropertyName("warehouse")] public string? Warehouse { get; set; }

    /// <summary><c>yyyy-MM-dd</c>; kept as text so a bad date rejects one operation, not the whole batch.</summary>
    [JsonPropertyName("expiryDate")] public string? ExpiryDate { get; set; }

    [JsonPropertyName("quantity")] public decimal? Quantity { get; set; }

    [JsonPropertyName("note")] public string? Note { get; set; }

    /// <summary>Missing or null = open.</summary>
    [JsonPropertyName("closed")] public bool? Closed { get; set; }
}

/// <summary>One queued change from a phone: <c>upsert</c> or <c>delete</c>.</summary>
public sealed class ExpiryOp
{
    [JsonPropertyName("opId")] public Guid OpId { get; set; }

    [JsonPropertyName("type")] public string? Type { get; set; }

    [JsonPropertyName("record")] public ExpiryRecordInput? Record { get; set; }
}

public sealed class ExpiryOpsRequest
{
    [JsonPropertyName("ops")] public ExpiryOp[]? Ops { get; set; }
}

public sealed class ExpiryOpResult
{
    [JsonPropertyName("opId")] public Guid OpId { get; set; }

    /// <summary><c>applied</c>, <c>duplicate</c> (already applied earlier) or <c>rejected</c> (drop it; do not retry).</summary>
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;

    [JsonPropertyName("errorCode")] public string? ErrorCode { get; set; }

    [JsonPropertyName("message")] public string? Message { get; set; }
}

public sealed class ExpiryOpsResponse
{
    [JsonPropertyName("results")] public ExpiryOpResult[] Results { get; set; } = [];

    /// <summary>The current state of every record an applied operation of the batch touched.</summary>
    [JsonPropertyName("records")] public ExpiryRecordDto[] Records { get; set; } = [];
}
