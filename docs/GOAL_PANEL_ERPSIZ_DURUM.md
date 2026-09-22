# ERP'siz panel yönetimi — Durum

`docs/GOAL_PANEL_ERPSIZ.md` planının nerede olduğu. Son güncelleme: 2026-09-22.

## Tamamlananlar

| Faz | İş | PR |
|---|---|---|
| E0a | Plan onaylandı ve genişletildi (kart + tahsilat/tediye + **satış/alış/iade faturası girişi/düzenleme** + hesap hareketi düzeltme + stok sayımı); `eb-erpsiz` worktree açıldı; `CLAUDE.md`'ye istisna eklendi; `GOAL_PANEL_ERPLI.md` durduruldu (öncelik bu goal'e kaydı) | #160 |
| E0b | Zemin dolduruldu: `PortalSession.DataSource` ve `/portal/summary`'nin `DataSource` alanı **zaten** vardı (E0b'nin "native tenant'ı panelde tanıyan bayrak var mı" sorusu → evet, ek iş gerekmedi); kök `.claude/launch.json`'a `eb-erpsiz-centralapi-local` (5481) + `eb-erpsiz-portal-local` (5495) eklendi | #161 |
| E7a | `RolePermissions.CanEditNativeData` (CentralApi, ADMIN-only, D4) + `PortalSession.CanEditNativeData` (ADMIN + `DataSource=native`) eklendi; ikisi de birim testli | #161 |
| E1a+E1b | Sunucu: `POST/GET/DELETE /api/v1/portal/native/stock-cards[/{code}]` — `PortalNativeCardsEndpoints`, `NativeDocumentProcessor`'ı doğrudan çağırıyor (panel oturumu `/ingest/jobs`'a yazamadığı için — `PORTAL_CANNOT_SUBMIT_DOCUMENTS`, koddan doğrulandı). `PortalRecords.CardPart`/`PortalStockCatalog.Product`'a `VatRate` eklendi. Codex incelemesi 4 bulgu çıkardı, üçü düzeltildi (tüm fiyat listeleri döner/eksik liste asla 0 sayılmaz; başka ürünün barkodu 409 `BARCODE_IN_USE` ile reddedilir; istemci `operationId` göndererek tekrar denemeyi idempotent yapabilir), dördüncüsü (düzenlemede barkod/fiyat temizlemek eskisini silmiyor) bilinçli olarak E1c'ye ertelendi — aşağıya not düşüldü. 12 ilişkisel test | #162 |
| E1c | Panel `/stok`: **"Yeni ürün"** (PageHeader) + genişleyen satırda **Düzenle**; `Shared/DetailSheet` çekmecesi (kod, ad, birim, KDV, kategori, marka, reyon, barkod, fiyat, açılış miktarı yalnız yeni üründe); **Sil** iki adımlı onayla. Yalnız `Session.CanEditNativeData` (ADMIN + native) görür. 6 yeni bUnit testi (yeni/düzenle/sil/yetkisiz/ERP tenant/barkod hatası). Yerelde `eb-erpsiz-centralapi-local`+`eb-erpsiz-portal-local` ile gerçek tarayıcıda denendi: form dolduruldu, kaydedildi, "kaydedildi" bildirimi göründü — **ama** bu makinenin `--environment Test` (EF Core **InMemory**) sunucusunda kart hiç listeye/`sync/pull`'a düşmedi. Bunun E1c'nin hatası olmadığı doğrulandı: aynı ortamda telefonun kendi `/ingest/jobs` → `stock_card` yolu da **aynı belirtiyi** veriyor (Job "Succeeded" ama `sync/pull` boş) — KB 01'in zaten belgelediği bilinen kısıt ("InMemory ... transaction desteği yoktur"). Gerçek doğrulama `SqliteCentralApiFactory` ilişkisel testleriyle yapıldı (12/12 E1a/E1b + 6/6 E1c yeşil) | (bu PR) |

## Sırada

E2 — cari (müşteri) kartı yönetimi, E1 ile simetrik (`POST/GET/DELETE /portal/native/customer-cards`, `/cariler` + `/cari` sayfalarına Yeni/Düzenle).

## Seni Bekleyenler

- **Yerel `--environment Test` (InMemory) sunucusu native belge akışını doğrulamaya uygun değil**
  (E1c'de keşfedildi, ErpBridge'e özgü değil): kart/satış/tahsilat gibi `MobileRecordProjector`
  kullanan her native belge "Succeeded" görünür ama `sync/pull`/Portal listeleri hiç güncellenmez.
  Gerçek uçtan uca tarayıcı testi (E9b) için ya gerçek PostgreSQL'e bağlı bir yerel CentralApi ya da
  canlı sunucu gerekir; `eb-erpsiz-*-local` launch config'leri yalnız "sayfa açılıyor mu, form
  doğru mu" düzeyinde işe yarıyor.

- **E1c'de tasarlanacak: alan temizleme.** Düzenleme formunda barkod/fiyat boşaltılıp kaydedilirse
  `BookStockCardAsync` eskisini silmiyor (Codex incelemesi #162, P2, bilinçli ertelendi — bu alanı
  temizleyebilecek bir arayüz henüz yok, "temizle" niyetinin tam olarak ne anlama geleceği (barkodu
  sil mi/fiyatı listeden kaldır mı) forma karar verilmeden tahmin edilmemeli). `NativeDocumentProcessor`
  telefonla paylaşıldığı için değişiklik ikisini de etkiler, dikkatli tasarlanmalı.
- E9b: gerçek native firmada canlı gözle kontrol.
- §2 (dönem kilidi, kasa/banka/çek-senet) ayrı karar gerektirir.
- D4 (yalnız ADMIN düzenler) 2026-09-21'de karara bağlandı; kapsam büyüdüğü için kullanıcı isterse gözden
  geçirilebilir (bugün tekrar sorulmadı).
- **Yerelde native firma tohumlama tarifi (E1c'de kullanıldı, bir sonraki E2/E3/E4/E5 için de geçerli):**
  `eb-erpsiz-centralapi-local` + `eb-erpsiz-portal-local` başlat → `POST /api/v1/admin/login`
  (`Admin:SeedEmail`/`Password`) → `POST /api/v1/admin/tenants` → `PUT .../mobile/data-source`
  (`native`) → `PUT .../mobile/subscription` → `POST .../mobile/users` (ADMIN) → `GET .../mobile`
  (`tenantCode` döner) → panelde bu kodla giriş. Bulguyu koddan doğrulamak (yukarıdaki InMemory notu)
  için: aynı admin token ile `PUT /api/v1/android/approvals/rules` (hepsi `false`) sonra
  `/api/v1/ingest/jobs`'a doğrudan `stock_card` gönder, `/api/v1/android/sync/pull` ile karşılaştır.
