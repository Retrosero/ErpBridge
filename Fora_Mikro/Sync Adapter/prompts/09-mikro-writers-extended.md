# 09 - Mikro Writers Extended Prompt

Implement extended Mikro writers after sales MVP is stable.

Writers:

- `ReturnWriter`
- `PurchaseInvoiceWriter`
- `PurchaseDispatchWriter`
- `ProformaOrderWriter`
- `WarehouseTransferWriter`
- `StockCountWriter`
- `ServiceRequestWriter`

Table targets:

- Returns: sales/purchase movement tables with correct return flags.
- Purchases: `CARI_HESAP_HAREKETLERI` and `STOK_HAREKETLERI`.
- Proforma: `PROFORMA_SIPARISLER`.
- Warehouse transfer: `STOK_HAREKETLERI`.
- Stock count: `SAYIM_SONUCLARI`.
- Service request: `BAKIM_KABUL_HAREKETLERI`.

Rules:

- Do not add a writer without idempotency tests.
- Do not reuse sales flags blindly; document type flags must be explicit.
- Each writer must document touched Mikro tables.

Tests:

- One happy path per writer.
- One rollback test per writer.
- One duplicate replay test per writer.
