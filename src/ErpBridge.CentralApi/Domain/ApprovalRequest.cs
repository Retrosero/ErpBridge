using System.Text.Json;

namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A document waiting for a company approver's decision.
///
/// <para>A sale, purchase, return, collection, disbursement, stock count or card
/// change that needs approval is sent here instead of being booked. It carries the
/// exact documents that would have been posted without approval
/// (<see cref="DocumentsJson"/>); approving posts them — through the native ledger
/// for a company without an ERP, or as jobs for the ERP agent — and rejecting posts
/// nothing. The request lives on the server, so every approver of the company sees
/// the same queue on any phone, and a decision survives a closed app.</para>
/// </summary>
public sealed class ApprovalRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>The phone's id for the request; retries with the same id are one request.</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>One of <see cref="ApprovalKinds"/>.</summary>
    public string Kind { get; set; } = string.Empty;

    public string CounterpartyName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    /// <summary>What the approver reads: description, reason, payment type, lines, card fields. Display only.</summary>
    public string SummaryJson { get; set; } = "{}";

    /// <summary>
    /// <c>[{ documentType, externalId, payload }]</c> — posted on approval, exactly as the
    /// phone would have posted them directly.
    /// </summary>
    public string DocumentsJson { get; set; } = "[]";

    public string Status { get; set; } = ApprovalStatuses.Pending;

    /// <summary>The rejected request this one corrects, when it was sent again.</summary>
    public Guid? ReplacesRequestId { get; set; }

    /// <summary>
    /// Log Merkezi L3g: the trace id of the phone request that asked for this approval. Approved documents
    /// become jobs days later, and they keep this id — otherwise the ERP write of an approved document would
    /// start a new thread and the phone's original request would be unreachable from it.
    /// </summary>
    public string? CorrelationId { get; set; }

    public Guid? RequestedByUserId { get; set; }

    public string? RequestedByName { get; set; }

    public DateTimeOffset RequestedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Unix milliseconds of <see cref="RequestedAtUtc"/>, for ordering on every provider.</summary>
    public long RequestedSeq { get; set; }

    /// <summary>Unix milliseconds of the last change; phones ask for requests changed after it.</summary>
    public long UpdatedSeq { get; set; }

    /// <summary>Who approved, rejected or withdrew the request.</summary>
    public Guid? DecidedByUserId { get; set; }

    public string? DecidedByName { get; set; }

    public DateTimeOffset? DecidedAtUtc { get; set; }

    public string? DecisionNote { get; set; }
}

/// <summary>One step in a request's life, kept for the history the approvers read.</summary>
public sealed class ApprovalRequestEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Guid RequestId { get; set; }

    public ApprovalRequest? Request { get; set; }

    /// <summary>One of <see cref="ApprovalActions"/>.</summary>
    public string Action { get; set; } = string.Empty;

    public Guid? ByUserId { get; set; }

    public string? ByName { get; set; }

    public DateTimeOffset AtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Unix milliseconds of <see cref="AtUtc"/>, for ordering on every provider.</summary>
    public long AtSeq { get; set; }

    public string? Note { get; set; }
}

/// <summary>
/// Which operations of a company go to the approval centre. One row per tenant; a
/// tenant without a row requires approval for everything.
/// </summary>
public sealed class TenantApprovalRules
{
    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public bool Sale { get; set; } = true;
    public bool Purchase { get; set; } = true;
    public bool Return { get; set; } = true;
    public bool Collection { get; set; } = true;
    public bool Disbursement { get; set; } = true;
    public bool StockCount { get; set; } = true;
    public bool ProductCard { get; set; } = true;
    public bool CustomerCard { get; set; } = true;

    public string? UpdatedByName { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public bool Requires(string kind) => kind switch
    {
        ApprovalKinds.Sale => Sale,
        ApprovalKinds.Purchase => Purchase,
        ApprovalKinds.Return => Return,
        ApprovalKinds.Collection => Collection,
        ApprovalKinds.Disbursement => Disbursement,
        ApprovalKinds.StockCount => StockCount,
        ApprovalKinds.ProductCard => ProductCard,
        ApprovalKinds.CustomerCard => CustomerCard,
        _ => false,
    };

    public void Set(string kind, bool required)
    {
        switch (kind)
        {
            case ApprovalKinds.Sale: Sale = required; break;
            case ApprovalKinds.Purchase: Purchase = required; break;
            case ApprovalKinds.Return: Return = required; break;
            case ApprovalKinds.Collection: Collection = required; break;
            case ApprovalKinds.Disbursement: Disbursement = required; break;
            case ApprovalKinds.StockCount: StockCount = required; break;
            case ApprovalKinds.ProductCard: ProductCard = required; break;
            case ApprovalKinds.CustomerCard: CustomerCard = required; break;
            default: throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown approval kind.");
        }
    }

    public Dictionary<string, bool> ToMap() => ApprovalKinds.All.ToDictionary(kind => kind, Requires);
}

public static class ApprovalStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    /// <summary>The requester took it back before anyone decided.</summary>
    public const string Withdrawn = "Withdrawn";

    /// <summary>Rejected, then corrected and sent again as a new request.</summary>
    public const string Resubmitted = "Resubmitted";

    public static readonly IReadOnlyList<string> All = [Pending, Approved, Rejected, Withdrawn, Resubmitted];
}

public static class ApprovalActions
{
    public const string Submitted = "Submitted";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Reopened = "Reopened";
    public const string Withdrawn = "Withdrawn";
    public const string Resubmitted = "Resubmitted";
}

/// <summary>The operations an approval rule can cover, and which documents belong to each.</summary>
public static class ApprovalKinds
{
    public const string Sale = "sale";
    public const string Purchase = "purchase";
    public const string Return = "return";
    public const string Collection = "collection";
    public const string Disbursement = "disbursement";
    public const string StockCount = "stock_count";
    public const string ProductCard = "product_card";
    public const string CustomerCard = "customer_card";

    public static readonly IReadOnlyList<string> All = [Sale, Purchase, Return, Collection, Disbursement, StockCount, ProductCard, CustomerCard];

    /// <summary>Card changes exist only for a company without an ERP.</summary>
    public static readonly IReadOnlySet<string> CardKinds = new HashSet<string> { ProductCard, CustomerCard };

    public static bool IsValid(string? kind) => kind is not null && All.Contains(kind);

    /// <summary>
    /// The kind a directly posted document counts as, or null when no rule covers it
    /// (Excel batches, expenses, other cash movements). A document may name its kind in
    /// <c>approvalKind</c>: the cash movement of a purchase is a <c>disbursement</c>
    /// document but belongs to the purchase.
    /// </summary>
    public static string? ForDocument(string documentType, string payloadJson)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("approvalKind", out var named)
                && named.ValueKind == JsonValueKind.String
                && IsValid(named.GetString()))
                return named.GetString();
        }
        catch (JsonException)
        {
            // An unreadable payload is judged by its type alone.
        }
        return ForDocumentType(documentType);
    }

    public static string? ForDocumentType(string documentType) => documentType.Trim().ToLowerInvariant() switch
    {
        "sales_order" => Sale,
        "purchase_receipt" or "purchase" => Purchase,
        "sales_return" or "return" => Return,
        "collection" => Collection,
        "disbursement" => Disbursement,
        "stock_count" => StockCount,
        "stock_card" or "stock_card_delete" => ProductCard,
        "customer_card" => CustomerCard,
        _ => null,
    };
}
