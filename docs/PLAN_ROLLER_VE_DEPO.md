# Goal Plan — Çoklu Rol, Muhasebe Onayı, Canlı Depo Ekranı ve Performans Paneli

Durum: **Onaylandı — Adım 5 bitti** (Adım 6 `GOAL_PANEL_GELISTIRMELERI.md` P4'e taşındı; sırada Adım 7) · Tarih: 2026-09-16 · Kapsam: ErpBridge.CentralApi + ErpBridge.Portal
(Sipariş Cepte telefon uygulamasında değişiklik yok.)

---

## 0. Özet

Firma yönetim paneli bugün tek rollü (ADMIN / MANAGER / SALES) ve yalnızca okuma + onay yapıyor.
Bu plan onu dört farklı kullanıcı grubuna hizmet eden bir operasyon sistemine çevirir:

| Kullanıcı | Nerede | Ne yapar |
|---|---|---|
| Yönetici / Admin | Panel (masaüstü) | Dashboard, performans raporları, roller, ekranlar, eşik ayarları |
| Muhasebe | Panel (geniş ekran, klavye) | Gelen siparişleri hızla onaylar/reddeder → onaylanan depoya düşer |
| Depo personeli | Panel (telefon/tablet tarayıcısı) | Siparişe başlar → paketler → araca yükler |
| Depo TV'si | Panel `/ekran` (kiosk) | Salt okunur canlı pano; gecikenleri kırmızıyla vurgular |

### Alınan kararlar (2026-09-16, kullanıcı)

1. **Depo personeli web panelden çalışır** — mobil uyumlu depo sayfası; Play yayını gerekmez.
2. **Depo kuyruğuna tüm satışlar düşer** — satış onayı açıksa onaylanınca, kapalıysa sipariş gelir gelmez.
3. **TV ekranı eşleştirme koduyla bağlanır** — kullanıcı koltuğu harcamaz, uzaktan iptal edilir.
4. **Koltuk kuralı değişmez** — rolü ne olursa olsun her aktif kullanıcı bir koltuk.

### Planın varsayılan kararları (itiraz edilebilir)

| # | Karar | Gerekçe |
|---|---|---|
| V1 | Depo modülü **firma bazında açılır** (`Enabled`, varsayılan kapalı) | Depo kullanmayan firmada kuyruk dolup anlamsız uyarı üretmesin |
| V2 | ERP'li firmada sipariş **ajan ERP'ye yazmadan önce** kuyruğa düşer; ERP'ye aktarılamazsa kartta "ERP hatası" rozeti | Depo, ajan gecikmesi yüzünden beklemesin; hata görünür kalsın |
| V3 | `MUHASEBE` rolü satış/iade/tahsilat/tediye/alış taleplerini onaylayabilir; kart ve sayım talepleri ADMIN/MANAGER'da kalır | Muhasebenin işi finansal belgeler |
| V4 | Depo ve muhasebe rolü **yalnız paneli** kullanır; bu rollerden başka rolü olmayan kullanıcı telefona giremez | Depocunun telefondan satış girmesi engellenir |
| V4′ | **2026-09-16 değişti** ([GOAL_PANEL_GELISTIRMELERI.md](GOAL_PANEL_GELISTIRMELERI.md) K4): yalnız DEPO rolü olan kullanıcı telefona girer ama yalnız Depo ekranını görür (yeni sürüm şartıyla); muhasebe için V4 aynen | Telefonun Depo ekranı sunucu kuyruğuna bağlanıyor |
| V5 | Gerçek zamanlılık **SignalR yerine mevcut long-poll + sıra numarası** deseniyle | Altyapıda zaten var (`/android/notify`), proxy/ölçek ayarı gerektirmez; tek konteynerde < 2 sn gecikme |
| V6 | Statü geri alınabilir (ör. yanlışlıkla "Başla"), ama **her geri alma loglanır** ve yalnız işi başlatan kişi 5 dk içinde ya da yönetici yapabilir | Hatalar düzelebilsin, rapor bozulmasın |
| V7 | Tüm süreler **sunucu saatiyle** hesaplanır | Cihaz saatleri güvenilmez |

---

## 1. Analiz — metnin mevcut sisteme etkisi

| İstek | Bugün | Boşluk |
|---|---|---|
| Beni hatırla, 30 gün | Mobil token zaten 30 gün; panel oturumu yalnız sekmede (`sessionStorage`) | Kalıcı saklama + "hatırlama" seçilmezse kısa token. **Portalın Data Protection anahtarları kalıcı değil** → her dağıtımda saklı oturumlar okunamaz |
| Çoklu rol | `mobile_users.Role` tek kolon + `CanApprove` bayrakları | Ara tablo, izin hesabı, eski telefon sürümleriyle uyum |
| Muhasebe onay ekranı | Onay merkezi sunucuda (kural 16), panelde basit kart listesi | Klavye kısayollu, bölünmüş görünüm, satır detayı, toplu onay |
| Canlı depo ekranı | Yok. Telefondaki WMS ekranı **tamamen yerel**, sunucuya bağlı değil | Sunucuda sipariş hazırlık kaydı, eşleştirilen ekran cihazı, canlı akış |
| Statü akışı | Yok | Durum makinesi + değişmez olay logu + eşzamanlılık kilidi |
| Performans raporu | Portal raporları satış/tahsilat odaklı (kural 18) | Personel bazında hazırlık süresi, bekleme süresi, geciken sayısı |

Mevcut desenler yeniden kullanılır (yeni icat yok):
- **Tek karar kilidi:** `ExecuteUpdate … WHERE Status = <beklenen>` (onay merkezindeki `ClaimAsync`) → iki depocu aynı siparişe aynı anda başlayamaz.
- **Değişiklik sırası:** `UpdatedSeq` + `changedSinceSeq` (onay listesi) → ekranlar yalnız değişeni çeker.
- **İzin satırdan okunur, token'dan değil** (kural 14) → rol geri alındığında anında etkili.
- **Bekleyen istek hub'ı:** `BootstrapNotificationHub` deseni.

---

## 2. Veritabanı tasarımı (PostgreSQL, EF Core migration'ları)

> Tablo adları mevcut `mobile_*` adlandırmasına uyar; istenen `user_roles` yapısı
> `mobile_user_roles` olarak kurulur (bu sistemdeki "kullanıcı" = `mobile_users`).

### 2.1 Çoklu rol

```text
mobile_user_roles
  UserId          uuid  FK → mobile_users.Id  (ON DELETE CASCADE)
  Role            varchar(16)   ADMIN | MANAGER | ACCOUNTING | WAREHOUSE | SALES
  GrantedAtUtc    timestamptz
  GrantedByUserId uuid NULL
  PRIMARY KEY (UserId, Role)
  INDEX (Role)                       -- "firmadaki depocular" sorgusu
```

- **Migration geri doldurma:** her kullanıcının mevcut `Role` değeri bir satır olarak eklenir. Satırı olmayan kullanıcı olamaz (servis garanti eder, en az 1 rol).
- `mobile_users.Role` kolonu bu planda **silinmez**; "birincil rol" olarak yazılmaya devam eder
  (öncelik: ADMIN > MANAGER > ACCOUNTING > WAREHOUSE > SALES). Eski telefon sürümleri
  `session.user.role` okur ve yalnız ADMIN/MANAGER/SALES tanır → session'da `role` **eski üç değere eşlenir**, yeni `roles: []` dizisi eklenir.
- `CanApprove` / `CanManageApprovalRules` bayrakları MANAGER için aynen kalır.
- **İzin hesabı tek sınıf:** `Mobile/RolePermissions` (rollerin birleşimi) — `CanUsePortal`, `CanUsePhone`, `CanApprove(kind)`, `CanManageUsers`, `CanOperateWarehouse`, `CanViewReports`, `CanManageDisplays`.
- **Son admin kuralı** (kural 14) rol setine göre yeniden yazılır: ADMIN rolü olan son aktif kullanıcıdan ADMIN kaldırılamaz.

### 2.2 Sipariş hazırlık kaydı (güncel durum)

```text
order_fulfillments
  Id                  uuid PK
  TenantId            uuid  FK
  SourceJobId         uuid  FK → jobs.Id          UNIQUE (TenantId, SourceJobId)  -- idempotency
  ApprovalRequestId   uuid NULL                    -- onaydan geldiyse
  OrderNo             varchar(64)                  -- mobileDocumentId / belge no
  CustomerCode        varchar(64)
  CustomerName        varchar(200)
  SalespersonUserId   uuid NULL
  SalespersonName     varchar(120)                 -- anlık görüntü (kullanıcı silinse de kalır)
  Amount              numeric(18,2)
  LineCount           int
  ItemQuantity        numeric(18,3)
  ItemsJson           jsonb                        -- toplama listesi: [{stockCode,name,quantity,unit}]
  Status              varchar(16)   PENDING | PREPARING | PACKED | LOADED | CANCELLED
  QueuedAtUtc         timestamptz                  -- kuyruğa düştüğü an
  StartedAtUtc        timestamptz NULL             -- son "Hazırlanıyor"a geçiş
  PackedAtUtc         timestamptz NULL
  LoadedAtUtc         timestamptz NULL
  AssigneeUserId      uuid NULL                    -- hazırlayan personel
  AssigneeName        varchar(120) NULL
  VehiclePlate        varchar(16) NULL
  ErpState            varchar(16)   NONE | PENDING | WRITTEN | FAILED   -- ERP'li firmada
  UpdatedSeq          bigint                        -- firma içi değişiklik sırası (sequence, identity değil — kural 11)
  CreatedAtUtc / UpdatedAtUtc

  INDEX (TenantId, Status, QueuedAtUtc)            -- pano ve kuyruk
  INDEX (TenantId, UpdatedSeq)                     -- canlı akış
  INDEX (TenantId, AssigneeUserId, PackedAtUtc)    -- personel raporu
```

### 2.3 Statü logu (değişmez, rapora kaynak)

```text
order_fulfillment_events
  Id               bigint PK (identity)
  TenantId         uuid
  FulfillmentId    uuid FK → order_fulfillments.Id
  FromStatus       varchar(16) NULL                -- ilk kayıtta NULL
  ToStatus         varchar(16)
  Action           varchar(24)   QUEUED | START | PACK | LOAD | UNDO | CANCEL | REASSIGN | ERP_FAILED
  ActorUserId      uuid NULL                       -- sistem olayında NULL
  ActorName        varchar(120)
  DeviceId         varchar(128) NULL
  Note             varchar(500) NULL
  OccurredAtUtc    timestamptz                     -- SUNUCU saati
  INDEX (TenantId, FulfillmentId, OccurredAtUtc)
  INDEX (TenantId, ActorUserId, OccurredAtUtc)
```

- Satırlar **asla güncellenmez/silinmez**. `order_fulfillments` hızlı okuma için özettir; tutarsızlıkta log esastır.
- **Süreler:** bekleme = `START.at − QUEUED.at` · net hazırlık = `PACK.at − (son START).at` · yüklemeye kadar = `LOAD.at − PACK.at`. Geri alınan (UNDO) başlangıç net süreye sayılmaz.
- **Geçişler:** `PENDING→PREPARING→PACKED→LOADED`; `PREPARING→PENDING` ve `PACKED→PREPARING` yalnız UNDO; `*→CANCELLED` yalnız yönetici. Her geçiş `UPDATE … WHERE Id = @id AND Status = @beklenen` → 0 satır = 409 `FULFILLMENT_STATE_CHANGED` (+ kimin değiştirdiği).

### 2.4 Firma depo ayarları

```text
tenant_warehouse_settings
  TenantId                 uuid PK FK
  Enabled                  bool   default false
  PendingWarnMinutes       int    default 15    -- sarı
  PendingCriticalMinutes   int    default 30    -- kırmızı + yanıp sönme
  PreparingWarnMinutes     int    default 20
  PreparingCriticalMinutes int    default 45
  PackedWarnMinutes        int    default 60    -- paketli ama yüklenmemiş
  UpdatedByUserId / UpdatedAtUtc
```

### 2.5 Depo TV ekranları

```text
display_devices
  Id               uuid PK
  TenantId         uuid FK
  Name             varchar(80)          -- "Depo girişi TV"
  TokenHash        char(64)             -- SHA-256; düz token saklanmaz
  CreatedByUserId  uuid
  CreatedAtUtc / LastSeenAtUtc / RevokedAtUtc NULL

display_pairing_codes
  Code             char(6) PK           -- TV'de görünen kod (yalnız 10 dk geçerli)
  PairingSecretHash char(64)            -- TV tarafının gizli yoklama anahtarı
  ExpiresAtUtc     timestamptz
  DisplayDeviceId  uuid NULL            -- admin eşleştirince dolar
  ClaimedAtUtc     timestamptz NULL
```

Eşleştirme: TV `/ekran`'ı açar → sunucu kod + gizli anahtar üretir, TV kodu gösterir ve yoklar →
admin panelde "Ekran ekle"ye kodu ve ekran adını yazar → TV bir sonraki yoklamada
`scope=display` token'ını alır (firma + cihaz iddiası, uzun ömürlü, her istekte `RevokedAtUtc` kontrolü).
Ekran **yalnız pano uçlarını** okuyabilir.

---

## 3. Mimari

```text
Telefon (satış) ──/ingest/jobs──┐
                                ▼
                       CentralApi ─ ApprovalService ──(onay)──┐
                                │                            ▼
                                └─(onaysız satış)──► FulfillmentService ──► order_fulfillments
                                                         │  + order_fulfillment_events
                                                         ▼
                                              TenantEventHub (bellek içi, firma başına)
                                                         │  long-poll /api/v1/portal/events?sinceSeq
                                                         ▼
Portal (Blazor Server) ─ TenantFeed (firma başına tek abonelik, devrelere dağıtır)
   ├─ /muhasebe  (klavye odaklı onay)          ├─ /depo   (personel, dokunmatik)
   ├─ /          (yönetici dashboard)          └─ /ekran  (TV, salt okunur, display token)
```

- **Kuyruğa alma tek noktadan:** `FulfillmentService.EnqueueSalesOrderAsync(db, tenant, job, payload, actor)` hem `IngestEndpoints` (onaysız satış) hem `ApprovalService` (onay) içinden, **aynı transaction'da** çağrılır; idempotent.
- **Canlı akış:** her geçiş commit sonrası `TenantEventHub.Publish(tenantId, seq)`. Portal firma başına tek long-poll tutar, değişenleri `changedSinceSeq` ile çeker ve o firmanın açık devrelerine iletir. Bağlantı koparsa 15 sn'lik yoklamaya düşer.
- **Kısıt:** hub bellek içidir → CentralApi **tek konteyner** varsayar (bugünkü Coolify kurulumu). Yatay ölçekte Redis/PostgreSQL LISTEN gerekir — bu planın dışında, KB'ye not düşülür.
- **TV dayanıklılığı:** `/ekran` sayfası bağlantı koparsa kendini otomatik yeniden yükler (Blazor yeniden bağlanma ayarı + zamanlayıcı), saat ve "bağlantı" göstergesi taşır, ekran koruyucuya karşı Wake Lock ister.

---

## 4. Rol × ekran matrisi

| Ekran / işlem | ADMIN | MANAGER | ACCOUNTING | WAREHOUSE | SALES |
|---|:-:|:-:|:-:|:-:|:-:|
| Panele giriş | ✅ | ✅ | ✅ | ✅ | ❌ |
| Telefona giriş | ✅ | ✅ | ❌* | ❌* | ✅ |
| Yönetici dashboard + performans | ✅ | ✅ | ❌ | ❌ | – |
| Satış/tahsilat raporları, cariler, stok | ✅ | ✅ | ✅ | ❌ | – |
| Muhasebe onay ekranı | ✅ | ✅ (CanApprove) | ✅ (V3) | ❌ | – |
| Depo sayfası (başla/paketle/yükle) | ✅ | ✅ | ❌ | ✅ | – |
| Sipariş iptal / yeniden atama | ✅ | ✅ | ❌ | ❌ | – |
| Kullanıcı ve rol yönetimi | ✅ | ❌ | ❌ | ❌ | – |
| TV ekranı eşleştirme, depo eşikleri | ✅ | ✅ | ❌ | ❌ | – |

\* Başka bir rolü (SALES/MANAGER/ADMIN) de varsa girer. Birden çok rol = izinlerin birleşimi.

---

## 5. Adım adım Goal Plan

Her adım **ayrı dal → PR → CI yeşil → main → Coolify otomatik dağıtım** olarak kapanır.
Her adımın sonunda: `dotnet build` 0 uyarı/0 hata, tüm testler yeşil, KB güncel, `deliverable.md` kaydı,
PR'daki inceleme yorumları çözülmüş. Migration içeren adımlarda **canlı şema doğrulaması**
(`__EFMigrationsHistory` + yeni tablolar) — `--migrate` hatası sessiz geçtiği için zorunlu.

### Adım 1 — Temel: kalıcı oturum anahtarı ve migration güvencesi
- Portal: Data Protection anahtarları kalıcı dizine (`/keys`) + Coolify'da `lisans-portal` için kalıcı volume.
- CentralApi `/health`: bekleyen migration varsa `degraded` döner (sessiz şema hatasını görünür yapar).
- **Kabul:** portal yeniden dağıtıldığında sekmedeki oturum düşmüyor; `/health` migration durumunu raporluyor.

### Adım 2 — Çoklu rol altyapısı (sunucu)
- `mobile_user_roles` + geri doldurma migration'ı; `RolePermissions`; `ApprovalPermissions` rol setine geçer (V3).
- Kullanıcı uçları `roles: []` alır/döner; son admin kuralı rol setiyle; session'da `roles` + eski `role` eşlemesi.
- Login'e `client` (`android` | `portal`); yalnız panel rolü olan kullanıcı telefonda 403 `ROLE_NOT_ALLOWED_ON_PHONE` (V4).
- **Kabul:** eski telefon sürümü aynı şekilde giriş yapıyor (sözleşme testi); çok rollü kullanıcının izinleri birleşim; rol kaldırılınca bir sonraki istekte etkili.

### Adım 3 — Panel: Beni Hatırla + rol bazlı arayüz
- Login'de "Beni hatırla" kutusu → `rememberMe` ile 30 günlük token + şifreli `localStorage`; seçilmezse 12 saatlik token + sekme oturumu.
- Menü ve sayfa korumaları rol birleşimine göre (`PortalPageBase.Requires`); rolüne göre açılış sayfası (depocu → `/depo`, muhasebe → `/muhasebe`).
- Kullanıcılar sayfasında çoklu rol seçimi (çipler) ve rol açıklamaları.
- **Kabul:** tarayıcı kapatılıp açılınca 30 gün boyunca oturum sürüyor; seçilmezse sekmeyle bitiyor; yetkisiz sayfa URL'i yönlendiriyor (bUnit).

### Adım 4 — Sipariş hazırlık çekirdeği (sunucu)
- `order_fulfillments`, `order_fulfillment_events`, `tenant_warehouse_settings` migration'ları.
- `FulfillmentService`: kuyruğa alma (ingest + onay, iki veri kaynağında), geçişler, geri alma, iptal, yeniden atama, ERP durumu (V2).
- Uçlar: `GET /api/v1/portal/fulfillments?status&changedSinceSeq`, `GET /{id}` (satırlar + olay geçmişi), `POST /{id}/start|pack|load|undo|cancel|reassign`, `GET|PUT /warehouse/settings`, `GET /api/v1/portal/events?sinceSeq&wait` (long-poll).
- **Kabul:** aynı siparişe eşzamanlı iki "başla" → biri 409; aynı job iki kez gelse tek kayıt; depo kapalı firmada kayıt açılmıyor; her geçiş log satırı üretiyor; ilişkisel testler (SQLite) iki veri kaynağında.

### Adım 5 — Muhasebe onay ekranı (geniş ekran, klavye)
- Solda sıkışık liste (tür, müşteri, tutar, plasiyer, bekleme süresi), sağda detay (satırlar, cari bakiye, stok uyarısı, geçmiş).
- Kısayollar: `↑/↓` gezin · `A` onayla · `R` reddet (not penceresi) · `N` not · `Space` seç · `Shift+A` seçilenleri onayla (onay penceresiyle) · `/` ara · `F` filtre. Ekranda kısayol yardımı (`?`).
- Onaylanan satış otomatik depoya düşer (V1 açıksa); liste canlı güncellenir, başkası karar verirse satır kilitlenip kaybolur.
- **Kabul:** fare kullanmadan 10 siparişi onaylamak mümkün (bUnit + tarayıcı kontrolü); çakışan kararlar doğru mesaj veriyor.

### Adım 6 — Depo personeli sayfası (telefon/tablet tarayıcısı)
- Sekmeler: **Bekleyenler · Benim hazırladıklarım · Paketlenenler**; büyük dokunmatik düğmeler: *Başla → Bitti (Paketlendi) → Araca yüklendi* (plaka isteğe bağlı).
- Toplama listesi (satırlar, işaretleme yalnız cihazda), 5 dk içinde geri al, çakışmada "Ahmet başladı" mesajı.
- **Kabul:** statü değişikliği panoya ve dashboard'a ≤ 2 sn'de yansıyor; 400 px genişlikte kullanılabilir.

### Adım 7 — Canlı Depo Ekranı (TV kiosk)
- `/ekran`: eşleştirme ekranı (büyük 6 haneli kod) → pano.
- Pano: koyu tema, büyük yazı; kolonlar **Bekliyor · Hazırlanıyor · Paketlendi (yüklemeye hazır)**; kartta sipariş no, müşteri, kalem sayısı, canlı süre, hazırlayan kişi.
- Eşik vurguları: sarı (uyarı), kırmızı + yavaş yanıp sönme (kritik); kolon başlığında geciken sayısı; taşarsa otomatik sayfa döngüsü.
- Admin: "Ekranlar" sayfası (ekle, adlandır, son görülme, iptal) ve depo eşik ayarları.
- **Kabul:** personel "Başla" dediğinde kart Bekliyor'dan Hazırlanıyor'a kendiliğinden geçiyor; bağlantı kesilip gelince pano kendini toparlıyor; iptal edilen ekran ≤ 1 dk'da kapanıyor.

### Adım 8 — Yönetici & performans paneli
- Dashboard'a depo kartları: kuyrukta, hazırlanıyor, geciken, bugün paketlenen/yüklenen, ortalama bekleme ve hazırlık süresi.
- **Performans raporu:** tarih aralığı; personel bazında tamamlanan sipariş, ortalama/medyan net hazırlık süresi, toplam hazırlık süresi, kalem başına süre; en uzun bekleyenler.
- **Sipariş zaman çizelgesi:** bir siparişin tüm olayları kim/ne zaman, aralarındaki süreler.
- **Kabul:** rapor rakamları olay logundan birebir hesaplanıyor (bilinen senaryolarla test); 92 günlük aralık sınırı.

### Adım 9 — Canlıya alma ve kullanım rehberi
- Canlı şema doğrulaması, bir test firmasında uçtan uca: satış → muhasebe onayı → depo başla/bitti/yükle → TV → rapor.
- Kısa kullanım rehberi (muhasebe kısayolları, depo akışı, TV kurulumu) panel içinde "Yardım" sayfası.
- KB: kural 14/16/19 güncellemesi + yeni kural (depo akışı).

---

## 6. Riskler

| Risk | Önlem |
|---|---|
| Canlıda migration sessizce başarısız olur | Adım 1 `/health` kontrolü + her migration adımında canlı şema doğrulaması |
| Eski telefon sürümleri yeni rolleri tanımaz | Session'da `role` eski üç değere eşlenir; sözleşme testi |
| Bellek içi hub, tek konteyner varsayımı | KB'de açık not; yoklama yedeği; ölçeklenme ayrı iş |
| TV'de günlerce açık Blazor devresi | Otomatik yeniden yükleme, hafif pano bileşeni, sunucu tarafında firma başına tek abonelik |
| "Beni hatırla" token'ı çalınırsa 30 gün geçerli | Token yalnız şifreli depoda; kullanıcı pasifleştirme ve cihaz engeli anında etkili (kural 14); panelden "tüm oturumları kapat" |
| Depocunun yanlış tıklaması raporu bozar | Geri alma loglu ve net süreden düşülür (V6) |

## 7. Kapsam dışı (sonraki fazlar)

Telefon uygulamasının depo ekranını sunucuya bağlama · barkodla toplama doğrulaması · araç/rota sevkiyat planlama ·
bildirim (SMS/push) · e-fatura · çoklu depo/şube ayrımı · CSV/Excel dışa aktarma.

---

## 8. İlerleme

| Adım | Durum | PR | Not |
|---|---|---|---|
| 1 — Kalıcı oturum anahtarı + şema kontrolü | ✅ | [#49](https://github.com/Retrosero/ErpBridge/pull/49) | Canlıda `/health/schema` current (20 migration); `lisans-portal` için `/app/keys` volume eklendi, açılışta uyarı yok |
| 2 — Çoklu rol altyapısı | ✅ | [#50](https://github.com/Retrosero/ErpBridge/pull/50) | `mobile_users.Role` öncelikli rol değil, eski uygulamalar için ADMIN/MANAGER/SALES türetilmiş değer olarak tutuldu (plan 2.1 taslağından sapma, daha güvenli). Telefon uygulamasına `ROLE_NOT_ALLOWED_ON_PHONE` Türkçe metni sonraki telefon sürümüne |
| 3 — Beni Hatırla + rol bazlı arayüz | ✅ | Faz 46 | Sunucu: login `rememberMe` → panelde 30 gün / 12 saat. Panel: şifreli `localStorage` ya da `sessionStorage`, rol birleşimine göre menü ve `PortalPageBase.Requires`, çoklu rol çipleri (ekle + düzenle). **Sapmalar:** muhasebenin açılış sayfası `/onaylar` (`/muhasebe` adım 5'te gelince değişecek); `/depo` şimdilik yer tutucu (adım 6); admin kendi rollerini panelden değiştiremez. Yerelde tarayıcıda doğrulandı (muhasebe/depo/admin, yeni sekmede hatırlanan oturum, rol kaydı) |
| 4 — Sipariş hazırlık çekirdeği | ✅ | Faz 47 | `order_fulfillments`, `order_fulfillment_events`, `tenant_warehouse_settings` (migration yalnız yeni tablo). `FulfillmentService` + `/api/v1/portal/fulfillments`, `/warehouse/settings`, `/events`; ingest (native + ERP) ve onay kuyruğa alır, ajan ack ve konsol retry ERP durumunu günceller. **Sapmalar:** `UpdatedSeq` mevcut `tenant_sync_counter`'dan (ayrı sıra açılmadı); kuyruk sırası için `QueuedSeq` eklendi (SQLite `DateTimeOffset` sıralayamaz), index `(TenantId, Status, QueuedSeq)`; `changedSinceSeq` durum filtresini yok sayar; hub mevcut bootstrap hub'ının ayrı bir örneği. 11 ilişkisel test |
| 5 — Muhasebe onay ekranı | ✅ | Faz 48 | `/muhasebe` onay masası: en eski üstte liste + detay (kalemler, ödemeler, stok uyarısı, cari bakiye, geçmiş), `portal-keys.js` ile tüm kısayollar, işaretle + onay pencereli toplu onay, `/portal/events?approvalsSeq` ile canlı liste. Muhasebenin açılış sayfası `/muhasebe`. **Sapmalar:** onaylar için ayrı hub yok — depo olay hub'ı ve `/events` iki konulu oldu; `/onaylar` kart sayfası kaldı (menüde ikisi de); canlı döngü sayfa başına (V5'teki firma başına ortak abonelik adım 7'ye). Tarayıcıda gerçek tuşlarla: 3 onay, notlu red, toplu onay, başka yetkilinin kararı ≤ 3 sn |
| 6 — Depo personeli sayfası | ⬜ | — | [GOAL_PANEL_GELISTIRMELERI.md](GOAL_PANEL_GELISTIRMELERI.md) P4 ile yapılacak (panel + telefon) |
| 7 — Canlı Depo Ekranı | ⬜ | — | |
| 8 — Yönetici & performans paneli | ⬜ | — | |
| 9 — Canlıya alma ve rehber | ⬜ | — | |

Her adım kullanıcının "Adım N'i başlat" onayıyla başlar.
