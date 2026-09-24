# Durum — Panel Düzeltmeleri

Goal: [GOAL_PANEL_DUZELTMELER.md](GOAL_PANEL_DUZELTMELER.md) · Başlangıç: 2026-09-24

| # | Görev | Durum | PR | Not |
|---|---|---|---|---|
| G1 | Çıkış hatası | ✅ | [#179](https://github.com/Retrosero/ErpBridge/pull/179) | Kök neden: MudBlazor 9 özel etkinleştirici menüyü kendisi açmalı; açmadığı için "Çıkış yap" hiç görünmüyordu. Etkinleştirici artık gerçek `<button>` (Codex: klavye erişimi). Yerel tarayıcıda "Beni hatırla" ile giriş → çıkış → `/cariler` login'e dönüyor, iki depo da boş; odak + Enter menüyü açıyor. 2 bUnit testi (düzeltmesiz kırılıyor) |
| G2 | Onaylar: "Tekrar onaya al" | ✅ | [#180](https://github.com/Retrosero/ErpBridge/pull/180) | Sunucudaki mevcut reopen ucu kullanıldı (telefonla aynı; sunucu/telefon değişmedi). Düğme reddedilen kartta + detay çekmecesinde, yalnız o türe karar verebilene (Codex: muhasebe kart/sayım talebine karar veremez). 4 bUnit testi. Yerel tarayıcıda denenmedi: bellek içi DB'de onay talebi oluşmuyor (GOAL_PANEL_DURUM P1c notu); sunucu davranışı `ApprovalCentreRelationalTests` ile kapsanıyor |
| G3 | Panel bakiyesi = Sipariş Cepte formülü (ERP'li) | ✅ | [#181](https://github.com/Retrosero/ErpBridge/pull/181) | ERP'li firmada liste/kart/ekstre/Onay masası bakiyesi = cari hareketlerinin toplamı (borç +, alacak −), kasa/banka tarafı hariç (`kapali`, `cariCins≠0`, eski ajanda ciro kodlu satır). Codex: hareketi kalmayan cari 0 (kart bakiyesi yalnız hiç hareket gelmeyen firmada). ERP'siz firmada değişiklik yok. KB 03 güncellendi |
| G4 | Mikro okuyucusu: hareketi değişen carinin bakiyesi | ✅ | [#182](https://github.com/Retrosero/ErpBridge/pull/182) | Artımlı cari okuması kartı değişenler ∪ cari hareketi (cari tarafı) filigrandan yeni olanlar; bakiye tüm hareketlerden. Hareket satırı `cariCins` taşıyor. Codex: `SnapshotProjectionVersion` 6→7, güncellenen ajan bir kez tam okur (eski bayat bakiyeler onarılır). Canlı test yalnız `MikroDB_V15_DEMO`'ya yazdı, değer geri kondu; eski sorguyla kırılıyor. V16_03 salt-okunur geçti |
| G5 | Stok sayfası hızı | 🔄 | [#183](https://github.com/Retrosero/ErpBridge/pull/183) | |

## Seni Bekleyenler
- **Ajan güncellemesi (G4):** Mikro'lu müşteri makinelerinde ajan yeni sürüme güncellenmeli; güncellemeden sonra ajan bir kez tam okuma yapar (projeksiyon 7) ve bayat kart bakiyeleri onarılır. Panel bakiyesi (G3) ajan güncellenmeden de doğru.
