namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A sales order the warehouse prepares, packs and loads (plan step 4, Faz 47).
///
/// <para>The row is the current state, kept for fast reads by the warehouse page and the TV board;
/// <see cref="OrderFulfillmentEvent"/> is the unchangeable history the reports are computed from.
/// Where the two disagree, the history is right.</para>
/// </summary>
public sealed class OrderFulfillment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>The <c>sales_order</c> job; one fulfillment per job (unique), so a retried document is one order.</summary>
    public Guid SourceJobId { get; set; }

    /// <summary>The approval request the order came through, when it needed one.</summary>
    public Guid? ApprovalRequestId { get; set; }

    /// <summary>The phone's document number (<c>mobileDocumentId</c>), else the job's external id.</summary>
    public string OrderNo { get; set; } = string.Empty;

    public string CustomerCode { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public Guid? SalespersonUserId { get; set; }

    /// <summary>Snapshot: stays readable after the user is renamed or deleted.</summary>
    public string SalespersonName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int LineCount { get; set; }

    public decimal ItemQuantity { get; set; }

    /// <summary>The pick list: <c>[{ stockCode, name, quantity, unit }]</c>.</summary>
    public string ItemsJson { get; set; } = "[]";

    /// <summary>One of <see cref="FulfillmentStatuses"/>.</summary>
    public string Status { get; set; } = FulfillmentStatuses.Pending;

    /// <summary>Server time the order entered the queue.</summary>
    public DateTimeOffset QueuedAtUtc { get; set; }

    /// <summary>The change number the order was queued with: queue order on every database provider.</summary>
    public long QueuedSeq { get; set; }

    /// <summary>The last move to <see cref="FulfillmentStatuses.Preparing"/>.</summary>
    public DateTimeOffset? StartedAtUtc { get; set; }

    public DateTimeOffset? PackedAtUtc { get; set; }

    public DateTimeOffset? LoadedAtUtc { get; set; }

    /// <summary>Who prepares the order: the one who started it, or the one a manager reassigned it to.</summary>
    public Guid? AssigneeUserId { get; set; }

    public string? AssigneeName { get; set; }

    public string? VehiclePlate { get; set; }

    /// <summary>One of <see cref="FulfillmentErpStates"/>: where the order stands in the company's ERP.</summary>
    public string ErpState { get; set; } = FulfillmentErpStates.None;

    /// <summary>
    /// The tenant's change order, reserved from <c>tenant_sync_counter</c> inside the writing transaction
    /// (never an identity or a clock, see rule 11): a reader asking for changes after a value never steps
    /// over a change that commits later.
    /// </summary>
    public long UpdatedSeq { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

/// <summary>One step of an order's preparation. Rows are only ever added.</summary>
public sealed class OrderFulfillmentEvent
{
    public long Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid FulfillmentId { get; set; }

    public OrderFulfillment? Fulfillment { get; set; }

    /// <summary>Null on <see cref="FulfillmentActions.Queued"/>.</summary>
    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = string.Empty;

    /// <summary>One of <see cref="FulfillmentActions"/>.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Null for what the system does (queueing, an ERP result).</summary>
    public Guid? ActorUserId { get; set; }

    public string ActorName { get; set; } = string.Empty;

    public string? DeviceId { get; set; }

    public string? Note { get; set; }

    /// <summary>Server time; device clocks are not trusted with durations.</summary>
    public DateTimeOffset OccurredAtUtc { get; set; }
}

/// <summary>A company's warehouse module: off until a manager turns it on, and the board's delay thresholds.</summary>
public sealed class TenantWarehouseSettings
{
    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>Off by default, so a company that does not use the warehouse gets no queue and no warnings.</summary>
    public bool Enabled { get; set; }

    public int PendingWarnMinutes { get; set; } = 15;

    public int PendingCriticalMinutes { get; set; } = 30;

    public int PreparingWarnMinutes { get; set; } = 20;

    public int PreparingCriticalMinutes { get; set; } = 45;

    /// <summary>Packed but not loaded for this long is late.</summary>
    public int PackedWarnMinutes { get; set; } = 60;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }
}

public static class FulfillmentStatuses
{
    public const string Pending = "PENDING";
    public const string Preparing = "PREPARING";
    public const string Packed = "PACKED";
    public const string Loaded = "LOADED";
    public const string Cancelled = "CANCELLED";

    public static readonly IReadOnlyList<string> All = [Pending, Preparing, Packed, Loaded, Cancelled];

    /// <summary>Still in the warehouse's hands.</summary>
    public static readonly IReadOnlyList<string> Open = [Pending, Preparing, Packed];
}

public static class FulfillmentActions
{
    public const string Queued = "QUEUED";
    public const string Start = "START";
    public const string Pack = "PACK";
    public const string Load = "LOAD";
    public const string Undo = "UNDO";
    public const string Cancel = "CANCEL";
    public const string Reassign = "REASSIGN";
    public const string ErpFailed = "ERP_FAILED";
}

public static class FulfillmentErpStates
{
    /// <summary>A company without an ERP: nothing to write.</summary>
    public const string None = "NONE";

    /// <summary>Waiting for the agent.</summary>
    public const string Pending = "PENDING";
    public const string Written = "WRITTEN";
    public const string Failed = "FAILED";
}
