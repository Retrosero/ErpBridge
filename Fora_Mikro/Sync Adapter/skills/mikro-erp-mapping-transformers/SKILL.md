---
name: mikro-erp-mapping-transformers
description: Implement Mikro ERP mapping, transformer, transaction, idempotency, and queue-processing logic for sales, collections, purchases, returns, stock, customer, price, barcode, and movement synchronization.
---

# Mikro ERP Mapping Transformers

Use this skill whenever code reads or writes Mikro ERP tables or translates Android/central API payloads into Mikro columns.

## Required Pattern

1. Validate payload and required mappings before opening a Mikro transaction.
2. Check idempotency before insert using `tenant_id + entity_type + external_id`.
3. Transform JSON payload into explicit Mikro document models.
4. Write all related Mikro rows in one transaction.
5. Update V15 `*_RECid_RECno` or V16 `*_uid` linkage after primary row identity is known.
6. Save mapping after ERP transaction succeeds.
7. Ack the central API only after local success is durable.

## Entity Rules

- Sales invoices write `CARI_HESAP_HAREKETLERI` and `STOK_HAREKETLERI`.
- Sales dispatches write `STOK_HAREKETLERI`.
- Sales orders write `SIPARISLER`.
- Collections write `CARI_HESAP_HAREKETLERI`; cheque, note, transfer or credit card flows also use `ODEME_EMIRLERI`.
- Purchases and returns follow the same transaction pattern with correct Mikro flags.
- Stock data uses `STOKLAR`, `STOK_SATIS_FIYAT_LISTELERI` and `BARKOD_TANIMLARI`.
- Customers use `CARI_HESAPLAR` and optional address/contact tables.

## Non-Negotiables

- Do not create duplicate Mikro documents when a job is replayed.
- Do not partially write document headers without lines.
- Do not advance checkpoints before the receiving system confirms success.
- Do not generate SQL with concatenated user input.
