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
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
    [JsonPropertyName("lastLoginAtUtc")] public DateTimeOffset? LastLoginAtUtc { get; set; }
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
    [JsonPropertyName("role")] public string? Role { get; set; }
}

/// <summary>Partial update; omitted fields stay unchanged.</summary>
public sealed class UpdateMobileUserRequest
{
    [JsonPropertyName("fullName")] public string? FullName { get; set; }
    [JsonPropertyName("password")] public string? Password { get; set; }
    [JsonPropertyName("role")] public string? Role { get; set; }
    [JsonPropertyName("isActive")] public bool? IsActive { get; set; }
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

/// <summary>PUT /api/v1/admin/tenants/{id}/mobile/data-source body.</summary>
public sealed class SetDataSourceRequest
{
    [JsonPropertyName("dataSource")] public string? DataSource { get; set; }
}
