using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErpBridge.Portal.Api;

// Mirrors of the central API contracts the portal reads (ErpBridge.CentralApi/Contracts:
// MobileAccountContracts, ApprovalContracts, PortalContracts). Kept local on purpose.

public sealed class LoginRequest
{
    [JsonPropertyName("tenantCode")] public string TenantCode { get; set; } = string.Empty;
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("password")] public string Password { get; set; } = string.Empty;
    [JsonPropertyName("deviceId")] public string DeviceId { get; set; } = string.Empty;
    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }

    /// <summary>Tells the server this is the portal: portal-only roles may sign in, documents may not be posted.</summary>
    [JsonPropertyName("client")] public string Client { get; set; } = "portal";

    /// <summary>True: a 30-day token the browser keeps; false: a 12-hour token for this tab.</summary>
    [JsonPropertyName("rememberMe")] public bool RememberMe { get; set; }
}

public sealed class LoginResponse
{
    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset ExpiresAtUtc { get; set; }
    [JsonPropertyName("session")] public SessionDto Session { get; set; } = new();
}

public sealed class SessionDto
{
    [JsonPropertyName("user")] public UserDto User { get; set; } = new();
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("tenantName")] public string TenantName { get; set; } = string.Empty;
    [JsonPropertyName("tenantCode")] public string? TenantCode { get; set; }
    [JsonPropertyName("seats")] public SeatsDto Seats { get; set; } = new();
    [JsonPropertyName("dataSource")] public string DataSource { get; set; } = "erp";
}

public sealed class SeatsDto
{
    [JsonPropertyName("max")] public int Max { get; set; }
    [JsonPropertyName("used")] public int Used { get; set; }
    [JsonPropertyName("endsAtUtc")] public DateTimeOffset? EndsAtUtc { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = "none";
}

public sealed class UserDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;

    /// <summary>Every role (ADMIN, MANAGER, ACCOUNTING, WAREHOUSE, SALES); empty from a server before multi-role accounts.</summary>
    [JsonPropertyName("roles")] public string[] Roles { get; set; } = [];

    /// <summary>The server's effective right to decide approvals: admin, approving manager or accounting.</summary>
    [JsonPropertyName("canApprove")] public bool CanApprove { get; set; }
    [JsonPropertyName("canManageApprovalRules")] public bool CanManageApprovalRules { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("lastLoginAtUtc")] public DateTimeOffset? LastLoginAtUtc { get; set; }

    /// <summary>The roles, or the single role a server before multi-role accounts sends.</summary>
    public IReadOnlyList<string> EffectiveRoles() =>
        Roles.Length > 0 ? Roles : string.IsNullOrWhiteSpace(Role) ? [] : [Role];
}

public sealed class UserListResponse
{
    [JsonPropertyName("seats")] public SeatsDto Seats { get; set; } = new();
    [JsonPropertyName("users")] public UserDto[] Users { get; set; } = [];
}

public sealed class CreateUserRequest
{
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("password")] public string Password { get; set; } = string.Empty;
    [JsonPropertyName("roles")] public List<string> Roles { get; set; } = [];

    /// <summary>Meaningful for a manager only; the server ignores it otherwise.</summary>
    [JsonPropertyName("canApprove")] public bool CanApprove { get; set; }
}

/// <summary>Replaces a user's roles (PATCH). The server keeps the last administrator.</summary>
public sealed class UpdateUserRolesRequest
{
    [JsonPropertyName("roles")] public List<string> Roles { get; set; } = [];

    /// <summary>
    /// The manager's own right to decide every kind; <c>null</c> keeps it as stored. The user list
    /// carries only the effective right (accounting approves money documents without this flag), so
    /// the editor sends a value only when the administrator changed the switch.
    /// </summary>
    [JsonPropertyName("canApprove")] public bool? CanApprove { get; set; }
}

public sealed class ApprovalDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("externalId")] public string ExternalId { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("counterpartyName")] public string CounterpartyName { get; set; } = string.Empty;
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("summary")] public JsonElement Summary { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("requestedByUserId")] public Guid? RequestedByUserId { get; set; }
    [JsonPropertyName("requestedByName")] public string? RequestedByName { get; set; }
    [JsonPropertyName("requestedAtUtc")] public DateTimeOffset RequestedAtUtc { get; set; }
    [JsonPropertyName("requestedSeq")] public long RequestedSeq { get; set; }
    [JsonPropertyName("decidedByName")] public string? DecidedByName { get; set; }
    [JsonPropertyName("decidedAtUtc")] public DateTimeOffset? DecidedAtUtc { get; set; }
    [JsonPropertyName("decisionNote")] public string? DecisionNote { get; set; }

    /// <summary>Whether the signed-in user's roles decide this kind; null from a server before Faz 48.</summary>
    [JsonPropertyName("canDecide")] public bool? CanDecide { get; set; }
}

/// <summary><c>GET /api/v1/android/approvals/{id}</c>: the request, its documents, history and stock warnings.</summary>
public sealed class ApprovalDetailDto
{
    [JsonPropertyName("request")] public ApprovalDto Request { get; set; } = new();

    /// <summary><c>[{ documentType, externalId, payload }]</c>, exactly as they are posted on approval.</summary>
    [JsonPropertyName("documents")] public JsonElement Documents { get; set; }
    [JsonPropertyName("events")] public ApprovalEventDto[] Events { get; set; } = [];
    [JsonPropertyName("warnings")] public StockWarningDto[] Warnings { get; set; } = [];
}

public sealed class ApprovalEventDto
{
    [JsonPropertyName("action")] public string Action { get; set; } = string.Empty;
    [JsonPropertyName("byName")] public string? ByName { get; set; }
    [JsonPropertyName("atUtc")] public DateTimeOffset AtUtc { get; set; }
    [JsonPropertyName("note")] public string? Note { get; set; }
}

/// <summary>A product the request would sell more of than is in stock (a warning; approval stays possible).</summary>
public sealed class StockWarningDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("requested")] public decimal Requested { get; set; }
    [JsonPropertyName("onHand")] public decimal OnHand { get; set; }
}

/// <summary><c>GET /api/v1/portal/events</c>.</summary>
public sealed class PortalEventsDto
{
    [JsonPropertyName("latestSeq")] public long LatestSeq { get; set; }

    /// <summary>The company's approval change version; sent back to ask for changes after it.</summary>
    [JsonPropertyName("approvalsVersion")] public long ApprovalsVersion { get; set; }
    [JsonPropertyName("changed")] public bool Changed { get; set; }
}

/// <summary><c>GET /api/v1/portal/fulfillments</c>.</summary>
public sealed class FulfillmentListDto
{
    [JsonPropertyName("latestSeq")] public long LatestSeq { get; set; }
    [JsonPropertyName("hasMore")] public bool HasMore { get; set; }
    [JsonPropertyName("items")] public FulfillmentDto[] Items { get; set; } = [];
}

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

/// <summary>A line of an order's pick list.</summary>
public sealed class PickItemDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("unit")] public string? Unit { get; set; }
}

/// <summary><c>GET /api/v1/portal/fulfillments/{id}</c>: the order, its pick list, its history and its measured times.</summary>
public sealed class FulfillmentDetailDto
{
    [JsonPropertyName("fulfillment")] public FulfillmentDto Fulfillment { get; set; } = new();
    [JsonPropertyName("items")] public PickItemDto[] Items { get; set; } = [];
    [JsonPropertyName("events")] public List<FulfillmentEventDto> Events { get; set; } = [];
    [JsonPropertyName("times")] public FulfillmentTimesDto Times { get; set; } = new();
}

public sealed class WarehouseBackfillDto
{
    [JsonPropertyName("days")] public int Days { get; set; }
    [JsonPropertyName("queued")] public int Queued { get; set; }
}

public sealed class ApprovalSummaryDto
{
    [JsonPropertyName("pendingCount")] public int PendingCount { get; set; }
}

public sealed class MoneyLine
{
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
}

public sealed class SummaryResponse
{
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("dataSource")] public string DataSource { get; set; } = string.Empty;
    [JsonPropertyName("sales")] public MoneyLine Sales { get; set; } = new();
    [JsonPropertyName("collections")] public MoneyLine Collections { get; set; } = new();
    [JsonPropertyName("disbursements")] public MoneyLine Disbursements { get; set; } = new();
    [JsonPropertyName("returns")] public MoneyLine Returns { get; set; } = new();
    [JsonPropertyName("visitsPlanned")] public int VisitsPlanned { get; set; }
    [JsonPropertyName("visitsCompleted")] public int VisitsCompleted { get; set; }
    [JsonPropertyName("visitsSkipped")] public int VisitsSkipped { get; set; }
    [JsonPropertyName("pendingApprovals")] public int PendingApprovals { get; set; }
}

public sealed class UserActivity
{
    [JsonPropertyName("userId")] public Guid? UserId { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;
    [JsonPropertyName("sales")] public MoneyLine Sales { get; set; } = new();
    [JsonPropertyName("collections")] public MoneyLine Collections { get; set; } = new();
    [JsonPropertyName("disbursements")] public MoneyLine Disbursements { get; set; } = new();
    [JsonPropertyName("returns")] public MoneyLine Returns { get; set; } = new();
    [JsonPropertyName("visitsCompleted")] public int VisitsCompleted { get; set; }
    [JsonPropertyName("visitsSkipped")] public int VisitsSkipped { get; set; }
}

public sealed class ActivityResponse
{
    [JsonPropertyName("from")] public string From { get; set; } = string.Empty;
    [JsonPropertyName("to")] public string To { get; set; } = string.Empty;
    [JsonPropertyName("users")] public List<UserActivity> Users { get; set; } = [];
}

public sealed class VisitRow
{
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("planName")] public string PlanName { get; set; } = string.Empty;
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;
    [JsonPropertyName("visitOrder")] public int VisitOrder { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = "PENDING";
    [JsonPropertyName("note")] public string Note { get; set; } = string.Empty;
    [JsonPropertyName("completedAt")] public long? CompletedAt { get; set; }
    [JsonPropertyName("planned")] public bool Planned { get; set; } = true;
}

public sealed class VisitsResponse
{
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("rows")] public List<VisitRow> Rows { get; set; } = [];
}

public sealed class BalanceRow
{
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("balance")] public decimal Balance { get; set; }
}

public sealed class BalancesResponse
{
    [JsonPropertyName("totalReceivable")] public decimal TotalReceivable { get; set; }
    [JsonPropertyName("totalPayable")] public decimal TotalPayable { get; set; }
    [JsonPropertyName("rows")] public List<BalanceRow> Rows { get; set; } = [];
    [JsonPropertyName("truncated")] public bool Truncated { get; set; }
}

public sealed class StockWarehouseDto
{
    [JsonPropertyName("warehouseNo")] public int WarehouseNo { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("reserved")] public decimal Reserved { get; set; }
}

public sealed class StockPriceDto
{
    [JsonPropertyName("listNumber")] public int ListNumber { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("price")] public decimal Price { get; set; }
}

/// <summary>One row of GET …/native/stock-cards/{code}/movements (GOAL_PANEL_ERPSIZ E6a).</summary>
public sealed class StockMovementRowDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;

    /// <summary>sale, purchase, sale_return, count, void, other.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = "other";
    [JsonPropertyName("documentNo")] public string? DocumentNo { get; set; }
    [JsonPropertyName("customerCode")] public string? CustomerCode { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("in")] public decimal In { get; set; }
    [JsonPropertyName("out")] public decimal Out { get; set; }
    [JsonPropertyName("balance")] public decimal Balance { get; set; }
    [JsonPropertyName("voided")] public bool Voided { get; set; }
    [JsonPropertyName("reason")] public string? Reason { get; set; }
}

public sealed class StockMovementsResponse
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("opening")] public decimal Opening { get; set; }
    [JsonPropertyName("closing")] public decimal Closing { get; set; }
    [JsonPropertyName("totalIn")] public decimal TotalIn { get; set; }
    [JsonPropertyName("totalOut")] public decimal TotalOut { get; set; }
    [JsonPropertyName("items")] public List<StockMovementRowDto> Items { get; set; } = [];
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
}

public sealed class NativeStockCountLine
{
    [JsonPropertyName("productCode")] public string ProductCode { get; set; } = string.Empty;
    [JsonPropertyName("countedQuantity")] public decimal CountedQuantity { get; set; }
}

/// <summary>Body of POST …/native/stock-counts (GOAL_PANEL_ERPSIZ E6b); the reason is mandatory.</summary>
public sealed class NativeStockCountRequest
{
    [JsonPropertyName("lines")] public List<NativeStockCountLine> Lines { get; set; } = [];
    [JsonPropertyName("reason")] public string? Reason { get; set; }
    [JsonPropertyName("occurredAt")] public string? OccurredAt { get; set; }
    [JsonPropertyName("operationId")] public string? OperationId { get; set; }
}

public sealed class StockItemDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("unit")] public string? Unit { get; set; }
    [JsonPropertyName("mainGroup")] public string? MainGroup { get; set; }
    [JsonPropertyName("subGroup")] public string? SubGroup { get; set; }
    [JsonPropertyName("brand")] public string? Brand { get; set; }
    [JsonPropertyName("shelf")] public string? Shelf { get; set; }
    [JsonPropertyName("barcodes")] public List<string> Barcodes { get; set; } = [];
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("reserved")] public decimal Reserved { get; set; }
    [JsonPropertyName("price")] public decimal? Price { get; set; }
    [JsonPropertyName("lastMovementDate")] public string? LastMovementDate { get; set; }
    [JsonPropertyName("warehouses")] public List<StockWarehouseDto> Warehouses { get; set; } = [];
    [JsonPropertyName("prices")] public List<StockPriceDto> Prices { get; set; } = [];
}

public sealed class StockSummaryDto
{
    [JsonPropertyName("products")] public int Products { get; set; }
    [JsonPropertyName("inStock")] public int InStock { get; set; }
    [JsonPropertyName("outOfStock")] public int OutOfStock { get; set; }
    [JsonPropertyName("negative")] public int Negative { get; set; }
}

public sealed class StockSearchResponse
{
    [JsonPropertyName("items")] public List<StockItemDto> Items { get; set; } = [];
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
    [JsonPropertyName("summary")] public StockSummaryDto Summary { get; set; } = new();
    [JsonPropertyName("priceList")] public int? PriceList { get; set; }
    [JsonPropertyName("warehouseNo")] public int? WarehouseNo { get; set; }
}

public sealed class FacetValueDto
{
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("parent")] public string? Parent { get; set; }
}

public sealed class NamedNumberDto
{
    [JsonPropertyName("number")] public int Number { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

public sealed class StockFacetsResponse
{
    [JsonPropertyName("mainGroups")] public List<FacetValueDto> MainGroups { get; set; } = [];
    [JsonPropertyName("subGroups")] public List<FacetValueDto> SubGroups { get; set; } = [];
    [JsonPropertyName("brands")] public List<FacetValueDto> Brands { get; set; } = [];
    [JsonPropertyName("shelves")] public List<FacetValueDto> Shelves { get; set; } = [];
    [JsonPropertyName("warehouses")] public List<NamedNumberDto> Warehouses { get; set; } = [];
    [JsonPropertyName("priceLists")] public List<NamedNumberDto> PriceLists { get; set; } = [];
    [JsonPropertyName("hasMovementDates")] public bool HasMovementDates { get; set; }
    [JsonPropertyName("hasReserved")] public bool HasReserved { get; set; }
}

// GOAL_PANEL_ERPSIZ E1: ERP-less tenant only (Session.CanEditNativeData) — /api/v1/portal/native/stock-cards.

public sealed class NativeStockCardDetailDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("unit")] public string? Unit { get; set; }
    [JsonPropertyName("vatRate")] public decimal? VatRate { get; set; }
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("brand")] public string? Brand { get; set; }
    [JsonPropertyName("aisle")] public string? Aisle { get; set; }
    [JsonPropertyName("barcodes")] public List<string> Barcodes { get; set; } = [];
    [JsonPropertyName("prices")] public List<StockPriceDto> Prices { get; set; } = [];
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("lastMovementDate")] public string? LastMovementDate { get; set; }
}

/// <summary>Body of a save (create or edit — the code decides which, and never changes on an edit).</summary>
public sealed class NativeStockCardRequest
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("unit")] public string? Unit { get; set; }
    [JsonPropertyName("vatRate")] public decimal? VatRate { get; set; }
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("brand")] public string? Brand { get; set; }
    [JsonPropertyName("aisle")] public string? Aisle { get; set; }
    [JsonPropertyName("barcode")] public string? Barcode { get; set; }
    [JsonPropertyName("price")] public decimal? Price { get; set; }
    [JsonPropertyName("openingQuantity")] public decimal? OpeningQuantity { get; set; }
    [JsonPropertyName("operationId")] public string? OperationId { get; set; }
}

/// <summary>One row of GET /api/v1/portal/movements (GOAL_PANEL_ERPSIZ E4e): any customer-side movement of the company.</summary>
public sealed class CompanyMovementDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("customerTitle")] public string CustomerTitle { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = "other";
    [JsonPropertyName("sourceType")] public string? SourceType { get; set; }
    [JsonPropertyName("documentNo")] public string? DocumentNo { get; set; }
    [JsonPropertyName("documentKey")] public string? DocumentKey { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("paymentType")] public string? PaymentType { get; set; }
    [JsonPropertyName("debit")] public decimal Debit { get; set; }
    [JsonPropertyName("credit")] public decimal Credit { get; set; }
    [JsonPropertyName("userName")] public string? UserName { get; set; }
    [JsonPropertyName("voided")] public bool Voided { get; set; }
    [JsonPropertyName("reason")] public string? Reason { get; set; }
}

public sealed class CompanyMovementsResponse
{
    [JsonPropertyName("items")] public List<CompanyMovementDto> Items { get; set; } = [];
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
    [JsonPropertyName("totalDebit")] public decimal TotalDebit { get; set; }
    [JsonPropertyName("totalCredit")] public decimal TotalCredit { get; set; }
}

public sealed class NativeJobResultDto
{
    [JsonPropertyName("jobId")] public Guid JobId { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
}

/// <summary>Body of a collection or disbursement — same shape for both; the call decides the route.</summary>
public sealed class NativePaymentRequest
{
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("amount")] public decimal? Amount { get; set; }
    [JsonPropertyName("paymentType")] public string? PaymentType { get; set; }
    [JsonPropertyName("occurredAt")] public string? OccurredAt { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("operationId")] public string? OperationId { get; set; }
}

/// <summary>Body of POST …/ledger/{key}/void (GOAL_PANEL_ERPSIZ E4a).</summary>
public sealed class NativeLedgerVoidRequest
{
    [JsonPropertyName("reason")] public string? Reason { get; set; }
    [JsonPropertyName("operationId")] public string? OperationId { get; set; }
}

/// <summary>Body of POST …/ledger/{key}/edit (GOAL_PANEL_ERPSIZ E4c) — void of the target plus a
/// corrected re-booking of the same kind, in one transaction (D11).</summary>
public sealed class NativeLedgerEditRequest
{
    [JsonPropertyName("amount")] public decimal? Amount { get; set; }

    /// <summary>Only meaningful for a manual adjustment; a collection/disbursement keeps its own direction.</summary>
    [JsonPropertyName("debit")] public bool? Debit { get; set; }
    [JsonPropertyName("paymentType")] public string? PaymentType { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }

    /// <summary>The corrected adjustment's reason; required when the target is a manual adjustment.</summary>
    [JsonPropertyName("reason")] public string? Reason { get; set; }
    [JsonPropertyName("occurredAt")] public string? OccurredAt { get; set; }

    /// <summary>Why the original is being corrected — mandatory, the same as a plain void's.</summary>
    [JsonPropertyName("voidReason")] public string? VoidReason { get; set; }
    [JsonPropertyName("operationId")] public string? OperationId { get; set; }
}

/// <summary>Body of POST …/ledger-adjustments (GOAL_PANEL_ERPSIZ E4b) — a manual correction of a
/// customer's balance; <see cref="Reason"/> is mandatory, unlike a payment's.</summary>
public sealed class NativeLedgerAdjustmentRequest
{
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("amount")] public decimal? Amount { get; set; }

    /// <summary>True increases what the customer owes (borç), false decreases it (alacak).</summary>
    [JsonPropertyName("debit")] public bool Debit { get; set; }
    [JsonPropertyName("reason")] public string? Reason { get; set; }
    [JsonPropertyName("occurredAt")] public string? OccurredAt { get; set; }
    [JsonPropertyName("operationId")] public string? OperationId { get; set; }
}

/// <summary>Body of a customer save (create or edit — the code decides which, and never changes on an edit).</summary>
public sealed class NativeCustomerCardRequest
{
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("taxNo")] public string? TaxNo { get; set; }
    [JsonPropertyName("taxOffice")] public string? TaxOffice { get; set; }
    [JsonPropertyName("phone")] public string? Phone { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("regionCode")] public string? RegionCode { get; set; }
    [JsonPropertyName("openingBalance")] public decimal? OpeningBalance { get; set; }
    [JsonPropertyName("operationId")] public string? OperationId { get; set; }
}

public sealed class ApiErrorDto
{
    [JsonPropertyName("errorCode")] public string? ErrorCode { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
}

public sealed class CustomerRowDto
{
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("balance")] public decimal Balance { get; set; }
    [JsonPropertyName("phone")] public string? Phone { get; set; }
    [JsonPropertyName("city")] public string? City { get; set; }
}

public sealed class CustomersResponse
{
    [JsonPropertyName("items")] public List<CustomerRowDto> Items { get; set; } = [];
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
    [JsonPropertyName("totalReceivable")] public decimal TotalReceivable { get; set; }
    [JsonPropertyName("totalPayable")] public decimal TotalPayable { get; set; }
}

public sealed class CustomerCardDto
{
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("balance")] public decimal Balance { get; set; }
    [JsonPropertyName("phone")] public string? Phone { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("taxOffice")] public string? TaxOffice { get; set; }
    [JsonPropertyName("taxNo")] public string? TaxNo { get; set; }
    [JsonPropertyName("address")] public string? Address { get; set; }
    [JsonPropertyName("salespersonCode")] public string? SalespersonCode { get; set; }
    [JsonPropertyName("regionCode")] public string? RegionCode { get; set; }
    [JsonPropertyName("groupCode")] public string? GroupCode { get; set; }
    [JsonPropertyName("currency")] public string? Currency { get; set; }
    [JsonPropertyName("isLocked")] public bool IsLocked { get; set; }
    [JsonPropertyName("dataSource")] public string DataSource { get; set; } = string.Empty;
}

public sealed class LedgerRowDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = "other";
    [JsonPropertyName("sourceType")] public string? SourceType { get; set; }
    [JsonPropertyName("documentNo")] public string? DocumentNo { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("debit")] public decimal Debit { get; set; }
    [JsonPropertyName("credit")] public decimal Credit { get; set; }
    [JsonPropertyName("balance")] public decimal Balance { get; set; }
    [JsonPropertyName("documentKey")] public string? DocumentKey { get; set; }

    /// <summary>GOAL_PANEL_ERPSIZ E4d/E4e.</summary>
    [JsonPropertyName("voided")] public bool Voided { get; set; }
    [JsonPropertyName("voidedBy")] public string? VoidedBy { get; set; }
    [JsonPropertyName("voidedAt")] public string? VoidedAt { get; set; }
    [JsonPropertyName("reason")] public string? Reason { get; set; }
    [JsonPropertyName("editable")] public bool Editable { get; set; }
}

public sealed class LedgerResponse
{
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("from")] public string? From { get; set; }
    [JsonPropertyName("to")] public string? To { get; set; }
    [JsonPropertyName("opening")] public decimal Opening { get; set; }
    [JsonPropertyName("closing")] public decimal Closing { get; set; }
    [JsonPropertyName("totalDebit")] public decimal TotalDebit { get; set; }
    [JsonPropertyName("totalCredit")] public decimal TotalCredit { get; set; }
    [JsonPropertyName("items")] public List<LedgerRowDto> Items { get; set; } = [];
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
}

/// <summary>One row of GET /api/v1/portal/native/audit (GOAL_PANEL_ERPSIZ E7b) — who changed a card/payment, and when.</summary>
public sealed class AuditRowDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("entity")] public string Entity { get; set; } = string.Empty;
    [JsonPropertyName("entityKey")] public string EntityKey { get; set; } = string.Empty;
    [JsonPropertyName("action")] public string Action { get; set; } = string.Empty;
    [JsonPropertyName("summary")] public string Summary { get; set; } = string.Empty;
    [JsonPropertyName("beforeJson")] public string? BeforeJson { get; set; }
    [JsonPropertyName("afterJson")] public string? AfterJson { get; set; }
    [JsonPropertyName("userId")] public Guid UserId { get; set; }
    [JsonPropertyName("userName")] public string UserName { get; set; } = string.Empty;
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class AuditResponse
{
    [JsonPropertyName("items")] public List<AuditRowDto> Items { get; set; } = [];
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
}

/// <summary>One row of GET /api/v1/portal/payments (GOAL_PANEL_ERPSIZ E3c) — a collection or payment, any customer.</summary>
public sealed class PaymentRowDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("customerTitle")] public string CustomerTitle { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("paymentType")] public string? PaymentType { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("debit")] public decimal Debit { get; set; }
    [JsonPropertyName("credit")] public decimal Credit { get; set; }
    [JsonPropertyName("userId")] public Guid? UserId { get; set; }
    [JsonPropertyName("userName")] public string? UserName { get; set; }
    [JsonPropertyName("documentKey")] public string? DocumentKey { get; set; }
}

/// <summary>One group's total (a calendar day or a payment type) in a payments summary.</summary>
public sealed class PaymentGroupTotalDto
{
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
    [JsonPropertyName("debit")] public decimal Debit { get; set; }
    [JsonPropertyName("credit")] public decimal Credit { get; set; }
}

public sealed class PaymentsResponse
{
    [JsonPropertyName("from")] public string From { get; set; } = string.Empty;
    [JsonPropertyName("to")] public string To { get; set; } = string.Empty;
    [JsonPropertyName("items")] public List<PaymentRowDto> Items { get; set; } = [];
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
    [JsonPropertyName("totalDebit")] public decimal TotalDebit { get; set; }
    [JsonPropertyName("totalCredit")] public decimal TotalCredit { get; set; }
    [JsonPropertyName("dailyTotals")] public List<PaymentGroupTotalDto> DailyTotals { get; set; } = [];
    [JsonPropertyName("paymentTypeTotals")] public List<PaymentGroupTotalDto> PaymentTypeTotals { get; set; } = [];
}

public sealed class CustomerDocumentLineDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("unitPrice")] public decimal UnitPrice { get; set; }
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("tax")] public decimal? Tax { get; set; }
    [JsonPropertyName("warehouseNo")] public int? WarehouseNo { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
}

public sealed class CustomerDocumentDto
{
    [JsonPropertyName("documentKey")] public string DocumentKey { get; set; } = string.Empty;
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = "other";
    [JsonPropertyName("documentNo")] public string? DocumentNo { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("lines")] public List<CustomerDocumentLineDto> Lines { get; set; } = [];
    [JsonPropertyName("linesAvailable")] public bool LinesAvailable { get; set; }
}

// ---- satış / alış / iade evrakları (GOAL_PANEL_ERPSIZ E5e) --------------------------------

/// <summary>One row of GET …/native/documents — a sale/purchase/return invoice, any customer/supplier.</summary>
public sealed class NativeDocumentRowDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("documentKey")] public string DocumentKey { get; set; } = string.Empty;

    /// <summary>sale, sale_return, purchase, purchase_return.</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("documentNo")] public string? DocumentNo { get; set; }
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("customerTitle")] public string CustomerTitle { get; set; } = string.Empty;
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("userId")] public Guid? UserId { get; set; }
    [JsonPropertyName("userName")] public string? UserName { get; set; }
    [JsonPropertyName("voided")] public bool Voided { get; set; }
}

public sealed class NativeDocumentsResponse
{
    [JsonPropertyName("from")] public string From { get; set; } = string.Empty;
    [JsonPropertyName("to")] public string To { get; set; } = string.Empty;
    [JsonPropertyName("items")] public List<NativeDocumentRowDto> Items { get; set; } = [];
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
}

/// <summary>GET …/native/documents/{key} — one invoice with its lines.</summary>
public sealed class NativeDocumentDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("documentKey")] public string DocumentKey { get; set; } = string.Empty;
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("customerTitle")] public string CustomerTitle { get; set; } = string.Empty;
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("documentNo")] public string? DocumentNo { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("lines")] public List<CustomerDocumentLineDto> Lines { get; set; } = [];
    [JsonPropertyName("linesAvailable")] public bool LinesAvailable { get; set; }
    [JsonPropertyName("voided")] public bool Voided { get; set; }
    [JsonPropertyName("paymentType")] public string? PaymentType { get; set; }
}

public sealed class NativeDocumentLineRequest
{
    [JsonPropertyName("productCode")] public string ProductCode { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("unitPrice")] public decimal UnitPrice { get; set; }

    /// <summary>Set only for a line discount; the server defaults it to quantity × unit price.</summary>
    [JsonPropertyName("lineTotal")] public decimal? LineTotal { get; set; }
}

/// <summary>Body of POST …/native/sales-orders, …/purchase-receipts, …/sales-returns (E5a) and, with
/// <see cref="VoidReason"/>, of POST …/native/documents/{key}/edit (E5d).</summary>
public sealed class NativeDocumentRequest
{
    [JsonPropertyName("partyCode")] public string PartyCode { get; set; } = string.Empty;
    [JsonPropertyName("lines")] public List<NativeDocumentLineRequest> Lines { get; set; } = [];

    /// <summary>Only for a purchase without lines; otherwise the lines' own total.</summary>
    [JsonPropertyName("amount")] public decimal? Amount { get; set; }

    /// <summary>Null for an open-account document; a payment type settles it on the spot.</summary>
    [JsonPropertyName("paymentType")] public string? PaymentType { get; set; }
    [JsonPropertyName("occurredAt")] public string? OccurredAt { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("documentNo")] public string? DocumentNo { get; set; }

    [JsonPropertyName("voidReason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? VoidReason { get; set; }

    [JsonPropertyName("operationId")] public string? OperationId { get; set; }
}

// ---- warehouse reports (Faz 50, plan step 8) ----------------------------------------------

/// <summary>An order's times measured from its history; durations in seconds.</summary>
public sealed class FulfillmentTimesDto
{
    [JsonPropertyName("waitSeconds")] public long? WaitSeconds { get; set; }
    [JsonPropertyName("netPreparationSeconds")] public long NetPreparationSeconds { get; set; }
    [JsonPropertyName("untilLoadingSeconds")] public long? UntilLoadingSeconds { get; set; }
    [JsonPropertyName("firstStartedAtUtc")] public DateTimeOffset? FirstStartedAtUtc { get; set; }
    [JsonPropertyName("startedByName")] public string? StartedByName { get; set; }
    [JsonPropertyName("packedAtUtc")] public DateTimeOffset? PackedAtUtc { get; set; }
    [JsonPropertyName("packedByName")] public string? PackedByName { get; set; }
    [JsonPropertyName("loadedAtUtc")] public DateTimeOffset? LoadedAtUtc { get; set; }
}

public sealed class WarehouseDashboardDto
{
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("enabled")] public bool Enabled { get; set; }
    [JsonPropertyName("pending")] public int Pending { get; set; }
    [JsonPropertyName("preparing")] public int Preparing { get; set; }
    [JsonPropertyName("packed")] public int Packed { get; set; }
    [JsonPropertyName("late")] public int Late { get; set; }
    [JsonPropertyName("critical")] public int Critical { get; set; }
    [JsonPropertyName("queuedOnDay")] public int QueuedOnDay { get; set; }
    [JsonPropertyName("packedOnDay")] public int PackedOnDay { get; set; }
    [JsonPropertyName("loadedOnDay")] public int LoadedOnDay { get; set; }
    [JsonPropertyName("averageWaitSeconds")] public long? AverageWaitSeconds { get; set; }
    [JsonPropertyName("averageNetPreparationSeconds")] public long? AverageNetPreparationSeconds { get; set; }
}

public sealed class WarehouseStaffDto
{
    [JsonPropertyName("userId")] public Guid? UserId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("packedCount")] public int PackedCount { get; set; }
    [JsonPropertyName("lineCount")] public int LineCount { get; set; }
    [JsonPropertyName("itemQuantity")] public decimal ItemQuantity { get; set; }
    [JsonPropertyName("totalNetPreparationSeconds")] public long TotalNetPreparationSeconds { get; set; }
    [JsonPropertyName("averageNetPreparationSeconds")] public long AverageNetPreparationSeconds { get; set; }
    [JsonPropertyName("medianNetPreparationSeconds")] public long MedianNetPreparationSeconds { get; set; }
    [JsonPropertyName("secondsPerLine")] public long? SecondsPerLine { get; set; }
}

public sealed class WarehouseDayDto
{
    [JsonPropertyName("date")] public string Date { get; set; } = string.Empty;
    [JsonPropertyName("queued")] public int Queued { get; set; }
    [JsonPropertyName("packed")] public int Packed { get; set; }
    [JsonPropertyName("averageWaitSeconds")] public long? AverageWaitSeconds { get; set; }
    [JsonPropertyName("averageNetPreparationSeconds")] public long? AverageNetPreparationSeconds { get; set; }
}

public sealed class WarehouseSlowOrderDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("orderNo")] public string OrderNo { get; set; } = string.Empty;
    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("lineCount")] public int LineCount { get; set; }
    [JsonPropertyName("queuedAtUtc")] public DateTimeOffset QueuedAtUtc { get; set; }
    [JsonPropertyName("times")] public FulfillmentTimesDto Times { get; set; } = new();
}

public sealed class WarehousePerformanceDto
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
    [JsonPropertyName("staff")] public List<WarehouseStaffDto> Staff { get; set; } = [];
    [JsonPropertyName("days")] public List<WarehouseDayDto> Days { get; set; } = [];
    [JsonPropertyName("longestWaits")] public List<WarehouseSlowOrderDto> LongestWaits { get; set; } = [];
    [JsonPropertyName("longestPreparations")] public List<WarehouseSlowOrderDto> LongestPreparations { get; set; } = [];
}
