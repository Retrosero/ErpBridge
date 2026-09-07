# Sync Adapter Agent Rules

Bu dosya Sync Adapter gelistirilirken tum agentlar ve gelistiriciler icin baglayici kurallari tanimlar.

## Mimari Kurallar

- Varsayilan mimari pull-mode olacaktir: musteri makinesindeki agent merkezi API'ye HTTPS ile baglanir.
- Musteri tarafinda inbound port acilmayacaktir.
- Android uygulama musteri SQL bilgisi, agent adresi veya Mikro veritabani bilgisi bilmeyecektir.
- Central API cok kiracili kaynak olarak lisans, konfig, kuyruk, ack ve Android bootstrap verilerini yonetecektir.
- Agent tarafinda surekli calisma icin Windows Service, ayar/durum icin kucuk UI kullanilacaktir.
- Local API gerekiyorsa yalnizca `127.0.0.1` uzerinden calisir ve local token ister.

## Guvenlik Kurallari

- SQL sifresi, API key, access token, refresh token ve connection string loglanmayacaktir.
- Secret alanlar Windows DPAPI veya Windows Credential Manager ile saklanacaktir.
- Destek paketi, hata mesaji, crash report ve UI ciktisi secret redaction uygulamak zorundadir.
- Arbitrary SQL endpoint veya serbest tablo sorgulama endpoint'i eklenmeyecektir.
- Tum entity ve tablolar allowlist ile sinirlandirilacaktir.
- Tum SQL komutlari parameterized query kullanacaktir. String concatenation ile kullanici verisi SQL'e eklenmeyecektir.

## Sync Kurallari

- Kuyruk once, transformer sonra, Mikro transaction sonra, ack/checkpoint en son gelir.
- Bir is merkezi API'den alindiktan sonra local store'a durable olarak yazilmadan islenmeyecektir.
- Ack yalnizca Mikro yazimi ve local mapping/checkpoint kalici olduktan sonra gonderilir.
- Checkpoint basarisiz batch veya kismi batch sonunda ilerletilmez.
- Transient hatalar retry edilir: timeout, deadlock, network, API 5xx.
- Data hatalari retry edilmez; is dead-letter durumuna alinir ve acik hata mesaji kaydedilir.

## Mikro Yazim Kurallari

- Duplicate evrak olusturmak yasaktir. Her evrak `tenant_id + document_type + external_id` ile idempotent olacaktir.
- Header yazilip satir yazilamayan durum yasaktir; tum ilgili Mikro satirlari tek SQL transaction icinde yazilacaktir.
- Transaction baslamadan once payload, mapping ve zorunlu lookup kontrolleri yapilir.
- V15/V16 farklari adapter katmaninda cozulur; is kurali kodu `RECno` veya `Guid` detayina baglanmaz.
- V15 icin `*_RECid_DBCno` ve `*_RECid_RECno`, V16 icin `*_uid` ve `Guid` alanlari dogru baglanir.
- Mikro alan uzunluklari asildiginda veri kurala gore kirpilir veya data hatasi uretilir; sessiz veri bozulmasi yapilmaz.

## Evrak Kapsami

- Satis faturasi: `CARI_HESAP_HAREKETLERI` + `STOK_HAREKETLERI`.
- Satis irsaliyesi: `STOK_HAREKETLERI`.
- Alinan siparis: `SIPARISLER`.
- Tahsilat: `CARI_HESAP_HAREKETLERI`, gerekiyorsa `ODEME_EMIRLERI`.
- Depo transfer/nakliye: `STOK_HAREKETLERI`.
- Yeni cari: `CARI_HESAPLAR`, gerekiyorsa `CARI_HESAP_ADRESLERI`.
- Ziyaret/lokasyon/gun acilis-kapanis: Mikro standart tablolarini kirletmeden `SA_*` prefix'li proje ozel tablolari.

## Test Kurallari

- Idempotency testi olmadan writer tamamlanmis sayilmaz.
- Transaction rollback testi olmadan evrak writer tamamlanmis sayilmaz.
- Log redaction testi olmadan security kabul edilmez.
- API key valid, expired, disabled, wrong tenant ve wrong device senaryolari test edilir.
- V15 ve V16 icin ayni payload testleri calisir.
