namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// What a tenant has paid for: a number of mobile seats until a date. Seats are
/// sold outside the app and entered by an operator from the admin console.
///
/// <para>Rows are never edited in place. Every change inserts a new row and
/// clears <see cref="IsCurrent"/> on the previous one, so the table doubles as
/// the history of who changed a tenant's seats, when, and against which
/// payment. A partial unique index keeps at most one current row per tenant.</para>
/// </summary>
public sealed class TenantSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>Maximum number of active mobile users, administrators included.</summary>
    public int Seats { get; set; }

    public DateTimeOffset StartsAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>End of the paid period; <c>null</c> for an open-ended agreement.</summary>
    public DateTimeOffset? EndsAtUtc { get; set; }

    /// <summary>How it was paid: <c>manual</c>, <c>bank_transfer</c>, <c>card</c>…</summary>
    public string Source { get; set; } = "manual";

    /// <summary>Invoice, receipt or payment reference, for reconciliation.</summary>
    public string? Reference { get; set; }

    public string? Note { get; set; }

    public bool IsCurrent { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Admin console user who recorded this row.</summary>
    public Guid? CreatedByAdminId { get; set; }
}
