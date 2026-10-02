# GOAL_PANEL_GIRIS — Durum

Plan: [GOAL_PANEL_GIRIS.md](GOAL_PANEL_GIRIS.md) · Başlangıç: 2026-10-02

| # | Görev | Durum | PR | Not |
|---|---|---|---|---|
| P0 | Plan + DURUM + `CLAUDE.md` istisnası | ✅ | [#249](https://github.com/Retrosero/ErpBridge/pull/249) | Codex 3 bulgu (operationId gönderim başına, çok belgeli yarış, görevlerde sunucu modül denetimi) belgeye işlendi |
| P1a | `DocumentPermissionCheck.Check` | ✅ | (bu PR) | İhlal yapılandırılmış (`PermissionViolation`); `Refusal()` metni birebir aynı, ingest/katalog değişmedi |
| P1b | `PanelEntryAccess` / `Kinds` / `Dates` | ✅ | (bu PR) | Yalnız `client=portal` (403 `ENTRY_REQUIRES_PORTAL`); geçmiş gün 12:00, ileri gün 400 |
| P1c | Sahip bağlamı + çevirici kuru çalıştırması | ✅ | (bu PR) | `PanelEntryErp`: sahibin kiralama bağlamıyla `MobileDocumentTranslator`; eşleme eksikse 409 `ERP_MAPPING_MISSING` |
| P1d | `PanelEntryWriter` | ✅ | (bu PR) | `SalesJobWriter` üzerinden; çok belge tek işlemde, yarışta kazanan döner; giren kişi `native_audit_log`'a |
| P1e | `context` / `customers` / `products` | ✅ | (bu PR) | Kişi başı hız sınırı; lookup satırına isteğe bağlı `rate` |
| P2a | Fiyatlama + telefon test vektörleri | ✅ (satış) | (bu PR) | Satış `CatalogPricing` (= `ErpSalePricing`); iade/alış hesabı P3e/f ile |
| P2b | Fiyat listesi kuralı | ✅ | (bu PR) | Ürünün başlık listesi (1, yoksa fiyatlı en küçük) ya da formun listesi; belge listesi en çok kullanılan; KDV yoksa %20 (telefon) |
| P2c | `PortalCustomerSales` + `returnables` | ⏳ | | İade ile (P3f) |
| P3a | Satış | ✅ | (bu PR) | 14 ilişkisel test |
| P3b | Tahsilat | ✅ | (bu PR) | ERP'li tek makbuz; ERP'siz yöntem başına belge, hepsi ya da hiçbiri |
| P3c | Tediye | ✅ | (bu PR) | ERP'li havalede `bankCode` da gider |
| P3d | Gider | ✅ | (bu PR) | ERP'li `expense` + KDV işaretçisi; ERP'siz müşterisiz `disbursement` (yalnız kayıt). 9 ilişkisel test (para belgeleri) |
| P3e | Alış | ⏳ | | |
| P3f | İade | ⏳ | | |
| P4 | `documents/{id}`, son girişlerim | ⏳ | | |
| P5a–e | Panel: alanlar/menü, istemci, bileşenler, sayfalar, yazdırma | ⏳ | | |
| P6a–f | Panel: Görevler | ⏳ | | |
| P7 | KB + sözleşmeler | ⏳ | | |
| P8 | Uçtan uca doğrulama + kapanış | ⏳ | | |

## Notlar
- **ERP'siz gider = tediye yetkisi:** telefon ERP'siz firmada gideri müşterisiz `disbursement` olarak gönderir ve ingest onu
  tediye yetkisi/limitiyle denetler; panel de aynısını yapar. Yani ERP'siz firmada panelden gider girmek için `module.expenses`
  yanında `module.disbursement` da açık olmalı (varsayılan rollerde ikisi de açık).
- **Fiyat grubu sunucuda yok:** telefonun ERP carilerinde de müşteri iskontosu 0 ve fiyat ürünün başlık listesinden; panel
  aynısını yapar, ayrıca tüm satırlar için liste seçtirir.

## Seni Bekleyenler
- Muhasebe (ACCOUNTING) rolü panelden giriş yapacaksa `/yetkiler`'den ilgili `module.*` anahtarları açılmalı (varsayılan kapalı).
- Sınırsız geçmiş tarih: 7 günden eski tarihli girişler panel günlük raporlarında o güne sayılmaz (ekstre/bakiye doğru) —
  rapor penceresinin genişletilmesi ayrı karar.
