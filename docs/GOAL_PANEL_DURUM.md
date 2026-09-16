# Goal Durumu — Yönetim Paneli Geliştirmeleri

Son güncelleme: 2026-09-16 (P2 bitti, sırada P3a)
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
| P3 — Cariler | 5 | 0 | ⬜ |
| P4 — Depo sunucu + panel | 4 | 0 | ⬜ |
| P5 — Telefon Depo ekranı | 5 | 0 | ⬜ |
| P6 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** P3a

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| P0a | Plan dalını main'e al | ✅ | [#54](https://github.com/Retrosero/ErpBridge/pull/54) | Codex 3 bulgu, üçü de planda düzeltildi: DURUM güncellemesi merge'den önce; P4b sürüm kapısı her istekte; stok önbelleği yalnız stok verisiyle geçersizlenir |
| P0b | Zemin + launch yapılandırmaları | ✅ | [#54](https://github.com/Retrosero/ErpBridge/pull/54) | `eb-panel-centralapi-local` (5281) + `eb-panel-portal-local` (5295) eklendi; yerelde giriş sayfası açılıyor |
| P1a | Onay listesi sayfalama + tür filtresi (sunucu) | ✅ | [#56](https://github.com/Retrosero/ErpBridge/pull/56) | `beforeSeq` + `beforeExternalId` imleci (Codex: aynı ms'de gelen talepler aynı `requestedSeq`'i alır → eşitlikte `externalId` bağı çözer) + `kind` (bilinmeyen → 400 `INVALID_KIND`); DTO'ya `requestedSeq` eklendi (sayfalama imleci için, yalnız ekleme). Sözleşme testi: parametresiz liste aynı. CentralApi onay testleri 19/19 |
| P1b | Onaylar sekmeleri | ✅ | [#58](https://github.com/Retrosero/ErpBridge/pull/58) | P1c ile tek PR (aynı sayfa). Sekmeler Bekleyen/Onaylanan/Reddedilen (`rejected,resubmitted`)/Tümü, tür filtresi (yerel `select`), 50'lik sayfa + "Daha fazla yükle" (`beforeSeq`+`beforeExternalId`), bekleyen sayısı rozeti (`/approvals/summary`, hata olursa yalnız rozet gizlenir). Sekme/tür/açık talep URL'de (`durum`, `tur`, `talep`) |
| P1c | Onay detay çekmecesi | ✅ | [#58](https://github.com/Retrosero/ErpBridge/pull/58) | Yeniden kullanılabilir `Shared/DetailSheet` (sağdan panel, dar ekranda tam ekran, Esc/arka plan kapatır). `Api/ApprovalDocuments` belge JSON'unu başlık alanları + satır tablosu + "Diğer bilgiler"e çevirir (tanınmayan alan düşmez), tarih `dd.MM.yyyy HH:mm`. Stok uyarısı, geçmiş zaman çizelgesi, çekmecede not + Onayla/Reddet; satırda varsa iskonto/KDV/iade durum oranı kolonları. Codex: geç gelen eski detay yanıtı yok sayılır, iskonto/KDV eklendi. bUnit 79→87. Tarayıcıda (yerel) bekleyen detay, reddedilen sekmesi, 400 px doğrulandı. **Yerel kısıt:** bellek içi DB'de onay kaydı yapılamıyor ("no known customer") — ilişkisel testler yeşil, yerel tohumda yalnız reddetme kullanıldı |
| P2a | Stok arama ucu (sayfalı, filtreli) | ✅ | [#60](https://github.com/Retrosero/ErpBridge/pull/60) | P2a+b+c tek PR. `Portal/PortalStockCatalog` + `GET /portal/stock/search`; önbellek anahtarı stok varlıklarının `MAX(UpdatedSeq)`+sayısı (#54 Codex bulgusu). 3 ilişkisel test (ERP alanları, hatalı sorgu/yetki, ERP'siz kart + önbelleğin yeni kartı ve satışı görmesi). **Ölçüm (SQLite, 20.000 ürün × 4 kayıt):** ilk arama 714 ms, sonraki 86 ms, facets 76 ms |
| P2b | Stok filtre seçenekleri ucu | ✅ | [#60](https://github.com/Retrosero/ErpBridge/pull/60) | `GET /portal/stock/facets`: grup/alt grup (üst grubuyla)/marka/reyon sayılarıyla, depo ve fiyat listesi adları, `hasMovementDates`, `hasReserved` |
| P2c | Stok sayfası | ✅ | [#60](https://github.com/Retrosero/ErpBridge/pull/60) | Sunucu sayfalı tablo (25/50/100/250; ilk/önceki/sonraki/son), kolon başlığıyla sıralama (sayılarda ilk tık azalan), durum sekmeleri (Tümü/Stokta/Tükenen/Eksi/Eşik altı + eşik), sağdan filtre paneli (depo, fiyat listesi, miktar/fiyat aralığı, hareketsizlik — yalnız ERP; aramalı çoklu seçim grup/alt grup/marka/reyon), etkin filtre çipleri, depo kolonları anahtarı, satır açılınca depo/fiyat/barkod. Durum URL'de (`Api/StockFilter`). 200 sınırı kalktı. bUnit +9. Yerelde Docker PostgreSQL ile 133 ürünlük tohumda filtre/sıralama/URL doğrulandı |
| P3a | Sayfalı cari listesi ucu | ⬜ | | |
| P3b | Cari kartı + ekstre ucu | ⬜ | | |
| P3c | Fatura detay ucu | ⬜ | | |
| P3d | Cariler + cari detay sayfaları | ⬜ | | |
| P3e | Telefon ERP'siz ekstre tutarı bulgusu (doğrulama) | ⬜ | | |
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

_(Kapsam dışı ama not edilmesi gerekenler)_

## Seni Bekleyenler

_(İnsan kapıları; goal bitince burada toplanır)_
