using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

// Kullanıcı yetkileri (GOAL_YETKILER): /api/v1/android/account/permissions…, shared by the phone and the portal.
// Values use the storage encoding: yes/no "1"/"0"; a limit as an invariant number, "" = unlimited.

public sealed class PermissionCatalogResponse
{
    [JsonPropertyName("version")] public int Version { get; set; }

    [JsonPropertyName("groups")] public PermissionGroupDto[] Groups { get; set; } = [];

    [JsonPropertyName("items")] public PermissionItemDto[] Items { get; set; } = [];

    /// <summary>Roles a company may change; ADMIN always has everything.</summary>
    [JsonPropertyName("editableRoles")] public string[] EditableRoles { get; set; } = [];
}

public sealed class PermissionGroupDto
{
    /// <summary><c>modules</c>, <c>portal</c>, <c>actions</c>, <c>visibility</c> or <c>limits</c>.</summary>
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;

    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
}

public sealed class PermissionItemDto
{
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;

    [JsonPropertyName("group")] public string Group { get; set; } = string.Empty;

    /// <summary><c>bool</c> or <c>limit</c>.</summary>
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;

    /// <summary><c>%</c> or <c>TL</c> for a limit.</summary>
    [JsonPropertyName("unit")] public string? Unit { get; set; }

    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;

    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

    /// <summary>The server checks it too; otherwise only the apps hide or refuse it.</summary>
    [JsonPropertyName("serverEnforced")] public bool ServerEnforced { get; set; }

    /// <summary>Not editable in role templates or personal overrides (e.g. user management is admin-only).</summary>
    [JsonPropertyName("locked")] public bool Locked { get; set; }

    /// <summary>Catalogue default per role.</summary>
    [JsonPropertyName("defaults")] public Dictionary<string, string> Defaults { get; set; } = new();
}

/// <summary>Every role's template: ADMIN locked at everything, the others as the company set them.</summary>
public sealed class RolePermissionsResponse
{
    [JsonPropertyName("roles")] public RolePermissionsDto[] Roles { get; set; } = [];
}

public sealed class RolePermissionsDto
{
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;

    /// <summary>ADMIN: always everything, not editable.</summary>
    [JsonPropertyName("locked")] public bool Locked { get; set; }

    /// <summary>The role's value of every key (the company's, or the catalogue default).</summary>
    [JsonPropertyName("values")] public Dictionary<string, string> Values { get; set; } = new();

    /// <summary>Keys the company changed from the catalogue default.</summary>
    [JsonPropertyName("customized")] public string[] Customized { get; set; } = [];
}

/// <summary>
/// A patch of a role's template: <c>key → value</c>, values as strings (<c>"1"</c>/<c>"0"</c>, a limit such as
/// <c>"10"</c>, <c>"unlimited"</c>); <c>null</c> = back to the catalogue default. Keys not named are unchanged.
/// </summary>
public sealed class UpdateRolePermissionsRequest
{
    [JsonPropertyName("values")] public Dictionary<string, string?>? Values { get; set; }
}

public sealed class UserPermissionsResponse
{
    [JsonPropertyName("userId")] public Guid UserId { get; set; }

    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;

    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("roles")] public string[] Roles { get; set; } = [];

    /// <summary>An administrator: everything, no overrides possible.</summary>
    [JsonPropertyName("locked")] public bool Locked { get; set; }

    [JsonPropertyName("items")] public UserPermissionItemDto[] Items { get; set; } = [];
}

public sealed class UserPermissionItemDto
{
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;

    /// <summary>The effective value.</summary>
    [JsonPropertyName("value")] public string Value { get; set; } = string.Empty;

    /// <summary><c>admin</c>, <c>role</c> or <c>override</c>.</summary>
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;

    /// <summary>What the roles alone give.</summary>
    [JsonPropertyName("roleValue")] public string RoleValue { get; set; } = string.Empty;

    [JsonPropertyName("roleSources")] public string[] RoleSources { get; set; } = [];

    /// <summary>The personal value; null = inherited from the roles.</summary>
    [JsonPropertyName("override")] public string? Override { get; set; }
}

/// <summary>
/// A patch of one person's overrides: <c>key → "allow" | "deny" | "1" | "0" | limit | "unlimited"</c>, <c>null</c> =
/// inherit from the roles. For a manager, the approval rights write the two approval switches.
/// </summary>
public sealed class UpdateUserPermissionsRequest
{
    [JsonPropertyName("overrides")] public Dictionary<string, string?>? Overrides { get; set; }
}

public sealed class PermissionChangeDto
{
    [JsonPropertyName("id")] public long Id { get; set; }

    [JsonPropertyName("actorName")] public string ActorName { get; set; } = string.Empty;

    [JsonPropertyName("client")] public string Client { get; set; } = string.Empty;

    /// <summary><c>role</c>, <c>user</c> or <c>roles</c>.</summary>
    [JsonPropertyName("scope")] public string Scope { get; set; } = string.Empty;

    [JsonPropertyName("role")] public string? Role { get; set; }

    [JsonPropertyName("targetUserId")] public Guid? TargetUserId { get; set; }

    [JsonPropertyName("targetUserName")] public string? TargetUserName { get; set; }

    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;

    [JsonPropertyName("oldValue")] public string? OldValue { get; set; }

    [JsonPropertyName("newValue")] public string? NewValue { get; set; }

    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class PermissionChangesResponse
{
    [JsonPropertyName("changes")] public PermissionChangeDto[] Changes { get; set; } = [];
}
