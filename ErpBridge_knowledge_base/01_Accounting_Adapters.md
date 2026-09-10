# 01 — ErpBridge ERP Entegrasyon Katmanı ve Writer Mimarisi

> **Hedef:** ErpBridge'in ERP sistemlerine veri yazma/okuma operasyonları,
> adaptör deseni, change-log motoru ve atomik mutabakat mekanizması.
>
> **Not:** Mikro tam implementasyon; `ErpBridge.Erp.Logo` iskelet örneğidir.
> Yeni adaptör eklemek için: [`docs/erp-adapter-contract.md`](../docs/erp-adapter-contract.md)

İlgili diğer modüller:
- Genel Mimari & Kurallar: [[00_System_Overview]]
- Veri Şeması ve Tablolar: [[03_Data_Dictionary_and_Rules]]
- AI Sorgulama Araçları: [[04_AI_Assistant_Function_Catalog]]

---

## 1. Adaptör Deseni

Host'lar (`Agent.Service`, `Agent.UI`) hiçbir ERP tipini görmez — tek giriş
noktası `ErpAdapterRegistration.AddErpBridgeErpAdapter(erpType, config)`
switch'idir. Adaptörü olmayan bir ERP için startup'ta hata verir.

```text
IErpAdapter (Erp.Abstractions)
├── MikroAdapter        (Erp.Mikro)  — tam
└── LogoAdapter         (Erp.Logo)   — iskelet, tüm write metotları NotImplementedException

IErpChangeLogSource (Erp.Abstractions)
└── SqlServerShadowTableChangeLog (Erp.Sql) — vendor bilgisi SIFIR
    ← katalog + KeyKindProjection + ShadowTableOptions verilir, gerisi hazır
```

### Mikro V15 / V16 Kimlik Stratejisi (`IMikroIdentityStrategy`)

```text
                  +--------------------------+
                  |  IMikroIdentityStrategy  |
                  +--------------------------+
                ┌──────────────┴──────────────┐
    +-----------------------+     +-----------------------+
    |     RecnoStrategy     |     |     GuidStrategy      |
    |      (Mikro V15)      |     |      (Mikro V16)      |
    +-----------------------+     +-----------------------+
    | PK: SCOPE_IDENTITY()  |     | PK: uygulama Guid'i   |
    | Self-link: *_RECid_*  |     | Self-link YOK         |
    +-----------------------+     +-----------------------+
```

**Versiyon algılama (`MikroVersionDetector`):** `INFORMATION_SCHEMA.COLUMNS`
üzerinden `*_Guid` kolonlarının varlığını sorgular; varsa `GuidStrategy` (V16),
yoksa `RecnoStrategy` (V15). Sonuç TTL'li cache'lenir. Bu versiyon hem writer
stratejisini hem `MikroTrackedTableCatalog.For(version)` katalog seçimini
belirler.

---

## 2. ERP Writer Modülleri (7 evrak tipi)

`IErpAdapter`'da 7 write metodu; hepsi `Task<ErpWriteResult>` döner. Kolon adları
canlı Mikro şemasına karşı doğrulanmıştır.

| Writer | Evrak Tipi | Hedef Tablo | Gerçek Kritik Kolonlar |
|---|---|---|---|
| `MikroSalesOrderWriter` | `sales_order` | `SIPARISLER` — **satır başına bir satır** | `sip_musteri_kod`, `sip_stok_kod`, `sip_b_fiyat`, `sip_miktar`, `sip_iskonto_1..6`, `sip_vergi_pntr`, `sip_evrakno_seri/sira/satirno`, `sip_tip=0`, `sip_cins=0` |
| `MikroCollectionWriter` | `collection` | `CARI_HESAP_HAREKETLERI` | `cha_kod` (cari), `cha_meblag`, `cha_tip=1` (alacak), `cha_d_cins`, `cha_evrak_tip=63`, `cha_evrakno_seri/sira/satir_no` |
| `MikroPaymentOrderWriter` | `payment_order` | `ODEME_EMIRLERI` (önek `sck_`) | `sck_sahip_cari_kodu`, `sck_bankano`, `sck_tutar`, `sck_doviz`, `sck_vade`, `sck_tip` (kanal→kod), `sck_refno` (kanal+açıklama katlanır) |
| `MikroDispatchNoteWriter` | `dispatch_note` | `STOK_HAREKETLERI` (önek `sth_`) | `sth_stok_kod`, `sth_cari_kodu`, `sth_miktar`, `sth_tutar`, `sth_birim_pntr`, `sth_vergi_pntr`, `sth_cikis_depo_no`, `sth_tip=1` (çıkış), `sth_evraktip=4` |
| `MikroInvoiceWriter` | `invoice` | `CARI_HESAP_HAREKETLERI` + `STOK_HAREKETLERI` | Header `cha_` (cha_tip=0 borç, evrak_tip=63), satırlar `sth_`; aynı transaction |
| `MikroCustomerCardWriter` | `customer_card` | `CARI_HESAPLAR` | `cari_kod`, `cari_unvan1`, `cari_vdaire_no`, `cari_vdaire_adi`, `cari_EMail`, `cari_CepTel`, `cari_odeme_gunu`, `cari_grup_kodu` — adres/telefon ayrı tablolarda |
| `MikroStockCardWriter` | `stock_card` | `STOKLAR` + `BARKOD_TANIMLARI` | `sto_kod`, `sto_isim`, `sto_kisa_ismi`, `sto_birim1_ad`, `sto_perakende_vergi`, `sto_toptan_vergi`, `sto_anagrup_kod`; barkod: `bar_kodu`, `bar_stokkodu`, `bar_birimpntr` — her ikisi firma-bağımsız |

### Ortak Writer Altyapısı

| Bileşen | İş |
|---|---|
| `MikroSelfLink` | V15'te `(*_RECid_DBCno, *_RECid_RECno)` UNIQUE index; INSERT benzersiz negatif placeholder tohumlar, aynı transaction'da `RECid_RECno = RECno`'ya çözer |
| `MikroCurrency` | ISO kod (`"TRY"`) → Mikro `tinyint` döviz kodu (0=TL). Bilinmeyen kod exception |
| `MikroDocumentCodes` | `sth_tip/cins/evraktip/*_pntr` ve `cha_tip/evrak_tip/cinsi` kod taksonomileri tek yerde |
| `MikroDocumentNumberAllocator` | `evrakno_sira` = `MAX+1` (transaction içinde, `UPDLOCK HOLDLOCK`). Payload numara taşıyorsa ona saygı |
| `ErpFieldText` (Erp.Sql) | Kimlik alanı taşarsa **exception**, serbest metin **kırpılır** |
| `SqlServerFieldWidthProvider` (Erp.Sql) | Genişlikleri canlı `INFORMATION_SCHEMA`'dan keşfeder + cache'ler |

### Atomik Transaction ve Idempotency

Yazım sırası (referans standardı): payload validate → `(tenant, doc_type,
external_id)` idempotency lookup → lookup kontrolleri → versiyon tespiti → evrak
sıra tahsisi → transaction başlat → header + satırlar + self-link → commit →
`mappings` kaydı → merkeze ack.

**Asla:** aynı iş ikinci evrak açmaz · header commit edilip satırlar yazılmadan
bitmez · ERP transaction başarısızken success ack gönderilmez · kullanıcı girdisi
string concat ile SQL'e girmez.

---

## 3. Çok Firmalı / Çok Şubeli Çalışma

`AgentConfig`: `CompanyNo` (varsayılan 1), `BranchNo` (1), `WarehouseNo` (1).
Firma-**bağımlı** tablolara enjekte edilir: `cari_firmano`, `cari_sube_no`,
`sip_firmano`, `sip_subeno`, `cha_firmano`, `cha_subeno`, `sth_firmano`,
`sth_subeno`, `sck_firmano`, `sck_subeno`. **`STOKLAR` ve `BARKOD_TANIMLARI`
firma-bağımsızdır** — bu kolonlar orada yoktur.

---

## 4. Değişiklik Yakalama (`SqlServerShadowTableChangeLog`)

Vendor bilgisi sıfır, `ErpBridge.Erp.Sql`'de. Mikro (ve Logo) sadece katalog +
anahtar projeksiyonu + kendi `ShadowTableOptions`'ını verir.

- **İki native tipli shadow anahtar kolonu:** `KayitRECno int NULL` **ve**
  `KayitGuid uniqueidentifier NULL`. Her tablo `ErpRowKeyKind`'ına uyanı doldurur.
  Okuyucu `T.[key] = S.KayitRECno` ile **doğrudan** join yapar (sargable).
- **`TabloID` shadow satırındaki tek ayırt edici alandır** — katalog içinde
  benzersiz olmalı, yayınlandıktan sonra asla yeniden numaralanmaz. Referansta
  `STOK_KATEGORILERI`/`STOK_SEKTORLERI` id 8'i paylaşıyordu; ErpBridge birine
  benzersiz id verdi.
- Genel SQL Server adaptöründe `_ERPB_SYNC` / `_ERPB_SYNC_DEL` AFTER trigger'ları.
  **Mikro V15 bu genel kurulum yolunu kullanmaz:** mevcut
  `_ERPB_SENKRONIZASYON` tablosunu (`Islem`: 0 silme, 1 güncelleme, 2 ekleme)
  salt okunur kaynak olarak tüketir ve ERP'de yeni tablo/trigger oluşturmaz.
  Katalog her kurulumun
  **üst kümesidir** (Mikro yalnızca lisanslı modülleri kurar), bu yüzden
  `InstallAsync`, `IsInstalledAsync` **ve** `ReadChangesAsync` üçü de
  `sys.tables`'da bulunmayan tabloları atlar. Bu filtre okuma tarafında
  eksikse: var olmayan tabloya join → SQL hata 208 (`Invalid object name`) →
  tüm batch düşer, cursor ilerlemez, delta sync kalıcı olarak takılır.
  `IsInstalledAsync`'te eksikse: yok olan tablolar hep "trigger'ı eksik"
  sayılır, her cycle gereksiz yere yeniden kurulum çalışır.
- ⚠️ Logo/Netsis'te trigger kurmak vendor'ın desteklenen konfigürasyonu dışında
  kalabilir — **SQL Server Change Tracking** alternatifi.

## 5. Idempotency ve Mutabakat (`CrossDbReconciliationWorker`)

1. Her işlem öncesi `mappings` tablosu kontrol edilir; kayıt varsa ERP'ye hiç
   gidilmeden mevcut `recno`/`guid` döner.
2. `CrossDbReconciliationWorker` periyodik olarak `mappings` satırlarını
   `IErpReconciliationProbe` ile (Mikro impl: `MikroReconciliationProbe`)
   çapraz kontrol eder; yetim mapping'i alarm olarak merkeze bildirir. Sadece
   okur, ERP'yi asla değiştirmez.

## 6. Sync Servisi (`ErpChangeLogSyncService`, Core)

ERP-bağımsız. `IErpAdapterFactory` + `IErpSyncCursorStore` + `IRemoteApiClient`'a
bağlı. Bir cycle: change log oku → merkeze push → **push kabul edildikten SONRA**
cursor'u ilerlet. Arada crash → sayfa tekrar oynatılır (her olay idempotent
upsert veya anahtarlı delete). Mikro'ya bağlı eski `TriggerChangeSetSyncService`
kaldırıldı.

### Silme ve değişiklik su-seviyeleri

Mikro V15'te insert/update/delete olaylarının tamamı `_ERPB_SENKRONIZASYON`
tablosundaki tek `TriggerRECno` dizisini paylaşır. Okuyucu her `TabloID` için bu
ortak sırayı cursor olarak saklar. Genel SQL Server adaptöründe ise upsert ve
delete tablolarının ayrı su-seviyeleri korunur.

### Olay-güdümlü sync (long-poll)

`GET /api/v1/android/notify?wait=30` — mobil uzun-yoklama ucu. WPF için olan
`bootstrap/notify` ile aynı `IBootstrapNotificationHub`'ı paylaşır ama API-key ile
kimliklenir ve **hem bootstrap snapshot yüklemesi hem agent change-set push'u**
(`ChangeSetEndpoints.IngestAsync` → `hub.Publish`) ile uyanır.

Android tarafında `LiveSyncManager.run(context)` uygulama ön plandayken
(`repeatOnLifecycle(STARTED)`) bu ucu yoklar; sinyal gelince anında
`SyncManager.startSyncAll` tetikler. Arka planda `PeriodicSyncWorker` (15 dk)
yedek kalır. Sonuç: ERP değişikliği cihaza **saatlik yerine ~5-20 sn**'de ulaşır.

### Android silme kuyruğu

Android `sync/queue?operation=delete` çağırır; `BridgeSyncHelper.syncMobileDeleteQueue`
her sayfayı tek koşuda drenaj eder (cursor ilerlemediğinde durur). Silmeler
`STOKLAR`/`CARI_HESAPLAR`/`CARI_HESAP_HAREKETLERI`/`STOK_HAREKETLERI`/`SIPARISLER`
için Room `deleteById(recordKey)` ile uygulanır. **Değişiklik (update) verileri Android'e `*_lastup_date` tabanlı bootstrap-delta
ile ulaşır** — `sync/urun`/`sync/cari` uçları `bootstrap_snapshots`'tan sayfa döner.
`BootstrapWorker` trigger modunda (`UseTriggerBasedSync=true`) her iterasyonda
**hem** change-log **hem** snapshot-delta cycle'ını çalıştırır
(`RefreshSnapshotInTriggerMode=true`, `BootstrapIntervalSeconds=20`). WPF
"Senkronize Et" butonu (`DashboardViewModel.RunSyncDeltaAsync`) ise operatörün
isteğiyle yalnızca `_ERPB_SENKRONIZASYON` change-log push'unu çalıştırır;
bootstrap snapshot yenilemez. Tam snapshot veya boyut ölçümlü aktarım ayrı
"Bootstrap" butonundadır.

### Chunked bootstrap upload — bölüm bazlı merge (2026-09-10 düzeltmesi)

`HttpRemoteApiClient.PushChunkedBootstrapDataAsync` her push'ta (otomatik
`BootstrapWorker` döngüsü dahil) **tüm 13 bölümü** gönderir — değişmeyen
bölümler tek boş chunk (`items: []`) olarak "placeholder" gider
(`SendChunksAsync`). `POST /bootstrap/upload/{id}/complete`
(`BootstrapUploadEndpoints.CompleteAsync` → `MergeIncrementalChunksAsync`),
staged snapshot'ta görünen **her** bölümü önceki aktif snapshot'la JSON
seviyesinde eşitleyip (parse + `Dictionary<key, JsonNode>` + yeniden
serileştirme) yeni chunk satırları yazıyordu.

**Bug (2026-09-10 öncesi):** `customerTransactions`/`stockTransactions` gibi
"yıllarca ledger hareketi" içerebilen bölümlerde bu tam-yeniden-inşa her
20-60 sn'lik döngüde tekrarlanıyordu — hiçbir satır değişmese bile. Ayrıca
`PushSectionAsync` ile tek bölüm push'u (`/complete`'in yalnızca gönderilen
bölümleri gezmesi yüzünden) **gönderilmeyen diğer bölümlerin verisini
sessizce siliyordu** — `previous` snapshot cascade-delete ile kaldırılırken o
bölümler hiç `staged`'a taşınmamış oluyordu.

**Düzeltme:** `MergeIncrementalChunksAsync` artık önce o bölümde **gerçek
(ItemCount > 0) bir değişiklik var mı** diye bakıyor. Yoksa (bölüm hiç
gönderilmemiş VEYA boş placeholder olarak gelmiş) pahalı JSON round-trip'i
atlayıp önceki snapshot'ın chunk satırlarını ucuz bir `SnapshotId`
güncellemesiyle yeni snapshot'a **taşıyor**. Gerçek değişiklik olan bölümler
hâlâ tam merge'den geçiyor (anahtar bazlı upsert/delete mantığı bozulmadı).
Bkz. `tests/ErpBridge.CentralApi.Tests/Endpoints/BootstrapUploadTests.cs`
(`Single_section_incremental_push_does_not_wipe_other_sections`,
`Incremental_push_with_no_changes_in_a_section_carries_its_chunk_rows_forward_unchanged`).

### `/bootstrap/upload/{id}/complete` HTTP 500 ve WPF "kitlenmesi" (2026-09-10)

`ui-20260910.log`'daki asıl arıza yukarıdaki merge maliyeti **değildi**:
`/complete` her seferinde ~0,2–3,9 sn içinde HTTP 500 dönüyordu (zaman aşımı
olsa 30 sn+ sürerdi). İki ayrı hata üst üste biniyordu.

**1) Sunucu — aktif snapshot benzersiz indeksi ihlali.**
`bootstrap_snapshots` üzerinde `(TenantId) WHERE "IsActive" = true` filtreli
**unique** indeks var (`IX_bootstrap_snapshots_TenantId_Active`).
`CompleteAsync` ise önceki snapshot'ı pasifleştirmeyi ve staged snapshot'ı
aktifleştirmeyi **tek `SaveChangesAsync`** içinde yapıyordu. EF Core bir
batch'teki aynı-tablo UPDATE'lerini birincil anahtara göre sıralar; sıra
`staged.IsActive = true` önce gelecek şekilde çıktığında PostgreSQL 23505
verip endpoint 500 dönüyordu. GUID'lere bağlı olduğu için **denemelerin
yaklaşık yarısında** patlıyor, aynı `uploadId`'nin her retry'ında ise aynı
sırayla tekrar patlıyordu.
*Düzeltme:* önceki snapshot'lar kendi round-trip'inde silinip flush ediliyor,
staged ancak ondan sonra aktifleştiriliyor (ikisi de aynı transaction'da).

**2) Ajan — çift retry katmanı.**
`ServiceCollectionExtensions.IsBootstrapRequest` yalnızca eski tekil uç olan
`/api/v1/bootstrap` yolunu tanıyordu; chunked upload yolları
(`/api/v1/bootstrap/upload/...`) HttpClient'ın 5+15+60+300 sn'lik Polly
politikasına takılıyordu. Bu, `BootstrapSyncService`'in kendi 5/15/60 sn'lik
pipeline'ının **altına** yerleşiyor, üstüne bir de 9 bölümlük fallback
geliyordu: tek bir `/complete` POST'u logda `397986 ms` sürüyor, "customers"
fallback'i 51 dakika sonra hata veriyordu. WPF'te "kitleniyor" denen şey buydu
(UI thread bloke değil — `RunBootstrapAsync` tamamen async; buton saatlerce
`IsBusy` kalıyordu).
*Düzeltme:* `SkipsTransportRetry` (eski adı `IsBootstrapRequest`) artık
bootstrap **yazma** yollarını (`/api/v1/bootstrap` + `/bootstrap/upload/...`) ve
kendi yeniden bağlanma döngüsü olan `/bootstrap/notify` long-poll'unu kapsıyor.

> ⚠️ `GET /bootstrap/status` bilerek **dışarıda**. O sonda kendi retry'ı olmayan
> tek bootstrap çağrısı: `BootstrapSyncService.RunOnceAsync` hatayı "status
> unavailable" diye yutup döngüyü **tam snapshot'a** düşürüyor. Yıllarca hareket
> taşıyan bir tenant'ta bu, küçük bir GET'i yeniden denemekten çok daha pahalı.
> Alt ağacın tamamını kapsamak bu regresyonu yaratmıştı (PR #19 kod incelemesi).

**Test altyapısı notu:** `CentralApiFactory` EF Core **InMemory** sağlayıcısını
kullanır; InMemory unique index'leri uygulamaz ve transaction desteği yoktur —
bu yüzden yukarıdaki 500 testlerden kaçtı. Yeni
`SqliteCentralApiFactory` (`ConfigureDatabase` hook'u ile) aynı host'u ilişkisel
bir SQLite dosyası üzerinde ayağa kaldırır;
`Endpoints/BootstrapUploadRelationalTests` 20 farklı tenant üzerinden
`/complete`'i koşturarak sıralamaya bağlı ihlali yakalar. **Kısıt/benzersizlik
davranışına dayanan yeni CentralApi testleri InMemory factory'ye değil bu
ilişkisel factory'ye yazılmalı.**

### `MikroAdapter.ChangeLog` asla fırlatmaz (2026-09-10)

`ErpChangeLogSyncService.RunOnceAsync`, `adapter.ChangeLog`'u **try bloğunun
dışında** dereference ediyor; `BootstrapWorker.RunSingleIterationAsync` ise
`catch (Exception)` ile **tüm iterasyonu** sarıyor — trigger modunda change-log
pass'inden *sonra* çalışan snapshot-delta pass'i dahil.

Dolayısıyla bu property'den kaçan bir istisna sadece change-log senkronunu
kapatmıyor, **iterasyonun tamamını** iptal ediyor: o veritabanı bootstrap
yenilemesi de almıyor. İki tetikleyicisi vardı:

1. **V16 veritabanı.** `_ERPB_SENKRONIZASYON` feed'i V15 RECno anahtarlarına
   dayanıyor; V16 Guid kullanıyor. Eski kod `NotSupportedException` fırlatıyordu,
   ama `ChangeDetection` hâlâ `ShadowTableChangeLog` ilan ettiği için tüketici
   zarif düşüş yapamıyordu.
2. **Geçici bağlantı hatası.** `MikroVersionDetector.DetectAsync` bağlantıyı
   `try/catch` olmadan açar — sunucu bir an erişilemezse `SqlException` fırlar.
   `Lazy<T>` istisnayı **kalıcı olarak cache'lediği** için tek bir kesinti,
   süreç yeniden başlatılana kadar senkronu öldürüyordu. ("Defaulting to V15"
   uyarısı yalnızca bağlantı *kurulduktan* sonraki belirsiz sürüm için çıkar,
   bağlantı hatası için değil.)

**Düzeltme:** `ChangeLog` artık `null` döner, fırlatmaz. V16 kesin bir cevaptır
ve cache'lenir; probe hatası **kesin değildir** ve cache'lenmez, sonraki cycle
yeniden dener. Tüketici zaten `ChangeLog is not { } changeLog` kontrolüyle
"change log yok" deyip snapshot yoluna düşüyor. Bkz.
`tests/ErpBridge.Erp.Mikro.Tests/Adapters/MikroAdapterChangeLogTests.cs`.
