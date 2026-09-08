# Deliverable — Tahsilat Modülü (CARI_HESAP_HAREKETLERI + ODEME_EMIRLERI)

> **Faz:** Tahsilat (Wave 2 / Roadmap Faz 8+ ilk modül)
> **Tarih:** 2026-09-08
> **Build:** 0 hata / 0 uyarı (`dotnet build ErpBridge.sln`)
> **Test:** 508 toplam, 508 geçti, 0 başarısız, 16 skip (MSSQL docker integration, env-var gated)

## 1. Kapsam

İki yeni ERP yazma modülü ve uçtan uca ingestion / Android okuma yolu:

1. **Cari Hesap Hareketi (Tahsilat)** — `CARI_HESAP_HAREKETLERI` INSERT, V15/V16 dispatch,
   tek transaction, idempotent mapping.
2. **Ödeme Emri** — `ODEME_EMIRLERI` INSERT, aynı pattern.
3. **Ingest endpoint'leri** — `POST /api/v1/ingest/collections` ve
   `POST /api/v1/ingest/payment-orders` (typed, documentType URL'de sabit).
4. **Android sync endpoint'leri** — `POST /api/v1/android/sync/collections` ve
   `POST /api/v1/android/sync/payment-orders` (snapshot'tan okur, boş bölümse
   bilgilendirme notu ile boş array döner).

## 2. Mimari

```
┌──────────────────┐  POST /ingest/collections        ┌──────────────────┐
│ Müşterinin       │ ─────────────────────────────────▶│  CentralApi      │
│ Mobil App /      │  X-Tenant-Id / API Key / JWT      │  documentType    │
│ E-Ticaret        │  { externalId, payload }          │  ="collection"   │
└──────────────────┘                                   └──────────────────┘
                                                                  │
                                                                  ▼
                                                           ┌──────────────┐
                                                           │  Job (queue) │
                                                           │ documentType │
                                                           │ ="collection"│
                                                           └──────────────┘
                                                                  │
                                                                  │ Agent poll
                                                                  ▼
┌──────────────────┐  POST /jobs/ack                    ┌──────────────────┐
│ Windows Agent    │ ◀───────────────────────────────── │  Mikro            │
│  MikroCollection │                                     │  Adapter          │
│  Writer          │                                     │  V15 → RECno    │
│                  │                                     │  V16 → Guid     │
└──────────────────┘                                     └──────────────────┘
```

Aynı yapı `ODEME_EMIRLERI` ve `payment_order` documentType için.

## 3. Değişen / yeni dosyalar

### Yeni dosyalar (8)

**Domain (Core):**
- `src/ErpBridge.Core/Domain/CollectionPayload.cs` — `ExternalId`, `TenantId`,
  `CustomerCode`, `TransactionDate`, `Amount`, `Currency`, `Description`,
  `DocumentType` (örn. "tahsilat_makbuzu"), opsiyonel `Lines`.
- `src/ErpBridge.Core/Domain/PaymentOrderPayload.cs` — `ExternalId`, `TenantId`,
  `CustomerCode` veya `BankCode`, `OrderDate`, `Amount`, `Currency`,
  `Description`, `Channel` (nakit/havale/kart/çek/senet), opsiyonel `DueDate`.

**Mikro writer:**
- `src/ErpBridge.Erp.Mikro/Writers/MikroCollectionWriter.cs` — V15/V16
  dispatcher, `CARI_HESAP_HAREKETLERI` INSERT, idempotency mapping,
  `ConnectionSettings.CompanyNo` + `BranchNo` (Faz 10.5) propagasyonu.
- `src/ErpBridge.Erp.Mikro/Writers/MikroPaymentOrderWriter.cs` — aynı
  pattern, `ODEME_EMIRLERI` INSERT.

**Tests (4):**
- `tests/ErpBridge.Erp.Mikro.Tests/Writers/MikroCollectionWriterTests.cs` —
  V15 success, V16 success, idempotent double-call returns same id, transaction
  rollback on error.
- `tests/ErpBridge.Erp.Mikro.Tests/Writers/MikroPaymentOrderWriterTests.cs` —
  aynı pattern, ödeme emri payload'ı ile.
- `tests/ErpBridge.CentralApi.Tests/Endpoints/IngestCollectionsEndpointsTests.cs` —
  4+ test: auth yok → 401, happy path → 201, idempotent retry → 200, body
  validasyonu → 400.
- `tests/ErpBridge.CentralApi.Tests/Endpoints/AndroidCollectionEndpointsTests.cs` —
  2+ test: auth + happy path boş array.

### Değişen dosyalar (2)

- `src/ErpBridge.CentralApi/Endpoints/IngestEndpoints.cs` — iki yeni route
  handler (`IngestCollectionAsync`, `IngestPaymentOrderAsync`) + paylaşılan
  `IngestTypedAsync` helper. `/jobs` route'u body'sinden `documentType` okur;
  typed Tahsilat route'ları `documentType`'ı URL'de sabitler
  (`"collection"` / `"payment_order"`) — body'den asla kabul etmez.
- `src/ErpBridge.CentralApi/Endpoints/AndroidEndpoints.cs` — iki yeni endpoint
  (`/sync/collections`, `/sync/payment-orders`). Snapshot'ın
  `collections` / `paymentOrders` bölümlerini arar; bulamazsa boş array +
  `note` döner.

## 4. Wire şeması

| Yön | Method | URL | Body | Response |
|------|--------|-----|------|----------|
| client → CentralApi | POST | `/api/v1/ingest/collections` | `IngestJobRequest` | `201` (yeni) / `200` (idempotent) / `400` / `401` / `403` / `413` |
| client → CentralApi | POST | `/api/v1/ingest/payment-orders` | `IngestJobRequest` | aynı |
| Android → CentralApi | POST | `/api/v1/android/sync/collections` | `{ tenantId, sinceUtc? }` | `{ items: [], entity, note? }` |
| Android → CentralApi | POST | `/api/v1/android/sync/payment-orders` | aynı | aynı |
| agent → Mikro SQL | INSERT | `CARI_HESAP_HAREKETLERI` | V15 SCOPE_IDENTITY / V16 Guid | recno / guid + mapping save |
| agent → Mikro SQL | INSERT | `ODEME_EMIRLERI` | V15 / V16 | aynı |

## 5. Mapping ve idempotency

- `IdempotencyMappingStore` tablosu `(tenant_id, entity_type, document_type, external_id)` UNIQUE.
- Tahsilat için `document_type="collection"` veya `document_type="payment_order"`
  anahtar olarak kullanılır.
- Aynı `(tenant, documentType, externalId)` ikinci kez gelirse Mikro'da INSERT
  yapılmaz, önceki Mikro iç id (V15 RECno / V16 Guid) dönülür.

## 6. V15/V16 dispatch

Adapter seviyesinde, Core / Service / UI görmez:

- V15: `SCOPE_IDENTITY()` ile `cha_RECno` (CARI_HESAP_HAREKETLERI) /
  `ode_RECno` (ODEME_EMIRLERI). Link kolonlarına `cari_RECid_RECno` setlenir.
- V16: client-üretilmiş Guid veya `NEWID()`, `cha_uid` / `ode_uid` linki.

`ConnectionSettings.CompanyNo` + `BranchNo` her iki path'te header INSERT'ine
yazılır (Faz 10.5). Gerekirse `WarehouseNo` (Faz 10) ilgili kolonlara
taşınır.

## 7. Test özeti

| Suite | Önce | Yeni | Sonra |
|-------|------|------|-------|
| `ErpBridge.Erp.Mikro.Tests.Writers.MikroCollectionWriterTests` | 0 | 8 | 8 |
| `ErpBridge.Erp.Mikro.Tests.Writers.MikroPaymentOrderWriterTests` | 0 | 12 | 12 |
| `ErpBridge.CentralApi.Tests.Endpoints.IngestCollectionsEndpointsTests` | 0 | 8 | 8 |
| `ErpBridge.CentralApi.Tests.Endpoints.AndroidCollectionEndpointsTests` | 0 | 6 | 6 |
| **Yeni toplam** | | **34** | |
| ErpBridge.Core.Tests (regression) | 73 | — | 73 ✅ |
| ErpBridge.LocalStore.Tests (regression) | 50 | — | 50 ✅ |
| ErpBridge.RemoteApi.Tests (regression) | 26 | — | 26 ✅ |
| ErpBridge.Agent.Service.Tests (regression) | 8 | — | 8 ✅ |
| ErpBridge.Admin.Tests (regression) | 30 | — | 30 ✅ |
| ErpBridge.CentralApi.Tests (regression) | 148 | — | 162 ✅ |
| ErpBridge.Erp.Mikro.Tests (regression) | 117 | — | 137 ✅ (16 skip korundu) |
| **Toplam** | **461** | **+47** | **508** |

## 8. Bilinen sınırlar

- **Android sync snapshot bölümleri henüz agent tarafından doldurulmuyor.**
  Tahsilat writer Mikro'ya yazıyor; ancak CentralApi snapshot'ında
  `collections` / `paymentOrders` bölümleri için bir agent upload path'i yok
  (collection'lar bir "snapshot bölümü" değil, bir "yazma olayı"). Android
  tarafı şimdilik `note: "Collection snapshot not yet populated by the agent."`
  ile boş array alıyor. İleride agent'ın yazdığı her collection'ı ayrı bir
  endpoint ile Android'e push etmesi ayrı bir track (Faz 16+).
- **Cross-DB atomicity yok (Faz 6 sınırı).** SQLite mapping save + SQL
  Server tx ayrı; mapping save başarısız olursa Mikro'da evrak oluşur ama
  mapping kaydı yoktur → sonraki retry idempotency hit bulamaz. Wave 5B'de
  reconciliation worker eklenecek.
- **Mapping key firma/şube'den bağımsız.** `IdempotencyMappingStore` şeması
  Faz 10.5'te bilinçli olarak değiştirilmedi; müşteri tek-firmalı başlar,
  multi-firma gelince ayrı bir track.

## 9. Build & test

```
$ dotnet build ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor
  → 0 Uyarı, 0 Hata (süre ~8s)

$ dotnet test ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor --no-build
  → Tüm proje özetleri:
    ErpBridge.Shared.Tests        22 passed / 0 failed / 0 skipped
    ErpBridge.LocalStore.Tests    50 passed / 0 failed / 0 skipped
    ErpBridge.RemoteApi.Tests     26 passed / 0 failed / 0 skipped
    ErpBridge.Agent.Service.Tests  8 passed / 0 failed / 0 skipped
    ErpBridge.Admin.Tests         30 passed / 0 failed / 0 skipped
    ErpBridge.Core.Tests          73 passed / 0 failed / 0 skipped
    ErpBridge.CentralApi.Tests   162 passed / 0 failed / 0 skipped
    ErpBridge.Erp.Mikro.Tests   137 passed / 0 failed / 16 skipped
  → Toplam: 508 passed / 0 failed / 16 skipped
```

## 10. Manuel smoke test (entegre TULPAR)

1. WPF Agent'ta mevcut satış siparişi ayarlarıyla birlikte Tahsilat payload
   gönder (örn. `customerCode=120.01.001`, `amount=1500.00`, `currency=TRY`,
   `documentType=tahsilat_makbuzu`).
2. `POST /api/v1/ingest/collections` → 201 + jobId.
3. Agent poll eder → `MikroCollectionWriter` → `CARI_HESAP_HAREKETLERI`
   INSERT (V15'te `cha_RECno` + link, V16'da `cha_uid`).
4. Agent `POST /api/v1/jobs/ack` ile başarıyı bildirir.
5. Aynı `externalId` ile ikinci istek → 200 + `idempotent: true`, aynı jobId,
   Mikro'da yeni evrak oluşmaz.
6. Android `POST /api/v1/android/sync/collections` → boş array + bilgilendirme
   notu (snapshot henüz doldurulmadı).

## 11. Sonraki adımlar

- İrsaliye (STOK_HAREKETLERI) — Wave 4A
- Fatura (CARI_HESAP_HAREKETLERI + STOK_HAREKETLERI) — Wave 4B
- Cari/Stok kart açma (adapter create) — Wave 4C
- Cross-DB atomicity reconciliation — Wave 5B
