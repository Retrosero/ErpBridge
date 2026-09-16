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
}

/// <summary>One request with the documents it would post, its history and stock warnings.</summary>
public sealed class ApprovalDetailDto
{
    [JsonPropertyName("request")] public ApprovalDto Request { get; set; } = new();
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

public sealed class StockWarningDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("requested")] public decimal Requested { get; set; }
    [JsonPropertyName("onHand")] public decimal OnHand { get; set; }
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

public sealed class StockRow
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
}

public sealed class StockResponse
{
    [JsonPropertyName("rows")] public List<StockRow> Rows { get; set; } = [];
    [JsonPropertyName("truncated")] public bool Truncated { get; set; }
}

public sealed class ApiErrorDto
{
    [JsonPropertyName("errorCode")] public string? ErrorCode { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
}
