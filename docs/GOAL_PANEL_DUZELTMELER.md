# Goal — Panel Düzeltmeleri (Çıkış · Tekrar onaya al · Stok hızı · Cari bakiyesi)

Tarih: 2026-09-24 · Kapsam: ErpBridge.Portal + ErpBridge.CentralApi + ErpBridge.Erp.Mikro (Sipariş Cepte'ye dokunulmaz)

Otonom çalışma içindir. İlerleme **[GOAL_PANEL_DUZELTMELER_DURUM.md](GOAL_PANEL_DUZELTMELER_DURUM.md)**'ye yazılır.
Çalışma kuralları, doğrulama ve yasaklar [GOAL_PANEL_GELISTIRMELERI.md](GOAL_PANEL_GELISTIRMELERI.md) §1–3 ile aynıdır.

## Kullanıcının kararları (2026-09-24)
- Çıkış: "tıklıyorum ama çıkmıyor" → gerçek hata, düzeltilecek.
- Bakiye sorunu **Mikro (ERP'li) firmada**; panel Sipariş Cepte'nin formülünü kullanacak. ERP'siz firmada panel zaten doğru, dokunulmaz.
- Kök neden (Mikro okuyucusu) de düzeltilecek; ajan güncellemesi gerekir.
- Push, PR, CI yeşil + yorumlar çözülünce squash-merge önceden onaylı (bkz. CLAUDE.md §2).

## Bulgular (inceleme, 2026-09-24)
Kullanıcı panelde 4 sorun bildirdi: (1) "Çıkış yap"a tıklayınca çıkış olmuyor, (2) reddedilen siparişi
tekrar onaya gönderecek buton yok, (3) Stok sayfası geç açılıyor, (4) Cariler'de Mikro'lu firmanın bakiyeleri
yanlış. Otonom çalışılacak (kullanıcı: push/PR/CI-yeşil merge önceden onaylı). İnceleme bulguları:

- **Çıkış (kök neden doğrulandı, G1):** "Çıkış yap" `MainLayout.razor`'daki hesap menüsünde. MudBlazor 9'da özel
  `ActivatorContent` tıklamayı kendisi dinlemiyor — etkinleştirici `MenuContext.ToggleAsync`'i çağırmalı. Çağırmadığı için
  **menü hiç açılmıyordu**, "Çıkış yap" görünmüyordu (tarayıcıda `aria-expanded` hep `false`). Ek sağlamlaştırma: önce
  kayıtlı oturum silinir (hata olsa da devam), sonra `forceLoad` ile login'e gidilir. Sunucuda portal token'ı iptal eden uç
  yok (stateless JWT, 12 saat) — kapsam dışı.
- **Tekrar onaya al:** sunucuda `POST api/v1/android/approvals/{id}/reopen` var (`ApprovalService.ReopenAsync`
  323-333, Rejected→Pending, yalnız onaylayıcı, olay "Reopened"); telefon zaten kullanıyor ("Tekrar onaya al").
  Portal `Onaylar.razor` "Reddedilen" sekmesini listeliyor ama buton ve `PortalApiClient` metodu yok. Sunucu/telefon değişmez.
- **Stok:** `PortalStockCatalog.LoadAsync` (80-155) her derlemede `stockTransactions` geçmişinin tamamını
  (`PortalRecordMirror` → `[.. _items.Values]` kopyası) yalnız ürün başına "son hareket tarihi" için okuyor;
  tek yeni satış tüm kataloğu yeniden kuruyor; `search` + `facets` iki paralel çağrı aynı anda kurabiliyor;
  facets ve varsayılan sıralama her istekte yeniden hesaplanıyor.
- **Bakiye:** Panel `customers.balance` kart alanını gösteriyor (`PortalLedger.cs:163`, `PortalReports.BalancesAsync`
  247-276, ekstre çapası `PortalLedger.cs:297`). Mikro okuyucu (`MikroDbReader.cs:136-177`) artımlı okumada yalnız
  **kartı değişen** carinin bakiyesini gönderiyor → yeni fatura/tahsilat sonrası bakiye bayat. Sipariş Cepte:
  `Σ(borcMu ? +tutar : −tutar)` cari hareketleri (cariKod eşleşmesi, kapalı/peşin hariç), hareket yoksa kart bakiyesi
  (`AppDatabase.kt:209-226`, `CustomerDetailLoader.kt:73`). ERP'siz firmada panel zaten doğru (açılış bakiyesi) → dokunulmaz.

## G1 — Çıkış hatası
1. Hatayı yeniden üret: yerel CentralApi + Portal (`.claude/launch.json` yapılandırmaları), girişe hem "Beni hatırla"lı hem
   hatırlamasız, `#logout` tıkla, konsol/sunucu logu oku (`systematic-debugging` becerisi).
2. Düzeltme (kök nedene göre; beklenen şekli): `SignOutAsync` önce `Persistence.ClearAsync()` (try/catch — başarısızsa da
   devam), sonra `Nav.NavigateTo("login", forceLoad: true)` ile devre sıfırlanır; `Session.SignOut()` sıra/yan etkisi
   layout'u handler ortasında sökmeyecek şekilde düzenlenir. Login sayfası açıkken depoda oturum kalmadığı doğrulanır.
3. Test: `tests/ErpBridge.Portal.Tests` bUnit — `#logout` tıklanınca `ISessionPersistence.ClearAsync` çağrılır, oturum boşalır,
   login'e yönlenir; `ClearAsync` hata atsa da çıkış tamamlanır.

## G2 — Onaylar: "Tekrar onaya al"
- `src/ErpBridge.Portal/Api/PortalApiClient.cs`: `ReopenAsync(Guid id, string? note)` → `POST api/v1/android/approvals/{id}/reopen`
  (mevcut `DecideAsync` 184-185 deseni).
- `src/ErpBridge.Portal/Pages/Onaylar.razor`: `Status == "Rejected" && Session.CanApprove` iken listede ve detay çekmecesi
  altbilgisinde "Tekrar onaya al" butonu (isteğe bağlı not), sonuç sonrası liste yenilenir; `DecideAsync`'teki 409 kodları
  (`APPROVAL_ALREADY_DECIDED`/`APPROVAL_STATE_CHANGED`/`APPROVAL_NOT_FOUND`) aynı şekilde ele alınır. `Resubmitted` için buton yok
  (sunucu reddeder; düzeltilmiş talep zaten bekleyende).
- Test: `PortalManagementPagesTests` bUnit — reddedilende buton görünür, onaylayıcı olmayana görünmez, tıklayınca reopen
  çağrılır. Sunucu davranışı `ApprovalCentreRelationalTests` (satır 91) ile zaten kapsanıyor.

## G3 — Panel bakiyesi = Sipariş Cepte formülü (Mikro/ERP'li firma)
- `src/ErpBridge.CentralApi/Portal/PortalLedger.cs`: ERP'li firmada (`DataSource != native`) cari bakiyesi =
  `Σ(borç ? +tutar : −tutar)` — `customerTransactions` satırları, `cariKod` trim + büyük/küçük harf duyarsız eşleşme,
  `kapali` hariç; carinin hiç hareketi yoksa kart `balance`. Hareketler mevcut `MovementsAsync` satır aynasından
  (tek geçişte `Dictionary<cariKod, decimal>`, katalog sürümüyle önbelleklenir — G5 ile aynı "tüm geçmişi kopyalama" tuzağına
  düşmeden).
- Aynı hesap `CustomersAsync`/`Search` (liste + toplam kartları), `PortalReports.BalancesAsync` (özet/Muhasebe) ve ekstre
  çapasına (`Statement`, 297: ERP'li firmada açılış = aralık öncesi hareketlerin toplamı) uygulanır → liste ve ekstre aynı sayıyı verir.
- ERP'siz firma (`native`) kodu değişmez.
- Test: CentralApi ilişkisel testi — kart bakiyesi bayatken (ör. 100) hareketler 250 → liste/özet/ekstre 250; kapalı satır hariç;
  hareketsiz cari kart bakiyesini gösterir; native firma eski davranış.
- KB: `ErpBridge_knowledge_base/03_Data_Dictionary_and_Rules.md`'ye "panel bakiyesi" kuralı eklenir.

## G4 — Mikro okuyucusu: hareketi değişen carinin bakiyesini yeniden gönder
- `src/ErpBridge.Erp.Mikro/Readers/MikroDbReader.cs:136-177`: artımlı okumada müşteri seti = kartı değişenler **∪**
  `CARI_HESAP_HAREKETLERI`'nde `cha_lastup_date`/`cha_create_date` imleçten yeni olan (`cha_cari_cins=0`) `cha_kod`'lar;
  bakiye alt sorgusu bu setin tamamı için (tam geçmiş, `cha_iptal=0`) hesaplanır. Kolon adları KB 03 + `Fora_Mikro/` ile doğrulanır.
  Logo adaptöründe benzer desen varsa kontrol edilir, varsa not düşülür (ayrı iş).
- Test: birim (SQL üretimi) + `ERPBridge_RUN_INTEGRATION=1` ile yalnız `MikroDB_V15_DEMO`'da: cari kartı değişmeden hareket ekle →
  artımlı okuma o carinin yeni bakiyesini döndürür. **`MikroDB_V15_02`'ye yazılmaz.**
- DURUM "Seni Bekleyenler": müşteri makinelerinde ajan güncellemesi gerekir.

## G5 — Stok sayfası hızı
- `PortalStockCatalog.LoadAsync`: satır aynasının tüm listesini kopyalayıp yürümek yerine, ayna uygularken ürün başına
  "son hareket günü" haritasını artımlı tutar (yeni satır → yalnız o ürünün tarihi güncellenir; katalog yalnız stok kısmı
  değişince baştan kurulur, hareket değişince yalnız tarih haritası birleşir).
- Kiracı başına tek derleme (`SemaphoreSlim`/`Lazy<Task>` single-flight) → paralel `search` + `facets` aynı derlemeyi bekler.
- Facets, varsayılan sıralı dizi ve varsayılan fiyat listesi katalog sürümüyle birlikte önbelleklenir.
- `Stok.razor:539-543` fazladan ikinci aramayı sunucu tarafında sayfa kırpmasıyla kaldır (sunucu son geçerli sayfayı döner) — yalnız
  sözleşmeyi bozmuyorsa.
- (`TenantId, Entity, UpdatedSeq`) indeksi: büyük tabloda kilit riski nedeniyle bu goal'de eklenmez; DURUM "Seni Bekleyenler"e
  canlıda `EXPLAIN ANALYZE` önerisiyle yazılır.
- Test: `PortalStockSearchRelationalTests` davranış korunur (mevcut testler yeşil) + yeni: tek yeni hareket sonrası sonuçta
  son-hareket tarihi güncellenir ve katalog yeniden kurulmaz; eşzamanlı iki çağrı tek derleme. Ölçüm: DURUM'daki
  20k ürün / 200k hareket SQLite senaryosu tekrar ölçülüp önce/sonra DURUM'a yazılır.

## Doğrulama (uçtan uca)
- `dotnet build ErpBridge.sln -c Debug` (0 uyarı/0 hata), `dotnet test ErpBridge.sln`.
- Tarayıcı bölmesi: yerel CentralApi + Portal; tohum veriyle (a) çıkış → login, geri tuşu/yenile ile panele dönülmez;
  (b) talep reddet → "Reddedilen"de "Tekrar onaya al" → "Bekleyen"e geçer, olay geçmişinde "Yeniden açıldı";
  (c) Cariler bakiyesi ekstre son bakiyesiyle aynı; (d) Stok ilk ve ikinci açılış süresi; dar ekran 400×800.
- G4 için entegrasyon testi yalnız `MikroDB_V15_DEMO`.
- Her merge sonrası (migration yok) Coolify dağıtımı; DURUM belgesi her görevde güncel.
