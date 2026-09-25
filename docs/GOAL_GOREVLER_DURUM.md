# GOAL_GOREVLER — Durum

| # | Görev | Durum | PR | Not |
|---|---|---|---|---|
| S1 | Domain + eşlemeler + migration `GorevlerVeBildirimler` | ✅ | #203 | 10 yeni tablo; mevcut tablolara dokunmaz |
| S2 | `TaskService` + görev uçları | ✅ | | Atama K3: ekip kavramı olmadığı için yönetici dışı yalnız kendine |
| S3 | Resim yükleme/indirme/silme + kota | ✅ | | PostgreSQL bytea (K9) |
| S4 | Bildirim uçları + `tasks/events` uzun yoklaması | ✅ | | Kullanıcı başı hız sınırı (dk'da 120) |
| S5 | `TaskSchedulerWorker` | ✅ | | Dakikada bir; kapalı kalınan dönemler için tek örnek |
| S6 | Testler | ✅ | | `TaskRelationalTests` (9), `TaskScheduleTests` (5) |
| S7 | Bilgi bankası (00 kural 27, 03), PR, Coolify | ✅ | #203 | Coolify dağıtımı 2026-09-25; `/health/schema` 39 uygulanmış / 0 bekleyen, görev ucu 401 (oturum ister) |
| A1–A5 | Telefon | ✅ | siparis_cepte #102 | 1.5.260 Play internal; `Siparis_Cepte/docs/GOAL_GOREVLER.md` |

**Sapma:** görünürlük ileri tarihli görevde gerçek saate değil zamanlayıcının başlatma işaretine
(`StartNotifiedAtMs`) bağlandı; atanan görevi bildirimiyle aynı anda görür.
