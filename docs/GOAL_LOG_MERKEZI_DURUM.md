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
| L1 — Log Merkezi v1 (Admin) | 3 | 3 | ✅ |
| L2 — Sunucu, Portal, Admin logları | 6 | 6 | ✅ |
| L3 — ERP Windows ajanı | 7 | 0 | ⬜ |
| L4 — Sipariş Cepte | 8 | 0 | ⬜ |
| L5 — Log Merkezi v2 | 6 | 0 | ⬜ |
| L6 — Denetim, tanılama, saklama | 4 | 0 | ⬜ |
| L7 — Uyarılar | 2 | 0 | ⬜ |
| L8 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** L2 — sunucu, Portal, Admin logları

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| L0a | Plan dalını main'e al | ✅ | [#67](https://github.com/Retrosero/ErpBridge/pull/67) | Codex 4 bulgu, dördü planda düzeltildi: pg_trgm eklentisi önce ve hata toleranslı; eski tablo için L1c geçişi; boştaki telefona `diagnostics/pending`; tekillik `(Source, EventId)` |
| L0b | `log_events` tablosu + eski telemetri kopyası | ✅ | [#68](https://github.com/Retrosero/ErpBridge/pull/68) | L0b+c+d tek PR. **Codex 5 bulgu, beşi düzeltildi:** boşluklu/tırnaklı gizli değer tam maskelenir; yazım tek transaction + çakışmada tüm deneme yeniden (sayaç satırlardan sapmaz, grupsuz WARN+ kalmaz); önem derecesi UPDATE içinde saklı değerle yükselir; kopyalanan satırları `LogGroupBackfillWorker` gruplar. Ayrıca TCKN/telefon kalıplarının GUID/hex içindeki rakamları maskelediği (kararsız test) bulundu ve düzeltildi. Zamanlar ayrıca Unix ms (SQLite). Kopya ve trigram SQL'i `LogCenterMigrationSql`'de (migration yeniden üretilebilsin). Yerelde Docker yok → PostgreSQL SQL'i CI'da çalışmaz; merge sonrası `/health/schema` ile doğrulanacak |
| L0c | `LogScrubber`, `LogNormalizer`, `ErrorFingerprint`, `log_error_groups`, `LogEventWriter` | ✅ | [#68](https://github.com/Retrosero/ErpBridge/pull/68) | Normalizasyon `LogSeverity` + `LogEventWriter.NormalizeKind`'da (ayrı sınıf gerekmedi). Grup sayacı ilişkisel sağlayıcıda tek `ExecuteUpdate`; in-memory testlerde izlenen varlık |
| L0d | Eski iki uç yeni tabloya yazar | ✅ | [#68](https://github.com/Retrosero/ErpBridge/pull/68) | Telefon: `UserId` + `DeviceId` token'dan (API anahtarında gövdedeki `deviceId`). Yazım hatası yakalanır, yükleme bozulmaz |
| L0e | `CorrelationIdMiddleware` + genel hata yakalayıcı | ✅ | [#69](https://github.com/Retrosero/ErpBridge/pull/69) | `CorrelationId.UseCorrelationId` hattın ilk ara katmanı; `UnhandledExceptionHandler` iç ayrıntısız 500 `INTERNAL_ERROR` + `traceId`, ayrıntıyı yeni DI kapsamında `log_events`'e yazar (logger kategorisi DB logger'ında atlanacak, L2b) |
| L0f | `log_settings` + `LogRetentionWorker` | ✅ | [#69](https://github.com/Retrosero/ErpBridge/pull/69) | Sunucu alış zamanına göre partili silme; bayat **açık** gruplar silinir, çözüldü/yok sayıldı korunur. **Codex 3 bulgu düzeltildi:** konsol satırı maskeli metin (istisna nesnesi değil); grup yalnız hiçbir saklı olay göstermiyorsa bayat; `MaxDeletesPerRun` kesin sınır. #68 canlıda `/health/schema` current (25 migration) |
| L1a | Admin log uçları (liste/detay/facets) | ✅ | [#70](https://github.com/Retrosero/ErpBridge/pull/70) | Filtreler tek yerde (`LogQuery`); **Codex 2 bulgu düzeltildi:** imleç `{OccurredAtMs}~{EventId}~{Source}` (kimlik yalnız kaynak içinde tekil), çelişen seviye filtresi boş sonuç; metin arama PostgreSQL'de ILIKE (Türkçe büyük/küçük harf ASCII dışında eşleşmez — kısıt) |
| L1b | Admin `/logs` sayfası | ✅ | [#70](https://github.com/Retrosero/ErpBridge/pull/70) | Filtreler + açık kayıt URL'de; detay paneli; saatler Türkiye saati. **Görsel kontrol:** giriş formuna parola yazılmadığı için (güvenlik kuralı) sayfa bUnit çıktısı Admin CSS'iyle tarayıcıda açılarak masaüstü ve dar ekranda kontrol edildi; kopyala düğmesi taşması düzeltildi |
| L1c | Geçiş + menü + Dashboard kartı | ✅ | [#70](https://github.com/Retrosero/ErpBridge/pull/70) | İki telemetri ucu yalnız `log_events`'e yazar (hata artık 5xx); `/admin/telemetry` yeni tablodan (tür büyük harf); Dashboard kartı son 24 saat ERROR+ → `/logs?minSeverity=ERROR` |
| L2a | CentralApi log yapılandırması | ✅ | #72 | Tek satır + kapsamlı konsol, EF SQL komutları Warning. **Sapma:** JSON yerine tek satır basit biçim (Coolify konsolunda okunur) |
| L2b | `DatabaseLoggerProvider` | ✅ | #72 | Ortak altyapı yeni `ErpBridge.Diagnostics` projesinde (`BufferedLogProvider`, paket bağımlılığı yok). EF + yakalanmamış istisna kategorileri hariç; Test ortamında kapalı |
| L2c | 5xx + yavaş istek kaydı | ✅ | #72 | Firma/kullanıcı/ajan kapsamı log anında okunur (API anahtarı uçta doğrulanıyor); uzun yoklamalar yavaş sayılmaz |
| L2d | `/internal/logs` + `RemoteLoggerProvider` | ✅ | #72 | Anahtar tanımsızsa uç 404, gönderici kapalı |
| L2e | Portal hata sınırı + loglama | ✅ | #72 | **Codex 4 bulgu düzeltildi:** iç log ucu gövdeyi anahtardan sonra ve en çok 1 MB okur, null olaylar 400; `/Error` kodu (`TraceIdentifier`) loglanan istisnanın iz kimliği (`UseRequestCorrelationScope`); `PortalPageBase.RunAsync` yakalanan API/bağlantı hatalarını sayfa, firma, kullanıcıyla loglar (5xx ERROR, diğerleri WARN). `PortalErrorBoundary` destek kodu = iz kimliği; firma/kullanıcı JWT'den (yalnız log bağlamı); `CorrelationIdHandler` 5xx/ulaşılamayan çağrıyı loglar; `/Error` sayfası (önceden yoktu) yerelde doğrulandı |
| L2f | Admin hata sınırı + loglama | ✅ | #72 | `AdminErrorBoundary`, `/Error`, aynı işleyici ve log gönderimi |
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
- **Coolify:** `Logs__InternalIngestKey` (en az 32 karakter rastgele) centralapi, admin **ve** portal servislerine aynı değerle eklenmeli; eklenene kadar Portal/Admin logları yalnız konsolda kalır (sunucunun kendi logları anahtarsız da Log Merkezi'ne yazılır).
- **K1:** Admin konsolu ortak oturum (`TokenStore` tekil) açığı — test aşaması bittiğinde ayrı iş olarak kapatılacak.
