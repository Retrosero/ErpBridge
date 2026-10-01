# 00 — ErpBridge Sistem Mimarisi ve Teknoloji Haritası

> **Hedef:** ErpBridge (.NET 10 Entegrasyon Ajanı & Merkezi SaaS API) projesinde
> çalışan AI kodlama editörleri ve mühendisler için temel başvuru kaynağı.
>
> **Durum:** Çok-ERP dönüşümü Faz 16–22 boyunca yapıldı. Mikro tam
> implementasyon; `Erp.Sql` ortak SQL Server change-log motoru; `Erp.Logo`
> iskelet.
>
> **Karar (2026-09-11):** Mikro V15'te değişiklik akışının **tek kaynağı**
> `dbo._ERPB_SENKRONIZASYON`'dur (`MikroLegacySynchronizationChangeLog`, ERP
> nesnesi yaratmaz). `Erp.Sql` gölge motoru (`_ERPB_SYNC` / `_ERPB_SYNC_DEL`)
> kodda durur ama V15 üretiminde **kullanılmaz**; ayrıntı [[01_Accounting_Adapters]] §4. Detay: [`docs/multi-erp-adapter-plan.md`](../docs/multi-erp-adapter-plan.md)

İlgili diğer modüller:
- ERP Entegrasyon Adaptörleri: [[01_Accounting_Adapters]]
- Donanım & Evrak Standartları: [[02_Hardware_Bluetooth_Printing]]
- Veri Şeması ve Kurallar: [[03_Data_Dictionary_and_Rules]]
- AI Asistan Fonksiyonları: [[04_AI_Assistant_Function_Catalog]]

---

## 1. Proje Özeti ve Katmanlı Mimari

ErpBridge; müşteri lokasyonundaki ERP (bugün: Mikro V15/V16) ile buluttaki saha
satış mobil uygulaması (`Siparis_Cepte`) arasında güvenli, çift yönlü, dayanıklı
bir köprü kurar.

- **Hedef Framework:** .NET 10 (C# 13), `TreatWarningsAsErrors=true`
- **Veritabanları:** SQLite (ajan yerel kuyruğu & mapping), PostgreSQL 16 (merkezi API), MS SQL Server (ERP)
- **İletişim:** Tüm trafik **outbound HTTPS**; müşteri ağına inbound port açılmaz.

```text
[Siparis_Cepte Mobil] ──(HTTPS)──> [ErpBridge.CentralApi (PostgreSQL)]
                                              ▲
                                              │ (Outbound Long-Polling / Ingest)
                                              ▼
                                   [ErpBridge.Agent.Service]
                                   (Worker, LocalStore, DPAPI)
                                              │
                                              ▼ (Yerel Ağ / TCP 1433)
                                   [ERP SQL Server (Mikro V15/V16)]
```

---

## 2. Çözüm Proje Ağacı

```text
ErpBridge/src/
├── ErpBridge.Shared/           # Result<T>, Error, ConnectionStringMasker, sabitler
├── ErpBridge.Erp.Abstractions/ # IErpAdapter, IErpChangeLogSource, IErpTrackedTableCatalog,
│                               #   ErpSyncCursor, IErpConnectionTestOrchestrator,
│                               #   IErpReconciliationProbe, Documents/*, ChangeLog/*
├── ErpBridge.Erp.Sql/          # SqlServerShadowTableChangeLog (vendor bilgisi SIFIR),
│                               #   ShadowTableDdl, ShadowCursor, KeyKindProjection,
│                               #   ErpFieldText, SqlServerFieldWidthProvider
├── ErpBridge.Core/             # AgentConfig, ErpChangeLogSyncService, BootstrapSyncService,
│                               #   AgentSyncLoop (giden) + AgentJobPump (gelen) — iki host ortak
├── ErpBridge.LocalStore/       # SQLite migrations, IMappingStore, SqliteErpSyncCursorStore, DPAPI
├── ErpBridge.RemoteApi/        # HttpRemoteApiClient, Polly v8, Idempotency-Key
├── ErpBridge.Erp.Mikro/        # MikroAdapter, 7 Writer, V15/V16 katalogları, Mikro trigger installer
├── ErpBridge.Erp.Logo/         # LogoAdapter iskeleti (Faz 21 — dikiş doğrulaması)
├── ErpBridge.Agent.Service/    # Windows Service: AgentWorker, BootstrapWorker, HeartbeatWorker
├── ErpBridge.Agent.UI/         # WPF Ayar Paneli + tepsi ajanı: DesktopBackgroundSyncService,
│                               #   DesktopJobPumpService (Service kurulu olmasa da köprü çalışır)
├── ErpBridge.CentralApi/       # ASP.NET Core Web API, PostgreSQL, JWT & Multi-Tenant
├── ErpBridge.Admin/            # Blazor Server operatör konsolu (ErpBridge ekibi)
└── ErpBridge.Portal/           # Blazor Server firma yönetici paneli (Faz 42; firmanın admin/yöneticisi)
```

### Güncel Referans Grafiği (Faz 16–22 sonrası)

| Proje | Referans veriyor |
|---|---|
| `ErpBridge.Shared` | — |
| `ErpBridge.Erp.Abstractions` | `Shared` |
| `ErpBridge.Erp.Sql` | `Shared`, `Erp.Abstractions` |
| `ErpBridge.Core` | `Shared`, `Erp.Abstractions` |
| `ErpBridge.LocalStore` | `Core`, `Shared`, `Erp.Abstractions` *(Faz 18.4 — `IErpSyncCursorStore` implemente ettiği için doğrudan)* |
| `ErpBridge.RemoteApi` | `Shared`, `Core` |
| `ErpBridge.Erp.Mikro` | `Shared`, `Erp.Abstractions`, `Erp.Sql`, `Core` |
| `ErpBridge.Erp.Logo` | `Shared`, `Erp.Abstractions`, `Erp.Sql` |
| `ErpBridge.Agent.Service` | `Shared`, `Core`, `LocalStore`, `RemoteApi`, `Erp.Mikro` |
| `ErpBridge.Agent.UI` | `Shared`, `Core`, `LocalStore`, `RemoteApi`, `Erp.Abstractions`, `Erp.Mikro` |

**Host'ların Mikro'ya bağlılığı (Faz 19):** `Agent.Service` ve `Agent.UI` kod
gövdesinde **hiçbir Mikro tipi yoktur**. Kalan tek referans, DI extension'daki
`using ErpBridge.Erp.Mikro.DependencyInjection` — `ErpAdapterRegistration`
switch'i için (hedeflenen istisna). İkinci bir ERP adaptörü geldiğinde bu
registration ayrı bir composition projesine taşınır.

---

## 3. Geliştirici ve AI Editör İçin Bağlayıcı Kurallar

1. **Şemayı tahmin etme.** Bu projenin en pahalı hatasıydı: 7 writer'ın tamamı ve
   katalogun yarısı hiç doğrulanmamış kolon adlarıyla yazılmıştı, 212 birim testi
   de hepsi mock bağlantı kullandığı için görmüyordu. Kolon adlarını canlı DB'den
   ya da vendor'ın kanıtlanmış uygulamasından türet; `MikroSchemaContractTests`
   gibi bir **canlı şema testi** yaz (`ERPBridge_RUN_INTEGRATION=1`).
2. **V15/V16 farkı adaptörde kalır.** `RECno`/`Guid` ayrımını sadece
   `IMikroIdentityStrategy` + `MikroTrackedTableCatalog.For(version)` bilir.
   Core/Service/CentralApi bilmez.
3. **SQL parametrik zorunluluğu.** Kullanıcı/payload verisi asla string concat
   ile SQL'e girmez — Dapper `@Param`. İnterpolate edilen tek şey compile-time
   katalogdan gelen tablo/kolon adları, `SqlIdentifier.Validate`'den geçirilir.
4. **ERP yazımı atomik transaction.** Header + satırlar + self-link + açıklamalar
   tek transaction; hata → rollback.
5. **Idempotent yazım.** Aynı `externalId` tekrar gelirse yeni evrak açılmaz,
   önceki `recno`/`guid` döner. `mappings` tablosu `(tenant, entity, doc_type,
   external_id)` UNIQUE.
6. **Sync cursor sıralaması.** Cursor **push kabul edildikten sonra** ilerler,
   önce değil. Arada crash → sayfa tekrar oynatılır (güvenli).
7. **Secret maskeleme.** Bağlantı cümleleri ve API anahtarları `ConnectionStringMasker`
   ile filtrelenir; Windows'ta DPAPI ile şifrelenir.
8. **Kimlik alanı taşarsa reddet.** `ErpFieldText.Identifier` exception atar
   (kısaltılmış `cari_kod` başka hesapla eşleşebilir); serbest metin kırpılır.
9. **Ajan token'ı süreli; "token var mı" kontrolü yeterli DEĞİL.**
   Central API ajanlara **60 dakikalık** JWT veriyor ve ajanlar için bir refresh
   ucu yok — yeni token almanın tek yolu yeniden `register` olmak. Bir token
   string'inin dolu olması onun geçerli olduğunu göstermez.
   *2026-09-10: `EnsureRegisteredAsync` (WPF), `DesktopHeartbeatService` ve
   `JwtTokenProvider`'ın üçü de yalnızca "JWT dolu mu" diye bakıyordu. Sonuç:
   ajan başladıktan 60 dk sonra kalıcı olarak ölüyordu — upload, status probe ve
   notify long-poll hepsi 401 alıyor, arayüzdeki hiçbir buton kurtaramıyordu
   (stale token dolu olduğu için kayıt atlanıyordu). Ayrıca notify 401'i anında
   döndüğü için döngü saniyede ~5 istekle sunucuyu dövüyordu.*
   Token yaşam döngüsü artık tek yerde: `IAgentTokenService` (Core). Süre
   dolmadan 5 dk önce proaktif yeniler; `AgentTokenRefreshHandler` da 401 gören
   her çağrıdan sonra reaktif olarak tazeler. Yeni bir "kayıtlı mıyım" kontrolü
   yazma — `EnsureValidAsync` çağır.
10. **Admin uçlarında tenant query'den gelir, token'dan DEĞİL.**
   `IJwtIssuer.IssueForAdmin` yalnızca `sub`, `scope=admin`, `jti` üretir —
   **`tenant` claim'i yoktur.** Bir admin ucunda `http.User.TryGetTenantId`
   çağırmak, her isteği 401 ile reddetmek demektir. Doğru desen
   `AdminBootstrapEndpoints`'tedir: `[FromQuery] Guid? tenantId` + boşsa
   `400 MISSING_TENANT`. (Ajan uçları farklıdır: agent JWT'si `tenant` claim'i
   taşır, orada `TryGetTenantId` doğrudur.)
   *2026-09-10: `AdminAuditEndpoints`'in üç ucu da bu hatayı taşıyordu; "Sync
   geçmişi" sayfası bu yüzden hiçbir zaman veri gösteremiyordu. Uçların hiç
   testi olmadığı için hata fark edilmemişti — bkz.
   `tests/ErpBridge.CentralApi.Tests/Endpoints/AdminAuditTests.cs`.*

11. **`mobile_records.UpdatedSeq` asla identity kolonu olmaz.**
   Identity değeri `INSERT` anında atanır, commit sonra olur. T1 seq=100 alır,
   T2 seq=101 alır ve **önce** commit ederse, `cursor=99` ile okuyan cihaz 101'i
   görüp imlecini ilerletir; T1 commit ettiğinde 100 **kalıcı olarak atlanmış**
   olur. İmleç tek doğruluk kaynağı olduğu için bu sessiz, kalıcı veri kaybıdır.
   Doğru yol `MobileRecordProjector.ReserveAsync`: `tenant_sync_counter` satırını
   `UPDATE … RETURNING` ile kilitleyip blok ayırmak. Satır kilidi transaction
   sonuna kadar durur, yani **tahsis sırası = commit sırası**.
   Bundan çıkan üç bağlayıcı kural:
   - `mobile_records`'a yazan **her** yol açık bir transaction içinde olmalı ve
     bloğunu bu sayaçtan almalı. Yeni bir yazar eklerken atlamak, boşluğu geri
     getirir.
   - Sayaç **mümkün olan en geç anda** tahsis edilir. Kilit tenant'ın tüm
     yazarlarını bekletir; snapshot yeniden-yazımı gibi uzun işler kilitten
     **önce** bitmiş olmalı.
   - Rollback sayaçta boşluk bırakır; bu zararsızdır. Zararlı olan tek şey
     yeniden sıralamadır.
   - **Sayacın ikinci kullanıcısı (Faz 47):** `order_fulfillments.UpdatedSeq` de bu sayaçtan
     (`ReserveAsync(…, 1)`) alınır; portalın `changedSinceSeq` okuması aynı garantiye dayanır.
     Değerler `mobile_records` ile aynı sayı uzayını paylaşır; cihaz imleçlerinde boşluk olur, zararsızdır.
12. **Mobil okuma yolu tek uçtur: `POST /api/v1/android/sync/pull`.**
   Yeni bir `/sync/<bölüm>` ucu **eklenmez**. Cihaz imleç göndermezse her şeyi,
   gönderirse yalnızca sonrasını alır — ikisi de aynı sorgu, aynı tablo.
   Silme de aynı akışta bir tombstone'dur; artımlı bir ERP okuması zaten yok
   olmuş satırı bildiremez, dolayısıyla cihazın silmeyi öğrenebileceği başka
   yer yoktur. İmleç **opak token**'dır (`SyncCursor`), çıplak sayı değil;
   `nextCursor` son sayfada bile doludur, "bitti" bilgisini `hasMore` taşır.
   Bu uç `mobile:read` kapsamıyla çalışır: silme bir *okuma* olayı olduğu için
   cihazın yerelinden kayıt düşürmesi için ayrı bir yazma kapsamı gerekmez.

   **Birleştirme sunucuda kalır.** `mobile_records` ERP şeklindedir (`stocks`,
   `barcodes`, `prices`, `inventory` ayrı satırlar); uygulamanın ürün satırı ise
   denormalizedir — biri değişince ürünün tamamı yeniden kurulup `urun` olarak
   gönderilir (`MobileEntityAssembler`). Ham bölümleri cihaza akıtmak bu birleşimi
   Kotlin'e taşırdı: fiyat ve fiyat-listesi adları için yeni Room tabloları, artı
   herhangi bir parça değişince ürünü yeniden hesaplama mantığı. Sunucu bu join'i
   zaten yapıyordu; fark, artık tüm katalog yerine yalnızca delta'nın dokunduğu
   kayıtlar için çalışması.

   Bir sayfa, imlecin üzerinden geçtiği ham satır sayısından **daha az** değişiklik
   taşıyabilir (bir stok kartı + 3 barkodu + 5 fiyatı tek üründür) ve bazen hiç
   taşımaz. Döngüyü `changes.size` değil **`hasMore`** sürdürür.

   **Akışın ürettiği varlıklar:** `urun`, `cari` (montajlı); `cariAdresleri`,
   `bankalar`, `kasalar`, `kasaYonetim`, `fiyatTanim`, `cariHareketleri`,
   `stokHareketleri` (düz eşleme). `stokSeviye` ve `fiyatListesi` ayrı varlık
   değildir — `urun` içinde gelirler. Bir kasa satırı **iki** kayıt üretir
   (`kasalar` + `kasaYonetim`), çünkü uygulama onu iki tabloda tutuyor.
   `faturaHareket` henüz akışta yok; eski ucundan gelmeye devam ediyor.

   İstemcinin henüz okumadığı bölümler (`openOrders`, `salesConditions`)
   değişiklik üretmez ama imleç yine de üzerlerinden geçer. Bu yüzden ileride yeni bir varlık eklenirse
   mevcut cihazların o geçmişi kaçırmaması için iki yoldan biri seçilir:
   - `SyncCursor.FormatVersion` **yükseltilir** → dağıtım anında **tüm filo** bir kez
     tam senkron alır (varlığı okuyamayan eski uygulamalar dahil). Varlığın geçmişi
     dağıtımdan önce zaten doluysa (ör. ajanın yıllardır yüklediği bir ERP bölümü)
     tek doğru yol budur.
   - Varlığı **yalnızca yeni bir uygulama sürümü yazabiliyor ve okuyorsa** (dağıtımda
     hiç satırı yoksa), o sürüm **kendi imlecini yükseltmede bir kez sıfırlar**;
     yalnızca yükselen cihaz tam senkron alır. Rota planları ve ziyaretler (kural 17)
     bu yolu kullanır. İstemci bilmediği varlığı atlar (`BridgeDeltaSync`: "An entity
     this build does not know is skipped"), yani eski uygulama bozulmaz.
13. **Aynı satırı yeniden göndermek bir değişiklik değildir.**
   Ajan her döngüde aynı satırları yükler. `PayloadSha256` değişmediyse
   `UpdatedSeq` ilerletilmez — ilerletilirse tüm filo 30 saniyede bir katalogun
   tamamını yeniden indirir.

---

14. **Mobil koltuk kuralları tek sınıftadır: `Mobile/MobileSeatService` (Faz 32, 2026-09-13).**
   Telefon (`/api/v1/android/account/*`, firma admini) ve Admin konsolu
   (`/api/v1/admin/tenants/{id}/mobile/*`, operatör) aynı servisi çağırır;
   kural bir uçta yeniden yazılmaz.
   - **Koltuk = aktif ve silinmemiş kullanıcı, admin dahil.** Ekleme ve yeniden
     etkinleştirme yalnızca boş koltuk varken ve abonelik `active`/`grace`
     iken olur (`SEAT_LIMIT_REACHED`, `SUBSCRIPTION_REQUIRED/EXPIRED`).
     Koltuk sayısı aktif kullanıcının altına indirilemez
     (`SEATS_BELOW_ACTIVE_USERS`); tenant'ta her zaman bir aktif admin kalır
     (`LAST_ADMIN`).
   - **Kilitlenme:** koltuğu değiştiren her işlem transaction içinde önce
     `tenants.SeatLockVersion`'ı artırır; bu UPDATE tenant satır kilidini alır
     ve eşzamanlı iki "kullanıcı ekle" isteğinin aynı son koltuğu almasını
     engeller. Kilitten önce sayım yapan bir kod bu garantiyi bozar.
   - **Abonelik satırları düzenlenmez.** `tenant_subscriptions`'a her değişiklik
     yeni satır ekler ve öncekinin `IsCurrent`'ını kapatır (tenant başına tek
     current, kısmi unique index). Önce eski satır kaydedilir, sonra yenisi
     eklenir — ikisi aynı `SaveChanges` batch'inde olursa index reddeder
     (bootstrap snapshot'taki aynı tuzak).
   - **Silme yumuşaktır** (`DeletedAtUtc`); kullanıcı adı unique index'i silinmiş
     satırları yok sayar, ad yeniden kullanılabilir, geçmiş kalır.
   - **Mobil kullanıcı token'ı** (`scope=mobile-user`) yalnızca imzayı
     kanıtlar. Süre: telefon her zaman **30 gün** (günlerce çevrimdışı çalışır); panel
     (`client=portal`) login gövdesinde `rememberMe=true` ise 30 gün, değilse **12 saat**
     (`JwtIssuer.PortalSessionHours`, Faz 46). Telefonun gönderdiği `rememberMe` yok sayılır. `MobileAccountEndpoints.AuthorizeAsync` her çağrıda kullanıcı,
     cihaz, tenant ve aboneliği veritabanından yeniden doğrular; rol token'dan
     değil satırdan okunur. Pasifleştirme ve cihaz engelleme token süresini
     beklemeden etkili olur.
   - **Çoklu rol (Faz 45, 2026-09-16):** roller `mobile_user_roles` (`UserId, Role` birleşik PK,
     `GrantedAtUtc`, `GrantedByUserId`) tablosundadır; roller `ADMIN`, `MANAGER`, `ACCOUNTING`,
     `WAREHOUSE`, `SALES`. İzin **birleşimdir** ve tek sınıftan okunur: `Domain/RolePermissions`
     (`CanUsePhone`, `CanUsePortal`, `CanManageUsers`, `CanViewReports`, `CanViewLedger`,
     `CanPlanRoutes`, `CanOperateWarehouse`). İzin kontrolü yapan her kullanıcı yüklemesi
     `.Include(u => u.Roles)` ister; rolleri yüklenmemiş satır yalnız eski kolona düşer (hiçbir zaman
     fazlasını vermez). `mobile_users.Role` artık **eski uygulamalar için türetilmiş** bir kolondur:
     her yazımda `MobileUserRoles.Legacy` ile ADMIN > MANAGER > SALES'ten biri; izin buradan okunmaz.
     Session/kullanıcı DTO'su `role` (bu eski değer) + `roles[]` taşır. Kullanıcı uçları `roles[]`
     alır; yalnız `role` gönderen eski ekran ADMIN/MANAGER/SALES'i değiştirir, ACCOUNTING ve
     WAREHOUSE'u korur. Son admin kuralı rol setine bakar. Migration mevcut her kullanıcıya eski
     `Role`'ünü satır olarak ekler.
   - **Telefon mu panel mi (Faz 45):** login gövdesinde `client` = `android` (varsayılan; telefon
     göndermez) | `portal`; token'da `client` iddiası. `MobileUserAccess.ClientDenial` **her istekte**:
     telefon oturumu `CanUsePhone` (ADMIN/MANAGER/SALES, **panel goal P4b'den beri WAREHOUSE da**) ister, yoksa 403
     `ROLE_NOT_ALLOWED_ON_PHONE`. **Yalnız depo rollü telefon kullanıcısı** (`IsWarehouseOnlyOnPhone`: WAREHOUSE var,
     ADMIN/MANAGER/SALES yok) ayrıca uygulama sürümü ister: cihazın kayıtlı `AppVersion`'ı (telefonun `versionName`'i,
     ör. `1.5.240`) `Mobile:MinWarehousePhoneVersion` ayarından (`appsettings.json` varsayılanı **`1.5.235`** — sunucu
     kuyruğuna bağlı Depo ekranlı ilk sürüm, panel goal P5d; Coolify'da `Mobile__MinWarehousePhoneVersion` ile ezilir)
     küçükse ya da ayar boşsa — girişte **ve her istekte** —
     aynı 403 döner (eski uygulama depo rolünü tanımaz, tüm ekranları açardı; rolü sonradan daralan açık oturum da
     kapanır). Bu kullanıcı `/ingest/*`'a hiç belge gönderemez: 403 `ROLE_NOT_ALLOWED`. Karma rollüler etkilenmez; panel oturumu `CanUsePortal` (SALES dışı) ister, yoksa 403
     `PORTAL_REQUIRES_MANAGER`. Login'de reddedilen istek cihaz satırı açmaz. Panel oturumu
     `/ingest/*`'a belge gönderemez (403 `PORTAL_CANNOT_SUBMIT_DOCUMENTS`) ve telefonun veri akışını
     okuyamaz: `MobileClientPolicy` (bootstrap, `sync/pull`, change set, notify, telemetri)
     `MobileUserStateRequirement.PhoneClientOnly` ile panel oturumuna 403
     `PORTAL_SESSION_NOT_ALLOWED` döner. Panelin de kullandığı hesap ve onay uçları
     `MobileUserPolicy`'dedir. Telefon uygulaması
     `ROLE_NOT_ALLOWED_ON_PHONE` için henüz Türkçe metin göstermez (genel hata mesajı).
   - **Aynı token veri ve belge uçlarında da geçer.** Telefonun okuduğu uçlar
     (`/android/*`, `/android/sync/pull`, notify, change-set, telemetri)
     `MobileClientPolicy`, belge gönderdiği `/ingest/*` uçları
     `AgentOrApiKeyPolicy` ile korunur; ikisi de API anahtarı **veya**
     `scope=mobile-user` kabul eder ve `MobileUserStateRequirement` ile
     kullanıcı/cihaz/tenant/abonelik durumunu her istekte doğrular. Red,
     `MobileUserAuthorizationResultHandler` ile hata kodlu `ApiError` gövdesi
     döner (`USER_INACTIVE`, `SUBSCRIPTION_EXPIRED`…) ki uygulama doğru mesajla
     oturumu kapatsın. Yeni bir mobil uç eklenirse bu iki politikadan biri
     kullanılır; yalnızca `ApiKeyPolicy` kullanan uç firma hesabıyla giren
     kullanıcıya kapalı kalır. API anahtarı yolu (eski ERP aktivasyonu) aynen
     çalışır.
   - Koltuklar Play Store dışında satılır; uygulamada satın alma yoktur
     (Siparis_Cepte `docs/PLAN_CALISMA_MODLARI.md` §7). Operatör ekranı:
     Admin konsolu `/tenants/{id}/mobile` (`Pages/TenantMobile.razor`); API hata
     kodlarının Türkçe karşılığı tek yerde, `Api/MobileSeatMessages`. Lisans
     sayfasında her lisans kartında "Telefon girişi oluştur" paneli
     (`Shared/MobileLoginPanel.razor`, Faz 35): firma kodu, hak kullanımı; hak
     yoksa hak tanımlama, varsa kullanıcı adı/parola/rol ile giriş oluşturma ve
     telefonda girilecek bilgilerin özeti. Parola geri gösterilmez.

15. **ERP'siz firmada defter merkez sunucudur: `Native/NativeDocumentProcessor` (Faz 33, 2026-09-13).**
   `tenants.DataSource` = `erp` (varsayılan, veriyi ajan getirir) veya `native`
   (ERP yok; telefonlar kart girer, satış/tahsilatı sunucu işler). Operatör
   Admin konsolundaki koltuk ekranından seçer. `native`'e geçiş, ajan veya ERP
   verisi varsa; `erp`'ye geçiş, telefondan girilmiş veri varsa reddedilir —
   iki kaynak birbirinin üstüne sessizce yazardı.
   - **Aynı okuma yolu.** İşleyici ajanın üreteceği ERP şeklindeki satırları
     (`stocks`, `barcodes`, `prices`, `inventory`, `customers`,
     `customerTransactions`, `stockTransactions`) `MobileRecordProjector` ile
     `mobile_records`'a yazar (`SourceDatabase = native`). Cihazlar bunları
     `sync/pull` ile alır; birleştiriciye özel durum eklenmez.
   - **Tek transaction, tenant kilidi.** `/ingest/*` native tenant'ta belgeyi
     kuyruğa değil işleyiciye verir: `tenants.NativeLockVersion` artırılır (satır
     kilidi), belge işlenir, `jobs` satırı `Succeeded`/`Failed` yazılır,
     projeksiyon yapılır, commit, `hub.Publish`. İş kayıtsız etki, etki kayıtsız
     iş olamaz. Aynı `externalId` tekrar gelirse ingest'in idempotency kontrolü
     etkiyi ikinci kez uygulatmaz.
   - **Sayısal gerçekler tipli tablolarda.** `native_stock_levels` (depo 1) ve
     `native_customer_balances` kesin decimal ile güncellenir; `mobile_records`
     yalnızca sonucun kopyasıdır.
   - **Belge kuralları:** `stock_card` yalnızca firma yöneticisi (açılış miktarı
     yalnızca stok satırı yokken alınır); `customer_card` herkes (açılış bakiyesi
     yalnızca bakiye satırı yokken). `sales_order`: her satır stok düşer, cari
     borçlanır; ödeme şekli anında ödeme ise (`Nakit`, `Kredi Kartı`, `EFT /
     Havale`…) aynı tutarda tahsilat hareketi de yazılır, açık bakiye değişmez.
     `collection`: cari alacaklanır. Diğer türler kayıt olarak saklanır, etki
     yapmaz. **Stok eksiye düşebilir** — çevrimdışı yapılmış satış sonradan
     reddedilmez.
   - **Kısmen işlenmiş belge olmaz.** Bir satır geçersizse belge `Failed`
     kaydedilir ve bellekteki tüm stok/bakiye değişiklikleri geri alınır
     (`DiscardLedgerChanges`); telefon yeniden denemez, hata Admin konsolunun iş
     kuyruğunda görünür. Cari `customerCode` ile, eski sürümler için yalnızca
     **tek** eşleşen `counterparty` unvanıyla çözülür.
   - ERP tenant'ında `stock_card`/`customer_card` 409 `CARDS_REQUIRE_NATIVE_TENANT`
     ile reddedilir (ajanın bu türler için yazıcısı yok).
   - **ERP yazıcıları native tenant'a yazamaz** (`Native/NativeTenantGuard`, 409
     `TENANT_IS_NATIVE`): ajan kaydı, legacy `/bootstrap`, `/bootstrap/upload`
     start **ve** complete, `/ingest/changeset`. Ajan kaydı ve veri kaynağı
     değişimi aynı tenant satır kilidini (`NativeLockVersion`) alır; kayıt veri
     kaynağını kilit altında `WHERE DataSource = 'erp'` ile yeniden okur. Yeni bir
     ERP yazma ucu eklenirse bu koruma da eklenir.
   - `native`'e geçiş, ajan veya ERP verisinin yanında **bekleyen/işlenen iş**
     (`jobs` Pending/Processing) varken de reddedilir; o belgeleri ne ajan ne
     işleyici alırdı.
   - **Ürün kartı düzenleme ve silme (Faz 34, 2026-09-13).** `stock_card` aynı
     `stockCode` ile yeniden gelirse kartı günceller; kod kimliktir, değişmez.
     Kart tek barkod listeler: aynı ürüne ait **eski barkod kayıtları**
     `MobileRecordProjector.TombstoneAsync` ile düşürülür, yoksa eski barkod
     kasada hâlâ bu ürüne çözülürdü. Açılış miktarı düzenlemede yok sayılır.
     `stock_card_delete` (yalnız yönetici) kartı `ApplyDeletesAsync("STOKLAR")`
     ile barkod/fiyat/stok satırlarıyla birlikte düşürür ve stok satırını siler;
     cihazlara `urun` silinmiş olarak gider. **Hareket görmüş ürün silinemez:**
     her satış satırı `native_stock_levels.LastMovementAtUtc`'yi işaretler, dolu
     ise silme `Failed` olur (satış geçmişi var olmayan ürüne işaret ederdi).
     ERP tenant'ında `stock_card_delete` de 409 `CARDS_REQUIRE_NATIVE_TENANT`.
   - **İade ve alış (Faz 36, 2026-09-14).** Satırlı iki belge:
     `sales_return` (stok **girer**, hareket `tip 2` iade giriş, cari
     **alacaklanır** "İade"; `paymentType` anında ödeme ise — `Nakit`,
     `Banka İade`, `EFT / Havale`… — "İade Ödemesi" borç hareketi de yazılır,
     açık bakiye değişmez) ve `purchase_receipt` (stok girer, `tip 0` giriş,
     tedarikçi — bir cari kartı, `supplierCode` — alacaklanır "Alış"; anında
     ödemede "Tediye" borç hareketi). Alışta satır **isteğe bağlıdır** (katalogda
     olmayan kalem tedarikçiye yine borç yazar), verilen her satır bilinen ürün
     olmalıdır; iadede satır zorunludur. İkisi de ürünü `LastMovementAtUtc` ile
     işaretler (silinemez). Satış, iade ve alış satırları tek yerde
     (`BookLinesAsync`) önce **tamamen doğrulanır**, sonra deftere yazılır. ERP
     tenant'ında `purchase_receipt` ve eski (telefon belgesi olmayan) `sales_return` 409
     `DOCUMENT_REQUIRES_NATIVE_TENANT` (ajanın yazıcısı yok); **telefonun satırlı `sales_return`'ü
     (`mobileDocumentId` gövdesi) ERP'li firmada da kabul edilir** — ajanın çeviricisi iade faturası yazar
     (`NativeDocumentProcessor.RequiresNativeTenant`, ERP yazım Y6b'de bulundu). Telefonun kasa defterinden gelen
     satırsız `return` belgesi etkisiz kayıt olarak kalır (iade `sales_return` ile işlenir).
   - **Tediye (Faz 40, 2026-09-16).** Kasa defterinden gelen `disbursement` bir
     müşteriye ödenmişse cariyi **borçlandırır** ("Tediye" hareketi) — tahsilatın
     aynası. Bu tarihten önce etkisiz kayıt olarak kalıyordu ve tediye yapılan her
     müşterinin sunucudaki bakiyesi **eksik** görünüyordu (geçmiş kayıtlar geriye
     dönük işlenmedi). İstisnalar: `approvalKind = purchase` taşıyan alış ödemesi
     atlanır (`purchase_receipt` zaten borçlandırdı; işlenseydi tedarikçi iki kez
     borçlanırdı); müşteri adlandırmayan ödeme (gider `"Gider: …"`, diğer çıkışlar)
     kayıt olarak kalır ve `LastError`'a not düşer; bilinmeyen `customerCode` `Failed`.
     Giderler bilinçli olarak **telefonlara yayılmaz**: Gün Sonu "kasaya ne kadar
     teslim edeceğim?" sorusunu cihazdaki kasa kayıtlarından hesaplar, başkasının
     gideri gelirse tutar yanlış çıkar. Yöneticinin herkesin giderini görme yeri
     yönetici panelidir (Sipariş Cepte yol haritası Faz D). ERP'de işlenmiş gider fişleri
     (`cha_evrak_tip=37`) ise cari hareketleriyle telefona zaten iner; Sipariş Cepte onları
     yalnızca ADMIN/MANAGER'a, gider ekranında **salt okunur** gösterir, kasa defterine yazmaz
     (Sipariş Cepte KB 00 kural 31). Kart adı için `customerTransactions` satırı
     `cha_kasa_hizmet`/`cha_kasa_hizkod` taşır (2026-09-23).
   - **Fatura iskonto/KDV (2026-09-23, projeksiyon 6).** `stockTransactions` satırı Mikro'nun gerçek
     `discountAmount` (sth_iskonto1..6 toplamı) ve `vergi` (sth_vergi) tutarlarını taşır; Android
     `faturaHareket` bunları satırda döndürür. Eski paketlerde `null`dır ve telefon gizler — tutar
     farkından tahmin yapılmaz (Sipariş Cepte KB kural 33).
   - **Sayım (Faz 37, 2026-09-14).** `stock_count` (yalnızca `status =
     COMPLETED`) her satırda stoğu **fark kadar** oynatır:
     `countedQuantity - expectedQuantity`. Sayılan sayıya eşitlemez: sayım
     çevrimdışı yapılıp sonra yüklenir; sayımdan sonra başka telefonun işlediği
     satış eşitlemede silinirdi, farkta korunur. Farkı olmayan satır hareket
     yazmaz; fark hareketi tutarsızdır (`birimFiyat`/`tutar` 0), açıklamada
     sayılan miktar ve sayan kişi. Satırlar önce tamamen doğrulanır. Bilinen
     sınır: telefonun `expectedQuantity`'si sayım anında eskiyse fark da o kadar
     sapar (sayımdan önce senkron alınmalı). ERP tenant'ında `stock_count`
     eskisi gibi ajan kuyruğuna gider.
   - **Toplu kart (Faz 35, 2026-09-13).** Excel içe aktarma `stock_card_batch`
     (yalnız yönetici) ve `customer_card_batch` belgeleriyle gelir:
     `{ "cards": [...] }`, belge başına en fazla 500 kart (telefon 200 gönderir).
     Tek kart belgesiyle 1000 satır, ingest'in kullanıcı başına dakikada 100
     istek sınırına takılırdı. Her kart tekil kartla aynı doğrulamadan geçer;
     geçersiz kart **atlanır**, iş `Succeeded` kalır ve `LastError`'a
     "N booked, M skipped" notu düşülür. Hiçbir kart geçerli değilse `Failed`.
     Doğrulama deftere dokunmadan önce yapıldığı için atlanan kart iz bırakmaz.
   - Satış doğrulaması: satır `quantity > 0`, ürün kartı `mobile_records`
     `stocks`'ta **var olmalı** (kod doğrudan gelse bile), fiyat ve toplam eksi
     olamaz. Telefon kuyruğu `createdAt` sırasıyla gönderdiği için yeni ürünün
     kartı satıştan önce gider.
   - **Panel yazma yolu (GOAL_PANEL_ERPSIZ, 2026-09-21…25; plan `docs/GOAL_PANEL_ERPSIZ.md`, durum `…_DURUM.md`).** ERP'siz firmanın admini aynı işleyiciye panelden
     de yazar: `/api/v1/portal/native/*` uçları yalnız ikinci bir kapıdır, ikinci bir yazma motoru değil (D1).
     Her yazma ucu (ve kart/hareket/barkod okumaları): firma token'dan, yetki `RolePermissions.CanEditNativeData`
     (yalnız ADMIN + `DataSource=native`, D4; ERP'li firmada 409 `TENANT_IS_NOT_NATIVE`) — evrak listesi/detayı
     (`GET documents`, `GET documents/{key}`) ise salt okunur, ekstrenin `CanViewLedger` kapısıyla (Yönetici/Muhasebe
     ve ERP'li firma da okur); `operationId` ile idempotent `externalId`
     (`PortalNativeWriteHelpers.OperationKey`), işlem kaydı `native_audit_log` (D5). İptal/düzenleme uçları (cari, evrak, sayım)
     "zaten iptal" kontrolünden **önce** aynı işlemi arar (`ReplayAsync`) — yanıtı kaybolan tekrar 200 alır.
     Uçlar: kartlar (`stock-cards`, `customer-cards`), `collections`/`disbursements`, `ledger-adjustments`,
     `ledger/{key}/void|edit`, `sales-orders`/`purchase-receipts`/`sales-returns`, `documents` (liste/detay,
     `status=all|active|voided`, detayda `paymentType`), `documents/{key}/void|edit`,
     `stock-cards/{code}/movements`, `stock-counts` ve `stock-counts/{key}/void`, `barcodes/{barcode}` (tam eşleşme),
     `stock-cards/batch` ve `customer-cards/batch` (dosyadan içe aktarma; telefonun toplu kart belgeleri, ≤ 500 kart/istek, satır
     numaralı atlananlar; panel CSV/.xlsx'i kendisi okur — `SpreadsheetReader`, ek kütüphane yok).
   - **Düzeltme = storno (D2/D11).** Defterden/evraktan satır fiziksel silinmez ve üzerine yazılmaz:
     orijinal yerinde `voided` (+ `voidedByUserId`, `voidedAt`, `voidReason`) işaretlenir, ters kayıt
     `{orijinal id}|void` anahtarıyla ve `voidsKey` alanıyla eklenir; bakiye/stok ters çevrilir, hepsi tek
     transaction. Belge türleri: `ledger_void` / `ledger_edit` (yalnız tek başına tahsilat/tediye/düzeltme;
     `ledger_edit`'in kendi satırı yine düzenlenebilir), `ledger_adjustment` (gerekçe zorunlu),
     `document_void` (satış/alış/iadenin cari bacakları — anında ödeme dahil — **ve** stok satırları birlikte),
     `document_edit` (`document_void` + aynı türde düzeltilmiş evrak; düzeltme `-D1`, `-D2`… revizyon
     numarası alır, çünkü native evrak anahtarı cari + evrak no'dur ve iptal edilen orijinal numarasını
     korur; dolu numara atlanır/reddedilir), `stock_void` (bir sayımın tüm stok satırları). Ters satırlar
     evrak satırı sayılmaz (`PortalRecords.ParseLine`) ve ürün hareketlerinde varsayılan olarak gizlidir. Cari
     ekstresi ve `/portal/movements` ise iptal edilen **orijinali** varsayılan gizler ama **ters kaydı her zaman
     gösterir** — görünen satırlar bakiyeyi açıklasın diye.
   - **Panel sayımı** `stock_count`'u yükte bir alanla değil, yalnız güvenilir çağıranın verebildiği iç seçenekle
     (`NativeBookingOptions(CountAgainstCurrentLevel: true)`, `IngestAsync` parametresi; yükteki böyle bir alan yok sayılır, telefon bu moda geçemez) işler: fark, işleyicide tenant kilidi
     altında o anki `native_stock_levels`'a göre hesaplanır. Telefonun çevrimdışı sayımı (`expectedQuantity`'ye
     göre fark, sonradan gelen satış korunur) değişmedi.
   - **Ürün hareketleri** (`PortalStockMovements`): devir mevcut stoktan geriye yürüyerek bulunur — kartın
     açılış miktarı hareket değildir — böylece devir + hareketler = `native_stock_levels`.
   - **PostgreSQL notu:** `mobile_records.PayloadJson` `jsonb`'dir; yükte arama `Contains`/`LIKE` ile değil
     `EF.Functions.JsonContains` (`@>`) ile yapılır (SQLite testleri bunu yakalayamaz;
     `NativeDocumentNumberQueryTests` sorgunun Npgsql çevirisini sunucusuz doğrular).

16. **Onay merkezi sunucudadır: `Approvals/ApprovalService` (Faz 38, 2026-09-14).**
   Telefon belleğindeki onay listesinin yerini aldı; firmanın tüm onaycıları aynı
   kuyruğu görür, uygulama kapansa da talep kaybolmaz.
   - **Roller** (rol seti için kural 14): `ACCOUNTING` rolü finansal türleri (`sale`, `return`,
     `collection`, `disbursement`, `purchase`) karara bağlar; kart ve sayım talepleri ADMIN/MANAGER'da
     kalır (`ApprovalPermissions.AccountingKinds`). Onaycı listede yalnız karar verebildiği türleri +
     kendi taleplerini görür; `DecideAsync`/`ReopenAsync` türü ayrıca kontrol eder. "Başka onaycı var
     mı" (kendi talebi) türe göre hesaplanır. Eski metin: `mobile_users.Role` = `ADMIN` | `MANAGER` | `SALES`. Admin her
     şeyi yapar. Yöneticiye (`MANAGER`) admin iki yetki verir: `CanApprove`
     (onay/red/tekrar açma) ve `CanManageApprovalRules`. Satış kullanıcısında ve
     admin'de bu bayraklar tutulmaz (rol değişince temizlenir; yeniden verilir).
     Karar `ApprovalPermissions` ile her istekte **veritabanındaki satırdan**
     okunur, token'dan değil.
   - **Kurallar:** `tenant_approval_rules`, 8 tür: `sale`, `purchase`, `return`,
     `collection`, `disbursement`, `stock_count`, `product_card`,
     `customer_card`. Satır yoksa **hepsi açık**. Kimse muaf değildir (admin
     dahil). Session (`/account/me`, login) `approvalRules` taşır.
   - **Zorlama:** mobil kullanıcı kuralı açık bir türde belgeyi doğrudan
     `/ingest/*`'a gönderirse 409 `APPROVAL_REQUIRED`. Tür belge tipinden
     (`ApprovalKinds.ForDocumentType`) ya da payload'daki `approvalKind`'dan
     çıkar — alışın kasa ödemesi `disbursement` belgesidir ama `purchase`
     kuralına uyar. API anahtarı ve ajan kişi taşımadığı için muaftır; Excel
     batch'leri ve gider/diğer kasa hareketleri kural dışıdır.
   - **Talep:** telefon kuyruğundan `documentType = approval_request`, payload
     `{ kind, counterpartyName, amount, summary, documents: [{ documentType,
     externalId, payload }], replacesRequestId? }` (1–20 belge). Belgeler,
     onaysız akışta aynen gönderilecek olanlardır. Kart türleri yalnız kendi kart
     belgelerini, diğer türler kart dışı belgeleri taşır; iç içe talep ve batch
     yok. ERP tenant'ında kart talebi 409 `CARDS_REQUIRE_NATIVE_TENANT`. Aynı
     `externalId` tekrar gelirse aynı talep döner. Yalnız mobil kullanıcı talep
     gönderir (API anahtarı 403 `APPROVAL_REQUIRES_MOBILE_USER`).
   - **Tek karar:** her geçiş `ExecuteUpdate … WHERE Status = <beklenen>` ile
     yapılır (`ClaimAsync`); aynı anda basan ikinci onaycı 409
     `APPROVAL_ALREADY_DECIDED` ("… by <ad>") alır. Onay, belgeleri **aynı
     transaction'da** işler: native tenant'ta `NativeDocumentProcessor.IngestAsync`
     dış transaction'a katılır (commit ve `hub.Publish` onay servisindedir), ERP
     tenant'ında `Pending` job açılır. Belgelerden biri `Failed` olursa her şey
     geri alınır, talep **beklemede kalır**, 422 `APPROVAL_DOCUMENT_FAILED` +
     sebep. Önceden sunucuya ulaşmış belge ikinci kez işlenmez. Ürün kartı
     onayında onay, kartın istediği admin yetkisinin yerine geçer.
   - **Kendi talebi:** onaycı kendi talebini onaylayamaz (403
     `SELF_APPROVAL_NOT_ALLOWED`); firmada başka aktif onaycı yoksa onaylayabilir. `GET /summary`
     bunu `canApproveOwnRequests` olarak döner; telefon sunucunun reddedeceği
     Onayla düğmesini göstermez.
   - **Durumlar:** `Pending` → `Approved` | `Rejected` | `Withdrawn` (yalnız
     talep eden). `Rejected` → `Pending` (onaycı "tekrar aç") veya
     `Resubmitted` (talep eden düzeltip `replacesRequestId` ile yeni talep
     gönderir; ikisi aynı transaction'da). Her adım `approval_request_events`'e
     yazılır.
   - **Uçlar:** `/api/v1/android/approvals` — `GET ?status=pending,rejected|all
     &changedSinceSeq&beforeSeq&beforeExternalId&kind=sale,collection&take` (onaycı hepsini, diğerleri kendi
     taleplerini görür; `beforeSeq`+`beforeExternalId` = ekrandaki son talebin `requestedSeq`/`externalId`'si ile
     eskiye sayfalama — sıra milisaniyeden türediği için eşitlikte `externalId` bağı çözer; sıralama `requestedSeq, externalId` azalan, `kind` bilinmezse
     400 `INVALID_KIND`; yeni parametreler olmadan liste eskisiyle aynı — panel goal P1a),
     `GET /{id}` (belgeler, geçmiş, native'de stok uyarısı; onay engellenmez),
     `GET /summary`, `POST /{id}/approve|reject|reopen|withdraw {note}`,
     `GET|PUT /rules`. Konsol: `GET /api/v1/admin/tenants/{id}/mobile/approvals`
     (salt okunur) ve overview'da `approvalRules`.

17. **Ekip belgeleri her firmada merkezde işlenir: `Team/TeamDocumentProcessor` (Faz 39, 2026-09-16).**
   Rota planları ve ziyaretler ERP verisi değil **ekip verisidir**; ajanın onlar için
   yazıcısı yoktur. Bu yüzden `approval_request` gibi, veri kaynağı `erp` ya da `native`
   fark etmeksizin `/ingest/jobs` içinde (adım 5c, native dalından **önce**) merkezde
   işlenir.
   - **Belge türleri:** `route_plan` (plan + duraklar + atananlar, aynı `planId` ile
     yeniden gelirse **tamamen değiştirir**), `route_plan_delete` (`planId`), `visit`.
   - **Yetki:** üçü de **oturum açmış firma kullanıcısı** ister; API anahtarı veya ajan
     403 `TEAM_DOCUMENT_REQUIRES_MOBILE_USER`. Rota planlama ve silme (`RolePermissions.CanPlanRoutes`) yalnızca
     `ADMIN` veya `MANAGER`; **ekip/bölgeden sorumlu yönetici yalnız kendi kişilerine atar ve başka ekibin planını
     değiştiremez/silemez** (`OutOfScopeAsync`, `TeamScope`; GOAL_HEDEF_RUT K14, kural 31) — panelde de telefonda da. Ziyaretin `username`'i **token'daki kullanıcıdan** gelir,
     yükteki isim yok sayılır — telefon başkası adına ziyaret kaydedemez.
   - **Doğrulama:** `planId`/`stopId`/`visitId` ≤ 64; plan ≤ 500 durak, ≤ 100 atanan;
     `dayOfWeek` 1 (Pazartesi)–7; tarihler `yyyy-MM-dd`; ziyaret `status`
     `COMPLETED`/`SKIPPED`. **Atanan kullanıcı adları firmada var olmalı** — yanlış
     yazılmış bir ad planı kimsenin telefonuna düşürmezdi. Geçersiz belge `Failed`
     kaydedilir, hiçbir cihaza gitmez. Sunucuda hiç olmamış planın silinmesi başarılı
     sayılır (çevrimdışı oluşturulup silinmiş olabilir).
   - **Yeni tablo yoktur.** Sonuç `mobile_records`'a `routePlans` (anahtar `planId`) ve
     `routeVisits` (anahtar `visitId`) bölümleri olarak `SourceDatabase = "team"` ile
     yazılır; cihaz `sync/pull` ile `rotaPlanlari` / `rotaZiyaretleri` varlıklarını
     olduğu gibi alır (`BuildKind.Direct`, geçirgen). İş kaydı `jobs`'tadır.
   - **İmleç:** `FormatVersion` yükseltilmedi (kural 12'deki ikinci yol). Bu varlıkları
     yalnızca 1.5.233+ yazar ve okur; dağıtımda hiç satır yoktu. Uygulama rota özelliğini
     ilk kez açtığında imlecini bir kez sıfırlar.
   - **Veri kaynağı değişimi:** `team` satırları "ERP verisi" sayılmaz; rota planı olan
     ERP'li firma ERP'siz moda geçebilir ve planlar kalır
     (`AdminMobileSeatsEndpoints.SetDataSourceAsync`). Bu kontrol yeni bir kaynak adıyla
     `mobile_records`'a yazan her yeni işleyicide gözden geçirilmelidir.
   - Testler: `TeamDocumentsRelationalTests` (her iki veri kaynağında).

18. **Yönetici paneli uçları salt okunurdur: `Endpoints/PortalEndpoints` + `Portal/PortalReports` (Faz 41, 2026-09-16).**
   `/api/v1/portal/{summary,activity,visits,balances,stock}` — firmanın yöneticisinin web
   panelinden okuduğu rakamlar. Telefonla aynı hesapla giriş yapılır
   (`/api/v1/android/account/login`); grup `MobileUserPolicy` ile korunur ve her çağrı
   `MobileAccountEndpoints.AuthorizeAsync` ile kullanıcı/cihaz/abonelik doğrular.
   - **Rol kapısı (Faz 45):** `summary`/`activity`/`visits` → `CanViewReports` (ADMIN, MANAGER);
     `balances`/`stock` → `CanViewLedger` (+ ACCOUNTING). Reddedilen 403 `PORTAL_REQUIRES_MANAGER`.
   - **Firma token'dan gelir**, istekten değil. Yalnızca `ADMIN` ve `MANAGER`; `SALES`
     403 `PORTAL_REQUIRES_MANAGER`. Onaylar ve kullanıcılar için yeni uç yoktur:
     panel mevcut `/api/v1/android/approvals` ve `/api/v1/android/account/users` uçlarını
     kullanır (kuralları orada zaten var).
   - **Kaynak telefonla aynıdır:** para belgeleri `jobs`'tan, cari/stok/rota
     `mobile_records`'tan. Panel ile telefon farklı rakam gösteremez. Cari başlığı
     `title1 + title2`, bakiye `balance`, stok miktarı `inventory` satırlarının toplamı.
   - **Stok arama (panel goal P2, 2026-09-17):** `GET /api/v1/portal/stock/search` (sunucu taraflı sayfa ≤ 250;
     `q` kod/ad/barkod; tekrarlı `mainGroup`/`subGroup`/`brand`/`shelf`; `warehouse`, `priceList`, `minQty`/`maxQty`,
     `minPrice`/`maxPrice`, `status=all|in|out|negative|below` + `below`, `idleDays`;
     `sort=name|code|qty|price|group|brand|shelf|lastMovement`, `dir`; geçersiz değer 400 `INVALID_QUERY`) ve
     `GET /stock/facets` (grup/alt grup/marka/reyon sayılarıyla; depo ve fiyat listesi adları `lookups`'tan).
     Özet (`products/inStock/outOfStock/negative`) sayfanın değil **filtrenin tamamının**.
     **Kayıt aynası (`Portal/PortalRecordMirror<T>`):** panel liste sayfaları `mobile_records`'u firma başına bellekte,
     ayrıştırılmış küçük kayıtlar olarak tutar ve her istekte yalnız `UpdatedSeq > son uygulanan` satırları sırayla uygular
     (silinen satır öğeyi kaldırır). Güvenli çünkü sıra numarası sayaç satırı kilitlenerek ayrılır ve commit sırasıyla
     görünür (`MobileRecordProjector`). `Portal/PortalRecords` tüm ayrıştırmayı tek yerde yapar. `PortalStockCatalog`
     "stock" (`stocks/inventory/prices/barcodes/lookups`) ve "lines" (`stockTransactions`) aynalarından kurulur, ikisinden
     biri değişince yeniden hesaplanır; depo olayları bu satırları yazmadığı için dokunmaz.
     **Alan eşlemesi:** ERP `mainGroupCode/subGroupCode/brandCode/shelfCode/unit1`, ERP'siz kart `kategori/marka/shelfCode/birim`.
     **Verinin gerçeği (Codex #60):** Mikro okuyucusu (`MikroDbReader.ReadInventoryAsync`) firma toplamını ajanın depo
     numarasıyla tek satır gönderir, `reservedQuantity = 0`, `lastMovementDate = NULL`. Bu yüzden depo seçeneği/kolonu yalnız
     envanterde birden çok depo varken sunulur (lookup'taki boş depolar listelenmez), rezerve yalnız sıfırdan farklıysa
     gösterilir, **son hareket** ürünün `stockTransactions` aynasındaki en yeni `tarih`'tir (ERP'de tam STOK_HAREKETLERI
     geçmişi, ERP'sizde her kayıtlı satır). Gerçek depo bazlı miktar ajan değişikliği ister (kapsam dışı).
     **Cariler ve ekstre (panel goal P3, 2026-09-17):** `GET /api/v1/portal/customers` (sayfalı; `q` kod/unvan/telefon,
     `balance=all|receivable|payable|nonzero`, `sort=title|code|balance|absBalance`, `dir`; toplam alacak/borç filtrenin tamamı),
     `GET /customers/card?code=`, `GET /customers/ledger?code=&from=&to=&kind=…&page&pageSize` (yeni hareket üstte;
     `kind` = `sale|sale_return|purchase|purchase_return|collection|payment|other`), `GET /customers/document?code=&key=`
     (fatura kalemleri). **Cari kodu sorguda taşınır, yolda değil:** Mikro kodları `/` içerebilir. Kaynak `PortalLedger`:
     "customers" (`customers/customerAddresses`), "ledger" (`customerTransactions`), "lines" (`stockTransactions`) aynaları.
     **Tür eşlemesi:** Mikro `type` (SATIS, SATIS_IADE, ALIS, ALIS_IADE, TAHSILAT, TEDIYE, HAREKET — `MikroDbReader`
     `cha_evrak_tip/cha_normal_Iade`'den türetir: `0+iade` = SATIS_IADE, `63+iade` = ALIS_IADE). **Kapalı (peşin)
     fatura** (`kapali=true`) müşteriye `ciroCariKod` ile bağlanır ve ekstreye/yürüyen bakiyeye girmez (Y0e), ERP'siz `type` Türkçe (Satış, İade, Alış, Tahsilat, Tediye, İade
     Ödemesi) — **büyük harfe tr-TR kültürüyle çevrilir**, InvariantCulture "ı"yı değiştirmez ve "Satış" eşleşmez. Tutar
     `tutar ?? meblag ?? amount`, yön `borcMu ?? tip==0`. **Belge anahtarı:** Mikro `r{cha_recno}` = satırların
     `faturaRecno`'su; ERP'siz `d{CARİ}|{evrakNo}` (satış ile peşin tahsilatı aynı `evrakNo`'yu taşır, yalnız satış/iade/alış
     türleri açılır). Satırı aynada olmayan fatura da açılır: boş liste + `linesAvailable=false` (Codex #62). Aynı gün Mikro hareketleri `cha_recno`'ya **sayı olarak** sıralanır ("2" < "10"). **Yürüyen bakiye kart bakiyesine sabitlenir:** devir =
     kart bakiyesi − `from`'dan bugüne hareketler; böylece ERP'siz `openingBalance` (hareket olarak yazılmaz) ve eksik
     geçmiş de doğru biter. Tür filtresi bakiye kolonunu değiştirmez.
     Varsayılan fiyat listesi 1 (yoksa en küçük). Ölçüm (SQLite, 20.000 ürün + 200.000 hareket): ilk yükleme ~2,4 sn,
     değişmemişken ~0,17 sn, tek satış sonrası ~0,35 sn; bellek ~130 MB. Eski `/portal/stock` (200 satır) sözleşme için
     duruyor, panel kullanmıyor.
   - **Hangi belge sayılır:** ERP'siz firmada yalnızca `Succeeded`; ERP'li firmada
     `Pending/Processing/Succeeded` (ajana yolda olan da satıştır), `Failed/DeadLetter`
     asla. İade türü ERP'sizde `sales_return`; ERP'lide telefonun satırlı `sales_return`'ü (1.5.237+) **ve** eski
     telefonların kasa defteri `return`'ü (bir telefon ikisinden yalnız birini gönderir).
     `approvalKind = purchase` taşıyan tediye sayılmaz (alışa aittir).
   - **İş günü:** yükteki `occurredAt` — telefon iki biçim yazar: ISO (satış) ve
     `dd.MM.yyyy HH:mm` (kasa defteri); saat dilimi olmayan değer İstanbul duvar saati
     sayılır, `Z`'li anlık değer İstanbul'a çevrilir. Okunamazsa sunucunun aldığı an
     (İstanbul günü). **Çevrimdışı tolerans 7 gün:** o günden bir haftadan geç ulaşan
     belge o güne sayılmaz (sorgu penceresi). Test: `PortalReportDatesTests`.
   - **SQLite notu:** EF Core SQLite `DateTimeOffset` karşılaştırmasını çeviremez;
     `MoneyDocumentsAsync` PostgreSQL'de pencereyi SQL'de, SQLite'ta (testler) bellekte
     uygular. Aynı sonuç, production'da tüm geçmiş belleğe çekilmez.
   - **Sınırlar:** `activity` en çok 92 gün; bakiye listesi 500, stok 200 satır
     (`Truncated`). Bakiye **yaşlandırması yoktur** — akışta vade tarihi taşınmıyor;
     "son hareket" gibi bir tahmin yaşlandırma diye gösterilmez.
   - Testler: `PortalRelationalTests` (yetki, firma yalıtımı, ERP'li/ERP'siz sayım, kişi
     bazlı atıf, rota günü, bakiye/stok, çevrimdışı tolerans).

19. **Firma yönetici paneli ayrı bir uygulamadır: `ErpBridge.Portal` (Faz 42, 2026-09-16).**
   Firmanın admini/yöneticisi tarayıcıdan telefondaki hesabıyla girer. `ErpBridge.Admin`
   operatör konsoludur (tüm firmalar, süper admin) — **ikisi birleştirilmez**, panel Admin'in
   kodunu veya token tutucusunu kopyalamaz.
   - **Oturum devre (circuit) başınadır:** `PortalSession` `Scoped` kayıtlıdır, asla
     `Singleton` değil — tek süreç birçok firmaya hizmet eder; singleton bir firmanın
     token'ını tüm ziyaretçilere sızdırır (test: `PortalSessionTests.Two_circuits_never_share_a_session`).
   - **"Beni hatırla" (Faz 46):** `Session/ProtectedBrowserPersistence` oturumu Data Protection ile
     şifreleyip ya sekmenin `sessionStorage`'ına (kutu boş: sekme kapanınca biter, token 12 saat) ya da
     tarayıcının `localStorage`'ına (kutu işaretli: token 30 gün) yazar. Bir depoya yazmak diğerini
     siler — sonradan "hatırlama" seçilmeden yapılan giriş tarayıcıda 30 günlük token bırakmaz. Okuma
     önce sekmeye, sonra tarayıcıya bakar; **süresi dolmuş kayıt döndürülmez, silinir** — yoksa başka
     sekmede "Beni hatırla" ile açılmış geçerli oturumu sayfa temizlerdi (Codex, PR #51). Çıkış ikisini de temizler. Token sunucu diskine/DB'ye
     yazılmaz. Okunamayan değer (anahtar değişti) oturumsuz sayılır. Saklanan durum `Roles[]` ve
     `RememberMe` taşır; bu alanlar olmadan kaydedilmiş eski durum tek `Role`'e düşer.
   - **Giriş:** `POST /api/v1/android/account/login`, `deviceId = "web-portal:" + kullanıcı adı`
     (kullanıcı başına tek cihaz satırı: operatör paneli girişini telefon gibi engelleyebilir,
     her girişte yeni cihaz birikmez). Panel cihazı da koltuk/cihaz sayımına girer.
     Yalnız `SALES` rolü olan hesap girişte reddedilir (sunucu 403 `PORTAL_REQUIRES_MANAGER`, istemci de
     rol setine bakar; panelde "Saha hesapları telefonda çalışır" metni) ve portal uçları aynı kodu
     döner (sunucu — asıl kapı budur).
   - **Rol bazlı arayüz (Faz 46):** `Session/PortalRoles` sunucudaki `RolePermissions`'ın panel
     karşılığıdır; menü yalnız sunucunun vereceği bölümleri gösterir. Her sayfa
     `PortalPageBase.Requires` ile bir `PortalArea` bildirir: `Reports` (Özet, Plasiyerler, Ziyaretler —
     ADMIN, MANAGER), `Ledger` (Cariler, Stok — + ACCOUNTING), `Approvals` (Onaylar — ADMIN, MANAGER,
     ACCOUNTING), `Warehouse` (Depo — ADMIN, MANAGER, WAREHOUSE), `Users` (Kullanıcılar — ADMIN), `Targets` (Hedef takibi,
     Hedef girişi, Bölge ve ekipler — ADMIN, MANAGER; GOAL_HEDEF_RUT, kural 31). Rolün
     açmadığı adres (yer imi, elle yazılan URL) API'ye hiç sormadan kullanıcının **açılış sayfasına**
     gider: raporları görebilen `/`, muhasebe `/muhasebe` (Faz 48), depo `/depo`. Girişten sonra da oraya gidilir.
     `PortalRoles` ile `RolePermissions` birlikte değişir; panel yalnız kolaylıktır, kapı sunucudadır.
   - **Roller tazelenir (Faz 46):** tarayıcıdan geri yüklenen oturumun rolleri günler öncesine ait olabilir.
     `PortalPageBase`, roller bir dakikadan eskiyse (`PortalSession.RoleRefreshInterval`; girişte taze sayılır)
     sayfa kapısından **önce** `GET /api/v1/android/account/me` okur, oturumu ve saklanan durumu günceller;
     menü `Changed` ile yeniden çizilir. `/me` 403 `PORTAL_REQUIRES_MANAGER` dönerse (panel rolü kalmadı)
     oturum kapanır, `login?reason=PORTAL_SALES_ONLY`. Ağ/diğer hata eldeki rollerle devam eder.
   - **Oturumu bitiren kodlar** (`INVALID_TOKEN`, `USER_INACTIVE`, `DEVICE_REVOKED`,
     `SUBSCRIPTION_*`, `TENANT_INACTIVE`, `SESSION_REVOKED`, her 401) sekmeyi temizleyip
     `login?reason=<kod>`'a döner; diğer retler oturumu korur ve Türkçe mesaj gösterir
     (`PortalMessages`, telefonun `AccountRepository.messageFor` metinleriyle aynı).
   - **Yeni sunucu ucu yoktur:** panel kural 18'deki portal uçlarını + mevcut
     `/api/v1/android/approvals` ve `/api/v1/android/account/users` uçlarını kullanır.
     Yetki kuralları (onaylayıcı, son admin, koltuk) sunucudadır; panel onları tekrar yazmaz.
   - **Dağıtım:** `Dockerfile.portal` (port 4003, yalnız `CentralApi__BaseUrl` gerekir, gizli
     anahtar yok). Coolify'da ayrı uygulama `lisans-portal` →
     `https://panel.admin.lisans.appsgo.cloud` (2026-09-16; main'e push'ta otomatik dağıtılır).
     `docker-compose.coolify.yml`'da yoktur. Cloudflare kaydı **DNS only** olmalı: iki seviyeli
     alt alan adı Cloudflare'in ücretsiz sertifikasına girmez, proxied olursa TLS kırılır.
   - **Oturum şifreleme anahtarları kalıcıdır (Faz 44):** `Session/PortalDataProtection`,
     `DataProtection:KeysPath` (imajda `/app/keys`). Coolify'da bu yola **adlandırılmış kalıcı
     volume** bağlıdır. Yol yoksa anahtarlar konteynerin kendi dosya sisteminde kalır ve her
     dağıtım tarayıcılarda saklı tüm oturumları okunamaz yapar (herkes çıkış yapar);
     Production'da bu durumda açılışta uyarı loglanır. Yol imajda hep dolu olduğu için volume
     bağlanmamışsa da uyarı verilir: `IsMountPoint` Linux'ta `/proc/self/mountinfo`'da bu yolun
     ayrı bir bağlama noktası olup olmadığına bakar. Dockerfile'a `VOLUME` yazılmaz: anonim
     volume her yeni konteynerde sıfırdan açılır. `ApplicationName` sabittir (`ErpBridge.Portal`).
   - **Arayüz: MudBlazor (MIT, Faz 43).** Tema `Shared/PortalTheme.cs`, kurumsal katman
     `wwwroot/css/site.css`; ortak parçalar `Shared/PageHeader`, `StatCard`, `EmptyState`,
     `PageLoading`. Varlıklar `_content/MudBlazor` altından kendi sunucumuzdan gelir; web fontu
     indirilmez (firma tarayıcısından üçüncü tarafa istek yok). Tablolar `MudSimpleTable` —
     satırlara `data-*` verilebilsin diye (testler bunlara bakar). `MudText Color.Secondary`
     marka ikincil rengi (turkuaz) demektir; soluk metin için `Class="text-muted"`.
     Giriş düğmesi alanlar boşken pasifleştirilmez: pasif gönder düğmesi tarayıcının Enter ile
     gönderimini engeller; boş alan kontrolü `SignInAsync` içinde yapılır.
     bUnit: `PortalPageTestContext` (MudBlazor `PopoverService` yalnız async dispose edilebilir),
     `AddMudServices` + `JSInterop.Mode = Loose` + `MudPopoverProvider`.
   - **Yerel çalıştırma:** `dotnet run` ile `--environment Development` verilmeli; Production
     ortamında `dotnet run` çerçeve betiğini (`_framework/blazor.server.js`) sunmaz (404, boş
     sayfa). Yayımlanmış imajda dosya `wwwroot/_framework` altındadır, sorun yoktur.
   - **Sayfalar:** Özet (`/`), Plasiyerler (aralık ≤ 92 gün, sunucuya sormadan reddedilir),
     Ziyaretler (`?date=`), Cariler (yaşlandırma yok — sayfada açıkça yazar), Stok (tükenenler
     filtresi), Onaylar (onay yetkisi yoksa salt görüntüleme; "başkası sonuçlandırdı" kodlarında
     liste yeniden okunur), **Onay masası** (`/muhasebe`, Faz 48, aşağıda), Depo (Faz 46'da yer tutucu; sipariş kuyruğu plan adım 6),
     Kullanıcılar (yalnız `ADMIN`; `Shared/RolePicker` ile çoklu rol çipleri ve rol açıklamaları,
     hem eklemede hem "Roller" ile düzenlemede `roles[]` gönderir — tek `role` göndermez; "Onay
     verebilsin" yalnız yönetici rolünde görünür. **Listedeki `canApprove` etkin haktır** (admin ve
     muhasebe için bayraktan bağımsız true); yöneticinin kendi bayrağı değildir. Bu yüzden düzenleyici
     anahtara dokunulmadıkça `canApprove: null` (sunucuda değişmez) gönderir; admin/muhasebe rolü olan
     kişide anahtar boş başlar ve "dokunmazsanız mevcut ayar korunur" yazar, "onaylayabilir" rozeti de
     yalnız bayrağın bilinebildiği kişide görünür (Codex, PR #51: eskisi rolleri değiştirmeden kaydetmekle
     muhasebeci yöneticiye tüm türleri onaylama yetkisi veriyordu); kişi kendini devre dışı bırakamaz ve kendi rollerini
     değiştiremez — kendini kilitlememesi için; son admin kuralı sunucudan `LAST_ADMIN` olarak gelir;
     satın alma metni yok).
   - Testler: `tests/ErpBridge.Portal.Tests` (bUnit) — oturum yalıtımı, rol kapısı, oturum
     bitişi, sayfa davranışları; `PortalRolesTests` (rol → bölüm/açılış sayfası, eski oturum biçimi,
     hangi deponun kullanıldığı — sahte `IJSRuntime` ile gerçek `ProtectedLocalStorage`/`ProtectedSessionStorage`),
     `PortalLayoutTests` (menü). Menü testleri `Register(..., popoverProvider: false)` ister: layout kendi
     `MudPopoverProvider`'ını getirir, ikincisi hata verir.
   - **Onay masası (`Pages/Muhasebe.razor`, Faz 48, plan adım 5):** muhasebe (ve onay bölümü açık her rol)
     için klavye odaklı ekran. Solda bekleyen talepler **en eski üstte** (tür, müşteri, plasiyer, tutar, bekleme;
     30 dk sarı, 2 sa kırmızı), sağda detay: belge kalemleri (`documents[].payload.lines`), ödemeler
     (`summary.payments`), ödeme türü/açıklama (`summary`), stok uyarıları, cari bakiye (`/portal/balances?search=
     customerCode`, `Ledger` bölümü açıksa) ve geçmiş. Kuyruk `approvals?status=pending&order=oldest&take=500` ile bir
     kerede okunur — `order=oldest` olmadan 500'den fazla bekleyende en eskiler sayfanın dışında kalırdı (Codex, PR #57).
     **Tür bazlı yetki:** liste her satırda `canDecide` taşır (`ApprovalPermissions.CanDecide(kullanıcı, tür)`); masa
     karar veremediği talepte düğme göstermez, `A`/`R` reddedilir, satır işaretlenmez ve toplu onaya girmez.
     Kısayollar: `↑↓`/`j k` gezin · `A` onayla (varsa notuyla) · `R` red penceresi (not) · `N` not · `Boşluk` işaretle
     · `Shift+A` işaretlileri onay penceresiyle onayla · `/` ara · `F` tür filtresi · `Esc` kapat/işaretleri temizle
     · `?` yardım. Onay yetkisi yoksa (`Session.CanApprove`) salt görüntüler. Karardan sonra sıradaki talebe geçer;
     `APPROVAL_ALREADY_DECIDED`/`STATE_CHANGED`/`NOT_FOUND` kuyruğu yeniden okur.
     - **Klavye:** `wwwroot/js/portal-keys.js` tek belge dinleyicisi; alanda yazarken yalnız `Esc` ve tek satırlık
       alanda `Enter` geçer, odaklı düğmenin Enter/Boşluk'u düğmede kalır. Pencere açılınca odak **pencerenin
       kendisine** (`tabindex=-1`) verilir, düğmeye değil — Enter her tarayıcıda kısayol olarak işlensin.
       `attach` bir tutamaç döner, `detach(tutamaç)` yalnız kendi dinleyicisini söker. `e.code === 'Space'` de Boşluk sayılır.
     - **Canlı:** sayfa `GET /api/v1/portal/events?approvalsVersion&wait=25` long-poll döngüsü tutar (aralarda en az 1 sn,
       hatada 15 sn); `changed` gelince kuyruğu yeniden okur. İmleç satırlardan **önce** alınır. Açık talebi başkası
       sonuçlandırdıysa "başka bir yetkili" bilgisi ve sıradaki talep. Kararlar ve yeniden okumalar bir `SemaphoreSlim`
       ile sıraya girer.
     - **Tuzak — sayfa kopyası yükleme ortasında atılır:** tarayıcıdan geri yüklenen oturumda `PortalPageBase`
       `Session.SignIn` yapınca `MainLayout` oturum açık görünümüne geçer ve `@Body`'deki sayfa **yeni bir örnekle
       değiştirilir**; ilk örneğin `LoadAsync`'i hâlâ sürerken `Dispose` çağrılır. Uzun ömürlü iş başlatan sayfa
       (döngü, JS dinleyicisi) `_disposed` bayrağına bakmalı ve iptal kaynağını dispose etmemeli, yalnız iptal etmeli —
       aksi hâlde `ObjectDisposedException` devreyi düşürür (Faz 48'de tarayıcıda yakalandı). Diğer sayfalar bu yüzden
       ilk açılışta veriyi iki kez okuyabilir (bilinen verimsizlik).
     - **Onay listesi uç parametreleri (Faz 48):** `/api/v1/android/approvals` `order=newest|oldest` (varsayılan
       newest, geçersiz 400 `INVALID_ORDER`) ve her satırda `canDecide`; telefonun kullandığı çağrı değişmedi.
     - Testler: `PortalApprovalDeskTests` (10 talep klavyeyle en eskiden başlayarak, ok + not + red, işaretle + toplu onay,
       başkası sonuçlandırmış 409, canlı güncelleme, arama + filtre, yetkisiz salt görüntü, yardım). Kuyruk sırası
       bozulunca 8 testin 6'sı kırıldı.
   - **Tarayıcıda deneme notu:** Claude'un tarayıcı bölmesinde "Enter" tuşu düz bir HTML formunu da
     göndermiyor (araç kısıtı); girişi denemek için "Giriş yap" düğmesine tıklanır. Tuş adı olarak `Return` ve `space` boş
     `key` üretir; `Enter` çalışır, Boşluk bu araçla denenemez.

20. **Şema durumu görünürdür: `GET /health/schema` (Faz 44, 2026-09-16).**
   Konteyner açılışta `--migrate` çalıştırır ama başarısız olursa uygulama **yine açılır** ve eski
   şemayla çalışır (2026-09-09'da canlı bir hafta 8 migration geride kaldı).
   - `Health/SchemaStatus`: uygulanan ve bekleyen migration sayısı. `/health/schema` bekleyen varsa
     **503** `{status:"pending", applied, pending}`, yoksa 200 `current`; bellek içi test sağlayıcısında
     `not-relational`. Migration adı dışarı verilmez.
   - Açılışta bekleyen migration varsa `LogCritical` ("DATABASE SCHEMA IS BEHIND").
   - Bu uç **konteynerin sağlık kontrolüne bağlanmaz**: orkestratör uygulamayı yeniden başlatır ama
     başarısız migration'ı uygulamaz, sonuç yeniden başlatma döngüsü olur. Migration içeren her
     dağıtımdan sonra `https://lisans.appsgo.cloud/health/schema` 200 `current` dönmelidir.
   - Testler: `HealthSchemaTests` (SQLite'ta geçmiş tablosu boş → pending; tüm migration kayıtlı → current).

21. **Depo sipariş hazırlığı sunucudadır: `Warehouse/FulfillmentService` (Faz 47, 2026-09-16).**
   Satış siparişleri depoda hazırlanır, paketlenir, araca yüklenir; panel depo sayfası (plan adım 6),
   TV panosu (adım 7) ve performans raporu (adım 8) bu çekirdeği okur. Plan: `docs/PLAN_ROLLER_VE_DEPO.md`.
   - **Modül firma bazında açılır** (`tenant_warehouse_settings.Enabled`, varsayılan kapalı, V1). Kapalı
     firmada hiçbir satış kuyruğa girmez; açılmadan önceki satışlar kendiliğinden eklenmez.
   - **Geri doldurma (panel goal P4a):** `POST /api/v1/portal/warehouse/backfill {days}` (varsayılan 2, en çok 30;
     `CanManageWarehouse`, modül kapalıysa 409 `WAREHOUSE_DISABLED`, aralık dışı 400 `INVALID_BACKFILL_DAYS`) son
     günlerin kuyrukta olmayan `sales_order` job'larını `EnqueueAsync` ile ekler — tek transaction, en çok 2000.
     `QueuedAtUtc` = job'ın geliş anı. Native'de yalnız `Succeeded`; ERP'de `ErpState` job durumundan (Succeeded →
     WRITTEN, Failed/DeadLetter → FAILED, diğer → PENDING). Onay bekleyen/reddedilen talep job değildir, girmez.
     İdempotent; `{days, queued}` döner. Firma sayaç kilidi **aday seçiminden önce** alınır (eşzamanlı iki doldurma
     UNIQUE'e çarpmaz); tür filtresi harfe duyarsız (`SALES_ORDER` da); PostgreSQL'de sınır SQL'de (Codex #63).
   - **Panel `/depo` (panel goal P4d):** sekmeler Bekleyenler/Hazırlanıyor (yalnız benimkiler: `AssigneeName ==` oturum adı)
     /Paketlenenler/Bugün yüklenenler (`status=loaded&newest=true&take=200`, İstanbul günü süzülür); kartta adıma göre
     geçen süre firma eşikleriyle sarı/kırmızı, tek dokunuşla Başla → Paketlendi → Araca yüklendi; detayda toplama listesi
     (işaretler yalnız tarayıcıda), olaylar, plaka, Geri al, yöneticiye İptal, admin'e Yeniden ata (kullanıcı listesi admin
     ucu). Canlı: `events?sinceSeq&wait=25`, imleç listelerden önce okunur; hata olursa 15 sn sonra tekrar. 409'da liste
     yenilenir ve "X siparişi az önce … durumuna aldı" gösterilir. Modül kapalıyken yönetici gün sayısıyla açar (ayar PUT +
     backfill); aynı teklif `/ekranlar` ayar formunda modül açılırken çıkar (P4c). Liste ucu `newest=true` isteğe bağlı
     parametresi: durum listesini `UpdatedSeq` azalan sıralar.
   - **Kuyruğa alma tek yerde:** `EnqueueAsync(db, tenant, job, approvalRequestId)` yalnız `sales_order`
     için, **belgeyi yazan transaction'ın içinde** çağrılır ve kaydetmez; çağıran commit'ten sonra
     `Notify(tenantId)` der. Çağıranlar: `/ingest/jobs` native dalı (uç kendi transaction'ını açar,
     `NativeDocumentProcessor` ona katılır — bu yüzden telefonları uyandıran `IBootstrapNotificationHub.Publish`
     de uçtadır), `/ingest/jobs` ERP dalı (job `Pending` ile aynı transaction), `ApprovalService.PostDocumentsAsync`
     (onaylanan satış, `ApprovalRequestId` dolu). Native'de yalnız `Succeeded` belge; ERP'de ajan yazmadan
     önce (V2). **İdempotent:** `(TenantId, SourceJobId)` UNIQUE; telefonun tekrar gönderdiği ya da onayda
     "zaten sunucuda" atlanan belge ikinci kez kuyruğa girmez.
   - **Durumlar:** `PENDING → PREPARING → PACKED → LOADED`; `CANCELLED` yalnız açık siparişten.
     Eylemler `POST /api/v1/portal/fulfillments/{id}/start|pack|load|undo|cancel|reassign`
     (`{note, vehiclePlate, assigneeUserId}`). **Tek kazanan:** her geçiş önce sayaçtan seq ayırır, sonra
     `ExecuteUpdate … WHERE Status = <beklenen>` (reassign'da `UpdatedSeq` de) — 0 satır = 409
     `FULFILLMENT_STATE_CHANGED` ("… by <ad>"). Sayaç kilidi satır kilidinden önce alınır; ERP sonucu
     da aynı sırayı izler, kilitlenme olmaz.
   - **Geri alma (V6):** `PREPARING→PENDING` (atanan ve başlama silinir) ve `PACKED→PREPARING`. Adımı atan
     kişi 5 dakika içinde (`UndoWindow`, adımın olayına bakılır) ya da ADMIN/MANAGER; aksi 403
     `UNDO_NOT_ALLOWED`. Başka durumda 409 `FULFILLMENT_CANNOT_UNDO`.
   - **Yetki (rol birleşimi, kural 14):** okuma, `events` ve adımlar `CanOperateWarehouse` (ADMIN, MANAGER,
     WAREHOUSE) → 403 `WAREHOUSE_ROLE_REQUIRED`; iptal, yeniden atama ve ayar yazma `CanManageWarehouse`
     (ADMIN, MANAGER) → 403 `WAREHOUSE_MANAGER_REQUIRED`. Yeniden atanan kişi firmada aktif ve depo rolü
     olan biri olmalı (400 `INVALID_ASSIGNEE`). Başka firmanın siparişi 404 `FULFILLMENT_NOT_FOUND`.
   - **Olay günlüğü:** `order_fulfillment_events` yalnız eklenir (QUEUED, START, PACK, LOAD, UNDO, CANCEL,
     REASSIGN, ERP_FAILED); aktör, cihaz (token `device`), not, **sunucu saati**. Özet satır ile günlük
     çelişirse günlük esastır; raporlar (adım 8) günlükten hesaplanır.
   - **ERP durumu (V2):** `ErpState` NONE (native) | PENDING | WRITTEN | FAILED. Ajanın `/jobs/ack`'i ve
     konsolun `/admin/jobs/{id}/retry`'ı `RecordErpResultAsync` çağırır (aynı transaction); başarısızlık
     `ERP_FAILED` olayı + hata metni. ERP hatası depo akışını durdurmaz.
   - **Okuma:** `GET /api/v1/portal/fulfillments?status=open|all|<liste>&changedSinceSeq&take≤500` →
     `{latestSeq, hasMore, items}`. `changedSinceSeq` verilince **durumdan bağımsız** değişen her sipariş
     `UpdatedSeq` sırasıyla döner (sayfa listeden düşeni de görsün) ve **`latestSeq` dönen son satırdır** —
     `take` dolarsa `hasMore` true, kalan hemen aynı imleçle istenir; boş sayfada imleç yerinde kalır
     (tablonun en büyük değeri dönülseydi sayfaya sığmayanlar kalıcı atlanırdı — Codex, PR #52). Durum
     listesinde `latestSeq` satırlardan **önce** okunur (arada commit olan değişiklik tekrar gelir, kaybolmaz);
     kuyruk sırası `QueuedSeq`
     (`DateTimeOffset` SQLite'ta sıralanamaz). `GET /{id}` → sipariş + toplama listesi (`ItemsJson`:
     `stockCode, name, quantity, unit`) + olaylar.
   - **Canlı akış (V5):** `GET /api/v1/portal/events?sinceSeq&approvalsVersion&wait=0-25` — değişiklik varsa hemen, yoksa
     `ITenantEventHub` ile bekler, `{latestSeq, changed}` döner; sayfa sonra `changedSinceSeq` okur.
     **Bekleyici okumadan önce kaydolur** (`WaitAsync` dönmeden kuyruğa girer): okuma ile bekleme arasında
     commit olan değişiklik de uyandırır (Codex, PR #52).
     **Onay konusu (Faz 48):** `ITenantEventHub.Publish(tenant, topic)` konu başına (`TenantEventTopics.Warehouse`,
     `Approvals`) bellek içi bir **sürüm** artırır; `ApprovalService` her değişiklikte (gönderim, karar, tekrar açma,
     geri çekme, kurallar) `Approvals` yayınlar, onaylanan satışta ayrıca `Warehouse`. `approvalsVersion` gönderen
     istek, sürüm farklıysa (büyük ya da süreç yeniden başladıysa küçük) hemen `changed` döner; `-1` "hiç
     görmedim" demektir. Onayların `UpdatedSeq`'i milisaniye olduğundan (kural 16) karşılaştırma için kullanılmaz:
     iki long-poll arasında, bekleyen yokken yayınlanan değişiklik de sürümden anlaşılır (Codex, PR #57). Uç panel
     rolü olan herkese açık (`CanUsePortal` veya depo); depo konusu yalnız depo rolüne. Hub
     **bellek içidir ve bootstrap hub'ından ayrıdır** (depo tıklaması telefonları senkrona uyandırmaz);
     CentralApi tek konteyner varsayar — yatay ölçek PostgreSQL LISTEN/Redis ister.
   - **Ayarlar:** `GET|PUT /api/v1/portal/warehouse/settings` (açık/kapalı + gecikme eşikleri dakika,
     1–1440, uyarı < kritik).
   - **Siparişteki alanlar:** `OrderNo` = `mobileDocumentId` (yoksa job `ExternalId`), müşteri
     `customerCode` + `counterparty`, plasiyer = job `CreatedByUserId` (ad anlık kopya), satırlar
     `productCode|stockCode|barcode`, `productTitle|name`, `quantity`, `unit`.
   - Testler: `WarehouseFulfillmentRelationalTests` (SQLite; modül kapalı, idempotent kuyruk, onaydan kuyruk,
     eşzamanlı başla → bir 409 — kilit kaldırılınca kırıldığı doğrulandı, tüm adımlar + günlük, geri alma
     penceresi, yönetici işlemleri, yetki/firma yalıtımı, ayar doğrulaması, ERP başarısız/yeniden dene/yazıldı,
     long-poll).

22. **Depo TV panoları eşleştirme koduyla bağlanır: `Endpoints/DisplayEndpoints` + panel `/ekran` (Faz 49, 2026-09-17).**
   Plan adım 7. TV bir kişi değildir: **koltuk harcamaz**, kullanıcı token'ı taşımaz, yalnız panoyu okur.
   - **Eşleştirme:** TV `POST /api/v1/display/pairings` (anonim, `AnonymousRateLimitPolicy`) ile 6 haneli kod +
     gizli anahtar alır, kodu gösterir ve 3 sn'de bir `POST /display/pairings/{code}/token {secret}` yoklar
     (`waiting` → `paired`). Yönetici panelde `POST /api/v1/portal/displays {code, name}` (ADMIN/MANAGER,
     `CanManageWarehouse`; kod boşluklu yazılabilir) ile sahiplenir. TV'nin sonraki yoklaması `scope=display`
     token'ı **bir kez** alır, kod satırı aynı kayıtta silinir (ikinci yoklama 404). Yanlış gizli anahtar
     bilinmeyen kodla aynı cevabı alır (404 `PAIRING_NOT_FOUND`); süresi dolmuş sahiplenilmemiş kod 410
     `PAIRING_EXPIRED`. **Sahiplenme tek kazananlıdır:** `ExecuteUpdate … WHERE Code = @kod AND DisplayDeviceId IS NULL`
     ile ekran satırı aynı transaction'da; aynı kodu aynı anda giren ikinci yönetici 404 alır, boşta ekran kalmaz
     (Codex, PR #61). Kodlar 10 dk yaşar; yeni kod üretilirken süresi dolanlar silinir (tablo küçük olduğu
     için bellekte süzülür — SQLite `DateTimeOffset` karşılaştıramaz). Gizli anahtar yalnız SHA-256 olarak saklanır.
   - **Token:** `JwtIssuer.IssueForDisplay`: `sub=displayDeviceId`, `tenant`, `scope=display`, 365 gün.
     `Program.DisplayPolicy` yalnız bu kapsamı kabul eder; başka hiçbir uç (portal, telefon) display token'ı
     kabul etmez. Her pano çağrısı `display_devices` satırına bakar: iptal edilmiş, silinmiş ya da firması pasif
     ekran 401 `DISPLAY_REVOKED`. **Abonelik de kontrol edilir** (`MobileSeatService.SubscriptionStatus`/`AllowsWork`,
     kullanıcılarla aynı kural): güncel aboneliği olmayan firmanın ekranı 403 `SUBSCRIPTION_EXPIRED|REQUIRED` alır —
     401 değil, çünkü ekran eşleşmiş kalır ve süre yenilenince kendiliğinden açılır (Codex, PR #61).
     `LastSeenAtUtc` en çok dakikada bir yazılır.
   - **Pano uçları:** `GET /api/v1/display/board` → firma ve ekran adı, depo ayarları (eşikler), `latestSeq`
     (satırlardan önce okunur), `serverTimeUtc`, açık siparişler **durum başına en çok `MaxCardsPerColumn` (200)**
     (`QueuedSeq` sırası; uzun bekleyen kuyruğu hazırlananları panodan itmesin) ve `counts` (durum başına gerçek
     toplam; kolon başlığı bunu gösterir — 500'de sessizce kırpılmaz, Codex PR #61).
     `GET /api/v1/display/events?sinceSeq&wait=0-25` depo konusunun long-poll'u (kural 21). **Hız sınırı ekran
     başınadır** (`PerDisplayRateLimitPolicy`, 60/dk): firma başına ortak `PerTenantRateLimitPolicy` (100/dk,
     telefon + portal paylaşır) duvardaki panolarla tükenmesin.
   - **İptal:** `POST /api/v1/portal/displays/{id}/revoke` depo konusuna yayın yapar; bekleyen pano uyanır, bir
     sonraki çağrısı 401 alır → TV saklı eşleştirmesini siler ve yeni kod gösterir (tarayıcıda ~4 sn ölçüldü;
     en kötü durum bir long-poll, 25 sn). `GET /api/v1/portal/displays` liste (ad, eklenme, son görülme, iptal).
   - **Panel `/ekran` (`Pages/Ekran.razor`, `KioskLayout`):** oturum yok, menü yok. Eşleştirme
     `IDisplaySessionStore` (`ProtectedLocalStorage`, anahtar `display-session`) ile saklanır; güç kesilen TV
     koda dönmeden panoya açılır. Pano: koyu tema, kolonlar **Bekliyor** (kuyruğa girişten), **Hazırlanıyor**
     (başlamadan), **Paketlendi** (paketlemeden; yalnız sarı). Eşik geçen kart sarı, kritik kırmızı ve yavaş
     yanıp söner (`prefers-reduced-motion` ile durur); kolon başında sayı ve "N geciken"; `CardsPerPage` (6)
     aşılınca `Rotate` (10 sn) aralıkla sayfa döner. Süreler API saatine göre (`serverTimeUtc` farkı). Bağlantı
     koparsa son pano kalır, kırmızı nokta + şerit, 10 sn'de bir dener. Eşleştirme yoklamasında yalnız 404/410
     kodu yeniler; 429/5xx'te kod korunur ve beklenir — tüm TV'ler panel sunucusunun tek IP'sinden 60/dk anonim
     sınırı paylaştığı için hemen yeni kod istemek sınırı kilitli tutardı (Codex, PR #61). Abonelik 403'ünde pano
     verisi silinir, "Pano şu an kapalı" gösterilir, eşleştirme korunur (`KioskMode.Suspended`). `wwwroot/js/portal-kiosk.js`: Wake Lock
     ve Blazor bağlantısı kalıcı düşünce (`components-reconnect-failed/rejected`) sayfayı yeniden yükleme.
     Zamanlamalar `KioskTiming` servisinden (testler kısaltır).
     - **Tuzak:** arka plan döngüsünden değişen durum sayfayı kendiliğinden yeniden çizmez; saat tiki 5 sn
       olduğundan kod ekranı o kadar "Kod alınıyor" kaldı (Faz 49'da tarayıcıda yakalandı). Görünür her durum
       değişikliğinden sonra `InvokeAsync(StateHasChanged)`. bUnit'te `WaitForAssertion` yalnız render'da tekrar
       kontrol eder — render tetiklemeyen koşul (ör. istek sayısı) `SpinWait.SpinUntil` ile beklenir; bUnit render'dan
       sonra servis eklemeye izin vermez (`PortalTestSetup.Register(..., kioskTiming:)`).
   - **Panel `/ekranlar` (`Pages/Ekranlar.razor`, `PortalArea.Displays` = ADMIN, MANAGER):** TV adresi
     (`{panel}/ekran`), kod + ad ile eşleştirme, ekran listesi (Açık = son 3 dk içinde görüldü, Görülmüyor,
     İptal edildi) ve iptal; depo modülü anahtarı ve eşikler (`PUT /portal/warehouse/settings`).
   - **Dikkat — paylaşılan sınırlar:** panel Blazor Server olduğundan API'ye tüm istekleri panel konteyneri atar;
     IP bazlı genel sınır (1000/dk) ve anonim eşleştirme sınırı (60/dk) tüm panel kullanıcıları/TV'ler arasında
     paylaşılır. Portal uçlarının firma başına 100/dk sınırı çok kullanıcılı firmada yetmeyebilir (ayrı iş).
   - Testler: `WarehouseFulfillmentRelationalTests` (eşleştirme → tek seferlik token → yalnız kendi firmasının
     panosu → portal/telefon uçlarına kapalı → iptal; süresi dolmuş kod; pano long-poll'u "başla" ve iptal ile
     uyanır), `PortalKioskTests` (kod → pano, kolon ve renkler, sayfa dönüşü, canlı değişiklik, bağlantı kaybı,
     iptal → yeni kod), `PortalDisplaysPageTests`.

23. **Depo raporları yalnız olay günlüğünden hesaplanır: `Warehouse/FulfillmentMetrics` + `FulfillmentReports` (Faz 50, 2026-09-17).**
   Plan adım 8. `order_fulfillments` satırındaki `StartedAtUtc/PackedAtUtc` **rapor için okunmaz** (geri almada
   silinir/üzerine yazılır); her süre `order_fulfillment_events` üzerinden `FulfillmentMetrics.Measure` ile çıkar
   (olaylar `Id` sırasında). Tanımlar:
   - **Bekleme** = ilk `START` − `QUEUED`. Yanlışlıkla başlatılıp geri alınan sipariş de o anda ele alınmış sayılır.
   - **Net hazırlama** = `PREPARING`'de geçen ve `PACKED` ile biten aralıkların toplamı. `UNDO` (→ PENDING) ya da
     `CANCEL` ile biten aralık sayılmaz; `PACKED → PREPARING` geri almasından sonra ek hazırlama eklenir, paketli
     beklenen süre eklenmez. Durumu değiştirmeyen olaylar (`REASSIGN`, `ERP_FAILED`) aralık açıp kapatmaz.
   - **Yüklemeye kadar** = `LOAD` − son `PACK`. Paketleme/yükleme anı ancak sipariş hâlâ PACKED/LOADED ise sayılır;
     sipariş **son paketleyene** yazılır (başlayana değil).
   - **Gün ataması (İstanbul günü, `PortalReports.IstanbulDay/IstanbulDayStartUtc`):** her rakam ölçtüğü olayın
     gününe düşer — bekleme ilk başlamanın, hazırlama geçerli paketlemenin, yükleme yüklemenin, "gelen" `QUEUED`
     olayının, iptal `CANCEL` olayının günü. Aralığa en az bir olayı düşen siparişlerin **tüm geçmişi** okunur
     (dün kuyruğa girip bugün paketlenen doğru ölçülür). Olay tablosuna `(TenantId, OccurredAtUtc)` indeksi.
     SQLite testlerinde tarih süzmesi bellekte (kural 11 tuzağı).
     Geriye dönük doldurulan siparişin (`/warehouse/backfill`) `QUEUED` olayı doldurma anındadır (satırın `QueuedAtUtc`'si
     satışın zamanı): rapor beklemeyi depo siparişi görebildiği andan, yani olaydan ölçer.
   - **Uçlar (ADMIN/MANAGER, `CanViewReports`; değilse 403 `PORTAL_REQUIRES_MANAGER`):**
     `GET /api/v1/portal/warehouse/dashboard?date` → açık sayılar (bekliyor/hazırlanıyor/paketli), **geciken/kritik
     şu an** (TV panosuyla aynı saatler ve eşikler: bekliyor kuyruğa girişten, hazırlanıyor başlamadan, paketli
     paketlemeden; paketlide kritik yok), günün gelen/paketlenen/yüklenen sayısı, ortalama bekleme ve net hazırlama,
     `enabled`. `GET /api/v1/portal/warehouse/performance?from&to` (en çok 92 gün; `INVALID_RANGE`,
     `RANGE_TOO_LONG`, `INVALID_DATE`) → toplamlar, ortalama/ortanca, personel satırları (paketlenen, kalem, adet,
     toplam/ortalama/ortanca net hazırlama, kalem başı = toplam ÷ kalem), günler, en uzun 10 bekleme ve 10 hazırlama.
     Süreler tam saniye. `GET /portal/fulfillments/{id}` yanıtına `times` eklendi (zaman çizelgesi).
   - **Panel:** Özet sayfasında modül açıksa üç depo kartı (açık, geciken, paketlenen/gelen; hata ya da eski
     sunucuda kartlar gizlenir, özet bozulmaz). `/depo-performans` (Genel bakış menüsü, `PortalArea.Reports`;
     `?from&to` kabul eder) ve `/depo-performans/siparis/{id}` zaman çizelgesi (adım, kim, önceki adımdan süre, not).
     `Fmt.Duration` "45 sn / 12 dk / 1 sa 5 dk / 2 gün 3 sa".
   - Testler: `FulfillmentMetricsTests` (düz akış, geri alınan başlama, geri alınan paketleme + yeniden atama,
     iptal, ERP olayı, ortanca), `WarehouseFulfillmentRelationalTests` (+3: bilinen senaryonun rakamları gece
     yarısı sınırıyla, geciken sayımı, yetki/aralık), `PortalWarehouseReportTests` (5).

24. **Loglar tek yoldan yazılır ve iz kimliği taşır: `LogCenter/` (Log Merkezi L0, 2026-09-17).**
   Plan ve kararlar: [`docs/GOAL_LOG_MERKEZI.md`](../docs/GOAL_LOG_MERKEZI.md).
   - **Tek yazım yolu `ILogEventWriter`.** `log_events` / `log_error_groups`'a başka kod yazmaz. Yazıcı seviyeyi
     (`LogSeverity`: DEBUG|INFO|WARN|ERROR|FATAL; "WARNING"→WARN) ve türü (büyük harf ASCII — sunucu kültürüne
     bağlı `ToUpper` kullanılmaz, Türkçe `i` tuzağı) normalize eder, metni `LogScrubber` ile maskeler, alanları
     sınırlar, `(Source, EventId)` tekrarını düşürür, WARN+ olayı `ErrorFingerprint` ile gruba sayar. Kaynaklar
     `LogSources`'taki altı değerden biridir; bilinmeyen kaynak programlama hatasıdır (`ArgumentException`).
   - **Log yazımı iş akışını bozmaz.** Telemetri uçları yeni tabloya yazım hatasını yakalar ve yanıtı değiştirmez;
     yeni bir log yolu eklerken de aynısı yapılır ve "log hedefi çökükken iş sürüyor" testi yazılır.
   - **`X-Correlation-Id`** (`CorrelationId.UseCorrelationId`, hattın ilk ara katmanı): güvenli (`[A-Za-z0-9._:-]`,
     ≤128) istemci kimliği korunur, yoksa GUID üretilir; yanıta yazılır, `HttpContext.TraceIdentifier` olur
     (`ApiError.traceId` bu değeri taşır) ve log kapsamına girer.
   - **Yakalanmamış istisna** (`UnhandledExceptionHandler`): istemciye iç ayrıntısız `500 INTERNAL_ERROR` +
     `traceId`; ayrıntı (rota şablonu, firma, mobil kullanıcı / ajan, tam istisna) **yeni bir DI kapsamında**
     `log_events`'e yazılır — isteğin DbContext'i kaydı başarısız olan varlıkları tutuyor olabilir. Bu sınıfın
     logger kategorisi (`LoggerCategory`) veritabanı logger'ı tarafından atlanmalıdır (çift kayıt). Konsol satırına
     istisna **nesnesi verilmez**, yalnız `LogScrubber`'dan geçmiş metin (konsol sağlayıcısı ham mesajı ve yığını basar).
   - **Saklama** (`LogRetention` + günlük `LogRetentionWorker`, `LogRetention:RunAtHourUtc` varsayılan 03): süreler
     `log_settings` tek satırından (yoksa INFO/DEBUG 14, WARN+ 90 gün; 1–730 arasına sıkıştırılır), **sunucunun
     aldığı zamana** (`ReceivedAtMs`) göre, 10.000'lik partilerle, bant başına çalıştırmada en çok 100.000 satır.
     Son görülmesi WARN süresinden eski ve **hiçbir saklı olayın göstermediği** açık gruplar silinir (grubun son görülmesi
     cihaz saatidir; çevrimdışı telefonun bugün getirdiği eski olay grubunu korur). Çözüldü/yok sayıldı grupları kalır.
     `MaxDeletesPerRun` kesin sınırdır; daha büyük `BatchSize` ona sıkıştırılır.
     Eski `mobile_telemetry_events` kendi 90 günlük işçisiyle temizlenmeye devam eder.
   - Testler: `tests/ErpBridge.CentralApi.Tests/LogCenter/` (maskeleme, normalizasyon, parmak izi, yazıcı SQLite
     testleri, iz kimliği, 500 kaydı, saklama).

25. **Ajan logları koddan kurulur ve maskelenir: `Agent.Service/Logging/AgentSerilog` (Log Merkezi L3a/L3b, 2026-09-17).**
   - **Eski arıza:** servis `ReadFrom.Configuration` ile yalnız `"Serilog"` bölümünü okuyordu, ayarlar ise `"Logging"`
     altındaydı — Windows Servisi **hiç log dosyası yazmıyordu**; masaüstü uygulaması Information'ın altını atıyordu.
   - **Şimdi:** servis ve masaüstü uygulaması aynı `AgentSerilog.Configure`'u kullanır (dosya UI projesine bağlantıyla
     derlenir). Konsol + günlük dönen dosya **kodda** kurulur: servis `agent-YYYYMMDD.log`, masaüstü `ui-YYYYMMDD.log`,
     14 gün, `shared: true`, 1 sn'de diske. `"Serilog"` bölümü yalnız seviyeleri değiştirir (varsayılan Information;
     `Microsoft` ve `System.Net.Http.HttpClient` Warning). Yalnız `Serilog:MinimumLevel` okunur (`AgentSerilog.ApplyLevels`);
     `ReadFrom.Configuration` **kullanılmaz** — yapılandırmadaki bir `WriteTo` maskesiz ikinci bir yazıcı eklerdi.
   - **Konum** (`Core/Logging/AgentLogLocation`): EXE yanındaki `logs\` yazılabiliyorsa orası, değilse
     `%ProgramData%\ErpBridge\logs` (Program Files'a kurulu servis, System32 çalışma dizini), o da olmazsa `%TEMP%`.
     Serilog selflog aynı klasörde `serilog-selflog.txt`.
   - **Maskeleme:** her satır — mesaj **ve istisna metni** — `MaskingTextFormatter` ile
     `ConnectionStringMasker.MaskSecrets`'ten geçer (bağlantı cümlesi parolası/kullanıcısı, bearer, JWT, `AK-`/`LIC-`,
     `token=`/`"licenseKey":`/`apiKey:` adlı değerler). Masaüstü telemetri raporlayıcısı da aynı listeyi kullanır; ajan
     tarafında ikinci bir gizli bilgi listesi yazılmaz. `MaskPassword` değeri artık satır sonunda durur (önceden bir
     sonraki satırdaki istisna türünü yutuyordu); tırnaklı değer (`Password="Top;Secret"`) bütün olarak maskelenir.
   - **Sürüm:** `Directory.Build.props` `VersionPrefix` (1.3.0) tüm derlemelerin sürümüdür; olaylar bunu taşır
     (ajan önceden hep `1.0.0.0` gönderiyordu). Müşteriye yeni ajan derlemesi çıkarken artırılır. 1.2.0
     (2026-09-27): alış faturası iskontosu — heartbeat'teki `appVersion` 1.2.0'dan küçük ajan iskontolu alışı yazamaz.
     1.3.0 (2026-09-28, GOAL_HEDEF_RUT E1/E2): hareket satırlarında `plasiyerKod`, lookups'ta `stock_brand`/`stock_main_group`
     (projeksiyon 8) **ve stok hareketi okumasının onarımı**: #178'den (2026-09-23) beri `ReadStockTransactionsAsync` her canlı
     okumada Dapper kurucu eşlemesiyle düşüyordu (aşağıdaki kolon sırası kuralı). 1.2.0 kurulu müşteride stok hareketleri
     aynaya gelmez; 1.3.0 kurulunca projeksiyon 8 bir kez tam okuma yapar.
   - **Log Merkezi'ne gönderim (L3c):** ajan kodu tanılama olaylarını `Core/Logging/IAgentLogReporter` ile bildirir;
     olay SQLite'taki `agent_log_outbox`'a yazılır (en çok 1.000 / 7 gün) ve heartbeat turunda `AgentLogUploader`
     en çok 50'lik partiyle `POST /api/v1/agents/logs/batch`'e gönderir. Aynı parmak izli hata 10 dakikada bir
     gönderilir, arada yalnız `repeat_count` artar. Raporlama **hiçbir zaman** çağıranın akışını bozmaz: mesaj ve
     istisna metni `ConnectionStringMasker.MaskSecrets`'ten geçer, kuyruk/gönderim hatası yerel logda uyarıdır.
     Eski `POST /api/v1/agents/telemetry` ucu tek olay için duruyor.
   - **Hangi satırlar gidiyor (L3d):** çağrı noktaları tek tek donatılmaz — `AgentLogCentreSink`, ajanın zaten
     yazdığı Serilog akışındaki **WARN ve üstü** her satırı raporlayıcıya verir. Böylece senkron döngüsü,
     bootstrap, change-log, Mikro bağlantı/sürüm, token yenileme, notify, heartbeat, `AgentWorker` + yerel kuyruk
     + ack, mutabakat alarmları ve Polly yeniden denemeleri (WARN) kendiliğinden kapsanır; **yeni bir hata yolu
     eklendiğinde ayrıca bağlamak gerekmez.** Olay türü satırın `Kind` özelliğinden, yoksa sınıf adından türer
     (`BootstrapSyncService` → `BOOTSTRAP_SYNC_SERVICE`); `Operation` ve `CorrelationId` varsa taşınır.
     `AgentLogReporter`/`AgentLogUploader` kendi satırlarını raporlamaz (geri besleme olmasın), kendisi raporlayan
     bir çağrı noktası da log kapsamına `LogCentreHandled` koyarak ikinci kaydı engeller. Ayrıca açık olaylar:
     `AGENT_STARTED` (sürüm, ERP türü, ERP veritabanı) ve `AGENT_STOPPING` — servis `AgentLifecycleWorker`,
     masaüstü `App.OnStartup`/`OnExit` ile; servis için `AppDomain.UnhandledException` → FATAL ve
     `TaskScheduler.UnobservedTaskException` → ERROR (kuyruğa yazımı **beklenir**, süreç ölmeden önce).
     Masaüstü kuyruğu `DesktopHeartbeatService` turunda, kapanışta ise son bir kez boşaltılır.
   - **İz kimliği (L3g):** telefonun isteğinden ERP yazımına kadar tek kimlik. `jobs.correlation_id` ingest
     isteğinin `X-Correlation-Id` başlığından yazılır (istemci göndermezse ara katman üretir), `GET /jobs/pending`
     yanıtında `correlationId` olarak ajana gider. Ajan işi `Core/Logging/AgentCorrelation.Begin(...)` kapsamında
     işler: kapsam **AsyncLocal** olduğu için o sırada yazılan her log satırı ve yapılan her HTTP isteği aynı
     kimliği taşır (`HttpRemoteApiClient` her isteğe `X-Correlation-Id` ekler; iş dışındaki çağrı da kendi taze
     kimliğini alır). Ack'te yazılan `ERP_WRITE_FAILED`/`ERP_WRITE_RETRY` olayı **işin** kimliğini kullanır, ack
     isteğininkini değil — hata telefon belgesinin izine bağlanır. Eski sunucudan gelen işte alan boştur; ajan o
     zaman kendi kimliğini üretir. **Onay bekleyen belge:** `approval_requests.correlation_id` istekle birlikte
     saklanır ve onaydan sonra oluşan işe geçer — günler sonra yazılan belge de telefonun ilk isteğine bağlı
     kalır. Ajan tarafında her tur (senkron turu, heartbeat, iş yoklaması) kendi kapsamını açar, böylece
     HTTP istemcisinin kendi uyarısı da o isteğin taşıdığı kimlikle kaydedilir.
   - **Heartbeat (L3f):** ajanın durumu tek yerde tutulur — `Core/Sync/AgentRunStatus`: senkron döngüsü ve iş
     kuyruğu yazar, heartbeat (hem Windows servisi hem masaüstü) okur. Heartbeat gövdesi `appVersion`,
     `hostKind` (`service`/`ui`), `erpKind`, `erpVersion`, `lastSyncResult`, `lastErrorCode` taşır; hepsi
     **isteğe bağlıdır** ve sunucu gönderilmeyen alanla saklı değeri ezmez (eski ajan aynen çalışır).
     `lastSyncAtUtc` artık "şimdi" değil gerçek son başarılı tur — hiç senkronize etmemiş ajan `null` gönderir;
     canlılık ayrı kolonda. ERP sürümü adaptörden **6 saatte bir** sorulur (her dakika bir DB gidiş-dönüşü
     etmesin). Sunucu `agents`'a nullable kolonları yazar, `lastError`'ı bir kez daha maskeler ve geçmişi
     `agent_heartbeat_log`'a **yalnız değişimde ya da 15 dakikada bir** satırlar.
   - **Senkron turu ölçümü (L3e):** her tur bir INFO `AGENT_SYNC_ROUND` olayı bildirir (`Core/Logging/AgentSyncRound`).
     Özellikler: `trigger` (`timer`/`manual`/`section` tetikleyicisi), `mode` (`changelog`/`snapshot`/`section`),
     `success`, `durationMs`, `rows`, hareket eden **bölüm başına** `rows.<bölüm>`, `payloadBytes`, `errorCode`.
     **Yalnız sayı taşır** — cari, stok ya da fiyat yok. 20 saniyede bir tur kuyruğu doldurmaz: parmak izi
     sayıları temizlediği için başarılı turlar tek satırda toplanır (`repeat_count`), başarısız tur ve farklı
     `errorCode` kendi satırını açar. Zamanlayıcı turları `AgentSyncLoop`'tan, operatörün düğmeleri
     `DashboardViewModel`'den bildirilir; raporlayıcısı olmayan bir host (ve testler) sessizce çalışmaya devam eder.

26. **Telefon belgeleri ERP'ye yalnız çevirici üzerinden yazılır (ERP yazım goal'ü Y0–Y5, 2026-09-17).**
   - **Tek yol:** `mobileDocumentId` taşıyan `sales_order` / `sales_return` / `collection` gövdesini ajan
     `Core/Jobs/MobileDocumentTranslator` ile ERP'den bağımsız komuta çevirir; `IErpAdapter.WriteSalesDocumentAsync /
     WriteSalesReturnAsync / WriteCollectionDocumentAsync` yazar. Bu gövdeler tipli eski writer'lara (`MikroSalesOrderWriter`,
     `MikroCollectionWriter`…) **gönderilmez**; eski gövde (liste fiyatı / yapılandırılmış ödeme yok)
     `MOBILE_APP_UPDATE_REQUIRED` ile reddedilir, net fiyattan iskonto ya da açıklama metninden çek/senet **tahmin edilmez**.
     Sözleşme: `docs/mobil-belge-sozlesmesi.md`; hata kodları `Shared/ErpWriteError`.
   - **Ayarlar kiralama anında:** `erpContext` (seri, depo, kasa/banka, ERP kullanıcı no, fiyat listesi, portföy kasaları)
     iş kiralanırken `erp_write_settings` + gönderenin `mobile_user_erp_mappings` satırından kurulur
     (`ErpWrite/ErpWriteContextBuilder`). Eşleme düzeltilip yeniden denenen iş düzeltilmiş değeri alır. Seriler telefonda
     seçilmez; Portal "ERP aktarım ayarları"ndan gelir.
   - **Idempotency Mikro'da:** her yazılan evrak aynı transaction'da `_ERPB_EVRAK_ESLESME`'ye `(DocumentType, ExternalId)`
     ile kaydedilir; aynı belge (yeniden deneme, Portal/Admin "yeniden dene", kaybolan ack) ikinci evrak açmaz. SQLite
     `mappings` yalnız önbellektir.
   - **Tutar sözleşmesi:** telefon toplamı Mikro hesabından ±0,05 TL farklıysa `TOTAL_MISMATCH` — telefon aynı
     aritmetiği kullanır (`ErpSalePricing` = `MikroPriceCalculator`). Telefon KDV oranını ve fiyat listesi KDV dahil
     bayrağını ajan okumasından alır (`kdvOrani`, `fiyatListeleri[].kdvDahil`; projeksiyon sürümü 4).
   - **Görünürlük:** yazım sonucu telefona `GET /api/v1/ingest/jobs/status`, Portal'a `/api/v1/portal/erp-documents`
     (+ Admin'e özel "yeniden dene"), Admin'e iş ayrıntısı (ajan sonuçları, `erpContext`) ile çıkar; ajanın yazamadığı
     belge Log Merkezi'ne `ERP_WRITE_FAILED` / `ERP_WRITE_RETRY` olur. ERP'ye ulaşılamaması (`retryable`) işi bekletip
     yeniden kuyruğa alır; kalıcı hata `Failed` kalır.
   - **Yazma testleri yalnız izinli test kopyasında:** canlı yazım testleri `ERPBridge_RUN_INTEGRATION=1` **ve**
     `ERPBridge_MIKRO_WRITE_DB` ∈ `MikroWriteTestDatabase.AllowedDatabases` (yalnız `MikroDB_V15_DEMO`; ilk kopya `MikroDB_V15_ERPBTEST` 2026-09-17'de silindi)
     ister; müşteri veritabanına (`MikroDB_V15_02` vb.) yazan test yazılmaz. Okuma testleri canlı veritabanına bağlanabilir.
   - **Dapper kolon sırası:** okuyucuların positional record'larına kolon eklerken SQL'deki sıra kurucuyla aynı olmalı
     (#95'te `VatRate` sırası canlı okumayı kırdı; #178'de `DiscountAmount`/`VatAmount` `Amount`'un hemen ardına yazıldı ve
     stok hareketi okuması 2026-09-23 → 2026-09-28 arası hiç çalışmadı; CI canlı test çalıştırmaz). Yeni okuyucu kolonu canlı okuma
     testiyle gelir: `MikroSalespersonReaderLiveTests` (salt okunur; `ERPBridge_RUN_INTEGRATION=1`, `ERPBridge_MIKRO_WRITE_DB`,
     bu makinede `ERPBridge_SCHEMA_SERVER=.` — TCP kapalı). **Dikkat:** `MikroCustomerLedgerReaderLiveTests` içinde yazan test de
     var; yalnız izinli kopyada (`MikroDB_V15_DEMO`) ve filtreyle çalıştırılır.
     Stok hareketi projeksiyonu `MikroDbReader.StockTransactionsSql` (`internal`) sabitindedir;
     `StockTransactionProjectionTests` SELECT listesinden kolon adı/sıra/CAST tipini çıkarıp Dapper'la veritabanısız
     materyalize eder — CI'da her zaman çalışır (#185). Her kolon `CAST(... AS <tip>) AS <Ad>` biçiminde olmalı.

27. **Görevler ve bildirimler merkezdedir: `Tasks/TaskService` + `Endpoints/MobileTaskEndpoints` (GOAL_GOREVLER, 2026-09-25).**
   - **ERP'ye hiç yazılmaz.** Görev, alt görev, yorum, resim ve bildirim ERP'li ve ERP'siz firmada aynı tablolarda
     (`TenantId`) tutulur; ajan görmez. Ayrıntı ve kararlar: `docs/GOAL_GOREVLER.md`.
   - **Yazım işlem partisiyle:** telefon çevrimdışı yaptığını `POST /api/v1/android/tasks/ops` ile gönderir. Her işlemin
     `opId`'si vardır; `task_ops_applied`'da olan id `duplicate` döner, yeniden uygulanmaz. Reddedilen işlem kendi
     savepoint'ine geri döner ve partinin geri kalanını durdurmaz (`applied|duplicate|rejected`). Görev ve alt nesne
     kimliklerini telefon üretir. `ingest/jobs` **kullanılmaz** (her tıklama bir iş satırı açardı).
   - **Okuma:** `GET tasks?changedSinceSeq` (görünürlük filtreli, `mobile_records`/`sync/pull` değil — kural 12'nin yasağı
     yeni `/sync/<bölüm>` uçlarıdır; görevler kişiye özel olduğu için onay merkezi gibi kendi ucundadır). Değişiklik
     numaraları (`tasks.UpdatedSeq`, `user_notifications.Seq`, `task_series.UpdatedSeq`) `tenant_sync_counter`'dan
     yazan işlem içinde ayrılır (kural 11). Bir kişi görevden çıkarılınca artımlı çekmede görev ona hiç gelmez;
     telefon ara ara tam çekimle (`changedSinceSeq=0`) yerel kopyayı uzlaştırır.
   - **Görünürlük/yetki:** ADMIN/MANAGER (`TaskService.CanManage`) her görevi görür ve herkese atar; diğerleri yalnız
     kendine atar, oluşturduğu/atandığı/takip ettiği/alt görevine atandığı görevi görür. İleri tarihli görev
     (`StartAtMs`) atanana zamanlayıcı başlatana (`StartNotifiedAtMs`) kadar görünmez. Düzenleme oluşturan veya
     yönetici; tamamlama/alt görev işaretleme/resim atananlar da. `requiresPhoto` görevi resimsiz tamamlanamaz.
   - **Bildirim:** `user_notifications` kişi başı gelen kutusu. Atanana `TASK_ASSIGNED`, tamamlanınca oluşturan +
     takipçilere `TASK_COMPLETED`, yorumda `TASK_COMMENTED`, yeniden açılınca `TASK_REOPENED`, zamanlayıcıdan
     `TASK_DUE_SOON` (60 dk kala) ve `TASK_OVERDUE`. İşlemi yapana kendi işlemi bildirilmez. Push yoktur: telefon
     açıkken `GET tasks/events` uzun yoklaması (`TenantEventHub` konu `tasks`, sürüm numarası), kapalıyken kendi
     15 dk'lık işçisi.
   - **Resim PostgreSQL'dedir:** `task_attachment_blobs` (bytea); Coolify'da kalıcı disk gerekmez, yedeğe girer.
     Ham gövde `PUT tasks/{id}/attachments/{attachmentId}`, yalnız JPEG/PNG/WEBP (dosya imzası da denetlenir), ≤ 2 MB,
     görev başı 10, firma kotası 1 GB (`Tasks:*`). `GET` yalnız görevi gören kullanıcıya, değişmez önbellek başlığıyla.
   - **Zamanlayıcı:** `Workers/TaskSchedulerWorker` dakikada bir `RunSchedulerAsync`: seri örnekleri (sunucu
     kapalı kaldıysa birikmiş örnek değil **tek** örnek), ileri tarihli başlangıçlar, bitiş uyarıları, 30 günlük
     temizlik (silinen görevin resimleri, silinen resimler, eski `opId`'ler). Seri saatleri `Europe/Istanbul`
     (`TaskSchedule`, saf; testli). Testler zamanlayıcıyı kapatır (`Tasks:SchedulerEnabled=false`) ve kendi saatiyle çağırır.
   - **Hız sınırı kullanıcı başınadır** (`PerMobileUserRateLimitPolicy`, dk'da 120): firma başı 100'lük ortak bütçeyi
     ekibin görev yoklaması tüketmesin.
   - **Cari ziyaretinde hatırlat (S8):** `tasks.VisitReminder` + `VisitReminderFromMs` (null = hemen). Yalnız cariye
     bağlı görevde açık kalır (`SetVisitReminder`; cari yoksa sunucu kapatır). Seride `task_series.VisitReminder`;
     her örnek kendi başlangıcından (`runAt`) itibaren hatırlatır. Hatırlatmayı sunucu göndermez: telefon, atanan
     kişi cariyi satış için açınca gösterir; "sonra tekrar sor" telefonda yereldir (Sipariş Cepte KB kural 34).
28. **Satılan ek modüller firma bazındadır; XML ürün beslemesi telefonda işlenir: `Endpoints/MobileXmlFeedEndpoints` (XML ürün modülü, 2026-09-26).**
   - **Modül seti yalnız operatörden:** `tenant_modules` (`TenantId, ModuleKey` birleşik PK). Bilinen anahtarlar tek yerde:
     `Domain/TenantModules.Known` (firma ek modülleri: `xml_import`, `customer_catalog` — kural 36). Admin konsolu `/tenants/{id}/mobile` "Ek modüller"
     paneli → `PUT /api/v1/admin/tenants/{id}/mobile/modules {modules:[…]}` **telefon ek modülleri** setini tamamen değiştirir (204; bilinmeyen
     anahtar 400 `UNKNOWN_MODULE`, tenant yoksa 404; zaten açık modülün `EnabledAtUtc`'si korunur, `EnabledBy` = admin
     e-postası). **Aynı tabloda Go modülleri de durur (`go_` öneki, kural 30):** bu uç `go_` anahtarını kabul etmez (400
     `UNKNOWN_MODULE`) ve `go_` satırlarını **silmez**; session ve overview `go_` anahtarlarını göstermez (2026-09-28; önceden
     boş set gönderen kaydetme Go modüllerini de silerdi). Ortak yazım `Endpoints/TenantModuleSets.ReplaceAsync` (kapsam süzgeciyle).
     Telefon/panel modülü açamaz. Session (login + `/account/me`) ve konsol overview'ı `modules[]` taşır
     (sıralı, küçük harf, yoksa boş dizi); telefon satın alınmamış özelliği gizler.
   - **Besleme ayarı firma geneli:** `tenant_xml_feed_settings` (tenant başına bir satır). `/api/v1/android/xml-feed/config`
     (`MobileUserPolicy` + firma başı hız sınırı): `GET` firmanın her aktif kullanıcısı; `PUT`/`DELETE` yalnız
     `RolePermissions.CanManageUsers` (403 `ADMIN_REQUIRED`). Sıra: önce modül (yoksa herkese 403 `MODULE_NOT_ENABLED`),
     sonra rol, sonra doğrulama (400 `INVALID_XML_FEED_CONFIG`): `url` mutlak http/https ≤ 2048, `recordPath` dolu ≤ 512,
     `mapping` anahtarları `CODE, IMAGE, DESCRIPTION, BARCODE, TITLE, BRAND, CATEGORY, PRICE, VAT, STOCK` (büyük harf,
     harfe duyarlı), `CODE` en az bir dolu yol, hedef başı ≤ 10 yol, yol ≤ 256. Boş yollar ve yolsuz kalan hedefler
     atılır. Satır yokken `GET` `configured=false`, `mapping {}`, `downloadImages=true`, diğerleri false/null döner.
     `DELETE` idempotent 204.
   - **ERP'li firmada tam aktarım yok:** `fullImport` yalnız `DataSource = native` ise saklanır; ERP'li firmada telefon ne
     gönderirse göndersin `false` yazılır — ana veri ERP'nindir, XML yalnız resim/açıklama ekler (Sipariş Cepte
     `DataEditPolicy`).
   - **Sunucu beslemeyi indirmez.** URL'yi her telefon kendisi çeker ve kendi yerel veritabanına işler; sunucu yalnız
     ayarı saklar (SSRF yüzeyi açılmaz, sunucuda XML ayrıştırma yok).
   - Testler: `XmlFeedModuleRelationalTests`, Admin `TenantMobilePageTests` (modül kutusu).
29. **SKT (son kullanma tarihi) kayıtları merkezdedir: `Expiry/StockExpiryService` + `Endpoints/MobileExpiryEndpoints` (2026-09-27).**
   - **Uygulamanın operasyonel verisidir, ERP ana verisi değil.** Kayıt = ürün (`stockCode`, barkod/ad anlık kopyası) +
     reyon/raf (`location`) + depo + son kullanma tarihi + isteğe bağlı miktar + not + `closed`. Telefonlardan girilir,
     firmanın bütün telefonları paylaşır; ERP'li ve ERP'siz firmada aynı tablo (`stock_expiry_records`, `TenantId`),
     **Mikro'ya hiç yazılmaz**, ajan görmez, `DataEditPolicy`'ye takılmaz. Miktar bilgi amaçlıdır: satış düşmez,
     raf bitince kayıt kapatılır (`closed`). Modül/rol kapısı yok: firmanın her aktif kullanıcısı okur ve yazar.
     Panel ekranı yok.
   - **Yazım görevlerle aynı işlem partisidir** (kural 27): `POST /api/v1/android/expiry/ops` `{ops:[{opId, type:
     upsert|delete, record}]}` ≤ 200 (fazlası 400 `EXPIRY_BATCH_TOO_LARGE`). `stock_expiry_ops_applied`'da olan
     `opId` `duplicate`; reddedilen işlem kendi savepoint'ine döner, partiyi durdurmaz. `upsert` yoksa açar
     (oluşturan yalnız burada), varsa tüm alanları yazar (son yazan kazanır). `delete` yumuşak silmedir; silinmiş
     kayıt yeniden düzenlenmez (`EXPIRY_NOT_FOUND`), ikinci silme `applied` ama değişiklik değildir. Kimliği telefon
     üretir ve **tüm firmalar arasında tekildir** (PK); başka firmanın kimliği `EXPIRY_NOT_FOUND`.
   - **Okuma:** `GET /api/v1/android/expiry?changedSinceSeq&take` (varsayılan 500, en çok 1000), silinenler
     `deleted=true` ile gelir. `UpdatedSeq` `tenant_sync_counter`'dan yazan işlem içinde ayrılır (kural 11); `upsert`
     sayacı kaydı aramadan **önce** alır, böylece aynı yeni kimliği açan iki telefon kilitte sıraya girer, PK'da
     çakışmaz. Kendi ucundadır (`sync/pull` değil): kayıtlar ERP şeklinde değil, telefonun yazdığı veridir.
   - Doğrulama tek yerde (`EXPIRY_INVALID`, Türkçe mesaj alanı adlandırır); sözleşme `docs/api-contracts.md`.
     Testler: `ExpiryRelationalTests` (SQLite, uçlar), `StockExpiryServiceTests` (bellek içi, doğrulama).
     `stock_expiry_ops_applied` henüz temizlenmiyor (`AppliedAtMs` indeksi hazır).
30. **Lisans sunucusu iki ürüne hizmet eder: ErpBridge ve Go (`Endpoints/GoLicenseEndpoints`, 2026-09-28).**
   Go, ayrı bir depodaki (Retrosero/entegrasyon) pazaryeri entegrasyonu masaüstü uygulamasıdır; lisansını bu sunucudan alır.
   - **Ürün lisansa bağlıdır:** `licenses.Product` = `erpbridge` (varsayılan; migration öncesi tüm anahtarlar) | `go`
     (`Domain/LicenseProducts`). Go anahtarı `GO-` + 32 hex, ErpBridge anahtarı `LIC-` + 32 hex. **Anahtar yalnız kendi
     ürününü açar:** `/licenses/validate` ve `/agents/register` Go anahtarına, Go ucu ErpBridge anahtarına bilinmeyen anahtar
     gibi **404 `LICENSE_NOT_FOUND`** döner. Lisans çözen yeni bir uç yazılırsa ürün filtresi şarttır.
   - **Tek uç, etkinleştirme = yenileme:** `POST /api/v1/go/license/activate {licenseKey, machineId, machineName?, appVersion?}`
     (anonim, `Anonymous` hız sınırı). Go uygulaması bunu ilk açılışta ve birkaç saatte bir çağırır. Cevaplar: 200 imzalı belge;
     400 `MISSING_LICENSE_KEY`/`INVALID_MACHINE_ID` (≤128); 404 `LICENSE_NOT_FOUND`; 410 `LICENSE_REVOKED` (lisans ya da firma
     pasif) / `LICENSE_EXPIRED`; 409 `DEVICE_LIMIT_REACHED`; 503 `GO_LICENSING_UNAVAILABLE` (imza anahtarı yok).
   - **1 anahtar = 1 bilgisayar:** `go_installations` (lisans başına tek satır, `LicenseId` unique; `MachineId` uygulamanın
     ürettiği hash, `MachineName`, `AppVersion`, `ActivatedAtUtc`, `LastSeenAtUtc`). Başka makine 409 alır; eşzamanlı iki ilk
     etkinleştirmede unique index birini reddeder (409). Taşıma: Admin → Lisanslar → "Bilgisayarı serbest bırak"
     (`POST /admin/licenses/{id}/go-installation/release`, satırı siler). Firma cihaz limiti (`MaxDeviceCount`) Go'ya uygulanmaz.
   - **Belge ES256 imzalıdır, uygulama çevrimdışı doğrular:** `base64url(json).base64url(imza)` (IEEE P1363, SHA-256).
     İddialar `GoLicensing/GoLicenseClaims`: `v, kid, product, licenseId, tenantId, tenantName, machineId, issuedAtUtc,
     validUntilUtc, licenseExpiresAtUtc, modules[]`. `validUntilUtc = min(şimdi + GoLicense:OfflineAllowance (3 gün),
     lisans bitişi)` — uygulama bu ana kadar sunucuya ulaşamasa da senkron yapar. `modules` = firmanın `go_` önekli
     `tenant_modules` anahtarları (Go'nun ücretli modülleri; aşağıdaki "Go modülleri" maddesi).
     `kid` = açık anahtarın (SubjectPublicKeyInfo DER) SHA-256'sının ilk 16 hex'i; anahtar değişirse Go'nun yeni sürümü
     yeni açık anahtarı gömülü taşımalıdır.
   - **İmza anahtarı:** `GoLicense__SigningKey` (ECDSA P-256 PKCS#8; PEM ya da tek satır base64). **Açılış şartı değildir:**
     yoksa yalnız Go ucu 503 döner, uyarı loglanır — ErpBridge etkilenmez. Değer depoya asla girmez.
   - **Yenileme anahtarı korur:** `PUT /admin/licenses/{id}/expiry {expiresAtUtc}` (null = süresiz); Go uygulaması yeni
     tarihi bir sonraki yenilemede alır. Konsol: ürün seçimi, ürün süzgeci, Go kartında bilgisayar bilgisi.
   - **Go modülleri firma bazındadır, lisans bazında değil (2026-09-28):** bilinen anahtarlar tek yerde
     `Domain/TenantModules.GoKnown` — Go uygulamasının modül kataloğuyla **birebir aynı** olmalı:

     | Anahtar | Konsol adı |
     |---|---|
     | `go_ai` | Yapay zekâ asistanı |
     | `go_einvoice` | E-fatura / e-arşiv |
     | `go_erp` | ERP bağlantısı |
     | `go_reports` | Raporlama ve kâr analizi |
     | `go_competition` | Rekabet ve akıllı fiyat |

     `PUT /api/v1/admin/licenses/{id}/go-modules {modules:[…]}` lisansın **firmasının** Go modül setini tamamen değiştirir ve
     güncel `LicenseDto`'yu döner (200). Sıra: lisans yok 404 `LICENSE_NOT_FOUND`; ErpBridge lisansı 400 `NOT_GO_LICENSE`;
     `modules` yok 400 `INVALID_BODY`; `GoKnown` dışı anahtar (`xml_import` dahil) 400 `UNKNOWN_MODULE` (reddedilen istek hiçbir
     şeyi değiştirmez). Anahtarlar kırpılır, küçük harfe çevrilir, tekilleştirilir. **Yalnız `go_` satırları değişir**; `xml_import`
     gibi telefon modüllerine dokunulmaz. Zaten açık modülün `EnabledAtUtc`/`EnabledBy`'ı korunur, yeni açılanın `EnabledBy` = admin
     e-postası. Firmanın bütün Go lisansları aynı seti taşır. `LicenseDto.goModules` (sıralı dizi) Go lisanslarında liste, oluşturma,
     süre ve modül cevaplarında hep vardır (boş olabilir); ErpBridge lisansında alan **yazılmaz**. Müşterinin bilgisayarı değişikliği
     bir sonraki yenilemede alır (Go 6 saatte bir yeniler; Go'nun Lisans ekranında "Şimdi doğrula" hemen yeniler).
     Konsol: Go kartında "Modüller · firma geneli" çipleri (Admin `Api/GoModuleCatalog` Türkçe adları) + etkin Go lisansında
     "Modülleri düzenle" onay kutusu paneli (not: firmanın tamamına uygulanır, bir sonraki yenilemede ulaşır). Yeni Go modülü
     eklenirse `TenantModules.GoKnown` + `GoModuleCatalog` + Go uygulamasının kataloğu birlikte güncellenir.
   - Testler: `GoLicenseActivateTests` (imza doğrulama, 1 PC, ürün ayrımı, modüller, 503), `AdminLicensesTests` (ürün, bırakma, süre,
     Go modülleri: yalnız `go_` satırları, token'a ulaşma, 400/404), `XmlFeedModuleRelationalTests` (telefon ve Go modülleri
     birbirini silmez), Admin `LicensesPageTests` (çipler, düzenleme paneli).
   - **Konsolda ürünler asla karışık listelenmez (2026-09-28):** ürün kimliği tek yerde, Admin `Api/LicenseProductCatalog`:

     | Ürün | Anahtar öneki | İşaret | Renk | Anahtarın girildiği yer |
     |---|---|---|---|---|
     | `erpbridge` | `LIC-` | E | lacivert `#4f46e5` | ErpBridge ajanı ayar ekranı (+ telefon girişleri) |
     | `go` | `GO-` | G | turuncu `#ea580c` | Go ilk açılış ekranı (tek bilgisayar) |

     Lisanslar sayfası ürün **sekmelerine** bölünür (`/licenses?urun=go`; boş/bilinmeyen = ErpBridge). Her sekmenin kendi sayacı
     (aktif/toplam), açıklama şeridi, özeti ve listesi vardır. **Yeni lisans formunda ürün seçimi yoktur**: ürün açık sekmeden gelir,
     düğme "Go (pazaryeri) lisansı oluştur" der; yanlış ürüne anahtar üretmek için sekme değiştirmek gerekir. Kartta ürün rengiyle sol
     kenar, ürün çipi ve öneki vurgulanmış anahtar; öneksiz eski anahtar "önekten önce üretilmiş" notu alır. Ürüne özel işlemler yalnız
     kendi kartında: telefon girişi ErpBridge'de, bilgisayar bilgisi/serbest bırakma ve modüller Go'da. Yeni ürün eklenirse önce
     `LicenseProducts` (sunucu) + `LicenseProductCatalog` (konsol) + CSS renk sınıfı `license-product--{anahtar}` eklenir.
     Test: Admin `LicensesPageTests`. Go uygulaması da `LIC-` ile başlayan anahtarı sunucuya sormadan "bu bir ErpBridge anahtarı" diye reddeder.

31. **Hedefler ve ekipler merkezdedir: `Targets/TargetService` + `Endpoints/PortalTargetEndpoints` / `MobileTargetEndpoints` (GOAL_HEDEF_RUT, 2026-09-28).**
   Plan ve kararlar: [`docs/GOAL_HEDEF_RUT.md`](../docs/GOAL_HEDEF_RUT.md). ERP'ye yazılmaz; ERP'li ve ERP'siz firma aynı tablolar.
   - **Ekip modeli (K2):** `sales_teams` (`REGION` | `TEAM`, ekibin isteğe bağlı üst bölgesi `ParentId`), `sales_team_members`
     (PK `TenantId+UserId` — kişi **tek ekipte**; başka ekibe eklenen eski ekibinden çıkar), `sales_team_managers` (sorumlu
     yönetici; bölge sorumluluğu altındaki ekipleri kapsar). Yapıyı yalnız ADMIN değiştirir (403 `TEAM_ADMIN_REQUIRED`);
     sorumlu yalnız ADMIN/MANAGER olabilir; altında ekip olan bölge silinemez (409 `TEAM_HAS_CHILDREN`); silinen ekibin hedefleri
     `IsDeleted` olur.
   - **Kapsam tek sınıftadır: `Targets/TeamScope` (K3).** ADMIN tüm firma; en az bir ekip/bölgeden sorumlu MANAGER yalnız o
     ekipler + üyeleri + kendisi; **sorumluluğu olmayan MANAGER tüm firma** (ekip kavramından önceki davranış); diğerleri yalnız
     kendisi. Hedef yazma `RolePermissions.CanManageTargets` (ADMIN, MANAGER) + kapsam (403 `TARGET_OUT_OF_SCOPE`); firma geneli
     hedefi yalnız tüm firmayı gören. Görev ve onay görünürlüğü bu kapsamı **kullanmaz**.
   - **Hedef (K5):** `sales_targets` — dönem (`DAILY` `2026-10-05` · `WEEKLY` ISO hafta Pazartesi `2026-W41` · `MONTHLY` `2026-10`,
     İstanbul günü; `Targets/TargetPeriod`), tür (`REVENUE`, `COLLECTION`, `PRODUCT`, `CATEGORY` = ana grup/ERP'siz `kategori`,
     `SUB_CATEGORY` = `anaGrup|altGrup`, `BRAND`, `VISIT`, `DOCUMENT_COUNT`), ölçü (`AMOUNT` | `QUANTITY` | `COUNT`), kalem kodu, sahip
     (`USER` | `TEAM` | `COMPANY`, firmada `OwnerId = Guid.Empty`). Doğal anahtar tekil (upsert); `value = null` yumuşak siler.
     Kayıt **hep ya hiç** (hatalı kalemin `index`'i döner), ≤ 2000 kalem. `operationId` işlemin **ilk yazımı** olarak
     `sales_target_operations`'a (PK `TenantId+OperationId`) girer: aynı anda gelen tekrar PK'da bekler, sonra `duplicate` alır;
     aynı doğal anahtarı eşzamanlı yaratan iki farklı kayıttan ikincisi 409 `TARGET_CONFLICT` (Codex, PR #214). Her değişiklik
     `sales_target_events`'e (eski/yeni değer, kim); ekip silinince hedeflerinin `DELETE` olayı da yazılır.
   - **Gerçekleşen (K6–K8, `Targets/TargetFactReader`):** ERP'siz firmada `jobs` (`Succeeded`; `PortalReports.PhoneDocumentsAsync`,
     `occurredAt` İstanbul günü + 7 gün tolerans): satış/iade satırı değeri `lineTotal` → adet × `unitPrice` → adet ×
     `listUnitPrice` × `conditionPercent` (ERP'siz defterle aynı, KDV ayrımı yapılmaz), anında ödemeli satış aynı zamanda tahsilat.
     **Panel düzeltmeleri:** `document_void`/`ledger_void` ile iptal edilen belge (`targetKey`'in işi) sayılmaz; ayakta kalan
     `document_edit`/`ledger_edit` onun yerine sayılır — düzeltilen belgenin **ilk satıcısına**, düzeltmenin tarihiyle (yoksa asılınkiyle).
     ERP'li firmada **Mikro aynası** (`stockTransactions`, `customerTransactions`, ayrı bir `PortalRecordMirror` "target-rows"):
     satış = `tip` 1 + `evrakTip` 1/4 (irsaliye/fatura) satırı, iade = `tip` 3 + `evrakTip` 3; tutar `tutar − discountAmount`
     (faturalanan irsaliye Mikro'da ikinci bir stok satırı açmaz — açsaydı stok iki kez düşerdi; canlı V15_02/V16_03'te satışların
     tamamı kendi faturasına bağlı `evrakTip` 4, bağlı `evrakTip` 1 satırı yok, 2026-09-28)
     (KDV hariç); tahsilat = `TAHSILAT` ve kapalı (peşin) `SATIS`. Satır kişiye `plasiyerKod` → `mobile_user_erp_mappings.SalespersonCode`
     ile bağlanır; kendi kodu olmayan kişinin kişisel gerçekleşeni yoktur (sahip satırında uyarı), firma geneli tüm satırlardır.
     Kişinin `Pending/Processing` telefon belgeleri ayrı `pending`'dir. **Ajan `plasiyerKod` göndermiyorsa** (hiçbir satırda alan
     yok) kaynak `phone-documents`: ERP'li Plasiyerler raporu gibi telefon belgeleri (Pending+Processing+Succeeded) sayılır, uyarı döner.
     Kaynak `source` alanındadır: `server-documents` | `erp-mirror` | `phone-documents`. Bir aralığın olguları 60 sn önbelleklenir.
     Bilinen sınır: satış türü "sipariş" olan ERP'li firmada telefon satışı Mikro'da faturalanana kadar gerçekleşene girmez.
   - **Hesap (K9–K11, `Targets/TargetBoard`, saf):** yüzde, bugüne kadar beklenen (iş günü oranı), tempoyla dönem sonu tahmini,
     kalan ÷ kalan iş günü (bugün dahil). Kendi günlük hedefi olmayan güne **aylık (yoksa haftalık) hedeften pay** türetilir:
     `(hedef − bugünden önceki gerçekleşen) ÷ kalan iş günü`, `derived=true`. `VISIT` hedefi girilmemişse hedef = dönemde planlanan
     rut durağı. İş günleri `target_settings.WorkDays` (Pzt=1…Paz=64, varsayılan Pzt–Cmt). Ekip/firma satırında `childrenSum` (bir alt
     seviyedeki aynı hedeflerin toplamı).
   - **Panel (GOAL_HEDEF_RUT P1–P3):** menüde "Hedefler & rut" grubu. `/hedefler` (takip): dönem seçici (`Shared/TargetPeriodPicker`,
     gün/hafta/ay + önceki/sonraki), ekip süzgeci, firma satırından dört özet kart, kişi/ekip tablosu (ciro ve tahsilat ilerleme
     çubuğu, tempo altında/yolda rozetleri, tahmin, günde gereken; satıra tıklayınca tüm hedefler). `/hedefler/giris`: kişi/ekip/firma ×
     ciro/tahsilat/ziyaret/fiş hücre tablosu — değişen hücreler işaretlenir, **tek `operationId` ile toplu kayıt**, sunucunun kalem
     hatası "kişi · tür" olarak gösterilir ve düzenleme kaybolmaz; sayı `12.500,50` / `12500.5` biçimlerini okur (virgül daima ondalık).
     Kalem hedefi (ürün/kategori/[ERP'de] alt kategori/marka) arama + seçimle eklenir/silinir; önceki dönemden kopyala (önizle → uygula),
     dağıt (önizle → tabloya **kaydedilmemiş** aktar), CSV/.xlsx içe aktarma (`Kim;Tür;Ölçü;Kalem;Hedef`, kim = kullanıcı adı/ad
     soyad/ekip adı/"Firma") ve CSV dışa aktarma (data URI), çalışma günleri. `/ekipler`: bölge → ekip ağacı, admin için
     oluştur/düzenle/sil (üyeler — başka ekipteki kişi taşınır —, sorumlu yöneticiler yalnız ADMIN/MANAGER), diğerlerine salt okunur.
     Testler: `PortalTargetPagesTests`.
   - **Rut (P4–P5, `Endpoints/PortalRouteEndpoints`):** `GET /api/v1/portal/routes` (kapsamdaki planlar + atanabilir kişiler;
     plan `canEdit` = tüm atananlar kapsamda), `PUT /routes/{planId}` (planId ≤ 64, harf/rakam/-/_), `DELETE /routes/{planId}`,
     `GET /routes/compliance?from&to&teamId` (≤ 92 gün; varsayılan bu hafta). **İkinci motor yok:** kayıt telefonun
     `route_plan` / `route_plan_delete` belgesinin aynısıdır (`ExternalId = portal-route:{planId}:{guid}`, her kayıt yeni belge),
     `TeamDocumentProcessor` işler, telefonlar akıştan (kural 17) alır. Panelde açılan durak id'sini sunucuda alır, yeniden kayıtta
     korunur (ziyaretler bağlı kalır). İşleyicinin İngilizce reddi panele Türkçe `ROUTE_INVALID` mesajıyla döner (kapsam dışı 403).
     Uyum: kişi/gün bazında planlanan, ziyaret, atlanan, **kaçırılan** (geçmiş günün kayıtsız durağı; bugünkü değil), plan dışı,
     uyum = ziyaret ÷ planlanan. **Her gün o gün geçerli plan sürümüyle hesaplanır** (`PortalReports.RoutePlanHistory`: `Succeeded`
     `route_plan`/`route_plan_delete` işlerinden, gün sonundan önceki son sürüm; iş geçmişi olmayan eski plan bugünkü haliyle) —
     sonradan düzenleme/silme geçmişi değiştirmez; plan kaydedildiği günden itibaren geçerlidir. **Ziyaret durağa `(planId, stopId,
     kullanıcı)` ile bağlanır** (`BuildVisits`; durak kimliği yalnız plan içinde tekildir), `planId` taşımayan eski ziyaret yalnız
     `stopId` ile eşleşir (Codex, PR #217). `GET /routes/{planId}` tek plan (liste kuralları; yoksa 404 `ROUTE_NOT_FOUND`). Panel `/rut` (plan listesi; editör: ad, başlangıç, etkin, atananlar, Pzt–Paz sekmeleri, durak sırası
     ↑↓, kaldır, günü başka güne kopyala, cari araması `/portal/customers`) ve `/rut/uyum`. Panel istemcisi `ROUTE_INVALID` için
     sunucunun metnini gösterir (`PortalApiClient.ServerWordedCodes`). Testler: `RoutePlanPortalRelationalTests`, `PortalRoutePagesTests`.
   - **Uçlar:** panel `GET|POST /api/v1/portal/teams`, `PUT|DELETE /teams/{id}`, `GET|PUT /targets` (pano + toplu kayıt),
     `POST /targets/copy` (±%, önizleme/uygula), `POST /targets/distribute` (EQUAL | LAST_PERIOD_SHARE, yalnız önizleme),
     `GET /targets/items?metric&q` (≤ 50; marka/ana grup adı ajan 1.3.0'ın `stock_brand`/`stock_main_group` lookup'larından —
     `PortalStockCatalog.Catalog.BrandNames/MainGroupNames`, yoksa kod), `GET|PUT /targets/settings`. Okuma `CanViewReports`. Telefon `GET /api/v1/android/targets/mine?date`
     (gün/hafta/ay, herkes) ve `/team?date&periodType` (ADMIN/MANAGER). Hız sınırı kullanıcı başına. Testler: `TargetRelationalTests`,
     `TargetBoardTests`, `TargetPeriodTests`.

32. **Bekleyen siparişler merkezdedir: `SuspendedSales/SuspendedSaleService` + `Endpoints/MobileSuspendedSaleEndpoints` (2026-09-29).**
   - Telefonda beklemeye alınan sepet (cari, depo, not, satırlar) firmanın bütün telefonlarında görünür; biri açıp
     tamamlar. Önceden yalnız o telefonun belleğindeydi (uygulama kapanınca kayboluyordu). ERP'ye yazılmaz, ajan görmez;
     ERP'li ve ERP'siz firmada aynı tablo (`suspended_sales`, satırlar `LinesJson` jsonb). Panel ekranı yok.
   - **SKT'nin (kural 29) işlem partisi, üstüne iki kural** (kullanıcı kararı): `claim` = telefonda açmak; firmanın her
     kullanıcısı açabilir, açılan sipariş herkesten kalkar, ikinci açma `SUSPENDED_SALE_TAKEN` + kimin aldığı (aynı sipariş
     iki kez girilmez). `delete` (iptal) ve var olanı `upsert` yalnız oluşturan ya da ADMIN/MANAGER
     (`SuspendedSaleService.CanManage`). `claim`/`delete` de sayacı kaydı okumadan önce alır: eş zamanlı iki açma kilitte sıralanır.
   - Reddedilen işlemin dokunduğu sipariş de yanıtta döner: telefon "başkası açtı" bilgisini ve mezar taşını aynı yanıtta alır.
   - Sözleşme `docs/api-contracts.md`; testler `SuspendedSaleRelationalTests` (SQLite). `suspended_sale_ops_applied` henüz temizlenmiyor.

33. **Kullanıcı yetkileri: rol şablonu + kişiye istisna (`Permissions/`, GOAL_YETKILER, 2026-09-29).**
   - **Katalog** `PermissionCatalog` (tek kaynak, `Version`): `module.*` (telefon modülleri), `portal.*` (web panel alanları),
     `action.*` (işlemler), `view.*` (görünürlük), `limit.*` (sayısal; boş = sınırsız). Anahtarlar kalıcıdır, yeniden adlandırılmaz.
     Varsayılanlar eski rol kurallarıyla birebir aynıdır; `PermissionResolverTests` 31 rol birleşiminin hepsinde bunu garanti eder
     (hiç kayıt yokken davranış değişmez).
   - **Hesaplama** `PermissionResolver.Resolve`: ADMIN her şeye yetkili ve sınırsız (şablonu yok, kişisel ayar yapılamaz).
     Diğerlerinde her rol için firmanın şablon satırı ya da katalog varsayılanı; evet/hayır rollerin VEYA'sı, limit en yükseği
     (sınırsız kazanır). Yöneticinin `CanApprove` / `CanManageApprovalRules` sütunları onay anahtarlarını yazar. Kişisel istisna
     en son ezer. Her girdi kaynağını taşır (`admin` / `role` + hangi roller / `override`).
   - **Saklama seyrektir:** `tenant_role_permissions` ve `mobile_user_permission_overrides` yalnız varsayılandan farkı tutar;
     "varsayılana dön" / "rolden" satırı siler. Her değişiklik `permission_changes`'e yazılır (kim, istemci, eski → yeni).
   - **Yükleme:** `MobileUserAccess.CheckAsync` her istekte `PermissionLoader.LoadAsync` ile `MobileUser.Permissions`'ı
     (`[NotMapped]`) doldurur. `RolePermissions.CanViewReports/Ledger/PlanRoutes/ManageTargets/OperateWarehouse/
     ManageWarehouse/EditNativeData`, `TaskService.CanManage`, `SuspendedSaleService.CanManage` ve ingest kart yetkisi anahtara
     bakar; yüklenmemişse katalog varsayılanı (eski rol kuralı). Kimlik kuralları (`IsAdmin`, `CanManageUsers`, `CanUsePhone`,
     `CanUsePortal`, ekip kapsamı) rolde kalır.
   - **Ingest (S5):** mobil kullanıcının doğrudan gönderdiği belge (`ApprovalKinds.ForDocument`) göndericinin modül yetkisine ve
     limitlerine göre denetlenir (`DocumentPermissionCheck`, `DocumentLimitFacts`: tutar = `grossAmount ?? amount ?? total`,
     satırların en yüksek `lineDiscountPercent`'i, `generalDiscountPercent`; satış `paymentType` "Cari Borç"/boş ve `payments`
     boşsa açık hesaptır → `action.sale.open_account` gerekir). Aşanda **ret değil** `409 APPROVAL_REQUIRED` +
     Türkçe neden: her telefon sürümü bunu onay talebine çevirir. Firmanın onay kuralı kapalı olsa da çalışır; onay talebi
     (`approval_request`), onaylanıp kaydedilen belge, Admin ve API anahtarı etkilenmez. Belgede olmayan alan denetlenmez.
   - **Uçlar** (`Endpoints/MobilePermissionEndpoints`, `/api/v1/android/account`): katalog, rol şablonları (Admin), kişi
     yetkileri (Admin herkesinkini, herkes kendininkini), değişiklik geçmişi. Oturum (`/login`, `/me`) `permissions`, `limits`,
     `permissionsVersion` taşır; `MobileUserDto.permissionOverrideCount`. Panel: `/yetkiler` (rol × izin matrisi, geçmiş) ve
     Kullanıcılar → Yetkiler (`UserPermissionsSheet`); `PortalRoles.Allows` oturum yetkisine bakar. Telefon: Siparis_Cepte KB kural 51.
   - **Yetki damgası:** her imzalı mobil yanıt `X-Permissions-Stamp` başlığı taşır (`PermissionStamp`: roller + bütün yetki ve
     limitlerin SHA-256 özetinin ilk 16 hanesi; `MobileUserStateHandler` ve `MobileAccountEndpoints.AuthorizeAsync` koyar),
     oturum da `permissionsStamp`. Telefon kaydettiğinden farklı damga görünce `/me`'yi yeniden okur: panelde yapılan değişiklik
     "beni hatırla" ile haftalarca açık kalan telefona yeniden giriş gerekmeden bir sonraki çağrıda (arka plan eşitlemesi dahil) ulaşır.
   - Sözleşme `docs/api-contracts.md`; testler `PermissionResolverTests`, `DocumentPermissionCheckTests`,
     `Permission*RelationalTests`, `IngestPermissionRelationalTests`, `PortalPermissionsTests`.

## 4. Yeni ERP Adaptörü Eklemek

Sözleşme, sıra ve tanım-tamamlandı listesi:
[`docs/erp-adapter-contract.md`](../docs/erp-adapter-contract.md)

Özet: adaptör yalnızca `Shared` + `Erp.Abstractions` (+ SQL Server ise `Erp.Sql`)
referans verir. `SecondAdapterSeamTests` (Logo iskeleti üzerinden) bunu mimari
değişmez olarak sabitler — o testler kırılıyorsa soyutlama gerilemiş demektir.

34. **Ürün kategorisi Mikro stok alt grubunun adıdır: `Sync/StockCategories` (2026-09-28).**
   - **Ajan:** `MikroDbReader.ReadLookupsAsync` `STOK_ALT_GRUPLARI`'nı `lookups` içinde `stock_sub_group` türüyle
     gönderir; `code` = `anaGrup|altGrup` (alt grup kodu tek başına benzersiz değil, 03'teki tablo satırı),
     `name` = `sta_isim`, `parentCode` = ana grup; `sta_iptal` olanlar gitmez, artımlı okuma `sta_lastup_date` ile.
     Mevcut alt gruplar imleçten eski olduğu için `MikroAdapter.SnapshotProjectionVersion` 9'a çıktı (bir kez tam okuma; 8'i GOAL_HEDEF_RUT plasiyer/marka için aldı).
   - **Merkez:** telefon ürününü kuran iki yol da (`AndroidEndpoints.ProductCatalogAsync` = `sync/urun`,
     `MobileEntityAssembler.BuildProduct` = `sync/pull` akışı) `StockCategories.Apply` ile `kategori` yazar.
     Çözüm sırası: (ana, alt) çifti → alt grup kodu tek bir ada çıkıyorsa o ad (ana grubu tutmayan az sayıda kart)
     → kodun kendisi. `#` ile başlayan yer tutucu (`#YOK`) ve boş kod kategori değildir. Ad yoksa `kategori`
     **hiç yazılmaz** (asla `""`): ERP'siz firmanın kartı kendi `kategori`'sini taşır ve korunur; telefon da yalnız
     null'da kendi yedeğine ("Diğer") düşer.
   - **Yeniden gönderim:** kategori ürün sunulurken birleştirilir, ürün satırının kendisi değişmez. Bu yüzden
     `MobileRecordProjector` bir `stock_sub_group` kaydı eklendiğinde, adı değiştiğinde ya da silindiğinde o alt gruptaki
     `stocks` satırlarını **aynı içerikle yeni sıra numarasıyla** yeniden damgalar (`CollectCategoryRestampsAsync`);
     ürünler akışta bir kez daha gider. Sürüm 9'un ilk tam okuması da budur: her cihaz, elindeki ürünleri kategorili
     olarak bir kez yeniden alır. Birebir aynı alt grup tekrar yüklenirse hiçbir şey hareket etmez.
   - **Bilinen sınır (Codex incelemesi, PR #210):** artımlı okuma `sta_iptal` olan alt grubu göndermez (reader'daki tüm lookup
     türleri gibi: `iptal = 0` süzgeci, silme mezar taşı yok). Bu yüzden Mikro'da pasife alınan bir alt grup, bir sonraki tam
     okumaya (projeksiyon sürümü artışı ya da elle yeniden okuma) kadar telefonlarda eski kategori adını korur; ürün
     yeniden atanmışsa yeni alt grup zaten doğru adı taşır. Mikro dolu bir alt grubun silinmesine pek izin vermediği için
     mezar taşı eklenmedi; gerekirse tüm lookup türleri için ortak bir silme yolu gerekir.
   - Telefon `UrunDto.kategori`'yi zaten okur (`BridgeDeltaSync`, `BridgeSyncHelper.syncUrunler`); uygulama değişmedi.
   - Testler: `AndroidEndpointsTests.Product_catalog_names_the_category_after_the_stock_sub_group`,
     `MobileEntityAssemblyTests.A_product_carries_its_stock_sub_group_name_as_category`,
     `MobileEntityAssemblyTests.A_new_or_renamed_sub_group_resends_only_the_products_in_it`.

35. **Kullanıcı görünüm tercihleri merkezdedir: `Endpoints/MobileUserPreferencesEndpoints`, tablo `mobile_user_preferences` (2026-09-30).**
   - **Tek JSON belge:** kullanıcı başına bir satır (`UserId` PK, `TenantId`, `Json` jsonb, `Version`, `UpdatedAtUtc`). Sunucu belgeyi yorumlamaz;
     şekil telefonda (`ViewPrefs`). `GET/PUT /api/v1/android/account/preferences`; kullanıcı yalnız jetondan gelir, yani kimse başkasınınkini okuyamaz.
     PUT belgeyi tümüyle değiştirir ve `Version`'ı artırır; gövde `{ "data": {…} }`, `data` nesne ve ≤ 16 KB olmalı (`400 INVALID_PREFERENCES`).
   - **Eşitleme telefonda karara bağlanır:** yerelde bekleyen değişiklik kazanır (son yazan), yoksa daha yüksek `Version` alınır. Kullanıcı silinince satırı da gider (cascade).
   - Test: `UserPreferencesRelationalTests`. Telefon ayağı: Siparis_Cepte KB kural 54.

36. **Müşteri kataloğu: gerçek istemci IP'si, giriş yavaşlatıcı, katalog tabloları, kilitli yetki, müşteri oturumu, talepler, belge bağı ve panelden siparişe çevirme (GOAL_MUSTERI_KATALOGU S1–S10, 2026-10-01).**
   - **Gerçek istemci IP'si:** `Security/ForwardedHeadersSetup`. `ForwardedHeaders:KnownNetworks` / `ForwardedHeaders:KnownProxies`
     (dizi ya da virgüllü tek değer; boş girdi yok sayılır) doluysa pipeline'ın **en başında** `UseForwardedHeaders`
     (`X-Forwarded-For` + `X-Forwarded-Proto`, `ForwardLimit=1`, varsayılan loopback güveni temizlenir; `X-Forwarded-Host` asla —
     katalog host denetimi atlatılırdı). Ayar boşsa ara katman eklenmez (eski davranış). Bozuk girdi başlatmayı durdurur.
     `CustomerCatalog:PublicHost` doluyken bu ayar test dışında **zorunlu** (`Program.ValidateRuntimeConfiguration`): aksi hâlde
     bütün firmaların katalog girişleri Traefik'in tek IP kovasına düşerdi. `AdminAuthEndpoints.ResolveClientIp` artık ham
     `X-Forwarded-For` başlığını değil `RemoteIpAddress`'i okur (güvenilmeyen istemci IP uyduramaz). `Security/UntrustedForwardWarning`
     (pipeline'ın başında, ayar olsa da olmasa da): güvenilmeyen adresten `X-Forwarded-For` gelen **ilk** istekte bir kez
     `app.Logger` uyarısı (adres + 64 karaktere kısaltılmış, denetim karaktersiz başlık) — Traefik arkasında ayar eksik/yanlış demektir.
   - **Bölümleme:** `Anonymous` politikası ve global limiter anahtarı `Security/ClientIpPartition.Of`: IPv4 aynen, IPv6 **/64 öneki**
     (`2001:db8:1:2::/64`; sağlayıcı müşteriye bütün bir /64 verir). Yeni katalog politikaları (S5/S6) da bunu kullanır.
     `per-catalog-account` ve `catalog-upload` `sub` yoksa (süresi geçmiş çerez, oturumsuz çağrı) ortak "anonymous" kovası yerine
     `Program.SignedOutPartition` = `ip:{ClientIpPartition}`: bir ziyaretçinin 401'leri başkasının bütçesini tüketmez.
   - **429 gövdesi:** `AddRateLimiter.OnRejected` ve giriş yavaşlatıcı aynı `Security/RateLimitedResponse`'u yazar:
     `ApiError{errorCode:"RATE_LIMITED", traceId}` + `Retry-After` (tam saniye, yukarı yuvarlanmış, en az 1). Telefon ve panel 429'u
     durum koduyla işlediği için geriye uyumlu.
   - **Giriş yavaşlatıcı** `Security/LoginThrottle` (singleton, bellek içi — tek container, `TenantEventHub` gibi; en çok
     `DefaultMaxEntries` = 100.000 anahtar: aşılınca önce sessizler, sonra en eski bekletilmeyenler, en son bekletilenler atılır,
     denemesi süren anahtar atılmaz). Anahtar = alan (`staff` / `catalog` / `admin`) + firma kodu (büyük harf) + ad (küçük harf) +
     isteğe bağlı istemci bölümü; hesap ya da firma olmasa da sayılır. 15 dk içinde 5 hata → 60 sn bekleme; sonraki her hata
     beklemeyi ikiye katlar (üst sınır 15 dk). Beklerken şifre **doğrulanmaz**: doğru ve yanlış şifre aynı 429'u alır. Başarılı
     giriş adı temizler; son hatadan **ve** son beklemenin bitiminden 15 dk sessizlik adı affeder. Hesap hiç kilitlenmez.
     **API:** `using var attempt = throttle.TryBegin(alan, firma, ad, istemci?)` → `attempt.RetryAfter` doluysa 429; şifre
     sonucuna göre `attempt.Failed()` / `attempt.Succeeded()`; bitirilmeyen deneme (hata/istisna) hiçbir şey saymaz. Devam eden
     denemeler adın kilidi altında sayılır: aynı anda en çok kalan hata hakkı kadar deneme yürür (bir kez bekletilmiş adda tek),
     fazlası `InFlightWait` (1 sn) ile 429 — paralel 20 tahmin eşiği geçemez.
     **Anahtarlar:** personel (`MobileAccountEndpoints.LoginAsync`, telefon + panel) = firma + kullanıcı adı + `ClientIpPartition`
     (başka adresten deneyen kullanıcıyı dışarıda bırakamaz); kullanıcının kayıtlı ve aktif telefonu (`mobile_devices`: aynı firma,
     istekteki `DeviceId`, `IsActive`, `LastUserId` = kullanıcı; yalnız `client=android`) kendi sayacını kullanır
     (`#{userId}@{deviceId}`, adressiz) — panelin cihaz kimliği `web-portal:{kullanıcı}` tahmin edilebildiği için muaf değil.
     Admin (`AdminAuthEndpoints.LoginAsync`) = e-posta + `ClientIpPartition`; bilinmeyen e-postada da `PasswordHashing.Dummy` ile
     BCrypt (yanıt süresi e-postanın varlığını söylemez). Katalog (`CatalogArea`, S6) = firma + kullanıcı adı; güncel cihaz çerezli
     tarayıcı ve oturumdaki şifre değişikliği `#{hesapId}`. **Sınır:** panel girişleri CentralApi'ye Portal konteynerinin, Admin
     girişleri Admin konteynerinin adresinden gelir — orada adres ayrımı yoktur (aynı adı yabancı ve sahibi aynı kovada denerse
     sahibi de bekler); telefon ve katalog girişleri gerçek istemci adresiyle gelir.
   - **Tablolar** (03 §2, migration `MusteriKatalogu`): `catalog_settings`, `catalog_category_settings`, `catalog_product_settings`,
     `catalog_accounts`, `catalog_images`, `catalog_image_blobs`, `catalog_orders` — `Domain/CustomerCatalog.cs`. Katalog hesapları
     **koltuk değildir**, `mobile_users`'a girmez, personel oturumu açamaz.
   - **Modül ve yetki:** `TenantModules.CustomerCatalog = "customer_catalog"` (`Known`; oturumun `modules[]`'ına kendiliğinden girer).
     `PermissionKeys.CustomerCatalogManage = "action.customer_catalog.manage"` **kilitli** (`locked: true`), yalnız ADMIN + MANAGER:
     rol şablonunda ve kişiye özel açma/kapama `409 PERMISSION_LOCKED`; `PermissionCatalog.Version` = 2;
     `RolePermissions.CanManageCustomerCatalog`. Bildirim türü `UserNotificationKinds.CatalogOrderNew = "CATALOG_ORDER_NEW"`.
   - **Yapılandırma** `CustomerCatalog/CustomerCatalogOptions` (`CustomerCatalog` bölümü): `PublicHost` (boş = katalog hiçbir yerde
     sunulmaz), `PublicBaseUrl`, `TokenDays` 30, `SessionHours` 12, `MaxImageBytesLarge` 1 MB, `MaxImageBytesSmall` 200 KB,
     `MaxImagesPerProduct` 8, `TenantImageQuotaBytes` 1 GB, `MaxOpenOrders` 20, `MaxOrderLines` 200, `WebRoot` `wwwroot/katalog`.
     `docker-compose.coolify.yml` `CustomerCatalog__PublicHost`, `CustomerCatalog__PublicBaseUrl`, `ForwardedHeaders__KnownNetworks__0`
     değişkenlerini boş varsayılanla geçirir (Coolify'da doldurulur, D3).
   - **Firma kodu güvencesi:** `MobileSeatService.EnsureTenantCodeAsync` — kodu olmayan firmaya (hiç koltuk almamış olabilir) ilk
     okumada 8 karakterlik kod üretir; yalnız kod hâlâ boşsa yazar, eşzamanlı iki okuma aynı kodu döner.
   - **Görseller (S5)** `Endpoints/CustomerCatalogImageEndpoints` + `CustomerCatalog/CatalogImages`: baytlar PostgreSQL'de
     (`catalog_image_blobs`, iki varyant); sunucuda görsel kütüphanesi yok — küçültmeyi gönderen yapar, sunucu yalnız bayt sınırını,
     sihirli baytı (`TaskService.LooksLike`) denetler ve üst veriyi kütüphanesiz atar (`CatalogImages.StripMetadata`: JPEG APP1
     EXIF/XMP; PNG `eXIf`/`tEXt`/`iTXt`/`zTXt` parçaları — CRC parça başına olduğu için kalanlar geçerli; WebP `EXIF`/`XMP `
     parçaları, VP8X bayraklarından 0x08/0x04 silinir, RIFF boyu yeniden yazılır; okunamayan dosya olduğu gibi kalır). JPEG yön
     bilgisi EXIF'le gider: telefon ve panel görseli zaten döndürüp yeniden kodlayarak gönderir. Bağlantı görseli hiç indirilmez.
     Görsel yazımları `WriteLayoutAsync(..., pictures: true)` ile aynı `catalog_settings` satır kilidinden geçer (kota yarışı yok)
     ama düzen `Revision`'ını değil **`ImageRevision`**'ı artırır (migration `KatalogGorselSayaci`): görsel yüklemesi panelin
     `Katalog.razor`'ında ya da telefonda açık düzen düzenlemesini `409 CATALOG_CHANGED`'e düşürmez; değişiklik yoksa sayaç artmaz.
     `CatalogViewService` önbelleği `StockVersion` + `Revision` + `ImageRevision`'a bakar; müşteri isteği yenileme aralığında (5 sn)
     ve iki revizyon saklananlarla aynıyken firmanın derleme kilidini beklemeden önbellekteki görünümü alır (tek satır okuma).
     Anonim `GET /api/v1/catalog/img/{id}/{s|l}` tahmin edilemez kimlikle, `immutable` önbellekle ve yalnız firma aktif ve modül
     açıkken sunulur; firmanın `IsEnabled`'ına bakılmaz (panel ve telefon katalog yayından önce görselleri gösterir). Hız politikaları `catalog-upload` (kullanıcı başına 300/dk) ve `catalog-public`
     (IP başına 600/dk, `ClientIpPartition`). Ayrıntı: `docs/api-contracts.md` "Müşteri kataloğu yönetimi".
   - **Müşteri oturumu (S6)** `Endpoints/CustomerCatalogPublicEndpoints` (`/api/v1/catalog/{code}`): token
     `JwtIssuer.IssueForCatalogAccount` (`scope=customer-catalog`, `tv` = hesabın `TokenVersion`'ı) yalnız HttpOnly çerezde
     (`CustomerCatalog/CatalogCookies`: `__Host-kt_{KOD}`, Secure, SameSite=Strict, `Path=/`; JwtBearer `OnMessageReceived` çerezi
     yalnız o kodun yolunda ve `Authorization` yokken okur). `CatalogCustomerPolicy` = kapsam + `CatalogAccountStateRequirement`
     (`Authentication/CatalogAccountStateHandler`): her istekte rota kodu, hesap (silinmemiş, aktif, `tv`), firma/modül/`IsEnabled`
     (`CatalogCustomerAccess.IsOpenAsync`) ve abonelik; ret `CatalogAccountStateHandler.DenialItemKey` ile **mevcut**
     `MobileUserAuthorizationResultHandler` üzerinden yazılır (ikinci result handler kaydı telefonların `SESSION_REVOKED`
     gövdelerini bozar). Oturum `CatalogSession.Of(http)` ile uçlara geçer (hesap ve firma yeniden okunmaz). Katalog token'ı başka
     hiçbir politikadan geçmez. Değiştirici her istek (giriş dahil) `X-Katalog: 1` + `Origin == https://{PublicHost}` (varsa) ister
     (`403 CSRF_REJECTED`); bütün yanıtlar `private, no-store`. Giriş: firma başına 300/dk kova ve aynı anda en çok 8 BCrypt
     (`CatalogLoginGate`, bellek içi), IP başına 10/dk (`catalog-login`), ad yavaşlatıcısı (`LoginThrottle.CatalogArea`), ortak
     sahte hash `Security/PasswordHashing.Dummy` (personel ve Admin girişiyle paylaşılır). İmzalı cihaz çerezi `__Host-kt_dev`
     (`hesap.tokenVersion.bitiş.hmac`, `Jwt:SigningKey`'den türetilmiş HMAC; `CatalogCookies.DeviceOf`) imzası veritabanından ve
     firma kovasından **önce** doğrulanır; hesabın güncel `TokenVersion`'ıyla eşleşen tarayıcı firma kovasını harcamaz ve
     `#{hesapId}` sayacını kullanır: yabancının hataları ya da kalabalık müşteriyi dışarıda bırakmaz. Başka hesabın ya da eski
     sürümün çerezi muaf değildir (kova yine uygulanır). `TokenVersion` artışları atomik
     (`CatalogAccounts.RevokeSessionsAsync`: `ExecuteUpdate TokenVersion + 1`, izlenen satır yeni değeri değiştirilmiş sayılmadan
     alır; hesabın kendi kaydından sonra çağrılır). Müşteri kendi şifresini değiştirince: mevcut şifre yanlışsa `#{hesapId}` sayılır
     (giriş sayfasındaki yabancı engelleyemez), `TokenVersion` +1, kendi tarayıcısına yeni oturum çerezi ve yeni sürümde cihaz çerezi.
   - **Müşteri gezinme (S7)** `CustomerCatalog/CatalogCustomerView` (hesabın görünürlüğü + etkin listesinde fiyatı olan ürünler,
     iskonto `noDiscount`'ta 0; stok `inStock` = her depo telefondaki gibi ayrı yuvarlanıp **sonra** toplanır, ≥ 1 —
     `CatalogViewService.IsInStock`; iki depoda 0,4 stokta yok sayılır; adet gösterilmez) ve `CatalogQuote` (satır sorunları `NOT_AVAILABLE|OUT_OF_STOCK|CARTON_MULTIPLE|INVALID_QUANTITY`;
     görünmeyen ürün hakkında hiçbir bilgi dönmez; toplamlar yalnız sorunsuz satırlar). Arama `CatalogProduct.Matches`: tr-TR
     `IgnoreCase | IgnoreNonSpace` (ç ğ ı ö ş ü Türkçede ayrı harf olarak kalır; "kagit" "kâğıt"ı bulmaz).
   - **Web barındırma (W0)** `CustomerCatalog/CatalogWeb`: yalnız `Host == CustomerCatalog:PublicHost` iken (boşsa hiç yok);
     `UseRouting`'den ve hız sınırından önce `/assets/{v}/…` (`v` = WebRoot dosyalarının SHA-256'sı, ilk 10 hex, açılışta;
     `immutable`; yanlış `v` `404 no-store`), `/robots.txt`, `/favicon.svg` ve izin listesi (o host'ta yalnız `/api/v1/catalog/**`,
     `/health*` ve kabuk yolları); kabuk `/{code}`, `/{code}/{**rest}` `RequireHost` uçları (`%V%`, `%TITLE%`, 5 dk başlık
     önbelleği, bilinmeyen kod aynı sayfa 404). Bu host'ta her yanıt CSP, `X-Robots-Tag: noindex`, `Referrer-Policy`, HSTS,
     `nosniff` taşır. Web dosyaları `wwwroot/katalog` (Web SDK publish'e kendiliğinden alır; Dockerfile değişmez).
   - **Sipariş talepleri (S8)** — müşteri `Endpoints/CatalogCustomerOrderEndpoints` (`POST/GET orders`, `orders/detail?id=`),
     personel `Endpoints/CustomerCatalogOrderEndpoints` (`/api/v1/customer-catalog/orders*`; modül var, yönetim yetkisi yok:
     yönetici hepsini, diğerleri `AssignedUserId`/`ClaimedByUserId` kendisi olanları görür), çekirdek `CustomerCatalog/CatalogOrders`.
     Gönderimde önce doğrulama ve fiyat (`CatalogQuote`; `422 CART_INVALID`/`409 PRICE_CHANGED` + `quote`), sonra tek işlemde
     **hesap satırının kilidi** (taşınabilir `FOR UPDATE`: boş `ExecuteUpdate`, PostgreSQL'de satır kilidi, SQLite'ta yazma kilidi)
     → açık talep sınırı → talep + bildirimler; sayaç (`ReserveAsync`) `SaveChanges`'in hemen önünde (kural 11); commit sonrası
     `Publish(Tasks)`. `requestId` = talebin PK'sı: aynı hesap → aynı talep (201), başka hesap → içeriksiz `409 REQUEST_ID_CONFLICT`.
     Personel eylemleri (`claim{force}`/`release`/`complete`/`reject`) talep satırının kilidi altında (aynı anda iki kişi → biri 409).
     Bildirim `CATALOG_ORDER_NEW` (`TaskId` null; `TaskService` türe göre süzmez): `AssignedUserId` + aktif katalog yöneticileri.
   - **Talep ataması (S11)** `CatalogOrders.AssigneeAsync(db, cache, tenantId, responsibleUserId, customerCode, card)` →
     `CatalogAssignee(UserId, Source)`; ilk eşleşen **aktif ve silinmemiş** kişi: (1) hesabın `ResponsibleUserId`'si
     (`responsible`); (2) kartın `SalespersonCode`'u (`cari_temsilci_kodu`) → `MobileUserErpMapping.SalespersonCode`
     (`salesperson`; kırpılmış, büyük/küçük harf duyarsız); (3) `PortalLedger.Customer.AddressSalespersonCode` — adres
     aynasındaki (`customerAddresses.salespersonCode` = `adr_temsilci_kodu`) temsilcisi dolu en küçük numaralı adres → aynı eşleme
     (`address`); (4) `ErpWriteSettings.DefaultSalespersonCode` → aynı eşleme (`default`); (5) `routePlans` aynasından
     (`PortalRecordMirror`, `catalog-route-plans`; yalnız değişen satırlar okunur) carinin durak olduğu, `isActive` ve
     `startDate` boş ya da İstanbul bugününe eşit/önce planlar — en yeni `startDate`, sonra `updatedAtUtc` — içinde ilk aktif
     atanan (kullanıcı adı, harf duyarsız; `route`; ERP'siz firmada da çalışır). Hiçbiri: `null` + `managersOnly`. Sorgu sayısı
     sabit (kullanıcılar, ayar, eşlemeler; plan aynası önbellekte). Panel önizlemesi `GET accounts/by-customer` →
     `notifyPreview {userId, userName, source}` (hesabın kayıtlı sorumlusuyla). Kullanıcı listesi (`GET android/account/users`)
     her kişinin `salespersonCode`'unu taşır; panel `Kullanicilar.razor` ERP'li + katalog modüllü firmada temsilcisi boş aktif
     SALES'e "Katalog talepleri bildirilemez" rozeti koyar. `GET orders/counts` = listenin `counts`'u veritabanında sayılmış
     (satır okunmaz): panel `MainLayout` menüsündeki "Müşteri siparişleri" rozeti onu sayfa geçişinde (yalnız yol değişince) ve
     `PortalRefreshTiming.CustomerOrders`'ta (60 sn; `PeriodicTimer`, dispose'da iptal) okur; `MusteriSiparisleri.razor` listeyi
     aynı aralıkla sessizce tazeler (meşgulken ya da kullanıcı süzgeci/sayfayı değiştirdiyse yanıt atılır; açık talep okunmaz).
   - **Talep ↔ satış bağı (T8)** `CustomerCatalog/CatalogOrderLinker.TryLinkAsync`: satış gövdesinin üst düzey `catalogOrderId`'si
     `IngestEndpoints`'te iş eklenmeden hemen önce (idempotent iş + onay/yetki denetimlerinden sonra; ERP yolunda ve ERP'siz
     yolda — o zaman defter kaydının kendi `SaveChanges`'inde) ve `ApprovalService.PostDocumentsAsync`'te (onay işleminde) okunur.
     Yalnız `documentType = sales_order` bağlanır (başka belgedeki alan yok sayılır; `Names(documentType, payload)` da öyle).
     Talep satırı kilitlenir; açıksa izlenen satır `COMPLETED` olur ve iş ile **aynı kayıtta** yazılır. Başkasının `CLAIMED`
     talebini gönderen o kişi ya da katalog yöneticisi değilse `409 CATALOG_ORDER_TAKEN` (personel `complete` ile tutarlı; gönderen =
     ingest'te `job.CreatedByUserId`, onayda `RequestedByUserId`). Başka belgeyle çevrilmiş `409 CATALOG_ORDER_ALREADY_CONVERTED` —
     ancak o belgenin işi (`Jobs`: `TenantId` + `ExternalId`, tür `sales_order`) `Failed`/`DeadLetter` ise yeni satış talebi
     devralır (`DocumentStandsAsync`). İş olmayan referans (personelin "başka yerde girildi" numarası, ya da boş) kalıcı sayılır:
     onu devretmek çift siparişe kapı açardı; düzeltme yolu `reopen`. Bilinen risk: devralınan başarısız iş sonradan elle yeniden
     denenirse (`PortalErpDocumentsEndpoints` retry) ikinci sipariş olabilir. Reddedilmiş `409 CATALOG_ORDER_CLOSED`, bilinmeyen
     `409 CATALOG_ORDER_NOT_FOUND`: iş yazılmaz. Alan yoksa hiçbir şey olmaz (eski davranış). ERP'siz defter satışı reddederse
     (`Failed` iş) `Link.Undo()` talebi önceki hâline (açık ya da devraldığı satışa bağlı) döndürür.
   - **Ortak iş yazıcı** `Jobs/SalesJobWriter` (scoped; S10): ingest, onay ve panelden çevirme işi tek yoldan yazar.
     `PlaceAsync` çağıranın işleminde çalışır ve kaydetmeyi ona bırakır (onay: `PostDocumentsAsync`, `callerIsAdmin: true`,
     `approvalRequestId`): `CatalogOrderLinker.TryLinkAsync` → ERP'siz firmada `NativeDocumentProcessor.IngestAsync` (defter
     reddederse `Link.Undo()`, iş `Failed` döner, depo kuyruğuna girmez) / ERP'li firmada `db.Jobs.Add` → `FulfillmentService.EnqueueAsync`.
     `WriteAsync` kendi başına: aynı anahtarlı iş varsa onu döner (`Idempotent`), satış ya da talep bağlı belgede kendi işlemini
     açar, `SaveChanges` + commit, `DbUpdateException`'da kazananı döner; commit sonrası ERP'siz başarılı satışta telefonlar,
     depo satırında depo ekranları uyandırılır. `IngestEndpoints` ilk mevcut-iş denetimini (onay kurallarından önce) aynen tutar.
   - **Yeniden açma** `POST /api/v1/customer-catalog/orders/{id}/reopen` (`CustomerCatalogOrderEndpoints.ReopenAsync`, talep
     satır kilidi altında; yalnız `CanManageCustomerCatalog`, değilse `403 CATALOG_MANAGE_REQUIRED`): `COMPLETED`/`REJECTED` → `NEW`;
     `DocumentRef`, `RejectReason`, `Closed*`, `Claimed*` temizlenir; açık talep değişmez. Panel `MusteriSiparisleri.razor`
     kapalı talepte "Yeniden aç" (onaylı satır içi uyarı; çevrilmiş talepte ikinci sipariş uyarısı), `PortalApiClient.ReopenCatalogOrderAsync`.
   - **Panelden siparişe çevirme (S10)** `CustomerCatalog/CatalogOrderConversion` + `CustomerCatalogOrderEndpoints`
     `GET orders/{id}/conversion` / `POST orders/{id}/convert {warehouseNo?, expectedTotal}`. Panel belge **göndermez**
     (`PORTAL_CANNOT_SUBMIT_DOCUMENTS` aynen); sunucu talepten Sipariş Cepte'nin `salesOrderPayload` gövdesini alan alan kurar:
     `CAT-SO-{Guid}` (her çevirmede yeni; başarısız işin anahtarıyla çakışmaz), `occurredAt` = `PortalReports.IstanbulTime`
     `dd.MM.yyyy HH:mm`, "Cari Borç", "Katalog siparişi KT-…" (+ `\n[Notlar: …]`), `catalogOrderId`, `priceListNo` = talebin
     listesi, `warehouseNo` yalnız varsayılandan farklı seçildiyse (ERP'li); satırda `listUnitPrice` = listenin **bugünkü** fiyatı,
     `customerDiscountPercent` = talepteki (`noDiscount` 0), satır/genel iskonto 0, `unitPrice`/`lineTotal` KDV hariç net
     (`CatalogPricing` = Mikro hesabı). Ürün yok/fiyatsız/hesabın görünürlüğünden çıkmış → `422 CART_INVALID`; toplam > 0,05 farklı
     → `409 PRICE_CHANGED`; ERP'li firmada sahibin bağlamında (`ErpWriteContextBuilder`, ajanın kira anındaki hesabı) ERP kullanıcı no /
     depo / satış belge türü yoksa iş yazılmadan `409 ERP_MAPPING_MISSING` (üçü de güncel önizlemeyle). **`CreatedByUserId` kuralı
     (kullanıcı kararı):** belge carinin plasiyeri = talebin `AssignedUserId`'si (aktif kullanıcıysa), yoksa çeviren adına; ajan
     depo/seri/temsilci/ERP kullanıcı numarasını bu kişinin eşlemesinden alır, raporlar onun sayar. Talebi kapatan (`closedBy*`,
     linker kullanıcısı) çevirendir. Erişim: katalog yöneticisi ya da talebi gören ve başkası almamış kişi; `NEW` talep önce
     çeviren adına alınır (claim, satır kilidi). Çeviren satışa karar veremiyor (`ApprovalPermissions.CanDecide(user, sale)`) ve
     firma kuralı ya da kendi yetki/limiti (`DocumentPermissionCheck`, ingest 5b/5b2 ile aynı) onay istiyorsa
     `ApprovalService.SubmitAsync` ile `CAT-APR-{Guid}` onay talebi açılır; talep onay anında kapanır ve o yolda işin sahibi onay
     isteyendir (`PostDocumentsAsync` kuralı). Aksi hâlde `SalesJobWriter.WriteAsync` (ERP'siz firmada defter reddederse
     `422 CATALOG_CONVERSION_FAILED`, talep açık). Panel `MusteriSiparisleri.razor` "Siparişe çevir" formu (kimin adına, depo seçimi,
     "Talep: x → Güncel: y", eksik eşleme uyarısı — eksikken gönderilmez; `PRICE_CHANGED`/`CART_INVALID`/`ERP_MAPPING_MISSING`'de
     form güncel önizlemeyle açık kalır), `PortalApiClient.CatalogOrderConversionAsync` / `ConvertCatalogOrderAsync`. Telefon değişmedi.
   - **Panel hata metinleri** `ErpBridge.Portal/Api/PortalMessages`: bilinen koda panelin metni; bilinmeyen kodda sunucunun mesajı
     Türkçe harf içeriyorsa (`LooksTurkish`; katalog uçları Türkçe yazar) o, değilse kodlu genel metin
     (`PortalApiClient.SendAsync` → `PortalMessages.For(code, message)`).
   - **Hesabım (S9)** `Endpoints/CatalogCustomerLedgerEndpoints`: `statement` (`PortalLedger.Statement`, açıklamasız dar DTO),
     `invoices` / `invoices/detail?key=` (izinli küme = carinin kendi `sale|sale_return`, `!OtherSide`, anahtarlı satırları; detay
     anahtarı önce bu kümede aranır, `DocumentByKey` **kullanılmaz** — kasa koduyla çakışan cari başka carinin `r` anahtarını
     açamaz), `purchased` (iptalsiz satışların `LinesByDocument` satırları stok koduna göre). Bayrak kapalı `403 FEATURE_DISABLED`.
   - Testler: `LoginThrottleTests` (paralel deneme, devam eden deneme, anahtar sınırı, adres bölümü), `ForwardedHeadersSetupTests`
     (tek seferlik uyarı), `RateLimitTests` (XFF bölümleri, güvenilmeyen atlama, /64, `catalog-login`, oturumsuz istek IP kovası),
     `LoginThrottleEndpointTests` (başka adres, kayıtlı telefon, Admin adres + sahte hash), `RuntimeConfigurationTests`, `CustomerCatalogFoundationRelationalTests`,
     `PermissionEndpointsRelationalTests`, `PermissionResolverTests`, `CustomerCatalogImagesRelationalTests`,
     `CustomerCatalogLoginRelationalTests`, `CustomerCatalogBrowseRelationalTests`, `CustomerCatalogOrdersRelationalTests`,
     `CustomerCatalogLedgerRelationalTests`, `CustomerCatalogConversionRelationalTests` (S10), `CatalogWebTests` (fixture
     `tests/ErpBridge.CentralApi.Tests/CustomerCatalog/WebFixture`, `CatalogHostFactory` = `katalog.test`), `CatalogLoginGateTests`.
