namespace ErpBridge.Erp.Abstractions.Sync;

/// <summary>
/// Açık sipariş (open / unfulfilled sales order) record carried inside a
/// <see cref="SyncPackage"/>. The agent surfaces these so the central API can
/// answer "what did customer X already order?" queries without a direct
/// Mikro connection. <see cref="RemainingQuantity"/> is denormalised because
/// the central API uses it for fast availability checks.
/// </summary>
/// <param name="Series">Evrak serisi (e.g. "S", "SS").</param>
/// <param name="Number">Evrak numarası.</param>
/// <param name="LineNo">Satır numarası (1-based within the order).</param>
/// <param name="CustomerCode">Cari kodu on the order header.</param>
/// <param name="StockCode">Stok kodu on the order line.</param>
/// <param name="Quantity">Original ordered miktar.</param>
/// <param name="DeliveredQuantity">Sevk / teslim edilen miktar (cumulative).</param>
/// <param name="RemainingQuantity">Quantity - DeliveredQuantity (denormalised).</param>
/// <param name="WarehouseNo">Depo numarası for fulfilment.</param>
/// <param name="SalespersonCode">Plasiyer kodu on the order header.</param>
/// <param name="OrderDate">Sipariş tarihi.</param>
/// <param name="DeliveryDate">Optional teslim tarihi.</param>
/// <param name="TotalAmount">Optional line total in the order currency.</param>
public sealed record OpenOrderPayload(
    string Series,
    int Number,
    int LineNo,
    string? CustomerCode,
    string StockCode,
    decimal Quantity,
    decimal DeliveredQuantity,
    decimal RemainingQuantity,
    int WarehouseNo,
    string? SalespersonCode,
    DateOnly OrderDate,
    DateOnly? DeliveryDate,
    decimal? TotalAmount);
