using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

/// <summary><c>POST /api/v1/display/pairings</c>: the code a TV shows and the secret only it keeps.</summary>
public sealed class DisplayPairingResponse
{
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
    [JsonPropertyName("secret")] public string Secret { get; set; } = string.Empty;
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset ExpiresAtUtc { get; set; }
}

/// <summary>Body of <c>POST /api/v1/display/pairings/{code}/token</c>.</summary>
public sealed class DisplayTokenRequest
{
    [JsonPropertyName("secret")] public string? Secret { get; set; }
}

/// <summary>
/// Answer to a TV polling its code: <c>waiting</c> until a manager enters the code, then <c>paired</c> once,
/// with the token.
/// </summary>
public sealed class DisplayTokenResponse
{
    [JsonPropertyName("status")] public string Status { get; set; } = "waiting";
    [JsonPropertyName("token")] public string? Token { get; set; }
    [JsonPropertyName("tenantName")] public string? TenantName { get; set; }
    [JsonPropertyName("displayName")] public string? DisplayName { get; set; }
}

/// <summary>A paired TV as the portal lists it.</summary>
public sealed class DisplayDeviceDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
    [JsonPropertyName("lastSeenAtUtc")] public DateTimeOffset? LastSeenAtUtc { get; set; }
    [JsonPropertyName("revokedAtUtc")] public DateTimeOffset? RevokedAtUtc { get; set; }
}

/// <summary>Body of <c>POST /api/v1/portal/displays</c>: the code on the TV and the name to give it.</summary>
public sealed class PairDisplayRequest
{
    [JsonPropertyName("code")] public string? Code { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
}

/// <summary><c>GET /api/v1/display/board</c>: everything the TV board draws.</summary>
public sealed class DisplayBoardResponse
{
    [JsonPropertyName("tenantName")] public string TenantName { get; set; } = string.Empty;
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = string.Empty;
    [JsonPropertyName("settings")] public WarehouseSettingsDto Settings { get; set; } = new();

    /// <summary>Cursor for <c>/display/events?sinceSeq</c>, read before the rows.</summary>
    [JsonPropertyName("latestSeq")] public long LatestSeq { get; set; }

    /// <summary>The API's clock: durations on the board are counted from it, not the TV's.</summary>
    [JsonPropertyName("serverTimeUtc")] public DateTimeOffset ServerTimeUtc { get; set; }

    /// <summary>
    /// Open orders, oldest first: up to <c>DisplayEndpoints.MaxCardsPerColumn</c> of each status, so a flood of
    /// pending orders never pushes the preparing and packed ones off the board.
    /// </summary>
    [JsonPropertyName("items")] public FulfillmentDto[] Items { get; set; } = [];

    /// <summary>Every open order per status (PENDING, PREPARING, PACKED), including those beyond the cards sent.</summary>
    [JsonPropertyName("counts")] public Dictionary<string, int> Counts { get; set; } = new();
}
