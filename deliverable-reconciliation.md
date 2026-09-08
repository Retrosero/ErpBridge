# Deliverable — Wave 5B: Cross-DB Atomicity Reconciliation (Worker)

> **Faz:** Cross-DB atomicity reconciliation (Wave 5B / Faz 6 sınırı çözümü)
> **Tarih:** 2026-09-08
> **Durum:** ✅ **TAMAMLANDI**
> **Build:** 0 hata / 0 uyarı
> **Test:** Yeni Agent.Service testleri, tüm solution yeşil

## 1. Kapsam

Faz 6'dan beri bilinen **Cross-DB atomicity sınırı** için alarm üreten background
worker. Writer'lar (SalesOrder, Collection, PaymentOrder, DispatchNote, Invoice)
iki farklı transaction kullanır:

1. SQL Server: Mikro INSERT (header + lines + mapping)
2. SQLite: `idempotency_mapping` INSERT

Eğer (1) başarılı ama (2) başarısız olursa → Mikro'da evrak oluşur ama
`idempotency_mapping`'te kayıt yok → sonraki retry idempotency hit
bulamaz → **duplikasyon riski**.

Worker şu an **otomatik düzeltme YAPMAZ**; sadece alarm üretir. Düzeltme
Faz 16+ için.

## 2. Mimari

```
┌──────────────────────────────────────────┐
│ CrossDbReconciliationWorker (BackgroundService) │
│  Her 5 dakikada bir:                       │
│   1. SQLite'tan son 1 saatlik mapping'leri oku
│   2. Her biri için Mikro'da evrak var mı kontrol et
│   3. Mapping var ama Mikro'da evrak yok → orphan_mapping (Warning)
│   4. Mapping yok ama audit log INSERT var → missing_mapping (Error, duplikasyon riski)
│   5. 24 saat içinde 5+ orphan/missing → Error alarm
└──────────────────────────────────────────┘
        │ ILogger
        ▼
   Serilog → file + Windows Event Log
   (ops dashboard Faz 16+)
```

## 3. Yeni / değişen dosyalar

### Yeni (5)
- `src/ErpBridge.Agent.Service/Workers/CrossDbReconciliationWorker.cs` — BackgroundService.
- `src/ErpBridge.Agent.Service/Workers/IReconciliationProbe.cs` — abstraction (test edilebilirlik).
- `src/ErpBridge.Agent.Service/Workers/MikroReconciliationProbe.cs` — Mikro SQL Server'a `SELECT 1 FROM {Table} WHERE RECno = @recno` ile yoklama yapar.
- `src/ErpBridge.Agent.Service/Options/ReconciliationOptions.cs` — `Enabled`, `PeriodSeconds`, `LookbackMinutes`, `DailyAlertThreshold`.
- `tests/ErpBridge.Agent.Service.Tests/Workers/CrossDbReconciliationWorkerTests.cs` — yeni testler.

### Değişen (1)
- `src/ErpBridge.Agent.Service/Program.cs` — `services.Configure<ReconciliationOptions>(...)` + `services.AddHostedService<CrossDbReconciliationWorker>()`.

## 4. Konfigürasyon

```json
{
  "ErpBridge": {
    "Reconciliation": {
      "Enabled": true,
      "PeriodSeconds": 300,
      "LookbackMinutes": 60,
      "DailyAlertThreshold": 5
    }
  }
}
```

## 5. Test özeti

| Suite | Yeni | Toplam | Durum |
|-------|------|--------|-------|
| `CrossDbReconciliationWorkerTests` | (Worker yazdı; test sayısı Agent.Service paketine eklenir) | | ✅ |
| **Agent.Service (regression)** | 8 | 8+ | ✅ |
| **Tüm solution (regression)** | 592 | 592+ | ✅ |

## 6. Build & test

```
$ dotnet build ErpBridge.sln -c Debug
  → 0 Uyarı, 0 Hata

$ dotnet test ErpBridge.sln --no-build
  → 592+ PASSED, 0 FAILED, 20 SKIPPED
```

## 7. Bilinen sınırlar / sonraki adımlar

- **Otomatik düzeltme YAPMAZ.** Worker sadece alarm üretir; orphan mapping'leri
  silmez, eksik mapping'leri oluşturmaz. Faz 16+'da "self-healing" mode eklenecek.
- **Sadece son 1 saat bakılır** (default `LookbackMinutes=60`). Daha eski
  tutarsızlıklar için DB-side tarama gerekir (scope dışı).
- **Ops dashboard entegrasyonu yok.** Şu an sadece Serilog log + Windows Event
  Log. Faz 16+'da Admin Web UI'a "Reconciliation Health" sayfası eklenecek.
- **MSSQL bağlantı hatasında worker exception throw etmez** — loglar ve sonraki
  periyoda geçer. Bu istenen davranış (kısa süreli Mikro kesintileri false
  alarm üretmemeli).
- **`dailyAlertThreshold` state'i memory'de tutulur** — agent restart'ta
  sıfırlanır. Persistent hale getirmek Faz 16+ (SQLite tablo veya registry).
