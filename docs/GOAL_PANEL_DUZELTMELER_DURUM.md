# Durum — Panel Düzeltmeleri

Goal: [GOAL_PANEL_DUZELTMELER.md](GOAL_PANEL_DUZELTMELER.md) · Başlangıç: 2026-09-24

| # | Görev | Durum | PR | Not |
|---|---|---|---|---|
| G1 | Çıkış hatası | ✅ | [#179](https://github.com/Retrosero/ErpBridge/pull/179) | Kök neden: MudBlazor 9 özel etkinleştirici menüyü kendisi açmalı; açmadığı için "Çıkış yap" hiç görünmüyordu. Etkinleştirici artık gerçek `<button>` (Codex: klavye erişimi). Yerel tarayıcıda "Beni hatırla" ile giriş → çıkış → `/cariler` login'e dönüyor, iki depo da boş; odak + Enter menüyü açıyor. 2 bUnit testi (düzeltmesiz kırılıyor) |
| G2 | Onaylar: "Tekrar onaya al" | ✅ | [#180](https://github.com/Retrosero/ErpBridge/pull/180) | Sunucudaki mevcut reopen ucu kullanıldı (telefonla aynı; sunucu/telefon değişmedi). Düğme reddedilen kartta + detay çekmecesinde, yalnız o türe karar verebilene (Codex: muhasebe kart/sayım talebine karar veremez). 4 bUnit testi. Yerel tarayıcıda denenmedi: bellek içi DB'de onay talebi oluşmuyor (GOAL_PANEL_DURUM P1c notu); sunucu davranışı `ApprovalCentreRelationalTests` ile kapsanıyor |
| G3 | Panel bakiyesi = Sipariş Cepte formülü (ERP'li) | 🔄 | [#181](https://github.com/Retrosero/ErpBridge/pull/181) | |
| G4 | Mikro okuyucusu: hareketi değişen carinin bakiyesi | 🔄 | [#182](https://github.com/Retrosero/ErpBridge/pull/182) | Canlı test DEMO'da (eski sorguyla kırılıyor) |
| G5 | Stok sayfası hızı | 🔄 | [#183](https://github.com/Retrosero/ErpBridge/pull/183) | |

## Seni Bekleyenler
- (henüz yok)
