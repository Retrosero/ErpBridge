# GOAL — Hedefler (günlük/haftalık/aylık) ve panelden rut planı

> Başlangıç: 2026-09-28. İstek: telefondaki "Günün Saha Hedefi" kartı gerçek olsun; günlük/haftalık/aylık hedef;
> ciro, tahsilat, ürün, kategori, marka bazlı; hedefleri admin ve yöneticiler **panelden** kolayca girsin;
> gelişmiş bir hedef + rut planı bölümü. Durum: `GOAL_HEDEF_RUT_DURUM.md`.
> Mobil taraf: `Siparis_Cepte/docs/GOAL_HEDEF_RUT.md` bu belgeye bağlanır.
>
> **Kullanıcı kararları (2026-09-28):**
> - Gerçekleşen kaynağı: **ERP'li firmada Mikro'dan**, ERP'siz firmada **sunucudaki belgelerden**.
> - Kapsam: **ekip/bölge kavramı eklenir** — Bölge → Ekip → Kişi; saha kullanıcısı tek ekipte; yönetici bir
>   veya birden çok ekip/bölgeye sorumlu atanır ve yalnız onları görür/girer; ADMIN her şeyi görür.
>   Ekibe atanmamış yönetici bugünkü gibi tüm firmayı görür (geriye uyum). Görevler/onaylar bu işte değişmez.
> - ERP'li firmada Mikro'ya henüz işlenmemiş telefon belgeleri **ayrı "yolda"** gösterilir (çift sayma yok).
> - Rut: **panel editörü, haftalık** (bugünkü `route_plan` yapısı; sıklık/bitiş tarihi yok).
> - Yetki: dala push, PR, yerel derleme+test yeşilken `main`'e birleştirme önceden onaylı (Actions kredisi yok;
>   kırmızı CI'nın sebebi kredi olduğu sürece). Play yüklemesi **hariç**. Ajan kurulumu müşteride kullanıcı işidir.

## 1. Bugünkü durum (keşif, 2026-09-28)

- Telefon kartı sahte: `DashboardScreen.DailyTargetCard` sabit değerler (hedef 80.000, %68, "14/20").
  Sunucuda hedef kavramı yok.
- Kişi bazlı gerçekleşen yalnız sunucuda hesaplanabilir: telefonun Room tablolarında kullanıcı/plasiyer yok.
  Sunucuda `jobs.CreatedByUserId` + gövdedeki `lines[]` var (`PortalReports.MoneyDocumentsAsync` bugün
  Plasiyerler raporunu buradan hesaplıyor).
- Mikro okuyucusu (`MikroDbReader`) `cha_satici_kodu` / `sth_plasiyer_kodu` **okumuyor** (yazıcılar yazıyor).
  Kullanıcı ↔ plasiyer eşlemesi var: `mobile_user_erp_mappings.SalespersonCode`.
- Marka (`STOK_MARKALARI`: `mrk_kod`, `mrk_ismi`) ve ana grup (`STOK_ANA_GRUPLARI`: `san_kod`, `san_isim`) adları
  okunmuyor; panel yalnız kod görüyor. Alt grup adı birleşmemiş `faz-stok-alt-grup-kategori` dalında.
- Rut: `route_plan` belgesi (haftalık, gün bazlı durak) yalnız telefondan yazılıyor (`TeamDocumentProcessor`);
  panel yalnız okuyor (`Ziyaretler`). Telefonda MANAGER rut ekranında saha görünümüne düşüyor
  (`RoutePlanScreen.kt`: `role == "ADMIN"`).
- Ekip/bölge/sorumlu yönetici kavramı yok.

## 2. Kararlar

| # | Karar |
|---|---|
| K1 | Hedefler ve ekipler **yalnız CentralApi PostgreSQL'de**; ERP'li/ERP'siz firma aynı tablolar (`TenantId`). ERP'ye yazılmaz. |
| K2 | **Ekip modeli:** `sales_teams` (`Kind` = `REGION` \| `TEAM`, ekibin isteğe bağlı üst bölgesi `ParentId`). Saha kullanıcısı en çok **bir** ekipte (`sales_team_members` PK `TenantId+UserId`). Sorumlu yönetici `sales_team_managers` (bir kişi birden çok ekip/bölge). Bölge sorumlusu altındaki tüm ekipleri kapsar. Ekip yapısını **yalnız ADMIN** değiştirir. |
| K3 | **Kapsam (`TeamScope`):** ADMIN → tüm firma. MANAGER + en az bir sorumluluk → o ekip/bölgelerin üyeleri ve ekipleri. MANAGER + sorumluluk yok → tüm firma (geriye uyum). Diğer roller → yalnız kendisi (salt okunur). Tek sınıf: `Targets/TeamScope`. |
| K4 | **Hedef yazma** `RolePermissions.CanManageTargets` (ADMIN, MANAGER) + kapsam: kişi/ekip hedefi yalnız kapsamdaki kişi/ekibe; **firma geneli hedef** yalnız tüm firmayı gören kişi. |
| K5 | **Hedef = dönem + tür + ölçü + kalem + sahip + değer.** Dönem `DAILY` (`2026-10-05`), `WEEKLY` (ISO hafta, Pazartesi başlar, `2026-W41`), `MONTHLY` (`2026-10`); gün İstanbul günüdür. Tür: `REVENUE` (ciro), `COLLECTION` (tahsilat), `PRODUCT`, `CATEGORY`, `BRAND`, `VISIT` (rut ziyareti), `DOCUMENT_COUNT` (satış fişi adedi). Ölçü: `AMOUNT` \| `QUANTITY` (ürün/kategori/marka için; ciro/tahsilat daima tutar, ziyaret/fiş daima adet). Kalem: ürün kodu / kategori (ERP'de ana grup, ERP'sizde `kategori`) / marka kodu. Sahip: `USER` \| `TEAM` (ekip ya da bölge) \| `COMPANY`. Doğal anahtar tekildir: aynı dönem + tür + ölçü + kalem + sahip için tek satır (upsert). |
| K6 | **Ciro tanımı:** KDV hariç, iskonto sonrası net satış − satış iadesi. ERP'de satış faturası/irsaliye satırları (`sth_tutar` − iskonto toplamı, faturaya bağlanan irsaliye iki kez sayılmaz — satır filtresi canlı veriyle doğrulanır). ERP'sizde telefon satırının tutarı olduğu gibi (`lineTotal`; ERP'siz defter de bu değeri yazar, KDV ayrımı yapılmaz). **Tahsilat:** tahsilat tutarı (ERP'de `TAHSILAT` cari hareketi; ERP'sizde `collection` + satıştaki anında ödeme). |
| K7 | **Gerçekleşen kaynağı (kullanıcı kararı):** ERP'li firmada Mikro aynası — `stockTransactions` / `customerTransactions` satırlarının yeni `salespersonCode` alanı kişinin `mobile_user_erp_mappings.SalespersonCode`'u ile eşlenir. **Firma varsayılan koduna düşen (kendi kodu olmayan) kişinin kişisel gerçekleşeni hesaplanamaz** → panelde "plasiyer kodu eşlenmemiş" uyarısı. Ekip/bölge = üyelerinin toplamı; firma geneli = tüm hareketler (ofis satışları dahil). ERP'siz firmada `jobs` (`Succeeded`, `PortalReports` kuralları: `occurredAt` İstanbul günü, 7 gün çevrimdışı tolerans). |
| K8 | **Yolda (ERP'li):** kişinin ajana henüz gitmemiş/işlenmemiş telefon belgeleri (`Pending`/`Processing`) ayrı `pending` alanında; gerçekleşene eklenmez. Ajan eski sürümdeyse (aynada `salespersonCode` hiç yok) gerçekleşen **geçici olarak telefon belgelerinden** hesaplanır ve yanıt `source = "phone-documents"` + uyarı taşır. |
| K9 | **Türetilmiş günlük hedef:** o gün için açık bir günlük hedef yoksa, aylık (yoksa haftalık) hedeften `(dönem hedefi − bugünden önceki gerçekleşen) ÷ kalan iş günü (bugün dahil)`, en az 0. Yanıt `derived = true`. İş günleri firma ayarı (`target_settings.WorkDays`, varsayılan Pzt–Cmt). Resmî tatil takvimi yok (ilk sürüm). |
| K10 | **Ziyaret hedefi:** `VISIT` hedefi girilmemişse hedef = rut planında o dönem kişiye düşen durak sayısı (`derived = true`); gerçekleşen = `COMPLETED` ziyaret. |
| K11 | **Kolay giriş (panel):** kişi × tür tablosunda hücre düzenleme; "önceki dönemden kopyala (±%)"; "üst hedefi dağıt" (firma → bölge → ekip → kişi; **eşit** ya da **geçen dönem payına göre**; önce önizleme); CSV/.xlsx içe/dışa aktarma (`SpreadsheetReader`, ek kütüphane yok). Ekip hedefi ile üyelerin toplamı uyuşmazsa uyarı, engel değil. |
| K12 | **Değişiklik izi:** her hedef yazımı `sales_target_events`'e (eski/yeni değer, kim, ne zaman). Silme yumuşak (`IsDeleted`). |
| K13 | **Telefon salt okunur:** kişi kendi hedeflerini, yönetici ayrıca kapsamındaki kişi/ekipleri görür; düzenleme panelde. Gerçekleşen sunucudan hazır gelir, telefon son yanıtı önbellekler (çevrimdışı "son güncelleme" ile). Push yok. |
| K14 | **Rut editörü (panel):** yeni motor yok — panel ucu aynı `route_plan` / `route_plan_delete` gövdesini `TeamDocumentProcessor`'a verir (ERP'siz panel yazımındaki "ikinci kapı" deseni). Ekip kapsamı `TeamDocumentProcessor`'a eklenir: sorumluluğu olan MANAGER yalnız kapsamındaki kişilere atar (telefon ve panel aynı kural). |
| K15 | Zamanlar `long` unix ms (SQLite testleri). Hedefler cihaza senkronlanmadığı (hesaplanmış yanıt) için `UpdatedSeq`/sayaç yok. ERP'li firmada gerçekleşen bellekteki kayıt aynasından (`PortalRecordMirror`) hesaplanır; bir tarih aralığının olguları firma başına 60 sn önbelleklenir. |

## 3. Veri modeli (CentralApi, EF Core, yeni tablolar)

- `sales_teams` — `Id` (Guid), `TenantId`, `Name`(100), `Kind` (`REGION|TEAM`), `ParentId?` (TEAM → REGION),
  `IsActive`, `CreatedAtMs`, `UpdatedAtMs`.
- `sales_team_members` — PK (`TenantId`, `UserId`), `TeamId` (yalnız `TEAM`), `AddedAtMs`.
- `sales_team_managers` — PK (`TeamId`, `UserId`), `TenantId`.
- `sales_targets` — `Id` (Guid), `TenantId`, `PeriodType`, `PeriodKey`(10), `PeriodStartDay`/`PeriodEndDay`
  (`yyyy-MM-dd`, sorgu için), `Metric`, `Measure`, `ItemCode`('' = yok, 64), `ItemName?`(200, anlık kopya),
  `OwnerKind`, `OwnerId` (Guid; firma için `Guid.Empty`), `Value` (decimal 18,4), `Note?`(500),
  `CreatedByUserId`, `UpdatedByUserId`, `CreatedAtMs`, `UpdatedAtMs`, `IsDeleted`.
  Tekil indeks: (`TenantId`, `PeriodType`, `PeriodKey`, `Metric`, `Measure`, `ItemCode`, `OwnerKind`, `OwnerId`).
- `sales_target_events` — `Id`, `TenantId`, `TargetId`, `Action` (`SET|DELETE`), `OldValue?`, `NewValue?`,
  `ActorUserId`, `ActorName`, `OccurredAtMs`, `OperationId?`.
- `target_settings` — `TenantId` PK, `WorkDays` (bit maskesi Pzt=1…Paz=64; varsayılan 63), `UpdatedAtMs`.

## 4. Uçlar

**Panel** (`/api/v1/portal/...`, `MobileUserPolicy`; okuma `CanViewReports`, yazma `CanManageTargets`, ekip
yapısı `CanManageUsers`):

| Uç | İş |
|---|---|
| `GET teams` · `POST teams` · `PUT teams/{id}` · `DELETE teams/{id}` | Ekip/bölge listesi ve bakımı (üyeler, sorumlular, üst bölge). Kapsamdaki kişi listesi de burada (`GET teams/people`). |
| `PUT teams/{id}/members` · `PUT teams/{id}/managers` | Üyelik (kişi başka ekipteyse taşınır) ve sorumlu yöneticiler (yalnız ADMIN/MANAGER rollü kişi). |
| `GET targets?periodType&periodKey&teamId` | Pano (giriş ve takip aynı yanıt): kapsamdaki sahipler (firma, bölge/ekip, kişi) × özet + hedefler; her hedefte gerçekleşen, yolda, %, bugüne kadar beklenen, tempoyla tahmin, günde gereken, türetilmiş mi, alt seviye toplamı; uyarılar. |
| `PUT targets` | Toplu upsert `{operationId, items:[{periodType, periodKey, metric, measure, itemCode, ownerKind, ownerId, value, note}]}` ≤ 2000; `value = null` siler. Tek işlem. |
| `POST targets/copy` | `{fromPeriodKey, toPeriodKey, periodType, percent, ownerKinds, overwrite}` → önizleme ya da uygula. |
| `POST targets/distribute` | `{source hedef, method: EQUAL|LAST_PERIOD_SHARE, targetLevel}` → önizleme; uygulama `PUT targets` ile. |
| `GET targets/items?metric&q` | Ürün/kategori/marka seçici (kod + ad). |
| `GET|PUT targets/settings` | İş günleri. |
| `GET routes` · `GET routes/{planId}` · `PUT routes/{planId}` · `DELETE routes/{planId}` | Panel rut editörü (K14). |
| `GET routes/compliance?from&to&teamId` | Planlanan / yapılan / atlanan ziyaret, kişi ve gün kırılımı. |

**Telefon** (`/api/v1/android/targets`, `MobileUserPolicy`, kişi başı hız sınırı):

| Uç | İş |
|---|---|
| `GET targets/mine?date` | Kişinin gün/hafta/ay hedefleri (türetilmiş günlük dahil), gerçekleşen, yolda, ziyaret planı, fiş adedi, `asOfMs`, `source`. |
| `GET targets/team?date` | Yönetici: kapsamdaki kişi ve ekiplerin özet ilerlemesi (salt okunur). Yetkisiz 403. |

## 5. Görevler

### Sunucu (ErpBridge)
| # | Görev |
|---|---|
| S1 | Domain + DbContext eşlemeleri + migration (`HedeflerVeEkipler`); `RolePermissions.CanManageTargets`; `TeamScope` |
| S2 | Ekip uçları + testler |
| S3 | `TargetService`: upsert/sil/kopyala/dağıt, yetki ve kapsam, değişiklik izi |
| S4 | `TargetProgressService`: ERP'siz (jobs) ve ERP'li (Mikro aynası + `salespersonCode`, yolda, yedek kaynak) gerçekleşen; türetilmiş günlük; ziyaret; 60 sn önbellek |
| S5 | Panel + telefon uçları, SQLite ilişkisel testler (ERP'li/ERP'siz, kapsam, firma yalıtımı, dönem sınırları, türetilmiş hedef) |
| S6 | KB (kural 31) + `docs/api-contracts.md` + PR |

### Ajan (ErpBridge, Mikro okuyucusu)
| # | Görev |
|---|---|
| E1 | `customerTransactions.salespersonCode` (`cha_satici_kodu`), `stockTransactions.salespersonCode` (`sth_plasiyer_kodu`); `SnapshotProjectionVersion` 7 → 8; canlı okuma testi (KB kural 26: Dapper kolon sırası) |
| E2 | Lookup'lar: `stock_brand` (`STOK_MARKALARI`), `stock_main_group` (`STOK_ANA_GRUPLARI`) — `faz-stok-alt-grup-kategori` ile çakışmaması için o dal birleştiyse üstüne |
| E3 | `VersionPrefix` 1.2.0 → 1.3.0; KB. **Not:** yeni alan hareket satırlarının özetini değiştirir → filo `cariHareketleri`/`stokHareketleri`'ni bir kez yeniden indirir (projeksiyon 6'daki gibi). Müşteride ajan güncellenene kadar K8 yedeği çalışır. |

### Panel (ErpBridge.Portal) — menüde yeni "Hedefler & Rut" grubu
| # | Görev |
|---|---|
| P1 | `PortalArea.Targets` (+ `PortalRoles`/`RolePermissions` birlikte); `Ekipler` sayfası (bölge → ekip ağacı, üyeler, sorumlular) |
| P2 | `Hedefler` sayfası: dönem seçici (gün/hafta/ay), kişi × tür tablosu (hücre düzenleme, toplu kaydet), ürün/kategori/marka sekmesi (arama ile kalem), kopyala, dağıt (önizleme), CSV/.xlsx içe/dışa aktarma |
| P3 | `Hedef takibi` sayfası: ilerleme çubukları, sıralama, tahmin, günlük gereken, ekip filtresi, uyarılar (eşlenmemiş plasiyer, eski ajan) |
| P4 | `Rut planı` sayfası: kişi/plan seç, 7 gün kolonunda cari ekle/sırala/kaldır, kopyala, kaydet (K14) |
| P5 | `Rut uyumu` raporu; bUnit testleri |

### Telefon (Siparis_Cepte) — ayrıntı `Siparis_Cepte/docs/GOAL_HEDEF_RUT.md`
| # | Görev |
|---|---|
| A1 | `TargetApi` + önbellek (Room, açık `Migration(46, 47)`), senkron tetikleyicileri |
| A2 | Ana ekran kartı: Gün/Hafta/Ay, gerçek ciro halkası + tahsilat/ziyaret/fiş; KPI şeridindeki sabit değerler gerçek |
| A3 | "Hedeflerim" ekranı (tüm hedefler, yolda, türetilmiş etiketi, günlük gereken) + yönetici "Ekip hedefleri" |
| A4 | Rut ekranı: yönetici = ADMIN veya MANAGER (rol seti) |
| A5 | KB + sürüm; Play yüklemesi kullanıcıda |

## 6. Sıra
S1–S6 (PR 1) → P1–P3 (PR 2) → E1–E3 (PR 3) → A1–A5 (Sipariş Cepte PR) → P4–P5 + K14 kapsamı (PR 4).
