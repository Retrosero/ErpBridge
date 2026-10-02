# GOAL_PANEL_GIRIS — Durum

Plan: [GOAL_PANEL_GIRIS.md](GOAL_PANEL_GIRIS.md) · Başlangıç: 2026-10-02

| # | Görev | Durum | PR | Not |
|---|---|---|---|---|
| P0 | Plan + DURUM + `CLAUDE.md` istisnası | ✅ | [#249](https://github.com/Retrosero/ErpBridge/pull/249) | Codex 3 bulgu (operationId gönderim başına, çok belgeli yarış, görevlerde sunucu modül denetimi) belgeye işlendi |
| P1a | `DocumentPermissionCheck.Check` | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | İhlal yapılandırılmış (`PermissionViolation`); `Refusal()` metni birebir aynı, ingest/katalog değişmedi |
| P1b | `PanelEntryAccess` / `Kinds` / `Dates` | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | Yalnız `client=portal` (403 `ENTRY_REQUIRES_PORTAL`); geçmiş gün 12:00, ileri gün 400 |
| P1c | Sahip bağlamı + çevirici kuru çalıştırması | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | `PanelEntryErp`: sahibin kiralama bağlamıyla `MobileDocumentTranslator`; eşleme eksikse 409 `ERP_MAPPING_MISSING` |
| P1d | `PanelEntryWriter` | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | `SalesJobWriter` üzerinden; çok belge tek işlemde, yarışta kazanan döner; giren kişi `native_audit_log`'a |
| P1e | `context` / `customers` / `products` | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | Kişi başı hız sınırı; lookup satırına isteğe bağlı `rate` |
| P2a | Fiyatlama + telefon test vektörleri | ✅ (satış) | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | Satış `CatalogPricing` (= `ErpSalePricing`); iade/alış hesabı P3e/f ile |
| P2b | Fiyat listesi kuralı | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | Ürünün başlık listesi (1, yoksa fiyatlı en küçük) ya da formun listesi; belge listesi en çok kullanılan; KDV yoksa %20 (telefon) |
| P2c | `PortalCustomerSales` + `returnables` | ✅ | (bu PR) | `stockTransactions` tip=1 + `cariKod`, müşteri başına katlanan ayna; iptal edilen satış sayılmaz; fiyat seçenekleri en yeni üstte |
| P3a | Satış | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | 14 ilişkisel test |
| P3b | Tahsilat | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | ERP'li tek makbuz; ERP'siz yöntem başına belge, hepsi ya da hiçbiri |
| P3c | Tediye | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | ERP'li havalede `bankCode` da gider |
| P3d | Gider | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | ERP'li `expense` + KDV işaretçisi; ERP'siz müşterisiz `disbursement` (yalnız kayıt). 9 ilişkisel test (para belgeleri) |
| P3e | Alış | ✅ | (bu PR) | `purchaseReceiptPayload`; satır + genel iskonto zinciri (≤ 6, `ErpPurchasePricing`); peşin kapalı fatura (K13); `amount` net, `grossAmount` KDV dahil |
| P3f | İade | ✅ | (bu PR) | Yalnız satılmış ürün + satıldığı fiyat (400 `NOT_SOLD_TO_CUSTOMER`); ERP'li `ErpReturnDocument`, ERP'siz `salesReturnPayload`. 8 ilişkisel test |
| P4 | `documents/{id}`, son girişlerim | ✅ | (bu PR) | Durum (bekliyor/yeniden/yazıldı + ERP seri-sıra/hata), giren/sahip, gövde; liste kişinin kendi girişleri, yönetici `all=true` |
| P5a–e | Panel: alanlar/menü, istemci, bileşenler, sayfalar, yazdırma | ⏳ | | |
| P6a–f | Panel: Görevler | ⏳ | | |
| P7 | KB + sözleşmeler | ⏳ | | |
| P8 | Uçtan uca doğrulama + kapanış | ⏳ | | |

## Notlar
- **Codex #250:** (P2) ERP tahsilatında kart slip / havale dekont numarası makbuzun açıklamasına yazılıyor (önceden düşüyordu).
  (P1, değiştirilmedi) eksi stok sahibin deposuna göre değil firma toplamına göre denetleniyor: Mikro okuyucusu stoğu depo
  bazında göndermiyor (KB 00 kural 18), telefon da toplamı denetliyor; depo bazlı denetim her başka depolu sahibin satışını reddederdi.
- **ERP'siz gider = tediye yetkisi:** telefon ERP'siz firmada gideri müşterisiz `disbursement` olarak gönderir ve ingest onu
  tediye yetkisi/limitiyle denetler; panel de aynısını yapar. Yani ERP'siz firmada panelden gider girmek için `module.expenses`
  yanında `module.disbursement` da açık olmalı (varsayılan rollerde ikisi de açık).
- **Fiyat grubu sunucuda yok:** telefonun ERP carilerinde de müşteri iskontosu 0 ve fiyat ürünün başlık listesinden; panel
  aynısını yapar, ayrıca tüm satırlar için liste seçtirir.

## Seni Bekleyenler
- Muhasebe (ACCOUNTING) rolü panelden giriş yapacaksa `/yetkiler`'den ilgili `module.*` anahtarları açılmalı (varsayılan kapalı).
- Sınırsız geçmiş tarih: 7 günden eski tarihli girişler panel günlük raporlarında o güne sayılmaz (ekstre/bakiye doğru) —
  rapor penceresinin genişletilmesi ayrı karar.
