# ErpBridge — Çok-ERP Adaptör Dönüşüm Planı (Faz 16+)

> **Amaç:** ErpBridge'i tek ERP'ye (Mikro) gömülü hâlden, Logo ve Netsis
> adaptörlerinin **aynı çalışma mantığını** (SQL Server + shadow-table +
> trigger tabanlı değişiklik yakalama) yeniden kullanabileceği bir
> ports-&-adapters mimarisine taşımak.
>
> **Kapsam kararı:** Bu turda **yalnızca Mikro'ya uyumlu parçalar dikkatlice
> soyutlanacak.** Logo/Netsis kodu yazılmayacak — sadece seam (dikiş)
> onların da bağlanabileceği şekilde açılacak. Uygulama test aşamasında
> olduğu için **geriye dönük uyumluluk / veri migration zorunluluğu yok**;
> namespace ve şema kırıcı değişiklikler serbest.
>
> **Bağlam:** Kod tabanı analizi ve mevcut kuplaj tespitleri için bkz.
> [architecture.md](architecture.md), [mikro-v15-v16-rules.md](mikro-v15-v16-rules.md).

---

## 0. Mevcut Durum Özeti (analiz çıktısı)

### 🟢 Hazır olan
- Temiz proje referans grafiği — `Core`, `RemoteApi`, `LocalStore`, `CentralApi`
  Mikro assembly'sine referans vermiyor.
- `IErpAdapter` + `IErpAdapterFactory` + `ErpType` enum var; Logo/Netsis/Paraşüt rezerve.
- Adaptör seçimi config'ten: `_adapterFactory.Create(config.ErpType)`
  (`AgentWorker`, `BootstrapSyncService`, `TriggerChangeSetSyncService`).
- Bootstrap sync payload DTO'ları (`SyncPackage` + `*Payload`) büyük ölçüde ERP-nötr.
- `IMappingStore` / idempotency ERP-nötr; `MappingRecord.ErpType` alanı var.
- SalesOrder yazımı zaten soyutlama üzerinden akıyor.

### 🟡 Sızıntı (kolay–orta)
- `IErpAdapter` sözleşmesinde sadece `WriteSalesOrderAsync` var. Diğer 6 evrak
  (invoice, collection, dispatch_note, payment_order, customer_card, stock_card)
  sözleşmede yok; `AgentWorker` bunlara `UNSUPPORTED_DOCUMENT_TYPE` dönüyor.
  `MikroInvoiceWriter` + `MikroDispatchNoteWriter` DI'a **kayıtlı bile değil**.
- `IErpAdapter.DetectVersionAsync` → `ErpVersionInfo` içinde `MikroVersion`,
  `IsV15`/`IsV16` — Mikro kavramları abstractions'a sızmış.
- `ErpType` iki kez tanımlı: `Core.Domain` + `Erp.Abstractions` (cast'lenerek kullanılıyor).
- `Core.Domain` payload'ları "Mikro-only" dokümante (`InvoicePayload`, `CollectionPayload`…).
- `AgentConfig.MikroDatabaseName`, `Shared.Constants` / `Shared.Error` Mikro isimli.

### 🔴 Mimari borç
- **Değişiklik-yakalama motoru Mikro şemasına gömülü ve en dipteki projede:**
  `ErpBridge.Shared/TrackedTableSchema.cs` — 49 Mikro tablosunun sabit kataloğu +
  kolon listeleri + Mikro'nun `TabloID` sayı uzayı. `Shared.SyncChangeSet` Mikro-şekilli.
- Trigger cursor modeli `IReadOnlyDictionary<int,int> lastTriggerByTabloId`
  (TabloID→TriggerRECno) — `IErpAdapter.ReadChangeSetAsync` imzasına sızmış.
- `ITriggerWatermarkStore` Mikro-şekilli (TabloID→int).
- `Agent.Service` + `Agent.UI` 12 dosyada doğrudan `using ErpBridge.Erp.Mikro.*`.
  `Program.cs` factory yerine doğrudan `AddErpBridgeMikro()` çağırıyor.
  WPF ViewModel'leri `Mikro.Adapters` / `Mikro.Connection` tiplerine bağlı.
- Timestamp-delta yolu (`MikroDbReader`): her sorguda
  `COALESCE(<önek>_lastup_date, <önek>_create_date, ...) > @changedSinceUtc`
  + Türkiye yerel saati dönüşümü (`MikroDateTime()`). **DELETE yakalayamaz.**

### Change-detection: ERP'ler arası gerçeklik
| ERP | Taşıma | Satır-değişiklik damgası | Shadow+trigger uygulanır mı? |
|---|---|---|---|
| **Mikro** | MSSQL | `*_lastup_date` / `*_create_date` her tabloda | ✅ (mevcut `_ERPB_SYNC` / `_ERPB_SYNC_DEL`) |
| **Logo** (Tiger/GO/WINGS) | MSSQL | `CAPIBLOCK_MODIFIEDDATE` çoğu `LG_*` tabloda, bazılarında yok | ✅ aynı desen uyarlanabilir (PK = `LOGICALREF`) |
| **Netsis** | MSSQL | Kapsama eksik, birçok master tabloda yok | ✅ aynı desen uyarlanabilir (custom trigger şart) |
| **Paraşüt** (ileride) | REST | `updated_at` + webhook | ❌ farklı `IErpChangeLogSource` impl'i |

**Sonuç:** Logo ve Netsis de MSSQL olduğu için mevcut trigger/shadow mantığı
gerçekten taşınabilir. İş, bu mantığı Mikro'ya özgü tablo kataloğundan ve
`int`/`RECno` varsayımından ayırıp **yeniden kullanılabilir bir SQL Server
change-log katmanına** çıkarmak.

---

## Tasarım İlkeleri

1. **Her fazın sonunda Mikro yeşil kalır.** `dotnet build` sıfır uyarı
   (`TreatWarningsAsErrors=true`), `dotnet test` geçer.
2. **Opak cursor.** Değişiklik-yakalama ilerleme işareti adaptöre ait bir
   token'dır (string/JSON). Çekirdek onun içini bilmez.
3. **Yetenek bildirimi.** Adaptör hangi change-detection modunu desteklediğini
   söyler; çekirdek buna göre davranır (full / timestamp-delta / change-log).
4. **Katalog + PK stratejisi adaptörden gelir.** Ortak SQL Server change-log
   motoru; tablo listesi ve anahtar projeksiyonu her ERP'nin kendi adaptöründe.
5. **Host kodu ERP assembly'si görmez.** Tek istisna: `Program.cs` / DI extension.

---

## Faz 16 — Kimlik & Sürüm Soyutlaması Temizliği ✅
**Boyut:** S · **Risk:** Düşük · **Durum:** Tamamlandı (branch `faz16-multi-erp-abstraction`)

Amaç: Mikro'ya özgü tip/isim sızıntılarını en ucuz yerden kapatmak.

- [x] `Core.Domain.ErpType` **silindi**; tek kaynak `Erp.Abstractions.ErpType`.
      `BootstrapSyncService`, `AgentWorker`, `TriggerChangeSetSyncService`'teki
      `(Abstractions.ErpType)config.ErpType` cast'leri kaldırıldı.
- [x] `Core.Domain.MikroVersion` **silindi**; `Erp.Abstractions.MikroVersion` kaldı.
- [x] `ErpVersionInfo`: `ErpType Erp` (default `Mikro`) + `string? Family` +
      `DisplayLabel` eklendi. `Version`/`IsV15`/`IsV16` şimdilik kaldı
      (Mikro adapter + WPF rozeti kullanıyor; nötr yol Faz 21'de kanıtlanınca temizlenecek).
      *`IErpAdapter.DetectVersionAsync` imzası değişmedi — Faz 18'e ertelendi.*
- [x] `AgentConfig`: `MikroDatabaseName` → `ErpDatabaseName`; `ErpOptions`
      (`Dictionary<string,string>`) eklendi. WPF VM property + XAML binding + SQLite
      key (`nameof`) + tüm appsettings.json anahtarları güncellendi.
- [x] `Shared/Constants.cs` + `Shared/Error.cs` — Mikro geçen **doküman yorumları**
      nötrlendi (üye adları zaten nötrdü; rename gerekmedi).
- [x] `Shared/AgentSettingsValidation.cs` — `mikroDatabaseName` parametresi →
      `erpDatabaseName`; hata mesajı "ERP veritabanı adı boş olamaz".
- [x] SQLite `agent_config` anahtarı `MikroDatabaseName` → `ErpDatabaseName`
      (test aşaması; eski satırlar orphan kalır, migration yok).

**Doğrulama:** `dotnet build` 0 uyarı / 0 hata; `dotnet test` 605 test yeşil (20 skip).

---

## Faz 17 — Yazım (Write) Sözleşmesinin Tamamlanması ✅
**Boyut:** M · **Durum:** Tamamlandı

Amaç: 7 evrak tipinin tamamını soyutlama üzerinden akıtmak.

- [x] 6 payload `Core.Domain` → `Erp.Abstractions.Documents`'e taşındı
      (`InvoicePayload`, `CollectionPayload`, `DispatchNotePayload`,
      `PaymentOrderPayload`, `CreateCustomerRequest`, `CreateStockRequest`).
      Writer + test `using`'leri güncellendi.
- [x] `IErpAdapter`'a 6 metot eklendi (hepsi `Task<ErpWriteResult>`):
      `WriteInvoiceAsync`, `WriteCollectionAsync`, `WriteDispatchNoteAsync`,
      `WritePaymentOrderAsync`, `WriteCustomerCardAsync`, `WriteStockCardAsync`.
- [x] `MikroInvoiceWriter` + `MikroDispatchNoteWriter` DI'a eklendi (loggerlarıyla).
- [x] `MikroAdapter` 6 metodu writer'lara delege ediyor;
      `_serviceProvider.GetRequiredService<T>()` ile lazy resolve (ctor churn yok).
      Card writer'ların `CreateResult` + throw-on-invalid sözleşmesi
      `RunCardWriteAsync` ile `ErpWriteResult`'a adapte edildi.
- [x] `AgentWorker`: `DispatchDocumentAsync` — bilinmeyen tip önce reddedilir
      (ERP bağlantısı açılmadan), sonra `switch (documentType)` → `Parse<T>` →
      `adapter.Write*Async` → `ToAck`. `Failed`/`ToAck` yardımcıları eklendi.
- [x] Testler: `AdapterContractTests.FakeAdapter` 6 metotla güncellendi;
      `AgentWorkerProcessJobTests`'e invoice-happy + collection-rejected +
      güncellenmiş unsupported-type testleri eklendi.
- [~] `SalesOrderPayloadDeserializer` → `ErpDocumentPayloadDeserializer` yeniden
      adlandırması **ertelendi**: sales_order özel shape-validation yolunu
      korudum, diğer 6 tip generic `System.Text.Json` ile parse ediliyor.

**Doğrulama:** `dotnet build` 0 uyarı; `dotnet test` 607 geçti / 20 atlandı.

---

## Faz 18 — Değişiklik-Yakalama Soyutlaması *(asıl mimari iş)*
**Boyut:** L · **Risk:** Yüksek · **Tahmini:** 2–3 hafta (alt-parçalara bölünebilir)

Amaç: `_ERPB_SYNC` trigger/shadow mantığını Mikro'dan söküp Logo/Netsis'in de
konfigüre edip kullanabileceği ortak bir SQL Server katmanına taşımak.

### 18.1 — Sözleşmeler (Abstractions)
- [ ] `ChangeDetectionCapability` enum: `FullSnapshotOnly | TimestampDelta | ShadowTableChangeLog`.
- [ ] `ErpSyncCursor` — **opak** token (`string Value`; içi adaptöre ait,
      Mikro'da `{"tableKey": lastTriggerRecno}` JSON'u).
- [ ] `ErpChangeRow(ChangeOp Op, string TableKey, string KeyValue, IReadOnlyDictionary<string,object?> Columns)`
      — `KeyValue` tagged string (`recno:123`, `guid:...`, `logicalref:...`).
- [ ] `ErpChangeBatch(IReadOnlyList<ErpChangeRow> Rows, ErpSyncCursor CursorAfter, bool MoreAvailable)`.
- [ ] `IErpChangeLogSource`:
      `Task<bool> IsInstalledAsync(ct)`, `Task InstallAsync(ct)`,
      `Task<ErpChangeBatch> ReadChangesAsync(ErpSyncCursor cursor, int maxRows, ct)`.
- [ ] `IErpAdapter`: `ReadChangeSetAsync(Dictionary<int,int>...)` **kaldır**;
      yerine `ChangeDetectionCapability ChangeDetection { get; }` +
      `IErpChangeLogSource? ChangeLog { get; }` (null = desteklemiyor).

### 18.2 — Ortak SQL Server change-log motoru
Yeni proje: **`ErpBridge.Erp.Sql`** (referans: `Shared`, `Core`, `Erp.Abstractions`,
`Microsoft.Data.SqlClient`, `Dapper`).
- [ ] `IErpTrackedTableCatalog` — `IReadOnlyList<ErpTrackedTable>` (tablo adı,
      şema, PK kolon(lar)ı, soft-delete filtresi, senkronize edilecek alanlar).
- [ ] `IErpKeyProjection` — PK satır değeri → tagged `KeyValue` string.
- [ ] `SqlServerShadowTableChangeLog : IErpChangeLogSource` — bugünkü
      `TriggerSchema` + `TriggerInstaller` + `TriggerChangeSetReader` mantığının
      ERP-bağımsız hâli. Shadow tablo adları / şema / prefix parametrik.
- [ ] `NewSchemaTriggerInstaller` ve `TriggerInstaller` bu projeye taşınır;
      DDL şablonlarındaki Mikro tablo adları katalogdan gelir.

### 18.3 — `Shared` temizliği
- [ ] `Shared/TrackedTableSchema.cs` (49 Mikro tablosu) → **`Erp.Mikro/ChangeLog/MikroTrackedTableCatalog.cs`**.
- [ ] `Shared/SyncChangeSet.cs`: wire tipi sadeleşir; `SyncTableDescriptor.TabloID (int)`
      yerine `string TableKey`. `Deleted` chunk `(int KayitRecNo, int TriggerRecNo)`
      → `(string KeyValue, string CursorToken)`.
- [ ] `Shared` artık hiçbir Mikro tablo/kolon adı içermez.

### 18.4 — Cursor deposu
- [ ] `ITriggerWatermarkStore` → `IErpSyncCursorStore`
      (`Task<ErpSyncCursor?> GetAsync(erpType, ct)`, `Task SetAsync(erpType, cursor, ct)`).
      SQLite şeması: `erp_sync_cursor(erp_type TEXT PK, cursor_json TEXT, updated_at)`.
- [ ] `SqliteTriggerWatermarkStore` (Agent.Service + Agent.UI'de kopya var) →
      tek `SqliteErpSyncCursorStore` (`LocalStore` projesinde).

### 18.5 — Sync servisi
- [ ] `TriggerChangeSetSyncService` → `ErpChangeLogSyncService`
      (`Erp.Sql` veya `Core`'da; `IErpChangeLogSource` + `IErpSyncCursorStore` +
      `IRemoteApiClient`'a bağlı, Mikro tipine değil). Polly retry aynen korunur.
- [ ] `Core/DependencyInjection`'daki "ChangeSetSyncService Mikro'da register edilir"
      notu kalkar; artık Core/Erp.Sql'de register edilir.

### 18.6 — Mikro adaptörünü yeni sözleşmeye bağla
- [ ] `MikroAdapter.ChangeDetection => ShadowTableChangeLog`.
- [ ] `MikroAdapter.ChangeLog => new SqlServerShadowTableChangeLog(MikroTrackedTableCatalog, MikroKeyProjection, MikroShadowOptions)`.
- [ ] `MikroKeyProjection` — V15 `recno:`, V16 `guid:` (bugünkü `KeyValueResolver` mantığı).
- [ ] Timestamp-delta yolu (`ReadBootstrapChangesAsync` + `MikroDbReader`
      `*_lastup_date` sorguları + `MikroDateTime()` TZ dönüşümü) Mikro adapter
      içinde **kapsüllü kalır**; `ChangeDetectionCapability.TimestampDelta`
      ikinci yetenek olarak bildirilir ama shadow-log mevcutken o tercih edilir.
- [ ] **Regresyon testi:** aynı Mikro DB'de eski ve yeni yolun ürettiği change-set
      satır-satır aynı olmalı (golden-file testi).

**Doğrulama:** Mikro DB'de bir cari güncelle + bir sipariş sil → yeni
`ErpChangeLogSyncService` her iki olayı da (`Update`, `Delete`) yakalayıp merkeze
push eder; cursor ilerler; yeniden başlatmada tekrar göndermez.

---

## Faz 19 — Host'u (Agent.Service + Agent.UI) Mikro Assembly'sinden Koparma
**Boyut:** M · **Risk:** Orta (WPF) · **Tahmini:** 1–2 hafta

Amaç: `Agent.Service` ve `Agent.UI` kod gövdesinde `using ErpBridge.Erp.Mikro`
sıfır (yalnızca DI extension'da referans).

- [ ] `Agent.Service/Program.cs`: `AddErpBridgeMikro()` doğrudan çağrısı →
      `services.AddErpBridgeErpAdapter(config.ErpType, ctx.Configuration)` —
      `switch` ile doğru modülü register eden yeni extension (şimdilik yalnızca
      `ErpType.Mikro => AddErpBridgeMikro(...)`, diğerleri `throw NotSupported`).
- [ ] `Agent.Service/Workers`:
      - `SqliteTriggerWatermarkStore` sil → Faz 18.4'teki `SqliteErpSyncCursorStore`.
      - `MikroReconciliationProbe` → `IErpReconciliationProbe` (arayüz Abstractions;
        Mikro impl'i `Erp.Mikro`'da; DI `switch`).
      - `BootstrapWorker` `using ErpBridge.Erp.Mikro.Trigger` kaldır → `IErpChangeLogSyncService`.
- [ ] `Agent.UI`:
      - `ServiceCollectionExtensions` `AddErpBridgeMikro` doğrudan → ortak `AddErpBridgeErpAdapter`.
      - `AgentSettingsViewModel` / `DashboardViewModel`: `Mikro.Adapters` /
        `Mikro.Connection` tipleri → `IErpAdapterFactory` + yeni `IErpConnectionTester`
        (arayüz Abstractions).
      - `MikroConnectionTestOrchestrator` → arayüz `IErpConnectionTestOrchestrator`
        (Abstractions), impl Mikro'da.
- [ ] WPF ayar ekranı: ERP'ye özgü alanlar (`MainWindow.xaml` — SQL Server,
      Kullanıcı, DB, Firma/Şube/Depo No, Windows Auth) `IErpSettingsPanel` arkasına.
      Şimdilik tek panel = `MikroSettingsPanel`; `ErpType` seçicisi UI'da görünür
      ama yalnızca "Mikro" aktif.
- [ ] `Agent.Service.csproj` / `Agent.UI.csproj` yine `Erp.Mikro`'ya
      `ProjectReference` verir (DI için) — kabul edilir; hedef **kod içi `using` = 0**.

**Doğrulama:** `grep -rn "using ErpBridge.Erp.Mikro" Agent.Service Agent.UI` →
yalnızca DI extension dosyaları. WPF'den Mikro'ya bağlanma + bootstrap + sync uçtan uca çalışır.

---

## Faz 20 — CentralApi & Wire-Format ERP-Nötrleştirme
**Boyut:** M · **Risk:** Orta · **Tahmini:** ~1 hafta

- [ ] `CentralApi/Domain` Mikro geçen tipler (`ChangeSetRecord`, `BootstrapPackage`,
      `ErpCompany`, `Job`, `JobAckRecord`, `ParameterRecord`) — alan/yorum isimleri generic.
- [ ] `jobs` + `change_sets` tablolarına `erp_type` kolonu (mapping'de zaten var).
- [ ] `change_sets.payload_json` şeması `TabloID` → `tableKey` string.
- [ ] `AndroidEndpoints` / `ParameterEndpoints`: mobil URL sözleşmesi
      (`sync/cari`, `sync/urun`) korunur; içerik ERP-nötr DTO'ya çevrilir.
- [ ] **Not (kapsam dışı):** `Siparis_Cepte` (Android) bu turda dokunulmuyor.
      CentralApi, mobilin beklediği JSON şeklini bozmadan üretmeye devam etmeli;
      mobil tarafın ERP-nötrleştirmesi ayrı bir iş kalemi.

**Doğrulama:** `CentralApi.Tests` yeşil; Android emülatörü mevcut şemayla sync olur.

---

## Faz 20.5 — Siparis_Cepte (Android) — Değerlendirme: **fonksiyonel değişiklik gerekmiyor**
**Boyut:** XS · **Risk:** Yok · **Tahmini:** 0 (opsiyonel temizlik ~1 gün)

Android istemci incelendi. Wire sözleşmesi **zaten ERP-nötr materyalize DTO'lar**
üzerinden akıyor — Mikro şeması istemciye sızmıyor:

- `ErpBridgeApi.kt` yalnızca materyalize uçları çağırıyor: `sync/cari`, `sync/urun`,
  `sync/stokSeviye`, `sync/cariHareketleri`, `sync/faturaHareket`, `sync/queue`.
  Ham `changeset/{TABLO}` ucu **hiç çağrılmıyor** (Faz 15 doküman var, kod yok).
- `CariDto` / `UrunDto` savunmacı: birincil alanlar iş-nötr (`customerCode`,
  `title1`, `urun_kodu`, `barkod`, `satis_fiyati`, `kdv`); Mikro kolon adları
  (`cari_vdaire_no`, `sto_marka_kodu`…) yalnızca **opsiyonel fallback alias**.
  `actualXxx` accessor'ları hangi alias gelirse çözüyor.
- Giden evrak (`MikroPayloadHelper`) iş terimleri kullanıyor (`cariKodu`,
  `stokKodu`, `evrakSeri`) — Mikro kolonu değil. Windows Agent'ın writer'ı
  `cariKodu → cari_kod` eşlemesini kendi yapıyor. **Ayrıca bu sınıf şu an
  hiçbir yerden çağrılmıyor (ölü/test kodu).**
- `MobileSyncQueueItem`: `entity` / `operation` / `recordKey` / `payload` nötr;
  yalnızca `table` + `triggerRecNo` alan adları Mikro-kokuyor (kozmetik).

**Bu turda yapılacak: hiçbir şey.** Android, Faz 16–20 boyunca mevcut şemayla
çalışmaya devam eder (Faz 20 DoD'si zaten "mobil şema bozulmaz").

**İkinci ERP geldiğinde (Faz 21 sonrası) opsiyonel temizlik:**
- [ ] `MikroPayloadHelper` → `ErpDocumentPayloadHelper` (veya ölü kodsa sil).
- [ ] `MobileSyncQueueItem.table/triggerRecNo` → `sourceKey/cursor`.
- [ ] DTO'lardaki Mikro-kolon alias'larını, gerçekten Logo/Netsis'ten hangileri
      gelmiyorsa, o zaman buda. Şimdi budarsak hangi alias'ın önemli olduğunu
      bilemeyiz — bekle.

> Not: Kök dizindeki `Siparis_Cepte/TestApi.kt` ve `new_erp_screen.kt` başıboş
> dosyalar; pakete dahil değiller, bu işin kapsamı dışında.

---

## Faz 21 — Logo / Netsis İskeleti *(kod yok — seam doğrulama)*
**Boyut:** S · **Risk:** Düşük · **Tahmini:** 3–5 gün

Amaç: Soyutlamanın gerçekten yeterli olduğunu, adaptör yazmadan kanıtlamak.

- [ ] `ErpBridge.Erp.Logo` + `ErpBridge.Erp.Netsis` projeleri (referans: `Shared`,
      `Erp.Abstractions`, `Erp.Sql`).
- [ ] Her birinde: `LogoAdapter : IErpAdapter` (tüm metotlar `throw new NotImplementedException`),
      `LogoTrackedTableCatalog` (birkaç örnek tablo: `LG_XXX_ITEMS`, `LG_XXX_CLCARD`…),
      `LogoKeyProjection` (`logicalref:`).
- [ ] `IErpAdapterFactory` `switch`'i Logo/Netsis için hâlâ `NotSupportedException`
      döner ama **derlenir**.
- [ ] İskeleti yazarken abstraction'da eksik/Mikro-kokan bir şey çıkarsa →
      Faz 18/19'a geri dön, düzelt. Bu fazın asıl çıktısı **gap listesi**.

**Doğrulama:** Çözüm 12 → 15 projeyle derlenir; `dotnet test` yeşil; gap listesi dokümante.

---

## Faz 22 — Dokümantasyon & Sözleşme Dondurma
**Boyut:** S · **Tahmini:** 2 gün

- [ ] `docs/erp-adapter-contract.md` — yeni adaptör eklemek için checklist
      (hangi arayüzler, hangi capability, katalog/PK stratejisi, testler).
- [ ] `docs/mikro-v15-v16-rules.md` — yeni soyutlamaya göre güncelle.
- [ ] `ErpBridge_knowledge_base/01_Accounting_Adapters.md` + kök `knowledge_base` — güncelle.
- [ ] `development-roadmap.md` — Faz 16–22 özeti eklenir.

---

## Sıra & Bağımlılık

```
Faz 16 ──> Faz 17 ──> Faz 18 ──> Faz 19 ──> Faz 20 ──> Faz 21 ──> Faz 22
 (kimlik)   (write)   (change-   (host      (central   (Logo/     (doküman)
                       log)       decouple)  api)       Netsis
                                                        iskelet)
                                              │
                                       Faz 20.5: Android — no-op
                                       (sözleşme zaten ERP-nötr)
```

- 16–17 birbirinden bağımsız başlatılabilir ama 17'den önce 16 önerilir (cast temizliği).
- **18 kritik yol** — istenirse alt-fazlara (18.1…18.6) bölünüp ayrı PR'lar.
- 21, 18+19'un yeterliliğini test eder; buradan 18/19'a geri dönüş normaldir.
- Mikro DB'ye canlı bağlanma işi (kullanıcının "ilk olarak" dediği) mevcut
  `MikroConnectionFactory` / `MikroConnectionTestOrchestrator` ile **bugün
  çalışıyor**; 16–17 bu katmanı bozmaz, paralel ilerleyebilir.

## Kaba Efor Toplamı
| Faz | Efor |
|---|---|
| 16 | 2–3 gün |
| 17 | 4–6 gün |
| 18 | 2–3 hafta |
| 19 | 1–2 hafta |
| 20 | ~1 hafta |
| 21 | 3–5 gün |
| 22 | 2 gün |
| **Toplam** | **~7–9 hafta** |

## Her PR İçin Tanım-Tamamlandı (DoD)
- `dotnet build` — 0 uyarı (`TreatWarningsAsErrors=true`).
- `dotnet test` — tüm projeler yeşil.
- Mikro uçtan uca smoke: bağlantı testi + bootstrap + değişiklik sync + 1 sipariş yazımı.
- `grep` kontrolü: yeni fazın hedeflediği Mikro sızıntısı gerçekten kapandı.
- İlgili KB / docs güncellendi.
