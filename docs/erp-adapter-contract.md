# Yeni ERP Adaptörü Ekleme Sözleşmesi

> Bu doküman, ErpBridge'e yeni bir ERP adaptörü (Logo, Netsis, Paraşüt…) eklemek
> için uyulması gereken sözleşmeyi ve sırayı tanımlar.
>
> **Referans:** Mikro adaptörü (`ErpBridge.Erp.Mikro`) tam implementasyon,
> `ErpBridge.Erp.Logo` ise iskelet örneğidir. Dikişin gerçekten tuttuğunu
> `SecondAdapterSeamTests` sabitler — o testler kırılıyorsa soyutlama gerilemiş
> demektir.

---

## 0. Altın kural: şemayı tahmin etme

ErpBridge'in en pahalı hatası buydu. Mikro kataloğu ve **yedi writer'ın tamamı**
gerçek bir veritabanına karşı hiç doğrulanmamış kolon adlarıyla yazılmıştı:

| | Canlı DB'ye karşı geçersiz kolon |
|---|---|
| Yazma yolu (7 writer) | **179** |
| Tracked-table kataloğu | **142** (63 tablodan 34'ünde) |

212 birim testinin hepsi geçiyordu — çünkü hepsi mock bağlantı kullanıyordu.
Var olmayan bir kolonu adlandıran `INSERT` de "geçer".

**Bu yüzden:**
1. Kolon adlarını **canlı bir veritabanından** ya da vendor'ın kanıtlanmış bir
   uygulamasından türet.
2. `MikroSchemaContractTests` benzeri bir **canlı şema sözleşme testi** yaz.
   Read-only'dir, `ERPBridge_RUN_INTEGRATION=1` ile açılır, CI hermetik kalır.
3. Her `INSERT`'i rollback edilen bir transaction içinde gerçek DB'de çalıştır.

---

## 1. Proje iskeleti

```
src/ErpBridge.Erp.<Vendor>/
├── ErpBridge.Erp.<Vendor>.csproj
├── <Vendor>Adapter.cs                    → IErpAdapter
├── ChangeLog/<Vendor>TrackedTableCatalog.cs → IErpTrackedTableCatalog
└── Writers/ …                            → evrak yazıcıları
```

**İzin verilen referanslar** — Mikro ve Logo'nun ikisi de bu setle yetiniyor:

| Referans | Neden |
|---|---|
| `ErpBridge.Shared` | `Result<T>`, maskeleme yardımcıları |
| `ErpBridge.Erp.Abstractions` | Sözleşmeler |
| `ErpBridge.Erp.Sql` | SQL Server ise: ortak change-capture motoru |

**Referans veremez:** `Core`, `LocalStore`, `RemoteApi`, `Agent.*`, başka bir
vendor adaptörü. Bunlardan birine ihtiyaç duyuyorsan soyutlama sızıyor demektir —
düzeltilecek yer adaptör değil, sözleşmedir.

---

## 2. Zorunlu sözleşmeler

### 2.1 `IErpAdapter`
7 evrak tipi + bootstrap okuma + bağlantı/sürüm testi.

**Kural:** implemente edilmemiş bir metot `NotImplementedException` atmalı,
boş/sahte sonuç dönmemeli. Yarım yazılmış bir evrak müşterinin defterine düşer;
boş bir bootstrap snapshot'ı merkezi API tarafından "bu müşteride veri yok" diye
okunur.

### 2.2 `IErpTrackedTableCatalog`
Değişiklik izlenecek tablolar.

| Alan | Kural |
|---|---|
| `TableId` | **Katalog içinde benzersiz olmalı.** Shadow satırındaki tek ayırt edici alandır; paylaşılırsa her iki tablonun değişikliği de sessizce kaybolur. Bir kez yayınlandıktan sonra asla yeniden numaralanmaz. |
| `TableKey` | Wire ve cursor'da kullanılan kararlı ad |
| `Fields` | Okunacak kolon beyaz listesi — anahtar kolonu da içermeli |
| `KeyKind` | `Int` → `KayitRECno`, `Guid` → `KayitGuid` |

**Sürüm farkı:** aynı ERP'nin iki majör sürümü farklı şema olabilir. Mikro V16,
V15'in üst kümesi *değil* — `*_RECno` kolonlarını tamamen kaldırmış. Bu durumda
sürüm başına ayrı katalog yaz ve `MikroTrackedTableCatalog.For(version)` gibi bir
seçici ekle.

### 2.3 Değişiklik yakalama
SQL Server tabanlı bir ERP için `SqlServerShadowTableChangeLog`'a katalog +
`KeyKindProjection` + kendi `ShadowTableOptions`'ını ver, gerisi hazır.

**Kendi `ShadowTableOptions`'ını vermek zorunludur** — iki ERP aynı sunucuda
kurulu olabilir.

REST tabanlı bir ERP (Paraşüt) için `IErpChangeLogSource`'u kendin implemente et
ya da `ChangeDetectionCapability.TimestampDelta` bildirip snapshot yoluna düş.

> ⚠️ **Trigger kurulum riski:** Logo/Netsis kurulumlarında müşterinin
> veritabanına trigger eklemek vendor'ın desteklenen konfigürasyonu dışında
> kalabilir. Müşteriyle teyit et. **SQL Server Change Tracking** daha düşük
> etkili alternatiftir: yerleşik, silmeleri verir, trigger gerektirmez.

---

## 3. Writer sözleşmesi

Referans uygulamanın standardı — her yazımda aynı sıra:

1. Payload şemasını doğrula
2. `tenant_id + document_type + external_id` ile **idempotency** kontrolü
3. Lookup kontrolleri (cari/stok/depo/kasa/banka var mı)
4. ERP sürümünü tespit et
5. **Evrak seri/sıra seç ve çakışma kontrolü** → `MikroDocumentNumberAllocator`
6. Transaction başlat
7. Header + satırlar + açıklamalar **aynı transaction içinde**
8. Sürüme özgü kimlik bağlantılarını kur
9. Commit
10. Yerel `mappings` tablosuna kaydet
11. Merkeze ack gönder

**Asla:**
- Aynı iş tekrar geldiğinde ikinci evrak oluşturma
- Header yazıp satırlar yazılmadan commit etme
- ERP transaction'ı başarısızken merkeze success ack gönderme
- Kullanıcı girdisini string concat ile SQL'e ekleme

### 3.1 String alan genişlikleri
`SqlServerFieldWidthProvider` ile **canlı şemadan keşfet**, sabit yazma —
genişlikler sürüme göre değişir (`sto_isim`: V15'te 50, V16'da 127).

`ErpFieldText` ile kimlik ve serbest metni ayır:

| Tür | Davranış | Neden |
|---|---|---|
| `Identifier()` | Taşarsa **exception** | Kısaltılmış bir `cari_kod` başka bir hesapla eşleşip belgeyi **yanlış müşteriye** yazabilir |
| `FreeText()` | **Kırpar** | Açıklamanın sonunu kaybetmek, belgeyi tümden reddetmekten iyidir |

Mikro genişlikleri dardır: `evrakno_seri` 6, `cari_kod` 25, `cha_aciklama` 40.

### 3.2 Kimlik / self-link
Mikro her tabloda `(*_RECid_DBCno, *_RECid_RECno)` UNIQUE index tutar ve
konvansiyon `RECid_RECno = RECno`'dur. `IDENTITY` insert öncesi bilinmediğinden
`MikroSelfLink` benzersiz negatif placeholder tohumlayıp aynı transaction içinde
çözer. Başka bir ERP'de benzer bir kısıt varsa aynı deseni kullan.

### 3.3 Tip eşlemesi
Vendor'ın kendi kod taksonomilerini **tek yerde** topla (`MikroDocumentCodes`
gibi) — iki writer aynı hareketi farklı kodla yazmasın. Para birimi gibi alanlar
genelde sayısal koddur, ISO string değil (`MikroCurrency`).

---

## 4. Kayıt (registration)

`ErpAdapterRegistration.AddErpBridgeErpAdapter` switch'ine bir arm ekle.
İskelet aşamasındaki bir adaptör için **açık bir red** döndür — kayıtlı olsa ajan
sonradan throw edeceği işleri kabul ederdi.

---

## 5. Tanım-tamamlandı

- [ ] `dotnet build` — 0 uyarı (`TreatWarningsAsErrors=true`)
- [ ] `dotnet test` — hermetik suite yeşil
- [ ] **Canlı şema sözleşme testi** — katalog ve writer kolonları 0 eksik
- [ ] Her `INSERT` gerçek DB'de rollback'li transaction içinde doğrulandı
- [ ] `TableId` benzersizliği test edilmiş
- [ ] Adaptör `Shared` + `Erp.Abstractions` (+ `Erp.Sql`) dışında referans vermiyor
- [ ] `SecondAdapterSeamTests` hâlâ geçiyor (ortak koda dokunulmadı)
- [ ] Kendi `ShadowTableOptions`'ı var
