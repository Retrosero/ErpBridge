using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

/// <summary>Request body of <c>POST /api/v1/android/sync/pull</c>.</summary>
public sealed class SyncPullRequest
{
    /// <summary>
    /// Where the device left off, as returned by the previous call. Absent on a
    /// fresh install, which is what asks for the whole feed.
    /// </summary>
    [JsonPropertyName("cursor")] public string? Cursor { get; set; }

    /// <summary>Records to return. Clamped server-side.</summary>
    [JsonPropertyName("limit")] public int? Limit { get; set; }
}

/// <summary>One record's current state, as the device should store it.</summary>
public sealed class SyncPullChange
{
    /// <summary>Entity the record belongs to — <c>stocks</c>, <c>customers</c>, and so on.</summary>
    [JsonPropertyName("entity")] public string Entity { get; set; } = string.Empty;

    /// <summary>Business key within that entity; composite parts are joined with <c>|</c>.</summary>
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;

    /// <summary>True when the ERP no longer has the record and the device should drop it.</summary>
    [JsonPropertyName("deleted")] public bool Deleted { get; set; }

    /// <summary>The record itself. Null for a deletion, which carries only its key.</summary>
    [JsonPropertyName("data")] public JsonElement? Data { get; set; }
}

/// <summary>Response body of <c>POST /api/v1/android/sync/pull</c>.</summary>
public sealed class SyncPullResponse
{
    /// <summary>This page of changes, oldest first.</summary>
    [JsonPropertyName("changes")] public List<SyncPullChange> Changes { get; set; } = [];

    /// <summary>
    /// Position to send back next time. Always present, including on the last
    /// page, so a client never has to track a position of its own.
    /// </summary>
    [JsonPropertyName("nextCursor")] public string NextCursor { get; set; } = string.Empty;

    /// <summary>Whether another page is waiting right now.</summary>
    [JsonPropertyName("hasMore")] public bool HasMore { get; set; }

    /// <summary>
    /// The device's position can no longer be honoured — it has been offline
    /// longer than tombstones are kept, so deletions it never saw have already
    /// been purged. It must clear its local data and start from no cursor.
    /// </summary>
    [JsonPropertyName("resyncRequired")] public bool ResyncRequired { get; set; }
}
