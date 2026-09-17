# Goal Durumu — Sunucudan Mikro'ya Yazım

Son güncelleme: 2026-09-17 (Y2a PR'ı)
Görev listesi: [GOAL_ERP_YAZIM.md](GOAL_ERP_YAZIM.md) · Mikro kuralları: [mikro-yazim-referansi.md](mikro-yazim-referansi.md)

> **Her görevden sonra, o görevin PR'ı içinde güncellenir.** Oturum kapanırsa buradan devam edilir.
> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı (kapı) · ⛔ tıkandı

---

## Özet

| Faz | Görev | Biten | Durum |
|---|---|---|---|
| Y0 — Referans ve temel düzeltmeler | 5 | 5 | ✅ |
| Y1 — Sunucu: ayarlar, eşleme, dayanıklılık | 5 | 0 | ⬜ |
| Y2 — Ajan: telefon belgesi → komut | 4 | 2 | 🔄 |
| Y3 — Mikro V15 writer'ları | 8 | 0 | ⬜ |
| Y4 — Sipariş Cepte | 6 | 0 | ⬜ |
| Y5 — İzleme ve operasyon | 2 | 0 | ⬜ |
| Y6 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** Y2a — yazma komutları (PR #79)

## Ortam
- Test veritabanı: **`MikroDB_V15_DEMO`** — kullanıcı Mikro'da açtı (Mikro'dan bağlanılabiliyor), 2026-09-17'de
  11:24 yedeği (`MikroDB_V15_02_17_09.bak`) üzerine geri yüklendi; dosyalar `F:\Mikro\v15xx\DEMO\DATA\`, mantıksal dosya
  adları `MikroDB_V15_DEMO`/`_log` yapıldı. Doğrulandı: 524 cari, 4.567 stok, 14.394 cari hareket, `fn_VergiYuzde(4)=20`.
  Canlı testler: `ERPBridge_RUN_INTEGRATION=1 ERPBridge_MIKRO_WRITE_DB=MikroDB_V15_DEMO`.
- İlk kopya `MikroDB_V15_ERPBTEST` (`F:\Mikro\ERPBTEST\`) Mikro'dan açılamadığı için bırakıldı; silinmedi (silme kararı kullanıcının).
- `MikroDB_V15_02` canlı firma verisi — **yalnız okuma**.

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| Y0a | Plan dalını main'e al | ✅ | [#74](https://github.com/Retrosero/ErpBridge/pull/74) | **Codex 3 bulgu, üçü planda düzeltildi:** iade ve tahsilat komutları ortak `ErpDocumentHeader` taşır (idempotency/cari/tarih/kullanıcı); okuyucu kapalı faturayı kasa koduna atfediyor ve satış/alış iadesini ters sınıflandırıyor → yeni **Y0e** (canlı veriyle doğrulandı: `0+iade` müşterilerde, `63+iade` tedarikçilerde). Ayrıca bakiye sorgusunda `cha_cari_cins=0` filtresi eksik |
| Y0b | Referansı tamamla + kolon sözleşme testi | ✅ | [#75](https://github.com/Retrosero/ErpBridge/pull/75) | **Sapma:** her kolonu tek tek listelemek yerine "boş kolon yok" kuralı — Mikro'nun kendi kayıtlarında hiç NULL yok ama kolonların hepsi nullable/varsayılansız; writer kolon listesini şemadan kurup atanmayanı tipine göre sıfırlar, referans yalnız sıfırdan farklı kolonları listeler. Yeni bulgular: bankaya kapalı faturada `cha_grupno=1`; faturada `cha_uuid` GUID; iskonto zinciri `isk_mas1=0, 2..10=1`; satış iadesi alış faturalarıyla aynı sırayı paylaşır; sipariş/irsaliye Fora değerleri. Sözleşme testi `MikroNativeDocumentConventionTests` (4 test, salt okuma, 5/5 yeşil). **Codex 1 bulgu düzeltildi:** canlı testler Docker fikstürünün paylaşılan anahtarıyla açılmasın diye yalnız açık `ERPBridge_MIKRO_WRITE_DB` ile çalışır. **Bağlantı bulgusu:** `localhost` paylaşılan bellekte async sorgular aralıklı düşüyor (mevcut şema testi 3/3); test varsayılanı `tcp:localhost` — ajanın kendi bağlantısı Y3a'da ölçülecek (bugünkü ajan loglarında bu hata yok) |
| Y0c | Yanlış evrak kodlarının düzeltilmesi | ✅ | [#75](https://github.com/Retrosero/ErpBridge/pull/75) | `MikroCodes` tek yerde; tahsilat `cha_evrak_tip` 63→1, irsaliye `sth_evraktip` 4→1, fatura `sth_evraktip` 63→4, fatura `cha_cinsi` 0→6. Eski fatura writer'ı zaten çalışmıyor (SQL'deki `@EvrakTip`, `@TotalAmount` parametre nesnesinde yok) — Y3c'de baştan yazılacak |
| Y0d | `_ERPB_EVRAK_ESLESME` tablosu | ✅ | [#77](https://github.com/Retrosero/ErpBridge/pull/77) | `MikroDocumentLedger`: `EnsureTableAsync` (yoksa oluşturur, varsa dokunmaz), `FindAsync` (`UPDLOCK, HOLDLOCK`), `TryRecordAsync` (tekil ihlalinde `false`). **Sapma:** tablo ajan kurulumunda değil, ilk yazımda tembel oluşturulacak (Y3a) — kurulum akışına dokunmadan, izin yoksa açık hata. Seri `nvarchar(6)` (Mikro genişliği; plan Y1b'deki "1–20" düzeltildi). Canlı testler ERPBTEST 2/2; `MikroWriteTestDatabase` yazma testlerini adında `ERPBTEST` geçmeyen DB'de atlar (V15_02 ile denendi: atlandı, tablo oluşmadı). Bu tablo ErpBridge'in kendi tablosu; Mikro tabloları/tetikleyicileri ve `_ERPB_SENKRONIZASYON` değişmez |
| Y0e | Okuyucu düzeltmeleri (kapalı fatura müşterisi, iade sınıfı, bakiye filtresi) | ✅ | [#76](https://github.com/Retrosero/ErpBridge/pull/76) | **Codex 2 bulgu, ikisi düzeltildi:** (1) ErpBridge'in eski tahsilat writer'ının `63 + alacak` satırları `TAHSILAT` kalır; (2) artımlı okuma değişmeyen satırlara ulaşmadığı için `IErpAdapter.SnapshotProjectionVersion` (Mikro 2) — ajan sürüm yükselince bir kez tam yeniden kurar, elle "Sıfırdan Kur" gerekmez. **Sapma (geriye uyumluluk):** kapalı faturada `cariKod` değiştirilmedi (eski telefonlar bakiyeyi `cariKod` üzerinden topluyor); yeni `ciroCariKod` + `kapali` alanları eklendi. Portal kapalı satırı müşteriye bağlar, ekstre/yürüyen bakiyeden çıkarır. İade sınıfı düzeltildi (`0+iade` SATIS_IADE). Bakiye sorgusuna `cha_cari_cins=0` (bu veride çakışan kod yok — koruma). Canlı okuma testleri ERPBTEST 2/2. **Telefon:** senkron satırlarında sunucu `type`'ı kullanılıyor, düzeltme telefona böyle ulaşır; `LedgerMovementMapper.typeForValues` yedek eşlemesi hâlâ ters → Y4e ile birlikte düzeltilecek |
| Y1a | `erp_write_settings` + `mobile_user_erp_mappings` | ⬜ | | |
| Y1b | Portal ayar, eşleme ve seçim listesi uçları | ⬜ | | |
| Y1c | Portal ayar sayfası + kullanıcı kartı | ⬜ | | |
| Y1d | `erpContext` kiralama yanıtında | ⬜ | | |
| Y1e | Kiralama süresi + geçici hata yeniden denemesi | ⬜ | | |
| Y2a | Satış / iade / tahsilat komutları + adaptör metotları | ✅ | [#79](https://github.com/Retrosero/ErpBridge/pull/79) | `Erp.Abstractions/Documents/MobileDocumentCommands.cs`: ortak `ErpDocumentHeader` + `SalesDocumentCommand` / `SalesReturnCommand` / `CollectionCommand`. `IErpAdapter`'a varsayılan gövdeli üç metot (`NotImplemented` sonucu) — Logo iskeleti değişmeden derlenir ve reddeder (seam testi). **Sapma:** iade kondisyonu yüzde değil `ConditionRatio` (0..1, telefonun `conditionPercent` alanı zaten oran); karma ödemede tahsilat serisi komutta (`ExtraPaymentsSeries`) |
| Y2b | `MobileDocumentTranslator` | ⬜ | | |
| Y2c | `AgentWorker` yeni yol + `retryable` | ⬜ | | |
| Y2d | Türkçe hata kataloğu | ✅ | [#80](https://github.com/Retrosero/ErpBridge/pull/80) | `Shared/ErpWriteError`: 23 kod, her birine tek fabrika; mesajlar yalnız kod ve fark tutarı taşır. Yeniden denenebilir yalnız `ERP_UNAVAILABLE` ve `ERP_CONTEXT_MISSING` (sunucu güncellenince kendiliğinden çözülür). Test: her sabit için tek fabrika, kodlar tekil |
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
| Y6b | Yerel uçtan uca duman testi (DEMO) | ⬜ | | |
| Y6c | Seni Bekleyenler son hâli | ⬜ | | |

---

## Seni Bekleyenler

- Y0e birleşince: müşteri PC'lerine yeni ajan sürümünün kurulması (ajan okuma biçimi sürümünü görüp anlık görüntüyü bir kez kendisi yeniden kurar).
- Y3 sonunda `MikroDB_V15_DEMO`'ya yazılan evrakların Mikro ekranında muhasebeci kontrolü.
- `MikroDB_V15_ERPBTEST` artık kullanılmıyor — silinmesini istersen söyle.
