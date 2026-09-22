# Goal — ERP'siz Firmalar İçin Yönetim Paneli (Ürün · Cari · Tahsilat · Satış/Alış/İade · Hesap Hareketleri)

Tarih: 2026-09-21 (kapsam genişletildi: 2026-09-22) · Kapsam: ErpBridge.CentralApi + ErpBridge.Portal
(Sipariş Cepte'ye dokunulmaz, bkz. E0'daki kontrol)

Bu belge otonom çalışma içindir (`/goal`). İlerleme **GOAL_PANEL_ERPSIZ_DURUM.md**'ye yazılır (E0'da açılır),
her görevden sonra güncellenir. Çalışma kuralları, doğrulama ve yetkiler **[GOAL_PANEL_GELISTIRMELERI.md](GOAL_PANEL_GELISTIRMELERI.md)
bölüm 1–3 ile aynıdır** (worktree, PR akışı, Codex bulguları, `TreatWarningsAsErrors`, yalnızca-ekleme kuralı,
`origin/main` alıp merge öncesi yerel build). Yalnız fark: yeni worktree **`2026/eb-erpsiz`**.

**Kardeş plan / öncelik notu (2026-09-22):** `docs/GOAL_PANEL_ERPLI.md` aynı sorunu ERP'li (Mikro) firmalar
için ele alıyordu (E0a `main`'e merge edildi, PR #159). Kullanıcı kararıyla **öncelik bu belgeye (ERP'siz)
kaydı** — "erpli müşterilerin evraklarını değil erpsiz müşteriler ... panelden ürün/müşteri girişi ...
satış/tahsilat/alış/iade/tediye evrakları ekleyip düzenleyebilmeli". `GOAL_PANEL_ERPLI.md` E1'de bulduğu
gerçek (Mikro writer'ları INSERT-only, canlı test DB'si bu makinede yok) geçerliliğini koruyor ama o goal
şimdilik **durduruldu** (bkz. `GOAL_PANEL_ERPLI_DURUM.md`); iki plan hâlâ bağımsız, biri `NativeDocumentProcessor`'a
biri ajan+Mikro writer'lara yazıyor.

---

## 0. Amaç ve mevcut durum

**Amaç:** ERP kullanmayan (`tenants.DataSource = 'native'`) firmanın yöneticisi/muhasebecisi, telefona ihtiyaç duymadan
**panelden** ürün ve müşteri kartı açıp düzenleyebilsin, tahsilat/tediye girebilsin, **satış/alış/iade faturası
girip düzenleyebilsin**, cari hesap hareketlerini görebilsin ve düzeltebilsin — ERP'nin muhasebe ekranlarının
yaptığı işin yeterli kısmı.

### Doğrulanmış zemin (kod + KB, 2026-09-21)
- **Yazma motoru hazır:** `Native/NativeDocumentProcessor` (Faz 33–40) `stock_card` (yeni + düzenle), `stock_card_delete`,
  `customer_card`, `stock_card_batch`/`customer_card_batch` (toplu), `sales_order`, `collection`, `disbursement`,
  `sales_return`, `purchase_receipt`, `stock_count` belgelerini tek transaction + tenant kilidiyle işliyor.
  Bakiye/stok sayıları tipli tablolarda (`native_customer_balances`, `native_stock_levels`), `mobile_records` yalnız kopya.
  **Önemli:** `sales_order`/`purchase_receipt`/`sales_return` yazıcıları zaten var ve telefon bunları kullanıyor —
  panelden eksik olan yalnız bu belgelere giden bir **form + uç**, yeni bir yazma motoru değil.
- **Panel şu an salt okunur** (stok, cariler + ekstre + fatura kalemleri, onaylar, depo). Portal yazma yalnız onay kararı,
  depo ayarı, kullanıcı ve ekran işlerinde var. **Panelden belge girişi yok.**
- Portal girişi telefon kullanıcısı JWT'siyle yapılıyor (`android/account/login`) → `/ingest/jobs` çağrılabilir; ayrı
  kimlik sistemi gerekmez.
- **Boşluklar (bu goal'ün işi):** panelde kart/tahsilat/satış-alış-iade formları yok; `customer_card_delete` yok;
  **hareket/evrak düzeltme-iptal yok** (defter ekleme-yalnız); manuel bakiye düzeltme yok; panelden yapılan işin
  kim tarafından yapıldığı denetim kaydı yok; ERP'li firma paneli bu ekranları görmemeli.

### Kararlar (goal'ün kendi seçimleri — itiraz edilirse değişir)
| # | Karar | Gerekçe |
|---|---|---|
| D1 | **Tek yazma yolu `NativeDocumentProcessor`.** Panel yeni belge türünü değil, mevcut `/ingest/jobs` semantiğini kullanır (idempotent `externalId`, tenant kilidi, `jobs` kaydı). Sunucuda yalnız gerçekten eksik türler eklenir | Kural 15: iş kayıtsız etki, etki kayıtsız iş olamaz; telefon ve panel aynı sonucu üretir |
| D2 | **Hareket "düzenleme" = iptal + yeniden kayıt (storno).** Defterden satır silinmez/üzerine yazılmaz; orijinal `Voided` işaretlenir, ters etki yazılır, gerekirse düzeltilmiş yeni hareket girilir; hepsi tek transaction. Ekstrede iptal edilen satır üstü çizili görünür | Muhasebe izi bozulmaz; `native_*` sayıları tutarlı kalır; telefondaki kopya tombstone ile düşer |
| D3 | Yalnız `DataSource=native` firmada yazma ekranları açık. ERP'li firmada aynı menüler **salt okunur** ve "ERP'den yönetilir" notu gösterir (sunucu zaten 409 `*_REQUIRES_NATIVE_TENANT` döner) | Kaynak karışması engellenir |
| D4 | **ERP'siz firmada panel yalnız ADMIN rolüne açık; ADMIN her şeyi düzenler** (kart, tahsilat, satış/alış/iade, hareket düzenleme/iptal, silme, sayım). Yeni `RolePermissions.CanEditNativeData` = yalnız ADMIN; her uçta sunucuda denetlenir (arayüz gizlemesi güvence değil). Rol bazlı ince ayrım yok | Kullanıcı kararı (2026-09-21): tek yetkili kişi admin |
| D10 | Düzenlemede **son kaydeden kazanır** (sürüm çakışma kontrolü yok). İki korunma kalır: bir hareket/evrak ikinci kez iptal edilemez (409), hareketi olan kart silinemez | Kart alanları para/stok taşımaz; bakiye hareketlerden hesaplanır |
| D5 | Panelden yapılan her yazma **denetim kaydı** alır: kim, ne zaman, hangi belge/kart, önce–sonra özeti. Ekranda "Geçmiş" olarak görünür | ERP'siz firmada defter merkezde; izlenebilirlik şart |
| D6 | Hareket girişinde **kilitli dönem yok** (v1). Geçmiş tarihli hareket girilebilir, bakiye/ekstre yeniden hesaplanır | Basit başla; dönem kilidi ayrı karar (Seni Bekleyenler) |
| D7 | **Satış/alış/iade faturası girişi VE düzenlemesi bu goal'ün kapsamındadır** (artık ertelenmiyor). Panel kart + tahsilat/tediye + **satış/alış/iade** + hesap hareketi düzeltme + stok sayımının **hepsini** kapsar | **Kullanıcı kararı (2026-09-22):** "erpsiz müşteriler ... yeni satış tahsilat alış iade tediye evrakları ekleyebilmeliyim aynı şekilde düzenleyebilmeliyim" — önceki D7 (fatura ertelensin) bu kararla geçersiz |
| D11 | **Evrak (satış/alış/iade) "düzenleme"si de storno'dur (D2'nin evrak karşılığı).** `document_void`: orijinal evrak `Voided`, satırların **hem stok hem cari** etkisi tek transaction'da ters çevrilir; düzenlemede aynı transaction içinde düzeltilmiş yeni evrak girilir (`document_void` + yeniden kayıt, E4c'deki ledger-edit deseninin satırlı hali) | Mikro/ERP mantığında da evrak fiziksel değişmez; D2 ile tutarlı, muhasebe izi korunur |
| D8 | Liste durumu URL'de (önceki goal D8), formlar sağdan çekmecede (`Shared/DetailSheet`), mevcut `Pager`/`PageHeader` bileşenleri yeniden kullanılır | Panel tutarlılığı |
| D9 | Mevcut uç ve telefon sözleşmeleri bozulmaz; yalnızca ekleme. Telefonun kullandığı yeni belge/alan yoksa Android'e dokunulmaz | Yayınlanmış sürümler çalışmaya devam eder |

---

## 1. Görevler

**[O]** otonom · **[K]** insan kapısı · **[O→K]** yapılabilen kısmı otonom.

### E0 — Hazırlık
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E0a** | Bu plan dalını push et, PR, CI yeşilse merge; `eb-erpsiz` worktree; `GOAL_PANEL_ERPSIZ_DURUM.md` | [O] | Plan `main`'de |
| **E0b** | Zemin: KB 00/01/03 oku; `RolePermissions` ve Portal oturumundaki yetki bayrakları haritası; native tenant'ı panelde tanıyan bayrak (`Session.IsNative` gibi) var mı — yoksa `/portal/summary` yanıtına **isteğe bağlı** `dataSource` alanı; yerelde native tohumlu panel açılır (launch.json'a `eb-erpsiz-*` yapılandırmaları) | [O] | DURUM'da zemin + yerelde native firma paneli |

### E1 — Ürün kartı yönetimi
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E1a** | Sunucu: `POST /portal/native/stock-cards` (yeni/düzenle) ve `DELETE …` — `stock_card`/`stock_card_delete` belgelerini `NativeDocumentProcessor`'a sarar (D1); doğrulama mesajları Türkçe alan bazlı (kod boş, birim, KDV, barkod tekrarı). Hareket görmüş ürün silinemez kuralı korunur, mesaj net | [O] | İlişkisel test: yeni, düzenle (kod değişmez), barkod değişimi eski barkodu düşürüyor, hareketli ürün silinmiyor, ERP firmada 409, yetkisiz 403 |
| **E1b** | Sunucu: tek ürün ucu `GET …/stock-cards/{code}` (kart + barkodlar + fiyat listeleri + stok miktarı + son hareket) — düzenleme formunu doldurur | [O] | Test |
| **E1c** | Panel `/stok`: **"Yeni ürün"** düğmesi + satırda **Düzenle/Sil**; çekmece formu: kod, ad, birim(ler), KDV, ana/alt grup, marka, reyon, barkodlar (çoklu), fiyat listeleri, açılış miktarı (yalnız yeni ürün), kritik stok eşiği. Kaydet → liste anında yenilenir; hata alan yanında | [O] | bUnit: form doğrulama, doğru istek; tarayıcıda yeni ürün → listede + `stock/search`'te görünüyor |
| **E1d** | Panel: **Excel/CSV toplu ürün içe aktarma** (`stock_card_batch`): şablon indir, yükle, önizleme (satır bazlı hata), onayla. Üst sınır sunucuda | [O] | Test: hatalı satırlar tüm işi bozmadan raporlanıyor (mevcut batch semantiği doğrulanır); tarayıcıda 100 satır |

### E2 — Müşteri (cari) kartı yönetimi
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E2a** | Sunucu: `POST /portal/native/customer-cards` (yeni/düzenle, `customer_card` sarmalı), `GET …/{code}`; **`customer_card_delete`** (yeni tür, yalnız ADMIN): **hareketi olan cari silinemez** (ürün kuralının aynısı), silinebilirse `mobile_records` tombstone + `native_customer_balances` satırı; cihazlara silinmiş gider | [O] | İlişkisel test: yeni, düzenle, açılış bakiyesi yalnız ilkte, hareketli cari silinmiyor, sunucu/telefon senkronu tombstone |
| **E2b** | Panel `/cariler`: **"Yeni müşteri"** + `/cari` sayfasında **Düzenle/Sil**; alanlar KB 03'teki kart şemasından doğrulanır (unvan, kod, vergi no/dairesi, telefon, adres, plasiyer, risk limiti varsa, açılış bakiyesi). Kod otomatik öneri (boşsa sıradaki) | [O] | bUnit + tarayıcıda yeni cari → listede, ekstre açılışı |
| **E2c** | Panel: **Excel/CSV toplu cari içe aktarma** (`customer_card_batch`), E1d ile aynı bileşen | [O] | Test + tarayıcı |

### E3 — Tahsilat / Tediye girişi
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E3a** | Sunucu: `POST /portal/native/collections` ve `…/disbursements` — mevcut `collection`/`disbursement` belgelerini sarar; alanlar: cari, tutar, ödeme şekli (Nakit/Kredi Kartı/EFT-Havale/Çek…), tarih, açıklama; idempotent `externalId` (panel kaydı çift tıklamada iki kez oluşmaz); tutar > 0 | [O] | Test: bakiye doğru yönde değişiyor, çift gönderim tek kayıt, geçmiş tarih ekstrede yerinde |
| **E3b** | Panel: **Tahsilat / Tediye** — `/cari` sayfasından ve üst menüden "Tahsilat al / Ödeme yap" çekmecesi (cari aramalı seçim, tutar, ödeme şekli, tarih, açıklama); kaydedince ekstre + bakiye anında güncellenir; **makbuz görünümü** (yazdırılabilir sayfa) | [O] | bUnit + tarayıcı: tahsilat → bakiye düştü, ekstrede satır |
| **E3c** | Panel `/tahsilatlar`: tarih aralıklı **tahsilat/tediye listesi** (cari, ödeme şekli, kullanıcı filtresi, günlük toplamlar, kasa özeti: ödeme şekline göre toplam) | [O] | bUnit + tarayıcı |

### E4 — Hesap hareketleri (ekstre) ve düzeltme
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E4a** | Sunucu (D2): yeni belge türü **`ledger_void`** — bir cari hareketini (**yalnız** tahsilat/tediye/manuel düzeltme; satış/alış/iade faturasının cari etkisi E5'teki `document_void`'e bağlı) iptal eder: orijinal `Voided`, ters hareket, `native_customer_balances` düzeltilir, `mobile_records` güncellenir/tombstone (cihazlar `sync/pull` ile alır), tek transaction, `jobs` kaydı, hub.Publish. Zaten iptal edilmiş → 409; yetki ADMIN | [O] | İlişkisel test: iptalden sonra bakiye = önceki, ekstre toplamı = kart bakiyesi (mevcut P3b değişmezi), çift iptal 409, telefon senkronu |
| **E4b** | Sunucu: yeni belge türü **`ledger_adjustment`** (manuel düzeltme fişi: cari, borç/alacak, tutar, gerekçe **zorunlu**) — yalnız ADMIN; gerekçe ekstrede görünür | [O] | Test |
| **E4c** | Sunucu: hareket **düzenleme** ucu `POST …/ledger/{key}/edit` = `ledger_void` + düzeltilmiş yeni hareket **tek transaction** (yarım kalmaz: yeni hareket geçersizse iptal de geri alınır) | [O] | Test: tutar/tarih/ödeme şekli düzeltmesi, hata durumunda hiçbir şey değişmiyor |
| **E4d** | Sunucu: ekstre ucuna (P3b) **isteğe bağlı** `includeVoided` ve satırda `voided`, `voidedBy`, `voidedAt`, `reason`, `editable` alanları; parametresiz çağrı bugünkü sözleşmeyle aynı | [O] | Sözleşme testi |
| **E4e** | Panel `/cari` ekstresi: satır menüsü **Düzenle · İptal et · Geçmiş**; "Manuel düzeltme" düğmesi; iptal gerekçesi zorunlu diyalog; iptal edilen satır üstü çizili + "İptal edilenleri göster" anahtarı; **satış/alış/iade kaynaklı hareket satırı tıklanınca ilgili evrağa gider** (E5e ile aynı iptal/düzenle akışı orada). **Tüm cariler için hareket listesi** `/hareketler` (tarih aralığı, tür, cari, kullanıcı, tutar aralığı, sayfalı) | [O] | bUnit + tarayıcıda: tahsilat gir → düzenle → iptal et → ekstre + bakiye doğru |

### E5 — Satış / Alış / İade faturası girişi ve düzenleme
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E5a** | Sunucu: `POST /portal/native/sales-orders`, `…/purchase-receipts`, `…/sales-returns` — mevcut `sales_order`/`purchase_receipt`/`sales_return` belgelerini `NativeDocumentProcessor`'a sarar (D1); satırlar (ürün, miktar, birim fiyat, iskonto), ödeme şekli (anında ödeme ise Faz 33 kuralı gereği aynı transaction'da tahsilat/tediye de yazılır, açık bakiye değişmez); idempotent `externalId`; satır doğrulaması Faz 33 ile aynı (ürün `mobile_records`'ta var olmalı, miktar>0, tutar eksi olamaz) | [O] | İlişkisel test: satış → stok düşer + cari borçlanır (+ ödemeliyse tahsilat hareketi), alış → stok girer + tedarikçi (cari) alacaklanır, iade → stok girer + cari alacaklanır; native tenant değilse 409, yetkisiz 403 |
| **E5b** | Sunucu: `GET …/{id}` tek evrak detayı (satırlar, toplam, ödeme, cari); `GET /portal/native/documents` liste ucu (tür, tarih aralığı, cari, kullanıcı filtresi, durum) | [O] | Test |
| **E5c** | Sunucu (D11): **`document_void`** belge türü — bir satış/alış/iade evrağını iptal eder: orijinal `Voided`, satırların **stok VE cari** etkisi birlikte tek transaction'da ters çevrilir, `native_stock_levels`/`native_customer_balances` düzeltilir, `mobile_records` güncellenir/tombstone; zaten iptal edilmiş → 409; yetki ADMIN | [O] | İlişkisel test: iptalden sonra stok + bakiye = önceki, çift iptal 409, telefon senkronu |
| **E5d** | Sunucu: evrak **düzenleme** ucu `POST …/{id}/edit` = `document_void` + düzeltilmiş yeni evrak **tek transaction** (E4c'deki ledger-edit deseniyle aynı desen: yeni evrak geçersizse iptal de geri alınır) | [O] | Test: satır/miktar/fiyat/ödeme düzeltmesi, hata durumunda hiçbir şey değişmiyor |
| **E5e** | Panel: **"Yeni satış" / "Yeni alış" / "Yeni iade"** ekranları (ürün ara/ekle satır satır, miktar, fiyat, iskonto, ödeme şekli, cari seçimi, canlı toplam); `/evraklar` liste ekranı (tür, tarih, cari, tutar, durum; satır menüsü **Düzenle · İptal et**, iptal gerekçesi zorunlu diyalog); **yazdırılabilir evrak görünümü** (E3b'deki makbuz ile aynı desen) | [O] | bUnit + tarayıcı: yeni satış gir → listede + stok/ekstre etkisi doğru → düzenle → iptal et → hepsi doğru |

### E6 — Stok hareketleri ve sayım
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E6a** | Sunucu: ürün hareket ucu `GET …/stock-cards/{code}/movements` (tarih, tür, evrak, miktar, yürüyen stok) — `stockTransactions` normalize (ERP'siz alan adları koddan doğrulanır) | [O] | Test: devir + hareketler = mevcut stok |
| **E6b** | Sunucu: **stok düzeltme/sayım** `stock_count` sarmalı `POST /portal/native/stock-counts` (ürün listesi + sayılan miktar → fark hareketi), gerekçe zorunlu; iptal için `stock_void` (E4a mantığı, ürün tarafı) | [O] | Test: fark doğru işaretle, iptal stoku geri getiriyor |
| **E6c** | Panel: ürün satırında **Hareketler** sekmesi (E6a) ve `/stok/sayim` **sayım ekranı** (ürün ara/ekle, sayılan miktar, fark önizlemesi, onayla; barkod okutma alanı) | [O] | bUnit + tarayıcı |

### E7 — Yetki, denetim ve mod kapısı
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E7a** | `RolePermissions.CanEditNativeData` + Portal oturum bayrağı; her `/portal/native/*` ucu yetki + `DataSource=native` denetler (403 `ROLE_NOT_ALLOWED`, 409 `TENANT_IS_NOT_NATIVE`); kullanıcı yönetiminde rol açıklaması güncellenir | [O] | Test matrisi: rol × uç |
| **E7b** | **Denetim kaydı** (D5): `native_audit_log` tablosu (yeni tablo, mevcut tabloya dokunulmaz; yeni EF migration, `/health/schema` doğrulanır), her E1–E6 yazması kayıt atar; panelde kart/cari/hareket/evrak "Geçmiş" çekmecesi + `/denetim` (yönetici, filtreli liste) | [O] | Test + migration canlıda `current` |
| **E7c** | Panel menüsü: ERP'siz firmada **Ürünler, Müşteriler, Satış, Alış, İade, Tahsilatlar, Hareketler, Sayım** grupları; ERP'li firmada yazma düğmeleri gizli/pasif + açıklama (D3) | [O] | bUnit: iki mod, ekran görüntüsü |

### E8 — Telefon uyumu ve performans (tek panel kullanıcısı: sürüm çakışma kontrolü kapsam dışı)
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E8a** | Telefon uyumu: panelde yapılan kart/tahsilat/satış-alış-iade/iptal `sync/pull` ile cihazlara iniyor, çevrimdışı telefonun sonradan gelen satışıyla çakışmıyor (mevcut "stok eksiye düşebilir" kuralı korunur). Uçtan uca ilişkisel test: panel yazar → `sync/pull` değişiklik döner | [O] | Test |
| **E8b** | Performans: 20.000 ürün / 5.000 cari / 200.000 hareketli tohumda liste, ekstre, kaydetme < 1 sn (süreler DURUM'a) | [O] | Ölçüm |

### E9 — Kapanış
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E9a** | KB güncelle: 00 (kural 15'e panel yazma yolu, D1–D11), 03 (`native_audit_log`, `ledger_void`/`ledger_adjustment`/`customer_card_delete`/`document_void` belge şemaları, Voided alanı), `docs/api-contracts.md` (yeni uçlar), `CLAUDE.md` gerekiyorsa | [O] | Belgeler güncel |
| **E9b** | Canlı kontrol: yerelde native firmayla uçtan uca akış (yeni ürün → yeni cari → satış → alış → iade → tahsilat → düzeltme → iptal → bakiye/ekstre/stok doğru) ekran görüntüleriyle; canlı sunucuda gerçek native firma ile gözle kontrol | [O→K] | DURUM'da kanıt; canlı kısım "Seni Bekleyenler" |
| **E9c** | Plan tablosu + özet + hafıza notu | [O] | DURUM kapalı |

---

## 2. Kapsam dışı — sıradaki karar (ayrı onay gerektirir)

- **Dönem kilidi** (D6) — geçmiş tarihli evrak/hareket girişine sınır konulup konulmayacağı ayrı karar.
- **Kasa/banka hesapları ve virman, çek/senet, vade yaşlandırma raporu** — bu goal'ün kapsamı dışında, "Seni
  Bekleyenler"e not düşülür.

## 3. Bağımlılıklar ve sıra

`E0 → E7a (yetki) → E1 → E2 → E3 → E7b (denetim; E4/E5'ten önce) → E4 → E5 (satış/alış/iade) → E6 (stok/sayım) → E7c (mod kapısı) → E8 (telefon/performans) → E9 (kapanış)`

- E7a/E7b **erken**: yazma uçları yetkisiz/denetimsiz yayına çıkmasın; E1–E3 uçları E7a bayrağını kullanır.
- E4 ve E5 **en riskli görevler** (defter + stok değişmezi); E4a testi "iptalden sonra bakiye = önceki" olmadan
  E4e başlamaz; E5c testi "iptalden sonra stok+bakiye = önceki" olmadan E5d/E5e başlamaz.
- E5, E4'ten **sonra** gelir çünkü `document_void` (E5c) `ledger_void` (E4a) ile aynı storno prensibini
  paylaşır — E4'te doğrulanan desen E5'e taşınır.
- Migration içeren görevler (E7b, gerekirse E4/E5) sonrası `https://lisans.appsgo.cloud/health/schema` → `current`.

## 4. Bozulmaz kurallar (bu goal'e özgü ek)
- Defterden/evraktan satır **fiziksel silinmez/güncellenmez**; yalnız `Voided` + ters kayıt (D2/D11).
- `native_customer_balances` / `native_stock_levels` yalnız `NativeDocumentProcessor` yazar — panel uçları asla doğrudan tabloya yazmaz (D1).
- Her yazma ucu: firma **token'dan**, yetki sunucuda, `externalId` idempotent, `DataSource=native` şartı.
- Panel bileşenleri `MudBlazor` + mevcut `Shared/*`; ekranda ham JSON/İngilizce hata gösterilmez (`PortalMessages` Türkçe kodları genişletilir).
- Android'e bu goal'de dokunulmaz; gerekirse (yeni belge türü telefonda görünmesi) DURUM > "Seni Bekleyenler".

## 5. İnsan kapıları (beklenenler)
Gerçek native firmada canlı gözle kontrol (E9b) · dönem kilidi kararı (§2) · mevcut native firmalarda geçmiş veri
yedeği (yazma ekranları açılmadan önce önerilir).

## 6. Bitti tanımı
ERP'siz firmanın yöneticisi yalnız panelden: ürün ve müşteri açıp düzenler, tahsilat/tediye girer, **satış/alış/iade
faturası girip düzenler/iptal eder**, cari ekstresini görür, hareketi düzeltip iptal eder, stok sayar; bakiyeler,
stoklar ve telefonlar tutarlı kalır; her işlem denetim kaydında görünür; ERP'li firmada bu ekranlar salt okunurdur;
CI yeşil, KB güncel.
