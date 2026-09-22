# Goal — ERP'li (Mikro) Firmalarda Panelden Ürün ve Cari Kartı Yönetimi

Tarih: 2026-09-22 · Kapsam: ErpBridge.CentralApi + ErpBridge.Erp.Mikro + ErpBridge.Portal
(Sipariş Cepte'ye yalnız E5'te dokunulur — telefonun zaten okuduğu `urun`/`cari` akışının panel yazımını doğru yansıttığını doğrulamak için)

**Onaylandı (2026-09-22)** — kapsam (yalnız ürün+cari, fiş sonraki goal'e) ve K6 (yetki:
ADMIN+MANAGER) kullanıcı tarafından onaylandı; E0'dan itibaren çalışmaya başlanabilir. İlerleme
**GOAL_PANEL_ERPLI_DURUM.md**'ye yazılır (E0'da açılır). Çalışma kuralları, doğrulama ve
yetkiler [GOAL_PANEL_GELISTIRMELERI.md](GOAL_PANEL_GELISTIRMELERI.md) bölüm 1–3 ile aynıdır
(worktree, PR akışı, Codex bulguları, `TreatWarningsAsErrors`, `origin/main` alıp merge
öncesi yerel build). Yeni worktree: **`2026/eb-panel-erpli`**.

**Kardeş plan:** `docs/GOAL_PANEL_ERPSIZ.md` aynı sorunu ERP'siz (`DataSource=native`) firmalar
için çözer. İki plan **bağımsızdır** — biri `NativeDocumentProcessor`'a, bu biri ajan +
Mikro writer'lara yazar; UI kalıpları (DetailSheet, Pager, PageHeader) ortak kullanılabilir
ama sunucu tarafı hiç örtüşmez.

---

## 0. Amaç ve mevcut durum

**Amaç:** Mikro (ERP) kullanan firmanın panele giren yöneticisi, ürün ve müşteri kartlarını
telefona ihtiyaç duymadan **panelden** açıp düzenleyebilsin. Fiş (irsaliye/fatura/sipariş)
yönetimi bu goal'ün **dışındadır** — bkz. §6.

### Doğrulanmış zemin (kod + KB, 2026-09-22 araştırması)

- **Panel şu an üçü de salt okunur.** `Pages/Stok.razor`, `Pages/Cari.razor`, `Pages/Cariler.razor`
  yalnız `PortalApiClient` üzerinden `GET /api/v1/portal/stock/search|facets`,
  `/customers`, `/customers/card|ledger|document` çağırır; hiçbirinde create/edit/save yok.
  `Pages/ErpBelgeler.razor`'daki tek yazma **"Yeniden dene"** — `Job.PayloadJson`'u
  değiştirmeden aynı işi tekrar kuyruğa alır, düzenleme değildir.
- **Mikro writer'ları zaten var ama hiçbir HTTP yolu onlara ulaşmıyor.**
  `MikroAdapter.WriteCustomerCardAsync`/`WriteStockCardAsync`
  (`src/ErpBridge.Erp.Mikro/Adapters/MikroAdapter.cs:663,672`) `AgentJobPump.DispatchDocumentAsync`
  switch'inde çağrılabilir durumda (`src/ErpBridge.Core/Jobs/AgentJobPump.cs:342-343`) — ama bunu
  tetikleyecek tek yer olan `POST /api/v1/ingest/jobs` (`IngestEndpoints.cs`), ERP (`DataSource != native`)
  tenant'lar için `customer_card`/`stock_card` belgesini **`Job` satırı hiç oluşturmadan**
  409 `CARDS_REQUIRE_NATIVE_TENANT` ile reddediyor (yaklaşık satır 426-433). Aynı kapı
  `ApprovalService.SubmitAsync`'te de var (`Approvals/ApprovalService.cs:188-189`).
  **Bu iki writer, derlenmiş ve birim testli ama ERP tenant için üretimde tetiklenemez durumda.**
- **Bilinen, izlenen boşluk — icat değil.** `docs/GOAL_ERP_YAZIM_2.md` (2026-09-19) bu tam
  çelişkiyi zaten tespit etmiş: kapı açılmadan writer'a hiçbir belge ulaşmaz (Z3e görevi),
  ayrıca writer'lar `mikro-yazim-referansi.md`'den **önce** yazıldığı için denetlenmemiş
  (Z3c görevi). `docs/GOAL_ERP_YAZIM_2_DURUM.md`: Z3c ⬜, Z3e ⬜ — ikisi de **yapılmamış**.
  Z3e yalnız **telefon** ingest yolu için `customer_card`'ı hedefliyordu, panel hiç kapsamda
  değildi; `stock_card` ise o planda **kapsam dışı** bırakılmıştı (`GOAL_ERP_YAZIM_2_DURUM.md:82`,
  `GOAL_ERP_YAZIM_3.md:79`). Bu goal her ikisini de (ürün + cari) hem denetler hem panelden
  ulaşılabilir yapar.
- **Bilinmeyen — E1'de netleşecek:** writer'lar **INSERT-only** mi (yalnız yeni kayıt) yoksa
  var olan `cari_kod`/`sto_kod` ile tekrar çağrılınca **UPDATE** mi yapıyor? "Yeni cari" (Faz Z3e
  hedefi) yalnız oluşturmayı ima ediyor; "kart **düzenleme**" (bu goal'ün istediği) var olan satırı
  bulup güncelleyen bir yol gerektirir — bu yol **hiç yazılmamış/denenmemiş olabilir**. KB kuralı
  gereği (şemayı tahmin etme — 179+142 bug'lık geçmiş) bu E1'in ilk ve en riskli görevi.
- **Yetki bayrağı yok.** `RolePermissions` (`src/ErpBridge.CentralApi/Domain/MobileUser.cs:125-174`)
  ve `PortalRoles`/`PortalArea` (`src/ErpBridge.Portal/Session/PortalRoles.cs`) içinde ürün/cari
  **yazma** için ayrılmış bir alan/bayrak yok; yalnız salt okunur `Ledger` alanı var
  (Admin, Manager, Accounting'e açık). Yeni bir yetki bayrağı gerekir.
- **Denetim kaydı altyapısı yok** (bu repoda); `GOAL_PANEL_ERPSIZ.md` kendi `native_audit_log`'unu
  planlıyor ama henüz kod yok. İki goal ayrı ayrı ilerlerse iki ayrı audit tablosu doğar — E4'te
  ortak bir isim/şema seçilmesi önerilir (bkz. §3).

### Kararlar

| # | Karar | Gerekçe |
|---|---|---|
| K1 | **Fiş (evrak) bu goal'ün dışında.** Kullanıcı hem "panelden yeni evrak" hem "var olan evrağı düzelt/iptal et" istiyor; ikisi de ayrı, daha büyük ve daha riskli bir goal'dür (bkz. §6) | Kullanıcı kararı (2026-09-22): önce dar kapsamlı ürün+cari, fiş sonraki goal |
| K2 | **Yazma yolu: mevcut `/ingest/jobs` + `AgentJobPump` + Mikro writer.** Yeni bir yazma motoru icat edilmez; panel de aynı `Job` kuyruğuna, aynı idempotency (`externalId`) ile yazar. Native goal'ün D1'i ile aynı prensip, farklı hedef | Kural: iş kayıtsız etki, etki kayıtsız iş olamaz; telefon ve panel aynı sonucu üretir |
| K3 | **Asenkron kabul edilir, gizlenmez.** Panelde kaydet = `Job` oluştu (Pending); Mikro'ya yazım ajan bir sonraki `JobPollIntervalSeconds` (varsayılan 30 sn) turunda olur. UI "Kaydedildi" değil **"Mikro'ya yazılıyor…"** gösterir, `ErpBelgeler.razor`'daki gibi durum takip edilir | Ajan yerelde, bağlantı kesik olabilir; sahte anlık başarı yanıltıcı olur (native'de ki gibi senkron değil) |
| K4 | **Kod = kimlik, değişmez.** `sto_kod`/`cari_kod` düzenlemede değiştirilemez (native goal E1a ile aynı davranış); değiştirmek istenirse yeni kart + eskiyi pasifleştirme ayrı bir akış (kapsam dışı) | Kimlik alanı taşarsa/karışırsa exception (KB kural 8) — kod üzerinden geri izlenebilirlik bozulmaz |
| K5 | **Silme bu goal'de yok.** `stock_card_delete`/`customer_card_delete` (native goal'de var) burada **planlanmaz** — Mikro'da kart silmek muhasebesel olarak farklı bir risk sınıfı (hareketli kart, cari mutabakatı); ayrı karar gerekir | Kullanıcı isteği yalnız "düzenleme"; silme istenirse ayrı goal |
| K6 | **Yetki: yeni `RolePermissions.CanManageErpCards`, ADMIN + MANAGER.** Native goal'ün "yalnız ADMIN" kararından bilinçli sapma çünkü ERP tenant'ta MANAGER zaten onay/depo gibi operasyonel yetkilere sahip (Faz 45) | **Kullanıcı kararı (2026-09-22): onaylandı** |
| K7 | **Denetim kaydı zorunlu**, tablo adı native goal'ünkiyle çakışmasın diye `portal_card_audit_log` (jenerik ad, ileride iki goal birleşebilir) | İzlenebilirlik — Mikro'ya giden her değişikliğin panelde kim/ne zaman/ne yazdığı görünmeli |
| K8 | **Test yazımı yalnız `MikroDB_V15_DEMO`/`MikroDB_V15_ERPBTEST`.** `CLAUDE.md`'deki mevcut istisna deseniyle aynı; `MikroDB_V15_02` (canlı firma) ve gerçek müşteri DB'lerine test yazımı yasak | Repo standart kuralı, sapma yok |

---

## 1. Görevler

**[O]** otonom · **[K]** insan kapısı · **[O→K]** yapılabilen kısmı otonom.

### E0 — Hazırlık
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E0a** | Bu plan onaylanınca dalı push et, PR, CI yeşilse merge; `eb-panel-erpli` worktree; `GOAL_PANEL_ERPLI_DURUM.md` aç | [K→O] | Plan `main`'de |
| **E0b** | Zemin: KB 00/01/03 + `mikro-yazim-referansi.md` oku; yerelde `erpbridge-centralapi-local`/`erpbridge-portal-local` (bkz. `PLAN_ROLLER_VE_DEPO` notu, kök `.claude/launch.json`) ile ERP'li test firması aç (bellek içi DB'de ERP'siz firma satış kaydı çalışmıyor — ERP'li test firması şart) | [O] | DURUM'da zemin notu |

### E1 — Mikro writer denetimi ve kapı açma (en riskli, önce biter)
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E1a** | `MikroCustomerCardWriter`/`MikroStockCardWriter` kodunu canlı `MikroDB_V15_DEMO` şemasına karşı denetle: hangi kolonlar yazılıyor, var olan `cari_kod`/`sto_kod` ile tekrar çağrılırsa ne oluyor (INSERT unique-violation mı, sessiz duplicate mi, yoksa zaten UPDATE mi var)? Barkod tablosu (`BARKOD_TANIMLARI`) eski barkodu düşürüyor mu (Faz 34 native kuralıyla aynı davranış native'de var, Mikro'da yok olabilir) | [O] | `MikroSchemaContractTests` benzeri canlı şema testi + bulgular DURUM'a yazılır |
| **E1b** | E1a'da UPDATE yolu eksikse ekle: var olan kart `sto_kod`/`cari_kod` ile bulunur, değişen alanlar `UPDATE`, self-link/kimlik dokunulmaz. Barkod diff'i (native `MobileRecordProjector.TombstoneAsync` mantığına benzer) uygulanır | [O] | İlişkisel test: aynı kod ikinci kez POST → tek satır, alanlar güncel, eski barkod düşmüş |
| **E1c** | `IngestEndpoints.cs`'teki `CARDS_REQUIRE_NATIVE_TENANT` kapısını **yalnız** `customer_card`/`stock_card` için ERP tenant'ta da geçirecek şekilde aç (native tenant davranışı aynen kalır). `ApprovalService.CardKinds` guard'ı aynı şekilde gözden geçirilir — kart düzenlemesi onay akışına girecek mi, yoksa (K6'daki gibi zaten yetkili roller) doğrudan mı gidecek: **öneri — onaya girmez**, çünkü gönderen zaten `CanManageErpCards` yetkili panel kullanıcısı (itiraz edilirse E4'e onay adımı eklenir) | [O] | Test: ERP tenant'ta `customer_card`/`stock_card` job'ı artık oluşuyor, ajan job pump onu işliyor |
| **E1d** | `mikro-yazim-referansi.md`'yi E1a/E1b bulgularıyla güncelle (kolon zorunluluk/opsiyonellik, update davranışı) | [O] | Belge güncel |

### E2 — Yetki ve denetim (E1'den sonra, panel formlarından önce)
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E2a** | `RolePermissions.CanManageErpCards` ekle (K6); `PortalRoles`/`PortalArea`'ya yeni alan (ör. `Cards`); her yeni portal ucu bu bayrağı + `DataSource=erp` (native'in tersi) denetler | [O] | Test matrisi: rol × uç, native tenant'ta 409 |
| **E2b** | `portal_card_audit_log` tablosu (yeni EF migration; mevcut tabloya dokunulmaz), her E3/E4 yazması kayıt atar (kim, ne zaman, hangi kart, önce/sonra özet, job id); migration sonrası `https://lisans.appsgo.cloud/health/schema` → `current` | [O] | Test + migration canlıda current |

### E3 — Panelden ürün kartı düzenleme
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E3a** | Sunucu: `POST /api/v1/portal/erp/stock-cards` (yeni+düzenle) — panel oturumundan `stock_card` `Job` oluşturur (K2/K3), idempotent `externalId`, Türkçe alan hatası; `GET …/{code}` tek kart detayı (kart+barkod+fiyat, form doldurmak için) | [O] | İlişkisel test: yeni, düzenle (kod sabit), barkod değişimi, native tenant'ta 409, yetkisiz 403 |
| **E3b** | Panel `/stok`: **"Yeni ürün"** + satırda **Düzenle**; `Shared/DetailSheet` çekmece formu (kod, ad, birim, KDV, grup, barkod). Kaydet → "Mikro'ya yazılıyor" durumu, `ErpBelgeler.razor`'a da düşen job ile durum takip edilebilir link | [O] | bUnit + tarayıcı: yeni ürün → job oluştu → (yerel ajan ile) Mikro'da göründü → `stock/search`'te güncel |

### E4 — Panelden cari kartı düzenleme (E3 ile simetrik)
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E4a** | Sunucu: `POST /api/v1/portal/erp/customer-cards` (yeni+düzenle, `customer_card` job'ı), `GET …/{code}` | [O] | İlişkisel test: yeni, düzenle, native tenant'ta 409 |
| **E4b** | Panel: `/cariler`'de **"Yeni müşteri"**, `/cari` sayfasında **Düzenle**; alanlar KB 03 kart şemasından (unvan, vergi no/dairesi, telefon, e-posta, ödeme günü, grup kodu — adres/telefon ayrı tablo olduğu bilgisiyle) | [O] | bUnit + tarayıcı |

### E5 — Telefon/agent uyumu
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E5a** | Uçtan uca ilişkisel test: panelden kart düzenle → ajan (yerel test agent) işler → Mikro'da güncellenir → değişiklik shadow-log (`_ERPB_SENKRONIZASYON`) üzerinden `sync/pull`'a düşer → telefon `urun`/`cari`'de güncel görür. Ajan offline senaryosu: job `Pending` kalır, panelde "beklemede" görünür, `ErpBelgeler.razor`'daki retry mekanizması buraya da uygulanabilir olmalı | [O] | Test + yerel ölçüm |

### E6 — Kapanış
| ID | Görev | Tip | Bitti ölçütü |
|---|---|---|---|
| **E6a** | KB güncelle: 00 (kural 15'e ERP tenant kart yazma yolu eklenir, native ile karışmasın diye ayrı madde), 01 (writer tablosuna "artık panel+telefon'dan ulaşılabilir" notu), `docs/api-contracts.md` (yeni uçlar), `CLAUDE.md` gerekirse | [O] | Belgeler güncel |
| **E6b** | Canlı kontrol: gerçek Mikro'lu bir firmada panelden kart düzenle, canlı DB'de doğrula (ekran görüntüsü) | [K] | İnsan kapısı |
| **E6c** | Plan tablosu + özet + hafıza notu | [O] | DURUM kapalı |

---

## 2. Bağımlılıklar ve sıra

`E0 → E1 (writer denetimi + kapı) → E2 (yetki+denetim) → E3 (ürün) → E4 (cari) → E5 (telefon uyumu) → E6`

- **E1 en riskli görev.** UPDATE yolu yanlış yazılırsa canlı Mikro'da yanlış kolon güncellenir —
  E1a'nın canlı şema testi olmadan E1b'ye, E1b'nin ilişkisel testi olmadan E1c'ye geçilmez.
- E2 (yetki bayrağı + denetim), E3/E4'ten **önce**: yazma uçları yetkisiz/denetimsiz yayına çıkmaz.
- Migration içeren görev (E2b) sonrası şema sağlık ucu kontrol edilir.

## 3. Bozulmaz kurallar (bu goal'e özgü ek)

- Şemayı tahmin etme (KB kural 1) — her writer değişikliği canlı şema testiyle kanıtlanır.
- SQL parametrik zorunluluğu (Dapper `@Param`); tablo/kolon adları yalnız derleme-zamanı katalogdan.
- Atomik transaction + idempotent yazım (`mappings` tablosu, `externalId`).
- Kimlik alanı (`sto_kod`/`cari_kod`) düzenlemede **değişmez**; taşarsa exception (KB kural 8).
- Test yazımı yalnız `MikroDB_V15_DEMO`/`MikroDB_V15_ERPBTEST` (K8); `MikroDB_V15_02` ve müşteri
  DB'lerine dokunulmaz.
- Panel yazımı **asenkron kabul edilir** (K3) — sahte anlık "başarılı" mesajı yasak.
- Android'e bu goal'de dokunulmaz (mevcut senkron akışı zaten okuyor); yalnız E5'te doğrulanır.

## 4. İnsan kapıları (beklenenler)

E1a'nın bulguları (writer'lar UPDATE'i hiç
desteklemiyorsa E1b'nin kapsamı büyür, plana geri bildirim gerekir) · E6b canlı doğrulama ·
silme (K5) ve fiş (K1/§6) kararları ayrı onay gerektirir.

## 5. Bitti tanımı

ERP'li (Mikro) firmanın yetkili panel kullanıcısı, panelden yeni ürün/müşteri kartı açabilir ve
var olan kartı düzenleyebilir; değişiklik ajan aracılığıyla Mikro'ya yazılır ve telefonlara doğru
şekilde yansır; her işlem denetim kaydında görünür; ERP'siz firmada bu uçlar zaten devre dışı
kalır (409); CI yeşil, KB güncel.

## 6. Kapsam dışı — sıradaki goal(ler)

Kullanıcı fiş (evrak) için **hem** yeni oluşturma **hem** düzeltme/iptal istedi; ikisi ayrı risk
sınıfında olduğu için tek bir sonraki goal yerine muhtemelen iki alt-bölüme ayrılır:

- **Panelden yeni evrak (irsaliye/fatura/sipariş) oluşturma** — göreceli olarak daha kolay: Mikro
  yazıcıları (`MikroSalesOrderWriter`, `MikroDispatchNoteWriter`, `MikroInvoiceWriter`) ve
  `MikroSalesDocumentWriter`/Y-serisi telefon belgesi altyapısı zaten `GOAL_ERP_YAZIM*` ile
  tamamlanmış durumda; eksik olan yalnız panelden aynı job kuyruğuna girecek bir form + uç
  (bu goal'ün E3/E4'üyle aynı desen).
- **Var olan evrağı düzeltme/iptal (storno)** — Mikro'da satır fiziksel silinmez/değiştirilmez;
  muhasebesel doğru yol "ters evrak" (iade faturası, giriş irsaliyesi vb.) yazmaktır. Bu mekanizma
  **hiçbir yerde** (telefon dahil) henüz yok; `GOAL_PANEL_ERPSIZ.md` E9a bile bunu "isteğe bağlı,
  ayrı goal" olarak native tarafta bile ertelemiş. ERP tarafında ek karmaşıklık: Mikro'nun kendi
  muhasebe mantığına (dönem kilidi, KDV beyannamesi etkisi) aykırı bir storno riskli olabilir —
  bu, kullanıcıyla birlikte ayrıca tasarlanmalı.

Bu goal (ürün+cari) bittiğinde, kullanıcı onayıyla yeni bir `GOAL_PANEL_ERPLI_FIS.md` açılabilir.
