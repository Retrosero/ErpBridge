namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Stock on hand of one product in one warehouse, for a tenant without an ERP.
///
/// <para>For ERP tenants the ERP owns this number and it only travels through
/// <see cref="MobileRecord"/> as JSON. A native tenant has no ERP, so the central
/// API keeps the authoritative value here, in a typed column it can change with
/// exact arithmetic inside a transaction, and projects the result to devices.</para>
/// </summary>
public sealed class NativeStockLevel
{
    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string StockCode { get; set; } = string.Empty;

    public int WarehouseNo { get; set; } = NativeLedgerDefaults.WarehouseNo;

    /// <summary>May go negative: a sale made offline is never refused after the fact.</summary>
    public decimal Quantity { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Open balance of one customer for a tenant without an ERP. Positive means the
/// customer owes the company (debit), matching the ERP convention the app shows.
/// </summary>
public sealed class NativeCustomerBalance
{
    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string CustomerCode { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public static class NativeLedgerDefaults
{
    /// <summary>A native tenant starts with a single warehouse.</summary>
    public const int WarehouseNo = 1;
}
