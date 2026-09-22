namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Who changed what from the portal in a native tenant's own books (GOAL_PANEL_ERPSIZ D5/E7b): a card,
/// a payment, a ledger correction. Separate from the operator console's <c>change_set_audit_log</c>
/// (an ERP agent's bundles for an ERP tenant) — a different actor, a different book.
///
/// <para>A phone's writes are not recorded here; the phone already has its own job history and the
/// approval centre. This table exists because a native tenant's book of record has no ERP behind it
/// to keep its own change history, and a single ADMIN doing everything from the portal still needs
/// one to answer "who changed this, and to what" (D5).</para>
/// </summary>
public sealed class NativeAuditLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Denormalized so history reads even after the user is deactivated or removed.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>stock_card, customer_card, collection, disbursement, ledger, …</summary>
    public string Entity { get; set; } = string.Empty;

    /// <summary>The card's code or the movement's key — what "Geçmiş" for one row filters by.</summary>
    public string EntityKey { get; set; } = string.Empty;

    /// <summary>create, edit, delete, void, …</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>One Turkish sentence for the list; the full state is in <see cref="AfterJson"/>.</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>The row's state just before this write; null when nothing existed yet (a create).</summary>
    public string? BeforeJson { get; set; }

    /// <summary>The row's state just after this write; null for a delete.</summary>
    public string? AfterJson { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
