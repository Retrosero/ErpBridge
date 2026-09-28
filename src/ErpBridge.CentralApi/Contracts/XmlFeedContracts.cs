using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

/// <summary>GET|PUT /api/v1/android/xml-feed/config response: the company's XML product feed.</summary>
public sealed class XmlFeedConfigDto
{
    /// <summary>False until an administrator saves a feed; the other fields then carry defaults.</summary>
    [JsonPropertyName("configured")] public bool Configured { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("recordPath")] public string? RecordPath { get; set; }

    /// <summary>Target field (CODE, IMAGE, …) → candidate paths inside a record, first match wins.</summary>
    [JsonPropertyName("mapping")] public Dictionary<string, string[]> Mapping { get; set; } = new();
    [JsonPropertyName("downloadImages")] public bool DownloadImages { get; set; } = true;
    [JsonPropertyName("importDescriptions")] public bool ImportDescriptions { get; set; }

    /// <summary>Import every mapped field; always false in an ERP company.</summary>
    [JsonPropertyName("fullImport")] public bool FullImport { get; set; }
    [JsonPropertyName("updatedAtUtc")] public DateTimeOffset? UpdatedAtUtc { get; set; }
    [JsonPropertyName("updatedByName")] public string? UpdatedByName { get; set; }
}

/// <summary>PUT /api/v1/android/xml-feed/config body.</summary>
public sealed class XmlFeedConfigRequest
{
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("recordPath")] public string? RecordPath { get; set; }
    [JsonPropertyName("mapping")] public Dictionary<string, string[]?>? Mapping { get; set; }
    [JsonPropertyName("downloadImages")] public bool DownloadImages { get; set; } = true;
    [JsonPropertyName("importDescriptions")] public bool ImportDescriptions { get; set; }
    [JsonPropertyName("fullImport")] public bool FullImport { get; set; }
}
