# ErpBridge — Proje Durum Özeti (Token-Dostu)

> **Amaç:** Bu dosya, herhangi bir AI asistanın (ChatGPT / Gemini / MiniMax) projeyi **baştan
> okumadan** hızlıca bağlam kurabilmesi için yazıldı. Tüm dokümanları değil, sadece
> **bilmesi gereken minimum karar-yerlerini** içerir.
>
> **Kullanım:** Yeni bir AI ile başlarken bu dosyayı + ihtiyaç duyulan tek deliverable'ı
> (örn. sıradaki faz için `deliverable-faz3.md`) yapıştır. Proje ağacını, README'yi veya
> `docs/` altını okutma.

---

## 0. Genel Bakış (30 saniye)

- **Proje:** ErpBridge — Windows Agent (WPF + Service) ↔ merkezi API ↔ Mikro ERP (V15/V16).
- **Dil/Platform:** C# / .NET 10 (Windows hedefli), SQLite (yerel) + PostgreSQL (merkez).
- **Mimari:** Katmanlı — `Abstractions` → `Core` → adapter'lar (Mikro). Service/UI sadece
  interface'leri görür; V15/V16 farkı sadece `ErpBridge.Erp.Mikro` içinde kalır.
- **Test disiplini:** Her commit sonrası `dotnet build` ve `dotnet test` temiz olmalı.
  Şu an **592/592 test PASSED + 20 SKIPPED** (16 integration + 4 NewSchemaTrigger yarım kaldı).
- **Çalışma modu:** Mavis Team (paralel track'ler → owner GATE → sıradaki faz). Her faz
  kendi `deliverable-*.md` dosyasıyla kapanır.

---

## 1. Bağlayıcı Kurallar (Hard Rules)

Bunlar **asla** çiğnenez:

1. **V15/V16 farkı adapter'da kalır.** Core / Service / UI / RemoteApi bu farkı bilmez.
2. **SQL her zaman parametrik.** Kullanıcı/payload verisi string concat ile SQL'e giremez.
3. **ERP yazma transaction içinde.** Header + satırlar + açıklamalar + mapping = tek transaction.
4. **Idempotent yazım.** Aynı `externalId` ikinci kez gelirse Mikro'da evrak **oluşturulmaz**,
   önceki Recno/Guid dönülür. Mapping store: `(tenant_id, entity_type, document_type, external_id)` UNIQUE.
5. **Secret loglanmaz.** SQL şifresi, lisans anahtarı, API token. UI'da `PasswordBox` ile alınır;
   `ConnectionStringMasker` + `AgentConfigMasker` ile süzülür.
6. **Build asla kırık bırakılmaz.** Her commit sonrası `dotnet build` temiz.
7. **Yeni interface'e test yanında.** Interface ekleniyorsa test de eklenir.
8. **Cross-reference katman kuralı:**
   - `ErpBridge.Erp.Abstractions` → sadece `Shared`
   - `ErpBridge.Core` → `Shared` (+ `Abstractions`)
   - `ErpBridge.LocalStore` → `Shared`, `Core`
   - `ErpBridge.RemoteApi` → `Shared`, `Core`
   - `ErpBridge.Erp.Mikro` → `Shared`, `Abstractions` (Service/UI'a bağımlı değil)
   - `ErpBridge.Agent.Service` → `Core`, `LocalStore`, `RemoteApi`, `Erp.Mikro`
   - `ErpBridge.Agent.UI` → `Core`, `LocalStore`, `RemoteApi`, `Erp.Mikro`
   - `ErpBridge.Admin` → sadece `Shared` (HTTP-only, CentralApi'ye ref yok)
   - `ErpBridge.Erp.Abstractions` ve `ErpBridge.Core` → **SqlClient/Dapper/Polly paketi YOK**
9. **`TreatWarningsAsErrors=true`** her projede.

---

## 2. Çözüm Yapısı (Hızlı Ağaç)

```
ErpBridge/
├── ErpBridge.sln                      # 17 src/test projesi
├── global.json                        # SDK rollForward: latestMajor
├── AGENTS.md                          # bağlayıcı kurallar (her agent okumalı)
├── docs/
│   ├── architecture.md                # katman yapısı, referans izinleri
│   ├── development-roadmap.md         # Faz 1-7+ yol haritası
│   ├── mikro-v15-v16-rules.md         # RECno vs Guid stratejisi
│   └── api-contracts.md               # Central API HTTP sözleşmesi
├── src/
│   ├── ErpBridge.Shared/              # Result, Error, Hash, ConnectionStringMasker, AgentSettingsValidation, TrackedTableSchema
│   ├── ErpBridge.Erp.Abstractions/    # IErpAdapter (Mikro yok)
│   ├── ErpBridge.Core/                # Domain modelleri, store interface'leri, AgentConfigMasker, TriggerChangeSet
│   ├── ErpBridge.LocalStore/          # SQLite migrations + 4 store + ProtectedConfig (AES/DPAPI)
│   ├── ErpBridge.RemoteApi/           # HttpRemoteApiClient + Polly v8 retry + Idempotency-Key header
│   ├── ErpBridge.Erp.Mikro/           # V15/V16 strategy, ConnectionTestOrchestrator, 7 writer (SalesOrder, Collection, PaymentOrder, DispatchNote, Invoice, CustomerCard, StockCard) + readers + triggers
│   ├── ErpBridge.Agent.Service/       # BackgroundService: AgentWorker + HeartbeatWorker + CrossDbReconciliationWorker
│   ├── ErpBridge.Agent.UI/            # WPF ayar paneli (PasswordBox, AsyncRelayCommand, badge, validation, Mikro bağlantı rozet)
│   ├── ErpBridge.CentralApi/          # PostgreSQL backend, JWT + API key auth, ingest, sync, admin, audit, parameters
│   └── ErpBridge.Admin/               # Blazor Server admin paneli (9 sayfa: Login, Logout, Dashboard, Tenants, Licenses, Agents, Jobs, Bootstrap, ApiKeys, Webhooks, SyncHistory, Parameters)
└── tests/
    ├── ErpBridge.Core.Tests/
    ├── ErpBridge.LocalStore.Tests/
    ├── ErpBridge.Erp.Mikro.Tests/     # 201 test (16 integration skip + 4 yarım kaldı skip)
    ├── ErpBridge.Shared.Tests/
    ├── ErpBridge.RemoteApi.Tests/     # 26 test (regression coverage)
    ├── ErpBridge.CentralApi.Tests/    # 182 test (auth, jobs, ingest, sync, admin, audit, parameters, refresh token)
    ├── ErpBridge.Agent.Service.Tests/
    ├── ErpBridge.Admin.Tests/         # 30 bUnit test
    + tests/docker-compose.test.yml    # MSSQL 2019 (V15) + MSSQL 2022 (V16) integration
```

---

## 3. Tamamlanan Fazlar (Wave 1-5, 2026-09-08 itibarıyla)

| Faz | İçerik | Deliverable | Testler |
|---|---|---|---|
| 1-7 | Original skeleton (Çözüm + 17 proje, DI, Serilog, SQLite, WPF/Service shell, ERP abstraction, Mikro skeleton, RemoteApi, Bağlantı testi + V15/V16 detector, Merkezi API skeleton, Mikro bootstrap okuma, Satış siparişi yazma, Admin panel) | `deliverable-faz1..7.md` | 286 → 453 |
| 8 | API Key + Webhook + Coolify deploy (SaaS) | `deliverable-faz8.md` | — |
| 9 | 1 dk delta sync + long-polling | `deliverable-faz9.md` | — |
| 10 | Multi-firm Mikro (Firma/Şube/Depo No) | `deliverable-faz10.md` | — |
| **Wave 1** (1A) | Faz 10.5 — Sales Order Writer multi-firm (CompanyNo+BranchNo) | `deliverable-faz10-5.md` | +8 |
| **Wave 1** (1B) | RemoteApi HttpClient 401/timeout regression testleri | `deliverable-remoteapi-regression-tests.md` | +6 |
| **Wave 1** (1C) | PostgreSQL migration: `change_set_audit_log` + `parameter_records` | `deliverable-migration-audit-parameters.md` | +7 |
| **Wave 2** (2A) | Faz 15.8 — WPF Admin Sekmeleri (Sync Geçmişi + Parametreler) | `deliverable-faz15-8.md` | +13 |
| **Wave 2** (2B) | Tahsilat modülü (CARI_HESAP_HAREKETLERI + ODEME_EMIRLERI) | `deliverable-tahsilat.md` | +34 |
| **Wave 3** (3A) | Faz 15.2 — Yeni trigger installer (yarım kaldı, 4 test skip) | `deliverable-faz15-2.md` | +8 (4 skip) |
| **Wave 3** (3B) | Faz 15.3+15.4 — TriggerChangeSet Reader + KeyValue semantiği | `deliverable-faz15-3-4.md` | +19 |
| **Wave 3** (3C) | Faz 10.6/10.7 — WPF validation + Mikro bağlantı rozet | (worker deliverable) | 0 (UI only) |
| **Wave 4** (4A) | İrsaliye modülü (STOK_HAREKETLERI writer + ingest + Android sync) | `deliverable-irsaliye.md` | +20 |
| **Wave 4** (4B) | Fatura modülü (CARI_HESAP_HAREKETLERI + STOK_HAREKETLERI, tek transaction) | `deliverable-fatura.md` | +21 |
| **Wave 4** (4C) | Cari/Stok kart açma (MikroCustomerCardWriter + MikroStockCardWriter) | `deliverable-card-create.md` | +21 |
| **Wave 5** (5A) | Admin refresh token flow (JWT access + rotation refresh) | `deliverable-refresh-token.md` | +7 |
| **Wave 5** (5B) | Cross-DB atomicity reconciliation worker (alarm only) | `deliverable-reconciliation.md` | (Agent.Service) |
| **Final** | PROJECT-CONTEXT güncelleme + bu dosya | — | — |

**Toplam:** 592 test PASSED + 20 SKIPPED (16 integration + 4 NewSchemaTrigger yarım kaldı), 0 FAILED.

---

## 4. Modül Envanteri

### ERP Writer'lar (`ErpBridge.Erp.Mikro/Writers/`)

| Writer | Document Type | Tablolar | Faz |
|---|---|---|---|
| `MikroSalesOrderWriter` | `sales_order` | `SIPARISLER` + lines | Faz 6 + Faz 10.5 |
| `MikroCollectionWriter` | `collection` | `CARI_HESAP_HAREKETLERI` | Wave 2 (Tahsilat) |
| `MikroPaymentOrderWriter` | `payment_order` | `ODEME_EMIRLERI` | Wave 2 (Tahsilat) |
| `MikroDispatchNoteWriter` | `dispatch_note` | `STOK_HAREKETLERI` | Wave 4A (İrsaliye) |
| `MikroInvoiceWriter` | `invoice` | `CARI_HESAP_HAREKETLERI` + `STOK_HAREKETLERI` × N | Wave 4B (Fatura) |
| `MikroCustomerCardWriter` | `customer_card` | `CARI_HESAPLAR` | Wave 4C |
| `MikroStockCardWriter` | `stock_card` | `STOKLAR` (+ `BARKOD_TANIMLARI`) | Wave 4C |

### CentralApi Ingest Endpoint'leri

| Method | URL | Document Type | Faz |
|---|---|---|---|
| POST | `/api/v1/ingest/jobs` | (body'den) | Faz 4 |
| POST | `/api/v1/ingest/collections` | `collection` | Wave 2 |
| POST | `/api/v1/ingest/payment-orders` | `payment_order` | Wave 2 |
| POST | `/api/v1/ingest/dispatch-notes` | `dispatch_note` | Wave 4A |
| POST | `/api/v1/ingest/invoices` | `invoice` | Wave 4B |
| POST | `/api/v1/ingest/parameters` | `_ERPB_PARAMETRELER` | Faz 15 |

### CentralApi Android Sync Endpoint'leri

| Method | URL | Bölüm |
|---|---|---|
| POST | `/api/v1/android/sync/collections` | `collections` |
| POST | `/api/v1/android/sync/payment-orders` | `paymentOrders` |
| POST | `/api/v1/android/sync/dispatch-notes` | `dispatchNotes` |
| POST | `/api/v1/android/parameters` | parameters |

### Admin Endpoint'ler (Blazor)

- Login, Logout, **/refresh** (Wave 5A)
- Dashboard, Tenants (+Create), Licenses, Agents, Jobs, Bootstrap
- ApiKeys, Webhooks, **SyncHistory** (Faz 15.8), **Parameters** (Faz 15.8)

### Agent Workers (`ErpBridge.Agent.Service/Workers/`)

- `AgentWorker` — job pull + dispatch
- `HeartbeatWorker` — merkezi API'ye periyodik heartbeat
- `CrossDbReconciliationWorker` (Wave 5B) — orphan/missing mapping alarm

### WPF Ayar Paneli (`ErpBridge.Agent.UI/`)

- Ana sayfa + Dashboard
- Mikro bağlantı testi (Faz 3 — V15/V16 detector)
- **Firma No / Şube No / Depo No** input (Faz 10) + **inline validation** (Faz 10.6/10.7)
- **Varsayılanlara sıfırla** link
- **Mikro bağlantı rozeti** (Faz 10.6)

---

## 5. V15 vs V16 — Tek Sayfa Karar Tablosu

| Konu | V15 | V16 |
|------|-----|-----|
| Primary key | `RECno INT IDENTITY` | `Guid UNIQUEIDENTIFIER` |
| Tablo deseni | `TABLO`, `TABLO_RECid` (lookup) | `TABLO`, `TABLO_uid` (lookup) |
| Link kolonu | `IliskiliTablo_RECid_RECno` | `IliskiliTablo_uid` |
| Id strategy | `SCOPE_IDENTITY()` | `NEWID()` veya client-üretilmiş Guid |
| Detection | DB metadata probe (RECno kolonu var mı?) | DB metadata probe (Guid PK mı?) |
| Unknown | major.minor parse edilemedi → Unknown | aynı |

Detay: `docs/mikro-v15-v16-rules.md`.

---

## 6. Multi-Firm (Faz 10) ve Multi-Şube (Faz 10.5)

| Alan | Kaynak | Default | Validation |
|---|---|---|---|
| `CompanyNo` | `AgentConfig.CompanyNo` → `MikroConnectionSettings.CompanyNo` | 1 | `>= 1` |
| `BranchNo` | `AgentConfig.BranchNo` → `MikroConnectionSettings.BranchNo` | 0 | `>= 0` |
| `WarehouseNo` | `AgentConfig.WarehouseNo` → `MikroConnectionSettings.WarehouseNo` | 1 | `>= 1` |

WPF UI: 3 input + inline error label + yeşil/kırmızı rozet + sıfırla butonu.
`AgentSettingsViewModel.Validate()` setter'larda tetiklenir.

---

## 7. Idempotency Mapping Kuralı

`external_id` (remote tarafın gönderdiği dış kimlik) → `(tenant_id, entity_type, document_type, external_id)`
UNIQUE index. Tekrar gelirse INSERT yapılmaz, önceki dahili id (V15 RECno / V16 Guid) dönülür.

Mapping tablosu: `idempotency_mapping` (Core'un store interface'i, LocalStore'da SQLite implementasyonu).

**Cross-DB atomicity sınırı (Faz 6 → Wave 5B):** Mapping save Writer tarafından (SQL Server transaction commit'ten sonra) yapılıyor;
mapping save başarısız olursa Mikro tarafında evrak oluşur ama mapping kaydı yoktur → sonraki retry idempotency hit bulamaz.
**Wave 5B `CrossDbReconciliationWorker` bu durumu periyodik olarak tespit eder ve alarm üretir** (otomatik düzeltme yok).

---

## 8. Logging & Secret Politikası

- `ConnectionStringMasker` (Shared): `Password`/`Pwd`/`User ID`/`UID` anahtarlarını
  `***REDACTED***` ile değiştirir. Case-insensitive, multi-equals güvenli.
- `AgentConfigMasker` (Core): log için `AgentConfig` klonu, `is_secret=1` alanları `********`.
- `IProtectedConfigProvider`: secret alanlar SQLite'ta düz metin değil, encrypt-at-rest.
  - Windows: DPAPI (`DpapiProtectedConfigProvider`)
  - Linux/macOS: AES-256-GCM (`AesProtectedConfigProvider`)
- Key dosyası: `KeyStore` — Windows `Hidden` attribute, Linux/macOS `0600` izni.
- Serilog config ile production'da secret property'ler maskelenir; **asla** düz metin loglanmaz.

---

## 9. Test & Build Komutları

```powershell
# Build (her zaman temiz olmalı)
dotnet build ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor

# Test (varsayılan: integration skip'li)
dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor

# Integration test (MSSQL docker ayağa kalktıktan sonra)
docker compose -f tests/docker-compose.test.yml up -d
ERPBridge_RUN_INTEGRATION=1 dotnet test ErpBridge.sln -p:EnableWindowsTargeting=true -p:RollForward=LatestMajor
```

---

## 10. Bilinen Sınırlar / Açık İşler (Faz 16+ Backlog)

| # | Konu | Öncelik | Durum |
|---|---|---|---|
| 1 | Logo / Paraşüt / Netsis adapter | Orta | Yapılmadı (kullanıcı "şimdilik atla" dedi) |
| 2 | Yeni trigger şeması (`_ERPB_SYNC` + `_ERPB_SYNC_DEL`) kurulumu | Yüksek | Kod var, 4 test yarım kaldı (Faz 15.2 fix gerekli) |
| 3 | `TriggerChangeSetReader` ↔ Yeni trigger şeması bağlantısı | Yüksek | Faz 15.3+15.4 ayrı track |
| 4 | Blazor UI auto-refresh (access token expire olunca) | Orta | Token store hâlâ eski davranışta |
| 5 | Admin Web UI "Reconciliation Health" sayfası | Orta | Worker logluyor, UI yok |
| 6 | Android snapshot bölümleri (collections, paymentOrders, dispatchNotes) agent upload path | Düşük | Snapshot endpoint'leri var, agent doldurmuyor |
| 7 | Refresh token cleanup job (DB shrink) | Düşük | 30 gün sonra şişer |
| 8 | Mapping key firma/şube bağlamı (multi-tenant) | Düşük | Şu an tenant-scoped, multi-firma gelince ayrı track |

---

## 11. Yeni AI'a Minimum Yükleme Protokolü

1. Bu dosyayı (`PROJECT-CONTEXT.md`) yapıştır.
2. Üzerinde çalışılacak fazın `deliverable-*.md`'sini yapıştır.
3. Spesifik görev için ilgili source dosyalarını ek olarak yapıştır.

**Yapıştırma:**
- ❌ `docs/architecture.md` (zaten burada özetlendi)
- ❌ `docs/api-contracts.md` (Faz 4 sözleşmeyse onu da ekle, yoksa gereksiz)
- ❌ `docs/mikro-v15-v16-rules.md` (kurallar burada)
- ❌ `README.md` (proje tanıtımı, bu dosyada var)
- ❌ Tüm solution ağacı
- ✅ Spesifik track deliverable'ı (örn. `deliverable-irsaliye.md`)
- ✅ Düzenlenecek / eklenecek source dosyalar

---

**Son güncelleme:** 2026-09-08 — Wave 1-5 tamamlandı, 592/592 + 20 skip, 0 FAILED. 7 ERP writer + 2 ingest pattern + 1 reconciliation worker + refresh token + WPF validation eklendi. Faz 15.2 trigger installer 4 test yarım kaldı (Faz 16 backlog). Logo/Paraşüt adapter kullanıcı kararıyla atlandı.
