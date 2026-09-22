# ERP'siz panel yönetimi — Durum

`docs/GOAL_PANEL_ERPSIZ.md` planının nerede olduğu. Son güncelleme: 2026-09-22.

## Tamamlananlar

| Faz | İş | PR |
|---|---|---|
| E0a | Plan onaylandı ve genişletildi (kart + tahsilat/tediye + **satış/alış/iade faturası girişi/düzenleme** + hesap hareketi düzeltme + stok sayımı); `eb-erpsiz` worktree açıldı; `CLAUDE.md`'ye istisna eklendi; `GOAL_PANEL_ERPLI.md` durduruldu (öncelik bu goal'e kaydı) | #160 |
| E0b | Zemin dolduruldu: `PortalSession.DataSource` ve `/portal/summary`'nin `DataSource` alanı **zaten** vardı (E0b'nin "native tenant'ı panelde tanıyan bayrak var mı" sorusu → evet, ek iş gerekmedi); kök `.claude/launch.json`'a `eb-erpsiz-centralapi-local` (5481) + `eb-erpsiz-portal-local` (5495) eklendi | #161 |
| E7a | `RolePermissions.CanEditNativeData` (CentralApi, ADMIN-only, D4) + `PortalSession.CanEditNativeData` (ADMIN + `DataSource=native`) eklendi; ikisi de birim testli | #161 |
| E1a+E1b | Sunucu: `POST/GET/DELETE /api/v1/portal/native/stock-cards[/{code}]` — `PortalNativeCardsEndpoints`, `NativeDocumentProcessor`'ı doğrudan çağırıyor (panel oturumu `/ingest/jobs`'a yazamadığı için — `PORTAL_CANNOT_SUBMIT_DOCUMENTS`, koddan doğrulandı). `PortalRecords.CardPart`/`PortalStockCatalog.Product`'a `VatRate` eklendi. Codex incelemesi 4 bulgu çıkardı, üçü düzeltildi (tüm fiyat listeleri döner/eksik liste asla 0 sayılmaz; başka ürünün barkodu 409 `BARCODE_IN_USE` ile reddedilir; istemci `operationId` göndererek tekrar denemeyi idempotent yapabilir), dördüncüsü (düzenlemede barkod/fiyat temizlemek eskisini silmiyor) bilinçli olarak E1c'ye ertelendi — aşağıya not düşüldü. 12 ilişkisel test | #162 |

## Sırada

E1c — panel `/stok` sayfasına **"Yeni ürün"** + satırda **Düzenle** (Blazor form, `Shared/DetailSheet` çekmecesi).

## Seni Bekleyenler

- **E1c'de tasarlanacak: alan temizleme.** Düzenleme formunda barkod/fiyat boşaltılıp kaydedilirse
  `BookStockCardAsync` eskisini silmiyor (Codex incelemesi #162, P2, bilinçli ertelendi — bu alanı
  temizleyebilecek bir arayüz henüz yok, "temizle" niyetinin tam olarak ne anlama geleceği (barkodu
  sil mi/fiyatı listeden kaldır mı) forma karar verilmeden tahmin edilmemeli). `NativeDocumentProcessor`
  telefonla paylaşıldığı için değişiklik ikisini de etkiler, dikkatli tasarlanmalı.
- E9b: gerçek native firmada canlı gözle kontrol.
- §2 (dönem kilidi, kasa/banka/çek-senet) ayrı karar gerektirir.
- D4 (yalnız ADMIN düzenler) 2026-09-21'de karara bağlandı; kapsam büyüdüğü için kullanıcı isterse gözden
  geçirilebilir (bugün tekrar sorulmadı).
- E0b'nin "yerelde native tohumlu panel" kısmı yalnız launch config'i kapsıyor; gerçek bir native tenant'ı
  admin API ile tohumlamak (`erpbridge-roller-depo-plani` notundaki desen) E1c elle/tarayıcıda denenmek
  istendiğinde yapılacak — otomatik testler `SqliteCentralApiFactory` ile DB'ye ihtiyaç duymuyor.
