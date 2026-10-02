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

    /// <summary>
    /// An immediate quarantine check is wanted (unix ms; GOAL_DEPOLAMA_R2 T4): the company was switched off or on, or its
    /// catalog module changed. The maintenance worker looks every minute; null = nothing waiting.
    /// </summary>
    public long? QuarantineRequestedAtMs { get; set; }
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

    /// <summary>
    /// The key the object had in the public bucket while it is quarantined in the private one (T4: a closed company, or a
    /// catalog picture of a company without the catalog module); null = not quarantined. The way back uses it.
    /// </summary>
    public string? QuarantinedFromKey { get; set; }
}

/// <summary>
/// One deletion in the trash (GOAL_DEPOLAMA_R2 S9, table <c>storage_trash_items</c>): what a user or the clean-up removed —
/// a product photo, a catalog picture, a banner, a task picture, a receipt — with its files (<see cref="StorageTrashItemFile"/>)
/// and, when the owner record itself went, a snapshot to put it back. Restoring brings the record back with its files;
/// after <c>Storage:TrashDays</c> the files are purged and the item goes. Written in the owner's own transaction.
/// </summary>
public sealed class StorageTrashItem
{
    public const int MaxLabelLength = 200;

    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>One of <see cref="StorageAreas"/>: where the files were.</summary>
    public string Area { get; set; } = string.Empty;

    /// <summary>One of <see cref="StorageTrashKinds"/>: what the owner was, so a restore knows how to bring it back.</summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>What the panel shows: a stock code, a banner title, a task title, a document id.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>The files' bytes together at the time of the delete.</summary>
    public long SizeBytes { get; set; }

    /// <summary>What a restore needs (the owner row as it was, or the soft-deleted row's id); null = cannot be restored.</summary>
    public string? SnapshotJson { get; set; }

    /// <summary>One of <see cref="StorageTrashSources"/>.</summary>
    public string Source { get; set; } = StorageTrashSources.User;

    public long TrashedAtMs { get; set; }

    public Guid? TrashedByUserId { get; set; }

    public List<StorageTrashItemFile> Files { get; set; } = [];
}

/// <summary>A file of a <see cref="StorageTrashItem"/> (table <c>storage_trash_item_files</c>); no foreign key to the ledger.</summary>
public sealed class StorageTrashItemFile
{
    public Guid ItemId { get; set; }

    public Guid FileId { get; set; }
}

/// <summary>Values of <see cref="StorageTrashItem.Kind"/>.</summary>
public static class StorageTrashKinds
{
    public const string ProductImage = "product_image";
    public const string CatalogImage = "catalog_image";

    /// <summary>A deleted banner and its picture.</summary>
    public const string Banner = "banner";

    /// <summary>A banner's picture replaced by another in an edit; the banner stayed.</summary>
    public const string BannerImage = "banner_image";
    public const string TaskAttachment = "task_attachment";
    public const string ExpenseAttachment = "expense_attachment";

    /// <summary>Files without a record (the daily sweep, a deleted task 30 days on): nothing to bring back.</summary>
    public const string Files = "files";
}

/// <summary>Values of <see cref="StorageTrashItem.Source"/>.</summary>
public static class StorageTrashSources
{
    /// <summary>A user deleted it (phone or panel).</summary>
    public const string User = "user";

    /// <summary>The panel's "Alan aç".</summary>
    public const string Cleanup = "cleanup";

    /// <summary>The daily sweep of files no record points at.</summary>
    public const string Sweep = "sweep";

    /// <summary>The record they belonged to was deleted long ago (a deleted task's pictures, 30 days on).</summary>
    public const string OwnerDeleted = "owner_deleted";
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
