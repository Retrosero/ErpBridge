# GOAL_DEPOLAMA_R2 — Firma bazlı merkezi dosya deposu (Cloudflare R2), kota ve temizlik

Onay: kullanıcı 2026-10-01 (plan onayı; görevlerin push, PR, birleştirme, sunucu dağıtımı ve Play internal yüklemesi
önceden onaylı). Durum: `GOAL_DEPOLAMA_R2_DURUM.md`. Telefon ayağı: Siparis_Cepte `docs/GOAL_DEPOLAMA_R2.md`.

## 1. Bağlam

Uygulamada resim üreten her alanın tek, kalıcı ve firmaya ayrılmış bir yerde saklanması isteniyor. Bugün resimler
dağınık durumda ve bir kısmı kaybolabiliyor.

**Bugünkü durum (2026-10-01 envanteri):**

| Alan | Bugün nerede | Sorun |
|---|---|---|
| Görev fotoğrafı | Sunucuda PostgreSQL `task_attachment_blobs` (bytea); telefonda `files/task_photos` | Kapanan görevin fotoğrafı hiç silinmez; telefondaki kopya da silinmez |
| Katalog ürün görseli ve banner | Sunucuda PostgreSQL `catalog_image_blobs` (bytea) | Ürün kaybolsa da görsel kalır |
| Gider fişi, araç bakım fotoğrafı | **Yalnız** telefonun `cacheDir`'i | Android önbelleği temizleyince kaybolur; sunucuya ve panele hiç gitmez |
| Ürün görseli (Excel, URL, elle) | Yalnız o telefonun `files/product_images` ve `product_media` klasörleri | Başka telefon ve web katalog görmez; dosyalar hiç silinmez |
| XML ürün görseli | Her telefon kendisi indirir (`files/xml_media`) | Web katalog görmez; her telefon aynı resmi ayrı ayrı indirir |
| Ürün için fotoğraf çekme/yükleme | **Yok**: yalnız metin alanı (URL / dosya yolu) | — |

Kota bugün iki ayrı ayardan geliyor: katalog için 1 GB, görev için 1 GB. Firmaya göre değiştirilemiyor; panelde yalnız
katalog kotası görünüyor.

**Hedef:**
- Bütün kalıcı resimler Cloudflare R2'de, **firma koduna göre ayrı klasörlerde** durur.
- Her firmanın tek bir depolama kotası olur. Admin'den değiştirilir; panelde kullanım ve kalan alan görünür.
- Kullanılmayan görseller panelden seçilerek temizlenebilir; güvenli durumlar kendiliğinden temizlenir.
- Telefon, panel ve web katalog aynı görseli görür.

## 2. Kararlar

**Kullanıcı kararları (2026-10-01):**
- **R1:** Depo Cloudflare R2. Firma başına klasör, klasör adı firma kodu.
- **R2:** Varsayılan kota firma başına **5 GB**, tek havuz. Admin konsolundan firmaya özel değiştirilebilir.
- **R3:** XML ürün görsellerini **sunucu kopyalar**: XML'i günde bir okur, görselleri küçültüp R2'ye koyar. Bu görseller
  kotaya sayılır. Böylece KB kural 40'taki "sunucu XML indirmez" kararı değişir.
- **R6 (kullanıcı, 2026-10-01):** XML eşitlemesi kaynağı izler: XML'de görseli **değişen** ürünün sunucudaki XML
  görseli yenisiyle değiştirilir, XML'den **silinen** görsel ya da ürün sunucudan da silinir. XML görselleri
  XML'den yeniden üretilebildiği için çöp kutusuna gitmez, doğrudan R2'den silinir.
- **R4:** Gider fişi ve araç bakım fotoğrafı R2'ye gider ve **panelde görünür**.
- **R5:** Temizlik panelden seçerek yapılır ("Alan aç"). Kendiliğinden yalnız güvenli durumlar temizlenir: silinmiş
  kayıtların dosyaları ve hiçbir kayda bağlı olmayan yetim nesneler.

**Tasarım kararları:**
- **T1 — İki kova:**
  - `…-public`: herkese açık; katalog ürün görseli, banner, XML görseli, ürün fotoğrafı.
  - `…-private`: kimliğe bağlı; görev fotoğrafı, gider fişi, araç bakım fotoğrafı.
  - Gerekçe: R2'de herkese açık erişim kova düzeyinde açılıyor; özel dosyalar herkese açık kovaya konamaz.
- **T2 — Nesne anahtarı:** `{FIRMAKODU}/{alan}/{yıl}/{ay}/{dosyaId}-{varyant}.{uzantı}`. Örnekler:
  - `ABCD2345/urun/2026/10/7f3c…-l.webp`
  - `ABCD2345/gorev/2026/10/91ab….jpg`

  Firma kodu kalıcıdır; değiştiren bir uç yok ve katalog linki, müşteri çerezi ve girişler ona bağlı. Kod boş olan
  firmada yüklemeden önce `MobileSeatService.EnsureTenantCodeAsync` ile kod üretilir. Dosya kimliği tahmin edilemez bir
  GUID'dir.
- **T3 — Yükleme sunucu üzerinden yapılır.**
  - Akış: telefon/panel → CentralApi → R2. Sunucu dosya türünü ilk baytlarından denetler, üst veriyi (EXIF/GPS)
    siler, gerekirse küçültür, kotayı kilit altında ayırır, sonra R2'ye yazar.
  - R2 anahtarı yalnız sunucuda durur; telefona hiçbir anahtar gitmez.
  - Görseller küçük olduğu için (≤1 MB) sunucu bant genişliği sorun değil.
- **T4 — İndirme:**
  - Herkese açık kova özel alan adından doğrudan, CDN önbelleğiyle sunulur. Önerilen alan adı `img.appsgo.cloud`
    (tek seviye alt alan adı, Cloudflare'de proxied).
  - Özel kovadaki dosya yalnız yetkili isteğe verilir: sunucu yetkiyi denetler, 5 dakikalık imzalı R2 adresine
    yönlendirir (302).
  - Web katalogdaki "firma kapalıysa görsel 404" kuralı herkese açık kovada kendiliğinden işlemez. Bunun yerine firma
    kapatılınca bir iş firmanın public klasörünü 30 gün sonra siler; adresler zaten tahmin edilemez.
- **T5 — Görsel işleme:** sunucuya **SkiaSharp** (MIT lisanslı) eklenir: küçültme ve WebP kodlama. XML görselleri ve
  panel/telefon yüklemeleri aynı kodla iki boyuta indirilir: l uzun kenar 1280 px, s 400 px. Banner için 1920×720 ve
  800×300. ImageSharp lisans koşulları nedeniyle seçilmedi.
- **T6 — S3 istemcisi:** `AWSSDK.S3` (Apache-2.0, R2 S3 uyumlu). İstek günlüğü kapalı tutulur.
  `LogCenter/LogScrubber` anahtar listesine `SecretAccessKey`/`AccessKeyId` eklenir (bugün eşleşmiyor).
- **T7 — Tek kayıt defteri:**
  - Her nesne `stored_files` tablosunda bir satırdır. Kota sayacı ve temizlik buna dayanır.
  - Mevcut tablolar (`catalog_images`, `task_attachments`, yeni gider eki tablosu) dosyaya `StoredFileId` ile bağlanır.
  - "Yetim" = hiçbir kayda bağlı olmayan `stored_files` satırı ya da defterde olmayan R2 nesnesi.
- **T8 — Kota ve sayaç:**
  - Firma başına `tenant_storage` satırı: `QuotaBytes?` (null = varsayılan 5 GB), `UsedBytes`, `ReservedBytes`.
    `TenantSyncCounter` gibi satır kilidiyle güncellenir.
  - Yükleme önce yer ayırır, yazım başarılı olunca kesinleştirir, hata olursa geri verir.
  - Günde bir çalışan iş sayacı defterden yeniden hesaplar ve sapmayı loglar.
  - Silinmiş ama henüz temizlenmemiş dosyalar kotaya **sayılmaz**: kullanıcı silince yeri hemen açılmış görür; bayt
    çöp kutusu süresince R2'de durur.
- **T9 — Geçiş:** mevcut bytea veriler bir arka plan göç işiyle R2'ye taşınır. Göç bitene kadar okuma ikilidir
  (`StoredFileId` boşsa bytea'dan okunur). Göç doğrulanınca bytea tabloları ayrı bir migration ile düşürülür ve
  PostgreSQL'de yer geri kazanılır.
- **T10 — R2'ye ulaşılamazsa:** yükleme `503 STORAGE_UNAVAILABLE` döner; telefon kuyruğu sonra yeniden dener.
  Bytea'ya geri düşülmez, yoksa iki yerde veri birikir.
- **T11 — Telefonda yerel kopyalar** (ürün, XML, görev) yalnız çevrimdışı önbellek olarak kalır; kalıcı yer R2'dir.
  Sahipsiz yerel dosyaları silen bir telefon temizliği eklenir (XML modülündeki `deleteOwnFile` deseni).

## 3. Veri modeli (ErpBridge, migration `MerkeziDepolama`)

| Tablo | Alanlar |
|---|---|
| `tenant_storage` (PK `TenantId`) | `QuotaBytes long?`, `UsedBytes long`, `ReservedBytes long`, `RecountedAtMs long?`, `UpdatedAtMs` |
| `stored_files` (PK `Id` uuid) | `TenantId`, `Area` (`catalog\|banner\|xml\|product\|task\|expense\|vehicle`), `Bucket` (`public\|private`), `ObjectKey` (512), `Variant` (`s\|l\|o`), `ContentType` (32), `SizeBytes`, `Sha256` (64), `OwnerType` (32), `OwnerKey` (128), `Status` (`active\|trashed\|purging`), `CreatedAtMs`, `CreatedByUserId?`, `TrashedAtMs?`, `TrashedByUserId?`. İndeksler: (`TenantId`,`Area`,`Status`); (`TenantId`,`OwnerType`,`OwnerKey`); unique (`Bucket`,`ObjectKey`) |
| `catalog_images` | `StoredFileSmallId?`, `StoredFileLargeId?` eklenir (göç bitince blob tablosu düşer) |
| `task_attachments` | `StoredFileId?` eklenir (göç bitince blob tablosu düşer) |
| `expense_attachments` (yeni) | `Id`, `TenantId`, `DocumentExternalId` (giderin telefondaki belge kimliği), `Kind` (`expense\|vehicle_maintenance`), `StoredFileId`, `CreatedAtMs`, `CreatedByUserId`, `IsDeleted` |
| `xml_image_sync` (yeni) | `TenantId`, `StockCode`, `SourceUrlHash`, `StoredFileSmallId`, `StoredFileLargeId`, `ETag?`, `LastSeenAtMs`, `LastCheckedAtMs` — XML görselinin kaynağı ve değişmediğinde atlama bilgisi |
| `TenantSubscription` | değişmez; kota `tenant_storage.QuotaBytes` ile verilir. Abonelikle birlikte değişmesi istenirse sonra taşınır |

Ayarlar `StorageOptions` (`Storage:*`; Coolify'da `Storage__*`):
- **Kurulu olanlar (2026-10-01):** R2 hesap kimliği `c14da2bbaeb637ef2c6b2a00d04d372a`; kovalar `siparis-cepte-public`
  (konum Eastern Europe, Standard) ve `siparis-cepte-private`; `img.appsgo.cloud` → `siparis-cepte-public` özel alan adı
  (Active). Ayar değerleri: `Storage__AccountId`, `Storage__PublicBucket=siparis-cepte-public`,
  `Storage__PrivateBucket=siparis-cepte-private`, `Storage__PublicBaseUrl=https://img.appsgo.cloud`.
- **Kullanıcıda kalan:** yalnız bu iki kovaya "Object Read & Write" yetkili R2 API anahtarı oluşturup
  `Storage__AccessKeyId` ve `Storage__SecretAccessKey` değerlerini Coolify'a Secret olarak girmek.
- R2: `AccountId`, `AccessKeyId`, `SecretAccessKey` (Coolify Secret), `PublicBucket`, `PrivateBucket`, `PublicBaseUrl`
  (`https://img.appsgo.cloud`).
- Sınırlar: `DefaultQuotaBytes` = 5 GB, `PresignMinutes` = 5, `TrashDays` = 7, `DeletedOwnerPurgeDays` = 30.
- `ValidateRuntimeConfiguration`: ayarlar eksikse uygulama açılır ama yükleme uçları `503 STORAGE_UNAVAILABLE` döner.

## 4. Uçlar ve akışlar

- **Ortak servis `Storage/FileStore`.** Üç işi var:
  - `PutAsync(tenant, area, owner, bytes)`: tür denetimi, üst veri silme, gerekirse küçültme, kota ayırma, R2 PUT,
    defter kaydı, kotanın kesinleşmesi.
  - `TrashAsync`
  - `UrlFor`: herkese açık dosyada doğrudan adres, özel dosyada sunucunun yönlendirme adresi.

  Mevcut uçlar bu servisi kullanacak şekilde değişir; uç yolları ve yanıt biçimleri aynı kalır, istemciler
  etkilenmez:
  - görev eki: `PUT/GET/DELETE android/tasks/{t}/attachments/{a}`
  - katalog görseli: `POST/PUT/DELETE customer-catalog/images*`
  - banner

  Görsel adresleri artık tam `https://img.appsgo.cloud/…` olur; panel ve telefon tam adresi zaten olduğu gibi
  kabul ediyor.
- **Hata kodları:** yeni `413 STORAGE_QUOTA_EXCEEDED {usedBytes, quotaBytes}`. Eski `CATALOG_IMAGE_QUOTA_EXCEEDED` ve
  `TASK_ATTACHMENT_QUOTA` kodları geriye uyum için aynı durumda döner; eski telefon sürümleri onlara bakıyor.
- **Gider ve araç fişi (yeni):**
  - Telefon fotoğrafı `filesDir/expense_photos` klasörüne taşır. Uzun kenar 1600 px'e küçültür (görev deseni), kuyruğa
    alır ve `PUT android/expenses/{docId}/attachments/{id}` ile gönderir.
  - Panel: gider/kasa hareketi detayında "Fiş" küçük resmi, tıklayınca yetkili yönlendirme ile açılır.
- **Ürün fotoğrafı (yeni):**
  - Telefonda stok detayına ve katalog yönetimine "Fotoğraf çek / Galeriden seç" eklenir. Fotoğraf ürünün sunucu
    görselidir (`area=product`) ve ERP'li firmada da çalışır: ERP kartını değil yalnız görseli değiştirir.
  - Panelde Stok sayfasındaki ürün detayına aynı yükleme gelir.
  - Ürünün görsel sırası: firmanın yüklediği → XML'den gelen → telefondaki eski yerel kopya.
- **XML görsel eşitleyici (yeni, `XmlImageSyncWorker`):**
  - Günde bir ve panelden "şimdi eşitle" ile çalışır. Firmanın kayıtlı XML adresini (`tenant_xml_feed_settings`) ve
    alan eşlemesini kullanır; ayrıştırıcı telefondaki `XmlFeedSync` kurallarıyla aynı yazılır.
  - Ürün başına en çok 6 görsel indirir, `xml_image_sync` ile değişmeyenleri atlar, iki boyuta küçültüp R2'ye koyar.
    XML eşitlemesi kaynağı izler (R6):
    - Görsel adresi ya da içeriği değişen ürünün eski XML görseli silinir, yenisi konur. İçerik değişimi ETag,
      Last-Modified ve sha256 ile anlaşılır.
    - XML'den kalkan görsel ya da ürün sunucudan doğrudan silinir; çöp kutusuna gitmez. XML eşitlemesi yarıda
      kalırsa (indirme ya da ayrıştırma hatası) hiçbir şey silinmez, ki boş ya da bozuk bir feed bütün görselleri
      silmesin.
  - Dış adres güvenliği (SSRF): yalnız http/https; DNS çözümünde özel, yerel ve link-local IP'ler reddedilir;
    yönlendirmede yeniden denetlenir. Dosya başı en çok 10 MB, istek başı 20 sn zaman aşımı, firma başı eşzamanlılık 4.
  - Telefonun kendi XML indirmesi bir süre yedek olarak kalır, sonra kapatılır (kural 40 güncellenir).
- **Kota ve kullanım:**
  - `GET /api/v1/customer-catalog/...` yerine genel `GET /api/v1/storage/usage`: alanlara göre kullanım ve kota (panel
    ve telefon).
  - Admin: `GET/PUT /api/v1/admin/tenants/{id}/storage` (kota değiştir, kullanım gör, sayacı yeniden hesapla).
- **Temizlik:**
  - `GET /api/v1/storage/cleanup/candidates?group=` aşağıdaki grupları her biri için toplam boyut ve örneklerle döner:
    - stoksuz ürünlerin görselleri (stok 0 olalı N gün),
    - katalogdan/ERP'den silinmiş ürünlerin görselleri,
    - kapanan görevlerin fotoğrafları (X günden eski),
    - süresi biten ya da pasif bannerlar,
    -     - silinmiş giderlerin fişleri.
  - `POST /api/v1/storage/cleanup {group, ids[] | all}` seçilenleri çöp kutusuna taşır; kota hemen düşer.
  - `POST /api/v1/storage/trash/restore {ids}`: çöp kutusu süresi (7 gün) dolmadan geri alma.
  - Bütün temizlik uçları yalnız yöneticiye açık (`action.storage.manage`, kilitli, ADMIN+MANAGER). Her işlem denetim
    kaydına yazılır.
- **Otomatik işler (`StorageMaintenanceWorker`, `TaskSchedulerWorker` deseni, tek container):**
  - Çöp kutusunda 7 günü dolanların R2'den silinmesi.
  - Silinmiş kayıtlara (silinmiş görev, gider, ürün kartı) bağlı dosyaların 30 gün sonra çöp kutusuna alınması.
  - 24 saati geçen yarım yüklemelerin ve defterde olmayan R2 nesnelerinin (yetimler) silinmesi; R2 listeleme ile haftada
    bir mutabakat.
  - Günlük sayaç yeniden hesabı.
  - Kapatılan firmanın public klasörünün 30 gün sonra silinmesi.

## 5. Panel ve Admin

- **Panel, yeni "Depolama" sayfası (yönetici):**
  - Kota çubuğu ("3,2 GB / 5 GB"); %80'de sarı, %95'te kırmızı uyarı.
  - Alanlara göre dağılım: ürün, XML, katalog, banner, görev, gider/araç.
  - "Alan aç" bölümü: gruplar, her grubun boyutu, örnek küçük resimler, seçerek ya da tümünü temizleme.
  - Çöp kutusu: geri alma ve kalan gün.
- **Kota görünümü başka yerlerde:** Katalog yönetimindeki "Görsel kotası" göstergesi bu birleşik kotaya bağlanır.
  Kullanıcılar sayfasındaki koltuk kartının yanına küçük bir "Depolama" özeti gelir.
- **Gider/kasa ekranları:** gider satırında fiş simgesi, tıklayınca fiş açılır.
- **Admin konsolu, firma mobil sayfası:** "Depolama: kullanılan / kota" kartı, kota değiştirme alanı, "yeniden hesapla"
  düğmesi.

## 6. Telefon (Siparis_Cepte)

- **Gider ve araç fişi:** `cacheDir` yerine `filesDir/expense_photos`; görev deseninde sıkıştırma ve kuyruk (Room'da
  yeni tablo değil, mevcut görev kuyruğu deseninin genişletilmiş hâli ya da yeni `pendingOp` kaydı — uygulamada koddan
  karar verilir; Room şeması değişirse açık migration zorunlu). Ekranda yüklenme durumu gösterilir.
- **Ürün fotoğrafı:** stok detayında ve katalog yönetiminde kamera/galeri; görsel sırası sunucu → XML → yerel.
- **Kota uyarısı:** `413 STORAGE_QUOTA_EXCEEDED` için Türkçe mesaj: "Firmanızın depolama alanı doldu. Yöneticiniz
  panelden alan açabilir."
- **Yerel temizlik:**
  - Yüklenmiş ve kapanmış görevlerin `task_photos` kopyaları silinir.
  - Silinen ya da değişen ürünlerin yerel görsel kopyaları silinir.
  - `product_images` ve `product_media` için sahipsiz dosya süpürmesi yapılır.
- **Ayarlar:** Ayarlar > Hakkında içinde yöneticiye "Firma depolaması: x / y" bilgisi.

## 7. Görevler

| # | Depo | Görev | Kabul |
|---|---|---|---|
| D0 | Claude + kullanıcı | ✅ kovalar ve `img.appsgo.cloud` (Claude, 2026-10-01). Kalan: API anahtarı ve Coolify sırları (kullanıcı), gizli olmayan `Storage__*` ayarları (Claude, S1 dağıtımında) | `curl https://img.appsgo.cloud/…` test nesnesini döner |
| S1 | ErpBridge | `StorageOptions`, `AWSSDK.S3` istemcisi, `FileStore`, `stored_files`, `tenant_storage`, migration, LogScrubber, açılış doğrulaması | Yükleme → R2 → defter; kota yarışı testi (paralel 20 yükleme kotayı aşmaz); R2 yokken 503 |
| S2 | ErpBridge | SkiaSharp ile küçültme ve WebP (Docker'da Linux native paketi doğrulanır) | 4000 px JPEG → 1280/400 WebP; EXIF yok; Docker imajında çalışır |
| S3 | ErpBridge | Katalog görseli ve banner `FileStore`'a; ikili okuma; adresler `img.appsgo.cloud` | Mevcut katalog testleri yeşil; yeni yükleme R2'de; eski bytea görsel hâlâ görünür |
| S4 | ErpBridge | Görev eki `FileStore`'a (özel kova, 302 imzalı adres) | Yetkisiz kişi 403/404; imzalı adres 5 dk sonra geçersiz |
| S5 | ErpBridge | Gider/araç fişi uçları + `expense_attachments` | Fiş yüklenir, yetkili GET 302; kota düşer |
| S6 | ErpBridge | Ürün fotoğrafı uçları (`area=product`), ürün görsel sırası | Panel ve telefondan yüklenen görsel katalogda ve telefonda görünür |
| S7 | ErpBridge | XML görsel eşitleyici (SSRF korumalı) | Değişmeyen görsel yeniden indirilmez; değişen görsel yenisiyle değişir; XML'den kalkan görsel/ürün sunucudan silinir; bozuk/boş feed hiçbir şey silmez; özel IP'ye giden adres reddedilir |
| S8 | ErpBridge | Kota ve kullanım uçları, Admin kota uçları, `action.storage.manage` | Admin kotayı değiştirir, panelde anında görünür |
| S9 | ErpBridge | Temizlik adayları, temizle/geri al, `StorageMaintenanceWorker` | Her grup için doğru adaylar; geri alma; 7 gün sonra R2'den silinir; yetim mutabakatı |
| S10 | ErpBridge | Bytea → R2 göçü (arka plan, devam edebilir), doğrulama, sonra blob tablolarını düşüren migration | Göç sayısı = kaynak sayısı; sha256 eşleşir; düşürme ayrı PR'da |
| P1 | Panel | Depolama sayfası, Alan aç, çöp kutusu | bUnit; %80/%95 uyarıları |
| P2 | Panel | Gider/kasa ekranında fiş; Stok'ta ürün fotoğrafı yükleme; katalog göstergesi birleşik kotaya | bUnit |
| P3 | Admin | Firma mobil sayfasında depolama kartı ve kota alanı | Admin testleri |
| A1 | Telefon | Gider/araç fişi kalıcı klasör + kuyruk + yükleme | Çevrimdışı çekilen fiş bağlantı gelince gider; `cacheDir`'de fiş kalmaz |
| A2 | Telefon | Ürün fotoğrafı çek/seç + görsel sırası | Çekilen fotoğraf başka telefonda ve web katalogda görünür |
| A3 | Telefon | Kota mesajı, depolama bilgisi, yerel temizlik | Temizlikten sonra kapanmış görevin yerel fotoğrafı yok |
| A4 | Telefon | XML'in telefonda indirilmesini sunucu görseline devretme (yedek olarak kalır) | Sunucu görseli varken telefon XML görselini indirmez |
| K1 | İkisi | KB: ErpBridge yeni kural (depolama), kural 36 ve 40 güncellemesi; Siparis_Cepte kural 55 ve yeni kural; yedekleme notu (PostgreSQL yedeği artık görselleri içermez; R2'de sürümleme/yaşam döngüsü) | — |

Sıra: D0 → S1 → S2 → (S3 ∥ S4 ∥ S5) → S6 → S7 → S8 → S9 → P1–P3 ∥ A1–A4 → S10 (en son, ayrı PR) → K1.

## 8. Riskler

| Risk | Önlem |
|---|---|
| R2 ile veritabanı arasında tutarsızlık (yarım yükleme, yetim nesne) | Önce R2 yazımı, sonra defter; 24 saatlik yarım yükleme temizliği; haftalık mutabakat |
| Sunucunun dış adres indirmesi (XML) güvenlik riski | Yalnız kayıtlı XML'in kendi adresleri; özel IP engeli; boyut ve süre sınırı; yönlendirme denetimi |
| SkiaSharp'ın Docker'da native bağımlılığı | S2'de Linux imajında açılış testi; gerekirse `NoDependencies` paketi |
| Herkese açık kovada firma kapatılınca görsel hemen gizlenmez | Tahmin edilemez adresler; kapanışta 30 gün sonra klasör silme |
| Kota yarışı | `tenant_storage` satır kilidiyle ayırma/kesinleştirme |
| PostgreSQL yedeği görselleri artık içermez | R2 nesne sürümleme ya da yaşam döngüsü kuralı (D0'da açılır) |
| Eski telefon sürümleri | Uç yolları ve eski kota kodları korunur; yeni alanlar geriye uyumlu |

## 9. Kullanıcıdan gerekenler (D0)

1. ✅ Cloudflare R2'de iki kova: `siparis-cepte-public` ve `siparis-cepte-private` (Claude oluşturdu).
2. Yalnız bu iki kovaya "Object Read & Write" yetkili bir R2 API anahtarı (R2 → Manage API Tokens).
3. ✅ Public kovaya özel alan adı `img.appsgo.cloud` (Claude bağladı, Active).
4. İsterseniz kovalarda nesne sürümleme ya da yaşam döngüsü (yedek yerine).
5. `Storage__AccessKeyId` ve `Storage__SecretAccessKey` değerlerini Coolify'a Secret olarak **sizin** girmeniz
   (anahtar değerlerini ben giremem, sohbete de yazmayın). Gizli olmayan ayarları (`Storage__AccountId`,
   `Storage__PublicBucket`, `Storage__PrivateBucket`, `Storage__PublicBaseUrl`) Coolify açıkken ben eklerim.
