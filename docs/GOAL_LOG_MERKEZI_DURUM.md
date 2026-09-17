# Goal Durumu — Log Merkezi

Son güncelleme: 2026-09-17 (L0b-d PR'ı)
Görev listesi: [GOAL_LOG_MERKEZI.md](GOAL_LOG_MERKEZI.md)

> **Her görevden sonra, o görevin PR'ı içinde güncellenir.** Oturum kapanırsa buradan devam edilir.
> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı (kapı) · ⛔ tıkandı

---

## Özet

| Faz | Görev | Biten | Durum |
|---|---|---|---|
| L0 — Temel: birleşik model + iz kimliği | 6 | 6 | ✅ |
| L1 — Log Merkezi v1 (Admin) | 3 | 0 | ⬜ |
| L2 — Sunucu, Portal, Admin logları | 6 | 0 | ⬜ |
| L3 — ERP Windows ajanı | 7 | 0 | ⬜ |
| L4 — Sipariş Cepte | 8 | 0 | ⬜ |
| L5 — Log Merkezi v2 | 6 | 0 | ⬜ |
| L6 — Denetim, tanılama, saklama | 4 | 0 | ⬜ |
| L7 — Uyarılar | 2 | 0 | ⬜ |
| L8 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** L1 — Log Merkezi v1 (Admin) PR'ı

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| L0a | Plan dalını main'e al | ✅ | [#67](https://github.com/Retrosero/ErpBridge/pull/67) | Codex 4 bulgu, dördü planda düzeltildi: pg_trgm eklentisi önce ve hata toleranslı; eski tablo için L1c geçişi; boştaki telefona `diagnostics/pending`; tekillik `(Source, EventId)` |
| L0b | `log_events` tablosu + eski telemetri kopyası | ✅ | [#68](https://github.com/Retrosero/ErpBridge/pull/68) | L0b+c+d tek PR. **Codex 5 bulgu, beşi düzeltildi:** boşluklu/tırnaklı gizli değer tam maskelenir; yazım tek transaction + çakışmada tüm deneme yeniden (sayaç satırlardan sapmaz, grupsuz WARN+ kalmaz); önem derecesi UPDATE içinde saklı değerle yükselir; kopyalanan satırları `LogGroupBackfillWorker` gruplar. Ayrıca TCKN/telefon kalıplarının GUID/hex içindeki rakamları maskelediği (kararsız test) bulundu ve düzeltildi. Zamanlar ayrıca Unix ms (SQLite). Kopya ve trigram SQL'i `LogCenterMigrationSql`'de (migration yeniden üretilebilsin). Yerelde Docker yok → PostgreSQL SQL'i CI'da çalışmaz; merge sonrası `/health/schema` ile doğrulanacak |
| L0c | `LogScrubber`, `LogNormalizer`, `ErrorFingerprint`, `log_error_groups`, `LogEventWriter` | ✅ | [#68](https://github.com/Retrosero/ErpBridge/pull/68) | Normalizasyon `LogSeverity` + `LogEventWriter.NormalizeKind`'da (ayrı sınıf gerekmedi). Grup sayacı ilişkisel sağlayıcıda tek `ExecuteUpdate`; in-memory testlerde izlenen varlık |
| L0d | Eski iki uç yeni tabloya yazar | ✅ | [#68](https://github.com/Retrosero/ErpBridge/pull/68) | Telefon: `UserId` + `DeviceId` token'dan (API anahtarında gövdedeki `deviceId`). Yazım hatası yakalanır, yükleme bozulmaz |
| L0e | `CorrelationIdMiddleware` + genel hata yakalayıcı | ✅ | #69 | `CorrelationId.UseCorrelationId` hattın ilk ara katmanı; `UnhandledExceptionHandler` iç ayrıntısız 500 `INTERNAL_ERROR` + `traceId`, ayrıntıyı yeni DI kapsamında `log_events`'e yazar (logger kategorisi DB logger'ında atlanacak, L2b) |
| L0f | `log_settings` + `LogRetentionWorker` | ✅ | #69 | Sunucu alış zamanına göre partili silme; bayat **açık** gruplar silinir, çözüldü/yok sayıldı korunur |
| L1a | Admin log uçları (liste/detay/facets) | ⬜ | | |
| L1b | Admin `/logs` sayfası | ⬜ | | |
| L1c | Menü + Dashboard kartı | ⬜ | | |
| L2a | CentralApi log yapılandırması | ⬜ | | |
| L2b | `DatabaseLoggerProvider` | ⬜ | | |
| L2c | 5xx + yavaş istek kaydı | ⬜ | | |
| L2d | `/internal/logs` + `RemoteLoggerProvider` | ⬜ | | |
| L2e | Portal hata sınırı + loglama | ⬜ | | |
| L2f | Admin hata sınırı + loglama | ⬜ | | |
| L3a | Ajan log yapılandırması + sürüm | ⬜ | | |
| L3b | Ajan maskeleme düzeltmeleri | ⬜ | | |
| L3c | Ajan olay kuyruğu + `/agents/logs/batch` | ⬜ | | |
| L3d | Raporlanmayan ajan hatalarının bağlanması | ⬜ | | |
| L3e | `AGENT_SYNC_ROUND` | ⬜ | | |
| L3f | Heartbeat zenginleştirme + geçmiş | ⬜ | | |
| L3g | `jobs.CorrelationId` + ajan iz kimliği | ⬜ | | |
| L4a | Telefon bağlam alanları + gizlilik belgeleri | ⬜ | | |
| L4b | Telefon iz kimliği | ⬜ | | |
| L4c | Telemetri yükleme işi düzeltmeleri | ⬜ | | |
| L4d | Telefon kapsama boşlukları | ⬜ | | |
| L4e | ANR / çıkış nedenleri | ⬜ | | |
| L4f | Breadcrumb | ⬜ | | |
| L4g | Yerel log + "Tanılama gönder" | ⬜ | | L6c'ye bağlı |
| L4h | Sürüm + Play internal | ⬜ | | |
| L5a | Hata grupları | ⬜ | | |
| L5b | İz zaman çizelgesi | ⬜ | | |
| L5c | Grafik + özet | ⬜ | | |
| L5d | Canlı akış | ⬜ | | |
| L5e | Cihaz + ajan sayfaları | ⬜ | | |
| L5f | CSV dışa aktarma | ⬜ | | |
| L6a | `audit_events` + yazım noktaları | ⬜ | | |
| L6b | Denetim sayfası | ⬜ | | |
| L6c | Uzaktan tanılama | ⬜ | | |
| L6d | Saklama ayarları sayfası | ⬜ | | |
| L7a | Uyarı kuralları + değerlendirme işi | ⬜ | | |
| L7b | Uyarı rozeti + `/alerts` | ⬜ | | |
| L8a | KB güncellemeleri | ⬜ | | |
| L8b | Canlı duman testi | ⬜ | | |
| L8c | Seni Bekleyenler son hâli | ⬜ | | |

---

## Seni Bekleyenler
- **K1:** Admin konsolu ortak oturum (`TokenStore` tekil) açığı — test aşaması bittiğinde ayrı iş olarak kapatılacak.
