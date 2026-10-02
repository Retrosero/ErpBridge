namespace ErpBridge.CentralApi.Storage;

/// <summary>
/// <c>Storage</c> configuration section (docs/GOAL_DEPOLAMA_R2.md §3; Coolify <c>Storage__*</c>): the Cloudflare R2
/// account, its two buckets and the limits of the central file store. The access key pair is a Coolify secret and
/// never reaches a log (<see cref="LogCenter.LogScrubber"/>). With any connection value missing the API still starts:
/// the store is "unavailable" and every storage endpoint answers <c>503 STORAGE_UNAVAILABLE</c> (T10).
/// </summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public const long GigaByte = 1024L * 1024 * 1024;

    /// <summary>The Cloudflare account id; the endpoint is <c>https://{AccountId}.r2.cloudflarestorage.com</c>.</summary>
    public string AccountId { get; set; } = string.Empty;

    public string AccessKeyId { get; set; } = string.Empty;

    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>Public pictures (catalog, banner, XML, product), served from <see cref="PublicBaseUrl"/>.</summary>
    public string PublicBucket { get; set; } = string.Empty;

    /// <summary>Pictures behind sign-in (task, expense, vehicle), served through a short presigned redirect.</summary>
    public string PrivateBucket { get; set; } = string.Empty;

    /// <summary>The public bucket's own domain, e.g. <c>https://img.appsgo.cloud</c>.</summary>
    public string PublicBaseUrl { get; set; } = string.Empty;

    /// <summary>A company's quota while its <c>tenant_storage.QuotaBytes</c> is empty (R2: 5 GB).</summary>
    public long DefaultQuotaBytes { get; set; } = 5 * GigaByte;

    /// <summary>How long a presigned address for a private file stays valid.</summary>
    public int PresignMinutes { get; set; } = 5;

    /// <summary>Days a trashed file can be restored before it is deleted from R2 (S9).</summary>
    public int TrashDays { get; set; } = 7;

    /// <summary>Days after which the files of a deleted owner (task, expense, product) are trashed (S9).</summary>
    public int DeletedOwnerPurgeDays { get; set; } = 30;

    /// <summary>The daily maintenance pass (<see cref="StorageMaintenanceWorker"/>); off in tests.</summary>
    public bool MaintenanceEnabled { get; set; } = true;

    /// <summary>UTC hour-of-day of the daily maintenance pass (0-23).</summary>
    public int MaintenanceHourUtc { get; set; } = 2;

    /// <summary>
    /// The XML picture sync (<see cref="XmlImageSyncWorker"/>): every minute the companies waiting for a run, and once a
    /// day, an hour after <see cref="MaintenanceHourUtc"/>, every company with an XML feed marked as waiting. Off in tests.
    /// </summary>
    public bool XmlSyncEnabled { get; set; } = true;

    /// <summary>Every value the R2 connection needs is present.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(AccountId)
        && !string.IsNullOrWhiteSpace(AccessKeyId)
        && !string.IsNullOrWhiteSpace(SecretAccessKey)
        && !string.IsNullOrWhiteSpace(PublicBucket)
        && !string.IsNullOrWhiteSpace(PrivateBucket)
        && !string.IsNullOrWhiteSpace(PublicBaseUrl)
        && !SameBuckets;

    /// <summary>Private files must never land in the bucket the CDN serves: the two names have to differ.</summary>
    private bool SameBuckets =>
        !string.IsNullOrWhiteSpace(PublicBucket)
        && string.Equals(PublicBucket.Trim(), PrivateBucket?.Trim(), StringComparison.OrdinalIgnoreCase);

    /// <summary>The names of the settings that are still empty (never their values), for the startup warning.</summary>
    public IReadOnlyList<string> MissingSettings()
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(AccountId)) missing.Add(nameof(AccountId));
        if (string.IsNullOrWhiteSpace(AccessKeyId)) missing.Add(nameof(AccessKeyId));
        if (string.IsNullOrWhiteSpace(SecretAccessKey)) missing.Add(nameof(SecretAccessKey));
        if (string.IsNullOrWhiteSpace(PublicBucket)) missing.Add(nameof(PublicBucket));
        if (string.IsNullOrWhiteSpace(PrivateBucket)) missing.Add(nameof(PrivateBucket));
        if (string.IsNullOrWhiteSpace(PublicBaseUrl)) missing.Add(nameof(PublicBaseUrl));
        if (SameBuckets) missing.Add(nameof(PrivateBucket) + " (must differ from " + nameof(PublicBucket) + ")");
        return missing;
    }
}
