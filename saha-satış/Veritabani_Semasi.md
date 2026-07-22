# Uygulama Veritabanı Şeması

Uygulamanın yerel veritabanı (Room Database tabanlı cihaz-içi SQLite) yapısı aşağıdaki tablo ve alanlardan oluşmaktadır.

## Tablolar

### 1. `users` (Kullanıcılar)
Uygulamaya kayıtlı sistemi kullanacak yetkili kullanıcı veya personellerin bilgisini saklar.

| Alan Adı | Veri Tipi | Kısıtlamalar | Açıklama |
| :--- | :--- | :--- | :--- |
| `username` | String | **Primary Key** | Kullanıcı adı (Benzersiz) |
| `passwordHash` | String | | Şifrelenmiş parola özeti |
| `fullName` | String | | Kullanıcının tam adı |
| `email` | String | | E-posta adresi |
| `isLoggedIn` | Boolean | | Kullanıcının oturum durumu |


### 2. `customers` (Müşteriler ve Cari Hesaplar)
Uygulamadaki alıcı, satıcı ve tüm cari hesap bilgilerini saklar.

| Alan Adı | Veri Tipi | Kısıtlamalar | Açıklama |
| :--- | :--- | :--- | :--- |
| `id` | String | **Primary Key** | Benzersiz cari kodu (Örn: C001) |
| `name` | String | | Cari unvanı / Adı Soyadı |
| `balance` | Double | | Güncel bakiye (Borç / Alacak) |
| `lastVisit` | String | | Son ziyaret / işlem tarihi |
| `contact` | String | | İlgili kişi |
| `phone` | String | | Telefon numarası |
| `address` | String | | Açık adres bilgisi |
| `taxOffice` | String | | Vergi dairesi |
| `taxNumber` | String | | Vergi kimlik veya TC kimlik numarası |
| `gpsLocation` | String | | GPS lokasyon bilgisi |
| `riskLimit` | Double | | Belirlenmiş açık hesap / risk limiti |
| `priceGroup` | String | | Atanmış fiyat grubu / klasmanı |
| `specialDiscountPercent`| Double | | Cari hesaba özel indirim oranı (%) |
| `transactionsJson` | String | | Müşteri işlemlerinin JSON formatında (`List<CustomerTx>`) kopyası |


### 3. `products` (Ürünler ve Stok Kataloğu)
Tüm ürünlerin tanımlarını, fiyatlarını ve depolardaki stok durumlarını tutar.

| Alan Adı | Veri Tipi | Kısıtlamalar | Açıklama |
| :--- | :--- | :--- | :--- |
| `barcode` | String | **Primary Key** | Barkod numarası (Benzersiz) |
| `code` | String | | Ürün veya stok (SKU) kodu |
| `title` | String | | Ürün adı |
| `category` | String | | Ürün kategorisi |
| `desc` | String | | Ürün açıklaması |
| `basePrice` | Double | | Temel / Perakende satış fiyatı |
| `dealerPrice` | Double | | Bayi satış fiyatı |
| `wholesalePrice` | Double | | Toptan satış fiyatı |
| `kdvPercent` | Int | | KDV Oranı (%) |
| `colorValue` | Long | | UI tarafında gösterilecek görsel renk değeri |
| `brand` | String | Nullable | Ürünün markası |
| `stockByWarehouseJson` | String | | Depo bazlı stok miktarları (JSON: `Map<String, Int>`) |
| `boxQty` | Int | Nullable | Kutu içi adet |
| `packageQty` | Int | Nullable | Koli içi adet |
| `imageUrl` | String | Nullable | Online resim bağlantısı |
| `localImagePath` | String | Nullable | Cihaz üzerindeki resim dosya yolu |
| `aisle` | String | Nullable | Raf veya reyon numarası |
| `customPricesJson` | String | Nullable | Gruba özel fiyat kural tanımı (JSON formatında) |


### 4. `banks` (Banka ve Kasa Hesapları)
Nakit, pos tahsilatı gibi tutarların toplandığı finansal hesapların listesidir.

| Alan Adı | Veri Tipi | Kısıtlamalar | Açıklama |
| :--- | :--- | :--- | :--- |
| `id` | String | **Primary Key** | Referans banka/hesap ID (Örn: BNK1) |
| `name` | String | | Banka ya da Kasa hesabı adı |
| `accountNo` | String | | Hesap Numarası |
| `iban` | String | | IBAN numarası |
| `balance` | Double | | Banka / Hesap bakiyesi |


### 5. `kasa_logs` (Finansal Hareketler / Kasa Defteri)
Yapılan satışların ödemeleri veya tahsilatlardan doğan kasa hareket (gelir/gider) dökümleridir.

| Alan Adı | Veri Tipi | Kısıtlamalar | Açıklama |
| :--- | :--- | :--- | :--- |
| `id` | String | **Primary Key** | İşlem ID'si |
| `date` | String | | İşlem tarihi |
| `type` | String | | İşlem türü ("Satış", "Tahsilat", "Tediye" vb.) |
| `customerOrSupplier` | String | | İşlem yapılan karşı tarafın unvanı |
| `amount` | Double | | İşlem tutarı |
| `paymentType` | String | | Ödeme türü ("Nakit", "Kredi Kartı", "Havale/EFT", "Açık Hesap") |
| `bankName` | String | Nullable | İlgili hesaba/bankaya aktarıldıysa hesap adı |
| `desc` | String | | Açıklama (Belge no, makbuz vb.) |


### 6. `sales_records` (Satış Hareketleri Raporu)
Yapılmış satışların kalem bazlı (hangi cariden, hangi üründen ne kadar alındı vb.) analiz / rapor amaçlı tutulduğu istatistiksel data havuzudur.

| Alan Adı | Veri Tipi | Kısıtlamalar | Açıklama |
| :--- | :--- | :--- | :--- |
| `id` | Int | **Primary Key** (Auto Increment) | Satış satırı ID'si |
| `customerId` | String | | Müşterinin ID'si (customers tablosu ile ilişkili) |
| `productBarcode` | String | | Ürün Barkodu (products tablosu ile ilişkili) |
| `quantity` | Int | | Satış adeti |
| `price` | Double | | Birim fiyatı |
| `date` | String | | Satış tarihi |
