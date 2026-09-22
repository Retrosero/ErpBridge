# Goal Durumu — Ölçeklenebilirlik Faz 0

Son güncelleme: 2026-09-22
Görev listesi: [GOAL_OLCEKLENEBILIRLIK.md](GOAL_OLCEKLENEBILIRLIK.md)

> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti (push/PR onayı bekliyor) · ⏭️ karar/onay bekliyor · ⛔ tıkandı

---

## Özet

| Görev | Durum |
|---|---|
| O0a — Npgsql pool sınırı | ✅ [ErpBridge PR #156](https://github.com/Retrosero/ErpBridge/pull/156) açıldı |
| O0b — LiveSyncManager jitter | ✅ [Siparis_Cepte PR #89](https://github.com/Retrosero/siparis_cepte/pull/89) açıldı |
| O0c — Görünürlük/metrik | ⏭️ kapsam netleşmedi |
| O0d — Gerçek yük testi | ⏭️ ertelendi (kullanıcı: canlıda test müşterisi varken risk almayalım) |
| O0e — Çakışma politikası kararı | ✅ karar verildi: "son yazan kazanır + denetim izi". Uygulaması bu goal'ün dışına, Faz 2'ye taşındı — hangi entity'lerin (sipariş/tahsilat/cari-stok düzenleme) gerçekten eşzamanlı çakışma riski taşıdığını netleştirmeden şema/kod tahmin edilmeyecek (bkz. Siparis_Cepte kuralı: ERP'li firmada zaten cari/stok uygulamadan değiştirilemiyor — asıl risk alanı önce doğrulanmalı) |

## Detaylar

**O0a — ErpBridge (`faz-olceklenebilirlik-goal` dalı)**
- Değişen dosya: `src/ErpBridge.CentralApi/Program.cs` — `ApplyPoolDefaults` eklendi, `ConfigureData` içinde kullanılıyor.
- Doğrulama: `dotnet build src/ErpBridge.CentralApi/ErpBridge.CentralApi.csproj -c Debug` → 0 uyarı/0 hata.
- Push edildi, PR açıldı: [#156](https://github.com/Retrosero/ErpBridge/pull/156). CI + inceleme bekleniyor, `main`'e merge onayı ayrıca alınacak (bu goal'e push/PR istisnası tanındı, merge istisnası tanınmadı).

**O0b — Siparis_Cepte (`faz-olceklenebilirlik-goal` dalı)**
- Değişen dosya: `app/src/main/java/com/example/util/LiveSyncManager.kt` — `ERROR_BACKOFF_JITTER_MS` + `errorBackoffDelayMs()` eklendi, 3 çağrı noktası güncellendi.
- Doğrulama: `./gradlew :app:compileDebugKotlin` başarılı. `testDebugUnitTest` çalıştırılmadı.
- Dalda ayrıca kullanıcıya ait, bu goal'e ait olmayan iki değişiklik/dosya var: `app/build.gradle.kts` versionCode/versionName bump (238→243) ve `release-upload-keystore.rar` (muhtemelen önceki bir oturumdan kalma). **Commit'e/PR'a dahil edilmedi**, olduğu gibi bırakıldı.
- Push edildi, PR açıldı: [#89](https://github.com/Retrosero/siparis_cepte/pull/89). CI + inceleme bekleniyor, `main`'e merge onayı ayrıca alınacak.

## Seni Bekleyenler (insan kararı gerekli)

1. **O0a/O0b merge onayı:** PR'lar CI'dan geçip incelenince `main`'e squash-merge edeyim mi, yoksa sen mi bakmak istersin?
2. **O0c kapsamı:** Görünürlük/metrik işini ayrı bir goal olarak mı ele alalım (Log Merkezi'nin devamı gibi), yoksa bu goal'e mi eklensin?
3. **O0d yük testi:** Ertelendi — canlıdaki test müşterisi bittikten/rahatladıktan sonra tekrar gündeme al.
4. **O0e çakışma politikası uygulaması (Faz 2):** Politika kararlaştırıldı ("son yazan kazanır + denetim izi") ama hangi entity'lerde gerçekten çakışma riski var (sipariş/tahsilat mı, yoksa ERP'siz firmalarda cari/stok düzenlemesi mi) netleşmeden koda geçilmeyecek — ayrı bir inceleme turu gerekiyor.
