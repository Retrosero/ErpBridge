# Panel ERP'li kart yönetimi — Durum

`docs/GOAL_PANEL_ERPLI.md` planının nerede olduğu. Son güncelleme: 2026-09-22.

## Tamamlananlar

| Faz | İş | PR |
|---|---|---|
| E0a | Plan onaylandı (kapsam ürün+cari, K6=ADMIN+MANAGER); `eb-panel-erpli` worktree açıldı; `CLAUDE.md`'ye istisna eklendi | (bu PR) |

## E1a — kaynak kodu denetimi (2026-09-22, canlı DB gerekmeden doğrulanabilen kısım)

**Bulgu doğrulandı: her iki writer da INSERT-only, UPDATE yolu yok.**

- `MikroCustomerCardWriter.InsertOrDetectAsync` (`src/ErpBridge.Erp.Mikro/Writers/MikroCustomerCardWriter.cs:267-393`)
  ve `MikroStockCardWriter.InsertOrDetectAsync` (`.../MikroStockCardWriter.cs:293-458`): aynı transaction içinde
  önce `cari_kod`/`sto_kod` ile bir "duplicate-key probe" (`SELECT`) çalışıyor; satır **varsa** hiçbir alanı
  güncellemeden mevcut kimliği `Created=false` ile döndürüp commit ediyor. Satır **yoksa** INSERT ediyor. Yani
  var olan bir kartı ikinci kez göndermek **sessizce no-op** — bu goal'ün istediği "düzenleme" bu koddan
  **hiç geçmiyor**. E1b bu UPDATE yolunu sıfırdan eklemeli.
- **Ek bulgu (planı büyütür):** writer'ların INSERT listesi çok dar — yalnız 11 kolon
  (`CARI_HESAPLAR`: kod/unvan/vergi no-dairesi/email/cep tel/döviz/ödeme günü/grup; `STOKLAR`: kod/isim/kısa
  isim/birim/KDV/grup/cins). `docs/mikro-yazim-referansi.md` §12 (Z0d denetimi, canlı `MikroDB_V15_02`'nin 524
  carisinden çıkarılmış) Mikro'nun **kendi ekranından açılan her caride sabit olan** ama bu writer'da hiç
  yazılmayan alanları listeliyor: `cari_fileid=31`, `cari_hareket_tipi=0`, `cari_doviz_cinsi1/2=255`,
  `cari_vade_fark_yuz=25`, `cari_KurHesapSekli=1`, `cari_fatura_adres_no/cari_sevk_adres_no=1`,
  `cari_EftHesapNum=1`, `cari_odemeplan_no=0`, dört teminat muhasebe kodu (`910/912/226/326`). Stok kartı için
  eşdeğer bir "her stokta sabit" tablo `mikro-yazim-referansi.md`'de **yok** — Z3c'nin asıl işi bu boşluğu
  kapatmak. Bu, E1a'nın orijinal sorusunu (`INSERT mi UPDATE mi`) doğrulamanın ötesine geçen, **KB kural 1**
  kapsamına giren ayrı bir risk: writer bugün haliyle kullanıma açılsa bile ürettiği kart Mikro'nun kendi
  ekranından açılan bir kartla **aynı görünmez** (raporlarda/filtrelerde sapma riski).

**Ortam engeli — E1a'nın canlı kısmı ve E1b bekliyor.** Bu makinede SQL Server çalışıyor ama yalnız
`MikroDB_V15_02` ve `MikroDB_V16_03` bağlı — ikisi de **canlı firma verisi**, `CLAUDE.md` istisnası bunlara
test yazımını **yasaklıyor**. Sanksiyonlu test DB'si `MikroDB_V15_DEMO` (önceki goal'lerde `F:\Mikro\v15xx\DEMO\DATA\`'dan
açılmış) bu makinede yok; `F:` sürücüsü de yok. `D:\sql backup` ve `D:\Mikro SQL Yedek` içinde yalnız
`MikroDB_V15_02`/`MikroDB_V16_*` yedekleri var, `V15_DEMO`/`ERPBTEST` yedeği yok. **Kullanıcıya soruldu:**
demo DB nasıl sağlanacak (Mikro V15 kurulum medyasından mı, başka bir yedekten mi, başka bir makineden mi).
E1a'nın canlı-şema doğrulaması ve E1b'nin ilişkisel testi bu DB olmadan **yapılamaz** — CLAUDE.md kuralı
gereği `MikroDB_V15_02`'ye test yazımı yapılmayacak.

## Sırada

**Durduruldu — insan kapısı:** E1a'nın canlı kısmı ve E1b, `MikroDB_V15_DEMO` test veritabanı bu makineye
kurulana kadar bekliyor. O zamana kadar E1d (referans belge güncellemesi, kod bulgusuyla) ve E2a (yetki
bayrağı — DB gerektirmez) ile devam edilebilir.

## Seni Bekleyenler

- **[K] Test DB:** `MikroDB_V15_DEMO` bu makineye nasıl kurulur (kurulum medyası / yedek dosya konumu)?
- E1b tamamlandığında: writer'ın ürettiği kartın Mikro'nun kendi ekranından açılanla aynı "sabit alan" setini
  taşıyıp taşımadığı (yukarıdaki ek bulgu) ayrıca doğrulanmalı.
- E6b: gerçek Mikro'lu bir firmada canlı gözle kontrol.
- K5 (silme) ve fiş/evrak goal'ü (§6) ayrı kullanıcı onayı gerektirir.
