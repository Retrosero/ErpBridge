# ErpBridge Saha-Satış ERP Sync Düzeltme Raporu

**Tarih:** 2026-08-12
**Tenant:** `ed4b71de`
**API Key:** `AK-38b87a7f8133c561a6a7357af2206fc491f12054a4daa70a`
**Base URL:** `https://lisans.appsgo.cloud`

## Sorun

Saha satış uygulaması ERP verilerini senkronize ederken sadece **stok kartları, barkod, eldeki miktar** tablolarını düzgün aktarıyordu; diğer tüm tablolar hata veriyordu.

## Kök Neden

Android istemcisindeki (`FieldOpsApiService.kt`) endpoint yolları, merkezi API'nin (`ErpBridge.CentralApi/Endpoints/AndroidEndpoints.cs`) sunduğu gerçek yollarla uyuşmuyordu.

## Yapılan Değişiklikler

### 1) `app/src/main/java/com/example/data/api/FieldOpsApiService.kt`

Üç yanlış yol, server'daki gerçek yola düzeltildi:

| Eski (yanlış)                  | Yeni (doğru)              | Açıklama                       |
| ------------------------------ | ------------------------- | ------------------------------ |
| `cariHareket`                  | `cariHareketleri`         | Server singular değil, plural  |
| `barkodTanimi`                 | `barkodlar`               | Server singular değil, plural  |
| `cariAdresleri`                | `cariAdresler`            | Server singular form kullanır  |

```kotlin
@POST("api/v1/android/sync/cariHareketleri")
suspend fun getCariHareket(...)

@POST("api/v1/android/sync/barkodlar")
suspend fun getBarkodTanimi(...)

@POST("api/v1/android/sync/cariAdresler")
suspend fun getCariAdresleri(...)
```

### 2) `app/src/main/java/com/example/ui/screens/BridgeSyncHelper.kt`

İki düzeltme:

a) `handleApiError` fonksiyonuna 404 case'i eklendi (daha anlamlı hata mesajı):
```kotlin
val userFriendlyMessage = when (code) {
    401, 403 -> "Yetkilendirme Hatası: ..."
    404 -> "Endpoint mevcut değil: ${response.raw().request.url.encodedPath} ($safeMessage)"
    422 -> "Doğrulama Hatası: ..."
    429 -> "İstek Sınırı Aşıldı: ..."
    in 500..599 -> "Sunucu Hatası: ..."
    else -> "Ağ Hatası [$code] ($safeMessage)"
}
```

b) Merkezi API'de hiç bulunmayan 4 endpoint (`cariBankaHesaplari`, `bankalar`, `kasalar`, `kasaYonetim`) artık 404 aldığında sync zincirini kırmak yerine bilgilendirici log düşüp graceful skip yapıyor:

```kotlin
} else {
    val err = handleApiError(response, log)
    if (response.code() == 404) {
        log("⚠️ '$entity' endpoint'i merkezi API'de mevcut değil (HTTP 404). Bu tablo için sync atlanıyor.")
        hasMore = false
    } else {
        throw err
    }
}
```

### 3) `gradle.properties`

Android Gradle Plugin'in path kontrolü override edildi (proje yolu Türkçe karakter içeriyor):
```
android.overridePathCheck=true
```

## Doğrulama — Canlı API Testi

`tenant=ed4b71de` ve `apikey=AK-...` ile merkezi API'den dönen kayıt sayıları:

| Endpoint                              | Server Total | Durum              |
| ------------------------------------- | -----------: | ------------------ |
| `cari` (müşteriler)                   |          518 | ✅ OK               |
| `urun` (stok kartları)                |        4,477 | ✅ OK               |
| `stokSeviye` (eldeki miktar)          |        4,276 | ✅ OK               |
| `barkodlar`                           |        4,448 | ✅ OK               |
| `cariAdresler`                        |          159 | ✅ OK               |
| `cariHareketleri`                     |       12,456 | ✅ OK               |
| `stokHareket`                         |       82,881 | ✅ OK               |
| `faturaHareket`                       |        9,574 | ✅ OK               |
| `stokSatisFiyatListeTanimlari`        |            3 | ✅ OK               |
| `stokSatisFiyatListeleri`             |       12,841 | ✅ OK               |
| `fiyatlar`                            |       12,841 | ✅ OK               |
| `cariYetkililer`                      |            1 | ✅ OK               |
| `satisSartlari`                       |            0 | ✅ OK (boş tenant)  |
| `acikSiparisler`                      |            0 | ✅ OK (boş tenant)  |
| `cariBankaHesaplari`                  |            - | ⚠️ 404 (skip edildi) |
| `bankalar`                            |            - | ⚠️ 404 (skip edildi) |
| `kasalar`                             |            - | ⚠️ 404 (skip edildi) |
| `kasaYonetim`                         |            - | ⚠️ 404 (skip edildi) |

**Toplam aktarılabilir kayıt:** 144,475 satır ERP verisi.

## Build Durumu

```
> Task :app:compileDebugKotlin
BUILD SUCCESSFUL in 2m 1s
8 actionable tasks: 3 executed, 5 up-to-date
```

Tek uyarılar mevcut `Divider` / `Icons.Filled.X` deprecated API'leri — bunlar bu değişikliklerden kaynaklanmıyor, önceden vardı.

## Google AI Studio'ya Kopyala-Yapıştır

AI Studio'daki prompt/projene aynı düzeltmeyi uygulamak istersen, sistem talimatına veya prompt'a şu metni ekle:

```
Bu projedeki tüm Android sync endpoint URL'leri
https://lisans.appsgo.cloud base URL'i için aşağıdaki kurallara
uymalıdır:

  - cariHareket DEĞİL, cariHareketleri (plural)
  - barkodTanimi DEĞİL, barkodlar
  - cariAdresleri DEĞİL, cariAdresler (tekil)
  - cariBankaHesaplari, bankalar, kasalar, kasaYonetim server'da
    yok; 404 aldığında exception fırlatmak yerine log düşüp skip et.
```
