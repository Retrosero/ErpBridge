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
| `CARI_HESAP_HAREKETLERI` | Cari ekstre + fatura başlıkları | `cha_RECno` / `cha_Guid` | `cha_tarihi`, `cha_evrakno_seri`, `cha_evrakno_sira`, `cha_satir_no`, `cha_kod` (cari kod), `cha_meblag` (tutar), `cha_tip` (**0=borç, 1=alacak**), `cha_evrak_tip`, `cha_d_cins` (döviz kodu), `cha_vade` (tahsilat satırında **yyyymmdd tarih**, Mikro'nun kendi faturalarında 0 — Fora faturada ödeme planı no yazar) |
| `STOKLAR` | Ürün ve malzeme kartları | `sto_RECno` / `sto_Guid` | `sto_kod`, `sto_isim` (V15: 50, V16: 127 karakter), `sto_kisa_ismi`, `sto_birim1_ad`, `sto_birim1_katsayi`, `sto_perakende_vergi`, `sto_toptan_vergi`, `sto_anagrup_kod`, `sto_cins` — **firma-bağımsız** (sto_firmano YOK), **fiyat kolonu YOK** (ayrı fiyat listesi tablosunda) |
| `STOK_HAREKETLERI` | İrsaliye + fatura satır hareketleri | `sth_RECno` / `sth_Guid` | `sth_tarih`, `sth_tip` (0=giriş, 1=çıkış), `sth_evraktip`, `sth_evrakno_seri`, `sth_evrakno_sira`, `sth_satirno`, `sth_stok_kod`, `sth_cari_kodu`, `sth_miktar`, `sth_tutar`, `sth_birim_pntr`, `sth_vergi_pntr`, `sth_cikis_depo_no`, `sth_giris_depo_no` |
| `SIPARISLER` | Alınan/verilen siparişler — **satır başına bir satır** (header tablosu yok) | `sip_RECno` / `sip_Guid` | `sip_tarih`, `sip_tip` (0=müşteri, 1=satınalma), `sip_cins`, `sip_evrakno_seri`, `sip_evrakno_sira`, `sip_satirno`, `sip_musteri_kod`, `sip_satici_kod`, `sip_stok_kod`, `sip_miktar`, `sip_b_fiyat`, `sip_birim_pntr`, `sip_iskonto_1..6`, `sip_vergi_pntr`, `sip_depono`, `sip_doviz_cinsi`, `sip_kapat_fl` |
| `ODEME_EMIRLERI` | Çek, senet, ödeme emirleri — önek `sck_` | `sck_RECno` / `sck_Guid` | `sck_duzen_tarih`, `sck_vade`, `sck_sahip_cari_kodu`, `sck_bankano`, `sck_tutar`, `sck_doviz`, `sck_tip` (0 müşteri çeki, 1 müşteri senedi, 2 kendi çekimiz, 3 kendi senedimiz, 4 müşteri havale sözü, 6 müşteri kredi kartı), `sck_refno` (`MC/MS/MH/MK-fff-sss-yyyy-nnnnnnnn`, tekil `(sck_tip, sck_refno)`) — **açıklama kolonu YOK** |
| `_ERPB_EVRAK_ESLESME` | **ErpBridge'in kendi tablosu** (Mikro nesnesi değil): telefondan yazılan her Mikro evrakının idempotency kaydı, evrakla **aynı transaction'da** yazılır; tekil `(DocumentType, ExternalId)`. İlk yazımda yoksa oluşturulur (`MikroDocumentLedger`) | `Id` IDENTITY | `DocumentType`, `ExternalId`, `DocumentTable`, `EvrakTip`, `EvrakSeri` (6), `EvrakSira`, `HeaderRecNo`, `CreatedAt` |
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
- `jobs`: Mobil → Agent yazma iş kuyruğu — Faz 20'de `ErpType` kolonu eklendi (çok-ERP tenant'ta doğru adaptöre yönlendirme). Faz 41'de `CreatedByUserId` (`uuid`, boş olabilir): belgeyi gönderen oturum açmış firma kullanıcısı; onay merkezinden gelen belgede **talep eden** (onaylayan değil). API anahtarı, ajan ve Faz 41 öncesi satırlarda boştur. Yönetici panelinin plasiyer bazlı raporu buna dayanır. *ERP yazım Y1e (2026-09-17):* `LeasedUntilMs` ve `NextAttemptAtMs` (`bigint`, boş olabilir, Unix ms — SQLite testlerinde `DateTimeOffset` karşılaştırılamadığı için). Kiralama 10 dk; süresi dolan `Processing` iş yeniden kiralanır, 10. denemeden sonra `Failed`. Ajan ack'inde `retryable=true` ise iş 1-2-4-8-15-30-60 dk bekleyip `Pending`'e döner (ack kaydı `retry`). Kolonlar eklenmeden önce kiralanmış işlerde `LeasedUntilMs` boştur ve süresi dolmaz. *ERP yazım Y5 (2026-09-17):* şema değişmedi; `job_acks.Status` `succeeded` | `failed` | `retry` (ajan `retryable=true` bildirdi). Portal `POST /portal/erp-documents/{id}/retry` yalnız `Failed`/`DeadLetter` işi `Pending` + `RetryCount = 0` yapar (yeni deneme hakkı); Admin `POST /admin/jobs/{id}/retry` her durumdaki işi `Pending` yapıp `RetryCount`'u artırır. İkisi de depo kuyruğunun `ErpState`'ini günceller.
- `change_set_audit_log`: Senkronizasyon denetim izleri.
- `mobile_sync_queue`: ERP → Android olay günlüğü (`sequence`, `entity`, `operation`, `recordKey`, `payload`). *(Faz 26 ile yerini `mobile_records`'a bırakıyor; geçiş süresince ikisine de yazılır.)*
- `bootstrap_snapshots` + `bootstrap_snapshot_chunks`: Android'in sıfırdan çektiği tam durum. Chunk'lar `jsonb` dizileri; `(TenantId)` üzerinde `IsActive = true` ile filtrelenmiş **kısmi unique index** var — aktif snapshot'ı değiştiren her kod bunu tek `SaveChanges` içinde yapmamalı (bkz. `BootstrapUploadEndpoints.CompleteAsync`).
  - **Chunk `ReceivedAtUtc` = bölümün sürümü (Faz 29, 2026-09-12).** Artımlı yüklemede değişmeyen bölümlerin chunk'ları yeni snapshot'a `ReceivedAtUtc` korunarak taşınır (`MergeIncrementalChunksAsync`), değişen bölüm ve `SnapshotDeleteApplier`'ın yeniden yazdığı bölüm yeni zaman alır. `POST /api/v1/android/bootstrap` bunu `sectionVersions` (bölüm → `MAX(ReceivedAtUtc)`) olarak döner; mobil `SnapshotSync` tablo bazında bununla atlama kararı verir (bkz. Siparis_Cepte KB 00, kural 5). Bu alan yalnızca chunk'lı snapshot yolunda vardır; eski `bootstrap_packages` yolunda dönmez. Chunk'ı `ReceivedAtUtc`'yi güncellemeden yeniden yazan bir kod, o bölümün değişikliğini cihazlardan **gizler**.
  - **Aynı satırlar yeniden gelirse bölüm yeniden yazılmaz, cihaz uyandırılmaz (Faz 30, 2026-09-12).** Ajan Mikro'yu 26 saatlik geriye bakış penceresiyle okuduğu için (`MikroDbReader.CoarseWatermarkLookback`) her 20 sn'lik turda son günün cari hareketlerini yeniden yükler. `MergeIncrementalChunksAsync` artık gelen satırı anahtarına göre saklı satırla `JsonNode.DeepEquals` ile karşılaştırır; hiçbir satır değişmediyse eski chunk'lar taşınır (sürüm oynamaz) ve `CompleteAsync` `hub.Publish` çağırmaz. Bu düzeltmeden önce her ajan turu her cihazda tam bir senkron turu başlatıyordu (canlı gözlem: 14:11, 14:12, 14:14, 14:17 art arda turlar, her birinde 14.303 cari hareketi).
  - **`POST /api/v1/android/sync/cariHareketleri` `since` uygular (Faz 30).** `PagedSectionAsync` daha önce `since`'i yok sayıp bölümün tamamını sayfalıyordu; artık bir zaman damgası gelirse `updatedAt > since` satırları döner ve en yeni `updatedAt`'i `watermark` olarak verir (stokHareket / faturaHareket uçlarıyla aynı sözleşme). Üç hareket ucu da `since`'i **26 saat geriden** uygular (`MovementCursorOverlap`): Mikro'da `UpdatedAt = COALESCE(cha_lastup_date, cha_create_date, cha_tarihi)` belge tarihine (gece yarısı) düşebildiği için aynı gün sonradan eklenen satır imleçle aynı damgayı taşır; katı `>` onu kalıcı olarak kaybederdi (Codex bulgusu, ErpBridge#33). Cihaz id ile üzerine yazdığından tekrar gelen satırlar zararsız. `AndroidPageRequest.Since` bu yüzden `string?` — cihazın gönderdiği imleç her zaman katı ISO 8601 değildir, bağlama hatası tüm isteği 400'e çevirirdi.
  - **Satır bazlı sunucu sürümü `changedAtUtc` (Faz 31, 2026-09-12).** ERP zaman damgası taşımayan bölümler (ör. `inventory`, bir view'dan okunur) için sürümü sunucu verir: `MergeIncrementalChunksAsync` bir bölümü yeniden yazarken **değişen** satıra `changedAtUtc = now` (ms hassasiyet, UTC, `SnapshotRowVersion`), değişmeyen satıra ise önceki sürümünü (varsa damgası, yoksa geldiği chunk'ın `ReceivedAtUtc`'si) yazar. Damgasız satırın sürümü = chunk'ının `ReceivedAtUtc`'si (tam yükleme chunk'ları olduğu gibi saklanır). `SameRow` karşılaştırması damgayı yok sayar. Genel `SectionAsync` (`/sync/stokSeviye`, `/sync/barkodlar`, `/sync/acikSiparisler`…) `since` alırsa yalnızca sürümü daha yeni satırları döner ve bölümün en yeni sürümünü `watermark` olarak verir; `since` yoksa `watermark` = bölümün en yeni chunk zamanı. Mobil `syncStokSeviyeleri` imlecini **yalnızca** bu `watermark`'tan yazar (`sync_state` anahtarı `stokSeviye.serverVersion`); ürün kataloğu görevi ise tüm seviyeleri istediği için `since = null` gönderir. `SnapshotDeleteApplier` bölümü yeniden yazınca damgasız satırlar yeni chunk zamanını alır → bir stok silindiğinde cihaz o bölümü bir kez tam indirir (kabul edilen maliyet).
- `mobile_records` *(Faz 26)*: Mobilin gördüğü her kaydın **güncel hâli** — olay günlüğü değil, `(TenantId, Entity, RecordKey)` başına tek satır. `Entity` = bootstrap bölüm adı (`stocks`, `customers`, `prices`…). Cihaz `UpdatedSeq` sırasına göre sayfalar; **ilk kurulum ile günlük delta aynı sorgudur** (`UpdatedSeq > cursor`). `IsDeleted` tombstone, `PayloadSha256` değişmemiş satırın imleci ilerletmesini engeller, `StockKey`/`CustomerKey` ebeveyn silmesinin indeksli cascade'ini taşır. `SourceRecordKey` (2026-09-12) kartın ERP fiziksel kimliğidir (RECno/Guid, küçük harf); silme olayı iş anahtarına buradan çevrilir. `SourceDatabase` bu kimliği hangi ERP veritabanının ürettiğini taşır — birden fazla veritabanı aynı tenant'a beslendiğinde aynı RECno çakışabileceği için çeviri sorgusu `(TenantId, Entity, SourceDatabase, SourceRecordKey)` ile daraltılır.
- `tenant_sync_counter` *(Faz 26)*: `mobile_records.UpdatedSeq` için tenant başına tahsis sayacı (`LastSeq`, `TombstoneHorizonSeq`).
- `mobile_users` *(Faz 32)*: Mobil uygulama kullanıcıları (`TenantId, Username` küçük harf, `FullName`, `PasswordHash` BCrypt, `Role` ADMIN|MANAGER|SALES (Faz 45'ten beri rol setinden türetilen eski değer), `IsActive`, `DeletedAtUtc`, `LastLoginAtUtc`). UNIQUE `(TenantId, Username) WHERE DeletedAtUtc IS NULL`. **Aktif + silinmemiş satır = bir ücretli koltuk.** `CanApprove`, `CanManageApprovalRules` *(Faz 38)*: yalnız `MANAGER` için anlamlı; admin bu yetkilere rolüyle sahiptir.
- `mobile_user_roles` *(Faz 45)*: kullanıcının rolleri. PK `(UserId, Role)`, FK `mobile_users` (cascade), `Role` ADMIN|MANAGER|ACCOUNTING|WAREHOUSE|SALES, `GrantedAtUtc`, `GrantedByUserId` (firma admini; konsol/migration için null), index `Role`. Her kullanıcıda en az bir satır; izinler bu tablodan (kural 14).
- `mobile_devices` *(Faz 32)*: Tenant'a giriş yapmış telefonlar (`DeviceId` uygulamanın kurulum kimliği, `LastUserId`, `AppVersion`, `IsActive`, `FirstSeenAtUtc`, `LastSeenAtUtc`). UNIQUE `(TenantId, DeviceId)`. Koltuk değildir; destek ve kayıp telefon engelleme içindir.
- `tenant_subscriptions` *(Faz 32)*: Koltuk satın alımları, yalnızca ekleme (`Seats`, `StartsAtUtc`, `EndsAtUtc` null = süresiz, `Source`, `Reference` fatura/dekont, `Note`, `IsCurrent`, `CreatedByAdminId`). UNIQUE `(TenantId) WHERE IsCurrent`. Bitişten sonra 7 gün `grace`, sonra `expired` (giriş kapanır, kullanıcı silinmez).
- `tenants.Code` *(Faz 32)*: Telefonda yazılan 8 karakterlik firma kodu (ilk abonelikte üretilir, yenilemede değişmez). `tenants.SeatLockVersion`: koltuk transaction'larının satır kilidi sayacı (bkz. 00 kural 14).
- `native_stock_levels` *(Faz 33)*: ERP'siz firmada stok miktarı (`TenantId, StockCode, WarehouseNo` PK, `Quantity decimal(18,4)`, eksi olabilir). `LastMovementAtUtc` (Faz 34): son hareketin zamanı; doluysa ürün silinemez. Yalnızca `NativeDocumentProcessor` yazar.
- `native_customer_balances` *(Faz 33)*: ERP'siz firmada cari bakiye (`TenantId, CustomerCode` PK, `Balance decimal(18,2)`, pozitif = cari borçlu). Yalnızca `NativeDocumentProcessor` yazar.
- `approval_requests` *(Faz 38)*: Onay talepleri (`TenantId, ExternalId` UNIQUE, `Kind` 8 türden biri, `CounterpartyName`, `Amount decimal(18,2)`, `SummaryJson` jsonb yalnız gösterim, `DocumentsJson` jsonb onayda işlenecek belgeler, `Status` Pending|Approved|Rejected|Withdrawn|Resubmitted, `ReplacesRequestId`, `RequestedByUserId/Name`, `RequestedAtUtc`, `RequestedSeq` ve `UpdatedSeq` unix ms — SQLite'ta da sıralanabilsin diye, `DecidedByUserId/Name`, `DecidedAtUtc`, `DecisionNote`). Durum değişimi yalnız `ApprovalService.ClaimAsync` ile (bkz. 00 kural 16).
- `approval_request_events` *(Faz 38)*: Talep geçmişi (`RequestId`, `Action` Submitted|Approved|Rejected|Reopened|Withdrawn|Resubmitted, `ByUserId/ByName`, `AtUtc`, `AtSeq`, `Note`).
- `order_fulfillments` *(Faz 47)*: Depoda hazırlanan satış siparişleri, güncel durum (kural 21). `TenantId`, `SourceJobId` FK `jobs` — `(TenantId, SourceJobId)` UNIQUE, `ApprovalRequestId`, `OrderNo(64)`, `CustomerCode(64)`, `CustomerName(200)`, `SalespersonUserId`, `SalespersonName(120)` anlık kopya, `Amount numeric(18,2)`, `LineCount`, `ItemQuantity numeric(18,3)`, `ItemsJson` jsonb, `Status` PENDING|PREPARING|PACKED|LOADED|CANCELLED, `QueuedAtUtc`, `QueuedSeq`, `StartedAtUtc`, `PackedAtUtc`, `LoadedAtUtc`, `AssigneeUserId/Name`, `VehiclePlate(16)`, `ErpState` NONE|PENDING|WRITTEN|FAILED, `UpdatedSeq` (`tenant_sync_counter`'dan, kural 11), `CreatedAtUtc/UpdatedAtUtc`. Index `(TenantId, Status, QueuedSeq)`, `(TenantId, UpdatedSeq)`, `(TenantId, AssigneeUserId, PackedAtUtc)`.
- `order_fulfillment_events` *(Faz 47)*: Değişmez adım günlüğü. `Id` bigint identity, `TenantId`, `FulfillmentId` FK, `FromStatus` (QUEUED'da null), `ToStatus`, `Action(24)` QUEUED|START|PACK|LOAD|UNDO|CANCEL|REASSIGN|ERP_FAILED, `ActorUserId` (sistemde null), `ActorName(120)` ("Sistem"), `DeviceId(128)`, `Note(500)`, `OccurredAtUtc` sunucu saati. Satırlar güncellenmez/silinmez. İndeksler `(TenantId, FulfillmentId, OccurredAtUtc)` ve *(Faz 50)* `(TenantId, OccurredAtUtc)` (rapor aralığı). Depo raporlarının tek kaynağı budur (kural 23).
- `tenant_warehouse_settings` *(Faz 47)*: `TenantId` PK, `Enabled` (varsayılan false), `PendingWarnMinutes` 15, `PendingCriticalMinutes` 30, `PreparingWarnMinutes` 20, `PreparingCriticalMinutes` 45, `PackedWarnMinutes` 60, `UpdatedByUserId`, `UpdatedAtUtc`. Satır yoksa modül kapalı, eşikler varsayılan.
- `display_devices` *(Faz 49)*: Eşleşmiş depo TV'leri (kural 22). `Id`, `TenantId` FK (index), `Name(80)`, `TokenHash char(64)` (eşleştirme gizli anahtarının SHA-256'sı; JWT saklanmaz), `CreatedByUserId`, `CreatedAtUtc`, `LastSeenAtUtc` (dakikada en çok bir yazılır), `RevokedAtUtc`. Koltuk sayılmaz.
- `log_events` *(Log Merkezi L0, 2026-09-17)*: Tüm kaynakların tanılama olayları (telefon `android`, ajan `windows_agent`/`windows_service`, `portal`, `admin`, `central_api`). Tek yazım yolu `LogCenter/LogEventWriter` (normalize → `LogScrubber` maskeleme → sınırla → WARN+ için gruplama). `EventId(64)`, `Source(32)`, `TenantId` boş olabilir, `OccurredAtUtc/Ms`, `ReceivedAtUtc/Ms` (filtre ve sıralama **ms** kolonlarında; SQLite `DateTimeOffset` karşılaştıramaz), `Severity(8)` DEBUG|INFO|WARN|ERROR|FATAL, `Kind(64)` büyük harf ASCII, `Operation(160)`, `Screen(160)`, `Message(2000)`, `ExceptionType(200)`, `StackTrace(8000)`, `AppVersion`, `OsVersion`, `DeviceModel`, `DeviceId` (telefonun rastgele kurulum UUID'si), `UserId`, `AgentId`, `SessionId`, `CorrelationId(128)`, `HttpMethod/Route/Status`, `DurationMs`, `RepeatCount`, `FingerprintId` → `log_error_groups`, `PropertiesJson`/`BreadcrumbsJson` jsonb. UNIQUE `(Source, EventId)` (`TenantId` bilinçli olarak dışarıda: PostgreSQL NULL'ları farklı sayar). PostgreSQL'de `Message` trigram GIN (`pg_trgm` yoksa atlanır). Migration eski `mobile_telemetry_events`'in son 90 gününü bir kez kopyaladı (grupsuz); `LogGroupBackfillWorker` açılıştan 30 sn sonra grupsuz WARN+ satırları 500'lük partilerle gruplar (çözülmüş grubu yeniden açmaz). Yazım relational sağlayıcıda tek transaction'dır (grup oluşturma + sayaç + olaylar); eşzamanlı çakışmada tüm deneme en çok 3 kez yeniden çalışır; **L1c'den beri** iki telemetri ucu (`/mobile/telemetry/batch`, `/agents/telemetry`) **yalnız** `log_events`'e yazar ve `GET /admin/telemetry` de buradan okur (yanıt şekli aynı; tür artık büyük harf, ör. `DESKTOP_EXCEPTION`). Yazım hatası artık 5xx'tir (istemci tekrar dener). Eski `mobile_telemetry_events` yazılmaz; kendi 90 günlük işçisiyle boşalır, sonra ayrı onayla düşürülür. Admin okuma uçları: `GET /api/v1/admin/logs` (filtreler `LogCenter/LogQuery`: `source`, `severity`/`minSeverity`, `tenantId`, `kind`, `operation`, `deviceId`, `userId`, `agentId`, `appVersion`, `correlationId`, `fingerprintId`, `from`/`to`, `q`; en yeni önce, `before` imleci `{OccurredAtMs}~{EventId}~{Source}` — olay kimliği yalnız kaynak içinde tekil olduğu için kaynak da bağı çözer; çelişen seviye filtreleri boş sonuç verir, `take` ≤ 200), `/{id}` (yığın, breadcrumb, özellikler, grup özeti), `/facets` (varsayılan son 24 saat).
- `log_error_groups` *(Log Merkezi L0)*: Aynı sorunun WARN+ olayları. `Fingerprint char(64)` UNIQUE (`LogCenter/ErrorFingerprint`: kaynak + tür + istisna tipi + işlem + rakam/GUID/tırnak temizlenmiş mesaj + ilk uygulama yığın satırı), `TotalCount` (tek `UPDATE` ile artar), `First/LastSeenAtUtc/Ms`, `Severity` (görülen en yüksek), `SampleMessage`, `TopFrame`, `LastAppVersion`, `Status` OPEN|RESOLVED|IGNORED, `ReopenedAtMs` (çözülmüş gruba yeni olay gelince OPEN'a döner). Tarifi değiştirmek tüm grupları böler. *ERP yazım Y5b:* ajanın yazamadığı telefon belgesi `POST /jobs/ack`'te `windows_agent` kaynaklı `ERP_WRITE_FAILED` (ERROR) ya da `ERP_WRITE_RETRY` (WARN) olayıdır; `Operation = erp.write.<belge türü>`, `PropertiesJson` `jobId`, `externalId`, `documentType`, `errorCode`, `attempt`.
- `log_settings` *(Log Merkezi L0)*: Saklama süreleri, tek satır `Id = 1` (yoksa varsayılanlar). `InfoRetentionDays` (14, DEBUG+INFO), `WarnRetentionDays` (90, WARN+ ve bayat açık gruplar), `UpdatedAtUtc`, `UpdatedBy(120)`. Okuyan `LogCenter/LogRetention` değerleri 1–730'a sıkıştırır.
- `display_pairing_codes` *(Faz 49)*: TV'nin gösterdiği kod. `Code char(6)` PK, `PairingSecretHash char(64)`, `ExpiresAtUtc` (+10 dk), `DisplayDeviceId` (yönetici sahiplenince), `ClaimedAtUtc`. TV token'ı alınca satır silinir.
- `tenant_approval_rules` *(Faz 38)*: Tenant başına onay kuralları (`TenantId` PK; `Sale, Purchase, Return, Collection, Disbursement, StockCount, ProductCard, CustomerCard` bool; `UpdatedByName`, `UpdatedAtUtc`). Satır yoksa hepsi açık sayılır.
- `tenants.DataSource` *(Faz 33)*: `erp` | `native`. `tenants.NativeLockVersion`: native belge transaction'larının satır kilidi sayacı (bkz. 00 kural 15).
- `parameter_records`: Müşteri bazlı konfigürasyon parametreleri.
- `erp_write_settings` *(ERP yazım Y1a, 2026-09-17)*: ERP'li firmanın telefon belgelerini ERP'ye nasıl yazacağı, `TenantId` PK (yoksa varsayılanlar). `SalesDocumentKind(16)` order|dispatch|invoice (varsayılan order), `OrderApprovalMode(16)` approved|pending, seriler `OrderSeries/DispatchSeries/InvoiceSeries/ReturnSeries/CollectionSeries` **(6, Mikro genişliği; boş = serisiz)**, varsayılanlar `DefaultWarehouseNo`, `DefaultCashCode(25)`, `DefaultCardBankCode(25)`, `DefaultTransferBankCode(25)`, `DefaultErpUserNo`, `DefaultSalespersonCode(25)`, `DefaultPriceListNo`, portföy kasaları `ChequePortfolioCode` ('ÇEK'), `NotePortfolioCode` ('SENET'), isteğe bağlı `ResponsibilityCenterCode`, `ProjectCode`, `DeliveryDayOffset`; `UpdatedAtUtc/ByUserId`. Ajana iş kiralanırken `erpContext` ile gider (Y1d).
- `mobile_user_erp_mappings` *(ERP yazım Y1a)*: telefon kullanıcısının ERP karşılıkları, `UserId` PK FK `mobile_users` (cascade), `TenantId` (index). Hepsi boş olabilir — boş değer firma ayarına düşer: `SalespersonCode`, `WarehouseNo`, `CashCode`, `CardBankCode`, `TransferBankCode`, `ErpUserNo` ve seri geçersiz kılmaları. `mobile_users` tablosu değişmez.

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
-- WHERE ISNULL(cha_cari_cins, 0) = 0  (yalnız cari tarafı)
```

- **Yalnız `cha_cari_cins = 0` satırları sayılır.** Peşin (kapalı) fatura `cha_cari_cins` 4 kasa / 2 banka ile
  `cha_kod` = kasa/banka kodu yazılır, müşteri `cha_ciro_cari_kodu`'dadır ve `cha_tpoz=1`; müşterinin bakiyesini
  değiştirmez (Mikro cari föyü de göstermez). Okuyucu bu satırlarda `ciroCariKod` + `kapali=true` gönderir,
  `cariKod` geriye uyumluluk için kasa/banka kodu kalır; Portal ekstresi kapalı satırı atlar (2026-09-17, Y0e).
- **İade yönü:** satıştan iade `cha_evrak_tip=0` (alış faturası) + `cha_normal_Iade=1` → `SATIS_IADE`; alıştan iade
  `cha_evrak_tip=63` + iade bayrağı → `ALIS_IADE`. 2026-09-17'ye kadar okuyucu bunları ters sınıflandırıyordu.
  Ayrıntı: [`docs/mikro-yazim-referansi.md`](../docs/mikro-yazim-referansi.md).
- `cha_tip = 0` → borç (satış faturası, borç dekontu). Bakiye artar.
- `cha_tip = 1` → alacak (tahsilat, iade faturası). Bakiye azalır.
- Net Bakiye > 0: Borçlu cari (firmaya borcu var).
- Net Bakiye < 0: Alacaklı cari (avans / firmadan alacağı var).

**Yazma tarafı karşılığı:** `MikroCollectionWriter` tahsilatı `cha_tip = 1`
(alacak) yazar. Yanlış değer her carinin bakiyesini sessizce tersine çevirir.

### Yaşlandırma / Vade

`cha_vade` tek anlamlı değildir: tahsilat makbuzu satırında (nakit, kart, havale, çek, senet) **`yyyymmdd` tarih**
(canlı: `20261130`); Mikro ekranından kesilen faturalarda 0; Fora faturada ödeme planı numarasını yazar. Yaşlandırma
hesabı bu ayrımı gözetmelidir (2026-09-17 canlı veriyle düzeltildi; önceki "gün sayısı" ifadesi hatalıydı).

### Çoklu İskonto Sıralaması (Mikro Native 6 Kademeli)

`sip_iskonto_1` ila `sip_iskonto_6` ardışık olarak bir önceki indirimli tutara
uygulanır:

$$Tutar_1 = Brüt - (Brüt \times \tfrac{Isk_1}{100}), \quad Tutar_2 = Tutar_1 - (Tutar_1 \times \tfrac{Isk_2}{100}), \quad \dots \quad Matrah = Tutar_6$$

Stok hareketinde karşılığı: `sth_iskonto1..6` **tutar** olarak saklanır; uygulama şekli `sth_isk_mas1..10`
(canlıda `isk_mas1=0` brüt üzerinden, `2..10=1` kalan üzerinden). Siparişte tutar `sip_iskonto_1..6`, şekil
`sip_iskonto1..6` (alt çizgisiz).

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
