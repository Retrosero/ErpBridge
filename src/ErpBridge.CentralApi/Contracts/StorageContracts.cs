using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

/// <summary>One area's share of the company storage (active files only).</summary>
public sealed class StorageAreaUsageDto
{
    [JsonPropertyName("area")] public string Area { get; set; } = string.Empty;
    [JsonPropertyName("usedBytes")] public long UsedBytes { get; set; }
    [JsonPropertyName("fileCount")] public int FileCount { get; set; }
}

/// <summary>
/// <c>GET /api/v1/storage/usage</c> (GOAL_DEPOLAMA_R2 S8). Everyone sees the total, the quota and how much of the total is
/// in the trash; the breakdown by area and the trash's file count only who manages storage (<c>action.storage.manage</c>)
/// — omitted otherwise. <see cref="UsedBytes"/> includes the trash: a file counts until it is purged (T8).
/// </summary>
public sealed class StorageUsageResponse
{
    /// <summary>False while the R2 settings are missing: uploads answer 503.</summary>
    [JsonPropertyName("available")] public bool Available { get; set; }
    [JsonPropertyName("usedBytes")] public long UsedBytes { get; set; }
    [JsonPropertyName("quotaBytes")] public long QuotaBytes { get; set; }
    [JsonPropertyName("freeBytes")] public long FreeBytes { get; set; }

    [JsonPropertyName("areas"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StorageAreaUsageDto[]? Areas { get; set; }

    /// <summary>Bytes in the trash: part of <see cref="UsedBytes"/> until purged; emptying the trash frees them (T8).</summary>
    [JsonPropertyName("trashedBytes")] public long TrashedBytes { get; set; }

    [JsonPropertyName("trashedCount"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TrashedCount { get; set; }
}

/// <summary><c>GET/PUT /api/v1/admin/tenants/{id}/storage</c>: the operator's view of a company's storage.</summary>
public sealed class AdminTenantStorageResponse
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("available")] public bool Available { get; set; }
    [JsonPropertyName("usedBytes")] public long UsedBytes { get; set; }
    [JsonPropertyName("reservedBytes")] public long ReservedBytes { get; set; }

    /// <summary>The quota in force: the company's own, else the default.</summary>
    [JsonPropertyName("quotaBytes")] public long QuotaBytes { get; set; }
    [JsonPropertyName("defaultQuotaBytes")] public long DefaultQuotaBytes { get; set; }

    /// <summary>The company's own quota; null = the default applies.</summary>
    [JsonPropertyName("customQuotaBytes")] public long? CustomQuotaBytes { get; set; }
    [JsonPropertyName("recountedAtMs")] public long? RecountedAtMs { get; set; }
    [JsonPropertyName("areas")] public StorageAreaUsageDto[] Areas { get; set; } = [];
    [JsonPropertyName("trashedBytes")] public long TrashedBytes { get; set; }
    [JsonPropertyName("trashedCount")] public int TrashedCount { get; set; }
}

/// <summary><c>PUT /api/v1/admin/tenants/{id}/storage</c>: null puts the company back on the default quota.</summary>
public sealed class SetTenantStorageRequest
{
    [JsonPropertyName("quotaBytes")] public long? QuotaBytes { get; set; }
}

/// <summary><c>POST /api/v1/admin/tenants/{id}/storage/recount</c>.</summary>
public sealed class AdminStorageRecountResponse
{
    [JsonPropertyName("usedBytesBefore")] public long UsedBytesBefore { get; set; }
    [JsonPropertyName("usedBytesAfter")] public long UsedBytesAfter { get; set; }
    [JsonPropertyName("storage")] public AdminTenantStorageResponse Storage { get; set; } = new();
}
