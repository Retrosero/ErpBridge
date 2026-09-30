# GOAL_YETKILER — Gelişmiş kullanıcı yetkileri

Onay: kullanıcı 2026-09-29 (plan onayı; aşamaların push, PR, birleştirme, sunucu dağıtımı ve Play internal yüklemesi önceden onaylı). Durum: `GOAL_YETKILER_DURUM.md`.

## Bağlam
Kullanıcı için bugün yalnız rol (pozisyon) seçilebiliyor; hangi kısıtlamaların olduğu görünmüyor, kişiye özel yetki girilemiyor.

**Bugünkü durum:**
- Telefondaki "Rol & Yetki Matrisi" (`SecurityScreen.kt`, 816 satır) sabit kodlu bir tanıtım ekranı: düzenlenmez, saklanmaz, hiçbir şeyi kısıtlamaz.
- Gerçek yetki = 5 rolün birleşimi (ADMIN, MANAGER, SALES, WAREHOUSE, ACCOUNTING) + yalnız yöneticiye iki bayrak (`CanApprove`, `CanManageApprovalRules`).
- Kontroller sunucuda `RolePermissions`'ta (yaklaşık 40 yer), telefonda dağınık (`DataEditPolicy`, `TaskRepository.canManage`, `EodModule` vb.).
- Firma Mikro kullanıyor. Fora `Hak*` parametreleri ERP ayarı olarak **uygulanmayacak**, yalnız izin kataloğuna örnek oldu. Telefonda zaten bağlı iki `Hak*` kontrolü (bakiye, ekstre) olduğu gibi kalır; yeni anahtarla birlikte ikisi de izin vermeli (VE).

**Kullanıcı kararları:**
- **Model:** Firma başına düzenlenebilir **rol şablonu** + **kişiye istisna** (izin ver / engelle / rolden). Ekranda her iznin nereden geldiği görünür.
- **Düzenleme yeri:** **Telefon + web panel**; ikisi aynı sunucu kaydına yazar.
- **v1 kapsamı:** modül erişimi, işlem yetkileri, veri görünürlüğü, sayısal limitler.

**Hedef:** Yöneticinin rol şablonlarını ve kişiye özel yetkileri görüp düzenleyebilmesi; bunların telefonda ve sunucuda gerçekten uygulanması; yetkisi olmayan kayıtları hiç değiştirmeyen, bugünkü davranışı koruyan bir geçiş.

## Temel kurallar
1. **Sunucu tek kaynak.** Etkin yetki her istekte sunucuda hesaplanır (kullanıcı ve roller zaten her istekte yeniden okunuyor, KB kural 14).
2. **Satır yoksa bugünkü davranış.** Kayıtlar yalnız varsayılandan farklarını tutar. Hiç satır yokken sonuç bugünkü `RolePermissions` ile birebir aynı olmalı; bunu bir parite testi garanti eder.
3. **Hesaplama sırası:**
   - ADMIN her şeye yetkili, limitsiz ve düzenlenemez (kilitlenme olmaz; `LAST_ADMIN` kuralı aynen kalır).
   - Evet/hayır izinleri rollerin **VEYA**'sı; limitler rollerin **en yükseği** (sınırsız olan kazanır).
   - Kişiye istisna birleşik rol değerini ezer.
4. **Limit aşımı engellemez, belgeyi onaya yollar.** Firmanın o tür için onay kuralı kapalı olsa bile belge onay merkezine gider.
5. **Sunucu ret kodu `409 APPROVAL_REQUIRED` olur.** Eski telefon sürümleri bu kodu zaten onay talebine çeviriyor (`OutgoingDocumentRepository.kt:429`); saha işi kaybolmaz, kuyruk tıkanmaz.
6. **Yerel/demo ve API anahtarlı oturumlarda her şey açık** (bugünkü gibi).
7. **Katalogdaki her anahtarın bir uygulandığı yer olmalı.** Karşılığı olmayan anahtar "sonra" listesine gider; yeni bir sahte matris yapılmaz.

## İzin kataloğu v1
Anahtarlar kalıcıdır ve yeniden adlandırılmaz. Varsayılanlar bugünkü davranışı korur.

### Modüller (telefon)
- **Anahtarlar:** `module.sales`, `quotes`, `suspended_sales`, `purchase`, `returns`, `collection`, `disbursement`, `cashbox`, `eod`, `customers`, `catalog`, `stocks`, `counting`, `warehouses`, `expenses`, `vehicles`, `reports`, `approvals`, `route_plans`, `tasks`, `targets`, `warehouse_queue`.
- **Varsayılan:** ADMIN, MANAGER ve SALES'te açık, WAREHOUSE ve ACCOUNTING'de kapalı. Tek istisna `warehouse_queue`: ADMIN, MANAGER ve WAREHOUSE'ta açık.

### Modüller (panel)
- **Anahtarlar:** `portal.reports`, `ledger`, `approvals`, `erp_documents`, `targets`, `displays`, `erp_settings`, `audit`.
- **Varsayılanlar:** bugünkü `PortalRoles.Allows` eşlemesi.

### İşlemler
- `action.users.manage`: ADMIN'e kilitli.
- `action.approvals.decide`, `action.approvals.manage_rules`: mevcut sütunlara yazar.
- Yönetim işleri: `action.route.plan`, `targets.manage`, `tasks.manage`, `suspended_sales.manage_others`, `warehouse.manage`.
- Kart ve veri işleri: `action.products.edit`, `customers.edit`, `master_data.import`, `native_books.edit`, `xml_feed.manage`.
- Satış işleri:
  - `action.sale.negative_stock`: firma ayarı açıkken bile bu anahtar gerekir (ikisi birlikte).
  - `action.sale.open_account`: veresiye satış; Fora KapamaSekli örnek alındı.

### Görünürlük
- `view.eod.all_users`, `view.expenses.all_users`.
- Cari: `view.customer.balance`, `ledger`, `risk_limit`, `purchased_products`. Fora HakGorme örnek alındı.
- `view.product.last_purchase_price`.

### Limitler (boş = sınırsız, varsayılan sınırsız)
- İskonto: `limit.sale.max_line_discount_pct`, `limit.sale.max_general_discount_pct`.
- Onaysız en yüksek tutar: `limit.sale.max_amount`, `limit.return.max_amount`, `limit.purchase.max_amount`, `limit.disbursement.max_amount`.

### Sonra (özellik önce var olmalı)
- **Fora'dan esinlenen değiştirme hakları:** belge tarihi, fiyat listesi, depo, ödeme planı, cari adresi, teslim tarihi, birim fiyat.
- **Diğer görünürlük anahtarları:** ciro, açık sipariş, vadesi gelen tahsilat.
- **Tür bazında onay verme yetkisi.**
- **v2 veri kapsamı:** yalnız kendi carileri/rotası, ürün grubu. Bunun için sunucu senkron verisini süzmeli; yalnız arayüzde gizlemek yetmez.

## Sunucu (ErpBridge, `origin/main` üzerinden yeni dallar)
- **Yeni `src/ErpBridge.CentralApi/Permissions/` klasörü:**
  - `PermissionKeys`, `PermissionCatalog` (tek kaynak, `Version`).
  - `EffectivePermissions`: `Can`, `Limit`, `Entries` + kaynak (admin, rol ya da kişisel).
  - `PermissionResolver`: saf `Resolve` + `LoadAsync` / `LoadManyAsync`.
- **Tablolar (tek migration `YetkiMatrisi`):**
  - `tenant_role_permissions` (Tenant, Role, Key, Value).
  - `mobile_user_permission_overrides` (User, Key, Value).
  - `permission_changes`: denetim kaydı; kim, hangi istemci, eski ve yeni değer.
  - `MobileUser.Permissions` eşlenmez (`[NotMapped]`); `MobileUserAccess.CheckAsync` doldurur.
- **`RolePermissions` geçişi:** `CanViewReports`, `CanViewLedger`, `CanPlanRoutes`, `CanManageTargets`, `CanOperateWarehouse`, `CanManageWarehouse`, `CanEditNativeData` resolver'a döner.
  - Aynı geçiş: `SuspendedSaleService.CanManage`, `TaskService.CanManage`, `DisplayEndpoints`, `IngestEndpoints.CallerIsAdminAsync` → `NativeDocumentProcessor` hakları.
  - Rol tabanlı kalanlar (kimlik bilgisi): `IsAdmin`, `CanManageUsers`, `CanUsePhone`, `CanUsePortal`, `IsWarehouseOnlyOnPhone`, `TeamScope`.
  - Yetkiler yüklenmeden sorgulanırsa hata verir (kapalı başarısızlık); ilişkisel testler unutulan yeri yakalar.
- **API:** `/api/v1/android/account` grubuna, yeni `Endpoints/MobilePermissionEndpoints.cs` + `Permissions/PermissionService.cs` ile (SuspendedSales düzeni örnek alınır).
  - `GET /permissions/catalog`.
  - `GET /roles/permissions`, `PUT /roles/{role}/permissions`.
  - `GET /users/{id}/permissions` (kullanıcının kendisi de okuyabilir), `PUT /users/{id}/permissions`.
  - `GET /permissions/changes`.
  - Ret kodları: `ADMIN_ROLE_LOCKED`, `ADMIN_USER_LOCKED`, `UNKNOWN_PERMISSION`, `INVALID_PERMISSION_VALUE`, `PERMISSION_LOCKED`.
  - `MobileSessionDto` (`/login` ve `/me`) alanları: `permissions`, `limits`, `permissionsVersion`. `MobileUserDto` alanı: `permissionOverrideCount`.
- **Ingest'te uygulama** (`IngestEndpoints`, yalnız mobil kullanıcı token'ı):
  - Belge türünün modül anahtarı kontrol edilir.
  - Yeni `DocumentLimitFacts` tutarı, satır iskontosunu ve genel iskontoyu okur.
  - Yetki yoksa ya da limit aşılırsa `409 APPROVAL_REQUIRED` döner ve mesaj nedeni söyler.
  - Onay talebi belgeleri ve onaylanmış belgenin kaydı bu kontrole takılmaz. `ApprovalService.DecideAsync` yolu uygulamadan önce doğrulanacak.
- **Panel (`src/ErpBridge.Portal`):**
  - `PortalSession`, yetkileri oturumda saklar.
  - `PortalRoles`: alan → anahtar; eski oturumda rol eşlemesine geri düşer.
  - `Kullanicilar.razor`'a kullanıcı başına "Yetkiler" düğmesi ve kişisel ayar sayısı rozeti.
  - Yeni `Shared/UserPermissionsSheet.razor`: üç konumlu seçim (Rolden / İzin / Engel), kaynak etiketi, limit alanı + "Sınırsız".
  - Yeni `Pages/Yetkiler.razor` (`/yetkiler`): rol × izin matrisi, ADMIN sütunu kilitli, değişiklik geçmişi sekmesi.

## Telefon (Siparis_Cepte)
- **Model:**
  - `data/account/UserPermissions.kt` (saf; bilinmeyen anahtar ya da eski sunucu = izinli, limit null = sınırsız).
  - `Permissions.kt` + `PermissionStore`: ekranlar yetkiler yenilenince güncellenir; hesap oturumu yoksa her şey açık.
  - Kullanıcı başına önbellek `LicenseRepository` içinde, `account_roles_user_<u>` gibi. Çevrimdışı girişte `activateCachedRoles` yanında yüklenir; çıkışta temizlenir.
  - `AccountRepository.applyProfile` yetkileri kaydeder ve yeniden yükler.
  - Room migration'ı gerekmez.
- **Ekranlar:**
  - `SecurityScreen.kt`'deki sahte ekran gider, yerine gerçek **Rol Yetkileri** ekranı gelir: rol sekmeleri; bölümler Modüller / Panel / İşlemler / Görünürlük / Limitler; "Varsayılana dön".
  - **Yetkilerim**: herkes kendi etkin yetkilerini ve nereden geldiklerini görür.
  - `UserManagementScreen`: 5 rolden çoklu seçim (sunucunun `roles[]` alanı artık kullanılır). "Yetkiler" düğmesi `UserPermissionsScreen`'i açar: üç konumlu seçim ve kaynak etiketi ("Rolden: Satış" ya da "Kişisel").
- **Uygulama yerleri:**
  - **Modüller:**
    - `RoutePermissions.kt` rota → anahtar eşlemesini tutar.
    - `NavApp` rota koruması (depo yönlendirmesinin yanında): yetkisiz ekran açılmaz, "Bu ekran için yetkiniz yok" mesajı çıkar.
    - Hızlı işlem kutuları ve Ayarlar > Görünüm listesi yetkisiz modülleri göstermez.
  - **Kart ve veri işleri:** `DataEditPolicy`, `XmlFeedScreen`, `TaskRepository`, `TargetRepository`, `RoutePlanScreen`, `SuspendedSaleRepository.canDelete`, `WarehouseAccess`.
  - **Satış:** eksi stok (tüm `allowNegativeStock` okumaları firma ayarı VE anahtar olur); açık hesap ödeme seçeneği.
  - **Görünürlük:** `EodModule`, `ErpExpenseHistory`, `CustomerDetailView` / `CustomerDetailTabs`, `PurchaseModule`.
  - **Limitler:**
    - `data/approval/LimitCheck.kt` + `ApprovalGate.route(breach)`: limit aşımında belge onaya gider.
    - Satış, iade, alış ve tediye ekranlarında uyarı çıkar ("İskonto sınırınız %10 — belge onaya gidecek").
    - Onay özetine gerekçe eklenir.
  - **Kuyruk:** gelecekteki `403 PERMISSION_DENIED` kalıcı başarısız olarak ayrılır, kuyruğu tıkamaz.

## Aşamalar (her biri ayrı PR; önce sunucu)
| # | Depo | İş |
|---|---|---|
| S1 | ErpBridge | Katalog + resolver + parite testi (rollerin 31 birleşimi ≡ bugünkü `RolePermissions`/`ApprovalPermissions`) |
| S2 | ErpBridge | Tablolar + migration + `CheckAsync` yüklemesi + oturum alanları + katalog ucu |
| S3 | ErpBridge | Rol ve kullanıcı okuma/yazma uçları + denetim kaydı |
| S6 | ErpBridge | Panel: oturum yetkileri, `PortalRoles`, `UserPermissionsSheet`, `Yetkiler.razor` |
| P1 | Siparis_Cepte | İzin modeli, önbellek, çevrimdışı geçiş |
| S4 ∥ P2 | ikisi | Sunucuda `RolePermissions` geçişi ∥ telefonda uygulama yerleri |
| P3 | Siparis_Cepte | Limitler ve onaya yönlendirme, uyarılar |
| S5 | ErpBridge | Ingest'te modül + limit kontrolü → 409 |
| P4 | Siparis_Cepte | Telefon editörleri (Rol Yetkileri, Yetkilerim, kullanıcı yetkileri, çoklu rol) |
| S7/P5 | ikisi | Bilgi bankası (kural + veri sözlüğü), `docs/mobil-belge-sozlesmesi.md`, `docs/GOAL_YETKILER.md` + `_DURUM.md` |

Sunucu PR'ları `main`'e birleşince Coolify canlıya dağıtır; her dağıtımdan sonra `/health/schema` kontrol edilir. Telefon sürümleri yerel derleme ve testler yeşilken elle birleştirilip Play internal kanalına yüklenir (Actions kredisi yok).

## Riskler
- **Kilitlenme:**
  - ADMIN kilitli; `action.users.manage` yalnız ADMIN'de; `LAST_ADMIN` kuralı aynen.
  - Hiç modülü kalmayan kullanıcı giriş yapar ve "Yetkili modül yok — yöneticinize başvurun" mesajını görür.
- **Çevrimdışı:** Yetki kaldırma telefona bir sonraki `/me` çağrısında gelir. Çevrimdışı girilen belge sunucuda yine kontrol edilir ve gerekirse onay talebine döner.
- **Eski uygulama sürümleri:** Arayüzde yetki kısıtı görmezler, ama belge yetkileri sunucuda 409 ile uygulanır.
- **Performans:** İstek başına iki küçük indeksli sorgu eklenir (genelde sıfır satır döner).

## Doğrulama
- **Sunucu:**
  - Derleme ve test: `dotnet build ErpBridge.sln -c Debug` (0 uyarı) ve `dotnet test`; `dotnet-ef migrations has-pending-model-changes` temiz.
  - Yeni testler:
    - `PermissionResolverTests` (parite testi dahil).
    - `PermissionSessionRelationalTests`.
    - Yetki uçlarının ilişkisel testleri: 403, 409 kilit, sıfırlama, denetim kaydı.
    - `IngestPermissionRelationalTests`: limit aşımında 409, API anahtarı etkilenmez, onaylanan belge kaydedilir.
    - `DocumentLimitFactsTests`, `PortalRolesTests`.
  - Dağıtım sonrası `/health/schema` "current" dönmeli.
- **Telefon:**
  - Derleme ve test: `./gradlew :app:compileDebugKotlin :app:testDebugUnitTest` ve `python scripts/check_error_reporting.py`.
  - Yeni testler: `UserPermissionsTest`, `RoutePermissionsTest`, `LimitCheckTest`, `ApprovalGate` yönlendirme testi, editör durumu → PUT farkı. `DataEditPolicy`, `QuickActionModules`, `EodOwnDay` ve depo testleri güncellenir.
- **Uçtan uca (elle):**
  - Panelden SALES şablonunda Raporlar kapatılır → satış kullanıcısının telefonunda Raporlar görünmez ve açılmaz.
  - Bir kişiye iskonto sınırı %10 verilir → %15 iskontolu satış uyarıyla onaya gider; sunucu da 409 ile onaya çevirir.
  - Kişiye "İzin ver" istisnası verilir → kaynak etiketi "Kişisel" görünür, değişiklik geçmişinde kim yaptığı yazar.
