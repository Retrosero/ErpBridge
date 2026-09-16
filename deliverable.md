# Faz 48 — Muhasebe onay masası (Plan Adım 5) — Teslimat

## Değişen dosyalar

- `src/ErpBridge.Portal/Pages/Muhasebe.razor` (yeni): `/muhasebe` onay masası — liste + detay, kısayollar, not/red/toplu onay pencereleri, canlı liste.
- `src/ErpBridge.Portal/wwwroot/js/portal-keys.js` (yeni), `Pages/_Host.cshtml`: klavye dinleyicisi.
- `Api/Models.cs`, `Api/PortalApiClient.cs`, `Api/PortalMessages.cs` (`Fmt.Waiting`), `Session/PortalRoles.cs` (muhasebe açılışı `/muhasebe`), `MainLayout.razor` (menüde "Onay masası"), `wwwroot/css/site.css`.
- `src/ErpBridge.CentralApi/Endpoints/WarehouseEndpoints.cs`, `Contracts/WarehouseContracts.cs`, `Notifications/TenantEventHub.cs`, `Approvals/ApprovalService.cs`: `/portal/events` onay konusu (`approvalsSeq`), onay değişiklikleri portal hub'ına yayınlanır.
- Testler: `PortalApprovalDeskTests` (8, yeni), `WarehouseFulfillmentRelationalTests` (+1 onay long-poll), açılış sayfası beklentileri güncellendi.
- KB kural 19 (onay masası, klavye, sayfa kopyası tuzağı) ve 21 (onay konusu), `docs/PLAN_ROLLER_VE_DEPO.md`.

## Davranış

- Muhasebe girişte onay masasına düşer; fare kullanmadan gezinir, onaylar, notla reddeder, işaretleyip toplu onaylar.
- Başka bir yetkilinin kararı ya da yeni talep sayfa yenilenmeden görünür.
- Migration yok. Telefon uygulamasında değişiklik yok.

## Testler / derleme

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı / 0 hata. CentralApi 397, Portal 88; tüm çözüm yeşil.
- Codex (PR #57) üç bulgu düzeltildi: kuyruk `order=oldest` ile en eskiden okunur (500 sınırında en eskiler kaybolmaz); canlı liste milisaniye yerine konu başına sürümle izlenir (iki poll arası değişiklik kaçmaz); talep bazında `canDecide` ile yetkisiz türde düğme/kısayol/toplu onay kapalı.
- Kuyruk sırası bozulunca masa testlerinin 6/8'i kırıldı.
- Yerelde tarayıcıda gerçek tuşlarla: `A` ile 3 onay, `↓` + `R` + yazı + `Enter` ile notlu red, işaret + `Shift+A` + `Enter` ile 2 talep toplu onay, başka yetkilinin API'den reddi ≤ 3 sn'de masada, geniş ekranda iki sütun. Bu denemede bulunan iki hata düzeltildi: geri yüklenen oturumda sayfa kopyası atılırken devre çöküyordu; toplu onay penceresinde Enter odaklı düğmeye takılıyordu.

---

# Faz 47 — Depo sipariş hazırlık çekirdeği (Plan Adım 4) — Teslimat

## Değişen dosyalar

- `src/ErpBridge.CentralApi/Domain/OrderFulfillment.cs` (yeni): `OrderFulfillment`, `OrderFulfillmentEvent`, `TenantWarehouseSettings`, durum/eylem/ERP durumu sabitleri.
- `Data/CentralApiDbContext.cs`, `Data/Migrations/*_Faz47OrderFulfillment.cs`: üç yeni tablo (mevcut tablolara dokunmaz).
- `Warehouse/FulfillmentService.cs` (yeni): kuyruğa alma, geçişler (tek kazanan), geri alma penceresi, iptal, yeniden atama, ERP durumu, ayarlar.
- `Endpoints/WarehouseEndpoints.cs` (yeni): `/api/v1/portal/fulfillments`, `/{id}`, `/{id}/{action}`, `/warehouse/settings`, `/events` (long-poll).
- `Notifications/TenantEventHub.cs` (yeni): portal için ayrı bellek içi hub.
- `Endpoints/IngestEndpoints.cs`, `Approvals/ApprovalService.cs`: satış siparişi yazıldığı transaction'da kuyruğa girer.
- `Endpoints/JobsEndpoints.cs`, `Endpoints/AdminJobsEndpoints.cs`: ajan sonucu ve yeniden deneme ERP durumunu günceller.
- `Domain/MobileUser.cs`: `RolePermissions.CanManageWarehouse`. `Program.cs`: kayıtlar ve uçlar.
- Testler: `WarehouseFulfillmentRelationalTests` (11).
- KB kural 11 (sayaç), yeni kural 21, 03 veri sözlüğü, `docs/PLAN_ROLLER_VE_DEPO.md`.

## Davranış

- Depo modülü firma bazında açılır (varsayılan kapalı). Açıkken telefondan doğrudan gelen ya da onaylanan her satış bir kez kuyruğa girer; ERP'li firmada ajan yazmadan önce.
- Eşzamanlı iki "başla"dan biri 409 alır; her adım değişmez günlüğe yazılır; geri alma yalnız adımı atana 5 dk içinde ya da yöneticiye açıktır; iptal/yeniden atama yöneticidedir.
- ERP'ye yazılamayan sipariş kartta `FAILED` olur, depo akışı durmaz.
- Panel arayüzü yok (adım 5-7). Telefon uygulamasında değişiklik yok.
- **Migration var** (`Faz47OrderFulfillment`, yalnız yeni tablolar): canlıda `/health/schema` 22 applied / 0 pending olmalı.

## Testler / derleme

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı / 0 hata; `has-pending-model-changes`: yok.
- CentralApi 392 (12 yeni), diğer projeler değişmeden yeşil.
- Codex (PR #52): değişiklik sayfasının imleci dönen son satırda biter (`hasMore`); long-poll bekleyicisi okumadan önce kaydolur. İkisi düzeltildi.
- Yarış testi, koşullu güncelleme kaldırılınca 3/3 kırıldı (testin kilidi gerçekten sınadığı doğrulandı).

---

# Faz 46 — Panel: Beni Hatırla + rol bazlı arayüz (Plan Adım 3) — Teslimat

## Değişen dosyalar

- `src/ErpBridge.CentralApi/Contracts/MobileAccountContracts.cs`, `Authentication/JwtIssuer.cs`, `Endpoints/MobileAccountEndpoints.cs`: login `rememberMe`; panel oturumu işaretliyse 30 gün, değilse 12 saat; telefon her zaman 30 gün.
- `src/ErpBridge.Portal/Session/PortalRoles.cs` (yeni): roller, açıklamalar, `PortalArea`, açılış sayfası.
- `Session/PortalSession.cs`: `Roles`, `RememberMe`, `Allows`, `HomePage`; eski tek rollü oturum durumu okunur.
- `Session/ProtectedBrowserPersistence.cs` (eski `ProtectedSessionPersistence`): hatırlanan oturum şifreli `localStorage`'da, diğeri `sessionStorage`'da; biri yazılınca öteki silinir.
- `Shared/PortalPageBase.cs`: `AdminOnly` yerine `Requires`; yetkisiz adres açılış sayfasına.
- `Shared/RolePicker.razor` (yeni), `Pages/Kullanicilar.razor`: çoklu rol çipleri, rol açıklamaları, "Roller" ile düzenleme.
- `Pages/Login.razor`: "Beni hatırla", rol setiyle kapı, role göre açılış sayfası. `MainLayout.razor`: rol bazlı menü, oturum kapsamı.
- `Pages/Depo.razor` (yeni, yer tutucu), tüm sayfalara `Requires`; `Api/Models.cs`, `PortalApiClient.cs`, `PortalMessages.cs`; `wwwroot/css/site.css`.
- Testler: `MobileUserRolesRelationalTests` (+1), `PortalRolesTests` (yeni), `PortalLayoutTests` (yeni), `PortalPagesTests`, `PortalManagementPagesTests`, `PortalSessionTests`.
- KB kural 14 ve 19, `docs/PLAN_ROLLER_VE_DEPO.md`.

## Davranış

- "Beni hatırla" işaretli: tarayıcı kapatılıp açılsa da 30 gün oturum sürer. Boş: sekme kapanınca biter, açık sekmede de en çok 12 saat.
- Menü ve sayfalar rol birleşimine göre: muhasebe Cariler/Stok/Onaylar, depo yalnız Depo, yönetici raporlar + onay + depo, admin hepsi. Yetkisiz adres açılış sayfasına yönlenir (muhasebe `/onaylar`, depo `/depo`).
- Kullanıcılar sayfasında birden çok rol verilir ve düzenlenir; admin kendi rollerini değiştiremez.
- Açık oturum rolleri bir dakikadan eskiyse `/account/me`'den tazelenir; panel rolü kalmayan kullanıcı çıkarılır.
- Codex incelemesi (PR #51) üç bulgu: rol düzenleyici etkin onay hakkını yönetici bayrağı sanıyordu (artık dokunulmadıkça `canApprove: null`); süresi dolmuş sekme oturumu hatırlanan oturumu siliyordu; açık oturum eski rollerle kalıyordu. Üçü düzeltildi ve testlendi.
- Migration yok.

## Testler / derleme

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı / 0 hata.
- CentralApi 380, Portal 79 test yeşil; diğer projeler değişmeden yeşil.
- Yerelde (Test ortamı + bellek içi DB) tarayıcıda: muhasebe ve depo girişi doğru sayfaya ve menüye; yetkisiz URL yönlendirmesi; "Beni hatırla" ile yeni sekmede ve panel yeniden başlatıldıktan sonra oturum sürüyor; rol düzenleme sunucuda kaydediliyor.

---

# Faz 45 — Çoklu rol altyapısı (Plan Adım 2) — Teslimat

## Değişen dosyalar

- `src/ErpBridge.CentralApi/Domain/MobileUser.cs`: `MobileUserRole`, `MobileUserRoles` (ACCOUNTING, WAREHOUSE, `Normalize`, `Legacy`), `RolePermissions`, `ApprovalPermissions` (tür bazlı).
- `Data/CentralApiDbContext.cs`, `Data/Migrations/*_Faz45MobileUserRoles.cs`: `mobile_user_roles` + mevcut rollerin taşınması.
- `Mobile/MobileSeatService.cs`: rol setiyle oluşturma/güncelleme, tek `role` ile uyumlu güncelleme, son admin kuralı.
- `Mobile/MobileUserAccess.cs`, `Authentication/CentralApiClaims.cs`, `Authentication/JwtIssuer.cs`: `client` iddiası, telefon/panel rol kapısı.
- `Endpoints/MobileAccountEndpoints.cs`, `AdminMobileSeatsEndpoints.cs`, `IngestEndpoints.cs`, `PortalEndpoints.cs`, `Approvals/ApprovalService.cs`, `Team/TeamDocumentProcessor.cs`: izinler `RolePermissions`'tan; panel oturumu belge gönderemez; muhasebe finansal türler.
- `Contracts/MobileAccountContracts.cs`: `client`, `roles`.
- `src/ErpBridge.Portal/Api/Models.cs`: login `client=portal`, kullanıcı `roles`.
- `Authentication/MobileUserStateHandler.cs`, `Program.cs`: `MobileClientPolicy` panel oturumunu reddeder (`PhoneClientOnly`).
- Testler: `MobileUserRolesRelationalTests` (13), `PortalSessionTests` güncellendi.
- KB kural 14/16/17/18, 03 veri sözlüğü, `docs/PLAN_ROLLER_VE_DEPO.md`.

## Davranış

- Mevcut kullanıcıların yetkisi değişmez (migration eski rolü satır olarak ekler; eski uygulamalar aynı `role` alanını görür).
- Panel oturumu telefonun veri akışını (sync/pull, bootstrap, notify…) okuyamaz (Codex incelemesi).
- Yalnız depo/muhasebe rolü olan kullanıcı telefondan giremez; yalnız saha rolü olan panelden giremez; rol kaldırılınca bir sonraki istekte etkili.

## Testler / derleme

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı / 0 hata; `has-pending-model-changes`: yok.
- Mevcut testler değişmeden geçti (CentralApi 366); yeni rol testleri 13/13. İki koruma bilinçli bozulunca 4 test kırıldı.

---

# Faz 44 — Kalıcı portal oturum anahtarı ve şema durumu (Plan Adım 1) — Teslimat

## Değişen dosyalar

- `src/ErpBridge.CentralApi/Health/SchemaStatus.cs`, `Program.cs`: `GET /health/schema` (bekleyen migration → 503) ve açılışta `LogCritical`.
- `tests/ErpBridge.CentralApi.Tests/Endpoints/HealthSchemaTests.cs`: pending / current / not-relational.
- `src/ErpBridge.Portal/Session/PortalDataProtection.cs`, `Program.cs`: `DataProtection:KeysPath` ile kalıcı anahtar dizini; yoksa Production'da uyarı.
- `Dockerfile.portal`: `DataProtection__KeysPath=/app/keys`.
- `tests/ErpBridge.Portal.Tests/PortalDataProtectionTests.cs`: dağıtım öncesi/sonrası aynı anahtarla okuma; farklı anahtarla okunamama.
- `docs/PLAN_ROLLER_VE_DEPO.md` (goal plan), `docs/deploy-coolify.tr.md`, `ErpBridge_knowledge_base/00_System_Overview.md` (kural 19, yeni kural 20).

## Davranış

- Migration başarısız olup uygulama eski şemayla açılırsa artık hem logda hem `/health/schema`'da görünür. Konteyner sağlık kontrolü değişmedi (yeniden başlatma döngüsü olmasın).
- Portal yeniden dağıtıldığında tarayıcıda saklı oturumlar düşmez (Coolify'da `/app/keys` kalıcı volume ile). "Beni hatırla" (Adım 3) için önkoşul.

## Testler / derleme

- Yeni testler: `HealthSchemaTests` 3/3, `PortalDataProtectionTests` 2/2. Tam derleme ve test sonucu PR'da.

---

# Faz 43 — Yönetici paneline kurumsal arayüz (MudBlazor) — Teslimat

## Değişen dosyalar

- `Directory.Packages.props`, `src/ErpBridge.Portal/ErpBridge.Portal.csproj`: `MudBlazor` 9.9.0 (MIT).
- `src/ErpBridge.Portal/Program.cs`, `Pages/_Host.cshtml`, `_Imports.razor`: MudBlazor servisleri ve kendi sunucumuzdan CSS/JS.
- `src/ErpBridge.Portal/Shared/PortalTheme.cs`, `PageHeader.razor`, `StatCard.razor`, `EmptyState.razor`, `PageLoading.razor`: tema ve ortak parçalar.
- `src/ErpBridge.Portal/MainLayout.razor`: sol menü, üst bar, kullanıcı menüsü.
- `src/ErpBridge.Portal/Pages/*.razor`: giriş, özet, plasiyerler, ziyaretler, cariler, stok, onaylar, kullanıcılar yeniden tasarlandı.
- `src/ErpBridge.Portal/Api/PortalMessages.cs`: `Fmt.Initials`.
- `src/ErpBridge.Portal/wwwroot/css/site.css`: kurumsal görünüm katmanı ve mobil kırılımlar.
- `tests/ErpBridge.Portal.Tests/*`: `PortalPageTestContext` (MudBlazor `PopoverService` yalnız async dispose edilir), seçiciler MudBlazor çıktısına uyarlandı.
- `ErpBridge_knowledge_base/00_System_Overview.md` (kural 19).

## Davranış

- Veri akışı, API çağrıları ve yetki kuralları değişmedi; CentralApi ve veritabanına dokunulmadı.
- Giriş düğmesi alanlar boşken pasif değil: pasif gönder düğmesi tarayıcının Enter gönderimini engelliyordu. Boş alan kontrolü `SignInAsync` içinde, Türkçe mesajla.

## Testler / derleme

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı / 0 hata. Portal testleri 37/37, CentralApi 363/363, çözümün tamamı yeşil.
- Yerel CentralApi + deneme verisiyle tarayıcıda giriş (Enter dahil), özet, plasiyerler, onaylar, kullanıcılar ekranları kontrol edildi. Telefon genişliği görsel olarak doğrulanmadı.

---

# Faz 42 — Firma yönetici paneli (`ErpBridge.Portal`) — Teslimat

## Değişen dosyalar

- `ErpBridge.sln`, `src/ErpBridge.Portal/*` (yeni Blazor Server uygulaması), `Dockerfile.portal` (port 4003).
- `tests/ErpBridge.Portal.Tests/*` (bUnit): oturum yalıtımı, rol kapısı, oturum bitişi, sayfa davranışları.
- `ErpBridge_knowledge_base/00_System_Overview.md` (kural 19).

## Davranış

- Firmanın admini/yöneticisi telefondaki hesabıyla tarayıcıdan girer; plasiyer istemcide ve sunucuda (`PORTAL_REQUIRES_MANAGER`) reddedilir.
- Oturum devre başına (`Scoped`), token yalnız sekmenin şifreli `sessionStorage`'ında.
- Yeni sunucu ucu yok: Faz 41 portal uçları ile mevcut onay ve kullanıcı uçları kullanılır.
- Canlıda Coolify uygulaması `lisans-portal`, `https://panel.admin.lisans.appsgo.cloud`.

## Testler / derleme

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı / 0 hata; Portal testleri 37/37, CentralApi 363/363.
- Yerel CentralApi'ye karşı tarayıcıda uçtan uca: plasiyer reddi, özet rakamları, kullanıcı ekleme/devre dışı bırakma (telefon girişi 403), onay/red notuyla.

---

# Düzeltme — Onay özeti: kendi talebini onaylayabilir mi — Teslimat

## Değişen dosyalar

- `src/ErpBridge.CentralApi/Contracts/ApprovalContracts.cs`, `Approvals/ApprovalService.cs`: `GET /api/v1/android/approvals/summary` yanıtına `canApproveOwnRequests` (onaycı ve firmada başka aktif onaycı yok).
- `tests/.../ApprovalCentreRelationalTests.cs`: özet alanı iki durumda doğrulanır.
- `ErpBridge_knowledge_base/00_System_Overview.md` (kural 16).

## Davranış

- İki cihazlı testte, başka onaycı varken talep sahibine sunucunun reddedeceği Onayla düğmesi gösteriliyordu; telefon bu alanla düğmeyi gizler. Alan eklemedir, eski uygulamalar etkilenmez.

## Testler / derleme

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı / 0 hata; `ApprovalCentreRelationalTests` 17/17.

---

# Düzeltme — Koltuk kaydında bitiş tarihi UTC'ye çevrilir — Teslimat

## Değişen dosyalar

- `src/ErpBridge.CentralApi/Mobile/MobileSeatService.cs`: `SetSubscriptionAsync` bitiş tarihini `ToUniversalTime()` ile kaydeder.
- `tests/ErpBridge.CentralApi.Tests/Endpoints/MobileSeatsRelationalTests.cs`: yerel saatle (+03:00) gönderilen bitiş tarihinin UTC olarak saklandığı test.

## Davranış

- Konsol bitiş tarihini Türkiye saatiyle gönderiyordu; PostgreSQL (`timestamp with time zone`, Npgsql) yalnızca UTC kabul ettiği için bitiş tarihli her koltuk kaydı 500 dönüyordu. API anahtarı ve lisans uçları aynı dönüşümü zaten yapıyordu.

## Testler / derleme

- Yeni test düzeltmeden önce başarısız (offset 3 saat), sonra başarılı. `dotnet build ErpBridge.sln -c Debug`: 0 uyarı / 0 hata; CentralApi testleri 322/322.

---

# Faz 38 — Sunucuda onay merkezi (çok onaycılı, kalıcı) — Teslimat

## Değişen dosyalar

- `src/ErpBridge.CentralApi/Domain/ApprovalRequest.cs`: `ApprovalRequest`, `ApprovalRequestEvent`, `TenantApprovalRules`, `ApprovalStatuses`, `ApprovalActions`, `ApprovalKinds`.
- `src/ErpBridge.CentralApi/Domain/MobileUser.cs`: `MANAGER` rolü, `CanApprove`, `CanManageApprovalRules`, `ApprovalPermissions`.
- `src/ErpBridge.CentralApi/Approvals/ApprovalService.cs`: talep, onay/red, tekrar açma, geri çekme, düzeltip yeniden gönderme, kurallar.
- `src/ErpBridge.CentralApi/Endpoints/MobileApprovalEndpoints.cs`: `/api/v1/android/approvals/*`.
- `src/ErpBridge.CentralApi/Endpoints/IngestEndpoints.cs`: `approval_request` belgesi; kuralı açık türde doğrudan belge 409 `APPROVAL_REQUIRED`.
- `src/ErpBridge.CentralApi/Native/NativeDocumentProcessor.cs`: dış transaction'a katılma; `StockShortagesAsync`.
- `src/ErpBridge.CentralApi/Mobile/MobileSeatService.cs`, `Contracts/*`, `Endpoints/MobileAccountEndpoints.cs`, `Endpoints/AdminMobileSeatsEndpoints.cs`: rol/yetki alanları, session `approvalRules`, konsol onay listesi.
- `src/ErpBridge.CentralApi/Data/CentralApiDbContext.cs`, migration `Faz38ApprovalCentre`.
- `src/ErpBridge.Admin/Api/CentralApiClient.cs`, `Pages/TenantMobile.razor(.css)`, `Shared/MobileLoginPanel.razor`: Admin/Yönetici/Saha rolleri, yönetici yetkileri, salt okunur onay merkezi bölümü.
- `src/ErpBridge.Admin/Shared/MobileLoginPanel.razor(.css)`: lisans sayfasında her telefon kullanıcısına (firma admini dahil) parola belirleme; ilk kullanıcı "Firma admini oluştur" olarak sunulur.
- `ErpBridge_knowledge_base/00_System_Overview.md` (kural 16), `03_Data_Dictionary_and_Rules.md`.

## Davranış

- Onay talepleri ve kuralları PostgreSQL'de; admin ve yetkili yöneticiler aynı kuyruğu görür, diğer kullanıcılar yalnız kendi taleplerini.
- Karar tek seferliktir; onay belgeleri aynı transaction'da işler, işlenemeyen belge talebi beklemede bırakır.
- Reddedilen talep tekrar açılabilir veya talep eden tarafından düzeltilip yeniden gönderilir; bekleyen talep geri çekilebilir.
- Kural açıkken telefon onayı atlayamaz (409). Eski uygulama sürümleri kural açık türlerde 409 alır; telefon sürümü aynı gün yayımlanmalı.

## Testler

- `ApprovalCentreRelationalTests`: 17 test (SQLite).
- `TenantMobilePageTests`: 2 yeni test (toplam 6).
- `MobileLoginPanelTests`: 2 yeni test (toplam 5).
- `NativeTenantRelationalTests`, `MobileSeatsRelationalTests`: doğrudan belge gönderen testler kuralları kapatarak / kural dışı tür kullanarak güncellendi.

## Derleme çıktısı

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı, 0 hata. `dotnet test ErpBridge.sln`: tümü başarılı (CentralApi 321, Admin 39; canlı Mikro testleri atlandı). `dotnet ef migrations has-pending-model-changes`: değişiklik yok.

---

# Faz 37 — ERP'siz firmada sayımın stoğa etkisi — Teslimat

## Değişen dosyalar

- `src/ErpBridge.CentralApi/Native/NativeDocumentProcessor.cs`: `stock_count` (COMPLETED); satır başına sayılan − beklenen farkı stoğa uygulanır, fark hareketi yazılır.
- `ErpBridge_knowledge_base/00_System_Overview.md` (kural 15).

## Davranış

- Sayım farkı stoğu artırır veya azaltır; sayımdan sonra işlenen satışlar korunur.
- Farksız satır hareket yazmaz. Bilinmeyen ürünlü veya tamamlanmamış sayım hiçbir stoğu değiştirmez.

## Testler

- `NativeTenantRelationalTests`: 2 yeni test (toplam 24).

## Derleme çıktısı

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı, 0 hata. `dotnet test ErpBridge.sln`: tümü başarılı. Şema değişikliği yok.

---

# Faz 36 — ERP'siz firmada iade ve alışın stoğa etkisi — Teslimat

## Değişen dosyalar

- `src/ErpBridge.CentralApi/Native/NativeDocumentProcessor.cs`: `sales_return`, `purchase_receipt`; satış/iade/alış satırları için ortak `BookLinesAsync` (önce tüm satırları doğrular); `NativeDocumentTypes`.
- `src/ErpBridge.CentralApi/Endpoints/IngestEndpoints.cs`: ERP tenant'ında iade/alış belgesi 409 `DOCUMENT_REQUIRES_NATIVE_TENANT`.
- `ErpBridge_knowledge_base/00_System_Overview.md` (kural 15).

## Davranış

- İade: stok girer, cari alacaklanır; nakit veya bankayla geri ödenirse açık bakiye değişmez.
- Alış: stok girer, tedarikçi alacaklanır; peşin ödenirse açık bakiye değişmez. Katalog dışı kalem stoğa girmez ama tutarı tedarikçiye yazılır.
- Bilinmeyen ürün satırı olan belge hiçbir stok veya bakiyeyi değiştirmez. İade veya alış görmüş ürün silinemez.

## Testler

- `NativeTenantRelationalTests`: 4 yeni test (toplam 22); mevcut satış testleri ortak satır koduyla geçer.

## Derleme çıktısı

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı, 0 hata. `dotnet test ErpBridge.sln`: tümü başarılı. Şema değişikliği yok.

---

# Faz 35 — Toplu kart aktarımı ve lisans sayfasında telefon girişi — Teslimat

## Değişen dosyalar

- `src/ErpBridge.CentralApi/Native/NativeDocumentProcessor.cs`: `stock_card_batch` (yalnız yönetici), `customer_card_batch`; geçersiz kart atlanır ve iş notuna yazılır.
- `src/ErpBridge.Admin/Shared/MobileLoginPanel.razor` (+ `.razor.css`): firma kodu, hak kullanımı, hak tanımlama, kullanıcı adı/parola/rol ile telefon girişi oluşturma.
- `src/ErpBridge.Admin/Pages/Licenses.razor` (+ `.razor.css`): her lisans kartında "Telefon girişi oluştur" düğmesi ve paneli.
- `ErpBridge_knowledge_base/00_System_Overview.md` (kural 14, 15).

## Davranış

- Excel ile gelen yüzlerce kart birkaç belgeyle işlenir; boş satır diğer kartları engellemez.
- Operatör lisans sayfasından doğrudan telefon kullanıcısı açar; ekranda firma kodu ve kullanıcı adı özetlenir, parola tekrar gösterilmez. Hak yoksa önce hak tanımlanır.

## Testler

- `NativeTenantRelationalTests`: 2 yeni test (toplam 18).
- `MobileLoginPanelTests`: 3 bUnit testi.

## Derleme çıktısı

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı, 0 hata. `dotnet test ErpBridge.sln`: tümü başarılı. Şema değişikliği yok.

---

# Faz 34 — ERP'siz firmada ürün kartı düzenleme ve silme — Teslimat

## Değişen dosyalar

- `src/ErpBridge.CentralApi/Native/NativeDocumentProcessor.cs`: `stock_card_delete` (yalnız yönetici, hareket görmüş ürün reddedilir); `stock_card` güncellemesinde eski barkod kayıtlarının düşürülmesi; satış satırı `LastMovementAtUtc` işaretler.
- `src/ErpBridge.CentralApi/Sync/MobileRecordProjector.cs`: `TombstoneAsync` (bilinen satırları imleç bloğuyla düşürür); `ApplyDeletesAsync` bunu kullanır.
- `src/ErpBridge.CentralApi/Domain/NativeLedger.cs`, `Data/Migrations/*_Faz34NativeStockMovementMark.cs`: `native_stock_levels.LastMovementAtUtc` (yalnızca ekleme).
- `ErpBridge_knowledge_base/00_System_Overview.md` (kural 15), `03_Data_Dictionary_and_Rules.md`.

## Davranış

- Yönetici ürün kartını aynı kodla yeniden gönderince ad, fiyat ve barkod tüm cihazlarda güncellenir; stok değişmez; eski barkodla satış reddedilir.
- Hareketi olmayan ürün silinince kart, barkod, fiyat ve stok satırları düşer, cihazlara silinmiş gider.
- Satışı olan ürün ve saha kullanıcısının silme isteği `Failed` kaydedilir. ERP tenant'ında silme belgesi 409.

## Testler

- `NativeTenantRelationalTests`: 3 yeni test (toplam 16). Eski barkod temizliği mutasyonla doğrulandı.

## Derleme çıktısı

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı, 0 hata. `dotnet test ErpBridge.sln`: tümü başarılı. `has-pending-model-changes`: değişiklik yok.

---

# Faz 33 — ERP'siz firma (native tenant) — Teslimat

## Değişen dosyalar

- `src/ErpBridge.CentralApi/Domain/Tenant.cs`: `DataSource` (`erp` | `native`), `NativeLockVersion`.
- `src/ErpBridge.CentralApi/Domain/NativeLedger.cs`: `NativeStockLevel`, `NativeCustomerBalance`.
- `src/ErpBridge.CentralApi/Data/CentralApiDbContext.cs`, `Data/Migrations/*_Faz33NativeTenants.cs`: `native_stock_levels`, `native_customer_balances`, yeni tenant kolonları (yalnızca ekleme).
- `src/ErpBridge.CentralApi/Native/NativeDocumentProcessor.cs`: native tenant belgelerini tek transaction'da saklar, işler ve `mobile_records`'a projekte eder.
- `src/ErpBridge.CentralApi/Native/NativeTenantGuard.cs`: ERP yazıcılarının native tenant'ı reddetmesi.
- `src/ErpBridge.CentralApi/Endpoints/IngestEndpoints.cs`: native tenant'ta işleyiciye yönlendirme; ERP tenant'ında kart belgesi 409.
- `src/ErpBridge.CentralApi/Endpoints/AgentsEndpoints.cs`, `BootstrapEndpoints.cs`, `BootstrapUploadEndpoints.cs`, `ChangeSetEndpoints.cs`: native tenant'a ajan kaydı ve ERP yüklemesi 409 `TENANT_IS_NATIVE`; ajan kaydı veri kaynağını tenant satır kilidi altında yeniden okur.
- `src/ErpBridge.CentralApi/Endpoints/AdminMobileSeatsEndpoints.cs`: `PUT .../mobile/data-source`; ajan, ERP verisi veya bekleyen iş varken native'e, telefon verisi varken erp'ye geçiş reddi; tenant kilidi altında.
- `src/ErpBridge.CentralApi/Contracts/MobileAccountContracts.cs`, `Endpoints/MobileAccountEndpoints.cs`: oturum ve genel görünümde `dataSource`.
- `src/ErpBridge.Admin/Api/CentralApiClient.cs`, `Pages/TenantMobile.razor`: konsolda veri kaynağı seçimi ve Türkçe hata mesajları.
- `ErpBridge_knowledge_base/00_System_Overview.md` (kural 15), `03_Data_Dictionary_and_Rules.md`.

## Davranış

- Native tenant'ta `stock_card` (yalnız yönetici), `customer_card`, `sales_order` (stok düşer, cari borçlanır, anında ödemede tahsilat hareketi), `collection` (cari alacaklanır) sunucuda işlenir; diğer türler kayıt olarak saklanır.
- Bilinmeyen ürün kodu, eksi fiyat veya eksi toplam içeren satış `Failed` kaydedilir ve hiçbir stok/bakiye değişmez. Stok eksiye düşebilir.
- ERP tenant'larının davranışı değişmedi.

## Testler

- `tests/ErpBridge.CentralApi.Tests/Endpoints/NativeTenantRelationalTests.cs`: 13 test (SQLite).

## Derleme çıktısı

- `dotnet build ErpBridge.sln -c Debug`: 0 uyarı, 0 hata.
- `dotnet test ErpBridge.sln`: tüm projeler başarılı (CentralApi, Admin, Core, Erp.Mikro, Erp.Sql, LocalStore, RemoteApi, Agent.Service, Shared); canlı Mikro testleri atlandı.
- `dotnet ef migrations has-pending-model-changes`: değişiklik yok.

---

# Faz 32 — Mobil kullanıcı, cihaz ve ücretli koltuk — Teslimat (özet)

- `mobile_users`, `mobile_devices`, `tenant_subscriptions`, `tenants.Code`; `Mobile/MobileSeatService`, `MobileUserAccess`; `/api/v1/android/account/*`, `/api/v1/admin/tenants/{id}/mobile/*`; Admin konsolu `Pages/TenantMobile.razor`.
- Testler: `MobileSeatsRelationalTests` (14), `TenantMobilePageTests` (4). Birleştirildi: Retrosero/ErpBridge#34.

---

# ErpBridge Admin Modernizasyonu — Teslimat

## Değişen alanlar

- `src/ErpBridge.Admin/MainLayout.razor`: masaüstü kenar çubuğu, mobil drawer, kullanıcı alanı ve erişilebilir navigasyon.
- `src/ErpBridge.Admin/wwwroot/css/site.css`: ortak tasarım tokenları, responsive grid/kart/tablo davranışları, odak ve azaltılmış hareket desteği.
- `src/ErpBridge.Admin/Shared/`: ortak sayfa başlığı, durum etiketi, yükleme ve boş/hata durumu bileşenleri.
- `src/ErpBridge.Admin/Pages/`: gerçek genel bakış Dashboard'u; Türkçeleştirilmiş, sadeleştirilmiş ve mobil uyumlu yönetim sayfaları.
- `tests/ErpBridge.Admin.Tests/`: ortak Razor bileşenleri için bUnit testleri.

## Davranış

- Mevcut API, DTO ve kimlik doğrulama sözleşmeleri korunmuştur.
- Teknik veri masaüstünde kompakt tablolar, mobilde etiketli kart satırları olarak gösterilir.
- Anahtar ve webhook sırlarının yalnızca bir kez gösterilmesi davranışı korunmuştur.
- 320, 375, 768, 1024 ve 1440 piksel genişliklerde giriş ekranında yatay taşma olmadığı doğrulanmıştır.
- Mobil menünün açma ve kapatma davranışı gerçek tarayıcıda doğrulanmıştır.

## Test ve derleme

- Admin bileşen testleri: 15/15 başarılı.
- `dotnet build ErpBridge.sln`: başarılı, 0 uyarı, 0 hata.
- `dotnet test`: Admin ve diğer bağımsız test projeleri başarılıdır. Tüm çözüm koşusunda canlı Mikro testleri atlandı ve mevcut `DotEnvLoaderSmokeTests` testi yerel `ERPBridge_TULPAR_*` bağlantı değişkenleri bulunmadığı için başarısız oldu. Bu modernizasyonla ilişkili değildir ve hiçbir gizli değer oluşturulmamıştır.
