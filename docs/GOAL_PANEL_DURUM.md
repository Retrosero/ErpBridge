# Goal Durumu — Yönetim Paneli Geliştirmeleri

Son güncelleme: 2026-09-16 (P1a bitti, sırada P1b)
Görev listesi: [GOAL_PANEL_GELISTIRMELERI.md](GOAL_PANEL_GELISTIRMELERI.md)

> **Her görevden sonra, o görevin PR'ı içinde güncellenir.** Oturum kapanırsa buradan devam edilir.
> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı (kapı) · ⛔ tıkandı

---

## Özet

| Faz | Görev | Biten | Durum |
|---|---|---|---|
| P0 — Hazırlık | 2 | 2 | ✅ |
| P1 — Onaylar | 3 | 1 | 🔄 |
| P2 — Stok | 3 | 0 | ⬜ |
| P3 — Cariler | 5 | 0 | ⬜ |
| P4 — Depo sunucu + panel | 4 | 0 | ⬜ |
| P5 — Telefon Depo ekranı | 5 | 0 | ⬜ |
| P6 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** P1b

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| P0a | Plan dalını main'e al | ✅ | [#54](https://github.com/Retrosero/ErpBridge/pull/54) | Codex 3 bulgu, üçü de planda düzeltildi: DURUM güncellemesi merge'den önce; P4b sürüm kapısı her istekte; stok önbelleği yalnız stok verisiyle geçersizlenir |
| P0b | Zemin + launch yapılandırmaları | ✅ | [#54](https://github.com/Retrosero/ErpBridge/pull/54) | `eb-panel-centralapi-local` (5281) + `eb-panel-portal-local` (5295) eklendi; yerelde giriş sayfası açılıyor |
| P1a | Onay listesi sayfalama + tür filtresi (sunucu) | ✅ | [#56](https://github.com/Retrosero/ErpBridge/pull/56) | `beforeSeq` + `beforeExternalId` imleci (Codex: aynı ms'de gelen talepler aynı `requestedSeq`'i alır → eşitlikte `externalId` bağı çözer) + `kind` (bilinmeyen → 400 `INVALID_KIND`); DTO'ya `requestedSeq` eklendi (sayfalama imleci için, yalnız ekleme). Sözleşme testi: parametresiz liste aynı. CentralApi onay testleri 19/19 |
| P1b | Onaylar sekmeleri | ⬜ | | |
| P1c | Onay detay çekmecesi | ⬜ | | |
| P2a | Stok arama ucu (sayfalı, filtreli) | ⬜ | | |
| P2b | Stok filtre seçenekleri ucu | ⬜ | | |
| P2c | Stok sayfası | ⬜ | | |
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

_(Planın varsayılan kararlarından sapmalar ve gerekçeleri)_

## Bulgular

_(Kapsam dışı ama not edilmesi gerekenler)_

## Seni Bekleyenler

_(İnsan kapıları; goal bitince burada toplanır)_
