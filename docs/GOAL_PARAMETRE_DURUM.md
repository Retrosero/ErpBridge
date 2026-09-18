# Goal Durumu — Parametre Yönetimi

Son güncelleme: 2026-09-18 (P0b)
Görev listesi: [GOAL_PARAMETRE_YONETIMI.md](GOAL_PARAMETRE_YONETIMI.md)

> **Her görevden sonra, o görevin PR'ı içinde güncellenir.** Oturum kapanırsa buradan devam edilir.
> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı (kapı) · ⛔ tıkandı

---

## Özet

| Faz | Görev | Biten | Durum |
|---|---|---|---|
| P0 — Katalog çıkarımı (ön koşul) | 7 | 4 | 🔄 |
| P1 — Merkez veri modeli ve API | 7 | 0 | ⬜ |
| P2 — Panel (Admin) parametre ekranı | 8 | 0 | ⬜ |
| P3 — Ajan: Mikro aynası ve Fora içe aktarımı | 6 | 0 | ⬜ |
| P4 — Sipariş Cepte: öncelikli öbekler | 6 | 0 | ⬜ |
| P5 — Kalan `akilli` öbekleri | 8 | 0 | ⬜ |
| P6 — Aktarım şablonları ve entegrasyonlar | 4 | 0 | ⬜ |
| P7 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** P0c — diğer editör ekranlarının UI metadata'sı

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| P00 | Plan dalını `main`'e al | ✅ | [#111](https://github.com/Retrosero/ErpBridge/pull/111) | **Codex 2 P1 bulgu, ikisi de kodda doğrulandı ve planda düzeltildi:** (1) değer anahtarında ERP firması yoktu — çok firmalı kiracıda bir firmanın depo/şube/seri ayarı diğerinin üzerine yazardı → D3b, `ErpCompanyId` değer/sürüm/API/ayna kapsamlarında zorunlu; (2) kullanıcı adı değişmez kimlik değil (`MobileSeatsRelationalTests.Deleting_a_user_releases_the_seat_and_the_username`: silinen satır geçmiş için kalıyor, aynı adla yeni kullanıcı açılabiliyor) — kullanıcı adına anahtarlamak silinenin `Hak*` yetkilerini devrederdi → D5 `MobileUser.Id`, D5b yalnız aktif kullanıcılar yayılır |
| P0a | `ParametrelerDefault.cs` → `defaults.json` (4.688 tanım) | ✅ | — | 19 set, 13 program, 4.688 tanım. **İki bulgu:** (1) kapsam her programda `ParametreUser`'da değil — aktarım şablonları `ParametreAltGrubu` kullanıyor, `ParametreAnaGrubu` sabit ayırıcı → D4b eklendi, katalog `scopeField` taşıyor; (2) `TahsilatAktarimTxtCsvSablon`'da **Fora'nın kendi hatası**: `belge_tarihi_yil/ay/gun_baslangic` üçü de id 16 (genel şablonda doğru şekilde 16/18/20), aynısı 17 ve 604'te. `_GetParametre(int)` ilk eşleşmeyi döndürdüğü için 5 kayıt Fora'da da okunamıyor → atılmıyor, `shadowed` ile işaretleniyor |
| P0b | `ForaAndroidKullaniciDuzenleme.cs` → `ui.akilli.json` | ✅ | — | 65 sekme (5 dış sekme altında iç içe), **1.782 parametre yerleşti, hepsi etiketli**. Etiket kaynakları: `caption` 733 (her `CheckEdit` kendi başlığını taşıyor), `tableCell` 641, `labelLeft` 387, `checkBoxLeft` 21. Editör dağılımı: 926 bool, 680 metin, 100 tam sayı, 53 seçim, 16 ondalık, 6 renk, 1 çok satırlı. **Üçüncü Fora hatası:** `GosterZyrt_Temsilci_Ozel_12..19_Var_Yok` yanlış kutucuklara yükleniyor ama doğru parametreye kaydediliyor — Fora'da o ekranı açıp kaydetmek bir parametreyi başkasının değeriyle eziyor; kaydetme yönü esas alındı, 8 uyuşmazlık `bindingMismatch` olarak raporlanıyor. `Sifre` hiçbir kontrole bağlanmıyor (D6 zaten hariç tutuyor) |
| P0c | Diğer editör ekranlarının UI metadata'sı | ⬜ | — | |
| P0d | Tip çıkarsama ve doğrulama | ⬜ | — | |
| P0e | Çıkarım script'i + altın dosya testi | ✅ | — | `tools/ForaCatalogExtractor` (Roslyn; dört ZPL şablonu kaçırılmış tırnakla bitiyor, regex sessizce bozuyor). `--check` bayat katalogda sıfırdan farklı kodla çıkar. `tests/ErpBridge.ForaCatalog.Tests` 13 test: bayt bayt altın dosya, iki kez çalıştırınca aynı çıktı, kapsam alanı iddiaları, çakışma kayıtları, Türkçe ve kaçırılmış tırnak korunumu, hatalı girdilerin reddi. Proje merkezi paket yönetiminin dışında: `Directory.Packages.props` geçişli sabitleme açtığı için Roslyn'i oraya eklemek EF Design üzerinden Roslyn çeken her projenin kilit dosyasını yeniden yazardı |
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
