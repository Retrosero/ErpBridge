# Goal — Kalan telefon belgeleri Mikro'ya (Tediye · Alış faturası · Yeni cari)

Tarih: 2026-09-19 · Kapsam: ErpBridge.Erp.Mikro + Core + Erp.Abstractions + CentralApi + Portal + Sipariş Cepte (Android)

İlerleme **[GOAL_ERP_YAZIM_2_DURUM.md](GOAL_ERP_YAZIM_2_DURUM.md)** dosyasına yazılır; her görevden sonra, o görevin PR'ı
içinde güncellenir. Oturum kapanırsa oradan devam edilir.

**Mikro yazım kuralları:** [mikro-yazim-referansi.md](mikro-yazim-referansi.md) — bu goal'ün de tek doğruluk kaynağı.
Orada olmayan kolon/kod **tahmin edilmez**; önce o belgeye kanıtıyla eklenir (Z0).

Önceki goal: [GOAL_ERP_YAZIM.md](GOAL_ERP_YAZIM.md) (Satış · Satış iadesi · Tahsilat — Y0–Y6 bitti). Oradaki kararlar
(idempotency, seri/sıra, hata sınıfları, log gizliliği, yalnız yeni evrak) burada da geçerlidir; yalnız farklar yazılır.

---

## 0. İstek, bulgu ve kararlar

### İstek (kullanıcı, 2026-09-19)
"İlk goal'de Satış + Tahsilat yazıldı, bunlar başarılı oldu. Şimdi geriye kalan diğer belge türleriyle ilgili de bir goal
planı çıkar."

### Bulgu: kalan türler bugün sessizce beklemiyor, kalıcı hata alıyor
Kod okunarak doğrulandı (2026-09-19, `origin/main`):

1. Telefon, ERP'li firmada da tediye / gider / alış / sayım belgelerini kuyruğa atıyor
   (`OutgoingDocumentRepository.documentType`, Sipariş Cepte).
2. Sunucunun ingest ucunda belge türü **beyaz listesi yok** — `documentType` yalnız uzunluk/boşluk açısından doğrulanıyor
   (`IngestEndpoints`), dolayısıyla her tür iş olarak kaydediliyor.
3. Ajan bu türleri tanımıyor: `AgentWorker.GenericDocumentTypes` yalnız `invoice, collection, dispatch_note,
   payment_order, customer_card, stock_card` içeriyor → `UNSUPPORTED_DOCUMENT_TYPE`, **`retryable=false`** — yani kalıcı
   `Failed`.

Sonuç: ERP'li bir firmada saha personelinin girdiği **tediye, gider, alış ve sayım belgeleri Mikro'ya hiç ulaşmıyor** ve
kuyrukta kalıcı hata olarak birikiyor. Bu goal bunların bir kısmını yazar, kalanını **temiz reddeder** (D5) — ikisi de
bugünkü "sessiz birikme" durumundan iyidir.

Ek bulgu: `customer_card` ve `stock_card` eski writer'lara düşüyor. Bu writer'lar `mikro-yazim-referansi.md` yazılmadan
önce üretildi; ilk goal'de satış/tahsilat kodlarının yanlış olduğu ortaya çıkmıştı (Y0c). Aynı denetim `customer_card`
için Z3c'de yapılır.

### Kullanıcı kararları (2026-09-19)
| # | Karar |
|---|---|
| U1 | **Kapsam: Tediye + Alış faturası (satırlı) + Yeni cari kartı.** Gider ve sayım bu goal'de **yok** |
| U2 | Yeni carinin **cari kodunu plasiyer telefonda girer** (ajan üretmez) |
| U3 | Yetkiler önceki goal'lerle aynı (§1) |
| U4 | Önce **Mikro V15**; V16 bu goal'de yazılmaz (önceki goal K13) |
| U5 | Test veritabanı **`MikroDB_V15_DEMO`**; `MikroDB_V15_02` yalnız okunur (önceki goal K14) |

### Varsayılan kararlar (goal'ün seçimleri — itiraz edilirse değişir)
| # | Karar | Gerekçe |
|---|---|---|
| D1 | **Peşin alış = kapalı alış faturası** (nakit → kasa, kart/havale → banka), satışın aynası; kısmi/karma ödeme → açık fatura + aynı transaction'da tediye makbuzu | Önceki goal D10/K9 ile aynı kalıp; Mikro ikisini de aynı biçimde tutuyor |
| D2 | Alışta KDV **stok kartının alış pointer'ından**, satışta olduğu gibi `fn_VergiYuzde`; iskonto zinciri satış faturasıyla aynı kolon düzeni | Z0b bunu canlı veriyle doğrular; doğrulanamazsa karar değişir |
| D3 | **Tediye = tahsilatın aynası**: tek makbuz, yöntem başına satır, nakit kasa / kart-havale banka. Çek-senet **çıkışı** bu goal'de yok (portföyden çıkış ayrı kural) | Kapsam şişmesin; saha tediyesi pratikte nakit/havale |
| D4 | Yeni cari kodu Mikro'da zaten varsa **kalıcı** `CUSTOMER_CODE_EXISTS`; telefon göndermeden önce senkron listesinde kontrol eder ve Portal'daki kod önekini gösterir | U2 seçildi; çakışma riski kullanıcıya ait, ama sessiz kalmamalı |
| D5 | **Gider ve sayım ERP'li firmada ingest tarafından reddedilir** (`RequiresNativeTenant` genişler); telefon "bu firmada desteklenmiyor" der ve belgeyi kuyruğa atmaz | Kalıcı hata birikmesin; kapsam dışı olmaları görünür olsun |
| D6 | Alış tedarikçisi **`CARI_HESAPLAR`'da normal cari**; telefon tedarikçi kodunu gönderir, ajan varlığını doğrular | Mikro'da ayrı tedarikçi tablosu yok |
| D7 | Yeni cari yazımı **başka belgeyi beklemez**: cari kartı işi bağımsız yazılır; aynı turda gelen satış/tediye cari yoksa geçici hata ile bekler | Sıralama bağımlılığı kurmadan yarış çözülür |
| D8 | Idempotency, seri/sıra, hata sınıfları, yuvarlama, log gizliliği, yalnız-yeni-evrak: önceki goal D4/D7/D9/D13/D14/D17 aynen | Tek kural seti |

---

## 1. Yetkiler

Kullanıcı 2026-09-19'da onayladı ("Evet, aynısı geçerli"):

| Yetki | Karar |
|---|---|
| Dala push, PR açma | Serbest |
| CI yeşil + inceleme yorumları çözülmüşken `main`'e squash-merge + dalı silme | Serbest |
| Play **internal** kanalına sürüm yükleme | Serbest (AAB'yi kullanıcı derliyor) |
| `MikroDB_V15_DEMO`'ya test evrakı yazma, aynı yedekten geri yükleme | Serbest (yalnız bu veritabanı) |
| İnsan gerektiren madde | Yapılabilen kısım yapılır, kalanı DURUM > "Seni Bekleyenler" |

**Asla:** `--force` push (rebase sonrası `--force-with-lease` dahil), CI kırmızıyken merge, `main`'e doğrudan push, Play
production yayını, **`MikroDB_V15_02` / `_03` / `_04` veya herhangi bir müşteri Mikro veritabanına yazma** (okuma
serbest), yedek dosyalarını silme/üzerine yazma, Mikro standart tablolarında şema değişikliği (yalnız `_ERPB_*`),
Mikro'daki mevcut evrakı silme/güncelleme (istisna: yeni evrakın aynı transaction'daki self-link UPDATE'i), tablo/kolon
silen veya tip değiştiren EF migration, başka oturumun commit edilmemiş değişikliğine dokunma, `git stash pop`.

---

## 2. Çalışma kuralları

- **Çalışma kopyası:** `git -C ErpBridge worktree add ../eb-yazim2 -b <dal> origin/main`. Ana klasör (`ErpBridge`) şu an
  **`faz-51-mobil-siparis-mikro`** dalında ve commit edilmemiş/push edilmemiş iş içeriyor — dokunulmaz.
- **Çakışma uyarısı:** o daldaki tek commit (`AgentJobPump`) belge dağıtımını `AgentWorker`'dan Core'a taşıyor, yani
  **Z3d'nin dokunacağı kodu yeniden yazıyor**. Z3d'ye başlamadan önce o dalın main'e girip girmediğine bakılır; girdiyse
  yeni tür kaydı `AgentJobPump`'a, girmediyse `AgentWorker`'a yapılır ve DURUM'a not düşülür.
- **Paralel goal'ler:** Parametre Yönetimi (P4) ve Log Merkezi (L4+) sürüyor. Ortak dosyalar (`IngestEndpoints`,
  `JobsEndpoints`, `erp_write_settings`, Portal) için dal açarken ve merge öncesi `origin/main` dala **merge** edilir.
- Her görev: build (0 uyarı / 0 hata) + `dotnet test ErpBridge.sln` yeşil → commit → PR → inceleme yorumları çözülür →
  squash-merge.

---

## Z0 — Referans ve envanter (yazan kod yok)

| ID | Görev | Kabul ölçütü |
|---|---|---|
| Z0a | Plan dalını main'e al: bu belge + DURUM + `CLAUDE.md` yetki istisnası | CI yeşil |
| Z0b | `mikro-yazim-referansi.md` **§10 Alış faturası**: CHA başlık (`cha_evrak_tip=0`, tedarikçiye alacak — §1'deki tekil indeks notu ve `EVRAK_ACIKLAMALARI (1, 0)` ipucu), STH kalem (giriş), KDV pointer'ı, iskonto zinciri, açık/kapalı fatura. **Canlı `MikroDB_V15_02`'den okunarak** kanıtlanır; örnek kayıt yoksa Mikro ekranından kesilmiş bir alış faturası istenir (DURUM > Seni Bekleyenler) | Salt okuma sözleşme testi; her satırın kanıt sütunu dolu |
| Z0c | **§11 Tediye**: CHA kodları (açıklama satırı `(0, 64)` ipucundan yola çıkarak gerçek `cha_evrak_tip`), kasa/banka tarafı, `ODEME_EMIRLERI` gerekiyor mu | Salt okuma testi |
| Z0d | **§12 Cari kartı**: `CARI_HESAPLAR` zorunlu kolonlar, grup kodu / ödeme planı / vergi dairesi / adres varsayılanları, `_RECid` self-link, mevcut kodun tekrarında Mikro'nun davranışı | Salt okuma testi |
| Z0e | Envanter: bugüne kadar `UNSUPPORTED_DOCUMENT_TYPE` almış işlerin firma/tür/adet dökümü | DURUM'a rapor; Z5a bu listeyi kullanır |

## Z1 — Sunucu: sözleşme, kabul kuralı, ayarlar

| ID | Görev | Kabul ölçütü |
|---|---|---|
| Z1a | `docs/mobil-belge-sozlesmesi.md` v3: `disbursement`, `purchase_receipt`, `customer_card` telefon gövdeleri (alan alan, örnekli) | Belge + çevirici testleri aynı örneği kullanır |
| Z1b | Gider ve sayım ERP'li firmada reddedilir (D5): ingest 409 + anlaşılır kod; telefon mesajı | Uç testi: ERP'li firma reddedilir, ERP'siz firma kabul eder |
| Z1c | `erp_write_settings`: alış serisi, tediye serisi, alış deposu, cari kartı varsayılanları (grup kodu, ödeme planı, vergi dairesi, kod öneki) + Portal UI | Uç testi + bUnit; boş bırakılan alan Mikro varsayılanına düşer |

## Z2 — Çevirici ve komutlar

| ID | Görev | Kabul ölçütü |
|---|---|---|
| Z2a | `PurchaseInvoiceCommand`, `DisbursementCommand`, `CustomerCardCommand` + `IErpAdapter`'a varsayılan gövdeli metotlar (`NotImplemented`) | Logo iskeleti değişmeden derlenir ve reddeder (seam testi) |
| Z2b | `MobileDocumentTranslator` genişlemesi: üç tür, doğrulama kodları, tutar kontrolü | Birim testler; eksik/hatalı alanda kalıcı hata kodu |

## Z3 — Mikro V15 writer'ları

| ID | Görev | Kabul ölçütü |
|---|---|---|
| Z3a | Alış faturası writer (açık + kasaya/bankaya kapalı) | `MikroDB_V15_DEMO`'da canlı test: kolonlar §10 ile birebir, ikinci gönderim yeni evrak açmaz |
| Z3b | Tediye writer | Canlı test; tahsilatla aynı idempotency ve kapatma kuralları |
| Z3c | Cari kartı writer + **eski `WriteCustomerCardAsync`'in referansa göre denetimi** | Canlı test; mevcut kodda çakışma → `CUSTOMER_CODE_EXISTS` |
| Z3d | Ajan yeni türleri tanısın (`AgentWorker` ya da `AgentJobPump` — §2 çakışma notu) | Birim test: tanınmayan tür hâlâ kalıcı reddedilir |

## Z4 — Sipariş Cepte

| ID | Görev | Kabul ölçütü |
|---|---|---|
| Z4a | Alış gövdesi ERP alanlarıyla (liste fiyatı, iskonto yüzdeleri, depo, tedarikçi kodu) | Birim test; ERP'siz firmada gövde değişmez |
| Z4b | Yeni cari: kod alanı, senkron listesinde çakışma kontrolü, yazım sonucu gösterimi | Birim test |
| Z4c | Tediye: ERP'li firmada tek belge (tahsilattaki `ErpCollectionDocument` kalıbı) | Birim test |
| Z4d | Sürüm yükseltme + Play internal (AAB'yi kullanıcı derler) | Derleme + `check_16kb.py` |

## Z5 — İzleme ve geriye dönük

| ID | Görev | Kabul ölçütü |
|---|---|---|
| Z5a | Z0e'deki başarısız işlerin yeniden denenmesi (Portal/Admin toplu yeniden deneme) | Uç testi; kapsam dışı türler yeniden denenmez, açıklamayla kapatılır |
| Z5b | Portal "ERP belgeleri" listesi ve Log Merkezi olayları yeni türleri tanısın | Test |

## Z6 — Kapanış

| ID | Görev | Kabul ölçütü |
|---|---|---|
| Z6a | `MikroDB_V15_DEMO`'da uçtan uca duman testi: alış faturası (açık + peşin), tediye, yeni cari + o cariye satış | DURUM'a rapor; Mikro ekranından kullanıcı/muhasebeci kontrolü |
| Z6b | KB güncellemesi: `00` kural, `01` writer listesi, `03` tablo/kolon | Diff |
| Z6c | DURUM kapanışı + "Seni Bekleyenler" | — |

---

## 3. Bu goal'de olmayanlar

Gider, sayım, stok kartı, ziyaret/rota, konum, teklif; çek-senet **çıkışı**; Mikro V16; e-belge (e-Fatura/e-Arşiv)
ayrımı; telefondan düzenleme/iptalin Mikro'ya yansıması. Gider ve sayım D5 ile **görünür biçimde** reddedilir, sessizce
kaybolmaz.
