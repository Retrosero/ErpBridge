namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A company's storage counter (GOAL_DEPOLAMA_R2 T8, table <c>tenant_storage</c>). Uploads reserve their bytes with one
/// conditional update of this row (<c>Used + Reserved + size ≤ quota</c>), so two uploads at once can never both slip
/// under the quota; a finished upload moves its bytes from <see cref="ReservedBytes"/> to <see cref="UsedBytes"/>, a
/// failed one gives them back. Trashed files count until they are purged (T8). The daily recount sets <see cref="UsedBytes"/> from
/// <see cref="StoredFile"/> again.
/// </summary>
public sealed class TenantStorage
{
    public Guid TenantId { get; set; }

    /// <summary>The company's own quota; null = <c>Storage:DefaultQuotaBytes</c> (5 GB).</summary>
    public long? QuotaBytes { get; set; }

    /// <summary>Bytes of the company's active and trashed files (a file leaves the quota when it is purged).</summary>
    public long UsedBytes { get; set; }

    /// <summary>Bytes of uploads in flight (reserved, not yet in the ledger).</summary>
    public long ReservedBytes { get; set; }

    /// <summary>When the counter was last recounted from the ledger (unix ms).</summary>
    public long? RecountedAtMs { get; set; }

    public long UpdatedAtMs { get; set; }
}

/// <summary>
/// One object in R2 (GOAL_DEPOLAMA_R2 T7, table <c>stored_files</c>): the single ledger the quota and the clean-up rest
/// on. Its owner (<see cref="OwnerType"/> + <see cref="OwnerKey"/>, e.g. a catalog picture or a task) is what the
/// object belongs to; a row no record points at is an orphan. Written only by <c>Storage.FileStore</c>.
/// </summary>
public sealed class StoredFile
{
    public const int MaxObjectKeyLength = 512;
    public const int MaxOwnerTypeLength = 32;
    public const int MaxOwnerKeyLength = 128;

    /// <summary>Unguessable; part of the object key.</summary>
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>One of <see cref="StorageAreas"/>.</summary>
    public string Area { get; set; } = string.Empty;

    /// <summary><see cref="StorageBuckets.Public"/> or <see cref="StorageBuckets.Private"/>; follows the area.</summary>
    public string Bucket { get; set; } = string.Empty;

    /// <summary><c>{FIRMAKODU}/{area}/{yyyy}/{MM}/{id}-{variant}.{ext}</c>.</summary>
    public string ObjectKey { get; set; } = string.Empty;

    /// <summary>One of <see cref="StoredFileVariants"/>.</summary>
    public string Variant { get; set; } = StoredFileVariants.Original;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    /// <summary>Lower-case hex SHA-256 of the stored bytes.</summary>
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>What kind of record the file belongs to (e.g. <c>catalog_image</c>, <c>task</c>).</summary>
    public string OwnerType { get; set; } = string.Empty;

    /// <summary>That record's key.</summary>
    public string OwnerKey { get; set; } = string.Empty;

    /// <summary>One of <see cref="StoredFileStatuses"/>.</summary>
    public string Status { get; set; } = StoredFileStatuses.Active;

    public long CreatedAtMs { get; set; }

    /// <summary>The mobile/panel user who uploaded it; null for server work (XML sync, migration).</summary>
    public Guid? CreatedByUserId { get; set; }

    public long? TrashedAtMs { get; set; }

    public Guid? TrashedByUserId { get; set; }
}

/// <summary>What a stored file is for; the area decides the bucket (T1).</summary>
public static class StorageAreas
{
    public const string Catalog = "catalog";
    public const string Banner = "banner";
    public const string Xml = "xml";
    public const string Product = "product";
    public const string Task = "task";
    public const string Expense = "expense";
    public const string Vehicle = "vehicle";

    /// <summary>Every area, in the order the usage screens list them.</summary>
    public static readonly IReadOnlyList<string> All = [Product, Xml, Catalog, Banner, Task, Expense, Vehicle];

    public static bool IsKnown(string? area) => area is not null && All.Contains(area, StringComparer.Ordinal);

    /// <summary>
    /// Public pictures are served from the CDN domain to anyone with the address; task, expense and vehicle pictures
    /// only through a signed-in, short presigned redirect. A public bucket cannot hold a private file (R2 opens access
    /// per bucket), so the bucket is never the caller's choice.
    /// </summary>
    public static string BucketOf(string area) => area switch
    {
        Catalog or Banner or Xml or Product => StorageBuckets.Public,
        Task or Expense or Vehicle => StorageBuckets.Private,
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown storage area."),
    };
}

public static class StorageBuckets
{
    public const string Public = "public";
    public const string Private = "private";
}

public static class StoredFileVariants
{
    public const string Small = "s";
    public const string Large = "l";
    public const string Original = "o";

    public static bool IsKnown(string? variant) => variant is Small or Large or Original;
}

public static class StoredFileStatuses
{
    public const string Active = "active";
    public const string Trashed = "trashed";

    /// <summary>Leaving: its R2 object is being deleted; the row goes once the object is gone.</summary>
    public const string Purging = "purging";
}
