# Goal — Log Merkezi (Sipariş Cepte · ERP Windows Ajanı · Portal · Sunucu)

Tarih: 2026-09-17 · Kapsam: ErpBridge.CentralApi + ErpBridge.Admin + ErpBridge.Portal +
ErpBridge.Agent.Service/UI (+ Core, RemoteApi, LocalStore) + Sipariş Cepte (Android)

İlerleme **[GOAL_LOG_MERKEZI_DURUM.md](GOAL_LOG_MERKEZI_DURUM.md)** dosyasına yazılır; her görevden
sonra, o görevin PR'ı içinde güncellenir. Oturum kapanırsa oradan devam edilir.

---

## 0. İstek, bulgular ve kararlar

### İstek (kullanıcı, 2026-09-17)
Sipariş Cepte, ERP Windows uygulaması ve panel uygulamasının log kayıtları **Admin konsolundan gelişmiş
şekilde** takip edilebilsin. Tüm sistem incelensin, eksikler tespit edilip geliştirilsin; iş ayrı dalda yapılsın.

### Bugünkü durum (inceleme, 2026-09-17 — kod okunarak doğrulandı)

**Olan:** telefon (`POST /api/v1/mobile/telemetry/batch`) ve masaüstü ajanı (`POST /api/v1/agents/telemetry`)
aynı `mobile_telemetry_events` tablosuna yazıyor (90 gün, sabit). Admin `/telemetry` sayfası son 500 satırı
firma/seviye/tür filtresiyle gösteriyor. `change_set_audit_log` (365 gün) ve `/sync-history` sayfası var.

**Eksikler:**

| Alan | Eksik |
|---|---|
| Sunucu (CentralApi) | Varsayılan konsol logu; loglar yalnız Coolify stdout'ta. Genel hata yakalayıcı, istek/iz kimliği, yavaş istek kaydı yok. `LogLevel` bölümü yok → EF'in her SQL komutu Information olarak basılıyor. appsettings'teki Serilog biçimli `Logging` bloğu ajandan kopya ve etkisiz |
| Portal | Beklenmeyen hata devreyi düşürüyor (`ErrorBoundary` yok, `/Error` sayfası yok). Yakalanan hatalar hiçbir yere yazılmıyor. Telemetri ucu portal oturumunu reddediyor |
| Admin | Aynı: `/Error` sayfası yok, hata sınırı yok |
| Windows ajanı | **Servisin log dosyası büyük olasılıkla hiç yazılmıyor**: `ReadFrom.Configuration` `"Serilog"` bölümü arıyor, ayarlar `"Logging"` altında. WPF minimum seviyesi Information (tüm `LogDebug` kayıp). Sunucuya yalnız WPF'in beklenmeyen istisnaları gidiyor; periyodik senkron, bootstrap, change-log, Mikro bağlantısı, token yenileme, notify, heartbeat hataları, servis döngüsü ve mutabakat alarmları **yalnız yerelde**. Heartbeat'in `LastError`/`LastSyncAtUtc`'si sunucuda atılıyor; servis bunları hiç doldurmuyor. Sürüm hep `1.0.0.0` (hiçbir projede `<Version>` yok). Maskeleme sızıntıları: writer'lar `LogError(ex, …)` ile ham istisnayı basıyor, genel `catch` dalları ve `MikroDbReader.cs:673,694` maskesiz. Polly yeniden denemeleri loglanmıyor. Sunucudan ajana komut kanalı yok |
| Sipariş Cepte | Olayda kullanıcı, cihaz, oturum, iz kimliği yok (`correlationId` alanı var, hiç doldurulmuyor). ~115 `catch` / ~80 `runCatching` bloğunun çoğu raporlamıyor (yazıcı sonuçları, giriş/çıkış, `ApprovalCheckWorker`, ön plan servisi, `resyncRequired`, depo kamerası, barkod, EOD dışa aktarma). ANR yakalanmıyor. Breadcrumb neredeyse hiç eklenmiyor. Yükleme işi `REPLACE` ile her olayda çalışan yüklemeyi iptal ediyor; sunucunun 400 ile reddettiği paket kuyruğu 7 gün kilitliyor. `local_data_persist_skipped` önem derecesi `"WARNING"` (standart dışı). Bazı mesajlar belge kimliği taşıyor (veri envanteri ruhuna aykırı). Yerel log dosyası / dışa aktarma yok |
| Admin konsolu | Tek kaynaklı liste, 500 satır sınırı, sayfalama/tarih/metin arama/cihaz/sürüm filtresi yok; hata gruplama, grafik, canlı akış, uyarı, dışa aktarma, iz kimliği ile uçtan uca görünüm, heartbeat geçmişi, denetim kaydı yok. `lastStatus` gösterilmiyor |
| Denetim | Admin/portal/telefon giriş denemeleri (başarılı/başarısız), operatör işlemleri (lisans, API anahtarı, firma, cihaz engelleme, iş yeniden deneme), portal kullanıcı yönetimi kaydedilmiyor. `api_key_secret_access_audits` yazılıyor ama okunmuyor |

### Kullanıcı kararları (2026-09-17)
| # | Karar |
|---|---|
| K1 | **Admin konsolunun ortak giriş anahtarı** (`TokenStore` tekil, tüm tarayıcılar aynı oturumu paylaşıyor) **şimdi kapatılmaz**; geliştirme/test aşamasındayız, sonra ayrıca kapatılacak. Bu goal'de ele alınmaz → "Seni Bekleyenler" |
| K2 | ERP'li müşteride **masaüstü uygulaması ve Windows Servisi birlikte kurulu**. İkisi de kapsamda; servisin log yapılandırması öncelikli |
| K3 | Telefon olaylarına **kullanıcı kimliği + rastgele cihaz UUID'si** eklenir (donanım kimliği değil). `docs/VERI_ENVANTERI.md`, gizlilik metinleri ve `docs/play-data-safety.md` **aynı PR'da** güncellenir |
| K4 | Uyarılar **yalnız Admin panelinde** (rozet + liste). E-posta/Telegram yok |

### Varsayılan kararlar (goal'ün seçimleri — itiraz edilirse değişir)
| # | Karar | Gerekçe |
|---|---|---|
| D1 | Loglar **mevcut PostgreSQL'de** tutulur; Seq/Loki/Grafana gibi yeni altyapı kurulmaz | Tek arayüz Admin olsun; Coolify'a yeni servis ve bakım yükü eklenmesin |
| D2 | Tek tablo: **`log_events`** (kaynak bağımsız). Eski `mobile_telemetry_events` **değiştirilmez/silinmez**; migration son 90 günü bir kez kopyalar, iki eski uç yeni tabloya yazar. Eski tablo kendi 90 günlük temizliğiyle boşalır; düşürülmesi ayrı insan onayı ister | Veri kaybı yok, eski uç sözleşmesi bozulmaz |
| D3 | Kaynak değerleri: `android`, `windows_agent`, `windows_service`, `portal`, `admin`, `central_api`. Seviye sunucuda normalize edilir: `DEBUG/INFO/WARN/ERROR/FATAL` (`WARNING`→`WARN`); tür büyük harf, ASCII (Türkçe `İ` tuzağı) | Filtreler güvenilir olsun |
| D4 | **İz kimliği `X-Correlation-Id`**: istemci üretir, yoksa sunucu üretir; yanıtta geri döner, sunucu log kapsamına girer, olaylara yazılır. Telefonun gönderdiği belge işi `jobs.CorrelationId`'ye yazılır, ajan işi çekerken alır ve yazım hatası olayına ekler → telefon → sunucu → ajan tek zaman çizelgesi | "Plasiyerin faturası neden Mikro'ya düşmedi" tek tıkla görünsün |
| D5 | Sunucu logları: `Warning` ve üstü + 5xx istekler + yavaş istekler (varsayılan > 3 sn) `log_events`'e **sınırlı kanal + toplu yazım** ile gider; kanal dolarsa düşer ve düşen sayısı bir sonraki yazımda tek olay olarak raporlanır. Veritabanı yazımı hatası tekrar loga yazmaz (sonsuz döngü koruması) | Log yazımı isteği yavaşlatmasın, DB sorununda çığ olmasın |
| D6 | Portal ve Admin ayrı konteyner, DB'ye erişmez → logları `POST /api/v1/internal/logs` ile gönderir; kimlik **`Logs:InternalIngestKey`** (ortam değişkeni, sabit zamanlı karşılaştırma). Anahtar yoksa gönderici kapalı, loglar stdout'ta kalır, uygulama çalışmaya devam eder | Coolify'da anahtar tanımlamak insan kapısı; plan onu beklemeden ilerler |
| D7 | **Hata grubu (fingerprint)** sunucuda hesaplanır: kaynak + tür + istisna tipi + işlem + normalize mesaj (rakam, GUID, hex, tırnak içi değerler atılır) + ilk uygulama yığın satırı → SHA-256. `log_error_groups`: sayı, ilk/son görülme, etkilenen firma/cihaz sayısı, durum `OPEN/RESOLVED/IGNORED`. Çözüldü denen grup tekrar gelirse `OPEN`a döner ve uyarı üretir | Tekrar eden binlerce satır yerine "kaç farklı sorun var" |
| D8 | Saklama: `log_settings` (tek satır) — INFO/DEBUG **14 gün**, WARN+ **90 gün**, denetim **365 gün**, tanılama paketi **14 gün**; panelden değiştirilir (en az 1, en çok 730 gün). Temizlik günlük, parti başına en çok 100.000 satır | Hacim kontrolü + ihtiyaca göre uzatma |
| D9 | Mevcut tablolara **yalnız nullable kolon eklemek** serbest (`jobs.CorrelationId`, `agents` sürüm/son hata alanları). Kolon silme, tip değiştirme, tablo düşürme yok. Mevcut uçların varsayılan davranışı değişmez; yeni alanlar isteğe bağlı | Eski telefon/ajan sürümleri bozulmasın |
| D10 | Ajan olayları çevrimdışıyken **LocalStore (SQLite) kuyruğunda** bekler (en çok 1.000 olay / 7 gün); aynı parmak izli hata 10 dakikada bir kez gönderilir, arada tekrar sayısı biriktirilir | Müşteri ağı kesikken hata kaybolmasın, sunucu dövülmesin |
| D11 | Heartbeat geçmişi: `agent_heartbeat_log` — yalnız **durum değiştiğinde** veya **15 dakikada bir** satır; çalışma süresi / kopukluk aralıkları buradan hesaplanır | Her 30 sn'lik heartbeat'i saklamak gereksiz hacim |
| D12 | Uzaktan tanılama: Admin "tanılama iste" → `diagnostic_requests`. Ajan bunu **heartbeat yanıt gövdesinden**, telefon **telemetri batch yanıtından** öğrenir (yeni polling yok). Ajan son log dosyasının maskelenmiş kuyruğunu (en çok 2 MB), telefon yerel halka tamponunu (en çok 500 satır) yükler → `diagnostic_bundles` | Kanal eklemeden mevcut trafiği kullanır |
| D13 | Telefon yerel log: `util/AppLog` sarmalayıcı (`android.util.Log` yerine) → bellek + dosya halka tamponu (en çok 1 MB, maskeli). Yalnız tanılama isteğinde veya kullanıcı "Tanılama gönder" dediğinde yüklenir | Uzaktan tanılamanın telefon tarafı; envanter + Play föyü güncellenir (K3 ile aynı PR) |
| D14 | Admin canlı akış: 5 sn'lik yoklama (Blazor Server zamanlayıcı), sekme görünmezken durur. Admin hız sınırı (60 istek/dk) aşılmasın diye canlı akış tek uç çağırır | SignalR/stream eklemeye gerek yok |
| D15 | Metin arama: PostgreSQL'de `pg_trgm` GIN indeksi ile `ILIKE` (migration yalnız Npgsql'de indeks oluşturur); SQLite testlerinde `Contains` | Tam metin motoru eklemeden hızlı arama |
| D16 | Uyarı kuralları (K4): ajan çevrimdışı > N dk (varsayılan 15), senkron durdu > N dk, hata patlaması (bir grupta 10 dk'da ≥ N, varsayılan 20), yeni hata grubu (ERROR/FATAL), çözülmüş grubun geri gelmesi, telemetri yüklemesi olmayan aktif cihaz (24 saat). Değerlendirme dakikada bir; aynı uyarı açıkken tekrar açılmaz | Panel içi bildirim, gürültüsüz |

---

## 1. Yetkiler

Kullanıcı 2026-09-17'de onayladı ("Evet, hepsi geçerli"):

| Yetki | Karar |
|---|---|
| Dala push, PR açma | Serbest |
| CI yeşil + inceleme yorumları çözülmüşken `main`'e squash-merge + dalı silme | Serbest (ErpBridge'de `main` → Coolify otomatik canlı dağıtım) |
| Play **internal** kanalına sürüm yükleme | Serbest |
| İnsan gerektiren madde | Yapılabilen kısım yapılır, kalanı DURUM > "Seni Bekleyenler" |

**Asla:** `--force` push, CI kırmızıyken merge, `main`'e doğrudan push, Play production yayını, tablo/kolon
silme veya tip değiştiren migration, `mobile_telemetry_events`'i düşürme, başka oturumun commit edilmemiş
değişikliğine dokunma, `git stash pop`, Admin `TokenStore` değişikliği (K1).

---

## 2. Çalışma kuralları

### Çalışma kopyaları
- **ErpBridge işleri yalnız `2026/eb-log`** worktree'sinde. Ana klasör (`2026/ErpBridge`), `eb-panel`, `eb-faz31`
  başka işlere ait; oralarda `switch/checkout/stash/reset` yapılmaz.
- **Sipariş Cepte işleri yalnız `2026/sc-log`** worktree'sinde
  (yoksa: `git -C Siparis_Cepte fetch && git -C Siparis_Cepte worktree add ../sc-log -b <dal> origin/main`).
  Ana klasörde açık `fix/z-raporu-sahte-bilgiler` dalı var; ona dokunulmaz.
- Room migration numarası dal açılırken `origin/main`'deki `AppDatabase` sürümünden alınır (2026-09-17: **36**);
  merge öncesi yeniden kontrol edilir.
- EF migration eklenmeden önce `origin/main`'deki son migration'a bakılır (2026-09-17: `Faz50FulfillmentEventReportIndex`);
  paralel dalda migration geldiyse model snapshot çakışması yeniden üretilerek çözülür.

### Her görev
1. `git -C <worktree> fetch origin && git -C <worktree> switch -c log-<id>-<kısa-ad> origin/main`
2. **Önce bilgi bankası** (`ErpBridge_knowledge_base/INDEX.md` → 00–04; mobil işte `Siparis_Cepte_knowledge_base/`).
   Şema, JSON alan adı, kolon tahmin edilmez — koddan ya da testten doğrulanır.
3. Kod + test. Davranış değişen her görevin testi olur (sunucu: xUnit + ilişkisel SQLite; Admin/Portal: bUnit;
   ajan: `Agent.Service.Tests`, `RemoteApi.Tests`, `Core.Tests`; Android: birim test).
4. Yerel doğrulama yeşil:
   - .NET: `dotnet build ErpBridge.sln -c Debug` (0 uyarı/0 hata) + ilgili `dotnet test`
   - Android: `./gradlew :app:compileDebugKotlin :app:testDebugUnitTest`
   - Admin/Portal sayfası değiştiyse yerel CentralApi + Admin ile tarayıcıda kontrol
5. Türkçe commit: ne + neden; sonuna `Co-Authored-By: Claude Opus 5 <noreply@anthropic.com>`.
6. Push → PR (gövde sonunda `🤖 Generated with [Claude Code](https://claude.com/claude-code)`) → CI.
   3 denemede yeşillenmezse ⛔, bağımlılar da ⛔.
7. İnceleme yorumları merge'ü bloklar: geçerli bulgu düzeltilir, geçersizse gerekçeyle yanıtlanır.
8. Merge öncesi `origin/main` dala alınır (push edilmiş dalda `git merge`, force-push yok), yerelde build + test.
   `GOAL_LOG_MERKEZI_DURUM.md` ve gerekiyorsa KB aynı dala commit edilir.
9. `gh pr merge <n> --squash --delete-branch`; migration içeren görevden sonra
   `https://lisans.appsgo.cloud/health/schema` → `current` doğrulanır, değilse ⛔ + "Seni Bekleyenler".

### Bozulmaz kurallar
- **Log yazımı iş akışını asla bozmaz:** gönderici/sink istisna fırlatmaz, isteği bekletmez, DB hatasında sessizce
  sayaç tutar. Her yeni log yolu için "log hedefi çökükken iş devam ediyor" testi yazılır.
- **Gizlilik:** parola, bağlantı cümlesi, API anahtarı, JWT, `AK-` lisans anahtarı, e-posta, telefon, TCKN/VKN,
  cari/belge değerleri log mesajına girmez. Maskeleme sunucuda da ikinci kez uygulanır (`LogScrubber`, tek sınıf).
  Telefon tarafında yeni alan → `VERI_ENVANTERI.md` + `play-data-safety.md` aynı PR (Android KB kural 23).
- Tüm yeni Admin uçları: `AdminPolicy` + `PerAdminRateLimitPolicy`; tenant **sorgudan** (`?tenantId=`), token'dan
  değil (ErpBridge KB kural 10).
- Kod ERP'den bağımsız kalır; Mikro'ya özgü bilgi yalnız adaptörde.
- `TreatWarningsAsErrors=true` → 0 uyarı.

---

## 3. Fazlar ve görevler

Bağımlılık: **L0 → L1** (panel erkenden yeni tabloyu gösterir) → **L2, L3, L4 paralel yapılabilir** →
**L5** (gelişmiş panel, L2–L4 verisini kullanır) → **L6** → **L7** → **L8**.

### L0 — Temel: birleşik log modeli ve iz kimliği (ErpBridge)
| ID | Görev | Kabul ölçütü |
|---|---|---|
| L0a | Plan dalını main'e al (bu belge + DURUM) | PR birleşti |
| L0b | `log_events` tablosu + EF entity + migration: `Id, EventId (uniq/kaynak+tenant), Source, TenantId?, OccurredAtUtc, ReceivedAtUtc, Severity, Kind, Operation, Screen, Message(2000), ExceptionType, StackTrace(8000), AppVersion, OsVersion, DeviceModel, DeviceId?, UserId?, AgentId?, SessionId?, CorrelationId?, HttpMethod/Route/Status, DurationMs?, RepeatCount (varsayılan 1), FingerprintId?, PropertiesJson (jsonb, 4000), BreadcrumbsJson`. İndeksler: `(OccurredAtUtc)`, `(TenantId, OccurredAtUtc)`, `(Source, Severity, OccurredAtUtc)`, `(CorrelationId)`, `(FingerprintId, OccurredAtUtc)`, `(DeviceId)`, `(UserId)`; Npgsql'de `Message` için trigram GIN (D15). Migration son 90 günün `mobile_telemetry_events` satırlarını kopyalar (D2) | Migration smoke testi; kopyalanan satır sayısı testi; eski tablo dokunulmamış |
| L0c | `LogScrubber` (sunucu maskeleme, tek sınıf) + `LogNormalizer` (D3) + `ErrorFingerprint` (D7) + `log_error_groups` tablosu ve `LogEventWriter` (tek yazım yolu: normalize → maskele → grupla → toplu insert, idempotent `EventId`) | Birim testler: maskeleme listesi, `WARNING`→`WARN`, Türkçe `i`, aynı hatanın farklı rakamlarla aynı gruba düşmesi, çift `EventId` yok sayılır |
| L0d | Mevcut iki uç (`/mobile/telemetry/batch`, `/agents/telemetry`) `LogEventWriter` üzerinden **yeni tabloya da** yazar; sözleşme değişmez. Telefon için `userId`/`deviceId`/`sessionId` token'dan ve isteğe bağlı yeni alanlardan doldurulur (token'daki kullanıcı istemcinin gönderdiğinden önceliklidir) | Eski istek gövdesiyle mevcut testler yeşil; yeni alanlı gövde testi |
| L0e | `CorrelationIdMiddleware` (D4): `X-Correlation-Id` oku/üret (en çok 128, güvenli karakterler), yanıta yaz, `ILogger` kapsamına ekle, `HttpContext.Items`. Genel hata yakalayıcı: beklenmeyen istisna → `ProblemDetails` (`correlationId` dahil, iç ayrıntı sızmaz) + ERROR olayı | Başlıklı/başlıksız istek testi; 500 yanıtında iz kimliği ve yığın sızmaması |
| L0f | `log_settings` (D8) + `LogRetentionWorker` (kaynak/seviye bazlı, partili) — eski `MobileTelemetryRetentionWorker` eski tabloyu temizlemeye devam eder | Saklama testi (INFO 14 / WARN 90) |

### L1 — Log Merkezi v1 (Admin)
| ID | Görev | Kabul ölçütü |
|---|---|---|
| L1a | `GET /api/v1/admin/logs`: filtreler `tenantId, source[], severity[] (en az seviye), kind, operation, deviceId, userId, agentId, appVersion, correlationId, fingerprintId, from, to, q (metin)`; **imleçli sayfalama** (`OccurredAtUtc, Id` azalan, `before` imleci), `take ≤ 200`; `GET /admin/logs/{id}` (breadcrumb + properties dahil); `GET /admin/logs/facets` (seçili aralıkta kaynak/seviye/tür/sürüm sayıları) | Uç testleri: her filtre, imleç tekrar/atlama yok, tenant zorunlu olmayan global liste, anonim 401 |
| L1b | Admin `/logs` sayfası: filtre çubuğu (kaynak çipleri, seviye, firma, tarih hızlı seçim 1s/24s/7g/özel, metin), tablo (zaman, kaynak rozeti, seviye, firma, kullanıcı/cihaz, tür/işlem, mesaj özeti), "Daha fazla yükle", satırdan sağ çekmece detay (mesaj, yığın + kopyala, breadcrumb zaman çizelgesi, HTTP satırı, properties, iz kimliğine/gruba/cihaza tıklayınca filtre). Durum URL sorgusunda | bUnit: filtre → URL, çekmece, kopyala; yerel tarayıcı kontrolü |
| L1c | Mevcut `/telemetry` sayfası korunur ama menüde "Log Merkezi" öne alınır; Dashboard'daki "Mobil hatalar" kartı son 24 saatin ERROR+ sayısını yeni uçtan okur ve `/logs`'a bağlanır | bUnit |

### L2 — Sunucu, Portal ve Admin logları (ErpBridge)
| ID | Görev | Kabul ölçütü |
|---|---|---|
| L2a | CentralApi log yapılandırması: etkisiz `Logging` bloğu temizlenir, `LogLevel` (EF komutları `Warning`, `Microsoft.AspNetCore` `Warning`), JSON konsol biçimi (Coolify'da okunur), kapsam dahil | appsettings testleri / başlangıç testi |
| L2b | `DatabaseLoggerProvider` (D5): `Warning+` → sınırlı `Channel` → `LogEventWriter` toplu yazım, kendi hatasını loglamaz, düşen sayısını raporlar; kaynak `central_api`, kategori `Operation`'a, `correlationId`/`tenantId` kapsamdan | Testler: DB çökükken istek başarılı; kanal dolunca düşme sayacı; döngü yok |
| L2c | İstek sonuç kaydı: 5xx → ERROR, süre > `Logs:SlowRequestMs` (3000) → WARN (`HttpRoute` şablon, sorgu dizesi yok, `DurationMs`) | Test |
| L2d | `POST /api/v1/internal/logs` (D6) + paylaşılan `RemoteLoggerProvider` (Portal/Admin için; sınırlı kuyruk, toplu gönderim, anahtar yoksa kapalı) | Yanlış/boş anahtar 401; gönderici hedef çökükken uygulama çalışır |
| L2e | Portal: `MainLayout`'ta `ErrorBoundary` (Türkçe mesaj + "Destek kodu: <correlationId>" + yeniden dene), `/Error` sayfası, `PortalApiClient` her isteğe `X-Correlation-Id`, `PortalPageBase.RunAsync` yakalanan hataları WARN/ERROR loglar (kullanıcı, firma, sayfa), `ReadFromJsonAsync` ayrıştırma hataları yakalanır | bUnit: sınır içinde hata sayfayı düşürmez; loglama çağrısı |
| L2f | Admin: aynı `ErrorBoundary` + `/Error` + `CentralApiClient`'a iz kimliği + `RemoteLoggerProvider` | bUnit |

### L3 — ERP Windows ajanı (Servis + Masaüstü)
| ID | Görev | Kabul ölçütü |
|---|---|---|
| L3a | **Log yapılandırması düzeltmesi:** Servis ve UI için tek `AgentLogging.Configure` (Core/Agent ortak): gerçek `"Serilog"` bölümü, servis dosyası `logs/agent-.log` (14 gün), UI `logs/ui-.log`; yazılamayan klasörde `%ProgramData%\ErpBridge\logs`'a düş; minimum seviye yapılandırılabilir; `Directory.Build.props`'a `<Version>` (CI'da build numarası) → `AppVersion` artık gerçek | Test: bölüm okunuyor, dosya yolu seçimi; servis başlatıldığında dosya oluşuyor (yerel) |
| L3b | **Maskeleme:** Serilog `MaskingEnricher` + istisna mesajı/yığını dahil `LogScrubber` eşdeğeri (ajan tarafı, `ConnectionStringMasker` genişletilir: bearer, JWT, `AK-`, `Password=` varyantları); writer'lardaki maskesiz `catch` dalları ve `MikroDbReader.cs:673,694` düzeltilir | Test: bağlantı cümlesi içeren istisna dosyaya maskeli düşer |
| L3c | **Ajan olay kuyruğu (D10):** LocalStore SQLite migration `agent_log_outbox`; `AgentLogReporter` (seviye, tür, işlem, parmak izi kısması, tekrar sayısı); `POST /api/v1/agents/logs/batch` (yeni uç, en çok 50, `LogEventWriter`); eski `/agents/telemetry` korunur; gönderim hatası artık Warning | Testler: çevrimdışı birikir, çevrimiçi boşalır; kısma; sunucu uç testi |
| L3d | **Raporlanmayan hataların bağlanması:** `AgentSyncLoop` döngü hataları, bootstrap, change-log, Mikro bağlantı/sürüm, token yenileme, notify 401/5xx (kısmalı), heartbeat hatası, `AgentWorker` döngüsü + yerel kuyruk + ack hataları, mutabakat alarmları, WPF'te `Success=false` sonuçları ve `RunSyncDeltaAsync`; Servis için `AppDomain`/`TaskScheduler` yakalayıcıları + başlangıç/durma olayları (`agent_started` sürüm, ERP türü/sürümü ile); Polly yeniden denemeleri WARN | Her yol için birim test (sahte reporter çağrıldı) |
| L3e | **Senkron turu ölçümü:** `AGENT_SYNC_ROUND` INFO olayı (tetikleyici, süre, bölüm başına satır/ms/hata) — telefondaki `SYNC_ROUND` karşılığı; iş verisi yok | Test |
| L3f | **Heartbeat zenginleştirme (D11):** DTO'ya isteğe bağlı `AppVersion, HostKind (service/ui), ErpKind, ErpVersion, LastSyncResult, LastErrorCode`; servis `RecordError/RecordSuccessfulSync`'i gerçekten çağırır, WPF gerçek son senkron zamanını gönderir; sunucu `agents`'a nullable kolonlar + `agent_heartbeat_log`; `lastError` artık saklanır (maskeli) | Uç testi: eski gövde de kabul; geçmiş yalnız değişimde/15 dk'da bir satır |
| L3g | **İz kimliği:** `jobs.CorrelationId` (nullable) ingest'te istek başlığından; `RemoteJob`'a isteğe bağlı alan; ajan yazım hatası ve ack olayına ekler; ajanın tüm HTTP istekleri `X-Correlation-Id` gönderir | Uçtan uca test: ingest başlığı → job → ack olayı aynı kimlik |

### L4 — Sipariş Cepte (Android)
| ID | Görev | Kabul ölçütü |
|---|---|---|
| L4a | **Bağlam alanları (K3):** Room migration 36→37 `telemetry_events` + `userId, deviceId, sessionId, networkType, sdkInt, workspaceMode`; oturum kimliği uygulama açılışında üretilir; `deviceId` = `LicenseRepository.getDeviceId` (rastgele UUID). Yükleme gövdesine eklenir. `VERI_ENVANTERI.md`, `gizlilik-politikasi.md`, `kvkk-aydinlatma.md`, `play-data-safety.md` aynı PR | Migration testi; gövde testi; belge farkı |
| L4b | **İz kimliği:** OkHttp interceptor her isteğe `X-Correlation-Id` (UUID) ekler; `HTTP_ERROR` ve outbox hata olaylarına aynı kimlik yazılır; giden belge işinin kimliği belgeyle saklanır ve yeniden denemede korunur | Birim test: başlık + olay alanı eşleşir |
| L4c | **Yükleme işi düzeltmeleri:** tek seferlik iş `KEEP`/`APPEND_OR_REPLACE` (çalışanı iptal etmez), paket 50, 400 → paket tek tek denenir ve reddedilen olay atılır (kuyruk kilitlenmez), sunucu yanıtındaki `accepted/duplicate` loglanır, yükleme hatası `AppLog`'a | Birim test: 400 zehirli paket kuyruğu kilitlemez |
| L4d | **Kapsama boşlukları:** yazıcı sonuçları (NO_PRINTER/NOT_PAIRED/BLUETOOTH_OFF, `SecurityException`), giriş/çıkış ve oturum düşmesi (`SESSION_ENDED` nedeniyle), `ApprovalCheckWorker`, `ProductImageDownloadWorker`, ön plan servisi başlatma/zaman aşımı, `resyncRequired` tam sıfırlama, önizleme/preflight hataları, depo kamerası/tarayıcı, barkod, EOD dışa aktarma, `AppDataStore` JSON ayrıştırma düşüşleri, notify 401 kısması; `printStackTrace`/çıplak `Log.*` → `AppLog`; `WARNING` → `WARN`; `CRASH` türü yalnız çökmeye, yakalanan istisna `HANDLED_ERROR`; mesajlardan belge kimlikleri çıkarılır | Birim testler + `printStackTrace` sayısı 0 (grep kontrolü) |
| L4e | **ANR ve çıkış nedenleri:** açılışta `ActivityManager.getHistoricalProcessExitReasons` (API 30+) → ANR/düşük bellek/yerel çökme `APP_EXIT` olayı (son okunan zaman damgası saklanır, tekrar raporlanmaz); Room açılamazsa bekleyen çökme dosyaları DB'siz doğrudan yüklenir (kilit döngüsü) | Birim test (sahte çıkış nedenleri) |
| L4f | **Breadcrumb:** ekran geçişleri, senkron başlangıç/bitiş, belge kuyruğa alma/gönderme sonucu, yazıcı bağlantısı otomatik breadcrumb (iş verisi yok) | Test |
| L4g | **Yerel log + tanılama (D13):** `AppLog` halka tamponu + "Diğer > Sistem > Tanılama gönder" düğmesi (onay diyaloğu) → `POST /api/v1/mobile/diagnostics` (L6'da sunucu ucu; bu görev L6c'ye bağlı) | Birim test: tampon sınırı, maskeleme |
| L4h | Sürüm yükseltme + Play internal (yetki onaylıysa) + KB kural 27 güncellemesi | Derleme + `check_16kb.py` |

### L5 — Log Merkezi v2 (Admin, gelişmiş)
| ID | Görev | Kabul ölçütü |
|---|---|---|
| L5a | **Hata grupları:** `GET /admin/log-groups` (durum, kaynak, firma, aralık, sıralama: son görülme/sayı/etkilenen cihaz) + `PATCH /admin/log-groups/{id}` (çözüldü/yok say, not, operatör); Admin `/logs/groups` sayfası (seyrek grafik, ilk/son görülme, sürümler, "olayları gör") | Uç + bUnit testleri |
| L5b | **İz zaman çizelgesi:** `GET /admin/logs/trace/{correlationId}` → aynı kimlikli tüm olaylar + ilgili `jobs` satırı (durum, deneme, son hata) + onay talebi varsa; Admin `/logs/trace/{id}`: telefon → sunucu → ajan → ERP sırası, kaynak şeritleri | Uçtan uca seed testi |
| L5c | **Grafikler ve özet:** `GET /admin/logs/stats` (saatlik/günlük kova, kaynak × seviye); `/logs` üstünde yığılmış çubuk grafik + "en çok hata veren firma/cihaz/sürüm" listeleri (grafik kütüphanesi eklenmez, SVG bileşen) | bUnit |
| L5d | **Canlı akış (D14):** `/logs` sayfasında "Canlı" anahtarı → `after` imleciyle 5 sn yoklama, yeni satırlar üste, durdur/başlat, en çok 1.000 satır bellekte | bUnit (zamanlayıcı sahte) |
| L5e | **Cihaz ve ajan sayfaları:** `/logs/device/{deviceId}` (telefon: kullanıcı, sürüm geçmişi, son senkron turları, hatalar), `/agents/{id}` (sürüm, ERP, heartbeat geçmişi → çevrimiçi/çevrimdışı şeridi, son senkron turları, hatalar); `Agents.razor` `lastStatus`, sürüm, son hata kolonları | Uç + bUnit |
| L5f | **Dışa aktarma:** `GET /admin/logs/export.csv` (aynı filtreler, en çok 50.000 satır, dosya indirme) | Test |

### L6 — Denetim kaydı, uzaktan tanılama, saklama ayarları
| ID | Görev | Kabul ölçütü |
|---|---|---|
| L6a | **`audit_events`** tablosu + `IAuditLog` (tek yazım yolu): aktör türü (admin/portal_user/mobile_user/agent/system), aktör kimliği ve adı, firma, eylem, hedef türü/kimliği, sonuç, IP (maskeli son oktet), kullanıcı ajanı özeti, iz kimliği, önce/sonra özeti (hassas alan yok). Yazılanlar: admin/portal/telefon **giriş başarılı/başarısız** (neden kodu), çıkış; operatör: firma oluştur/güncelle, lisans oluştur/iptal, API anahtarı oluştur/iptal/döndür/kopyala, webhook, iş yeniden deneme, mobil kullanıcı/cihaz/koltuk/abonelik, veri kaynağı değişimi; portal: kullanıcı ekle/güncelle/sil/rol, cihaz engelle, onay kuralları, depo ayarları, TV ekranı iptal. **Not (K1):** Admin tokenı ortak olduğu sürece operatör kimliği son giriş yapan operatördür — bu kısıt kayıtta `ActorConfidence=shared_session` ile işaretlenir | Uç başına test: eylem kaydı oluştu, parola/anahtar kayıtta yok |
| L6b | `GET /admin/audit` (filtre: firma, aktör, eylem, sonuç, aralık, imleç) + Admin `/audit` sayfası; başarısız giriş patlaması için özet kartı; `api_key_secret_access_audits` da bu görünümde | Uç + bUnit |
| L6c | **Uzaktan tanılama (D12):** `diagnostic_requests` + `diagnostic_bundles`; Admin ajan/cihaz sayfasında "Tanılama iste"; heartbeat yanıtına `diagnostics: {requestId}` (eski ajan gövdeyi yok sayar — test), telemetri batch yanıtına aynı alan; `POST /api/v1/agents/diagnostics/{requestId}` ve `POST /api/v1/mobile/diagnostics` (boyut sınırı, maskeleme); ajan tarafı log kuyruğu yükleme; Admin'de paket görüntüleme/indirme | Uçtan uca test; boyut sınırı 413 |
| L6d | **Saklama ayarları sayfası** (D8) + tahmini tablo boyutu gösterimi | bUnit + uç testi |

### L7 — Uyarılar (K4, yalnız panel)
| ID | Görev | Kabul ölçütü |
|---|---|---|
| L7a | `alert_rules` (varsayılan kurallar seed, D16) + `alerts` (açık/onaylandı/kapandı, ilk/son tetiklenme, bağlam bağlantısı) + `AlertEvaluationWorker` (dakikada bir, tekrar açmama) | Her kural için test (sahte saat) |
| L7b | Admin üst menüde **uyarı rozeti** (açık uyarı sayısı, 30 sn yoklama), `/alerts` sayfası (onayla, kapat, ilgili log/grup/ajan bağlantısı), kural eşiklerini düzenleme | bUnit |

### L8 — Kapanış
| ID | Görev | Kabul ölçütü |
|---|---|---|
| L8a | KB güncellemeleri: ErpBridge 00 (yeni kurallar: log yazımı iş akışını bozmaz, iz kimliği, `LogEventWriter` tek yol, denetim tek yol), 03 (yeni tablolar), Android KB kural 23/25/27 + yeni kural; ekosistem `knowledge_base/00` | Belgeler güncel |
| L8b | Canlıda duman testi: `/health/schema` current, `/logs` açılıyor, telefon + ajan olayı görünüyor, iz zaman çizelgesi bir satış için dolu | DURUM'da kanıt |
| L8c | "Seni Bekleyenler" son hâli | — |

---

## 4. Beklenen insan kapıları (şimdiden bilinenler)
- K1: Admin ortak oturum açığının kapatılması (ayrı iş).
- Coolify'da `Logs__InternalIngestKey` ortam değişkeninin centralapi, admin ve portal konteynerlerine eklenmesi (D6).
- Müşteri PC'lerinde yeni ajan sürümünün (servis + masaüstü) kurulması; servis log dosyasının oluştuğunun gözle kontrolü.
- Telefonda uçtan uca deneme: satış → iz zaman çizelgesi; "Tanılama gönder".
- Play Console Data Safety formunun `play-data-safety.md`'ye göre güncellenmesi (K3) ve Sipariş Cepte production kararı.
- Eski `mobile_telemetry_events` tablosunun düşürülmesi (90 gün sonra, ayrı onay).
