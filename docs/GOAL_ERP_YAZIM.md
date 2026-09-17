# Goal — Sunucudan Mikro'ya Yazım (Satış · Satış İadesi · Tahsilat)

Tarih: 2026-09-17 · Kapsam: ErpBridge.Erp.Mikro + Core + Erp.Abstractions + Agent.Service + LocalStore +
RemoteApi + CentralApi + Portal (+ Admin) + Sipariş Cepte (Android)

İlerleme **[GOAL_ERP_YAZIM_DURUM.md](GOAL_ERP_YAZIM_DURUM.md)** dosyasına yazılır; her görevden sonra, o görevin
PR'ı içinde güncellenir. Oturum kapanırsa oradan devam edilir.

**Mikro yazım kuralları (kolon, kod, değer):** [mikro-yazim-referansi.md](mikro-yazim-referansi.md) — bu goal'ün
tek doğruluk kaynağı. Orada olmayan kolon/kod tahmin edilmez; önce o belgeye kanıtla eklenir.

---

## 0. İstek, bulgular ve kararlar

### İstek (kullanıcı, 2026-09-17)
Ajan ERP verisini sunucuya istendiği gibi gönderiyor. Şimdi **telefondan sunucuya gelen belgeler Mikro veritabanına
doğru işlensin**. Referans: `Fora_Mikro/`. İkinci turda: "Önce tahsilat, satış, iade gibi faturaların Mikro'da nasıl
eklendiğini netleştir, sonra karar ver. Önemli olan Mikro'ya doğru işlenmesi; gerekirse Sipariş Cepte düzeltilsin."

### ErpBridge'de bugünkü durum (inceleme, 2026-09-17 — kod okunarak doğrulandı)
**Olan:** Telefon → `jobs` → `GET /api/v1/jobs/pending` → `AgentWorker` (Windows servisi) → `IErpAdapter.Write*Async`
→ 7 Mikro writer → `POST /api/v1/jobs/ack`. V15/V16 kimlik stratejisi, `MikroSelfLink`, `MikroDocumentNumberAllocator`,
`ErpFieldText`, SQLite `mappings` var.

**Eksikler:**

| Alan | Eksik |
|---|---|
| Sözleşme | Telefonun `sales_order` gövdesi (`mobileDocumentId, counterparty, customerCode?, amount, paymentType, lines[barcode, productCode, quantity, unitPrice(net, iskontolar gömülü)]`) ajanın `SalesOrderPayload`'u ile **uyuşmuyor** → her satış `INVALID_PAYLOAD_SHAPE`. `collection` gövdesi yöntem başına ayrı belge, çek/senet bilgisi yalnız `description` metninde. ERP'li firmada iade yalnız satırsız kasa kaydı (`return`) olarak gidiyor; satırlı `sales_return` yalnız ERP'siz firmada |
| Kodlar | `MikroCollectionWriter` `cha_evrak_tip=63` (doğrusu **1**); irsaliye `sth_evraktip=4` (doğrusu **1**; 4 = çıkış faturası); fatura `sth_evraktip=63` (doğrusu **4**) |
| Kolonlar | Sipariş writer'ı 26 kolon; `sip_tutar`, KDV, kullanıcı, onay alanları yok. Fatura başlığında KDV kovaları, iskonto toplamları, kapalı fatura (kasa/banka) yok. Tahsilatta `ODEME_EMIRLERI` referans no kuralı, grup no, pozisyon yok |
| Dayanıklılık | `Processing` kiralaması hiç düşmüyor; geçici hata kalıcı `Failed` oluyor; commit ile SQLite mapping arasında çökme çift evrak üretebilir |
| Telefon | Yazım sonucu görünmüyor; Mikro'dan geri gelen evrakla yerel kayıt çift görünebilir; liste fiyatı / iskonto yüzdeleri / fiyat listesi no gönderilmiyor |

### Mikro'da nasıl yazılıyor (özet — ayrıntı referans belgesinde)
- **Satış faturası:** CHA başlık `63 / borç / cinsi 6`, STH kalem `evraktip 4 / çıkış`, kalem `sth_fat_recid_recno` = başlık.
  `sth_tutar` brüt, iskontolar `sth_iskonto1..6` tutar olarak, KDV `sth_vergi_pntr` + `sth_vergi`, başlıkta kovalanmış.
- **Peşin satış:** ayrı tahsilat yok — **kapalı fatura**: `cha_cari_cins` 4 kasa / 2 banka, `cha_kod` kasa/banka kodu,
  `cha_ciro_cari_kodu` müşteri, `cha_tpoz=1`.
- **Satış iadesi:** CHA `0 / alacak / iade=1`, STH `evraktip 3 / giriş / iade=1`.
- **Tahsilat makbuzu:** tek evrak, yöntem başına satır; `cha_evrak_tip=1`, alacak; nakit `cinsi 0` kasa; kart `19` banka
  grup 7; havale `17` banka grup 9; çek `1` portföy `ÇEK`; senet `2` portföy `SENET`; nakit dışı `ODEME_EMIRLERI` +
  `MK/MH/MC/MS-fff-sss-yyyy-nnnnnnnn`.
- **KDV oranı** `dbo.fn_VergiYuzde(pntr)`; **KDV dahil fiyat** `STOK_SATIS_FIYAT_LISTE_TANIMLARI.sfl_kdvdahil`.
- Firma **sipariş kullanmıyor** (`SIPARISLER` 1 satır); satış doğrudan fatura.

### Kullanıcı kararları (2026-09-17)
| # | Karar |
|---|---|
| K1 | Telefondaki Satış'ın Mikro türü **firma parametresi**: Sipariş / İrsaliye / Fatura (ayar varsayılanı Sipariş; **bu firma Fatura** seçecek) |
| K2 | Kapsam: **Satış + Satış iadesi (ayrı iade faturası) + Tahsilat**. Tediye, gider, alış, sayım, yeni cari, ziyaret, konum → sonraki goal |
| K3 | Seri parametreden, **sıra ajan** tarafından yazım anında Mikro'dan MAX+1 |
| K4 | **KDV dahil/hariç Mikro fiyat listesinden** (`sfl_kdvdahil`); telefon fiyat listesi no'yu gönderir. Oran stok kartı pointer'ı + `fn_VergiYuzde` |
| K5 | Temsilci, depo, kasa, banka, Mikro kullanıcı no **Portal kullanıcı kartından**; boşsa firma varsayılanı |
| K6 | Tahsilat yöntemleri: **Nakit, Kredi kartı, Havale/EFT, Çek, Senet** |
| K7 | Sipariş onay durumu **firma parametresi**, varsayılan onaylı |
| K8 | Yetkiler önceki goal'lerle aynı (§1) |
| K9 | **Peşin satış = kapalı fatura** (nakit → kasa, kart/havale → banka); ayrı tahsilat makbuzu yok |
| K10 | Birden çok yöntemli tahsilat = **tek makbuz, yöntem başına ayrı satır** (Sipariş Cepte buna göre tek belge gönderir) |
| K11 | Kredi kartı taksit **vade farkı açıklamaya** yazılır, ayrı hareket yok |
| K12 | **İskonto ayrı yazılır**: `sth_tutar` = liste fiyatı × miktar, iskontolar ayrı kolonlarda. Sipariş Cepte liste fiyatını, fiyat listesi no'yu ve iskonto yüzdelerini gönderecek şekilde düzeltilir |
| K13 | **Önce Mikro V15.** V16 bu goal'de yazılmaz |
| K14 | Test: **`MikroDB_V15_ERPBTEST`** (2026-09-17 11:24 `MikroDB_V15_02_17_09.bak` yedeğinden bu oturumda geri yüklendi, `F:\Mikro\ERPBTEST\`). **`MikroDB_V15_02` canlı firma verisi — yalnız okunur** |
| K15 | E-belge müşteriye göre değişir; bu goal'de ajan **normal Mikro faturası** yazar (e-belge alanları boş), e-Fatura/e-Arşiv'i ofis Mikro'dan gönderir. Tam e-belge ayrımı ayrı goal |
| K16 | Mikro'ya doğru işlenmesi için **Sipariş Cepte değiştirilebilir** |
| K17 | Fora kullanılmıyor; ErpBridge ile sıfırdan başlanıyor. Veritabanındaki `T/H/ST` serili saha kayıtları örnek alınmaz (referans §6) |

### Varsayılan kararlar (goal'ün seçimleri — itiraz edilirse değişir)
| # | Karar | Gerekçe |
|---|---|---|
| D1 | Firma ayarları ve kullanıcı eşlemesi sunucuda: `erp_write_settings` + `mobile_user_erp_mappings` (`mobile_users`'a kolon eklenmez). Ajan işi kiralarken `erpContext` alanıyla alır; saklanan `PayloadJson` değişmez | Eşleme düzeltilip "yeniden dene" denince güncel ayar kullanılır; eski ajan yeni alanı yok sayar |
| D2 | Telefon gövdesi → ERP-bağımsız komut çevirisi **Core**'da (`MobileDocumentTranslator`); KDV, iskonto tutarı, kolon ve kod hesabı yalnız **Mikro adaptöründe** | KB kuralı: Mikro bilgisi adaptörde |
| D3 | Mevcut tipli ingest sözleşmeleri korunur; telefon gövdesi `mobileDocumentId` ile ayırt edilir | Geriye uyumluluk |
| D4 | Idempotency kaydı **Mikro DB'de** `dbo._ERPB_EVRAK_ESLESME` (UNIQUE `DocumentType, ExternalId`), evrakla aynı transaction; SQLite `mappings` önbellek | Commit ile mapping arasında çökme çift evrak üretemez |
| D5 | Kolon değerleri: **Mikro'nun kendi ekran kaydı** + Fora; ikisi ayrışırsa Mikro'nun kendi kaydı esas (referans §6). Fora INSERT listesindeki her kolon yazılır | "Mikro'ya doğru işlensin" |
| D6 | V16 bağlantısında yeni telefon yolu kalıcı `ERP_VERSION_NOT_SUPPORTED` döner (mevcut tipli yol dokunulmadan kalır) | K13 — yarım V16 yazımı olmasın |
| D7 | Seri: **kullanıcı kartı > firma varsayılanı** (evrak türü başına: sipariş, irsaliye, fatura, iade, tahsilat). Sıra: seri + evrak türü içinde `UPDLOCK, HOLDLOCK` ile MAX+1 | Veride seriler plasiyer/araç başına |
| D8 | İskonto zinciri telefonla aynı sırada: `sth_iskonto1` satır iskontosu, `sth_iskonto2` müşteri özel iskontosu, `sth_iskonto3` genel (sepet) iskontosu; her biri bir öncekinden kalan tutar üzerinden **tutar** olarak. Ajan hesapladığı evrak toplamını telefonun `amount`'u ile karşılaştırır; fark > 0,05 TL → kalıcı `TOTAL_MISMATCH` (evrak yazılmaz) | Yanlış tutarla fatura kesilmesin |
| D9 | Yuvarlama: satır tutarları ve KDV 2 hane, `MidpointRounding.AwayFromZero`; başlık = satır toplamları (başlıkta ayrıca yuvarlama yok, `cha_yuvarlama=0`) | Fora + canlı veride başlık = satır toplamı |
| D10 | Satışta telefonda tek ödeme şekli: `Cari Borç`/boş → açık fatura; `Nakit` → kasaya kapalı; `Kredi/Banka Kartı`, `EFT / Havale` → bankaya kapalı. Kısmi/karma ödeme gelirse → açık fatura + aynı transaction'da tahsilat makbuzu | K9; karma ödeme bugün telefonda yok, gelirse doğru kayıt |
| D11 | İade: `Cari Alacak` → açık iade faturası; `Nakit` → kasaya kapalı; `Banka İade` → bankaya kapalı (Fora'nın kapama şekli iade faturasına da uygulanır). Hasarlı ürün kondisyon farkı `sth_iskonto1` (iade satırında başka iskonto yok), iade nedeni satır açıklamasına; orijinal fatura bağlanmaz (`sth_iade_evrak_*` boş, Fora gibi) | Tutar cariye/kasaya doğru düşer, stok tam miktarla girer |
| D12 | Çek/senet portföy kasası **firma ayarı**, varsayılan `ÇEK` / `SENET` (kullanıcı cevabı) | Canlı veri |
| D13 | Hata sınıfları: veri/eşleme/lookup/tutar farkı **kalıcı** (`Failed` + Türkçe neden); bağlantı/timeout/deadlock/ajan kapanması **geçici** (1-2-4-8-15-30-60 dk, en çok 10 deneme). `Processing` kiralaması 10 dk | Kendiliğinden toparlanma |
| D14 | Yalnız **yeni evrak**; telefondan düzenleme/iptal Mikro'ya yansımaz, yazılmış belge telefonda kilitlenir | Fora da yalnız yeni kayıt yazar |
| D15 | Döviz yalnız TL; cari kodu olmayan belge ERP'li firmada reddedilir, telefon göndermeden uyarır | Telefonda döviz ve Mikro'da cari açma bu goal'de yok |
| D16 | Sipariş teslim tarihi, adres no, teslim türü, sorumluluk merkezi, proje: Fora varsayılanı (Y0b); firma ayarında isteğe bağlı | Kapsam şişmesin |
| D17 | Yazım hataları Log Merkezi'ne iş kimliği + iz kimliğiyle; mesajda cari/stok **kodu** olabilir, unvan/tutar olmaz | Log goal'ünün gizlilik kuralı |

---

## 1. Yetkiler

Kullanıcı 2026-09-17'de onayladı ("Evet, hepsi geçerli"; test kopyası için "Ayrı kopya aç"):

| Yetki | Karar |
|---|---|
| Dala push, PR açma | Serbest |
| CI yeşil + inceleme yorumları çözülmüşken `main`'e squash-merge + dalı silme | Serbest (ErpBridge'de `main` → Coolify otomatik canlı dağıtım) |
| Play **internal** kanalına sürüm yükleme | Serbest |
| `MikroDB_V15_ERPBTEST`'e test evrakı yazma, gerektiğinde **aynı yedekten yeniden geri yükleme** | Serbest (yalnız bu veritabanı) |
| İnsan gerektiren madde | Yapılabilen kısım yapılır, kalanı DURUM > "Seni Bekleyenler" |

**Asla:** `--force` push, CI kırmızıyken merge, `main`'e doğrudan push, Play production yayını, **`MikroDB_V15_02` / `_03` /
`_04` veya herhangi bir müşteri Mikro veritabanına yazma** (okuma serbest; ajanın mevcut `_ERPB_*` kurulumu hariç), yedek
dosyalarını silme/üzerine yazma, Mikro standart tablolarında şema değişikliği (yalnız `_ERPB_*` tabloları oluşturulabilir),
Mikro'daki mevcut evrakı silme/güncelleme (tek istisna: yeni evrakın aynı transaction'daki self-link UPDATE'i), tablo/kolon
silme veya tip değiştiren EF migration, başka oturumun commit edilmemiş değişikliğine dokunma, `git stash pop`.

---

## 2. Çalışma kuralları

### Çalışma kopyaları
- **ErpBridge:** `git -C ErpBridge worktree add ../eb-yazim -b <dal> origin/main`. Ana klasördeki commit edilmemiş
  `packages.lock.json` değişikliklerine dokunulmaz.
- **Sipariş Cepte:** güncel kopya `Documents/GitHub/siparis_cepte` (`C:\src\siparis_cepte` eski dal — kullanılmaz).
  `git -C siparis_cepte worktree add ../sc-yazim -b <dal> origin/main`.
- **Log Merkezi goal'ü paralel sürüyor** (L3c–L8). `AgentWorker`, `HttpRemoteApiClient`, `JobsEndpoints` ortak: dal
  açılırken ve merge öncesi `origin/main` dala alınır; force-push yok.
- EF migration öncesi `origin/main`'deki son migration'a bakılır (2026-09-17: `LogMerkeziL0LogSettings`); Room migration
  numarası `origin/main`'deki `AppDatabase` sürümünden alınır.

### Her görev
1. `git -C <worktree> fetch origin && git -C <worktree> switch -c yaz-<id>-<kısa-ad> origin/main`
2. **Önce bilgi bankası** (`ErpBridge_knowledge_base/INDEX.md` → 00–04; mobilde `Siparis_Cepte_knowledge_base/`) ve
   **`docs/mikro-yazim-referansi.md`**.
3. Kod + test. Writer: birim + canlı entegrasyon (`ERPBridge_RUN_INTEGRATION=1`,
   `ERPBridge_MIKRO_WRITE_DB=MikroDB_V15_ERPBTEST`; testler bu değişken **ERPBTEST değilse yazma testlerini atlar**).
   Sunucu: xUnit + ilişkisel SQLite; Portal/Admin: bUnit; Android: birim test.
4. Yerel doğrulama yeşil:
   - .NET: `dotnet build ErpBridge.sln -c Debug` (0 uyarı/0 hata) + ilgili `dotnet test`
   - Writer görevleri: ERPBTEST'te canlı testler yeşil; test evrakları **`ERPBT`** serisiyle
   - Android: `./gradlew :app:compileDebugKotlin :app:testDebugUnitTest` (bu PC'de ASCII yol ayarları gerekir — memory)
5. Türkçe commit: ne + neden; sonuna `Co-Authored-By: Claude Opus 5 <noreply@anthropic.com>`.
6. Push → PR (gövde sonunda `🤖 Generated with [Claude Code](https://claude.com/claude-code)`) → CI.
   3 denemede yeşillenmezse ⛔, bağımlılar da ⛔.
7. İnceleme yorumları merge'ü bloklar: geçerli bulgu düzeltilir, geçersizse gerekçeyle yanıtlanır.
8. Merge öncesi `origin/main` dala alınır, yerelde build + test. DURUM, referans belgesi ve gerekiyorsa KB aynı dala.
9. `gh pr merge <n> --squash --delete-branch`; migration içeren görevden sonra
   `https://lisans.appsgo.cloud/health/schema` → `current`, değilse ⛔ + "Seni Bekleyenler".

### Bozulmaz kurallar
- **Mikro'ya yazım atomik:** başlık + kalemler + açıklama + self-link + ödeme emirleri + (karma ödemede) tahsilat +
  `_ERPB_EVRAK_ESLESME` tek transaction. Hata → rollback. Commit olmadan `succeeded` ack yok.
- **Aynı `(documentType, externalId)` asla ikinci Mikro evrakı açmaz** — her writer için "iki kez çalıştır → tek evrak"
  ve "commit sonrası yerel mapping yazılmadan çökme → tek evrak" testleri zorunlu.
- **Doğruluk testi:** her writer testi yazılan satırları okuyup referans belgesindeki **her kod kolonunu** ve tutar
  kolonunu beklenenle karşılaştırır; ayrıca Mikro'nun kendi fonksiyonlarıyla çapraz kontrol
  (`fn_CariHesapVergiDahilMeblag`, `fn_StokHareketVergiDahilNetDeger` — varsa) yapılır.
- **Parametrik SQL**; payload verisi string concat ile SQL'e girmez.
- **V15/V16 farkı yalnız adaptörde**; Core/Service/CentralApi Mikro kodu, kolon adı bilmez.
- **Kimlik alanı taşarsa reddet, serbest metin kırp** (`ErpFieldText`).
- Yeni Portal uçları `ADMIN`; Admin uçları `AdminPolicy` + `PerAdminRateLimitPolicy`.
- Eski ajan / eski telefon sürümü bozulmaz; ERP'siz firma defteri (`NativeDocumentProcessor`) eski ve yeni gövdeyi kabul eder.
- Telefona yeni alan → `VERI_ENVANTERI.md` + `play-data-safety.md` aynı PR.
- `TreatWarningsAsErrors=true` → 0 uyarı.

---

## 3. Fazlar ve görevler

Bağımlılık: **Y0 → Y1 ve Y2 paralel → Y3** → **Y4** (Y4a–c Y1 ile paralel başlayabilir) **→ Y5 → Y6**.

### Y0 — Referans ve temel düzeltmeler
| ID | Görev | Kabul ölçütü |
|---|---|---|
| Y0a | Plan dalını main'e al (bu belge + DURUM + `mikro-yazim-referansi.md` + `CLAUDE.md` yetki istisnası) | PR birleşti |
| Y0b | **Referansı tamamla:** Fora `V15_CariHareketYaz`, `V15_Stok_Hareketleri_Yaz`, `V15_TahsilatOdemeEmriYaz`, `V15_YeniSiparisKaydet`, `V15_YeniEvrakAciklamaKaydet` INSERT kolon listelerinin **tamamı** + her kolonun değeri (sabit / evrak / cari / stok / ayar); satış faturası, kapalı fatura, iade, 5 tahsilat yöntemi için canlı veriden (V15_02, yalnız okuma) birer tam satır dökümü; sipariş ve irsaliye için Fora değerleri; `sth_isk_mas1..10`, `cha_grupno`, `sth_cari_grup_no`, `EvrakVarMi` sorgusu, sipariş satır no başlangıcı, Mikro'nun iskonto zinciri tanımı | Belge; `MikroWriteColumnContractTests` (ERPBTEST): her kolon var, NOT NULL + varsayılansız her kolon listede |
| Y0c | **Yanlış kodların düzeltilmesi:** tahsilat `cha_evrak_tip`, irsaliye ve fatura `sth_evraktip`; kodlar tek yerde (`MikroDocumentCodes`) Fora enum adlarıyla | Birim testi; ERPBTEST'te yazılan evrakın kodları referansla aynı |
| Y0d | `_ERPB_EVRAK_ESLESME` tablosu (D4): DDL, ajan kurulum akışında `IF OBJECT_ID … IS NULL`, `MikroDocumentLedger` okuma/yazma | ERPBTEST: yoksa oluşur, varsa dokunulmaz; UNIQUE ihlali "zaten yazılmış" sonucu döner |

### Y1 — Sunucu: firma ayarları, kullanıcı eşlemesi, iş dayanıklılığı (CentralApi + Portal)
| ID | Görev | Kabul ölçütü |
|---|---|---|
| Y1a | `erp_write_settings` (firma başına): `SalesDocumentKind (order/dispatch/invoice)`, `OrderApprovalMode (approved/pending)`, seriler `OrderSeries, DispatchSeries, InvoiceSeries, ReturnSeries, CollectionSeries`, varsayılanlar `DefaultWarehouseNo, DefaultCashCode, DefaultCardBankCode, DefaultTransferBankCode, DefaultErpUserNo, DefaultSalespersonCode?, DefaultPriceListNo`, portföy `ChequePortfolioCode ('ÇEK'), NotePortfolioCode ('SENET')`, isteğe bağlı `ResponsibilityCenterCode?, ProjectCode?, DeliveryDayOffset?`. `mobile_user_erp_mappings`: `SalespersonCode?, WarehouseNo?, CashCode?, CardBankCode?, TransferBankCode?, ErpUserNo?` + seri geçersiz kılmaları (`OrderSeries?, DispatchSeries?, InvoiceSeries?, ReturnSeries?, CollectionSeries?`). Yalnız yeni tablolar | Migration smoke testi; varsayılanlar |
| Y1b | Portal uçları `GET/PUT /api/v1/portal/erp-settings`, `GET/PUT /api/v1/portal/users/{id}/erp-mapping`, `GET /api/v1/portal/erp-lookups` (sunucudaki mobil kayıtlarda depo/kasa/banka/temsilci/fiyat listesi bölümleri varsa; yoksa boş → serbest metin); yalnız `ADMIN`; temel doğrulama (kesin genişlik ajanda) | Uç testleri: 403, 400, başka firmanın kullanıcısı 404 |
| Y1c | Portal UI: "ERP Aktarım Ayarları" sayfası (ERP'siz firmada gizli) + `Kullanicilar.razor` "Mikro karşılıkları" bölümü ("boş = firma varsayılanı") | bUnit; yerel tarayıcı masaüstü + dar ekran |
| Y1d | **`erpContext`:** `GET /jobs/pending` yanıtında her işe firma ayarları + işi oluşturan kullanıcının eşlemesi (varsayılanla birleştirilmiş) + `createdByUsername`; ERP'siz firmada yok | Eşleme değişip iş yeniden kiralanınca yeni değer; eski yanıt şekli bozulmadı |
| Y1e | **İş dayanıklılığı (D13):** `jobs.LeasedUntilUtc?`, `jobs.NextAttemptAtUtc?`; kiralama süresi 10 dk; ack'e isteğe bağlı `retryable` → beklemeyle `Pending`, 10 denemede `Failed`; `retryable`'sız eski ack eskisi gibi terminal | Sahte saatle testler |

### Y2 — Ajan: telefon belgesini ERP-bağımsız komuta çevirme (Core + Abstractions + Service)
| ID | Görev | Kabul ölçütü |
|---|---|---|
| Y2a | `Erp.Abstractions` komutları: **`SalesDocumentCommand`** (`Kind: Order/Dispatch/Invoice`, `ExternalId, OccurredAt, CustomerCode, SalespersonCode?, WarehouseNo, Series, PriceListNo, ApprovalMode, ErpUserNo, Description?, ExpectedTotal`, `Settlement: Open/Cash/Card/Transfer` + `SettlementAccountCode?`, `Lines[StockCode, Quantity, UnitPointer, ListUnitPrice, LineDiscountPct, CustomerDiscountPct, GeneralDiscountPct, Note?]`, `ExtraPayments?`); **`SalesReturnCommand`** (`Settlement: Open/Cash/Bank` + hesap kodu, `Lines[StockCode, Quantity, UnitPointer, ListUnitPrice, ConditionPct, Reason]`, `WarehouseNo, Series, PriceListNo, ExpectedTotal`); **`CollectionCommand`** (`Series`, `Payments[Method: Cash/Card/Transfer/Cheque/Note, Amount, DueDate, AccountCode, Installments?, SurchargeAmount?, Cheque?{No, BankName, Branch, AccountNo, Drawer}, Note?{No, Debtor}]`). `IErpAdapter.WriteSalesDocumentAsync / WriteSalesReturnAsync / WriteCollectionDocumentAsync` (Logo iskeleti `NotSupported` sonuç) | Derleme; `SecondAdapterSeamTests` yeşil |
| Y2b | Core `MobileDocumentTranslator`: telefon gövdesi (`mobileDocumentId`) + `erpContext` → komut. Kurallar: `customerCode` zorunlu (D15), `productCode` zorunlu, miktar > 0, TL; ödeme şekli eşlemesi (D10, D11); karma ödeme → `ExtraPayments`; eksik eşleme → `ERP_MAPPING_MISSING` + alan; eski gövde (liste fiyatı / iskonto yüzdesi yok) → `MOBILE_APP_UPDATE_REQUIRED` (net fiyattan iskonto **tahmin edilmez**); çek/senet ayrıntısı açıklama metninden **okunmaz** | Birim testler: Y4 örnek gövdeleri, her ödeme şekli, her hata kodu |
| Y2c | `AgentWorker`: telefon gövdesi → çevirici → yeni adaptör metotları; eski tipli gövde → eski yol (D3). Ack'e `retryable` (SqlException bağlantı/timeout/deadlock numaraları, `TimeoutException`, kapanma), `ErpDocumentSeries/Number`; `RemoteJob.ErpContext` | Test: yeni ve eski yol; geçici hata `retryable=true`; `erpContext`'siz iş `ERP_CONTEXT_MISSING` |
| Y2d | Türkçe hata kataloğu (`ErpWriteErrorCatalog`): kod → mesaj ("Müşteri Mikro'da bulunamadı: 120.001", "Kullanıcının kasa eşlemesi yok — Portal > Kullanıcılar", "Telefondaki toplam ile Mikro hesabı tutmuyor (fark 0,12 TL)", "Sipariş Cepte'yi güncelleyin"…) | Her kod için test; mesajda unvan/tutar dışı kişisel veri yok (D17) |

### Y3 — Mikro V15 writer'ları (Erp.Mikro)
Her görevde: yalnız V15 (`RecnoStrategy`); kolon/değerler `mikro-yazim-referansi.md`'den; ERPBTEST'te canlı test; test
yazıldıktan sonra satırlar okunup referansla kolon kolon karşılaştırılır.

| ID | Görev | Kabul ölçütü |
|---|---|---|
| Y3a | **Ortak altyapı:** `MikroWriteSession` (bağlantı, transaction, firma/şube no, Mikro kullanıcı no, gün tarihi), `_ERPB_EVRAK_ESLESME` ön kontrol + aynı transaction'da kayıt, seri/sıra tahsisi + Fora `EvrakVarMi` eşdeğeri, commit sonrası SQLite önbellek; V16 bağlantısında `ERP_VERSION_NOT_SUPPORTED` (D6) | İki kez çalıştır → tek evrak; commit sonrası çökme simülasyonu → tek evrak; eşzamanlı iki yazım aynı seride farklı sıra |
| Y3b | **Lookup + fiyat hesabı:** cari var + kilitli değil, stok var (Fora'nın satış engeli kontrolü), depo/kasa/banka/temsilci var, fiyat listesi var; `sfl_kdvdahil` → KDV dahil ise liste fiyatı KDV hariçe çevrilir (oran `fn_VergiYuzde(sto_toptan_vergi)` — perakende satışta `sto_perakende_vergi`); iskonto zinciri tutarları (D8), satır KDV'si iskontolu net üzerinden, 2 hane (D9); `ExpectedTotal` karşılaştırması | Birim: KDV dahil/hariç, %0/%1/%10/%20, 3 iskonto zinciri, yuvarlama, `TOTAL_MISMATCH`; canlı: olmayan cari/stok/depo/kasa/liste kalıcı hata |
| Y3c | **Satış faturası** (açık + kapalı): CHA başlık (63/borç/6, iskonto ve KDV kovaları, `cha_meblag` formülü), kapalıda `cha_cari_cins` 4/2 + `cha_kod` kasa/banka + `cha_ciro_cari_kodu` müşteri + `tpoz=1`; STH kalemler (4/çıkış, brüt tutar, iskonto1..3, KDV, depo, fiyat listesi, `sth_fat_recid_recno`); `EVRAK_ACIKLAMALARI` (51/0/63) | ERPBTEST: açık, nakit kapalı, kart kapalı, havale kapalı için kolon karşılaştırması; cari bakiye sorgusu açıkta artar, kapalıda değişmez; kasa/banka bakiyesi kapalıda artar |
| Y3d | **Sipariş** (`SIPARISLER` + açıklama): Fora V15 kolon seti, `sip_tutar`, `sip_iskonto_1..3`, `sip_vergi_pntr`/`sip_vergi`, onay moduna göre `sip_OnaylayanKulNo`/`sip_cagrilabilir_fl` (K7), kullanıcı, temsilci, depo, fiyat listesi, teslim tarihi/adres/teslim türü (D16) | ERPBTEST kolon testi; iki onay modu; `SUM(sip_tutar - iskontolar + sip_vergi)` = telefon toplamı |
| Y3e | **Satış irsaliyesi** (STH 1/çıkış + açıklama 16/1/1) | ERPBTEST kolon testi; depo stok miktarı irsaliye kadar azalır |
| Y3f | **Satış iadesi faturası** (açık + kasaya/bankaya kapalı): CHA 0/alacak/6/iade=1; STH 3/giriş/iade=1, kondisyon farkı `sth_iskonto1`, neden açıklamada (D11); `EVRAK_ACIKLAMALARI` (51/1/0) | ERPBTEST: üç iade şekli kolon testi; cari bakiye açıkta azalır; stok miktarı iade kadar artar |
| Y3g | **Tahsilat makbuzu** (tek evrak, yöntem başına satır): CHA 1/alacak + yönteme göre `cinsi`, `kasa_hizmet/hizkod`, `sntck_poz`, `karsidgrupno`, `cha_vade`; nakit dışı `ODEME_EMIRLERI` (tip, sonpoz, nerede cins/kod/grup, ilk evrak seri/sıra/satır, borçlu, vergi dairesi, çekte no/banka/şube/hesap, `sck_imza`); referans no `MK/MH/MC/MS-fff-sss-yyyy-nnnnnnnn` (Mikro'nun 8 haneli kuralı, aynı transaction'da kilitli MAX+1); kart vade farkı + taksit açıklamada (K11); `EVRAK_ACIKLAMALARI` (51/1/1) | ERPBTEST: 5 yöntemi içeren tek makbuz → 5 satır, `cha_satir_no` 0..4, 4 ödeme emri, referans nolar sıralı ve 8 haneli; cari bakiye toplam kadar azalır |
| Y3h | **Karma ödemeli satış (D10):** açık fatura + tahsilat makbuzu aynı `MikroWriteSession`'da; biri düşerse ikisi de geri alınır | ERPBTEST: tahsilat adımında hata → fatura da yok; başarıda iki evrak, tek eşleşme kaydı + türetilmiş tahsilat kimliği |

### Y4 — Sipariş Cepte (Android)
| ID | Görev | Kabul ölçütü |
|---|---|---|
| Y4a | **Satış gövdesi:** satırda `listUnitPrice`, `lineDiscountPercent`, `customerDiscountPercent`, `generalDiscountPercent`, `unitPrice` (net, eski alan korunur), `quantity`; belgede `priceListNo` (seçilen fiyat grubu → Mikro fiyat listesi sıra no eşlemesi, ürün verisinden doğrulanır), `paymentType`, `bankName`, `customerCode` (ERP'li firmada zorunlu — yoksa belge gönderilmez, "Bu müşteri Mikro'da kayıtlı değil"); onay merkezi aynı gövdeyi kullanır | Birim test: gövde çıktısı, iskonto zinciri telefondaki toplamla aynı, cari kodsuz engel |
| Y4b | **İade gövdesi:** ERP'li firmada da satırlı `sales_return` (`productCode`, `listUnitPrice`, `quantity`, `conditionPercent`, `reason`, `warehouse`, `settlementMethod`, `bankName`, `priceListNo`); kasa kaydı ayrıca ajana gönderilmez (çift kayıt olmasın); ERP'siz firma defteri eskisi gibi | Birim test; `NativeDocumentProcessor` yeni gövdeyle testleri yeşil |
| Y4c | **Tahsilat gövdesi:** tek `collection` belgesi, `payments[{method: cash/card/transfer/cheque/note, amount, bankName?, installments?, surchargeAmount?, dueDate?, cheque?{no, bankName, branch, accountNo, drawer}, note?{no, debtor}}]`; onay merkezi aynı; ERP'siz firma defteri yeni gövdeyi kabul eder (eski gövde de) | Birim test: karma tahsilat tek belge; sunucu `NativeDocumentProcessor` testi; `VERI_ENVANTERI.md` + `play-data-safety.md` (çek/senet bilgisi) |
| Y4d | **Yazım sonucu telefonda:** belge listesinde "Mikro'ya yazıldı: <seri>-<sıra>", "Bekliyor", "Yeniden denenecek", "Hata: <Türkçe neden>"; yazılmış belge düzenlenemez (D14). Durum mevcut senkron yolundan, yoksa `GET /api/v1/mobile/documents/status?externalIds=` | Birim + UI testi; uç testi (eklendiyse) |
| Y4e | **Çift görünme önleme:** Mikro'dan senkronla gelen evrak, ack'teki seri/sıra ile yerel `MOB-…` kaydıyla eşleşir; liste ve bakiyede tek kez | Birim test |
| Y4f | Sürüm artışı + Play **internal** | Yükleme kanıtı DURUM'da |

### Y5 — İzleme ve operasyon
| ID | Görev | Kabul ölçütü |
|---|---|---|
| Y5a | Portal "ERP Aktarım" listesi: tür, kullanıcı, müşteri kodu, durum, Mikro seri/sıra, Türkçe hata, deneme; filtreler; `ADMIN` "Yeniden dene". `GET /api/v1/portal/erp-documents`, `POST …/{jobId}/retry` | Uç + bUnit |
| Y5b | Admin `Jobs`: `erpContext`, `retryable`, `NextAttemptAtUtc`, `LeasedUntilUtc`; yazım hataları Log Merkezi'ne (L3c birleşmişse onun kuyruğu, değilse mevcut telemetri yolu) | bUnit; log olayı testi |

### Y6 — Kapanış
| ID | Görev | Kabul ölçütü |
|---|---|---|
| Y6a | KB: ErpBridge `01` (writer'lar, kodlar, kapalı fatura, `MikroWriteSession`, `_ERPB_EVRAK_ESLESME`), `03` (yeni tablolar, `jobs` kolonları), `02` (seriler Portal'da, e-belge ayrımı yok), `00` (telefon gövdesi yalnız çevirici üzerinden; idempotency Mikro'da; yazma testleri yalnız ERPBTEST); `docs/api-contracts.md`; Sipariş Cepte KB | Belgeler güncel |
| Y6b | **Yerel uçtan uca duman testi (ERPBTEST):** yerel CentralApi + Portal + ajan servisi (bağlantı ERPBTEST) → Portal'da ayar + eşleme → telefon/test istemcisi gerçek gövdeyle: açık fatura, nakit/kart/havale kapalı fatura, sipariş, irsaliye, 3 iade şekli, 5 yöntemli tahsilat → SQL ile kolon/tutar kontrolü → aynı belge tekrar → tek evrak → ajan durdur/başlat → iş kendiliğinden tamamlanır | DURUM'da sorgu çıktıları |
| Y6c | "Seni Bekleyenler" son hâli | — |

---

## 4. Beklenen insan kapıları (şimdiden bilinenler)
- **Mikro ekranında muhasebeci kontrolü:** ERPBTEST'e yazılan açık/kapalı fatura, iade, tahsilat makbuzu (5 yöntem),
  sipariş ve irsaliyenin Mikro'da açılıp doğru görünmesi (tutar, iskonto, KDV, cari/kasa/banka bakiyesi, çek/senet
  portföyü). ERPBTEST'i Mikro'da görmek için test firması olarak tanımlanması gerekir. Onay gelmeden canlı DB'ye bağlanılmaz.
- Portal'da bu firma için **satış türü = Fatura**, seriler (plasiyer başına), kasa/banka/depo eşlemeleri.
- Ajan servisinin canlıda `MikroDB_V15_02`'ye yazacak şekilde devreye alınması (ayrı onay).
- e-Fatura/e-Arşiv müşterilerinin telefonda ayrılması ve bilgi fişi basımı — ayrı goal (K15).
- Mikro V16 desteği — ayrı goal (K13).
- Tediye, gider, alış, sayım, yeni cari, ziyaret/konum — sonraki goal (K2).
- Telefonda gerçek cihazla uçtan uca deneme; Sipariş Cepte production kararı ve Play Data Safety formu.
