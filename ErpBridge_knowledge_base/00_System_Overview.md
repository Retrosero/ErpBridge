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
└── ErpBridge.Admin/            # Blazor Server yönetim paneli
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
   `SyncCursor.FormatVersion` **yükseltilmelidir** — yoksa mevcut cihazlar o
   geçmişi sessizce kaçırır.
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
   - **Mobil kullanıcı token'ı** (`scope=mobile-user`, 30 gün) yalnızca imzayı
     kanıtlar. `MobileAccountEndpoints.AuthorizeAsync` her çağrıda kullanıcı,
     cihaz, tenant ve aboneliği veritabanından yeniden doğrular; rol token'dan
     değil satırdan okunur. Pasifleştirme ve cihaz engelleme token süresini
     beklemeden etkili olur.
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
     yok). Telefonun kasa defterinden gelen satırsız `return` / `disbursement`
     belgeleri etkisiz kayıt olarak kalır.
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

## 4. Yeni ERP Adaptörü Eklemek

Sözleşme, sıra ve tanım-tamamlandı listesi:
[`docs/erp-adapter-contract.md`](../docs/erp-adapter-contract.md)

Özet: adaptör yalnızca `Shared` + `Erp.Abstractions` (+ SQL Server ise `Erp.Sql`)
referans verir. `SecondAdapterSeamTests` (Logo iskeleti üzerinden) bunu mimari
değişmez olarak sabitler — o testler kırılıyorsa soyutlama gerilemiş demektir.
