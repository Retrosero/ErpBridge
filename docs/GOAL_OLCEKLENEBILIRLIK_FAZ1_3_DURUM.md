# Goal Durumu — Ölçeklenebilirlik Faz 1-3

Son güncelleme: 2026-09-22
Görev listesi: [GOAL_OLCEKLENEBILIRLIK_FAZ1_3.md](GOAL_OLCEKLENEBILIRLIK_FAZ1_3.md)

> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ insan kapısı (bu goal yapamaz) · ⛔ tıkandı

---

## Özet

| Faz | Görev | Biten | Durum |
|---|---|---|---|
| Faz 1 — Yazma yolu | 6 (F1a-f) | 0 | ⬜ |
| Faz 2 — Veri bütünlüğü | 4 (F2a-d) | 0 | ⬜ |
| Faz 3 — Yatay ölçekleme | 6 (F3a-f) | 0 | ⬜ |

**Şu anki görev:** F1a — yerel/izole yük testi kurulumu

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| F1a | Yerel/izole yük testi kurulumu | ⬜ | | |
| F1b | Yük testi bulgu raporu | ⬜ | | |
| F1c | docker-compose CPU/RAM limiti | ⬜ | | |
| F1d | Ağır yazma uçlarını hafif kuyruğa alma | ⬜ | | |
| F1e | Rate limiter QueueLimit ayarı | ⬜ | | |
| F1f | VPS büyütme | ⏭️ | | İnsan kapısı — Hostinger panelinden |
| F2a | Çakışma riski taşıyan entity araştırması | ⬜ | | |
| F2b | Son yazan kazanır + denetim izi uygulaması | ⬜ | | F2a'nın bulgusuna bağlı |
| F2c | DB izolasyon/managed DB analizi | ⬜ | | |
| F2d | Managed DB'ye gerçek geçiş | ⏭️ | | İnsan kapısı |
| F3a | Redis backplane soyutlaması | ⬜ | | |
| F3b | docker-compose Redis servis tanımı | ⬜ | | |
| F3c | PgBouncer config | ⬜ | | |
| F3d | Hangfire entegrasyonu | ⬜ | | |
| F3e | Yerel uçtan uca çoklu-instance testi | ⬜ | | |
| F3f | Coolify'da gerçek çoklu instance/Redis/PgBouncer | ⏭️ | | İnsan kapısı — Coolify panel erişimi gerekir |

## Seni Bekleyenler (insan kararı/eylemi gerekli)

1. **Otonomi seviyesi:** Bu goal'ü tam otonom (`/goal` tarzı, müdahalesiz) mi çalıştırmak istiyorsun, yoksa her PR sonrası merge onayını ben mi sorayım? Tam otonom istiyorsan CLAUDE.md'ye istisna satırını kendin eklemen gerekiyor (bkz. goal belgesi §3) — ben kendi kural dosyamı düzenleyemiyorum.
2. **F1f, F2d, F3f** — bunlar zaten insan kapısı, goal bunlara geldiğinde koddan hazırladığını burada bırakıp bir sonraki bağımsız göreve geçecek; gerçek uygulamayı (Hostinger/Coolify panelinde) sen yapacaksın.
