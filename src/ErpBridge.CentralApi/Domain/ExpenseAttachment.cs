namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A receipt photo of a phone's expense or vehicle maintenance document (<c>expense_attachments</c>, GOAL_DEPOLAMA_R2 S5).
/// The document is named by its phone id (<see cref="DocumentExternalId"/>, the cash-log id the phone sends as
/// <c>mobileDocumentId</c> and the server keeps as <c>jobs.ExternalId</c>), so a receipt can go up before or after its
/// document reaches the server. The photo is in the central file store's private bucket (<see cref="StoredFileId"/>).
/// The id is made by the phone: a retried upload finds the same row.
/// </summary>
public sealed class ExpenseAttachment
{
    public const int MaxDocumentIdLength = 128;

    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>The phone's document id (e.g. <c>K-…</c>); no foreign key, the document may not be on the server yet.</summary>
    public string DocumentExternalId { get; set; } = string.Empty;

    /// <summary>One of <see cref="ExpenseAttachmentKinds"/>.</summary>
    public string Kind { get; set; } = ExpenseAttachmentKinds.Expense;

    /// <summary>The photo's <c>stored_files</c> row (no foreign key: the ledger follows the store's own life).</summary>
    public Guid StoredFileId { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public int SizeBytes { get; set; }

    public long CreatedAtMs { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string CreatedByName { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public long? DeletedAtMs { get; set; }
}

/// <summary>Values of <see cref="ExpenseAttachment.Kind"/>; the kind decides the storage area.</summary>
public static class ExpenseAttachmentKinds
{
    public const string Expense = "expense";
    public const string VehicleMaintenance = "vehicle_maintenance";

    public static bool IsKnown(string? kind) => kind is Expense or VehicleMaintenance;

    /// <summary><see cref="StorageAreas.Expense"/> or <see cref="StorageAreas.Vehicle"/>.</summary>
    public static string AreaOf(string kind) => kind == VehicleMaintenance ? StorageAreas.Vehicle : StorageAreas.Expense;
}
