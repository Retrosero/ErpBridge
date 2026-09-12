# 03 — ErpBridge Veri Sözlüğü ve Muhasebe İş Kuralları

> **Hedef:** ErpBridge ekosistemindeki Mikro SQL Server tablolarını, CentralApi
> PostgreSQL şemasını, SQLite LocalStore tablolarını ve kritik muhasebe
> kurallarını içerir.
>
> **⚠️ Kolon adları canlı `MikroDB_V15_02` + `MikroDB_V16_03` veritabanlarına
> karşı doğrulanmıştır** (`MikroSchemaContractTests`). Önceki sürüm kolon
> adlarını tahmin ediyordu ve 63 tablodan 34'ünde yanlıştı.

İlgili diğer modüller:
- Genel Mimari & Kurallar: [[00_System_Overview]]
- ERP Adaptörleri & Yazıcılar: [[01_Accounting_Adapters]]
- AI Parametrik Fonksiyonları: [[04_AI_Assistant_Function_Catalog]]

---

## 1. Hedef ERP (Mikro SQL Server) Tablo Sözlüğü

> **Önek tuzağı:** Tablo öneki tablo adından türetilemez.
> `STOK_HAREKETLERI` → `sth_`, `STOKLAR` → `sto_`, `ODEME_EMIRLERI` → `sck_`,
> `CARI_HESAP_HAREKETLERI` → `cha_`, `SIPARISLER` → `sip_`.

| Tablo Adı | Açıklama | Birincil Anahtar (V15 / V16) | Gerçek Kritik Kolonlar |
|---|---|---|---|
| `CARI_HESAPLAR` | Cari kart ana tablosu | `cari_RECno` / `cari_Guid` | `cari_kod`, `cari_unvan1`, `cari_vdaire_no`, `cari_vdaire_adi`, `cari_EMail`, `cari_CepTel`, `cari_grup_kodu`, `cari_odeme_gunu` — **bakiye kolonu YOK**, hareketlerden hesaplanır |
| `CARI_HESAP_HAREKETLERI` | Cari ekstre + fatura başlıkları | `cha_RECno` / `cha_Guid` | `cha_tarihi`, `cha_evrakno_seri`, `cha_evrakno_sira`, `cha_satir_no`, `cha_kod` (cari kod), `cha_meblag` (tutar), `cha_tip` (**0=borç, 1=alacak**), `cha_evrak_tip`, `cha_d_cins` (döviz kodu), `cha_vade` (vade **gün sayısı**, tarih değil) |
| `STOKLAR` | Ürün ve malzeme kartları | `sto_RECno` / `sto_Guid` | `sto_kod`, `sto_isim` (V15: 50, V16: 127 karakter), `sto_kisa_ismi`, `sto_birim1_ad`, `sto_birim1_katsayi`, `sto_perakende_vergi`, `sto_toptan_vergi`, `sto_anagrup_kod`, `sto_cins` — **firma-bağımsız** (sto_firmano YOK), **fiyat kolonu YOK** (ayrı fiyat listesi tablosunda) |
| `STOK_HAREKETLERI` | İrsaliye + fatura satır hareketleri | `sth_RECno` / `sth_Guid` | `sth_tarih`, `sth_tip` (0=giriş, 1=çıkış), `sth_evraktip`, `sth_evrakno_seri`, `sth_evrakno_sira`, `sth_satirno`, `sth_stok_kod`, `sth_cari_kodu`, `sth_miktar`, `sth_tutar`, `sth_birim_pntr`, `sth_vergi_pntr`, `sth_cikis_depo_no`, `sth_giris_depo_no` |
| `SIPARISLER` | Alınan/verilen siparişler — **satır başına bir satır** (header tablosu yok) | `sip_RECno` / `sip_Guid` | `sip_tarih`, `sip_tip` (0=müşteri, 1=satınalma), `sip_cins`, `sip_evrakno_seri`, `sip_evrakno_sira`, `sip_satirno`, `sip_musteri_kod`, `sip_satici_kod`, `sip_stok_kod`, `sip_miktar`, `sip_b_fiyat`, `sip_birim_pntr`, `sip_iskonto_1..6`, `sip_vergi_pntr`, `sip_depono`, `sip_doviz_cinsi`, `sip_kapat_fl` |
| `ODEME_EMIRLERI` | Çek, senet, ödeme emirleri — önek `sck_` | `sck_RECno` / `sck_Guid` | `sck_duzen_tarih`, `sck_vade`, `sck_sahip_cari_kodu`, `sck_bankano`, `sck_tutar`, `sck_doviz`, `sck_tip` (0=çek, 1=senet, 2=nakit), `sck_refno` (serbest metin, 25 karakter) — **açıklama kolonu YOK** |
| `BARKOD_TANIMLARI` | Stoklara bağlı çoklu barkodlar — **firma-bağımsız** | `bar_RECno` / `bar_Guid` | `bar_kodu` (25), `bar_stokkodu` (25, string bağ), `bar_birimpntr`, `bar_barkodtipi` |

### V15 / V16 Kimlik Farkı

| | V15 | V16 |
|---|---|---|
| Satır kimliği | `int *_RECno` (IDENTITY) | `uniqueidentifier *_Guid` |
| Self-link çifti | `(*_RECid_DBCno, *_RECid_RECno)` — UNIQUE index, konvansiyon `RECid_RECno = RECno` | **Kaldırılmış** — kimlik doğrudan Guid |
| `*_RECno` kolonları | Var | **Tamamen kaldırılmış** |
| Örnek: `sto_isim` genişliği | 50 karakter | 127 karakter |

V16 bir üst küme **değil**. Bu yüzden `MikroV15TrackedTableCatalog` (49 tablo) ve
`MikroV16TrackedTableCatalog` (50 tablo) ayrıdır; `MikroTrackedTableCatalog.For(version)`
seçer.

---

## 2. Merkezi SaaS API (PostgreSQL) Şeması

CentralApi tarafından yönetilen multi-tenant veri modeli:

- `tenants`: Müşteri kiracı kayıtları (`id`, `name`, `status`, `created_at`).
- `licenses`: Ajan lisansları (`id`, `tenant_id`, `license_key`, `expires_at`, `max_agents`).
- `agents`: Kayıtlı Windows Sync Agent'lar (`id`, `tenant_id`, `machine_id`, `last_heartbeat_at`).
- `jobs`: Mobil → Agent yazma iş kuyruğu (`id`, `tenant_id`, `document_type`, `payload`, `status`).
- `change_sets`: Agent → merkez değişiklik paketleri (`ErpType`, `TableKey`, `TableName`, `LastTriggerRecNo`, `PayloadJson`). *(Faz 20 — ERP-nötr: eski `TabloID` int kaldırıldı, `TableKey` string + `ErpType` eklendi.)*
- `jobs`: Mobil → Agent yazma iş kuyruğu — Faz 20'de `ErpType` kolonu eklendi (çok-ERP tenant'ta doğru adaptöre yönlendirme).
- `change_set_audit_log`: Senkronizasyon denetim izleri.
- `mobile_sync_queue`: ERP → Android olay günlüğü (`sequence`, `entity`, `operation`, `recordKey`, `payload`). *(Faz 26 ile yerini `mobile_records`'a bırakıyor; geçiş süresince ikisine de yazılır.)*
- `bootstrap_snapshots` + `bootstrap_snapshot_chunks`: Android'in sıfırdan çektiği tam durum. Chunk'lar `jsonb` dizileri; `(TenantId)` üzerinde `IsActive = true` ile filtrelenmiş **kısmi unique index** var — aktif snapshot'ı değiştiren her kod bunu tek `SaveChanges` içinde yapmamalı (bkz. `BootstrapUploadEndpoints.CompleteAsync`).
  - **Chunk `ReceivedAtUtc` = bölümün sürümü (Faz 29, 2026-09-12).** Artımlı yüklemede değişmeyen bölümlerin chunk'ları yeni snapshot'a `ReceivedAtUtc` korunarak taşınır (`MergeIncrementalChunksAsync`), değişen bölüm ve `SnapshotDeleteApplier`'ın yeniden yazdığı bölüm yeni zaman alır. `POST /api/v1/android/bootstrap` bunu `sectionVersions` (bölüm → `MAX(ReceivedAtUtc)`) olarak döner; mobil `SnapshotSync` tablo bazında bununla atlama kararı verir (bkz. Siparis_Cepte KB 00, kural 5). Bu alan yalnızca chunk'lı snapshot yolunda vardır; eski `bootstrap_packages` yolunda dönmez. Chunk'ı `ReceivedAtUtc`'yi güncellemeden yeniden yazan bir kod, o bölümün değişikliğini cihazlardan **gizler**.
- `mobile_records` *(Faz 26)*: Mobilin gördüğü her kaydın **güncel hâli** — olay günlüğü değil, `(TenantId, Entity, RecordKey)` başına tek satır. `Entity` = bootstrap bölüm adı (`stocks`, `customers`, `prices`…). Cihaz `UpdatedSeq` sırasına göre sayfalar; **ilk kurulum ile günlük delta aynı sorgudur** (`UpdatedSeq > cursor`). `IsDeleted` tombstone, `PayloadSha256` değişmemiş satırın imleci ilerletmesini engeller, `StockKey`/`CustomerKey` ebeveyn silmesinin indeksli cascade'ini taşır. `SourceRecordKey` (2026-09-12) kartın ERP fiziksel kimliğidir (RECno/Guid, küçük harf); silme olayı iş anahtarına buradan çevrilir. `SourceDatabase` bu kimliği hangi ERP veritabanının ürettiğini taşır — birden fazla veritabanı aynı tenant'a beslendiğinde aynı RECno çakışabileceği için çeviri sorgusu `(TenantId, Entity, SourceDatabase, SourceRecordKey)` ile daraltılır.
- `tenant_sync_counter` *(Faz 26)*: `mobile_records.UpdatedSeq` için tenant başına tahsis sayacı (`LastSeq`, `TombstoneHorizonSeq`).
- `parameter_records`: Müşteri bazlı konfigürasyon parametreleri.

---

## 3. SQLite LocalStore (Ajan İçi Depolama)

Windows Agent'ın yerel SQLite veritabanı — gerçek tablo adları:

| Tablo | İçerik |
|---|---|
| `mappings` | `(tenant_id, entity_type, document_type, external_id)` UNIQUE — idempotency + ERP kimliği (`recno` / `guid`), `erp_type`, `erp_version`, `erp_database_name` |
| `local_jobs` | Ağ koptuğunda merkeze iletilecek yerel iş kuyruğu |
| `checkpoints` | Resume cursor'ları — `sync_scope`: bootstrap için, `erpcursor:<ErpType>` change-log cursor'ı için (opak token), eski `trigger:<TABLO>` satırları reset'te temizlenir |
| `agent_config` | Ajan ayarları (`erp_database_name`, `ErpType`, Firma/Şube/Depo No, Bağlantı Stringi). Secret alanlar DPAPI ile şifreli |
| `schema_version` | Migration versiyonu |

---

## 4. Muhasebe ve Bakiye Hesaplama Kuralları

### Cari Bakiye Formülü

`CARI_HESAPLAR`'da bakiye kolonu **yoktur**; `CARI_HESAP_HAREKETLERI` üzerinden
canlı hesaplanır. Yön fatura/tahsilat etiketiyle değil, `cha_tip` ile belirlenir:

```sql
SUM(CASE WHEN ISNULL(cha_tip, 0) = 0 THEN ISNULL(cha_meblag, 0)
                                     ELSE -ISNULL(cha_meblag, 0) END)
```

- `cha_tip = 0` → borç (satış faturası, borç dekontu). Bakiye artar.
- `cha_tip = 1` → alacak (tahsilat, iade faturası). Bakiye azalır.
- Net Bakiye > 0: Borçlu cari (firmaya borcu var).
- Net Bakiye < 0: Alacaklı cari (avans / firmadan alacağı var).

**Yazma tarafı karşılığı:** `MikroCollectionWriter` tahsilatı `cha_tip = 1`
(alacak) yazar. Yanlış değer her carinin bakiyesini sessizce tersine çevirir.

### Yaşlandırma / Vade

`cha_vade` bir **gün sayısıdır** (tarih değil). Vade tarihi = `cha_tarihi + cha_vade gün`.

### Çoklu İskonto Sıralaması (Mikro Native 6 Kademeli)

`sip_iskonto_1` ila `sip_iskonto_6` ardışık olarak bir önceki indirimli tutara
uygulanır:

$$Tutar_1 = Brüt - (Brüt \times \tfrac{Isk_1}{100}), \quad Tutar_2 = Tutar_1 - (Tutar_1 \times \tfrac{Isk_2}{100}), \quad \dots \quad Matrah = Tutar_6$$

### Evrak Seri / Sıra

`(evrak_tip, evrakno_seri, evrakno_sira, satır_no)` üzerinde UNIQUE index vardır.
Payload sıra taşımıyorsa (`0`) `MikroDocumentNumberAllocator` transaction içinde
`MAX(sıra)+1` tahsis eder; `UPDLOCK, HOLDLOCK` ile eş zamanlı tahsisler sırayla
işlenir.

### String Alan Genişlikleri

Genişlikler **canlı `INFORMATION_SCHEMA`'dan** keşfedilir (`SqlServerFieldWidthProvider`) —
sürüme göre değişir. `ErpFieldText` kimlik alanını taşarsa **reddeder**
(kısaltılmış kod başka hesapla eşleşebilir), serbest metni **kırpar**. Mikro
genişlikleri dardır: `evrakno_seri` 6, `cari_kod` 25, `cha_aciklama` 40.
