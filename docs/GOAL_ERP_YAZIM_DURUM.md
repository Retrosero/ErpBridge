# Goal Durumu — Sunucudan Mikro'ya Yazım

Son güncelleme: 2026-09-17 (Y0a PR'ı)
Görev listesi: [GOAL_ERP_YAZIM.md](GOAL_ERP_YAZIM.md) · Mikro kuralları: [mikro-yazim-referansi.md](mikro-yazim-referansi.md)

> **Her görevden sonra, o görevin PR'ı içinde güncellenir.** Oturum kapanırsa buradan devam edilir.
> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı (kapı) · ⛔ tıkandı

---

## Özet

| Faz | Görev | Biten | Durum |
|---|---|---|---|
| Y0 — Referans ve temel düzeltmeler | 5 | 1 | 🔄 |
| Y1 — Sunucu: ayarlar, eşleme, dayanıklılık | 5 | 0 | ⬜ |
| Y2 — Ajan: telefon belgesi → komut | 4 | 0 | ⬜ |
| Y3 — Mikro V15 writer'ları | 8 | 0 | ⬜ |
| Y4 — Sipariş Cepte | 6 | 0 | ⬜ |
| Y5 — İzleme ve operasyon | 2 | 0 | ⬜ |
| Y6 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** Y0b — referansı tamamla

## Ortam
- Test veritabanı: `MikroDB_V15_ERPBTEST` — 2026-09-17 11:24 yedeğinden (`MikroDB_V15_02_17_09.bak`) geri yüklendi,
  dosyalar `F:\Mikro\ERPBTEST\`. Doğrulandı: 14.394 cari hareket, 4.567 stok, `fn_VergiYuzde(4)=20`.
- `MikroDB_V15_02` canlı firma verisi — **yalnız okuma**.

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| Y0a | Plan dalını main'e al | ✅ | [#74](https://github.com/Retrosero/ErpBridge/pull/74) | **Codex 3 bulgu, üçü planda düzeltildi:** iade ve tahsilat komutları ortak `ErpDocumentHeader` taşır (idempotency/cari/tarih/kullanıcı); okuyucu kapalı faturayı kasa koduna atfediyor ve satış/alış iadesini ters sınıflandırıyor → yeni **Y0e** (canlı veriyle doğrulandı: `0+iade` müşterilerde, `63+iade` tedarikçilerde). Ayrıca bakiye sorgusunda `cha_cari_cins=0` filtresi eksik |
| Y0b | Referansı tamamla + kolon sözleşme testi | ⬜ | | Satış/kapalı fatura/iade/tahsilat bölümleri canlı veriyle yazıldı; tam kolon listeleri kaldı |
| Y0c | Yanlış evrak kodlarının düzeltilmesi | ⬜ | | |
| Y0d | `_ERPB_EVRAK_ESLESME` tablosu | ⬜ | | |
| Y0e | Okuyucu düzeltmeleri (kapalı fatura müşterisi, iade sınıfı, bakiye filtresi) | ⬜ | | |
| Y1a | `erp_write_settings` + `mobile_user_erp_mappings` | ⬜ | | |
| Y1b | Portal ayar, eşleme ve seçim listesi uçları | ⬜ | | |
| Y1c | Portal ayar sayfası + kullanıcı kartı | ⬜ | | |
| Y1d | `erpContext` kiralama yanıtında | ⬜ | | |
| Y1e | Kiralama süresi + geçici hata yeniden denemesi | ⬜ | | |
| Y2a | Satış / iade / tahsilat komutları + adaptör metotları | ⬜ | | |
| Y2b | `MobileDocumentTranslator` | ⬜ | | |
| Y2c | `AgentWorker` yeni yol + `retryable` | ⬜ | | |
| Y2d | Türkçe hata kataloğu | ⬜ | | |
| Y3a | `MikroWriteSession` + idempotency + seri/sıra | ⬜ | | |
| Y3b | Lookup + fiyat/iskonto/KDV hesabı | ⬜ | | |
| Y3c | Satış faturası (açık + kapalı) | ⬜ | | |
| Y3d | Sipariş | ⬜ | | |
| Y3e | Satış irsaliyesi | ⬜ | | |
| Y3f | Satış iadesi faturası | ⬜ | | |
| Y3g | Tahsilat makbuzu (5 yöntem, tek evrak) | ⬜ | | |
| Y3h | Karma ödemeli satış | ⬜ | | |
| Y4a | Telefon satış gövdesi (liste fiyatı, iskontolar, fiyat listesi) | ⬜ | | |
| Y4b | Telefon iade gövdesi (satırlı) | ⬜ | | |
| Y4c | Telefon tahsilat gövdesi (tek belge, `payments[]`) | ⬜ | | |
| Y4d | Yazım sonucu telefonda | ⬜ | | |
| Y4e | Çift görünme önleme | ⬜ | | |
| Y4f | Play internal sürüm | ⬜ | | |
| Y5a | Portal "ERP Aktarım" listesi | ⬜ | | |
| Y5b | Admin iş ayrıntısı + log olayları | ⬜ | | |
| Y6a | KB ve sözleşme belgeleri | ⬜ | | |
| Y6b | Yerel uçtan uca duman testi (ERPBTEST) | ⬜ | | |
| Y6c | Seni Bekleyenler son hâli | ⬜ | | |

---

## Seni Bekleyenler

- Y0e birleşince: müşteri PC'sine yeni ajan + cari hareketleri bölümü için "Sıfırdan Kur" (sunucudaki eski satırlar yanlış müşteri/tür taşıyor).
- ERPBTEST'i Mikro'da test firması olarak tanımlamak (evrakları Mikro ekranında görmek için; Y3 sonunda muhasebeci kontrolü).
