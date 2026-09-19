# ERP yazım 3 — Durum

`docs/GOAL_ERP_YAZIM_3.md` planının nerede olduğu. Son güncelleme: 2026-09-19.

## Tamamlananlar

| Faz | İş | PR |
|---|---|---|
| Y0a–Y0e | Kanıt toplama; referans §13, §14, §15 | #143 |
| Y3b | Gider yazıcısı (kasa masraf fişi, `cha_evrak_tip=37`) | #143 |
| Y3c | Sayım yazıcısı (`SAYIM_SONUCLARI`) | #144 |
| Y3a | Alış faturası yazıcısı (CHA 0 + STH 3 giriş) | #145 |
| Y3d | Çevirmen: `expense`, `stock_count`, `purchase_receipt` | #146 |
| Y3e | Ajan gönderimi (tek liste, iki host) | #146 |
| Y1c | Ingest kapısı: telefonun kendi alış gövdesi ERP'li kiracıda kabul | #146 |
| Y4c | Telefon: sayım gövdesi (`amount`, `warehouseNo`, `stockCode`) | siparis_cepte#84 |
| Y4d | Telefon: alışta KDV, ödeme KDV'li, alış her firmada gönderiliyor | siparis_cepte#84 |

Üç belge de artık **telefon → merkez → ajan → Mikro** zincirinde uçtan uca
bağlı. Eksik olan tek şey telefonun bu gövdeleri **göndermesi** (Y4).

## Araştırmada çıkan ve planı düzelten bulgular

1. **Gider evrak tipi 37'dir, 0 değil.** Fora gideri hizmet faturası olarak
   yazıyor ve `KasaMasrafFisi` tipini hiç kullanmıyor; firmanın kendi Mikro
   ekranı 37 yazmış (canlıda 151 satır). Referans §6 gereği Mikro'nun kendi
   kaydı esas alındı (K1). Fora'nın "eski ve çalışan sürüm" olması onu kolon
   kaynağı yapar, tasarım kaynağı değil.
2. **Gider tediyenin aynası değildir.** Tediyede ödeyen hesap
   `cha_kasa_hizmet`'te ve karşı taraf caridir; giderde gider kartı
   `cha_kasa_hizmet`'e, ödeyen hesap `cha_cari_cins`/`cha_kod`'a geçer.
3. **Kredi kartı Mikro'da banka hesabıdır.** `cha_cinsi=22` satırlarının
   `cha_kasa_hizkod`'u bir `BANKALAR.ban_kod`; `FIRMA_KREDI_KARTI_TANIMLARI`
   canlıda boş.
4. **Alışta seri tedarikçinindir.** Mikro'da seri tanım tablosu yok; alış
   faturası tedarikçinin kendi fatura serisini taşıyor (ÇINAR, JUMBO, CS…).
   "ERP'de tanımlı seri" pratikte veride kullanılmış seri demek (K7).
5. **Y3e'nin riski yapısal olarak kalkmış.** Tediyede "No writer is configured"
   hatasına yol açan iki ayrı tür listesi, PR #142 ile tek `AgentJobPump`'a
   indi. Yeni belge türü artık bir kez eklenir.

## Seni bekleyenler (karar/aksiyon gerekiyor)

### 1. Gider kartı kataloğu (Y2b)

K2'ye göre telefon gider kartlarını ERP'den çekecek. Bu yeni bir katalog
senkronu demek (`MASRAF_HESAPLARI` → merkez → telefon). Yapılana kadar telefon
`expenseCardCode` gönderemeyeceği için gider `MOBILE_APP_UPDATE_REQUIRED` ile
reddedilir — sessiz kayıp yok, ama gider de akmaz.

### 2. Ayarların panele bağlanması (Y2a)

`ErpWriteContext`'e alış deposu ve "tedarikçi fiyatı KDV içeriyor mu" eklendi
ama henüz `erp_write_settings`'ten gelmiyor; şimdilik satış deposuna ve
"KDV hariç"e düşüyor.

## Karara bağlananlar

**K12 (2026-09-19): alışta KDV telefonda gösterilir (seçenek a).** Alış ekranı
artık ara toplam + KDV / genel toplam gösteriyor; tedarikçi bakiyesi, kasa çıkışı
ve tediye genel toplamı taşıyor. Gövdede `amount` KDV hariç kalır (fatura
satırlarının toplamı odur), `vatAmount`/`grossAmount` yanında gider.

Bu turda çıkan ikinci bir sessiz kayıp da kapatıldı: kasa kayıtlarının cari kodu
**ada göre** aranıyordu ve aynı adda iki cari varsa arama boş dönüyordu — alış
faturası Mikro'ya yazılıp ödemesi düşerdi. Kodu bilen taraf artık söylüyor.

## Kalanlar

| Faz | İş |
|---|---|
| Y1a | Gövde sözleşmesi v3 belgesi |
| Y1b | Giderin `disbursement` yerine `expense` ile gelmesi (telefon tarafı) |
| Y1d | Kapsam dışı tür mesajlarının Türkçeleşmesi |
| Y2a | `erp_write_settings` + Portal: alış deposu, KDV dahil mi, gider kasası |
| Y2b | Gider kartı kataloğu ve senkronu |
| Y2c | Portal'da gider kartı listesi |
| Y4a, Y4b, Y4e | Telefon: gider ekranı (ERP kartları, KDV, kredi kartı), araç bakım gideri |
| Y5a–Y5d | Canlı uçtan uca doğrulama (`MikroDB_V15_DEMO`) |
| Y6a–Y6c | Portal Türkçe adlar, bilgi bankası, Play internal |
