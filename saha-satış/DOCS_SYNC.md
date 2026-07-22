# GoApp Cloud - Ürün ve Stok Senkronizasyon Akışı

Bu dokümantasyon, FieldSales uygulamasının sunucudan (GoApp Cloud) ürünleri ve ilgili stok/fiyat verilerini nasıl çektiğini ve yerel veritabanına kaydettiğini açıklar.

## 1. Ağ Bağlantısı ve Güvenlik (ApiClient)
Uygulama, `ApiClient` sınıfı üzerinden Retrofit yapılandırmasını yönetir.
- **Base URL:** Sabit olarak `https://api.appsgo.cloud/` şeklindedir.
- **Endpointler:** Retrofit `@POST` annotation'ı ile `api/v1/android/sync/...` şeklinde tanımlanmıştır.
- **Güvenlik:** API anahtarı (`api_key`) ve Müşteri Kodu (`tenant_id`), `HttpLoggingInterceptor` tarafından `BASIC` seviyesinde loglanarak konsola veya loglara sızması engellenmiştir. İstek body'sinde güvenli şekilde taşınırlar.

## 2. API İstek Yapısı (PullJobsRequest)
Tüm senkronizasyon istekleri `POST` metodu ile yapılır. `BridgeSyncHelper`, `erp_settings` adlı `EncryptedSharedPreferences` (veya normal SharedPreferences) alanından ilgili bilgileri okur:
- `tenant_id` (Müşteri Kodu)
- `api_key` (Güvenlik Anahtarı)
- `device_id` (Uygulama ilk kurulduğunda üretilen UUID)
- `agent_version` (Varsayılan olarak "v2.0-multi-tenant")
- `page` ve `pageSize` (Sayfalama desteği)

## 3. Ürün Çekme Süreci (BridgeSyncHelper.syncUrunler)
Ürün senkronizasyonu `syncUrunler` fonksiyonunda üç aşamalı olarak gerçekleştirilir:

### Adım 3.1: Stok Seviyelerinin Çekilmesi
İlk olarak `api/v1/android/sync/stokSeviye` endpoint'ine POST isteği atılır. Dönen kayıtlardaki `actualMiktar`, ürün koduna (`stokKod`) karşılık gelecek şekilde yerel bir `HashMap` içinde eşleştirilmek üzere geçici olarak belleğe alınır.

### Adım 3.2: Barkodların Çekilmesi
Daha sonra `api/v1/android/sync/barkodTanimi` endpoint'ine POST isteği atılarak ürünlere ait barkod listeleri çekilir. Bunlar da ürün kodu bazında gruplanarak bir map'te tutulur.

### Adım 3.3: Ana Ürünlerin Çekilmesi ve Eşleştirme
Son olarak `api/v1/android/sync/urun` endpoint'ine sayfalama yapılarak (hasMore mantığı ile) POST istekleri atılır. 
- Sunucudan gelen `UrunDto` listesi (tenant_id, api_key gibi standart alanları içeren JSON DTO) üzerinden geçilir.
- Her ürün için, 3.1 ve 3.2 adımlarından elde edilen stok miktarı ve barkodlar ürün nesnesi ile birleştirilerek `ProductEntity` modeline dönüştürülür.

## 4. Yerel Veritabanına Yazma (Room DB)
Bellekte birleştirilen veriler, doğrudan uygulamanın ön yüzündeki State listelerine yazılmaz.
- Veriler, uygulamanın `AppDatabase` katmanındaki `ProductDao` sınıfına `insertAll` metodu ile yollanır.
- Bu işlem, `OnConflictStrategy.REPLACE` mantığı kullanılarak **Idempotent Upsert** şeklinde gerçekleştirilir. (Yani, aynı ürün kodu / barkod geldiğinde önceki veriyi ezer, yeni gelenleri ekler).
- Uygulamanın UI (Arayüz) tarafı, `ProductDao.getAllProductsFlow()` üzerinden aktif olarak Room DB'yi dinler (`collect`). Bu sayede arka planda ağdan veri gelip veritabanı güncellendiğinde, arayüz anında tepki vererek yeni listeyi gösterir.
