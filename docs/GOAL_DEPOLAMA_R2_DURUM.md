# GOAL_DEPOLAMA_R2 — Durum

Plan: `GOAL_DEPOLAMA_R2.md`. İşaretler: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı · ⛔ tıkandı.

## Yetkiler (kullanıcı, 2026-10-01)
Dala push, PR, `main`'e birleştirme (CI yeşilken; Actions kredisi yoksa yerel `dotnet build` 0 uyarı + `dotnet test`
yeşilken), Coolify dağıtımı ve `/health/schema` kontrolü, Sipariş Cepte Play **internal** yüklemesi önceden onaylı.
Asla: `--force` push, CI kırmızıyken (kredi dışı sebeple) birleştirme, `main`'e doğrudan kod push'u, Play production,
R2 API anahtarı değerlerini görmek/girmek (kullanıcı girer).

| # | Görev | Durum | PR | Not |
|---|---|---|---|---|
| D0 | R2 kovaları + `img.appsgo.cloud` | 🔄 | | Kovalar ve alan adı hazır; API anahtarı + Coolify sırları kullanıcıda |
| S1 | `FileStore`, `stored_files`, `tenant_storage`, R2 istemcisi | ⬜ | | |
| S2 | SkiaSharp küçültme/WebP | ⬜ | | |
| S3 | Katalog + banner → R2 | ⬜ | | |
| S4 | Görev eki → R2 (özel kova) | ⬜ | | |
| S5 | Gider/araç fişi | ⬜ | | |
| S6 | Ürün fotoğrafı | ⬜ | | |
| S7 | XML görsel eşitleyici (değişen/silinen izlenir) | ⬜ | | |
| S8 | Kota/kullanım + Admin | ⬜ | | |
| S9 | Temizlik + bakım işi | ⬜ | | |
| S10 | Bytea → R2 göçü | ⬜ | | |
| P1–P3 | Panel + Admin | ⬜ | | |
| A1–A4 | Telefon | ⬜ | | |
| K1 | Bilgi bankası | ⬜ | | |

## Karar günlüğü
| Tarih | Karar | Gerekçe |
|---|---|---|
| 2026-10-01 | R1–R6, T1–T11 (plan §2); varsayılan kota 5 GB | Kullanıcı plan onayı |
