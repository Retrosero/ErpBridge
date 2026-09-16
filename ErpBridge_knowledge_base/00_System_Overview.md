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
├── ErpBridge.Core/             # AgentConfig, ErpChangeLogSyncService, BootstrapSyncService
├── ErpBridge.LocalStore/       # SQLite migrations, IMappingStore, SqliteErpSyncCursorStore, DPAPI
├── ErpBridge.RemoteApi/        # HttpRemoteApiClient, Polly v8, Idempotency-Key
├── ErpBridge.Erp.Mikro/        # MikroAdapter, 7 Writer, V15/V16 katalogları, Mikro trigger installer
├── ErpBridge.Erp.Logo/         # LogoAdapter iskeleti (Faz 21 — dikiş doğrulaması)
├── ErpBridge.Agent.Service/    # Windows Service: AgentWorker, BootstrapWorker, HeartbeatWorker
├── ErpBridge.Agent.UI/         # WPF Ayar Paneli
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
     telefon oturumu `CanUsePhone` (ADMIN/MANAGER/SALES) ister, yoksa 403
     `ROLE_NOT_ALLOWED_ON_PHONE`; panel oturumu `CanUsePortal` (SALES dışı) ister, yoksa 403
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
     tenant'ında bu türler 409 `DOCUMENT_REQUIRES_NATIVE_TENANT` (ajanın yazıcısı
     yok). Telefonun kasa defterinden gelen satırsız `return` belgesi etkisiz kayıt
     olarak kalır (iade `sales_return` ile işlenir).
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
     yönetici panelidir (Sipariş Cepte yol haritası Faz D).
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
     `ADMIN` veya `MANAGER`. Ziyaretin `username`'i **token'daki kullanıcıdan** gelir,
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
   - **Hangi belge sayılır:** ERP'siz firmada yalnızca `Succeeded`; ERP'li firmada
     `Pending/Processing/Succeeded` (ajana yolda olan da satıştır), `Failed/DeadLetter`
     asla. İade türü ERP'sizde `sales_return`, ERP'lide kasa defterinin `return`'ü.
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
     ACCOUNTING), `Warehouse` (Depo — ADMIN, MANAGER, WAREHOUSE), `Users` (Kullanıcılar — ADMIN). Rolün
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
     firmada hiçbir satış kuyruğa girmez; açılmadan önceki satışlar geriye dönük eklenmez.
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

## 4. Yeni ERP Adaptörü Eklemek

Sözleşme, sıra ve tanım-tamamlandı listesi:
[`docs/erp-adapter-contract.md`](../docs/erp-adapter-contract.md)

Özet: adaptör yalnızca `Shared` + `Erp.Abstractions` (+ SQL Server ise `Erp.Sql`)
referans verir. `SecondAdapterSeamTests` (Logo iskeleti üzerinden) bunu mimari
değişmez olarak sabitler — o testler kırılıyorsa soyutlama gerilemiş demektir.
