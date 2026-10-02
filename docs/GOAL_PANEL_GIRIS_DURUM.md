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
| P2c | `PortalCustomerSales` + `returnables` | ✅ | [#251](https://github.com/Retrosero/ErpBridge/pull/251) | `stockTransactions` tip=1 + `cariKod`, müşteri başına katlanan ayna; iptal edilen satış sayılmaz; fiyat seçenekleri en yeni üstte |
| P3a | Satış | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | 14 ilişkisel test |
| P3b | Tahsilat | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | ERP'li tek makbuz; ERP'siz yöntem başına belge, hepsi ya da hiçbiri |
| P3c | Tediye | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | ERP'li havalede `bankCode` da gider |
| P3d | Gider | ✅ | [#250](https://github.com/Retrosero/ErpBridge/pull/250) | ERP'li `expense` + KDV işaretçisi; ERP'siz müşterisiz `disbursement` (yalnız kayıt). 9 ilişkisel test (para belgeleri) |
| P3e | Alış | ✅ | [#251](https://github.com/Retrosero/ErpBridge/pull/251) | `purchaseReceiptPayload`; satır + genel iskonto zinciri (≤ 6, `ErpPurchasePricing`); peşin kapalı fatura (K13); `amount` net, `grossAmount` KDV dahil |
| P3f | İade | ✅ | [#251](https://github.com/Retrosero/ErpBridge/pull/251) | Yalnız satılmış ürün + satıldığı fiyat (400 `NOT_SOLD_TO_CUSTOMER`); ERP'li `ErpReturnDocument`, ERP'siz `salesReturnPayload`. 8 ilişkisel test |
| P4 | `documents/{id}`, son girişlerim | ✅ | [#251](https://github.com/Retrosero/ErpBridge/pull/251) | Durum (bekliyor/yeniden/yazıldı + ERP seri-sıra/hata), giren/sahip, gövde; liste kişinin kendi girişleri, yönetici `all=true` |
| P5a–d | Panel: alanlar/menü, istemci, bileşenler, altı sayfa, yazdırma, son girişler | ✅ | [#252](https://github.com/Retrosero/ErpBridge/pull/252) | `/giris/{satis,tahsilat,alis,iade,tediye,gider}`, `/girisler`, `/giris-yazdir`; `PortalArea.Entry*` → `module.*`; 10 bUnit testi |
| P5e | Evraklar/Cari giriş düğmeleri → yeni sayfalar | ✅ | [#254](https://github.com/Retrosero/ErpBridge/pull/254) | Evraklar'ın Yeni satış/alış/iade'si ve Cari'nin Tahsilat al/Ödeme yap'ı giriş sayfalarına (`?cari=` dolu); eski `evraklar?yeni=` adresi yönlenir. Düzenle/İptal aynen ADMIN'de |
| P6a–f | Panel: Görevler | ✅ | [#253](https://github.com/Retrosero/ErpBridge/pull/253) | `/gorevler`, `/gorevler/seriler`, bildirim zili; sunucu panel oturumunda `module.tasks` (403 `TASKS_MODULE_DENIED`); tarayıcı kontrolünde `add_subtask` hatası bulunup düzeltildi (telefonu da etkiliyordu) |
| P7 | KB + sözleşmeler | ⏳ | | |
| P8 | Uçtan uca doğrulama + kapanış | ⏳ | | |

## Notlar
- **Codex #251:** (P1) firmanın "tedarikçi fiyatı KDV dahil" ayarı açıksa alış fiyatı KDV dahil okunur, `amount` KDV dahil toplamdır
  (Mikro o durumda toplamla karşılaştırır); (P2) giriş sonucu/listesi alışta ödenen KDV dahil tutarı (`grossAmount`) gösterir.
- **Codex #250:** (P2) ERP tahsilatında kart slip / havale dekont numarası makbuzun açıklamasına yazılıyor (önceden düşüyordu).
  (P1, değiştirilmedi) eksi stok sahibin deposuna göre değil firma toplamına göre denetleniyor: Mikro okuyucusu stoğu depo
  bazında göndermiyor (KB 00 kural 18), telefon da toplamı denetliyor; depo bazlı denetim her başka depolu sahibin satışını reddederdi.
- **ERP'siz gider = tediye yetkisi:** telefon ERP'siz firmada gideri müşterisiz `disbursement` olarak gönderir ve ingest onu
  tediye yetkisi/limitiyle denetler; panel de aynısını yapar. Yani ERP'siz firmada panelden gider girmek için `module.expenses`
  yanında `module.disbursement` da açık olmalı (varsayılan rollerde ikisi de açık).
- **Fiyat grubu sunucuda yok:** telefonun ERP carilerinde de müşteri iskontosu 0 ve fiyat ürünün başlık listesinden; panel
  aynısını yapar, ayrıca tüm satırlar için liste seçtirir.

- **`add_subtask` düzeltmesi (P6):** var olan göreve alt görev eklemek `DbUpdateConcurrencyException` ile düşüyordu: istemcinin
  kimliğiyle gelen yeni satır yalnız izlenen görevin koleksiyonuna eklenince EF onu var olan satır sanıp UPDATE deniyordu. Satır
  artık `DbSet` üzerinden eklenir. Telefonun aynı işlemi de bundan etkileniyordu (yeni görevle birlikte gelen alt görevler değil,
  sonradan eklenenler). Test: `PortalTasksAccessRelationalTests.A_subtask_added_to_an_existing_task_is_saved`.

- **Davranış değişikliği (P5e):** ERP'siz firmada panelden **yeni** belge girişi artık yalnız ADMIN'e değil telefonun `module.*`
  yetkisine bağlı (K3, GOAL_PANEL_ERPSIZ D4'ün giriş kısmının yerini alır): varsayılan rollerde yönetici de girer. Düzeltme/iptal,
  kart düzenleme ve sayım D4'teki gibi yalnız ADMIN. ERP'li firmada Cari ve Evraklar da artık giriş düğmesi gösterir (belge ERP'ye yazılır).

## Seni Bekleyenler
- Muhasebe (ACCOUNTING) rolü panelden giriş yapacaksa `/yetkiler`'den ilgili `module.*` anahtarları açılmalı (varsayılan kapalı).
- Sınırsız geçmiş tarih: 7 günden eski tarihli girişler panel günlük raporlarında o güne sayılmaz (ekstre/bakiye doğru) —
  rapor penceresinin genişletilmesi ayrı karar.
