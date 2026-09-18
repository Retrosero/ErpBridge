# Goal Durumu — Kalan telefon belgeleri Mikro'ya

Son güncelleme: 2026-09-19 (plan)
Görev listesi: [GOAL_ERP_YAZIM_2.md](GOAL_ERP_YAZIM_2.md) · Mikro kuralları: [mikro-yazim-referansi.md](mikro-yazim-referansi.md)

> **Her görevden sonra, o görevin PR'ı içinde güncellenir.** Oturum kapanırsa buradan devam edilir.
> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı (kapı) · ⛔ tıkandı

---

## Özet

| Faz | Görev | Biten | Durum |
|---|---|---|---|
| Z0 — Referans ve envanter | 5 | 0 | ⬜ |
| Z1 — Sunucu: sözleşme, kabul, ayarlar | 3 | 0 | ⬜ |
| Z2 — Çevirici ve komutlar | 2 | 0 | ⬜ |
| Z3 — Mikro V15 writer'ları | 4 | 0 | ⬜ |
| Z4 — Sipariş Cepte | 4 | 0 | ⬜ |
| Z5 — İzleme ve geriye dönük | 2 | 0 | ⬜ |
| Z6 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** Z0a — plan dalını main'e al

**Kapsam:** Tediye · Alış faturası (satırlı) · Yeni cari kartı. Gider ve sayım **kapsam dışı** (kullanıcı kararı U1);
ikisi de Z1b ile ERP'li firmada görünür biçimde reddedilecek.

## Ortam
- Test veritabanı: **`MikroDB_V15_DEMO`** (önceki goal'den; `F:\Mikro\v15xx\DEMO\DATA\`).
  Canlı testler: `ERPBridge_RUN_INTEGRATION=1 ERPBridge_MIKRO_WRITE_DB=MikroDB_V15_DEMO`, bağlantı `tcp:localhost`.
- `MikroDB_V15_02` canlı firma verisi — **yalnız okuma** (Z0b/Z0c/Z0d buradan kanıt toplar).

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| Z0a | Plan dalını main'e al | 🔄 | — | Bu belge + goal + `CLAUDE.md` yetki istisnası |
| Z0b | Referans §10 — alış faturası | ⬜ | | |
| Z0c | Referans §11 — tediye | ⬜ | | |
| Z0d | Referans §12 — cari kartı | ⬜ | | |
| Z0e | Başarısız iş envanteri | ⬜ | | Z5a bu listeyi kullanır |
| Z1a | Gövde sözleşmesi v3 | ⬜ | | |
| Z1b | Gider/sayım ERP'li firmada reddedilir | ⬜ | | |
| Z1c | `erp_write_settings` + Portal UI | ⬜ | | |
| Z2a | Komutlar + adaptör metotları | ⬜ | | |
| Z2b | `MobileDocumentTranslator` genişlemesi | ⬜ | | |
| Z3a | Alış faturası writer | ⬜ | | |
| Z3b | Tediye writer | ⬜ | | |
| Z3c | Cari kartı writer + eski writer denetimi | ⬜ | | |
| Z3d | Ajan yeni türleri tanısın | ⬜ | | **Çakışma:** `AgentJobPump` dalı (`faz-51-mobil-siparis-mikro`) bu kodu yeniden yazıyor; başlamadan önce main'e girip girmediğine bakılır |
| Z4a | Telefon: alış gövdesi | ⬜ | | |
| Z4b | Telefon: yeni cari | ⬜ | | |
| Z4c | Telefon: tediye tek belge | ⬜ | | |
| Z4d | Sürüm + Play internal | ⬜ | | AAB'yi kullanıcı derler |
| Z5a | Başarısız işlerin yeniden denenmesi | ⬜ | | |
| Z5b | Portal listesi + Log Merkezi olayları | ⬜ | | |
| Z6a | DEMO'da uçtan uca duman testi | ⬜ | | |
| Z6b | KB güncellemesi | ⬜ | | |
| Z6c | DURUM kapanışı | ⬜ | | |

---

## Bulgular

### Kalan türler bugün kalıcı hata alıyor (2026-09-19, plan aşaması)
ERP'li firmada telefonun gönderdiği **tediye, gider, alış ve sayım** belgeleri Mikro'ya ulaşmıyor:
ingest'te belge türü beyaz listesi yok (her tür iş oluyor), ajan ise yalnız altı türü tanıyor
(`AgentWorker.GenericDocumentTypes`) ve tanımadığını `UNSUPPORTED_DOCUMENT_TYPE` + `retryable=false` ile
**kalıcı** reddediyor. Bu goal tediye ve alışı yazar, gider ile sayımı görünür biçimde reddeder (D5),
geriye dönük birikmiş işleri Z5a temizler.

---

## Seni Bekleyenler

| # | Konu | Neden sen |
|---|---|---|
| 1 | Mikro'da **alış faturası örneği**: `MikroDB_V15_02`'de canlı alış faturası yoksa, Mikro ekranından DEMO'ya bir örnek kesilmesi | Z0b kanıtı ekran kaydından gelir; tahminle yazılmaz |
| 2 | **Tediye** örneği (nakit ve havale) — aynı gerekçe | Z0c |
| 3 | Yeni cari için **kod öneki / grup kodu / ödeme planı** hangi değerler olmalı | Muhasebe kararı; Z1c bunları Portal ayarına koyar |
| 4 | Z6a duman testinden sonra Mikro ekranından kontrol | Muhasebeci gözü |
