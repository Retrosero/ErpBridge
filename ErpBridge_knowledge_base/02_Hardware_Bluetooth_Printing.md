# 02 — ErpBridge Evrak Serileri ve Yazıcı Parametreleri Entegrasyonu

> **Hedef:** Bu doküman, sahadan mobil Bluetooth yazıcılar ile basılan fiş, fatura ve irsaliyelerin ERP tarafındaki evrak serileri (`cha_evrakno_seri`, `sth_evrakno_seri`, `sip_evrakno_seri`), sıra numaraları ve yazıcı şablon parametrelerinin ErpBridge tarafından nasıl yönetildiğini açıklar.

İlgili diğer modüller:
- Genel Sistem Mimarisi: [[00_System_Overview]]
- Muhasebe Yazıcıları: [[01_Accounting_Adapters]]
- Veri Sözlüğü & Şema: [[03_Data_Dictionary_and_Rules]]

---

## 1. ERP Tarafında Evrak Serileri ve Sıra Yönetimi

Saha satış mobil uygulamasında (`Siparis_Cepte`) termal yazıcıdan çıktı alındığında, basılan belgenin üzerindeki seri ve sıra numarası ERP'deki muhasebe kaydıyla birebir örtüşmelidir.

### Evrak Numaralandırma Kuralları
- **Fatura Serisi (`DocumentSerial`):** Mikro'da faturalar harf/rakam serisi ve artan tamsayı sıra numarası ile takip edilir (Örn: Seri `"A"`, Sıra `1042`).
- **İrsaliye Serisi:** Sevk irsaliyeleri için ayrı bir seri kodu tahsis edilir (Örn: `"IRS"`).
- **Tahsilat Makbuzu Serisi:** Tahsilatlar için kasa veya banka makbuz serisi kullanılır (Örn: `"THS"`).

### Telefon belgelerinde seri ve sıra (ERP yazım goal'ü, 2026-09-17)
- **Seri Portal'dan gelir, telefondan değil:** Portal "ERP aktarım ayarları" (`erp_write_settings`: sipariş, irsaliye,
  fatura, iade, tahsilat serisi; en çok 6 karakter, boş = serisiz) ve kullanıcı başına geçersiz kılma
  (`mobile_user_erp_mappings`, plasiyer başına seri). Ajan işi kiralarken `erpContext.series` ile alır.
- **Sıra ajanda verilir:** yazım transaction'ında aynı numarayı paylaşan tüm Mikro tablolarında `UPDLOCK, HOLDLOCK`
  ile `MAX+1` (`MikroDocumentNumbering`). Telefon belge basarken Mikro numarasını henüz bilmez; fişte kendi referansını
  (`MOB-…` / basım referansı) kullanır. Yazılınca seri-sıra telefona "Mikro'ya yazıldı: T-1234" olarak döner
  (`GET /api/v1/ingest/jobs/status`) ama basılmış fiş değişmez.
- Aşağıdaki `default_invoice_serial` parametresi eski sözleşmedir; ERP'ye yazılan telefon belgesinin serisini **belirlemez**.

---

## 2. Yazıcı ve Evrak Parametrelerinin Senkronizasyonu

ErpBridge Central API üzerinden mobil istemcilere aktarılan parametreler:
- `POST /api/v1/android/parameters`:
  - `receipt_header_title`: Fiş başlığında yer alacak şirket resmi unvanı.
  - `receipt_tax_info`: Şirket vergi dairesi ve VKN bilgisi.
  - `default_invoice_serial`: Sahada kullanılacak aktif fatura serisi.
  - `printer_paper_width`: Plasiyere atanmış standart kağıt genişliği (`58mm` veya `80mm`).
  - `auto_print_receipt`: Tahsilat tamamlandığında otomatik fiş basma bayrağı.

---

## 3. Resmî E-Fatura / E-İrsaliye Süreçleri ve Bilgi Fişi Ayrımı

> **ERP yazım goal'ü kararı (K15, 2026-09-17):** telefon belgesi Mikro'ya e-belge ayrımı yapılmadan yazılır; e-Fatura /
> e-Arşiv / e-İrsaliye ofiste Mikro'dan düzenlenir. Telefonda e-belge mükellefi müşteriyi ayırıp bilgi fişi basmak ayrı
> bir goal'dür — aşağıdaki modlar hedef tasarımdır, bu goal'de uygulanmadı.

1. **Bilgi Fişi Modu (Thermal Receipt):**
   Sahada e-Fatura / e-Arşiv mükellefi olan işletmeler için termal yazıcıdan çıkan belge mali bir fatura değil, müşteri bilgilendirme ve teslim tesellüm fişidir. Üzerinde *"Bu belge mali değer taşımaz, e-Fatura merkezden düzenlenecektir"* ibaresi yer alır.
2. **Matbu Evrak Modu:**
   İnternetsiz ortamda matbu kağıda basılan irsaliyeli faturalarda, mobil uygulama basılan seri ve sıra numarasını `externalId` ve evrak no olarak ErpBridge'e gönderir. ErpBridge bu numarayı Mikro'nun ilgili tablosuna aynen yazar.
