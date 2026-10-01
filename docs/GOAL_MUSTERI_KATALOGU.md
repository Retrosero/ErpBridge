# GOAL_MUSTERI_KATALOGU — Müşteriye özel web katalog

Onay: kullanıcı 2026-10-01 (plan onayı; görevlerin push, PR, birleştirme, sunucu dağıtımı ve Play internal
yüklemesi önceden onaylı). Durum: `GOAL_MUSTERI_KATALOGU_DURUM.md`. Telefon ayağı: Siparis_Cepte
`docs/GOAL_MUSTERI_KATALOGU.md`.

## 1. Bağlam

Firma, carilerine link gönderir; cari kendisine tanımlı kullanıcı adı + şifreyle
`https://sipariscepte.appsgo.cloud/{FIRMAKODU}` adresinde katalogu görür, iskontolu fiyatını görür, sipariş talebi
gönderir; istenirse ekstresini, faturalarını, daha önce aldığı ürünleri görür. Sayfa %100 mobil uyumludur.

**Bugünkü durum (koddan doğrulandı, `origin/main` 6984b96):**
- Ürün, fiyat, stok, hareket yalnız `mobile_records` (jsonb). Okuma: `Portal/PortalRecordMirror`, `PortalStockCatalog`,
  `PortalLedger` (ekstre `Statement`, belge `Document`, hareketler `MovementsAsync`).
- Kategori telefonda Mikro alt grup adı (`Sync/StockCategories.cs`); native firmada kart üstünde düz metin.
- Sunucuda ürün görseli, cari iskontosu, sıralama/gizleme/"yalnız koli" alanı, müşteri tipi oturum **yok**.
- Her aktif `mobile_user` ücretli koltuktur (kural 14) → katalog müşterileri ayrı tabloda, ayrı JWT kapsamında.

## 2. Kararlar

**Kullanıcı kararları (2026-10-01):**
- **K1** İskonto % katalog erişiminde girilir, sunucuda saklanır; Mikro'ya yazılmaz, Mikro'dan okunmaz.
- **K2** Görseller telefondan aktarılır (https bağlantı = link; telefonda çekilmiş/yerel ya da http = küçültülmüş dosya);
  panelden de yüklenir. Sunucu dış adres **indirmez**.
- **K3** Müşteri **sipariş talebi** gönderir; personel kontrol edip normal siparişe çevirir (ERP'ye o zaman gider).
- **K4** Firma geneli tek ana katalog düzeni (kategori/ürün sırası, gizle-göster) + cariye özel istisna.
- **K5** Firma geneli varsayılan fiyat listesi; cariye farklı liste. KDV dahil/hariç etiketi listenin `includesVat`'ından.
- **K6** Stok adedi gösterilmez; stoksuz ürün "Stokta yok" rozetiyle görünür, sepete eklenemez.
- **K7** Adres `https://sipariscepte.appsgo.cloud/{FIRMAKODU}`.
- **K8** Push, PR, birleştirme, dağıtım, Play internal önceden onaylı (Actions kredisi yoksa yerel derleme + test yeşilse).

**Tasarım kararları:**
- **T1** ERP'li firmada talebi siparişe **yalnız telefon** çevirir (panel belge gönderemez,
  `IngestEndpoints` `PORTAL_CANNOT_SUBMIT_DOCUMENTS`). Panel: gör, üstüne al, reddet, "başka yerde girildi".
- **T2** Hesap **kilitlenmez**; kullanıcı adı başına yavaşlatma (5 hata / 15 dk → şifreye bakmadan 429 + `Retry-After`).
  Başarılı girişte imzalı cihaz çerezi (`__Host-kt_dev`) taşıyan tarayıcı yavaşlatmadan muaftır. Aynı yavaşlatıcı
  personel girişine (`MobileAccountEndpoints.LoginAsync`) de eklenir.
- **T3** Şifre: boş gönderilirse sunucu 10 karakterlik şifre üretir, **bir kez** `issuedPassword` döner; elle şifre
  8–72 bayt. Paylaşım mesajına şifre varsayılan olarak girmez. Müşteri web'de kendi şifresini değiştirebilir.
- **T4** "Kategori ayarından iskontosuz ürün" = ürün bayrağı `noDiscount`; kategori ekranında toplu seçim.
- **T5** Talep durumları `NEW`, `CLAIMED`, `COMPLETED`, `REJECTED`. v1'de müşteri iptali yok.
- **T6** Ek modül `customer_catalog` (operatör açar). Yetki `action.customer_catalog.manage` kilitli, yalnız ADMIN+MANAGER.
- **T7** Web: derleme adımsız, kütüphanesiz statik SPA; CentralApi yalnız `Host = CustomerCatalog:PublicHost` için sunar.
- **T8** Talep ↔ belge bağı sunucuda: satış gövdesindeki `catalogOrderId` ingest'te (ve onay anında) doğrulanır,
  aynı işlemde talep `COMPLETED` olur; ikinci belge 409. Çift sipariş olmaz.
- **T9** Stok "var" = her depo telefondaki gibi ayrı yuvarlanıp toplanınca ≥ 1. Fiyatsız ürün (etkin listede fiyatı yok)
  müşteriye görünmez; başka listeye düşülmez.

## 3. Veri modeli (`Domain/CustomerCatalog.cs`, migration `MusteriKatalogu`)

Zamanlar unix ms `long`; tablolar snake_case; her tablonun `TenantId`'si `tenants`'a cascade.

| Tablo | Alanlar |
|---|---|
| `catalog_settings` PK `TenantId` | `IsEnabled bool`, `DefaultPriceListNo int?`, `Revision long` (düzen), `ImageRevision long` (görseller; migration `KatalogGorselSayaci`), `UpdatedAtMs long`, `UpdatedByUserId uuid?` |
| `catalog_category_settings` PK (`TenantId`,`CategoryKey` varchar 160) | `SortOrder int?`, `IsHidden bool`, `UpdatedAtMs` |
| `catalog_product_settings` PK (`TenantId`,`StockCode` varchar 64) | `SortOrder int?`, `IsHidden`, `NoDiscount`, `CartonOnly`, `CartonQuantity int?` (≥2), `UpdatedAtMs`. Satır yalnız varsayılandan farklıysa |
| `catalog_accounts` PK `Id` | `TenantId`, `CustomerCode` (64), `CustomerName` (200), `Username` (64, normalize `^[a-z0-9._-]{3,64}$`), `PasswordHash` (100, BCrypt), `IsActive`, `DiscountPercent numeric(5,2)` 0–99.99, `PriceListNo int?`, `VisibilityJson jsonb` `{mode,rules}`, `ShowStatement`, `ShowInvoices`, `ShowPurchased`, `CanOrder`, `ResponsibleUserId uuid?`, `TokenVersion int`, `LastLoginAtMs?`, `PasswordChangedAtMs`, `CreatedAtMs`, `UpdatedAtMs`, `CreatedByUserId?`, `CreatedByName` (120), `UpdatedByUserId?`, `DeletedAtMs?`. Filtreli unique: (`TenantId`,`Username`) ve (`TenantId`,`CustomerCode`) where `DeletedAtMs IS NULL` |
| `catalog_images` PK `Id` (sunucu üretir) | `TenantId`, `StockCode`, `Kind` `link\|file`, `Url` (2048)?, `SourceHash` (80), `Source` `phone\|panel`, `SortOrder`, `SizeBytes int` (iki varyant toplamı), `HasSmall bool`, `HasLarge bool`, `ContentType` (32)?, `Sha256Small/Large` (64)?, `CreatedAtMs`, `CreatedByUserId?`. Unique (`TenantId`,`StockCode`,`SourceHash`) |
| `catalog_image_blobs` PK (`ImageId`,`Variant` `s\|l`) | `Data bytea`, FK cascade |
| `catalog_orders` PK `Id` (müşterinin `requestId`'si) | `TenantId`, `AccountId`, `CustomerCode`, `CustomerName`, `AccountUsername`, `No` (16, `KT-XXXXXX`, unique per tenant), `Status` (16), `Note` (1000)?, `RejectReason` (500)?, `DocumentRef` (128)?, `PriceListNo`, `PriceIncludesVat`, `DiscountPercent`, `Total numeric(18,2)`, `LineCount`, `LinesJson jsonb`, `AssignedUserId?`, `ClaimedByUserId?`, `ClaimedByName`?, `ClaimedAtMs?`, `ClosedByUserId?`, `ClosedByName?`, `ClosedAtMs?`, `SubmittedAtMs`, `UpdatedAtMs`. İndeks (`TenantId`,`Status`,`SubmittedAtMs`), (`TenantId`,`AccountId`,`SubmittedAtMs`), (`TenantId`,`DocumentRef`) |

Ek: `TenantModules.CustomerCatalog = "customer_catalog"` (`Known`), `PermissionKeys.CustomerCatalogManage =
"action.customer_catalog.manage"` (`Flag(..., managers, server: true, locked: true)`, `PermissionCatalog.Version` +1),
`RolePermissions.CanManageCustomerCatalog`, `UserNotificationKinds.CatalogOrderNew = "CATALOG_ORDER_NEW"`.
`Tenant.Code` boşsa katalog ayarı ilk okunduğunda `MobileSeatService.NewTenantCodeAsync` ile üretilir.

## 4. Katalog derleme kuralları (`CustomerCatalog/`)

- **Kaynak:** `PortalRecordMirror<PortalRecords.StockPart>.For(cache, "stock", tenantId, PortalRecords.StockEntities,
  PortalRecords.ParseStock)`. `ParseStock` eklemeli genişler: `SubGroupPart(Code, Name)` (lookups `stock_sub_group`),
  `CardPart.Category` (`kategori`/`category`), `CardPart.CartonCode` (`cartonCode`/`koliAdet`/`sto_kalkon_kodu`),
  `LookupPart.IncludesVat`.
- **Kategori anahtarı** = telefonun gösterdiği ad, kırpılmış: `StockCategories.Resolve(main, sub) ?? card.Category ?? "Diğer"`.
  Müşteri API'sinde kategori kimliği = SHA-256(anahtar) ilk 12 hex.
- **ERP koli** = `cartonCode` metni kırp, `,`→`.`, sayıya çevir; tam sayı ve > 1 ise geçerli (telefon `quickBoxQuantity`).
  Etkin koli = `CartonQuantity ?? ERP koli`. `CartonOnly` yalnız etkin koli ≥ 2 iken geçerli.
- **Stok** = her deponun miktarı ayrı ayrı `Math.Round(AwayFromZero)` ile yuvarlanır, sonra toplanır; toplam ≥ 1 → `inStock`
  (telefonla aynı: iki depoda 0,4 → 0 + 0 = stokta yok; tek depoda 0,5 → 1 = stokta). `CatalogViewService.IsInStock`.
- **Etkin fiyat listesi** = `account.PriceListNo ?? settings.DefaultPriceListNo ?? (liste 1 varsa 1, yoksa en küçük)`.
  Ürünün o listede fiyatı yoksa müşteri görmez.
- **Sıra:** kategoriler `SortOrder` (null en sona), sonra ad (tr-TR). Ürünler kategori içinde `SortOrder` (null en sona),
  ad (tr-TR), kod.
- **Görünürlük** (`CatalogVisibility.IsVisible`), öncelik sırası:
  1. ürün kuralı varsa → `effect == allow`
  2. ürün gizliyse → hayır
  3. kategori kuralı varsa → `effect == allow`
  4. mod `only` ise → hayır
  5. kategori gizli değilse → evet
  Arayüz eşlemesi: "Ana katalog" = `all` + kural yok; "Seçilenler hariç" = `all` + deny kuralları; "Yalnız seçilenler" =
  `only` + allow kuralları; "bu cariye göster" (global gizliyi açma) = allow kuralı.
- **İskonto ve fiyat** (`CatalogPricing`, `Siparis_Cepte data/erp/ErpSalePricing.kt` ile birebir): `d = noDiscount ? 0 :
  account.DiscountPercent`. Gösterim `net = R2(list × (1 − d/100))`. Satır: `unitExVat = includesVat && vat≠0 ? list/(1+vat/100)
  : list` (yuvarlanmaz); `gross = R2(unitExVat × qty)`; `disc = R2(gross × d/100)`; `vatAmt = R2((gross−disc) × vat/100)`;
  `total = gross − disc + vatAmt`. R2 = 2 hane `MidpointRounding.AwayFromZero`. KDV oranı `card.VatRate` (`kdvOrani`).

## 5. Uç sözleşmesi

JSON camelCase. Hata gövdesi `ApiError {errorCode, message, traceId}`; `PRICE_CHANGED` ve `CART_INVALID` ek olarak `quote`
taşır. 429 gövdesi `{errorCode:"RATE_LIMITED"}` + `Retry-After`. Stok kodu, cari kodu, kategori anahtarı, belge anahtarı
**URL yolunda taşınmaz** (sorgu ya da gövde); yolda yalnız GUID ve firma kodu olur.

### 5.1 Yönetim — `/api/v1/customer-catalog` (telefon + panel ortak)

Politika `MobileUserPolicy` + `PerMobileUserRateLimitPolicy`; görsel yüklemede `catalog-upload`. Önce modül
(`403 MODULE_NOT_ENABLED`), sonra `CanManageCustomerCatalog` (`403 CATALOG_MANAGE_REQUIRED`). Talep uçları (`orders*`)
yönetim yetkisi istemez: yetkili tümünü, diğerleri yalnız kendine atananı görür.

| Yöntem ve yol | Gövde → Yanıt |
|---|---|
| `GET settings` | → `Settings {isEnabled, defaultPriceListNo, effectiveDefaultPriceListNo, revision, tenantCode, publicUrl, priceLists[{no,name,includesVat}], imageQuota{usedBytes,limitBytes}, counts{categories,products,visibleProducts,accounts,openOrders}}` |
| `PUT settings` | `{revision, isEnabled, defaultPriceListNo}` → `Settings`. Eski revizyon `409 CATALOG_CHANGED`; bilinmeyen liste `400 UNKNOWN_PRICE_LIST` |
| `GET categories` | → `{revision, items[{key, name, sortOrder, hidden, productCount, hiddenCount, noDiscountCount, cartonOnlyCount}]}` (etkin sırada) |
| `PUT categories` | `{revision, items[{key, hidden}]}` — dizi sırası = kategori sırası (tam liste) → `{revision}` |
| `GET products?category=&q=` | → `{revision, truncated, items[Product]}`; `Product {stockCode, name, unit, brand, categoryKey, sortOrder, hidden, noDiscount, cartonOnly, cartonQuantity, erpCartonQuantity, listPrice, inStock, imageCount, thumbUrl}`. `category` ile kategorinin tamamı (en çok 5000), `q` ile tüm katalogda en çok 50 |
| `PUT products` | `{revision, items[{stockCode, sortOrder, hidden, noDiscount, cartonOnly, cartonQuantity}]}` (en çok 5000; verilmeyen ürün değişmez) → `{revision}`. Etkin koli yokken `cartonOnly` `400 CARTON_QUANTITY_REQUIRED` |
| `GET accounts?q=&page=` | → `{items[AccountSummary], total}`; `AccountSummary {id, customerCode, customerName, username, isActive, discountPercent, priceListNo, lastLoginAtMs, openOrderCount}` |
| `GET accounts/by-customer?code=` | → `{account: Account|null, customerName, suggestedUsername}`; cari yok `404 CUSTOMER_NOT_FOUND` |
| `POST accounts` | `{customerCode, username, password?, isActive, discountPercent, priceListNo?, visibility{mode,rules[{type,key,effect}]}, showStatement, showInvoices, showPurchased, canOrder, responsibleUserId?}` → `201 {account, issuedPassword?}` |
| `PATCH accounts/{id}` | aynı alanlar (customerCode ve password hariç), hepsi isteğe bağlı → `{account}` |
| `PUT accounts/{id}/password` | `{password?}` → `{issuedPassword?}` |
| `POST accounts/{id}/revoke-sessions` | → 204 |
| `DELETE accounts/{id}` | → 204 (yumuşak) |
| `GET images/manifest` | → `{usedBytes, limitBytes, items[{stockCode, images[Image]}]}`; `Image {id, kind, url, sourceHash, source, sortOrder, hasSmall, hasLarge, thumbUrl, fullUrl}` |
| `PUT images/links` | `{items[{stockCode, links[{url, sourceHash}]}]}` (en çok 500 ürün; o ürünün `phone` kaynaklı link görsellerini değiştirir) → `{updated}` |
| `POST images` | `{stockCode, sourceHash, source}` → `{image: Image}` (idempotent) |
| `PUT images/{id}/{s\|l}` | ham gövde, `Content-Type` jpeg/png/webp → 204 |
| `PUT images/order?stockCode=` | `{ids[]}` → 204 |
| `DELETE images/{id}` | → 204 |
| `GET orders?status=&q=&page=` | → `{items[OrderSummary], total, counts{new,claimed,completed,rejected}}`; `OrderSummary {id, no, customerCode, customerName, status, total, lineCount, submittedAtMs, assignedUserName, claimedByUserId, claimedByName, claimedAtMs}` |
| `GET orders/{id}` | → `OrderDetail` = summary + `{note, priceListNo, priceListName, priceIncludesVat, discountPercent, rejectReason, documentRef, closedByName, closedAtMs, lines[{stockCode, name, unit, quantity, cartonQuantity, listPrice, discountPercent, vatRate, gross, discount, vat, total, inStockNow}]}` |
| `POST orders/{id}/claim` | `{force}` (`force` yalnız yetkili) → `OrderDetail`; başkasında `409 CATALOG_ORDER_TAKEN`, kapalı `409 CATALOG_ORDER_CLOSED` |
| `POST orders/{id}/release` / `complete {documentRef}` / `reject {reason}` | → `OrderDetail` |
| `POST orders/{id}/reopen` | → `OrderDetail`. Yalnız yönetim yetkili (`403 CATALOG_MANAGE_REQUIRED`): `COMPLETED`/`REJECTED` → `NEW`; `documentRef`, `rejectReason`, `closedBy*`, `claimedBy*` temizlenir; açık talepte değişiklik yok. Panel: onaylı uyarıyla "Yeniden aç" |

Görsel boyutu: `l` ≤ 1 MB ve uzun kenar ≤ 1280; `s` ≤ 200 KB ve ≤ 400 (boyut istemci sorumluluğunda; sunucu bayt
sınırını ve sihirli baytı denetler, `415 INVALID_IMAGE`, `413 IMAGE_TOO_LARGE`). Ürün başına en çok 8 görsel
(`409 CATALOG_IMAGE_LIMIT`), firma kotası 1 GB (`413 CATALOG_IMAGE_QUOTA_EXCEEDED`). Üst veri atılır: JPEG APP1 (EXIF/XMP), PNG
`eXIf`/`tEXt`/`iTXt`/`zTXt`, WebP `EXIF`/`XMP ` (istemci görseli zaten döndürüp yeniden kodlar; yön bilgisi gerekmez). Görsel
yazımları `ImageRevision`'ı artırır, düzen `revision`'ını değil (açık düzen düzenlemesi 409 almaz).
Link: yalnız `https`, port 443, IP/`localhost`/`.local` host yok, ≤ 2048 (`400 INVALID_IMAGE_URL`).

### 5.2 Müşteri — `/api/v1/catalog/{code}`

`info`, `login` anonim (`catalog-login` / `catalog-public`); diğerleri `CatalogCustomerPolicy` (`scope=customer-catalog`,
`CatalogAccountStateRequirement` ile her istekte firma/abonelik/modül/`IsEnabled`/hesap/`TokenVersion`/rota kodu
denetimi) + `per-catalog-account`. Değiştirici isteklerde `X-Katalog: 1` ve `Origin == https://{PublicHost}` zorunlu
(`403 CSRF_REJECTED`). Çerez `__Host-kt_{CODE}` (HttpOnly, Secure, SameSite=Strict, Path=/; "beni hatırla" 30 gün,
değilse oturum çerezi + 12 saatlik JWT).

| Yöntem ve yol | Gövde → Yanıt |
|---|---|
| `GET info` | → `{companyName, code}`; yok/modül kapalı/`IsEnabled=false` → `404 CATALOG_NOT_FOUND` |
| `POST login` | `{username, password, remember}` → `{me: Me}` + çerezler. Hatalı `401 INVALID_CREDENTIALS`; yavaşlatma `429 RATE_LIMITED`; sonra `403 ACCOUNT_INACTIVE`, `403 CATALOG_UNAVAILABLE`, `403 SUBSCRIPTION_*` |
| `POST logout` | → 204 |
| `GET me` | → `Me {companyName, code, customer{code,name}, username, discountPercent, priceList{no,name,includesVat}, features{order,statement,invoices,purchased}, balance{amount}|null}` (`balance` yalnız `statement` açıkken) |
| `POST password` | `{current, next}` → 204; yanlış `400 INVALID_CREDENTIALS`, zayıf `400 INVALID_PASSWORD` |
| `GET categories` | → `{items[{id, name, count}]}` (yalnız görünür ürünü olanlar, sıralı) |
| `GET products?category=&q=&page=1&pageSize=48` | → `{items[CProduct], total, page, pageSize}`; `CProduct {key, code, name, unit, brand, categoryId, price{list, net, discountPercent, includesVat}, box{qty, only}|null, inStock, thumb|null}` (`thumb` = görsel URL'si; `pageSize` en çok 60; `q` ≥ 2 karakter, ad/kod/barkod/marka, tr-TR) |
| `GET products/detail?key=` | → `CProduct` + `{images[{thumb, full}]}`; görünmüyorsa `404 NOT_FOUND` |
| `POST cart/quote` | `{lines[{key, quantity}]}` → `Quote {lines[{key, code, name, unit, quantity, box, price, vatRate, gross, discount, vat, total, issue}], totals{gross, discount, vat, total}}`; `issue` ∈ `NOT_AVAILABLE`, `OUT_OF_STOCK`, `CARTON_MULTIPLE`, `INVALID_QUANTITY` ya da null |
| `POST orders` | `{requestId (uuid), lines[{key, quantity}], note, expectedTotal}` → `201 {order: COrder}`. `409 PRICE_CHANGED {quote}` (fark > 0,05), `422 CART_INVALID {quote}`, `403 ORDERING_DISABLED`, `429 TOO_MANY_OPEN_ORDERS` (açık talep ≤ 20), satır ≤ 200, aynı `requestId` aynı talebi döner |
| `GET orders` | → `{items[COrder]}`; `COrder {id, no, status, total, lineCount, submittedAtMs, rejectReason}` |
| `GET orders/detail?id=` | → `COrder` + `{note, lines[{key, code, name, quantity, net, total}]}` |
| `GET statement?from=&to=` | → `{balance, rows[{date, kind, documentNo, debit, credit, balance}]}` (açıklama yok) — `403 FEATURE_DISABLED` |
| `GET invoices?from=&to=&page=` | → `{items[{key, date, documentNo, kind, total}], total}` |
| `GET invoices/detail?key=` | → `{key, date, documentNo, kind, total, lines[{code, name, quantity, unitPrice, amount, productKey|null}]}` (anahtar yalnız listeyle aynı süzgeçten: `Kind ∈ {sale, sale_return}`, `!OtherSide`; `DocumentByKey` kullanılmaz) |
| `GET purchased?q=&page=` | → `{items[{code, name, lastDate, totalQuantity, times, product: CProduct|null}], total}` |

Görsel: `GET /api/v1/catalog/img/{id}/{s|l}?h={sha8}` anonim (`catalog-public`), `Cache-Control: public,
max-age=31536000, immutable`, `ETag`, `If-None-Match` → 304 (yalnız meta okunur), `nosniff`,
`Cross-Origin-Resource-Policy: same-site`; firma pasif, modül kapalı ya da görsel yoksa 404 (`IsEnabled` sorulmaz: panel ve
telefon yayından önce görselleri gösterir); `s` yoksa `l` döner.

### 5.3 Talep ↔ satış bağı ve bildirim

- Telefonun satış gövdesi (`sales_order` payload) üst düzeyde `catalogOrderId` taşır.
- `CatalogOrderLinker.TryLinkAsync(db, tenantId, userId, documentType, payloadJson, externalId)` `IngestEndpoints` job
  yazımından hemen önce ve `ApprovalService` onay anında çağrılır. Yalnız `sales_order` bağlanır (başka belgede alan yok
  sayılır). Talep aynı firmada ve `NEW`/`CLAIMED` olmalı → aynı `SaveChanges`'te `COMPLETED`, `DocumentRef = externalId`,
  `ClosedBy*`. Aynı `externalId` ile tekrar → idempotent. Başkasının `CLAIMED` talebini o kişi ya da yönetici değilse
  `409 CATALOG_ORDER_TAKEN`. Başka belgeyle bağlıysa `409 CATALOG_ORDER_ALREADY_CONVERTED`; o belgenin işi `Failed`/`DeadLetter`
  ise yeni satış talebi devralır (iş olmayan "başka yerde girildi" referansı kalıcı; yalnız `reopen`). Onayda reddedilen belge
  ingest'e girmez → talep `CLAIMED` kalır.
- Yeni talepte `UserNotification{Kind="CATALOG_ORDER_NEW", TaskId=null, Title="Yeni müşteri siparişi: {cari}",
  Body="{No} · {n} kalem · {toplam} TL"}`. Alıcılar: `ResponsibleUserId` ya da plasiyer eşlemesi
  (`Customer.SalespersonCode` → `MobileUserErpMapping.SalespersonCode`) → `AssignedUserId`; artı yönetim yetkili aktif
  kullanıcılar. Sıra `ReserveAsync` ile `SaveChanges`'ten hemen önce; ardından `ITenantEventHub.Publish(Tasks)`.
- Durum etiketleri — personel: Yeni / İşlemde ({ad}) / Siparişe çevrildi / Reddedildi; müşteri: Alındı / İnceleniyor /
  Siparişe çevrildi / Reddedildi.

## 6. Güvenlik

- `UseForwardedHeaders` (`XForwardedFor|XForwardedProto`, `ForwardLimit=1`, `KnownNetworks`/`KnownProxies`
  `ForwardedHeaders:*` ayarından; `XForwardedHost` yok). `CustomerCatalog:PublicHost` doluyken ayar zorunlu
  (`ValidateRuntimeConfiguration`). IP bölümlemesi IPv6'da /64. `AdminAuthEndpoints.ResolveClientIp` → `RemoteIpAddress`.
- Giriş: `BCrypt.Verify` her zaman (hesap/e-posta yoksa ortak sahte hash; Admin dahil); bellek içi `LoginThrottle` (tek container
  varsayımı, `TenantEventHub`/`PortalRecordMirror` gibi; en çok 100.000 anahtar). Deneme `TryBegin` ile adın kilidi altında
  başlar, devam edenler de sayılır (kontrol–kayıt yarışı yok). Anahtarlar: katalog = firma + normalize kullanıcı adı (güncel
  cihaz çerezi ve oturumdaki şifre değişikliği `#hesap`); personel = firma + kullanıcı adı + istemci adres bölümü (kayıtlı aktif
  telefon kendi sayacıyla); Admin = e-posta + adres bölümü. Firma başına giriş kovası (güvenilir cihaz harcamaz); BCrypt
  eşzamanlılık sınırı (8). Sınır: panel/Admin girişi kendi konteynerinin adresinden gelir.
- `OnRejected`: tüm 429'lara `RATE_LIMITED` gövdesi + `Retry-After`.
- Katalog alan adında izin listesi: `/assets/**`, `/robots.txt`, `/favicon.svg`, `/api/v1/catalog/**`, `/health*`, kabuk
  yolları; gerisi 404. CSP `default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' https: data:;
  connect-src 'self'; object-src 'none'; base-uri 'none'; frame-ancestors 'none'; form-action 'self'`,
  `X-Robots-Tag: noindex`, `Referrer-Policy: same-origin`, HSTS.
- Kimlikli JSON yanıtları `Cache-Control: private, no-store`.

## 7. Web (`src/ErpBridge.CentralApi/wwwroot/katalog/`, `CustomerCatalog/CatalogWeb.cs`)

- Varlık yolu `/assets/{v}/…` (`v` = dosyaların SHA-256'sından 10 hex; `immutable`); kabuk `GET /{code}` ve
  `/{code}/{**rest}` (`no-cache`, `%V%` ve `%TITLE%` yer tutucuları; bilinmeyen kodda aynı HTML 404 ile).
- Rotalar: `/{KOD}` katalog, `/{KOD}/giris?r=`, `/{KOD}/urun?kod=`, `/{KOD}/sepet`, `/{KOD}/siparisler`,
  `/{KOD}/siparisler/detay?id=`, `/{KOD}/hesap`, `/{KOD}/hesap/ekstre`, `/{KOD}/hesap/faturalar`,
  `/{KOD}/hesap/fatura?key=`, `/{KOD}/hesap/aldiklarim`, `/{KOD}/hesap/sifre`.
- Tasarım token'ları `css/tokens.css` = Siparis_Cepte `docs/design-tokens.css` kopyası. innerHTML yasak; JS+CSS gzip ≤ 100 KB.

## 8. Görevler

### Sunucu (ErpBridge)
| # | Görev | Kabul |
|---|---|---|
| S1 | ForwardedHeaders + /64 + giriş yavaşlatıcı (personel dahil) + `OnRejected` | İki farklı XFF ayrı kovaya düşer (ayar varken); yavaşlatma süresince doğru ve yanlış şifre aynı 429; ayar yokken mevcut `RateLimitTests` değişmeden yeşil |
| S2 | Tablolar + migration + modül + kilitli yetki + Admin kutucuğu + Tenant.Code güvencesi + yapılandırma (`CustomerCatalogOptions`) | `has-pending-model-changes` temiz; SALES'e anahtar açma `PERMISSION_LOCKED`; modül açık firmada `modules` içinde `customer_catalog` |
| S3 | `ParseStock` genişletmesi, `CatalogViewService`, `CatalogVisibility`, `CatalogPricing` | Fiyat örnekleri `ErpSalePricingTest` ile birebir; görünürlük tablosunun her satırı; koli "12"→12, "12,0"→12, "abc"→null; native `kategori`, ERP alt grup |
| S4 | Yönetim uçları (settings, categories, products, accounts) | SALES/ACCOUNTING 403; eski revision 409; "A.B" → "a.b"; ikinci aynı ad 409; aynı cariye ikinci hesap 409; `issuedPassword` yalnız bir kez |
| S5 | Görsel uçları + anonim görsel | Sihirli bayt 415; 1 MB+1 413; kota; `http://` link 400; anonim GET başlıkları; başka firma 404; 304 |
| S6 | Müşteri oturumu (`JwtIssuer.IssueForCatalogAccount`, çerez okuma, `CatalogCustomerPolicy`, state requirement), `info/login/logout/me/password` | Çerez bayrakları; şifre değişince eski çerez 401; katalog token'ı `/api/v1/android/*`'da reddedilir, mobil token katalogda 401; `X-Katalog`'suz POST 403 |
| S7 | Müşteri gezinme: `categories`, `products`, `products/detail`, `cart/quote` | Gizli ürün 404; `noDiscount` tek fiyat; fiyatsız ürün yok; stoksuz `inStock=false`; "ışık" "IŞIK"ı bulur; cariye özel açılan ürün başka hesapta görünmez |
| S8 | Talepler (müşteri + personel), `CatalogOrderLinker` (ingest + onay), bildirim | 0,06 TL fark 409; koli katı olmayan 422; aynı `requestId` tek kayıt; plasiyer eşlemeli carinin talebi o kişiye bildirim; ikinci claim 409; aynı talebe ikinci belge 409 |
| S9 | `statement`, `invoices`, `invoices/detail`, `purchased` | Bayrak kapalı 403; başka carinin belge anahtarı 404; kasa koduyla çakışan cari başka carinin `r` anahtarıyla 404 |

### Web (ErpBridge)
| # | Görev | Kabul |
|---|---|---|
| W0 | `CatalogWeb.cs` barındırma, kabuk, sürüm, başlıklar, izin listesi | Host=katalog iken `/ABCD2345` 200 + CSP; diğer host 404; `/assets/{v}/js/main.js` immutable; yanlış `v` 404; katalog host'undan `/api/v1/admin/...` 404 |
| W1 | Kabuk, giriş, oturum, `api.js`, `store`, `format`, `cart`, `route-parse` + `node --test` | node testleri yeşil |
| W2 | Katalog ızgarası, kategori sheet/panel, arama, ürün detayı | 390×844'te 2 kolon; üstü çizili fiyat + KDV etiketi; stoksuzda ekleme yok |
| W3 | Sepet, quote, talep gönderme, Siparişlerim | Yalnız-koli ürün koli katında; çift tıklama tek talep; 409'da güncel fiyat bandı |
| W4 | Hesabım: ekstre/bakiye, faturalar, aldıklarım, şifre değiştir | Bayrağı kapalı bölüm görünmez |
| W5 | Cihaz turu, erişilebilirlik, import grafiği/innerHTML/bütçe testleri | 360/390/768/1280 ekran görüntüleri |

### Panel (ErpBridge.Portal + Admin)
| # | Görev | Kabul |
|---|---|---|
| P1 | Oturuma `Modules`, `PortalArea.CustomerCatalog`, menü, `PortalMessages` | Modül açık ve ADMIN → menü görünür; ACCOUNTING ya da modül yok → görünmez; eski oturum JSON'u yüklenir |
| P2 | `/katalog`: Genel, Kategoriler & Ürünler (sıra, bayraklar, toplu seçim) | 409'da yeniden okuma; kaydedilmemiş değişiklikte uyarı |
| P3 | Ürün sheet'inde görseller (2 varyant, sıralı yükleme, link ekleme) | `PUT images/{id}/l` ve `/s` gider |
| P4 | `CatalogAccessSheet`, Cari düğmesi, Müşteri erişimleri sekmesi, paylaşım | Cari açılışında ek istek yok; `issuedPassword` bir kez |
| P5 | `/musteri-siparisleri` | Reddetmede gerekçe zorunlu; ERP'li firmada "telefondan çevirin" kutusu |

### Telefon — Siparis_Cepte `docs/GOAL_MUSTERI_KATALOGU.md` (A1–A7)

### Dağıtım ve insan kapıları
| # | Kim | İş |
|---|---|---|
| D1 | Kullanıcı + Claude | DNS `sipariscepte.appsgo.cloud` → sunucu IP'si (kullanıcı açtı, ✅); Coolify centralapi Domains'e `https://sipariscepte.appsgo.cloud` (Claude ekledi, ✅) |
| D2 | Claude (K8) | CentralApi dağıtımı; `https://lisans.appsgo.cloud/health/schema` → `current`, `pending:0` |
| D3 | Kullanıcı + Claude | Coolify ortam: `CustomerCatalog__PublicHost=sipariscepte.appsgo.cloud`, `CustomerCatalog__PublicBaseUrl=https://sipariscepte.appsgo.cloud`, `ForwardedHeaders__KnownNetworks__0=10.0.1.0/24` (coolify ağı; Claude ekledi, ✅); `ports: 5080` dışa açık mı ve Cloudflare proxy durumu kontrol. Loglarda gerçek istemci IP'si görülene kadar modül hiçbir firmaya açılmaz |
| D4 | Claude (K8) | Portal ve Admin ayrı dağıtım |
| D5 | Kullanıcı | Test firmasında Admin'den `customer_catalog`; uçtan uca duman testi |
| D6 | Claude (K8) | Play internal |

## 9. Riskler
| Risk | Önlem |
|---|---|
| Traefik/Cloudflare arkasında gerçek IP görünmez | S1 + D3; doğrulanana kadar modül kapalı |
| Görseller PostgreSQL'i büyütür | Firma kotası 1 GB, istemci küçültmesi; ileride nesne depolama |
| Aynı talep iki kez siparişe çevrilir | T8 belge düzeyi bağ |
| Web ile telefon fiyatı ayrışır | `CatalogPricing` = `ErpSalePricing` testleri; telefon talebin listesinden **tam** eşleşmeyle fiyat alır |
| Kategori adı değişince ayar yetim kalır | Kabul edildi; yetim satır yok sayılır (v1) |

## 10. Kapsam dışı (v1)
Panelden ERP'siz firmada siparişe çevirme · müşteri iptali · cari başına birden çok kullanıcı · cari grubu/şablon katalog ·
Mikro'dan iskonto · ondalıklı birim (KG) · PWA/koyu tema · firma logosu/rengi · "müşteri gözüyle önizleme".
