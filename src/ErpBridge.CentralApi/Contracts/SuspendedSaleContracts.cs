using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

// Bekleyen siparişler: /api/v1/android/suspended-sales. Times are unix milliseconds (UTC).

public sealed class SuspendedSaleLineDto
{
    [JsonPropertyName("barcode")] public string Barcode { get; set; } = string.Empty;

    [JsonPropertyName("stockCode")] public string? StockCode { get; set; }

    /// <summary>Snapshot of the product name when parked.</summary>
    [JsonPropertyName("productName")] public string? ProductName { get; set; }

    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }

    [JsonPropertyName("price")] public decimal Price { get; set; }

    [JsonPropertyName("lineDiscountPercent")] public decimal LineDiscountPercent { get; set; }

    [JsonPropertyName("note")] public string? Note { get; set; }
}

public sealed class SuspendedSaleDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("docNo")] public string DocNo { get; set; } = string.Empty;

    [JsonPropertyName("customerId")] public string? CustomerId { get; set; }

    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("warehouse")] public string? Warehouse { get; set; }

    [JsonPropertyName("note")] public string? Note { get; set; }

    [JsonPropertyName("totalAmount")] public decimal TotalAmount { get; set; }

    [JsonPropertyName("lines")] public SuspendedSaleLineDto[] Lines { get; set; } = [];

    /// <summary>Tombstone (claimed or deleted): the phone drops its copy.</summary>
    [JsonPropertyName("deleted")] public bool Deleted { get; set; }

    /// <summary><c>claimed</c> or <c>deleted</c> when <see cref="Deleted"/>.</summary>
    [JsonPropertyName("closedReason")] public string? ClosedReason { get; set; }

    [JsonPropertyName("closedBy")] public string? ClosedBy { get; set; }

    /// <summary>Display name of the user who parked it.</summary>
    [JsonPropertyName("createdBy")] public string CreatedBy { get; set; } = string.Empty;

    /// <summary>Lets the phone offer "delete" only to its creator (managers may delete any).</summary>
    [JsonPropertyName("createdByUserId")] public Guid CreatedByUserId { get; set; }

    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }

    [JsonPropertyName("updatedAtMs")] public long UpdatedAtMs { get; set; }

    [JsonPropertyName("updatedSeq")] public long UpdatedSeq { get; set; }
}

public sealed class SuspendedSaleListResponse
{
    /// <summary>Claimed and deleted sales included (<c>deleted=true</c>), so phones remove them.</summary>
    [JsonPropertyName("sales")] public SuspendedSaleDto[] Sales { get; set; } = [];

    /// <summary>Pass back as <c>changedSinceSeq</c>; the page's last <c>updatedSeq</c>.</summary>
    [JsonPropertyName("latestSeq")] public long LatestSeq { get; set; }

    [JsonPropertyName("hasMore")] public bool HasMore { get; set; }
}

/// <summary>
/// The full state of a parked sale. <c>upsert</c> overwrites every field; <c>claim</c> and <c>delete</c> read
/// only <see cref="Id"/>.
/// </summary>
public sealed class SuspendedSaleInput
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("docNo")] public string? DocNo { get; set; }

    [JsonPropertyName("customerId")] public string? CustomerId { get; set; }

    [JsonPropertyName("customerName")] public string? CustomerName { get; set; }

    [JsonPropertyName("warehouse")] public string? Warehouse { get; set; }

    [JsonPropertyName("note")] public string? Note { get; set; }

    [JsonPropertyName("totalAmount")] public decimal? TotalAmount { get; set; }

    [JsonPropertyName("lines")] public SuspendedSaleLineDto[]? Lines { get; set; }
}

/// <summary>One queued change from a phone: <c>upsert</c>, <c>claim</c> or <c>delete</c>.</summary>
public sealed class SuspendedSaleOp
{
    [JsonPropertyName("opId")] public Guid OpId { get; set; }

    [JsonPropertyName("type")] public string? Type { get; set; }

    [JsonPropertyName("sale")] public SuspendedSaleInput? Sale { get; set; }
}

public sealed class SuspendedSaleOpsRequest
{
    [JsonPropertyName("ops")] public SuspendedSaleOp[]? Ops { get; set; }
}

public sealed class SuspendedSaleOpResult
{
    [JsonPropertyName("opId")] public Guid OpId { get; set; }

    /// <summary><c>applied</c>, <c>duplicate</c> (already applied earlier) or <c>rejected</c> (drop it; do not retry).</summary>
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;

    [JsonPropertyName("errorCode")] public string? ErrorCode { get; set; }

    [JsonPropertyName("message")] public string? Message { get; set; }
}

public sealed class SuspendedSaleOpsResponse
{
    [JsonPropertyName("results")] public SuspendedSaleOpResult[] Results { get; set; } = [];

    /// <summary>The current state of every sale an operation of the batch touched (applied or rejected).</summary>
    [JsonPropertyName("sales")] public SuspendedSaleDto[] Sales { get; set; } = [];
}
