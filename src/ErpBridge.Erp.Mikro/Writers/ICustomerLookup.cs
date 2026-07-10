namespace ErpBridge.Erp.Mikro.Writers;

/// <summary>
/// Verifies that a Mikro cari (customer) exists for the supplied code. The MVP
/// implementation is in-memory; the production implementation in Phase 5+ will
/// hit the <c>CARI_HESAPLAR</c> table via <c>Microsoft.Data.SqlClient</c>.
/// </summary>
public interface ICustomerLookup
{
    /// <summary>True when a customer with this code is known to the ERP.</summary>
    Task<bool> ExistsAsync(string customerCode, CancellationToken ct = default);
}

/// <summary>
/// Verifies that a Mikro stok (stock) item exists for the supplied code.
/// </summary>
public interface IStockLookup
{
    /// <summary>True when a stock item with this code is known to the ERP.</summary>
    Task<bool> ExistsAsync(string stockCode, CancellationToken ct = default);
}

/// <summary>
/// Verifies that a Mikro depo (warehouse) exists for the supplied number.
/// </summary>
public interface IWarehouseLookup
{
    /// <summary>True when a warehouse with this number is known to the ERP.</summary>
    Task<bool> ExistsAsync(int warehouseNo, CancellationToken ct = default);
}
