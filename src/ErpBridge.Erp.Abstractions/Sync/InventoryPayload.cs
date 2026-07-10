namespace ErpBridge.Erp.Abstractions.Sync;

/// <summary>
/// Stok envanter (inventory) snapshot for a single (stok, depo) pair. The
/// central API uses this to drive stock-availability checks on incoming
/// sales orders without round-tripping the agent.
/// </summary>
/// <param name="StockCode">Stok kodu the inventory row refers to.</param>
/// <param name="WarehouseNo">Depo numarası the inventory is held at.</param>
/// <param name="Quantity">Anlık miktar (on-hand) — may be negative for back-orders.</param>
/// <param name="ReservedQuantity">Rezerve miktar (allocated to open orders).</param>
/// <param name="LastMovementDate">Last <c>STOK_HAREKETLERI</c> date for the pair.</param>
public sealed record InventoryPayload(
    string StockCode,
    int WarehouseNo,
    decimal Quantity,
    decimal ReservedQuantity,
    DateOnly LastMovementDate);
