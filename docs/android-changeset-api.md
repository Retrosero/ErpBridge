# Android Change-Set API (Faz 15.7)

Bu doküman, Android saha satış uygulamasının lisans sunucudan
**değişen** ve **silinen** verileri nasıl çektiğini tanımlar. Faz 15
ile birlikte FORA referans uygulamasının semantiği benimsenmiştir:
"new + changed" tek bir akışta birleştirilmiş, "deleted" ayrı bir
akışta gelir; hepsi `TriggerRECno` tabanlı cursor ile sayfalar.

## 1. Endpoint özeti

| Yol | Yön | Auth | Açıklama |
|---|---|---|---|
| `GET /api/v1/android/changeset/tables` | keşif | ApiKey | İzlenen tabloların listesi + son `TriggerRECno` |
| `GET /api/v1/android/changeset/{table}/new_or_changed` | yeni + güncellenen (tek akış) | ApiKey | **Faz 15 birincil yön.** |
| `GET /api/v1/android/changeset/{table}/new` | yeni (alias) | ApiKey | Geriye uyumluluk — `/new_or_changed` ile aynı payload |
| `GET /api/v1/android/changeset/{table}/changed` | güncellenen (alias) | ApiKey | Geriye uyumluluk |
| `GET /api/v1/android/changeset/{table}/deleted` | silinen | ApiKey | Sadece `KayitKey` + `TriggerRecNo` |
| `GET /api/v1/android/changeset/{table}/status` | meta | ApiKey | Son `TriggerRECno` + "yeni veri var mı?" |
| `GET /api/v1/android/parameters` | parametre | ApiKey | `_ERPB_PARAMETRELER` snapshot |

## 2. İstek / cevap sözleşmesi

### 2.1 Tables keşfi

```http
GET /api/v1/android/changeset/tables
Authorization: Bearer <api-key>
```

Yanıt:

```json
{
  "tenantId": "4d3f...-...-...-...-...",
  "tables": [
    {
      "table": "STOKLAR",
      "tabloId": 13,
      "lastTriggerRecNo": 12345,
      "lastPulledAtUtc": "2026-09-07T10:30:00Z",
      "acceptedBundles": 7
    }
  ]
}
```

### 2.2 New + Changed (birleşik)

```http
GET /api/v1/android/changeset/STOKLAR/new_or_changed?cursor=12345&size=1000
```

Yanıt:

```json
{
  "table": "STOKLAR",
  "direction": "new_or_changed",
  "cursor": 12345,
  "nextCursor": 13345,
  "rows": [
    { "sto_RECno": 67890, "sto_kod": "ABC-001", "sto_isim": "..." }
  ]
}
```

- `cursor`: son çektiğin `TriggerRECno`. İlk çağrıda `0` gönderilir.
- `nextCursor`: bu sayfadaki son `TriggerRECno`. `null` ise batch tamam.
- `rows`: payload içindeki tüm kolonlar (Mikro'nun şemasından).

### 2.3 Deleted

```http
GET /api/v1/android/changeset/STOKLAR/deleted?cursor=12000
```

Yanıt:

```json
{
  "table": "STOKLAR",
  "cursor": 12000,
  "nextCursor": 12010,
  "rows": [
    { "KayitRecNo": 12321, "TriggerRecNo": 12001 },
    { "KayitRecNo": 12322, "TriggerRecNo": 12002 }
  ]
}
```

Deleted payload'ı sadece primary key + `TriggerRecNo` taşır; silinen
satırın geri kalan kolonları Mikro'da artık yoktur.

### 2.4 Status (operatör UI ve Android keşfi)

```http
GET /api/v1/android/changeset/STOKLAR/status
```

Yanıt:

```json
{
  "table": "STOKLAR",
  "lastTriggerRecNo": 12345,
  "hasPending": true
}
```

### 2.5 Parametreler

```http
GET /api/v1/android/parameters
```

Yanıt:

```json
{
  "tenantId": "...",
  "count": 42,
  "items": [
    {
      "parametreProgram": "FORA",
      "parametreUser": "ADMIN",
      "parametreAnaGrubu": "GENEL",
      "parametreAltGrubu": "SENKRONIZASYON",
      "parametreID": "AktifDonem",
      "parametreAdi": "Aktif Dönem",
      "parametreDegeri": "2026",
      "updatedAtUtc": "2026-09-01T08:00:00Z"
    }
  ]
}
```

## 3. Tipik Android pseudocode

```kotlin
suspend fun pullChanges(table: String) {
    // 1) yeni + güncellenen (Faz 15 birincil yön)
    var cursor = localStore.getCursor(table, direction = "new_or_changed")
    while (true) {
        val resp = api.get("/android/changeset/$table/new_or_changed?cursor=$cursor&size=1000")
        localStore.upsertAll(table, resp.rows)        // payload'daki kolonlar
        if (resp.nextCursor == null) break
        cursor = resp.nextCursor
        localStore.setCursor(table, direction = "new_or_changed", cursor)
    }

    // 2) silinen (Faz 11'den beri var)
    cursor = localStore.getDeletedCursor(table)
    while (true) {
        val resp = api.get("/android/changeset/$table/deleted?cursor=$cursor&size=1000")
        resp.rows.forEach { row ->
            localStore.deleteByPrimaryKey(table, row.KayitRecNo)
        }
        if (resp.nextCursor == null) break
        cursor = resp.nextCursor
        localStore.setDeletedCursor(table, cursor)
    }
}
```

## 4. Idempotency ve retry

- Aynı `cursor` ile ikinci kez çağrıldığında agent'ın gönderdiği
  aynı satırlar dönebilir; Android client **upsert** kullandığı için
  bu güvenlidir.
- `nextCursor` null olduğunda batch tamamlanmıştır. Null olmaması
  "daha fazla var" anlamına gelir; sayfa sonuna kadar çağrıya devam
  edilir.
- `change_sets` tablosu `(TenantId, SourceDatabase, TableName,
  LastTriggerRecNo)` üzerinden unique constraint ile korunur; agent'ın
  aynı bundle'ı tekrar göndermesi 23505 unique violation ile reddedilir.
- `change_set_audit_log` tablosu `(TenantId, IdempotencyKey, Direction)`
  üzerinden unique constraint ile korunur; operatör "agent ne gönderdi"
  sorusunu audit log üzerinden cevaplayabilir.

## 5. Rate limiting

- `/api/v1/android/changeset/*` yolları `PerTenantRateLimitPolicy`
  (varsayılan: dakikada 100 istek/tenant) ile sınırlıdır.
- Audit log retention `AuditRetentionOptions.RetentionDays` ile
  yapılandırılır (varsayılan 365 gün).

## 6. Faz 15 kapsamı

- ✅ `_ERPB_SYNC` + `_ERPB_SYNC_DEL` FORA benzeri iki tablo (DDL
  sabitleri `TriggerSchema.cs` içinde).
- ✅ `change_set_audit_log` append-only tablo (admin `GET /audit/changeset`).
- ✅ `_ERPB_PARAMETRELER` POST ingest + Android pull.
- ✅ Android `new_or_changed` birleşik endpoint.
- ⏳ `TriggerInstaller` büyük refactor (mevcut `Islem`-tabanlı trigger
  korunur, yeni `KayitKey` tabanlı trigger'lar bir sonraki turda
  aktifleştirilecek).
