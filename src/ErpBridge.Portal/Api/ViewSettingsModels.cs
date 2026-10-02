using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ErpBridge.Portal.Api;

// Görünüm ayarları (Siparis_Cepte KB kural 59, ErpBridge KB kural 35). The catalog is the phone's own list of settings,
// copied from Siparis_Cepte docs/view-settings-catalog.json into Resources/; the API DTOs mirror UserPreferencesContracts.

public sealed class ViewSettingsCatalogDto
{
    [JsonPropertyName("version")] public int Version { get; set; }
    [JsonPropertyName("pages")] public ViewSettingsPageDto[] Pages { get; set; } = [];

    public IEnumerable<ViewSettingDto> Settings => Pages.SelectMany(p => p.Sections).SelectMany(s => s.Settings);
}

public sealed class ViewSettingsPageDto
{
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("route")] public string? Route { get; set; }
    [JsonPropertyName("gate")] public string? Gate { get; set; }
    [JsonPropertyName("gateNote")] public string? GateNote { get; set; }
    [JsonPropertyName("sections")] public ViewSettingSectionDto[] Sections { get; set; } = [];

    public IEnumerable<ViewSettingDto> Settings => Sections.SelectMany(s => s.Settings);
}

public sealed class ViewSettingSectionDto
{
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("settings")] public ViewSettingDto[] Settings { get; set; } = [];
}

/// <summary>
/// One setting, addressed by its path in the phone's profile. <see cref="Type"/>: <c>toggle</c>, <c>choice</c>, <c>multi</c>,
/// <c>ordered_multi</c>, <c>order</c> (every option in order; <see cref="SwitchPath"/> lists the switched-on ones) or
/// <c>member</c> (on when <see cref="Member"/> is in the list at the path).
/// </summary>
public sealed class ViewSettingDto
{
    [JsonPropertyName("path")] public string Path { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = "toggle";
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("default")] public JsonElement? Default { get; set; }
    [JsonPropertyName("options")] public ViewSettingOptionDto[] Options { get; set; } = [];
    [JsonPropertyName("member")] public string? Member { get; set; }
    [JsonPropertyName("switchPath")] public string? SwitchPath { get; set; }
    [JsonPropertyName("switchDefault")] public JsonElement? SwitchDefault { get; set; }
    [JsonPropertyName("requires")] public string? Requires { get; set; }
    [JsonPropertyName("gate")] public string? Gate { get; set; }
    [JsonPropertyName("gateNote")] public string? GateNote { get; set; }

    /// <summary>The key a lock or a role default is stored under: the path, or <c>path#member</c> for one list member.</summary>
    public string Key => Type == "member" ? $"{Path}#{Member}" : Path;
}

public sealed class ViewSettingOptionDto
{
    [JsonPropertyName("value")] public JsonElement Value { get; set; }
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
}

/// <summary>The catalog embedded in the portal (<c>Resources/view-settings-catalog.json</c>).</summary>
public static class ViewSettingsCatalogFile
{
    private static readonly Lazy<ViewSettingsCatalogDto> Catalog = new(Read);

    public static ViewSettingsCatalogDto Load() => Catalog.Value;

    private static ViewSettingsCatalogDto Read()
    {
        using var stream = typeof(ViewSettingsCatalogFile).Assembly.GetManifestResourceStream("ErpBridge.Portal.Resources.view-settings-catalog.json")
            ?? throw new InvalidOperationException("view-settings-catalog.json is not embedded.");
        return JsonSerializer.Deserialize<ViewSettingsCatalogDto>(stream) ?? new ViewSettingsCatalogDto();
    }
}

public sealed class ViewPreferenceBaseDto
{
    [JsonPropertyName("data")] public JsonElement Data { get; set; }
    [JsonPropertyName("locks")] public JsonElement Locks { get; set; }
    [JsonPropertyName("stamp")] public string Stamp { get; set; } = string.Empty;
}

public sealed class UserViewPreferencesDto
{
    [JsonPropertyName("userId")] public Guid UserId { get; set; }
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("roles")] public string[] Roles { get; set; } = [];
    [JsonPropertyName("version")] public long Version { get; set; }
    [JsonPropertyName("updatedAtUtc")] public DateTimeOffset? UpdatedAtUtc { get; set; }
    [JsonPropertyName("updatedByName")] public string? UpdatedByName { get; set; }
    [JsonPropertyName("updatedByClient")] public string? UpdatedByClient { get; set; }
    [JsonPropertyName("data")] public JsonElement? Data { get; set; }
    [JsonPropertyName("locks")] public JsonElement Locks { get; set; }
    [JsonPropertyName("roleBase")] public ViewPreferenceBaseDto RoleBase { get; set; } = new();
}

public sealed class UpdateViewPreferencesRequest
{
    [JsonPropertyName("data")] public JsonObject Data { get; set; } = new();
    [JsonPropertyName("locks")] public JsonObject Locks { get; set; } = new();
    [JsonPropertyName("expectedVersion")] public long? ExpectedVersion { get; set; }
}

public sealed class CopyViewPreferencesRequest
{
    [JsonPropertyName("targetUserIds")] public Guid[] TargetUserIds { get; set; } = [];
    [JsonPropertyName("includeLocks")] public bool IncludeLocks { get; set; }
}

public sealed class CopyViewPreferencesResponse
{
    [JsonPropertyName("copied")] public int Copied { get; set; }
}

public sealed class RoleViewPreferencesDto
{
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;
    [JsonPropertyName("data")] public JsonElement Data { get; set; }
    [JsonPropertyName("locks")] public JsonElement Locks { get; set; }
    [JsonPropertyName("version")] public long Version { get; set; }
    [JsonPropertyName("updatedAtUtc")] public DateTimeOffset? UpdatedAtUtc { get; set; }
}
