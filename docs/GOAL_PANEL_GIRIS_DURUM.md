# GOAL_PANEL_GIRIS — Durum

Plan: [GOAL_PANEL_GIRIS.md](GOAL_PANEL_GIRIS.md) · Başlangıç: 2026-10-02

| # | Görev | Durum | PR | Not |
|---|---|---|---|---|
| P0 | Plan + DURUM + `CLAUDE.md` istisnası | 🔄 | | Worktree `ErpBridge-panel-giris` (`origin/main` ea784a8) |
| P1a | `DocumentPermissionCheck.Check` | ⏳ | | |
| P1b | `PanelEntryAccess` / `Kinds` / `Dates` | ⏳ | | |
| P1c | Sahip bağlamı + çevirici kuru çalıştırması | ⏳ | | |
| P1d | `PanelEntryWriter` | ⏳ | | |
| P1e | `context` / `customers` / `products` | ⏳ | | |
| P2a | `PanelPricing` + telefon test vektörleri | ⏳ | | |
| P2b | Fiyat listesi kuralı | ⏳ | | |
| P2c | `PortalCustomerSales` + `returnables` | ⏳ | | |
| P3a | Satış | ⏳ | | |
| P3b | Tahsilat | ⏳ | | |
| P3c | Tediye | ⏳ | | |
| P3d | Gider | ⏳ | | |
| P3e | Alış | ⏳ | | |
| P3f | İade | ⏳ | | |
| P4 | `documents/{id}`, son girişlerim | ⏳ | | |
| P5a–e | Panel: alanlar/menü, istemci, bileşenler, sayfalar, yazdırma | ⏳ | | |
| P6a–f | Panel: Görevler | ⏳ | | |
| P7 | KB + sözleşmeler | ⏳ | | |
| P8 | Uçtan uca doğrulama + kapanış | ⏳ | | |

## Seni Bekleyenler
- Muhasebe (ACCOUNTING) rolü panelden giriş yapacaksa `/yetkiler`'den ilgili `module.*` anahtarları açılmalı (varsayılan kapalı).
- Sınırsız geçmiş tarih: 7 günden eski tarihli girişler panel günlük raporlarında o güne sayılmaz (ekstre/bakiye doğru) —
  rapor penceresinin genişletilmesi ayrı karar.
