# Katalog tasarımı teslim notu — 2026-10-01

Çalışma dizini: C:\Users\retro\Documents\GitHub\ErpBridge-katalog
Dal: duzeltme/katalog-kok

## Davranış

- Katalog başlangıcı, ürün kartları, masaüstü filtre paneli ve telefon/tablet yerleşimi yenilendi.
- Marka, stok, fiyat aralığı, indirim, koli ve görselli ürün filtreleri; ad, net fiyat ve ürün kodu sıralamaları eklendi. Filtreleme ve sıralama sayfalama öncesinde uygulanır.
- Etkin filtreler tek tek kaldırılabilir. Ürün detayından dönüşte arama ve filtreler korunur. Sıralama değişince henüz uygulanmamış marka seçimi korunur.
- Sipariş detayında mevcut ürün küçük görselleri gösterilir. Görseller siparişin firmasından alınır; geçmiş fiyatlara dokunulmaz.
- Kullanıcı isteğiyle canlıdaki eksik görsellerin yükleme/senkronizasyon araştırması ertelendi. Canlıya yayın yapılmadı.

## Değişen dosyalar

- src/ErpBridge.CentralApi/wwwroot/katalog/js/views/catalog.js
- src/ErpBridge.CentralApi/wwwroot/katalog/css/app.css
- src/ErpBridge.CentralApi/wwwroot/katalog/js/views/orders.js
- src/ErpBridge.CentralApi/Contracts/CatalogCustomerContracts.cs
- src/ErpBridge.CentralApi/Endpoints/CustomerCatalogPublicEndpoints.cs
- src/ErpBridge.CentralApi/Endpoints/CatalogCustomerOrderEndpoints.cs
- tests/katalog-web/mock-server.mjs
- tests/katalog-web/mock-server.test.mjs
- tests/katalog-web/catalog-filters.test.mjs (yeni)
- tests/ErpBridge.CentralApi.Tests/Endpoints/CustomerCatalogBrowseRelationalTests.cs
- tests/ErpBridge.CentralApi.Tests/Endpoints/CustomerCatalogOrdersRelationalTests.cs
- docs/GOAL_MUSTERI_KATALOGU.md

## Doğrulama

- dotnet build ErpBridge.sln: başarılı, 0 uyarı / 0 hata.
- dotnet test tests/ErpBridge.CentralApi.Tests --no-build --filter FullyQualifiedName~Catalog: 187 başarılı, 0 başarısız, 0 atlanan.
- node --test tests/katalog-web/: 77 başarılı, 0 başarısız.
- git diff --check: temiz.
- Yeni kontroller: fiyat/stok/marka/koli/görsel filtreleri; sıralama ve sayfalama; geçersiz fiyat aralığı; gizli markaların sızmaması; firma bazlı sipariş görselleri ve geçmiş fiyatların korunması; URL filtre dönüşümü.
- Tarayıcı: 320x740, 390x844, 768x1024, 1440x900 boyutlarında yatay taşma görülmedi. Birleşik filtre, fiyat sıralaması, ürün detayından dönüş, filtre kaldırma ve telefon sipariş detayı kontrol edildi. Bu kontrol tüm cihaz/tarayıcı kombinasyonlarını kapsamaz.
- Derleme ve test çıktıları artifacts/catalog-solution-build.txt, artifacts/catalog-dotnet-tests.txt ve artifacts/catalog-node-tests.txt altında.

Yerel demo: http://localhost:5174/DEMO1234 (demo veriler; sunucu çalışırken erişilebilir).

## Tam genişlik ve sol menü güncellemesi

- Değişen dosyalar: katalog/css/app.css, katalog/js/views/catalog.js ve bu teslim notu.
- Katalog ve üst çubuk 1280 piksel sınırı olmadan ekranı kullanır; yatay kenar boşluğu 16 pikseldir.
- Kategori ve filtreler aynı sol menüde ayrı native details bölümleridir. Masaüstünde tüm menü 260 pikselden 56 piksele daraltılabilir. Telefon/tablette kapalı başlar.
- Mobil kategori seçimi veya filtre uygulama menüyü kapatır ve klavye odağını menü başlığına getirir.
- Yeni doğrulama senaryoları: Enter ile menüyü açma; kategori bölümünü kapatma; stok filtresini uygulama; mobil kategori seçimi sonrası kapalı menü ve odak; tablet filtre bölümünü açma; 320/390/768/1440/1920 piksel genişlikte taşma kontrolü. Tarayıcı konsolunda hata görülmedi.
- 77 mevcut JavaScript testi geçti. dotnet build ErpBridge.sln --no-restore: 0 hata, 0 uyarı. Sunucu kodu değişmedi.
