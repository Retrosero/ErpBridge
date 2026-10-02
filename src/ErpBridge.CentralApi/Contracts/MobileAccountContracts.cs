using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

/// <summary>POST /api/v1/android/account/login body.</summary>
public sealed class MobileLoginRequest
{
    [JsonPropertyName("tenantCode")] public string? TenantCode { get; set; }
    [JsonPropertyName("username")] public string? Username { get; set; }
    [JsonPropertyName("password")] public string? Password { get; set; }
    [JsonPropertyName("deviceId")] public string? DeviceId { get; set; }
    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }

    /// <summary><c>android</c> (default, what phones send by omitting it) or <c>portal</c>.</summary>
    [JsonPropertyName("client")] public string? Client { get; set; }

    /// <summary>
    /// Portal only: <c>true</c> keeps the browser signed in for 30 days, otherwise the session
    /// lasts a workday (12 hours). A phone always gets 30 days — it works offline for days.
    /// </summary>
    [JsonPropertyName("rememberMe")] public bool? RememberMe { get; set; }
}

/// <summary>Successful sign-in: a bearer token plus the session the app shows.</summary>
public sealed class MobileLoginResponse
{
    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset ExpiresAtUtc { get; set; }
    [JsonPropertyName("session")] public MobileSessionDto Session { get; set; } = new();
}

/// <summary>Who is signed in, to which tenant, and how many seats are left.</summary>
public sealed class MobileSessionDto
{
    [JsonPropertyName("user")] public MobileUserDto User { get; set; } = new();
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("tenantName")] public string TenantName { get; set; } = string.Empty;
    [JsonPropertyName("tenantCode")] public string? TenantCode { get; set; }
    [JsonPropertyName("seats")] public SeatUsageDto Seats { get; set; } = new();

    /// <summary><c>erp</c> (data comes from the company's ERP) or <c>native</c> (the phones create it).</summary>
    [JsonPropertyName("dataSource")] public string DataSource { get; set; } = "erp";

    /// <summary>
    /// How the phone reads its data: <c>tables</c> (per-table <c>/sync/…</c> endpoints) or <c>feed</c>
    /// (the change feed, <c>POST /sync/pull</c>). Set per company by an operator; independent of
    /// <see cref="DataSource"/>. A change also changes <see cref="PermissionsStamp"/>, so a signed-in
    /// phone re-reads <c>/me</c> at its next call.
    /// </summary>
    [JsonPropertyName("syncMode")] public string SyncMode { get; set; } = "tables";

    /// <summary>Operations that go to the approval centre, keyed by approval kind.</summary>
    [JsonPropertyName("approvalRules")] public Dictionary<string, bool> ApprovalRules { get; set; } = new();

    /// <summary>Sellable add-ons the company bought (<c>xml_import</c>…), sorted, lowercase; empty when none.</summary>
    [JsonPropertyName("modules")] public string[] Modules { get; set; } = [];

    /// <summary>The user's yes/no permissions (GOAL_YETKILER), every catalogue key. Absent from older servers.</summary>
    [JsonPropertyName("permissions")] public Dictionary<string, bool> Permissions { get; set; } = new();

    /// <summary>The user's limits; <c>null</c> = unlimited.</summary>
    [JsonPropertyName("limits")] public Dictionary<string, decimal?> Limits { get; set; } = new();

    /// <summary>The catalogue version the maps follow; 0 (absent) = a server without permissions.</summary>
    [JsonPropertyName("permissionsVersion")] public int PermissionsVersion { get; set; }

    /// <summary>
    /// Fingerprint of the roles and permissions above and of <see cref="SyncMode"/>; every signed-in response carries the current one in the
    /// <c>X-Permissions-Stamp</c> header, and a phone that sees another one re-reads <c>/me</c>.
    /// </summary>
    [JsonPropertyName("permissionsStamp")] public string? PermissionsStamp { get; set; }
}

/// <summary>Seat capacity and subscription state of a tenant.</summary>
public sealed class SeatUsageDto
{
    /// <summary>Paid seats; 0 when the tenant has no subscription.</summary>
    [JsonPropertyName("max")] public int Max { get; set; }

    /// <summary>Active, non-deleted users, administrators included.</summary>
    [JsonPropertyName("used")] public int Used { get; set; }

    [JsonPropertyName("endsAtUtc")] public DateTimeOffset? EndsAtUtc { get; set; }

    /// <summary><c>active</c>, <c>grace</c>, <c>expired</c> or <c>none</c>.</summary>
    [JsonPropertyName("status")] public string Status { get; set; } = "none";
}

/// <summary>A mobile user as shown to tenant and console administrators. Never carries the hash.</summary>
public sealed class MobileUserDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;
    /// <summary>ADMIN, MANAGER or SALES — the one role an app built before multi-role accounts understands.</summary>
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;

    /// <summary>Every role: ADMIN, MANAGER, ACCOUNTING, WAREHOUSE, SALES (precedence order).</summary>
    [JsonPropertyName("roles")] public string[] Roles { get; set; } = [];

    /// <summary>Effective right to approve: always true for an administrator.</summary>
    [JsonPropertyName("canApprove")] public bool CanApprove { get; set; }

    /// <summary>Effective right to change the approval rules: always true for an administrator.</summary>
    [JsonPropertyName("canManageApprovalRules")] public bool CanManageApprovalRules { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
    [JsonPropertyName("lastLoginAtUtc")] public DateTimeOffset? LastLoginAtUtc { get; set; }

    /// <summary>How many permissions are set for this person apart from their roles (a badge in the users list).</summary>
    [JsonPropertyName("permissionOverrideCount")] public int PermissionOverrideCount { get; set; }

    /// <summary>
    /// The person's Mikro salesperson code (<c>MobileUserErpMapping.SalespersonCode</c>), in the users list only; null when
    /// none is mapped. The panel warns that a customer's catalog request cannot reach a salesperson without one (S11).
    /// </summary>
    [JsonPropertyName("salespersonCode")] public string? SalespersonCode { get; set; }
}

/// <summary>User list with the seat state that decides whether another one fits.</summary>
public sealed class MobileUserListResponse
{
    [JsonPropertyName("seats")] public SeatUsageDto Seats { get; set; } = new();
    [JsonPropertyName("users")] public MobileUserDto[] Users { get; set; } = Array.Empty<MobileUserDto>();
}

/// <summary>Create a mobile user (tenant admin on the phone, or console admin).</summary>
public sealed class CreateMobileUserRequest
{
    [JsonPropertyName("username")] public string? Username { get; set; }
    [JsonPropertyName("fullName")] public string? FullName { get; set; }
    [JsonPropertyName("password")] public string? Password { get; set; }

    /// <summary>A single role (older callers). Ignored when <see cref="Roles"/> is given.</summary>
    [JsonPropertyName("role")] public string? Role { get; set; }

    /// <summary>Every role the user gets; at least one.</summary>
    [JsonPropertyName("roles")] public string[]? Roles { get; set; }

    /// <summary>For a manager only; ignored for other roles.</summary>
    [JsonPropertyName("canApprove")] public bool? CanApprove { get; set; }

    /// <summary>For a manager only; ignored for other roles.</summary>
    [JsonPropertyName("canManageApprovalRules")] public bool? CanManageApprovalRules { get; set; }
}

/// <summary>Partial update; omitted fields stay unchanged.</summary>
public sealed class UpdateMobileUserRequest
{
    [JsonPropertyName("fullName")] public string? FullName { get; set; }
    [JsonPropertyName("password")] public string? Password { get; set; }
    /// <summary>
    /// Replaces the user's field/management role (ADMIN, MANAGER or SALES) and keeps ACCOUNTING and
    /// WAREHOUSE, so an older screen that knows only one role does not strip the others.
    /// Ignored when <see cref="Roles"/> is given.
    /// </summary>
    [JsonPropertyName("role")] public string? Role { get; set; }

    /// <summary>Replaces every role; at least one.</summary>
    [JsonPropertyName("roles")] public string[]? Roles { get; set; }
    [JsonPropertyName("isActive")] public bool? IsActive { get; set; }

    /// <summary>For a manager only; a user who stops being a manager loses both rights.</summary>
    [JsonPropertyName("canApprove")] public bool? CanApprove { get; set; }

    /// <summary>For a manager only; a user who stops being a manager loses both rights.</summary>
    [JsonPropertyName("canManageApprovalRules")] public bool? CanManageApprovalRules { get; set; }
}

/// <summary>PUT /api/v1/admin/tenants/{id}/subscription body — records a new seat purchase.</summary>
public sealed class SetSubscriptionRequest
{
    [JsonPropertyName("seats")] public int Seats { get; set; }
    [JsonPropertyName("endsAtUtc")] public DateTimeOffset? EndsAtUtc { get; set; }
    [JsonPropertyName("source")] public string? Source { get; set; }
    [JsonPropertyName("reference")] public string? Reference { get; set; }
    [JsonPropertyName("note")] public string? Note { get; set; }
}

/// <summary>One subscription row, current or historical.</summary>
public sealed class SubscriptionDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("seats")] public int Seats { get; set; }
    [JsonPropertyName("startsAtUtc")] public DateTimeOffset StartsAtUtc { get; set; }
    [JsonPropertyName("endsAtUtc")] public DateTimeOffset? EndsAtUtc { get; set; }
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;
    [JsonPropertyName("reference")] public string? Reference { get; set; }
    [JsonPropertyName("note")] public string? Note { get; set; }
    [JsonPropertyName("isCurrent")] public bool IsCurrent { get; set; }
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
    [JsonPropertyName("createdByAdminId")] public Guid? CreatedByAdminId { get; set; }
}

/// <summary>A phone that signed in to the tenant.</summary>
public sealed class MobileDeviceDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("deviceId")] public string DeviceId { get; set; } = string.Empty;
    [JsonPropertyName("lastUserId")] public Guid? LastUserId { get; set; }
    [JsonPropertyName("lastUsername")] public string? LastUsername { get; set; }
    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("firstSeenAtUtc")] public DateTimeOffset FirstSeenAtUtc { get; set; }
    [JsonPropertyName("lastSeenAtUtc")] public DateTimeOffset LastSeenAtUtc { get; set; }
}

/// <summary>Everything support needs about a tenant's mobile side, on one call.</summary>
public sealed class TenantMobileOverviewResponse
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("tenantCode")] public string? TenantCode { get; set; }
    [JsonPropertyName("dataSource")] public string DataSource { get; set; } = "erp";
    [JsonPropertyName("syncMode")] public string SyncMode { get; set; } = "tables";
    [JsonPropertyName("approvalRules")] public ApprovalRulesDto ApprovalRules { get; set; } = new();
    [JsonPropertyName("modules")] public string[] Modules { get; set; } = [];
    [JsonPropertyName("seats")] public SeatUsageDto Seats { get; set; } = new();
    [JsonPropertyName("subscriptions")] public SubscriptionDto[] Subscriptions { get; set; } = Array.Empty<SubscriptionDto>();
    [JsonPropertyName("users")] public MobileUserDto[] Users { get; set; } = Array.Empty<MobileUserDto>();
    [JsonPropertyName("devices")] public MobileDeviceDto[] Devices { get; set; } = Array.Empty<MobileDeviceDto>();
}

/// <summary>PATCH body for a device: revoke or restore.</summary>
public sealed class UpdateMobileDeviceRequest
{
    [JsonPropertyName("isActive")] public bool? IsActive { get; set; }
}

/// <summary>PUT /api/v1/admin/tenants/{id}/mobile/modules body: the complete set of add-ons.</summary>
public sealed class SetTenantModulesRequest
{
    [JsonPropertyName("modules")] public string[]? Modules { get; set; }
}

/// <summary>PUT /api/v1/admin/tenants/{id}/mobile/data-source body.</summary>
public sealed class SetDataSourceRequest
{
    [JsonPropertyName("dataSource")] public string? DataSource { get; set; }
}

/// <summary>PUT /api/v1/admin/tenants/{id}/mobile/sync-mode body: <c>tables</c> or <c>feed</c>.</summary>
public sealed class SetSyncModeRequest
{
    [JsonPropertyName("syncMode")] public string? SyncMode { get; set; }
}
