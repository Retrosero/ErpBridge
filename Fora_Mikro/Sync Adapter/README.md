# Sync Adapter

Sync Adapter, Mikro ERP kullanan firmalarda Android saha satis uygulamasi ile Mikro SQL veritabani arasinda guvenli ve kontrollu veri alisverisi yapmak icin tasarlanmis bir senkronizasyon urunudur.

## Ana Kararlar

- Android uygulama Mikro SQL'e veya musteri agindaki bir porta dogrudan baglanmaz.
- Android uygulama merkezi API ile JSON uzerinden haberlesir.
- Musteri bilgisayarinda .NET 8 tabanli Windows Service calisir.
- Windows Service yalnizca disariya HTTPS istegi atar; musterinin port acmasi gerekmez.
- Agent merkezi API'den bekleyen isleri ceker, Mikro SQL'e transaction icinde yazar ve basarili olunca ack gonderir.
- API key suresi bitince ERP'ye yazilmis yasal kayitlara dokunulmaz; local cache, token, kuyruk ve gecici veriler temizlenir.
- Mikro V15 ve V16 desteklenir. V15 `RECno/RECid`, V16 `Guid/uid` ayrimlari adapter katmaninda cozulur.

## Bilesenler

- Central API: Android ve agent icin cok kiracili API.
- Sync Adapter Agent: Musteri makinesinde calisan Windows Service.
- Agent UI: Ayar, durum, log ve destek paketi icin kucuk tray/WPF/WinUI uygulamasi.
- Local Store: SQLite ile ayar, kuyruk, mapping, checkpoint ve log saklama.
- Mikro Writer: Evrak ve master data kayitlarini Mikro tablolarina yazan transaction katmani.

## Gelistirme Fazlari

1. Agent shell, local store ve encrypted settings.
2. Lisans aktivasyonu, heartbeat ve expiry purge.
3. Mikro SQL baglantisi, versiyon tespiti ve schema discovery.
4. Merkezi API kontratlari ve agent pull/ack akisi.
5. Android bootstrap verileri.
6. Evrak transformer ve Mikro writer katmani.
7. Genis evrak kapsamı, observability, installer ve release.

Detaylar icin `docs/` klasorune, kod yazim kurallari icin `AGENTS.md` dosyasina, adim adim gelistirme promptlari icin `prompts/` klasorune bak.
