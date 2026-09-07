# 01 - SpAdmin Architecture

## Hedef

SpAdmin, Sync Adapter ekosisteminin merkezi yonetim uygulamasidir. Firmalar, lisanslar, moduller, cihazlar, health durumlari, job queue ve destek paketleri buradan yonetilir.

## Aktorler

- SuperAdmin: tum tenant, lisans, modul ve sistem ayarlarini yonetir.
- Support: agent durumlari, ozet loglar, support bundle talepleri ve job hatalarini inceler.
- Sales: firma, plan, lisans suresi ve yenileme bilgilerini gorur; yetkisine gore islem yapar.
- TenantAdmin: kendi firmasina ait cihaz, mobil kullanici ve durum bilgilerini sinirli gorur.
- ReadOnly: sadece rapor ve durum ekranlarini gorur.

## Bilesenler

- Admin Web UI: yonetim ekranlari.
- Admin API: RBAC korumali yonetim endpointleri.
- Agent API: aktivasyon, heartbeat, job pull, ack ve support bundle upload.
- Mobile API: Android belge kabul ve bootstrap veri endpointleri.
- SQL Server: merkezi metadata, tenant data cache, queue ve audit.
- File/Object Storage: support bundle dosyalari.

## Veri Akisi

1. SuperAdmin tenant ve lisans olusturur.
2. SpAdmin API key uretir, sadece ilk gostermede plain text dondurur, DB'ye hash kaydeder.
3. Musteri makinesindeki Sync Adapter agent API key ile aktivasyon yapar.
4. SpAdmin tenant, cihaz, lisans ve modul kontrollerini yapar.
5. Agent access/refresh token alir ve duzenli heartbeat yollar.
6. Heartbeat cevabinda lisans durumu, modul listesi, config version ve komutlar doner.
7. Android belgeleri merkezi API'ye yollar; SpAdmin modul/lisans kontrolu sonrasi job queue'ya alir.
8. Agent job'lari ceker, Mikro'ya yazar ve ack gonderir.
9. Support ozet loglari ve gerekirse talep edilen support bundle'i SpAdmin'den inceler.

## Sync Adapter Baglantisi

SpAdmin, Sync Adapter icin control plane'dir:

- Lisans kontrolu.
- Tenant config.
- Modul flags.
- Agent commands.
- Job queue.
- Ack ve Mikro sonuc metadata.
- Health/log summary.

SpAdmin, musteri SQL veritabanina dogrudan baglanmaz.

## Deployment

- Web/API uygulamasi: .NET 8.
- DB: SQL Server.
- Reverse proxy/load balancer: TLS termination.
- Background workers: expiry checks, job lease cleanup, retention cleanup, notification jobs.
- Storage: support bundle dosyalari icin local secure storage veya object storage.
