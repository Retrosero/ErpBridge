using System.Text.Json;

namespace ErpBridge.CentralApi.Contracts;

/// <summary>
/// <c>GET/PUT /api/v1/android/account/preferences</c>: a phone user's view preferences. <see cref="Data"/> is the phone's own
/// JSON object, kept as is; <see cref="Version"/> is 0 (and <see cref="Data"/> null) until the user first saves.
/// </summary>
public sealed class UserPreferencesDto
{
    public long Version { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public JsonElement? Data { get; set; }

    /// <summary>
    /// What lies under <see cref="Data"/>: the user's roles' defaults and every lock that applies to them (KB kural 35). Sent
    /// with the user's own GET; an app built before it ignores the field.
    /// </summary>
    public ViewPreferenceBaseDto? Base { get; set; }
}

/// <summary>Flat JSON objects of setting path → value, and a stamp that changes whenever either may have changed.</summary>
public sealed class ViewPreferenceBaseDto
{
    public JsonElement Data { get; set; }

    public JsonElement Locks { get; set; }

    public string Stamp { get; set; } = string.Empty;
}

/// <summary>
/// <c>GET /api/v1/android/account/users/{id}/preferences</c> (administrators): one person's preferences as the portal edits
/// them — their own document, their own locks, and what their roles give them.
/// </summary>
public sealed class UserViewPreferencesResponse
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    /// <summary>Highest precedence first.</summary>
    public string[] Roles { get; set; } = [];

    public long Version { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public string? UpdatedByName { get; set; }

    /// <summary><c>android</c> or <c>portal</c>.</summary>
    public string? UpdatedByClient { get; set; }

    /// <summary>The person's own document; null when they never saved.</summary>
    public JsonElement? Data { get; set; }

    /// <summary>Locks set for this person only.</summary>
    public JsonElement Locks { get; set; }

    /// <summary>The person's roles merged: defaults and locks (without <see cref="Locks"/>).</summary>
    public ViewPreferenceBaseDto RoleBase { get; set; } = new();
}

/// <summary>
/// <c>PUT /api/v1/android/account/users/{id}/preferences</c>: replaces the person's document and locks. When
/// <see cref="ExpectedVersion"/> is set and the stored version differs, nothing changes (409 <c>PREFERENCES_CONFLICT</c>).
/// </summary>
public sealed class UpdateUserViewPreferencesRequest
{
    public JsonElement? Data { get; set; }

    public JsonElement? Locks { get; set; }

    public long? ExpectedVersion { get; set; }
}

/// <summary><c>POST /api/v1/android/account/users/{id}/preferences/copy</c>: the person's document (and locks) onto others.</summary>
public sealed class CopyViewPreferencesRequest
{
    public Guid[] TargetUserIds { get; set; } = [];

    public bool IncludeLocks { get; set; }
}

public sealed class CopyViewPreferencesResponse
{
    public int Copied { get; set; }
}

/// <summary>One role's template (<c>GET /api/v1/android/account/roles/view-preferences</c>).</summary>
public sealed class RoleViewPreferencesDto
{
    public string Role { get; set; } = string.Empty;

    public JsonElement Data { get; set; }

    public JsonElement Locks { get; set; }

    public long Version { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }
}

/// <summary><c>PUT /api/v1/android/account/roles/{role}/view-preferences</c>: replaces the role's defaults and locks.</summary>
public sealed class UpdateRoleViewPreferencesRequest
{
    public JsonElement? Data { get; set; }

    public JsonElement? Locks { get; set; }

    public long? ExpectedVersion { get; set; }
}
