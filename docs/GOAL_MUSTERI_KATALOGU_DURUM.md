# GOAL_MUSTERI_KATALOGU — Durum

Plan: `GOAL_MUSTERI_KATALOGU.md`. İşaretler: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı · ⛔ tıkandı.
Her görevden sonra, o görevin PR'ı içinde güncellenir.

## Yetkiler (kullanıcı, 2026-10-01)
Dala push, PR, `main`'e birleştirme (Actions kredisi yoksa yerel `dotnet build` 0 uyarı + `dotnet test` yeşilken),
Coolify dağıtımı ve `/health/schema` kontrolü, Sipariş Cepte Play **internal** yüklemesi önceden onaylı.
Asla: `--force` push (ve `--force-with-lease`), CI kırmızıyken (kredi dışı sebeple) birleştirme, `main`'e doğrudan kod
push'u, Play production.

| # | Görev | Durum | PR | Not |
|---|---|---|---|---|
| S1 | ForwardedHeaders, /64, giriş yavaşlatıcı, `OnRejected` | ✅ (yerel, PR bekliyor) | | Personel + Admin girişi; katalog girişi S6'da `CatalogArea` |
| S2 | Tablolar, migration, modül, kilitli yetki, Admin kutucuğu | ✅ (yerel, PR bekliyor) | | Sunucu tarafı (migration `MusteriKatalogu`, `PermissionCatalog.Version` 2); Admin `TenantMobile.razor` kutucuğu panel kolunda |
| S3 | Katalog derleme (görünüm, görünürlük, fiyat) | ✅ (yerel, PR bekliyor) | | `CatalogViewService` (stok aynası + `Revision`), `CatalogVisibility`, `CatalogPricing` (`ErpSalePricingTest` örnekleri) |
| S4 | Yönetim uçları | ✅ (yerel, PR bekliyor) | | `CustomerCatalogManageEndpoints` (settings, categories, products, accounts); hata kodları `docs/api-contracts.md` |
| S5 | Görsel uçları | ✅ (yerel, PR bekliyor) | | `CustomerCatalogImageEndpoints` (manifest, links, kayıt, ham yükleme, sıra, silme, anonim `catalog/img`); `catalog-upload` / `catalog-public` politikaları |
| S6 | Müşteri oturumu | ✅ (yerel, PR bekliyor) | | `CustomerCatalogPublicEndpoints` (info/login/logout/me/password), `__Host-kt_{KOD}` çerezi, `CatalogCustomerPolicy`, cihaz çerezi, `CatalogLoginGate` |
| S7 | Müşteri gezinme + quote | ✅ (yerel, PR bekliyor) | | `CatalogCustomerView`, `CatalogQuote` (categories, products, products/detail, cart/quote) |
| S8 | Talepler + belge bağı + bildirim | ✅ (yerel, PR bekliyor) | | `CatalogCustomerOrderEndpoints` (müşteri), `CustomerCatalogOrderEndpoints` (personel), `CatalogOrderLinker` (ingest + onay), `CATALOG_ORDER_NEW` bildirimi |
| S9 | Ekstre, faturalar, aldıkları | ✅ (yerel, PR bekliyor) | | `CatalogCustomerLedgerEndpoints` (statement, invoices, invoices/detail, purchased) |
| W0 | Web barındırma | ✅ (yerel, PR bekliyor) | | `CatalogWeb` (`/assets/{v}`, kabuk, başlıklar, izin listesi); dosyalar W1+ |
| W1–W5 | Web katalog | ✅ (yerel, PR bekliyor) | | Statik SPA `wwwroot/katalog` (giriş, katalog, sepet, talepler, hesabım); `node --test tests/katalog-web/` |
| P1–P5 | Panel | ✅ (yerel, PR bekliyor) | | `/katalog`, ürün sheet'i görselleri, `CatalogAccessSheet`, `/musteri-siparisleri` (yeniden aç dahil) |
| A1–A7 | Telefon (Siparis_Cepte) | ⬜ | | |
| D1 | Cloudflare + Coolify alan adı (kullanıcı) | ⬜ | | |
| D2 | CentralApi dağıtımı | ⬜ | | |
| D3 | Ortam değişkenleri + gerçek IP doğrulaması | ⬜ | | |
| D4 | Portal + Admin dağıtımı | ⬜ | | |
| D5 | Test firmasında modül + duman testi (kullanıcı) | ⬜ | | |
| D6 | Play internal | ⬜ | | |

## Karar günlüğü
| Tarih | Karar | Gerekçe |
|---|---|---|
| 2026-10-01 | K1–K8, T1–T9 (plan §2) | Kullanıcı plan onayı |
| 2026-10-01 | Kod incelemesi düzeltmeleri: `LoginThrottle.TryBegin` (devam eden denemeler sayılır, 100.000 anahtar sınırı); personel anahtarına istemci adres bölümü + kayıtlı aktif telefon muafiyeti (kendi sayacı); Admin anahtarı e-posta + adres, bilinmeyen e-postada sahte hash; katalog cihaz çerezi `TokenVersion` taşır ve firma kovasından önce doğrulanır; müşteri şifre değişikliği `#hesap` anahtarı; `TokenVersion` artışları atomik; oturumsuz katalog istekleri IP kovasına; güvenilmeyen XFF için tek seferlik uyarı | Bir saldırganın başka adresten personeli, kalabalığın firma bütçesiyle müşteriyi dışarıda bırakması ve paralel tahminle eşiğin aşılması kapandı. Panel/Admin girişi kendi konteyner adresinden geldiği için orada adres ayrımı yok (KB kural 36) |
| 2026-10-01 | Talep bağı: yalnız `sales_order`; başkasının `CLAIMED` talebi `409 CATALOG_ORDER_TAKEN`; işi `Failed`/`DeadLetter` olan satışa bağlı talebi düzeltilmiş satış devralır; iş olmayan referans (elle "başka yerde girildi") kalıcı; `POST orders/{id}/reopen` (yönetici) + panelde "Yeniden aç" | İncelemenin "hiç yok"u da devretmesi önerisi uygulanmadı: iş olmayan referans yalnız elle tamamlamada oluşur, onu devretmek çift siparişe kapı açar; düzeltme yolu reopen |
| 2026-10-01 | Görsel yazımları `ImageRevision`'ı artırır (migration `KatalogGorselSayaci`), düzen revizyonunu değil; PNG/WebP üst verisi de atılır; anonim görsel firma pasifken 404 | Görsel yüklemesi panelin/telefonun açık düzen düzenlemesini 409'a düşürüyordu. `IsEnabled` kapalıyken görsel sunmaya devam: panel ve telefon katalog yayına alınmadan görselleri gösterir |
| 2026-10-01 | Web: `TOO_MANY_OPEN_ORDERS` kendi metni; sipariş kapalı hesapta Siparişlerim görünür (sunucu listeyi `CanOrder`'dan bağımsız verir). Panel: eksik hata kodlarına Türkçe metin, bilinmeyen kodda sunucunun Türkçe mesajı. §4 stok tanımı koda göre düzeltildi (depo başına yuvarla, sonra topla) | İnceleme bulguları C-2, C-4, C-5, S-17 |
