# Deliverable — Cari / Stok Kart Açma (Wave 4C)

> **Faz:** Wave 4C — Adapter-level CREATE
> **Tarih:** 2026-09-08
> **Build:** 0 hata / 0 uyarı (`dotnet build ErpBridge.sln`)
> **Test:** 567 toplam, 567 geçti, 0 başarısız, 20 skip (MSSQL docker integration, env-var gated)

## 1. Kapsam

İki yeni ERP adapter writer'ı — saha satış temsilcisi Mikro'da henüz tanımlı olmayan
bir cari veya stok kodu gönderdiğinde, Windows Agent otomatik olarak yeni kartı
açar; aynı `externalId` veya aynı `cari_kod` / `sto_kod` ile ikinci çağrıda
INSERT yapılmaz (idempotent).

1. **MikroCustomerCardWriter** — `CARI_HESAPLAR` INSERT, V15 RECno / V16 Guid
   dispatch, tek transaction, mapping store + duplicate-key idempotency.
2. **MikroStockCardWriter** — `STOKLAR` INSERT + opsiyonel `BARKOD_TANIMLARI`
   INSERT, aynı pattern.

Bu writer'lar **CentralApi ingest endpoint'ine BAĞLANMAZ** (kapsam dışı). Agent
yerel tarafta, saha satış uygulaması Mikro'ya yazmadan önce kart açma adımını
çoğunlukla otomatik olarak yapar. Mapping store şeması değişmedi; sadece
yeni `(documentType="customer_card" | "stock_card")` satırları eklenir.

## 2. Mimari

```
┌──────────────────┐                                      ┌──────────────────┐
│ Saha satış       │  CreateCustomerRequest /            │ Windows Agent    │
│ uygulaması       │  CreateStockRequest                  │ (local)          │
│                  │ ──────────────────────────────────▶  │                  │
│                  │                                      │ ┌──────────────┐ │
│                  │                                      │ │ Mapping      │ │
│                  │                                      │ │ Store lookup │ │
│                  │                                      │ └──────┬───────┘ │
│                  │                                      │        │ miss    │
│                  │                                      │ ┌──────▼───────┐ │
│                  │                                      │ │ Mikro        │ │
│                  │                                      │ │ V15/V16 dup  │ │
│                  │                                      │ │ key probe    │ │
│                  │                                      │ └──────┬───────┘ │
│                  │                                      │        │ miss    │
│                  │                                      │ ┌──────▼───────┐ │
│                  │  CreateResult                        │ │ INSERT       │ │
│                  │ ◀─────────────────────────────────── │ │ CARI_HESAPLAR│ │
│                  │  (NewRECno | NewUid, Created)        │ │ / STOKLAR    │ │
│                  │                                      │ │ + barkod?    │ │
│                  │                                      │ └──────┬───────┘ │
│                  │                                      │        │         │
│                  │                                      │ ┌──────▼───────┐ │
│                  │                                      │ │ Mapping      │ │
│                  │                                      │ │ Store save   │ │
│                  │                                      │ └──────────────┘ │
└──────────────────┘                                      └──────────────────┘
```

V15 ve V16 dispatch `IMikroIdentityStrategy` (Recno / Guid) üzerinden çözülür;
Core / Service / UI bu farkı görmez. `ConnectionSettings.CompanyNo` ve
`BranchNo` her iki path'te header INSERT'ine yazılır (Faz 10.5).

## 3. Değişen / yeni dosyalar

### Yeni dosyalar (6)

**Domain (Core):**
- `src/ErpBridge.Core/Domain/CreateCustomerRequest.cs` — Mikro'ya yeni cari
  INSERT'i için taşıyıcı DTO (`ExternalId`, `TenantId`, `CustomerCode`,
  `CustomerName`, opsiyonel `TaxNumber` / `TaxOffice` / `Address` / `Phone1`
  / `Phone2` / `Email` / `ContactPerson`, `Currency` TRY default,
  `PaymentTermDays` 0 default, `GroupCode` 1 default).
- `src/ErpBridge.Core/Domain/CreateStockRequest.cs` — Mikro'ya yeni stok
  INSERT'i için taşıyıcı DTO (`ExternalId`, `TenantId`, `StockCode`,
  `StockName`, `Unit` ADET default, opsiyonel `Barcode`, `VatRate` 20 default,
  `GroupCode` 1 default, `SalePrice1..3`, `WarehouseNo` 1 default).

**Mikro writer:**
- `src/ErpBridge.Erp.Mikro/Writers/MikroCustomerCardWriter.cs` — V15/V16
  dispatcher, `CARI_HESAPLAR` INSERT, V15'te self-link UPDATE, mapping save,
  `CreateResult` record, `MikroCardValidationException` (validation hatası
  için typed exception — `ErpWriteResult` shape'i ile aynı `ErrorCode`).
- `src/ErpBridge.Erp.Mikro/Writers/MikroStockCardWriter.cs` — Aynı pattern
  + `BARKOD_TANIMLARI` opsiyonel INSERT (V15: RECno link; V16: Guid link).

**Tests (2):**
- `tests/ErpBridge.Erp.Mikro.Tests/Writers/MikroCustomerCardWriterTests.cs`
  — 11 yeni test (constants / validation × 3 / idempotency × 2 / V15+V16
  dispatcher / duplicate-key / CompanyNo+BranchNo / secret-mask).
- `tests/ErpBridge.Erp.Mikro.Tests/Writers/MikroStockCardWriterTests.cs`
  — 10 yeni test (constants / validation × 2 / idempotency / V15+V16
  dispatcher / barcode-with / barcode-without / CompanyNo+BranchNo /
  secret-mask).

### Değişen dosyalar (1)

- `src/ErpBridge.Erp.Mikro/DependencyInjection/ServiceCollectionExtensions.cs`
  — `services.AddSingleton<MikroCustomerCardWriter>()` +
  `services.AddSingleton<MikroStockCardWriter>()` ve iki yeni
  `TryAddSingletonLogger<>` satırı. Mevcut pattern birebir takip edildi
  (Wave 4 ile aynı stil).

## 4. Wire şeması

Adapter-local API, HTTP üzerinden değil. Windows Agent içinden çağrılır:

| Çağrı | Girdi | Çıktı |
|------|-------|-------|
| `writer.CreateCustomerAsync(req, mappings, settings, ct)` | `CreateCustomerRequest` + `IMappingStore` + `MikroConnectionSettings` | `CreateResult(NewRECno, NewUid, Created)` veya `MikroCardValidationException` |
| `writer.CreateStockAsync(req, mappings, settings, ct)` | `CreateStockRequest` + `IMappingStore` + `MikroConnectionSettings` | `CreateResult(NewRECno, NewUid, Created)` veya `MikroCardValidationException` |

| Yön | Method | URL | Body | Response |
|------|--------|-----|------|---------:|
| agent → Mikro SQL | INSERT | `CARI_HESAPLAR` | V15 SCOPE_IDENTITY / V16 Guid | recno / guid + mapping save |
| agent → Mikro SQL | INSERT | `STOKLAR` | V15 / V16 | aynı |
| agent → Mikro SQL | INSERT | `BARKOD_TANIMLARI` (opsiyonel) | V15 RECno link / V16 Guid link | aynı |

## 5. Idempotency katmanları

İki seviyeli kısayol:

1. **Mapping store (externalId).** Aynı `(TenantId, documentType, ExternalId)`
   çifti ikinci kez gelirse Mikro'ya hiç bağlanılmaz; önceki identifier
   `Created=false` ile dönülür.
2. **Duplicate-key (code-level).** Mapping miss sonrası aynı transaction
   içinde `CARI_HESAPLAR` / `STOKLAR` tablosunda `cari_kod` / `sto_kod`
   (firma + şube skobuyla) aranır. Bulunursa INSERT yapılmaz, mevcut
   identifier dönülür, mapping yine de kaydedilir
   (`Created=false`).

## 6. V15/V16 dispatch

Adapter seviyesinde, Core / Service / UI görmez:

- **V15** — `SCOPE_IDENTITY()` ile `cari_RECno` / `sto_RECno`. INSERT
  sırasında `*_RECid_DBCno = 0` + `*_RECid_RECno = 0` placeholder'ı
  setlenir; SCOPE_IDENTITY() döndükten sonra aynı transaction içinde
  `*_RECid_RECno` yeni RECno'ya eşitlenir (self-link UPDATE).
- **V16** — Client-üretilmiş `Guid.NewGuid()` INSERT'ten önce
  bağlanır; `cari_Guid` / `sto_Guid` kolonuna yazılır. SCOPE_IDENTITY
  round-trip'i yok.

`ConnectionSettings.CompanyNo` + `BranchNo` her iki path'te header INSERT'ine
yazılır (`cari_firmano` / `cari_sube_no` / `sto_firmano` / `sto_sube_no`).
Faz 10.5 multi-firma gereksinimi bu katmanda tamamlanmış olur.

## 7. `CreateResult`

```csharp
public sealed record CreateResult(int NewRECno, Guid? NewUid, bool Created);
```

- `NewRECno` — V15 RECno; V16 path'inde `0`.
- `NewUid` — V16 Guid; V15 path'inde `null`.
- `Created` — `true`: yeni INSERT commit edildi. `false`: mapping hit veya
  duplicate-key short-circuit.

## 8. Test özeti

| Suite | Önce | Yeni | Sonra |
|-------|------|------|-------|
| `ErpBridge.Erp.Mikro.Tests.Writers.MikroCustomerCardWriterTests` | 0 | 11 | 11 |
| `ErpBridge.Erp.Mikro.Tests.Writers.MikroStockCardWriterTests` | 0 | 10 | 10 |
| **Yeni toplam** | | **21** | |
| ErpBridge.Core.Tests (regression) | 73 | — | 73 ✅ |
| ErpBridge.LocalStore.Tests (regression) | 50 | — | 50 ✅ |
| ErpBridge.RemoteApi.Tests (regression) | 26 | — | 26 ✅ |
| ErpBridge.Agent.Service.Tests (regression) | 8 | — | 8 ✅ |
| ErpBridge.Admin.Tests (regression) | 30 | — | 30 ✅ |
| ErpBridge.CentralApi.Tests (regression) | 162 → 171 | — | 171 ✅ |
| ErpBridge.Shared.Tests (regression) | 22 | — | 22 ✅ |
| ErpBridge.Erp.Mikro.Tests (regression) | 137 → 166 | +21 | 187 ✅ (20 skip korundu) |
| **Toplam** | **508** | **+21** | **567** |

> Not: `ErpBridge.CentralApi.Tests` Faz 10.6+7'de 162 → 171'e yükselmişti
> (bu deliverable kapsamı dışı); burada regression baseline olarak raporlanır.

Yeni test isimleri (özet):

- `MikroCustomerCardWriterTests` (11):
  `Constants_expected_by_implementation_are_stable`,
  `Empty_CustomerCode_throws_validation_exception`,
  `Empty_Currency_throws_validation_exception`,
  `Negative_PaymentTermDays_throws_validation_exception`,
  `Existing_mapping_returns_idempotent_ack_without_insert`,
  `Existing_mapping_V15_recno_is_returned_unchanged`,
  `CreateCustomer_V15_uses_RecnoStrategy_and_propagates_CompanyNo_BranchNo`,
  `CreateCustomer_V16_uses_GuidStrategy_and_emits_header_guid`,
  `Duplicate_code_returns_existing_id_idempotent`,
  `CreateCustomer_propagates_CompanyNo_and_BranchNo`,
  `SqlException_message_is_masked_before_logging`.

- `MikroStockCardWriterTests` (10):
  `Constants_expected_by_implementation_are_stable`,
  `Empty_StockCode_throws_validation_exception`,
  `VatRate_out_of_range_throws_validation_exception`,
  `Existing_mapping_returns_idempotent_ack_without_insert`,
  `CreateStock_V15_uses_RecnoStrategy_and_propagates_CompanyNo_BranchNo`,
  `CreateStock_V16_uses_GuidStrategy_and_emits_header_guid`,
  `CreateStock_with_barcode_surfaces_hasBarcode_in_audit_log`,
  `CreateStock_without_barcode_surfaces_hasBarcode_in_audit_log`,
  `CreateStock_propagates_CompanyNo_and_BranchNo`,
  `SqlException_message_is_masked_before_logging`.

## 9. Bilinen sınırlar

- **Cross-DB atomicity yok (Faz 6 sınırı).** SQLite mapping save + SQL
  Server tx ayrı; mapping save başarısız olursa Mikro'da evrak oluşur ama
  mapping kaydı yoktur → sonraki retry idempotency hit bulamaz. Wave 5B'de
  reconciliation worker eklenecek.
- **Mapping key firma/şube'den bağımsız.** `IdempotencyMappingStore` şeması
  Faz 10.5'te bilinçli olarak değiştirilmedi; müşteri tek-firmalı başlar,
  multi-firma gelince ayrı bir track.
- **Self-link UPDATE V15'te ek round-trip.** Identity round-trip'ine ek
  olarak `*_RECid_RECno`'yu setlemek için aynı transaction içinde bir
  UPDATE gerekiyor. Prod şemasında `*_RECid_DBCno` / `*_RECid_RECno`
  kolonları NULL ise UPDATE atlanabilir; bu optimizasyon ayrı bir track.
- **Integration tests bu PR'a dahil değil.** `MikroCustomerCardWriter` /
  `MikroStockCardWriter` için canlı SQL Server integration testleri
  `tests/ErpBridge.Erp.Mikro.Tests/Integration/`'a Faz 16+ kapsamında
  eklenecek; bu deliverable hermetic test tier'ı ile sınırlı.

## 10. Build & test

```
$ dotnet build ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor
  → 0 Uyarı, 0 Hata (süre ~7s)

$ dotnet test ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor --no-build
  → Tüm proje özetleri:
    ErpBridge.Shared.Tests        22 passed / 0 failed / 0 skipped
    ErpBridge.LocalStore.Tests    50 passed / 0 failed / 0 skipped
    ErpBridge.RemoteApi.Tests     26 passed / 0 failed / 0 skipped
    ErpBridge.Agent.Service.Tests  8 passed / 0 failed / 0 skipped
    ErpBridge.Admin.Tests         30 passed / 0 failed / 0 skipped
    ErpBridge.Core.Tests          73 passed / 0 failed / 0 skipped
    ErpBridge.CentralApi.Tests   171 passed / 0 failed / 0 skipped
    ErpBridge.Erp.Mikro.Tests   187 passed / 0 failed / 20 skipped
  → Toplam: 567 passed / 0 failed / 20 skipped
```

## 11. Sonraki adımlar

- İrsaliye (STOK_HAREKETLERI / SIPARISLER) — Wave 4A
- Fatura (CARI_HESAP_HAREKETLERI + STOK_HAREKETLERI) — Wave 4B
- Cross-DB atomicity reconciliation worker — Wave 5B
- Integration tests (canlı SQL Server fixture'lara karşı) — Faz 16+
