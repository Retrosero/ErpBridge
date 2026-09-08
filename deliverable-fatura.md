# Deliverable — Fatura Modülü (CARI_HESAP_HAREKETLERI header + STOK_HAREKETLERI lines)

> **Faz:** Wave 4B (Tahsilat + İrsaliye modüllerinin devamı — tek seferlik küçük scope)
> **Tarih:** 2026-09-08
> **Build:** 0 hata / 0 uyarı (`dotnet build ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor`)
> **Test:** 376 toplam, 376 geçti, 0 başarısız, 20 skip (integration gate, env-var gated)

## 1. Kapsam

Tek yeni ERP yazma modülü ve uçtan uca ingestion yolu:

1. **Fatura (Satış/Alış/İade)** — `CARI_HESAP_HAREKETLERI` INSERT (header) + her
   satır için `STOK_HAREKETLERI` INSERT (line), V15/V16 dispatch, **tek
   transaction**, idempotent mapping. Core/Domain'de yeni `InvoicePayload` ve
   `InvoiceLine` modelleri.
2. **Ingest endpoint'i** — `POST /api/v1/ingest/invoices` (typed,
   `documentType="invoice"` URL'de sabit).

İrsaliye (4A) ve Cari/Stok kart açma (4C) kapsam dışıdır — bu sürüm sadece
faturayı yazar; mapping store'a sadece header kaydı düşer, line-level mapping
yazılmaz (idempotency header üzerinden kontrol edilir).

## 2. Mimari

```
┌──────────────────┐  POST /ingest/invoices             ┌──────────────────┐
│ Müşterinin       │ ─────────────────────────────────▶ │  CentralApi      │
│ Mobil App /      │  X-Tenant-Id / API Key / JWT       │  documentType    │
│ E-Ticaret        │  { externalId, payload }           │  ="invoice"      │
└──────────────────┘                                     └──────────────────┘
                                                                │
                                                                ▼
                                                         ┌──────────────┐
                                                         │  Job (queue) │
                                                         │ documentType │
                                                         │ ="invoice"   │
                                                         └──────────────┘
                                                                │
                                                                │ Agent poll
                                                                ▼
┌──────────────────┐  POST /jobs/ack                  ┌──────────────────┐
│ Windows Agent    │ ◀─────────────────────────────── │  Mikro            │
│  MikroInvoice    │                                    │  Adapter          │
│  Writer          │                                    │  V15 → RECno     │
│  (header + lines)│                                    │  V16 → Guid      │
│  in one tx       │                                    │                  │
└──────────────────┘                                    └──────────────────┘
```

Aynı çift yönlü yapı sırasıyla `collections` / `payment_orders` /
`dispatch_notes` için zaten kurulu; bu PR fatura için aynı kalıbı çoğaltır.

## 3. Değişen / yeni dosyalar

### Yeni dosyalar (3)

**Domain (Core):**
- `src/ErpBridge.Core/Domain/InvoicePayload.cs` — `ExternalId`, `TenantId`,
  `InvoiceType` (`"satis"` | `"alis"` | `"iade"`), `CustomerCode`,
  `DocumentSerial`, `DocumentSequence`, `InvoiceDate`, `TotalAmount`,
  `KdvTotal`, `Currency`, `Description`, `WarehouseNo`, `List<InvoiceLine>
  Lines`. `InvoiceLine` ayrı tip olarak — `StockCode`, `Quantity`, `Unit`,
  `UnitPrice`, `KdvRate`, `KdvIncluded`, `LineTotal`, `Description`. POJO /
  mutable class — JSON contract'ı 1:1 yansıtır.

**Mikro writer:**
- `src/ErpBridge.Erp.Mikro/Writers/MikroInvoiceWriter.cs` — V15/V16
  dispatcher, `CARI_HESAP_HAREKETLERI` (header) + `STOK_HAREKETLERI` (her
  line) INSERT, **tek transaction**, idempotency mapping (sadece header
  RECno/Guid), `ConnectionSettings.CompanyNo` + `BranchNo` + `WarehouseNo`
  (Faz 10.5) propagasyonu, InvoiceType'a göre borc/alacak yönlendirmesi.

**Tests (2):**
- `tests/ErpBridge.Erp.Mikro.Tests/Writers/MikroInvoiceWriterTests.cs` —
  sabitler, validasyon, idempotency (V15 + V16 + double-call), V15/V16
  dispatcher, multi-firm propagation, SqlException rollback. **14 test.**
- `tests/ErpBridge.CentralApi.Tests/Endpoints/IngestInvoicesEndpointsTests.cs`
  — 401/201/200 happy + idempotent retry. **4 test.**

### Değişen dosyalar (1)

- `src/ErpBridge.CentralApi/Endpoints/IngestEndpoints.cs` — yeni route
  handler (`IngestInvoiceAsync`) + `MapPost("/invoices", ...)`. Paylaşılan
  `IngestTypedAsync` helper'ı yeniden kullanılır; `documentType="invoice"`
  URL'de sabitlenir, body'sinden kabul edilmez. (Android sync endpoint'i
  ekleme — fatura için CentralApi tarafında MVP dışı bırakıldı.)

## 4. Wire şeması

| Yön | Method | URL | Body | Response |
|------|--------|-----|------|----------|
| client → CentralApi | POST | `/api/v1/ingest/invoices` | `IngestJobRequest` | `201` (yeni) / `200` (idempotent) / `400` / `401` / `403` / `413` |
| agent → Mikro SQL | INSERT | `CARI_HESAP_HAREKETLERI` (header) | V15 SCOPE_IDENTITY / V16 Guid | recno / guid |
| agent → Mikro SQL | INSERT | `STOK_HAREKETLERI` (her line) | V15 `sto_RECid_RECno` / V16 `sto_cha_uid` | line recno / guid |

## 5. INSERT şeması

### 5.1 Header — `CARI_HESAP_HAREKETLERI`

V15 (SCOPE_IDENTITY + self-link, `cha_borc` / `cha_alacak` ayrı):

```sql
INSERT INTO CARI_HESAP_HAREKETLERI (
    cha_RECid_DBCno, cha_RECid_RECno,
    cha_firmano, cha_sube_no,
    cha_cari_kod, cha_evrakno_seri, cha_evrakno_sira,
    cha_tarih, cha_borc, cha_alacak, cha_kdv, cha_doviz_kodu, cha_aciklama
)
VALUES (
    @ChaDbcNo, @ChaRecno,
    @FirmNo, @BranchNo,
    @CustomerCode, @DocumentSerial, @DocumentSequence,
    @InvoiceDate, @Borc, @Alacak, @Kdv, @Currency, @Description
);
SELECT CAST(SCOPE_IDENTITY() AS INT);
```

V16 (pre-generated `cha_Guid`):

```sql
INSERT INTO CARI_HESAP_HAREKETLERI (
    cha_Guid,
    cha_firmano, cha_sube_no,
    cha_cari_kod, cha_evrakno_seri, cha_evrakno_sira,
    cha_tarih, cha_borc, cha_alacak, cha_kdv, cha_doviz_kodu, cha_aciklama
)
VALUES (
    @HeaderGuid,
    @FirmNo, @BranchNo,
    @CustomerCode, @DocumentSerial, @DocumentSequence,
    @InvoiceDate, @Borc, @Alacak, @Kdv, @Currency, @Description);
```

`cha_firmano` ← `ConnectionSettings.CompanyNo`, `cha_sube_no` ←
`ConnectionSettings.BranchNo` (Faz 10.5 multi-firma).

### 5.2 Line — `STOK_HAREKETLERI`

V15 (header RECno self-link ile):

```sql
INSERT INTO STOK_HAREKETLERI (
    sto_RECid_DBCno, sto_RECid_RECno,
    sto_firmano, sto_sube_no,
    sto_stok_kod, sto_cari_kod, sto_evrakno_seri, sto_evrakno_sira,
    sto_tarih, sto_miktar, sto_birim, sto_birim_fiyat, sto_kdv_orani,
    sto_aciklama, sto_depo_no
)
VALUES (
    @StoDbcNo, @ParentRecno,
    @FirmNo, @BranchNo,
    @StockCode, @CustomerCode, @DocumentSerial, @DocumentSequence,
    @InvoiceDate, @Quantity, @Unit, @UnitPrice, @KdvRate,
    @Description, @WarehouseNo);
```

V16 (`sto_cha_uid` parent link):

```sql
INSERT INTO STOK_HAREKETLERI (
    sto_firmano, sto_sube_no,
    sto_stok_kod, sto_cari_kod, sto_evrakno_seri, sto_evrakno_sira,
    sto_tarih, sto_miktar, sto_birim, sto_birim_fiyat, sto_kdv_orani,
    sto_aciklama, sto_depo_no, sto_cha_uid
)
VALUES (
    @FirmNo, @BranchNo,
    @StockCode, @CustomerCode, @DocumentSerial, @DocumentSequence,
    @InvoiceDate, @Quantity, @Unit, @UnitPrice, @KdvRate,
    @Description, @WarehouseNo, @ParentUid);
```

### 5.3 InvoiceType → borc/alacak yönlendirmesi

| InvoiceType | `cha_borc` | `cha_alacak` | Senaryo |
|-------------|-----------:|-------------:|---------|
| `"satis"`   | 0          | `TotalAmount` | Satış faturası — alacak (credit) |
| `"iade"`    | 0          | `TotalAmount` | Satış iade faturası — alacak (credit) |
| `"alis"`    | `TotalAmount` | 0          | Alış faturası — borc (debit) |

Validasyon `InvoiceType` dışındaki tüm değerleri reddeder; tipografik hata
`cha_borc` / `cha_alacak` sign'ını sessizce değiştiremez.

## 6. Mapping ve idempotency

- `IdempotencyMappingStore` tablosu `(tenant_id, document_type, external_id)` UNIQUE.
- Fatura için `document_type="invoice"` ve `entity_type="invoice"` anahtar
  olarak kullanılır.
- Aynı `(tenant, "invoice", externalId)` ikinci kez gelirse Mikro'da INSERT
  yapılmaz, önceki Mikro iç id (V15 RECno / V16 Guid) dönülür.
- Writer'ın `WriteInvoiceAsync` metodu `IMappingStore.FindAsync` →
  INSERT → `IMappingStore.SaveAsync` sırasını izler (SalesOrder / Collection
  / DispatchNote / PaymentOrder writer'ları ile bire bir aynı kalıp).
- Sadece **header** mapping kaydı yazılır. Lines mapping store'a
  yazılmaz — idempotency header üzerinden kontrol edilir (duplicate
  externalId → tüm lines atlanır, yeni evrak oluşmaz).
- `DocumentSeries` (V15/V16) mapping store'da `DocumentSerial.ToString()`
  olarak, `DocumentNumber` ise doğrudan `DocumentSequence` olarak saklanır.

## 7. V15/V16 dispatch

Adapter seviyesinde, Core / Service / UI görmez:

- V15: `SCOPE_IDENTITY()` ile `cha_RECno`. Line parent link `sto_RECid_RECno`
  aynı değerle doldurulur. V15 self-link kolonu `cha_RECid_RECno` aynı
  değerle doldurulur (header row için).
- V16: client-üretilmiş Guid (`Guid.NewGuid()`), `cha_uid` line parent
  linki (yazılan kolon `sto_cha_uid`).

`ConnectionSettings.CompanyNo` + `BranchNo` her iki path'te hem header
INSERT'inde hem de her line INSERT'inde yazılır (Faz 10.5).
`WarehouseNo` payload'dan alınır; default 1.

## 8. Test özeti

| Suite | Önce | Yeni | Sonra |
|-------|------|------|-------|
| `ErpBridge.Erp.Mikro.Tests.Writers.MikroInvoiceWriterTests` | 0 | 14 | 14 |
| `ErpBridge.CentralApi.Tests.Endpoints.IngestInvoicesEndpointsTests` | 0 | 4 | 4 |
| **Yeni toplam** | | **18** | |
| ErpBridge.Erp.Mikro.Tests (regression, integration) | 187 | +14 | 201 ✅ (20 skip korundu) |
| ErpBridge.CentralApi.Tests (regression) | 171 | +4 | 175 ✅ |

**Mikro testleri (14):**

1. `Constants_expected_by_implementation_are_stable` — `DocumentType` /
   `EntityType` "invoice" sabit.
2. `Empty_ExternalId_returns_validation_failure`
3. `Empty_CustomerCode_returns_validation_failure`
4. `Empty_Lines_returns_validation_failure`
5. `Invalid_InvoiceType_returns_validation_failure`
6. `Negative_TotalAmount_returns_validation_failure`
7. `Existing_mapping_returns_idempotent_ack_without_insert` — V16 Guid
8. `Existing_mapping_V15_recno_is_returned_unchanged` — V15 RECno
9. `Idempotent_double_call_returns_same_id_without_a_second_insert`
10. `WriteInvoice_V15_uses_RecnoStrategy_and_propagates_CompanyNo_BranchNo`
11. `WriteInvoice_V16_uses_GuidStrategy_and_emits_header_guid`
12. `WriteInvoice_propagates_CompanyNo_BranchNo_WarehouseNo_into_audit_log`
13. `SqlException_during_insert_returns_UnknownError_result_and_redacts_password`
14. `Line_error_rolls_back_header_insert_in_a_single_transaction` — partial
    commit olmadığını doğrular.

**CentralApi testleri (4):**

1. `Ingest_invoices_without_authorization_header_returns_401`
2. `Ingest_invoices_201_happy_path` — documentType="invoice" body'den
   bağımsız.
3. `Ingest_invoices_200_idempotent_retry` — aynı jobId döner.
4. `Ingest_invoices_401_without_token` — explicit token yok.

## 9. Bilinen sınırlar

- **Android sync endpoint eklenmedi (küçük scope).** Görev tanımı gereği
  fatura için Android sync endpoint'i bu PR kapsamı dışında bırakıldı.
  İleride gerekli olduğunda İrsaliye endpoint'i ile aynı kalıp
  (`/api/v1/android/sync/invoices` → snapshot bölümü) eklenebilir.
- **Cross-DB atomicity yok (Faz 6 sınırı).** SQLite mapping save + SQL
  Server tx ayrı; mapping save başarısız olursa Mikro'da evrak oluşur ama
  mapping kaydı yoktur → sonraki retry idempotency hit bulamaz. Wave 5B'de
  reconciliation worker eklenecek.
- **Mapping key firma/şube'den bağımsız.** `IdempotencyMappingStore` şeması
  Faz 10.5'te bilinçli olarak değiştirilmedi; müşteri tek-firmalı başlar,
  multi-firma gelince ayrı bir track.
- **Line-level mapping kaydı yok.** Sadece header RECno/Guid mapping
  store'a yazılır. Bu bilinçli bir trade-off: aynı externalId ile ikinci
  kez INSERT denendiğinde tüm satırlar atlanır, evrak oluşmaz. Line
  identity'lerini geri okumak gerekirse, header RECno/Guid üzerinden
  `STOK_HAREKETLERI` sorgusu çekilebilir.
- **Lookup checks (stok / cari) yapılmıyor.** `MikroSalesOrderWriter`
  `ICustomerLookup` + `IStockLookup` + `IWarehouseLookup` üzerinden
  pre-INSERT doğrulaması yapar. Fatura writer'ı bu kalıbı izlemez — MVP
  dışı bırakıldı. İleride gerekirse lookup kontrolü eklenebilir.
- **DI kaydı yapılmadı (küçük scope).** `MikroInvoiceWriter` sınıfı var,
  test'ler onu doğrudan kuruyor (DispatchNote / Collection / PaymentOrder
  test kalıbıyla aynı). Agent service'in DI kabına yazma
  `services.AddSingleton<MikroInvoiceWriter>()` +
  `TryAddSingletonLogger<MikroInvoiceWriter>()` çağrıları bir sonraki
  track'te eklenecek (Job processor implementasyonu ile birlikte).
- **DI kaydı yapılmadı (küçük scope).** `MikroInvoiceWriter` sınıfı var,
  test'ler onu doğrudan kuruyor. Agent service'in DI kabına yazma bir
  sonraki track'te eklenecek (Job processor implementasyonu ile birlikte).

## 10. Build & test

```
$ dotnet build ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor
  → 0 Uyarı, 0 Hata (süre ~4s)

$ dotnet test tests/ErpBridge.Erp.Mikro.Tests/ErpBridge.Erp.Mikro.Tests.csproj -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor --no-build
  → Toplam: 221
    Geçti:  201
    Atlandı: 20 (integration gate)
    Başarısız: 0

$ dotnet test tests/ErpBridge.CentralApi.Tests/ErpBridge.CentralApi.Tests.csproj -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor --no-build
  → Toplam: 175
    Geçti:  175
    Atlandı: 0
    Başarısız: 0
```

**Not:** Mikro test run'ı içinde 14 yeni fatura testi bulunmaktadır;
integration-tier testler (V15/V16 gerçek INSERT) `ERPBridge_RUN_INTEGRATION=1`
env-var'ı set edilmediği sürece skip'li kalır.

## 11. Kabul kriterleri

| Kriter | Durum |
|--------|-------|
| `InvoicePayload` Core/Domain'de, tüm gerekli alanlarla | ✅ |
| `MikroInvoiceWriter` V15 + V16 INSERT, **tek transaction** | ✅ |
| V15: `SCOPE_IDENTITY()` + `cha_RECid_RECno` self-link | ✅ |
| V16: pre-generated Guid + `cha_uid` line parent link (`sto_cha_uid`) | ✅ |
| Header INSERT: `cha_borc` / `cha_alacak` InvoiceType'a göre yönlendirilir | ✅ |
| Line INSERT: `sto_RECid_RECno` (V15) / `sto_cha_uid` (V16) header linki | ✅ |
| Idempotency: aynı externalId → aynı ERP id, ikinci INSERT yok | ✅ |
| Mapping save + ConnectionStringMasker + secret redaction | ✅ |
| `POST /api/v1/ingest/invoices` 401/201/200 + idempotent retry | ✅ |
| `CompanyNo` + `BranchNo` + `WarehouseNo` hem header hem line INSERT'inde | ✅ |
| Build temiz (0 uyarı, 0 hata) | ✅ |
| Testler geçiyor (≥ 5 yeni Mikro + ≥ 3 yeni CentralApi) | ✅ (+14 + +4) |
| Mapping store şeması değişmedi | ✅ |
| Cross-DB atomicity eklenmedi | ✅ |
| İrsaliye (4A) / Cari-Stok kart açma (4C) değiştirilmedi | ✅ |
| Android sync endpoint eklenmedi (görev kapsamı dışı) | ✅ |

## 12. Sonraki adımlar

- **Fatura Agent DI kaydı + Job processor implementasyonu** — Wave 4B.1
  (Küçük): `ServiceCollectionExtensions`'a `AddSingleton<MikroInvoiceWriter>()`
  + agent job dispatcher tarafında `documentType="invoice"` → writer
  mapping'i.
- **Cari / Stok kart açma (4C)** — `CARI_HESAPLER` + `STOKLAR` INSERT,
  adapter create.
- **Android sync endpoint** — Fatura yazıldıktan sonra CentralApi'ye
  anlık push; Android sync bölümü dolu dönmeye başlar. Bu PR
  kapsamında bilinçli olarak eklenmedi.
- **Cross-DB atomicity reconciliation** — Wave 5B.
- **Lookup checks (cari/stok)** — Fatura writer'a pre-INSERT doğrulaması
  eklenebilir (SalesOrder writer kalıbı).
