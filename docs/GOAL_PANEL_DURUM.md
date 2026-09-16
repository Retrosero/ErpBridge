# Goal Durumu — Yönetim Paneli Geliştirmeleri

Son güncelleme: 2026-09-16 (P3 bitti, sırada P4a)
Görev listesi: [GOAL_PANEL_GELISTIRMELERI.md](GOAL_PANEL_GELISTIRMELERI.md)

> **Her görevden sonra, o görevin PR'ı içinde güncellenir.** Oturum kapanırsa buradan devam edilir.
> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı (kapı) · ⛔ tıkandı

---

## Özet

| Faz | Görev | Biten | Durum |
|---|---|---|---|
| P0 — Hazırlık | 2 | 2 | ✅ |
| P1 — Onaylar | 3 | 3 | ✅ |
| P2 — Stok | 3 | 3 | ✅ |
| P3 — Cariler | 5 | 5 | ✅ |
| P4 — Depo sunucu + panel | 4 | 0 | ⬜ |
| P5 — Telefon Depo ekranı | 5 | 0 | ⬜ |
| P6 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** P4a

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| P0a | Plan dalını main'e al | ✅ | [#54](https://github.com/Retrosero/ErpBridge/pull/54) | Codex 3 bulgu, üçü de planda düzeltildi: DURUM güncellemesi merge'den önce; P4b sürüm kapısı her istekte; stok önbelleği yalnız stok verisiyle geçersizlenir |
| P0b | Zemin + launch yapılandırmaları | ✅ | [#54](https://github.com/Retrosero/ErpBridge/pull/54) | `eb-panel-centralapi-local` (5281) + `eb-panel-portal-local` (5295) eklendi; yerelde giriş sayfası açılıyor |
| P1a | Onay listesi sayfalama + tür filtresi (sunucu) | ✅ | [#56](https://github.com/Retrosero/ErpBridge/pull/56) | `beforeSeq` + `beforeExternalId` imleci (Codex: aynı ms'de gelen talepler aynı `requestedSeq`'i alır → eşitlikte `externalId` bağı çözer) + `kind` (bilinmeyen → 400 `INVALID_KIND`); DTO'ya `requestedSeq` eklendi (sayfalama imleci için, yalnız ekleme). Sözleşme testi: parametresiz liste aynı. CentralApi onay testleri 19/19 |
| P1b | Onaylar sekmeleri | ✅ | [#58](https://github.com/Retrosero/ErpBridge/pull/58) | P1c ile tek PR (aynı sayfa). Sekmeler Bekleyen/Onaylanan/Reddedilen (`rejected,resubmitted`)/Tümü, tür filtresi (yerel `select`), 50'lik sayfa + "Daha fazla yükle" (`beforeSeq`+`beforeExternalId`), bekleyen sayısı rozeti (`/approvals/summary`, hata olursa yalnız rozet gizlenir). Sekme/tür/açık talep URL'de (`durum`, `tur`, `talep`) |
| P1c | Onay detay çekmecesi | ✅ | [#58](https://github.com/Retrosero/ErpBridge/pull/58) | Yeniden kullanılabilir `Shared/DetailSheet` (sağdan panel, dar ekranda tam ekran, Esc/arka plan kapatır). `Api/ApprovalDocuments` belge JSON'unu başlık alanları + satır tablosu + "Diğer bilgiler"e çevirir (tanınmayan alan düşmez), tarih `dd.MM.yyyy HH:mm`. Stok uyarısı, geçmiş zaman çizelgesi, çekmecede not + Onayla/Reddet; satırda varsa iskonto/KDV/iade durum oranı kolonları. Codex: geç gelen eski detay yanıtı yok sayılır, iskonto/KDV eklendi. bUnit 79→87. Tarayıcıda (yerel) bekleyen detay, reddedilen sekmesi, 400 px doğrulandı. **Yerel kısıt:** bellek içi DB'de onay kaydı yapılamıyor ("no known customer") — ilişkisel testler yeşil, yerel tohumda yalnız reddetme kullanıldı |
| P2a | Stok arama ucu (sayfalı, filtreli) | ✅ | [#60](https://github.com/Retrosero/ErpBridge/pull/60) | P2a+b+c tek PR. `Portal/PortalStockCatalog` + `GET /portal/stock/search`; önbellek anahtarı stok varlıklarının `MAX(UpdatedSeq)`+sayısı (#54 Codex bulgusu). 3 ilişkisel test (ERP alanları, hatalı sorgu/yetki, ERP'siz kart + yeni kart/satış/silme anında görünür). **Codex (#60):** Mikro okuyucusu depo bazlı miktar, rezerve ve son hareket üretmiyor → depo seçeneği yalnız envanterde >1 depo varken, son hareket `stockTransactions`'tan türetildi; tam yeniden okuma yerine artımlı **kayıt aynası** (`PortalRecordMirror`). **Ölçüm (SQLite, 20.000 ürün + 200.000 hareket):** ilk 2,4 sn, değişmemiş 0,17 sn, tek satış sonrası 0,35 sn, ~130 MB |
| P2b | Stok filtre seçenekleri ucu | ✅ | [#60](https://github.com/Retrosero/ErpBridge/pull/60) | `GET /portal/stock/facets`: grup/alt grup (üst grubuyla)/marka/reyon sayılarıyla, depo ve fiyat listesi adları, `hasMovementDates`, `hasReserved` |
| P2c | Stok sayfası | ✅ | [#60](https://github.com/Retrosero/ErpBridge/pull/60) | Sunucu sayfalı tablo (25/50/100/250; ilk/önceki/sonraki/son), kolon başlığıyla sıralama (sayılarda ilk tık azalan), durum sekmeleri (Tümü/Stokta/Tükenen/Eksi/Eşik altı + eşik), sağdan filtre paneli (depo, fiyat listesi, miktar/fiyat aralığı, hareketsizlik — yalnız ERP; aramalı çoklu seçim grup/alt grup/marka/reyon), etkin filtre çipleri, depo kolonları anahtarı, satır açılınca depo/fiyat/barkod. Durum URL'de (`Api/StockFilter`). 200 sınırı kalktı. bUnit +9. Yerelde Docker PostgreSQL ile 133 ürünlük tohumda filtre/sıralama/URL doğrulandı |
| P3a | Sayfalı cari listesi ucu | ✅ | #PR | P3a–e tek PR. `GET /portal/customers` (kod/unvan/telefon araması, bakiye filtresi, sıralama, sayfa); toplamlar filtrenin tamamı. Kayıt aynası "customers" |
| P3b | Cari kartı + ekstre ucu | ✅ | #PR | **Sapma:** yol yerine sorgu (`/customers/card?code=`, `/customers/ledger?code=`) — Mikro kodu `/` içerebilir. **Sapma (D7):** devir = kart bakiyesi − aralıktaki hareketler (plan: aralık öncesi toplam); ERP'siz `openingBalance` hareket olarak yazılmadığı ve ayna eksik geçmiş taşıyabileceği için ekstre her zaman kart bakiyesinde biter. Türkçe tür adları tr-TR büyük harfle eşlenir (Invariant "Satış"ı eşlemiyordu, testte yakalandı) |
| P3c | Fatura detay ucu | ✅ | #PR | `GET /customers/document?code=&key=`; Mikro `r{recno}`, ERP'siz `d{cari}|{evrakNo}`. `jobs` payload yedeği gerekmedi: ERP'siz satış/iade/alış satırları her zaman `stockTransactions`'a yazılıyor; satırı olmayan hareket açılamaz görünür |
| P3d | Cariler + cari detay sayfaları | ✅ | #PR | `/cariler` sayfalı tablo (bakiye sekmeleri, başlıktan sıralama, satıra tıkla → `/cari?kod=…&liste=…`, geri dönüşte liste korunur). `/cari`: kart (bakiye, telefon, vergi, adres), tarih aralığı (varsayılan bu yıl; Bu yıl/Son 3 ay/Tümü), tür düğmeleri, devir/borç/alacak/dönem sonu, sayfalı ekstre, kalemli satıra tıklayınca çekmecede kalemler. Ortak `Shared/Pager`. bUnit +2 (eski bakiye testi değişti). Yerelde PostgreSQL'de onaylanan satışın ekstresi ve kalemi doğrulandı |
| P3e | Telefon ERP'siz ekstre tutarı bulgusu (doğrulama) | ✅ | #PR | **Hata yok:** telefon API hareketinde `tutar ?: meblag ?: cha_meblag ?: amount …` okuyor (`FieldOpsApiService.kt:506`) |
| P4a | Depo geri doldurma ucu | ⬜ | | |
| P4b | Depocu telefona girişi + sunucu kısıtları | ⬜ | | |
| P4c | Panel depo ayarları | ⬜ | | |
| P4d | Panel depo sayfası | ⬜ | | |
| P5a | Telefon: API + Room önbelleği | ⬜ | | |
| P5b | Telefon: Depo ekranı sunucu kuyruğuna | ⬜ | | |
| P5c | Telefon: yalnız depo rolü arayüzü | ⬜ | | |
| P5d | Sürüm + internal yükleme + EB sürüm ayarı | ⬜ | | |
| P5e | Cihazda uçtan uca | ⬜ | | [K] |
| P6a | KB ve sözleşme belgeleri | ⬜ | | |
| P6b | Canlı kontrol | ⬜ | | [O→K] |
| P6c | Plan tablosu + özet | ⬜ | | |

---

## Zemin

`main` @ 33298cb (Faz 47 dahil), 2026-09-16:
- `dotnet build`: **0 uyarı, 0 hata, 40 sn**
- `dotnet test` (~105 sn): CentralApi **392**, Portal **79**, Core 100, Mikro 188 (+16 atlanan entegrasyon), Erp.Sql 64, LocalStore 60, Admin 41, RemoteApi 39, Shared 22, Agent.Service 15 — başarısız 0
- Yerel build bazı projelerin `packages.lock.json` dosyalarını değiştiriyor (Windows yan etkisi) → commit'e **alınmaz**, `git checkout --` ile geri alınır

## Karar günlüğü

- **2026-09-17 — Yerel doğrulama PostgreSQL ile:** Test ortamının bellek içi DB'si ERP'siz belge kayıtlarını (`mobile_records`) yazamıyor (onay "no known customer", stok boş). Docker Desktop açılıp `eb-panel-pg` (postgres:16-alpine, port 55433, yalnız yerel) kuruldu, `--migrate` uygulandı; kök `.claude/launch.json` → `eb-panel-centralapi-pg`. Tohum betiği oturum scratchpad'inde (`seed_panel.py`).
- **2026-09-17 — Main derleme kırığı (#59):** #57 (başka oturum, muhasebe onay masası) ile #58 aynı model sınıflarını ekledi, metin çakışması olmadığı için ikisi de birleşti ve main derlenmedi. Çift kopyalar #59 ile silindi (CI yeşil, merge). Çalışma kuralı 9 güncellendi: merge'den hemen önce rebase + yerel build.

## Bulgular

- **Mikro depo bazlı stok yok (2026-09-17, Codex #60):** `MikroDbReader.ReadInventoryAsync` `STOK_HAREKETTEN_ELDEKI_MIKTAR_VIEW`'den firma toplamını ajanın depo numarasıyla gönderiyor; `ReservedQuantity = 0`, `LastMovementDate = NULL` sabit. Telefonda "Depo N" kırılımı da bu yüzden tek satır. Gerçek depo bazlı miktar için ajan okuyucusunun depo kırılımlı bir kaynağa geçmesi gerekir (ajan müşteride kurulu → ayrı iş, kullanıcı kararı).

## Seni Bekleyenler

_(İnsan kapıları; goal bitince burada toplanır)_
