# Goal — Yönetim Paneli Geliştirmeleri (Onaylar · Stok · Cariler · Depo)

Tarih: 2026-09-16 · Kapsam: ErpBridge.CentralApi + ErpBridge.Portal + Sipariş Cepte Depo ekranı

Bu belge otonom çalışma içindir. `/goal` ile başlatılır ve insan müdahalesi olmadan
sırayla işlenir. İlerleme **[GOAL_PANEL_DURUM.md](GOAL_PANEL_DURUM.md)** dosyasına yazılır;
her görevden sonra güncellenir. Oturum kapanırsa oradan devam edilir.

İlgili plan: [PLAN_ROLLER_VE_DEPO.md](PLAN_ROLLER_VE_DEPO.md). Bu goal o planın
**Adım 6'sını (depo personeli sayfası)** üstlenir ve **V4 kararını değiştirir** (aşağıda).
Adım 5 (muhasebe klavye ekranı), 7 (TV) ve 8 (performans) bu goal'ün dışındadır.

---

## 0. Kullanıcının istekleri ve kararları (2026-09-16)

### İstekler
1. **Onaylar:** onay bekleyenlerin yanında onaylananlar da görünsün; talebe tıklayınca detayı açılsın.
2. **Depo:** telefonda Depo ekranında görünen siparişler panelde görünmüyor.
3. **Cariler:** listede müşteriye tıklanınca fatura/hareket listesi, faturaya tıklanınca içeriği görünsün.
4. **Stok:** yalnız 200 ürün görünüyor; sayfalama, gelişmiş sıralama ve filtreleme olsun.

### Kök neden bulguları (kararlardan önce doğrulandı)
- **Onaylar:** sunucuda `GET /api/v1/android/approvals?status=…` (onaylanan/reddedilen dahil) ve
  `GET /api/v1/android/approvals/{id}` (belgeler, olay geçmişi, stok uyarısı) **zaten var**; panel yalnız
  `status=pending` çağırıyor, detay ucunu hiç kullanmıyor.
- **Depo:** telefonun Depo ekranı (`WarehouseScreen` / `WarehouseViewModel`) **sunucuyla hiç konuşmuyor**.
  `wms_orders`/`wms_order_items` Room tablolarını okuyor; kaynakları: sahte demo siparişleri
  (`seedMockOrders`, `SM-2026-…`), taklit `syncAll()` (hiçbir şey göndermiyor), o telefonda girilen satışlar
  ve ERP faturaları (`/sync/faturaHareket` → "Sevk Edildi"). Panelin `/depo` sayfası yer tutucu.
  Sunucu kuyruğu (`order_fulfillments`, PR #52) yalnız `tenant_warehouse_settings.Enabled=true` firmada ve
  **açıldıktan sonra gelen** satışları alıyor; geri doldurma yok.
- **Cariler:** `mobile_records` içinde `customerTransactions` (CARI_HESAP_HAREKETLERI aynası) ve
  `stockTransactions` (STOK_HAREKETLERI aynası) var. ERP'li firmada fatura ↔ kalem bağı
  `cha_recno` ↔ `faturaRecno`; ERP'siz firmada bu alanlar **yok** (bağ `evrakNo` + `cariKod` ya da `jobs` payload'u).
- **Stok:** `PortalReports.MaxStockRows = 200` (cariler 500); tüm satırlar belleğe alınıp kesiliyor. Kaynakta
  grup/marka/reyon, barkod, fiyat listeleri ve depo bazlı miktar (rezerve, son hareket) mevcut.

### Kararlar
| # | Karar |
|---|---|
| K1 | **Depo tek doğru kaynağı sunucu kuyruğu.** Panel `/depo` kuyruğu gösterir; modül açılırken son N günün siparişleri geri doldurulur; **telefonun Depo ekranı da kuyruğa bağlanır**, sahte veri ve taklit senkron kaldırılır |
| K2 | **Cari detayı = ekstre + tıklanabilir faturalar:** tüm hareketler tarih sırasıyla, devir ve yürüyen bakiye, tarih aralığı + tür filtresi; fatura satırı tıklanınca kalemler |
| K3 | **Stok:** sunucu taraflı sayfalama + grup/alt grup/marka/reyon filtresi + depo bazlı miktar + fiyat listesi ve fiyat/miktar aralığı + durum (tükenen/eksi/eşik altı) + barkod arama + kolon sıralama + son hareket tarihi |
| K4 | **V4 değişti:** yalnız DEPO rolü olan kullanıcı **telefona girebilir**, ama telefonda **yalnız Depo ekranını** görür. Panelden çalışmak da mümkün kalır. Koltuk kuralı değişmez |
| K5 | Android sürümü bitince **Play internal test** kanalına otomatik yüklenir; production'a asla |

### Varsayılan kararlar (goal'ün kendi seçimleri — itiraz edilirse değişir)
| # | Karar | Gerekçe |
|---|---|---|
| D1 | Eski telefon sürümü DEPO rolünü tanımadığı için tüm ekranları açar → yalnız DEPO rolü olan kullanıcıya telefonda giriş ve her istek **yalnız `versionCode ≥ Mobile:MinWarehousePhoneVersionCode`** ise kabul edilir; ayar boşken eski davranış (`ROLE_NOT_ALLOWED_ON_PHONE`) sürer | Depocunun eski sürümden satış girmesi engellenir |
| D2 | Yalnız DEPO rolü olan kullanıcının satış/tahsilat/iade/tediye/alış belgesi ingest'i sunucuda 403 `ROLE_NOT_ALLOWED` | Arayüz gizlemesi tek başına güvence değil |
| D3 | Geri doldurma varsayılanı **son 2 gün**, en çok 30; onay bekleyen/reddedilen satışlar alınmaz; idempotent | Modülü açan firmada kuyruk eski siparişlerle dolmasın |
| D4 | Telefonda depo **eylemleri çevrimiçi** (başla/paketle/yükle/geri al); liste Room önbelleğinden çevrimdışı okunur | Çakışma (409) ve sıra sunucuda çözülür |
| D5 | `wms_orders`/`wms_order_items` tablolarına ve onları dolduran yazımlara dokunulmaz; Depo ekranı artık onları listelemez | `InvoiceDetailLoader` fatura kalemlerini bu tablolardan okuyor |
| D6 | Stok ve cari listelerinin **mevcut** uçları (`/portal/stock`, `/portal/balances`) sözleşmesi bozulmaz; yeni sayfalı uçlar eklenir | Yalnızca ekleme kuralı |
| D7 | Ekstre varsayılan aralığı **yılbaşından bugüne**; devir = aralık öncesi hareketlerin toplamı | Muhasebe alışkanlığı |
| D8 | Liste filtre/sıralama/sayfa durumu **URL sorgusunda** tutulur (yenile, geri, bağlantı paylaşma korunur) | Panel kullanımında beklenen davranış |

---

## 1. Yetkiler (kullanıcı, 2026-09-16)

| Yetki | Karar |
|---|---|
| Dala push, PR açma | **Serbest** |
| CI yeşil + inceleme yorumları çözülmüşse `main`'e squash-merge + dalı silme | **Serbest** (ErpBridge'de `main` → Coolify otomatik canlı dağıtım) |
| Play **internal** kanalına sürüm yükleme | **Serbest** |
| İnsan gerektiren madde | Yapılabilen kısmı yapılır, kalanı `GOAL_PANEL_DURUM.md` > "Seni Bekleyenler"e yazılır |

Bu yetkiler **yalnızca bu belgedeki görevler** için geçerlidir. Kapsam dışı iş çıkarsa sorulur.

**Asla:** `--force` push, CI kırmızıyken merge, `main`'e doğrudan push (belge dahil — otomatik kip
engelliyor), Play production yayını, veritabanı/tablo silme, mevcut tabloyu değiştiren migration,
başka oturumun commit edilmemiş değişikliğine dokunma, `git stash pop`.

---

## 2. Çalışma kuralları

### Eşzamanlı oturumlar — ÖNEMLİ
Bu goal yazılırken başka oturumlar çalışıyordu: ErpBridge ana klasörü (`2026/ErpBridge`) başka dallarda
kullanılıyor, Sipariş Cepte ana klasöründe **Faz 0 goal'ü** (`docs/GOAL_FAZ0_TEMEL.md`) commit edilmemiş
değişikliklerle sürüyor. Bu yüzden:

- **ErpBridge işleri yalnız `2026/eb-panel` çalışma kopyasında** (git worktree) yapılır.
- **Sipariş Cepte işleri yalnız `2026/sc-depo` çalışma kopyasında** yapılır
  (yoksa: `git -C Siparis_Cepte fetch && git -C Siparis_Cepte worktree add ../sc-depo -b <dal> origin/main`).
- Ana klasörlerde `switch`, `checkout`, `stash`, `reset` yapılmaz.
- Room migration numarası dal açılırken **o anki `origin/main`'deki `AppDatabase` sürümünden** alınır;
  merge öncesi `origin/main` ilerlemişse rebase edilir ve numara yeniden kontrol edilir.
- `PLAN_ROLLER_VE_DEPO.md` Adım 5'i başka oturumda başlamışsa `Onaylar.razor` çakışabilir → rebase, iki davranış da korunur.

### Her görev
1. `git -C <worktree> fetch origin && git -C <worktree> switch -c panel-<id>-<kısa-ad> origin/main`
2. **Önce bilgi bankası** (`ErpBridge_knowledge_base/INDEX.md` → 00–04; mobil işte
   `Siparis_Cepte_knowledge_base/`). Şema, JSON alan adı, kolon **tahmin edilmez** — koddan ya da testten doğrulanır.
3. Kod + test. Davranış değişen her görevin testi olur (sunucu: xUnit/ilişkisel SQLite; panel: bUnit
   `tests/ErpBridge.Portal.Tests`; Android: birim test).
4. Yerel doğrulama (bölüm 3) yeşil; panel görevlerinde tarayıcıda kontrol (bölüm 3).
5. Türkçe commit: ne + neden. Sonuna `Co-Authored-By: Claude Opus 5 <noreply@anthropic.com>`.
6. `git push -u origin <dal>` → `gh pr create` (gövde sonunda `🤖 Generated with [Claude Code](https://claude.com/claude-code)`).
7. `gh pr checks <n> --watch`. Kırmızıysa düzelt; **3 denemede yeşillenmezse** ⛔ yazılır, sıradakine geçilir
   (bağımlı görevler de ⛔).
8. **Codex inceleme yorumları** merge'u bloklar: geçerli bulgu düzeltilir, geçersizse gerekçeyle yanıtlanır, konuşma çözülür.
9. **Merge'den önce** `GOAL_PANEL_DURUM.md` (görev ✅, PR numarası, not) ve gerekiyorsa KB aynı dala commit + push
   edilir, CI yeniden yeşil beklenir (ayrı belge PR'ı açılmaz).
10. `gh pr merge <n> --squash --delete-branch` ("main is already used by worktree" hatası merge'u bozmaz —
    `gh pr view <n> --json state` ile doğrula).
11. ErpBridge'de merge canlıya dağıtır: migration içeren görevden sonra
    `https://lisans.appsgo.cloud/health/schema` → `current` doğrulanır; değilse ⛔ ve "Seni Bekleyenler".

### Bozulmaz kurallar
- ErpBridge: mevcut tablolara ve mevcut uç sözleşmelerine dokunulmaz, **yalnızca ekleme**. Telefonun kullandığı
  `/api/v1/android/*` uçlarında varsayılan davranış değişmez (yeni parametre isteğe bağlı).
- `TreatWarningsAsErrors=true` → 0 uyarı.
- Her uç firmayı **token'dan** alır, izin `RolePermissions`'tan (`CanViewLedger`, `CanOperateWarehouse`,
  `CanManageWarehouse`, onaylarda `ApprovalService.Visible`).
- Büyük listeler: sayfa boyutu üst sınırı sunucuda (≤ 250); sıralama anahtarı beyaz listeden.
- Android: `.fallbackToDestructiveMigration()` YASAK, açık `Migration(x, y)` + `DatabaseProvider.addMigrations`;
  yazımlar `DataWriteCoordinator` üzerinden; ekranda `AppColors`/`Spacing`/`Sizes`/`AppType`; `LazyColumn` `key`.

---

## 3. Doğrulama

### ErpBridge (worktree kökünde)
```bash
dotnet build ErpBridge.sln -v q --nologo
dotnet test ErpBridge.sln --nologo
```

### Panelde tarayıcı kontrolü
Kök `2026/.claude/launch.json`'daki `erpbridge-centralapi-local` (5181, Test ortamı, bellek içi DB) ve
`erpbridge-portal-local` (5195, `--environment Development`) **`ErpBridge/` ana klasörünü** çalıştırır.
Bu goal için P0b'de aynı yapılandırmaların `eb-panel` yollu kopyaları (`eb-panel-centralapi-local`,
`eb-panel-portal-local`, farklı port) eklenir. Veri admin API ile tohumlanır. Tarayıcı bölmesinde Enter formu
göndermez — düğmeye tıklanır. Dar ekran kontrolü `resize_window` 400×800.

### Sipariş Cepte (`sc-depo` kökünde)
```bash
./gradlew :app:compileDebugKotlin :app:testDebugUnitTest
```
CI: `testDebugUnitTest` + `assembleDebug`. Sürüm: `fastlane android bump_version_code_from_play` →
`bundleRelease` → `python scripts/check_16kb.py <aab>` → `fastlane android internal_upload_only`
(ayrıntı `Siparis_Cepte/docs/GOAL_YOL_HARITASI.md` bölüm 2; anahtar `~/.playstore/play-fastlane-key.json`).

---

## 4. Görevler

**[O]** otonom · **[K]** insan kapısı · **[O→K]** yapılabilen kısmı otonom, kalanı kapı.
Depo: **EB** ErpBridge · **SC** Sipariş Cepte.

### P0 — Hazırlık

| ID | Görev | Tip | Depo | Bitti ölçütü |
|---|---|---|---|---|
| **P0a** | Bu plan dalını (`docs/goal-panel-gelistirmeleri`) push et, PR aç, CI yeşilse merge | [O] | EB | Plan `main`'de |
| **P0b** | Zemin: `main`'de build + test sayıları; `eb-panel-*` launch yapılandırmaları; yerelde panel açılıyor | [O] | EB | DURUM'da zemin satırı |

### P1 — Onaylar

| ID | Görev | Tip | Depo | Bitti ölçütü |
|---|---|---|---|---|
| **P1a** | Sunucu: onay listesine isteğe bağlı `beforeSeq` (sayfalama, `RequestedSeq < beforeSeq`) ve `kind` filtresi. Parametresiz çağrı bugünküyle aynı | [O] | EB | Sözleşme testi: eski çağrı aynı sonuç; sayfalama testi |
| **P1b** | Panel: sekmeler **Bekleyen · Onaylanan · Reddedilen · Tümü** (sayılar `/approvals/summary`'den), tür filtresi, "Daha fazla yükle". Kararlı kartta karar veren, zaman, not; geri çekilen/yeniden gönderilen durum rozetleri | [O] | EB | bUnit: sekme değişince doğru `status` çağrılıyor, kararlı kartta düğme yok |
| **P1c** | Panel: kart tıklanınca **detay çekmecesi** (masaüstünde sağdan, dar ekranda tam ekran): başlık (tür, müşteri, tutar, plasiyer, istek zamanı), **belge satırları tablosu** (satış/iade/alış: kod, ad, miktar, birim, fiyat, iskonto, KDV, tutar; tahsilat/tediye: ödeme tipi, tutar, vade, açıklama), stok uyarıları, olay zaman çizelgesi (kim/ne/ne zaman/not). Bekleyen talepte not + Onayla/Reddet çekmecede de var. `/onaylar?talep={id}` ile doğrudan açılır. `DocumentsJson` şeması `ApprovalService` + telefon gönderim kodundan doğrulanır; tanınmayan tür için anahtar/değer görünümü | [O] | EB | bUnit: detay yükleniyor, satırlar doğru; tarayıcıda satış + tahsilat detayı ekran görüntüsüyle doğrulandı |

### P2 — Stok

| ID | Görev | Tip | Depo | Bitti ölçütü |
|---|---|---|---|---|
| **P2a** | Sunucu: `GET /api/v1/portal/stock/search` — parametreler: `q` (kod/ad/**barkod**), `mainGroups`, `subGroups`, `brands`, `shelves` (çoklu), `warehouse`, `priceList`, `minQty`/`maxQty`, `minPrice`/`maxPrice`, `status` (`all\|in\|out\|negative\|below`), `below` (eşik), `idleDays` (son hareket X günden eski), `sort` (`code\|name\|qty\|price\|group\|brand\|lastMovement`), `dir`, `page`, `pageSize` (25/50/100/250). Yanıt: `items`, `total`, `page`, `pageSize`, **filtre sonucunun tamamı** için özet (ürün, stokta, tükenen, eksi). Satır: kod, ad, birim, ana/alt grup, marka, reyon, barkodlar, toplam + rezerve miktar, depo bazlı miktarlar, seçili liste fiyatı, son hareket tarihi. ERP (`mainGroupCode`/`brandCode`) ve ERP'siz (`kategori`/`marka`) alan farkları **tek normalize noktasında**; `MobileEntityAssembler.BuildProduct` mantığı yeniden kullanılır, kopyalanmaz. Firma başına izdüşüm önbelleği **yalnız stok verisi değişince** geçersizlenir: `stocks`/`inventory`/`prices`/`barcodes` kayıtlarının en yüksek sırası ya da güncelleme zamanı anahtar olur (`tenant_sync_counter` depo olaylarında da arttığı için kullanılmaz) | [O] | EB | Her filtre/sıralama için test (ERP + ERP'siz veri); 20.000 ürünlük tohumla yerelde < 1 sn (süre DURUM'a) |
| **P2b** | Sunucu: `GET /api/v1/portal/stock/facets` — gruplar, alt gruplar, markalar, reyonlar (sayılarıyla), depolar, fiyat listeleri | [O] | EB | Test |
| **P2c** | Panel `/stok`: sunucu verili tablo, sayfalama (sayfa boyutu seçimi, toplam kayıt), **kolon başlığına tıklayarak sıralama**, açılır **filtre paneli** (çoklu seçim grup/alt grup/marka/reyon, depo, fiyat listesi, miktar ve fiyat aralığı, durum, hareketsizlik günü), etkin filtre çipleri + "temizle", arama kutusu barkodu da bulur, depo kolonlarını göster/gizle, satır açılınca depo bazlı miktar + tüm liste fiyatları + barkodlar. Özet kartlar filtre sonucunun tamamını yansıtır. Durum URL'de (D8). 200 sınırı kalkar | [O] | EB | bUnit: filtre → doğru sorgu, sayfa değişimi; tarayıcıda 1000+ ürünle sayfalama/sıralama/filtre ekran görüntüsü; 400 px'de kullanılabilir |

### P3 — Cariler

| ID | Görev | Tip | Depo | Bitti ölçütü |
|---|---|---|---|---|
| **P3a** | Sunucu: `GET /api/v1/portal/customers` — sayfalı cari listesi: `q`, `balance` (`all\|receivable\|payable\|nonzero`), `sort` (`code\|title\|balance`), `dir`, `page`, `pageSize`; özet (toplam alacak/borç) filtre sonucunun tamamından | [O] | EB | Test |
| **P3b** | Sunucu: `GET /api/v1/portal/customers/{code}` (kart: kod, unvan, bakiye, mevcutsa telefon/adres/plasiyer — alanlar `customers` kaydından doğrulanır) ve `GET …/{code}/ledger?from&to&types&page&pageSize` — **ekstre**: devir bakiyesi (D7), tarih sıralı satırlar (tarih, belge türü, evrak no, açıklama, borç, alacak, yürüyen bakiye), faturada `documentKey`. Kaynak `customerTransactions`; ERP (`tutar`+`borcMu`) ve ERP'siz alan adları (`NativeDocumentProcessor`'dan doğrulanır) tek normalize fonksiyonunda | [O] | EB | Test: devir + satırlar toplamı = kart bakiyesi (ERP ve ERP'siz); sayfa sınırında yürüyen bakiye doğru |
| **P3c** | Sunucu: `GET …/{code}/documents/{documentKey}` — fatura başlığı + kalemler (kod, ad, miktar, birim fiyat, tutar, depo). ERP: `cha_recno` → `stockTransactions.faturaRecno` (`/sync/faturaHareket` sorgusu paylaşılan yardımcıya taşınır). ERP'siz: `evrakNo`+`cariKod` eşleşmesi, yoksa ilgili `jobs` payload'u. Kalem bulunamazsa boş liste + `linesAvailable=false` | [O] | EB | Test: ERP faturası, ERP'siz satış, kalemsiz hareket |
| **P3d** | Panel: `/cariler` sayfalı tablo (sıralama, bakiye filtresi, "sıfır bakiyelileri de göster"), **satır tıklanır** → `/cariler/{kod}`: üst kart, tarih aralığı + tür filtresi, ekstre tablosu (sayfalı, devir satırı), fatura satırına tıklanınca **kalem çekmecesi**. Geri dönüşte liste durumu korunur (D8) | [O] | EB | bUnit + tarayıcıda ERP'li ve ERP'siz tohumla ekran görüntüsü |
| **P3e** | Bulgu doğrulama: telefon ekstresi `tutar` okuyor (`CustomerDetailLoader.kt:77`), ERP'siz hareketler tutarı başka alanda yazıyor olabilir. **Yalnız doğrula**; doğruysa DURUM > "Bulgular"a yaz (bu goal'de düzeltilmez) | [O] | SC (salt okuma) | DURUM'da sonuç |

### P4 — Depo: sunucu ve panel (PLAN_ROLLER_VE_DEPO Adım 6)

| ID | Görev | Tip | Depo | Bitti ölçütü |
|---|---|---|---|---|
| **P4a** | Sunucu: `POST /api/v1/portal/warehouse/backfill {days}` (`CanManageWarehouse`, 0–30, D3) — son N günün `sales_order` job'larını `FulfillmentService` üzerinden idempotent kuyruğa alır; `QueuedAtUtc` = siparişin geliş zamanı; ERP'li firmada `ErpState` job durumundan (başarılı → `WRITTEN`, hata → `FAILED`). Modül kapalıysa 409 | [O] | EB | İlişkisel test: iki çağrı tek kayıt; onay bekleyen/reddedilen alınmıyor; ERP ve ERP'siz |
| **P4b** | Sunucu (K4, D1, D2): `RolePermissions.CanUsePhone` DEPO'yu kapsar; yalnız DEPO rolü olan kullanıcı için `versionCode ≥ Mobile:MinWarehousePhoneVersionCode` şartı (ayar boş = reddet) **girişte ve her telefon isteğinde** (`MobileUserAccess` yetki kontrolü, cihazın kayıtlı sürümüyle) — rolleri sonradan daralan açık oturum da kapsanır. Girişte gönderilen sürüm alanı koddan doğrulanır. Yalnız DEPO rollü kullanıcının para/satış belgesi ingest'i 403 `ROLE_NOT_ALLOWED`. Session'daki `roles` telefonda okunabilir halde. `PLAN_ROLLER_VE_DEPO.md` V4 satırı ve KB kuralı güncellenir | [O] | EB | Testler: eski sürüm reddediliyor, yeni sürüm giriyor, SALES rolü kaldırılan eski sürümlü açık oturumun sonraki isteği reddediliyor, ingest 403, çok rollü kullanıcı etkilenmiyor |
| **P4c** | Panel: **depo ayarları** (ADMIN/MANAGER): modülü aç/kapat; açarken "son kaç günün siparişi kuyruğa alınsın?" (varsayılan 2) → backfill; eşik süreleri. Modül kapalıyken `/depo` yöneticiye "Depo modülü kapalı — Aç", depocuya "yöneticinize başvurun" gösterir | [O] | EB | bUnit: aç → settings PUT + backfill çağrısı |
| **P4d** | Panel `/depo`: sekmeler **Bekleyenler · Hazırlananlar** (benim / tümü) **· Paketlenenler · Bugün yüklenenler**; kart: sipariş no, müşteri, plasiyer, kalem sayısı, tutar, canlı bekleme süresi (eşik rengi), hazırlayan, ERP rozeti. Kart tıklanınca detay: toplama listesi (işaretleme yalnız tarayıcıda), olay geçmişi. Büyük düğmeler: **Başla → Paketlendi → Araca yüklendi** (plaka isteğe bağlı), **Geri al** (V6: 5 dk / yönetici), yöneticiye **İptal** ve **Yeniden ata**. 409'da "X başladı" mesajı ve liste yenilenir. Canlı güncelleme `/api/v1/portal/events` long-poll (≤ 2 sn), koparsa 15 sn yoklama | [O] | EB | bUnit + tarayıcıda iki sekmeyle: birinde "Başla", diğerinde ≤ 2 sn'de taşınıyor; 400 px'de kullanılabilir |

### P5 — Telefon Depo ekranı sunucu kuyruğuna

| ID | Görev | Tip | Depo | Bitti ölçütü |
|---|---|---|---|---|
| **P5a** | API + önbellek: fulfillments liste/detay/eylem/events çağrıları (mevcut `FieldOpsApiService` / `ApiClient` deseni); yeni Room tablosu `warehouse_fulfillments` (+ kalemler JSON) açık migration ile; `wms_*` tablolarına dokunulmaz (D5) | [O] | SC | Migration testi; API eşleme birim testi |
| **P5b** | `WarehouseScreen`/`WarehouseViewModel` sunucu kuyruğunu okur: sekmeler **Bekleyen · Hazırlanıyor · Paketlendi · Yüklendi**; eylemler çevrimiçi (D4; çevrimdışıyken düğme kapalı + açıklama), 409 mesajı, toplama işaretleme cihazda, ekran açıkken canlı yenileme. **Kaldırılır:** `seedMockOrders`, sahte WMS müşterileri, taklit `syncAll`. Modül kapalı firmada "Depo modülü kapalı" boş durumu. Mevcut koli barkodu/yazdırma "Paketlendi" adımına bağlanabiliyorsa korunur, bağlanamıyorsa DURUM'a not | [O] | SC | ViewModel testleri: durum eşlemesi, çevrimdışı, 409; demo verisi oluşmuyor |
| **P5c** | Yalnız DEPO rolü olan kullanıcı (K4): girişten sonra doğrudan Depo ekranı; menüde yalnız Depo + Ayarlar/Çıkış; satış, tahsilat, cari, rota rotaları kapalı (deep link dahil). `ROLE_NOT_ALLOWED_ON_PHONE` ve `ROLE_NOT_ALLOWED` Türkçe metinleri | [O] | SC | Navigasyon koruması testi |
| **P5d** | Sürüm artır → AAB → 16 KB denetimi → **internal** yükleme (K5). Yeni `versionCode` EB'de `Mobile:MinWarehousePhoneVersionCode` varsayılanına yazılır (ayrı küçük EB PR'ı) | [O] | SC+EB | `fastlane android check` → internal'da yeni sürüm; EB ayarı `main`'de |
| **P5e** | Gerçek cihazda uçtan uca: telefondan satış → panelde onay → telefonda depocu "Başla" → panelde ≤ 2 sn'de görünür → panelde "Paketlendi" → telefonda görünür | [K] | — | Seni Bekleyenler |

### P6 — Kapanış

| ID | Görev | Tip | Depo | Bitti ölçütü |
|---|---|---|---|---|
| **P6a** | KB ve sözleşme belgeleri: `ErpBridge_knowledge_base/03_…` (yeni uçlar, ekstre/fatura eşleme kuralı, geri doldurma, K4/D1/D2), `docs/api-contracts.md`, `Siparis_Cepte_knowledge_base` (Depo ekranı artık sunucuda). Gerekiyorsa son görevin PR'ına eklenir | [O] | EB+SC | Belgeler kodu yansıtıyor |
| **P6b** | Canlı kontrol: `/health/schema` current; firmada depo modülünü açma, gerçek verilerle panel ekranlarını gözle kontrol | [O→K] | — | Otonom kısım DURUM'da; gözle kontrol "Seni Bekleyenler"de |
| **P6c** | `PLAN_ROLLER_VE_DEPO.md` §8 ilerleme tablosu (Adım 6 ✅ bu goal ile; V4 değişikliği notu), DURUM özeti | [O] | EB | Tablo güncel |

**Sıra:** P0 → P1 → P2 → P3 → P4 → P5 → P6. Bağımsız bir görev ⛔ olursa sonraki fazlara geçilir;
P4a/P4b ⛔ ise P5 başlamaz.

---

## 5. Riskler

| Risk | Önlem |
|---|---|
| Başka oturumlarla aynı dosyalar (`Onaylar.razor`, Room sürümü, `SyncManager`) | Ayrı worktree'ler, dal açarken güncel `origin/main`, merge öncesi rebase |
| Merge = canlı dağıtım; bozuk sürüm canlıya çıkar | CI yeşil + inceleme yorumları çözülmeden merge yok; migration sonrası `/health/schema` |
| Stok izdüşümü büyük katalogda yavaş | Senkron sayacıyla önbellek, ölçüm DURUM'a; eşik aşılırsa ⛔ değil not + iyileştirme |
| ERP'siz firmada fatura kalemi eşleşmesi eksik | `linesAvailable=false` ile dürüst boş durum; bulgu DURUM'a |
| Eski telefon sürümünde depocu tüm ekranları görür | D1 sürüm kapısı + D2 sunucu reddi |
| Telefon Depo ekranında yerel satışlar artık görünmez | Bilinçli (K1); modül kapalı firmada açıklayıcı boş durum |

## 6. Kapsam dışı
Muhasebe klavye ekranı (Adım 5) · TV ekranı (Adım 7) · performans raporu (Adım 8) · Excel/CSV dışa aktarma ·
barkodla toplama doğrulaması · telefonda ERP'siz ekstre tutar hatasının düzeltilmesi (yalnız P3e ile doğrulanır).
