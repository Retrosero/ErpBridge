# 00 — ErpBridge Sistem Mimarisi ve Teknoloji Haritası

> **Hedef:** ErpBridge (.NET 10 Entegrasyon Ajanı & Merkezi SaaS API) projesinde
> çalışan AI kodlama editörleri ve mühendisler için temel başvuru kaynağı.
>
> **Durum:** Çok-ERP dönüşümü Faz 16–22 boyunca yapıldı. Mikro tam
> implementasyon; `Erp.Sql` ortak SQL Server change-log motoru; `Erp.Logo`
> iskelet. Detay: [`docs/multi-erp-adapter-plan.md`](../docs/multi-erp-adapter-plan.md)

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

   İstemcinin henüz okumadığı bölümler değişiklik üretmez ama imleç yine de
   üzerlerinden geçer. Bu yüzden ileride yeni bir varlık eklenirse
   `SyncCursor.FormatVersion` **yükseltilmelidir** — yoksa mevcut cihazlar o
   geçmişi sessizce kaçırır.
13. **Aynı satırı yeniden göndermek bir değişiklik değildir.**
   Ajan her döngüde aynı satırları yükler. `PayloadSha256` değişmediyse
   `UpdatedSeq` ilerletilmez — ilerletilirse tüm filo 30 saniyede bir katalogun
   tamamını yeniden indirir.

---

## 4. Yeni ERP Adaptörü Eklemek

Sözleşme, sıra ve tanım-tamamlandı listesi:
[`docs/erp-adapter-contract.md`](../docs/erp-adapter-contract.md)

Özet: adaptör yalnızca `Shared` + `Erp.Abstractions` (+ SQL Server ise `Erp.Sql`)
referans verir. `SecondAdapterSeamTests` (Logo iskeleti üzerinden) bunu mimari
değişmez olarak sabitler — o testler kırılıyorsa soyutlama gerilemiş demektir.
