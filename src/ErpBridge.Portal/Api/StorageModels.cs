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
