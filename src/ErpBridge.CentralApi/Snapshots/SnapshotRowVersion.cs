using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ErpBridge.CentralApi.Snapshots;

/// <summary>
/// Server-assigned version of a snapshot row. ERP sections without a row
/// timestamp (inventory is read from a view) still need to answer "what changed
/// since my last sync?", so the incremental merge stamps every row it rewrites
/// with the time it last saw that row change. Rows that were never rewritten
/// (a full upload's chunks are stored verbatim) take the time of the chunk they
/// live in. Millisecond precision throughout, so a watermark handed to a device
/// and echoed back compares exactly against the stored stamps.
/// </summary>
public static class SnapshotRowVersion
{
    public const string PropertyName = "changedAtUtc";
    private const string Layout = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

    public static DateTimeOffset Truncate(DateTimeOffset value)
    {
        var utc = value.ToUniversalTime();
        return new DateTimeOffset(utc.Ticks - utc.Ticks % TimeSpan.TicksPerMillisecond, TimeSpan.Zero);
    }

    public static string Format(DateTimeOffset value) =>
        Truncate(value).ToString(Layout, CultureInfo.InvariantCulture);

    /// <summary>The row's explicit stamp, else the (truncated) time of its chunk.</summary>
    public static DateTimeOffset Of(JsonElement row, DateTimeOffset chunkReceivedAtUtc)
    {
        if (row.ValueKind == JsonValueKind.Object
            && row.TryGetProperty(PropertyName, out var raw)
            && raw.ValueKind == JsonValueKind.String
            && DateTimeOffset.TryParse(raw.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var stamped))
            return Truncate(stamped);
        return Truncate(chunkReceivedAtUtc);
    }

    /// <summary>
    /// Structural equality that ignores the server stamp, so a row the agent
    /// re-sends unchanged compares equal to the stored copy that carries one.
    /// </summary>
    public static bool SameRow(JsonNode stored, JsonNode incoming)
    {
        if (stored is JsonObject withStamp && withStamp.ContainsKey(PropertyName))
        {
            var bare = (JsonObject)withStamp.DeepClone();
            bare.Remove(PropertyName);
            return JsonNode.DeepEquals(bare, incoming);
        }
        return JsonNode.DeepEquals(stored, incoming);
    }
}
