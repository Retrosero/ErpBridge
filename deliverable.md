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
