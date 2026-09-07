# 04 - Security and License Expiry

## Aktivasyon

Agent sync baslatmadan once su bilgileri merkezi API'ye gonderir:

- API key
- Tenant id
- Machine fingerprint
- Agent version
- OS bilgisi

Merkezi API valid lisans icin access token, refresh token, expiry ve config version dondurur.

## Secret Saklama

Agent tarafinda su alanlar DPAPI veya Credential Manager ile saklanir:

- API key
- Access token
- Refresh token
- SQL password
- Connection string parcalari

SQLite icinde secret alanlar plain text tutulmaz.

## Log Redaction

Su degerler log, UI, support bundle ve crash report icinde maskelenir:

- API key
- Bearer token
- SQL password
- Full connection string
- Refresh token
- Musteri ozel gizli alanlar

Redaction uygulamasi unit test ile dogrulanir.

## License State

Lisans durumlari:

- `valid`: sync calisir.
- `expiring`: sync calisir, UI uyari verir.
- `expired`: yeni sync durur ve purge politikasi uygulanir.
- `disabled`: sync durur, local cache temizlenir.
- `device_mismatch`: sync baslamaz.

## Expiry Purge Politikasi

API key suresi bitince silinecekler:

- Agent access token ve refresh token.
- Local bootstrap cache.
- Bekleyen ve islenmemis local job queue.
- Merkezi API'deki gecici mobile session verileri.
- Merkezi API'deki agent temp cache verileri.

Silinmeyecekler:

- Mikro ERP'ye yazilmis muhasebe evraklari.
- Mikro standart tablolarindaki yasal kayitlar.
- Audit amacli redacted minimum lisans olaylari.

## Offline Davranis

- Agent merkezi API'ye ulasamiyorsa son bilinen lisans durumuna gore kisa grace period kullanabilir.
- Grace period suresi merkezi config ile belirlenir.
- Grace period sonunda merkezi API hala ulasilamazsa sync durur ama ERP kayitlari silinmez.

## Local API

- Varsayilan bind: `127.0.0.1`.
- Her endpoint local token ister.
- Remote bind ancak explicit ayar ve guvenlik uyarisi ile acilabilir.
- Local API arbitrary SQL veya tablo gezme endpoint'i sunmaz.
