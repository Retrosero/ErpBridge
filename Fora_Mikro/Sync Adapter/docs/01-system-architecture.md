# 01 - System Architecture

## Hedef

Sync Adapter, Android saha satis uygulamasini Mikro ERP ile guvenli sekilde entegre eder. Musteri aginda port acilmaz; musteri makinesindeki agent merkezi API'ye disariya dogru HTTPS istekleri atar.

## Veri Akisi

1. Android kullanicisi belge, tahsilat, ziyaret veya cari kaydi olusturur.
2. Android kaydi merkezi API'ye JSON olarak gonderir.
3. Merkezi API kaydi tenant bazli job queue'ya yazar.
4. Musteri makinesindeki Sync Adapter Agent heartbeat atar ve pending job'lari ceker.
5. Agent isi local SQLite kuyruğuna durable olarak yazar.
6. Agent payload'u validate eder, Mikro transformer ile tablo modeline cevirir.
7. Agent Mikro SQL'e tek transaction icinde yazar.
8. Basariliysa local mapping/checkpoint kaydedilir ve merkezi API'ye ack gonderilir.
9. Basarisizsa transient hatalar retry edilir, data hatalari dead-letter olarak raporlanir.

## Ana Moduller

- `LicenseService`: API key, tenant, device ve expiry state yonetimi.
- `SettingsService`: encrypted settings ve UI ayarlari.
- `MikroDbConnector`: SQL baglanti, Mikro DB secimi, versiyon tespiti.
- `RemoteApiClient`: central API activate, heartbeat, pull, ack, upload islemleri.
- `LocalApiHost`: sadece localhost ve local token ile agent UI entegrasyonu.
- `SyncEngine`: batch dongusu, retry, backoff, concurrency limit.
- `QueueManager`: local job queue ve dead-letter islemleri.
- `TransformerRegistry`: document type -> transformer/writer secimi.
- `MappingService`: external id -> Mikro seri/sira/RECno/Guid mapping.
- `CheckpointService`: Mikro -> cloud incremental sync checkpointleri.
- `SchemaExplorer`: V15/V16 tablo ve alan uygunlugu.
- `AuditLogger`: redacted log, support bundle ve health eventleri.

## Central API Sorumluluklari

- Android kimlik dogrulama ve tenant izolasyonu.
- Mobil JSON kabul ve validation.
- Agent lisans aktivasyonu, heartbeat, device binding.
- Agent job queue ve ack state.
- Bootstrap veri cache'i ve mobil oturum temizleme.

## Agent Sorumluluklari

- Musteri SQL bilgilerini local encrypted saklama.
- Mikro baglanti testi ve schema discovery.
- Pending job'lari merkezi API'den cekme.
- Mikro'ya allowlist dahilinde yazma.
- Mikro'dan Android'i besleyecek verileri merkezi API'ye push etme.
- Lisans suresi bitince local cache/token/kuyruk temizleme.

## Local Store

SQLite tablolar:

- `settings`: secret olmayan ayarlar.
- `license_state`: tenant, device, expiry, state.
- `sync_jobs`: pending, processing, completed, failed, dead-letter job'lar.
- `job_attempts`: retry sayisi, hata kodu, son hata.
- `mappings`: external id ile Mikro seri/sira/RECno/Guid eslesmesi.
- `checkpoints`: Mikro -> cloud incremental sync pozisyonlari.
- `audit_logs`: redacted olay kayitlari.
- `schema_cache`: Mikro versiyon ve tablo alan cache'i.

## Guvenlik Sinirlari

- Android musteri SQL'e ulasamaz.
- Central API musteri SQL'e direkt ulasamaz.
- Agent inbound port acmaz.
- Local API sadece `127.0.0.1`.
- Secret alanlar loglanmaz ve destek paketine acik yazilmaz.
