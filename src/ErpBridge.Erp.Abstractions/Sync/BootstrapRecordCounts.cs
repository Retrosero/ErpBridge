namespace ErpBridge.Erp.Abstractions.Sync;

/// <summary>
/// Lightweight row totals for the data sets included in a bootstrap snapshot.
/// The totals are read by the ERP adapter without materialising bootstrap rows.
/// </summary>
public sealed record BootstrapRecordCounts(
    long Customers,
    long CustomerAddresses,
    long CustomerContacts,
    long Stocks,
    long Barcodes,
    long OpenOrders,
    long CashAndBank,
    long Lookups,
    long Prices,
    long SalesConditions,
    long Inventory,
    long CustomerTransactions,
    long StockTransactions);
