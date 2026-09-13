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
