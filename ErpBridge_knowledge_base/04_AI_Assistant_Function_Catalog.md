# 04 — ErpBridge AI Asistanı Veri ve Fonksiyon Kataloğu

> **Hedef:** Doğal dil analiz sorularını yanıtlamak üzere koşturulacak
> salt-okunur (read-only) SQL/Dapper sorgu şablonları ve parametrik tool
> tanımları.
>
> **⚠️ Kolon adları canlı Mikro şemasına karşı doğrulanmıştır.** Kritik nüanslar:
> - `CARI_HESAP_HAREKETLERI`'de `cha_borc`/`cha_alacak` **yoktur** —
>   `cha_meblag` (tutar) + `cha_tip` (0=borç, 1=alacak).
> - Cari kod kolonu `cha_kod`'dur (`cha_cari_kod` değil).
> - `cha_vade` bir **gün sayısıdır**, tarih değil. Vade = `cha_tarihi + cha_vade gün`.
> - `SIPARISLER` **satır başına bir satırdır**; sipariş sayısı
>   `(sip_evrakno_seri, sip_evrakno_sira)` üzerinden `DISTINCT` sayılır.

İlgili diğer modüller:
- Genel Mimari: [[00_System_Overview]]
- Veri Sözlüğü & Şemalar: [[03_Data_Dictionary_and_Rules]]

---

## 1. Backend Data Provider Mimarisi

```text
[AI Model Tool Call] ──> [GET /api/v1/analytics/...] ──> [Dapper Compiled Query] ──> [JSON Response]
```

---

## 2. Fonksiyonel Tool Tanımları ve SQL Şablonları

### 2.1 `getRevenueComparison` (Ciro Karşılaştırma)
İki tarih aralığının cirosunu ve sipariş sayısını karşılaştırır.

```sql
SELECT
    COUNT(DISTINCT RTRIM(sip_evrakno_seri) + '-' + CAST(sip_evrakno_sira AS VARCHAR(12))) AS OrderCount,
    SUM(sip_tutar)                       AS LineRevenue,
    SUM(sip_b_fiyat * sip_miktar)        AS GrossRevenue
FROM SIPARISLER WITH (NOLOCK)
WHERE sip_tarih BETWEEN @StartDate AND @EndDate
  AND ISNULL(sip_kapat_fl, 0) = 0
  AND (@CompanyNo = 0 OR sip_firmano = @CompanyNo);
```

---

### 2.2 `getOverdueReceivables` (Vadesi Geçen Alacaklar)
Vadesi geçmiş bakiyeleri ve gecikmeyi çeker. Bakiye yönü `cha_tip` ile, vade
`cha_tarihi + cha_vade gün` ile hesaplanır.

```sql
SELECT
    h.cha_kod                                   AS CustomerCode,
    c.cari_unvan1                               AS CustomerName,
    SUM(CASE WHEN ISNULL(h.cha_tip, 0) = 0
             THEN ISNULL(h.cha_meblag, 0)
             ELSE -ISNULL(h.cha_meblag, 0) END) AS Balance,
    DATEDIFF(day,
             MIN(DATEADD(day, ISNULL(h.cha_vade, 0), h.cha_tarihi)),
             GETDATE())                         AS MaxDelayDays
FROM CARI_HESAP_HAREKETLERI h WITH (NOLOCK)
JOIN CARI_HESAPLAR c WITH (NOLOCK) ON c.cari_kod = h.cha_kod
WHERE DATEADD(day, ISNULL(h.cha_vade, 0), h.cha_tarihi) < CAST(GETDATE() AS DATE)
GROUP BY h.cha_kod, c.cari_unvan1
HAVING SUM(CASE WHEN ISNULL(h.cha_tip, 0) = 0
                THEN ISNULL(h.cha_meblag, 0)
                ELSE -ISNULL(h.cha_meblag, 0) END) > 0
ORDER BY Balance DESC;
```

---

### 2.3 `getDormantCustomers` (Hareketsiz Cariler)
Belirli gün sayısından uzun süredir sipariş vermeyen müşteriler.

```sql
SELECT
    c.cari_kod, c.cari_unvan1,
    MAX(s.sip_tarih)                                 AS LastOrderDate,
    DATEDIFF(day, MAX(s.sip_tarih), GETDATE())       AS DaysInactive
FROM CARI_HESAPLAR c WITH (NOLOCK)
LEFT JOIN SIPARISLER s WITH (NOLOCK) ON s.sip_musteri_kod = c.cari_kod
GROUP BY c.cari_kod, c.cari_unvan1
HAVING DATEDIFF(day, MAX(s.sip_tarih), GETDATE()) > @InactivityDays
ORDER BY DaysInactive DESC;
```

---

### 2.4 `getCustomerBalance` (Cari Bakiye)
Tek bir carinin resmi bakiyesi — `CARI_HESAPLAR`'da bakiye kolonu olmadığından
hareketlerden toplanır.

```sql
SELECT
    SUM(CASE WHEN ISNULL(cha_tip, 0) = 0
             THEN ISNULL(cha_meblag, 0)
             ELSE -ISNULL(cha_meblag, 0) END) AS Balance
FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK)
WHERE cha_kod = @CustomerCode;
```

---

### 2.5 `getTopSellingEntitiesByProduct` (Ürün Bazlı Liderler)
Belirli bir ürünü en çok alan müşteriler / satan plasiyerler. Kaynak
`STOK_HAREKETLERI` (`sth_stok_kod`, `sth_cari_kodu`, `sth_miktar`, `sth_tutar`,
`sth_plasiyer_kodu`).

- **Parametreler:** `stockCode`, `startDate`, `endDate`, `limit`.

---

### 2.6 `getRouteReplenishmentSuggestions` (Proaktif Sipariş Önerisi)
Rota gününde müşterinin ortalama sipariş frekansına göre stok yenileme ihtiyacı.
Kaynak `SIPARISLER` (`sip_musteri_kod`, `sip_stok_kod`, `sip_miktar`, `sip_tarih`).
