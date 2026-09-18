# Goal Durumu — Parametre Yönetimi

Son güncelleme: 2026-09-18 (plan dalı)
Görev listesi: [GOAL_PARAMETRE_YONETIMI.md](GOAL_PARAMETRE_YONETIMI.md)

> **Her görevden sonra, o görevin PR'ı içinde güncellenir.** Oturum kapanırsa buradan devam edilir.
> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı (kapı) · ⛔ tıkandı

---

## Özet

| Faz | Görev | Biten | Durum |
|---|---|---|---|
| P0 — Katalog çıkarımı (ön koşul) | 7 | 0 | 🔄 |
| P1 — Merkez veri modeli ve API | 7 | 0 | ⬜ |
| P2 — Panel (Admin) parametre ekranı | 8 | 0 | ⬜ |
| P3 — Ajan: Mikro aynası ve Fora içe aktarımı | 6 | 0 | ⬜ |
| P4 — Sipariş Cepte: öncelikli öbekler | 6 | 0 | ⬜ |
| P5 — Kalan `akilli` öbekleri | 8 | 0 | ⬜ |
| P6 — Aktarım şablonları ve entegrasyonlar | 4 | 0 | ⬜ |
| P7 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** P00 — plan dalını `main`'e al

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| P00 | Plan dalını `main`'e al | 🔄 | [#111](https://github.com/Retrosero/ErpBridge/pull/111) | **Codex 2 P1 bulgu, ikisi de kodda doğrulandı ve planda düzeltildi:** (1) değer anahtarında ERP firması yoktu — çok firmalı kiracıda bir firmanın depo/şube/seri ayarı diğerinin üzerine yazardı → D3b, `ErpCompanyId` değer/sürüm/API/ayna kapsamlarında zorunlu; (2) kullanıcı adı değişmez kimlik değil (`MobileSeatsRelationalTests.Deleting_a_user_releases_the_seat_and_the_username`: silinen satır geçmiş için kalıyor, aynı adla yeni kullanıcı açılabiliyor) — kullanıcı adına anahtarlamak silinenin `Hak*` yetkilerini devrederdi → D5 `MobileUser.Id`, D5b yalnız aktif kullanıcılar yayılır |
| P0a | `ParametrelerDefault.cs` → `defaults.json` (4.688 tanım) | ⬜ | — | |
| P0b | `ForaAndroidKullaniciDuzenleme.cs` → `ui.akilli.json` (63 sekme, 3.572 bağlama) | ⬜ | — | |
| P0c | Diğer editör ekranlarının UI metadata'sı | ⬜ | — | |
| P0d | Tip çıkarsama ve doğrulama | ⬜ | — | |
| P0e | Çıkarım script'i + altın dosya testi | ⬜ | — | |
| P0f | El ile gözden geçirme raporu | ⬜ | — | |
| P1a | `parameter_catalog_entries` + tohumlama | ⬜ | — | |
| P1b | `parameter_values` + migration | ⬜ | — | |
| P1c | `ParameterResolver` (Fora semantiği) | ⬜ | — | |
| P1d | `parameter_revisions` + `parameter_audit` | ⬜ | — | |
| P1e | Admin API uçları | ⬜ | — | |
| P1f | `/android/parameters` yeniden bağlama + `revision`/`304` | ⬜ | — | |
| P1g | Ajan uçları | ⬜ | — | |
| P2a | `/parameters` ekran iskeleti + scope seçimi | ⬜ | — | |
| P2b | Katalogdan üretilen sekme ağacı | ⬜ | — | |
| P2c | Editör bileşenleri (7 tip) | ⬜ | — | |
| P2d | Efektif değer / varsayılan rozeti / sıfırlama | ⬜ | — | |
| P2e | Arama, "yalnız sapanlar", "etkisiz" rozeti | ⬜ | — | |
| P2f | Toplu işlemler (kopyalama, dışa/içe aktarma) | ⬜ | — | |
| P2g | Değişiklik geçmişi görünümü | ⬜ | — | |
| P2h | Vergi oranları onay adımı | ⬜ | — | |
| P3a | `_ERPB_PARAMETRELER` kurulumu (V15/V16) | ⬜ | — | |
| P3b | `ParameterMirrorWorker` (merkez → Mikro) | ⬜ | — | |
| P3c | `ForaParameterImporter` (salt okunur) | ⬜ | — | |
| P3d | Panelde "Fora'dan içe aktar" akışı | ⬜ | — | |
| P3e | Ayna fark raporu | ⬜ | — | |
| P3f | ERP lookup beslemesi | ⬜ | — | |
| P4a | Room tablosu + gömülü katalog + `ParameterProvider` | ⬜ | — | |
| P4b | `Goster_AnaMenu_*` (43) | ⬜ | — | |
| P4c | `Hak*` (71) | ⬜ | — | |
| P4d | `EvrakSeri_*` (14) + `Default*` (37) | ⬜ | — | |
| P4e | `SenkronizeEt_*` (44) | ⬜ | — | |
| P4f | "Telefonda etkili" rozeti | ⬜ | — | |
| P5a | `Stok*` + `Listeleme*` | ⬜ | — | |
| P5b | `Sepet*` + `Evrak*` + `Siparis*` | ⬜ | — | |
| P5c | `YeniCari*` (137) | ⬜ | — | |
| P5d | `Yazdirma*` + `Koli*` + `YaziciAyarlari` tasarımcısı | ⬜ | — | |
| P5e | `Rapor*` + `MobilRapor*` | ⬜ | — | |
| P5f | `Metin*` (415) | ⬜ | — | |
| P5g | `Formul*`, `Firma*`, `Zorunlu*`, `risk_hesabi_*`, artakalanlar | ⬜ | — | |
| P5h | `ForaMikro`/`foramikro` (95) + vergi oranları | ⬜ | — | |
| P6a | `GenelAktarim` (1.486) | ⬜ | — | |
| P6b | `TahsilatAktarim` (756) | ⬜ | — | |
| P6c | `BankaAktarim` (503) | ⬜ | — | |
| P6d | `b2b` + `ComarchEdi*` | ⬜ | — | |
| P7a | Bilgi bankası + API sözleşmeleri | ⬜ | — | |
| P7b | Eski `ParameterRecord` yolunun işaretlenmesi | ⬜ | — | |
| P7c | Operatör kılavuzu (Fora'dan geçiş) | ⬜ | — | |

---

## Seni bekleyenler

| # | Madde | Neden insan gerekiyor |
|---|---|---|
| B1 | **Fora sürüm doğrulaması.** Decompile edilen `Fora Mikro.exe` hangi sürüm ve müşteride çalışan sürümle aynı mı? | Katalog `sourceBuild` alanı için gerekli; müşteri kurulumuna bakılmalı |

---

## Notlar

- **2026-09-18:** `MikroDB_V16_03202403221700.bak` yedeği `MikroDB_V16_03` olarak yüklendi
  (`F:\Mikro\v16xx\03\DATA\`). Fora'nın canlı parametre verisi (516 satır, 4 mobil kullanıcı) ve
  `_FORA_SYNC` tetikleyicileri buradan incelendi. Yedek 2024-03-22 tarihli — şema referansı olarak
  kullanılır, veri karşılaştırması için değil.
