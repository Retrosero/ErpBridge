# Goal Durumu — Ölçeklenebilirlik Faz 0

Son güncelleme: 2026-09-22
Görev listesi: [GOAL_OLCEKLENEBILIRLIK.md](GOAL_OLCEKLENEBILIRLIK.md)

> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti (push/PR onayı bekliyor) · ⏭️ karar/onay bekliyor · ⛔ tıkandı

---

## Özet

| Görev | Durum |
|---|---|
| O0a — Npgsql pool sınırı | ✅ kod yazıldı, derleme yeşil, **push onayı bekliyor** |
| O0b — LiveSyncManager jitter | ✅ kod yazıldı, derleme yeşil, **push onayı bekliyor** |
| O0c — Görünürlük/metrik | ⏭️ kapsam netleşmedi |
| O0d — Gerçek yük testi | ⏭️ ne zaman/nasıl çalıştırılacağına onay bekliyor |
| O0e — Çakışma politikası kararı | ⏭️ karar bekliyor |

## Detaylar

**O0a — ErpBridge (`faz-olceklenebilirlik-goal` dalı)**
- Değişen dosya: `src/ErpBridge.CentralApi/Program.cs` — `ApplyPoolDefaults` eklendi, `ConfigureData` içinde kullanılıyor.
- Doğrulama: `dotnet build src/ErpBridge.CentralApi/ErpBridge.CentralApi.csproj -c Debug` → 0 uyarı/0 hata.
- Henüz commit edilmedi (kullanıcı push/PR onayını verince commit + push + PR açılacak).

**O0b — Siparis_Cepte (`faz-olceklenebilirlik-goal` dalı)**
- Değişen dosya: `app/src/main/java/com/example/util/LiveSyncManager.kt` — `ERROR_BACKOFF_JITTER_MS` + `errorBackoffDelayMs()` eklendi, 3 çağrı noktası güncellendi.
- Doğrulama: `./gradlew :app:compileDebugKotlin` başarılı. `testDebugUnitTest` çalıştırılmadı.
- Dalda ayrıca kullanıcıya ait, bu goal'e ait olmayan bir değişiklik var: `app/build.gradle.kts` versionCode/versionName bump (238→243, muhtemelen önceki bir oturumdan kalma). **Bu commit'e dahil edilmeyecek**, olduğu gibi bırakıldı.
- Henüz commit edilmedi.

## Seni Bekleyenler (insan kararı gerekli)

1. **O0a/O0b push+PR onayı:** İki dalı da push edip PR açayım mı, yoksa önce sen mi bakmak istersin?
2. **O0c kapsamı:** Görünürlük/metrik işini ayrı bir goal olarak mı ele alalım (Log Merkezi'nin devamı gibi), yoksa bu goal'e mi eklensin?
3. **O0d yük testi:** Canlıda gerçek bir test müşterin var — yük testini nerede/ne zaman çalıştıralım (örn. bakım penceresi, ya da ayrı bir dev/staging ortamı kurulur mu)?
4. **O0e çakışma politikası:** "Son yazan kazanır + denetim izi" mi, yoksa gerçek optimistic concurrency (409 + çakışma çözme ekranı) mı istiyorsun?
