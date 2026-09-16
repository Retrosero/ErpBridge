using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErpBridge.Portal.Api;

/// <summary>The screen's pairing was revoked (or its company closed): it goes back to showing a code.</summary>
public sealed class DisplayRevokedException() : Exception("This screen is no longer paired.");

/// <summary>
/// Calls the central API for a warehouse TV (plan step 7). Unlike <see cref="PortalApiClient"/> there is no
/// signed-in person: pairing is anonymous and the board is read with the screen's own token, passed per call.
/// </summary>
public sealed class DisplayApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<DisplayPairingDto> CreatePairingAsync(CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync("api/v1/display/pairings", new { }, Json, ct);
        return await ReadAsync<DisplayPairingDto>(response, ct);
    }

    /// <summary><c>waiting</c> until a manager enters the code, then <c>paired</c> with the token (once).</summary>
    public async Task<DisplayTokenDto> TakeTokenAsync(string code, string secret, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync($"api/v1/display/pairings/{Uri.EscapeDataString(code)}/token", new { secret }, Json, ct);
        return await ReadAsync<DisplayTokenDto>(response, ct);
    }

    public Task<DisplayBoardDto> BoardAsync(string token, CancellationToken ct = default) =>
        GetAsync<DisplayBoardDto>("api/v1/display/board", token, ct);

    /// <summary>Long-poll on the warehouse queue; the server caps <paramref name="waitSeconds"/> at 25, below the client timeout.</summary>
    public Task<PortalEventsDto> EventsAsync(string token, long sinceSeq, int waitSeconds, CancellationToken ct = default) =>
        GetAsync<PortalEventsDto>($"api/v1/display/events?sinceSeq={sinceSeq}&wait={waitSeconds}", token, ct);

    private async Task<T> GetAsync<T>(string path, string token, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await http.SendAsync(request, ct);
        // 401 only: a 403 (no current subscription) leaves the screen paired until the company renews.
        if (response.StatusCode == HttpStatusCode.Unauthorized) throw new DisplayRevokedException();
        return await ReadAsync<T>(response, ct);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return (await response.Content.ReadFromJsonAsync<T>(Json, ct))!;
        string code;
        try
        {
            code = (await response.Content.ReadFromJsonAsync<ApiErrorDto>(Json, ct))?.ErrorCode ?? $"HTTP_{(int)response.StatusCode}";
        }
        catch (JsonException)
        {
            code = $"HTTP_{(int)response.StatusCode}";
        }
        throw new PortalApiException(code, (int)response.StatusCode);
    }
}

public sealed class DisplayPairingDto
{
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
    [JsonPropertyName("secret")] public string Secret { get; set; } = string.Empty;
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset ExpiresAtUtc { get; set; }
}

public sealed class DisplayTokenDto
{
    [JsonPropertyName("status")] public string Status { get; set; } = "waiting";
    [JsonPropertyName("token")] public string? Token { get; set; }
    [JsonPropertyName("tenantName")] public string? TenantName { get; set; }
    [JsonPropertyName("displayName")] public string? DisplayName { get; set; }
}

public sealed class DisplayBoardDto
{
    [JsonPropertyName("tenantName")] public string TenantName { get; set; } = string.Empty;
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = string.Empty;
    [JsonPropertyName("settings")] public WarehouseSettingsDto Settings { get; set; } = new();
    [JsonPropertyName("latestSeq")] public long LatestSeq { get; set; }
    [JsonPropertyName("serverTimeUtc")] public DateTimeOffset ServerTimeUtc { get; set; }
    [JsonPropertyName("items")] public FulfillmentDto[] Items { get; set; } = [];

    /// <summary>Every open order per status, including those beyond the cards sent; empty from an older server.</summary>
    [JsonPropertyName("counts")] public Dictionary<string, int> Counts { get; set; } = new();
}

/// <summary>An order in the warehouse queue (mirror of the central API's <c>FulfillmentDto</c>).</summary>
public sealed class FulfillmentDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("orderNo")] public string OrderNo { get; set; } = string.Empty;
    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;
    [JsonPropertyName("salespersonName")] public string SalespersonName { get; set; } = string.Empty;
    [JsonPropertyName("lineCount")] public int LineCount { get; set; }
    [JsonPropertyName("itemQuantity")] public decimal ItemQuantity { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("queuedAtUtc")] public DateTimeOffset QueuedAtUtc { get; set; }
    [JsonPropertyName("startedAtUtc")] public DateTimeOffset? StartedAtUtc { get; set; }
    [JsonPropertyName("packedAtUtc")] public DateTimeOffset? PackedAtUtc { get; set; }
    [JsonPropertyName("assigneeName")] public string? AssigneeName { get; set; }
    [JsonPropertyName("erpState")] public string ErpState { get; set; } = string.Empty;
}

public sealed class WarehouseSettingsDto
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; }
    [JsonPropertyName("pendingWarnMinutes")] public int PendingWarnMinutes { get; set; } = 15;
    [JsonPropertyName("pendingCriticalMinutes")] public int PendingCriticalMinutes { get; set; } = 30;
    [JsonPropertyName("preparingWarnMinutes")] public int PreparingWarnMinutes { get; set; } = 20;
    [JsonPropertyName("preparingCriticalMinutes")] public int PreparingCriticalMinutes { get; set; } = 45;
    [JsonPropertyName("packedWarnMinutes")] public int PackedWarnMinutes { get; set; } = 60;
    [JsonPropertyName("updatedAtUtc")] public DateTimeOffset? UpdatedAtUtc { get; set; }
}

public sealed class DisplayDeviceDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
    [JsonPropertyName("lastSeenAtUtc")] public DateTimeOffset? LastSeenAtUtc { get; set; }
    [JsonPropertyName("revokedAtUtc")] public DateTimeOffset? RevokedAtUtc { get; set; }
}
