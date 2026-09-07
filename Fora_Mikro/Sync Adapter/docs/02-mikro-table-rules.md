# 02 - Mikro Table Rules

## Genel Yazim Sirasi

1. Payload semasi ve zorunlu alanlar validate edilir.
2. `tenant_id + document_type + external_id` ile idempotency kontrol edilir.
3. Lookup alanlari dogrulanir: cari, stok, depo, kasa, banka, odeme plani, doviz, temsilci.
4. Mikro versiyonu tespit edilir: V15 veya V16.
5. Seri/sira belirlenir ve cakisma kontrol edilir.
6. SQL transaction baslatilir.
7. Header, satirlar, aciklamalar, baglanti alanlari ve gerekiyorsa siparis karsilama update'leri yazilir.
8. V15 `RECid_RECno`, V16 `uid/Guid` baglantilari guncellenir.
9. Transaction commit edilir.
10. Mapping local store'a yazilir ve merkezi API'ye ack gonderilir.

## Satis Faturasi

Ana tablolar:

- `CARI_HESAP_HAREKETLERI`
- `STOK_HAREKETLERI`
- `EVRAK_ACIKLAMALARI` opsiyonel
- `STOK_SERINO_TANIMLARI` veya lot/seri tablolar opsiyonel

Kurallar:

- Cari hareketi ve stok satirlari ayni transaction icinde yazilir.
- Stok satirlarinin fatura header ile V15'te `sth_fat_recid_*`, V16'da `sth_fat_uid` baglantisi kurulmalidir.
- Satis faturasi cari hareketinde Mikro'nun satis fatura evrak tipi korunur.
- Siparisten karsilama varsa `SIPARISLER.sip_teslim_miktar` ve gerekiyorsa `BEDEN_HAREKETLERI` update edilir.

## Satis Irsaliyesi

Ana tablo:

- `STOK_HAREKETLERI`

Kurallar:

- Satis irsaliyesi Mikro evrak tipi ve `sth_tip` degerleri mevcut Fora mantigina uygun set edilir.
- Depo, cari, sevk adresi, temsilci ve sorumluluk merkezi alanlari zorunlu lookup kontrollerinden gecmelidir.
- Irsaliyeden siparis karsilanacaksa siparis teslim miktari transaction icinde guncellenir.

## Alinan Siparis

Ana tablo:

- `SIPARISLER`

Kurallar:

- Her satir bir siparis satiri olarak yazilir.
- Seri/sira tum satirlarda ayni, satir no artan olmalidir.
- Teslim miktari baslangicta sifir olmalidir.
- Sonraki irsaliye/fatura karsilamalarinda teslim miktari atomik update edilir.

## Tahsilat

Ana tablolar:

- `CARI_HESAP_HAREKETLERI`
- `ODEME_EMIRLERI` opsiyonel

Kurallar:

- Nakit tahsilatta cari hareket yeterlidir.
- Cek, senet, kredi karti, havale veya banka hareketi tiplerinde gereken `ODEME_EMIRLERI` kaydi olusturulur veya mevcut kayit update edilir.
- Kapama hesabi, kasa/banka kodu, vade, doviz ve tutar alanlari transformer tarafinda dogrulanir.

## Depo Transfer ve Nakliye

Ana tablo:

- `STOK_HAREKETLERI`

Kurallar:

- Kaynak depo ve hedef depo farkli olmalidir.
- Nakliye fislerinde nakliye depo/durum alanlari ayrica set edilir.
- Nakliye onaylama varsa once bekleyen hareket okunur, sonra karsilik sevk hareketi uretilir.

## Yeni Cari

Ana tablolar:

- `CARI_HESAPLAR`
- `CARI_HESAP_ADRESLERI`
- `CARI_HESAP_YETKILILERI` opsiyonel

Kurallar:

- Upsert anahtari `cari_kod` olmalidir.
- Vergi no, unvan, temsilci, bolge, grup ve varsayilan depo alanlari parametreye gore validate edilir.
- Adres yaziminda adres no ve cari kod iliskisi korunur.

## Ziyaret ve Mobil Olaylar

Ozel tablolar:

- `SA_VISITS`
- `SA_DAY_SESSIONS`
- `SA_CUSTOMER_LOCATIONS`
- `SA_CUSTOMER_NOTES`

Kurallar:

- Mikro standart tablolarina mobil olay verisi karistirilmaz.
- Musteri bazli konum update'i gerekiyorsa once ozel tabloda audit tutulur, sonra izin verilen cari adres GPS alanlari guncellenir.

## Idempotency Mapping

`mappings` tablosu su alanlari icermelidir:

- `tenant_id`
- `entity_type`
- `external_id`
- `mikro_version`
- `mikro_db_name`
- `evrak_seri`
- `evrak_sira`
- `recno`
- `guid`
- `created_at`
- `checksum`

Tekrar calisan job mapping bulursa yeni Mikro evragi olusturmaz, mevcut sonucu done olarak ack eder.
