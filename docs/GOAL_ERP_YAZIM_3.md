# GOAL — ERP yazım 3: Alış faturası · Gider · Sayım

**Amaç:** telefonda üretilen **alış faturası**, **gider** ve **sayım** belgelerinin Mikro'ya doğru
evrak tipleriyle yazılması. ERP yazım 1 satış + tahsilatı, ERP yazım 2 tediye + alış kolon düzeni +
yeni cariyi kapsadı; bu goal kalan üç belgeyi kapatıyor.

**Test veritabanı:** `MikroDB_V15_DEMO`. `MikroDB_V15_02/_03/_04` ve müşteri veritabanlarına **yazılmaz**
(okuma serbest).

**Referans:** `docs/mikro-yazim-referansi.md` — bu goal için §13 (kasa masraf fişi), §14 (sayım sonuçları),
§15 (alış evrak numarası) eklendi; alış faturasının kolon kovalaması zaten §10'da.

---

## Kullanıcı kararları (2026-09-19)

| # | Karar |
|---|---|
| **K1** | **Gider = Kasa masraf fişi (`cha_evrak_tip=37`)**, alış faturası ayrı bir belgedir. Fora'nın "gideri hizmet faturası yaz" yaklaşımı **kullanılmaz** (§13). |
| **K2** | Telefon gider kartlarını **ERP'den çeker**. Sabit 6 kategori (Yemek, Kırtasiye, Kargo, Yol/Yakıt, Bakım, Diğer) kaldırılır; yerine Mikro'daki `MASRAF_HESAPLARI` kartları gelir. |
| **K3** | Gider **kredi kartıyla da** ödenebilir: nakit / banka havalesi / kredi kartı. |
| **K4** | Gider **KDV'si telefondan gelir** (kullanıcı girer), ERP'de hesaplanmaz. |
| **K5** | Telefon alışı Mikro'da **iki evraktır**: alış faturası (ürün girişi) + tediye (ödeme). Tediye tahsilatın tersidir; alış faturası ürün girişi içindir. |
| **K6** | Alış KDV'si **stok kartının `vergi_pntr`'ından**; depo **panelden ayarlanan alış deposu**. |
| **K7** | Evrak serisi: **ERP'de kullanılmış seri varsa ona göre devam edilir** (§15). |
| **K8** | **Alış irsaliyesi kapsam dışı.** |
| **K9** | Sayımda **yalnız fiş yazılır**; sayım sonuçları **ERP'de kesinleştirilir** (Mikro'nun kendi uygulama adımı). |
| **K10** | Sayımda telefon **stok kodu gönderir** — ERP'ye tam uyum; barkod→stok kodu tahmini yapılmaz. |
| **K11** | Sayım fiş numarası **depo bazlı MAX+1**. |

## Varsayılanlar (aksi söylenmedikçe)

| # | Varsayılan |
|---|---|
| **V1** | Yazım idempotenttir: her belge `_ERPB_EVRAK_ESLESME` defterine işlenir, aynı `externalId` iki kez yazılmaz. |
| **V2** | Mikro satırında NULL bırakılmaz (§1). |
| **V3** | Eksik/çözülemeyen zorunlu alan → **kalıcı hata**, sessiz varsayılan seçilmez. |
| **V4** | Yeni ayarlar `erp_write_settings`'e eklenir ve Portal'da Türkçe gösterilir. |
| **V5** | Telefon tarafı değişiklikleri geriye uyumludur: eski sürüm gönderirse ajan **anlaşılır Türkçe hata** üretir (`MOBILE_APP_UPDATE_REQUIRED`). |
| **V6** | V15 hedeflenir; V16 farkları not edilir ama bu goal'de yazılmaz. |

---

## Bilinen durum (ERP yazım 2 sonrası)

| Telefondaki iş | `documentType` | Bugün |
|---|---|---|
| Satış / iade / tahsilat / tediye | `sales_order`, `sales_return`, `collection`, `disbursement` | ✅ yazılıyor |
| **Alış faturası** | `purchase_receipt` | ⛔ ERP'li firmada telefon **göndermiyor**; ingest 409 |
| **Gider** | `disbursement` (ayırt edilemiyor) | ⛔ cari kodu yok → `MISSING_CUSTOMER_CODE` |
| **Sayım** | `stock_count` | ⛔ ajan tanımıyor → kalıcı hata |

Üç belgenin de **telefon tarafında** değişiklik gerektirmesinin nedeni bu tablo: gider tediyeden
ayırt edilemiyor, alış ERP'li firmada hiç gönderilmiyor, sayım stok kodu taşımıyor.

---

## Fazlar

### Y0 — Kanıt toplama *(tamamlandı)*

| # | İş | Durum |
|---|---|---|
| Y0a | Gider evrak tipinin 37 olduğunu Fora enum sayımı + canlı veriyle doğrula, §13'ü yaz | ✅ |
| Y0b | `SAYIM_SONUCLARI` kolon/numara düzenini çıkar, §14'ü yaz | ✅ |
| Y0c | Alış evrak numarasının tedarikçi serisi olduğunu doğrula, §15'i yaz | ✅ |
| Y0d | Kredi kartının Mikro'da banka hesabı üzerinden yürüdüğünü doğrula (`cinsi=22`, `hizkod=ban_kod`) | ✅ |
| Y0e | Telefonun bugün ürettiği üç belgenin gövdesini kod okuyarak çıkar | ✅ |

### Y1 — Sözleşme ve kapı

| # | İş | Çıktı |
|---|---|---|
| Y1a | Gövde sözleşmesi v3: `expense` (yeni tür), `stock_count` (stok kodlu), `purchase_receipt` (KDV + depo) | `docs/mobil-belge-sozlesmesi.md` |
| Y1b | Gider artık `disbursement` değil **`expense`** türüyle gelir; tediye yalnız cari ödemesidir | ingest + çevirmen |
| Y1c | Ingest kapısı: ERP'li kiracıda `purchase_receipt`, `expense`, `stock_count` **kabul edilir** (bugün 409) | `NativeDocumentProcessor`, `ApprovalService` |
| Y1d | Kapsam dışı türlerin (`stock_card`, `customer_card_batch`…) reddi kalır, mesajı Türkçeleşir | ingest |

### Y2 — Ayarlar ve katalog

| # | İş | Çıktı |
|---|---|---|
| Y2a | `erp_write_settings`: alış deposu, alış serisi davranışı, gider varsayılan kasası | migration + Portal |
| Y2b | **Gider kartı kataloğu**: `MASRAF_HESAPLARI` → merkez → telefon senkronu | yeni katalog tablosu + uç nokta |
| Y2c | Portal'da gider kartı listesi (salt okunur, senkron durumu) | `ErpGiderKartlari.razor` |

### Y3 — Mikro yazıcıları

| # | İş | Çıktı |
|---|---|---|
| Y3a | `MikroPurchaseInvoiceWriter` — CHA 0/alacak/cinsi 6 + STH 3/giriş, §10 + §15 numaralama | writer + testler |
| Y3b | `MikroExpenseWriter` — CHA 37, §13 (nakit / havale / kredi kartı üç yol) | writer + testler |
| Y3c | `MikroStockCountWriter` — `SAYIM_SONUCLARI`, depo bazlı MAX+1 | writer + testler |
| Y3d | Üç belge için komut + çevirmen (`MobileDocumentTranslator`) | `Core/Jobs` |
| Y3e | **İki ajan yolu da** tanısın: `AgentWorker` **ve** `AgentJobPump` | `MobileDocumentTypes` |

> Y3e neden ayrı bir madde: ERP yazım 2'de tediye yalnız `AgentWorker`'a eklendiği için kullanıcı
> tepsi uygulamasında "No writer is configured" hatası aldı. Her yeni belge türü **iki yere** eklenir.

### Y4 — Telefon

| # | İş | Çıktı |
|---|---|---|
| Y4a | Gider ekranı: kategori çipleri → **ERP gider kartları**, KDV alanı, kredi kartı seçeneği | `ExpensesModule.kt` |
| Y4b | Gider `expense` türüyle kuyruğa girer (kasa defterinde tediye görünmeye devam eder) | `ApprovalSubmissions.kt` |
| Y4c | Sayım gövdesine **stok kodu** eklenir; kodu olmayan satır sayıma alınmaz | `ApprovalSubmissions.kt` |
| Y4d | Alış ERP'li firmada da kuyruğa girer (bugün yalnız `NativeCompany.isActive` iken) | `PurchaseModule.kt` |
| Y4e | Araç bakım gideri de `expense` yoluna taşınır | `VehiclesModule.kt` |

### Y5 — Canlı doğrulama (`MikroDB_V15_DEMO`)

| # | İş |
|---|---|
| Y5a | Üç belge türü için uçtan uca yazım + Mikro ekranında gözle doğrulama |
| Y5b | Alış: aynı tedarikçiye ikinci fatura → serinin MAX+1'den devam ettiği |
| Y5c | Gider: nakit / havale / kredi kartı üç yol, KDV'li ve KDV'siz |
| Y5d | Sayım: fişin Mikro'nun sayım ekranında açıldığı ve uygulanabildiği |

### Y6 — Kapanış

| # | İş |
|---|---|
| Y6a | Portal ERP Belgeleri sayfasında yeni türlerin Türkçe adları ve sayım için "ERP'de kesinleştirilmeyi bekliyor" rozeti |
| Y6b | Bilgi bankası + `GOAL_ERP_YAZIM_3_DURUM.md` güncel |
| Y6c | Sipariş Cepte sürüm yükseltme + Play **internal** |

---

## Riskler

| Risk | Karşılık |
|---|---|
| Gider kartı kataloğu telefona ulaşmadan kullanıcı gider girerse | Kart seçilmeden kayıt yapılamaz; katalog boşsa ekran "ERP'den gider kartları bekleniyor" der |
| Tedarikçi serisi yanlış seçilirse fatura numarası muhasebede bozulur | Y5b canlı testi; seri seçimi tek bir yerde (`PurchaseSeriesResolver`) ve testli |
| Sayım fişi uygulanmadan unutulursa stok düzelmez | K9 gereği bu kullanıcının işi; Portal'da bekleme rozeti (Y6a) |
| Eski telefon sürümü gideri `disbursement` içinde göndermeye devam ederse | V5: cari kodu yoksa `MOBILE_APP_UPDATE_REQUIRED` ile açık hata |

## Yetkiler

ERP yazım 2'deki yetkiler bu belge için de geçerlidir: dala push, PR açma, CI yeşil ve inceleme
yorumları çözülmüşken `main`'e squash-merge, dal silme, Play **internal** yükleme ve
`MikroDB_V15_DEMO`'ya test evrakı yazma. Yasaklar aynen sürer: `--force` push, CI kırmızıyken merge,
`main`'e doğrudan push, Play production, müşteri veritabanlarına yazma.
