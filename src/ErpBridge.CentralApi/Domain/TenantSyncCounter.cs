namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Per-tenant cursor allocator for <see cref="MobileRecord.UpdatedSeq"/>.
///
/// <para>A PostgreSQL identity column cannot be used here. Identity values are
/// assigned at INSERT and the transaction commits later, so a transaction that
/// takes 100 can commit <i>after</i> one that took 101. A device reading at
/// cursor 99 would see 101, advance past it, and never be offered 100 again.
/// With the cursor as the single source of truth that is silent, permanent data
/// loss.</para>
///
/// <para>Allocating from this row instead closes the hole: the
/// <c>UPDATE … RETURNING</c> that reserves a block holds the row lock until the
/// transaction commits, so a second ingest for the same tenant cannot reserve
/// anything until the first is durable. Allocation order therefore equals commit
/// order. A rolled-back transaction leaves a gap in the numbering, which is
/// harmless — only re-ordering is harmful.</para>
/// </summary>
public sealed class TenantSyncCounter
{
    /// <summary>Owning tenant; primary key.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Tenant navigation.</summary>
    public Tenant? Tenant { get; set; }

    /// <summary>Highest sequence handed out so far.</summary>
    public long LastSeq { get; set; }

    /// <summary>
    /// Tombstones at or below this point have been purged. A device whose cursor
    /// is older cannot be brought up to date incrementally — it may have missed a
    /// deletion — so the pull endpoint answers it with <c>resyncRequired</c>.
    /// </summary>
    public long TombstoneHorizonSeq { get; set; }
}
