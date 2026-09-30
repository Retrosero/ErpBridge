# GOAL_MUSTERI_KATALOGU — Durum

Plan: `GOAL_MUSTERI_KATALOGU.md`. İşaretler: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı · ⛔ tıkandı.
Her görevden sonra, o görevin PR'ı içinde güncellenir.

## Yetkiler (kullanıcı, 2026-10-01)
Dala push, PR, `main`'e birleştirme (Actions kredisi yoksa yerel `dotnet build` 0 uyarı + `dotnet test` yeşilken),
Coolify dağıtımı ve `/health/schema` kontrolü, Sipariş Cepte Play **internal** yüklemesi önceden onaylı.
Asla: `--force` push (ve `--force-with-lease`), CI kırmızıyken (kredi dışı sebeple) birleştirme, `main`'e doğrudan kod
push'u, Play production.

| # | Görev | Durum | PR | Not |
|---|---|---|---|---|
| S1 | ForwardedHeaders, /64, giriş yavaşlatıcı, `OnRejected` | ⬜ | | |
| S2 | Tablolar, migration, modül, kilitli yetki, Admin kutucuğu | ⬜ | | |
| S3 | Katalog derleme (görünüm, görünürlük, fiyat) | ⬜ | | |
| S4 | Yönetim uçları | ⬜ | | |
| S5 | Görsel uçları | ⬜ | | |
| S6 | Müşteri oturumu | ⬜ | | |
| S7 | Müşteri gezinme + quote | ⬜ | | |
| S8 | Talepler + belge bağı + bildirim | ⬜ | | |
| S9 | Ekstre, faturalar, aldıkları | ⬜ | | |
| W0–W5 | Web katalog | ⬜ | | |
| P1–P5 | Panel | ⬜ | | |
| A1–A7 | Telefon (Siparis_Cepte) | ⬜ | | |
| D1 | Cloudflare + Coolify alan adı (kullanıcı) | ⬜ | | |
| D2 | CentralApi dağıtımı | ⬜ | | |
| D3 | Ortam değişkenleri + gerçek IP doğrulaması | ⬜ | | |
| D4 | Portal + Admin dağıtımı | ⬜ | | |
| D5 | Test firmasında modül + duman testi (kullanıcı) | ⬜ | | |
| D6 | Play internal | ⬜ | | |

## Karar günlüğü
| Tarih | Karar | Gerekçe |
|---|---|---|
| 2026-10-01 | K1–K8, T1–T9 (plan §2) | Kullanıcı plan onayı |
