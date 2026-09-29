namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A sale a user parked on the phone to finish later ("bekleyen sipariş"): the cart with its customer,
/// warehouse and note. Shared by every phone of the company, so anyone can pick it up and complete it; kept
/// only in the central database, for ERP and ERP-less companies alike. Never written to the ERP: it is a
/// draft, not a document.
///
/// <para>The id is made by the phone, so a sale parked offline keeps its id when the queued operation reaches
/// the server. Opening it on a phone <b>claims</b> it: it leaves every phone's list, and a second phone trying
/// to open the same sale is refused. Times are unix milliseconds (UTC).</para>
/// </summary>
public sealed class SuspendedSale
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>Short number shown to people ("BS-4821"); the id is what identifies it.</summary>
    public string DocNo { get; set; } = string.Empty;

    /// <summary>The phone's customer id (the ERP cari code on ERP companies); empty for a walk-in sale.</summary>
    public string? CustomerId { get; set; }

    /// <summary>Snapshot of the customer's name when parked.</summary>
    public string CustomerName { get; set; } = string.Empty;

    public string? Warehouse { get; set; }

    public string? Note { get; set; }

    /// <summary>Informational: the lines' total after line discounts, as the phone computed it.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>The lines, validated, as a JSON array of <c>SuspendedSaleLineDto</c>.</summary>
    public string LinesJson { get; set; } = "[]";

    public int LineCount { get; set; }

    /// <summary>Tombstone: claimed (opened on a phone) or deleted. Phones pull it to drop their copy.</summary>
    public bool IsDeleted { get; set; }

    /// <summary><c>claimed</c> or <c>deleted</c> once <see cref="IsDeleted"/>.</summary>
    public string? ClosedReason { get; set; }

    /// <summary>Who claimed or deleted it; told to a second phone trying to open it.</summary>
    public string? ClosedByName { get; set; }

    public Guid CreatedByUserId { get; set; }

    /// <summary>Snapshot: stays readable after the user is renamed or deleted.</summary>
    public string CreatedByName { get; set; } = string.Empty;

    public long CreatedAtMs { get; set; }

    public long UpdatedAtMs { get; set; }

    /// <summary>The tenant's change order from <c>tenant_sync_counter</c> (rule 11).</summary>
    public long UpdatedSeq { get; set; }
}

/// <summary>A suspended-sale operation id the server already applied; a retried batch is not applied twice.</summary>
public sealed class SuspendedSaleOpApplied
{
    public Guid TenantId { get; set; }

    public Guid OpId { get; set; }

    public Guid UserId { get; set; }

    public long AppliedAtMs { get; set; }
}
