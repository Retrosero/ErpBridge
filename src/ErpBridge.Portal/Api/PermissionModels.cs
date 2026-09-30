using System.Globalization;
using System.Text.Json.Serialization;

namespace ErpBridge.Portal.Api;

// Kullanıcı yetkileri (GOAL_YETKILER): mirrors the central API's PermissionContracts. Values are strings: yes/no
// "1"/"0", a limit as an invariant number, "" = unlimited.

public sealed class PermissionCatalogDto
{
    [JsonPropertyName("version")] public int Version { get; set; }
    [JsonPropertyName("groups")] public PermissionGroupDto[] Groups { get; set; } = [];
    [JsonPropertyName("items")] public PermissionItemDto[] Items { get; set; } = [];
    [JsonPropertyName("editableRoles")] public string[] EditableRoles { get; set; } = [];
}

public sealed class PermissionGroupDto
{
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
}

public sealed class PermissionItemDto
{
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
    [JsonPropertyName("group")] public string Group { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = "bool";
    [JsonPropertyName("unit")] public string? Unit { get; set; }
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("serverEnforced")] public bool ServerEnforced { get; set; }
    [JsonPropertyName("locked")] public bool Locked { get; set; }
    [JsonPropertyName("defaults")] public Dictionary<string, string> Defaults { get; set; } = new();

    public bool IsLimit => Type == "limit";
}

public sealed class RolePermissionsResponse
{
    [JsonPropertyName("roles")] public RolePermissionsDto[] Roles { get; set; } = [];
}

public sealed class RolePermissionsDto
{
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;
    [JsonPropertyName("locked")] public bool Locked { get; set; }
    [JsonPropertyName("values")] public Dictionary<string, string> Values { get; set; } = new();
    [JsonPropertyName("customized")] public string[] Customized { get; set; } = [];
}

public sealed class UpdateRolePermissionsRequest
{
    [JsonPropertyName("values")] public Dictionary<string, string?> Values { get; set; } = new();
}

public sealed class UserPermissionsDto
{
    [JsonPropertyName("userId")] public Guid UserId { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("roles")] public string[] Roles { get; set; } = [];
    [JsonPropertyName("locked")] public bool Locked { get; set; }
    [JsonPropertyName("items")] public UserPermissionItemDto[] Items { get; set; } = [];
}

public sealed class UserPermissionItemDto
{
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
    [JsonPropertyName("value")] public string Value { get; set; } = string.Empty;

    /// <summary><c>admin</c>, <c>role</c> or <c>override</c>.</summary>
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;
    [JsonPropertyName("roleValue")] public string RoleValue { get; set; } = string.Empty;
    [JsonPropertyName("roleSources")] public string[] RoleSources { get; set; } = [];
    [JsonPropertyName("override")] public string? Override { get; set; }
}

public sealed class UpdateUserPermissionsRequest
{
    [JsonPropertyName("overrides")] public Dictionary<string, string?> Overrides { get; set; } = new();
}

public sealed class PermissionChangeDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("actorName")] public string ActorName { get; set; } = string.Empty;
    [JsonPropertyName("client")] public string Client { get; set; } = string.Empty;
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

/// <summary>How a permission value reads in the portal.</summary>
public static class PermissionText
{
    public static string Value(PermissionItemDto? item, string? value)
    {
        if (value is null) return "—";
        if (item?.IsLimit != true) return value == "1" ? "Açık" : "Kapalı";
        if (value.Length == 0) return "Sınırsız";
        var number = decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var n) ? n.ToString("#,0.##", CultureInfo.GetCultureInfo("tr-TR")) : value;
        return item.Unit == "%" ? $"%{number}" : $"{number} {item.Unit}".Trim();
    }

    /// <summary>A limit typed in the portal ("12,5", "1.000") as the API wants it, or null when it is not a number.</summary>
    public static string? ParseLimit(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        return decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.GetCultureInfo("tr-TR"), out var value) && value >= 0
            ? value.ToString("0.####", CultureInfo.InvariantCulture)
            : null;
    }
}
