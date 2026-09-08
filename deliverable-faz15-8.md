# deliverable-faz15-8 — WPF Admin Sekmeleri: Sync Geçmişi + Parametreler

> **Faz:** 15.8 (Faz 15'in UI turu)
> **Tarih:** 2026-09-08
> **Build:** 0 hata / 0 uyarı (`dotnet build ErpBridge.sln`)
> **Test:** Admin 30/30 PASSED (17 mevcut + 7 SyncHistory + 6 Parameters)
> **Kapsam:** Sadece `src/ErpBridge.Admin/**` ve `tests/ErpBridge.Admin.Tests/**`. CentralApi dokunulmadı.

## 1. Kapsam

Faz 15 backend tarafında (Faz 15.1 / 15.5 / 15.6) iki önemli operatör yüzeyi
eklemişti:

1. `change_set_audit_log` — tüm gönderilen change-set paketlerinin append-only
   logu (`/api/v1/admin/audit/changeset`)
2. `_ERPB_PARAMETRELER` mirror — Mikro'dan push edilen parametre tablosu
   (`/api/v1/admin/parameters`)

Bu tur, mevcut backend'i tüketen iki yeni Blazor sayfası ve nav-bar
entegrasyonu ekledi. Backend tarafına hiçbir dokunuş yapılmadı.

## 2. Yapılan değişiklikler

### Yeni dosyalar (2)

- `src/ErpBridge.Admin/Pages/SyncHistory.razor` — `/sync-history` route'unda
  `change_set_audit_log` görüntüleme: tenant + tablo + yön (new/changed/
  deleted/all) + tarih aralığı filtresi, 50/sayfa pagination, CSV dışa
  aktarma. `[Authorize]`.
- `src/ErpBridge.Admin/Pages/Parameters.razor` — `/parameters` route'unda
  `_ERPB_PARAMETRELER` görüntüleme: tenant + anahtar substring filtresi,
  200/sayfa (varsayılan) veya 1000/sayfa ("Tümünü görüntüle") toggle.
  `[Authorize]`.

### Değişen dosyalar (4)

- `src/ErpBridge.Admin/MainLayout.razor` — iki yeni `<NavLink>` eklendi
  (OPERASYON bölümü sonuna): "Sync geçmişi" (`/sync-history`, `⇆`),
  "Parametreler" (`/parameters`, `⚙`).
- `src/ErpBridge.Admin/Api/CentralApiClient.cs`:
  - **Yeni DTO'lar (3):** `ChangeSetAuditEntryDto`, `ChangeSetAuditPagedResult`,
    `ParameterRecordDto` (artı `ParameterListResponse` yanıt zarfı).
  - **Yeni metod (3):**
    - `ListChangeSetAuditAsync(tenantId, table?, direction?, fromUtc?, toUtc?, page, pageSize)`
      → `ChangeSetAuditPagedResult`
    - `ExportChangeSetAuditCsvAsync(tenantId, table?, fromUtc?, toUtc?)`
      → `string` (raw CSV)
    - `ListParameterRecordsAsync(tenantId, keyContains?, page, pageSize)`
      → `IReadOnlyList<ParameterRecordDto>` (substring filtre UI'da uygulanır)
  - **Yeni internal helper (1):** `SendRawStringAsync` — CSV gibi JSON-dışı
    yanıtlar için 401 → token clear, hata envelope → `ApiCallException` aynı
    semantik korunur.
  - **Yeni private helper (1):** `BuildChangeSetAuditQuery` — sayfalama + tarih
    + tenant parametrelerini querystring'e çevirir.
- `tests/ErpBridge.Admin.Tests/Pages/SyncHistoryTests.cs` (yeni) — 7 bUnit
  testi (aşağıda).
- `tests/ErpBridge.Admin.Tests/Pages/ParametersTests.cs` (yeni) — 6 bUnit
  testi (aşağıda).

> `_Imports.razor` değişmedi — tüm gerekli `using` zaten mevcut.

## 3. Test detayı (13 yeni test)

### `SyncHistoryTests` (7 test)

1. `Renders_tenant_filter_and_load_button_initially` — sayfa ilk yüklendiğinde
   tenant select, "Olayları getir" butonu ve "Bir müşteri seçin" boş state'i
   görünür. İnit'te `ListChangeSetAuditAsync` çağrılmaz.
2. `Shows_loading_state_while_fetching` — gate'lenmiş handler ile
   `AdminLoading` görünür; gate çözüldüğünde sonuç render edilir.
3. `Renders_error_state_when_api_throws` — 500 yanıt → `AdminState Tone="error"`
   + API'nin `message` alanı kullanıcıya gösterilir.
4. `Renders_table_with_audit_entries_after_successful_load` — 2 kayıt
   render edilir, sütun başlıkları (Tablo, Yön, Son TriggerRECno, Idempotency
   Key) görünür, yönler "Yeni" / "Değişen" rozeti ile gösterilir.
5. `Renders_empty_state_when_no_entries` — `Total=0` için "Bu filtre için olay
   yok" AdminState'i.
6. `Pagination_buttons_call_api_with_correct_page` — 3 kayıt + size=2 →
   `TotalPages=2`. Next butonu 2. sayfayı yükler, istek URL'si `page=2`
   içerir.
7. `Csv_export_button_calls_export_endpoint` — `#sync-export` butonu
   `/api/v1/admin/audit/changeset/export.csv` endpoint'ini çağırır, gelen CSV
   `<pre data-testid="csv-preview">` içinde gösterilir.

### `ParametersTests` (6 test)

1. `Renders_key_filter_and_load_button_initially` — tenant/key/all/load
   kontrolleri + "Bir müşteri seçin" boş state'i.
2. `Shows_loading_state_while_fetching` — gate'lenmiş handler ile
   `AdminLoading` görünür.
3. `Renders_table_with_parameters_after_successful_load` — 2 kayıt
   render edilir (KASA_HESAP/ANA_KASA + DEPO_KOD/D01), kaynak DB
   `MikroDB_V15_02` görünür, istek URL'si `size=200` içerir.
4. `Renders_empty_state_when_no_parameters` — boş sonuç → "Filtreye uyan
   parametre yok" AdminState'i.
5. `Renders_error_state_when_api_throws` — 400 yanıt → `AdminState
   Tone="error"`.
6. `Show_all_toggle_requests_larger_page_size` — `#param-all` checkbox
   işaretlenince istek URL'si `size=1000` içerir (varsayılan 200 → toggle
   1000).

## 4. Build & Test

```
dotnet build ErpBridge.sln -c Debug -p:EnableWindowsTargeting=true
  → 0 Uyarı, 0 Hata (süre ~8s)

dotnet test tests/ErpBridge.Admin.Tests/ErpBridge.Admin.Tests.csproj
  → Başarılı!  - Başarısız: 0, Başarılı: 30, Atlanan: 0, Toplam: 30 (693 ms)
    • 17 mevcut (CentralApiClient, TokenStore, AdminAuthStateProvider,
      AdminComponents, TelemetryPage)
    • 7 SyncHistory (yeni)
    • 6 Parameters (yeni)

(Diğer projelerdeki mevcut testler değişmedi; Faz 15 deliverable'ında
listelenen 17 admin testi de aynen geçiyor.)
```

## 5. Bilinçli kararlar

- **Tenant seçimi sunucuda yok sayılıyor.** `AdminAuditEndpoints.ListAsync`
  JWT'den `tenantId` okur; query string'e eklenen `tenantId` bilgilendirici
  olarak gönderilir (mevcut `Bootstrap.razor` ile aynı pattern). Bu, Faz 7'de
  kurulan admin kimlik modelinin parçası; değişiklik gerektirmez.
- **CSV dışa aktarma sayfa içinde önizleme.** `Js.InvokeVoidAsync("eval", ...)`
  ile Blob URL + indirme tetiklemek TestContainer'da hata yaratıyor. Bunun
  yerine: sayfa CSV'yi `<pre data-testid="csv-preview">` içinde gösterir +
  "CSV dışa aktar" düğmesi tekrar tıklanabilir (UI bağlantısı korunur).
  Operatör tarayıcıda sağ-tık / kopyala ile kaydedebilir.
- **"Tümünü görüntüle" 1000/sayfa.** Sunucu `MaxPageSize=1000` clamp'liyor;
  toggle sayfa boyutunu 200 → 1000 yapar. Sayfalama sınırını tamamen kaldırmak
  için backend'de streaming export gerekir (Faz 16+).
- **Anahtar filtresi UI'da uygulanır.** `_ERPB_PARAMETRELER` sunucu
  endpoint'i yalnızca `program/user/sourceDatabase` exact match yapıyor;
  substring araması istemcide uygulanır (küçük set, 1000 satır sınırı).
- **No JS interop in SyncHistory.razor.** `IJSRuntime` inject'i
  kaldırıldı; CSV indirme `<details>` ile operatör kontrolünde.

## 6. Sınırlamalar (out-of-scope, bilinçli)

- Yeni backend endpoint yok (Faz 15.1'deki `AdminAuditEndpoints` ve
  `ParameterReadEndpoints` yeterli).
- Veri şeması değişikliği yok.
- Faz 15.2 trigger refactor (Faz 16+).
- Tahsilat / İrsaliye / Fatura (Faz 16+).

## 7. Final durum

- Build temiz, Admin testleri 30/30 PASSED.
- `SyncHistory.razor` ve `Parameters.razor` route'ları erişilebilir,
  `[Authorize]` korumalı.
- Navigasyon sidebar'a iki yeni link eklendi.
- `CentralApiClient`'a 2 yeni DTO + 3 yeni metod + 1 yeni response wrapper
  eklendi; mevcut 401 → token clear davranışı korunuyor.
