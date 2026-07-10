namespace ErpBridge.Erp.Abstractions.SalesOrder;

/// <summary>
/// One line of a <see cref="SalesOrderPayload"/>. Up to six discount rates per line,
/// applied in Mikro-native order.
/// </summary>
/// <param name="StockCode">Stok kodu — must be non-empty.</param>
/// <param name="Quantity">Miktar — must be &gt; 0.</param>
/// <param name="UnitPointer">Birim pointer (1=adet, 2=kilo, ...).</param>
/// <param name="UnitPrice">Birim fiyat.</param>
/// <param name="TaxPointer">KDV pointer.</param>
/// <param name="Discounts">Up to six discount rates.</param>
public sealed record SalesOrderLinePayload(
    string StockCode,
    decimal Quantity,
    int UnitPointer,
    decimal UnitPrice,
    int TaxPointer,
    IReadOnlyList<decimal> Discounts);
