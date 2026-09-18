# Goal Durumu — Kalan telefon belgeleri Mikro'ya

Son güncelleme: 2026-09-19 (Z0b–Z0c)
Görev listesi: [GOAL_ERP_YAZIM_2.md](GOAL_ERP_YAZIM_2.md) · Mikro kuralları: [mikro-yazim-referansi.md](mikro-yazim-referansi.md)

> **Her görevden sonra, o görevin PR'ı içinde güncellenir.** Oturum kapanırsa buradan devam edilir.
> Durum: ⬜ bekliyor · 🔄 sürüyor · ✅ bitti · ⏭️ atlandı (kapı) · ⛔ tıkandı

---

## Özet

| Faz | Görev | Biten | Durum |
|---|---|---|---|
| Z0 — Referans ve envanter | 5 | 3 | 🔄 |
| Z1 — Sunucu: sözleşme, kabul, ayarlar | 3 | 0 | ⬜ |
| Z2 — Çevirici ve komutlar | 2 | 0 | ⬜ |
| Z3 — Mikro V15 writer'ları | 5 | 0 | ⬜ |
| Z4 — Sipariş Cepte | 4 | 0 | ⬜ |
| Z5 — İzleme ve geriye dönük | 2 | 0 | ⬜ |
| Z6 — Kapanış | 3 | 0 | ⬜ |

**Şu anki görev:** Z0d — cari kartı referansı

**Kapsam:** Tediye · Alış faturası (satırlı) · Yeni cari kartı. Gider ve sayım **kapsam dışı** (kullanıcı kararı U1);
ikisi de Z1b ile ERP'li firmada görünür biçimde reddedilecek.

## Ortam
- Test veritabanı: **`MikroDB_V15_DEMO`** (önceki goal'den; `F:\Mikro\v15xx\DEMO\DATA\`).
  Canlı testler: `ERPBridge_RUN_INTEGRATION=1 ERPBridge_MIKRO_WRITE_DB=MikroDB_V15_DEMO`, bağlantı `tcp:localhost`.
- `MikroDB_V15_02` canlı firma verisi — **yalnız okuma** (Z0b/Z0c/Z0d buradan kanıt toplar).

---

## Görevler

| ID | Görev | Durum | PR | Not |
|---|---|---|---|---|
| Z0a | Plan dalını main'e al | ✅ | [#138](https://github.com/Retrosero/ErpBridge/pull/138) | İnceleme (Codex P1) kapı boşluğunu yakaladı → Z3e eklendi |
| Z0b | Referans §10 — alış faturası | ✅ | #139 | **Fora ve canlı veri tam uyuştu.** CHA: `evrak_tip=0`, `tip=1 alacak`, `cinsi=6`, `cari_cins=0`/tedarikçi, `ciro_cari_kodu` açık faturada da dolu, uuid 36 karakter. Peşin alış = **kapalı fatura** (canlı örnek: `kod=001, cari_cins=4, tpoz=1, ciro=tedarikçi`) — D1 artık tahmin değil, kanıt. STH: `evraktip=3`, `tip=0 giriş`, `cins=0`, `fat_recid_recno`=başlık. **Kritik:** alış faturası ile satış iadesi aynı `evrak_tip=0`'ı paylaşıyor (yalnız `normal_Iade` ayırıyor) → MAX+1 ikisini birlikte saymalı; iade filtresi koyan writer Mikro'nun tekil indeksine takılır |
| Z0c | Referans §11 — tediye | ✅ | #139 | Tahsilatın birebir aynası: tek evrak, **yöntem başına bir CHA satırı**, ayrı kasa/banka satırı **yok** — hesap aynı satırda `cha_kasa_hizmet` (4 kasa / 2 banka) + `cha_kasa_hizkod`. `cha_evrak_tip=64`, `cha_tip=0 borç`, `cari_cins=0`, `tpoz=0`. `cha_cinsi` **firma ailesi**: 0 nakit (kasa), **20 FirmaHavaleEmri** (banka), 3 firma çeki, 4 firma senedi, 22 firma kredi kartı, 1/2 ciro edilen müşteri çeki/senedi. Havalede tahsilatın 17'si (gelen havale) **kullanılmaz**. **İnceleme (Codex P1) iki hatamı yakaladı:** ilk yazdığım "ayrı kasa/banka satırı" yanlıştı ve `cinsi` 3/4/20/22 zaten `enum_cha_cinsi`'de çözülüydü — açık uç bırakmama gerek yoktu |
| Z0d | Referans §12 — cari kartı | ⬜ | | |
| Z0e | Başarısız iş envanteri | ⬜ | | Z5a bu listeyi kullanır |
| Z1a | Gövde sözleşmesi v3 | ⬜ | | |
| Z1b | Gider/sayım ERP'li firmada reddedilir | ⬜ | | |
| Z1c | `erp_write_settings` + Portal UI | ⬜ | | |
| Z2a | Komutlar + adaptör metotları | ⬜ | | |
| Z2b | `MobileDocumentTranslator` genişlemesi | ⬜ | | |
| Z3a | Alış faturası writer | ⬜ | | |
| Z3b | Tediye writer | ⬜ | | |
| Z3c | Cari kartı writer + eski writer denetimi | ⬜ | | |
| Z3d | Ajan yeni türleri tanısın | ⬜ | | **Çakışma:** `AgentJobPump` dalı (`faz-51-mobil-siparis-mikro`, 2026-09-19'da push edildi) bu kodu yeniden yazıyor; başlamadan önce main'e girip girmediğine bakılır |
| Z3e | Kapıyı aç: ERP'li firmada `purchase_receipt` + `customer_card` kabul | ⬜ | | Writer'lar hazır olduktan **sonra**; `IngestEndpoints` ve `ApprovalService.RejectDocument` birlikte |
| Z4a | Telefon: alış gövdesi | ⬜ | | |
| Z4b | Telefon: yeni cari | ⬜ | | |
| Z4c | Telefon: tediye tek belge | ⬜ | | |
| Z4d | Sürüm + Play internal | ⬜ | | AAB'yi kullanıcı derler |
| Z5a | Başarısız işlerin yeniden denenmesi | ⬜ | | |
| Z5b | Portal listesi + Log Merkezi olayları | ⬜ | | |
| Z6a | DEMO'da uçtan uca duman testi | ⬜ | | |
| Z6b | KB güncellemesi | ⬜ | | |
| Z6c | DURUM kapanışı | ⬜ | | |

---

## Bulgular

### Kalan türler bugün Mikro'ya ulaşmıyor — ama iki farklı sebeple (2026-09-19)
- **Kuyrukta kalıcı hata birikiyor:** `disbursement` (tediye), `expense`, `purchase` (kasadan alış ödemesi),
  `stock_count`, `cash_transaction`. Ingest'te belge türü beyaz listesi yok, iş kaydediliyor; ajan yalnız altı
  türü tanıyor (`AgentWorker.GenericDocumentTypes`) ve tanımadığını `UNSUPPORTED_DOCUMENT_TYPE` +
  `retryable=false` ile **kalıcı** reddediyor.
- **Ajana hiç ulaşmıyor:** `purchase_receipt` (satırlı alış) ve `customer_card` (yeni cari) ingest'te 409 ile
  reddediliyor (`DOCUMENT_REQUIRES_NATIVE_TENANT` / `CARDS_REQUIRE_NATIVE_TENANT`); `ApprovalService.RejectDocument`
  onaydan geçen belge için aynı kuralı ayrıca uyguluyor.

İkinci maddeyi PR #138 incelemesi (Codex, P1) yakaladı ve plan düzeltildi: kapsamdaki iki türün writer'ı yazılsa
bile kapı açılmadan hiçbir belge ulaşmaz → **Z3e** eklendi, writer'lardan sonra çalışacak. Aksi hâlde bir sessiz
hatayı (409) başka bir sessiz hatayla (`UNSUPPORTED_DOCUMENT_TYPE`) değiştirmiş olurduk.

### Fora güvenilir bir kaynak çıktı (2026-09-19)
Kullanıcının işaret ettiği `Fora_Mikro/` (bu işi daha önce başarıyla yapan uygulama) decompile edilmiş halde
okunabiliyor. Z0b/Z0c'de **Fora'nın yazdığı her kod canlı veriyle birebir uyuştu** — alış faturası, tediye, kapalı
fatura, kalem kodları. Bundan sonraki görevlerde kolon listesi ve değerler için önce Fora okunacak, sonra canlı
veriyle doğrulanacak.

### Fora'dan ilk kanıt (2026-09-19)
`Fora_Mikro/.decompiled/Core/Fora.Mikro.CariHesapHareket/enum_cha_evrak_tip.cs`: `0 = AlisFaturasi`,
`1 = TahsilatMakbuzu`, `63 = SatisFaturasi`, **`64 = TediyeMakbuzu`**, `65 = KasaTediyeFisi`. İlk goal'de canlı
veriyle doğrulanan satış/tahsilat kodlarıyla birebir uyuşuyor; `EVRAK_ACIKLAMALARI`'ndaki tediye `(0, 64)` ipucunu
da açıklıyor. Z0b/Z0c bunu canlı kayıtla da doğrular.

---

## Seni Bekleyenler

| # | Konu | Neden sen |
|---|---|---|
| 1 | *(çözüldü 2026-09-19: kullanıcı "veritabanında örnekler var" dedi; ayrıca `Fora_Mikro/` kaynak olarak verildi)* | — |
| 3 | Yeni cari için **kod öneki / grup kodu / ödeme planı** hangi değerler olmalı | Muhasebe kararı; Z1c bunları Portal ayarına koyar |
| 4 | Z6a duman testinden sonra Mikro ekranından kontrol | Muhasebeci gözü |
