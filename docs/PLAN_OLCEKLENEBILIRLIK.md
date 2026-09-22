# Ölçeklenebilirlik Faz Planı — 100 Firma / 1000 Kullanıcı Hedefi

> Durum: 2026-09-22 itibariyle 1 firma test aşamasında, 10 bekleyen müşteri var,
> 0-6 ay içinde ~100 kullanıcılık canlı kullanım hedefleniyor. Bu doküman kod/config
> incelemesine dayanır (yorum değil, dosya referanslı bulgu). Henüz kod yazılmadı —
> bu bir karar/öncelik haritasıdır.

## Sonuç (TL;DR)

Mevcut mimari **100 firma / 1000 eşzamanlı kullanıcıya hazır değil**, ama bunun
nedeni "telefonun gücü" değil — istemci tarafı zaten doğru tasarlanmış (delta sync,
sayfalama, idempotency). Asıl sınır **merkezi ErpBridge.CentralApi tek instance
olarak yazılmış** ve bunun farkında: kod içinde doğrudan şu yorum var
(`Program.cs:209-210, 222`):

> "Single replica only — multi-instance scale would need a Redis backplane"

Yani bu bir sürpriz değil, bilinen ve ertelenmiş bir sınır. 0-6 aylık ~100
kullanıcı hedefi için mevcut tek-sunucu mimarisi muhtemelen yeterli (aşağıda Faz 0-1),
ama 100 firma/1000 kullanıcı hedefi için Faz 3'teki mimari değişiklik (yatay
ölçekleme) kaçınılmaz.

## Kanıtlanmış bulgular (dosya referanslı)

**ErpBridge.CentralApi (sunucu):**
- PostgreSQL bağlantı havuzu hiç ayarlanmamış, Npgsql varsayılanı (max 100) kullanılıyor — `appsettings.example.json:3`, `Program.cs:236-237`.
- PgBouncer/ayrı pooler yok, Postgres Coolify-managed tek konteyner — `docker-compose.coolify.yml`.
- `docker-compose.coolify.yml`'de CPU/RAM limiti tanımlı değil (kaynak sınırsız paylaşım, izole değil).
- Notify/senkron mekanizması SignalR değil, custom long-polling: `/api/v1/android/notify?wait=` isteği sunucuda 1-60 sn (varsayılan 30 sn) açık tutuluyor, tamamen **process-içi** `ConcurrentDictionary` ile (`TenantEventHub.cs`, `BootstrapNotificationHub.cs`). Redis/DB backplane yok → bu nedenle instance sayısı artırılamıyor.
- Multi-tenancy: shared-schema + `TenantId` kolonu, hot tablolarda index'in ilk kolonu doğru şekilde `TenantId` (`change_sets`, `mobile_sync_queue`, `mobile_records`) — bu kısım sağlam.
- Rate limiting **var**: per-agent/per-tenant/global fixed-window limitleri (`Program.cs:440-536`), ama `QueueLimit = 0` — limit aşımında istek kuyruğa alınmadan direkt 429 dönüyor (yumuşak geçiş yok).
- Ana ingest/senkron yazımları (`IngestEndpoints.cs`, `ChangeSetEndpoints.cs`) kuyruğa alınmıyor; `SaveChangesAsync` doğrudan request thread'inde çalışıyor. Hangfire/Quartz yok.
- Cache sadece process-içi `IMemoryCache` (Redis yok) — bu da instance sayısını 1'e kilitleyen ikinci sebep.
- **Hiçbir yük testi / kapasite ölçümü belgesi yok** — repo genelinde grep sıfır sonuç verdi. Yukarıdaki her şey teorik risk, ölçülmüş değil.

**Siparis_Cepte (mobil):**
- Gerçek delta/cursor bazlı senkron (tam tablo çekmiyor), sayfalama 500-1000 kayıt, DB toplu yazımlar chunk'lı — bu kısım doğru tasarlanmış.
- Ön planda `LiveSyncManager` sunucuya sürekli long-poll bağlantısı tutuyor (`wait=30`, hata sonrası 4 sn bekleyip yeniden bağlanıyor). 1000 aktif kullanıcı = sunucuda ~1000 eşzamanlı açık bağlantı potansiyeli.
- **Jitter yok**: ne WorkManager'ın exponential backoff'unda ne `LiveSyncManager`'ın sabit 15 sn hata gecikmesinde rastgelelik var. Sunucu kısa süreliğine yavaşlarsa/yeniden başlarsa, yüzlerce cihaz aynı anda aynı ritimde tekrar dener → "thundering herd" riski koddan doğrulandı.
- İdempotency var (`documentType, externalId` unique index, hem istemci hem sunucuda) — mükerrer kayıt riski düşük.
- **Optimistic locking / çakışma çözümü yok**: her dokümanda `revision` alanı var ama sabit `1` gönderiliyor, sunucu tarafında artırma/çakışma kontrolü kodda görünmüyor. İki farklı cihazdan aynı cari/stok üzerinde eşzamanlı işlem yapılırsa sessiz veri kaybı riski var — bu, kullanıcı sayısı değil **aynı firmadaki eşzamanlı kullanıcı sayısı** arttıkça büyüyen bir risk.

## Faz Haritası

### Faz 0 — Görünürlük ve ucuz düzeltmeler (0-6 ay hedefi öncesi, ~100 kullanıcıya çıkmadan önce yapılmalı)
Maliyet: düşük (sadece mühendislik zamanı, altyapı değişikliği yok).
1. Npgsql connection pool'u açıkça ayarla (Max/Min Pool Size, Timeout) ve Postgres `max_connections` ile uyumlu hale getir.
2. Tüm retry/backoff noktalarına jitter ekle (WorkManager + `LiveSyncManager`'ın sabit 15 sn gecikmesi) — thundering herd riskini azaltır.
3. Gerçek bir yük testi yap: mevcut VPS üzerinde N eşzamanlı long-poll + gerçekçi yazma yükü simüle et (örn. k6/locust). Şu an her şey teorik — gerçek tavan sayısını ölçmeden Faz 1-3'e karar verilmemeli.
4. Temel görünürlük: CPU/RAM/DB bağlantı sayısı/aktif long-poll sayısı için basit bir metrik/uyarı ([[Log Merkezi goal]] ile birleştirilebilir) — sorunu kör noktada değil, görerek yakala.
5. Çakışma politikasına karar ver: en azından "son yazan kazanır + denetim izi" mi, yoksa `revision` alanını gerçek optimistic concurrency kontrolüne mi çevireceksin — bu bir tasarım kararı, kod yazmadan önce netleşmeli.

### Faz 1 — Yazma yolunu sağlamlaştırma (~100 kullanıcı canlıya çıkarken)
Maliyet: düşük-orta (mühendislik zamanı + belki küçük dikey büyütme).
1. Faz 0'daki yük testi sonucuna göre VPS'i gerekiyorsa dikey büyüt (en ucuz seçenek — yeni mimari gerekmez).
2. `docker-compose.coolify.yml`'e CPU/RAM limiti koy (komşu container'lar birbirini boğmasın).
3. En ağır yazma uçlarını (ingest/changeset) request thread'inden ayırmayı değerlendir (tam kuyruk sistemi şart değil, en azından burst'leri yumuşatacak bir ara adım).
4. Rate limit `QueueLimit`'i 0'dan küçük bir değere çekmeyi değerlendir (sert 429 yerine kısa kuyruklama — meşru burst'lerde kullanıcı deneyimini korur).

### Faz 2 — Çoklu-kullanıcı veri bütünlüğü (bir firmada birden fazla saha satışçısı yaygınlaştıkça)
Maliyet: düşük-orta (mühendislik zamanı ağırlıklı).
1. `revision` alanını gerçek optimistic concurrency kontrolüne çevir (409 + çakışma çözme akışı).
2. PostgreSQL'i aynı sunucuda ayrı kaynak rezervasyonuyla izole et ya da (bütçeye göre) yönetilen (managed) Postgres'e taşımayı değerlendir.

### Faz 3 — Yatay ölçekleme (100 firma / 1000 kullanıcı hedefine yaklaşırken)
Maliyet: orta-yüksek (yeni altyapı bileşenleri gerekir — kod da bunu zaten bekliyor).
1. Redis ekle: hem notify hub'ı (`TenantEventHub`) hem `IMemoryCache`'i Redis backplane'e taşı — bu olmadan 2. bir CentralApi instance'ı **çalışmaz**, kodun kendisi bunu söylüyor.
2. CentralApi'yi birden fazla replica + load balancer arkasında çalıştır (Coolify bunu destekler).
3. PgBouncer veya yönetilen/havuzlu Postgres — N replica × pool size çarpımı tek Postgres'i boğmasın diye.
4. Gerçek arka plan iş kuyruğu (Hangfire vb.) — yazma yükünü istek gecikmesinden ayır.

## Maliyet karşılaştırması (Hostinger KVM VPS planlarına göre)

Sunucu Hostinger KVM VPS. Güncel plan tablosu (2026, tanıtım fiyatı — yenileme
Hostinger'da genelde daha yüksektir, satın alırken kontrol et):

| Plan | vCPU | RAM | Disk | Aylık (tanıtım) |
|---|---|---|---|---|
| KVM 1 | 1 | 4 GB | 50 GB NVMe | ~$4.99 |
| KVM 2 | 2 | 8 GB | 100 GB NVMe | ~$8.99 |
| KVM 4 | 4 | 16 GB | 200 GB NVMe | ~$14.99 |
| KVM 8 | 8 | 32 GB | 400 GB NVMe | ~$29.99 |

Hostinger'da **yönetilen (managed) PostgreSQL veya Redis satılmıyor** — bunlar
şu an olduğu gibi VPS üzerinde Docker/Coolify ile kendin barındırıyorsun ve
büyüdükçe de öyle kalacak (ya aynı VPS'te, ya ayrı bir Hostinger VPS'te).
Alternatif olarak Redis/Postgres için ayrı bir sağlayıcı (Upstash, Neon, Aiven
vb.) kullanılabilir ama bu, Hostinger dışına çıkıp ek gecikme ve ayrı fatura
demek — önerilen varsayılan Hostinger içinde kalmak.

| Faz | Ek altyapı maliyeti | Ne zaman gerekli |
|---|---|---|
| 0 | Yok (sadece kod/ölçüm) | Hemen, 100 kullanıcıya çıkmadan önce |
| 1 | Düşük — mevcut plan KVM 1/2 ise KVM 4'e geçiş (~+$6-10/ay) | Yük testi tavanı gösterirse |
| 2 | Düşük — aynı sunucuda kalınabilir; ayrı DB sunucusu istenirse ikinci bir KVM 2 (~+$9/ay) | Aynı firmada çoklu eşzamanlı kullanıcı yaygınlaşınca |
| 3 | Orta — Redis için ikinci küçük VPS (KVM 1/2, ~$5-9/ay) + CentralApi için KVM 4/8 (~$15-30/ay) + gerekirse yük dengeleyici | 100 firma/1000 kullanıcıya gerçekten yaklaşırken |

Yani 100 firma/1000 kullanıcı hedefine tam çıkana kadar toplam sunucu maliyeti
muhtemelen ayda $30-50 bandında kalır (Hostinger içinde kalınırsa) — bu ölçekte
büyük bir bütçe kalemi değil, asıl maliyet mühendislik zamanı (Faz 0-2'deki
kod değişiklikleri).

## Ayrı onay gerektiren noktalar
- Faz 1: VPS büyütme kararı (maliyet artışı).
- Faz 2: Managed Postgres'e geçiş kararı (mevcut veri taşıma riski + maliyet).
- Faz 3: Redis + çoklu instance mimarisine geçiş (en büyük mimari değişiklik, muhtemelen ayrı bir teknik tasarım turu gerektirir).
