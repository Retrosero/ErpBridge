# 08 - Mikro Writers Sales Prompt

Implement Mikro writers for sales MVP.

Writers:

- `SalesOrderWriter`
- `SalesInvoiceWriter`
- `SalesDispatchWriter`
- `CollectionWriter`

Table targets:

- Sales order: `SIPARISLER`.
- Sales invoice: `CARI_HESAP_HAREKETLERI` and `STOK_HAREKETLERI`.
- Sales dispatch: `STOK_HAREKETLERI`.
- Collection: `CARI_HESAP_HAREKETLERI`, optional `ODEME_EMIRLERI`.

Rules:

- Each writer runs inside one SQL transaction.
- Idempotency mapping is checked before insert.
- Seri/sira conflict must fail as duplicate, not create another document.
- Order fulfillment updates delivery quantities in the same transaction.
- V15/V16 linkage is delegated to version adapters.

Tests:

- Duplicate job creates no second evrak.
- Invoice rollback when one stock line fails.
- Collection with cheque writes payment order.
- Dispatch updates related order quantity.
