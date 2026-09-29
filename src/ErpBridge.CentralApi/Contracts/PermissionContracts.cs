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
