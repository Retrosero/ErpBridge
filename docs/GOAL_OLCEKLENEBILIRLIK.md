# Goal — Ölçeklenebilirlik Faz 0 (100 Firma / 1000 Kullanıcı Hazırlığı)

Tarih: 2026-09-22 · Kapsam: ErpBridge.CentralApi + Siparis_Cepte (LiveSyncManager)

İlgili plan: [PLAN_OLCEKLENEBILIRLIK.md](PLAN_OLCEKLENEBILIRLIK.md). Bu goal o planın
**Faz 0'ını** (görünürlük + ucuz düzeltmeler) üstlenir. Faz 1-3 (VPS büyütme, managed DB,
Redis/çoklu instance) bu goal'ün dışındadır — her biri ayrı maliyet/mimari onayı gerektirir.

İlerleme **[GOAL_OLCEKLENEBILIRLIK_DURUM.md](GOAL_OLCEKLENEBILIRLIK_DURUM.md)** dosyasına
yazılır.

---

## 0. Bağlam ve kararlar

- Sunucu: Hostinger KVM VPS (küçük plan, 2-4 vCPU / 4-8 GB RAM), PostgreSQL aynı sunucuda, Coolify ile yönetiliyor.
- Şu an 1 firma test aşamasında, 10 bekleyen müşteri var; 0-6 ay içinde ~100 kullanıcılık canlı kullanım hedefleniyor.
- Kod incelemesiyle doğrulanan ana bulgu: CentralApi bilinçli olarak tek instance yazılmış
  (`Program.cs` yorumu: *"Single replica only — multi-instance scale would need a Redis
  backplane"*), Npgsql bağlantı havuzu hiç sınırlanmamış, mobil tarafta hata sonrası
  yeniden deneme mantığında jitter yok.
- Altyapı bütçesi kararı **henüz verilmedi** — kullanıcı önce seçenek/maliyet karşılaştırması
  istedi (bkz. PLAN dosyasındaki tablo). Bu yüzden Faz 0 bilinçli olarak **sıfır ek altyapı
  maliyeti** gerektiren maddelerden seçildi.

## 1. Görevler

| ID | Görev | Neden | Durum |
|---|---|---|---|
| O0a | ErpBridge.CentralApi: Npgsql `Maximum Pool Size`'ı operatör ayarlamamışsa 60'a sabitle (`Program.cs`, `ConfigureData`) | Tek instance'ın kendisi Postgres'in varsayılan `max_connections` (100) sınırının tamamını tüketip migration/health-check/manuel bağlantıya yer bırakmayabilir | ✅ |
| O0b | Siparis_Cepte: `LiveSyncManager`'ın sabit 15 sn hata gecikmesine rastgele jitter (0-5 sn) ekle | Sunucu kısa süre yavaşlar/yeniden başlarsa yüzlerce cihaz aynı anda aynı ritimde tekrar deniyordu (thundering herd) | ✅ |
| O0c | Temel görünürlük: CPU/RAM/DB bağlantı sayısı/aktif long-poll sayısı için metrik | Sorunu kör noktada değil, ölçerek yakalamak için; [[Log Merkezi goal]] altyapısıyla birleştirilebilir | ⏭️ kapsam netleşmedi — ayrı görüşme gerekir (Log Merkezi ile çakışmasın diye) |
| O0d | Gerçek yük testi: mevcut VPS'te simüle edilmiş N eşzamanlı `/notify` long-poll + yazma yükü | Şu ana kadarki her şey teorik risk; gerçek tavan sayısı olmadan Faz 1 kararı (VPS büyütme) veri olmadan alınır | ⏭️ **onay bekliyor** — canlıda 1 gerçek test müşterisi olduğu için üretim sunucusuna karşı ne zaman/nasıl çalıştırılacağına kullanıcı karar vermeli |
| O0e | Çakışma politikası kararı: `revision` alanını gerçek optimistic concurrency kontrolüne çevirmek mi, yoksa "son yazan kazanır + denetim izi" mi | İki farklı cihazdan aynı cari/stok üzerinde eşzamanlı işlemde sessiz veri kaybı riski var; bu bir tasarım kararı, kod yazmadan önce netleşmeli | ⏭️ **karar bekliyor** |

## 2. Yetkiler

CLAUDE.md'deki **varsayılan kural geçerli**: dala push ve PR açma onay gerektirir, bu goal
için ayrıca bir istisna verilmedi. O0a ve O0b değişiklikleri ilgili dallarda
(`faz-olceklenebilirlik-goal`) yapıldı ve derleme doğrulandı; push/PR için kullanıcı onayı
bekleniyor.

**Asla:** `--force` push, `main`'e doğrudan push, canlı Hostinger VPS'e onaysız yük testi,
üretim veritabanında deneme yazımı.

## 3. Test / build

- ErpBridge: `dotnet build src/ErpBridge.CentralApi/ErpBridge.CentralApi.csproj -c Debug` → 0 uyarı / 0 hata (doğrulandı).
- Siparis_Cepte: `./gradlew :app:compileDebugKotlin` → başarılı, yeni uyarı yok (doğrulandı). `testDebugUnitTest` henüz çalıştırılmadı.
