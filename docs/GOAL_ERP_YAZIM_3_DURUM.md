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
| Y2b | Gider kartları ERP'den telefona (`lookups` / `expense_card`) | #148 |
| Y4a | Telefon: gider ekranı ERP kartlarından seçtiriyor, KDV alanı | siparis_cepte#85 |
| Y4b | Gider `expense` belgesi olarak gidiyor | siparis_cepte#85 |
| Y2a | Alış deposu ve "tedarikçi fiyatı KDV dahil mi" panelde | #150 |
| Y6a | Yazılmış sayım "ERP'de kesinleştirilmeyi bekliyor" diyor | #151 |
| Y4e | Araç bakım gideri de gider kartı taşıyor | siparis_cepte#86 |

**Üç belge de uçtan uca tamam**: telefon → merkez → ajan → Mikro. Alış faturası,
gider ve sayım artık hem yazılabiliyor hem de telefon tarafından gönderiliyor.

Kalan tek şey doğrulama ve teslim: canlı uçtan uca test (Y5) ve Sipariş Cepte'nin
yeni sürümünün cihazlara ulaşması (Y6c). Kod `main`'de ama **bu PC'de release
keystore ve Play yükleme yolu yok**, yani telefon değişiklikleri ancak yeni bir
sürüm yüklendiğinde çalışmaya başlar.

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

## 2026-09-20: alış iki evrak gönderiyordu (K13)

Kullanıcı bildirdi: bir alış için sisteme bir alış faturası **ve** bir tediye
gidiyor, tediye yazılıyor ama alış yazılmıyor. İki ayrı sorun çıktı.

**1. Tasarım — K5 yanlıştı, K13 ile değişti.** Canlı `MikroDB_V15_02`'de 718 alış
faturasının **hepsi tek CHA satırı**: 650 açık hesap, 68 kasadan kapatılmış
(`tpoz=1`, `cari_cins=4`, `cha_kod`=kasa, `cha_ciro_cari_kodu`=tedarikçi).
Faturayı açık yazıp ödemeyi ayrı tediye evrağı yapmak aynı ödemeyi muhasebede iki
kez gösteriyordu. Artık peşin alış tek kapalı evrak (#153, siparis_cepte#87);
desen satış faturasının peşin hali (§3) ve iadeyle (§4) aynı — iadelerin zaten
doğru aktarılmasının sebebi buydu.

**2. Yazılmamasının sebebi kod değil, ajanın sürümüydü.** Çalışan tepsi ajanı
2026-09-19 15:36 derlemesiydi; alış faturası yazıcısı (#145) ve çevirmen
bağlantısı (#146) ondan ~4,5 saat sonra `main`'e girdi. Ajan logu:
`Received job ... with unsupported document type purchase_receipt`.

**Geçmiş belge ne olacak:** kuyruktaki alış eski gövdeyle geldiği için ödeme
bilgisi taşımıyor ve **açık** yazılacak. Ödemesi zaten ayrı tediye olarak
yazılmıştı, yani mükerrer ödeme oluşmaz — yalnız tek evrak yerine iki evrak
görünür.

## Seni bekleyenler (karar/aksiyon gerekiyor)

### 1. Ajanı yeni derlemeyle yeniden başlat

Ajan `main`'den derlendi. Eski ajan kapatılıp yenisi açılmalı; kalıcı hataya
düşmüş alış belgesi Portal'daki ERP Belgeler sayfasından yeniden denenebilir.

### 2. Sipariş Cepte sürümü cihazlara ulaşmalı (Y6c)

Telefon tarafındaki üç değişiklik de `main`'de: alışta KDV, sayım gövdesi ve
gider ekranı. Ama bu PC'de release keystore ve Play yükleme yolu yok, yani
cihazlarda hiçbiri çalışmıyor. Yeni sürüm yüklenene kadar gider yine
`MOBILE_APP_UPDATE_REQUIRED` ile reddedilir (sessiz kayıp yok).

### 3. Alış ayarları gözden geçirilmeli

Panelde artık **alış deposu** ve **tedarikçi fiyatı KDV dahil mi** var (#150).
İkincisi rakamlardan çıkarılamaz: yanlış seçim faturayı KDV kadar şişirir ya da
eksiltir. Ayarlanmamışsa "KDV hariç" kabul edilir.

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
| Y1b | ~~Giderin `disbursement` yerine `expense` ile gelmesi~~ — gider kartının varlığından ayırt edilerek çözüldü (siparis_cepte#85); kasa defterinin çıkış toplamları bozulmasın diye kayıt "Tediye" kalıyor |
| Y1d | Kapsam dışı tür mesajlarının Türkçeleşmesi |
| Y2a | `erp_write_settings` + Portal: alış deposu, KDV dahil mi, gider kasası |
| Y2c | Portal'da gider kartı listesi (uç hazır, ekran yok) |
| Y5a–Y5d | Canlı uçtan uca doğrulama (`MikroDB_V15_DEMO`) |
| Y6a–Y6c | Portal Türkçe adlar, bilgi bankası, Play internal |
