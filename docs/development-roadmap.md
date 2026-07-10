# ErpBridge — Development Roadmap

## Faz 1 — Proje iskeleti ✅ (Mavis Faz 1)

- Solution + 8 proje + 3 test projesi
- DI, Serilog, SQLite altyapısı
- WPF shell, Service shell, ERP abstraction, Mikro skeleton, Remote API skeleton
- Örnek testler
- `dotnet build` temiz, `dotnet test` çalışıyor

## Faz 2 — Config & local store

- SQLite migration sistemi (CREATE TABLE if not exists)
- `agent_config`, `mappings`, `local_jobs`, `checkpoints` tabloları
- WPF ekranından ayar kaydetme/okuma
- Şifre alanı `is_secret=1` flag ile saklanır (encryption sonraki fazda)
- Unit testler: `IAgentConfigStore.Save/Load` roundtrip, secret maskeleme

## Faz 3 — Mikro bağlantı testi + V15/V16 detector

- `Microsoft.Data.SqlClient` ile `SqlConnection.Open` testi
- Versiyon detector: `SERVERPROPERTY('ProductVersion')` veya Mikro'ya özgü
  metadata tabloları/sorguları
- WPF'te "Bağlantıyı test et" butonu gerçek sonuç gösterir
- Adapter cache'li versiyon bilgisi

## Faz 4 — Merkezi API iskeleti

- .NET 8 Web API + Npgsql + EF Core / Dapper
- Tenant, License, Agent, Job, JobAck modelleri
- Endpointler: `POST /agents/register`, `POST /agents/heartbeat`,
  `POST /licenses/validate`, `GET /jobs/pending`, `POST /jobs/ack`,
  `POST /bootstrap`
- JWT auth + rate limit
- xUnit + WebApplicationFactory test

## Faz 5 — Mikro bootstrap okuma

- `ReadBootstrapDataAsync` implementasyonu
- `customers` paketi: `CARI_HESAPLAR` + `CARI_HESAP_ADRESLERI` + `CARI_HESAP_YETKILILERI`
- `stocks`: `STOKLAR` + `BARKOD_TANIMLARI` + lookup tablolar
- `prices`: `STOK_SATIS_FIYAT_LISTELERI` + `SATIS_SARTLARI`
- `inventory`: `STOK_HAREKETLERI` agregatları
- `openOrders`: `SIPARISLER` (kapatılmamış, kalan miktar > 0)
- `cashAndBank`: `KASALAR` + `BANKALAR`
- `lookups`: `DEPOLAR`, `CARI_PERSONEL_TANIMLARI`, `ODEME_PLANLARI`, vs.
- Merkezi API'ye `PushBootstrapDataAsync`

## Faz 6 — Satış siparişi yazma

- `SalesOrderPayload` validation (zorunlu alanlar, lookup kontrolleri)
- Mapping idempotency kontrolü
- Mikro `SIPARISLER` insert
- V15: `SCOPE_IDENTITY()` ile `sip_RECno` → link alanları
- V16: Guid insert
- Mapping kaydı + ack

## Faz 7 — Admin panel (Web)

- Firma oluşturma / lisans oluşturma
- Agent listesi ve son heartbeat
- Pending / failed joblar ve detay ekranı

## Faz 8+ — Sonraki modüller

- Tahsilat (`CARI_HESAP_HAREKETLERI` + `ODEME_EMIRLERI`)
- İrsaliye (`STOK_HAREKETLERI`)
- Fatura (`CARI_HESAP_HAREKETLERI` + `STOK_HAREKETLERI`)
- Cari kart açma / Stok kart açma
- Logo adapter
- Paraşüt adapter