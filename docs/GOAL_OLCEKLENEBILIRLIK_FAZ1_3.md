# Goal — Ölçeklenebilirlik Faz 1-3 (100 Firma / 1000 Kullanıcıya Doğru)

Tarih: 2026-09-22 · Kapsam: ErpBridge.CentralApi (öncelikli), Siparis_Cepte (F2'de dokunma ihtimali)

İlgili belgeler: [PLAN_OLCEKLENEBILIRLIK.md](PLAN_OLCEKLENEBILIRLIK.md) (analiz + faz tanımı),
[GOAL_OLCEKLENEBILIRLIK.md](GOAL_OLCEKLENEBILIRLIK.md) / `_DURUM.md` (Faz 0, bitti).

Bu belge otonom çalışma içindir. İlerleme
**[GOAL_OLCEKLENEBILIRLIK_FAZ1_3_DURUM.md](GOAL_OLCEKLENEBILIRLIK_FAZ1_3_DURUM.md)**
dosyasına yazılır; her görevden sonra güncellenir.

---

## 0. Önemli sınır: bu goal'ün YAPAMAYACAĞI şeyler

Bu bir kod/yazılım asistanı goal'ü. Aşağıdakiler bu belgenin **kapsamı dışındadır**
çünkü ne teknik erişimim (Hostinger/Coolify panelleri) ne de yetkim (para harcama)
var. Görev listesinde bu maddelere gelindiğinde **kod/config hazırlanır ama
gerçek uygulama insan eliyle yapılır** — otonom çalışma orada durur, iş
`_DURUM.md` > "Seni Bekleyenler"e yazılır, sıradaki bağımsız göreve geçilir:

- Hostinger'da VPS planı yükseltme (ödeme gerektirir).
- Coolify panelinde yeni servis/instance açma, ikinci bir VPS satın alma/kurma.
- Canlı Hostinger VPS'ine karşı gerçek yük testi çalıştırma (üzerinde gerçek
  test müşterisi varken risk; kullanıcı bunu 2026-09-22'de zaten ertelemişti).
- Managed Postgres/Redis sağlayıcısına gerçek geçiş (yeni hesap/fatura).

Bu maddelerin karşılığı olan kod (Redis entegrasyonu, PgBouncer config, Hangfire
vb.) yine de yazılır ve **yerel/izole ortamda** test edilir — sadece "canlıya
gerçekten geçirme" adımı insan kapısıdır.

## 1. Bağlam (PLAN'dan taşındı)

- Sunucu: Hostinger KVM VPS (küçük plan), PostgreSQL aynı sunucuda, Coolify.
- CentralApi bilinçli tek instance yazılmış (`Program.cs` yorumu: Redis
  backplane olmadan çoklu instance çalışmaz).
- Faz 0 bitti: Npgsql pool sınırı + mobil jitter main'de.
- O0d (yük testi) canlıda ertelendi — bu goal'de **yerel/izole** bir yük testi
  kurulumu yazılacak (F1a), canlıya karşı değil.
- O0e çakışma politikası kararı: **"son yazan kazanır + denetim izi"**. Hangi
  entity'lerde gerçek risk var, netleşmemişti — bu goal'in F2a görevi bunu
  koddan doğrulayıp F2b'nin kapsamını belirleyecek.

## 2. Görevler

### Faz 1 — Yazma yolunu sağlamlaştırma

| ID | Görev | Tip | Notlar |
|---|---|---|---|
| F1a | Yerel/izole yük testi kurulumu: docker-compose ile CentralApi + Postgres ayağa kaldırıp k6 (veya benzeri) script'iyle N eşzamanlı `/notify` long-poll + yazma isteği simüle et | 🤖 otonom | Canlı Hostinger VPS'ine dokunmaz; sonuç mutlak tavan sayısı değil, göreli darboğaz tespiti (CPU mu, bağlantı havuzu mu, thread pool mu) |
| F1b | F1a sonuçlarını `docs/` altına kısa bir bulgu raporu olarak yaz | 🤖 otonom | Faz 1f (VPS büyütme) kararını insan verirken kullanılacak veri |
| F1c | `docker-compose.coolify.yml`'e CentralApi/Postgres için CPU/RAM limiti ekle | 🤖 otonom | Komşu container birbirini boğmasın |
| F1d | En ağır ingest/changeset yazma uçlarını request thread'inden ayırmayı değerlendir; gerekiyorsa hafif bir in-process kuyruk (System.Threading.Channels) ile uygula — tam Hangfire değil, sadece burst'leri yumuşat | 🤖 otonom | F3e'deki gerçek Hangfire kuyruğuyla çakışmasın diye kapsamı dar tut |
| F1e | Rate limiter `QueueLimit`'i 0'dan küçük bir kuyruklama değerine çek (`Program.cs:440-536`) | 🤖 otonom | Sert 429 yerine kısa kuyruklama; meşru burst'lerde kullanıcı deneyimi korunur |
| F1f | VPS büyütme kararı ve uygulaması (Hostinger panelinden plan yükseltme) | 👤 **insan kapısı** | F1b'nin raporu karar için veri sağlar; ben yapamam |

### Faz 2 — Çoklu-kullanıcı veri bütünlüğü

| ID | Görev | Tip | Notlar |
|---|---|---|---|
| F2a | Hangi entity'lerde gerçek eşzamanlı çakışma riski var, koddan doğrula: sipariş/tahsilat/iade gibi belgeler mi (idempotency zaten var, edit yok), yoksa ERP'siz firmada cari/stok düzenlemesi mi (ERP'li firmada zaten uygulamadan değiştirilemiyor) | 🤖 otonom (araştırma) | Şema/davranış **tahmin edilmeyecek** — knowledge_base + kod + gerekirse canlı DB'den doğrulanacak |
| F2b | F2a'da bulunan gerçek risk taşıyan entity(ler) için "son yazan kazanır + denetim izi" uygula: üzerine yazma anını tespit eden bir kontrol + kim/ne zaman/eski değer kaydeden bir denetim tablosu/log kaydı | 🤖 otonom | Kapsam F2a'nın bulgusuna göre daralır/genişler; migration gerekebilir |
| F2c | PostgreSQL izolasyonu / managed DB seçenek analizi (maliyet + geçiş riski) | 🤖 otonom (analiz) | Gerçek geçiş F2d'de |
| F2d | Managed Postgres'e gerçek geçiş veya aynı sunucuda kaynak rezervasyonu | 👤 **insan kapısı** | Veri taşıma riski + yeni fatura kalemi |

### Faz 3 — Yatay ölçekleme

| ID | Görev | Tip | Notlar |
|---|---|---|---|
| F3a | `TenantEventHub` ve `IMemoryCache` kullanımını Redis backplane'e taşıyacak soyutlama yaz (StackExchange.Redis); **config'te Redis tanımlı değilse mevcut in-memory davranışa düş** (tek instance'ta hâlâ Redis'siz çalışsın) | 🤖 otonom | Redis sunucusu olmadan da derlenip test geçmeli — geriye dönük uyumluluk şart |
| F3b | `docker-compose.coolify.yml`'e (etkisiz/opsiyonel) bir `redis` servis tanımı ekle | 🤖 otonom | Coolify'da fiilen devreye alınması ayrı adım (F3f) |
| F3c | PgBouncer için docker-compose config'i hazırla (opsiyonel servis) | 🤖 otonom | Aktivasyonu F3f'de |
| F3d | Gerçek arka plan iş kuyruğu (Hangfire, Postgres storage) entegrasyonu — F1d'deki hafif kuyruktan farklı, kalıcı/izlenebilir kuyruk | 🤖 otonom | Yeni sunucu gerektirmez, sadece ek Postgres tablosu (migration) |
| F3e | F3a-F3d'nin tümü yerel/izole ortamda (docker-compose ile Redis + PgBouncer + 2 CentralApi container) uçtan uca test edilsin | 🤖 otonom | Bu, "çoklu instance gerçekten çalışıyor mu" sorusunun kanıtı olacak |
| F3f | Coolify'da gerçek çoklu instance + load balancer + Redis + PgBouncer'ı canlıya alma | 👤 **insan kapısı** | Coolify panel erişimi ve olası ikinci VPS gerektirir |

## 3. Yetkiler

CLAUDE.md'deki **varsayılan kural geçerli**: dala push ve PR açma onay gerektirir.
Bu goal'e CLAUDE.md üzerinden kalıcı bir istisna **eklenmedi** — Claude Code'un
kendi güvenlik katmanı (auto-mode classifier) bir asistanın kendi CLAUDE.md
kural dosyasını düzenleyip kendine yetki tanımasını ("Self-Modification")
engelliyor. Yani:

- Eğer bu goal'ü tam otonom çalıştırmak istiyorsan, **CLAUDE.md'ye istisna
  satırını sen eklemelisin** (diğer goal'lerdeki kalıpla aynı: "İstisna
  (tarih): `docs/GOAL_OLCEKLENEBILIRLIK_FAZ1_3.md` görevleri için push/PR/merge
  önceden onaylıdır").
- Eklemezsen, her görev sonrası push/PR açılır ama `main`'e merge için ayrı
  onayını isterim (2026-09-22'de Faz 0'da yaptığım gibi, tur tur onay).

**Asla:** `--force` push, `main`'e doğrudan push, CI kırmızıyken merge, canlı
Hostinger VPS'ine onaysız yük testi, gerçek altyapı harcaması/satın alma.

## 4. Test / build

- ErpBridge: `dotnet build ErpBridge.sln -c Debug` (0 uyarı/0 hata) + `dotnet test ErpBridge.sln`.
- F3e için ek: docker-compose ile yerel çoklu-instance senaryosu manuel/script ile doğrulanır (birim test değil, entegrasyon senaryosu).
- Siparis_Cepte'ye dokunulursa: `./gradlew :app:compileDebugKotlin :app:testDebugUnitTest`.
