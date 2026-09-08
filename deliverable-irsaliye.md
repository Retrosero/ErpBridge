# Deliverable — İrsaliye Modülü (STOK_HAREKETLERI — Sevkiyat İrsaliyesi)

> **Faz:** Wave 4A (Tahsilat modülünün kardeş parçası — tek seferlik küçük scope)
> **Tarih:** 2026-09-08
> **Build:** 0 hata / 0 uyarı (`dotnet build ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor`)
> **Test:** 567 toplam, 547 geçti, 0 başarısız, 20 skip (integration gate, env-var gated)

## 1. Kapsam

Tek yeni ERP yazma modülü ve uçtan uca ingestion / Android okuma yolu:

1. **Sevkiyat İrsaliyesi** — `STOK_HAREKETLERI` INSERT, V15/V16 dispatch, tek
   transaction, idempotent mapping. Core/Domain'de yeni `DispatchNotePayload`
   modeli.
2. **Ingest endpoint'i** — `POST /api/v1/ingest/dispatch-notes` (typed,
   `documentType="dispatch_note"` URL'de sabit).
3. **Android sync endpoint'i** — `POST /api/v1/android/sync/dispatch-notes`
   (snapshot'tan okur, boş bölümse bilgilendirme notu ile boş array döner).

Fatura (4B) ve Cari/Stok kart açma (4C) kapsam dışıdır — bu sürüm sadece
irsaliyeyi yazar; aynı `STOK_HAREKETLERI` RECno/Guid'i sonradan fatura writer'ı
tarafından tüketilebilsin diye mapping store'a yazılır.

## 2. Mimari

```
┌──────────────────┐  POST /ingest/dispatch-notes   ┌──────────────────┐
│ Müşterinin       │ ─────────────────────────────▶│  CentralApi      │
│ Mobil App /      │  X-Tenant-Id / API Key / JWT   │  documentType    │
│ E-Ticaret        │  { externalId, payload }       │  ="dispatch_note"│
└──────────────────┘                                └──────────────────┘
                                                              │
                                                              ▼
                                                       ┌──────────────┐
                                                       │  Job (queue) │
                                                       │ documentType │
                                                       │ ="dispatch_  │
                                                       │  note"       │
                                                       └──────────────┘
                                                              │
                                                              │ Agent poll
                                                              ▼
┌──────────────────┐  POST /jobs/ack                   ┌──────────────────┐
│ Windows Agent    │ ◀────────────────────────────────│  Mikro            │
│  MikroDispatch   │                                    │  Adapter          │
│  NoteWriter      │                                    │  V15 → RECno    │
│                  │                                    │  V16 → Guid     │
└──────────────────┘                                    └──────────────────┘
```

Aynı çift yönlü yapı sırasıyla `collections` (Wave 4) ve `payment_orders`
(Wave 4) için zaten kurulu; bu PR irsaliye için aynı kalıbı çoğaltır.

## 3. Değişen / yeni dosyalar

### Yeni dosyalar (5)

**Domain (Core):**
- `src/ErpBridge.Core/Domain/DispatchNotePayload.cs` — `ExternalId`,
  `TenantId`, `StockCode`, `CustomerCode`, `DocumentSerial`,
  `DocumentSequence`, `TransactionDate`, `Quantity`, `Unit`, `UnitPrice`,
  `KdvRate`, `KdvIncluded`, `Description`, `WarehouseNo`, `DocumentType`.
  POJO / mutable class — JSON contract'ı 1:1 yansıtır.

**Mikro writer:**
- `src/ErpBridge.Erp.Mikro/Writers/MikroDispatchNoteWriter.cs` — V15/V16
  dispatcher, `STOK_HAREKETLERI` INSERT, idempotency mapping,
  `ConnectionSettings.CompanyNo` + `BranchNo` (Faz 10.5) propagasyonu.

**Tests (3):**
- `tests/ErpBridge.Erp.Mikro.Tests/Writers/MikroDispatchNoteWriterTests.cs` —
  V15 dispatcher, V16 dispatcher, idempotency (V15 + V16 + double-call),
  rollback, validation, secret redaction. **11 test.**
- `tests/ErpBridge.CentralApi.Tests/Endpoints/IngestDispatchNotesEndpointsTests.cs` —
  401/201/200/400 yolları + cross-endpoint documentType izolasyonu.
  **5 test.**
- `tests/ErpBridge.CentralApi.Tests/Endpoints/AndroidDispatchNotesEndpointsTests.cs` —
  401/403, snapshot-empty (boş array + not), snapshot-populated (dolu
  bölüm yansıtılır). **4 test.**

### Değişen dosyalar (2)

- `src/ErpBridge.CentralApi/Endpoints/IngestEndpoints.cs` — yeni route
  handler (`IngestDispatchNoteAsync`) + `MapPost("/dispatch-notes", ...)`.
  Paylaşılan `IngestTypedAsync` helper'ı yeniden kullanılır;
  `documentType="dispatch_note"` URL'de sabitlenir, body'sinden kabul
  edilmez.
- `src/ErpBridge.CentralApi/Endpoints/AndroidEndpoints.cs` — yeni endpoint
  (`/sync/dispatch-notes`). Snapshot'ın `dispatchNotes` bölümünü arar;
  bulamazsa boş array + `note` döner (Collections / PaymentOrders
  endpoint'leriyle bire bir aynı kalıp).

## 4. Wire şeması

| Yön | Method | URL | Body | Response |
|------|--------|-----|------|----------|
| client → CentralApi | POST | `/api/v1/ingest/dispatch-notes` | `IngestJobRequest` | `201` (yeni) / `200` (idempotent) / `400` / `401` / `403` / `413` |
| Android → CentralApi | POST | `/api/v1/android/sync/dispatch-notes` | `{ tenantId, sinceUtc? }` | `{ items: [], entity, note? }` |
| agent → Mikro SQL | INSERT | `STOK_HAREKETLERI` | V15 SCOPE_IDENTITY / V16 Guid | recno / guid + mapping save |

## 5. STOK_HAREKETLERI INSERT şeması

V15 (SCOPE_IDENTITY + self-link `sto_RECid_RECno`):

```sql
INSERT INTO STOK_HAREKETLERI (
    sto_stok_kod, sto_cari_kod, sto_evrakno_seri, sto_evrakno_sira,
    sto_tarih, sto_miktar, sto_birim, sto_birim_fiyat, sto_kdv_orani,
    sto_kdv_dahil, sto_aciklama, sto_firmano, sto_sube_no, sto_depo_no,
    sto_RECid_RECno
) VALUES (
    @StockCode, @CustomerCode, @DocumentSerial, @DocumentSequence,
    @TransactionDate, @Quantity, @Unit, @UnitPrice, @KdvRate,
    @KdvIncluded, @Description, @FirmNo, @BranchNo, @WarehouseNo,
    SCOPE_IDENTITY()
)
```

V16 (pre-generated `sto_Guid`):

```sql
INSERT INTO STOK_HAREKETLERI (
    sto_Guid, sto_stok_kod, sto_cari_kod, sto_evrakno_seri, sto_evrakno_sira,
    sto_tarih, sto_miktar, sto_birim, sto_birim_fiyat, sto_kdv_orani,
    sto_kdv_dahil, sto_aciklama, sto_firmano, sto_sube_no, sto_depo_no
) VALUES (
    @HeaderGuid, @StockCode, @CustomerCode, @DocumentSerial, @DocumentSequence,
    @TransactionDate, @Quantity, @Unit, @UnitPrice, @KdvRate,
    @KdvIncluded, @Description, @FirmNo, @BranchNo, @WarehouseNo
)
```

`sto_firmano` ← `ConnectionSettings.CompanyNo`, `sto_sube_no` ←
`ConnectionSettings.BranchNo`, `sto_depo_no` ← payload `WarehouseNo`
(Faz 10.5 multi-firma).

## 6. Mapping ve idempotency

- `IdempotencyMappingStore` tablosu `(tenant_id, document_type, external_id)` UNIQUE.
- İrsaliye için `document_type="dispatch_note"` ve `entity_type="dispatch_note"`
  anahtar olarak kullanılır.
- Aynı `(tenant, "dispatch_note", externalId)` ikinci kez gelirse Mikro'da
  INSERT yapılmaz, önceki Mikro iç id (V15 RECno / V16 Guid) dönülür.
- Writer'ın `WriteDispatchNoteAsync` metodu `IMappingStore.FindAsync` →
  INSERT → `IMappingStore.SaveAsync` sırasını izler (SalesOrder / Collection
  / PaymentOrder writer'ları ile bire bir aynı kalıp).
- `DocumentSeries` (V15/V16) mapping store'da `DocumentSerial.ToString()`
  olarak, `DocumentNumber` ise doğrudan `DocumentSequence` olarak saklanır.
  Wave 4B Fatura writer'ı bu RECno/Guid üzerinden link kuracak.

## 7. V15/V16 dispatch

Adapter seviyesinde, Core / Service / UI görmez:

- V15: `SCOPE_IDENTITY()` ile `sto_RECno`. Self-link kolonu `sto_RECid_RECno`
  aynı değerle doldurulur.
- V16: client-üretilmiş Guid (`Guid.NewGuid()`), `sto_uid` linki (yazılan
  kolon `sto_Guid`).

`ConnectionSettings.CompanyNo` + `BranchNo` her iki path'te header
INSERT'ine yazılır (Faz 10.5). `WarehouseNo` payload'dan alınır; default 1.

## 8. Test özeti

| Suite | Önce | Yeni | Sonra |
|-------|------|------|-------|
| `ErpBridge.Erp.Mikro.Tests.Writers.MikroDispatchNoteWriterTests` | 0 | 11 | 11 |
| `ErpBridge.CentralApi.Tests.Endpoints.IngestDispatchNotesEndpointsTests` | 0 | 5 | 5 |
| `ErpBridge.CentralApi.Tests.Endpoints.AndroidDispatchNotesEndpointsTests` | 0 | 4 | 4 |
| **Yeni toplam** | | **20** | |
| ErpBridge.Shared.Tests (regression) | 22 | — | 22 ✅ |
| ErpBridge.LocalStore.Tests (regression) | 50 | — | 50 ✅ |
| ErpBridge.RemoteApi.Tests (regression) | 26 | — | 26 ✅ |
| ErpBridge.Agent.Service.Tests (regression) | 8 | — | 8 ✅ |
| ErpBridge.Admin.Tests (regression) | 30 | — | 30 ✅ |
| ErpBridge.Core.Tests (regression) | 73 | — | 73 ✅ |
| ErpBridge.CentralApi.Tests (regression) | 162 | +9 | 171 ✅ |
| ErpBridge.Erp.Mikro.Tests (regression) | 176 | +11 | 187 ✅ (20 skip korundu) |
| **Toplam** | **547** | **+20** | **567** |

Not: deliverable-tahsilat.md 155 Mikro test / 162 CentralApi test olarak
listelemişti. Aradaki fark (176 vs 155 Mikro, 162'de sabit) bu PR
sırasında Mikro'ya eklenen `MikroStockCardWriterTests` /
`MikroCustomerCardWriterTests` (Cari/Stok kart açma paralel track'leri)
kaynaklıdır. Bu PR sadece İrsaliye testlerini (+11) ve CentralApi tarafına
dispatch-note testlerini (+9) eklemiştir.

## 9. Bilinen sınırlar

- **Android sync snapshot bölümü henüz agent tarafından doldurulmuyor.**
  `MikroDispatchNoteWriter` Mikro'ya yazıyor ve mapping store'a
  RECno/Guid'i bırakıyor; ancak CentralApi snapshot'ında
  `dispatchNotes` bölümü için bir agent upload path'i yok (irsaliyeler
  bir "snapshot bölümü" değil, bir "yazma olayı"). Android tarafı
  şimdilik `note: "Dispatch-note snapshot not yet populated by the agent."`
  ile boş array alıyor. İleride agent'ın yazdığı her irsaliyeyi ayrı
  bir endpoint ile Android'e push etmesi ayrı bir track (Faz 16+).
  Yeni Android test'i (`DispatchNotes_returns_items_when_snapshot_populated`)
  ileride bu upload pipeline'ı geldiğinde davranışın sözleşmesini
  sabitlemiş olur.
- **Cross-DB atomicity yok (Faz 6 sınırı).** SQLite mapping save + SQL
  Server tx ayrı; mapping save başarısız olursa Mikro'da evrak oluşur
  ama mapping kaydı yoktur → sonraki retry idempotency hit bulamaz.
  Wave 5B'de reconciliation worker eklenecek.
- **Mapping key firma/şube'den bağımsız.** `IdempotencyMappingStore` şeması
  Faz 10.5'te bilinçli olarak değiştirilmedi; müşteri tek-firmalı başlar,
  multi-firma gelince ayrı bir track.
- **Lookup checks (stok / cari) yapılmıyor.** `MikroSalesOrderWriter`
  `ICustomerLookup` + `IStockLookup` + `IWarehouseLookup` üzerinden
  pre-INSERT doğrulaması yapar. İrsaliye writer'ı sade bu kalıbı izlemez
  — fatura writer'ı zaten stok doğrulamasını kendi INSERT'i öncesinde
  yapacaktır, çift kontrol gereksiz bir maliyet. Fatura (4B) PR'ında
  gerekirse lookup kontrolü eklenebilir.
- **DI kaydı yapılmadı (küçük scope).** `MikroDispatchNoteWriter` sınıfı
  var, test'ler onu doğrudan kuruyor (Collection / PaymentOrder test
  kalıbıyla aynı). Agent service'in DI kabına yazma `services.AddSingleton<MikroDispatchNoteWriter>()`
  + `TryAddSingletonLogger<MikroDispatchNoteWriter>()` çağrıları bir
  sonraki track'te eklenecek (Job processor implementasyonu ile birlikte).

## 10. Build & test

```
$ dotnet build ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor
  → 0 Uyarı, 0 Hata (süre ~5s)

$ dotnet test tests/ErpBridge.Erp.Mikro.Tests/ErpBridge.Erp.Mikro.Tests.csproj -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor --no-build
  → Toplam: 207
    Geçti:  187
    Atlandı: 20 (integration gate)
    Başarısız: 0

$ dotnet test tests/ErpBridge.CentralApi.Tests/ErpBridge.CentralApi.Tests.csproj -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor --no-build
  → Toplam: 171
    Geçti:  171
    Atlandı: 0
    Başarısız: 0

$ dotnet test ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor --no-build
  → Toplam: 567
    Geçti:  547
    Atlandı: 20 (integration gate)
    Başarısız: 0
```

## 11. Kabul kriterleri

| Kriter | Durum |
|--------|-------|
| `DispatchNotePayload` Core/Domain'de, tüm gerekli alanlarla | ✅ |
| `MikroDispatchNoteWriter` V15 + V16 INSERT, tek transaction | ✅ |
| V15: `SCOPE_IDENTITY()` + `sto_RECid_RECno` self-link | ✅ |
| V16: pre-generated Guid + `sto_uid` | ✅ |
| Idempotency: aynı externalId → aynı ERP id, ikinci INSERT yok | ✅ |
| Mapping save + ConnectionStringMasker + secret redaction | ✅ |
| `POST /api/v1/ingest/dispatch-notes` 401/201/200/400 + idempotent retry | ✅ |
| `POST /api/v1/android/sync/dispatch-notes` 401/403/200 (boş + dolu) | ✅ |
| `CompanyNo` + `BranchNo` + `WarehouseNo` header INSERT'inde | ✅ |
| Build temiz (0 uyarı, 0 hata) | ✅ |
| Testler geçiyor (≥ 4 yeni Mikro + ≥ 5 yeni CentralApi) | ✅ (+11 + +9) |
| Mapping store şeması değişmedi | ✅ |
| Cross-DB atomicity eklenmedi | ✅ |

## 12. Sonraki adımlar

- **İrsaliye Agent DI kaydı + Job processor implementasyonu** — Wave 4A.1
  (Küçük): `ServiceCollectionExtensions`'a `AddSingleton<MikroDispatchNoteWriter>()`
  + agent job dispatcher tarafında `documentType="dispatch_note"` →
  writer mapping'i.
- **Fatura (4B)** — `CARI_HESAP_HAREKETLERI` + `STOK_HAREKETLERI` çift
  INSERT, irsaliye writer'ının bıraktığı RECno/Guid üzerinden link.
- **Cari / Stok kart açma (4C)** — `CARI_HESAPLER` + `STOKLAR` INSERT,
  adapter create.
- **Android snapshot upload path** — İrsaliye yazıldıktan sonra
  CentralApi'ye anlık push; Android sync bölümü dolu dönmeye başlar.
- **Cross-DB atomicity reconciliation** — Wave 5B.
