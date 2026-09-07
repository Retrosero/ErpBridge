# 05 - Development Roadmap

## Faz 1 - Agent Shell

- .NET 8 Worker Service projesi.
- WPF veya WinUI tray/settings UI.
- SQLite local store migration altyapisi.
- Serilog veya Microsoft logging ile redacted log.
- Health status ve support bundle iskeleti.

Kabul:

- Servis Windows'ta console ve service modunda calisir.
- UI servis durumunu gosterir.
- Local store olusur ve migration versiyonu tutulur.

## Faz 2 - License Activation

- API key aktivasyonu.
- Tenant/device binding.
- Heartbeat ve token refresh.
- Expired/disabled durumda sync stop ve cache purge.

Kabul:

- Invalid API key sync baslatmaz.
- Expired lisans local cache'i temizler, ERP'ye dokunmaz.

## Faz 3 - Mikro Connector

- SQL Server baglanti testi.
- Mikro DB secimi.
- V15/V16 versiyon tespiti.
- Schema discovery ve allowlist kontrolu.

Kabul:

- UI'dan Mikro baglantisi test edilir.
- V15/V16 farki kaydedilir.

## Faz 4 - Queue and Remote API

- Agent pull jobs.
- Local durable queue.
- Retry, backoff, dead-letter.
- Ack/checkpoint akisi.

Kabul:

- Merkezi API kapaliyken job kaybi olmaz.
- Basarili Mikro yazimi olmadan ack gonderilmez.

## Faz 5 - Android Bootstrap

- Cari, stok, fiyat, bakiye, depo, kasa/banka, odeme plani ve mobil ayarlar.
- Incremental push ve checkpoint.
- Merkezi API tarafinda tenant cache.

Kabul:

- Android sadece merkezi API'den veri alir.
- Checkpoint basarisiz batch sonunda ilerlemez.

## Faz 6 - Evrak Writers MVP

- Alinan siparis.
- Satis faturasi.
- Satis irsaliyesi.
- Tahsilat.
- Yeni cari.
- Ziyaret/lokasyon/gun acilis-kapanis.

Kabul:

- Idempotency tekrar calistirmada duplicate uretmez.
- Header/satir transaction rollback testleri gecer.

## Faz 7 - Genis Evrak Kapsami

- Iade.
- Alis.
- Proforma.
- Depo transfer/nakliye.
- Sayim.
- Servis talep.

Kabul:

- Her evrak tipi icin mapping, writer ve test matrisi vardir.

## Faz 8 - Release

- MSI/MSIX/WiX installer.
- Windows Service install/uninstall.
- Signed installer ve rollback plani.
- Clean uninstall veri politikasi.

Kabul:

- Temiz Windows makinede kurulum, calisma ve kaldirma test edilir.
