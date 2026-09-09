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
- `_ERPB_SYNC` / `_ERPB_SYNC_DEL` AFTER trigger'ları. Kurulum olmayan tabloları
  atlar (Mikro yalnızca lisanslı modülleri kurar).
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

### Silme ve değişiklik su-seviyeleri ayrıdır (Faz 20.D)

SQL Server change-log'da insert/update olayları `_ERPB_SYNC`, delete olayları
`_ERPB_SYNC_DEL` tablosundan gelir — **ayrı IDENTITY dizileri**. `SyncTableChangeSet`
her ikisini de taşır (`New/UpsertSequence` + `New/DeleteSequence`) ve `change_sets`
satır kimliği `(TenantId, SourceDatabase, TableName, LastTriggerRecNo, LastDeleteRecNo)`.
Tek bir sayıya katlamak, **sadece silme içeren bir döngüyü** önceki döngünün
kopyası gibi gösterip silmeleri sessizce düşürürdü.

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
(`RefreshSnapshotInTriggerMode=true`, `BootstrapIntervalSeconds=20`). Trigger-only
mod (delete-only) yalnızca `*_lastup_date`'i olmayan bir ERP'de mantıklı; o zaman
shadow-log `upsert` kuyruğu Android tarafına bağlanmalı (ileride
`/android/changeset/*/new_or_changed`).

---

## ERP'de Silinen Satırların Snapshot'tan Düşürülmesi (Faz 21)

### Sorunun kaynağı

Aktif bootstrap snapshot'ı, `MergeIncrementalChunksAsync` tarafından **anahtara göre birleştirilen** bir projeksiyondur: her artımlı yükleme satırlarını bir öncekinin üzerine upsert eder. Birleştirme bir `isDeleted` tombstone'unu anlar (`AddItems` → `records.Remove(key)`), ama **bunu üreten hiçbir şey yoktu**:

- `MikroDbReader` iptal satırlarını `WHERE ISNULL(*_iptal, 0) = 0` ile sonuç kümesinden **eler**, silinmiş olarak bildirmez.
- ERP'den tamamen kalkmış bir satır ise delta'da hiç görünmez — yokluk, birleştirme için "değişmedi" demektir.

Sonuç: ERP'de silinen satır snapshot'ta **süresiz** kalır ve `/sync/cari`, `/sync/urun`, `/sync/faturaHareket` tarafından her cihaza servis edilmeye devam eder. Android tarafındaki silme kuyruğu düzgün çalışsa bile bir sonraki tam indirmede satır geri gelirdi.

### Çözüm: `SnapshotDeleteApplier`

Shadow-log hangi satırların kalktığını zaten biliyor. `ChangeSetEndpoints.IngestAsync` artık bir bundle'daki silme olaylarını toplayıp aktif snapshot'tan da düşürüyor:

```text
_ERPB_SYNC_DEL  ──>  /api/v1/ingest/changeset
                          │
                          ├─> mobile_sync_queue  (operation=delete)   [mobil kuyruk]
                          └─> SnapshotDeleteApplier                    [YENİ]
                                    │
                                    ▼
                          bootstrap_snapshot_chunks'tan satırı çıkar
                          + snapshot.PulledAtUtc'yi ilerlet
```

- Eviction `SaveChangesAsync`'ten **önce** çalışır; kuyruk satırı, change-set kaydı ve snapshot yeniden yazımı tek transaction'dadır. Cihaz, snapshot eviction'ı kaybolmuş bir silme olayını asla göremez.
- `PulledAtUtc` ilerletilir — yoksa cihazlar paketi "değişmedi" sayıp bayat kopyalarını korurdu.
- Hiçbir satır eşleşmezse bölüm yeniden yazılmaz (`removed == 0` → no-op).

### Kimlik çevirimi

Silme olayı yalnızca **fiziksel satır kimliğini** taşır. Bölüm bazında hangi kimliğin kullanıldığı farklıdır:

| ERP tablosu | Snapshot bölümü | Eşleşme alanı | Çeviri gerekli mi |
|---|---|---|---|
| `STOKLAR` | `stocks`, `barcodes`, `prices`, `inventory`, `salesConditions` | `stockCode` | **Evet** — `sto_RECno` → `sto_kod` |
| `CARI_HESAPLAR` | `customers`, `customerAddresses`, `customerContacts`, `salesConditions` | `customerCode` | **Evet** — `cari_RECno` → `cari_kod` |
| `STOK_HAREKETLERI` | `stockTransactions` | `id` | Hayır — `id` zaten `sth_RECno` |
| `CARI_HESAP_HAREKETLERI` | `customerTransactions` | `id` | Hayır — `id` zaten `cha_RECno` |

Çeviri `AddMobileQueueItems` içinde iki kademeli yapılır:
1. Aynı bundle'ın kendi upsert satırları (aynı pencerede oluşup silinen kayıt için tek kaynak).
2. `mobile_sync_queue`'daki en son `upsert` satırının `PayloadJson`'u — `sto_kod`/`cari_kod` orada durur.

> **Eski hata:** bu arama `PayloadJson` yerine `RecordKey` kolonunu seçiyordu; `RecordKey == SourceRecordKey` olduğu için işlem no-op'tu ve mobil silme hiçbir satırı tutturamıyordu. Bu düzeltme, mobil tarafında `erp_record_map` bulunmayan eski uygulama sürümleri için de silmeyi doğru hale getirir.

### Kasıtlı olarak yapılmayanlar

- **Hareketler kaskad edilmez.** Mikro'da stok kartı silindiğinde `STOK_HAREKETLERI` satırları durur; uygulama da ERP'nin hâlâ sahip olduğu evrakları göstermeye devam etmelidir. Hareket satırı ancak kendisi silindiğinde düşer.
- `openOrders`, `cashAndBank`, `lookups` bölümleri kapsam dışıdır; ilgili tablolar shadow-log kataloğunda silme takibi yapmıyor.

### Açık kalan: ana veride gerçek artımlı okuma

Ajan tarafı **zaten artımlı** (`ReadBootstrapChangesAsync`, `*_lastup_date` filtresi) ve boş delta yeni snapshot yaratmaz (`BootstrapSyncService`, `IsEmpty(package)` kontrolü) — yani `PulledAtUtc` yalnızca ERP gerçekten değiştiğinde ilerler.

Asimetri **okuma** tarafındadır: `CustomersAsync` / `ProductCatalogAsync` / `SectionAsync` `AndroidPageRequest.Since` alanını hiç okumaz ve her istekte birleştirilmiş bölümün tamamını sayfalayarak döner. Bu yüzden tek bir satışın yarattığı küçük delta, cihazda tam katalog indirmesine dönüşür.

`mobile_sync_queue`'nun `operation=upsert` akışı bu boşluğu kapatacak veriyi taşır (satır + tüm kolonları), ancak Android'de bunu **veri olarak uygulayan** bir tüketici yoktur — mevcut tüketici yalnızca kimlik haritası için anahtar çıkarır.
