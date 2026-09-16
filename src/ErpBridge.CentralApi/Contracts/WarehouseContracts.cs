using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

/// <summary>An order in the warehouse queue, as lists and the board show it.</summary>
public sealed class FulfillmentDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("orderNo")] public string OrderNo { get; set; } = string.Empty;
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;
    [JsonPropertyName("salespersonName")] public string SalespersonName { get; set; } = string.Empty;
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("lineCount")] public int LineCount { get; set; }
    [JsonPropertyName("itemQuantity")] public decimal ItemQuantity { get; set; }

    /// <summary>PENDING, PREPARING, PACKED, LOADED or CANCELLED.</summary>
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("queuedAtUtc")] public DateTimeOffset QueuedAtUtc { get; set; }
    [JsonPropertyName("startedAtUtc")] public DateTimeOffset? StartedAtUtc { get; set; }
    [JsonPropertyName("packedAtUtc")] public DateTimeOffset? PackedAtUtc { get; set; }
    [JsonPropertyName("loadedAtUtc")] public DateTimeOffset? LoadedAtUtc { get; set; }
    [JsonPropertyName("assigneeUserId")] public Guid? AssigneeUserId { get; set; }
    [JsonPropertyName("assigneeName")] public string? AssigneeName { get; set; }
    [JsonPropertyName("vehiclePlate")] public string? VehiclePlate { get; set; }

    /// <summary>NONE (no ERP), PENDING, WRITTEN or FAILED.</summary>
    [JsonPropertyName("erpState")] public string ErpState { get; set; } = string.Empty;
    [JsonPropertyName("approvalRequestId")] public Guid? ApprovalRequestId { get; set; }
    [JsonPropertyName("updatedSeq")] public long UpdatedSeq { get; set; }
}

/// <summary><c>GET /api/v1/portal/fulfillments</c>.</summary>
public sealed class FulfillmentListResponse
{
    /// <summary>
    /// Where to continue: ask again with <c>changedSinceSeq</c> set to it. A change page ends at its last
    /// row, so no change is skipped; see <see cref="HasMore"/>.
    /// </summary>
    [JsonPropertyName("latestSeq")] public long LatestSeq { get; set; }

    /// <summary>More rows matched than were returned: for a change page, ask again at once from <see cref="LatestSeq"/>.</summary>
    [JsonPropertyName("hasMore")] public bool HasMore { get; set; }
    [JsonPropertyName("items")] public FulfillmentDto[] Items { get; set; } = [];
}

/// <summary>One step of an order's history.</summary>
public sealed class FulfillmentEventDto
{
    [JsonPropertyName("action")] public string Action { get; set; } = string.Empty;
    [JsonPropertyName("fromStatus")] public string? FromStatus { get; set; }
    [JsonPropertyName("toStatus")] public string ToStatus { get; set; } = string.Empty;
    [JsonPropertyName("actorUserId")] public Guid? ActorUserId { get; set; }
    [JsonPropertyName("actorName")] public string ActorName { get; set; } = string.Empty;
    [JsonPropertyName("note")] public string? Note { get; set; }
    [JsonPropertyName("occurredAtUtc")] public DateTimeOffset OccurredAtUtc { get; set; }
}

/// <summary><c>GET /api/v1/portal/fulfillments/{id}</c>: the order, its pick list and its history.</summary>
public sealed class FulfillmentDetailResponse
{
    [JsonPropertyName("fulfillment")] public FulfillmentDto Fulfillment { get; set; } = new();

    /// <summary><c>[{ stockCode, name, quantity, unit }]</c>.</summary>
    [JsonPropertyName("items")] public JsonElement Items { get; set; }
    [JsonPropertyName("events")] public FulfillmentEventDto[] Events { get; set; } = [];
}

/// <summary>Body of <c>POST /api/v1/portal/fulfillments/{id}/{action}</c>; each action reads what it needs.</summary>
public sealed class FulfillmentActionRequest
{
    [JsonPropertyName("note")] public string? Note { get; set; }

    /// <summary><c>load</c> only, optional.</summary>
    [JsonPropertyName("vehiclePlate")] public string? VehiclePlate { get; set; }

    /// <summary><c>reassign</c> only.</summary>
    [JsonPropertyName("assigneeUserId")] public Guid? AssigneeUserId { get; set; }
}

/// <summary><c>GET|PUT /api/v1/portal/warehouse/settings</c>.</summary>
public sealed class WarehouseSettingsDto
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; }
    [JsonPropertyName("pendingWarnMinutes")] public int PendingWarnMinutes { get; set; }
    [JsonPropertyName("pendingCriticalMinutes")] public int PendingCriticalMinutes { get; set; }
    [JsonPropertyName("preparingWarnMinutes")] public int PreparingWarnMinutes { get; set; }
    [JsonPropertyName("preparingCriticalMinutes")] public int PreparingCriticalMinutes { get; set; }
    [JsonPropertyName("packedWarnMinutes")] public int PackedWarnMinutes { get; set; }
    [JsonPropertyName("updatedAtUtc")] public DateTimeOffset? UpdatedAtUtc { get; set; }
}

/// <summary><c>GET /api/v1/portal/events</c>: the topics' latest changes, once one moved or the wait ends.</summary>
public sealed class PortalEventsResponse
{
    /// <summary>The warehouse queue's latest change; 0 without a warehouse role.</summary>
    [JsonPropertyName("latestSeq")] public long LatestSeq { get; set; }

    /// <summary>The company's approval change version; send it back as <c>approvalsVersion</c>.</summary>
    [JsonPropertyName("approvalsVersion")] public long ApprovalsVersion { get; set; }

    /// <summary>A topic the caller asked about moved.</summary>
    [JsonPropertyName("changed")] public bool Changed { get; set; }
}
