# GOAL — Panelden belge girişi (Satış · Tahsilat · Alış · İade · Tediye · Gider) + Görevler

> Başlangıç: 2026-10-02. Durum: `GOAL_PANEL_GIRIS_DURUM.md`. Kapsam: ErpBridge.CentralApi + ErpBridge.Portal
> (Sipariş Cepte'ye dokunulmaz; Play yüklemesi yok).

Kullanıcı isteği: "web panelden, uygulamadan yapabildiğim satış tahsilat alış iade tediye todo gider sayfalarını aynı
şekilde uygulamadaki gibi kullanıp sisteme veri kaydedebilmek istiyorum."

## 0. Zemin (`origin/main` ea784a8, 2026-10-02)

- **ERP'siz firma:** panelde `/evraklar` (satış/alış/iade) ve `/cari` (tahsilat/tediye) girişi var; yalnız ADMIN
  (`action.native_books.edit`, GOAL_PANEL_ERPSIZ D4), gövde telefonla birebir değil (`PortalNativeSalesEndpoints.BuildDocumentAsync`:
  `portal-sale-…`, `priceListNo`/`listUnitPrice`/`payments[]` yok), gider yok.
- **ERP'li (Mikro) firma:** panelden giriş yok. Tek örnek katalog "Siparişe çevir" (`CustomerCatalog/CatalogOrderConversion`,
  KB 00 kural 36 S10): sunucu Sipariş Cepte'nin `sales_order` gövdesini kendisi kurar, `SalesJobWriter` ile yazar.
  Panel `/ingest`'e belge **gönderemez** (`PORTAL_CANNOT_SUBMIT_DOCUMENTS`) — bu kural korunur.
- **Görevler:** sunucu hazır (`Tasks/TaskService`, `/api/v1/android/tasks/*` + `/notifications`, `MobileUserPolicy` → panel
  token'ı geçer; KB 00 kural 27). Panelde ekran yok (GOAL_GOREVLER K11 "web paneli sonraki adım").
- **Mikro evrakı sistemden düzenlenemez/iptal edilemez** (yazıcılar yalnız INSERT). ERP'li firmada panel girişi yalnız
  oluşturmadır; düzeltme Mikro'dan. ERP'siz düzenle/iptal (`Evraklar`/`Cari`) aynen kalır.

## 1. Kararlar (kullanıcı, 2026-10-02)

| # | Karar |
|---|---|
| K1 | ERP'li **ve** ERP'siz firmalar |
| K2 | Panel girişi **onaysız**: firma onay kurallarına bakılmaz, onay talebi açılmaz |
| K3 | Erişim telefonun yetki sistemiyle (GOAL_YETKILER): `module.sales/collection/purchase/returns/disbursement/expenses/tasks`, `action.sale.open_account`, `limit.*`. Panele giriş yine `CanUsePortal` ister |
| K4 | Limit/yetki aşımı **engellenir** (form kaydetmez, sunucu reddeder). Telefondaki gibi yalnız ADMIN muaf (`DocumentPermissionCheck`); onaycı/muhasebe muaf değil. Limit **giren kişiye** uygulanır |
| K5 | ERP'li firmada belgenin **sahibi formda seçilir** (varsayılan giren kişi); ajan seri/depo/plasiyer/ERP kullanıcı no'yu sahibin eşlemesinden alır. Eşleme eksikse form uyarır, gönderilmez |
| K6 | Formlar telefonla **birebir** + **belge tarihi seçimi**. Geçmiş tarih **sınırsız**, ileri tarih reddedilir |
| K7 | Görevler panelde **telefonla tam eşdeğer** (liste/özet, oluştur/düzenle, atanan/takipçi, alt görev, yorum, resim, seri, cari + ziyaret hatırlatma, bildirimler) |
| K8 | Eksi stok: her zaman uyarı; `action.sale.negative_stock` yoksa kaydedilmez |
| K9 | Müdahalesiz çalışma (CLAUDE.md istisnası 2026-10-02): push/PR/squash-merge, CI yeşil (Actions kredisi yoksa yerel build 0 uyarı + test yeşil), Coolify dağıtımı + `/health/schema`; Mikro test yazımı yalnız `MikroDB_V15_DEMO` |

### Doğrulanmış notlar (planlama, `origin/main`)
- 6 Mikro yazıcısının hepsi belge tarihini `header.OccurredAt.Date`'ten alır (`cha_tarihi`/`cha_belge_tarih`, `sth_tarih`,
  `sip_tarih`); tahsilat referans yılı `OccurredAt.Year`. `NativeDocumentProcessor` `occurredAt`'i `tarih` olarak saklar.
  Hiçbir yazıcıda tarih sınırı yok → sınırı sunucu koyar (yalnız ileri tarih reddi, K6).
- Fiyat grubu ve müşteri iskontosu **sunucuda yok**: telefon ERP'den gelen cariye `priceGroup="Özel Fiyat"` /
  `specialDiscountPercent=0` koyar, fiilen ürünün başlık listesini kullanır (`MobileEntityAssembler` `satisFiyatListeNo`:
  fiyatı varsa 1, yoksa fiyatlı en küçük liste). Belgenin `priceListNo`'su satırların en çok kullandığı liste (eşitlikte
  küçük). Panel aynı kuralı uygular, müşteri iskontosu 0; ayrıca liste seçimi için açılır menü.
- `module.expenses` ve `module.tasks` sunucuda denetlenmiyor (`PermissionCatalog` `ServerEnforced=false`). Görevler yalnız
  panelde kapılanır (telefon davranışı değişmez); yeni gider ucu `module.expenses`'i sunucuda denetler.
- ERP'siz gider telefonda müşterisiz `disbursement` ("Gider: {kategori}") → yalnız kayıt, bakiyeye etkisiz; ingest 5b2'de
  `disbursement` yetki/limitine tabi. Panel de aynısını yapar.
- Varsayılan rol şablonunda `module.*` yalnız ADMIN/MANAGER/SALES'te açık; muhasebe girecekse `/yetkiler`'den açılır.
- `DocumentPermissionCheck.Refusal` yalnız `IsAdmin`'i muaf tutar ve "onaya gönderilmeli" der; panel bunun yapılandırılmış
  halini (`Check`) kullanır.
- `allowNegativeStock` telefonda cihaz ayarıdır, sunucuda firma ayarı yoktur → K8 yalnız yetkiye bakar.
- **Davranış değişikliği:** ERP'siz firmada yeni belge girişi artık ADMIN'e değil `module.*` yetkisine bağlı (K3,
  GOAL_PANEL_ERPSIZ D4'ün giriş kısmının yerini alır). Düzenle/iptal `action.native_books.edit`'te kalır.

## 2. Yaklaşım — iki firma türü için tek sunucu yolu

Katalog "Siparişe çevir" deseni genelleştirilir: panel formu gönderir → sunucu telefonun gövdesini alan alan kurar →
yetki/limit denetler → `Jobs/SalesJobWriter` ile yazar (ERP'li: `Pending` iş + ajan uyanır; ERP'siz:
`NativeDocumentProcessor`; satış depo kuyruğuna da girer — telefonla aynı). Mevcut `PortalNative*` uçları silinmez
(yalnız ekleme); eski giriş düğmeleri yeni sayfalara yönlenir.

### 2.1 Sunucu — `src/ErpBridge.CentralApi/PanelEntry/` + `Endpoints/PortalEntryEndpoints.cs`

Grup `/api/v1/portal/entry`, `MobileUserPolicy` + `PerMobileUserRateLimitPolicy` (önizlemeler firma bütçesini tüketmesin).

- **`PanelEntryAccess`**: `MobileAccountEndpoints.AuthorizeAsync`; **yalnız `client=portal`** token'ı (aksi 403
  `ENTRY_REQUIRES_PORTAL` — yoksa telefon bu uçla onayı atlardı); ekranın modül anahtarı (403 `ENTRY_MODULE_DENIED`);
  sahip = aynı firmada aktif, silinmemiş, telefon rolü olan kullanıcı.
- **`PanelEntryKinds`**: önekler `PNL-SO-`, `PNL-TH-`, `PNL-PR-`, `PNL-SR-`, `PNL-TD-`, `PNL-GD-`;
  `externalId = önek + operationId` (tekrar deneme idempotent; ERP'siz çok yöntemli tahsilatta `-{n}`).
- **`PanelEntryDates`**: `date` (yyyy-MM-dd) ≤ İstanbul bugünü, aksi 400 `INVALID_DOCUMENT_DATE`. Bugün → şimdiki saat
  (`PortalReports.IstanbulTime`), geçmiş gün → 12:00. Biçim telefondaki gibi (`dd.MM.yyyy HH:mm`; alışta ISO `+03:00`).
- **`Permissions/DocumentPermissionCheck.Check(...)`**: ihlali yapılandırılmış döner (anahtar, tür, gerçek, limit);
  `Refusal()` bunun biçimleyicisi olarak kalır (ingest metinleri/testleri değişmez). Panel her kurulan belge için
  `ApprovalKinds.ForDocument` ile çağırır → 403 `ENTRY_MODULE_DENIED` / 409 `ENTRY_LIMIT_EXCEEDED` ("onaya" demeyen metin).
  `ApprovalService` kuralları çağrılmaz (K2).
- **ERP eşleme kuru çalıştırması**: sahibin `ErpWriteContextBuilder.Build(...)` bağlamı → `ErpWriteContext` →
  `ErpBridge.Core.Jobs.MobileDocumentTranslator.Translate(...)`. `ErpMappingMissing` → 409 `ERP_MAPPING_MISSING`
  ("{sahip} adına; eksik: …"); diğer çeviri hatası → 422 `ENTRY_DOCUMENT_INVALID` (gövde kurucusuna kalkan).
- **`PanelEntryWriter`**: tek belge `SalesJobWriter.WriteAsync` (`CreatedByUserId = sahip`); çok belge işlem içinde
  `PlaceAsync`, biri `Failed` → hepsi geri, 422 `ENTRY_BOOKING_FAILED` (`ApprovalService.PostDocumentsAsync` deseni).
  Her girişe `native_audit_log` satırı (iki firma türünde): giren kişi, belge, "X adına" özeti.
  `expectedTotal` tolerans dışında → 409 `PRICE_CHANGED` + güncel önizleme.
- **`PanelPricing`**: satış `CustomerCatalog/CatalogPricing`; iade satırı ve alış (6 satır + 6 genel iskonto) Sipariş Cepte
  `ErpSalePricing.kt`/`ErpPurchasePricing.kt`'den birebir; telefonun test vektörleri `PanelPricingTests`'e
  (`TOTAL_MISMATCH` kalkanı).
- **`Portal/PortalCustomerSales`**: `PortalStockMovements` gibi katlanan ayna (`stockTransactions` → `cariKod`, `tip==1`,
  fiyat, tarih). İade fiyat seçenekleri telefonun `salePricesFor`'u gibi; sunucu iade fiyatının geçmiş fiyatlardan biri
  olduğunu denetler.
- **Gövdeler telefonla alan alan aynı** (kaynak kod yorumunda telefon dosyası/fonksiyonu belirtilir):

  | Belge | ERP'li | ERP'siz |
  |---|---|---|
  | Satış | `salesOrderPayload` ("Bank Kartı" → "Kredi Kartı" + `bankCode`; depo yalnız varsayılandan farklıysa) | aynı gövde |
  | Tahsilat | `ErpCollectionDocument.payload` — `payments[]` nakit/kart/havale/çek/senet; çek/senet no + vade | yöntem başına `kasaLogPayload` |
  | Tediye | `kasaLogPayload`; havalede **`bankCode` gönderilir** (telefon yalnız `bankName` gönderiyor) | aynı |
  | Gider | `expense` + `expenseCardCode`, `vatAmount`, `vatPointer`, `cashCode`/`bankCode` | müşterisiz `disbursement` "Gider: {kategori}" |
  | Alış | `purchaseReceiptPayload` (`invoiceNo` "Seri-Sıra", net + KDV, peşin "Nakit") | aynı |
  | İade | `ErpReturnDocument.payload` | `salesReturnPayload` |

- **Uçlar**

  | Uç | İş |
  |---|---|
  | `GET context` | veri kaynağı, girilebilir türler, sahip adayları; ERP'li: depolar, fiyat listeleri (KDV dahil mi), kasa/banka, gider kartları, KDV oranları; ERP'siz: sabit gider kategorileri (Yemek, Kırtasiye, Kargo, Yol/Yakıt, Bakım, Diğer) |
  | `GET customers?q=` | `PortalLedger` araması (bakiye yalnız `view.customer.balance`) |
  | `GET products?q=&priceListNo=` | `CatalogViewService` + `PortalStockCatalog` stoğu |
  | `GET returnables?customerCode=` | müşteriye satılmış ürünler + geçmiş fiyatlar |
  | `POST {kind}/preview` | fiyatlı satırlar, toplamlar, ret, eksik eşleme, stok uyarısı; hiçbir şey yazmaz |
  | `POST {kind}` | yazar; 201 `{documents:[{jobId, externalId, documentType, status}]}` |
  | `GET documents/{jobId}` | durum (`ErpDocumentStates`), ERP seri/sıra (`JobAcks`), giren kişi |
  | `GET documents` | son girişlerim |

  `PortalErpWriteEndpoints.ReadLookupsAsync` internal yapılır ve KDV `rate` döner (önce `lookups`'ta `vat_rate` satırının
  `rate` taşıdığı testle doğrulanır).
- **Eksi stok (K8):** önizleme stoğu yetmeyen satırı işaretler; `action.sale.negative_stock` yoksa create 409
  `ENTRY_NEGATIVE_STOCK`.

### 2.2 Panel — `src/ErpBridge.Portal`
- `Session/PortalRoles.cs`: `PortalArea` `EntrySales`, `EntryCollection`, `EntryPurchase`, `EntryReturns`,
  `EntryDisbursement`, `EntryExpenses`, `Tasks` → `module.*`.
- `MainLayout.razor`: "Hızlı giriş" izinli giriş alanı olan her firmada: `/giris/satis`, `/giris/tahsilat`, `/giris/alis`,
  `/giris/iade`, `/giris/tediye`, `/giris/gider` (Stok sayımı `CanEditNativeData`'da kalır); "Görevler" + üst çubukta zil.
- `Shared/Entry/`: `EntryHeader` (sahip + tarih), `CustomerPicker`, `ProductLineGrid`, `PaymentLines`, `EntryTotals`,
  `RefusalBanner` (ret/eksik eşlemede Kaydet pasif), `EntryResultCard` (durum, ERP belgeleri bağlantısı, yazdır).
  Önizleme ~400 ms gecikmeli; her kayıt denemesi yeni `operationId`.
- Altı sayfa, alanlar telefonla aynı (satış: satır/genel iskonto, not, Cari Borç/Nakit/Kredi Kartı+banka; tahsilat:
  bölünmüş ödeme, taksit, çek/senet no+vade; alış: seri/sıra, 6+6 iskonto; iade: yalnız satılmış ürünler, durum oranı,
  neden, Cari Alacak/Nakit/Banka İade; tediye: Nakit/EFT+banka; gider: kart/kategori, KDV, Kasa/Banka/Kredi Kartı).
- Yazdırma `Pages/GirisYazdir.razor` (`/giris-yazdir?is=`, `EvrakYazdir.razor` deseni); giderde yok (telefonla aynı).
- `Evraklar ?yeni=` ve `Cari` ödeme düğmeleri yeni sayfalara gider (`?cari=` dolu); düzenle/iptal aynen.
- `Api/PortalApiClient.cs` + `Api/EntryModels.cs`; `Api/PortalMessages.cs`'e yeni kodların Türkçe metni.

### 2.3 Görevler (panel; sunucu değişmez)
- `Api/TaskModels.cs` (`Contracts/TaskContracts.cs` aynası); liste (`changedSinceSeq`), özet, kişiler, seriler, `events`
  uzun yoklama, `ops` (işlem başına `opId`, tekrarda korunur), detay, ek PUT/GET/DELETE, bildirimler + okundu.
- `/gorevler` (özet kartları; bana atanan/oluşturduğum/takip ettiğim/hepsi `canManage`), detay (alt görev, yorum,
  olaylar, tamamla/yeniden aç/iptal/sil), düzenleme çekmecesi (atanan/takipçi, cari + ziyaret hatırlatma,
  başlangıç/bitiş, öncelik, alt görevler, `requiresPhoto`), `/gorevler/seriler`; canlı güncelleme `Muhasebe.razor`
  uzun yoklama deseniyle.
- Resimler: tarayıcı Bearer taşıyamaz → baytlar devre üzerinden alınıp `data:` URL (devre başı önbellek); yükleme
  `InputFile` → `IImageShrinker` → PUT (JPEG/PNG/WEBP ≤ 2 MB, görev başı 10).
- Zil: 60 sn'de bir özet (`PortalRefreshTiming`/`PeriodicTimer`), açılır listede bildirimler + okundu.

## 3. Görevler

| ID | Görev | Bağımlı |
|---|---|---|
| P0 | Plan + DURUM + `CLAUDE.md` istisnası (bu PR) | – |
| P1a | `DocumentPermissionCheck.Check` + testler (ingest davranışı değişmez) | P0 |
| P1b | `PanelEntryAccess` / `PanelEntryKinds` / `PanelEntryDates` | P0 |
| P1c | Sahip bağlamı + çevirici kuru çalıştırması | P1b |
| P1d | `PanelEntryWriter` (tek/çok belge + denetim) | P1b |
| P1e | `context` / `customers` / `products` uçları + lookup'ların açılması | P1b |
| P2a | `PanelPricing` + telefon test vektörleri | P0 |
| P2b | Fiyat listesi kuralı | P2a |
| P2c | `PortalCustomerSales` + `returnables` | P0 |
| P3a–f | Gövde kurucusu + preview/create: satış, tahsilat, tediye, gider, alış, iade (her biri ERP'li + ERP'siz, testli; bir PR = bir tür) | P1*, P2* |
| P4 | `documents/{id}`, son girişlerim | P3 |
| P5a | Alanlar + menü | P0 |
| P5b | İstemci / modeller / mesajlar | P3 sözleşmeleri |
| P5c | `Shared/Entry` bileşenleri | P5b |
| P5d | Altı giriş sayfası | P5c |
| P5e | Yazdırma + Evraklar/Cari yönlendirme | P4, P5d |
| P6a–f | Görevler: istemci; liste + canlı; detay/düzenle; resimler; seriler; zil | P0 (paralel yürüyebilir) |
| P7 | KB 00 yeni kural 38 "Panelden belge girişi" + kural 18/19/27/33/36 notları; `docs/api-contracts.md`; `docs/mobil-belge-sozlesmesi.md` (`PNL-*`); `GOAL_GOREVLER.md` K11 | – |
| P8 | Uçtan uca doğrulama + DURUM kapanışı | hepsi |

## 4. Çalışma kuralları
- İş yalnız **`ErpBridge-panel-giris`** worktree'sinde (`C:\Users\retro\Documents\GitHub\ErpBridge-panel-giris`); ana
  klasörde ve başka oturumların worktree'lerinde `switch`/`checkout`/`stash`/`reset` yapılmaz.
- Her görev: `git fetch origin && git switch -c panel-giris-<id>-<kısa-ad> origin/main` → önce KB (INDEX → 00–04) →
  kod + test → yerel `dotnet build ErpBridge.sln -c Debug` (0 uyarı) + `dotnet test ErpBridge.sln` → Türkçe commit
  (ne + neden, `Co-Authored-By`) → push → PR → CI (kredi yoksa yerel yeşil) → inceleme yorumları çözülür →
  merge öncesi `git merge origin/main` (push edilmiş dalda **rebase yok**) + yerel build/test → squash-merge →
  DURUM güncellenir (aynı dalda).
- Migration içeren görevden sonra `https://lisans.appsgo.cloud/health/schema` → `current`.
- **Yasak:** `--force` / `--force-with-lease` push, CI kırmızıyken merge, `main`'e doğrudan push, `MikroDB_V15_02` ve
  diğer müşteri Mikro DB'lerine yazım, mevcut tabloyu değiştiren migration, Play yüklemesi.
- Bozulmaz: `PORTAL_CANNOT_SUBMIT_DOCUMENTS` aynen kalır (panel `/ingest`'e yazmaz); telefon uç sözleşmeleri değişmez;
  `native_customer_balances`/`native_stock_levels`'a yalnız `NativeDocumentProcessor` yazar; ekranda ham JSON/İngilizce
  hata gösterilmez.

## 5. Riskler / bilinen sınırlar
- Fiyat aritmetiği telefondan saparsa Mikro `TOTAL_MISMATCH` verir → telefon test vektörleri + canlı DEMO testi.
- Sınırsız geçmiş tarih: panel raporları (`/plasiyerler`, özet) belgeyi alındığı günden en çok 7 gün geriye sayar
  (`PortalReports.OfflineTolerance`); daha eski tarihli giriş o günün raporunda görünmez (ekstre/bakiye doğru). DURUM'a not.
- Dönem kilidi yok: kapalı döneme fiş girilebilir (Mikro'nun kendi kilidi; ileride ayrı karar).
- Görev resimleri `data:` URL ile SignalR üzerinden taşınır (görev başı ≤ 10, küçültülmüş); ileride dosya deposu bağlantısı.

## 6. Doğrulama
- İlişkisel testler `tests/ErpBridge.CentralApi.Tests/Endpoints/PortalEntry*RelationalTests.cs` (`SqliteCentralApiFactory`),
  her tür × iki firma türü: ERP'li `Pending` iş + `CreatedByUserId = sahip` + gövde anahtarları telefon fikstürüyle aynı +
  çevirici sahibin bağlamıyla kabul eder; eksik eşleme 409; modül 403; limit 409 ve **onay talebi açılmaz**; firma onay
  kuralı açıkken bile doğrudan yazılır; telefon token'ı 403; `/ingest` panel için hâlâ `PORTAL_CANNOT_SUBMIT_DOCUMENTS`;
  `operationId` idempotent; ileri tarih 400; geçmiş tarih `occurredAt`'e o gün; ERP'siz bakiye/stok doğru, satış depo
  kuyruğunda, iki yöntemli tahsilat atomik, gider yalnız kayıt, denetim satırı; `returnables` yalnız o müşteriye
  satılanı verir; eksi stok yetkisiz 409.
- bUnit: her giriş sayfası, menü/rol (`PortalLayoutTests`, `PortalRolesTests`), görev sayfaları, yazdırma.
- Canlı Mikro (yalnız `MikroDB_V15_DEMO`, `ERPBridge_RUN_INTEGRATION=1` + `ERPBridge_MIKRO_WRITE_DB`): panel gövdeleri geçmiş
  tarihle çevirici → yazıcı; `cha_tarihi`/`sth_tarih` seçilen gün.
- Tarayıcı: yerel launch yapılandırmalarıyla formlar açılır, önizleme/ret görünür (bellek içi DB kısıtı: kayıt sonrası
  listeler ilişkisel testlerle); canlıda gözle kontrol "Seni Bekleyenler"e.

## 7. Bitti tanımı
ERP'li ve ERP'siz firmada yetkisi olan panel kullanıcısı satış, tahsilat, alış, iade, tediye ve gideri telefondaki
alanlarla (ve seçtiği tarihle) panelden girer; belge telefonunkiyle aynı yoldan Mikro'ya/merkez deftere yazılır, onaya
düşmez, yetki/limit aşımı reddedilir; Görevler panelde telefonla eşdeğer çalışır; CI yeşil, KB güncel.
