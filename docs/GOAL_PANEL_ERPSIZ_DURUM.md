# ERP'siz panel yönetimi — Durum

`docs/GOAL_PANEL_ERPSIZ.md` planının nerede olduğu. Son güncelleme: 2026-09-22.

## Tamamlananlar

| Faz | İş | PR |
|---|---|---|
| E0a | Plan onaylandı ve genişletildi (kart + tahsilat/tediye + **satış/alış/iade faturası girişi/düzenleme** + hesap hareketi düzeltme + stok sayımı); `eb-erpsiz` worktree açıldı; `CLAUDE.md`'ye istisna eklendi; `GOAL_PANEL_ERPLI.md` durduruldu (öncelik bu goal'e kaydı) | #160 |
| E0b | Zemin dolduruldu: `PortalSession.DataSource` ve `/portal/summary`'nin `DataSource` alanı **zaten** vardı (E0b'nin "native tenant'ı panelde tanıyan bayrak var mı" sorusu → evet, ek iş gerekmedi); kök `.claude/launch.json`'a `eb-erpsiz-centralapi-local` (5481) + `eb-erpsiz-portal-local` (5495) eklendi | #161 |
| E7a | `RolePermissions.CanEditNativeData` (CentralApi, ADMIN-only, D4) + `PortalSession.CanEditNativeData` (ADMIN + `DataSource=native`) eklendi; ikisi de birim testli | #161 |
| E1a+E1b | Sunucu: `POST/GET/DELETE /api/v1/portal/native/stock-cards[/{code}]` — `PortalNativeCardsEndpoints`, `NativeDocumentProcessor`'ı doğrudan çağırıyor (panel oturumu `/ingest/jobs`'a yazamadığı için — `PORTAL_CANNOT_SUBMIT_DOCUMENTS`, koddan doğrulandı). `PortalRecords.CardPart`/`PortalStockCatalog.Product`'a `VatRate` eklendi. Codex incelemesi 4 bulgu çıkardı, üçü düzeltildi (tüm fiyat listeleri döner/eksik liste asla 0 sayılmaz; başka ürünün barkodu 409 `BARCODE_IN_USE` ile reddedilir; istemci `operationId` göndererek tekrar denemeyi idempotent yapabilir), dördüncüsü (düzenlemede barkod/fiyat temizlemek eskisini silmiyor) bilinçli olarak E1c'ye ertelendi — aşağıya not düşüldü. 12 ilişkisel test | #162 |
| E1c | Panel `/stok`: **"Yeni ürün"** (PageHeader) + genişleyen satırda **Düzenle**; `Shared/DetailSheet` çekmecesi (kod, ad, birim, KDV, kategori, marka, reyon, barkod, fiyat, açılış miktarı yalnız yeni üründe); **Sil** iki adımlı onayla. Yalnız `Session.CanEditNativeData` (ADMIN + native) görür. 9 bUnit testi (yeni/düzenle/sil/yetkisiz/ERP tenant/barkod hatası + Codex'in bulduğu iki P1 için regresyon). Yerelde `eb-erpsiz-centralapi-local`+`eb-erpsiz-portal-local` ile gerçek tarayıcıda denendi: form dolduruldu, kaydedildi, "kaydedildi" bildirimi göründü — **ama** bu makinenin `--environment Test` (EF Core **InMemory**) sunucusunda kart hiç listeye/`sync/pull`'a düşmedi. Bunun E1c'nin hatası olmadığı doğrulandı: aynı ortamda telefonun kendi `/ingest/jobs` → `stock_card` yolu da **aynı belirtiyi** veriyor (Job "Succeeded" ama `sync/pull` boş) — KB 01'in zaten belgelediği bilinen kısıt ("InMemory ... transaction desteği yoktur"). Gerçek doğrulama `SqliteCentralApiFactory` ilişkisel testleriyle yapıldı (12/12 E1a/E1b + 9/9 E1c yeşil). Codex incelemesi 3 bulgu çıkardı, ikisi **P1** düzeltildi: (1) düzenlemede değişmeyen barkod artık sunucuya hiç gönderilmiyor — önceden her kayıtta aynı barkodu tekrar göndermek `stock_card`'ın "gönderilmeyen barkodu sil" kuralı yüzünden ürünün fazladan barkodlarını er geç silerdi; (2) fiyat artık yalnız **liste 1**'den okunup yazılıyor — önceden ürünün tek fiyatı liste 2'deyse form onu gösterip liste 1'e yanlış fiyat yazardı. Üçüncüsü (P2, alanı boşaltıp kaydetmek eskisini silmiyor) PR #162'dekiyle aynı gerekçeyle yine ertelendi, ek not: P1 düzeltmesi zaten "istemeden silme" riskini kaldırdı, kalan yalnız "bilerek temizleme" — ayrı bir "temizle" eylemi gerektirir | #163 |

| E2a+E2b | Sunucu: `POST /api/v1/portal/native/customer-cards` (`PortalNativeCustomerCardsEndpoints`). `PortalNativeCardsEndpoints`'in auth/booking mantığı `PortalNativeWriteHelpers`'a çıkarıldı, iki kart uç dosyası artık onu paylaşıyor. Panel: `/cariler`'e **"Yeni müşteri"** (`Shared/DetailSheet`: kod, unvan, telefon, e-posta, vergi dairesi/no, bölge, açılış bakiyesi yalnız yeni müşteride); `/cari`'ye **"Düzenle"** (zaten yüklü `CustomerCardDto`'dan doldurulur — stok'un aksine ayrı bir GET ucu gerekmedi, `PortalLedger.Customer` zaten yazılan her alanı taşıyordu). 5+5 test (sunucu ilişkisel + panel bUnit). **`customer_card_delete` bilinçli olarak yapılmadı**: ürünün aksine cari hareketleri (`customerTransactions`) `MobileRecord.CustomerKey`'i kasıtlı olarak taşımıyor (Mikro'da cari silinse de evrak geçmişi kaybolmasın diye) — "hareketi olan cari silinemez" kontrolü indexli bir sorguyla yapılamıyor; bakiye==0 gibi bir yaklaşıklık gerçek geçmişi olan bir cariyi yanlışlıkla silinebilir kılardı (KB kural 1). Doğru kapsam ayrı bir görev | #164 |
| E3a+E3b | Sunucu: `POST /api/v1/portal/native/collections` ve `.../disbursements` (`PortalNativePaymentsEndpoints`, `PortalNativeWriteHelpers` paylaşımlı); `customerCode` var mı diye ayrıca kontrol edilip 400 döner (booking'in genel 422'sine düşürülmez), tutar ≤ 0 reddedilir. Panel `/cari`'de **"Tahsilat al"/"Ödeme yap"** düğmeleri → tutar/ödeme şekli/tarih/açıklama formu; kaydedince ekstre + bakiye anında yeniden yükleniyor (`LoadAsync`). 7+4 test. **E3c (ayrı `/tahsilatlar` liste sayfası) ve makbuz (yazdırılabilir) görünümü bilinçli olarak bu PR'da yok** — plan bunları ayrı alt-görev olarak zaten ayırmıştı (E3c), tahsilat/tediye girişinin kendisi tamamlandığı için ayrı bir PR'a bırakıldı | #165 |
| E3c | Sunucu: `GET /api/v1/portal/payments` (`PortalLedger.Payments`) — şirket geneli tahsilat/tediye, varsayılan bu ay, `kind`/`customer`/`userId` süzgeci, günlük ve ödeme-şekli ("kasa özeti") toplamları. Kullanıcı, hareketin `id`'sindeki `{externalId}\|{suffix}` önekinden `Jobs.CreatedByUserId`'e bağlanarak bulunuyor (ERP ajanı satırlarında eşleşme yok, kullanıcı boş kalır — beklenen). Yan etki: `NativeDocumentProcessor.PostToCustomerAsync`'e ayrı bir `paymentType` alanı eklendi — önceden serbest `description` doluysa ödeme şekli hiç saklanmıyordu, artık ikisi ayrı alanlar. Panel `/tahsilatlar`: tarih/tür/cari/kullanıcı süzgeci, özet kartları, kasa özeti rozetleri, sayfalı tablo; menüye eklendi (`PortalArea.Ledger`, Cariler/Stok ile aynı yetki). 6 ilişkisel + 4 bUnit test | #166 |

## Sırada

**Sıra düzeltmesi (2026-09-22):** E4'e geçmeden önce plandaki kendi sıralama notu (`E3 → E7b (denetim; E4/E5'ten önce) → E4`) fark edildi — E4'ün storno/iptal mekanizmasından önce E7b'nin denetim kaydı altyapısının kurulmuş olması gerekiyordu. Bu yüzden E4 yerine önce **E7b** (denetim kaydı: `native_audit_log`, yeni migration, her E1–E6 yazması kayıt atar, panelde "Geçmiş" + `/denetim`) yapılacak; E7b tamamlanınca E4 — hesap hareketleri (ekstre) ve düzeltme.

## Seni Bekleyenler

- **E2'de ertelenen: `customer_card_delete`.** Doğru "hareketi var mı" kontrolü için ya `MobileRecord`'a
  cari hareketleri için de bir indeksli anahtar eklemek (şema değişikliği, telefonu da etkiler) ya da
  `customerTransactions` satırlarını `PayloadJson` üzerinden tarayan (indekssiz, büyük kiracılarda
  yavaş) bir kontrol gerekiyor — hangisi tercih edilecek ayrı bir karar.
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
