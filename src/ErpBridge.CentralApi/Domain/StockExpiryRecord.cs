namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// One shelf's expiry note ("SKT kaydı"): a product on a shelf/aisle with the date its lot expires.
/// Operational data entered from the phones and shared by every phone of the company; kept only in the
/// central database for ERP and ERP-less companies alike. Never written to the ERP: it is not master data.
///
/// <para>The id is made by the phone, so a record created offline keeps its id when the queued operation
/// reaches the server. The quantity is informational (sales do not decrement it); a record is closed when
/// the shelf runs out. Times are unix milliseconds (UTC).</para>
/// </summary>
public sealed class StockExpiryRecord
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string StockCode { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    /// <summary>Snapshot of the product name when entered; the product may be renamed later.</summary>
    public string? ProductName { get; set; }

    /// <summary>Reyon / raf.</summary>
    public string Location { get; set; } = string.Empty;

    public string? Warehouse { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public decimal? Quantity { get; set; }

    public string? Note { get; set; }

    /// <summary>The shelf ran out; kept for history, hidden from the phone's open list.</summary>
    public bool IsClosed { get; set; }

    /// <summary>Tombstone: phones pull it to drop their copy. A deleted record is never edited again.</summary>
    public bool IsDeleted { get; set; }

    public Guid CreatedByUserId { get; set; }

    /// <summary>Snapshot: stays readable after the user is renamed or deleted.</summary>
    public string CreatedByName { get; set; } = string.Empty;

    public long CreatedAtMs { get; set; }

    public long UpdatedAtMs { get; set; }

    /// <summary>The tenant's change order from <c>tenant_sync_counter</c> (rule 11).</summary>
    public long UpdatedSeq { get; set; }
}

/// <summary>An expiry operation id the server already applied; a phone retrying a batch is not applied twice.</summary>
public sealed class StockExpiryOpApplied
{
    public Guid TenantId { get; set; }

    public Guid OpId { get; set; }

    public Guid UserId { get; set; }

    public long AppliedAtMs { get; set; }
}
