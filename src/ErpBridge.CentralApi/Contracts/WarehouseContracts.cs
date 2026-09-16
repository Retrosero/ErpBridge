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

    /// <summary>What the history says about the order's times (plan step 8).</summary>
    [JsonPropertyName("times")] public FulfillmentTimesDto Times { get; set; } = new();
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

/// <summary><c>POST /api/v1/portal/warehouse/backfill</c> body.</summary>
public sealed class WarehouseBackfillRequest
{
    [JsonPropertyName("days")] public int? Days { get; set; }
}

public sealed class WarehouseBackfillResponse
{
    [JsonPropertyName("days")] public int Days { get; set; }

    /// <summary>Orders newly queued; ones already in the queue are not counted.</summary>
    [JsonPropertyName("queued")] public int Queued { get; set; }
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

/// <summary>An order's measured times, from its history (plan step 8). Durations in whole seconds.</summary>
public sealed class FulfillmentTimesDto
{
    /// <summary>From queueing to the first start; null until started.</summary>
    [JsonPropertyName("waitSeconds")] public long? WaitSeconds { get; set; }

    /// <summary>Time in preparation that ended in packing; undone starts and cancelled work do not count.</summary>
    [JsonPropertyName("netPreparationSeconds")] public long NetPreparationSeconds { get; set; }

    /// <summary>From the last packing to loading; null until loaded.</summary>
    [JsonPropertyName("untilLoadingSeconds")] public long? UntilLoadingSeconds { get; set; }
    [JsonPropertyName("firstStartedAtUtc")] public DateTimeOffset? FirstStartedAtUtc { get; set; }
    [JsonPropertyName("startedByName")] public string? StartedByName { get; set; }
    [JsonPropertyName("packedAtUtc")] public DateTimeOffset? PackedAtUtc { get; set; }
    [JsonPropertyName("packedByName")] public string? PackedByName { get; set; }
    [JsonPropertyName("loadedAtUtc")] public DateTimeOffset? LoadedAtUtc { get; set; }
}

/// <summary><c>GET /api/v1/portal/warehouse/dashboard</c>: the warehouse today, for the manager's home page.</summary>
public sealed class WarehouseDashboardResponse
{
    /// <summary>yyyy-MM-dd, Istanbul.</summary>
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;

    /// <summary>The module is on; the home page hides the warehouse cards otherwise.</summary>
    [JsonPropertyName("enabled")] public bool Enabled { get; set; }
    [JsonPropertyName("pending")] public int Pending { get; set; }
    [JsonPropertyName("preparing")] public int Preparing { get; set; }
    [JsonPropertyName("packed")] public int Packed { get; set; }

    /// <summary>Open orders past their status's warning threshold now (the board's yellow and red cards).</summary>
    [JsonPropertyName("late")] public int Late { get; set; }

    /// <summary>Of <see cref="Late"/>, past the critical threshold.</summary>
    [JsonPropertyName("critical")] public int Critical { get; set; }
    [JsonPropertyName("queuedOnDay")] public int QueuedOnDay { get; set; }
    [JsonPropertyName("packedOnDay")] public int PackedOnDay { get; set; }
    [JsonPropertyName("loadedOnDay")] public int LoadedOnDay { get; set; }

    /// <summary>Orders first started that day; null when none.</summary>
    [JsonPropertyName("averageWaitSeconds")] public long? AverageWaitSeconds { get; set; }

    /// <summary>Orders packed that day; null when none.</summary>
    [JsonPropertyName("averageNetPreparationSeconds")] public long? AverageNetPreparationSeconds { get; set; }
}

/// <summary>One packer's work in a range: the orders whose (still standing) packing fell in it.</summary>
public sealed class WarehouseStaffPerformance
{
    [JsonPropertyName("userId")] public Guid? UserId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("packedCount")] public int PackedCount { get; set; }
    [JsonPropertyName("lineCount")] public int LineCount { get; set; }
    [JsonPropertyName("itemQuantity")] public decimal ItemQuantity { get; set; }
    [JsonPropertyName("totalNetPreparationSeconds")] public long TotalNetPreparationSeconds { get; set; }
    [JsonPropertyName("averageNetPreparationSeconds")] public long AverageNetPreparationSeconds { get; set; }
    [JsonPropertyName("medianNetPreparationSeconds")] public long MedianNetPreparationSeconds { get; set; }

    /// <summary>Net preparation per pick line; null when the orders had no lines.</summary>
    [JsonPropertyName("secondsPerLine")] public long? SecondsPerLine { get; set; }
}

/// <summary>One day of the range.</summary>
public sealed class WarehouseDayPerformance
{
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("queued")] public int Queued { get; set; }
    [JsonPropertyName("packed")] public int Packed { get; set; }
    [JsonPropertyName("averageWaitSeconds")] public long? AverageWaitSeconds { get; set; }
    [JsonPropertyName("averageNetPreparationSeconds")] public long? AverageNetPreparationSeconds { get; set; }
}

/// <summary>An order in the range's slowest lists, linking to its timeline.</summary>
public sealed class WarehouseSlowOrder
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("orderNo")] public string OrderNo { get; set; } = string.Empty;
    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("lineCount")] public int LineCount { get; set; }
    [JsonPropertyName("queuedAtUtc")] public DateTimeOffset QueuedAtUtc { get; set; }
    [JsonPropertyName("times")] public FulfillmentTimesDto Times { get; set; } = new();
}

/// <summary><c>GET /api/v1/portal/warehouse/performance</c>: a range's totals, packers and slowest orders.</summary>
public sealed class WarehousePerformanceResponse
{
    [JsonPropertyName("from")] public string From { get; set; } = string.Empty;
    [JsonPropertyName("to")] public string To { get; set; } = string.Empty;
    [JsonPropertyName("queued")] public int Queued { get; set; }
    [JsonPropertyName("packed")] public int Packed { get; set; }
    [JsonPropertyName("loaded")] public int Loaded { get; set; }
    [JsonPropertyName("cancelled")] public int Cancelled { get; set; }
    [JsonPropertyName("averageWaitSeconds")] public long? AverageWaitSeconds { get; set; }
    [JsonPropertyName("medianWaitSeconds")] public long? MedianWaitSeconds { get; set; }
    [JsonPropertyName("averageNetPreparationSeconds")] public long? AverageNetPreparationSeconds { get; set; }
    [JsonPropertyName("medianNetPreparationSeconds")] public long? MedianNetPreparationSeconds { get; set; }
    [JsonPropertyName("averageUntilLoadingSeconds")] public long? AverageUntilLoadingSeconds { get; set; }
    [JsonPropertyName("staff")] public WarehouseStaffPerformance[] Staff { get; set; } = [];
    [JsonPropertyName("days")] public WarehouseDayPerformance[] Days { get; set; } = [];

    /// <summary>Longest waits among orders first started in the range (at most 10).</summary>
    [JsonPropertyName("longestWaits")] public WarehouseSlowOrder[] LongestWaits { get; set; } = [];

    /// <summary>Longest net preparations among orders packed in the range (at most 10).</summary>
    [JsonPropertyName("longestPreparations")] public WarehouseSlowOrder[] LongestPreparations { get; set; } = [];
}
