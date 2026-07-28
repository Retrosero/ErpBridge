package com.example.data.api

import retrofit2.Response
import retrofit2.http.*

interface FieldOpsApiService {
    @POST("api/v1/android/sync/stokHareket")
    suspend fun getStokHareket(@Body request: PullJobsRequest): Response<StokHareketResponse>

    @POST("api/v1/android/sync/cariHareket")
    suspend fun getCariHareket(@Body request: PullJobsRequest): Response<CariHareketResponse>

    @POST("api/v1/android/sync/barkodTanimi")
    suspend fun getBarkodTanimi(@Body request: PullJobsRequest): Response<BarkodTanimiResponseDto>

    @POST("api/v1/android/sync/cariAdresleri")
    suspend fun getCariAdresleri(@Body request: PullJobsRequest): Response<CariAdresResponseDto>

    @POST("api/v1/android/sync/cariBankaHesaplari")
    suspend fun getCariBankaHesaplari(@Body request: PullJobsRequest): Response<CariBankaHesapResponseDto>

    @POST("api/v1/android/sync/trigger")
    suspend fun triggerSync(@Body request: PullJobsRequest): Response<FieldOpsTriggerResponse>

    @POST("api/v1/android/sync/status")
    suspend fun getSyncStatus(@Body request: PullJobsRequest): Response<SyncStatusResponseDto>

    @POST("api/v1/android/sync/pushStatus")
    suspend fun getPushStatus(@Body request: PullJobsRequest): Response<FieldOpsPushStatusResponse>

    @POST("api/v1/android/sync/fiyatListesi")
    suspend fun getFiyatListesi(@Body request: PullJobsRequest): Response<okhttp3.ResponseBody>












    @POST("api/v1/sync/push")
    suspend fun push(@Body request: Map<String, Any?>): Response<FieldOpsPushResponse>








    @POST("api/v1/android/bootstrap")
    suspend fun bootstrap(@Body request: BootstrapRequest): Response<BootstrapResponse>

    @POST("api/v1/android/pull")
    suspend fun pullJobs(@Body request: PullJobsRequest): Response<PullJobsResponse>

    @POST("api/v1/android/push")
    suspend fun pushJobs(@Body request: PushJobsRequest): Response<PushJobsResponse>

    @POST("api/v1/android/sync/cari")
    suspend fun getCariler(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<CariDto>>

    @POST("api/v1/android/sync/urun")
    suspend fun getUrunler(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<UrunDto>>

    @POST("api/v1/android/sync/stokSeviye")
    suspend fun getStokSeviye(@Body request: PullJobsRequest): Response<StokSeviyeResponse>

    @POST("api/v1/android/sync/cariHareketleri")
    suspend fun getCariHareketleri(@Body request: PullJobsRequest): Response<CariHareketResponse>

    @POST("api/v1/android/sync/faturaHareket")
    suspend fun getFaturaHareket(@Body request: PullJobsRequest): Response<FaturaHareketResponse>

    @POST("api/v1/android/sync/stokSatisFiyatListeTanimlari")
    suspend fun getStokSatisFiyatListeTanimlari(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<StokSatisFiyatListeTanimlariDto>>

    @POST("api/v1/android/sync/stokSatisFiyatListeleri")
    suspend fun getStokSatisFiyatListeleri(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<StokSatisFiyatListeleriDto>>

    @POST("api/v1/android/sync/bankalar")
    suspend fun getBankalar(@Body request: PullJobsRequest): Response<BankalarResponseDto>

    @POST("api/v1/android/sync/kasalar")
    suspend fun getKasalar(@Body request: PullJobsRequest): Response<KasalarResponseDto>

    @POST("api/v1/android/sync/kasaYonetim")
    suspend fun getKasaYonetim(@Body request: PullJobsRequest): Response<KasaYonetimResponseDto>

    @POST("api/v1/android/sync/status")
    suspend fun getSyncStatusPost(@Body request: PullJobsRequest): Response<SyncStatusResponseDto>

    @POST("api/v1/android/license/status")
    suspend fun getLicenseStatus(@Body request: PullJobsRequest): Response<LicenseStatusDto>

    @POST("api/v1/android/sync/fiyatlar")
    suspend fun getFiyatlar(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<FiyatDto>>

    @POST("api/v1/android/sync/acikSiparisler")
    suspend fun getAcikSiparisler(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<AcikSiparisDto>>

    @POST("api/v1/android/sync/cariAdresler")
    suspend fun getCariAdresler(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<CustomerAddressDto>>

    @POST("api/v1/android/sync/cariYetkililer")
    suspend fun getCariYetkililer(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<CustomerContactDto>>

    @POST("api/v1/android/sync/barkodlar")
    suspend fun getBarkodlar(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<BarcodeDto>>

    @POST("api/v1/mobile/activate")
    suspend fun activateDevice(@Body request: ActivationRequest): Response<ActivationResponse>

    @POST("api/v1/mobile/migrate")
    suspend fun migrateDevice(@Body request: ActivationRequest): Response<ActivationResponse>

    @POST("api/v1/mobile/renew")
    suspend fun renewDeviceToken(): Response<ActivationResponse>

    @POST("api/v1/android/sync/satisSartlari")
    suspend fun getSalesConditions(@Body request: PullJobsRequest): Response<FieldOpsSyncResponse<SalesConditionDto>>
}

// DTOs
@androidx.annotation.Keep
data class ActivationRequest(
    val code: String,
    val installationId: String,
    val deviceName: String,
    val appVersion: String
)

@androidx.annotation.Keep
data class ActivationResponse(
    val token: String,
    val tenantId: String,
    val deviceId: String,
    val expiresAtUtc: String
)
@androidx.annotation.Keep
data class FieldOpsSyncResponse<T>(
    val success: Boolean? = null,
    val message: String? = null,
    val entity: String? = null,
    val page: Int? = null,
    val pageSize: Int? = null,
    val total: Int? = null,
    val since: String? = null,
    val watermark: String? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<T>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<T>? = null
) {
    val actualItems: List<T> get() = items ?: data ?: emptyList()
}

@androidx.annotation.Keep
data class CariHareketDto(
    val id: String? = null,
    val date: String? = null,
    val type: String? = null, // SATIŞ, TAHSİLAT, TEDİYE
    val amount: Double? = null,
    val description: String? = null
)

@androidx.annotation.Keep
data class CariDto(
    @com.squareup.moshi.Json(name = "customerCode") val id: String? = null,
    val erpRef: String? = null,
    val erpKod: String? = null,
    val cariKod: String? = null,
    val erp: String? = null,
    @com.squareup.moshi.Json(name = "title1") val unvan: String? = null,
    val cariUnvan: String? = null,
    val cariTip: Int? = null,
    @com.squareup.moshi.Json(name = "taxNo") val vergiNo: String? = null,
    @com.squareup.moshi.Json(name = "taxOffice") val vergiDairesi: String? = null,
    val tcKimlikNo: String? = null,
    val adres: String? = null,
    val il: String? = null,
    val ilce: String? = null,
    @com.squareup.moshi.Json(name = "phone") val telefon: String? = null,
    val email: String? = null,
    val musteri: Boolean? = null,
    val updatedAt: String? = null,
    val createdAt: String? = null,
    val isDeleted: Boolean? = null,
    val paraBirimi: String? = null,
    // Additional/undocumented fields from bridge:
    @com.squareup.moshi.Json(name = "bakiye") val bakiye: Double? = null,
    @com.squareup.moshi.Json(name = "balance") val balance: Double? = null,
    @com.squareup.moshi.Json(name = "netBakiye") val netBakiye: Double? = null,
    @com.squareup.moshi.Json(name = "net_bakiye") val net_bakiye: Double? = null,
    @com.squareup.moshi.Json(name = "cari_bakiye") val cari_bakiye: Double? = null,
    @com.squareup.moshi.Json(name = "borc_bakiye") val borc_bakiye: Double? = null,
    @com.squareup.moshi.Json(name = "Bakiye") val Bakiye_Pascal: Double? = null,
    @com.squareup.moshi.Json(name = "Balance") val Balance_Pascal: Double? = null,
    @com.squareup.moshi.Json(name = "NetBakiye") val NetBakiye_Pascal: Double? = null,
    @com.squareup.moshi.Json(name = "borc") val borc: Double? = null,
    @com.squareup.moshi.Json(name = "alacak") val alacak: Double? = null,
    val hareketler: List<CariHareketDto>? = null,
    val transactions: List<CariHareketDto>? = null
) {
    val actualCariKod: String get() = (cariKod ?: erpKod ?: id ?: "").trim()
    val actualCariUnvan: String get() = cariUnvan ?: unvan ?: "İsimsiz Cari"
    val actualBakiye: Double get() {
        val candidates = listOf(bakiye, balance, netBakiye, net_bakiye, cari_bakiye, borc_bakiye, Bakiye_Pascal, Balance_Pascal, NetBakiye_Pascal)
        val firstVal = candidates.firstOrNull { it != null }
        if (firstVal != null) return firstVal
        if (borc != null || alacak != null) {
            return (borc ?: 0.0) - (alacak ?: 0.0)
        }
        return 0.0
    }
}

@androidx.annotation.Keep
data class UrunDto(
    @com.squareup.moshi.Json(name = "tenant_id") val tenant_id: String? = null,
    @com.squareup.moshi.Json(name = "api_key") val api_key: String? = null,
    @com.squareup.moshi.Json(name = "urun_kodu") val urun_kodu: String? = null,
    @com.squareup.moshi.Json(name = "urun_adi") val urun_adi: String? = null,
    @com.squareup.moshi.Json(name = "barkod") val barkod: String? = null,
    @com.squareup.moshi.Json(name = "satis_fiyati") val satis_fiyati: Double? = null,
    @com.squareup.moshi.Json(name = "kdv") val kdv: Double? = null,
    @com.squareup.moshi.Json(name = "stok") val stok: Int? = null,
    
    // Fallback/Legacy fields
    val id: String? = null,
    val erpRef: String? = null,
    val erpKod: String? = null,
    val urunKod: String? = null,
    val erp: String? = null,
    val ad: String? = null,
    val urunAd: String? = null,
    val urunTip: Int? = null,
    val birim: String? = null,
    val kdvOrani: Double? = null,
    val alisFiyat: Double? = null,
    val satisFiyat: Double? = null,
    val listeFiyati: Double? = null,
    val paraBirimi: String? = null,
    val kategori: String? = null,
    val marka: String? = null,
    val aktif: Boolean? = null,
    val updatedAt: String? = null,
    val createdAt: String? = null,
    val isDeleted: Boolean? = null,
    val miktar: Int? = null,
    val quantity: Int? = null,
    val stock: Int? = null,
    val miktarDepo: Map<String, Int>? = null,
    val stockByWarehouse: Map<String, Int>? = null,
    val bayiFiyati: Double? = null,
    val toptanFiyati: Double? = null,
    val customPrices: Map<String, Double>? = null,
    @com.squareup.moshi.Json(name = "stockCode") val stockCode: String? = null,
    @com.squareup.moshi.Json(name = "barcodes") val stockBarcodes: List<BarcodeDto>? = null,
    @com.squareup.moshi.Json(name = "reyonKod") val reyonKod: String? = null,
    @com.squareup.moshi.Json(name = "olcu") val olcu: String? = null,
    @com.squareup.moshi.Json(name = "ambalaj") val ambalaj: String? = null,
    @com.squareup.moshi.Json(name = "koliAdet") val koliAdet: String? = null,
    @com.squareup.moshi.Json(name = "sto_yer_kod") val sto_yer_kod: String? = null,
    @com.squareup.moshi.Json(name = "sto_sektor_kodu") val sto_sektor_kodu: String? = null,
    @com.squareup.moshi.Json(name = "sto_ambalaj_kodu") val sto_ambalaj_kodu: String? = null,
    @com.squareup.moshi.Json(name = "sto_marka_kodu") val sto_marka_kodu: String? = null,
    @com.squareup.moshi.Json(name = "sto_kalkon_kodu") val sto_kalkon_kodu: String? = null,
    @com.squareup.moshi.Json(name = "shelfCode") val shelfCode: String? = null,
    @com.squareup.moshi.Json(name = "sectorCode") val sectorCode: String? = null,
    @com.squareup.moshi.Json(name = "packageCode") val packageCode: String? = null,
    @com.squareup.moshi.Json(name = "brandCode") val brandCode: String? = null,
    @com.squareup.moshi.Json(name = "cartonCode") val cartonCode: String? = null,

    // Additional Possible ERP/Turkish Naming variations to capture with Moshi
    @com.squareup.moshi.Json(name = "stokKod") val stokKod: String? = null,
    @com.squareup.moshi.Json(name = "stokKodu") val stokKodu: String? = null,
    @com.squareup.moshi.Json(name = "stok_kodu") val stok_kodu: String? = null,
    @com.squareup.moshi.Json(name = "sto_kod") val sto_kod: String? = null,
    @com.squareup.moshi.Json(name = "sto_kodu") val sto_kodu: String? = null,
    @com.squareup.moshi.Json(name = "code") val code: String? = null,
    @com.squareup.moshi.Json(name = "sku") val sku: String? = null,
    @com.squareup.moshi.Json(name = "stokAd") val stokAd: String? = null,
    @com.squareup.moshi.Json(name = "stokAdi") val stokAdi: String? = null,
    @com.squareup.moshi.Json(name = "stok_adi") val stok_adi: String? = null,
    @com.squareup.moshi.Json(name = "stok_ad") val stok_ad: String? = null,
    @com.squareup.moshi.Json(name = "sto_isim") val sto_isim: String? = null,
    @com.squareup.moshi.Json(name = "sto_adi") val sto_adi: String? = null,
    @com.squareup.moshi.Json(name = "isim") val isim: String? = null,
    @com.squareup.moshi.Json(name = "name") val name: String? = null,
    @com.squareup.moshi.Json(name = "title") val title: String? = null,
    @com.squareup.moshi.Json(name = "price") val price: Double? = null,
    @com.squareup.moshi.Json(name = "fiyat") val fiyat: Double? = null,

    // Additional Price/Barcode/Stock fields from Mikro/ERP
    @com.squareup.moshi.Json(name = "sfiyat_fiyat") val sfiyat_fiyat: Double? = null,
    @com.squareup.moshi.Json(name = "sfiyat_fiyati") val sfiyat_fiyati: Double? = null,
    @com.squareup.moshi.Json(name = "sto_satis_fiyat") val sto_satis_fiyat: Double? = null,
    @com.squareup.moshi.Json(name = "sto_birim1_barkod") val sto_birim1_barkod: String? = null,
    @com.squareup.moshi.Json(name = "bar_kodu") val bar_kodu: String? = null,
    @com.squareup.moshi.Json(name = "sto_miktari") val sto_miktari: Int? = null,
    @com.squareup.moshi.Json(name = "sto_eldeki_miktar") val sto_eldeki_miktar: Int? = null,

    // PascalCase and Case-Insensitive ERP Variations
    @com.squareup.moshi.Json(name = "StokKodu") val StokKodu_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "StokKod") val StokKod_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Stok_Kodu") val Stok_Kodu_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Stok_Kod") val Stok_Kod_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "UrunKodu") val UrunKodu_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "UrunKod") val UrunKod_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Urun_Kodu") val Urun_Kodu_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Urun_Kod") val Urun_Kod_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Code") val Code_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Id") val Id_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "ID") val ID_Cap: String? = null,
    @com.squareup.moshi.Json(name = "ErpKod") val ErpKod_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "ErpRef") val ErpRef_Pascal: String? = null,
    
    @com.squareup.moshi.Json(name = "StokAdi") val StokAdi_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "StokAd") val StokAd_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Stok_Adi") val Stok_Adi_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Stok_Ad") val Stok_Ad_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "UrunAdi") val UrunAdi_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "UrunAd") val UrunAd_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Urun_Adi") val Urun_Adi_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Urun_Ad") val Urun_Ad_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Name") val Name_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Title") val Title_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Isim") val Isim_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Sto_Isim") val Sto_Isim_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Sto_Adi") val Sto_Adi_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Ad") val Ad_Pascal: String? = null,
    
    @com.squareup.moshi.Json(name = "Barkod") val Barkod_Pascal: String? = null,
    @com.squareup.moshi.Json(name = "Barcode") val Barcode_Pascal: String? = null,
    
    @com.squareup.moshi.Json(name = "Fiyat") val Fiyat_Pascal: Double? = null,
    @com.squareup.moshi.Json(name = "SatisFiyati") val SatisFiyati_Pascal: Double? = null,
    @com.squareup.moshi.Json(name = "Satis_Fiyati") val Satis_Fiyati_Pascal: Double? = null,
    @com.squareup.moshi.Json(name = "SatisFiyat") val SatisFiyat_Pascal: Double? = null,
    @com.squareup.moshi.Json(name = "Price") val Price_Pascal: Double? = null,
    @com.squareup.moshi.Json(name = "ListeFiyati") val ListeFiyati_Pascal: Double? = null,
    
    @com.squareup.moshi.Json(name = "KdvOrani") val KdvOrani_Pascal: Double? = null,
    @com.squareup.moshi.Json(name = "KdvPercent") val KdvPercent_Pascal: Double? = null,
    @com.squareup.moshi.Json(name = "Kdv_Orani") val Kdv_Orani: Double? = null,
    @com.squareup.moshi.Json(name = "Kdv") val Kdv_Pascal: Double? = null,
    
    @com.squareup.moshi.Json(name = "ToplamMevcutStok") val ToplamMevcutStok_Pascal: Int? = null,
    @com.squareup.moshi.Json(name = "toplamMevcutStok") val toplamMevcutStok_Camel: Int? = null,
    @com.squareup.moshi.Json(name = "toplam_mevcut_stok") val toplam_mevcut_stok_Snake: Int? = null,
    @com.squareup.moshi.Json(name = "Stok") val Stok_Pascal: Int? = null,
    @com.squareup.moshi.Json(name = "Miktar") val Miktar_Pascal: Int? = null,
    @com.squareup.moshi.Json(name = "Quantity") val Quantity_Pascal: Int? = null,
    @com.squareup.moshi.Json(name = "Stock") val Stock_Pascal: Int? = null,
    @com.squareup.moshi.Json(name = "BayiFiyati") val BayiFiyati_Pascal: Double? = null,
    @com.squareup.moshi.Json(name = "ToptanFiyati") val ToptanFiyati_Pascal: Double? = null
) {
    val explicitStok: Int? get() {
        val candidates = listOf(
            stok, miktar, quantity, stock, sto_miktari, sto_eldeki_miktar,
            ToplamMevcutStok_Pascal, toplamMevcutStok_Camel, toplam_mevcut_stok_Snake, Stok_Pascal, Miktar_Pascal, Quantity_Pascal, Stock_Pascal
        )
        return candidates.firstOrNull { it != null }
    }

    val actualBayiFiyati: Double? get() {
        val candidates = listOf(
            bayiFiyati, BayiFiyati_Pascal
        )
        return candidates.firstOrNull { it != null }
    }

    val actualToptanFiyati: Double? get() {
        val candidates = listOf(
            toptanFiyati, ToptanFiyati_Pascal
        )
        return candidates.firstOrNull { it != null }
    }

    val actualUrunKod: String get() {
        val candidates = listOf(
            stockCode, urun_kodu, urunKod, stokKod, stokKodu, stok_kodu, sto_kod, sto_kodu, erpKod, id, code, sku,
            StokKodu_Pascal, StokKod_Pascal, Stok_Kodu_Pascal, Stok_Kod_Pascal, UrunKodu_Pascal, UrunKod_Pascal, Urun_Kodu_Pascal, Urun_Kod_Pascal, Code_Pascal, Id_Pascal, ID_Cap, ErpKod_Pascal, ErpRef_Pascal
        )
        return candidates.firstOrNull { !it.isNullOrBlank() }?.trim() ?: ""
    }

    val actualUrunAd: String get() {
        val candidates = listOf(
            urun_adi, urunAd, ad, stokAd, stokAdi, stok_adi, stok_ad, sto_isim, sto_adi, isim, name, title,
            StokAdi_Pascal, StokAd_Pascal, Stok_Adi_Pascal, Stok_Ad_Pascal, UrunAdi_Pascal, UrunAd_Pascal, Urun_Adi_Pascal, Urun_Ad_Pascal, Name_Pascal, Title_Pascal, Isim_Pascal, Sto_Isim_Pascal, Sto_Adi_Pascal, Ad_Pascal
        )
        return candidates.firstOrNull { !it.isNullOrBlank() } ?: "İsimsiz Ürün"
    }

    val actualSatisFiyat: Double get() {
        val candidates = listOf(
            satis_fiyati, satisFiyat, listeFiyati, price, fiyat, sfiyat_fiyat, sfiyat_fiyati, sto_satis_fiyat,
            Fiyat_Pascal, SatisFiyati_Pascal, Satis_Fiyati_Pascal, SatisFiyat_Pascal, Price_Pascal, ListeFiyati_Pascal
        )
        return candidates.firstOrNull { it != null && it > 0.0 } ?: candidates.firstOrNull { it != null } ?: 0.0
    }

    val actualKdv: Double get() {
        val candidates = listOf(
            kdv, kdvOrani,
            KdvOrani_Pascal, KdvPercent_Pascal, Kdv_Orani, Kdv_Pascal
        )
        return candidates.firstOrNull { it != null } ?: 20.0
    }

    val actualBarkod: String get() {
        val candidates = listOf(
            barkod, sto_birim1_barkod, bar_kodu, Barkod_Pascal, Barcode_Pascal
        )
        val b = candidates.firstOrNull { !it.isNullOrBlank() }?.trim()
        return if (!b.isNullOrBlank()) b else actualUrunKod
    }

    val actualReyonKod: String? get() = listOf(reyonKod, sto_yer_kod, shelfCode)
        .firstOrNull { !it.isNullOrBlank() }?.trim()

    val actualOlcu: String? get() = listOf(olcu, sto_sektor_kodu, sectorCode)
        .firstOrNull { !it.isNullOrBlank() }?.trim()

    val actualAmbalaj: String? get() = listOf(ambalaj, sto_ambalaj_kodu, packageCode)
        .firstOrNull { !it.isNullOrBlank() }?.trim()

    val actualMarka: String? get() = listOf(marka, sto_marka_kodu, brandCode)
        .firstOrNull { !it.isNullOrBlank() }?.trim()

    val actualKoliAdet: String? get() = listOf(koliAdet, sto_kalkon_kodu, cartonCode)
        .firstOrNull { !it.isNullOrBlank() }?.trim()

    val actualStok: Int get() {
        val candidates = listOf(
            stok, miktar, quantity, stock,
            ToplamMevcutStok_Pascal, toplamMevcutStok_Camel, toplam_mevcut_stok_Snake, Stok_Pascal, Miktar_Pascal, Quantity_Pascal, Stock_Pascal
        )
        return candidates.firstOrNull { it != null } ?: 0
    }
}

@androidx.annotation.Keep
data class StokHareketResponse(
    val entity: String? = null,
    val stokKod: String? = null,
    val page: Int? = null,
    val pageSize: Int? = null,
    val total: Int? = null,
    val since: String? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<StokHareketiDto>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<StokHareketiDto>? = null,
    @com.squareup.moshi.Json(name = "hareketler") val hareketler: List<StokHareketiDto>? = null,
    @com.squareup.moshi.Json(name = "movements") val movements: List<StokHareketiDto>? = null
) {
    val actualItems: List<StokHareketiDto> get() = items ?: data ?: hareketler ?: movements ?: emptyList()
}

@androidx.annotation.Keep
data class StokHareketiDto(
    val id: String? = null,
    val erpRef: String? = null,
    val erp: String? = null,
    val stokKod: String? = null,
    val urunKod: String? = null,
    val tarih: String? = null,
    val tip: Int? = null,             // 0=giriş 1=çıkış 2=iade giriş 3=iade çıkış
    val cins: Int? = null,
    val evrakTip: Int? = null,
    val evrakNo: String? = null,
    val girisMiktar: Double? = null,
    val cikisMiktar: Double? = null,
    val miktar: Double? = null,       // negative for çıkış (legacy field)
    val birimFiyat: Double? = null,   // KDV hariç
    val tutar: Double? = null,
    val cariKod: String? = null,
    val girisDepoNo: Int? = null,
    val cikisDepoNo: Int? = null,
    val aciklama: String? = null,
    val updatedAt: String? = null,
    val faturaRecno: Int? = null
)

@androidx.annotation.Keep
data class CariHareketResponse(
    val entity: String? = null,
    val cariKod: String? = null,
    val page: Int? = null,
    val pageSize: Int? = null,
    val total: Int? = null,
    val since: String? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<CariHareketiDto>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<CariHareketiDto>? = null,
    @com.squareup.moshi.Json(name = "hareketler") val hareketler: List<CariHareketiDto>? = null,
    @com.squareup.moshi.Json(name = "transactions") val transactions: List<CariHareketiDto>? = null
) {
    val actualItems: List<CariHareketiDto> get() = items ?: data ?: hareketler ?: transactions ?: emptyList()
}

@androidx.annotation.Keep
data class CariHareketiDto(
    val id: String,
    val erpRef: String? = null,
    val erp: String? = null,
    val cariKod: String? = null,
    val tarih: String? = null,
    val evrakTip: Int? = null,
    val evrakNo: String? = null,
    val tip: Int? = null,             // 0=borç 1=alacak
    val tutar: Double? = null,
    val borcMu: Boolean? = null,      // tip==0 ise true
    val aciklama: String? = null,
    val updatedAt: String? = null,
    val cha_recno: Int? = null,
    val cha_RECno: Int? = null,
    val chaRecNo: Int? = null,
    val recno: Int? = null
) {
    val realChaRecNo: Int? get() = recno ?: cha_recno ?: cha_RECno ?: chaRecNo
}

@androidx.annotation.Keep
data class FieldOpsPushResponse(
    val requestId: String,
    val status: String,
    val isNew: Boolean,
    val queuedAt: String?
)

@androidx.annotation.Keep
data class FieldOpsPushStatusResponse(
    val requestId: String,
    val status: String,
    val entity: String? = null,
    val erp: String? = null,
    val attemptCount: Int? = null,
    val lastErrorCode: String? = null,
    val lastErrorMessage: String? = null,
    val createdAt: String? = null,
    val updatedAt: String? = null,
    val completedAt: String?
)

@androidx.annotation.Keep
data class FieldOpsTriggerResponse(
    val message: String? = null,
    val erp: String? = null,
    val entity: String? = null,
    val triggeredAt: String?
)

@androidx.annotation.Keep
data class LicenseStatusDto(
    val state: String,
    val reason: String? = null,
    val lastCheckedAt: String? = null,
    val expiresAt: String? = null,
    val daysUntilExpiry: Int? = null,
    val daysRemaining: Int? = null,
    val enabledErps: List<String>? = null,
    val erpAllowed: List<String>? = null,
    val licensee: String? = null,
    val machineFingerprint: String? = null,
    val allowsSync: Boolean?
)

@androidx.annotation.Keep
data class StokSatisFiyatListeTanimlariDto(
    val id: String,
    val erpRef: String? = null,
    val listNo: Int? = null,                // sfiyat_listeno
    val aciklama: String? = null,            // sfiyat_aciklama
    @com.squareup.moshi.Json(name = "sfiyat_listeno") val sfiyat_listeno: Int? = null,
    @com.squareup.moshi.Json(name = "listNumber") val listNumber: Int? = null,
    @com.squareup.moshi.Json(name = "listeNo") val listeNo: Int? = null,
    @com.squareup.moshi.Json(name = "sfiyat_aciklama") val sfiyat_aciklama: String? = null,
    @com.squareup.moshi.Json(name = "description") val description: String? = null,
    @com.squareup.moshi.Json(name = "name") val name: String? = null,
    @com.squareup.moshi.Json(name = "listeAdi") val listeAdi: String? = null,
    val paraBirimi: String? = null,          // sfiyat_parabirimi
    val updatedAt: String? = null,
    val isDeleted: Boolean?
) {
    val actualListNo: Int get() = listNumber ?: sfiyat_listeno ?: listNo ?: listeNo ?: 0
    val actualName: String get() = listOf(
        sfiyat_aciklama, aciklama, description, name, listeAdi
    ).firstOrNull { !it.isNullOrBlank() }?.trim().orEmpty()
}

@androidx.annotation.Keep
data class StokSatisFiyatListeleriDto(
    val id: String? = null,
    val erpRef: String? = null,
    @com.squareup.moshi.Json(name = "sfiyat_stokkod") val sfiyat_stokkod: String? = null,
    @com.squareup.moshi.Json(name = "stokKod") val stokKod: String? = null,
    @com.squareup.moshi.Json(name = "stok_kod") val stok_kod: String? = null,
    @com.squareup.moshi.Json(name = "stockCode") val stockCode: String? = null,
    @com.squareup.moshi.Json(name = "sfiyat_listeno") val sfiyat_listeno: Int? = null,
    @com.squareup.moshi.Json(name = "listNo") val listNo: Int? = null,
    @com.squareup.moshi.Json(name = "listNumber") val listNumber: Int? = null,
    @com.squareup.moshi.Json(name = "sfiyat_fiyat") val sfiyat_fiyat: Double? = null,
    @com.squareup.moshi.Json(name = "sfiyat_fiyati") val sfiyat_fiyati: Double? = null,
    @com.squareup.moshi.Json(name = "fiyat") val fiyat: Double? = null,
    @com.squareup.moshi.Json(name = "price") val price: Double? = null,
    @com.squareup.moshi.Json(name = "listName") val listName: String? = null,
    @com.squareup.moshi.Json(name = "aciklama") val aciklama: String? = null,
    @com.squareup.moshi.Json(name = "sfiyat_aciklama") val sfiyat_aciklama: String? = null,
    val doviz: String? = null,
    val updatedAt: String? = null,
    val isDeleted: Boolean? = null
) {
    val actualStokKod: String get() = (stockCode ?: sfiyat_stokkod ?: stokKod ?: stok_kod ?: "").trim()
    val actualListNo: Int get() = listNumber ?: sfiyat_listeno ?: listNo ?: 0
    val actualFiyat: Double get() = price ?: sfiyat_fiyat ?: sfiyat_fiyati ?: fiyat ?: 0.0
    val actualListName: String get() = listOf(listName, aciklama, sfiyat_aciklama)
        .firstOrNull { !it.isNullOrBlank() }?.trim().orEmpty()
}

@androidx.annotation.Keep
data class StokSeviyeResponse(
    val entity: String? = null,
    val stokKod: String? = null,
    val page: Int? = null,
    val pageSize: Int? = null,
    val total: Int? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<StokSeviyeDto>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<StokSeviyeDto>? = null
) {
    val actualItems: List<StokSeviyeDto> get() = items ?: data ?: emptyList()
}

@androidx.annotation.Keep
data class StokSeviyeDto(
    val erpRef: String? = null,
    val erp: String? = null,
    @com.squareup.moshi.Json(name = "sto_kod") val sto_kod: String? = null,
    @com.squareup.moshi.Json(name = "stokKod") val stokKod: String? = null,
    @com.squareup.moshi.Json(name = "stok_kod") val stok_kod: String? = null,
    @com.squareup.moshi.Json(name = "stockCode") val stockCode: String? = null,
    @com.squareup.moshi.Json(name = "warehouseNo") val warehouseNo: Int? = null,
    @com.squareup.moshi.Json(name = "quantity") val quantity: Double? = null,
    @com.squareup.moshi.Json(name = "eldEkiMiktar") val eldEkiMiktar: Double? = null,
    @com.squareup.moshi.Json(name = "eldekiMiktar") val eldekiMiktar: Double? = null,
    @com.squareup.moshi.Json(name = "eldEki_Miktar") val eldEki_Miktar: Double? = null,
    @com.squareup.moshi.Json(name = "sto_miktari") val sto_miktari: Double? = null,
    @com.squareup.moshi.Json(name = "sto_eldeki_miktar") val sto_eldeki_miktar: Double? = null,
    @com.squareup.moshi.Json(name = "stok_seviye") val stok_seviye: Double? = null,
    val updatedAt: String? = null,
    val id: String? = null,
    val depoNo: Int? = null,
    val depoAd: String? = null,
    val miktar: Double? = null,
    val birim: String? = null
) {
    val actualStokKod: String get() = (stockCode ?: sto_kod ?: stokKod ?: stok_kod ?: "").trim()
    val actualDepoNo: Int? get() = warehouseNo ?: depoNo
    val actualMiktar: Double get() = quantity ?: miktar ?: eldekiMiktar ?: eldEki_Miktar ?: sto_miktari ?: sto_eldeki_miktar ?: stok_seviye ?: 0.0
}

@androidx.annotation.Keep
data class FiyatListesiTanimiDto(
    val erpRef: String? = null,
    val erp: String? = null,
    val listeNo: Int,
    val aciklama: String? = null,
    val kdvDahil: Boolean? = null,
    val otvDahil: Boolean? = null,
    val oivDahil: Boolean? = null,
    val ilkTarih: String? = null,
    val sonTarih: String? = null,
    val updatedAt: String?
)

@androidx.annotation.Keep
data class FiyatListesiSatirDto(
    val erpRef: String? = null,
    val erp: String? = null,
    val listeNo: Int,
    val stokKod: String,
    val depoNo: Int? = null,
    val odemePlani: Int? = null,
    val fiyat: Double,
    val doviz: Int? = null,
    val iskontoKod: String? = null,
    val kampanyaKod: String? = null,
    val primYuzdesi: Double? = null,
    val updatedAt: String? = null,
    val aciklama: String? // Present in getFiyatListesi byStokKod (Mod 3) response
)

@androidx.annotation.Keep
data class FaturaHareketResponse(
    val entity: String? = null,
    val cariKod: String? = null,
    val page: Int? = null,
    val pageSize: Int? = null,
    val total: Int? = null,
    val since: String? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<FaturaHareketDto>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<FaturaHareketDto>? = null
) {
    val actualItems: List<FaturaHareketDto> get() = items ?: data ?: emptyList()
}

@androidx.annotation.Keep
data class FaturaHareketDto(
    val erpRef: String,
    val erp: String? = null,
    val cariKod: String? = null,
    val tarih: String? = null,
    val evrakTip: Int? = null,
    val evrakNo: String? = null,
    val tip: Int? = null,
    val tutar: Double? = null,
    val updatedAt: String? = null,
    val satirlar: List<FaturaSatirDto>?
)

@androidx.annotation.Keep
data class FaturaSatirDto(
    val erpRef: String,
    val stokKod: String? = null,
    val stokAd: String? = null,
    val tarih: String? = null,
    val tip: Int? = null,
    val cins: Int? = null,
    val girisMiktar: Double? = null,
    val cikisMiktar: Double? = null,
    val miktar: Double? = null,
    val birimFiyat: Double? = null,
    val tutar: Double? = null,
    val vergi: Double? = null,
    val girisDepoNo: Int? = null,
    val cikisDepoNo: Int? = null,
    val aciklama: String? = null,
    val updatedAt: String? = null,
    val sth_fat_recid_recno: Int? = null,
    val sth_fat_recid_RECno: Int? = null,
    val sth_fat_recid_recNo: Int? = null,
    val sthFatRecidRecno: Int? = null,
    val sth_fat_recid_recno_cir: Int? = null
) {
    val realSthFatRecidRecno: Int? get() = sth_fat_recid_recno ?: sth_fat_recid_RECno ?: sth_fat_recid_recNo ?: sthFatRecidRecno ?: sth_fat_recid_recno_cir
}

@androidx.annotation.Keep
data class SyncStatusResponseDto(
    val erp: String? = null,
    val syncInProgress: Boolean? = null,
    val syncStartedAt: String? = null,
    val syncEntity: String? = null,
    val watermarks: List<WatermarkDto>? = null,
    val isRunning: Boolean? = null,
    val lastRunAt: String? = null,
    val lastRunEntity: String? = null,
    val progress: Int? = null,
    val queueDepth: Int? = null
)

@androidx.annotation.Keep
data class WatermarkDto(
    val entity: String,
    val lastSyncAt: String? = null,
    val totalSynced: Int,
    val mode: String?
)

@androidx.annotation.Keep
data class BarkodTanimiResponseDto(
    val entity: String? = null,
    val mode: String? = null,
    val barkodKod: String? = null,
    val stokKod: String? = null,
    val page: Int? = null,
    val pageSize: Int? = null,
    val total: Int? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<BarkodTanimiDto>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<BarkodTanimiDto>? = null
) {
    val actualItems: List<BarkodTanimiDto> get() = items ?: data ?: emptyList()
}

@androidx.annotation.Keep
data class BarkodTanimiDto(
    val erpRef: String? = null,
    val erp: String? = null,
    @com.squareup.moshi.Json(name = "bar_kodu") val bar_kodu: String? = null,
    @com.squareup.moshi.Json(name = "barkod") val barkod: String? = null,
    @com.squareup.moshi.Json(name = "barkodKod") val barkodKod: String? = null,
    @com.squareup.moshi.Json(name = "bar_stokkod") val bar_stokkod: String? = null,
    @com.squareup.moshi.Json(name = "stokKod") val stokKod: String? = null,
    @com.squareup.moshi.Json(name = "stok_kod") val stok_kod: String? = null,
    @com.squareup.moshi.Json(name = "barcode") val barcode: String? = null,
    @com.squareup.moshi.Json(name = "stockCode") val stockCode: String? = null,
    val partiKodu: String? = null,
    val lotNo: Int? = null,
    val serinoVeyaBaglantiKodu: String? = null,
    val barkodTipi: Int? = null,
    val icerigi: Int? = null,
    val birimGosterge: Int? = null,
    val master: Boolean? = null,
    val updatedAt: String? = null
) {
    val actualBarkod: String get() = (barcode ?: bar_kodu ?: barkod ?: barkodKod ?: "").trim()
    val actualStokKod: String get() = (stockCode ?: bar_stokkod ?: stokKod ?: stok_kod ?: "").trim()
}

@androidx.annotation.Keep
data class CariAdresResponseDto(
    val entity: String? = null,
    val mode: String? = null,
    val cariKod: String? = null,
    val page: Int? = null,
    val pageSize: Int? = null,
    val total: Int? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<CariAdresDto>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<CariAdresDto>? = null
) {
    val actualItems: List<CariAdresDto> get() = items ?: data ?: emptyList()
}

@androidx.annotation.Keep
data class CariAdresDto(
    val erpRef: String? = null,
    val erp: String? = null,
    val cariKod: String? = null,
    val adresNo: Int? = null,
    val yazdirilabilir: Boolean? = null,
    val cadde: String? = null,
    val mahalle: String? = null,
    val sokak: String? = null,
    val semt: String? = null,
    val ilce: String? = null,
    val il: String? = null,
    val ulke: String? = null,
    val postaKodu: String? = null,
    val telUlkeKodu: String? = null,
    val telBolgeKodu: String? = null,
    val telNo1: String? = null,
    val gpsEnlem: Double? = null,
    val gpsBoylam: Double? = null,
    val ziyaretPeriyodu: Int? = null,
    val ziyaretGunu: Int? = null,
    val eFaturaAlias: String? = null,
    val updatedAt: String? = null,
    val id: String? = null,
    val recno: Int? = null,
    val adresTip: Int? = null,
    val adresSatir1: String? = null,
    val adresSatir2: String? = null,
    val fax: String? = null,
    val email: String? = null,
    val telefon: String? = null
)

@androidx.annotation.Keep
data class CariBankaHesapResponseDto(
    val entity: String? = null,
    val mode: String? = null,
    val cariKod: String? = null,
    val page: Int? = null,
    val pageSize: Int? = null,
    val total: Int? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<CariBankaHesapDto>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<CariBankaHesapDto>? = null
) {
    val actualItems: List<CariBankaHesapDto> get() = items ?: data ?: emptyList()
}

@androidx.annotation.Keep
data class CariBankaHesapDto(
    val erpRef: String? = null,
    val erp: String? = null,
    val cariKod: String? = null,
    val slot: Int? = null,
    val tCMBKodu: String? = null,
    val tCMBSubeKodu: String? = null,
    val tCMBIlKodu: String? = null,
    val hesapNumarasi: String? = null,
    val swiftKodu: String? = null,
    val dovizCinsi: Int? = null,
    val musteriNo: String? = null,
    val updatedAt: String? = null,
    val id: String? = null,
    val recno: Int? = null,
    val bankaKod: String? = null,
    val subeKod: String? = null,
    val hesapNo: String? = null,
    val iban: String? = null,
    val hesapSahibi: String? = null,
    val paraBirimi: String? = null,
    val defaultMu: Boolean? = null
)

@androidx.annotation.Keep
data class BankalarResponseDto(
    val entity: String? = null,
    val page: Int? = null,
    val pageSize: Int? = null,
    val total: Int? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<BridgeBankaDto>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<BridgeBankaDto>? = null
) {
    val actualItems: List<BridgeBankaDto> get() = items ?: data ?: emptyList()
}

@androidx.annotation.Keep
data class BridgeBankaDto(
    val erpRef: String? = null,
    val erp: String? = null,
    val kod: String? = null,
    val isim: String? = null,
    val sube: String? = null,
    val swiftKodu: String? = null,
    val iBANKodu: String? = null,
    val hesapNumarasi: String? = null,
    val tCMBKodu: String? = null,
    val tCMBSubeKodu: String? = null,
    val tCMBIlKodu: String? = null,
    val musteriNo: String? = null,
    val hesapTipi: Int? = null,
    val dovizCinsi: Int? = null,
    val cadde: String? = null,
    val mahalle: String? = null,
    val il: String? = null,
    val ulke: String? = null,
    val temsilci: String? = null,
    val temsilciEposta: String? = null,
    val updatedAt: String? = null,
    val id: String? = null,
    val recno: Int? = null,
    val bankaKod: String? = null,
    val bankaAd: String? = null,
    val subeAd: String? = null,
    val swiftKod: String? = null
)

@androidx.annotation.Keep
data class KasalarResponseDto(
    val entity: String? = null,
    val page: Int? = null,
    val pageSize: Int? = null,
    val total: Int? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<KasalarDto>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<KasalarDto>? = null
) {
    val actualItems: List<KasalarDto> get() = items ?: data ?: emptyList()
}

@androidx.annotation.Keep
data class KasalarDto(
    val erpRef: String? = null,
    val erp: String? = null,
    val kod: String? = null,
    val isim: String? = null,
    val tip: Int? = null,
    val muhasebeKod: String? = null,
    val dovizCinsi: Int? = null,
    val bankaKodu: String? = null,
    val updatedAt: String? = null,
    val id: String? = null
)

@androidx.annotation.Keep
data class KasaYonetimResponseDto(
    val entity: String? = null,
    val total: Int? = null,
    @com.squareup.moshi.Json(name = "items") val items: List<KasaYonetimDto>? = null,
    @com.squareup.moshi.Json(name = "data") val data: List<KasaYonetimDto>? = null
) {
    val actualItems: List<KasaYonetimDto> get() = items ?: data ?: emptyList()
}

@androidx.annotation.Keep
data class KasaYonetimDto(
    val erpRef: String? = null,
    val erp: String? = null,
    val kasaKod: String? = null,
    val kasaAd: String? = null,
    val yonetim: String? = null,
    val muhasebeKod: String? = null,
    val updatedAt: String? = null
)

@androidx.annotation.Keep
data class BootstrapRequest(
    val tenant_id: String,
    val api_key: String,
    val device_id: String,
    val agent_version: String
)

@androidx.annotation.Keep
data class BootstrapResponse(
    val tenantId: String? = null,
    val sourceDatabase: String? = null,
    val pulledAtUtc: String? = null,
    val receivedAtUtc: String? = null,
    val success: Boolean = true,
    val message: String? = null,
    val tenant_name: String? = null,
    val allowed_erps: List<String>? = null,
    val active_modules: List<String>? = null
)
@androidx.annotation.Keep
data class PullJobsRequest(
    val tenant_id: String,
    val api_key: String,
    val device_id: String,
    val agent_version: String,
    val entity: String? = null,
    val since: String? = null,
    val page: Int? = 1,
    val pageSize: Int? = 200
)

@androidx.annotation.Keep
data class PullJobsResponse(
    val sourceDatabase: String? = null,
    val pulledAtUtc: String? = null,
    val receivedAtUtc: String? = null,
    val data: Map<String, Any?> = emptyMap(),
    val success: Boolean? = null,
    val message: String? = null,
    val entity: String? = null,
    val watermark: String? = null,
    val items: List<Map<String, Any?>>? = null
)
@androidx.annotation.Keep
data class PushJobsRequest(
    val tenant_id: String,
    val api_key: String,
    val device_id: String,
    val agent_version: String,
    val entity: String,
    val payload: Map<String, Any?>
)

@androidx.annotation.Keep
data class PushJobsResponse(
    val success: Boolean,
    val message: String? = null,
    val requestId: String? = null,
    val status: String? = null
)

@androidx.annotation.Keep
data class FiyatDto(
    val id: String? = null,
    val urunKod: String? = null,
    val fiyat: Double? = null,
    val paraBirimi: String? = null,
    val fiyatListesiKod: String? = null,
    @com.squareup.moshi.Json(name = "stockCode") val stockCode: String? = null,
    @com.squareup.moshi.Json(name = "listNumber") val listNumber: Int? = null,
    @com.squareup.moshi.Json(name = "price") val price: Double? = null,
    @com.squareup.moshi.Json(name = "currency") val currency: String? = null
) {
    val actualStokKod: String get() = (stockCode ?: urunKod ?: "").trim()
    val actualListeNo: Int get() = listNumber ?: fiyatListesiKod?.toIntOrNull() ?: 0
    val actualFiyat: Double get() = price ?: fiyat ?: 0.0
}

@androidx.annotation.Keep
data class AcikSiparisDto(
    val id: String? = null,
    val cariKod: String? = null,
    val tarih: String? = null,
    val tutar: Double? = null,
    val paraBirimi: String? = null,
    val aciklama: String? = null
)
