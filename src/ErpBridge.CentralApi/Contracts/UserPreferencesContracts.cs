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
}
