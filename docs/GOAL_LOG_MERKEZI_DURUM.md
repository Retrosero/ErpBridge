# Goal Durumu — Log Merkezi

Son güncelleme: 2026-09-18 (L4a)
Görev listesi: [GOAL_LOG_MERKEZI.md](GOAL_LOG_MERKEZI.md)

> **Her görevden sonra, o görevin PR'ı içinde güncellenir.** Oturum kapanırsa buradan devam edilir.
> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı (kapı) · ⛔ tıkandı

---

## Özet

| Faz | Görev | Biten | Durum |
|---|---|---|---|
| L0 — Temel: birleşik model + iz kimliği | 6 | 6 | ✅ |
| L1 — Log Merkezi v1 (Admin) | 3 | 3 | ✅ |
| L2 — Sunucu, Portal, Admin logları | 6 | 6 | ✅ |
| L3 — ERP Windows ajanı | 7 | 7 | ✅ |
| L4 — Sipariş Cepte | 8 | 1 | 🔄 |
| L5 — Log Merkezi v2 | 6 | 0 | ⬜ |
| L6 — Denetim, tanılama, saklama | 4 | 0 | ⬜ |
| L7 — Uyarılar | 2 | 0 | ⬜ |
| L8 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** L4b — telefonda iz kimliği (OkHttp interceptor)

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| L0a | Plan dalını main'e al | ✅ | [#67](https://github.com/Retrosero/ErpBridge/pull/67) | Codex 4 bulgu, dördü planda düzeltildi: pg_trgm eklentisi önce ve hata toleranslı; eski tablo için L1c geçişi; boştaki telefona `diagnostics/pending`; tekillik `(Source, EventId)` |
| L0b | `log_events` tablosu + eski telemetri kopyası | ✅ | [#68](https://github.com/Retrosero/ErpBridge/pull/68) | L0b+c+d tek PR. **Codex 5 bulgu, beşi düzeltildi:** boşluklu/tırnaklı gizli değer tam maskelenir; yazım tek transaction + çakışmada tüm deneme yeniden (sayaç satırlardan sapmaz, grupsuz WARN+ kalmaz); önem derecesi UPDATE içinde saklı değerle yükselir; kopyalanan satırları `LogGroupBackfillWorker` gruplar. Ayrıca TCKN/telefon kalıplarının GUID/hex içindeki rakamları maskelediği (kararsız test) bulundu ve düzeltildi. Zamanlar ayrıca Unix ms (SQLite). Kopya ve trigram SQL'i `LogCenterMigrationSql`'de (migration yeniden üretilebilsin). Yerelde Docker yok → PostgreSQL SQL'i CI'da çalışmaz; merge sonrası `/health/schema` ile doğrulanacak |
| L0c | `LogScrubber`, `LogNormalizer`, `ErrorFingerprint`, `log_error_groups`, `LogEventWriter` | ✅ | [#68](https://github.com/Retrosero/ErpBridge/pull/68) | Normalizasyon `LogSeverity` + `LogEventWriter.NormalizeKind`'da (ayrı sınıf gerekmedi). Grup sayacı ilişkisel sağlayıcıda tek `ExecuteUpdate`; in-memory testlerde izlenen varlık |
| L0d | Eski iki uç yeni tabloya yazar | ✅ | [#68](https://github.com/Retrosero/ErpBridge/pull/68) | Telefon: `UserId` + `DeviceId` token'dan (API anahtarında gövdedeki `deviceId`). Yazım hatası yakalanır, yükleme bozulmaz |
| L0e | `CorrelationIdMiddleware` + genel hata yakalayıcı | ✅ | [#69](https://github.com/Retrosero/ErpBridge/pull/69) | `CorrelationId.UseCorrelationId` hattın ilk ara katmanı; `UnhandledExceptionHandler` iç ayrıntısız 500 `INTERNAL_ERROR` + `traceId`, ayrıntıyı yeni DI kapsamında `log_events`'e yazar (logger kategorisi DB logger'ında atlanacak, L2b) |
| L0f | `log_settings` + `LogRetentionWorker` | ✅ | [#69](https://github.com/Retrosero/ErpBridge/pull/69) | Sunucu alış zamanına göre partili silme; bayat **açık** gruplar silinir, çözüldü/yok sayıldı korunur. **Codex 3 bulgu düzeltildi:** konsol satırı maskeli metin (istisna nesnesi değil); grup yalnız hiçbir saklı olay göstermiyorsa bayat; `MaxDeletesPerRun` kesin sınır. #68 canlıda `/health/schema` current (25 migration) |
| L1a | Admin log uçları (liste/detay/facets) | ✅ | [#70](https://github.com/Retrosero/ErpBridge/pull/70) | Filtreler tek yerde (`LogQuery`); **Codex 2 bulgu düzeltildi:** imleç `{OccurredAtMs}~{EventId}~{Source}` (kimlik yalnız kaynak içinde tekil), çelişen seviye filtresi boş sonuç; metin arama PostgreSQL'de ILIKE (Türkçe büyük/küçük harf ASCII dışında eşleşmez — kısıt) |
| L1b | Admin `/logs` sayfası | ✅ | [#70](https://github.com/Retrosero/ErpBridge/pull/70) | Filtreler + açık kayıt URL'de; detay paneli; saatler Türkiye saati. **Görsel kontrol:** giriş formuna parola yazılmadığı için (güvenlik kuralı) sayfa bUnit çıktısı Admin CSS'iyle tarayıcıda açılarak masaüstü ve dar ekranda kontrol edildi; kopyala düğmesi taşması düzeltildi |
| L1c | Geçiş + menü + Dashboard kartı | ✅ | [#70](https://github.com/Retrosero/ErpBridge/pull/70) | İki telemetri ucu yalnız `log_events`'e yazar (hata artık 5xx); `/admin/telemetry` yeni tablodan (tür büyük harf); Dashboard kartı son 24 saat ERROR+ → `/logs?minSeverity=ERROR` |
| L2a | CentralApi log yapılandırması | ✅ | #72 | Tek satır + kapsamlı konsol, EF SQL komutları Warning. **Sapma:** JSON yerine tek satır basit biçim (Coolify konsolunda okunur) |
| L2b | `DatabaseLoggerProvider` | ✅ | #72 | Ortak altyapı yeni `ErpBridge.Diagnostics` projesinde (`BufferedLogProvider`, paket bağımlılığı yok). EF + yakalanmamış istisna kategorileri hariç; Test ortamında kapalı |
| L2c | 5xx + yavaş istek kaydı | ✅ | #72 | Firma/kullanıcı/ajan kapsamı log anında okunur (API anahtarı uçta doğrulanıyor); uzun yoklamalar yavaş sayılmaz |
| L2d | `/internal/logs` + `RemoteLoggerProvider` | ✅ | #72 | Anahtar tanımsızsa uç 404, gönderici kapalı |
| L2e | Portal hata sınırı + loglama | ✅ | #72 | **Codex 4 bulgu düzeltildi:** iç log ucu gövdeyi anahtardan sonra ve en çok 1 MB okur, null olaylar 400; `/Error` kodu (`TraceIdentifier`) loglanan istisnanın iz kimliği (`UseRequestCorrelationScope`); `PortalPageBase.RunAsync` yakalanan API/bağlantı hatalarını sayfa, firma, kullanıcıyla loglar (5xx ERROR, diğerleri WARN). `PortalErrorBoundary` destek kodu = iz kimliği; firma/kullanıcı JWT'den (yalnız log bağlamı); `CorrelationIdHandler` 5xx/ulaşılamayan çağrıyı loglar; `/Error` sayfası (önceden yoktu) yerelde doğrulandı |
| L2f | Admin hata sınırı + loglama | ✅ | #72 | `AdminErrorBoundary`, `/Error`, aynı işleyici ve log gönderimi |
| L3a | Ajan log yapılandırması + sürüm | ✅ | [#71](https://github.com/Retrosero/ErpBridge/pull/71) | `AgentSerilog` (servis + masaüstü ortak, sink'ler kodda), `AgentLogLocation` (EXE yanı → ProgramData → TEMP), `VersionPrefix` 1.1.0. Kullanılmayan maskesiz `CreateLoggerFactory` silindi. **Sapma:** CI derleme numarası yerine elle artırılan `VersionPrefix` (ajan derlemeleri CI'da paketlenmiyor) |
| L3b | Ajan maskeleme düzeltmeleri | ✅ | [#71](https://github.com/Retrosero/ErpBridge/pull/71) | Yazıcıları tek tek düzeltmek yerine çıkışta maskeleme: `MaskingTextFormatter` tüm satırı (istisna dahil) `ConnectionStringMasker.MaskSecrets`'ten geçirir. Test, `MaskPassword`'ün satır sonunu aşıp istisna türünü yuttuğunu yakaladı — düzeltildi |
| L3c | Ajan olay kuyruğu + `/agents/logs/batch` | ✅ | #106 | LocalStore migration 3: `agent_log_outbox` (en çok 1.000 olay / 7 gün) + `agent_log_sent` (kısma belleği). `Core/Logging`: `IAgentLogReporter` (maskeleme, seviye/tür normalizasyonu, parmak izi) ve `AgentLogUploader` (50'lik parti, heartbeat turunda; sunucu almazsa kuyrukta kalır ve **Warning**). Aynı parmak izi 10 dakikada bir gönderilir, arada `repeat_count` artar; kuyrukta bekleyen aynı hata yeni satır açmaz. Sunucu `POST /api/v1/agents/logs/batch` (1–50, `LogEventWriter`, `(Source, EventId)` ile tekrar saklanmaz, `source` gövdeden ama firma/ajan token'dan); eski `/agents/telemetry` duruyor. Parmak izi ajan tarafında sayı/tırnaklı metin temizlenerek hesaplanır (sunucunun grup tarifiyle aynı fikir). Testler: kuyruk sırası/kısma/budama (LocalStore), raporlayıcı maskeleme + yükleyici çevrimdışı/çevrimiçi (Core), uç (CentralApi). **Not:** olayları üretecek çağrı noktaları L3d'de bağlanıyor; şu an yalnız altyapı |
| L3d | Raporlanmayan ajan hatalarının bağlanması | ✅ | #107 | **Sapma (bilinçli):** goal metni ~15 çağrı noktasını tek tek donatmayı söylüyordu; onun yerine `AgentLogCentreSink` Serilog hattına takıldı ve **WARN+ her satır** raporlayıcıya gidiyor. Gerekçe: listedeki yolların tamamı zaten `ILogger` ile uyarı/hata yazıyor, tek tek donatmak hem bu 15'i kapsar hem de **16.'sını unutur**. Kapsanan: senkron döngüsü, bootstrap, change-log, Mikro bağlantı/sürüm, token yenileme, notify, heartbeat, `AgentWorker` + yerel kuyruk + ack, mutabakat alarmları, WPF `Success=false` dalları ve `RunSyncDeltaAsync`. Tür `Kind` özelliğinden ya da sınıf adından (`MikroAdapter` → `MIKRO_ADAPTER`); `Operation`/`CorrelationId` taşınır. Geri besleme yok (raporlayıcı/yükleyici hariç tutulur; `LogCentreHandled` kapsamı ikinci kaydı keser). Açık olaylar: `AGENT_STARTED` (sürüm/ERP türü/ERP veritabanı) + `AGENT_STOPPING` (servis `AgentLifecycleWorker`, masaüstü `App`), `AppDomain` → FATAL, `TaskScheduler` → ERROR (süreç ölmeden kuyruğa yazılması beklenir). Polly yeniden denemeleri artık WARN yazıyor (önceden `onRetry` boş bir yer tutucuydu). **Düzeltilen boşluk:** WPF `PushSectionAsync` hatası yalnız rozeti kırmızıya çeviriyordu, hiçbir yere yazmıyordu. Masaüstü kuyruğu heartbeat turunda ve kapanışta boşalır. Testler: `AgentLogCentreSinkTests` (seviye eşiği, tür türetme, `Kind`/`Operation`/`CorrelationId`, geri besleme yok, raporlayıcı yokken yerel log), `AgentLifecycleWorkerTests` (başlangıç/durma olayları, config okunamazsa da başlangıç olayı) |
| L3e | `AGENT_SYNC_ROUND` | ✅ | #108 | `Core/Logging/AgentSyncRound`: INFO olay, `trigger` (`timer`/`manual`), `mode` (`changelog`/`snapshot`/`section`), `success`, `durationMs`, `rows`, hareket eden bölüm başına `rows.<bölüm>`, `payloadBytes`, `errorCode`. **Yalnız sayı** — iş verisi yok. Zamanlayıcı turları `AgentSyncLoop`'tan (servis + WPF aynı döngüyü kullanır), operatör düğmeleri `DashboardViewModel`'den (senkronize, sıfırdan kur, elle bootstrap, bölüm gönder). Kuyruğu doldurmaması parmak izine bırakıldı: mesajdaki sayılar temizlendiği için başarılı turlar tek satırda toplanır, başarısız tur/farklı `errorCode` kendi satırını açar. Raporlayıcısı olmayan host sessizce çalışır (testler bare provider ile kurar). Testler: `AgentSyncRoundTests` (bölüm sayıları, boş bölüm yazılmaz, değişiklik günlüğü turu, başarısız tur, parmak izi toplanması, döngüden iki turun bildirilmesi) |
| L3f | Heartbeat zenginleştirme + geçmiş | ✅ | #109 | **Kök sorun:** `HeartbeatWorker.RecordSuccessfulSync`/`RecordError` hiç çağrılmayan boş dikişlerdi; her heartbeat "son senkron = şimdi" diyordu, masaüstü ise hep `null` gönderiyordu. Çözüm: `Core/Sync/AgentRunStatus` — süreç başına tek durum, senkron döngüsü ve iş kuyruğu yazar, iki heartbeat de (servis + WPF) okur. Gövdeye `appVersion`, `hostKind` (`service`/`ui`), `erpKind`, `erpVersion`, `lastSyncResult`, `lastErrorCode` eklendi; **hepsi isteğe bağlı**, gönderilmeyen alan sunucudaki değeri ezmiyor (eski ajan gövdesi aynen kabul ediliyor, testli). ERP sürümü adaptörden 6 saatte bir sorulur. Sunucu: `agents`'a 8 nullable kolon + yeni `agent_heartbeat_log` tablosu (migration `20260917201420_LogMerkeziL3fHeartbeat`, yalnız ekleme); `lastError` ajanda maskelenir, sunucuda bir kez daha maskelenip **saklanır** (önceden atılıyordu). Geçmiş satırı yalnız imza değişiminde (durum, kuyruk derinliği, sonuç, hata, sürümler) ya da son satır 15 dakikadan eskiyse yazılır. Testler: `AgentHeartbeatEnrichmentTests` (zengin gövde + maskeleme, eski gövde, geçmiş seyreltme), `AgentRunStatusTests` (yalnız başarılı tur saati ilerletir, maskeleme, sürüm tazeliği, döngünün durumu yazması). **Panelde gösterim L4–L8'e ait.** İnceleme (Codex) üzerine 5 düzeltme: (1) `lastSyncResult=ok` gelince sunucu eski hatayı **temizler** (yoksa iyileşen ajan pazartesinin hatasını göstermeye devam ediyordu), (2) başarısız ERP sürüm sondası da denemeyi kaydeder ve 15 dk sonra yeniden dener (erişilemeyen ERP her dakika sorgulanmasın), (3) `ClearError` artık sürüm numaralı — heartbeat uçarken kaydedilen hata silinmiyor, (4) ERP sürüm sondası `Core/Sync/ErpVersionProbe`'a taşındı, masaüstü host da kullanıyor, (5) `AgentWorker` reddedilen belgeyi, yerel kuyruk ve yoklama hatasını `AgentRunStatus`'a yazıyor |
| L3g | `jobs.CorrelationId` + ajan iz kimliği | ✅ | #110 | `jobs.correlation_id` (nullable, migration `20260917…L3gJobCorrelationId`) ingest isteğinin `X-Correlation-Id` başlığından; `GET /jobs/pending` yanıtında `correlationId`; `RemoteJob.CorrelationId`. Ajan işi `Core/Logging/AgentCorrelation` kapsamında işler — **AsyncLocal**, yani o sırada yazılan her log satırı (raporlayıcı varsayılanı) ve yapılan her HTTP isteği (`HttpRemoteApiClient` her isteğe başlık ekler) aynı kimliği taşır; iş dışındaki çağrı taze kimlik üretir. Ack'te yazılan `ERP_WRITE_FAILED`/`ERP_WRITE_RETRY` **işin** kimliğini kullanır, ack isteğininkini değil. Eski sunucu/eski iş → alan boş, ajan kendi kimliğini üretir. Testler: uçtan uca (ingest başlığı → iş → kiralama yanıtı → ack olayı aynı kimlik), kimliksiz çağrı da kimlik alır, ajan istek başlığı ve kapsam davranışı (RemoteApi), raporlayıcı devralması (Core) |. İnceleme (Codex) üzerine 2 düzeltme: (1) onay bekleyen belge — `approval_requests.correlation_id` eklendi (ayrı migration), onaydan sonra oluşan iş telefonun izini koruyor (önceden onaydan geçen belge yeni iz başlatıyordu), (2) iş dışındaki ajan çağrıları (heartbeat, yoklama, senkron turu) artık kendi kapsamını açıyor; önceden başlık üretiliyor ama log satırı kimliksiz kalıyordu |
| L4a | Telefon bağlam alanları + gizlilik belgeleri | ✅ | siparis_cepte#76 | `util/TelemetryContext`: `sessionId` (açılış başına UUID), `deviceId` (lisanslamanın kurulum UUID'si), `userId`, `networkType` (`wifi`/`mobile`/`ethernet`/`none`), `sdkInt`, `workspaceMode`. Room 38→39 (varsayılanlı ekleme; kuyruktaki olaylar korunur). Bağlam **olay üretilirken** okunur, yüklenirken değil. Gövde: `deviceId`/`sessionId` mevcut alanlar, diğer üçü `properties` içinde; **`userId` gönderilmez** — sunucu jetondan yazıyor. `CrashStore` bağlamı dosyaya yazıp geri okuyor (çökme bir sonraki açılışta tabloya giriyor; o an oturum/ağ değişmiş olur). Okunamayan depo süreç başına bir kez denenir: bu depolar kendi hatalarını raporladığı için her olayda yeniden denemek bozuk keystore'da olayları ikiye katlıyordu. Sürüm 1.5.238. Belgeler: `VERI_ENVANTERI`, gizlilik politikası, KVKK aydınlatma, Play Data Safety (yeni veri türü doğmuyor). Testler: migration + bağlam + çökme bağlamı; telefon paketi 301 yeşil |
| L4b | Telefon iz kimliği | ⬜ | | |
| L4c | Telemetri yükleme işi düzeltmeleri | ⬜ | | |
| L4d | Telefon kapsama boşlukları | ⬜ | | |
| L4e | ANR / çıkış nedenleri | ⬜ | | |
| L4f | Breadcrumb | ⬜ | | |
| L4g | Yerel log + "Tanılama gönder" | ⬜ | | L6c'ye bağlı |
| L4h | Sürüm + Play internal | ⬜ | | |
| L5a | Hata grupları | ⬜ | | |
| L5b | İz zaman çizelgesi | ⬜ | | |
| L5c | Grafik + özet | ⬜ | | |
| L5d | Canlı akış | ⬜ | | |
| L5e | Cihaz + ajan sayfaları | ⬜ | | |
| L5f | CSV dışa aktarma | ⬜ | | |
| L6a | `audit_events` + yazım noktaları | ⬜ | | |
| L6b | Denetim sayfası | ⬜ | | |
| L6c | Uzaktan tanılama | ⬜ | | |
| L6d | Saklama ayarları sayfası | ⬜ | | |
| L7a | Uyarı kuralları + değerlendirme işi | ⬜ | | |
| L7b | Uyarı rozeti + `/alerts` | ⬜ | | |
| L8a | KB güncellemeleri | ⬜ | | |
| L8b | Canlı duman testi | ⬜ | | |
| L8c | Seni Bekleyenler son hâli | ⬜ | | |

---

## Seni Bekleyenler
- **Coolify:** `Logs__InternalIngestKey` (en az 32 karakter rastgele) centralapi, admin **ve** portal servislerine aynı değerle eklenmeli; eklenene kadar Portal/Admin logları yalnız konsolda kalır (sunucunun kendi logları anahtarsız da Log Merkezi'ne yazılır).
- **K1:** Admin konsolu ortak oturum (`TokenStore` tekil) açığı — test aşaması bittiğinde ayrı iş olarak kapatılacak.
